#!/usr/bin/env python3
"""
包打包工具 - 将游戏内容打包成可下载的.zip文件
同时生成热更新清单(manifest.json)
使用方法: python3 build_packages.py [--all] [--package base_game]
"""

import zipfile
import os
import sys
import json
import hashlib
import shutil
from pathlib import Path
from datetime import datetime

PROJECT_ROOT = Path(__file__).parent.parent
GAME_MODES = PROJECT_ROOT / "GameModes" / "base_game"
OUTPUT_DIR = PROJECT_ROOT / "test_cdn" / "packages"
UPDATES_DIR = PROJECT_ROOT / "test_cdn" / "updates"

PACKAGE_DEFINITIONS = {
    "base_game": {
        "name": "杀戮尖塔复刻版",
        "version": "1.0.0",
        "description": "经典Roguelike卡牌游戏体验",
        "include_patterns": [
            ("GameModes/base_game/Config/Data/cards.json", "config/"),
            ("GameModes/base_game/Config/Data/characters.json", "config/"),
            ("GameModes/base_game/Config/Data/enemies.json", "config/"),
            ("GameModes/base_game/Config/Data/events.json", "config/"),
            ("GameModes/base_game/Config/Data/relics.json", "config/"),
            ("GameModes/base_game/Config/Data/potions.json", "config/"),
            ("GameModes/base_game/Config/Data/effects.json", "config/"),
            ("GameModes/base_game/Config/Data/audio.json", "config/"),
            ("GameModes/base_game/Scenes/CombatScene.tscn", "scenes/"),
            ("GameModes/base_game/Scenes/CharacterSelect.tscn", "scenes/"),
            ("GameModes/base_game/Scenes/MainMenu.tscn", "scenes/"),
            ("GameModes/base_game/Scenes/MapScene.tscn", "scenes/"),
            ("GameModes/base_game/Resources/Images/Backgrounds/", "images/backgrounds/"),
            ("GameModes/base_game/Resources/Images/Characters/", "images/characters/"),
            ("GameModes/base_game/Resources/Images/Enemies/", "images/enemies/"),
            ("GameModes/base_game/Resources/Images/Potions/", "images/potions/"),
            ("GameModes/base_game/Resources/Images/Relics/", "images/relics/"),
            ("GameModes/base_game/Resources/Images/Events/", "images/events/"),
            ("GameModes/base_game/Resources/Icons/Cards/", "icons/cards/"),
            ("GameModes/base_game/Resources/Icons/Enemies/", "icons/enemies/"),
            ("GameModes/base_game/Resources/Icons/Items/", "icons/items/"),
            ("GameModes/base_game/Resources/Icons/Relics/", "icons/relics/"),
            ("GameModes/base_game/Resources/Icons/Skills/", "icons/skills/"),
            ("GameModes/base_game/Resources/Icons/Achievements/", "icons/achievements/"),
            ("GameModes/base_game/Resources/Icons/Rest/", "icons/rest/"),
            ("GameModes/base_game/Resources/Icons/Services/", "icons/services/"),
            ("GameModes/base_game/Resources/Audio/BGM/", "audio/bgm/"),
            ("GameModes/base_game/Resources/Audio/SFX/", "audio/sfx/"),
        ],
        "exclude_patterns": [
            "*.cs",
            "*.csproj",
            "*.sln",
            ".gitignore",
            "*.md",
        ],
        "hotfix_patterns": [
            ("GameModes/base_game/Scripts/", "Scripts/", "script"),
            ("GameModes/base_game/Config/Data/", "Config/Data/", "config"),
        ]
    }
}

def compute_hash(file_path):
    h = hashlib.sha256()
    with open(file_path, 'rb') as f:
        for chunk in iter(lambda: f.read(8192), b''):
            h.update(chunk)
    return h.hexdigest()

def should_exclude(file_path, exclude_patterns):
    for pattern in exclude_patterns:
        if pattern.startswith("*."):
            if file_path.name.endswith(pattern[1:]):
                return True
        elif pattern in str(file_path):
            return True
    return False

def create_package(package_id, package_def):
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    output_file = OUTPUT_DIR / f"{package_id}.zip"

    print(f"\n  Building package: {package_id}")
    print(f"   Name: {package_def['name']}")
    print(f"   Version: {package_def['version']}")

    with zipfile.ZipFile(output_file, 'w', zipfile.ZIP_DEFLATED) as zipf:
        files_added = 0
        total_size = 0

        for pattern, dest_dir in package_def["include_patterns"]:
            src_path = PROJECT_ROOT / pattern

            if src_path.is_file():
                arcname = f"{dest_dir}{src_path.name}"
                zipf.write(src_path, arcname)
                files_added += 1
                total_size += src_path.stat().st_size
                print(f"   + {arcname} ({src_path.stat().st_size / 1024:.1f} KB)")

            elif src_path.is_dir():
                for file_path in src_path.rglob("*"):
                    if file_path.is_file():
                        if should_exclude(file_path, package_def.get("exclude_patterns", [])):
                            continue
                        rel_path = file_path.relative_to(src_path)
                        arcname = f"{dest_dir}{rel_path.as_posix()}"
                        zipf.write(file_path, arcname)
                        files_added += 1
                        total_size += file_path.stat().st_size

                        if files_added <= 10 or files_added % 50 == 0:
                            print(f"   + {arcname}")

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
        print(f"   + manifest.json (package metadata)")

    final_size = output_file.stat().st_size
    compression_ratio = (1 - final_size / total_size) * 100 if total_size > 0 else 0

    print(f"\n   Package Statistics:")
    print(f"      Files: {files_added}")
    print(f"      Original Size: {total_size / 1024 / 1024:.2f} MB")
    print(f"      Compressed Size: {final_size / 1024 / 1024:.2f} MB")
    print(f"      Compression: {compression_ratio:.1f}%")
    print(f"   Package created: {output_file.name}\n")

    return output_file, final_size

def generate_hotfix_manifest(package_id, package_def):
    UPDATES_DIR.mkdir(parents=True, exist_ok=True)
    pkg_update_dir = UPDATES_DIR / package_id
    pkg_update_dir.mkdir(parents=True, exist_ok=True)

    files_list = []
    total_size = 0

    for pattern, dest_prefix, file_type in package_def.get("hotfix_patterns", []):
        src_path = PROJECT_ROOT / pattern
        if not src_path.exists():
            continue

        if src_path.is_file():
            file_hash = compute_hash(src_path)
            file_size = src_path.stat().st_size
            total_size += file_size
            files_list.append({
                "path": f"{dest_prefix}{src_path.name}",
                "hash": file_hash,
                "size": file_size,
                "type": file_type
            })
        elif src_path.is_dir():
            for file_path in src_path.rglob("*"):
                if file_path.is_file():
                    if should_exclude(file_path, package_def.get("exclude_patterns", [])):
                        continue
                    rel_path = file_path.relative_to(src_path)
                    file_hash = compute_hash(file_path)
                    file_size = file_path.stat().st_size
                    total_size += file_size
                    files_list.append({
                        "path": f"{dest_prefix}{rel_path.as_posix()}",
                        "hash": file_hash,
                        "size": file_size,
                        "type": file_type
                    })

    if not files_list:
        print(f"   No hotfix files found for {package_id}")
        return None

    version_parts = package_def["version"].split(".")
    version_parts[-1] = str(int(version_parts[-1]) + 1)
    new_version = ".".join(version_parts)

    manifest = {
        "packageId": package_id,
        "version": new_version,
        "previousVersion": package_def["version"],
        "description": f"Hot update for {package_def['name']}",
        "releaseDate": datetime.now().isoformat(),
        "total_size": total_size,
        "files_count": len(files_list),
        "download_url": f"/packages/{package_id}.zip",
        "files": files_list,
        "changelog": [
            f"Updated {len(files_list)} files"
        ]
    }

    manifest_path = pkg_update_dir / "manifest.json"
    with open(manifest_path, 'w', encoding='utf-8') as f:
        json.dump(manifest, f, ensure_ascii=False, indent=2)

    print(f"   Hotfix manifest generated: {package_id} v{new_version} ({len(files_list)} files, {total_size / 1024:.1f} KB)")
    return manifest_path

def generate_registry(packages_info):
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

    print(f"  Registry generated: {registry_path}")
    return registry_path

def main():
    print("\n" + "=" * 70)
    print("  Game Package Builder (with Hotfix Manifest)")
    print("=" * 70)

    args = sys.argv[1:]
    build_all = "--all" in args
    gen_hotfix = "--hotfix" in args

    packages_to_build = []

    if build_all:
        packages_to_build = list(PACKAGE_DEFINITIONS.keys())
    else:
        for arg in args:
            if arg in PACKAGE_DEFINITIONS and arg not in ("--hotfix",):
                packages_to_build.append(arg)

    if not packages_to_build:
        print("\nUsage: python3 build_packages.py [--all] [--package <id>] [--hotfix]")
        print("\nAvailable packages:")
        for pkg_id, pkg_def in PACKAGE_DEFINITIONS.items():
            print(f"  * {pkg_id}: {pkg_def['name']}")
        print("\nFlags:")
        print("  --hotfix  Also generate hotfix manifests")
        print("\nBuilding all packages by default...")
        packages_to_build = list(PACKAGE_DEFINITIONS.keys())

    print(f"\n  Packages to build: {len(packages_to_build)}\n")

    packages_info = {}

    for package_id in packages_to_build:
        if package_id not in PACKAGE_DEFINITIONS:
            print(f"  Unknown package: {package_id}")
            continue

        try:
            output_file, size = create_package(package_id, PACKAGE_DEFINITIONS[package_id])
            packages_info[package_id] = {"path": output_file, "size": size}

            if gen_hotfix:
                generate_hotfix_manifest(package_id, PACKAGE_DEFINITIONS[package_id])
        except Exception as e:
            print(f"  Failed to build {package_id}: {e}")

    if packages_info:
        generate_registry(packages_info)

        print("\n" + "=" * 70)
        print("  Build Complete!")
        print("=" * 70)
        print(f"\n  Output Directory: {OUTPUT_DIR.absolute()}")
        print(f"  Packages Built: {len(packages_info)}")

        total_size = sum(p["size"] for p in packages_info.values())
        print(f"  Total Size: {total_size / 1024 / 1024:.2f} MB")

        print(f"\n  Next Steps:")
        print(f"   1. Start CDN server: python3 Client/Tools/local_cdn_server.py")
        print(f"   2. Open game and test package download")
        print(f"   3. Access: http://localhost:8080/registry.json")
        print(f"   4. Hotfix manifest: http://localhost:8080/updates/<pkg>/manifest.json")
        print()

if __name__ == "__main__":
    main()
