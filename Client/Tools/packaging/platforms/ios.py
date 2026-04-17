import os
import subprocess
from pathlib import Path

from .base import BasePlatform


class IOSPlatform(BasePlatform):
    platform_name = "ios"
    display_name = "iOS"
    output_dir = "ios"
    preset_name = "iOS"

    def prepare(self) -> bool:
        if not shutil_which("xcodebuild"):
            return False
        output_path = self.build_dir / self.output_dir
        output_path.mkdir(parents=True, exist_ok=True)
        return True

    def build(self, debug: bool = False) -> bool:
        if not self._godot_export(self.preset_name, debug):
            return False
        return self._xcode_build(debug)

    def validate(self) -> bool:
        return self._output_exists("*.ipa") or self._output_exists("*.xcworkspace") or self._output_exists("*.xcodeproj")

    def post_process(self) -> bool:
        return True

    def _xcode_build(self, debug: bool = False) -> bool:
        ios_output = self.build_dir / self.output_dir

        xcworkspace = list(ios_output.rglob("*.xcworkspace"))
        xcodeproj = list(ios_output.rglob("*.xcodeproj"))

        if not xcworkspace and not xcodeproj:
            return True

        project_path = str(xcworkspace[0]) if xcworkspace else str(xcodeproj[0])
        scheme = self._find_scheme(project_path)

        if not scheme:
            return False

        config = "Debug" if debug else "Release"
        archive_path = self.build_dir / self.output_dir / f"{self.config.project_name}.xcarchive"

        archive_result = subprocess.run(
            [
                "xcodebuild",
                "-workspace" if xcworkspace else "-project", project_path,
                "-scheme", scheme,
                "-configuration", config,
                "-destination", "generic/platform=iOS",
                "-archivePath", str(archive_path),
                "archive",
                "CODE_SIGN_IDENTITY=",
                "CODE_SIGNING_REQUIRED=NO",
                "CODE_SIGNING_ALLOWED=NO",
            ],
            capture_output=True, text=True, timeout=1800,
        )

        if archive_result.returncode != 0:
            return False

        sign_config = self.config.get("signing.ios", {})
        team_id = sign_config.get("team_id", "")

        if team_id:
            return self._export_ipa(archive_path, sign_config)

        return True

    def _find_scheme(self, project_path: str) -> str:
        try:
            result = subprocess.run(
                ["xcodebuild", "-list", "-workspace" if ".xcworkspace" in project_path else "-project", project_path],
                capture_output=True, text=True, timeout=30,
            )
            for line in result.stdout.split("\n"):
                line = line.strip()
                if line and not line.startswith("Schemes:") and not line.startswith("Targets:") and not line.startswith("Project:"):
                    if line and not any(c in line for c in [":", "Information"]):
                        return line
        except Exception:
            pass
        return self.config.project_name

    def _export_ipa(self, archive_path: Path, sign_config: dict) -> bool:
        export_dir = self.build_dir / self.output_dir / "output"
        export_dir.mkdir(parents=True, exist_ok=True)

        plist_content = f"""<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>method</key>
    <string>app-store</string>
    <key>teamID</key>
    <string>{sign_config.get('team_id', '')}</string>
    <key>uploadSymbols</key>
    <true/>
</dict>
</plist>"""

        plist_path = self.build_dir / self.output_dir / "ExportOptions.plist"
        with open(plist_path, "w") as f:
            f.write(plist_content)

        try:
            result = subprocess.run(
                [
                    "xcodebuild",
                    "-exportArchive",
                    "-archivePath", str(archive_path),
                    "-exportPath", str(export_dir),
                    "-exportOptionsPlist", str(plist_path),
                ],
                capture_output=True, text=True, timeout=600,
            )
            return result.returncode == 0
        except Exception:
            return False


def shutil_which(cmd: str) -> str:
    import shutil
    return shutil.which(cmd) or ""
