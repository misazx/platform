#!/usr/bin/env python3
"""
Kenney.nl 下载机制深度分析工具
分析网站结构、提取真实下载URL、测试各种下载方式
"""

import urllib.request
import urllib.error
import ssl
import re
import json
from pathlib import Path
from datetime import datetime


def analyze_kenney_page(url: str):
    """分析 Kenney.nl 页面，提取下载信息"""
    print(f"\n{'='*70}")
    print(f"🔍 分析页面: {url}")
    print('='*70)
    
    # 创建 SSL 上下文（跳过验证）
    ctx = ssl.create_default_context()
    ctx.check_hostname = False
    ctx.verify_mode = ssl.CERT_NONE
    
    # 发送请求
    req = urllib.request.Request(
        url,
        headers={
            'User-Agent': 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
            'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8',
            'Accept-Language': 'en-US,en;q=0.9',
        }
    )
    
    try:
        with urllib.request.urlopen(req, context=ctx, timeout=30) as response:
            html = response.read().decode('utf-8', errors='ignore')
            
            print(f"\n✅ 页面获取成功!")
            print(f"   大小: {len(html)} 字符")
            print(f"   HTTP状态: {response.status}")
            
            # 分析1: 查找所有链接
            print(f"\n📋 分析1: 提取所有链接...")
            links = re.findall(r'href=["\']([^"\']+)["\']', html)
            download_links = [l for l in links if any(kw in l.lower() for kw in 
                             ['download', '.zip', '.rar', 'asset', 'media'])]
            
            print(f"   总链接数: {len(links)}")
            print(f"   下载相关链接: {len(download_links)}")
            
            if download_links:
                print(f"\n   发现的潜在下载链接:")
                for i, link in enumerate(download_links[:20], 1):
                    print(f"      {i}. {link[:100]}")
            
            # 分析2: 查找 JavaScript 数据
            print(f"\n📋 分析2: 查找 JavaScript 数据/配置...")
            js_patterns = [
                r'["\']downloadUrl["\']\s*:\s*["\']([^"\']+)["\']',
                r'["\']file_url["\']\s*:\s*["\']([^"\']+)["\']',
                r'["\']asset_url["\']\s*:\s*["\']([^"\']+)["\']',
                r'url\s*=\s*["\']([^"\']+\.zip)["\']',
                r'data-url=["\']([^"\']+)["\']',
                r'src=["\']([^"\']*\.zip)["\']',
            ]
            
            found_urls = []
            for pattern in js_patterns:
                matches = re.findall(pattern, html, re.IGNORECASE)
                if matches:
                    found_urls.extend(matches)
                    print(f"   ✓ 匹配模式 '{pattern[:40]}...': {len(matches)} 个结果")
                    
            if found_urls:
                print(f"\n   提取到的URL:")
                for i, url in enumerate(set(found_urls)[:10], 1):
                    print(f"      {i}. {url}")
            
            # 分析3: 查找表单和按钮
            print(f"\n📋 分析3: 查找下载表单/按钮...")
            forms = re.findall(r'<form[^>]*>(.*?)</form>', html, re.DOTALL | re.IGNORECASE)
            buttons = re.findall(r'<button[^>]*>(.*?)</button>', html, re.IGNORECASE | re.DOTALL)
            inputs_hidden = re.findall(r'<input[^>]*type=["\']hidden["\'][^>]*value=["\']([^"\']+)["\']', html)
            
            print(f"   表单数: {len(forms)}")
            print(f"   按钮数: {len(buttons)}")
            print(f"   隐藏字段: {len(inputs_hidden)}")
            
            if buttons:
                print(f"\n   按钮:")
                for btn in buttons[:10]:
                    clean_btn = re.sub(r'<[^>]+>', '', btn).strip()[:80]
                    if clean_btn:
                        print(f"      • {clean_btn}")
                        
            if inputs_hidden:
                print(f"\n   隐藏字段值:")
                for val in inputs_hidden[:5]:
                    print(f"      • {val[:80]}")
            
            # 分析4: 检查是否需要登录/认证
            print(f"\n📋 分析4: 认证需求检查...")
            auth_indicators = [
                ('login', 'login' in html.lower()),
                ('signup', 'sign.up' in html.lower()),
                ('account', 'account' in html.lower()),
                ('authenticate', 'authenticat' in html.lower()),
                ('cookie', 'cookie' in html.lower()),
                ('session', 'session' in html.lower()),
            ]
            
            needs_auth = False
            for name, found in auth_indicators:
                status = "⚠️ 发现" if found else "✅ 未发现"
                print(f"   {status} {name} 相关内容")
                if found and name in ['login', 'signup', 'authenticate']:
                    needs_auth = True
            
            return {
                "success": True,
                "url": url,
                "html_size": len(html),
                "total_links": len(links),
                "download_links": download_links,
                "extracted_urls": list(set(found_urls)),
                "forms": len(forms),
                "buttons": [re.sub(r'<[^>]+>', '', b).strip() for b in buttons],
                "needs_authentication": needs_auth,
                "raw_html_sample": html[:2000]
            }
            
    except Exception as e:
        print(f"\n❌ 错误: {e}")
        import traceback
        traceback.print_exc()
        return {"success": False, "error": str(e)}


def test_download_methods(url: str, output_path: Path):
    """测试多种下载方法"""
    print(f"\n{'='*70}")
    print(f"🧪 测试下载方法: {url[:60]}...")
    print('='*70)
    
    results = []
    
    # 方法1: curl 基础
    print("\n[方法1] curl 基础下载:")
    result1 = subprocess.run(
        ["curl", "-sS", "-L", "-o", str(output_path / "test1.zip"), 
         "--max-time", "20", "-w", "%{http_code}", url],
        capture_output=True, text=True, timeout=25
    )
    code1 = result1.stdout.strip()
    exists1 = (output_path / "test1.zip").exists()
    size1 = (output_path / "test1.zip").stat().st_size if exists1 else 0
    success1 = exists1 and size1 > 1000 and code1 == "200"
    
    print(f"   HTTP: {code1} | 文件存在: {exists1} | 大小: {size1} bytes | 成功: {'✅' if success1 else '❌'}")
    results.append(("curl基础", success1, code1))
    
    # 方法2: curl + 浏览器UA
    print("\n[方法2] curl + 浏览器 User-Agent:")
    result2 = subprocess.run(
        ["curl", "-sS", "-L", "-o", str(output_path / "test2.zip"),
         "--max-time", "20",
         "-A", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36",
         "-H", "Accept: application/zip,*/*",
         "-H", "Referer: https://kenney.nl/",
         "-w", "%{http_code}", url],
        capture_output=True, text=True, timeout=25
    )
    code2 = result2.stdout.strip()
    exists2 = (output_path / "test2.zip").exists()
    size2 = (output_path / "test2.zip").stat().st_size if exists2 else 0
    success2 = exists2 and size2 > 1000 and code2 == "200"
    
    print(f"   HTTP: {code2} | 文件存在: {exists2} | 大小: {size2} bytes | 成功: {'✅' if success2 else '❌'}")
    results.append(("curl+UA", success2, code2))
    
    # 方法3: wget
    print("\n[方法3] wget 下载:")
    result3 = subprocess.run(
        ["wget", "-q", "--timeout=20", "--tries=2",
         "-U", "Mozilla/5.0",
         "-O", str(output_path / "test3.zip"),
         "--content-on-error", url],
        capture_output=True, text=True, timeout=25
    )
    exists3 = (output_path / "test3.zip").exists()
    size3 = (output_path / "test3.zip").stat().st_size if exists3 else 0
    success3 = exists3 and size3 > 1000
    
    print(f"   文件存在: {exists3} | 大小: {size3} bytes | 成功: {'✅' if success3 else '❌'}")
    results.append(("wget", success3, "N/A"))
    
    # 清理测试文件
    for f in output_path.glob("test*.zip"):
        f.unlink(missing_ok=True)
    
    return results


import subprocess

if __name__ == "__main__":
    print("""
╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║   🔬 Kenney.nl 下载机制深度诊断工具                       ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝
""")
    
    test_dir = Path("/tmp/kenney_analysis")
    test_dir.mkdir(exist_ok=True)
    
    # 分析 UI Pack 页面
    ui_pack_result = analyze_kenney_page("https://kenney.nl/assets/ui-pack")
    
    # 保存结果
    report_path = test_dir / "analysis_report.json"
    with open(report_path, 'w') as f:
        json.dump(ui_pack_result, f, indent=2, default=str, ensure_ascii=False)
    
    print(f"\n\n{'='*70}")
    print(f"📊 诊断总结")
    print('='*70)
    
    if ui_pack_result.get("success"):
        print(f"""
✅ 页面分析完成!

关键发现:
  • 页面大小: {ui_pack_result['html_size']} 字符
  • 总链接数: {ui_pack_result['total_links']}
  • 下载相关链接: {len(ui_pack_result['download_links'])}
  • 提取到的URL: {len(ui_pack_result['extracted_urls'])}
  • 表单数量: {ui_pack_result['forms']}
  • 按钮数量: {len(ui_pack_result['buttons'])}

认证需求: {'⚠️ 可能需要' if ui_pack_result.get('needs_authentication') else '✅ 无明显认证要求'}
""")
        
        if ui_pack_result['extracted_urls']:
            print("\n发现的URL:")
            for url in ui_pack_result['extracted_urls'][:5]:
                print(f"  → {url}")
                
        if ui_pack_result['buttons']:
            print("\n页面上的按钮:")
            for btn in ui_pack_result['buttons'][:5]:
                if btn:
                    print(f"  → {btn}")
    
    print(f"\n📝 详细报告已保存: {report_path}")
