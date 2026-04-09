#!/usr/bin/env python3
"""
图像资源生成器 - 使用Python PIL生成游戏美术资源
"""

from PIL import Image, ImageDraw
import math
import random
import os

class ImageGenerator:
    def __init__(self, output_dir):
        self.output_dir = output_dir
        os.makedirs(output_dir, exist_ok=True)
        
    def save_image(self, img, filename):
        """保存图像"""
        filepath = os.path.join(self.output_dir, filename)
        img.save(filepath, 'PNG')
        print(f"已生成: {filepath}")
    
    def draw_circle_filled(self, draw, cx, cy, radius, color):
        """绘制填充圆形"""
        draw.ellipse([cx - radius, cy - radius, cx + radius, cy + radius], fill=color)
    
    def draw_ellipse_filled(self, draw, cx, cy, rx, ry, color):
        """绘制填充椭圆"""
        draw.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=color)
    
    def draw_rect_filled(self, draw, x, y, w, h, color):
        """绘制填充矩形"""
        draw.rectangle([x, y, x + w, y + h], fill=color)
    
    def draw_line_thick(self, draw, x0, y0, x1, y1, color, thickness=2):
        """绘制粗线"""
        draw.line([x0, y0, x1, y1], fill=color, width=thickness)
    
    def draw_triangle_filled(self, draw, x0, y0, x1, y1, x2, y2, color):
        """绘制填充三角形"""
        draw.polygon([(x0, y0), (x1, y1), (x2, y2)], fill=color)

def generate_character_portraits(base_dir):
    """生成角色肖像"""
    print("=== 生成角色肖像 ===")
    
    characters = {
        'Ironclad': '#FF4444',
        'Silent': '#44FF44',
        'Defect': '#4444FF',
        'Watcher': '#AA44AA',
        'Necromancer': '#444444',
        'Heir': '#FFAA44'
    }
    
    output_dir = os.path.join(base_dir, 'Images', 'Characters')
    os.makedirs(output_dir, exist_ok=True)
    
    for char_name, color_hex in characters.items():
        img = Image.new('RGBA', (200, 280), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        
        # 背景色
        color = Image.new('RGB', (1, 1), color_hex).getpixel((0, 0))
        bg_color = tuple(int(c * 0.4) for c in color[:3])
        img.paste(bg_color, [0, 0, 200, 280])
        
        # 绘制背景纹理
        for y in range(280):
            for x in range(200):
                noise = math.sin(x * 0.1) * math.cos(y * 0.1) * 0.1
                c = tuple(int(v * (0.5 + noise)) for v in bg_color)
                img.putpixel((x, y), c + (255,))
        
        # 绘制角色轮廓
        dark = tuple(int(c * 0.6) for c in color[:3])
        light = tuple(min(255, int(c * 1.3)) for c in color[:3])
        
        # 头部
        gen = ImageGenerator(output_dir)
        gen.draw_circle_filled(draw, 100, 120, 30, dark + (255,))
        # 身体
        gen.draw_ellipse_filled(draw, 100, 160, 25, 40, dark + (255,))
        # 腿部
        gen.draw_rect_filled(draw, 75, 195, 20, 35, dark + (255,))
        gen.draw_rect_filled(draw, 105, 195, 20, 35, dark + (255,))
        # 手臂
        gen.draw_line_thick(draw, 85, 170, 60, 210, light + (255,), 5)
        gen.draw_line_thick(draw, 115, 170, 140, 210, light + (255,), 5)
        
        # 边框
        frame_color = tuple(min(255, int(c * 1.4)) for c in color[:3]) + (200,)
        draw.rectangle([0, 0, 199, 279], outline=frame_color, width=2)
        
        gen.save_image(img, f'{char_name}.png')

def generate_backgrounds(base_dir):
    """生成背景图像"""
    print("=== 生成背景图像 ===")
    
    backgrounds = {
        'glory': (153, 128, 77),
        'hive': (128, 102, 51),
        'overgrowth': (77, 128, 77),
        'underdocks': (51, 77, 102)
    }
    
    output_dir = os.path.join(base_dir, 'Images', 'Backgrounds')
    os.makedirs(output_dir, exist_ok=True)
    
    for bg_name, base_color in backgrounds.items():
        img = Image.new('RGBA', (1280, 720), (0, 0, 0, 0))
        
        for y in range(720):
            for x in range(1280):
                noise = math.sin(x * 0.02 + y * 0.015) * 0.05 + \
                       math.sin(x * 0.035 - y * 0.025) * 0.03 + \
                       math.sin(y * 0.03) * 0.02
                
                darken = y / 720.0 * 0.3
                c = tuple(int(v * (1 - darken + noise)) for v in base_color)
                img.putpixel((x, y), c + (255,))
        
        # 添加地面
        floor_y = int(720 * 0.75)
        floor_color = tuple(int(v * 0.7) for v in base_color) + (128,)
        
        for y in range(floor_y, 720):
            for x in range(1280):
                existing = img.getpixel((x, y))
                blend = (y - floor_y) / (720 - floor_y)
                new_color = tuple(int(existing[i] * (1 - blend * 0.5) + floor_color[i] * blend * 0.5) for i in range(3))
                img.putpixel((x, y), new_color + (255,))
        
        gen = ImageGenerator(output_dir)
        gen.save_image(img, f'{bg_name}.png')

def generate_ui_icons(base_dir):
    """生成UI图标"""
    print("=== 生成UI图标 ===")
    
    # 休息点图标
    rest_icons = {
        'heal': '#4DFF66',
        'upgrade': '#FFD94D',
        'recall': '#8080FF',
        'smith': '#CC804D',
        'default': '#808080'
    }
    
    output_dir = os.path.join(base_dir, 'Icons', 'Rest')
    os.makedirs(output_dir, exist_ok=True)
    
    gen = ImageGenerator(output_dir)
    
    for icon_name, color_hex in rest_icons.items():
        img = Image.new('RGBA', (48, 48), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        
        color = Image.new('RGB', (1, 1), color_hex).getpixel((0, 0))
        
        if icon_name == 'heal':
            # 心形
            gen.draw_circle_filled(draw, 18, 21, 7, color + (255,))
            gen.draw_circle_filled(draw, 30, 21, 7, color + (255,))
            gen.draw_triangle_filled(draw, 12, 25, 36, 25, 24, 40, color + (255,))
        elif icon_name == 'upgrade':
            # 向上箭头
            gen.draw_line_thick(draw, 24, 40, 24, 8, color + (255,), 4)
            gen.draw_line_thick(draw, 24, 8, 16, 18, color + (255,), 4)
            gen.draw_line_thick(draw, 24, 8, 32, 18, color + (255,), 4)
        elif icon_name == 'recall':
            # 回收图标
            draw.arc([12, 12, 36, 36], 0, 270, fill=color + (255,), width=3)
            gen.draw_line_thick(draw, 36, 24, 36, 16, color + (255,), 3)
            gen.draw_line_thick(draw, 36, 24, 28, 24, color + (255,), 3)
        elif icon_name == 'smith':
            # 锤子
            gen.draw_rect_filled(draw, 14, 9, 20, 12, color + (255,))
            gen.draw_rect_filled(draw, 21, 21, 6, 20, tuple(int(c * 0.8) for c in color) + (255,))
        else:
            gen.draw_circle_filled(draw, 24, 24, 16, color + (255,))
        
        gen.save_image(img, f'{icon_name}.png')
    
    # 成就图标
    achievement_icons = {
        'FirstVictory': '#FFD94D',
        'Kill100': '#CC3333',
        'AllRelics': '#994DCC',
        'NoDamage': '#4DCC4D'
    }
    
    output_dir = os.path.join(base_dir, 'Icons', 'Achievements')
    os.makedirs(output_dir, exist_ok=True)
    
    gen = ImageGenerator(output_dir)
    
    for icon_name, color_hex in achievement_icons.items():
        img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        
        color = Image.new('RGB', (1, 1), color_hex).getpixel((0, 0))
        
        if icon_name == 'FirstVictory':
            # 奖杯
            gen.draw_rect_filled(draw, 20, 17, 24, 20, color + (255,))
            gen.draw_rect_filled(draw, 27, 37, 10, 8, tuple(int(c * 0.8) for c in color) + (255,))
            gen.draw_rect_filled(draw, 22, 45, 20, 4, tuple(int(c * 0.7) for c in color) + (255,))
            gen.draw_rect_filled(draw, 10, 20, 6, 12, tuple(min(255, int(c * 1.2)) for c in color) + (255,))
            gen.draw_rect_filled(draw, 48, 20, 6, 12, tuple(min(255, int(c * 1.2)) for c in color) + (255,))
        elif icon_name == 'Kill100':
            # 骷髅
            gen.draw_ellipse_filled(draw, 32, 27, 15, 18, color + (255,))
            gen.draw_rect_filled(draw, 22, 40, 20, 10, color + (255,))
            gen.draw_circle_filled(draw, 26, 24, 4, (26, 26, 26, 255))
            gen.draw_circle_filled(draw, 38, 24, 4, (26, 26, 26, 255))
        elif icon_name == 'AllRelics':
            # 菱形
            gen.draw_triangle_filled(draw, 32, 12, 12, 32, 32, 52, color + (255,))
            gen.draw_triangle_filled(draw, 32, 12, 52, 32, 32, 52, color + (255,))
        elif icon_name == 'NoDamage':
            # 盾牌
            gen.draw_line_thick(draw, 17, 17, 47, 17, color + (255,), 4)
            gen.draw_line_thick(draw, 17, 17, 17, 32, color + (255,), 3)
            gen.draw_line_thick(draw, 47, 17, 47, 32, color + (255,), 3)
            gen.draw_line_thick(draw, 17, 32, 32, 50, color + (255,), 4)
            gen.draw_line_thick(draw, 47, 32, 32, 50, color + (255,), 4)
        
        gen.save_image(img, f'{icon_name}.png')

def generate_enemy_images(base_dir):
    """生成敌人图像"""
    print("=== 生成敌人图像 ===")
    
    enemies = {
        'Cultist': {'color': (153, 77, 153), 'type': 'humanoid'},
        'JawWorm': {'color': (128, 102, 77), 'type': 'beast'},
        'Lagavulin': {'color': (102, 128, 153), 'type': 'construct'},
        'TheGuardian': {'color': (179, 77, 77), 'type': 'boss'}
    }
    
    output_dir = os.path.join(base_dir, 'Images', 'Enemies')
    os.makedirs(output_dir, exist_ok=True)
    
    gen = ImageGenerator(output_dir)
    
    for enemy_name, data in enemies.items():
        color = data['color']
        enemy_type = data['type']
        
        img = Image.new('RGBA', (150, 200), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        
        # 背景
        bg_color = tuple(int(c * 0.5) for c in color)
        img.paste(bg_color + (255,), [0, 0, 150, 200])
        
        # 绘制背景纹理
        for y in range(200):
            for x in range(150):
                noise = math.sin(x * 0.1) * math.cos(y * 0.1) * 0.05
                c = tuple(int(v * (0.6 + noise)) for v in bg_color)
                img.putpixel((x, y), c + (255,))
        
        dark = tuple(int(c * 0.7) for c in color)
        light = tuple(min(255, int(c * 1.2)) for c in color)
        
        if enemy_type == 'humanoid':
            gen.draw_circle_filled(draw, 75, 70, 25, dark + (255,))
            gen.draw_ellipse_filled(draw, 75, 120, 30, 45, dark + (255,))
            gen.draw_line_thick(draw, 50, 110, 30, 150, light + (255,), 5)
            gen.draw_line_thick(draw, 100, 110, 120, 150, light + (255,), 5)
        elif enemy_type == 'beast':
            gen.draw_ellipse_filled(draw, 75, 100, 50, 35, dark + (255,))
            gen.draw_circle_filled(draw, 40, 90, 12, dark + (255,))
            gen.draw_circle_filled(draw, 110, 90, 12, dark + (255,))
            gen.draw_circle_filled(draw, 40, 88, 5, (255, 77, 77, 255))
            gen.draw_circle_filled(draw, 110, 88, 5, (255, 77, 77, 255))
        elif enemy_type == 'construct':
            gen.draw_rect_filled(draw, 40, 60, 70, 80, dark + (255,))
            gen.draw_rect_filled(draw, 50, 70, 20, 20, (51, 204, 255, 255))
            gen.draw_rect_filled(draw, 80, 70, 20, 20, (51, 204, 255, 255))
            gen.draw_rect_filled(draw, 60, 110, 30, 20, light + (255,))
        elif enemy_type == 'boss':
            gen.draw_circle_filled(draw, 75, 80, 40, dark + (255,))
            gen.draw_ellipse_filled(draw, 75, 140, 45, 50, dark + (255,))
            gen.draw_circle_filled(draw, 57, 75, 8, (255, 128, 0, 255))
            gen.draw_circle_filled(draw, 93, 75, 8, (255, 128, 0, 255))
            gen.draw_line_thick(draw, 40, 50, 30, 30, light + (255,), 6)
            gen.draw_line_thick(draw, 110, 50, 120, 30, light + (255,), 6)
        
        # 边框
        frame_color = tuple(min(255, int(c * 1.3)) for c in color) + (180,)
        draw.rectangle([0, 0, 149, 199], outline=frame_color, width=2)
        
        gen.save_image(img, f'{enemy_name}.png')

def generate_relic_images(base_dir):
    """生成遗物图像"""
    print("=== 生成遗物图像 ===")
    
    output_dir = os.path.join(base_dir, 'Images', 'Relics')
    os.makedirs(output_dir, exist_ok=True)
    
    gen = ImageGenerator(output_dir)
    
    for i in range(20):
        color = (random.randint(50, 255), random.randint(50, 255), random.randint(50, 255))
        
        img = Image.new('RGBA', (80, 80), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        
        dark = tuple(int(c * 0.7) for c in color)
        
        gen.draw_circle_filled(draw, 40, 40, 37, dark + (255,))
        gen.draw_circle_filled(draw, 40, 40, 34, color + (255,))
        
        # 随机形状
        shape_type = random.randint(0, 5)
        if shape_type == 0:
            gen.draw_triangle_filled(draw, 40, 18, 18, 40, 40, 62, (255, 255, 255, 255))
            gen.draw_triangle_filled(draw, 40, 18, 62, 40, 40, 62, (255, 255, 255, 255))
        elif shape_type == 1:
            for angle in range(0, 360, 36):
                rad = math.radians(angle)
                rad2 = math.radians(angle + 18)
                x1 = 40 + int(math.cos(rad) * 20)
                y1 = 40 + int(math.sin(rad) * 20)
                x2 = 40 + int(math.cos(rad2) * 8)
                y2 = 40 + int(math.sin(rad2) * 8)
                draw.line([x1, y1, x2, y2], fill=(255, 255, 255, 255), width=2)
        elif shape_type == 2:
            gen.draw_ellipse_filled(draw, 40, 40, 18, 9, (255, 255, 255, 255))
            gen.draw_circle_filled(draw, 40, 40, 6, (26, 26, 38, 255))
            gen.draw_circle_filled(draw, 37, 37, 3, (255, 255, 255, 255))
        elif shape_type == 3:
            gen.draw_line_thick(draw, 40, 20, 40, 60, (255, 255, 255, 255), 4)
            gen.draw_line_thick(draw, 20, 40, 60, 40, (255, 255, 255, 255), 4)
        elif shape_type == 4:
            gen.draw_circle_filled(draw, 40, 40, 20, (255, 255, 255, 255))
            gen.draw_circle_filled(draw, 50, 33, 15, (20, 15, 30, 255))
        else:
            gen.draw_circle_filled(draw, 40, 40, 16, (255, 255, 255, 255))
            gen.draw_circle_filled(draw, 40, 40, 11, (26, 20, 30, 255))
        
        gen.save_image(img, f'Relic_{i}.png')

def generate_potion_images(base_dir):
    """生成药水图像"""
    print("=== 生成药水图像 ===")
    
    output_dir = os.path.join(base_dir, 'Images', 'Potions')
    os.makedirs(output_dir, exist_ok=True)
    
    gen = ImageGenerator(output_dir)
    
    for i in range(15):
        color = (random.randint(50, 255), random.randint(50, 255), random.randint(50, 255))
        
        img = Image.new('RGBA', (56, 56), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        
        # 瓶子
        gen.draw_rect_filled(draw, 25, 8, 6, 5, (179, 179, 191, 255))
        gen.draw_ellipse_filled(draw, 28, 30, 10, 14, (179, 179, 191, 255))
        gen.draw_ellipse_filled(draw, 28, 33, 7, 10, color + (255,))
        gen.draw_circle_filled(draw, 28, 20, 3, (204, 217, 230, 255))
        
        gen.save_image(img, f'Potion_{i}.png')

def main():
    print("========================================")
    print("   图像资源生成器 - 杀戮尖塔2")
    print("========================================")
    
    base_dir = os.path.dirname(os.path.abspath(__file__))
    
    generate_character_portraits(base_dir)
    generate_backgrounds(base_dir)
    generate_ui_icons(base_dir)
    generate_enemy_images(base_dir)
    generate_relic_images(base_dir)
    generate_potion_images(base_dir)
    
    print("========================================")
    print("   所有图像资源生成完成！")
    print("========================================")

if __name__ == '__main__':
    main()
