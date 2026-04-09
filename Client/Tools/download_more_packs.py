#!/usr/bin/env python3
"""搜索并下载更多 Kenney 素材包"""
import urllib.request, re, ssl, os, zipfile

ctx = ssl.create_default_context()
ctx.check_hostname = False
ctx.verify_mode = ssl.CERT_NONE

urls = [
    'https://kenney.nl/assets/roguelike-base-pack',
    'https://kenney.nl/assets/roguelike-pack',
    'https://kenney.nl/assets/1-bit-pack',
    'https://kenney.nl/assets/mini-characters-1bit',
    'https://kenney.nl/assets/monster-builder-pack',
    'https://kenney.nl/assets/tiny-dungeon',
    'https://kenney.nl/assets/simplified-platformer-pack',
]

for url in urls:
    try:
        req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
        html = urllib.request.urlopen(req, context=ctx, timeout=10).read().decode('utf-8')
        links = [l for l in re.findall(r'https://kenney\.nl/media/pages/assets/[^"]*?\.zip', html) if 'kenney_' in l]
        title_m = re.search(r'<title>(.*?)</title>', html)
        title = title_m.group(1) if title_m else "Unknown"
        print(f'✅ {url}')
        print(f'   标题: {title}')
        if links:
            print(f'   下载链接: {links[0][:80]}...')
            # 下载
            pack_name = url.split('/')[-1]
            out_dir = f'Assets_Library/_downloads/{pack_name}'
            os.makedirs(out_dir, exist_ok=True)
            zip_path = f'{out_dir}/{pack_name}.zip'
            
            if not os.path.exists(zip_path) or os.path.getsize(zip_path) < 1000:
                print(f'   ⬇️ 下载中...')
                req2 = urllib.request.Request(links[0], headers={'User-Agent': 'Mozilla/5.0'})
                data = urllib.request.urlopen(req2, context=ctx, timeout=120).read()
                with open(zip_path, 'wb') as f:
                    f.write(data)
                print(f'   ✅ 下载完成: {len(data)/1024/1024:.1f} MB')
                
                # 解压
                with zipfile.ZipFile(zip_path, 'r') as z:
                    z.extractall(out_dir)
                png_count = sum(1 for r, d, fs in os.walk(out_dir) for f in fs if f.endswith('.png'))
                print(f'   ✅ 解压完成: {png_count} 个PNG')
            else:
                print(f'   ✅ 已缓存')
        else:
            print(f'   无下载链接')
        print()
    except Exception as e:
        print(f'❌ {url} → {e}')
        print()
