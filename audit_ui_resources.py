#!/usr/bin/env python3
"""Audit all UI resources referenced by UITheme for correctness."""
from PIL import Image
import os

ui_dir = "/Users/zhuyong/trae-game/Client/GameModes/base_game/Resources/UI"

resources = {
    "btn_wide_normal": ("StyleBoxTexture button", 90, 27),
    "btn_wide_hover": ("StyleBoxTexture button", 90, 27),
    "btn_wide_pressed": ("StyleBoxTexture button", 90, 27),
    "btn_wide_disabled": ("StyleBoxTexture button", 90, 27),
    "btn_sq26_s1_normal": ("StyleBoxTexture small_button", 26, 28),
    "btn_sq26_s1_hover": ("StyleBoxTexture small_button", 26, 28),
    "btn_sq26_s1_pressed": ("StyleBoxTexture small_button", 26, 28),
    "btn_sq26_s1_disabled": ("StyleBoxTexture small_button", 26, 28),
    "btn_sq26_s2_normal": ("StyleBoxTexture icon_button", 26, 28),
    "btn_sq26_s2_hover": ("StyleBoxTexture icon_button", 26, 28),
    "btn_sq26_s2_pressed": ("StyleBoxTexture icon_button", 26, 28),
    "btn_sq26_s2_disabled": ("StyleBoxTexture icon_button", 26, 28),
    "panel_light": ("StyleBoxTexture panel_bg", 490, 172),
    "panel_wood": ("StyleBoxTexture wood_panel", 110, 140),
    "panel_medium": ("StyleBoxTexture medium_panel", 86, 130),
    "dialog_box_big": ("StyleBoxTexture dark_panel/bar", 176, 48),
    "dialog_box_small": ("StyleBoxTexture card_panel", 112, 48),
    "icon_skull": ("TextureRect icon", 14, 14),
    "icon_star": ("TextureRect icon", 14, 14),
    "icon_coin": ("TextureRect icon", 14, 14),
    "icon_heart": ("TextureRect icon", 14, 14),
    "icon_shield": ("TextureRect icon", 14, 14),
    "icon_sword": ("TextureRect icon", 14, 14),
}

print("=== UITheme Resource Audit ===\n")
issues = []
for name, (usage, exp_w, exp_h) in sorted(resources.items()):
    path = os.path.join(ui_dir, name + ".png")
    if not os.path.exists(path):
        issues.append(f"MISSING: {name} ({usage})")
        continue
    img = Image.open(path)
    w, h = img.size
    
    # Check if the image is a proper single sprite
    # For StyleBoxTexture: the image should be a complete panel/button that can be 9-sliced
    # For TextureRect (icons): should be a single icon
    
    status = "OK"
    note = ""
    
    if "btn_wide" in name:
        if w != exp_w or h != exp_h:
            status = "WRONG SIZE"
            note = f"expected {exp_w}x{exp_h}, got {w}x{h}"
    elif "btn_sq26" in name:
        if w != exp_w or h != exp_h:
            status = "WRONG SIZE"
            note = f"expected {exp_w}x{exp_h}, got {w}x{h}"
    elif "panel_light" in name:
        # panel_light is 490x172 - this is the FULL sheet panel, which is fine for StyleBoxTexture
        # BUT the margin must be set correctly to use only the border
        if w > 500:
            status = "SUSPICIOUS"
            note = f"very large ({w}x{h}) - may be entire sheet"
    elif "panel_wood" in name or "panel_medium" in name:
        if w > 150 or h > 180:
            status = "SUSPICIOUS"
            note = f"too large ({w}x{h}) for assembled 9-patch panel"
    elif "dialog_box" in name:
        if w > 200 or h > 60:
            status = "SUSPICIOUS"
            note = f"too large ({w}x{h}) for dialog box"
    elif "icon_" in name:
        if w > 20 or h > 20:
            status = "WRONG SIZE"
            note = f"expected ~14x14, got {w}x{h}"
    
    if status != "OK":
        issues.append(f"{status}: {name} = {w}x{h} ({usage}) {note}")
    else:
        print(f"  OK: {name} = {w}x{h} ({usage})")

if issues:
    print(f"\n=== ISSUES ({len(issues)}) ===")
    for issue in issues:
        print(f"  {issue}")
else:
    print("\n  All resources look correct!")

# Now check the key problem: panel_light is the FULL 490x172 sheet
# This means StyleBoxTexture will show the ENTIRE sheet as background
# We need to understand the structure of this image
print("\n=== Detailed Analysis of panel_light ===")
panel_light = Image.open(os.path.join(ui_dir, "panel_light.png"))
print(f"  Size: {panel_light.size}")
# Check if it has transparent borders (9-patch style)
arr = __import__("numpy").array(panel_light.convert("RGBA"))
alpha = arr[:, :, 3]
# Find non-transparent region bounds
rows_with_content = __import__("numpy").where(alpha.max(axis=1) > 0)[0]
cols_with_content = __import__("numpy").where(alpha.max(axis=0) > 0)[0]
if len(rows_with_content) > 0 and len(cols_with_content) > 0:
    print(f"  Content bounds: x=[{cols_with_content[0]}, {cols_with_content[-1]}], y=[{rows_with_content[0]}, {rows_with_content[-1]}]")
    print(f"  Content size: {cols_with_content[-1]-cols_with_content[0]+1}x{rows_with_content[-1]-rows_with_content[0]+1}")

# Check dialog_box_big
print("\n=== Detailed Analysis of dialog_box_big ===")
db_big = Image.open(os.path.join(ui_dir, "dialog_box_big.png"))
print(f"  Size: {db_big.size}")
