#!/bin/bash
# 杀戮尖塔2 - 可视化打包工具启动脚本
# 自动设置 .NET 环境变量

# 设置 DOTNET_ROOT - 修复 Godot Mono .NET 加载问题
if [ -d "/usr/local/share/dotnet" ]; then
    export DOTNET_ROOT="/usr/local/share/dotnet"
    export DOTNET_ROOT_ARM64="/usr/local/share/dotnet"
elif [ -d "/opt/homebrew/share/dotnet" ]; then
    export DOTNET_ROOT="/opt/homebrew/share/dotnet"
    export DOTNET_ROOT_ARM64="/opt/homebrew/share/dotnet"
fi

echo "==========================================="
echo "🎮 杀戮尖塔2 - Godot Build Tool"
echo "==========================================="
echo ""
echo "环境检查:"
echo "  DOTNET_ROOT: ${DOTNET_ROOT:-未设置}"
echo "  Godot Path: /Users/zhuyong/Downloads/Godot_mono.app"
echo ""

PYTHON_CMD="/usr/bin/python3"

echo "✅ 使用 Python: $($PYTHON_CMD --version)"
echo ""

# 切换到脚本所在目录
cd "$(dirname "$0")"

# 启动 GUI
$PYTHON_CMD Client/Tools/build_gui.py

# 如果出错，显示错误信息
if [ $? -ne 0 ]; then
    echo ""
    echo "❌ 启动失败！"
    echo "   可能的原因:"
    echo "   1. 缺少 tkinter 库（Python GUI 组件）"
    echo "   2. Python 版本过低（需要 3.7+）"
    echo "   3. .NET SDK 未安装或路径错误"
    echo ""
    echo "请检查日志输出"
fi
