#!/usr/bin/env python3
from PIL import Image
import os

BASE = 'Client/GameModes/light_shadow_traveler/Resources'
DIRS = ['Characters', 'Enemies', 'UI', 'Platforms', 'Effects']

count = 0
for d in DIRS:
    dpath = os.path.join(BASE, d)
    if not os.path.isdir(dpath):
        continue
    for f in sorted(os.listdir(dpath)):
        if not f.endswith('.png') or f.endswith('.import'):
            continue
        fp = os.path.join(dpath, f)
        try:
            img = Image.open(fp)
            if img.mode != 'RGBA':
                img = img.convert('RGBA')
            bg = img.getpixel((0, 0))[:3]
            datas = list(img.getdata())
            new_data = []
            changed = False
            for item in datas:
                r, g, b, a = item
                dr = abs(r - bg[0])
                dg = abs(g - bg[1])
                db_abs = abs(b - bg[2])
                dist = (dr*dr + dg*dg + db_abs*db_abs) ** 0.5
                if dist < 35 and a > 0:
                    new_data.append((r, g, b, 0))
                    changed = True
                else:
                    new_data.append(item)
            if changed:
                img.putdata(new_data)
                img.save(fp, 'PNG')
                count += 1
        except Exception as e:
            print(f"ERR {f}: {e}")
print(f"Done: {count} files made transparent")
