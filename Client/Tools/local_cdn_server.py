#!/usr/bin/env python3
"""
本地测试CDN服务器 - 模拟真实的包下载环境
使用方法: python3 local_cdn_server.py
访问地址: http://localhost:8080/packages/
"""

import http.server
import socketserver
import json
import os
import sys
from urllib.parse import urlparse, parse_qs
from pathlib import Path

PORT = 8080
PACKAGES_DIR = Path(__file__).parent.parent / "test_cdn" / "packages"

class CDNHandler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=str(PACKAGES_DIR), **kwargs)
    
    def do_GET(self):
        self._handle_request()
    
    def do_HEAD(self):
        self._handle_request(head_only=True)
    
    def _handle_request(self, head_only=False):
        parsed = urlparse(self.path)
        
        # 处理 registry.json 请求
        if parsed.path == '/packages/registry.json' or parsed.path == '/registry.json':
            self.send_registry()
            return
        
        # 处理包下载请求
        if parsed.path.startswith('/packages/') and parsed.path.endswith('.zip'):
            package_name = Path(parsed.path).name  # 获取完整文件名
            self.send_package(package_name.replace('.zip', ''))  # 移除.zip后缀
            return
        
        # 也支持直接访问 /<package_name>.zip
        if parsed.path.endswith('.zip') and not parsed.path.startswith('/packages/'):
            package_name = Path(parsed.path).name
            self.send_package(package_name.replace('.zip', ''))
            return
        
        # 默认处理（静态文件）
        super().do_GET()
    
    def send_registry(self):
        """发送包注册表"""
        registry_path = PACKAGES_DIR / "registry.json"
        
        if not registry_path.exists():
            self.send_error(404, "Registry not found")
            return
        
        try:
            with open(registry_path, 'r', encoding='utf-8') as f:
                registry_data = json.load(f)
            
            self.send_response(200)
            self.send_header('Content-Type', 'application/json')
            self.send_header('Access-Control-Allow-Origin', '*')
            self.end_headers()
            
            response = json.dumps(registry_data, ensure_ascii=False, indent=2)
            self.wfile.write(response.encode('utf-8'))
            
            print(f"✅ [CDN] Registry served ({len(registry_data.get('packages', []))} packages)")
        except Exception as e:
            self.send_error(500, f"Error reading registry: {str(e)}")
    
    def send_package(self, package_id, head_only=False):
        """发送包文件"""
        zip_path = PACKAGES_DIR / f"{package_id}.zip"
        
        if not zip_path.exists():
            self.send_error(404, f"Package '{package_id}' not found. Run build_packages.py first!")
            return
        
        try:
            file_size = zip_path.stat().st_size
            
            self.send_response(200)
            self.send_header('Content-Type', 'application/zip')
            self.send_header('Content-Disposition', f'attachment; filename="{package_id}.zip"')
            self.send_header('Content-Length', str(file_size))
            self.send_header('Access-Control-Allow-Origin', '*')
            self.end_headers()
            
            if not head_only:
                with open(zip_path, 'rb') as f:
                    self.wfile.write(f.read())
            
            print(f"✅ [CDN] Package served: {package_id}.zip ({file_size / 1024 / 1024:.2f} MB)")
        except Exception as e:
            self.send_error(500, f"Error serving package: {str(e)}")
    
    def log_message(self, format, *args):
        """自定义日志格式"""
        print(f"📡 [CDN] {args[0]}")

def create_test_directory_structure():
    """创建测试目录结构"""
    PACKAGES_DIR.mkdir(parents=True, exist_ok=True)
    
    print(f"\n📁 CDN Root Directory: {PACKAGES_DIR.absolute()}")
    print("📦 Place your .zip packages here for testing\n")

def main():
    print("\n" + "="*60)
    print("🚀 本地测试 CDN 服务器")
    print("="*60)
    
    create_test_directory_structure()
    
    with socketserver.TCPServer(("", PORT), CDNHandler) as httpd:
        print(f"\n✨ Server running at:")
        print(f"   🌐 Local:   http://localhost:{PORT}")
        print(f"   📋 Registry: http://localhost:{PORT}/registry.json")
        print(f"   📦 Packages: http://localhost:{PORT}/packages/<name>.zip")
        print(f"\n⏹️  Press Ctrl+C to stop")
        print("-"*60 + "\n")
        
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print("\n\n⏹️  Server stopped by user")
            print("👋 Goodbye!\n")

if __name__ == "__main__":
    main()
