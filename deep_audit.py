#!/usr/bin/env python3
"""
Deep analysis of UI resource usage issues:
1. Event panel showing entire sheet - panel_light is 490x172, margin=24 means 
   only 24px border is used for 9-slice, center is tiled. BUT the entire image 
   is still the texture, so if the panel is smaller than 490x172, it will show 
   the full image compressed.

2. Card panel only shows background, no card icon - need to check card icon paths.

The ROOT CAUSE: StyleBoxTexture with a large panel image (490x172) will 
compress the ENTIRE image to fit the control size. The margin only determines 
which parts stretch vs tile. We need EITHER:
A) Use a properly sized panel image (just the 9-patch border pieces assembled 
   to a small size like 48x48)
B) Use AtlasTexture to reference a region of the sheet
C) Use the Premade dialog boxes which are already properly sized complete panels
"""
from PIL import Image
import os

ui_dir = "/Users/zhuyong/trae-game/Client/GameModes/base_game/Resources/UI"
sprint_dir = "/Users/zhuyong/trae-game/Sprout Lands - UI Pack - Basic Pack"

# Check what premade panels are available
print("=== Available Premade Panels ===")
premade_dir = os.path.join(sprint_dir, "Sprite sheets", "Dialouge UI")
for f in sorted(os.listdir(premade_dir)):
    if f.endswith(".png"):
        img = Image.open(os.path.join(premade_dir, f))
        print(f"  {f}: {img.size[0]}x{img.size[1]}")

# Check card icon resources
print("\n=== Card Icon Resources ===")
card_dir = "/Users/zhuyong/trae-game/Client/GameModes/base_game/Resources/Icons/Cards"
skill_dir = "/Users/zhuyong/trae-game/Client/GameModes/base_game/Resources/Icons/Skills"
for d in [card_dir, skill_dir]:
    if os.path.exists(d):
        for f in sorted(os.listdir(d)):
            if f.endswith(".png"):
                img = Image.open(os.path.join(d, f))
                print(f"  {os.path.join(d, f)}: {img.size[0]}x{img.size[1]}")
    else:
        print(f"  DIR NOT FOUND: {d}")

# The real solution: use Premade dialog boxes for panels
# They are complete, properly sized panels that work with StyleBoxTexture
print("\n=== Solution Analysis ===")
print("Problem 1: panel_light (490x172) is too large for StyleBoxTexture")
print("  - StyleBoxTexture displays the ENTIRE texture, then uses margins for 9-slice")
print("  - A 490x172 image compressed to a 560x480 panel will look distorted")
print("  - Solution: Use Premade dialog boxes (already properly sized)")
print()
print("Problem 2: Card icons not found at expected paths")
print("  - combat_hud.gd references: res://GameModes/base_game/Resources/Icons/Cards/strike.png")
print("  - Need to verify these paths exist")
