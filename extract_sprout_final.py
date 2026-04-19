#!/usr/bin/env python3
"""
Correctly extract UI resources from Sprout Lands for StyleBoxTexture usage.

KEY INSIGHT: StyleBoxTexture displays the ENTIRE texture, then uses margins
for 9-slice stretching. Therefore, panel textures must be SMALL images 
with clear border regions, not large complete panels.

Solution: Use Premade dialog boxes which are properly sized (48x48, 176x64, etc.)
and assemble small 9-patch panels from the sheet pieces.
"""
from PIL import Image
import os
import shutil

SPROUT = "/Users/zhuyong/trae-game/Sprout Lands - UI Pack - Basic Pack"
OUT = "/Users/zhuyong/trae-game/Client/GameModes/base_game/Resources/UI"

os.makedirs(OUT, exist_ok=True)

def extract(img: Image.Image, x: int, y: int, w: int, h: int, name: str) -> None:
    region = img.crop((x, y, x + w, y + h))
    region.save(os.path.join(OUT, name))
    print(f"  {name}: {w}x{h}")

# === 1. Premade Dialog Boxes - BEST for StyleBoxTexture panels ===
print("=== Premade Dialog Boxes (for panels) ===")
dialog_dir = os.path.join(SPROUT, "Sprite sheets", "Dialouge UI")

# dialog box.png (48x48) - perfect 9-slice panel, small enough for StyleBoxTexture
shutil.copy2(os.path.join(dialog_dir, "dialog box.png"), os.path.join(OUT, "panel_9slice.png"))
print(f"  panel_9slice.png: 48x48 (from dialog box.png)")

# Premade dialog box small.png (176x64) - good for medium panels
shutil.copy2(os.path.join(dialog_dir, "Premade dialog box small.png"), os.path.join(OUT, "panel_light.png"))
print(f"  panel_light.png: 176x64 (from Premade dialog box small.png)")

# Premade dialog box medium.png (240x64) - good for wide panels
shutil.copy2(os.path.join(dialog_dir, "Premade dialog box medium.png"), os.path.join(OUT, "panel_medium.png"))
print(f"  panel_medium.png: 240x64 (from Premade dialog box medium.png)")

# Premade dialog box big.png (304x64) - good for large panels
shutil.copy2(os.path.join(dialog_dir, "Premade dialog box  big.png"), os.path.join(OUT, "panel_dark.png"))
print(f"  panel_dark.png: 304x64 (from Premade dialog box big.png)")

# dialog box small.png (112x48) - good for card-sized panels
shutil.copy2(os.path.join(dialog_dir, "dialog box small.png"), os.path.join(OUT, "panel_card.png"))
print(f"  panel_card.png: 112x48 (from dialog box small.png)")

# dialog box medium.png (128x48) - good for medium card panels
shutil.copy2(os.path.join(dialog_dir, "dialog box medium.png"), os.path.join(OUT, "panel_dialog.png"))
print(f"  panel_dialog.png: 128x48 (from dialog box medium.png)")

# dialog box big.png (176x48) - good for dark/wide panels
shutil.copy2(os.path.join(dialog_dir, "dialog box big.png"), os.path.join(OUT, "panel_wide.png"))
print(f"  panel_wide.png: 176x48 (from dialog box big.png)")

# === 2. Assemble wood-style panel from 9-patch pieces ===
print("\n=== Wood Panel (from 9-patch pieces) ===")
main_sheet = Image.open(os.path.join(SPROUT, "Sprite sheets", "Sprite sheet for Basic Pack.png"))

# Panel style 1 pieces (Cols 0-2)
# The pieces are: left edge(4px wide), center(34px wide), right edge(4px wide)
# Rows: top(28px), middle(28px), bottom(28px)
# Assemble into a small 3x3 tiled panel
p1_tl = main_sheet.crop((2, 11, 6, 39))
p1_t  = main_sheet.crop((7, 11, 41, 39))
p1_tr = main_sheet.crop((42, 11, 46, 39))
p1_l  = main_sheet.crop((2, 59, 6, 87))
p1_c  = main_sheet.crop((7, 59, 41, 87))
p1_r  = main_sheet.crop((42, 59, 46, 87))
p1_bl = main_sheet.crop((2, 107, 6, 135))
p1_b  = main_sheet.crop((7, 107, 41, 135))
p1_br = main_sheet.crop((42, 107, 46, 135))

# Assemble 1x1 tile (minimal size for 9-slice)
pw = p1_tl.width + p1_t.width + p1_tr.width  # 4+34+4 = 42
ph = p1_tl.height + p1_c.height + p1_bl.height  # 28+28+28 = 84
panel_wood = Image.new("RGBA", (pw, ph), (0, 0, 0, 0))
panel_wood.paste(p1_tl, (0, 0))
panel_wood.paste(p1_t, (p1_tl.width, 0))
panel_wood.paste(p1_tr, (p1_tl.width + p1_t.width, 0))
panel_wood.paste(p1_l, (0, p1_tl.height))
panel_wood.paste(p1_c, (p1_tl.width, p1_tl.height))
panel_wood.paste(p1_r, (p1_tl.width + p1_t.width, p1_tl.height))
panel_wood.paste(p1_bl, (0, p1_tl.height + p1_c.height))
panel_wood.paste(p1_b, (p1_tl.width, p1_tl.height + p1_c.height))
panel_wood.paste(p1_br, (p1_tl.width + p1_t.width, p1_tl.height + p1_c.height))
panel_wood.save(os.path.join(OUT, "panel_wood.png"))
print(f"  panel_wood.png: {pw}x{ph}")

# === 3. Bar background from dialog box ===
# Use the smallest dialog box (48x48) for bar backgrounds too
shutil.copy2(os.path.join(dialog_dir, "dialog box.png"), os.path.join(OUT, "bar_bg.png"))
print(f"  bar_bg.png: 48x48 (from dialog box.png)")

# === 4. Buttons (from Big Play Button sheet) ===
print("\n=== Buttons ===")
play_btn = Image.open(os.path.join(SPROUT, "Sprite sheets", "UI Big Play Button.png"))
# 4 states: normal(3,2), hover(99,2), pressed(3,34), disabled(99,34)
# Each ~90x27
extract(play_btn, 3, 2, 90, 27, "btn_wide_normal.png")
extract(play_btn, 99, 2, 90, 27, "btn_wide_hover.png")
extract(play_btn, 3, 34, 90, 27, "btn_wide_pressed.png")
extract(play_btn, 99, 34, 90, 27, "btn_wide_disabled.png")

# Square buttons for small/icon buttons
sq26 = Image.open(os.path.join(SPROUT, "Sprite sheets", "buttons", "Square Buttons 26x26.png"))
# Style 1: rows 0-1 (normal/hover/pressed/disabled)
# The sheet has 2 columns and 4 rows
# Each button is 26x26 with padding
# Col 0: normal/pressed, Col 1: hover/disabled
# Row 0-1: style 1, Row 2-3: style 2
# Let me re-analyze the exact positions
# Sheet is 96x192
# 2 cols: x=11(w=26), x=59(w=26)  with gaps at 0-10, 37-58, 85-95
# 4 rows: y=11(h=26), y=59(h=26), y=107(h=26), y=155(h=26)
extract(sq26, 11, 11, 26, 26, "btn_sq_s1_normal.png")
extract(sq26, 59, 11, 26, 26, "btn_sq_s1_hover.png")
extract(sq26, 11, 59, 26, 26, "btn_sq_s1_pressed.png")
extract(sq26, 59, 59, 26, 26, "btn_sq_s1_disabled.png")
extract(sq26, 11, 107, 26, 26, "btn_sq_s2_normal.png")
extract(sq26, 59, 107, 26, 26, "btn_sq_s2_hover.png")
extract(sq26, 11, 155, 26, 26, "btn_sq_s2_pressed.png")
extract(sq26, 59, 155, 26, 26, "btn_sq_s2_disabled.png")

# === 5. Icons ===
print("\n=== Icons ===")
icons = Image.open(os.path.join(SPROUT, "Sprite sheets", "Icons", "All Icons.png"))
# 18 icons per row, 3 rows. Each icon ~14x14 with 2px gap
icon_names = [
    "icon_heart", "icon_shield", "icon_sword", "icon_coin",
    "icon_star", "icon_skull", "icon_arrow_up", "icon_arrow_down",
    "icon_arrow_left", "icon_arrow_right", "icon_check", "icon_cross",
    "icon_dot", "icon_ring", "icon_diamond", "icon_triangle",
    "icon_square", "icon_circle"
]
for row in range(3):
    for col in range(18):
        idx = row * 18 + col
        if idx < len(icon_names):
            x = 2 + col * 16
            y = 1 + row * 16
            extract(icons, x, y, 14, 14, f"{icon_names[idx]}.png")

# White icons
white_icons = Image.open(os.path.join(SPROUT, "Sprite sheets", "Icons", "white icons.png"))
for i, name in enumerate(icon_names[:6]):
    x = 2 + i * 16
    extract(white_icons, x, 1, 14, 14, f"{name}_white.png")

# === 6. Settings menu ===
print("\n=== Settings ===")
settings_menu = Image.open(os.path.join(SPROUT, "Sprite sheets", "Setting menu.png"))
settings_menu.save(os.path.join(OUT, "settings_menu.png"))
print(f"  settings_menu.png: {settings_menu.size[0]}x{settings_menu.size[1]}")

# Settings buttons
settings = Image.open(os.path.join(SPROUT, "Sprite sheets", "UI Settings Buttons.png"))
extract(settings, 1, 54, 93, 19, "settings_btn_normal.png")
extract(settings, 1, 86, 93, 19, "settings_btn_hover.png")

print("\n=== Done! ===")
print("\nKey changes:")
print("  - panel_light: 490x172 -> 176x64 (Premade dialog box small)")
print("  - panel_medium: 86x130 -> 240x64 (Premade dialog box medium)")
print("  - panel_dark: 490x60 -> 304x64 (Premade dialog box big)")
print("  - panel_card: NEW 112x48 (dialog box small)")
print("  - panel_9slice: NEW 48x48 (dialog box - perfect 9-slice)")
print("  - bar_bg: NEW 48x48 (dialog box)")
print("  - panel_wood: 110x140 -> 42x84 (proper 9-patch assembly)")
print("  - btn_sq: 26x28 -> 26x26 (corrected from Square Buttons)")
