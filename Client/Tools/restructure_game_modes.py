#!/usr/bin/env python3
"""
游戏玩法目录重构工具 - 自动化迁移脚本
使用方法: python3 Tools/restructure_game_modes.py [--plan] [--execute] [--dry-run]
"""

import os
import sys
import shutil
import json
from pathlib import Path
from datetime import datetime

PROJECT_ROOT = Path(__file__).parent.parent
GAME_MODES_DIR = PROJECT_ROOT / "GameModes"

# 定义玩法目录映射结构
GAME_MODE_STRUCTURE = {
    "base_game": {
        "name": "杀戮尖塔复刻版",
        "description": "经典Roguelike卡牌游戏",
        "directories": [
            "Code",
            "Config",
            "Scenes",
            "Resources/Images/Backgrounds",
            "Resources/Images/Characters",
            "Resources/Images/Enemies",
            "Resources/Images/Events",
            "Resources/Images/Potions",
            "Resources/Images/Relics",
            "Resources/Audio/BGM",
            "Resources/Audio/SFX",
            "Resources/Icons/Cards",
            "Resources/Icons/Enemies",
            "Resources/Icons/Items",
            "Resources/Icons/Relics",
            "Resources/Icons/Skills",
            "Resources/Icons/Achievements",
            "Resources/Icons/Rest",
            "Resources/Icons/Services"
        ],
        "file_mappings": {
            # 配置文件 -> Config/
            "Config/Data/cards.json": "Config/cards.json",
            "Config/Data/characters.json": "Config/characters.json",
            "Config/Data/enemies.json": "Config/enemies.json",
            "Config/Data/events.json": "Config/events.json",
            "Config/Data/relics.json": "Config/relics.json",
            "Config/Data/potions.json": "Config/potions.json",
            "Config/Data/effects.json": "Config/effects.json",
            "Config/Data/audio.json": "Config/audio.json",
            
            # 包配置 -> 根目录
            "Packages/base_game/base_game_config.json": "package_config.json",
            
            # 场景文件 -> Scenes/
            "Scenes/CombatScene.tscn": "Scenes/CombatScene.tscn",
            "Scenes/CharacterSelect.tscn": "Scenes/CharacterSelect.tscn",
            "Scenes/MainMenu.tscn": "Scenes/MainMenu.tscn",
            "Scenes/MapScene.tscn": "Scenes/MapScene.tscn",
            "Scenes/EventPanel.tscn": "Scenes/EventPanel.tscn",
            "Scenes/ShopPanel.tscn": "Scenes/ShopPanel.tscn",
            "Scenes/RestSitePanel.tscn": "Scenes/RestSitePanel.tscn",
            "Scenes/SettingsPanel.tscn": "Scenes/SettingsPanel.tscn",
            "Scenes/GameOverScreen.tscn": "Scenes/GameOverScreen.tscn",
            "Scenes/VictoryScreen.tscn": "Scenes/VictoryScreen.tscn",
            "Scenes/TutorialOverlay.tscn": "Scenes/TutorialOverlay.tscn",
            "Scenes/AchievementPopup.tscn": "Scenes/AchievementPopup.tscn"
        },
        "directory_mappings": {
            # 图片资源 -> Resources/Images/
            "Images/Backgrounds": "Resources/Images/Backgrounds",
            "Images/Characters": "Resources/Images/Characters",
            "Images/Enemies": "Resources/Images/Enemies",
            "Images/Events": "Resources/Images/Events",
            "Images/Potions": "Resources/Images/Potions",
            "Images/Relics": "Resources/Images/Relics",
            
            # 音频资源 -> Resources/Audio/
            "Audio/BGM": "Resources/Audio/BGM",
            "Audio/SFX": "Resources/Audio/SFX",
            
            # 图标资源 -> Resources/Icons/
            "Icons/Cards": "Resources/Icons/Cards",
            "Icons/Enemies": "Resources/Icons/Enemies",
            "Icons/Items": "Resources/Icons/Items",
            "Icons/Relics": "Resources/Icons/Relics",
            "Icons/Skills": "Resources/Icons/Skills",
            "Icons/Achievements": "Resources/Icons/Achievements",
            "Icons/Rest": "Resources/Icons/Rest",
            "Icons/Services": "Resources/Icons/Services"
        }
    }
}

def create_directory_structure():
    """创建完整的目录结构"""
    print("\n📁 Creating directory structure...")
    
    for game_mode, config in GAME_MODE_STRUCTURE.items():
        mode_dir = GAME_MODES_DIR / game_mode
        
        print(f"\n   📂 {game_mode}/ ({config['name']})")
        
        for dir_path in config["directories"]:
            full_path = mode_dir / dir_path
            full_path.mkdir(parents=True, exist_ok=True)
            print(f"      ✅ {dir_path}/")
    
    print(f"\n✅ Directory structure created at: {GAME_MODES_DIR}")

def plan_migration():
    """生成迁移计划（不执行）"""
    print("\n" + "="*70)
    print("📋 MIGRATION PLAN (Dry Run)")
    print("="*70)
    
    total_files = 0
    total_dirs = 0
    
    for game_mode, config in GAME_MODE_STRUCTURE.items():
        print(f"\n{'='*70}")
        print(f"🎮 Game Mode: {game_mode}")
        print(f"   Name: {config['name']}")
        print(f"{'='*70}")
        
        mode_dir = GAME_MODES_DIR / game_mode
        
        # 统计文件映射
        print(f"\n📄 File Mappings ({len(config['file_mappings'])} files):")
        for src, dst in config["file_mappings"].items():
            src_path = PROJECT_ROOT / src
            if src_path.exists():
                total_files += 1
                size = src_path.stat().st_size
                print(f"   ✅ {src} → {dst} ({size/1024:.1f} KB)")
            else:
                print(f"   ⚠️  {src} → NOT FOUND")
        
        # 统计目录映射
        print(f"\n📁 Directory Mappings ({len(config['directory_mappings'])} directories):")
        for src_dir, dst_dir in config["directory_mappings"].items():
            src_path = PROJECT_ROOT / src_dir
            if src_path.exists():
                file_count = len(list(src_path.rglob("*")))
                total_dirs += 1
                total_files += file_count
                print(f"   ✅ {src_dir}/ → {dst_dir}/ ({file_count} files)")
            else:
                print(f"   ⚠️  {src_dir}/ → NOT FOUND")
    
    print(f"\n{'='*70}")
    print(f"📊 Summary:")
    print(f"   Total Files to Move: {total_files}")
    print(f"   Total Directories to Organize: {total_dirs}")
    print(f"   Target Location: {GAME_MODES_DIR}")
    print(f"{'='*70}\n")

def execute_migration(dry_run=True):
    """执行迁移"""
    action = "🔍 Planning" if dry_run else "🚀 Executing"
    
    print(f"\n{action} Migration...")
    print(f"{'='*70}")
    
    if dry_run:
        print("⚠️  DRY RUN MODE - No files will be moved!")
        print("   Use --execute flag to actually move files\n")
    
    success_count = 0
    error_count = 0
    skip_count = 0
    
    for game_mode, config in GAME_MODE_STRUCTURE.items():
        print(f"\n🎮 Processing: {game_mode}")
        mode_dir = GAME_MODES_DIR / game_mode
        
        # 迁移单个文件
        print("\n   📄 Moving files:")
        for src, dst in config["file_mappings"].items():
            src_path = PROJECT_ROOT / src
            dst_path = mode_dir / dst
            
            if not src_path.exists():
                print(f"      ⚠️  SKIP: {src} (not found)")
                skip_count += 1
                continue
            
            if dst_path.exists():
                print(f"      ⚠️  SKIP: {dst} (already exists)")
                skip_count += 1
                continue
            
            try:
                if not dry_run:
                    dst_path.parent.mkdir(parents=True, exist_ok=True)
                    shutil.copy2(src_path, dst_path)
                
                size = src_path.stat().st_size
                print(f"      ✅ {src.split('/')[-1]} → {dst} ({size/1024:.1f} KB)")
                success_count += 1
                
            except Exception as e:
                print(f"      ❌ ERROR: {src} - {str(e)}")
                error_count += 1
        
        # 迁移整个目录
        print("\n   📁 Moving directories:")
        for src_dir, dst_dir in config["directory_mappings"].items():
            src_path = PROJECT_ROOT / src_dir
            dst_path = mode_dir / dst_dir
            
            if not src_path.exists():
                print(f"      ⚠️  SKIP: {src_dir}/ (not found)")
                skip_count += 1
                continue
            
            try:
                file_list = list(src_path.rglob("*"))
                file_count = len([f for f in file_list if f.is_file()])
                
                if not dry_run:
                    dst_path.mkdir(parents=True, exist_ok=True)
                    shutil.copytree(src_path, dst_path, dirs_exist_ok=True)
                
                print(f"      ✅ {src_dir}/ → {dst_dir}/ ({file_count} files)")
                success_count += 1
                
            except Exception as e:
                print(f"      ❌ ERROR: {src_dir}/ - {str(e)}")
                error_count += 1
        
        # 复制包配置
        config_src = PROJECT_ROOT / "Packages" / game_mode / f"{game_mode}_config.json"
        config_dst = mode_dir / "package_config.json"
        
        if config_src.exists():
            try:
                if not dry_run:
                    shutil.copy2(config_src, config_dst)
                print(f"\n   📋 Package Config: ✅ Copied")
                success_count += 1
            except Exception as e:
                print(f"\n   📋 Package Config: ❌ Error - {e}")
                error_count += 1
    
    # 生成迁移报告
    print(f"\n{'='*70}")
    print(f"📊 Migration Report:")
    print(f"   ✅ Success: {success_count}")
    print(f"   ⚠️  Skipped: {skip_count}")
    print(f"   ❌ Errors: {error_count}")
    print(f"   Mode: {'DRY RUN' if dry_run else 'EXECUTED'}")
    print(f"{'='*70}\n")
    
    if not dry_run and error_count == 0:
        # 生成路径映射配置
        generate_path_mapping()
        
        print("✅ Migration completed successfully!")
        print("\n📝 Next Steps:")
        print("   1. Review the new structure in GameModes/")
        print("   2. Update resource paths in code (see path_mapping.json)")
        print("   3. Test the game to ensure everything works")
        print("   4. Once verified, you can delete old directories")

def generate_path_mapping():
    """生成新旧路径映射配置"""
    mapping = {
        "generated_at": datetime.now().isoformat(),
        "version": "1.0",
        "mappings": {}
    }
    
    for game_mode, config in GAME_MODE_STRUCTURE.items():
        mode_mappings = []
        
        # 文件映射
        for src, dst in config["file_mappings"].items():
            old_path = f"res://{src}"
            new_path = f"res://GameModes/{game_mode}/{dst}"
            mode_mappings.append({
                "type": "file",
                "old_path": old_path,
                "new_path": new_path,
                "description": f"{src.split('/')[-1]}"
            })
        
        # 目录映射
        for src_dir, dst_dir in config["directory_mappings"].items():
            old_base = f"res://{src_dir}"
            new_base = f"res://GameModes/{game_mode}/{dst_dir}"
            mode_mappings.append({
                "type": "directory",
                "old_path": old_base,
                "new_path": new_base,
                "description": f"{src_dir} contents"
            })
        
        mapping["mappings"][game_mode] = mode_mappings
    
    mapping_file = PROJECT_ROOT / "Tools" / "path_mapping.json"
    with open(mapping_file, 'w', encoding='utf-8') as f:
        json.dump(mapping, f, indent=2, ensure_ascii=False)
    
    print(f"\n📋 Path mapping saved to: {mapping_file}")

def create_readme():
    """在GameModes目录创建README"""
    readme_content = f"""# 🎮 Game Modes Directory

This directory contains isolated game modes (playstyles), each with its own code, resources, and configuration.

## Structure

```
GameModes/
├── base_game/          # Base game (Slay the Spire clone)
│   ├── Code/           # Gameplay-specific C# scripts
│   ├── Config/         # JSON configuration files
│   ├── Scenes/         # Godot scene files (.tscn)
│   └── Resources/      # Art & audio assets
│       ├── Images/     # PNG images
│       ├── Audio/      # OGG/WAV audio
│       └── Icons/      # UI icons
│
├── frost_expansion/    # Future: Ice theme expansion
└── shadow_realm/       # Future: Shadow mod
```

## Adding a New Game Mode

1. Create a new folder under `GameModes/<mode_name>/`
2. Follow the same structure as `base_game/`
3. Add your mode-specific files
4. Update `PackageManager.cs` to recognize the new mode

## Integration with Package System

Each folder corresponds to a downloadable package. When a package is installed:

1. Download ZIP from CDN
2. Extract to `user://packages/<mode_id>/`
3. Load configuration from `package_config.json`
4. Register gameplay extensions via `IPackageExtension`

## Notes

- **Framework code** stays in `Scripts/` (shared across all modes)
- **Mode-specific code** goes in `GameModes/<mode>/Code/`
- **Resources** are isolated per mode to prevent conflicts
- **Configuration** is self-contained in each mode's folder

Last updated: {datetime.now().strftime("%Y-%m-%d %H:%M")}
"""
    
    readme_path = GAME_MODES_DIR / "README.md"
    with open(readme_path, 'w', encoding='utf-8') as f:
        f.write(readme_content)
    
    print(f"📖 README created at: {readme_path}")

def main():
    args = sys.argv[1:]
    
    print("\n" + "="*70)
    print("🎮 Game Modes Restructuring Tool")
    print("="*70)
    print(f"Project Root: {PROJECT_ROOT}")
    print(f"Target Dir: {GAME_MODES_DIR}")
    
    if "--help" in args or "-h" in args:
        print("""
Usage:
  python3 restructure_game_modes.py [options]

Options:
  --plan       Show migration plan only (dry run)
  --execute    Execute actual file migration
  --dry-run    Same as --plan
  --help       Show this help message

Examples:
  python3 restructure_game_modes.py --plan      # Preview changes
  python3 restructure_game_modes.py --execute   # Apply changes
""")
        return
    
    # Step 1: Create directory structure
    create_directory_structure()
    
    # Step 2: Create README
    create_readme()
    
    # Step 3: Plan or Execute
    if "--execute" in args:
        execute_migration(dry_run=False)
    elif "--plan" in args or "--dry-run" in args or len(args) == 0:
        plan_migration()
        execute_migration(dry_run=True)
        print("\n💡 Tip: Run with --execute to actually move files")
    
    print("\n✨ Done!\n")

if __name__ == "__main__":
    main()
