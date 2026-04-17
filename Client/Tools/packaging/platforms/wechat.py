import json
import shutil
from pathlib import Path

from .base import BasePlatform


class WeChatPlatform(BasePlatform):
    platform_name = "wechat"
    display_name = "WeChat Mini Game"
    output_dir = "wechat"
    preset_name = "Web"

    def prepare(self) -> bool:
        output_path = self.build_dir / self.output_dir
        output_path.mkdir(parents=True, exist_ok=True)
        return True

    def build(self, debug: bool = False) -> bool:
        web_platform = __import__("platforms.web", fromlist=["WebPlatform"]).WebPlatform(
            self.config, self.env_detector
        )
        if not web_platform.build(debug):
            return False
        return self._convert_to_wechat()

    def validate(self) -> bool:
        output_path = self.build_dir / self.output_dir
        return (output_path / "game.js").exists() and (output_path / "project.config.json").exists()

    def post_process(self) -> bool:
        return True

    def _convert_to_wechat(self) -> bool:
        web_output = self.build_dir / "web"
        wechat_output = self.build_dir / self.output_dir

        if not web_output.exists():
            return False

        if wechat_output.exists():
            shutil.rmtree(wechat_output)
        wechat_output.mkdir(parents=True)

        self._copy_web_files(web_output, wechat_output)
        self._create_game_js(wechat_output)
        self._create_game_json(wechat_output)
        self._create_project_config(wechat_output)
        self._create_adapter(wechat_output)

        return True

    def _copy_web_files(self, src: Path, dst: Path) -> None:
        extensions = {".wasm", ".js", ".pck", ".png", ".json", ".html", ".ico"}
        for item in src.iterdir():
            if item.is_file() and item.suffix.lower() in extensions:
                shutil.copy2(item, dst / item.name)

    def _create_game_js(self, output: Path) -> None:
        content = """require('./weapp-adapter.js');
const canvas = wx.createCanvas();
const systemInfo = wx.getSystemInfoSync();
canvas.width = systemInfo.screenWidth;
canvas.height = systemInfo.screenHeight;

wx.onWindowResize && wx.onWindowResize(function(res) {
    canvas.width = res.size.windowWidth;
    canvas.height = res.size.windowHeight;
});

if (wx.showLoading) {
    wx.showLoading({ title: 'Loading...', mask: true });
}

const Engine = require('./godot.js');
if (Engine) {
    const engine = new Engine({ canvas: canvas });
    engine.startGame().then(function() {
        if (wx.hideLoading) wx.hideLoading();
    }).catch(function(err) {
        console.error('Engine start failed:', err);
    });
}
"""
        (output / "game.js").write_text(content, encoding="utf-8")

    def _create_game_json(self, output: Path) -> None:
        config = {
            "deviceOrientation": "landscape",
            "showStatusBar": False,
            "networkTimeout": {
                "request": 10000,
                "connectSocket": 10000,
                "uploadFile": 10000,
                "downloadFile": 10000,
            },
        }
        (output / "game.json").write_text(json.dumps(config, indent=2), encoding="utf-8")

    def _create_project_config(self, output: Path) -> None:
        appid = self.config.get("platforms.wechat.appid", "")
        config = {
            "description": f"{self.config.project_name} - WeChat Mini Game",
            "setting": {
                "urlCheck": True,
                "es6": True,
                "postcss": True,
                "minified": True,
            },
            "compileType": "game",
            "libVersion": "2.25.0",
            "appid": appid,
            "projectname": self.config.project_name,
            "condition": {},
        }
        (output / "project.config.json").write_text(json.dumps(config, indent=2, ensure_ascii=False), encoding="utf-8")

    def _create_adapter(self, output: Path) -> None:
        adapter_src = self.config.project_root / "build_tools" / "wechat_converter.py"
        if adapter_src.exists():
            pass

        content = """if (typeof window === 'undefined') { var window = global || {}; }
if (typeof document === 'undefined') {
    var document = {
        createElement: function(tag) {
            if (tag === 'canvas') return wx.createCanvas();
            return {};
        },
        getElementById: function(id) { return wx.createCanvas(); },
        addEventListener: function() {},
        body: { appendChild: function() {}, removeChild: function() {} },
        readyState: 'complete',
        hidden: false,
    };
}
if (typeof navigator === 'undefined') {
    var navigator = { userAgent: 'wxgame', platform: 'wxgame', language: 'zh-CN', onLine: true };
}
if (typeof performance === 'undefined') {
    window.performance = { now: function() { return Date.now(); } };
}
if (typeof localStorage === 'undefined' && typeof wx !== 'undefined') {
    window.localStorage = {
        getItem: function(k) { return wx.getStorageSync(k) || null; },
        setItem: function(k, v) { wx.setStorageSync(k, v); },
        removeItem: function(k) { wx.removeStorageSync(k); },
        clear: function() { wx.clearStorageSync(); },
    };
}
if (typeof requestAnimationFrame === 'undefined') {
    window.requestAnimationFrame = function(cb) { return setTimeout(function() { cb(Date.now()); }, 1000/60); };
    window.cancelAnimationFrame = function(id) { clearTimeout(id); };
}
console.log('[WeChat Adapter] Loaded');
"""
        (output / "weapp-adapter.js").write_text(content, encoding="utf-8")
