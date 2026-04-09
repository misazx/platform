#!/bin/bash
# 一键启动包管理系统测试环境
# 使用方法: ./start_test_environment.sh

echo ""
echo "======================================================================"
echo "🚀 启动游戏包管理系统 - 本地测试环境"
echo "======================================================================"
echo ""

PROJECT_DIR="/Users/zhuyong/trae-game"
CDN_PORT=8080

cd "$PROJECT_DIR"

# 检查Python是否安装
if ! command -v python3 &> /dev/null; then
    echo "❌ 错误: Python3 未安装！请先安装Python3"
    exit 1
fi

# 检查包文件是否存在
if [ ! -f "Client/test_cdn/packages/base_game.zip" ]; then
    echo "⚠️  包文件不存在，正在生成..."
    python3 Client/Tools/build_packages.py --all
    if [ $? -ne 0 ]; then
        echo "❌ 打包失败！请检查错误信息"
        exit 1
    fi
fi

echo "✅ 包文件已就绪"
echo ""

# 显示系统信息
echo "📊 系统状态:"
echo "   📦 base_game.zip: $(du -h Client/test_cdn/packages/base_game.zip | cut -f1)"
echo "   📋 registry.json: $(wc -l < Client/test_cdn/packages/registry.json) 行"
echo ""

# 启动CDN服务器
echo "🌐 正在启动本地CDN服务器 (端口: $CDN_PORT)..."
echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✨ 服务已启动!"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "📡 服务地址:"
echo "   • Registry: http://localhost:$CDN_PORT/registry.json"
echo "   • Package:  http://localhost:$CDN_PORT/packages/base_game.zip"
echo ""
echo "🎮 下一步操作:"
echo "   1. 在Godot中运行游戏 (F5)"
echo "   2. 点击主菜单的 '📦 游戏包商店'"
echo "   3. 测试下载/安装功能"
echo ""
echo "⏹️  按 Ctrl+C 停止服务器"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

python3 Client/Tools/local_cdn_server.py
