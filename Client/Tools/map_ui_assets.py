#!/usr/bin/env python3
"""精确映射UI资源 - 使用Kenney官方真实素材"""
from PIL import Image
from pathlib import Path

DL = Path("../../../Assets_Library/_downloads")
P = Path("../GameModes/base_game/Resources")

mappings = {
    "Icons/Cards/strike.png": DL / "ui-pack/PNG/Red/Default/icon_cross.png",
    "Icons/Cards/defend.png": DL / "ui-pack/PNG/Blue/Default/icon_checkmark.png",
    "Icons/Cards/bash.png": DL / "ui-pack/PNG/Red/Default/star_outline_depth.png",
    "Icons/Cards/cleave.png": DL / "ui-pack/PNG/Red/Default/icon_circle.png",
    "Icons/Cards/iron_wave.png": DL / "ui-pack/PNG/Blue/Default/icon_outline_square.png",
    "Icons/Skills/fireball.png": DL / "ui-pack/PNG/Red/Default/star_outline_depth.png",
    "Icons/Skills/heal.png": DL / "ui-pack/PNG/Green/Default/icon_checkmark.png",
    "Icons/Skills/dash.png": DL / "ui-pack/PNG/Blue/Default/icon_outline_square.png",
    "Icons/Skills/iron_skin.png": DL / "ui-pack/PNG/Yellow/Default/icon_square.png",
    "Icons/Items/iron_sword.png": DL / "platformer-characters/PNG/Soldier/Limbs/head.png",
    "Icons/Items/health_potion_small.png": DL / "monster-builder-pack/PNG/Default/body_redA.png",
    "Icons/Items/health_potion_large.png": DL / "monster-builder-pack/PNG/Default/body_redB.png",
    "Icons/Items/steel_armor.png": DL / "monster-builder-pack/PNG/Default/body_darkA.png",
    "Icons/Relics/burning_blood.png": DL / "ui-pack/PNG/Red/Default/star_outline_depth.png",
    "Icons/Relics/anchor.png": DL / "ui-pack/PNG/Blue/Default/icon_circle.png",
    "Icons/Relics/lantern.png": DL / "ui-pack/PNG/Yellow/Default/star_outline_depth.png",
    "Icons/Relics/ice_cream.png": DL / "ui-pack/PNG/Blue/Default/icon_outline_checkmark.png",
    "Icons/Rest/heal.png": DL / "ui-pack/PNG/Green/Default/icon_checkmark.png",
    "Icons/Rest/upgrade.png": DL / "ui-pack/PNG/Yellow/Default/star_outline_depth.png",
    "Icons/Rest/recall.png": DL / "ui-pack/PNG/Blue/Default/icon_outline_checkmark.png",
    "Icons/Rest/smith.png": DL / "ui-pack/PNG/Red/Default/icon_cross.png",
    "Icons/Rest/default.png": DL / "ui-pack/PNG/Grey/Default/icon_circle.png",
    "Icons/Achievements/FirstVictory.png": DL / "ui-pack/PNG/Yellow/Default/star_outline_depth.png",
    "Icons/Achievements/Kill100.png": DL / "ui-pack/PNG/Red/Default/icon_cross.png",
    "Icons/Achievements/AllRelics.png": DL / "ui-pack/PNG/Extra/Default/icon_circle.png",
    "Icons/Achievements/NoDamage.png": DL / "ui-pack/PNG/Green/Default/icon_checkmark.png",
    "Icons/Services/default.png": DL / "ui-pack/PNG/Grey/Default/icon_circle.png",
    "Images/Potions/HealthPotion.png": DL / "monster-builder-pack/PNG/Default/body_redC.png",
    "Images/Potions/StrengthPotion.png": DL / "monster-builder-pack/PNG/Default/body_redA.png",
    "Images/Potions/BlockPotion.png": DL / "monster-builder-pack/PNG/Default/body_blueA.png",
    "Images/Potions/FirePotion.png": DL / "monster-builder-pack/PNG/Default/body_redB.png",
    "Images/Potions/EnergyPotion.png": DL / "ui-pack/PNG/Yellow/Default/icon_square.png",
    "Images/Relics/BurningBlood.png": DL / "ui-pack/PNG/Red/Default/star_outline_depth.png",
    "Images/Relics/Anchor.png": DL / "ui-pack/PNG/Blue/Default/icon_circle.png",
    "Images/Relics/Lantern.png": DL / "ui-pack/PNG/Yellow/Default/star_outline_depth.png",
    "Images/Relics/IceCream.png": DL / "ui-pack/PNG/Blue/Default/icon_outline_checkmark.png",
}

sizes = {
    "Icons/Cards/": (120, 170),
    "Icons/Skills/": (64, 64),
    "Icons/Items/": (64, 64),
    "Icons/Relics/": (64, 64),
    "Icons/Rest/": (48, 48),
    "Icons/Achievements/": (64, 64),
    "Icons/Services/": (48, 48),
    "Images/Potions/": (56, 56),
    "Images/Relics/": (80, 80),
}

copied = 0
skipped = 0

for dst_rel, src in mappings.items():
    dst = P / dst_rel
    dst.parent.mkdir(parents=True, exist_ok=True)
    
    if not src.exists():
        print(f"  SKIP {dst_rel} - source not found: {src.name}")
        skipped += 1
        continue
    
    size = None
    for prefix, s in sizes.items():
        if dst_rel.startswith(prefix):
            size = s
            break
    
    img = Image.open(src).convert("RGBA")
    if size:
        img = img.resize(size, Image.LANCZOS)
    img.save(str(dst))
    print(f"  OK {dst_rel} <- {src.name} {size or 'original'}")
    copied += 1

print(f"\nDone: {copied} copied, {skipped} skipped")
