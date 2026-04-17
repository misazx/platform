import os
import subprocess
from .base import BasePlatform


class AndroidPlatform(BasePlatform):
    platform_name = "android"
    display_name = "Android"
    output_dir = "android"
    preset_name = "Android"

    def prepare(self) -> bool:
        sdk_root = os.environ.get("ANDROID_SDK_ROOT", "")
        if not sdk_root:
            home = os.path.expanduser("~")
            candidate = os.path.join(home, "Library", "Android", "sdk")
            if os.path.exists(candidate):
                os.environ["ANDROID_SDK_ROOT"] = candidate

        output_path = self.build_dir / self.output_dir
        output_path.mkdir(parents=True, exist_ok=True)
        return True

    def build(self, debug: bool = False) -> bool:
        return self._godot_export(self.preset_name, debug)

    def validate(self) -> bool:
        return self._output_exists("*.apk", "*.aab")

    def post_process(self) -> bool:
        sign_config = self.config.get("signing.android", {})
        keystore_path = sign_config.get("keystore_path", "")
        keystore_alias = sign_config.get("keystore_alias", "")
        keystore_password = sign_config.get("keystore_password", "")

        if not all([keystore_path, keystore_alias, keystore_password]):
            return True

        output_path = self.build_dir / self.output_dir
        apk_files = list(output_path.glob("*.apk"))

        for apk in apk_files:
            if not self._sign_apk(apk, keystore_path, keystore_alias, keystore_password):
                return False

        return True

    def _sign_apk(self, apk_path, keystore: str, alias: str, password: str) -> bool:
        try:
            zipalign = os.path.join(
                os.environ.get("ANDROID_SDK_ROOT", ""),
                "build-tools", os.listdir(
                    os.path.join(os.environ.get("ANDROID_SDK_ROOT", ""), "build-tools")
                )[0], "zipalign"
            ) if os.environ.get("ANDROID_SDK_ROOT") else "zipalign"

            aligned_path = str(apk_path).replace(".apk", "-aligned.apk")
            subprocess.run(
                [zipalign, "-f", "4", str(apk_path), aligned_path],
                capture_output=True, text=True, timeout=60,
            )

            apksigner = os.path.join(
                os.environ.get("ANDROID_SDK_ROOT", ""),
                "build-tools", os.listdir(
                    os.path.join(os.environ.get("ANDROID_SDK_ROOT", ""), "build-tools")
                )[0], "apksigner"
            ) if os.environ.get("ANDROID_SDK_ROOT") else "apksigner"

            result = subprocess.run(
                [
                    apksigner, "sign",
                    "--ks", keystore,
                    "--ks-key-alias", alias,
                    "--ks-pass", f"pass:{password}",
                    "--key-pass", f"pass:{password}",
                    aligned_path,
                ],
                capture_output=True, text=True, timeout=60,
            )
            return result.returncode == 0
        except Exception:
            return False
