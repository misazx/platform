#!/bin/bash
# 🎮 杀戮尖塔2 - 可视化打包工具 (Automator 版)
# 使用 Python 3.14

cd "$(dirname "$0")"

# 设置 DOTNET_ROOT
if [ -d "/usr/local/share/dotnet" ]; then
    export DOTNET_ROOT="/usr/local/share/dotnet"
    export DOTNET_ROOT_ARM64="/usr/local/share/dotnet"
elif [ -d "/opt/homebrew/share/dotnet" ]; then
    export DOTNET_ROOT="/opt/homebrew/share/dotnet"
    export DOTNET_ROOT_ARM64="/opt/homebrew/share/dotnet"
fi

/Library/Frameworks/Python.framework/Versions/3.14/bin/python3 Client/Tools/build_gui.py &
