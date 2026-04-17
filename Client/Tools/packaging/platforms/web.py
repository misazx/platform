from .base import BasePlatform


class WebPlatform(BasePlatform):
    platform_name = "web"
    display_name = "Web (HTML5)"
    output_dir = "web"
    preset_name = "Web"

    def prepare(self) -> bool:
        output_path = self.build_dir / self.output_dir
        output_path.mkdir(parents=True, exist_ok=True)
        return True

    def build(self, debug: bool = False) -> bool:
        return self._godot_export(self.preset_name, debug)

    def validate(self) -> bool:
        output_path = self.build_dir / self.output_dir
        return (output_path / "index.html").exists()

    def post_process(self) -> bool:
        return True
