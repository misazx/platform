#!/usr/bin/env python3
"""
本地测试CDN服务器 - 模拟真实的包下载和热更新环境
使用方法: python3 local_cdn_server.py
访问地址: http://localhost:8080/
支持:
  - /registry.json - 包注册表
  - /packages/<name>.zip - 完整包下载
  - /updates/<package_id>/manifest.json - 热更新清单
  - /updates/<package_id>/files/<path> - 单文件下载
"""

import http.server
import socketserver
import json
import os
import hashlib
import sys
from urllib.parse import urlparse, unquote
from pathlib import Path
from datetime import datetime

PORT = 8080
PACKAGES_DIR = Path(__file__).parent.parent / "test_cdn" / "packages"
UPDATES_DIR = Path(__file__).parent.parent / "test_cdn" / "updates"
GAME_MODES_DIR = Path(__file__).parent.parent / "GameModes"

class CDNHandler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=str(PACKAGES_DIR), **kwargs)

    def do_GET(self):
        self._handle_get()

    def do_HEAD(self):
        self._handle_get(head_only=True)

    def do_POST(self):
        content_length = int(self.headers.get('Content-Length', 0))
        body = self.rfile.read(content_length) if content_length > 0 else b''

        parsed = urlparse(self.path)
        if parsed.path == '/api/generate-manifest':
            self._handle_generate_manifest(body)
        else:
            self.send_error(404, "Not Found")

    def _handle_get(self, head_only=False):
        parsed = urlparse(self.path)
        path = unquote(parsed.path)

        if path in ['/packages/registry.json', '/registry.json']:
            self._send_registry(head_only)
            return

        if path.startswith('/packages/') and path.endswith('.zip'):
            package_name = Path(path).name.replace('.zip', '')
            self._send_package(package_name, head_only)
            return

        if path.endswith('.zip') and not path.startswith('/packages/'):
            package_name = Path(path).name.replace('.zip', '')
            self._send_package(package_name, head_only)
            return

        if '/updates/' in path and '/manifest.json' in path:
            parts = path.split('/updates/')
            if len(parts) >= 2:
                package_id = parts[1].split('/')[0]
                self._send_update_manifest(package_id, head_only)
                return

        if '/updates/' in path and '/files/' in path:
            parts = path.split('/updates/')
            if len(parts) >= 2:
                rest = parts[1]
                pkg_and_file = rest.split('/files/', 1)
                if len(pkg_and_file) >= 2:
                    package_id = pkg_and_file[0]
                    file_path = pkg_and_file[1]
                    self._send_update_file(package_id, file_path, head_only)
                    return

        if path == '/api/list-updates':
            self._list_available_updates(head_only)
            return

        super().do_GET()

    def _send_registry(self, head_only=False):
        registry_path = PACKAGES_DIR / "registry.json"

        if not registry_path.exists():
            self.send_error(404, "Registry not found")
            return

        try:
            with open(registry_path, 'r', encoding='utf-8') as f:
                registry_data = json.load(f)

            self._send_json(registry_data, head_only)
            print(f"  [CDN] Registry served ({len(registry_data.get('packages', []))} packages)")
        except Exception as e:
            self.send_error(500, f"Error reading registry: {str(e)}")

    def _send_package(self, package_id, head_only=False):
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

            print(f"  [CDN] Package served: {package_id}.zip ({file_size / 1024 / 1024:.2f} MB)")
        except Exception as e:
            self.send_error(500, f"Error serving package: {str(e)}")

    def _send_update_manifest(self, package_id, head_only=False):
        manifest_path = UPDATES_DIR / package_id / "manifest.json"

        if not manifest_path.exists():
            self._generate_manifest_on_demand(package_id)
            if not manifest_path.exists():
                self.send_error(404, f"No updates available for package '{package_id}'")
                return

        try:
            with open(manifest_path, 'r', encoding='utf-8') as f:
                manifest_data = json.load(f)

            self._send_json(manifest_data, head_only)
            print(f"  [CDN] Update manifest served for '{package_id}' v{manifest_data.get('version', '?')}")
        except Exception as e:
            self.send_error(500, f"Error reading manifest: {str(e)}")

    def _send_update_file(self, package_id, file_path, head_only=False):
        full_path = UPDATES_DIR / package_id / "files" / file_path

        if not full_path.exists():
            game_modes_path = GAME_MODES_DIR / package_id / file_path
            if game_modes_path.exists():
                full_path = game_modes_path
            else:
                self.send_error(404, f"File not found: {file_path}")
                return

        try:
            file_size = full_path.stat().st_size

            self.send_response(200)
            content_type = self._get_content_type(file_path)
            self.send_header('Content-Type', content_type)
            self.send_header('Content-Length', str(file_size))
            self.send_header('Access-Control-Allow-Origin', '*')
            self.end_headers()

            if not head_only:
                with open(full_path, 'rb') as f:
                    self.wfile.write(f.read())

            print(f"  [CDN] Update file served: {package_id}/{file_path}")
        except Exception as e:
            self.send_error(500, f"Error serving file: {str(e)}")

    def _generate_manifest_on_demand(self, package_id):
        game_mode_dir = GAME_MODES_DIR / package_id
        if not game_mode_dir.exists():
            return

        scripts_dir = game_mode_dir / "Scripts"
        if not scripts_dir.exists():
            return

        UPDATES_DIR.mkdir(parents=True, exist_ok=True)
        pkg_update_dir = UPDATES_DIR / package_id
        pkg_update_dir.mkdir(parents=True, exist_ok=True)

        files_list = []
        total_size = 0

        for gd_file in scripts_dir.rglob("*.gd"):
            rel_path = gd_file.relative_to(game_mode_dir)
            file_hash = self._compute_hash(gd_file)
            file_size = gd_file.stat().st_size
            total_size += file_size

            files_list.append({
                "path": str(rel_path.as_posix()),
                "hash": file_hash,
                "size": file_size,
                "type": "script"
            })

        for json_file in game_mode_dir.rglob("Config/Data/*.json"):
            rel_path = json_file.relative_to(game_mode_dir)
            file_hash = self._compute_hash(json_file)
            file_size = json_file.stat().st_size
            total_size += file_size

            files_list.append({
                "path": str(rel_path.as_posix()),
                "hash": file_hash,
                "size": file_size,
                "type": "config"
            })

        if not files_list:
            return

        registry_path = PACKAGES_DIR / "registry.json"
        current_version = "1.0.0"
        if registry_path.exists():
            try:
                with open(registry_path, 'r', encoding='utf-8') as f:
                    registry = json.load(f)
                for pkg in registry.get("packages", []):
                    if pkg.get("id") == package_id:
                        current_version = pkg.get("version", "1.0.0")
                        break
            except:
                pass

        version_parts = current_version.split(".")
        version_parts[-1] = str(int(version_parts[-1]) + 1)
        new_version = ".".join(version_parts)

        manifest = {
            "packageId": package_id,
            "version": new_version,
            "previousVersion": current_version,
            "description": f"Hot update for {package_id}",
            "releaseDate": datetime.now().isoformat(),
            "total_size": total_size,
            "files_count": len(files_list),
            "download_url": f"/packages/{package_id}.zip",
            "files": files_list,
            "changelog": [
                f"Updated {len(files_list)} files"
            ]
        }

        manifest_path = pkg_update_dir / "manifest.json"
        with open(manifest_path, 'w', encoding='utf-8') as f:
            json.dump(manifest, f, ensure_ascii=False, indent=2)

        print(f"  [CDN] Auto-generated manifest for '{package_id}' v{new_version} ({len(files_list)} files)")

    def _list_available_updates(self, head_only=False):
        updates = {}
        if UPDATES_DIR.exists():
            for pkg_dir in UPDATES_DIR.iterdir():
                if pkg_dir.is_dir():
                    manifest_path = pkg_dir / "manifest.json"
                    if manifest_path.exists():
                        try:
                            with open(manifest_path, 'r', encoding='utf-8') as f:
                                manifest = json.load(f)
                            updates[pkg_dir.name] = {
                                "version": manifest.get("version"),
                                "files_count": manifest.get("files_count", 0),
                                "total_size": manifest.get("total_size", 0)
                            }
                        except:
                            pass

        self._send_json({"updates": updates}, head_only)

    def _handle_generate_manifest(self, body):
        try:
            data = json.loads(body) if body else {}
            package_id = data.get("package_id", "")
            version = data.get("version", "")
            changelog = data.get("changelog", [])

            if not package_id:
                self.send_error(400, "Missing package_id")
                return

            self._generate_manifest_on_demand(package_id)

            manifest_path = UPDATES_DIR / package_id / "manifest.json"
            if not manifest_path.exists():
                self.send_error(404, f"Cannot generate manifest for '{package_id}'")
                return

            if version:
                with open(manifest_path, 'r', encoding='utf-8') as f:
                    manifest = json.load(f)
                manifest["version"] = version
                if changelog:
                    manifest["changelog"] = changelog
                with open(manifest_path, 'w', encoding='utf-8') as f:
                    json.dump(manifest, f, ensure_ascii=False, indent=2)

            self._send_json({"success": True, "package_id": package_id})
            print(f"  [CDN] Manifest generated for '{package_id}'")
        except Exception as e:
            self.send_error(500, f"Error generating manifest: {str(e)}")

    def _send_json(self, data, head_only=False):
        response = json.dumps(data, ensure_ascii=False, indent=2)
        self.send_response(200)
        self.send_header('Content-Type', 'application/json')
        self.send_header('Access-Control-Allow-Origin', '*')
        self.end_headers()
        if not head_only:
            self.wfile.write(response.encode('utf-8'))

    def _compute_hash(self, file_path):
        h = hashlib.sha256()
        with open(file_path, 'rb') as f:
            for chunk in iter(lambda: f.read(8192), b''):
                h.update(chunk)
        return h.hexdigest()

    def _get_content_type(self, file_path):
        ext = Path(file_path).suffix.lower()
        content_types = {
            '.gd': 'text/plain',
            '.json': 'application/json',
            '.tscn': 'text/plain',
            '.tres': 'text/plain',
            '.png': 'image/png',
            '.jpg': 'image/jpeg',
            '.ogg': 'audio/ogg',
            '.wav': 'audio/wav',
            '.zip': 'application/zip',
        }
        return content_types.get(ext, 'application/octet-stream')

    def log_message(self, format, *args):
        pass

def create_test_directory_structure():
    PACKAGES_DIR.mkdir(parents=True, exist_ok=True)
    UPDATES_DIR.mkdir(parents=True, exist_ok=True)

    print(f"\n  CDN Root Directory: {PACKAGES_DIR.absolute()}")
    print(f"  Updates Directory: {UPDATES_DIR.absolute()}")
    print(f"  GameModes Directory: {GAME_MODES_DIR.absolute()}")

def main():
    print("\n" + "=" * 60)
    print("  Local CDN Server (with Hot Update Support)")
    print("=" * 60)

    create_test_directory_structure()

    with socketserver.TCPServer(("", PORT), CDNHandler) as httpd:
        print(f"\n  Server running at:")
        print(f"    Local:    http://localhost:{PORT}")
        print(f"    Registry: http://localhost:{PORT}/registry.json")
        print(f"    Packages: http://localhost:{PORT}/packages/<name>.zip")
        print(f"    Manifest: http://localhost:{PORT}/updates/<pkg>/manifest.json")
        print(f"    Files:    http://localhost:{PORT}/updates/<pkg>/files/<path>")
        print(f"    API:      http://localhost:{PORT}/api/list-updates")
        print(f"\n  Press Ctrl+C to stop")
        print("-" * 60 + "\n")

        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print("\n\n  Server stopped by user")
            print("  Goodbye!\n")

if __name__ == "__main__":
    main()
