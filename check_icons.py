#!/usr/bin/env python3
import os, re, glob

base = 'Client/GameModes/base_game/Resources'

code_icons = set()
for ext in ['*.gd', '*.cs']:
    for fp in glob.glob(f'Client/**/{ext}', recursive=True):
        with open(fp, 'r', errors='ignore') as f:
            content = f.read()
        # 匹配第二个参数（图标名）: make_button("文本", "icon_name", ...) 或 get_icon("icon_name")
        # make_button(text, icon_name, size)
        matches1 = re.findall(
            r'make_button\s*\(\s*["\'][^"\']*["\']\s*,\s*["\']([\w_]+)["\']',
            content, re.IGNORECASE
        )
        # get_icon("icon_name")
        matches2 = re.findall(
            r'get_icon\s*\(\s*["\']([\w_]+)["\']',
            content, re.IGNORECASE
        )
        # make_icon_label("icon_name", ...)
        matches3 = re.findall(
            r'make_icon_label\s*\(\s*["\']([\w_]+)["\']',
            content, re.IGNORECASE
        )
        # MakeIconLabel("icon_name", ...)
        matches4 = re.findall(
            r'MakeIconLabel\s*\(\s*["\']([\w_]+)["\']',
            content
        )
        for m in matches1 + matches2 + matches3 + matches4:
            if m:  # 非空字符串
                code_icons.add(m)

available = set()
for root, dirs, files in os.walk(base):
    for f in files:
        if f.endswith('.png') and not f.endswith('.import'):
            available.add(f)

print('=== 代码中引用的图标 ===')
missing = []
for icon in sorted(code_icons):
    png_name = icon + '.png'
    if png_name in available:
        print(f'  OK {png_name}')
    else:
        missing.append(png_name)
        print(f'  MISSING {png_name} !!!')

print(f'\n=== 新生成但未被引用的图标 ===')
unused = []
skip_prefixes = ('btn_', 'panel_', 'bar_bg', 'settings_btn', 'Inventory_',
                 'slide_', 'button_', 'check_', 'arrow_', 'icon_outline', 'relic_',
                 'Preview', 'Sample', 'star_outline')
new_icons = [
    'icon_coin', 'icon_star', 'icon_heart', 'icon_sword', 'icon_shield', 'icon_skull',
    'icon_coin_white', 'icon_star_white', 'icon_heart_white', 'icon_sword_white', 'icon_shield_white', 'icon_skull_white',
    'icon_square', 'icon_triangle', 'icon_ring', 'icon_circle', 'icon_diamond', 'icon_dot',
    'icon_arrow_up', 'icon_arrow_down', 'icon_arrow_left', 'icon_arrow_right',
    'icon_check', 'icon_cross',
    'status_buff', 'status_debuff', 'status_poison', 'status_burn', 'status_freeze', 'status_stun',
    'fireball', 'heal', 'iron_skin', 'dash', 'strike', 'defend', 'poison', 'weak', 'vulnerable', 'strength', 'dexterity', 'rage',
    'health_potion_small', 'health_potion_large', 'iron_sword', 'steel_armor', 'gold_bag', 'key_item', 'scroll', 'gem',
    'jaw_worm', 'cultist', 'lagavulin', 'the_guardian', 'slime_blue', 'slime_red', 'bat', 'skeleton',
    'merchant',
    'achievement_combat', 'achievement_gold', 'achievement_speed', 'achievement_survive', 'achievement_collect', 'achievement_secret',
]
for icon_name in new_icons:
    png_name = icon_name + '.png'
    if png_name in available and icon_name not in code_icons:
        unused.append(png_name)
print(f'  共 {len(unused)} 个:')
for f in unused:
    print(f'  - {f}')

print(f'\n总计: 代码引用={len(code_icons)}, 可用PNG={len(available)}, 缺失={len(missing)}')
