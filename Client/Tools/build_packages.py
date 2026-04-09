#!/usr/bin/env python3
"""
包打包工具 - 将游戏内容打包成可下载的.zip文件
使用方法: python3 build_packages.py [--all] [--package base_game]
"""

import zipfile
import os
import sys
import json
import shutil
from pathlib import Path
from datetime import datetime

PROJECT_ROOT = Path(__file__).parent.parent
OUTPUT_DIR = PROJECT_ROOT / "test_cdn" / "packages"

# 包定义：指定每个包包含的文件和目录
PACKAGE_DEFINITIONS = {
    "base_game": {
        "name": "杀戮尖塔复刻版",
        "version": "1.0.0",
        "description": "经典Roguelike卡牌游戏体验",
        "include_patterns": [
            # 配置文件
            ("Config/Data/cards.json", "config/"),
            ("Config/Data/characters.json", "config/"),
            ("Config/Data/enemies.json", "config/"),
            ("Config/Data/events.json", "config/"),
            ("Config/Data/relics.json", "config/"),
            ("Config/Data/potions.json", "config/"),
            ("Config/Data/effects.json", "config/"),
            ("Config/Data/audio.json", "config/"),
            
            # 包配置文件
            ("Packages/base_game/base_game_config.json", ""),
            
            # 场景文件
            ("Scenes/CombatScene.tscn", "scenes/"),
            ("Scenes/CharacterSelect.tscn", "scenes/"),
            ("Scenes/MainMenu.tscn", "scenes/"),
            ("Scenes/MapScene.tscn", "scenes/"),
            
            # 图片资源 - 背景
            ("Images/Backgrounds/", "images/backgrounds/"),
            
            # 图片资源 - 角色
            ("Images/Characters/", "images/characters/"),
            
            # 图片资源 - 敌人
            ("Images/Enemies/", "images/enemies/"),
            
            # 图片资源 - 药水
            ("Images/Potions/", "images/potions/"),
            
            # 图片资源 - 圣物
            ("Images/Relics/", "images/relics/"),
            
            # 图片资源 - 事件
            ("Images/Events/", "images/events/"),
            
            # 图标资源
            ("Icons/Cards/", "icons/cards/"),
            ("Icons/Enemies/", "icons/enemies/"),
            ("Icons/Items/", "icons/items/"),
            ("Icons/Relics/", "icons/relics/"),
            ("Icons/Skills/", "icons/skills/"),
            ("Icons/Achievements/", "icons/achievements/"),
            ("Icons/Rest/", "icons/rest/"),
            ("Icons/Services/", "icons/services/"),
            
            # 音频资源
            ("Audio/BGM/", "audio/bgm/"),
            ("Audio/SFX/", "audio/sfx/"),
        ],
        "exclude_patterns": [
            "*.cs",
            "*.csproj",
            "*.sln",
            ".gitignore",
            "*.md",
        ]
    }
}

def create_package(package_id, package_def):
    """创建单个包的ZIP文件"""
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    
    output_file = OUTPUT_DIR / f"{package_id}.zip"
    
    print(f"\n📦 Building package: {package_id}")
    print(f"   Name: {package_def['name']}")
    print(f"   Version: {package_def['version']}")
    
    with zipfile.ZipFile(output_file, 'w', zipfile.ZIP_DEFLATED) as zipf:
        files_added = 0
        total_size = 0
        
        for pattern, dest_dir in package_def["include_patterns"]:
            src_path = PROJECT_ROOT / pattern
            
            if src_path.is_file():
                # 单个文件
                arcname = f"{dest_dir}{src_path.name}"
                zipf.write(src_path, arcname)
                files_added += 1
                total_size += src_path.stat().st_size
                print(f"   ✅ {arcname} ({src_path.stat().st_size / 1024:.1f} KB)")
                
            elif src_path.is_dir():
                # 目录
                for file_path in src_path.rglob("*"):
                    if file_path.is_file():
                        rel_path = file_path.relative_to(src_path)
                        arcname = f"{dest_dir}{rel_path.as_posix()}"
                        zipf.write(file_path, arcname)
                        files_added += 1
                        total_size += file_path.stat().st_size
                        
                        if files_added <= 10 or files_added % 50 == 0:
                            print(f"   ✅ {arcname}")
        
        # 添加包信息清单
        manifest = {
            "packageId": package_id,
            "name": package_def["name"],
            "version": package_def["version"],
            "description": package_def["description"],
            "buildDate": datetime.now().isoformat(),
            "filesCount": files_added,
            "totalSize": total_size,
            "entryPoint": "scenes/CombatScene.tscn"
        }
        
        zipf.writestr("manifest.json", json.dumps(manifest, indent=2))
        print(f"   ✅ manifest.json (package metadata)")
    
    final_size = output_file.stat().st_size
    compression_ratio = (1 - final_size / total_size) * 100 if total_size > 0 else 0
    
    print(f"\n   📊 Package Statistics:")
    print(f"      Files: {files_added}")
    print(f"      Original Size: {total_size / 1024 / 1024:.2f} MB")
    print(f"      Compressed Size: {final_size / 1024 / 1024:.2f} MB")
    print(f"      Compression: {compression_ratio:.1f}%")
    print(f"   ✅ Package created: {output_file.name}\n")
    
    return output_file, final_size

def generate_registry(packages_info):
    """生成registry.json配置文件"""
    registry = {
        "version": "1.0.0",
        "lastUpdated": datetime.now().isoformat(),
        "featuredPackages": ["base_game"],
        "categories": [
            {
                "id": "official",
                "name": "官方内容",
                "description": "开发团队制作的官方扩展包",
                "icon": "🎮",
                "packageIds": list(packages_info.keys())
            },
            {
                "id": "community",
                "name": "社区创作",
                "description": "社区玩家创作的自定义内容",
                "icon": "👥",
                "packageIds": []
            },
            {
                "id": "dlc",
                "name": "DLC扩展",
                "description": "大型付费扩展内容",
                "icon": "💎",
                "packageIds": []
            }
        ],
        "packages": []
    }
    
    for pkg_id, pkg_data in packages_info.items():
        package_entry = {
            "id": pkg_id,
            "name": PACKAGE_DEFINITIONS[pkg_id]["name"],
            "description": PACKAGE_DEFINITIONS[pkg_id]["description"],
            "version": PACKAGE_DEFINITIONS[pkg_id]["version"],
            "type": 0 if pkg_id == "base_game" else 1,
            "author": "Development Team",
            "iconPath": "",
            "thumbnailPath": "",
            "downloadUrl": f"http://localhost:8080/packages/{pkg_id}.zip",
            "fileSize": pkg_data["size"],
            "requiredBaseVersion": "1.0.0",
            "dependencies": [],
            "tags": ["roguelike", "card", "strategy", "official", "turn-based"],
            "features": [
                "经典卡牌战斗系统",
                "随机地图生成",
                "多角色选择",
                "圣物收集系统",
                "药水与事件"
            ],
            "isFree": True,
            "price": 0,
            "rating": 4.8,
            "downloadCount": 10000,
            "releaseDate": datetime.now().strftime("%Y-%m-%dT00:00:00"),
            "lastUpdated": datetime.now().isoformat(),
            "entryScene": "scenes/CombatScene.tscn",
            "configFile": "config/cards.json"
        }
        registry["packages"].append(package_entry)
    
    registry_path = OUTPUT_DIR / "registry.json"
    with open(registry_path, 'w', encoding='utf-8') as f:
        json.dump(registry, f, ensure_ascii=False, indent=2)
    
    print(f"✅ Registry generated: {registry_path}")
    return registry_path

def main():
    print("\n" + "="*70)
    print("🔧 游戏包打包工具")
    print("="*70)
    
    args = sys.argv[1:]
    build_all = "--all" in args
    
    packages_to_build = []
    
    if build_all:
        packages_to_build = list(PACKAGE_DEFINITIONS.keys())
    else:
        for arg in args:
            if arg in PACKAGE_DEFINITIONS:
                packages_to_build.append(arg)
    
    if not packages_to_build:
        print("\nUsage: python3 build_packages.py [--all] [--package <id>]")
        print("\nAvailable packages:")
        for pkg_id, pkg_def in PACKAGE_DEFINITIONS.items():
            print(f"  • {pkg_id}: {pkg_def['name']}")
        print("\nBuilding all packages by default...")
        packages_to_build = list(PACKAGE_DEFINITIONS.keys())
    
    print(f"\n📦 Packages to build: {len(packages_to_build)}\n")
    
    packages_info = {}
    
    for package_id in packages_to_build:
        if package_id not in PACKAGE_DEFINITIONS:
            print(f"❌ Unknown package: {package_id}")
            continue
        
        try:
            output_file, size = create_package(package_id, PACKAGE_DEFINITIONS[package_id])
            packages_info[package_id] = {"path": output_file, "size": size}
        except Exception as e:
            print(f"❌ Failed to build {package_id}: {e}")
    
    if packages_info:
        generate_registry(packages_info)
        
        print("\n" + "="*70)
        print("✅ Build Complete!")
        print("="*70)
        print(f"\n📂 Output Directory: {OUTPUT_DIR.absolute()}")
        print(f"📦 Packages Built: {len(packages_info)}")
        
        total_size = sum(p["size"] for p in packages_info.values())
        print(f"💾 Total Size: {total_size / 1024 / 1024:.2f} MB")
        
        print(f"\n🚀 Next Steps:")
        print(f"   1. Start CDN server: python3 Tools/local_cdn_server.py")
        print(f"   2. Open game and test package download")
        print(f"   3. Access: http://localhost:8080/registry.json")
        print()

if __name__ == "__main__":
    main()
