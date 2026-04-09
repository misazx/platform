#!/bin/bash
# 可视化打包工具启动脚本
# 双击运行或执行: ./start_gui.sh

echo "🎮 启动杀戮尖塔2 - 可视化打包工具..."
echo ""

# 检查 Python
if command -v python3 &> /dev/null; then
    PYTHON_CMD="python3"
elif command -v python &> /dev/null; then
    PYTHON_CMD="python"
else
    echo "❌ 错误: 未找到 Python！"
    echo "   请安装 Python 3.7+ 并添加到 PATH"
    read -p "按回车键退出..."
    exit 1
fi

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
    echo ""
    echo "   解决方案:"
    if [[ "$OSTYPE" == "darwin"* ]]; then
        echo "   macOS: brew install python-tk@3.10"
        echo "   或使用系统自带的 Python: /usr/bin/python3 build_gui.py"
    elif [[ "$OSTYPE" == "linux"* ]]; then
        echo "   Ubuntu/Debian: sudo apt-get install python3-tk"
        echo "   Fedora: sudo dnf install python3-tkinter"
    fi
    echo ""
    read -p "按回车键退出..."
fi
