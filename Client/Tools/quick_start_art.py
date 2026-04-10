#!/usr/bin/env python3
"""
Godot Roguelike - 快速美术资源集成脚本
一键下载并集成 Kenney.nl + OpenGameArt 免费素材

运行方式:
  python3 quick_start_art.py              # 交互式模式
  python3 quick_start_art.py --auto       # 自动模式（使用默认配置）
"""

import os
import sys
import json
import shutil
import subprocess
import urllib.request
import zipfile
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Tuple

# ==================== 配置 ====================

PROJECT_ROOT = Path(__file__).parent.parent / "GameModes" / "base_game" / "Resources"
ASSETS_DIR = PROJECT_ROOT / "Assets_Library"
DOWNLOAD_CACHE = ASSETS_DIR / "_downloads"

# Kenney.nl 免费素材包 (CC0 授权 - 可免费商用)
KENNEY_PACKS = {
    "rpg-urban-pack": {
        "url": "https://kenney.nl/media/pages/assets/rpg-urban-pack/d9c0e6e2b6-1731502215/rpg-urban-pack.zip",
        "description": "RPG 城市素材包 - 包含角色、UI、道具等",
        "priority_assets": [
            ("PNG/*.png", "cards", "卡牌相关"),
            ("PNG/*.png", "items", "物品图标"),
            ("PNG/*.png", "relics", "遗物/道具"),
        ]
    },
    "ui-pack": {
        "url": "https://kenney.nl/media/pages/assets/ui-pack/d0a74a60c8-1705084399/ui-pack.zip",
        "description": "UI 素材包 - 按钮、面板、框架等界面元素",
        "priority_assets": [
            ("PNG/*.png", "relics", "UI元素"),
        ]
    },
    "platformer-characters": {
        "url": "https://kenney.nl/media/pages/assets/platformer-characters/16b8b90bca-1697459305/platformer-characters.zip",
        "description": "平台跳跃角色 - 可用作敌人/NPC",
        "priority_assets": [
            ("PNG/*.png", "enemies_full", "敌人形象"),
            ("PNG/*.png", "characters", "角色立绘"),
        ]
    },
    "particle-effects": {
        "url": "https://kenney.nl/media/pages/assets/particle-effects/e34f1f06e8-1688099841/particle-effects.zip",
        "description": "粒子特效 - 攻击、魔法、爆炸等效果",
        "priority_assets": [
            ("PNG/*.png", "skills", "技能特效"),
        ]
    }
}

# OpenGameArt 精选资源 (各种开源授权)
OPENGAMEART_ASSETS = {
    "lpc-swords": {
        "name": "LPC 武器图标集",
        "url": "https://opengameart.org/sites/default/files/lpc_weapons_0.zip",
        "category": "cards",
        "description": "武器图标 - 适合卡牌攻击图标"
    },
    "fantasy-icons": {
        "name": "奇幻风格图标",
        "url": "https://opengameart.org/sites/default/files/fantasy-ui-icons.zip",
        "category": "relics",
        "description": "奇幻UI图标"
    }
}

# 项目目录映射
TARGET_DIRS = {
    "cards": PROJECT_ROOT / "Icons" / "Cards",
    "enemies_icon": PROJECT_ROOT / "Icons" / "Enemies",
    "relics": PROJECT_ROOT / "Icons" / "Relics",
    "skills": PROJECT_ROOT / "Icons" / "Skills",
    "items": PROJECT_ROOT / "Icons" / "Items",
    "characters": PROJECT_ROOT / "Images" / "Characters",
    "enemies_full": PROJECT_ROOT / "Images" / "Enemies",
    "backgrounds": PROJECT_ROOT / "Images" / "Backgrounds",
}


def print_banner():
    """打印横幅"""
    banner = """
╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║   🎮 Godot Roguelike 美术资源快速集成工具                  ║
║                                                           ║
║   ✨ 自动下载 Kenney.nl + OpenGameArt 免费素材             ║
║   🔧 一键整理到项目目录结构                               ║
║   📊 生成资源清单和配置更新建议                           ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝
"""
    print(banner)


def init_directories():
    """初始化目录"""
    ASSETS_DIR.mkdir(exist_ok=True)
    DOWNLOAD_CACHE.mkdir(exist_ok=True)
    
    for dir_path in TARGET_DIRS.values():
        dir_path.mkdir(parents=True, exist_ok=True)


def download_file(url: str, dest: Path, description: str = "") -> bool:
    """下载文件（优先使用 curl，避免 SSL 问题）"""
    print(f"\n📥 下载: {description or '文件'}")
    print(f"   从: {url[:80]}...")
    
    try:
        # 方法1：使用 curl (更可靠)
        result = subprocess.run(
            ["curl", "-L", "-o", str(dest), url],
            capture_output=True,
            text=True,
            timeout=120
        )
        
        if result.returncode == 0 and dest.exists():
            file_size = dest.stat().st_size / 1024 / 1024
            if file_size > 0.01:  # 至少 10KB
                print(f"   ✅ 完成! ({file_size:.2f} MB)")
                return True
            else:
                print(f"   ⚠️ 文件过小，可能下载不完整")
                dest.unlink(missing_ok=True)
                
        # 方法2：回退到 urllib (如果 curl 失败)
        print(f"   🔄 尝试备用下载方式...")
        import ssl
        ctx = ssl.create_default_context()
        ctx.check_hostname = False
        ctx.verify_mode = ssl.CERT_NONE
        
        urllib.request.urlretrieve(url, dest)
        file_size = dest.stat().st_size / 1024 / 1024
        print(f"   ✅ 完成! ({file_size:.2f} MB)")
        return True
        
    except Exception as e:
        print(f"   ❌ 失败: {str(e)[:100]}")
        if dest.exists():
            dest.unlink(missing_ok=True)
        return False


def extract_zip(zip_path: Path, extract_to: Path) -> bool:
    """解压 ZIP"""
    print(f"\n📦 解压: {zip_path.name}")
    
    try:
        with zipfile.ZipFile(zip_path, 'r') as zf:
            zf.extractall(extract_to)
        file_count = len(list(extract_to.rglob("*")))
        print(f"   ✅ 解压完成! ({file_count} 个文件)")
        return True
    except Exception as e:
        print(f"   ❌ 失败: {e}")
        return False


def find_png_files(directory: Path) -> List[Path]:
    """查找所有 PNG 文件"""
    return list(directory.rglob("*.png"))


def smart_categorize_file(filename: str) -> str:
    """智能分类文件"""
    name_lower = filename.lower()
    
    # 卡牌/技能相关
    if any(w in name_lower for w in ['card', 'attack', 'strike', 'slash', 'weapon', 'sword']):
        return "cards"
    if any(w in name_lower for w in ['shield', 'defend', 'block', 'armor']):
        return "cards"
    if any(w in name_lower for w in ['skill', 'magic', 'spell', 'fireball', 'effect']):
        return "skills"
        
    # 角色/敌人
    if any(w in name_lower for w in ['player', 'character', 'hero', 'warrior', 'knight']):
        return "characters"
    if any(w in name_lower for w in ['enemy', 'monster', 'creature', 'slime', 'goblin']):
        return "enemies_full"
    if any(w in name_lower for w in ['npc', 'villager', 'person']):
        return "characters"
        
    # 物品/遗物
    if any(w in name_lower for w in ['item', 'potion', 'coin', 'treasure', 'chest', 'key']):
        return "items"
    if any(w in name_lower for w in ['relic', 'artifact', 'amulet', 'ring']):
        return "relics"
        
    # UI 元素
    if any(w in name_lower for w in ['button', 'panel', 'frame', 'box', 'ui', 'icon']):
        return "relics"
        
    # 背景
    if any(w in name_lower for w in ['background', 'bg', 'ground', 'tile', 'scene']):
        return "backgrounds"
        
    # 默认归类到 relics (通用图标)
    return "relics"


def integrate_assets(source_dir: Path, pack_name: str, max_files: int = 50) -> List[Dict]:
    """整合素材到项目目录"""
    print(f"\n🔧 整合素材: {pack_name}")
    
    png_files = find_png_files(source_dir)
    print(f"   找到 {len(png_files)} 个 PNG 文件")
    
    integrated = []
    count = 0
    
    for png_file in png_files:
        if count >= max_files:
            break
            
        filename = png_file.name
        category = smart_categorize_file(filename)
        target_dir = TARGET_DIRS[category]
        target_path = target_dir / filename
        
        # 避免覆盖已有文件（除非是占位符）
        if target_path.exists() and not _is_placeholder(target_path):
            continue
            
        # 复制文件
        shutil.copy2(png_file, target_path)
        
        integrated.append({
            "source_pack": pack_name,
            "filename": filename,
            "category": category,
            "target_path": str(target_path.relative_to(PROJECT_ROOT)),
            "original_path": str(png_file.relative_to(source_dir))
        })
        
        count += 1
        
    print(f"   ✅ 成功整合 {count} 个资源")
    return integrated


def _is_placeholder(file_path: Path) -> bool:
    """检查是否为占位符图片（简单启发式检查）"""
    if not file_path.exists():
        return False
        
    # 小于 10KB 的可能是占位符
    size_kb = file_path.stat().st_size / 1024
    return size_kb < 15


def download_and_integrate_kenney_pack(pack_id: str, pack_info: Dict) -> List[Dict]:
    """下载并整合单个 Kenney 素材包"""
    print("\n" + "="*70)
    print(f"📦 处理 Kenney 素材包: {pack_id}")
    print(f"   {pack_info['description']}")
    print("="*70)
    
    # 准备目录
    pack_cache = DOWNLOAD_CACHE / "kenney" / pack_id
    zip_file = pack_cache / f"{pack_id}.zip"
    extract_dir = pack_cache / "extracted"
    
    pack_cache.mkdir(parents=True, exist_ok=True)
    
    # 下载（如果尚未下载）
    if not zip_file.exists():
        if not download_file(pack_info["url"], zip_file, f"Kenney {pack_id}"):
            return []
    else:
        print(f"\n✅ 已存在，跳过下载: {zip_file.name}")
    
    # 解压
    if extract_dir.exists():
        shutil.rmtree(extract_dir)
    extract_dir.mkdir(exist_ok=True)
    
    if not extract_zip(zip_file, extract_dir):
        return []
    
    # 整合资源
    return integrate_assets(extract_dir, f"kenney-{pack_id}")


def generate_integration_report(all_integrated: List[Dict]):
    """生成集成报告"""
    report_path = ASSETS_DIR / "integration_report.json"
    
    # 统计信息
    stats = {
        "integration_date": datetime.now().isoformat(),
        "total_assets_integrated": len(all_integrated),
        "by_category": {},
        "by_source": {},
        "assets": all_integrated
    }
    
    for asset in all_integrated:
        cat = asset["category"]
        src = asset["source_pack"]
        
        stats["by_category"][cat] = stats["by_category"].get(cat, 0) + 1
        stats["by_source"][src] = stats["by_source"].get(src, 0) + 1
    
    with open(report_path, 'w', encoding='utf-8') as f:
        json.dump(stats, f, indent=2, ensure_ascii=False)
    
    # 打印摘要
    print("\n" + "="*70)
    print("📊 资源集成完成！统计摘要:")
    print("="*70)
    print(f"\n总集成资源数: {stats['total_assets_integrated']}")
    
    print("\n按分类:")
    for cat, count in sorted(stats["by_category"].items()):
        print(f"  📁 {cat}: {count} 个")
    
    print("\n按来源:")
    for src, count in sorted(stats["by_source"].items()):
        print(f"  📦 {src}: {count} 个")
    
    print(f"\n📝 详细报告已保存至: {report_path}")
    
    return stats


def suggest_config_updates(integrated: List[Dict]):
    """生成配置文件更新建议"""
    suggestions_path = ASSETS_DIR / "config_update_suggestions.json"
    
    suggestions = {
        "generated_at": datetime.now().isoformat(),
        "instructions": "以下是可以替换的资源配置建议。请手动编辑对应的 JSON 配置文件。",
        "updates": []
    }
    
    # 按类别分组
    by_category = {}
    for asset in integrated:
        cat = asset["category"]
        if cat not in by_category:
            by_category[cat] = []
        by_category[cat].append(asset)
    
    # 卡牌图标替换建议
    if "cards" in by_category and by_category["cards"]:
        card_files = [a["filename"] for a in by_category["cards"]]
        suggestions["updates"].append({
            "config_file": "Config/Data/cards.json",
            "field": "iconPath",
            "available_icons": card_files[:20],
            "note": "可将卡牌的 iconPath 更新为新的高质量图标"
        })
    
    # 角色立绘替换建议
    if "characters" in by_category and by_category["characters"]:
        char_files = [a["filename"] for a in by_category["characters"]]
        suggestions["updates"].append({
            "config_file": "Config/Data/characters.json",
            "field": "portraitPath",
            "available_portraits": char_files[:10],
            "note": "可更新角色的 portraitPath 为新立绘"
        })
    
    # 敌人图标替换建议
    if "enemies_full" in by_category and by_category["enemies_full"]:
        enemy_files = [a["filename"] for a in by_category["enemies_full"]]
        suggestions["updates"].append({
            "config_file": "Config/Data/enemies.json",
            "fields": ["portraitPath", "iconPath"],
            "available_enemies": enemy_files[:10],
            "note": "可更新敌人的肖像和图标路径"
        })
    
    with open(suggestions_path, 'w', encoding='utf-8') as f:
        json.dump(suggestions, f, indent=2, ensure_ascii=False)
    
    print(f"\n💡 配置更新建议已保存至: {suggestions_path}")
    print("   请查看此文件了解如何更新 Config/Data/*.json 配置")


def main_auto_mode():
    """自动模式 - 使用默认配置"""
    print_banner()
    init_directories()
    
    all_integrated = []
    
    print("\n🚀 开始自动下载和集成 Kenney.nl 免费素材...\n")
    
    # 下载并整合每个 Kenney 包
    for pack_id, pack_info in KENNEY_PACKS.items():
        integrated = download_and_integrate_kenney_pack(pack_id, pack_info)
        all_integrated.extend(integrated)
    
    # 生成报告
    stats = generate_integration_report(all_integrated)
    
    # 配置更新建议
    suggest_config_updates(all_integrated)
    
    # 最终提示
    print("\n" + "="*70)
    print("✅ 快速集成完成!")
    print("="*70)
    print("""
下一步操作:

1. 📖 查看集成报告:
   cat Assets_Library/integration_report.json

2. ⚙️  更新游戏配置文件:
   - 编辑 Config/Data/cards.json (更新卡牌图标路径)
   - 编辑 Config/Data/characters.json (更新角色立绘)
   - 编辑 Config/Data/enemies.json (更新敌人图像)

3. 🎮 在 Godot 中验证:
   - 打开项目
   - 运行游戏检查显示效果
   - 根据需要微调资源

4. 🔧 后续添加新资源:
   python3 art_asset_manager.py add --source <path> --category <type>

5. 📚 查看完整文档:
   查看 ART_ASSET_GUIDE.md
""")


def main_interactive_mode():
    """交互式模式"""
    print_banner()
    init_directories()
    
    print("\n请选择要下载的素材包 (输入编号，多个用逗号分隔):\n")
    
    for idx, (pack_id, info) in enumerate(KENNEY_PACKS.items(), 1):
        print(f"{idx:2d}. {pack_id}")
        print(f"     {info['description']}")
        print()
    
    print(f"{len(KENNEY_PACKS)+1}. 全部下载")
    print(f" 0. 退出\n")
    
    choice = input("你的选择: ").strip()
    
    if choice == "0":
        print("已取消")
        return
    
    selected_packs = []
    
    if choice == str(len(KENNEY_PACKS)+1):
        selected_packs = list(KENNEY_PACKS.keys())
    else:
        try:
            indices = [int(x.strip()) for x in choice.split(",")]
            pack_list = list(KENNEY_PACKS.keys())
            for idx in indices:
                if 1 <= idx <= len(pack_list):
                    selected_packs.append(pack_list[idx-1])
        except ValueError:
            print("❌ 无效输入")
            return
    
    if not selected_packs:
        print("❌ 未选择任何素材包")
        return
    
    print(f"\n✅ 已选择 {len(selected_packs)} 个素材包")
    
    all_integrated = []
    
    for pack_id in selected_packs:
        integrated = download_and_integrate_kenney_pack(pack_id, KENNEY_PACKS[pack_id])
        all_integrated.extend(integrated)
    
    if all_integrated:
        generate_integration_report(all_integrated)
        suggest_config_updates(all_integrated)
    else:
        print("\n⚠️ 未能集成任何资源")


if __name__ == "__main__":
    if "--auto" in sys.argv:
        main_auto_mode()
    else:
        main_interactive_mode()
