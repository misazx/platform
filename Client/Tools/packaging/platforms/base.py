import os
import subprocess
from abc import ABC, abstractmethod
from pathlib import Path

from core.config import BuildConfig
from core.environment import EnvironmentDetector


class BasePlatform(ABC):
    platform_name: str = ""
    display_name: str = ""
    output_dir: str = ""
    preset_name: str = ""

    def __init__(self, config: BuildConfig, env_detector: EnvironmentDetector):
        self.config = config
        self.env_detector = env_detector
        self.build_dir = config.build_dir
        self.client_dir = config.client_dir

    @abstractmethod
    def prepare(self) -> bool:
        pass

    @abstractmethod
    def build(self, debug: bool = False) -> bool:
        pass

    @abstractmethod
    def validate(self) -> bool:
        pass

    @abstractmethod
    def post_process(self) -> bool:
        pass

    def _godot_export(self, preset: str, debug: bool = False) -> bool:
        godot_path = self.config.godot_path
        if not godot_path:
            return False

        output_path = self.build_dir / self.output_dir
        output_path.mkdir(parents=True, exist_ok=True)

        export_flag = "--export-debug" if debug else "--export-release"
        cmd = [
            godot_path,
            "--headless",
            "--path", str(self.client_dir),
            export_flag,
            preset,
        ]

        build_env = self.env_detector.get_build_env()
        try:
            result = subprocess.run(
                cmd,
                cwd=str(self.client_dir),
                capture_output=True, text=True, timeout=600,
                env=build_env,
            )
            if result.stdout:
                for line in result.stdout.strip().split("\n"):
                    if line.strip():
                        pass
            if result.returncode != 0:
                return False
            return True
        except subprocess.TimeoutExpired:
            return False
        except Exception:
            return False

    def _output_exists(self, *patterns: str) -> bool:
        output_path = self.build_dir / self.output_dir
        if not output_path.exists():
            return False
        for pattern in patterns:
            if list(output_path.glob(pattern)):
                return True
        return False
