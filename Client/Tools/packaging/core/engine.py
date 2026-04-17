import os
import subprocess
from datetime import datetime
from pathlib import Path
from typing import Callable, Optional

from .config import BuildConfig
from .environment import EnvironmentDetector
from .reporter import BuildReporter
from .validator import BuildValidator

PLATFORM_MAP: dict[str, type] = {}


class BuildEngine:
    def __init__(self, config: Optional[BuildConfig] = None):
        self.config = config or BuildConfig()
        self.env_detector = EnvironmentDetector()
        self.validator = BuildValidator()
        self.reporter = BuildReporter(self.config.build_dir)
        self._is_building = False
        self._should_stop = False
        self._log_callback: Optional[Callable[[str, str], None]] = None
        self._progress_callback: Optional[Callable[[int, str], None]] = None
        self._init_platform_map()

    def _init_platform_map(self) -> None:
        from platforms.windows import WindowsPlatform
        from platforms.macos import MacOSPlatform
        from platforms.android import AndroidPlatform
        from platforms.ios import IOSPlatform
        from platforms.web import WebPlatform
        from platforms.wechat import WeChatPlatform
        global PLATFORM_MAP
        PLATFORM_MAP = {
            "windows": WindowsPlatform,
            "macos": MacOSPlatform,
            "android": AndroidPlatform,
            "ios": IOSPlatform,
            "web": WebPlatform,
            "wechat": WeChatPlatform,
        }

    def set_log_callback(self, callback: Callable[[str, str], None]) -> None:
        self._log_callback = callback

    def set_progress_callback(self, callback: Callable[[int, str], None]) -> None:
        self._progress_callback = callback

    def _log(self, msg: str, level: str = "info") -> None:
        if self._log_callback:
            self._log_callback(msg, level)

    def _progress(self, pct: int, status: str = "") -> None:
        if self._progress_callback:
            self._progress_callback(pct, status)

    def detect_environment(self) -> list:
        return self.env_detector.detect_all()

    def get_platform(self, name: str):
        cls = PLATFORM_MAP.get(name)
        if cls:
            return cls(self.config, self.env_detector)
        return None

    def get_enabled_platforms(self) -> list[str]:
        return [p for p in PLATFORM_MAP if self.config.is_platform_enabled(p)]

    def build_dotnet(self) -> bool:
        self._log("[DOTNET] Building C# project...", "info")
        dotnet_root = self.env_detector.get_dotnet_root()
        dotnet_cmd = os.path.join(dotnet_root, "dotnet") if dotnet_root else "dotnet"
        csproj = self.config.client_dir / "RoguelikeGame.csproj"
        if not csproj.exists():
            self._log(f"[DOTNET] ERROR: .csproj not found: {csproj}", "err")
            return False
        build_env = self.env_detector.get_build_env()
        try:
            result = subprocess.run(
                [dotnet_cmd, "build", str(csproj), "-c", "Release"],
                capture_output=True, text=True, timeout=300,
                env=build_env, cwd=str(self.config.client_dir),
            )
            if result.stdout:
                for line in result.stdout.strip().split("\n"):
                    if line.strip():
                        self._log(f"  {line.strip()}", "info")
            if result.stderr:
                for line in result.stderr.strip().split("\n"):
                    if line.strip():
                        self._log(f"  {line.strip()}", "warn")
            if result.returncode == 0:
                self._log("[DOTNET] Build succeeded", "ok")
                return True
            else:
                self._log(f"[DOTNET] Build failed (code={result.returncode})", "err")
                return False
        except FileNotFoundError:
            self._log("[DOTNET] ERROR: dotnet command not found", "err")
            return False
        except subprocess.TimeoutExpired:
            self._log("[DOTNET] ERROR: Build timed out (300s)", "err")
            return False
        except Exception as e:
            self._log(f"[DOTNET] ERROR: {e}", "err")
            return False

    def build_platforms(self, platforms: Optional[list[str]] = None, debug: bool = False) -> dict:
        if platforms is None:
            platforms = self.get_enabled_platforms()
        if not platforms:
            self._log("[ENGINE] No platforms selected", "warn")
            return {}
        self._is_building = True
        self._should_stop = False
        self.reporter.start()
        self._log(f"[ENGINE] Starting build for {len(platforms)} platform(s)", "info")
        self._log(f"[ENGINE] Version: {self.config.version} Build: {self.config.build_number}", "info")
        if not self.build_dotnet():
            self._log("[ENGINE] C# build failed, aborting", "err")
            self.reporter.finish()
            return {p: {"status": "failed", "error": "C# build failed"} for p in platforms}
        total = len(platforms)
        results = {}
        for i, plat_name in enumerate(platforms):
            if self._should_stop:
                self._log("[ENGINE] Build stopped by user", "warn")
                break
            pct = int((i / total) * 100)
            self._progress(pct, f"Building {plat_name}...")
            platform = self.get_platform(plat_name)
            if not platform:
                self._log(f"[ENGINE] Unknown platform: {plat_name}", "err")
                results[plat_name] = {"status": "failed", "error": "Unknown platform"}
                continue
            self._log(f"\n{'='*50}", "hdr")
            self._log(f"[{i+1}/{total}] Building: {plat_name}", "hdr")
            t0 = datetime.now()
            success = self._build_single_platform(platform, debug)
            dt = (datetime.now() - t0).total_seconds()
            if success:
                self._log(f"[{i+1}/{total}] {plat_name} OK ({dt:.1f}s)", "ok")
                validation = self.validator.validate_output(
                    plat_name, self.config.build_dir / platform.output_dir
                )
                self.reporter.add_platform_result(
                    plat_name, "success",
                    output=str(self.config.build_dir / platform.output_dir),
                    size_mb=validation.get("total_size_mb", 0),
                    duration_seconds=dt,
                )
                results[plat_name] = {"status": "success", "duration": dt}
            else:
                self._log(f"[{i+1}/{total}] {plat_name} FAILED ({dt:.1f}s)", "err")
                self.reporter.add_platform_result(
                    plat_name, "failed", duration_seconds=dt, error="Build failed",
                )
                results[plat_name] = {"status": "failed", "duration": dt}
            pct = int(((i + 1) / total) * 100)
            self._progress(pct, f"Completed {plat_name}")
        self._is_building = False
        self._progress(100, "Done")
        self.reporter.finish()
        self.config.increment_build_number()
        self._log(self.reporter.get_summary(), "hdr")
        return results

    def _build_single_platform(self, platform, debug: bool = False) -> bool:
        steps = [
            ("Prepare", platform.prepare),
            ("Build", lambda: platform.build(debug=debug)),
            ("Validate", platform.validate),
            ("Post-process", platform.post_process),
        ]
        for step_name, step_fn in steps:
            if self._should_stop:
                return False
            self._log(f"  [{step_name}]...", "info")
            try:
                if not step_fn():
                    self._log(f"  [{step_name}] FAILED", "err")
                    return False
                self._log(f"  [{step_name}] OK", "ok")
            except Exception as e:
                self._log(f"  [{step_name}] ERROR: {e}", "err")
                return False
        return True

    def stop(self) -> None:
        self._should_stop = True
        self._is_building = False

    @property
    def is_building(self) -> bool:
        return self._is_building
