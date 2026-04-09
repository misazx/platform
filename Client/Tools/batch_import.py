#!/usr/bin/env python3
"""
Godot Roguelike - 美术资源批量导入工具
用于将已下载的素材包快速整理到项目目录

用法:
  python3 batch_import.py <source_directory> [category]

示例:
  # 导入整个目录到指定分类
  python3 batch_import.py ~/Downloads/kenney-rpg-pack cards
  
  # 智能分类导入（自动识别类型）
  python3 batch_import.py ~/Downloads/my_art_assets
  
  # 批量导入多个来源
  python3 batch_import.py ~/Downloads/kenney-ui ~/Downloads/opengameart-sprites
"""

import os
import sys
import json
import shutil
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Optional


PROJECT_ROOT = Path(__file__).parent
TARGET_DIRS = {
    "cards": PROJECT_ROOT / "Icons" / "Cards",
    "enemies_icon": PROJECT_ROOT / "Icons" / "Enemies",
    "relics": PROJECT_ROOT / "Icons" / "Relics",
    "skills": PROJECT_ROOT / "Icons" / "Skills",
    "items": PROJECT_ROOT / "Icons" / "Items",
    "achievements": PROJECT_ROOT / "Icons" / "Achievements",
    "rest_sites": PROJECT_ROOT / "Icons" / "Rest",
    "services": PROJECT_ROOT / "Icons" / "Services",
    "characters": PROJECT_ROOT / "Images" / "Characters",
    "enemies_full": PROJECT_ROOT / "Images" / "Enemies",
    "backgrounds": PROJECT_ROOT / "Images" / "Backgrounds",
    "events": PROJECT_ROOT / "Images" / "Events",
    "potions": PROJECT_ROOT / "Images" / "Potions",
}

# 文件名关键词映射到分类规则
CATEGORY_RULES = {
    "cards": [
        "card", "attack", "strike", "slash", "weapon", "sword", "axe", "bow",
        "shield", "defend", "block", "armor", "helm", "potion", "spell",
        "magic", "fireball", "ice", "lightning", "heal", "cure"
    ],
    "skills": [
        "skill", "effect", "particle", "spark", "glow", "fx", "burst",
        "explosion", "flash", "beam", "wave", "dash", "jump"
    ],
    "characters": [
        "player", "character", "hero", "warrior", "knight", "mage", "rogue",
        "archer", "wizard", "human", "elf", "dwarf", "male", "female"
    ],
    "enemies_full": [
        "enemy", "monster", "creature", "slime", "goblin", "orc", "dragon",
        "demon", "undead", "zombie", "skeleton", "wolf", "bat", "spider",
        "boss", "miniboss", "mob", "npc_enemy"
    ],
    "items": [
        "item", "coin", "treasure", "chest", "key", "food", "meat",
        "bread", "apple", "gem", "crystal", "gold", "silver"
    ],
    "relics": [
        "relic", "artifact", "amulet", "ring", "talisman", "charm",
        "button", "panel", "frame", "box", "ui", "icon_", "symbol",
        "badge", "medal", "emblem", "scroll", "book", "map"
    ],
    "backgrounds": [
        "background", "bg_", "ground", "tileset", "tile_map", "scene",
        "forest", "cave", "castle", "dungeon", "town", "village"
    ],
}


def print_header():
    """打印标题"""
    print("""
╔═════════════════════════════════════════════╗
║  🎨 Godot Roguelike 美术资源批量导入工具   ║
║                                             ║
║  快速整理已下载的美术素材到项目目录         ║
╚═════════════════════════════════════════════╝
    """)


def ensure_directories():
    """确保目标目录存在"""
    for dir_path in TARGET_DIRS.values():
        dir_path.mkdir(parents=True, exist_ok=True)


def find_image_files(directory: Path) -> List[Path]:
    """查找所有图片文件"""
    extensions = {'.png', '.jpg', '.jpeg', '.webp', '.bmp', '.gif'}
    images = []
    
    for ext in extensions:
        images.extend(directory.rglob(f"*{ext}"))
        images.extend(directory.rglob(f"*{ext.upper()}"))
    
    return sorted(set(images))


def categorize_by_filename(filename: str) -> Optional[str]:
    """根据文件名智能判断分类"""
    name_lower = filename.lower().replace(" ", "_").replace("-", "_")
    
    for category, keywords in CATEGORY_RULES.items():
        for keyword in keywords:
            if keyword in name_lower:
                return category
    
    return None


def categorize_file(file_path: Path, forced_category: Optional[str] = None) -> str:
    """确定文件的分类"""
    if forced_category and forced_category in TARGET_DIRS:
        return forced_category
    
    auto_category = categorize_by_filename(file_path.name)
    
    if auto_category:
        return auto_category
    
    # 默认归类到 relics (通用图标)
    return "relics"


def import_single_file(source: Path, target_dir: Path, backup: bool = True) -> bool:
    """导入单个文件"""
    target_path = target_dir / source.name
    
    # 跳过已存在的非占位符文件
    if target_path.exists():
        size_kb = target_path.stat().st_size / 1024
        if size_kb > 15:  # 不是小占位符
            return False
        
        # 备份小的占位符文件
        if backup:
            backup_path = target_path.with_suffix('.backup')
            shutil.copy2(target_path, backup_path)
    
    try:
        shutil.copy2(source, target_path)
        return True
    except Exception as e:
        print(f"   ❌ 复制失败: {e}")
        return False


def batch_import(source_dirs: List[str], forced_category: Optional[str] = None) -> Dict:
    """批量导入"""
    print_header()
    ensure_directories()
    
    stats = {
        "total_found": 0,
        "total_imported": 0,
        "total_skipped": 0,
        "by_category": {},
        "imported_files": [],
        "timestamp": datetime.now().isoformat()
    }
    
    all_sources = []
    for src in source_dirs:
        source_path = Path(src).expanduser().resolve()
        
        if not source_path.exists():
            print(f"⚠️ 目录不存在: {source_path}")
            continue
            
        if source_path.is_file():
            all_sources.append(source_path.parent)
        else:
            all_sources.append(source_path)
    
    if not all_sources:
        print("❌ 没有有效的源目录")
        return stats
    
    print(f"\n📂 源目录:")
    for src in all_sources:
        print(f"   • {src}")
    
    if forced_category:
        print(f"\n🎯 强制分类: {forced_category}")
    else:
        print(f"\n🔍 使用智能分类模式")
    
    print("\n" + "="*60)
    print("开始导入...")
    print("="*60 + "\n")
    
    for source_dir in all_sources:
        image_files = find_image_files(source_dir)
        stats["total_found"] += len(image_files)
        
        print(f"📁 处理: {source_dir.name}")
        print(f"   找到 {len(image_files)} 个图片文件\n")
        
        for img_file in image_files:
            category = categorize_file(img_file, forced_category)
            target_dir = TARGET_DIRS[category]
            
            if import_single_file(img_file, target_dir):
                stats["total_imported"] += 1
                stats["by_category"][category] = stats["by_category"].get(category, 0) + 1
                
                relative_target = target_dir / img_file.name
                stats["imported_files"].append({
                    "source": str(img_file),
                    "target": str(relative_target.relative_to(PROJECT_ROOT)),
                    "category": category
                })
                
                print(f"  ✅ [{category:15s}] {img_file.name}")
            else:
                stats["total_skipped"] += 1
                print(f"  ⏭️ [{category:15s}] {img_file.name} (已存在)")
        
        print()
    
    # 保存报告
    report_path = PROJECT_ROOT / "Assets_Library" / "import_report.json"
    report_path.parent.mkdir(exist_ok=True)
    
    with open(report_path, 'w', encoding='utf-8') as f:
        json.dump(stats, f, indent=2, ensure_ascii=False, default=str)
    
    # 打印统计
    print("="*60)
    print("📊 导入完成！统计:")
    print("="*60)
    print(f"\n总发现: {stats['total_found']} 个文件")
    print(f"成功导入: {stats['total_imported']} 个文件")
    print(f"跳过: {stats['total_skipped']} 个文件\n")
    
    print("按分类:")
    for cat, count in sorted(stats["by_category"].items()):
        print(f"  📁 {cat}: {count} 个")
    
    print(f"\n📝 详细报告: {report_path}")
    
    return stats


def generate_config_suggestions(import_stats: Dict):
    """生成配置更新建议"""
    suggestions = {
        "generated_at": datetime.now().isoformat(),
        "instructions": """
请根据以下建议更新 Config/Data/ 目录下的配置文件:

1. cards.json - 更新卡牌的 iconPath 字段
2. characters.json - 更新角色的 portraitPath 字段  
3. enemies.json - 更新敌人的 portraitPath 和 iconPath 字段
4. relics.json - 更新遗物的 iconPath 和 imagePath 字段
5. potions.json - 更新药水的 imagePath 字段

路径格式示例: "res://Icons/Cards/new_card.png"
        """,
        "by_category": {}
    }
    
    by_cat = import_stats.get("by_category", {})
    imported = import_stats.get("imported_files", [])
    
    for cat, files_group in [
        ("cards", [f for f in imported if f["category"] == "cards"]),
        ("characters", [f for f in imported if f["category"] == "characters"]),
        ("enemies_full", [f for f in imported if f["category"] == "enemies_full"]),
        ("relics", [f for f in imported if f["category"] == "relics"]),
        ("items", [f for f in imported if f["category"] == "items"]),
        ("skills", [f for f in imported if f["category"] == "skills"]),
    ]:
        if files_group:
            suggestions["by_category"][cat] = {
                "count": len(files_group),
                "available_files": [f["target"] for f in files_group[:30]],
                "config_field": _get_config_field_for_category(cat)
            }
    
    suggestion_path = PROJECT_ROOT / "Assets_Library" / "config_suggestions.json"
    with open(suggestion_path, 'w', encoding='utf-8') as f:
        json.dump(suggestions, f, indent=2, ensure_ascii=False)
    
    print(f"\n💡 配置建议已保存: {suggestion_path}")


def _get_config_field_for_category(category: str) -> str:
    fields = {
        "cards": "iconPath",
        "characters": "portraitPath",
        "enemies_full": ["portraitPath", "iconPath"],
        "relics": ["iconPath", "imagePath"],
        "items": "imagePath",
        "skills": "iconPath",
    }
    return fields.get(category, "imagePath")


def list_available_categories():
    """列出所有可用的分类"""
    print("\n可用的资源分类:\n")
    print(f"{'分类代码':<20} {'目标目录':<40} {'用途'}")
    print("-" * 80)
    
    for code, dir_path in TARGET_DIRS.items():
        rel_path = dir_path.relative_to(PROJECT_ROOT)
        usage = _get_category_usage(code)
        print(f"{code:<20} {str(rel_path):<40} {usage}")


def _get_category_usage(code: str) -> str:
    usages = {
        "cards": "卡牌图标 (120×170px)",
        "enemies_icon": "敌人头像 (64×64px)",
        "relics": "遗物/道具图标 (64×64px)",
        "skills": "技能图标 (64×64px)",
        "items": "物品图标 (48×48px)",
        "achievements": "成就图标 (64×64px)",
        "rest_sites": "休息站图标 (64×64px)",
        "services": "服务图标 (64×64px)",
        "characters": "角色立绘 (400×600px)",
        "enemies_full": "敌人战斗图 (200×300px)",
        "backgrounds": "背景图 (1920×1080px)",
        "events": "事件插图 (400×300px)",
        "potions": "药水图片 (64×64px)",
    }
    return usages.get(code, "通用")


def main():
    if len(sys.argv) < 2:
        print("""
🎨 Godot Roguelike 美术资源批量导入工具

用法:
  python3 batch_import.py <source_directory> [category]
  python3 batch_import.py --list-categories
  python3 batch_import.py --help

参数:
  source_directory    素材所在目录（支持多个，用空格分隔）
  category           可选：强制分类 (cards/relics/skills 等)

示例:
  # 智能分类导入
  python3 batch_import.py ~/Downloads/kenney-assets
  
  # 导入到指定分类
  python3 batch_import.py ~/Downloads/card-icons cards
  
  # 多个目录同时导入
  python3 batch_import.py ~/Downloads/pack1 ~/Downloads/pack2
  
  # 列出所有可用分类
  python3 batch_import.py --list-categories
""")
        sys.exit(0)
    
    arg1 = sys.argv[1]
    
    if arg1 == "--list-categories":
        list_available_categories()
        return
    
    if arg1 == "--help":
        main()
        return
    
    # 收集源目录
    source_dirs = []
    forced_category = None
    
    i = 1
    while i < len(sys.argv):
        arg = sys.argv[i]
        
        if arg.startswith("--"):
            if i + 1 < len(sys.argv) and not sys.argv[i+1].startswith("-"):
                forced_category = sys.argv[i+1]
                i += 2
            else:
                i += 1
        else:
            source_dirs.append(arg)
            i += 1
    
    if not source_dirs:
        print("❌ 请提供至少一个源目录")
        sys.exit(1)
    
    # 执行导入
    stats = batch_import(source_dirs, forced_category)
    
    # 生成配置建议
    if stats["total_imported"] > 0:
        generate_config_suggestions(stats)
        
        print("\n" + "="*60)
        print("✅ 下一步操作:")
        print("="*60)
        print("""
1. 查看 Assets_Library/config_suggestions.json 了解可用的资源
2. 编辑 Config/Data/*.json 更新资源配置
3. 在 Godot 编辑器中打开项目验证效果
4. 运行游戏测试显示是否正常
""")


if __name__ == "__main__":
    main()
