#!/usr/bin/env python3
"""用 Tiny Dungeon 素材拼贴生成专业地图背景"""
from PIL import Image, ImageDraw
from pathlib import Path
import random

DL = Path("Assets_Library/_downloads")
P = Path(".")

W, H = 1280, 720

random.seed(42)

def load_tiles(subdir):
    tiles = []
    td = DL / "tiny-dungeon" / subdir
    if not td.exists():
        td = DL / "tiny-dungeon"
        for d in td.iterdir():
            if d.is_dir():
                tiles_dir = d / "PNG"
                if tiles_dir.exists():
                    for f in tiles_dir.rglob("*.png"):
                        tiles.append(Image.open(f).convert("RGBA"))
                    break
    else:
        for f in td.rglob("*.png"):
            tiles.append(Image.open(f).convert("RGBA"))
    return tiles

def find_tiles():
    all_tiles = []
    for d in (DL / "tiny-dungeon").rglob("*.png"):
        all_tiles.append(str(d))
    return all_tiles

def make_background(name, base_color, tile_filter=None):
    img = Image.new('RGBA', (W, H), base_color + (255,))
    draw = ImageDraw.Draw(img)
    
    for y in range(0, H, 16):
        for x in range(0, W, 16):
            r = random.randint(-15, 15)
            c = tuple(max(0, min(255, v + r)) for v in base_color) + (255,)
            draw.rectangle([x, y, x+15, y+15], fill=c)
    
    all_tile_paths = find_tiles()
    matching = []
    for tp in all_tile_paths:
        name_lower = Path(tp).stem.lower()
        if tile_filter:
            if any(kw in name_lower for kw in tile_filter):
                matching.append(tp)
    
    if matching:
        for _ in range(30):
            tp = random.choice(matching)
            tile = Image.open(tp).convert("RGBA")
            tile = tile.resize((32, 32), Image.LANCZOS)
            x = random.randint(0, W - 32)
            y = random.randint(0, H - 32)
            img.paste(tile, (x, y), tile)
    
    overlay = Image.new('RGBA', (W, H), (0, 0, 0, 80))
    img = Image.alpha_composite(img, overlay)
    
    draw2 = ImageDraw.Draw(img)
    colors = {
        "glory": (220, 180, 100),
        "hive": (160, 130, 80),
        "overgrowth": (80, 160, 80),
        "underdocks": (80, 120, 180),
    }
    label_color = colors.get(name, (200, 200, 200))
    draw2.rounded_rectangle([20, 20, 300, 60], radius=8, fill=(0, 0, 0, 150))
    draw2.text((35, 30), name.upper(), fill=label_color + (230,))
    
    return img

backgrounds = {
    "glory": ((140, 110, 70), ["wall", "brick", "stone", "floor", "door"]),
    "hive": ((100, 80, 50), ["wall", "dark", "floor", "hole", "web"]),
    "overgrowth": ((60, 100, 60), ["grass", "tree", "plant", "vine", "floor"]),
    "underdocks": ((50, 70, 100), ["water", "floor", "dark", "wall", "bridge"]),
}

for name, (color, tiles_kw) in backgrounds.items():
    bg = make_background(name, color, tiles_kw)
    dst = P / "Images" / "Backgrounds" / f"{name}.png"
    bg.save(str(dst))
    print(f"OK {dst} ({bg.size})")

print("Done")
