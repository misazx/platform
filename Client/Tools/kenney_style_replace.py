#!/usr/bin/env python3
"""
Godot Roguelike - 🎨 终极资源替换器 v8.0 (KENNEY STYLE)
=====================================================

用 Kenney 设计风格重新生成所有游戏资源：
• 精细的矢量风格图标（模仿 Kenney 的扁平化设计）
• 正确的尺寸和颜色编码
• 完全覆盖所有配置文件需要的资源

运行:
  python3 kenney_style_replace.py    # 一键全部替换
"""

import os
import sys
import json
import shutil
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Tuple


try:
    from PIL import Image, ImageDraw, ImageFont
    HAS_PIL = True
except ImportError:
    os.system("pip3 install Pillow -q")
    from PIL import Image, ImageDraw, ImageFont
    HAS_PIL = True


PROJECT_ROOT = Path(__file__).parent

DIRS = {
    "cards": PROJECT_ROOT / "Icons" / "Cards",
    "enemies_icon": PROJECT_ROOT / "Icons" / "Enemies",
    "relics": PROJECT_ROOT / "Icons" / "Relics",
    "skills": PROJECT_ROOT / "Icons" / "Skills",
    "items": PROJECT_ROOT / "Icons" / "Items",
    "characters": PROJECT_ROOT / "Images" / "Characters",
    "enemies_full": PROJECT_ROOT / "Images" / "Enemies",
    "backgrounds": PROJECT_ROOT / "Images" / "Backgrounds",
}


class KenneyStyleGenerator:
    """Kenney 风格资源生成器"""
    
    def __init__(self):
        self.stats = {"generated": 0, "replaced": 0}
        self._ensure_dirs()
    
    def _ensure_dirs(self):
        for d in DIRS.values():
            d.mkdir(parents=True, exist_ok=True)
    
    # ==================== 卡牌图标 (Kenney 风格) ====================
    
    def generate_card_kenney(self, name: str, card_type: str, color_hex: str) -> Image.Image:
        """生成 Kenney 风格的卡牌图标"""
        size = (120, 170)
        img = Image.new('RGBA', size, (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        
        # 解析颜色
        r, g, b = int(color_hex[1:3], 16), int(color_hex[3:5], 16), int(color_hex[5:7], 16)
        border_color = (r, g, b)
        fill_color = (255, 255, 255)
        dark_color = (max(0, r-40), max(0, g-40), max(0, b-40))
        
        # 外框 - Kenney 圆角矩形风格
        margin = 6
        draw.rounded_rectangle(
            [margin, margin, size[0]-margin, size[1]-margin],
            radius=10,
            fill=(245, 245, 250, 255),
            outline=border_color,
            width=4
        )
        
        # 内部装饰线
        inner_margin = 14
        draw.rounded_rectangle(
            [inner_margin, inner_margin+20, size[0]-inner_margin, size[1]-inner_margin-25],
            radius=6,
            outline=(200, 200, 210),
            width=1
        )
        
        cx, cy = size[0] // 2, (size[1] // 2) + 5
        
        if card_type == "Attack":
            # 剑/武器 - Kenney 简洁风格
            self._draw_sword_kenney(draw, cx, cy-8, border_color)
        elif card_type == "Skill":
            if "defend" in name.lower() or "block" in name.lower() or "armor" in name.lower():
                # 盾牌
                self._draw_shield_kenney(draw, cx, cy-5, border_color)
            else:
                # 技能符号
                self._draw_skill_symbol(draw, cx, cy-5, border_color)
        elif card_type == "Power":
            # 能量/力量 - 闪电或星形
            self._draw_power_symbol(draw, cx, cy-5, border_color)
        elif card_type == "Status":
            # 状态效果
            self._draw_status_symbol(draw, cx, cy-5, border_color, name)
        else:
            # 默认攻击图标
            self._draw_sword_kenney(draw, cx, cy-8, border_color)
        
        # 底部名称区域背景
        draw.rectangle([12, size[1]-30, size[0]-12, size[1]-10], 
                    fill=(border_color[0], border_color[1], border_color[2], 40))
        
        return img
    
    def _draw_sword_kenney(self, draw, cx, cy, color):
        """Kenney 风格的剑"""
        # 剑身 - 简洁的菱形
        points = [
            (cx, cy-35),      # 尖端
            (cx+15, cy-10),   # 右边
            (cx+10, cy+5),    # 右下
            (cx, cy+35),      # 底部
            (cx-10, cy+5),    # 左下
            (cx-15, cy-10),   # 左边
        ]
        draw.polygon(points, fill=color, outline=self._darken(color))
        
        # 护手
        draw.line([(cx-18, cy-5), (cx+18, cy-5)], fill=(180, 160, 100), width=4)
        draw.line([(cx-18, cy), (cx+18, cy)], fill=(180, 160, 100), width=4)
        
        # 剑柄
        draw.rectangle([cx-3, cy+5, cx+3, cy+28], fill=(140, 120, 80))
        # 柄头圆
        draw.ellipse([cx-7, cy+26, cx+7, cy+34], fill=(160, 140, 90))
    
    def _draw_shield_kenney(self, draw, cx, cy, color):
        """Kenney 风格的盾牌"""
        # 盾体 - 圆润的五边形
        points = [
            (cx, cy-32),      # 顶部尖
            (cx+28, cy-18),   # 右上
            (cx+28, cy+12),   # 右下
            (cx, cy+28),      # 底部
            (cx-28, cy+12),   # 左下
            (cx-28, cy-18),   # 左上
        ]
        draw.polygon(points, fill=color, outline=self._darken(color), width=2)
        
        # 十字装饰
        draw.rectangle([cx-3, cy-22, cx+3, cy+18], fill=(255, 255, 255, 200), width=3)
        draw.rectangle([cx-18, cy-5, cx+18, cy+5], fill=(255, 255, 255, 200), width=3)
    
    def _draw_skill_symbol(self, draw, cx, cy, color):
        """技能符号"""
        # 旋转箭头/循环符号
        for i in range(3):
            angle = i * 120 - 90
            import math
            rad = math.radians(angle)
            x1 = cx + math.cos(rad) * 22
            y1 = cy + math.sin(rad) * 22
            x2 = cx + math.cos(rad + 0.5) * 12
            y2 = cy + math.sin(rad + 0.5) * 12
            
            draw.line([(x1, y1), (x2, y2)], fill=color, width=3)
        
        # 中心圆点
        draw.ellipse([cx-5, cy-5, cx+5, cy+5], fill=color)
    
    def _draw_power_symbol(self, draw, cx, cy, color):
        """力量/能量符号 - 闪电"""
        points = [
            (cx+2, cy-28), (cx-8, cy-8), (cx+8, cy-8),
            (cx-2, cy), (cx-8, cy+8), (cx+8, cy+8),
            (cx+2, cy+28)
        ]
        draw.polygon(points, fill=color, outline=self._darken(color))
    
    def _draw_status_symbol(self, draw, cx, cy, color, name):
        """状态效果符号"""
        name_lower = name.lower()
        
        if any(w in name_lower for w in ['weak', 'vulnerable']):
            # 易伤 - 向下箭头
            draw.polygon([(cx, cy-25), (cx-18, cy), (cx+18, cy)], fill=color)
            draw.rectangle([cx-18, cy, cx+18, cy+25], fill=color)
            
        elif any(w in name_lower for w in ['strength', 'strong']):
            # 强壮 - 向上箭头 + 肌肉
            draw.rectangle([cx-18, cy-25, cx+18, cy], fill=color)
            draw.polygon([(cx, cy+25), (cx-18, cy), (cx+18, cy)], fill=color)
            
        elif any(w in name_lower for w in ['artifact', 'echo', 'metallic']):
            # 金属感 - 六边形
            for r in [28, 20]:
                draw.regular_polygon((cx, cy-3), 6, r, fill=None, outline=color, width=3)
                
        else:
            # 默认圆形标记
            draw.ellipse([cx-24, cy-28, cx+24, cy+22], outline=color, width=4)
            draw.ellipse([cx-14, cy-18, cx+14, cy+12], fill=(*color, 100))
    
    # ==================== 角色立绘 (Kenney 风格) ====================
    
    def generate_character_kenney(self, char_id: str, char_class: str, bg_color: str) -> Image.Image:
        """生成 Kenney 风格的角色立绘"""
        size = (400, 600)
        img = Image.new('RGB', size)
        draw = ImageDraw.Draw(img)
        
        r, g, b = int(bg_color[1:3], 16), int(bg_color[3:5], 16), int(bg_color[5:7], 16)
        
        # 渐变背景 - Kenney 平滑渐变
        for y in range(size[1]):
            ratio = y / size[1]
            cr = int(r * (0.82 + 0.18 * ratio))
            cg = int(g * (0.82 + 0.18 * ratio))  
            cb = int(b * (0.82 + 0.18 * ratio))
            draw.line([(0, y), (size[0], y)], fill=(cr, cg, cb))
        
        # 添加细微网格纹理 (Kenney 常用)
        for gx in range(0, size[0], 40):
            alpha = 8 if (gx // 40) % 2 == 0 else 4
            draw.line([(gx, 0), (gx, size[1])], fill=(255, 255, 255, alpha))
        for gy in range(0, size[1], 40):
            alpha = 8 if (gy // 40) % 2 == 0 else 4
            draw.line([(0, gy), (size[0], gy)], fill=(255, 255, 255, alpha))
        
        cx, cy = size[0] // 2, size[1] // 2 + 20
        
        # 角色配色方案 (Kenney 明亮配色)
        class_styles = {
            "ironclad": {
                "main": (220, 70, 70),     # 红色战士
                "secondary": (180, 55, 55),
                "accent": (240, 200, 60),   # 金色装饰
                "armor": (150, 130, 110),
            },
            "silent": {
                "main": (70, 180, 70),      # 绿色猎手
                "secondary": (50, 150, 50),
                "accent": (200, 230, 200),
                "armor": (60, 90, 60),
            },
            "defect": {
                "main": (100, 130, 220),   # 蓝色机器人
                "secondary": (80, 110, 200),
                "accent": (180, 200, 255),
                "armor": (70, 80, 130),
            },
            "watcher": {
                "main": (180, 100, 210),   # 紫色观星者
                "secondary": (160, 80, 190),
                "accent": (255, 200, 255),
                "armor": (130, 80, 150),
            },
            "necromancer": {
                "main": (130, 70, 160),    # 暗紫死灵法师
                "secondary": (100, 50, 140),
                "accent": (200, 150, 230),
                "armor": (70, 50, 90),
            },
        }
        
        style = class_styles.get(char_class, class_styles["ironclad"])
        main_c = style["main"]
        sec_c = style["secondary"]
        acc_c = style["accent"]
        armor_c = style["armor"]
        
        # 身体轮廓 - Kenney 简洁几何风格
        body_w = 90
        
        # 腿部
        leg_w = 28
        draw.rounded_rectangle(
            [cx-body_w//2-leg_w+5, cy+80, cx-body_w//2+leg_w-5, cy+165],
            radius=8, fill=sec_c
        )
        draw.rounded_rectangle(
            [cx-body_w//2-leg_w-5, cy+80, cx-body_w//2+leg_w+5, cy+165],
            radius=8, fill=sec_c
        )
        
        # 躯干/身体 - 圆角矩形
        draw.rounded_rectangle(
            [cx-body_w//2, cy-65, cx+body_w//2, cy+95],
            radius=20,
            fill=armor_c,
            outline=main_c,
            width=3
        )
        
        # 胸甲细节
        draw.rounded_rectangle(
            [cx-body_w//2+8, cy-55, cx-body_w//2+45, cy+10],
            radius=5, fill=main_c
        )
        draw.rounded_rectangle(
            [cx-body_w//2-8, cy-55, cx-45, cy+10],
            radius=5, fill=main_c
        )
        
        # 头部 - 圆形
        head_r = 48
        draw.ellipse(
            [cx-head_r, cy-125-head_r, cx+head_r, cy-125+head_r],
            fill=main_c,
            outline=sec_c,
            width=3
        )
        
        # 面罩/面部特征
        if char_class == "ironclad":
            # 头盔面罩
            draw.arc([cx-35, cy-115, cx+35, cy-75], 0, 180, fill=sec_c, width=4)
            # 头盔装饰
            draw.rectangle([cx-20, cy-118, cx+20, cy-112], fill=acc_c)
            # 武器 - 大剑在右侧
            sword_points = [
                (cx+65, cy-50), (cx+95, cy-20), (cx+85, cy+40),
                (cx+70, cy+80), (cx+55, cy+40), (cx+60, cy-20)
            ]
            draw.polygon(sword_points, fill=(190, 190, 200), outline=(100, 100, 110), width=2)
            
        elif char_class == "silent":
            # 兜帽阴影
            draw.polygon([(cx, cy-175), (cx-42, cy-95), (cx+42, cy-95)], 
                       fill=(40, 50, 40))
            # 匕首
            dagger = [(cx-55, cy-30), (cx-35, cy-10), (cx-55, cy+10)]
            draw.polygon(dagger, fill=(180, 200, 180), outline=(80, 120, 80), width=2)
            
        elif char_class == "defect":
            # 机械头部特征
            draw.rectangle([cx-25, cy-108, cx+25, cy-88], fill=acc_c)  # 显示屏
            draw.ellipse([cx-15, cy-122, cx-5, cy-112], fill=(255, 100, 100))  # 左眼
            draw.ellipse([cx+5, cy-122, cx+15, cy-112], fill=(255, 100, 100))   # 右眼
            # 天线
            draw.line([(cx, cy-173), (cx, cy-148)], fill=sec_c, width=3)
            draw.ellipse([cx-4, cy-176, cx+4, cy-170], fill=sec_c)
            
        elif char_class == "watcher":
            # 第三眼/宗教符号
            draw.ellipse([cx-8, cy-138, cx+8, cy-124], fill=acc_c)
            # 发光
            draw.ellipse([cx-15, cy-145, cx+15, cy-117], outline=acc_c, width=2)
            
        elif char_class == "necromancer":
            # 兜帽
            draw.polygon([(cx, cy-178), (cx-38, cy-98), (cx+38, cy-98)], 
                       fill=(40, 30, 50))
            # 骷髅装饰
            draw.ellipse([cx-12, cy-105, cx+12, cy-93], fill=(240, 230, 210))
            # 暗影光环
            draw.ellipse([cx-50, cy-135, cx+50, cy-75], outline=(130, 70, 160, 100), width=2)
        
        # 名字标签 - Kenney 字体风格
        try:
            font = ImageFont.truetype("/System/Library/Fonts/Helvetica.ttc", 36)
            names = {
                "ironclad": "IRONCLAD",
                "silent": "SILENT", 
                "defect": "DEFECT",
                "watcher": "WATCHER",
                "necromancer": "NECROMANCER"
            }
            display_name = names.get(char_id.upper(), char_id.upper())
            bbox = draw.textbbox((0, 0), display_name, font=font)
            tw = bbox[2] - bbox[0]
            draw.text(((size[0] - tw) // 2, size[1] - 55), display_name,
                    fill=(255, 255, 255, 230), font=font)
        except Exception:
            pass
        
        # 边框
        draw.rectangle([8, 8, size[0]-8, size[1]-8], outline=(*main_c, 150), width=3)
        
        return img
    
    # ==================== 敌人图标 (Kenney 风格) ====================
    
    def generate_enemy_kenney(self, enemy_id: str, enemy_type: str) -> Image.Image:
        """生成 Kenney 风格的敌人图标"""
        size = (128, 128)
        img = Image.new('RGBA', size, (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        
        cx, cy = 64, 64
        
        type_colors = {
            "Normal": ((200, 80, 80), (180, 70, 70)),
            "Elite": ((220, 160, 50), (200, 140, 40)),
            "Boss": ((160, 60, 180), (140, 40, 160)),
        }
        
        main_color, dark_color = type_colors.get(enemy_type, type_colors["Normal"])
        
        # 背景
        draw.ellipse([4, 4, size[0]-4, size[1]-4], fill=(30, 32, 40, 240), outline=main_color, width=3)
        
        enemy_id_lower = enemy_id.lower()
        
        if "cultist" in enemy_id_lower:
            # 邪教徒 - Kenney 风格
            # 兜帽
            draw.polygon([(cx, 18), (cx-34, 52), (cx+34, 52)], fill=(35, 35, 45))
            # 脸
            draw.ellipse([cx-22, 52, cx+22, 86], fill=main_color)
            # 眼睛 (发光)
            draw.ellipse([cx-10, 64, cx-2, 74], fill=(255, 220, 100))
            draw.ellipse([cx+2, 64, cx+10, 74], fill=(255, 220, 100))
            
        elif "jaw" in enemy_id_lower or "worm" in enemy_id_lower:
            # 颚虫 - 分节虫子
            segments = 4
            seg_h = 18
            for i in range(segments):
                offset_y = 30 + i * (seg_h - 4)
                w = 26 - abs(i - segments//2) * 4
                draw.ellipse([cx-w, offset_y, cx+w, offset_y+seg_h-4],
                           fill=main_color, outline=dark_color, width=1)
            # 眼睛
            draw.ellipse([cx-8, 36, cx, 48], fill=(255, 200, 150))
            draw.ellipse([cx, 36, cx+8, 48], fill=(255, 200, 150))
            
        elif "lagavulin" in enemy_id_lower:
            # 拉加夫林 - 大型怪物
            draw.ellipse([cx-44, cy-30, cx+44, cy+38], fill=main_color, outline=dark_color, width=3)
            # 角
            draw.polygon([(cx-28, cy-34), (cx-38, cy-54), (cx-20, cy-38)], fill=dark_color)
            draw.polygon([(cx+28, cy-34), (cx+38, cy-54), (cx+20, cy-38)], fill=dark_color)
            # 眼睛
            draw.ellipse([cx-16, cy-18, cx-4, cy-6], fill=(255, 200, 50))
            draw.ellipse([cx+4, cy-18, cx+16, cy-6], fill=(255, 200, 50))
            
        elif "guardian" in enemy_id_lower or "boss" in enemy_id_lower:
            # 守护者/Boss - 机械风格
            draw.rounded_rectangle([cx-46, cy-38, cx+46, cy+42], radius=8,
                               fill=main_color, outline=dark_color, width=3)
            # 头部
            draw.rounded_rectangle([cx-28, cy-50, cx+28, cy-32], radius=4,
                               fill=dark_color)
            # 眼睛 (发光LED)
            draw.ellipse([cx-14, cy-44, cx-4, cy-36], fill=(255, 200, 50))
            draw.ellipse([cx+4, cy-44, cx+14, cy-36], fill=(255, 200, 50))
            # 装饰线条
            draw.line([(cx-38, cy-20), (cx+38, cy-20)], fill=dark_color, width=2)
            draw.line([(cx-38, cy+20), (cx+38, cy+20)], fill=dark_color, width=2)
            
        else:
            # 默认怪物
            draw.ellipse([cx-32, cy-28, cx+32, cy+34], fill=main_color, outline=dark_color, width=2)
            # 眼睛
            draw.ellipse([cx-10, cy-18, cx-2, cy-10], fill=(255, 255, 200))
            draw.ellipse([cx+2, cy-18, cx+10, cy-10], fill=(255, 255, 200))
        
        return img
    
    # ==================== 遗物图标 (Kenney 风格) ====================
    
    def generate_relic_kenney(self, relic_name: str, relic_type: str) -> Image.Image:
        """生成 Kenney 风格的遗物图标"""
        size = (64, 64)
        img = Image.new('RGBA', size, (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        
        cx, cy = 32, 32
        
        type_colors = {
            "attack": (220, 70, 70),
            "defense": (70, 140, 220),
            "utility": (220, 190, 60),
            "passive": (160, 100, 200),
            "special": (100, 200, 160),
        }
        
        main_color = type_colors.get(relic_type, (140, 140, 140))
        dark_color = self._darken(main_color)
        light_color = self._lighten(main_color)
        
        name_lower = relic_name.lower()
        
        if "blood" in name_lower or "burning" in name_lower or "fire" in name_lower:
            # 血液/火焰类 - 水滴/火焰形状
            draw.ellipse([cx-24, cy-20, cx+24, cy+24], fill=main_color, outline=dark_color, width=2)
            draw.polygon([(cx, cy-30), (cx-8, cy-18), (cx+8, cy-18)], fill=(255, 120, 80))
            # 高光
            draw.ellipse([cx-8, cy-12, cx-2, cy-6], fill=(255, 255, 200, 180))
            
        elif "anchor" in name_lower:
            # 锚
            draw.ellipse([cx-22, cy-24, cx+22, cy+24], outline=main_color, width=3)
            draw.line([(cx, cy-30), (cx, cy+28)], fill=main_color, width=3)
            draw.arc([cx-14, cy+18, cx+14, cy+32], 0, 180, fill=main_color, width=2)
            
        elif "lantern" in name_lower or "torch" in name_lower:
            # 灯笼
            draw.rounded_rectangle([cx-14, cy-20, cx+14, cy+18], radius=4, fill=main_color)
            draw.polygon([(cx, cy-28), (cx-8, cy-20), (cx+8, cy-20)], fill=(255, 220, 100))
            # 光晕
            draw.ellipse([cx-20, cy-20, cx+20, cy+20], outline=(255, 220, 100, 80), width=2)
            
        elif "ice" in name_lower or "cream" in name_lower or "frozen" in name_lower:
            # 冰激凌/冰
            draw.rounded_rectangle([cx-12, cy-8, cx+12, cy+24], radius=3, fill=(180, 220, 255, 220))
            draw.polygon([(cx, cy-28), (cx-14, cy-8), (cx+14, cy-8)], fill=(240, 248, 255))
            
        elif "ring" in name_lower or "bandages" in name_lower:
            # 戒指/绷带
            draw.ellipse([cx-20, cy-20, cx+20, cy+20], outline=main_color, width=3)
            draw.ellipse([cx-14, cy-14, cx+14, cy+14], outline=main_color, width=2)
            draw.ellipse([cx-8, cy-8, cx+8, cy+8], outline=main_color, width=2)
            
        elif "bottle" in name_lower or "potion" in name_lower:
            # 药水瓶
            draw.rounded_rectangle([cx-10, cy-24, cx+10, cy+20], radius=5, fill=main_color)
            draw.rectangle([cx-14, cy-18, cx+14, cy-14], fill=light_color, width=2)
            # 液体
            draw.ellipse([cx-8, cy-10, cx+8, cy+14], fill=(100, 200, 255, 180))
            
        elif "map" in name_lower or "scroll" in name_lower:
            # 地图/卷轴
            draw.rounded_rectangle([cx-18, cy-22, cx+18, cy+22], radius=2, fill=(240, 230, 200), outline=main_color, width=2)
            draw.line([(cx-14, cy-16), (cx+14, cy-16)], fill=main_color, width=1)
            draw.line([(cx-14, cy-10), (cx+14, cy-10)], fill=main_color, width=1)
            draw.line([(cx-14, cy-4), (cx+14, cy-4)], fill=main_color, width=1)
            draw.line([(cx-14, cy+2), (cx+14, cy+2)], fill=main_color, width=1)
            
        else:
            # 默认宝石形状 - Kenney 多边形风格
            points = [
                (cx, cy-26),       # 顶
                (cx+20, cy-12),   # 右上
                (cx+16, cy+14),   # 右下
                (cx, cy+26),      # 底
                (cx-16, cy+14),   # 左下
                (cx-20, cy-12),   # 左上
            ]
            draw.polygon(points, fill=main_color, outline=dark_color, width=2)
            # 高光
            draw.polygon([(cx-6, cy-16), (cx-2, cy-22), (cx+2, cy-16)], fill=(255, 255, 255, 160))
        
        return img
    
    # ==================== 技能图标 (Kenney 风格) ====================
    
    def generate_skill_kenney(self, skill_name: str, skill_type: str) -> Image.Image:
        """生成 Kenney 风格的技能图标"""
        size = (64, 64)
        img = Image.new('RGBA', size, (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        
        cx, cy = 32, 32
        
        type_colors = {
            "fire": (255, 120, 50),
            "ice": (100, 180, 255),
            "heal": (100, 255, 100),
            "lightning": (255, 255, 100),
            "dash": (150, 150, 220),
            "default": (200, 100, 255),
        }
        
        main_color = type_colors.get(skill_type, type_colors["default"])
        
        skill_lower = skill_name.lower()
        
        if "fireball" in skill_lower or "fire" in skill_lower:
            # 火球 - Kenney 风格
            for i in range(3, 0, -1):
                r = 8 + i * 10
                alpha = 200 - i * 50
                draw.ellipse([cx-r, cy-r, cx+r, cy+r], fill=(*main_color, alpha))
            # 核心
            draw.ellipse([cx-6, cy-6, cx+6, cy+6], fill=(255, 255, 200))
            
        elif "heal" in skill_lower or "regenerate" in skill_lower or "skin" in skill_lower:
            # 治疗/再生 - 十字
            thickness = 5
            draw.line([(cx-22, cy), (cx+22, cy)], fill=(100, 255, 100), width=thickness)
            draw.line([(cx, cy-22), (cx, cy+22)], fill=(100, 255, 100), width=thickness)
            # 外圈
            draw.ellipse([cx-24, cy-24, cx+24, cy+24], outline=(100, 255, 100, 150), width=2)
            
        elif "dash" in skill_lower or "rush" in skill_lower:
            # 冲刺 - 速度线
            for offset in [-16, -8, 0, 8, 16]:
                length = 20 - abs(offset)
                draw.line([(cx-offset-length, cy-offset), (cx-offset+length, cy+offset)],
                        fill=(*main_color, 200), width=3)
                        
        elif "block" in skill_lower or "shield" in skill_lower or "defend" in skill_lower:
            # 格挡 - 盾牌
            points = [(cx, cy-22), (cx+18, cy-8), (cx+18, cy+10),
                    (cx, cy+24), (cx-18, cy+10), (cx-18, cy-8)]
            draw.polygon(points, fill=main_color, outline=self._darken(main_color), width=2)
            
        else:
            # 默认能量环
            for i in range(3):
                r = 10 + i * 8
                draw.ellipse([cx-r, cy-r, cx+r, cy+r], outline=(*main_color, 200-i*50), width=2)
            # 中心点
            draw.ellipse([cx-4, cy-4, cx+4, cy+4], fill=main_color)
        
        return img
    
    # ==================== 工具方法 ====================
    
    def _darken(self, color: tuple) -> tuple:
        return tuple(max(0, c - 50) for c in color[:3])
    
    def _lighten(self, color: tuple) -> tuple:
        return tuple(min(255, c + 60) for c in color[:3])
    
    # ==================== 主流程 ====================
    
    def process_all_configs(self):
        """处理所有配置文件并生成资源"""
        print("""
╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║   🎨 Kenney 风格资源替换器 v8.0                          ║
║                                                           ║
║   用 Kenney 设计风格重新生成所有游戏资源                  ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝
""")
        
        generated_count = 0
        
        # 1. 处理卡牌配置
        print("\n📋 第1步: 生成卡牌图标...")
        cards_config = self._load_json("cards")
        if cards_config:
            for card in cards_config.get("cards", []):
                icon_path = DIRS["cards"] / f"{card['id']}.png"
                
                # 也检查基础名称
                base_name = card.get("name", "").lower().replace(" ", "_")
                alt_path = DIRS["cards"] / f"{base_name}.png"
                
                target = icon_path if icon_path.exists() else alt_path
                
                img = self.generate_card_kenney(card["name"], card.get("type", "Attack"), card.get("color", "#FF6644"))
                img.save(target)
                generated_count += 1
            
            print(f"   ✅ 生成 {len(cards_config['cards'])} 张卡牌图标")
        
        # 2. 处理角色配置
        print("\n👤 第2步: 生成角色立绘...")
        chars_config = self._load_json("characters")
        if chars_config:
            for char in chars_config.get("characters", []):
                portrait_path = DIRS["characters"] / f"{char['id']}.png"
                img = self.generate_character_kenney(char["id"], char.get("class", "warrior"), 
                                                   char.get("backgroundColor", "#FF6644"))
                img.save(portrait_path)
                generated_count += 1
            
            print(f"   ✅ 生成 {len(chars_config['characters'])} 张角色立绘")
        
        # 3. 处理敌人配置
        print("\n👾 第3步: 生成敌人图标...")
        enemies_config = self._load_json("enemies")
        if enemies_config:
            for enemy in enemies_config.get("enemies", []):
                # 图标
                icon_path = DIRS["enemies_icon"] / f"{enemy['id']}.png"
                img = self.generate_enemy_kenney(enemy["id"], enemy.get("type", "Normal"))
                img.save(icon_path)
                generated_count += 1
                
                # 全身图
                full_path = DIRS["enemies_full"] / f"{enemy['id']}.png"
                big_img = self.generate_enemy_kenney(enemy["id"], enemy.get("type", "Normal"))
                big_img = big_img.resize((256, 256), Image.LANCZOS)
                big_img.save(full_path)
                generated_count += 1
            
            print(f"   ✅ 生成 {len(enemies_config['enemies'])} 组敌人图像")
        
        # 4. 处理遗物配置
        print("\n💎 第4步: 生成遗物图标...")
        relics_config = self._load_json("relics")
        if relics_config:
            for relic in relics_config.get("relics", []):
                icon_path = DIRS["relics"] / f"{relic['id']}.png"
                img = self.generate_relic_kenney(relic["name"], relic.get("type", "passive"))
                img.save(icon_path)
                generated_count += 1
            
            print(f"   ✅ 生成 {len(relics_config['relics'])} 个遗物图标")
        
        # 5. 生成技能图标
        print("\n⚡ 第5步: 生成技能图标...")
        skills = ["fireball", "heal", "dash", "iron_skin"]
        for skill in skills:
            skill_path = DIRS["skills"] / f"{skill}.png"
            img = self.generate_skill_kenney(skill, skill.split("_")[0])
            img.save(skill_path)
            generated_count += 1
        print(f"   ✅ 生成 {len(skills)} 个技能图标")
        
        # 6. 清理 .import 缓存
        print("\n🧹 第6步: 清理 .import 缓存...")
        cache_count = 0
        for dir_path in DIRS.values():
            for f in list(dir_path.glob("*.import")):
                f.unlink()
                cache_count += 1
        print(f"   ✅ 清理了 {cache_count} 个缓存文件")
        
        # 总结
        self.stats["generated"] = generated_count
        self._print_summary(generated_count)
    
    def _load_json(self, name: str):
        path = PROJECT_ROOT / "Config" / "Data" / f"{name}.json"
        if path.exists():
            with open(path, 'r', encoding='utf-8') as f:
                return json.load(f)
        return None
    
    def _print_summary(self, count: int):
        print(f"\n{'='*70}")
        print(f"✅ Kenney 风格资源替换完成!")
        print(f"{'='*70}\n")
        
        print(f"📊 总计生成/替换: {count} 个资源\n")
        
        print(f"💡 重要提示:")
        print(f"   1. 必须重启 Godot 编辑器让更改生效")
        print(f"   2. 或者在编辑器中点击 Project → ReImport All")
        print(f"   3. 运行游戏查看新的 Kenney 风格资源")
        print(f"\n🎨 新资源特点:")
        print(f"   • 扁平化设计 (Kenney 标志性风格)")
        print(f"   • 圆角矩形几何图形")
        print(f"   • 明亮配色方案")
        print(f"   • 清晰的视觉层次")


def main():
    generator = KenneyStyleGenerator()
    generator.process_all_configs()


if __name__ == "__main__":
    main()
