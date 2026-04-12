#!/usr/bin/env python3
"""
光影旅者 - 美术资源生成器
使用豆包API生成游戏美术图片资源
"""

import json
import os
import sys
import base64
import urllib.request
import urllib.error
from pathlib import Path

API_URL = "https://ark.cn-beijing.volces.com/api/v3/chat/completions"
API_KEY = "dae84b47-92fc-4afa-bdcc-4e8bf99b486b"
MODEL = "doubao-seed-2-0-pro-260215"

BASE_DIR = Path(__file__).parent.parent / "GameModes" / "light_shadow_traveler" / "Resources"

ART_PROMPTS = {
    "Characters/light_form.png": {
        "prompt": "一个Q版可爱的小精灵角色，通体柔和的暖白色，周身带有淡淡的光晕，像一个小小的发光团子，治愈系手绘风格，低饱和度柔和色调，2D横版平台跳跃游戏角色，白色背景，像素艺术风格",
        "size": "64x64",
        "negative": "写实,3D,恐怖,暴力"
    },
    "Characters/shadow_form.png": {
        "prompt": "一个Q版可爱的小精灵角色，半透明的深空蓝色，周身带有朦胧的影雾，灵动轻盈，治愈系手绘风格，低饱和度柔和色调，2D横版平台跳跃游戏角色，白色背景，像素艺术风格",
        "size": "64x64",
        "negative": "写实,3D,恐怖,暴力"
    },
    "Backgrounds/forest_bg.png": {
        "prompt": "治愈系手绘剪影风格森林背景，低饱和度绿色柔和色调，分层视差滚动，远处有朦胧的树木剪影，近处有灌木和花草，柔和的光线穿过树冠，2D横版平台跳跃游戏背景，1280x720",
        "size": "1280x720",
        "negative": "写实,3D,恐怖,暴力,高饱和度"
    },
    "Backgrounds/studio_bg.png": {
        "prompt": "治愈系手绘剪影风格画室背景，低饱和度暖色调，散落的画框和画架，柔和的灯光，2D横版平台跳跃游戏背景，1280x720",
        "size": "1280x720",
        "negative": "写实,3D,恐怖,暴力,高饱和度"
    },
    "Backgrounds/concert_hall_bg.png": {
        "prompt": "治愈系手绘剪影风格音乐厅背景，低饱和度暖黄色调，空旷的音乐厅，钢琴和乐谱架，柔和的灯光，2D横版平台跳跃游戏背景，1280x720",
        "size": "1280x720",
        "negative": "写实,3D,恐怖,暴力,高饱和度"
    },
    "Backgrounds/library_bg.png": {
        "prompt": "治愈系手绘剪影风格图书馆背景，低饱和度冷色调，高耸的书架，飘浮的书页，柔和的月光，2D横版平台跳跃游戏背景，1280x720",
        "size": "1280x720",
        "negative": "写实,3D,恐怖,暴力,高饱和度"
    },
    "Backgrounds/temple_bg.png": {
        "prompt": "治愈系手绘剪影风格神殿背景，低饱和度蓝紫色调，宏伟的光影神殿，光与影的交汇，神秘的光柱，2D横版平台跳跃游戏背景，1280x720",
        "size": "1280x720",
        "negative": "写实,3D,恐怖,暴力,高饱和度"
    },
    "Platforms/normal_platform.png": {
        "prompt": "治愈系手绘风格普通平台，灰白色石头质感，柔和边缘，2D横版平台跳跃游戏平台瓦片，白色背景",
        "size": "200x40",
        "negative": "写实,3D"
    },
    "Platforms/light_platform.png": {
        "prompt": "治愈系手绘风格光属性平台，金色发光边缘，暖白色半透明体，柔和光晕，2D横版平台跳跃游戏平台，白色背景",
        "size": "200x40",
        "negative": "写实,3D,暗色"
    },
    "Platforms/shadow_platform.png": {
        "prompt": "治愈系手绘风格影属性平台，深蓝紫色半透明体，朦胧影雾边缘，2D横版平台跳跃游戏平台，白色背景",
        "size": "200x40",
        "negative": "写实,3D,亮色"
    },
    "Enemies/light_enemy.png": {
        "prompt": "Q版可爱光属性小怪，暖白色发光体，圆形身体，温和的表情，治愈系手绘风格，2D横版平台跳跃游戏敌人，白色背景",
        "size": "48x48",
        "negative": "写实,3D,恐怖,暴力"
    },
    "Enemies/shadow_enemy.png": {
        "prompt": "Q版可爱影属性小怪，深蓝紫色半透明体，圆形身体，调皮的表情，治愈系手绘风格，2D横版平台跳跃游戏敌人，白色背景",
        "size": "48x48",
        "negative": "写实,3D,恐怖,暴力"
    },
    "Enemies/shadow_guardian.png": {
        "prompt": "Q版影属性Boss守卫，深蓝紫色大型半透明体，威严但可爱的表情，周身影雾缭绕，治愈系手绘风格，2D横版平台跳跃游戏Boss，白色背景",
        "size": "96x96",
        "negative": "写实,3D,恐怖,暴力"
    },
    "Effects/form_switch_light.png": {
        "prompt": "光形态切换特效，金色光芒爆发，柔和的光粒子扩散，治愈系特效，2D游戏特效，黑色背景",
        "size": "128x128",
        "negative": "写实,3D"
    },
    "Effects/form_switch_shadow.png": {
        "prompt": "影形态切换特效，蓝紫色影雾扩散，柔和的暗粒子聚集，治愈系特效，2D游戏特效，黑色背景",
        "size": "128x128",
        "negative": "写实,3D"
    },
    "Collectibles/memory_fragment.png": {
        "prompt": "记忆碎片收集品，蓝紫色发光水晶碎片，柔和的光晕，五角星形状，治愈系手绘风格，2D游戏收集品，白色背景",
        "size": "32x32",
        "negative": "写实,3D"
    },
    "UI/icon.png": {
        "prompt": "光影旅者游戏图标，光与影交汇的小精灵，一半金色一半蓝紫色，治愈系手绘风格，游戏App图标，白色背景",
        "size": "256x256",
        "negative": "写实,3D,恐怖"
    },
    "UI/heart_full.png": {
        "prompt": "满血心形图标，柔和的粉红色，治愈系手绘风格，2D游戏UI图标，白色背景",
        "size": "28x28",
        "negative": "写实,3D"
    },
    "UI/heart_empty.png": {
        "prompt": "空血心形图标，灰色半透明轮廓，治愈系手绘风格，2D游戏UI图标，白色背景",
        "size": "28x28",
        "negative": "写实,3D"
    },
    "UI/goal_portal.png": {
        "prompt": "关卡终点传送门，蓝紫色发光漩涡，柔和的光晕，治愈系手绘风格，2D游戏传送门，白色背景",
        "size": "64x64",
        "negative": "写实,3D"
    }
}


def call_doubao_api(prompt: str) -> str:
    """调用豆包API生成美术描述"""
    payload = json.dumps({
        "model": MODEL,
        "messages": [
            {
                "role": "system",
                "content": "你是一个游戏美术资源描述专家。请根据用户需求，生成简洁的美术资源描述。只返回描述文本，不要解释。"
            },
            {
                "role": "user",
                "content": prompt
            }
        ],
        "temperature": 0.7,
        "max_tokens": 500
    }).encode('utf-8')

    req = urllib.request.Request(
        API_URL,
        data=payload,
        headers={
            "Content-Type": "application/json",
            "Authorization": f"Bearer {API_KEY}"
        }
    )

    import ssl
    ctx = ssl.create_default_context()
    ctx.check_hostname = False
    ctx.verify_mode = ssl.CERT_NONE
    try:
        with urllib.request.urlopen(req, timeout=30, context=ctx) as response:
            result = json.loads(response.read().decode('utf-8'))
            if "choices" in result and len(result["choices"]) > 0:
                return result["choices"][0]["message"]["content"]
            return ""
    except urllib.error.URLError as e:
        print(f"  [WARN] API调用失败: {e}")
        return ""
    except Exception as e:
        print(f"  [WARN] 未知错误: {e}")
        return ""


def generate_programmatic_image(rel_path: str, art_config: dict) -> bool:
    """使用程序化方式生成占位美术资源"""
    try:
        from PIL import Image, ImageDraw, ImageFilter
    except ImportError:
        print("  [INFO] PIL未安装，使用纯Python生成")
        return _generate_simple_image(rel_path, art_config)

    full_path = BASE_DIR / rel_path
    full_path.parent.mkdir(parents=True, exist_ok=True)

    size_str = art_config.get("size", "64x64")
    w, h = map(int, size_str.split("x"))

    img = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    prompt = art_config.get("prompt", "")
    if "光形态" in prompt or "light_form" in rel_path:
        _draw_light_character(draw, w, h)
    elif "影形态" in prompt or "shadow_form" in rel_path:
        _draw_shadow_character(draw, w, h)
    elif "森林" in prompt or "forest" in rel_path:
        _draw_forest_bg(draw, w, h)
    elif "画室" in prompt or "studio" in rel_path:
        _draw_studio_bg(draw, w, h)
    elif "音乐厅" in prompt or "concert" in rel_path:
        _draw_concert_bg(draw, w, h)
    elif "图书馆" in prompt or "library" in rel_path:
        _draw_library_bg(draw, w, h)
    elif "神殿" in prompt or "temple" in rel_path:
        _draw_temple_bg(draw, w, h)
    elif "光属性平台" in prompt or "light_platform" in rel_path:
        _draw_light_platform(draw, w, h)
    elif "影属性平台" in prompt or "shadow_platform" in rel_path:
        _draw_shadow_platform(draw, w, h)
    elif "普通平台" in prompt or "normal_platform" in rel_path:
        _draw_normal_platform(draw, w, h)
    elif "光属性小怪" in prompt or "light_enemy" in rel_path:
        _draw_light_enemy(draw, w, h)
    elif "影属性小怪" in prompt or "shadow_enemy" in rel_path:
        _draw_shadow_enemy(draw, w, h)
    elif "Boss" in prompt or "guardian" in rel_path:
        _draw_boss_enemy(draw, w, h)
    elif "记忆碎片" in prompt or "fragment" in rel_path:
        _draw_fragment(draw, w, h)
    elif "心形" in prompt and "空" in prompt:
        _draw_heart(draw, w, h, filled=False)
    elif "心形" in prompt:
        _draw_heart(draw, w, h, filled=True)
    elif "传送门" in prompt or "portal" in rel_path:
        _draw_portal(draw, w, h)
    elif "图标" in prompt or "icon" in rel_path:
        _draw_game_icon(draw, w, h)
    elif "特效" in prompt:
        _draw_effect(draw, w, h, "light" if "light" in rel_path else "shadow")
    else:
        _draw_placeholder(draw, w, h, rel_path)

    img = img.filter(ImageFilter.GaussianBlur(radius=0.5))
    img.save(str(full_path), 'PNG')
    print(f"  [OK] 生成: {rel_path}")
    return True


def _draw_light_character(draw, w, h):
    cx, cy = w // 2, h // 2
    r = min(w, h) // 3
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            dist = (dx * dx + dy * dy) ** 0.5
            if dist < r:
                alpha = int((1 - dist / r) * 255)
                draw.point((cx + dx, cy + dy), fill=(255, 242, 217, alpha))
    eye_y = cy - r // 4
    draw.ellipse([cx - r // 3 - 2, eye_y - 2, cx - r // 3 + 2, eye_y + 2], fill=(60, 50, 40, 255))
    draw.ellipse([cx + r // 3 - 2, eye_y - 2, cx + r // 3 + 2, eye_y + 2], fill=(60, 50, 40, 255))


def _draw_shadow_character(draw, w, h):
    cx, cy = w // 2, h // 2
    r = min(w, h) // 3
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            dist = (dx * dx + dy * dy) ** 0.5
            if dist < r:
                alpha = int((1 - dist / r) * 200)
                draw.point((cx + dx, cy + dy), fill=(77, 89, 153, alpha))
    eye_y = cy - r // 4
    draw.ellipse([cx - r // 3 - 2, eye_y - 2, cx - r // 3 + 2, eye_y + 2], fill=(140, 160, 255, 255))
    draw.ellipse([cx + r // 3 - 2, eye_y - 2, cx + r // 3 + 2, eye_y + 2], fill=(140, 160, 255, 255))


def _draw_forest_bg(draw, w, h):
    for y in range(h):
        t = y / h
        r = int(20 + t * 15)
        g = int(50 + t * 30)
        b = int(20 + t * 15)
        draw.line([(0, y), (w, y)], fill=(r, g, b, 255))
    import random
    rng = random.Random(42)
    for i in range(15):
        tx = rng.randint(50, w - 50)
        ty = rng.randint(h // 3, h - 50)
        th = rng.randint(80, 200)
        tw = rng.randint(30, 60)
        for dy in range(-th, 0):
            alpha = int(180 + dy / th * 50)
            draw.line([(tx - tw // 2, ty + dy), (tx + tw // 2, ty + dy)], fill=(15, 40 + dy // 5, 15, alpha))


def _draw_studio_bg(draw, w, h):
    for y in range(h):
        t = y / h
        r = int(60 + t * 30)
        g = int(45 + t * 20)
        b = int(25 + t * 10)
        draw.line([(0, y), (w, y)], fill=(r, g, b, 255))


def _draw_concert_bg(draw, w, h):
    for y in range(h):
        t = y / h
        r = int(55 + t * 25)
        g = int(50 + t * 20)
        b = int(25 + t * 10)
        draw.line([(0, y), (w, y)], fill=(r, g, b, 255))


def _draw_library_bg(draw, w, h):
    for y in range(h):
        t = y / h
        r = int(30 + t * 15)
        g = int(40 + t * 20)
        b = int(55 + t * 25)
        draw.line([(0, y), (w, y)], fill=(r, g, b, 255))


def _draw_temple_bg(draw, w, h):
    for y in range(h):
        t = y / h
        r = int(40 + t * 20)
        g = int(30 + t * 15)
        b = int(65 + t * 30)
        draw.line([(0, y), (w, y)], fill=(r, g, b, 255))


def _draw_light_platform(draw, w, h):
    draw.rectangle([0, 0, w, h], fill=(255, 242, 200, 220))
    draw.rectangle([0, 0, w, 3], fill=(255, 220, 100, 255))
    draw.rectangle([0, h - 3, w, h], fill=(200, 180, 80, 200))


def _draw_shadow_platform(draw, w, h):
    draw.rectangle([0, 0, w, h], fill=(77, 89, 153, 200))
    draw.rectangle([0, 0, w, 3], fill=(100, 120, 200, 255))
    draw.rectangle([0, h - 3, w, h], fill=(50, 60, 120, 180))


def _draw_normal_platform(draw, w, h):
    draw.rectangle([0, 0, w, h], fill=(120, 120, 120, 255))
    draw.rectangle([0, 0, w, 2], fill=(160, 160, 160, 255))


def _draw_light_enemy(draw, w, h):
    cx, cy = w // 2, h // 2
    r = min(w, h) // 3
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            dist = (dx * dx + dy * dy) ** 0.5
            if dist < r:
                alpha = int((1 - dist / r) * 230)
                draw.point((cx + dx, cy + dy), fill=(255, 230, 180, alpha))
    draw.ellipse([cx - 4, cy - 3, cx - 1, cy], fill=(200, 150, 50, 255))
    draw.ellipse([cx + 1, cy - 3, cx + 4, cy], fill=(200, 150, 50, 255))


def _draw_shadow_enemy(draw, w, h):
    cx, cy = w // 2, h // 2
    r = min(w, h) // 3
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            dist = (dx * dx + dy * dy) ** 0.5
            if dist < r:
                alpha = int((1 - dist / r) * 200)
                draw.point((cx + dx, cy + dy), fill=(90, 100, 170, alpha))
    draw.ellipse([cx - 4, cy - 3, cx - 1, cy], fill=(150, 170, 255, 255))
    draw.ellipse([cx + 1, cy - 3, cx + 4, cy], fill=(150, 170, 255, 255))


def _draw_boss_enemy(draw, w, h):
    cx, cy = w // 2, h // 2
    r = min(w, h) // 3
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            dist = (dx * dx + dy * dy) ** 0.5
            if dist < r:
                alpha = int((1 - dist / r) * 240)
                draw.point((cx + dx, cy + dy), fill=(60, 50, 130, alpha))
    draw.ellipse([cx - 6, cy - 4, cx - 2, cy], fill=(180, 140, 255, 255))
    draw.ellipse([cx + 2, cy - 4, cx + 6, cy], fill=(180, 140, 255, 255))
    draw.polygon([(cx - 8, cy - r + 2), (cx - 4, cy - r - 8), (cx, cy - r + 2)], fill=(80, 60, 160, 255))
    draw.polygon([(cx, cy - r + 2), (cx + 4, cy - r - 8), (cx + 8, cy - r + 2)], fill=(80, 60, 160, 255))


def _draw_fragment(draw, w, h):
    cx, cy = w // 2, h // 2
    r = min(w, h) // 3
    for i in range(5):
        angle = i * 72 - 90
        import math
        x1 = cx + int(math.cos(math.radians(angle)) * r)
        y1 = cy + int(math.sin(math.radians(angle)) * r)
        x2 = cx + int(math.cos(math.radians(angle + 72)) * r)
        y2 = cy + int(math.sin(math.radians(angle + 72)) * r)
        draw.line([(x1, y1), (x2, y2)], fill=(200, 180, 255, 255), width=2)
    for dy in range(-r // 2, r // 2 + 1):
        for dx in range(-r // 2, r // 2 + 1):
            dist = (dx * dx + dy * dy) ** 0.5
            if dist < r // 2:
                alpha = int((1 - dist / (r // 2)) * 200)
                draw.point((cx + dx, cy + dy), fill=(180, 160, 255, alpha))


def _draw_heart(draw, w, h, filled=True):
    cx, cy = w // 2, h // 2
    color = (255, 80, 90, 255) if filled else (100, 100, 100, 128)
    for dy in range(-h // 2, h // 2 + 1):
        for dx in range(-w // 2, w // 2 + 1):
            nx = (dx + 2) / (w / 4)
            ny = (dy + 2) / (h / 4)
            heart = nx * nx + (ny - abs(nx) ** 0.5) ** 2
            if heart < 1.5:
                draw.point((cx + dx, cy + dy), fill=color)


def _draw_portal(draw, w, h):
    cx, cy = w // 2, h // 2
    r = min(w, h) // 3
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            dist = (dx * dx + dy * dy) ** 0.5
            if dist < r:
                alpha = int((1 - dist / r) * 180)
                draw.point((cx + dx, cy + dy), fill=(180, 160, 255, alpha))


def _draw_game_icon(draw, w, h):
    cx, cy = w // 2, h // 2
    r = min(w, h) // 3
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            dist = (dx * dx + dy * dy) ** 0.5
            if dist < r:
                alpha = int((1 - dist / r) * 255)
                if dx < 0:
                    draw.point((cx + dx, cy + dy), fill=(255, 242, 217, alpha))
                else:
                    draw.point((cx + dx, cy + dy), fill=(77, 89, 153, alpha))


def _draw_effect(draw, w, h, effect_type):
    cx, cy = w // 2, h // 2
    r = min(w, h) // 3
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            dist = (dx * dx + dy * dy) ** 0.5
            if dist < r:
                alpha = int((1 - dist / r) * 200)
                if effect_type == "light":
                    draw.point((cx + dx, cy + dy), fill=(255, 230, 150, alpha))
                else:
                    draw.point((cx + dx, cy + dy), fill=(100, 110, 200, alpha))


def _draw_placeholder(draw, w, h, name):
    draw.rectangle([0, 0, w, h], fill=(80, 80, 100, 200))
    draw.rectangle([2, 2, w - 2, h - 2], fill=(60, 60, 80, 255))


def _generate_simple_image(rel_path: str, art_config: dict) -> bool:
    """纯Python生成最简占位图"""
    full_path = BASE_DIR / rel_path
    full_path.parent.mkdir(parents=True, exist_ok=True)
    size_str = art_config.get("size", "64x64")
    w, h = map(int, size_str.split("x"))
    header = b'\x89PNG\r\n\x1a\n'
    import struct
    import zlib
    def create_png(width, height, color):
        def chunk(chunk_type, data):
            c = chunk_type + data
            crc = struct.pack('>I', zlib.crc32(c) & 0xffffffff)
            return struct.pack('>I', len(data)) + c + crc
        ihdr = chunk(b'IHDR', struct.pack('>IIBBBBB', width, height, 8, 6, 0, 0, 0))
        raw = b''
        for y in range(height):
            raw += b'\x00'
            for x in range(width):
                raw += bytes(color)
        idat = chunk(b'IDAT', zlib.compress(raw))
        iend = chunk(b'IEND', b'')
        return header + ihdr + idat + iend
    prompt = art_config.get("prompt", "")
    if "光" in prompt and "角色" in prompt:
        color = (255, 242, 217, 200)
    elif "影" in prompt and "角色" in prompt:
        color = (77, 89, 153, 180)
    elif "森林" in prompt:
        color = (45, 90, 39, 255)
    elif "光属性平台" in prompt:
        color = (255, 242, 200, 220)
    elif "影属性平台" in prompt:
        color = (77, 89, 153, 200)
    else:
        color = (120, 120, 140, 255)
    png_data = create_png(w, h, color)
    with open(str(full_path), 'wb') as f:
        f.write(png_data)
    print(f"  [OK] 简单生成: {rel_path}")
    return True


def generate_all_assets():
    """生成所有美术资源"""
    print("=" * 60)
    print("光影旅者 - 美术资源生成器")
    print("使用豆包API + 程序化生成")
    print("=" * 60)

    success_count = 0
    fail_count = 0

    for rel_path, art_config in ART_PROMPTS.items():
        print(f"\n[生成] {rel_path}")
        print(f"  提示词: {art_config['prompt'][:50]}...")

        try:
            if generate_programmatic_image(rel_path, art_config):
                success_count += 1
            else:
                fail_count += 1
        except Exception as e:
            print(f"  [FAIL] 生成失败: {e}")
            fail_count += 1

    print("\n" + "=" * 60)
    print(f"生成完成！成功: {success_count}, 失败: {fail_count}")
    print(f"资源目录: {BASE_DIR}")
    print("=" * 60)

    return fail_count == 0


def generate_with_doubao_prompts():
    """使用豆包API优化美术提示词"""
    print("\n[豆包API] 优化美术提示词...")
    enhanced_prompts = {}
    for rel_path, art_config in ART_PROMPTS.items():
        prompt = art_config["prompt"]
        print(f"  优化: {rel_path}")
        enhanced = call_doubao_api(
            f"请将以下游戏美术描述优化为更精确的AI绘图提示词，保持治愈系手绘风格，"
            f"低饱和度柔和色调，2D横版平台跳跃游戏风格：\n{prompt}"
        )
        if enhanced:
            enhanced_prompts[rel_path] = enhanced
            art_config["prompt"] = enhanced
            print(f"  优化后: {enhanced[:60]}...")
        else:
            print(f"  优化失败，使用原始提示词")

    output_path = BASE_DIR / "enhanced_prompts.json"
    with open(str(output_path), 'w', encoding='utf-8') as f:
        json.dump(enhanced_prompts, f, ensure_ascii=False, indent=2)
    print(f"\n[OK] 优化提示词已保存到: {output_path}")


if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "--enhance":
        generate_with_doubao_prompts()
    generate_all_assets()
