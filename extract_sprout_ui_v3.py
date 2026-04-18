#!/usr/bin/env python3
"""
Correctly extract sprites from Sprout Lands UI Pack sprite sheets.
Uses proper region cutting from the original sheets.
"""
from PIL import Image
import os

SPROUT = "/Users/zhuyong/trae-game/Sprout Lands - UI Pack - Basic Pack"
OUT = "/Users/zhuyong/trae-game/Client/GameModes/base_game/Resources/UI"

os.makedirs(OUT, exist_ok=True)

def extract_region(img: Image.Image, x: int, y: int, w: int, h: int, name: str) -> None:
    region = img.crop((x, y, x + w, y + h))
    region.save(os.path.join(OUT, name))
    print(f"  Extracted {name}: {w}x{h}")

# === 1. Main sprite sheet - panels and bars ===
main_sheet = Image.open(os.path.join(SPROUT, "Sprite sheets", "Sprite sheet for Basic Pack.png"))
print("=== Main Sheet Panels ===")

# Large pre-made panel at Col 10 (x=148, y=2, w=490, h=172)
extract_region(main_sheet, 148, 2, 490, 172, "panel_light.png")

# Second panel variant at Col 10 (x=148, y=178, w=490, h=60) - this is a bar/strip
extract_region(main_sheet, 148, 178, 490, 60, "panel_dark.png")

# Panel 9-patch pieces - Panel style 1 (green/brown)
# Col 0-2: left edge(4px), center column(34px), right edge(4px)
# Rows within center column: y=11(h=28), y=59(h=28), y=107(h=28), y=155(h=28)
# These are: top-left+top+top-right, left+center+right, bottom-left+bottom+bottom-right, extra
# Assemble a complete panel from 9-patch pieces

# Panel style 1 (Cols 0-2, rows 0-2)
p1_tl = main_sheet.crop((2, 11, 2+4, 11+28))       # left edge top
p1_t  = main_sheet.crop((7, 11, 7+34, 11+28))       # center top
p1_tr = main_sheet.crop((42, 11, 42+4, 11+28))      # right edge top
p1_l  = main_sheet.crop((2, 59, 2+4, 59+28))        # left edge middle
p1_c  = main_sheet.crop((7, 59, 7+34, 59+28))       # center middle
p1_r  = main_sheet.crop((42, 59, 42+4, 59+28))      # right edge middle
p1_bl = main_sheet.crop((2, 107, 2+4, 107+28))      # left edge bottom
p1_b  = main_sheet.crop((7, 107, 7+34, 107+28))     # center bottom
p1_br = main_sheet.crop((42, 107, 42+4, 107+28))    # right edge bottom

# Assemble panel style 1 (tile center 3x3)
tile_w, tile_h = 3, 3
pw = p1_tl.width + p1_t.width * tile_w + p1_tr.width
ph = p1_tl.height + p1_c.height * tile_h + p1_bl.height
panel1 = Image.new("RGBA", (pw, ph), (0, 0, 0, 0))
panel1.paste(p1_tl, (0, 0))
for i in range(tile_w):
    panel1.paste(p1_t, (p1_tl.width + i * p1_t.width, 0))
panel1.paste(p1_tr, (pw - p1_tr.width, 0))
for j in range(tile_h):
    panel1.paste(p1_l, (0, p1_tl.height + j * p1_l.height))
    for i in range(tile_w):
        panel1.paste(p1_c, (p1_tl.width + i * p1_c.width, p1_tl.height + j * p1_c.height))
    panel1.paste(p1_r, (pw - p1_r.width, p1_tl.height + j * p1_r.height))
panel1.paste(p1_bl, (0, ph - p1_bl.height))
for i in range(tile_w):
    panel1.paste(p1_b, (p1_tl.width + i * p1_b.width, ph - p1_b.height))
panel1.paste(p1_br, (pw - p1_br.width, ph - p1_br.height))
panel1.save(os.path.join(OUT, "panel_wood.png"))
print(f"  Assembled panel_wood.png: {pw}x{ph}")

# Panel style 2 (Cols 3-5)
p2_tl = main_sheet.crop((54, 11, 54+4, 11+26))
p2_t  = main_sheet.crop((59, 11, 59+26, 11+26))
p2_tr = main_sheet.crop((97, 11, 97+4, 11+26))
p2_l  = main_sheet.crop((54, 59, 54+4, 59+26))
p2_c  = main_sheet.crop((59, 59, 59+26, 59+26))
p2_r  = main_sheet.crop((97, 59, 97+4, 59+26))
p2_bl = main_sheet.crop((54, 107, 54+4, 107+26))
p2_b  = main_sheet.crop((59, 107, 59+26, 107+26))
p2_br = main_sheet.crop((97, 107, 97+4, 107+26))

pw2 = p2_tl.width + p2_t.width * tile_w + p2_tr.width
ph2 = p2_tl.height + p2_c.height * tile_h + p2_bl.height
panel2 = Image.new("RGBA", (pw2, ph2), (0, 0, 0, 0))
panel2.paste(p2_tl, (0, 0))
for i in range(tile_w):
    panel2.paste(p2_t, (p2_tl.width + i * p2_t.width, 0))
panel2.paste(p2_tr, (pw2 - p2_tr.width, 0))
for j in range(tile_h):
    panel2.paste(p2_l, (0, p2_tl.height + j * p2_l.height))
    for i in range(tile_w):
        panel2.paste(p2_c, (p2_tl.width + i * p2_c.width, p2_tl.height + j * p2_c.height))
    panel2.paste(p2_r, (pw2 - p2_r.width, p2_tl.height + j * p2_r.height))
panel2.paste(p2_bl, (0, ph2 - p2_bl.height))
for i in range(tile_w):
    panel2.paste(p2_b, (p2_tl.width + i * p2_b.width, ph2 - p2_b.height))
panel2.paste(p2_br, (pw2 - p2_br.width, ph2 - p2_br.height))
panel2.save(os.path.join(OUT, "panel_medium.png"))
print(f"  Assembled panel_medium.png: {pw2}x{ph2}")

# Panel style 3 (Cols 6-8)
p3_tl = main_sheet.crop((102, 11, 102+4, 11+26))
p3_t  = main_sheet.crop((107, 11, 107+26, 11+26))
p3_tr = main_sheet.crop((134, 11, 134+4, 11+26))
p3_l  = main_sheet.crop((102, 59, 102+4, 59+26))
p3_c  = main_sheet.crop((107, 59, 107+26, 59+26))
p3_r  = main_sheet.crop((134, 59, 134+4, 59+26))
p3_bl = main_sheet.crop((102, 107, 102+4, 107+26))
p3_b  = main_sheet.crop((107, 107, 107+26, 107+26))
p3_br = main_sheet.crop((134, 107, 134+4, 107+26))

pw3 = p3_tl.width + p3_t.width * tile_w + p3_tr.width
ph3 = p3_tl.height + p3_c.height * tile_h + p3_bl.height
panel3 = Image.new("RGBA", (pw3, ph3), (0, 0, 0, 0))
panel3.paste(p3_tl, (0, 0))
for i in range(tile_w):
    panel3.paste(p3_t, (p3_tl.width + i * p3_t.width, 0))
panel3.paste(p3_tr, (pw3 - p3_tr.width, 0))
for j in range(tile_h):
    panel3.paste(p3_l, (0, p3_tl.height + j * p3_l.height))
    for i in range(tile_w):
        panel3.paste(p3_c, (p3_tl.width + i * p3_c.width, p3_tl.height + j * p3_c.height))
    panel3.paste(p3_r, (pw3 - p3_r.width, p3_tl.height + j * p3_r.height))
panel3.paste(p3_bl, (0, ph3 - p3_bl.height))
for i in range(tile_w):
    panel3.paste(p3_b, (p3_tl.width + i * p3_b.width, ph3 - p3_b.height))
panel3.paste(p3_br, (pw3 - p3_br.width, ph3 - p3_br.height))
panel3.save(os.path.join(OUT, "panel_settings.png"))
print(f"  Assembled panel_settings.png: {pw3}x{ph3}")

# Bar pieces from bottom of main sheet (y=194, h=12; y=210, h=12; y=226, h=12)
# These are horizontal bar pieces
extract_region(main_sheet, 2, 194, 40, 12, "bar_h_left.png")
extract_region(main_sheet, 7, 194, 34, 12, "bar_h_center.png")
extract_region(main_sheet, 42, 194, 4, 12, "bar_h_right.png")

# === 2. Button columns from main sheet (Col 11-18) ===
print("\n=== Main Sheet Buttons ===")
# Col 11 (x=642, w=29): Button style 1
# Rows: y=5(h=21), y=37(h=21), y=68(h=24), y=100(h=24), y=132(h=24), y=164(h=24)
# Row 0-1: normal/hover (small), Row 2-5: pressed/disabled variants
extract_region(main_sheet, 642, 5, 29, 21, "btn_col11_r0.png")
extract_region(main_sheet, 642, 37, 29, 21, "btn_col11_r1.png")

# Col 12 (x=674, w=28)
extract_region(main_sheet, 674, 5, 28, 21, "btn_col12_r0.png")
extract_region(main_sheet, 674, 37, 28, 21, "btn_col12_r1.png")

# === 3. Big Play Button sheet (wide buttons) ===
print("\n=== Big Play Button ===")
play_btn = Image.open(os.path.join(SPROUT, "Sprite sheets", "UI Big Play Button.png"))
# 4 states in 2x2 grid, each ~90x27
extract_region(play_btn, 3, 2, 90, 27, "btn_wide_normal.png")
extract_region(play_btn, 99, 2, 90, 27, "btn_wide_hover.png")
extract_region(play_btn, 3, 34, 90, 27, "btn_wide_pressed.png")
extract_region(play_btn, 99, 34, 90, 27, "btn_wide_disabled.png")

# === 4. Square button sheets ===
print("\n=== Square Buttons ===")
sq26 = Image.open(os.path.join(SPROUT, "Sprite sheets", "buttons", "Square Buttons 26x26.png"))
# 4 rows x 2 cols, each 26x28 (with padding)
# Row 0: style 1 normal/hover, Row 1: style 1 pressed/disabled
# Row 2: style 2 normal/hover, Row 3: style 2 pressed/disabled
extract_region(sq26, 11, 11, 26, 28, "btn_sq26_s1_normal.png")
extract_region(sq26, 59, 11, 26, 28, "btn_sq26_s1_hover.png")
extract_region(sq26, 11, 59, 26, 28, "btn_sq26_s1_pressed.png")
extract_region(sq26, 59, 59, 26, 28, "btn_sq26_s1_disabled.png")
extract_region(sq26, 11, 107, 26, 28, "btn_sq26_s2_normal.png")
extract_region(sq26, 59, 107, 26, 28, "btn_sq26_s2_hover.png")
extract_region(sq26, 11, 155, 26, 28, "btn_sq26_s2_pressed.png")
extract_region(sq26, 59, 155, 26, 28, "btn_sq26_s2_disabled.png")

# Small square buttons
small_sq = Image.open(os.path.join(SPROUT, "Sprite sheets", "buttons", "Small Square Buttons.png"))
# 32x128 - 4 rows of small buttons
extract_region(small_sq, 0, 0, 16, 32, "btn_small_sq_top.png")
extract_region(small_sq, 16, 0, 16, 32, "btn_small_sq_bottom.png")

# 26x19 buttons (horizontal rectangular)
sq2619 = Image.open(os.path.join(SPROUT, "Sprite sheets", "buttons", "Square Buttons 26x19.png"))
extract_region(sq2619, 11, 6, 26, 19, "btn_26x19_r0_normal.png")
extract_region(sq2619, 59, 6, 26, 19, "btn_26x19_r0_hover.png")
extract_region(sq2619, 11, 38, 26, 19, "btn_26x19_r1_normal.png")
extract_region(sq2619, 59, 38, 26, 19, "btn_26x19_r1_hover.png")
extract_region(sq2619, 11, 70, 26, 19, "btn_26x19_r2_normal.png")
extract_region(sq2619, 59, 70, 26, 19, "btn_26x19_r2_hover.png")
extract_region(sq2619, 11, 102, 26, 19, "btn_26x19_r3_normal.png")
extract_region(sq2619, 59, 102, 26, 19, "btn_26x19_r3_hover.png")

# 19x26 buttons (vertical rectangular)
sq1926 = Image.open(os.path.join(SPROUT, "Sprite sheets", "buttons", "Square Buttons 19x26.png"))
extract_region(sq1926, 7, 9, 18, 30, "btn_19x26_r0_normal.png")
extract_region(sq1926, 39, 9, 18, 30, "btn_19x26_r0_hover.png")
extract_region(sq1926, 71, 9, 18, 30, "btn_19x26_r0_pressed.png")
extract_region(sq1926, 103, 9, 18, 30, "btn_19x26_r0_disabled.png")
extract_region(sq1926, 7, 57, 18, 30, "btn_19x26_r1_normal.png")
extract_region(sq1926, 39, 57, 18, 30, "btn_19x26_r1_hover.png")
extract_region(sq1926, 71, 57, 18, 30, "btn_19x26_r1_pressed.png")
extract_region(sq1926, 103, 57, 18, 30, "btn_19x26_r1_disabled.png")

# === 5. Settings buttons ===
print("\n=== Settings Buttons ===")
settings = Image.open(os.path.join(SPROUT, "Sprite sheets", "UI Settings Buttons.png"))
# Complex layout - extract key elements
extract_region(settings, 1, 35, 93, 10, "settings_slider_handle.png")
extract_region(settings, 1, 54, 93, 19, "settings_btn_normal.png")
extract_region(settings, 1, 86, 93, 19, "settings_btn_hover.png")

# === 6. Icon sheets ===
print("\n=== Icons ===")
icons = Image.open(os.path.join(SPROUT, "Sprite sheets", "Icons", "All Icons.png"))
# 18 icons per row, 3 rows. Each icon ~14x14 with 2px gap
icon_w, icon_h = 14, 14
icon_names_row1 = [
    "icon_heart", "icon_shield", "icon_sword", "icon_coin",
    "icon_star", "icon_skull", "icon_arrow_up", "icon_arrow_down",
    "icon_arrow_left", "icon_arrow_right", "icon_check", "icon_cross",
    "icon_dot", "icon_ring", "icon_diamond", "icon_triangle",
    "icon_square", "icon_circle"
]
icon_positions = []
for row in range(3):
    for col in range(18):
        x = 2 + col * 16
        y = 1 + row * 16
        icon_positions.append((x, y))

for i, name in enumerate(icon_names_row1):
    if i < len(icon_positions):
        x, y = icon_positions[i]
        extract_region(icons, x, y, icon_w, icon_h, f"{name}.png")

# White icons
white_icons = Image.open(os.path.join(SPROUT, "Sprite sheets", "Icons", "white icons.png"))
for i, name in enumerate(icon_names_row1[:6]):
    x = 2 + i * 16
    extract_region(white_icons, x, 1, icon_w, icon_h, f"{name}_white.png")

# Special icons
special = Image.open(os.path.join(SPROUT, "Sprite sheets", "Icons", "special icons", "Special Icons.png"))
# 7 icons per row, 4 rows
special_names = ["icon_happy", "icon_sad", "icon_neutral", "icon_star_small", "icon_heart_small", "icon_skull_small", "icon_dot_small"]
for row in range(4):
    h = [8, 10, 13, 11][row]
    y = [4, 19, 33, 51][row]
    for col in range(7):
        x = [1, 17, 33, 51, 67, 83, 99][col]
        w = [14, 14, 14, 11, 11, 11, 9][col]
        if row < len(special_names):
            name = f"{special_names[col]}_r{row}"
            extract_region(special, x, y, w, h, f"{name}.png")

# === 7. Dialogue boxes (complete panels) ===
print("\n=== Dialogue Boxes ===")
dialog_dir = os.path.join(SPROUT, "Sprite sheets", "Dialouge UI")
for name in ["dialog box.png", "dialog box small.png", "dialog box medium.png", "dialog box big.png"]:
    src = os.path.join(dialog_dir, name)
    if os.path.exists(src):
        img = Image.open(src)
        dst = os.path.join(OUT, name.replace(" ", "_"))
        img.save(dst)
        print(f"  Copied {name}: {img.size[0]}x{img.size[1]}")

# Premade dialog boxes
for name in ["Premade dialog box small.png", "Premade dialog box medium.png", "Premade dialog box big.png"]:
    src = os.path.join(dialog_dir, name)
    if os.path.exists(src):
        img = Image.open(src)
        dst = os.path.join(OUT, name.replace(" ", "_"))
        img.save(dst)
        print(f"  Copied {name}: {img.size[0]}x{img.size[1]}")

# === 8. Settings menu (complete panel) ===
settings_menu = Image.open(os.path.join(SPROUT, "Sprite sheets", "Setting menu.png"))
settings_menu.save(os.path.join(OUT, "settings_menu.png"))
print(f"  Copied settings_menu.png: {settings_menu.size[0]}x{settings_menu.size[1]}")

# === 9. Checkbox and other UI elements ===
# Copy the inventory spritesheet as-is
for name in ["Inventory_Spritesheet.png", "Inventory_Blocks_Spritesheet.png"]:
    src = os.path.join(OUT, name)
    if os.path.exists(src):
        print(f"  Keeping existing {name}")

# Speech bubble
speech_src = os.path.join(OUT, "speech_bubble_grey.png")
if os.path.exists(speech_src):
    print(f"  Keeping existing speech_bubble_grey.png")

# Emoji sheets
emoji_src = os.path.join(OUT, "emoji_happy_sad_sheet.png")
if os.path.exists(emoji_src):
    print(f"  Keeping existing emoji_happy_sad_sheet.png")

print("\n=== Done! ===")
