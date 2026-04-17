#!/usr/bin/env python3
from PIL import Image
import os
import shutil

base_src = "/Users/zhuyong/trae-game/Sprout Lands - UI Pack - Basic pack/Sprite sheets"
base_dst = "/Users/zhuyong/trae-game/Client/GameModes/base_game/Resources/UI"
os.makedirs(base_dst, exist_ok=True)

def crop_and_save(img: Image.Image, x: int, y: int, w: int, h: int, name: str) -> None:
    tile = img.crop((x, y, x + w, y + h))
    tile.save(os.path.join(base_dst, name))
    print(f"  Saved {name} ({tile.size})")

# 1. UI Big Play Button.png (192x64) - 3 states x 64px wide
img = Image.open(os.path.join(base_src, "UI Big Play Button.png"))
print(f"Big Play Button: {img.size}")
for i, state in enumerate(["normal", "hover", "pressed"]):
    crop_and_save(img, i * 64, 0, 64, 64, f"btn_play_{state}.png")

# 2. UI Settings Buttons.png (128x240) - 2 columns x 5 rows of 64x48
img = Image.open(os.path.join(base_src, "UI Settings Buttons.png"))
print(f"Settings Buttons: {img.size}")
states = ["normal", "hover"]
for row in range(5):
    for col in range(2):
        crop_and_save(img, col * 64, row * 48, 64, 48, f"btn_settings_r{row}_{states[col]}.png")

# 3. Square Buttons 26x26.png (96x192) - 3 columns x 6 rows of 32x32
img = Image.open(os.path.join(base_src, "buttons", "Square Buttons 26x26.png"))
print(f"Square Buttons 26x26: {img.size}")
states = ["normal", "hover", "pressed"]
for row in range(6):
    for col in range(3):
        crop_and_save(img, col * 32, row * 32, 32, 32, f"btn_square_26_r{row}_{states[col]}.png")

# 4. Square Buttons 19x26.png (128x96) - 4 columns x 3 rows of 32x32
img = Image.open(os.path.join(base_src, "buttons", "Square Buttons 19x26.png"))
print(f"Square Buttons 19x26: {img.size}")
for row in range(3):
    for col in range(4):
        state_names = ["normal", "hover", "pressed", "disabled"]
        crop_and_save(img, col * 32, row * 32, 32, 32, f"btn_19x26_r{row}_{state_names[col]}.png")

# 5. Square Buttons 26x19.png (96x128) - 3 columns x 4 rows of 32x32
img = Image.open(os.path.join(base_src, "buttons", "Square Buttons 26x19.png"))
print(f"Square Buttons 26x19: {img.size}")
for row in range(4):
    for col in range(3):
        crop_and_save(img, col * 32, row * 32, 32, 32, f"btn_26x19_r{row}_{states[col]}.png")

# 6. Small Square Buttons.png (32x128) - 1 column x 4 rows of 32x32
img = Image.open(os.path.join(base_src, "buttons", "Small Square Buttons.png"))
print(f"Small Square Buttons: {img.size}")
for i in range(4):
    crop_and_save(img, 0, i * 32, 32, 32, f"btn_small_sq_{i}.png")

# 7. All Icons.png (288x48) - 6 icons of 48x48
img = Image.open(os.path.join(base_src, "Icons", "All Icons.png"))
print(f"All Icons: {img.size}")
icon_names = ["heart", "coin", "star", "sword", "shield", "skull"]
for i in range(6):
    name = icon_names[i] if i < len(icon_names) else f"icon_{i}"
    crop_and_save(img, i * 48, 0, 48, 48, f"icon_{name}.png")

# 8. white icons.png (96x48) - 2 icons of 48x48
img = Image.open(os.path.join(base_src, "Icons", "white icons.png"))
print(f"White Icons: {img.size}")
for i in range(2):
    crop_and_save(img, i * 48, 0, 48, 48, f"icon_white_{i}.png")

# 9. Special Icons.png
img = Image.open(os.path.join(base_src, "Icons", "special icons", "Special Icons.png"))
print(f"Special Icons: {img.size}")
w, h = img.size
if w > 0 and h > 0:
    img.save(os.path.join(base_dst, "special_icons_sheet.png"))
    print(f"  Saved special_icons_sheet.png ({img.size})")

# 10. Small Happiness-Sadness icons.png
img = Image.open(os.path.join(base_src, "Icons", "special icons", "Small Happines-Sadness icons.png"))
print(f"Happiness-Sadness: {img.size}")
img.save(os.path.join(base_dst, "emoji_happy_sad_sheet.png"))
print(f"  Saved emoji_happy_sad_sheet.png ({img.size})")

# 11. Dialog boxes - copy as panel backgrounds
for fname in os.listdir(os.path.join(base_src, "Dialouge UI")):
    if fname.endswith(".png") and "Emotes" not in fname:
        src = os.path.join(base_src, "Dialouge UI", fname)
        safe_name = fname.replace(" ", "_").replace("__", "_").replace("-_spritesheet_", "")
        shutil.copy2(src, os.path.join(base_dst, safe_name))
        img_d = Image.open(src)
        print(f"  Copied {safe_name} ({img_d.size})")

# 12. Setting menu - copy as settings panel background
shutil.copy2(
    os.path.join(base_src, "Setting menu.png"),
    os.path.join(base_dst, "panel_settings.png")
)
print(f"  Copied panel_settings.png")

# 13. Sprite sheet for Basic Pack - copy whole sheet for reference
shutil.copy2(
    os.path.join(base_src, "Sprite sheet for Basic Pack.png"),
    os.path.join(base_dst, "basic_pack_sheet.png")
)
img_bp = Image.open(os.path.join(base_src, "Sprite sheet for Basic Pack.png"))
print(f"  Copied basic_pack_sheet.png ({img_bp.size})")

# 14. Copy font
font_src = "/Users/zhuyong/trae-game/Sprout Lands - UI Pack - Basic pack/fonts/pixelFont-7-8x14-sproutLands.ttf"
font_dst = "/Users/zhuyong/trae-game/Client/Assets_Library/Fonts"
os.makedirs(font_dst, exist_ok=True)
shutil.copy2(font_src, os.path.join(font_dst, "pixelFont-sproutLands.ttf"))
print(f"  Copied font to {font_dst}/pixelFont-sproutLands.ttf")

# 15. Copy emoji inventory sheets
emoji_dir = "/Users/zhuyong/trae-game/Sprout Lands - UI Pack - Basic pack/emojis-free/emoji style ui"
if os.path.exists(emoji_dir):
    for fname in ["Inventory_Spritesheet.png", "Inventory_Blocks_Spritesheet.png"]:
        src = os.path.join(emoji_dir, fname)
        if os.path.exists(src):
            shutil.copy2(src, os.path.join(base_dst, fname))
            print(f"  Copied {fname}")

# 16. Copy speech bubble
bubble_src = "/Users/zhuyong/trae-game/Sprout Lands - UI Pack - Basic pack/emojis-free/speech_bubble_grey.png"
if os.path.exists(bubble_src):
    shutil.copy2(bubble_src, os.path.join(base_dst, "speech_bubble_grey.png"))
    print(f"  Copied speech_bubble_grey.png")

print("\n=== Done! All UI resources extracted. ===")

# List all extracted files
print("\n=== Extracted files: ===")
for f in sorted(os.listdir(base_dst)):
    fpath = os.path.join(base_dst, f)
    if os.path.isfile(fpath):
        size = os.path.getsize(fpath)
        print(f"  {f} ({size} bytes)")
