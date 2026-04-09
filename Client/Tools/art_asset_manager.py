#!/usr/bin/env python3
"""
Godot Roguelike Game - Art Asset Manager
自动化的美术资源集成工具
支持 Kenney.nl 和 OpenGameArt 免费素材的下载、整理和管理
"""

import os
import sys
import json
import shutil
import urllib.request
import zipfile
import hashlib
from pathlib import Path
from typing import Dict, List, Optional, Tuple
from dataclasses import dataclass, field
from datetime import datetime

@dataclass
class AssetSource:
    """素材来源定义"""
    name: str
    url: str
    source_type: str  # 'kenney', 'opengameart', 'custom'
    license: str
    description: str
    categories: List[str] = field(default_factory=list)
    
@dataclass  
class AssetMapping:
    """资源映射关系"""
    original_file: str
    target_path: str
    category: str
    usage: str

class ArtAssetManager:
    """美术资源管理器"""
    
    def __init__(self, project_root: str):
        self.project_root = Path(project_root)
        self.assets_dir = self.project_root / "Assets_Library"
        self.download_cache = self.assets_dir / "_downloads"
        self.mapping_file = self.assets_dir / "asset_mapping.json"
        self.registry_file = self.assets_dir / "source_registry.json"
        
        # 项目目录结构
        self.target_dirs = {
            "cards": self.project_root / "Icons" / "Cards",
            "enemies_icon": self.project_root / "Icons" / "Enemies",
            "relics": self.project_root / "Icons" / "Relics", 
            "skills": self.project_root / "Icons" / "Skills",
            "items": self.project_root / "Icons" / "Items",
            "achievements": self.project_root / "Icons" / "Achievements",
            "rest_sites": self.project_root / "Icons" / "Rest",
            "services": self.project_root / "Icons" / "Services",
            "characters": self.project_root / "Images" / "Characters",
            "enemies_full": self.project_root / "Images" / "Enemies",
            "backgrounds": self.project_root / "Images" / "Backgrounds",
            "events": self.project_root / "Images" / "Events",
            "potions": self.project_root / "Images" / "Potions",
        }
        
        # 初始化目录结构
        self._init_directories()
        
    def _init_directories(self):
        """初始化必要的目录结构"""
        self.assets_dir.mkdir(exist_ok=True)
        self.download_cache.mkdir(exist_ok=True)
        
        for dir_path in self.target_dirs.values():
            dir_path.mkdir(parents=True, exist_ok=True)
            
    def get_source_registry(self) -> Dict:
        """获取素材源注册表"""
        if self.registry_file.exists():
            with open(self.registry_file, 'r', encoding='utf-8') as f:
                return json.load(f)
        return {
            "version": "1.0.0",
            "last_updated": None,
            "sources": [],
            "statistics": {
                "total_assets": 0,
                "by_category": {},
                "by_source": {}
            }
        }
        
    def save_source_registry(self, registry: Dict):
        """保存素材源注册表"""
        registry["last_updated"] = datetime.now().isoformat()
        with open(self.registry_file, 'w', encoding='utf-8') as f:
            json.dump(registry, f, indent=2, ensure_ascii=False)
            
    def get_asset_mapping(self) -> Dict:
        """获取资源映射表"""
        if self.mapping_file.exists():
            with open(self.mapping_file, 'r', encoding='utf-8') as f:
                return json.load(f)
        return {"mappings": [], "version": "1.0.0"}
        
    def save_asset_mapping(self, mapping: Dict):
        """保存资源映射表"""
        with open(self.mapping_file, 'w', encoding='utf-8') as f:
            json.dump(mapping, f, indent=2, ensure_ascii=False)

    def download_file(self, url: str, destination: Path, description: str = "") -> bool:
        """下载文件（支持进度显示）"""
        print(f"\n📥 下载中: {description or url}")
        print(f"   URL: {url}")
        print(f"   目标: {destination}")
        
        try:
            urllib.request.urlretrieve(url, destination)
            print(f"   ✅ 下载成功!")
            return True
        except Exception as e:
            print(f"   ❌ 下载失败: {e}")
            return False
            
    def extract_zip(self, zip_path: Path, extract_to: Path) -> bool:
        """解压 ZIP 文件"""
        print(f"\n📦 解压中: {zip_path.name}")
        try:
            with zipfile.ZipFile(zip_path, 'r') as zip_ref:
                zip_ref.extractall(extract_to)
            print(f"   ✅ 解压完成!")
            return True
        except Exception as e:
            print(f"   ❌ 解压失败: {e}")
            return False
            
    def find_files_by_pattern(self, directory: Path, patterns: List[str]) -> List[Path]:
        """按模式查找文件"""
        results = []
        for pattern in patterns:
            results.extend(directory.rglob(pattern))
        return results
        
    def copy_asset(self, source: Path, target: Path, backup: bool = True) -> bool:
        """复制资源文件到目标位置"""
        if not source.exists():
            print(f"   ❌ 源文件不存在: {source}")
            return False
            
        # 备份原有文件
        if backup and target.exists():
            backup_path = target.with_suffix(target.suffix + '.backup')
            shutil.copy2(target, backup_path)
            print(f"   📋 已备份原文件: {backup_path.name}")
            
        # 确保目标目录存在
        target.parent.mkdir(parents=True, exist_ok=True)
        
        # 复制文件
        shutil.copy2(source, target)
        print(f"   ✅ 已复制: {target.name}")
        return True

    # ==================== Kenney.nl 集成 ====================
    
    def download_kenney_pack(self, pack_name: str, url: str) -> bool:
        """下载 Kenney.nl 的素材包"""
        pack_dir = self.download_cache / "kenney" / pack_name
        zip_file = pack_dir / f"{pack_name}.zip"
        
        pack_dir.mkdir(parents=True, exist_ok=True)
        
        # 检查是否已下载
        if zip_file.exists():
            print(f"\n✅ {pack_name} 已存在，跳过下载")
            return True
            
        # 下载
        if not self.download_file(url, zip_file, f"Kenney {pack_name}"):
            return False
            
        # 解压
        extract_dir = pack_dir / "extracted"
        if extract_dir.exists():
            shutil.rmtree(extract_dir)
        extract_dir.mkdir(exist_ok=True)
        
        return self.extract_zip(zip_file, extract_dir)
        
    def integrate_kenney_rpg_pack(self):
        """集成 Kenney RPG 素材包"""
        print("\n" + "="*60)
        print("🎮 开始集成 Kenney RPG Asset Pack")
        print("="*60)
        
        # Kenney RPG Pack 下载地址
        rpg_pack_url = "https://kenney.nl/media/pages/assets/rpg-urban-pack/d9c0e6e2b6-1731502215/rpg-urban-pack.zip"
        
        if not self.download_kenney_pack("rpg-urban-pack", rpg_pack_url):
            return
            
        pack_dir = self.download_cache / "kenney" / "rpg-urban-pack" / "extracted"
        
        # 查找所有 PNG 文件
        png_files = self.find_files_by_pattern(pack_dir, ["*.png"])
        print(f"\n📊 找到 {len(png_files)} 个 PNG 文件")
        
        # 分类整理
        mappings = []
        
        for png_file in png_files[:20]:  # 先处理前20个作为示例
            rel_path = png_file.relative_to(pack_dir)
            filename = png_file.name.lower()
            
            # 根据文件名智能分类
            target_category = self._categorize_kenney_asset(filename)
            if target_category:
                target_path = self.target_dirs[target_category] / png_file.name
                if self.copy_asset(png_file, target_path):
                    mappings.append({
                        "source": "kenney-rpg-pack",
                        "original": str(rel_path),
                        "target": str(target_path.relative_to(self.project_root)),
                        "category": target_category,
                        "added_date": datetime.now().isoformat()
                    })
                    
        # 更新映射表
        mapping_data = self.get_asset_mapping()
        mapping_data["mappings"].extend(mappings)
        self.save_asset_mapping(mapping_data)
        
        print(f"\n✅ 成功集成 {len(mappings)} 个资源!")
        
    def _categorize_kenney_asset(self, filename: str) -> Optional[str]:
        """根据文件名分类 Kenney 资源"""
        filename_lower = filename.lower()
        
        # 卡牌相关
        if any(word in filename_lower for word in ['card', 'attack', 'defend', 'skill', 'spell']):
            return "cards"
            
        # 角色相关
        if any(word in filename_lower for word in ['player', 'character', 'hero', 'warrior', 'rogue', 'mage']):
            return "characters"
            
        # 敌人/怪物
        if any(word in filename_lower for word in ['enemy', 'monster', 'creature', 'mob', 'npc']):
            return "enemies_full"
            
        # 物品/道具
        if any(word in filename_lower for word in ['item', 'potion', 'object', 'prop', 'treasure']):
            return "items"
            
        # 图标/UI
        if any(word in filename_lower for word in ['icon', 'ui', 'button', 'panel', 'frame']):
            return "relics"
            
        # 技能效果
        if any(word in filename_lower for word in ['effect', 'particle', 'spark', 'glow', 'fx']):
            return "skills"
            
        # 背景
        if any(word in filename_lower for word in ['background', 'bg', 'scene', 'tilemap']):
            return "backgrounds"
            
        return None

    # ==================== OpenGameArt 集成 ====================
    
    def download_opengameart_asset(self, asset_id: str, url: str, name: str) -> bool:
        """从 OpenGameArt 下载单个资源"""
        asset_dir = self.download_cache / "opengameart" / asset_id
        zip_file = asset_dir / f"{name}.zip"
        
        asset_dir.mkdir(parents=True, exist_ok=True)
        
        if zip_file.exists():
            print(f"\n✅ {name} 已存在")
            return True
            
        if not self.download_file(url, zip_file, f"OpenGameArt: {name}"):
            return False
            
        extract_dir = asset_dir / "extracted"
        if extract_dir.exists():
            shutil.rmtree(extract_dir)
        extract_dir.mkdir(exist_ok=True)
        
        return self.extract_zip(zip_file, extract_dir)
        
    def integrate_opengameart_characters(self):
        """集成 OpenGameArt 角色素材"""
        print("\n" + "="*60)
        print("👤 集成 OpenGameArt 角色素材")
        print("="*60)
        
        # LPC (Liberated Pixel Cup) 角色 - 开源像素艺术角色集
        characters = [
            {
                "id": "lpc-characters",
                "name": "LPC Characters",
                "url": "https://opengameart.org/sites/default/files/lpc_base_sprites.zip",  # 示例URL
                "category": "characters",
                "usage": "角色立绘和战斗动画"
            },
            # 可以继续添加更多角色资源
        ]
        
        for char in characters:
            if self.download_opengameart_asset(char["id"], char["url"], char["name"]):
                print(f"   ✅ 已下载: {char['name']}")
                
    # ==================== 统一工作流接口 ====================
    
    def add_custom_asset(self, source_path: str, target_category: str, 
                         new_filename: str = None, source_info: Dict = None) -> bool:
        """
        添加自定义美术资源（统一工作流入口）
        
        Args:
            source_path: 源文件路径（可以是本地文件或URL）
            target_category: 目标分类 (cards/enemies/relics/skills等)
            new_filename: 新文件名（可选）
            source_info: 来源信息（名称、作者、许可证等）
        
        使用示例:
            manager.add_custom_asset(
                "/path/to/new_card.png",
                "cards",
                "fireball.png",
                {"author": "ArtistName", "license": "CC0"}
            )
        """
        print(f"\n🎨 添加新美术资源")
        print(f"   源: {source_path}")
        print(f"   目标分类: {target_category}")
        
        source = Path(source_path)
        
        # 验证分类
        if target_category not in self.target_dirs:
            print(f"❌ 无效的分类: {target_category}")
            print(f"   可用分类: {list(self.target_dirs.keys())}")
            return False
            
        # 处理源文件
        if source.exists():
            # 本地文件
            actual_source = source
        elif source_path.startswith('http'):
            # URL下载
            filename = new_filename or source_path.split('/')[-1].split('?')[0]
            temp_file = self.download_cache / "custom" / filename
            self.download_cache.mkdir(parents=True, exist_ok=True)
            
            if not self.download_file(source_path, temp_file, f"自定义资源: {filename}"):
                return False
            actual_source = temp_file
        else:
            print(f"❌ 文件不存在或无法访问: {source_path}")
            return False
            
        # 确定目标文件名
        target_filename = new_filename or actual_source.name
        target_path = self.target_dirs[target_category] / target_filename
        
        # 复制文件
        if self.copy_asset(actual_source, target_path):
            # 记录到映射表
            mapping_data = self.get_asset_mapping()
            new_mapping = {
                "source": source_info.get("name", "custom") if source_info else "custom",
                "original": str(actual_source),
                "target": str(target_path.relative_to(self.project_root)),
                "category": target_category,
                "added_date": datetime.now().isoformat(),
                "metadata": source_info or {}
            }
            mapping_data["mappings"].append(new_mapping)
            self.save_asset_mapping(mapping_data)
            
            # 更新注册表
            registry = self.get_source_registry()
            registry["statistics"]["total_assets"] += 1
            registry["statistics"]["by_category"][target_category] = \
                registry["statistics"]["by_category"].get(target_category, 0) + 1
            self.save_source_registry(registry)
            
            print(f"\n✅ 资源添加成功!")
            print(f"   位置: {target_path}")
            return True
            
        return False
        
    def batch_import_from_directory(self, source_dir: str, target_category: str, 
                                    pattern: str = "*.png") -> int:
        """
        从目录批量导入资源
        
        Args:
            source_dir: 源目录
            target_category: 目标分类
            pattern: 文件匹配模式
            
        Returns:
            成功导入的数量
        """
        print(f"\n📁 批量导入资源")
        print(f"   源目录: {source_dir}")
        print(f"   目标分类: {target_category}")
        
        source_path = Path(source_dir)
        if not source_path.exists():
            print(f"❌ 目录不存在: {source_dir}")
            return 0
            
        files = list(source_path.glob(pattern))
        print(f"   找到 {len(files)} 个文件")
        
        success_count = 0
        for file_path in files:
            if self.add_custom_asset(str(file_path), target_category):
                success_count += 1
                
        print(f"\n✅ 成功导入 {success_count}/{len(files)} 个资源")
        return success_count
        
    def generate_resource_report(self) -> Dict:
        """生成资源报告"""
        print("\n" + "="*60)
        print("📊 美术资源统计报告")
        print("="*60)
        
        mapping_data = self.get_asset_mapping()
        registry = self.get_source_registry()
        
        report = {
            "total_mappings": len(mapping_data.get("mappings", [])),
            "by_category": {},
            "by_source": {},
            "recent_additions": []
        }
        
        # 按分类统计
        for mapping in mapping_data.get("mappings", []):
            cat = mapping.get("category", "unknown")
            src = mapping.get("source", "unknown")
            
            report["by_category"][cat] = report["by_category"].get(cat, 0) + 1
            report["by_source"][src] = report["by_source"].get(src, 0) + 1
            
        # 最近添加的资源
        recent = sorted(
            mapping_data.get("mappings", []),
            key=lambda x: x.get("added_date", ""),
            reverse=True
        )[:10]
        report["recent_additions"] = recent
        
        # 打印报告
        print(f"\n总资源数: {report['total_mappings']}")
        print(f"\n按分类:")
        for cat, count in sorted(report["by_category"].items()):
            print(f"  • {cat}: {count}")
            
        print(f"\n按来源:")
        for src, count in sorted(report["by_source"].items()):
            print(f"  • {src}: {count}")
            
        return report
        
    def list_missing_assets(self) -> List[Dict]:
        """列出缺失的资源（对比配置文件需求）"""
        print("\n" + "="*60)
        print("🔍 检查缺失资源")
        print("="*60)
        
        missing = []
        
        # 检查卡牌配置
        cards_config = self.project_root / "Config" / "Data" / "cards.json"
        if cards_config.exists():
            with open(cards_config, 'r', encoding='utf-8') as f:
                cards = json.load(f).get("cards", [])
                
            for card in cards:
                icon_path = card.get("iconPath", "")
                if icon_path.startswith("res://"):
                    icon_path = icon_path[6:]
                    
                full_path = self.project_root / icon_path
                if not full_path.exists():
                    missing.append({
                        "type": "card_icon",
                        "id": card["id"],
                        "name": card["name"],
                        "path": icon_path,
                        "required": True
                    })
                    
        # 检查角色配置
        characters_config = self.project_root / "Config" / "Data" / "characters.json"
        if characters_config.exists():
            with open(characters_config, 'r', encoding='utf-8') as f:
                chars = json.load(f).get("characters", [])
                
            for char in chars:
                portrait_path = char.get("portraitPath", "")
                if portrait_path.startswith("res://"):
                    portrait_path = portrait_path[6:]
                    
                full_path = self.project_root / portrait_path
                if not full_path.exists():
                    missing.append({
                        "type": "character_portrait",
                        "id": char["id"],
                        "name": char["name"],
                        "path": portrait_path,
                        "required": True
                    })
                    
        # 检查敌人配置
        enemies_config = self.project_root / "Config" / "Data" / "enemies.json"
        if enemies_config.exists():
            with open(enemies_config, 'r', encoding='utf-8') as f:
                enemies = json.load(f).get("enemies", [])
                
            for enemy in enemies:
                for path_key in ["portraitPath", "iconPath"]:
                    path_val = enemy.get(path_key, "")
                    if path_val.startswith("res://"):
                        path_val = path_val[6:]
                        
                    full_path = self.project_root / path_val
                    if not full_path.exists():
                        missing.append({
                            "type": f"enemy_{path_key.replace('Path', '').lower()}",
                            "id": enemy["id"],
                            "name": enemy["name"],
                            "path": path_val,
                            "required": True
                        })
                        
        print(f"\n发现 {len(missing)} 个缺失资源:")
        for item in missing:
            print(f"  ❌ [{item['type']}] {item['name']} ({item['id']})")
            print(f"      路径: {item['path']}")
            
        return missing
        
    def create_integration_guide(self):
        """生成资源集成指南文档"""
        guide_content = """# 🎨 Godot Roguelike 美术资源集成指南

## 📋 快速开始

### 1. 添加单个资源
```bash
python art_asset_manager.py add --source /path/to/image.png --category cards --name my_card.png
```

### 2. 批量导入目录
```bash
python art_asset_manager.py batch-import --source ./my_art/ --category skills --pattern "*.png"
```

### 3. 从 URL 下载并集成
```bash
python art_asset_manager.py add --source https://example.com/image.png --category relics --name cool_relic.png
```

### 4. 下载 Kenney 免费素材包
```bash
python art_asset_manager.py kenney-download
```

### 5. 查看资源状态
```bash
python art_asset_manager.py status
python art_asset_manager.py missing
```

## 📂 资源分类说明

| 分类代码 | 目录 | 用途 | 推荐尺寸 |
|---------|------|------|---------|
| `cards` | Icons/Cards/ | 卡牌图标 | 120×170px |
| `enemies_icon` | Icons/Enemies/ | 敌人头像 | 64×64px 或 128×128px |
| `relics` | Icons/Relics/ | 遗物图标 | 64×64px |
| `skills` | Icons/Skills/ | 技能图标 | 64×64px |
| `items` | Icons/Items/ | 物品图标 | 48×48px |
| `achievements` | Icons/Achievements/ | 成就图标 | 64×64px |
| `rest_sites` | Icons/Rest/ | 休息站图标 | 64×64px |
| `services` | Icons/Services/ | 服务图标 | 64×64px |
| `characters` | Images/Characters/ | 角色立绘 | 400×600px |
| `enemies_full` | Images/Enemies/ | 敌人战斗图 | 200×300px |
| `backgrounds` | Images/Backgrounds/ | 背景图 | 1920×1080px |
| `events` | Images/Events/ | 事件插图 | 400×300px |
| `potions` | Images/Potions/ | 药水图片 | 64×64px |

## 🔗 推荐免费资源网站

### Kenney.nl (CC0 免费商用)
- **网址**: https://kenney.nl/assets
- **推荐包**:
  - RPG Urban Pack (城市RPG素材)
  - Platformer Pack (平台跳跃素材)
  - UI Pack (界面元素)
  - Particle Effects (粒子特效)

### OpenGameArt (多种授权)
- **网址**: https://opengameart.org
- **推荐搜索关键词**: "rpg", "pixel art", "fantasy", "cards"

### 其他资源
- Itch.io 免费素材: https://itch.io/game-assets/free
- Game-Icons.net: https://game-icons.net (3000+ CC BY 3.0 图标)

## 🛠️ 工作流程

### 日常开发流程
1. ✅ 找到合适的免费素材
2. ✅ 使用 `add` 命令集成到项目
3. ✅ 更新 Config/Data/*.json 配置文件中的路径
4. ✅ 在 Godot 编辑器中验证显示效果
5. ✅ 提交版本控制

### 批量替换流程
1. ✅ 准备好新的素材集合
2. ✅ 使用 `batch-import` 导入
3. ✅ 运行 `missing` 检查遗漏
4. ✅ 更新 JSON 配置
5. ✅ 测试游戏功能

## 📝 配置文件格式

添加资源时可以附带元数据:

```json
{{
  "author": "艺术家名称",
  "license": "CC0",
  "source_url": "原始下载链接",
  "notes": "使用说明"
}}
```

## ⚠️ 注意事项

1. **授权合规**: 确保使用的素材符合项目授权要求
2. **尺寸规范**: 尽量遵循推荐尺寸以保证显示效果
3. **命名规范**: 使用英文小写+下划线命名
4. **备份机制**: 工具会自动备份被替换的原文件
5. **格式要求**: 优先使用 PNG 格式（支持透明通道）

## 📞 常见问题

**Q: 如何处理不同尺寸的资源?**
A: 工具会保持原始尺寸，建议在导入前使用图像编辑软件调整。

**Q: 支持哪些图片格式?**
A: PNG (推荐), JPG, WebP, BMP。

**Q: 如何撤销某个资源的替换?**
A: 查找 `.backup` 后缀的文件恢复即可。

---

最后更新: {date}
""".format(date=datetime.now().strftime("%Y-%m-%d %H:%M:%S"))
        
        guide_path = self.project_root / "ART_ASSET_GUIDE.md"
        with open(guide_path, 'w', encoding='utf-8') as f:
            f.write(guide_content)
            
        print(f"\n📝 已生成集成指南: {guide_path}")
        return guide_path


# ==================== CLI 命令行接口 ====================

def main():
    """命令行主入口"""
    if len(sys.argv) < 2:
        print("""
🎨 Godot Roguelike 美术资源管理器

用法:
  python art_asset_manager.py <command> [options]

命令:
  init                          初始化资源管理系统
  status                        查看资源统计
  missing                       检查缺失资源
  add --source <path> --cat <category> [--name <filename>]
                                添加单个资源
  batch-import --source <dir> --cat <category> [--pattern <glob>]
                                批量导入目录
  kenney-download               下载 Kenney 免费素材包
  oga-download                  下载 OpenGameArt 素材
  guide                         生成集成指南文档
  
示例:
  python art_asset_manager.py add --source card.png --category cards --name strike.png
  python art_asset_manager.py batch-import --source ./new_art/ --category relics
  python art_asset_manager.py status
  python art_asset_manager.py missing
""")
        sys.exit(1)
        
    command = sys.argv[1]
    project_root = Path(__file__).parent
    
    manager = ArtAssetManager(str(project_root))
    
    if command == "init":
        print("\n✅ 资源管理系统已初始化!")
        print(f"   项目根目录: {project_root}")
        print(f"   资源库目录: {manager.assets_dir}")
        
    elif command == "status":
        manager.generate_resource_report()
        
    elif command == "missing":
        manager.list_missing_assets()
        
    elif command == "add":
        import argparse
        parser = argparse.ArgumentParser()
        parser.add_argument("--source", required=True, help="源文件路径或URL")
        parser.add_argument("--cat", "--category", required=True, help="目标分类")
        parser.add_argument("--name", help="新文件名")
        args = parser.parse_args(sys.argv[2:])
        
        manager.add_custom_asset(args.source, args.cat, args.name)
        
    elif command == "batch-import":
        import argparse
        parser = argparse.ArgumentParser()
        parser.add_argument("--source", required=True, help="源目录")
        parser.add_argument("--cat", "--category", required=True, help="目标分类")
        parser.add_argument("--pattern", default="*.png", help="文件匹配模式")
        args = parser.parse_args(sys.argv[2:])
        
        manager.batch_import_from_directory(args.source, args.cat, args.pattern)
        
    elif command == "kenney-download":
        manager.integrate_kenney_rpg_pack()
        
    elif command == "oga-download":
        manager.integrate_opengameart_characters()
        
    elif command == "guide":
        manager.create_integration_guide()
        
    else:
        print(f"❌ 未知命令: {command}")
        sys.exit(1)


if __name__ == "__main__":
    main()
