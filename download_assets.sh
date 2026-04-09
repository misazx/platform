#!/bin/bash

# ============================================================
# Godot Roguelike - 一键下载免费游戏资源包
# 
# 使用方法:
#   chmod +x download_assets.sh
#   ./download_assets.sh              # 下载全部（推荐）
#   ./download_assets.sh --art-only   # 仅美术
#   ./download_assets.sh --audio-only # 仅音频
#
# 功能:
# 🎨 自动打开浏览器下载 Kenney.nl 免费美术素材 (CC0)
# 🔊 自动下载 Kenney Audio 免费音效 (CC0)
# 🎵 自动下载 Kenney Music 免费背景音乐 (CC0)
# 📁 下载完成后运行 python3 batch_import.py 自动整理
# ============================================================

echo "
╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║   🎮 Godot Roguelike - 免费资源一键下载器                  ║
║                                                           ║
║   将自动打开浏览器下载以下免费资源 (CC0授权,可商用):       ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝
"

# 创建下载目录
DOWNLOAD_DIR="$HOME/Downloads/godot-assets-$(date +%Y%m%d)"
mkdir -p "$DOWNLOAD_DIR"

echo "📁 下载目录: $DOWNLOAD_DIR"
echo ""

# 计数器
TOTAL_URLS=0
OPENED_URLS=0

# ==================== 美术资源 URL ====================

open_url() {
    local url="$1"
    local description="$2"
    
    TOTAL_URLS=$((TOTAL_URLS + 1))
    echo "[$TOTAL_URLS] 📦 $description"
    echo "     $url"
    
    if command -v open &> /dev/null; then
        open "$url" 2>/dev/null &
        OPENED_URLS=$((OPENED_URLS + 1))
    elif command -v xdg-open &> /dev/null; then
        xdg-open "$url" 2>/dev/null &
        OPENED_URLS=$((OPENED_URLS + 1))
    else
        echo "     ⚠️ 请手动复制链接到浏览器打开"
    fi
    
    echo ""
}

if [[ "$1" != "--audio-only" ]]; then
    echo "=========================================="
    echo "🎨 美术资源 (Kenney.nl CC0 免费)"
    echo "=========================================="
    echo ""
    
    # RPG Urban Pack (最重要的一个)
    open_url "https://kenney.nl/assets/rpg-urban-pack" \
             "RPG Urban Pack - 角色/UI/道具/武器 (~15MB) ⭐推荐"
    
    # UI Pack
    open_url "https://kenney.nl/assets/ui-pack" \
             "UI Pack - 按钮/面板/框架界面元素 (~8MB)"
    
    # Platformer Characters
    open_url "https://kenney.nl/assets/platformer-characters" \
             "Platformer Characters - 角色精灵图 (~5MB)"
    
    # Particle Effects
    open_url "https://kenney.nl/assets/particle-effects" \
             "Particle Effects - 攻击/魔法特效 (~2MB)"
    
    # Tileset Platformer (可用作背景)
    open_url "https://kenney.nl/assets/tileset-platformer" \
             "Tileset Platformer - 瓦片集/背景图 (~10MB)"
    
    # Roguelike Pack (特别适合!)
    open_url "https://kenney.nl/assets/roguelike-pack" \
             "Roguelike Pack - Roguelike专用素材 ⭐⭐强烈推荐"
fi

# ==================== 音效资源 ====================

if [[ "$1" != "--art-only" ]]; then
    echo "=========================================="
    echo "🔊 音效资源 (Kenney Audio CC0 免费)"
    echo "=========================================="
    echo ""
    
    # Sound Effects
    open_url "https://kenney.nl/assets/audio-soundeffects" \
             "Audio SoundEffects - 攻击/受伤/技能音效 (~5MB) ⭐推荐"
    
    # Jingles
    open_url "https://kenney.nl/assets/audio-jingles" \
             "Audio Jingles - 提示音/UI音效 (~3MB)"
fi

# ==================== 背景音乐 ====================

if [[ "$1" != "--art-only" ]]; then
    echo "=========================================="
    echo "🎵 背景音乐 (Kenney Music CC0 免费)"
    echo "=========================================="
    echo ""
    
    # Music Pack
    open_url "https://kenney.nl/assets/audio-music" \
             "Audio Music - 循环BGM背景音乐 (~12MB) ⭐推荐"
fi

# ==================== OpenGameArt 额外资源 ====================

if [[ "$1" != "--audio-only" ]]; then
    echo "=========================================="
    echo "🌐 额外资源 (OpenGameArt)"
    echo "=========================================="
    echo ""
    
    # LPC Characters
    open_url "https://opengameart.org/content/lpc-base-sprites" \
             "LPC Base Sprites - 开源像素角色集"
    
    # Fantasy UI Icons
    open_url "https://opengameart.org/content/fantasy-ui-icons-kit" \
             "Fantasy UI Icons - 奇幻风格图标"
fi

# ==================== 总结 ====================

echo ""
echo "╔═══════════════════════════════════════════════════════════╗"
echo "║                                                         ║"
echo "║   ✅ 已在浏览器中打开 $OPENED_URLS 个下载页面            ║"
echo "║                                                         ║"
echo "║   📋 下一步操作:                                        ║"
echo "║                                                         ║"
echo "║   1. 在浏览器中逐个点击 'Download Asset Pack'           ║"
echo "║      (通常在每个页面的顶部或底部)                       ║"
echo "║                                                         ║"
echo "║   2. 将所有下载的 ZIP 文件移动到统一目录:                ║"
echo "║      $DOWNLOAD_DIR                                      ║"
echo "║                                                         ║
echo "║   3. 下载完成后，运行自动整理命令:                       ║"
echo "║      python3 Client/Tools/batch_import.py $DOWNLOAD_DIR               ║"
echo "║                                                         ║
echo "║   4. 查看集成报告并更新配置文件                          ║"
echo "║                                                         ║"
echo "╚═══════════════════════════════════════════════════════════╝"
echo ""

# 创建快速整理脚本
CAT_SCRIPT="$DOWNLOAD_DIR/../auto_import_when_done.sh"
cat > "$CAT_SCRIPT" << 'EOF'
#!/bin/bash
# 下载完成后运行此脚本自动整理

echo "🚀 开始整理已下载的资源..."

# 查找所有 ZIP 文件并解压
find . -name "*.zip" -exec unzip -o {} -d extracted_{} \;

# 运行批量导入
python3 Client/Tools/batch_import.py ./extracted_*

echo "✅ 整理完成！请查看 Assets_Library/import_report.json"
EOF

chmod +x "$CAT_SCRIPT"

echo "💡 提示:"
echo "   • 所有 ZIP 文件建议保存到: $DOWNLOAD_DIR"
echo "   • 也可以直接将解压后的文件夹路径提供给 batch_import.py"
echo "   • 如需帮助查看: cat ART_ASSET_COMPLETE_GUIDE.md"
echo ""

# 可选：等待用户确认后自动导入
read -p "是否已下载完成？现在运行自动整理？(y/n): " -n 1 -r
echo ""

if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo ""
    echo "🔧 开始自动整理..."
    sleep 2
    python3 Client/Tools/batch_import.py "$DOWNLOAD_DIR"
else
    echo ""
    echo "💾 下载完成后，请手动运行:"
    echo "   python3 Client/Tools/batch_import.py $DOWNLOAD_DIR"
    echo ""
    echo "或使用快捷脚本:"
    echo "   bash $CAT_SCRIPT"
fi
