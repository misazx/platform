#!/usr/bin/env python3
"""修复卡牌图标尺寸 + 生成专业卡牌图标"""
from PIL import Image, ImageDraw, ImageFont
from pathlib import Path

DL = Path("Assets_Library/_downloads")
P = Path(".")

CARD_W, CARD_H = 120, 170

def make_card_icon(name, color, symbol_draw_fn):
    img = Image.new('RGBA', (CARD_W, CARD_H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    bg = color + (230,)
    border = tuple(max(0, c - 40) for c in color) + (255,)
    
    draw.rounded_rectangle([2, 2, CARD_W-3, CARD_H-3], radius=8, fill=bg, outline=border, width=2)
    
    inner_bg = tuple(min(255, c + 30) for c in color) + (60,)
    draw.rounded_rectangle([8, 8, CARD_W-9, CARD_H-30], radius=5, fill=inner_bg)
    
    symbol_draw_fn(draw, CARD_W//2, (CARD_H-30)//2 + 8)
    
    label_color = (255, 255, 255, 220)
    name_text = name.replace("_", " ").title()
    bbox = draw.textbbox((0, 0), name_text)
    tw = bbox[2] - bbox[0]
    draw.text(((CARD_W - tw) // 2, CARD_H - 26), name_text, fill=label_color)
    
    return img

def draw_sword(draw, cx, cy):
    draw.rectangle([cx-2, cy-35, cx+2, cy+10], fill=(220, 220, 230))
    draw.rectangle([cx-12, cy+5, cx+12, cy+10], fill=(180, 150, 80))
    draw.polygon([(cx-2, cy-35), (cx+2, cy-35), (cx, cy-45)], fill=(220, 220, 230))

def draw_shield(draw, cx, cy):
    points = [(cx, cy-30), (cx+25, cy-15), (cx+25, cy+10), (cx, cy+30), (cx-25, cy+10), (cx-25, cy-15)]
    draw.polygon(points, fill=(100, 150, 220))
    draw.polygon(points, outline=(60, 100, 180), width=2)
    inner = [(cx, cy-20), (cx+16, cy-8), (cx+16, cy+6), (cx, cy+20), (cx-16, cy+6), (cx-16, cy-8)]
    draw.polygon(inner, fill=(130, 180, 240))

def draw_star(draw, cx, cy):
    import math
    points = []
    for i in range(10):
        angle = math.pi / 2 + i * math.pi / 5
        r = 25 if i % 2 == 0 else 12
        points.append((cx + r * math.cos(angle), cy - r * math.sin(angle)))
    draw.polygon(points, fill=(255, 200, 60))
    draw.polygon(points, outline=(200, 150, 30), width=2)

def draw_wave(draw, cx, cy):
    for i in range(3):
        y = cy - 15 + i * 15
        points = []
        for x in range(cx - 25, cx + 26, 2):
            py = y + 8 * math.sin((x - cx + 25) * 0.3)
            points.append((x, py))
        if len(points) > 1:
            draw.line(points, fill=(100, 180, 255), width=3)

def draw_cleave(draw, cx, cy):
    draw.ellipse([cx-28, cy-28, cx+28, cy+28], fill=(200, 60, 60), outline=(150, 40, 40), width=2)
    draw.ellipse([cx-18, cy-18, cx+18, cy+18], fill=(230, 80, 80))

import math

cards = {
    "strike": ((180, 50, 50), draw_sword),
    "defend": ((50, 100, 180), draw_shield),
    "bash": ((200, 60, 40), draw_star),
    "cleave": ((190, 55, 55), draw_cleave),
    "iron_wave": ((60, 120, 190), draw_wave),
}

for name, (color, draw_fn) in cards.items():
    img = make_card_icon(name, color, draw_fn)
    dst = P / "Icons" / "Cards" / f"{name}.png"
    img.save(str(dst))
    print(f"OK {dst} ({img.size})")

print("Done")
