#!/usr/bin/osascript
# ============================================================
# 🎮 杀戮尖塔2 - 打包工具启动器 (AppleScript 应用)
# 双击即可启动，最稳定可靠
# ============================================================

-- 获取项目目录
set projectPath to (POSIX path of (path to me)) & "::"
set projectPath to do shell script "cd " & quoted form of projectPath & " && pwd"

-- 启动 Python GUI
do shell script "cd " & quoted form of projectPath & " && /usr/bin/python3 build_gui.py > /dev/null 2>&1 &"
