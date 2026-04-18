#!/usr/bin/env python3
from PIL import Image
import os
import shutil

base_src = "/Users/zhuyong/trae-game/Sprout Lands - UI Pack - Basic pack/Sprite sheets"
base_dst = "/Users/zhuyong/trae-game/Client/GameModes/base_game/Resources/UI"
os.makedirs(base_dst, exist_ok=True)

sheet = Image.open(os.path.join(base_src, "Sprite sheet for Basic Pack.png"))
sw, sh = sheet.size
cell_w, cell_h = 56, 48
cols, rows = sw // cell_w, sh // cell_h

NAMES = {
    (0, 0): "checkbox_empty", (0, 1): "checkbox_checked",
    (0, 2): "checkbox_empty_alt", (0, 3): "checkbox_checked_alt",
    (0, 4): "btn_wide_normal", (0, 5): "btn_wide_hover",
    (0, 6): "btn_wide_pressed", (0, 7): "btn_wide_disabled",
    (0, 8): "btn_normal_normal", (0, 9): "btn_normal_hover",
    (0, 10): "btn_normal_pressed", (0, 11): "btn_normal_disabled",
    (0, 12): "btn_small_normal", (0, 13): "btn_small_hover",
    (0, 14): "btn_small_pressed", (0, 15): "btn_small_disabled",
    (1, 0): "panel_tl", (1, 1): "panel_t", (1, 2): "panel_tr",
    (1, 3): "panel_l", (1, 4): "panel_c", (1, 5): "panel_r",
    (1, 6): "panel_bl", (1, 7): "panel_b", (1, 8): "panel_br",
    (1, 9): "panel2_tl", (1, 10): "panel2_t", (1, 11): "panel2_tr",
    (1, 12): "panel2_l", (1, 13): "panel2_c", (1, 14): "panel2_r",
    (1, 15): "panel2_bl", (2, 0): "panel2_b", (2, 1): "panel2_br",
    (2, 2): "panel3_tl", (2, 3): "panel3_t", (2, 4): "panel3_tr",
    (2, 5): "panel3_l", (2, 6): "panel3_c", (2, 7): "panel3_r",
    (2, 8): "panel3_bl", (2, 9): "panel3_b", (2, 10): "panel3_br",
    (2, 11): "bar_h_left", (2, 12): "bar_h_center", (2, 13): "bar_h_right",
    (2, 14): "bar_v_top", (2, 15): "bar_v_center",
    (3, 0): "bar_v_bottom", (3, 1): "arrow_right",
    (3, 2): "arrow_left", (3, 3): "arrow_up", (3, 4): "arrow_down",
    (3, 5): "dot_empty", (3, 6): "dot_filled",
    (3, 7): "cross", (3, 8): "check",
    (3, 9): "btn_icon_normal", (3, 10): "btn_icon_hover",
    (3, 11): "btn_icon_pressed", (3, 12): "btn_icon_disabled",
    (3, 13): "panel_dark_tl", (3, 14): "panel_dark_t", (3, 15): "panel_dark_tr",
    (4, 0): "panel_dark_l", (4, 1): "panel_dark_c", (4, 2): "panel_dark_r",
    (4, 3): "panel_dark_bl", (4, 4): "panel_dark_b", (4, 5): "panel_dark_br",
}

for (row, col), name in NAMES.items():
    if row < rows and col < cols:
        cell = sheet.crop((col * cell_w, row * cell_h, (col + 1) * cell_w, (row + 1) * cell_h))
        cell.save(os.path.join(base_dst, f"{name}.png"))

# Create full panel 9-patch images by combining cells
def make_panel_image(tl_name, t_name, tr_name, l_name, c_name, r_name, bl_name, b_name, br_name, width_cells=5, height_cells=3, out_name="panel_full"):
    tl = Image.open(os.path.join(base_dst, f"{tl_name}.png"))
    t = Image.open(os.path.join(base_dst, f"{t_name}.png"))
    tr = Image.open(os.path.join(base_dst, f"{tr_name}.png"))
    l = Image.open(os.path.join(base_dst, f"{l_name}.png"))
    c = Image.open(os.path.join(base_dst, f"{c_name}.png"))
    r = Image.open(os.path.join(base_dst, f"{r_name}.png"))
    bl = Image.open(os.path.join(base_dst, f"{bl_name}.png"))
    b = Image.open(os.path.join(base_dst, f"{b_name}.png"))
    br = Image.open(os.path.join(base_dst, f"{br_name}.png"))
    
    cw, ch = c.size
    tw, th = t.size
    lw, lh = l.size
    pw = tl.width + t.width * (width_cells - 2) + tr.width
    ph = tl.height + c.height * (height_cells - 2) + bl.height
    
    panel = Image.new("RGBA", (pw, ph), (0, 0, 0, 0))
    panel.paste(tl, (0, 0))
    for i in range(1, width_cells - 1):
        panel.paste(t, (tl.width + (i-1) * t.width, 0))
    panel.paste(tr, (pw - tr.width, 0))
    for j in range(1, height_cells - 1):
        panel.paste(l, (0, tl.height + (j-1) * l.height))
        for i in range(1, width_cells - 1):
            panel.paste(c, (tl.width + (i-1) * c.width, tl.height + (j-1) * c.height))
        panel.paste(r, (pw - r.width, tl.height + (j-1) * r.height))
    panel.paste(bl, (0, ph - bl.height))
    for i in range(1, width_cells - 1):
        panel.paste(b, (tl.width + (i-1) * b.width, ph - b.height))
    panel.paste(br, (pw - br.width, ph - br.height))
    
    panel.save(os.path.join(base_dst, f"{out_name}.png"))
    print(f"  Created {out_name}.png ({panel.size})")

make_panel_image("panel_tl", "panel_t", "panel_tr", "panel_l", "panel_c", "panel_r", "panel_bl", "panel_b", "panel_br", 6, 4, "panel_light")
make_panel_image("panel2_tl", "panel2_t", "panel2_tr", "panel2_l", "panel2_c", "panel2_r", "panel2_bl", "panel2_b", "panel2_br", 6, 4, "panel_medium")
make_panel_image("panel3_tl", "panel3_t", "panel3_tr", "panel3_l", "panel3_c", "panel3_r", "panel3_bl", "panel3_b", "panel3_br", 6, 4, "panel_wood")
make_panel_image("panel_dark_tl", "panel_dark_t", "panel_dark_tr", "panel_dark_l", "panel_dark_c", "panel_dark_r", "panel_dark_bl", "panel_dark_b", "panel_dark_br", 6, 4, "panel_dark")

# Create full button images by combining cells
def make_btn_image(normal_name, hover_name, pressed_name, disabled_name, width_cells=4, out_name="btn_full"):
    for state, cell_name in [("normal", normal_name), ("hover", hover_name), ("pressed", pressed_name), ("disabled", disabled_name)]:
        cell = Image.open(os.path.join(base_dst, f"{cell_name}.png"))
        cw, ch = cell.size
        btn = Image.new("RGBA", (cw * width_cells, ch), (0, 0, 0, 0))
        for i in range(width_cells):
            btn.paste(cell, (i * cw, 0))
        btn.save(os.path.join(base_dst, f"{out_name}_{state}.png"))
        print(f"  Created {out_name}_{state}.png ({btn.size})")

make_btn_image("btn_wide_normal", "btn_wide_hover", "btn_wide_pressed", "btn_wide_disabled", 4, "btn_wide")
make_btn_image("btn_normal_normal", "btn_normal_hover", "btn_normal_pressed", "btn_normal_disabled", 3, "btn_medium")
make_btn_image("btn_small_normal", "btn_small_hover", "btn_small_pressed", "btn_small_disabled", 2, "btn_small")

# Clean up sheet_cells directory
cells_dir = os.path.join(base_dst, "sheet_cells")
if os.path.exists(cells_dir):
    shutil.rmtree(cells_dir)
    print("  Cleaned up sheet_cells/")

print("\n=== Done! All UI resources extracted. ===")
