#!/bin/bash
# 一键完整构建脚本
# 使用方法: ./build_all.sh [options]
#
# Options:
#   --clean     清理构建目录后重新构建
#   --debug     构建调试版本
#   --help      显示帮助信息

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  --clean     Clean build directory before building"
    echo "  --debug     Build debug versions"
    echo "  --platform  Build specific platform (windows|mac|android|ios|web|wechat)"
    echo "  --all       Build all platforms (default)"
    echo "  --help      Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                  # Build all platforms"
    echo "  $0 --platform web   # Build only Web version"
    echo "  $0 --clean --debug  # Clean and build debug versions"
}

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# 解析参数
CLEAN_BUILD=false
DEBUG_MODE=false
PLATFORM=""
BUILD_ALL=true

while [[ $# -gt 0 ]]; do
    case $1 in
        --clean)
            CLEAN_BUILD=true
            shift
            ;;
        --debug)
            DEBUG_MODE=true
            shift
            ;;
        --platform)
            PLATFORM="$2"
            BUILD_ALL=false
            shift 2
            ;;
        --all)
            BUILD_ALL=true
            shift
            ;;
        --help|-h)
            usage
            exit 0
            ;;
        *)
            log_error "Unknown option: $1"
            usage
            exit 1
            ;;
    esac
done

echo ""
echo "╔══════════════════════════════════════════════════════╗"
echo "║         杀戮尖塔2 - 一键构建工具 v1.0              ║"
echo "╚══════════════════════════════════════════════════════╝"
echo ""

# 检查 Python
if ! command -v python3 &> /dev/null && ! command -v python &> /dev/null; then
    log_error "Python is not installed!"
    exit 1
fi

PYTHON_CMD="python3"
if command -v python &> /dev/null; then
    PYTHON_CMD="python"
fi

log_info "Using Python: $($PYTHON_CMD --version)"

# 检查 Godot（可选）
if command -v godot &> /dev/null || [ -f "/Applications/Godot.app/Contents/MacOS/Godot" ]; then
    log_success "Godot found!"
else
    log_warning "Godot not found in PATH. Make sure it's available or use --godot-path"
fi

START_TIME=$(date +%s)

# 清理构建目录
if [ "$CLEAN_BUILD" = true ]; then
    log_info "Cleaning build directory..."
    $PYTHON_CMD Client/Tools/build.py --clean
fi

# 执行构建
BUILD_ARGS=""
if [ "$DEBUG_MODE" = true ]; then
    BUILD_ARGS="$BUILD_ARGS --debug"
fi

if [ "$BUILD_ALL" = true ]; then
    log_info "Building ALL platforms..."

    # 定义平台列表（排除 iOS，因为需要特殊环境）
    PLATFORMS=("web" "windows" "mac" "android")

    FAILED_PLATFORMS=()
    SUCCESS_COUNT=0

    for platform in "${PLATFORMS[@]}"; do
        echo ""
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        log_info "Building: $platform"
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

        if $PYTHON_CMD Client/Tools/build.py --platform "$platform" $BUILD_ARGS; then
            log_success "✓ $platform build completed"
            ((SUCCESS_COUNT++))
        else
            log_error "✗ $platform build failed"
            FAILED_PLATFORMS+=("$platform")
        fi
    done
    
    # 微信小游戏（基于 Web）
    echo ""
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    log_info "Building: WeChat Mini Game"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

    if $PYTHON_CMD Client/Tools/build.py --platform wechat $BUILD_ARGS; then
        log_success "✓ WeChat Mini Game build completed"
        ((SUCCESS_COUNT++))
    else
        log_error "✗ WeChat Mini Game build failed"
        FAILED_PLATFORMS+=("wechat")
    fi
    
    # 汇总报告
    END_TIME=$(date +%s)
    DURATION=$((END_TIME - START_TIME))
    
    echo ""
    echo "╔══════════════════════════════════════════════════════╗"
    echo "║                    BUILD SUMMARY                     ║"
    echo "╠══════════════════════════════════════════════════════╣"
    printf "║  Total Time: %-40s ║\n" "$(printf '%02d:%02d' $((DURATION/60)) $((DURATION%60)))"
    printf "║  Successful: %-40s ║\n" "$SUCCESS_COUNT/${#PLATFORMS[@]} + wechat"
    
    if [ ${#FAILED_PLATFORMS[@]} -eq 0 ]; then
        printf "║  Status:    %-40s ║\n" "✅ ALL SUCCESSFUL"
    else
        printf "║  Failed:    %-40s ║\n" "${FAILED_PLATFORMS[*]}"
        printf "║  Status:    %-40s ║\n" "⚠️  SOME FAILURES"
    fi
    
    echo "╚══════════════════════════════════════════════════════╝"
    echo ""
    
    if [ ${#FAILED_PLATFORMS[@]} -gt 0 ]; then
        log_warning "Some builds failed. Check the logs above for details."
        exit 1
    fi
    
else
    if [ -z "$PLATFORM" ]; then
        log_error "No platform specified!"
        usage
        exit 1
    fi
    
    log_info "Building platform: $PLATFORM"

    if $PYTHON_CMD Client/Tools/build.py --platform "$PLATFORM" $BUILD_ARGS; then
        log_success "Build completed successfully!"
    else
        log_error "Build failed!"
        exit 1
    fi
fi

END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))

echo ""
log_success "🎉 Total build time: $(printf '%02d:%02d' $((DURATION/60)) $((DURATION%60)))"
log_success "📁 Output location: ../build/"
echo ""

# 显示输出文件
if [ -d "../build" ]; then
    echo "Generated files:"
    find ../build -type f \( -name "*.exe" -o -name "*.zip" -o -name "*.apk" -o -name "*.html" \) | while read file; do
        size=$(du -h "$file" | cut -f1)
        echo "  📦 $(basename $file) ($size)"
    done
fi

echo ""
