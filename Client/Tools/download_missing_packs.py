#!/usr/bin/env python3
"""下载 Kenney Roguelike 和 Platformer Characters - 修复版"""

import urllib.request
import re
import ssl
import os
import zipfile

ctx = ssl.create_default_context()
ctx.check_hostname = False
ctx.verify_mode = ssl.CERT_NONE

PACKS = {
    "roguelike-base-pack": "https://kenney.nl/assets/roguelike-base-pack",
    "platformer-characters": "https://kenney.nl/assets/platformer-characters",
}

BASE_DIR = "Assets_Library/_downloads"

for pack_name, page_url in PACKS.items():
    print(f"\n{'='*60}")
    print(f"📦 下载: {pack_name}")
    print(f"{'='*60}")
    
    out_dir = os.path.join(BASE_DIR, pack_name)
    os.makedirs(out_dir, exist_ok=True)
    zip_path = os.path.join(out_dir, f"{pack_name}.zip")
    
    if os.path.exists(zip_path) and os.path.getsize(zip_path) > 1000:
        print(f"✅ 已缓存: {zip_path}")
    else:
        try:
            print(f"🔍 获取页面: {page_url}")
            req = urllib.request.Request(page_url, headers={'User-Agent': 'Mozilla/5.0'})
            html = urllib.request.urlopen(req, context=ctx, timeout=30).read().decode('utf-8')
            
            # 精确提取 ZIP 下载链接 - 只匹配 kenney_xxx.zip 格式
            links = re.findall(r'https://kenney\.nl/media/pages/assets/[^"\'>\s]+?\.zip', html)
            # 过滤只保留真正的下载链接（kenney_ 开头的ZIP）
            download_links = [l for l in links if 'kenney_' in l]
            
            if not download_links:
                print(f"❌ 未找到下载链接，尝试所有链接...")
                download_links = links
            
            if not download_links:
                print(f"❌ 完全没有下载链接")
                continue
            
            download_url = download_links[0]
            print(f"🔗 下载链接: {download_url}")
            
            print(f"⬇️ 下载中...")
            req2 = urllib.request.Request(download_url, headers={'User-Agent': 'Mozilla/5.0'})
            data = urllib.request.urlopen(req2, context=ctx, timeout=120).read()
            
            with open(zip_path, 'wb') as f:
                f.write(data)
            
            size_mb = len(data) / 1024 / 1024
            print(f"✅ 下载完成: {size_mb:.1f} MB")
        except Exception as e:
            print(f"❌ 下载失败: {e}")
            continue
    
    # 解压
    try:
        with zipfile.ZipFile(zip_path, 'r') as z:
            z.extractall(out_dir)
        
        png_count = 0
        for root, dirs, files in os.walk(out_dir):
            png_count += sum(1 for f in files if f.endswith('.png'))
        
        print(f"✅ 解压完成，PNG 文件数: {png_count}")
        
        # 列出关键文件
        for root, dirs, files in os.walk(out_dir):
            for f in sorted(files):
                if f.endswith('.png'):
                    print(f"   🎨 {os.path.relpath(os.path.join(root, f), out_dir)}")
                    if png_count < 50:
                        pass  # 列出所有
    except Exception as e:
        print(f"❌ 解压失败: {e}")

print(f"\n{'='*60}")
print(f"🎉 下载完成！")
