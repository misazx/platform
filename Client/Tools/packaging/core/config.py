import json
import os
from pathlib import Path
from typing import Any, Optional


CONFIG_FILENAME = "build_config.json"

DEFAULT_CONFIG = {
    "version": "1.0.0",
    "build_number": 1,
    "project": {
        "name": "Anan",
        "bundle_id": "com.roguelikegame.slaythespire2",
        "godot_path": "",
        "dotnet_root": "",
    },
    "signing": {
        "macos": {
            "identity": "",
            "apple_id": "",
            "team_id": "",
            "app_password": "",
        },
        "ios": {
            "team_id": "",
            "provisioning_profile": "",
            "export_options_plist": "",
        },
        "android": {
            "keystore_path": "",
            "keystore_alias": "",
            "keystore_password": "",
        },
    },
    "platforms": {
        "windows": {"enabled": True, "architecture": "x86_64"},
        "macos": {"enabled": True, "architecture": "universal", "sign": False},
        "android": {"enabled": True, "architectures": ["arm64"], "min_sdk": 21},
        "ios": {"enabled": True, "min_version": "12.0"},
        "web": {"enabled": True, "thread_support": True},
        "wechat": {"enabled": True, "appid": ""},
    },
}


class BuildConfig:
    def __init__(self, config_path: Optional[Path] = None):
        self.script_dir = Path(__file__).parent.parent.resolve()
        self.project_root = self.script_dir.parent.parent
        self.client_dir = self.script_dir.parent
        self.build_dir = self.project_root / "build"

        if config_path is None:
            config_path = self.script_dir / CONFIG_FILENAME

        self.config_path = Path(config_path)
        self._data: dict[str, Any] = {}
        self.load()

    def load(self) -> None:
        if self.config_path.exists():
            with open(self.config_path, "r", encoding="utf-8") as f:
                self._data = json.load(f)
        else:
            self._data = dict(DEFAULT_CONFIG)
            self.save()

    def save(self) -> None:
        with open(self.config_path, "w", encoding="utf-8") as f:
            json.dump(self._data, f, indent=2, ensure_ascii=False)

    def get(self, key_path: str, default: Any = None) -> Any:
        keys = key_path.split(".")
        obj = self._data
        for k in keys:
            if isinstance(obj, dict) and k in obj:
                obj = obj[k]
            else:
                return default
        return obj

    def set(self, key_path: str, value: Any) -> None:
        keys = key_path.split(".")
        obj = self._data
        for k in keys[:-1]:
            if k not in obj or not isinstance(obj[k], dict):
                obj[k] = {}
            obj = obj[k]
        obj[keys[-1]] = value

    @property
    def version(self) -> str:
        return self._data.get("version", "1.0.0")

    @version.setter
    def version(self, val: str) -> None:
        self._data["version"] = val

    @property
    def build_number(self) -> int:
        return self._data.get("build_number", 1)

    @build_number.setter
    def build_number(self, val: int) -> None:
        self._data["build_number"] = val

    def increment_build_number(self) -> int:
        num = self.build_number + 1
        self.build_number = num
        self.save()
        return num

    @property
    def project_name(self) -> str:
        return self.get("project.name", "Anan")

    @property
    def bundle_id(self) -> str:
        return self.get("project.bundle_id", "com.roguelikegame.slaythespire2")

    @property
    def godot_path(self) -> str:
        path = self.get("project.godot_path", "")
        if path and os.path.exists(path):
            return path
        return ""

    @godot_path.setter
    def godot_path(self, val: str) -> None:
        self.set("project.godot_path", val)

    @property
    def dotnet_root(self) -> str:
        root = self.get("project.dotnet_root", "")
        if root and os.path.exists(root):
            return root
        for candidate in ["/usr/local/share/dotnet", "/opt/homebrew/share/dotnet"]:
            if os.path.exists(candidate):
                return candidate
        return ""

    def is_platform_enabled(self, platform: str) -> bool:
        return self.get(f"platforms.{platform}.enabled", False)

    def set_platform_enabled(self, platform: str, enabled: bool) -> None:
        self.set(f"platforms.{platform}.enabled", enabled)

    def get_platform_config(self, platform: str) -> dict[str, Any]:
        return self.get(f"platforms.{platform}", {})

    @property
    def data(self) -> dict[str, Any]:
        return self._data
