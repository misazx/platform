#!/usr/bin/env python3
"""生成 Kenney 风格的玩家角色图标"""

from PIL import Image, ImageDraw
import os

# 创建 Kenney 风格的玩家角色图标 (64x64)
size = (64, 64)
img = Image.new('RGBA', size, (0, 0, 0, 0))
draw = ImageDraw.Draw(img)

# Ironclad 红色战士风格
main_color = (220, 70, 70)
dark_color = (180, 55, 55)
accent_color = (240, 200, 60)

cx, cy = 32, 32

# 头部 - 圆形
draw.ellipse([cx-16, cy-20, cx+16, cy+12], fill=main_color, outline=dark_color, width=2)

# 头盔面罩
draw.arc([cx-12, cy-17, cx+12, cy-7], 0, 180, fill=dark_color, width=2)
draw.rectangle([cx-8, cy-19, cx+8, cy-17], fill=accent_color)

# 身体
draw.rounded_rectangle([cx-14, cy+10, cx+14, cy+28], radius=3, fill=(150, 130, 110), outline=main_color, width=2)

# 武器 - 大剑
sword_points = [
    (cx+18, cy), (cx+28, cy+12), (cx+26, cy+22),
    (cx+20, cy+28), (cx+14, cy+22), (cx+16, cy+12)
]
draw.polygon(sword_points, fill=(200, 200, 210), outline=(100, 100, 110), width=1)

# 保存
output_path = "Icons/Items/iron_sword.png"
img.save(output_path)
print(f"✅ 已生成: {output_path} ({os.path.getsize(output_path)} bytes)")
