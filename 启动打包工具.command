#!/bin/bash
# 杀戮尖塔2 - 可视化打包工具
# 自动设置 .NET 环境变量

cd "$(dirname "$0")"

# 设置 DOTNET_ROOT - 修复 Godot Mono .NET 加载问题
if [ -d "/usr/local/share/dotnet" ]; then
    export DOTNET_ROOT="/usr/local/share/dotnet"
    export DOTNET_ROOT_ARM64="/usr/local/share/dotnet"
elif [ -d "/opt/homebrew/share/dotnet" ]; then
    export DOTNET_ROOT="/opt/homebrew/share/dotnet"
    export DOTNET_ROOT_ARM64="/opt/homebrew/share/dotnet"
fi

# 启动 GUI
exec /usr/bin/python3 Client/Tools/build_gui.py "$@"
