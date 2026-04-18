#!/usr/bin/env python3
"""Analyze Sprout Lands sprite sheet layout and extract individual sprites."""
from PIL import Image
import numpy as np
import os

SHEET_DIR = "/Users/zhuyong/trae-game/Sprout Lands - UI Pack - Basic pack/Sprite sheets"
OUT_DIR = "/Users/zhuyong/trae-game/Client/GameModes/base_game/Resources/UI"

def analyze_sheet(sheet_path: str) -> list:
    """Detect individual sprites in a sprite sheet by finding transparent gaps."""
    img = Image.open(sheet_path).convert("RGBA")
    arr = np.array(img)
    alpha = arr[:, :, 3]
    
    transparent_cols = np.where(np.all(alpha == 0, axis=0))[0]
    col_groups = []
    if len(transparent_cols) > 0:
        start = transparent_cols[0]
        for i in range(1, len(transparent_cols)):
            if transparent_cols[i] - transparent_cols[i-1] > 1:
                col_groups.append((start, transparent_cols[i-1]))
                start = transparent_cols[i]
        col_groups.append((start, transparent_cols[-1]))
    
    transparent_rows = np.where(np.all(alpha == 0, axis=1))[0]
    row_groups = []
    if len(transparent_rows) > 0:
        start = transparent_rows[0]
        for i in range(1, len(transparent_rows)):
            if transparent_rows[i] - transparent_rows[i-1] > 1:
                row_groups.append((start, transparent_rows[i-1]))
                start = transparent_rows[i]
        row_groups.append((start, transparent_rows[-1]))
    
    x_bounds = [0]
    for s, e in col_groups:
        x_bounds.extend([s, e + 1])
    x_bounds.append(img.size[0])
    
    y_bounds = [0]
    for s, e in row_groups:
        y_bounds.extend([s, e + 1])
    y_bounds.append(img.size[1])
    
    sprites = []
    for i in range(0, len(y_bounds)-1, 2):
        for j in range(0, len(x_bounds)-1, 2):
            y1, y2 = y_bounds[i], y_bounds[i+1]
            x1, x2 = x_bounds[j], x_bounds[j+1]
            if x2 > x1 and y2 > y1:
                region = alpha[y1:y2, x1:x2]
                if np.any(region > 0):
                    sprites.append({"x": x1, "y": y1, "w": x2-x1, "h": y2-y1})
    
    return sprites

# Analyze main sheet
main_sheet = os.path.join(SHEET_DIR, "Sprite sheet for Basic Pack.png")
print(f"=== Main Sheet: {main_sheet} ===")
sprites = analyze_sheet(main_sheet)
for i, s in enumerate(sprites):
    print(f"  [{i+1}] pos=({s['x']},{s['y']}) size={s['w']}x{s['h']}")

# Analyze button sheets
print(f"\n=== Button Sheets ===")
for name in ["Square Buttons 26x26.png", "Square Buttons 26x19.png", 
             "Square Buttons 19x26.png", "Small Square Buttons.png",
             "UI Settings Buttons.png", "UI Big Play Button.png"]:
    path = os.path.join(SHEET_DIR, "buttons", name) if "Square" in name or "Small" in name else os.path.join(SHEET_DIR, name)
    if not os.path.exists(path):
        path = os.path.join(SHEET_DIR, name)
    if os.path.exists(path):
        img = Image.open(path)
        print(f"\n  {name}: {img.size[0]}x{img.size[1]}")
        sprites = analyze_sheet(path)
        for i, s in enumerate(sprites):
            print(f"    [{i+1}] pos=({s['x']},{s['y']}) size={s['w']}x{s['h']}")

# Analyze icon sheets
print(f"\n=== Icon Sheets ===")
for name in ["All Icons.png", "white icons.png", "Special Icons.png"]:
    path = os.path.join(SHEET_DIR, "Icons", name)
    if not os.path.exists(path):
        path = os.path.join(SHEET_DIR, "Icons", "special icons", name)
    if os.path.exists(path):
        img = Image.open(path)
        print(f"\n  {name}: {img.size[0]}x{img.size[1]}")
        sprites = analyze_sheet(path)
        for i, s in enumerate(sprites):
            print(f"    [{i+1}] pos=({s['x']},{s['y']}) size={s['w']}x{s['h']}")

# Analyze dialogue sheets
print(f"\n=== Dialogue Sheets ===")
for name in ["dialog box.png", "dialog box small.png", "dialog box medium.png", "dialog box big.png"]:
    path = os.path.join(SHEET_DIR, "Dialouge UI", name)
    if os.path.exists(path):
        img = Image.open(path)
        print(f"\n  {name}: {img.size[0]}x{img.size[1]}")
        sprites = analyze_sheet(path)
        for i, s in enumerate(sprites):
            print(f"    [{i+1}] pos=({s['x']},{s['y']}) size={s['w']}x{s['h']}")
