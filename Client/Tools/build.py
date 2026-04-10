#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Godot C# 项目一键打包工具
支持平台：Windows, macOS, Android, iOS, Web, 微信小游戏

使用方法：
  python build.py --platform windows      # 打包 Windows 版本
  python build.py --platform mac          # 打包 macOS 版本
  python build.py --platform android      # 打包 Android APK
  python build.py --platform ios          # 打包 iOS (需要 Mac)
  python build.py --platform web          # 打包 Web 版本
  python build.py --platform wechat       # 打包微信小游戏
  python build.py --all                   # 打包所有平台
  python build.py --list                  # 列出可用平台
"""

import os
import sys
import subprocess
import shutil
import argparse
from datetime import datetime
from pathlib import Path


class BuildTool:
    def __init__(self):
        self.script_dir = Path(__file__).parent.resolve()
        self.project_root = self.script_dir.parent
        self.build_dir = self.project_root / "build"
        # 延迟初始化 godot_executable，避免启动时立即失败
        self.godot_executable = None
        self.platforms = {
            "windows": {
                "preset": "Windows Desktop",
                "output_dir": "windows",
                "description": "Windows 桌面版 (.exe)"
            },
            "mac": {
                "preset": "macOS",
                "output_dir": "macOS",
                "description": "macOS 桌面版 (.app/.zip)"
            },
            "android": {
                "preset": "Android",
                "output_dir": "android",
                "description": "Android APK"
            },
            "ios": {
                "preset": "iOS",
                "output_dir": "ios",
                "description": "iOS (需要 Mac + Xcode)"
            },
            "web": {
                "preset": "Web",
                "output_dir": "web",
                "description": "Web 版本 (HTML5/WASM)"
            },
            "wechat": {
                "preset": "Web",
                "output_dir": "wechat",
                "description": "微信小游戏",
                "special": True
            }
        }

    def _find_godot_executable(self):
        """查找 Godot 可执行文件"""
        possible_paths = [
            "/Applications/Godot.app/Contents/MacOS/Godot",
            "/usr/local/bin/godot",
            "godot",
            "godot4"
        ]

        for path in possible_paths:
            if shutil.which(path) or os.path.exists(path):
                return path

        # 尝试查找系统中的 Godot
        if sys.platform == "darwin":
            app_path = Path("/Applications")
            for app in app_path.glob("Godot*.app"):
                executable = app / "Contents" / "MacOS" / "Godot"
                if executable.exists():
                    return str(executable)

        raise FileNotFoundError(
            "未找到 Godot 可执行文件。\n"
            "请确保已安装 Godot 并添加到 PATH，或设置 GODOT_PATH 环境变量。\n"
            "下载地址: https://godotengine.org/download"
        )

    def list_platforms(self):
        """列出所有可用平台"""
        print("\n📋 可用打包平台:\n")
        print(f"{'平台':<12} {'描述':<30} {'预设名称'}")
        print("-" * 70)
        for platform, config in self.platforms.items():
            preset = config["preset"]
            if config.get("special"):
                preset += " (特殊处理)"
            print(f"{platform:<12} {config['description']:<30} {preset}")
        print("\n使用示例:")
        print("  python build.py --platform windows")
        print("  python build.py --all")

    def check_prerequisites(self, platform):
        """检查平台特定的前置条件"""
        issues = []

        if platform == "android":
            if not os.environ.get("ANDROID_SDK_ROOT"):
                issues.append("❌ 未设置 ANDROID_SDK_ROOT 环境变量")
            if not os.environ.get("ANDROID_NDK_ROOT"):
                issues.append("⚠️  建议设置 ANDROID_NDK_ROOT 环境变量")

        elif platform == "ios":
            if sys.platform != "darwin":
                issues.append("❌ iOS 打包需要 macOS 系统")
            else:
                if not shutil.which("xcodebuild"):
                    issues.append("❌ 未安装 Xcode 或 xcodebuild 不在 PATH 中")

        elif platform == "windows":
            if sys.platform == "darwin":
                print("⚠️  在 macOS 上交叉编译 Windows 版本需要额外配置")

        elif platform == "wechat":
            if not shutil.which("node") and not shutil.where("node"):
                issues.append("❌ 微信小游戏转换需要 Node.js")

        return issues

    def prepare_build_directory(self, output_dir):
        """准备构建输出目录"""
        full_path = self.build_dir / output_dir
        full_path.mkdir(parents=True, exist_ok=True)
        return full_path

    def export_platform(self, platform_name, debug=False):
        """导出指定平台"""
        if platform_name not in self.platforms:
            print(f"❌ 未知平台: {platform_name}")
            print("使用 --list 查看可用平台")
            return False

        platform_config = self.platforms[platform_name]
        preset_name = platform_config["preset"]
        output_dir = platform_config["output_dir"]

        print(f"\n{'='*60}")
        print(f"🚀 开始打包: {platform_name.upper()}")
        print(f"   平台: {platform_config['description']}")
        print(f"   预设: {preset_name}")
        print(f"{'='*60}\n")

        # 检查前置条件
        issues = self.check_prerequisites(platform_name)
        if issues:
            print("⚠️  前置条件检查:\n")
            for issue in issues:
                print(f"  {issue}")
            response = input("\n是否继续? (y/N): ")
            if response.lower() != 'y':
                print("❌ 已取消")
                return False

        # 准备输出目录
        try:
            self.prepare_build_directory(output_dir)
        except Exception as e:
            print(f"❌ 创建输出目录失败: {e}")
            return False

        # 特殊处理：微信小游戏
        if platform_config.get("special"):
            return self._export_wechat_mini_game(output_dir, debug)

        # 标准 Godot 导出
        try:
            # 确保 godot_executable 已设置
            if not self.godot_executable:
                print("⚠️  未指定 Godot 路径，尝试自动查找...")
                self.godot_executable = self._find_godot_executable()

            cmd = [
                self.godot_executable,
                "--headless",
                "--path", str(self.project_root),
                "--export-release" if not debug else "--export-debug",
                preset_name  # 移除多余的引号
            ]

            print(f"📦 执行命令:")
            print(f"   {' '.join(cmd)}\n")

            # 使用列表形式执行，避免 shell=True 的潜在问题
            result = subprocess.run(
                cmd,
                cwd=str(self.project_root),
                capture_output=True,
                text=True,
                timeout=600  # 添加超时
            )

            if result.returncode == 0:
                output_path = self.build_dir / output_dir
                print(f"\n✅ 打包成功!")
                print(f"   输出路径: {output_path}")

                # 显示输出文件信息
                self._show_output_info(output_path, platform_name)
                return True
            else:
                print(f"\n❌ 打包失败!")
                print(f"   错误代码: {result.returncode}")
                if result.stdout:
                    print(f"   标准输出:\n{result.stdout}")
                if result.stderr:
                    print(f"   标准错误:\n{result.stderr}")
                if not result.stdout and not result.stderr:
                    print(f"   ⚠️  无错误输出，可能 Godot 命令执行失败")
                    print(f"   请检查 Godot 路径是否正确: {self.godot_executable}")
                return False

        except subprocess.TimeoutExpired:
            print(f"❌ 执行超时 (600秒)")
            return False
        except FileNotFoundError as e:
            print(f"❌ 找不到文件: {e}")
            print(f"   请检查 Godot 可执行文件是否存在: {self.godot_executable}")
            return False

        except Exception as e:
            print(f"❌ 执行错误: {e}")
            return False

    def _export_wechat_mini_game(self, output_dir, debug=False):
        """导出微信小游戏"""
        print("📱 微信小游戏打包流程:\n")

        # 步骤1: 先导出 Web 版本
        print("[1/3] 导出 Web 版本...")
        web_success = self.export_platform("web", debug)

        if not web_success:
            print("❌ Web 导出失败，无法继续转换微信小游戏")
            return False

        # 步骤2: 转换为微信小游戏格式
        print("\n[2/3] 转换为微信小游戏格式...")
        try:
            converter_script = self.script_dir.parent.parent / "build_tools" / "wechat_converter.py"
            if converter_script.exists():
                result = subprocess.run([
                    sys.executable,
                    str(converter_script),
                    "--input", str(self.build_dir / "web"),
                    "--output", str(self.build_dir / output_dir)
                ], capture_output=True, text=True)

                if result.returncode != 0:
                    print(f"❌ 转换失败:\n{result.stderr}")
                    return False
            else:
                print("⚠️  未找到微信转换脚本，仅复制 Web 文件")
                web_output = self.build_dir / "web"
                wechat_output = self.build_dir / output_dir
                if wechat_output.exists():
                    shutil.rmtree(wechat_output)
                shutil.copytree(web_output, wechat_output)

        except Exception as e:
            print(f"❌ 转换过程出错: {e}")
            return False

        # 步骤3: 生成微信项目配置
        print("\n[3/3] 生成微信项目配置...")
        self._generate_wechat_project_config(output_dir)

        print(f"\n✅ 微信小游戏打包完成!")
        print(f"   输出路径: {self.build_dir / output_dir}")
        print(f"\n📝 下一步操作:")
        print(f"   1. 使用微信开发者工具打开: {self.build_dir / output_dir}")
        print(f"   2. 配置 AppID 和其他设置")
        print(f"   3. 预览或上传发布")
        return True

    def _generate_wechat_project_config(self, output_dir):
        """生成微信小游戏项目配置"""
        project_path = self.build_dir / output_dir
        config_content = '''{
  "deviceOrientation": "landscape",
  "showStatusBar": false,
  "networkTimeout": {
    "request": 5000,
    "connectSocket": 5000,
    "uploadFile": 5000,
    "downloadFile": 5000
  }
}'''

        game_js = project_path / "game.js"
        if not game_js.exists():
            with open(game_js, 'w', encoding='utf-8') as f:
                f.write('''// 微信小游戏入口
requireAdapter('./weapp-adapter.js');

// 加载 Godot 引擎
window.onload = function() {
    // Godot 引擎初始化代码将由导出器生成
};
''')

        project_config = project_path / "project.config.json"
        with open(project_config, 'w', encoding='utf-8') as f:
            f.write(config_content)

    def _show_output_info(self, output_path, platform):
        """显示输出文件信息"""
        print(f"\n📁 输出文件:")

        if platform == "windows":
            exe_files = list(output_path.glob("*.exe"))
            if exe_files:
                size_mb = exe_files[0].stat().st_size / (1024 * 1024)
                print(f"   📦 {exe_files[0].name}: {size_mb:.2f} MB")

        elif platform == "mac":
            zip_files = list(output_path.glob("*.zip"))
            if zip_files:
                size_mb = zip_files[0].stat().st_size / (1024 * 1024)
                print(f"   📦 {zip_files[0].name}: {size_mb:.2f} MB")

        elif platform == "android":
            apk_files = list(output_path.glob("*.apk"))
            if apk_files:
                size_mb = apk_files[0].stat().st_size / (1024 * 1024)
                print(f"   📦 {apk_files[0].name}: {size_mb:.2f} MB")

        elif platform in ["web", "wechat"]:
            file_count = sum(1 for _ in output_path.rglob("*"))
            total_size = sum(f.stat().st_size for f in output_path.rglob("*") if f.is_file())
            size_mb = total_size / (1024 * 1024)
            print(f"   📂 文件数: {file_count}")
            print(f"   💾 总大小: {size_mb:.2f} MB")

    def build_all(self, platforms=None, debug=False):
        """批量打包所有指定平台"""
        if platforms is None:
            platforms = ["windows", "mac", "android", "web"]

        results = {}
        failed_platforms = []

        print(f"\n{'='*70}")
        print(f"🏭 批量打包模式")
        print(f"   目标平台: {', '.join(platforms)}")
        print(f"   时间: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        print(f"{'='*70}\n")

        for i, platform in enumerate(platforms, 1):
            print(f"\n[{i}/{len(platforms)}]", end=" ")
            success = self.export_platform(platform, debug)
            results[platform] = success
            if not success:
                failed_platforms.append(platform)

        # 汇总报告
        print(f"\n\n{'='*70}")
        print(f"📊 打包汇总报告")
        print(f"{'='*70}\n")

        for platform, success in results.items():
            status = "✅ 成功" if success else "❌ 失败"
            print(f"   {platform:<12} {status}")

        if failed_platforms:
            print(f"\n⚠️  失败的平台: {', '.join(failed_platforms)}")
            return False
        else:
            print(f"\n🎉 所有平台打包成功!")
            return True

    def clean_build(self):
        """清理构建目录"""
        if self.build_dir.exists():
            shutil.rmtree(self.build_dir)
            print(f"✅ 已清理构建目录: {self.build_dir}")
        else:
            print("ℹ️  构建目录不存在，无需清理")


def main():
    parser = argparse.ArgumentParser(
        description='Godot C# 项目一键打包工具',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog='''
示例:
  %(prog)s --platform windows     打包 Windows 版本
  %(prog)s --platform android     打包 Android APK
  %(prog)s --platform wechat      打包微信小游戏
  %(prog)s --all                  打包所有桌面和移动端平台
  %(prog)s --clean                清理构建目录
  %(prog)s --list                 列出可用平台
        '''
    )

    parser.add_argument(
        '--platform', '-p',
        choices=['windows', 'mac', 'android', 'ios', 'web', 'wechat'],
        help='目标平台'
    )
    parser.add_argument(
        '--all', '-a',
        action='store_true',
        help='打包所有平台（除 iOS 外）'
    )
    parser.add_argument(
        '--debug',
        action='store_true',
        help='导出调试版本（非 release）'
    )
    parser.add_argument(
        '--list', '-l',
        action='store_true',
        help='列出可用平台'
    )
    parser.add_argument(
        '--clean', '-c',
        action='store_true',
        help='清理构建目录'
    )
    parser.add_argument(
        '--godot-path',
        help='指定 Godot 可执行文件路径'
    )

    args = parser.parse_args()

    try:
        tool = BuildTool()

        if args.godot_path:
            tool.godot_executable = args.godot_path

        if args.list:
            tool.list_platforms()
            return 0

        if args.clean:
            tool.clean_build()
            return 0

        if args.platform:
            success = tool.export_platform(args.platform, args.debug)
            return 0 if success else 1

        if args.all:
            success = tool.build_all(debug=args.debug)
            return 0 if success else 1

        # 无参数时显示帮助
        parser.print_help()
        return 0

    except FileNotFoundError as e:
        print(f"\n❌ 错误: {e}", file=sys.stderr)
        return 1
    except KeyboardInterrupt:
        print("\n\n❌ 用户取消操作")
        return 130
    except Exception as e:
        print(f"\n❌ 未预期的错误: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        return 1


if __name__ == "__main__":
    sys.exit(main())
