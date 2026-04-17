from .base import BasePlatform


class WindowsPlatform(BasePlatform):
    platform_name = "windows"
    display_name = "Windows Desktop"
    output_dir = "windows"
    preset_name = "Windows Desktop"

    def prepare(self) -> bool:
        output_path = self.build_dir / self.output_dir
        output_path.mkdir(parents=True, exist_ok=True)
        return True

    def build(self, debug: bool = False) -> bool:
        return self._godot_export(self.preset_name, debug)

    def validate(self) -> bool:
        return self._output_exists("*.exe")

    def post_process(self) -> bool:
        return True
