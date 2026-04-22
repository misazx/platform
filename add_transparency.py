#!/usr/bin/env python3
"""
为豆包生成的UI图标添加透明通道
检测背景色并将其变为透明，保留主体内容
"""
from PIL import Image
import os
import sys

PROJECT_ROOT = os.path.dirname(os.path.abspath(__file__))
BASE = os.path.join(PROJECT_ROOT, "Client/GameModes/base_game/Resources")

# 需要处理的目录和排除的大背景图
DIRS_TO_PROCESS = [
    ("UI", ["panel_", "btn_", "bar_bg", "settings_menu", "speech_bubble", "Inventory_"]),
    ("Icons/Skills", []),
    ("Icons/Items", []),
    ("Icons/Enemies", []),
    ("Icons/Rest", []),
    ("Icons/Services", []),
    ("Icons/Achievements", []),
]

# 排除的文件前缀（大背景图不需要透明）
EXCLUDE_PREFIXES = {
    "panel_", "btn_", "bar_bg", "settings_menu",
    "speech_bubble", "Inventory_", "slide_", "button_"
}


def is_background_image(filename: str) -> bool:
    """判断是否是大背景图（不需要透明处理）"""
    for prefix in EXCLUDE_PREFIXES:
        if filename.startswith(prefix):
            return True
    return False


def get_background_color(img: Image) -> tuple:
    """从四个角采样确定背景色"""
    pixels = [
        img.getpixel((0, 0)),
        img.getpixel((img.width - 1, 0)),
        img.getpixel((0, img.height - 1)),
        img.getpixel((img.width - 1, img.height - 1)),
    ]
    # 取RGB平均值作为背景色参考
    r = sum(p[0] for p in pixels) // len(pixels)
    g = sum(p[1] for p in pixels) // len(pixels)
    b = sum(p[2] for p in pixels) // len(pixels)
    return (r, g, b)


def make_transparent(img: Image, tolerance: int = 30) -> Image:
    """
    将图片背景变为透明
    使用基于角落颜色的阈值法
    """
    if img.mode != 'RGBA':
        img = img.convert('RGBA')

    bg_color = get_background_color(img)
    datas = img.getdata()

    new_data = []
    for item in datas:
        r, g, b, a = item
        # 计算与背景色的距离
        dr = abs(r - bg_color[0])
        dg = abs(g - bg_color[1])
        db = abs(b - bg_color[2])
        dist = (dr * dr + dg * dg + db * db) ** 0.5

        if dist < tolerance:
            new_data.append((r, g, b, 0))
        else:
            new_data.append((r, g, b, a))

    img.putdata(new_data)
    return img


def process_file(filepath: str) -> bool:
    """处理单个文件"""
    try:
        img = Image.open(filepath)

        if img.mode == 'P':
            img = img.convert('RGBA')
        elif img.mode == 'RGBA':
            # 检查是否已经有透明像素
            has_transparency = False
            for y in range(min(10, img.height)):
                for x in range(min(10, img.width)):
                    if img.getpixel((x, y))[3] < 255:
                        has_transparency = True
                        break
                if has_transparency:
                    break

            # 再检查四角
            if not has_transparency:
                for pos in [(0,0), (img.width-1,0), (0,img.height-1), (img.width-1,img.height-1)]:
                    if img.getpixel(pos)[3] < 128:
                        has_transparency = True
                        break

            if has_transparency:
                print(f"  [SKIP] {os.path.basename(filepath)} 已有透明通道")
                return False

        elif img.mode == 'RGB':
            img = img.convert('RGBA')

        result = make_transparent(img, tolerance=35)

        # 保存
        buf = bytearray()
        result.save(filepath, format='PNG', optimize=True)
        return True

    except Exception as e:
        print(f"  [ERROR] {os.path.basename(filepath)}: {e}")
        return False


def main():
    processed = 0
    skipped = 0
    errors = 0

    for dir_name, _ in DIRS_TO_PROCESS:
        full_dir = os.path.join(BASE, dir_name)
        if not os.path.isdir(full_dir):
            continue

        files = sorted([f for f in os.listdir(full_dir)
                       if f.endswith('.png') and not f.endswith('.import')])
        if not files:
            continue

        print(f"\n📁 {dir_name}/ ({len(files)} files)")

        for fname in files:
            filepath = os.path.join(full_dir, fname)

            if is_background_image(fname):
                skipped += 1
                continue

            if process_file(filepath):
                processed += 1
                print(f"  [OK] {fname}")
            else:
                skipped += 1

    print(f"\n{'='*50}")
    print(f"处理完成 | 成功: {processed}  跳过: {skipped}  错误: {errors}")
    print(f"{'='*50}")


if __name__ == "__main__":
    main()
