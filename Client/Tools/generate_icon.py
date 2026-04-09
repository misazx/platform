#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
生成 macOS 应用程序图标（简化版）
"""

try:
    from PIL import Image, ImageDraw, ImageFont
    import math

    def create_icon():
        # 创建 512x512 图标 (macOS 需要)
        size = 512
        img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)

        # 背景渐变圆角矩形
        margin = 40
        radius = 80

        # 绘制圆角矩形背景
        for y in range(margin, size - margin):
            for x in range(margin, size - margin):
                # 计算是否在圆角矩形内
                in_rect = True

                # 检查四个角的圆形区域
                corners = [
                    (margin + radius, margin + radius),  # 左上
                    (size - margin - radius, margin + radius),  # 右上
                    (margin + radius, size - margin - radius),  # 左下
                    (size - margin - radius, size - margin - radius)  # 右下
                ]

                for cx, cy in corners:
                    if x < cx and y < cy:
                        dist = math.sqrt((x - cx)**2 + (y - cy)**2)
                        if dist > radius:
                            in_rect = False
                            break
                    elif x > cx and y < cy:
                        dist = math.sqrt((x - cx)**2 + (y - cy)**2)
                        if dist > radius:
                            in_rect = False
                            break
                    elif x < cx and y > cy:
                        dist = math.sqrt((x - cx)**2 + (y - cy)**2)
                        if dist > radius:
                            in_rect = False
                            break
                    elif x > cx and y > cy:
                        dist = math.sqrt((x - cx)**2 + (y - cy)**2)
                        if dist > radius:
                            in_rect = False
                            break

                if in_rect:
                    # 渐变色背景
                    progress = (y - margin) / (size - 2 * margin)
                    r = int(74 + (56 - 74) * progress)  # 从 #4a9eff 到 #3880ff
                    g = int(158 + (128 - 158) * progress)
                    b = int(255 + (255 - 255) * progress)

                    img.putpixel((x, y), (r, g, b, 255))

        # 绘制游戏手柄图标（简化的）
        center_x, center_y = size // 2, size // 2

        # 手柄外框
        pad_width = 280
        pad_height = 160
        pad_radius = 60

        # 绘制手柄主体
        draw.rounded_rectangle(
            [center_x - pad_width//2, center_y - pad_height//2,
             center_x + pad_width//2, center_y + pad_height//2],
            radius=pad_radius,
            fill=(255, 255, 255, 230),
            outline=(220, 220, 220, 255),
            width=4
        )

        # 左侧摇杆
        stick_radius = 35
        draw.ellipse(
            [center_x - 90 - stick_radius, center_y - stick_radius,
             center_x - 90 + stick_radius, center_y + stick_radius],
            fill=(100, 100, 100, 200),
            outline=(70, 70, 70, 255),
            width=3
        )

        # 右侧按钮组 (ABXY)
        button_positions = [
            (center_x + 70, center_y - 25),   # 上 (Y)
            (center_x + 95, center_y),         # 右 (B)
            (center_x + 70, center_y + 25),    # 下 (A)
            (center_x + 45, center_y),         # 左 (X)
        ]
        button_colors = [(76, 175, 80), (244, 67, 54), (255, 152, 0), (33, 150, 243)]

        for (bx, by), color in zip(button_positions, button_colors):
            draw.ellipse(
                [bx - 18, by - 18, bx + 18, by + 18],
                fill=color,
                outline=(255, 255, 255, 200),
                width=2
            )

        # 中间的文字 "BUILD"
        try:
            font = ImageFont.truetype("/System/Library/Fonts/Helvetica.ttc", 48)
            small_font = ImageFont.truetype("/System/Library/Fonts/Helvetica.ttc", 20)
        except:
            font = ImageFont.load_default()
            small_font = font

        text = "BUILD"
        bbox = draw.textbbox((0, 0), text, font=font)
        text_width = bbox[2] - bbox[0]
        text_height = bbox[3] - bbox[1]

        draw.text(
            (center_x - text_width//2, center_y - text_height//2 - 5),
            text,
            fill=(50, 50, 50, 255),
            font=font
        )

        # 保存不同尺寸的图标
        icon_path = "/Users/zhuyong/trae-game/打包工具.app/Contents/Resources/icon.png"

        # 保存 512x512 (用于 App Store 和高分辨率显示)
        img.save(icon_path, 'PNG')

        # 创建 icns 文件（macOS 原生格式需要额外工具，这里用 PNG 替代）
        print(f"✅ 图标已生成: {icon_path}")

        return True

    if __name__ == "__main__":
        create_icon()

except ImportError:
    print("⚠️  Pillow 未安装，跳过图标生成")
    print("   可选: pip install Pillow")
    exit(0)
except Exception as e:
    print(f"⚠️  图标生成失败: {e}")
    exit(0)
