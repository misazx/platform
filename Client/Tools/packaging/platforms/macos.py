import shutil
import subprocess
from pathlib import Path

from .base import BasePlatform


class MacOSPlatform(BasePlatform):
    platform_name = "macos"
    display_name = "macOS"
    output_dir = "macOS"
    preset_name = "macOS"

    def prepare(self) -> bool:
        output_path = self.build_dir / self.output_dir
        output_path.mkdir(parents=True, exist_ok=True)
        return True

    def build(self, debug: bool = False) -> bool:
        return self._godot_export(self.preset_name, debug)

    def validate(self) -> bool:
        return self._output_exists("*.app", "*.zip")

    def post_process(self) -> bool:
        sign_config = self.config.get("signing.macos", {})
        should_sign = self.config.get("platforms.macos.sign", False)

        if not should_sign:
            return True

        identity = sign_config.get("identity", "")
        if not identity:
            return True

        output_path = self.build_dir / self.output_dir
        app_bundles = list(output_path.glob("*.app"))

        for app in app_bundles:
            if not self._codesign(app, identity):
                return False
            if not self._notarize(app, sign_config):
                return False
            self._staple(app)

        return True

    def _codesign(self, app_path: Path, identity: str) -> bool:
        try:
            result = subprocess.run(
                ["codesign", "--deep", "--force", "--sign", identity, str(app_path)],
                capture_output=True, text=True, timeout=120,
            )
            return result.returncode == 0
        except Exception:
            return False

    def _notarize(self, app_path: Path, sign_config: dict) -> bool:
        apple_id = sign_config.get("apple_id", "")
        team_id = sign_config.get("team_id", "")
        app_password = sign_config.get("app_password", "")

        if not all([apple_id, team_id, app_password]):
            return True

        zip_path = self.build_dir / self.output_dir / f"{app_path.stem}.zip"
        try:
            subprocess.run(
                ["ditto", "-c", "-k", "--keepParent", str(app_path), str(zip_path)],
                capture_output=True, text=True, timeout=60,
            )

            result = subprocess.run(
                [
                    "xcrun", "notarytool", "submit", str(zip_path),
                    "--apple-id", apple_id,
                    "--team-id", team_id,
                    "--password", app_password,
                    "--wait",
                ],
                capture_output=True, text=True, timeout=1800,
            )
            return result.returncode == 0
        except Exception:
            return False

    def _staple(self, app_path: Path) -> bool:
        try:
            result = subprocess.run(
                ["xcrun", "stapler", "staple", str(app_path)],
                capture_output=True, text=True, timeout=60,
            )
            return result.returncode == 0
        except Exception:
            return False
