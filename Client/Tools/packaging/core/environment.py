import os
import shutil
import subprocess
from dataclasses import dataclass, field
from pathlib import Path
from typing import Optional


@dataclass
class EnvCheckResult:
    name: str
    installed: bool = False
    version: str = ""
    path: str = ""
    message: str = ""
    required_for: list[str] = field(default_factory=list)


class EnvironmentDetector:
    def __init__(self):
        self.results: list[EnvCheckResult] = []
        self._dotnet_root: Optional[str] = None

    def detect_all(self) -> list[EnvCheckResult]:
        self.results = [
            self._check_godot(),
            self._check_dotnet(),
            self._check_xcode(),
            self._check_android_sdk(),
            self._check_java(),
            self._check_nodejs(),
            self._check_wechat_devtools(),
            self._check_signing_certificates(),
        ]
        return self.results

    def get_dotnet_root(self) -> str:
        if self._dotnet_root:
            return self._dotnet_root
        for p in ["/usr/local/share/dotnet", "/opt/homebrew/share/dotnet"]:
            if os.path.exists(p):
                self._dotnet_root = p
                return p
        return ""

    def get_build_env(self) -> dict[str, str]:
        env = dict(os.environ)
        dotnet_root = self.get_dotnet_root()
        if dotnet_root:
            env["DOTNET_ROOT"] = dotnet_root
            env["DOTNET_ROOT_ARM64"] = dotnet_root
            if dotnet_root not in env.get("PATH", ""):
                env["PATH"] = f"{dotnet_root}:{env.get('PATH', '')}"
        env["GODOT_DISABLE_CONSOLE"] = "1"
        return env

    def _check_godot(self) -> EnvCheckResult:
        result = EnvCheckResult(
            name="Godot Engine",
            required_for=["windows", "macos", "android", "ios", "web", "wechat"],
        )

        search_paths = [
            "/Applications/Godot.app/Contents/MacOS/Godot",
            "/Applications/Godot_mono.app/Contents/MacOS/Godot",
        ]

        home = Path.home()
        for pattern_dir in [home / "Downloads", home / "Applications"]:
            if pattern_dir.exists():
                for app in pattern_dir.glob("Godot*.app"):
                    exe = app / "Contents" / "MacOS" / "Godot"
                    if exe.exists():
                        search_paths.append(str(exe))

        cmd_path = shutil.which("godot")
        if cmd_path:
            search_paths.append(cmd_path)

        for path in search_paths:
            if os.path.exists(path):
                result.installed = True
                result.path = path
                try:
                    r = subprocess.run(
                        [path, "--headless", "--version"],
                        capture_output=True, text=True, timeout=10,
                        env=self.get_build_env(),
                    )
                    if r.stdout.strip():
                        result.version = r.stdout.strip()
                except Exception:
                    result.version = "found"
                break

        if not result.installed:
            result.message = "Download from https://godotengine.org/download"
        return result

    def _check_dotnet(self) -> EnvCheckResult:
        result = EnvCheckResult(
            name=".NET SDK",
            required_for=["windows", "macos", "android", "ios", "web", "wechat"],
        )

        dotnet_root = self.get_dotnet_root()
        dotnet_cmd = os.path.join(dotnet_root, "dotnet") if dotnet_root else shutil.which("dotnet")

        if dotnet_cmd and os.path.exists(dotnet_cmd):
            result.installed = True
            result.path = dotnet_cmd
            try:
                r = subprocess.run(
                    [dotnet_cmd, "--version"],
                    capture_output=True, text=True, timeout=10,
                )
                result.version = r.stdout.strip()
            except Exception:
                result.version = "found"
        else:
            result.message = "Install: brew install dotnet or download from https://dotnet.microsoft.com"

        return result

    def _check_xcode(self) -> EnvCheckResult:
        result = EnvCheckResult(
            name="Xcode",
            required_for=["ios", "macos"],
        )

        if shutil.which("xcodebuild"):
            result.installed = True
            result.path = shutil.which("xcodebuild")
            try:
                r = subprocess.run(
                    ["xcodebuild", "-version"],
                    capture_output=True, text=True, timeout=10,
                )
                first_line = r.stdout.strip().split("\n")[0] if r.stdout else ""
                result.version = first_line
            except Exception:
                result.version = "found"
        else:
            result.message = "Install from Mac App Store"

        return result

    def _check_android_sdk(self) -> EnvCheckResult:
        result = EnvCheckResult(
            name="Android SDK",
            required_for=["android"],
        )

        sdk_root = os.environ.get("ANDROID_SDK_ROOT", "")
        if not sdk_root:
            home = Path.home()
            candidates = [
                home / "Library" / "Android" / "sdk",
                Path("/usr/local/share/android-sdk"),
            ]
            for c in candidates:
                if c.exists():
                    sdk_root = str(c)
                    break

        if sdk_root and os.path.exists(sdk_root):
            result.installed = True
            result.path = sdk_root
            result.version = "found"
        else:
            result.message = "Install Android Studio or set ANDROID_SDK_ROOT"

        return result

    def _check_java(self) -> EnvCheckResult:
        result = EnvCheckResult(
            name="Java JDK",
            required_for=["android"],
        )

        java_cmd = shutil.which("java")
        if java_cmd:
            result.installed = True
            result.path = java_cmd
            try:
                r = subprocess.run(
                    ["java", "-version"],
                    capture_output=True, text=True, timeout=10,
                )
                output = r.stderr or r.stdout
                first_line = output.strip().split("\n")[0] if output else ""
                result.version = first_line
            except Exception:
                result.version = "found"
        else:
            result.message = "Install: brew install openjdk@17"

        return result

    def _check_nodejs(self) -> EnvCheckResult:
        result = EnvCheckResult(
            name="Node.js",
            required_for=["wechat"],
        )

        node_cmd = shutil.which("node")
        if node_cmd:
            result.installed = True
            result.path = node_cmd
            try:
                r = subprocess.run(
                    ["node", "--version"],
                    capture_output=True, text=True, timeout=10,
                )
                result.version = r.stdout.strip()
            except Exception:
                result.version = "found"
        else:
            result.message = "Install: brew install node"

        return result

    def _check_wechat_devtools(self) -> EnvCheckResult:
        result = EnvCheckResult(
            name="WeChat DevTools",
            required_for=["wechat"],
        )

        app_path = "/Applications/wechatwebdevtools.app"
        if os.path.exists(app_path):
            result.installed = True
            result.path = app_path
            result.version = "found"
        else:
            result.message = "Download from https://developers.weixin.qq.com/miniprogram/dev/devtools/download.html"

        return result

    def _check_signing_certificates(self) -> EnvCheckResult:
        result = EnvCheckResult(
            name="Signing Certificates",
            required_for=["ios", "macos"],
        )

        try:
            r = subprocess.run(
                ["security", "find-identity", "-v", "-p", "codesigning"],
                capture_output=True, text=True, timeout=10,
            )
            lines = [l.strip() for l in r.stdout.strip().split("\n") if "Developer ID" in l or "Apple Development" in l]
            if lines:
                result.installed = True
                result.version = f"{len(lines)} certificate(s)"
            else:
                result.message = "No Developer ID certificates found. Register at https://developer.apple.com"
        except Exception:
            result.message = "Cannot check certificates"

        return result
