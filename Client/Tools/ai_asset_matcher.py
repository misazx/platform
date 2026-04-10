#!/usr/bin/env python3
"""
🎨 AI智能美术资源匹配下载器 v1.0
===================================

功能：
  • 读取 asset_download_config.json 配置
  • 扫描已下载的 Kenney 素材库
  • 调用豆包模型 AI 判断最合适的资源匹配
  • 自动下载缺失的素材包
  • 自动映射到游戏配置路径
  • 清理 .import 缓存

使用：
  python3 ai_asset_matcher.py              # 全部匹配
  python3 ai_asset_matcher.py --dry-run    # 仅预览不执行
  python3 ai_asset_matcher.py --category card_attack  # 只匹配某类
  python3 ai_asset_matcher.py --download   # 先下载再匹配
"""

import os
import sys
import json
import shutil
import re
import ssl
import urllib.request
import zipfile
import subprocess
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Optional, Tuple

try:
    from PIL import Image
    HAS_PIL = True
except ImportError:
    HAS_PIL = False

PROJECT_ROOT = Path(__file__).parent.parent / "GameModes" / "base_game" / "Resources"
CONFIG_PATH = PROJECT_ROOT / "asset_download_config.json"
DL_DIR = PROJECT_ROOT / "Assets_Library" / "_downloads"

ctx = ssl.create_default_context()
ctx.check_hostname = False
ctx.verify_mode = ssl.CERT_NONE


class AIAssetMatcher:
    def __init__(self, config_path: str = None):
        self.config = self._load_config(config_path or str(CONFIG_PATH))
        self.local_assets = {}
        self.match_results = {}
        self.stats = {"matched": 0, "ai_matched": 0, "fallback": 0, "skipped": 0, "downloaded": 0}

    def _load_config(self, path: str) -> dict:
        with open(path, 'r', encoding='utf-8') as f:
            return json.load(f)

    def scan_local_assets(self):
        """扫描已下载的素材库，建立文件名索引"""
        print("\n📁 扫描本地素材库...")
        
        for pack_dir in DL_DIR.iterdir():
            if not pack_dir.is_dir():
                continue
            for png_file in pack_dir.rglob("*.png"):
                name = png_file.stem.lower()
                rel = str(png_file.relative_to(DL_DIR))
                self.local_assets[name] = str(png_file)
                for kw in name.split('_'):
                    if kw not in self.local_assets:
                        self.local_assets[kw] = str(png_file)
        
        print(f"   ✅ 索引了 {len(self.local_assets)} 个素材条目")

    def ai_match(self, requirement: str, candidates: List[str]) -> Optional[str]:
        """调用豆包模型 AI 判断最合适的资源"""
        ai_config = self.config.get("ai", {})
        api_key = ai_config.get("api_key", "")
        
        if not api_key:
            return None
        
        api_url = ai_config.get("api_url", "")
        model = ai_config.get("model", "doubao-1.5-pro-256k")
        
        if not api_url:
            return None
        
        candidates_str = "\n".join(f"  {i+1}. {c}" for i, c in enumerate(candidates[:50]))
        
        prompt = ai_config.get("prompt_template", "").format(
            requirement=requirement,
            candidates=candidates_str
        )
        
        try:
            payload = json.dumps({
                "model": model,
                "messages": [{"role": "user", "content": prompt}],
                "max_tokens": 100,
                "temperature": 0.1
            }).encode('utf-8')
            
            req = urllib.request.Request(
                api_url,
                data=payload,
                headers={
                    "Content-Type": "application/json",
                    "Authorization": f"Bearer {api_key}"
                }
            )
            
            resp = urllib.request.urlopen(req, context=ctx, timeout=30)
            result = json.loads(resp.read().decode('utf-8'))
            
            answer = result["choices"][0]["message"]["content"].strip()
            for c in candidates:
                if c.lower() in answer.lower() or answer.lower() in c.lower():
                    return c
            
            return None
        except Exception as e:
            print(f"   ⚠️  AI匹配失败: {e}")
            return None

    def keyword_match(self, requirement: dict) -> Optional[str]:
        """基于关键词的本地匹配"""
        keywords = requirement.get("keywords", [])
        preferred_color = requirement.get("preferred_color", "")
        description = requirement.get("description", "").lower()
        
        color_map = {
            "red": "Red", "blue": "Blue", "green": "Green",
            "yellow": "Yellow", "grey": "Grey", "orange": "Extra",
            "purple": "Extra"
        }
        color_dir = color_map.get(preferred_color, "")
        
        best_match = None
        best_score = 0
        
        for name, path in self.local_assets.items():
            score = 0
            name_lower = name.lower()
            path_lower = path.lower()
            
            for kw in keywords:
                if kw.lower() in name_lower:
                    score += 10
            
            if color_dir and color_dir.lower() in path_lower:
                score += 5
            
            if "default" in path_lower:
                score += 2
            
            if score > best_score:
                best_score = score
                best_match = path
        
        return best_match if best_score > 0 else None

    def download_pack(self, pack_name: str) -> bool:
        """下载指定的 Kenney 素材包"""
        packs = self.config.get("download_sources", {}).get("kenney", {}).get("packs", {})
        if pack_name not in packs:
            print(f"   ❌ 未知素材包: {pack_name}")
            return False
        
        page_url = f"https://kenney.nl/assets/{pack_name}"
        out_dir = DL_DIR / pack_name
        out_dir.mkdir(parents=True, exist_ok=True)
        zip_path = out_dir / f"{pack_name}.zip"
        
        if zip_path.exists() and zip_path.stat().st_size > 1000:
            print(f"   ✅ 已缓存: {pack_name}")
            return True
        
        try:
            req = urllib.request.Request(page_url, headers={'User-Agent': 'Mozilla/5.0'})
            html = urllib.request.urlopen(req, context=ctx, timeout=30).read().decode('utf-8')
            
            links = [l for l in re.findall(r'https://kenney\.nl/media/pages/assets/[^"\'>\s]+?\.zip', html) if 'kenney_' in l]
            if not links:
                print(f"   ❌ 未找到下载链接: {pack_name}")
                return False
            
            print(f"   ⬇️  下载: {pack_name}...")
            req2 = urllib.request.Request(links[0], headers={'User-Agent': 'Mozilla/5.0'})
            data = urllib.request.urlopen(req2, context=ctx, timeout=120).read()
            
            with open(zip_path, 'wb') as f:
                f.write(data)
            
            with zipfile.ZipFile(zip_path, 'r') as z:
                z.extractall(out_dir)
            
            size_mb = len(data) / 1024 / 1024
            print(f"   ✅ 下载完成: {size_mb:.1f} MB")
            self.stats["downloaded"] += 1
            return True
        except Exception as e:
            print(f"   ❌ 下载失败: {e}")
            return False

    def copy_and_resize(self, src: str, dst: str, size: tuple = None):
        """复制并调整大小"""
        dst = Path(dst)
        dst.parent.mkdir(parents=True, exist_ok=True)
        
        if HAS_PIL and size:
            img = Image.open(src)
            if img.mode != 'RGBA':
                img = img.convert('RGBA')
            img = img.resize(size, Image.LANCZOS)
            img.save(str(dst))
        else:
            shutil.copy2(str(src), str(dst))

    def match_all(self, dry_run: bool = False, category: str = None):
        """匹配所有资源"""
        print(f"\n{'='*60}")
        print(f"🎨 AI智能美术资源匹配下载器 v1.0")
        print(f"{'='*60}")
        
        self.scan_local_assets()
        
        mappings = self.config.get("resource_mappings", {})
        size_presets = self.config.get("size_presets", {})
        
        print(f"\n🔍 开始匹配 {len(mappings)} 个资源...")
        
        for target_path, requirement in mappings.items():
            if category and requirement.get("category", "") != category:
                continue
            
            full_dst = PROJECT_ROOT / target_path
            desc = requirement.get("description", target_path)
            target_size = requirement.get("size")
            
            print(f"\n   🎯 {target_path}")
            print(f"      需求: {desc}")
            
            # 1. 先尝试关键词匹配
            match = self.keyword_match(requirement)
            
            if match:
                print(f"      ✅ 关键词匹配: {Path(match).name}")
                self.stats["matched"] += 1
            else:
                # 2. 尝试AI匹配
                candidates = list(set(self.local_assets.values()))[:50]
                if candidates:
                    ai_result = self.ai_match(desc, [Path(c).name for c in candidates])
                    if ai_result:
                        for name, path in self.local_assets.items():
                            if Path(path).name == ai_result:
                                match = path
                                break
                        print(f"      🤖 AI匹配: {ai_result}")
                        self.stats["ai_matched"] += 1
                
                if not match:
                    print(f"      ⚠️  未找到匹配，跳过")
                    self.stats["skipped"] += 1
                    continue
            
            # 执行复制
            if not dry_run and match:
                self.copy_and_resize(match, str(full_dst), target_size)
                print(f"      📋 已复制到: {target_path}")
            
            self.match_results[target_path] = {
                "source": match,
                "method": "keyword" if self.stats["matched"] > self.stats["ai_matched"] else "ai",
                "timestamp": datetime.now().isoformat()
            }
        
        # 清理缓存
        if not dry_run:
            self._clean_caches()
        
        # 打印总结
        self._print_summary(dry_run)

    def _clean_caches(self):
        """清理 .import 缓存（不删除 .godot 目录，让 Godot 自行管理）"""
        print(f"\n🧹 清理 .import 缓存...")
        count = 0
        for dir_path in [PROJECT_ROOT / "Icons", PROJECT_ROOT / "Images", PROJECT_ROOT / "Audio"]:
            if dir_path.exists():
                for f in dir_path.rglob("*.import"):
                    f.unlink()
                    count += 1
        
        print(f"   ✅ 清理了 {count} 个 .import 文件")
        print(f"   💡 提示: 请在 Godot 中执行 Project → ReImport All 刷新资源")

    def _print_summary(self, dry_run: bool):
        """打印总结"""
        mode = "预览模式" if dry_run else "执行模式"
        print(f"\n{'='*60}")
        print(f"🎉 匹配完成 ({mode})")
        print(f"{'='*60}")
        print(f"📊 统计:")
        print(f"   ✅ 关键词匹配: {self.stats['matched']}")
        print(f"   🤖 AI匹配: {self.stats['ai_matched']}")
        print(f"   ⚠️  跳过: {self.stats['skipped']}")
        print(f"   📥 新下载: {self.stats['downloaded']}")
        
        ai_config = self.config.get("ai", {})
        if not ai_config.get("api_key"):
            print(f"\n💡 提示: 未配置豆包API Key，当前仅使用关键词匹配")
            print(f"   配置方法: 编辑 asset_download_config.json 中的 ai.api_key")
            print(f"   获取Key: https://console.volcengine.com/ark")


def main():
    import argparse
    parser = argparse.ArgumentParser(description="AI智能美术资源匹配下载器")
    parser.add_argument("--dry-run", action="store_true", help="仅预览不执行")
    parser.add_argument("--category", type=str, help="只匹配指定类别")
    parser.add_argument("--download", action="store_true", help="先下载缺失的素材包")
    parser.add_argument("--config", type=str, help="配置文件路径")
    args = parser.parse_args()
    
    log_path = PROJECT_ROOT / ".asset_download_log.txt"
    class Tee:
        def __init__(self, *streams):
            self.streams = streams
        def write(self, data):
            for s in self.streams:
                try:
                    s.write(data)
                    s.flush()
                except:
                    pass
        def flush(self):
            for s in self.streams:
                try:
                    s.flush()
                except:
                    pass
    
    log_f = open(str(log_path), 'w', encoding='utf-8')
    sys.stdout = Tee(sys.stdout, log_f)
    sys.stderr = Tee(sys.stderr, log_f)
    
    matcher = AIAssetMatcher(args.config)
    
    if args.download:
        print("\n📥 下载缺失的素材包...")
        packs = matcher.config.get("download_sources", {}).get("kenney", {}).get("packs", {})
        for pack_name, pack_info in packs.items():
            if pack_info.get("enabled", True):
                matcher.download_pack(pack_name)
        matcher.scan_local_assets()
    
    matcher.match_all(dry_run=args.dry_run, category=args.category)


if __name__ == "__main__":
    main()
