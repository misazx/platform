#!/usr/bin/env python3
"""
🎨 真实素材映射器 v10.0 - 使用 Kenney 官方下载的真实素材
=========================================================

将 Kenney 官方下载的真实 PNG 素材映射到游戏配置文件引用的路径，
替换所有程序化生成的假资源。

素材来源：
  - Platformer Characters: Soldier/Adventurer/Female/Zombie/Player
  - Monster Builder Pack: body/arm/eye/mouth/horn/wing
  - UI Pack: 按钮/图标/面板
  - Tiny Dungeon: 地牢瓦片
  - 1-Bit Pack: 像素素材
  - Impact/Interface Sounds: 音效
  - Music Jingles: BGM
"""

import os
import sys
import json
import shutil
from pathlib import Path
from PIL import Image

PROJECT_ROOT = Path(__file__).parent
DL = PROJECT_ROOT / "Assets_Library" / "_downloads"

def copy_and_resize(src, dst, size=None):
    """复制并调整大小"""
    dst = Path(dst)
    dst.parent.mkdir(parents=True, exist_ok=True)
    
    img = Image.open(src)
    if img.mode == 'RGBA':
        bg = Image.new('RGBA', img.size, (0, 0, 0, 0))
        bg.paste(img, mask=img.split()[3])
        img = bg
    elif img.mode != 'RGBA':
        img = img.convert('RGBA')
    
    if size:
        img = img.resize(size, Image.LANCZOS)
    
    img.save(str(dst))
    return dst

def build_monster(parts, size=(128, 128)):
    """用 Monster Builder Pack 部件组装怪物"""
    canvas = Image.new('RGBA', size, (0, 0, 0, 0))
    
    for part_name in parts:
        part_path = DL / "monster-builder-pack" / "PNG" / "Default" / part_name
        if part_path.exists():
            part = Image.open(part_path).convert('RGBA')
            part = part.resize(size, Image.LANCZOS)
            canvas = Image.alpha_composite(canvas, part)
    
    return canvas

def map_all_resources():
    """映射所有真实素材到游戏路径"""
    
    stats = {"copied": 0, "built": 0, "skipped": 0}
    
    # ==================== 1. 角色立绘 ====================
    print("\n👤 第1步: 映射角色立绘 (Images/Characters/)")
    
    char_map = {
        "Ironclad.png": {
            "src": DL / "platformer-characters" / "PNG" / "Soldier" / "Poses" / "soldier_stand.png",
            "size": (400, 600),
        },
        "Silent.png": {
            "src": DL / "platformer-characters" / "PNG" / "Female" / "Poses" / "female_stand.png",
            "size": (400, 600),
        },
        "Defect.png": {
            "src": DL / "platformer-characters" / "PNG" / "Adventurer" / "Poses" / "adventurer_stand.png",
            "size": (400, 600),
        },
        "Watcher.png": {
            "src": DL / "platformer-characters" / "PNG" / "Player" / "Poses" / "player_stand.png",
            "size": (400, 600),
        },
        "Necromancer.png": {
            "src": DL / "platformer-characters" / "PNG" / "Zombie" / "Poses" / "zombie_stand.png",
            "size": (400, 600),
        },
        "Heir.png": {
            "src": DL / "platformer-characters" / "PNG" / "Adventurer" / "Poses" / "adventurer_idle.png",
            "size": (400, 600),
        },
    }
    
    for name, info in char_map.items():
        dst = PROJECT_ROOT / "Images" / "Characters" / name
        src = info["src"]
        if src.exists():
            copy_and_resize(src, dst, info.get("size"))
            print(f"   ✅ {name} ← {src.name}")
            stats["copied"] += 1
        else:
            print(f"   ⚠️  {name} 源文件不存在: {src}")
            stats["skipped"] += 1
    
    # ==================== 2. 玩家角色图标 ====================
    print("\n⚔️ 第2步: 映射玩家角色图标 (Icons/Items/iron_sword.png)")
    
    player_icon_src = DL / "platformer-characters" / "PNG" / "Soldier" / "Limbs" / "head.png"
    if player_icon_src.exists():
        copy_and_resize(player_icon_src, PROJECT_ROOT / "Icons" / "Items" / "iron_sword.png", (64, 64))
        print(f"   ✅ iron_sword.png ← Soldier head")
        stats["copied"] += 1
    
    # ==================== 3. 敌人图标 ====================
    print("\n👾 第3步: 映射敌人图标 (Icons/Enemies/)")
    
    enemy_icon_map = {
        "cultist.png": DL / "platformer-characters" / "PNG" / "Zombie" / "Limbs" / "head.png",
        "jawworm.png": DL / "monster-builder-pack" / "PNG" / "Default" / "body_greenA.png",
        "jaw_worm.png": DL / "monster-builder-pack" / "PNG" / "Default" / "body_greenA.png",
        "lagavulin.png": DL / "monster-builder-pack" / "PNG" / "Default" / "body_darkA.png",
        "the_guardian.png": DL / "monster-builder-pack" / "PNG" / "Default" / "body_redA.png",
        "theguardian.png": DL / "monster-builder-pack" / "PNG" / "Default" / "body_redA.png",
    }
    
    for name, src in enemy_icon_map.items():
        dst = PROJECT_ROOT / "Icons" / "Enemies" / name
        if src.exists():
            copy_and_resize(src, dst, (128, 128))
            print(f"   ✅ {name} ← {src.name}")
            stats["copied"] += 1
        else:
            print(f"   ⚠️  {name} 源文件不存在")
            stats["skipped"] += 1
    
    # ==================== 4. 敌人全身图 ====================
    print("\n👹 第4步: 映射敌人全身图 (Images/Enemies/)")
    
    enemy_full_map = {
        "Cultist.png": DL / "platformer-characters" / "PNG" / "Zombie" / "Poses" / "zombie_stand.png",
        "JawWorm.png": DL / "platformer-characters" / "PNG" / "Zombie" / "Poses" / "zombie_idle.png",
        "Lagavulin.png": DL / "platformer-characters" / "PNG" / "Zombie" / "Poses" / "zombie_hurt.png",
        "TheGuardian.png": DL / "platformer-characters" / "PNG" / "Soldier" / "Poses" / "soldier_action1.png",
    }
    
    for name, src in enemy_full_map.items():
        dst = PROJECT_ROOT / "Images" / "Enemies" / name
        if src.exists():
            copy_and_resize(src, dst, (256, 256))
            print(f"   ✅ {name} ← {src.name}")
            stats["copied"] += 1
        else:
            print(f"   ⚠️  {name} 源文件不存在")
            stats["skipped"] += 1
    
    # ==================== 5. 卡牌图标 ====================
    print("\n🃏 第5步: 映射卡牌图标 (Icons/Cards/)")
    
    ui_red = DL / "ui-pack" / "PNG" / "Red" / "Default"
    ui_blue = DL / "ui-pack" / "PNG" / "Blue" / "Default"
    ui_green = DL / "ui-pack" / "PNG" / "Green" / "Default"
    ui_yellow = DL / "ui-pack" / "PNG" / "Yellow" / "Default"
    
    card_map = {
        "strike.png": ui_red / "icon_cross.png",
        "defend.png": ui_blue / "icon_checkmark.png",
        "bash.png": ui_red / "star_outline_depth.png",
        "cleave.png": ui_red / "icon_circle.png",
        "iron_wave.png": ui_blue / "icon_outline_square.png",
    }
    
    for name, src in card_map.items():
        dst = PROJECT_ROOT / "Icons" / "Cards" / name
        if src.exists():
            copy_and_resize(src, dst, (120, 170))
            print(f"   ✅ {name} ← {src.name}")
            stats["copied"] += 1
        else:
            print(f"   ⚠️  {name} 源文件不存在")
            stats["skipped"] += 1
    
    # ==================== 6. 遗物图标 ====================
    print("\n💎 第6步: 映射遗物图标 (Icons/Relics/)")
    
    relic_map = {
        "burning_blood.png": ui_red / "star_outline_depth.png",
        "anchor.png": ui_blue / "icon_circle.png",
        "ring_of_the_snake.png": ui_green / "icon_outline_checkmark.png",
        "cracked_core.png": ui_yellow / "icon_square.png",
    }
    
    for name, src in relic_map.items():
        dst = PROJECT_ROOT / "Icons" / "Relics" / name
        if src.exists():
            copy_and_resize(src, dst, (64, 64))
            print(f"   ✅ {name} ← {src.name}")
            stats["copied"] += 1
    
    # ==================== 7. 技能图标 ====================
    print("\n⚡ 第7步: 映射技能图标 (Icons/Skills/)")
    
    skill_map = {
        "fireball.png": ui_red / "star_outline_depth.png",
        "heal.png": ui_green / "icon_checkmark.png",
        "dash.png": ui_blue / "icon_outline_square.png",
        "iron_skin.png": ui_yellow / "icon_square.png",
    }
    
    for name, src in skill_map.items():
        dst = PROJECT_ROOT / "Icons" / "Skills" / name
        if src.exists():
            copy_and_resize(src, dst, (64, 64))
            print(f"   ✅ {name} ← {src.name}")
            stats["copied"] += 1
    
    # ==================== 8. 音频资源 ====================
    print("\n🔊 第8步: 映射音频资源 (Audio/)")
    
    # SFX
    sfx_src = DL / "impact-sounds"
    sfx_dst = PROJECT_ROOT / "Audio" / "SFX"
    sfx_dst.mkdir(parents=True, exist_ok=True)
    
    sfx_count = 0
    for root, dirs, files in os.walk(sfx_src):
        for f in files:
            if f.endswith(('.wav', '.ogg')):
                src_path = Path(root) / f
                dst_path = sfx_dst / f
                shutil.copy2(str(src_path), str(dst_path))
                sfx_count += 1
    print(f"   ✅ 复制 {sfx_count} 个音效文件")
    stats["copied"] += sfx_count
    
    # BGM
    bgm_src = DL / "music-jingles"
    bgm_dst = PROJECT_ROOT / "Audio" / "BGM"
    bgm_dst.mkdir(parents=True, exist_ok=True)
    
    bgm_count = 0
    for root, dirs, files in os.walk(bgm_src):
        for f in files:
            if f.endswith(('.wav', '.ogg')):
                src_path = Path(root) / f
                dst_path = bgm_dst / f
                shutil.copy2(str(src_path), str(dst_path))
                bgm_count += 1
    print(f"   ✅ 复制 {bgm_count} 个BGM文件")
    stats["copied"] += bgm_count
    
    # ==================== 9. 清理缓存 ====================
    print("\n🧹 第9步: 清理 .import 缓存")
    
    cache_count = 0
    for dir_path in [PROJECT_ROOT / "Icons", PROJECT_ROOT / "Images", PROJECT_ROOT / "Audio"]:
        for f in dir_path.rglob("*.import"):
            f.unlink()
            cache_count += 1
    
    print(f"   ✅ 清理了 {cache_count} 个 .import 文件")
    print(f"   💡 请在 Godot 中执行 Project → ReImport All 刷新资源")
    
    # ==================== 总结 ====================
    print(f"\n{'='*60}")
    print(f"🎉 真实素材映射完成！")
    print(f"{'='*60}")
    print(f"📊 统计:")
    print(f"   ✅ 复制/映射: {stats['copied']} 个文件")
    print(f"   ⚠️  跳过: {stats['skipped']} 个文件")
    print(f"   🗑️  清理缓存: {cache_count} 个")
    print(f"\n💡 下一步:")
    print(f"   1. 完全关闭 Godot (Cmd+Q)")
    print(f"   2. 重新打开项目")
    print(f"   3. 等待导入完成 (10-30秒)")
    print(f"   4. 按 F5 运行游戏")
    
    return stats

if __name__ == "__main__":
    map_all_resources()
