#!/usr/bin/env python3
"""
Godot Roguelike - 配置文件自动更新器
根据导入的资源，自动建议/更新 Config/Data/*.json 中的资源路径

用法:
  python3 update_configs.py --preview    # 预览建议（不修改）
  python3 update_configs.py --auto       # 自动更新（推荐）
  python3 update_configs.py --cards      # 仅更新卡牌
  python3 update_configs.py --interactive # 交互式选择
"""

import os
import sys
import json
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Optional, Tuple


PROJECT_ROOT = Path(__file__).parent
CONFIG_DIR = PROJECT_ROOT / "Config" / "Data"
ASSETS_DIR = PROJECT_ROOT / "Assets_Library"
ICONS_DIR = PROJECT_ROOT / "Icons"
IMAGES_DIR = PROJECT_ROOT / "Images"
AUDIO_DIR = PROJECT_ROOT / "Audio"


def find_available_assets(category: str) -> List[str]:
    """查找指定分类下的所有可用资源文件"""
    if category in ["cards"]:
        base_dir = ICONS_DIR / "Cards"
    elif category in ["enemies_icon"]:
        base_dir = ICONS_DIR / "Enemies"
    elif category in ["relics", "skills", "items"]:
        base_dir = ICONS_DIR / category.capitalize() if category != "relics" else ICONS_DIR / "Relics"
        if not base_dir.exists():
            base_dir = ICONS_DIR / category
    elif category == "characters":
        base_dir = IMAGES_DIR / "Characters"
    elif category == "enemies_full":
        base_dir = IMAGES_DIR / "Enemies"
    elif category == "potions":
        base_dir = IMAGES_DIR / "Potions"
    else:
        return []
    
    if not base_dir.exists():
        return []
    
    extensions = {'.png', '.jpg', '.jpeg', '.webp'}
    files = []
    
    for ext in extensions:
        files.extend([f.name for f in base_dir.glob(f"*{ext}")])
        files.extend([f.name for f in base_dir.glob(f"*{ext.upper()}")])
    
    return sorted(set(files))


def load_config(config_name: str) -> Optional[Dict]:
    """加载配置文件"""
    config_path = CONFIG_DIR / f"{config_name}.json"
    
    if not config_path.exists():
        print(f"⚠️ 配置文件不存在: {config_path}")
        return None
    
    with open(config_path, 'r', encoding='utf-8') as f:
        return json.load(f)


def save_config(config_name: str, data: Dict):
    """保存配置文件（带备份）"""
    config_path = CONFIG_DIR / f"{config_name}.json"
    
    # 备份原文件
    backup_path = config_path.with_suffix('.json.backup')
    if config_path.exists():
        import shutil
        shutil.copy2(config_path, backup_path)
    
    with open(config_path, 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=2, ensure_ascii=False)


def suggest_card_updates(cards_data: Dict) -> List[Dict]:
    """为卡牌生成更新建议"""
    available_icons = find_available_assets("cards")
    suggestions = []
    
    for card in cards_data.get("cards", []):
        current_icon = card.get("iconPath", "")
        
        # 智能匹配：根据卡牌名称/ID找相似图标
        card_id = card.get("id", "").lower()
        card_name = card.get("name", "").lower()
        card_type = card.get("type", "").lower()
        
        best_match = None
        
        # 策略1: ID 直接匹配
        for icon in available_icons:
            icon_lower = icon.lower().replace(".png", "")
            if icon_lower in card_id or card_id.replace("_", "") in icon_lower:
                best_match = icon
                break
        
        # 策略2: 名称关键词匹配
        if not best_match:
            keywords = {
                "strike": ["strike", "attack", "sword", "slash"],
                "defend": ["defend", "shield", "block", "armor"],
                "bash": ["bash", "heavy", "crush"],
                "cleave": ["cleave", "sweep", "aoe"],
                "iron_wave": ["wave", "iron", "metal"],
            }
            
            for keyword, patterns in keywords.items():
                if keyword in card_id or keyword in card_name:
                    for pattern in patterns:
                        for icon in available_icons:
                            if pattern in icon.lower():
                                best_match = icon
                                break
                        if best_match:
                            break
                    break
        
        # 策略3: 类型匹配
        if not best_match:
            type_keywords = {
                "attack": ["attack", "sword", "weapon", "damage"],
                "skill": ["skill", "magic", "spell"],
            }
            
            for type_key, patterns in type_keywords.items():
                if type_key in card_type:
                    for pattern in patterns[:3]:  # 只取前几个
                        for icon in available_icons:
                            if pattern in icon.lower():
                                best_match = icon
                                break
                        if best_match:
                            break
        
        if best_match and best_match != Path(current_icon).name:
            suggestions.append({
                "card_id": card["id"],
                "card_name": card["name"],
                "current": current_icon,
                "suggested": f"res://Icons/Cards/{best_match}",
                "confidence": "high" if any(kw in best_match.lower() for kw in [card_id.split("_")[0], card_name.split("")[0]]) else "medium"
            })
    
    return suggestions


def suggest_character_updates(chars_data: Dict) -> List[Dict]:
    """为角色生成更新建议"""
    available_portraits = find_available_assets("characters")
    suggestions = []
    
    for char in chars_data.get("characters", []):
        current = char.get("portraitPath", "")
        char_id = char.get("id", "").lower()
        
        # 尝试找匹配的角色立绘
        best_match = None
        for portrait in available_portraits:
            portrait_lower = portrait.lower().replace(".png", "")
            if char_id in portrait_lower or portrait_lower in char_id:
                best_match = portrait
                break
        
        # 如果没找到精确匹配，尝试首字母或类型匹配
        if not best_match:
            char_class = char.get("class", "").lower()
            class_matches = {
                "ironclad": ["warrior", "knight", "fighter", "soldier"],
                "silent": ["rogue", "thief", "assassin", "ninja", "ranger"],
                "defect": ["robot", "golem", "machine", "android", "wizard"],
                "watcher": ["mage", "priest", "monk", "mystic", "female"],
                "necromancer": ["necromancer", "dark", "evil", "wizard_male"],
            }
            
            matches = class_matches.get(char_id, class_matches.get(char_class, []))
            for match_word in matches:
                for portrait in available_portraits:
                    if match_word in portrait.lower():
                        best_match = portrait
                        break
                if best_match:
                    break
        
        if best_match and best_match != Path(current).name:
            suggestions.append({
                "char_id": char["id"],
                "char_name": char["name"],
                "current": current,
                "suggested": f"res://Images/Characters/{best_match}",
                "confidence": "high" if char_id in best_match.lower() else "medium"
            })
    
    return suggestions


def suggest_enemy_updates(enemies_data: Dict) -> List[Dict]:
    """为敌人生成更新建议"""
    available_icons = find_available_assets("enemies_icon")
    available_full = find_available_assets("enemies_full")
    suggestions = []
    
    for enemy in enemies_data.get("enemies", []):
        enemy_id = enemy.get("id", "").lower().replace("_", " ")
        
        # 图标更新
        current_icon = enemy.get("iconPath", "")
        best_icon = None
        for icon in available_icons:
            icon_lower = icon.lower().replace(".png", "")
            if any(word in icon_lower for word in enemy_id.split()):
                best_icon = icon
                break
        
        if best_icon and best_icon != Path(current_icon).name:
            suggestions.append({
                "enemy_id": enemy["id"],
                "enemy_name": enemy["name"],
                "field": "iconPath",
                "current": current_icon,
                "suggested": f"res://Icons/Enemies/{best_icon}"
            })
        
        # 全身图更新
        current_portrait = enemy.get("portraitPath", "")
        best_full = None
        for full in available_full:
            full_lower = full.lower().replace(".png", "")
            if any(word in full_lower for word in enemy_id.split()):
                best_full = full
                break
        
        if best_full and best_full != Path(current_portrait).name:
            suggestions.append({
                "enemy_id": enemy["id"],
                "enemy_name": enemy["name"],
                "field": "portraitPath",
                "current": current_portrait,
                "suggested": f"res://Images/Enemies/{best_full}"
            })
    
    return suggestions


def apply_suggestions(config_name: str, data: Dict, suggestions: List[Dict]) -> int:
    """应用更新建议到配置数据"""
    applied = 0
    
    if config_name == "cards":
        cards_list = data.get("cards", [])
        for sug in suggestions:
            for card in cards_list:
                if card["id"] == sug["card_id"]:
                    old_value = card.get("iconPath", "")
                    card["iconPath"] = sug["suggested"]
                    applied += 1
                    print(f"  ✅ {card['name']}: {Path(old_value).name} → {Path(sug['suggested']).name}")
                    break
    
    elif config_name == "characters":
        chars_list = data.get("characters", [])
        for sug in suggestions:
            for char in chars_list:
                if char["id"] == sug["char_id"]:
                    old_value = char.get("portraitPath", "")
                    char["portraitPath"] = sug["suggested"]
                    applied += 1
                    print(f"  ✅ {char['name']}: {Path(old_value).name} → {Path(sug['suggested']).name}")
                    break
    
    elif config_name == "enemies":
        enemies_list = data.get("enemies", [])
        for sug in suggestions:
            for enemy in enemies_list:
                if enemy["id"] == sug["enemy_id"] and sug["field"] in enemy:
                    old_value = enemy.get(sug["field"], "")
                    enemy[sug["field"]] = sug["suggested"]
                    applied += 1
                    print(f"  ✅ {enemy['name']}.{sug['field']}: {Path(old_value).name} → {Path(sug['suggested']).name}")
                    break
    
    return applied


def main():
    """主函数"""
    if len(sys.argv) < 2 or "--help" in sys.argv:
        print("""
🎮 Godot Roguelike - 配置文件自动更新器

用法:
  python3 update_configs.py [选项]

选项:
  --preview       预览更新建议（不修改文件）
  --auto          自动应用所有建议（推荐）
  --cards         仅处理卡牌配置
  --characters    仅处理角色配置
  --enemies       仅处理敌人配置
  --all           处理所有配置
  --interactive   交互式逐个确认
  --help          显示帮助信息

示例:
  python3 update_configs.py --preview     # 先看看有什么可以更新的
  python3 update_configs.py --auto        # 一键全部更新
""")
        sys.exit(0)
    
    mode = sys.argv[1]
    
    print("\n" + "="*70)
    print("🎮 Godot Roguelike - 配置文件更新器")
    print("="*70 + "\n")
    
    total_suggestions = 0
    
    configs_to_process = []
    
    if "--cards" in sys.argv or "--all" in sys.argv or mode in ["--auto", "--preview", "--interactive"]:
        configs_to_process.append(("cards", "卡牌配置"))
    if "--characters" in sys.argv or "--all" in sys.argv or mode in ["--auto", "--preview", "--interactive"]:
        configs_to_process.append(("characters", "角色配置"))
    if "--enemies" in sys.argv or "--all" in sys.argv or mode in ["--auto", "--preview", "--interactive"]:
        configs_to_process.append(("enemies", "敌人配置"))
    
    all_suggestions_by_config = {}
    
    for config_name, description in configs_to_process:
        print(f"\n{'─'*70}")
        print(f"📋 分析: {description} ({config_name}.json)")
        print(f"{'─'*70}\n")
        
        data = load_config(config_name)
        if not data:
            continue
        
        suggestions = []
        
        if config_name == "cards":
            suggestions = suggest_card_updates(data)
        elif config_name == "characters":
            suggestions = suggest_character_updates(data)
        elif config_name == "enemies":
            suggestions = suggest_enemy_updates(data)
        
        all_suggestions_by_config[config_name] = (data, suggestions)
        total_suggestions += len(suggestions)
        
        if suggestions:
            print(f"发现 {len(suggestions)} 个可优化项:\n")
            for i, sug in enumerate(suggestions, 1):
                entity_name = sug.get("card_name") or sug.get("char_name") or sug.get("enemy_name", "?")
                field = sug.get("field", "iconPath/portraitPath")
                
                print(f"  {i}. [{entity_name}]")
                print(f"     字段: {field}")
                print(f"     当前: {Path(sug.get('current', '')).name}")
                print(f"     建议: {Path(sug.get('suggested', '')).name}")
                print(f"     置信度: {sug.get('confidence', 'medium')}")
                print()
        else:
            print("✅ 未找到可优化的项（可能已经是最新的）\n")
    
    # 总结
    print(f"\n{'='*70}")
    print(f"📊 总计发现 {total_suggestions} 个可优化项")
    print(f"{'='*70}\n")
    
    if total_suggestions == 0:
        print("✅ 所有配置已是最新状态！\n")
        return
    
    if mode == "--preview":
        print("💡 以上是预览结果。使用 --auto 应用更改\n")
        return
    
    elif mode == "--auto":
        print("🚀 开始自动应用更新...\n")
        
        total_applied = 0
        for config_name, (data, suggestions) in all_suggestions_by_config.items():
            if suggestions:
                print(f"\n更新 {config_name}.json:")
                applied = apply_suggestions(config_name, data, suggestions)
                save_config(config_name, data)
                total_applied += applied
                
                print(f"\n  ✅ 已更新 {applied} 项")
                print(f"  💾 原文件备份为: {config_name}.json.backup")
        
        print(f"\n{'='*70}")
        print(f"✅ 完成！共更新 {total_applied} 个资源配置")
        print(f"{'='*70}")
        print("""
下一步:
  1. 在 Godot 编辑器中打开项目
  2. 运行游戏验证显示效果
  3. 如需恢复原配置:
     cp Config/Data/*.json.backup Config/Data/*.json
""")
    
    elif mode == "--interactive":
        print("🔍 交互模式 - 逐个确认更新\n")
        
        for config_name, (data, suggestions) in all_suggestions_by_config.items():
            if not suggestions:
                continue
                
            print(f"\n--- {config_name}.json ---\n")
            
            new_suggestions = []
            for sug in suggestions:
                entity_name = sug.get("card_name") or sug.get("char_name") or sug.get("enemy_name", "?")
                
                while True:
                    choice = input(f"更新 [{entity_name}]? (y/n/s=跳过剩余, q=退出): ").strip().lower()
                    
                    if choice == 'y':
                        new_suggestions.append(sug)
                        print(f"  ✓ 已标记更新\n")
                        break
                    elif choice == 'n':
                        print(f"  ✗ 跳过\n")
                        break
                    elif choice == 's':
                        print(f"  → 跳过剩余项\n")
                        break
                    elif choice == 'q':
                        print("\n取消操作")
                        return
                    else:
                        print("  请输入 y/n/s/q")
            
            if new_suggestions:
                applied = apply_suggestions(config_name, data, new_suggestions)
                save_config(config_name, data)
                print(f"\n✅ 已更新 {applied} 项")


if __name__ == "__main__":
    main()
