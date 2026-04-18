#!/usr/bin/env python3
"""Analyze the main Sprout Lands sprite sheet structure."""
from PIL import Image
import numpy as np

sheet_path = "/Users/zhuyong/trae-game/Sprout Lands - UI Pack - Basic pack/Sprite sheets/Sprite sheet for Basic Pack.png"
img = Image.open(sheet_path).convert("RGBA")
arr = np.array(img)
alpha = arr[:, :, 3]

transparent_cols = set(np.where(np.all(alpha == 0, axis=0))[0])

col_starts = []
in_sprite = False
for x in range(896):
    is_transparent = x in transparent_cols
    if not in_sprite and not is_transparent:
        col_starts.append(x)
        in_sprite = True
    elif in_sprite and is_transparent:
        in_sprite = False

print("Column regions:")
for i, start in enumerate(col_starts):
    end = start
    while end < 896 and end not in transparent_cols:
        end += 1
    width = end - start
    
    col_alpha = alpha[:, start:end]
    transparent_rows = set(np.where(np.all(col_alpha == 0, axis=1))[0])
    
    row_starts = []
    in_sprite_r = False
    for y in range(240):
        is_t = y in transparent_rows
        if not in_sprite_r and not is_t:
            row_starts.append(y)
            in_sprite_r = True
        elif in_sprite_r and is_t:
            in_sprite_r = False
    
    row_info = []
    for j, rs in enumerate(row_starts):
        re = rs
        while re < 240 and re not in transparent_rows:
            re += 1
        row_info.append((rs, re - rs))
    
    print(f"  Col {i}: x={start}, w={width}, rows={row_info}")
