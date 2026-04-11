#!/bin/bash
# 杀戮尖塔2 - 可视化打包工具启动脚本
# 使用 Python 3.14

# 设置 DOTNET_ROOT
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
echo "  Python: $(/Library/Frameworks/Python.framework/Versions/3.14/bin/python3 --version)"
echo "  DOTNET_ROOT: ${DOTNET_ROOT:-未设置}"
echo ""

cd "$(dirname "$0")"

/Library/Frameworks/Python.framework/Versions/3.14/bin/python3 Client/Tools/build_gui.py

if [ $? -ne 0 ]; then
    echo ""
    echo "❌ 启动失败！"
    echo "   可能的原因:"
    echo "   1. 缺少 tkinter 库"
    echo "   2. Python 版本问题"
    echo "   3. .NET SDK 未安装或路径错误"
fi
