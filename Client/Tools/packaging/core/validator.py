import os
from pathlib import Path
from typing import Optional


class BuildValidator:
    def validate_output(self, platform: str, output_path: Path) -> dict:
        result = {
            "platform": platform,
            "output_path": str(output_path),
            "exists": False,
            "files": [],
            "total_size_mb": 0.0,
            "valid": False,
            "errors": [],
        }

        if not output_path.exists():
            result["errors"].append(f"Output directory does not exist: {output_path}")
            return result

        result["exists"] = True

        validators = {
            "windows": self._validate_windows,
            "macos": self._validate_macos,
            "android": self._validate_android,
            "ios": self._validate_ios,
            "web": self._validate_web,
            "wechat": self._validate_wechat,
        }

        validator = validators.get(platform)
        if validator:
            result.update(validator(output_path))
        else:
            result["errors"].append(f"Unknown platform: {platform}")

        total_size = 0
        file_list = []
        for f in output_path.rglob("*"):
            if f.is_file():
                size = f.stat().st_size
                total_size += size
                file_list.append({
                    "name": f.name,
                    "path": str(f.relative_to(output_path)),
                    "size_bytes": size,
                })

        result["files"] = file_list
        result["total_size_mb"] = round(total_size / (1024 * 1024), 2)
        result["valid"] = len(result["errors"]) == 0

        return result

    def _validate_windows(self, output_path: Path) -> dict:
        errors = []
        exe_files = list(output_path.glob("*.exe"))
        pck_files = list(output_path.glob("*.pck"))

        if not exe_files:
            errors.append("No .exe file found in output")
        if not pck_files and not any(output_path.rglob("*.pck")):
            errors.append("No .pck file found (may be embedded in .exe)")

        return {"errors": errors}

    def _validate_macos(self, output_path: Path) -> dict:
        errors = []
        app_bundles = list(output_path.glob("*.app"))
        zip_files = list(output_path.glob("*.zip"))

        if not app_bundles and not zip_files:
            errors.append("No .app or .zip file found in output")

        if app_bundles:
            for app in app_bundles:
                info_plist = app / "Contents" / "Info.plist"
                if not info_plist.exists():
                    errors.append(f"Missing Info.plist in {app.name}")

        return {"errors": errors}

    def _validate_android(self, output_path: Path) -> dict:
        errors = []
        apk_files = list(output_path.glob("*.apk"))
        aab_files = list(output_path.glob("*.aab"))

        if not apk_files and not aab_files:
            errors.append("No .apk or .aab file found in output")

        return {"errors": errors}

    def _validate_ios(self, output_path: Path) -> dict:
        errors = []
        ipa_files = list(output_path.glob("*.ipa"))
        xcworkspace = list(output_path.rglob("*.xcworkspace"))
        xcodeproj = list(output_path.rglob("*.xcodeproj"))

        if not ipa_files and not xcworkspace and not xcodeproj:
            errors.append("No .ipa, .xcworkspace or .xcodeproj found in output")

        return {"errors": errors}

    def _validate_web(self, output_path: Path) -> dict:
        errors = []
        index_html = output_path / "index.html"
        wasm_files = list(output_path.glob("*.wasm"))
        js_files = list(output_path.glob("*.js"))

        if not index_html.exists():
            errors.append("No index.html found in output")
        if not wasm_files:
            errors.append("No .wasm file found in output")
        if not js_files:
            errors.append("No .js file found in output")

        return {"errors": errors}

    def _validate_wechat(self, output_path: Path) -> dict:
        errors = []
        game_js = output_path / "game.js"
        game_json = output_path / "game.json"
        project_config = output_path / "project.config.json"

        if not game_js.exists():
            errors.append("No game.js found in output")
        if not game_json.exists():
            errors.append("No game.json found in output")
        if not project_config.exists():
            errors.append("No project.config.json found in output")

        return {"errors": errors}

    def check_export_templates(self, godot_path: str) -> tuple[bool, str]:
        if not godot_path or not os.path.exists(godot_path):
            return False, "Godot path not set"

        template_base = Path.home() / "Library" / "Application Support" / "Godot" / "export_templates"
        if not template_base.exists():
            return False, "Export templates directory not found. Install via Godot Editor -> Manage Export Templates"

        version_dirs = list(template_base.glob("*"))
        if not version_dirs:
            return False, "No export templates installed. Install via Godot Editor -> Manage Export Templates"

        return True, f"Templates found: {', '.join(v.name for v in version_dirs)}"
