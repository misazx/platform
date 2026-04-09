# Godot C# 项目一键打包工具使用指南

## 📋 概述

本工具为 **杀戮尖塔2** Godot 4.6.1 C# 项目提供一键打包能力，支持以下平台：

| 平台 | 输出格式 | 说明 |
|------|---------|------|
| **Windows** | .exe | Windows 桌面版 (x86_64) |
| **macOS** | .app/.zip | macOS 桌面版 (Universal) |
| **Android** | .apk | Android 安装包 (ARM64) |
| **iOS** | Xcode 项目 | iOS 应用 (需要 Mac) |
| **Web** | HTML5/WASM | 网页版本 |
| **微信小游戏** | 微信项目 | 微信小程序游戏 |

## 🚀 快速开始

### 前置条件

1. **Godot 引擎** (4.6.1+)
   - 下载地址: https://godotengine.org/download
   - 推荐: Godot 4.6.1 .NET 版本
   - 确保添加到系统 PATH 或设置 `GODOT_PATH` 环境变量

2. **Python** (3.7+)
   ```bash
   python --version
   ```

3. **平台特定依赖**

#### Android 打包
```bash
# 设置环境变量
export ANDROID_SDK_ROOT=/path/to/android/sdk
export ANDROID_NDK_ROOT=/path/to/android/ndk
```

#### iOS 打包
- 需要 macOS 系统
- 安装 Xcode (14+)
- Apple 开发者账号 ($99/年)

#### 微信小游戏
- [微信开发者工具](https://developers.weixin.qq.com/miniprogram/dev/devtools/download.html)
- Node.js (可选，用于高级转换)

### 基本用法

```bash
# 列出所有可用平台
python3 Client/Tools/build.py --list

# 打包单个平台
python3 Client/Tools/build.py --platform windows      # Windows
python3 Client/Tools/build.py --platform mac          # macOS
python3 Client/Tools/build.py --platform android      # Android APK
python3 Client/Tools/build.py --platform ios          # iOS (需要 Mac)
python3 Client/Tools/build.py --platform web          # Web 版本
python3 Client/Tools/build.py --platform wechat       # 微信小游戏

# 批量打包所有桌面和移动端平台
python3 Client/Tools/build.py --all

# 打包调试版本（非 release）
python3 Client/Tools/build.py --platform windows --debug

# 清理构建目录
python3 Client/Tools/build.py --clean

# 指定 Godot 路径
python3 Client/Tools/build.py --platform windows --godot-path /path/to/godot

# 或使用根目录的一键构建脚本
./build_all.sh --platform windows
./build_all.sh --all

# 启动可视化打包工具
./start_gui.sh
```

## 📦 平台详细说明

### Windows 桌面版

**输出位置**: `../build/windows/杀戮尖塔2.exe`

**特性**:
- x86_64 架构
- 内嵌 PCK 资源包
- S3TC 纹理压缩
- 可选代码签名

**分发方式**:
- 直接发送 .exe 文件
- 使用 NSIS/Inno Setup 创建安装程序
- 上传到 Steam/其他平台

### macOS 桌面版

**输出位置**: `../build/macOS/杀戮尖塔2.zip`

**特性**:
- Universal 二进制 (Intel + Apple Silicon)
- BPTC/S3TC 双纹理格式
- 可选代码签名和公证

**分发**:
- 解压 .zip 即可运行
- 提交 Mac App Store 需要签名

### Android APK

**输出位置**: `../build/android/杀戮尖塔2.apk`

**配置要求**:
1. 安装 Android SDK
2. 配置调试/发布密钥库 (keystore)

**环境变量**:
```bash
export ANDROID_SDK_ROOT=/Users/yourname/Library/Android/sdk
```

**优化建议**:
- 使用 App Bundle (.aab) 替代 APK 用于 Google Play
- 启用 ProGuard/R8 混淆
- 配置 ABI 分包减少体积

### iOS

**输出位置**: `../build/ios/`

**前置步骤**:
1. 在 Xcode 中打开生成的项目
2. 配置开发者账号和证书
3. 选择目标设备或模拟器
4. 点击 Run 或 Archive

**提交 App Store**:
1. Product → Archive
2. Distribute App
3. 选择上传方式

### Web 版本

**输出位置**: `../build/web/index.html`

**部署选项**:
- GitHub Pages
- Netlify / Vercel
- 自托管服务器
- CDN 分发

**性能优化**:
- 启用 Gzip/Brotli 压缩
- 配置正确的 MIME 类型
- 使用 HTTP/2
- 启用缓存策略

**Nginx 配置示例**:
```nginx
server {
    listen 80;
    server_name your-game.com;
    root /path/to/build/web;

    location / {
        try_files $uri $uri/ =404;
    }

    # WASM MIME 类型
    types {
        application/wasm wasm;
    }

    # 启用 gzip
    gzip on;
    gzip_types text/html text/css application/javascript application/wasm;
}
```

### 微信小游戏 ⭐

**输出位置**: `../build/wechat/`

**工作流程**:

1. **打包生成**
   ```bash
   python build.py --platform wechat
   ```

2. **导入微信开发者工具**
   - 打开微信开发者工具
   - 导入项目: `../build/wechat`
   - 填写测试 AppID 或正式 AppID
   - 点击"编译"

3. **预览与调试**
   - 使用真机预览功能测试
   - 检查控制台错误日志
   - 测试触摸操作和屏幕适配

4. **发布上线**
   - 点击"上传"按钮
   - 在微信公众平台提交审核
   - 审核通过后发布

**适配层说明** (`weapp-adapter.js`):
- 自动模拟浏览器 API (window, document, canvas 等)
- 兼容 AudioContext、localStorage、XMLHttpRequest
- 支持 WebSocket、Performance API
- 处理微信特有限制（文件大小、内存等)

**常见问题解决**:
- 包体过大: 启用资源分包加载
- 音频不兼容: 使用 InnerAudioContext 适配
- 性能问题: 降低画质、减少粒子效果

## 🔧 高级配置

### 自定义 export_presets.cfg

编辑项目根目录的 `export_presets.cfg` 可以自定义：

- 应用图标
- 显示名称
- 版本号
- 权限声明
- 签名信息
- 图标集

### CI/CD 集成

#### GitHub Actions 示例

```yaml
name: Build Game

on:
  push:
    tags:
      - 'v*'

jobs:
  build-windows:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup Godot
      run: |
        choco install godot
        
    - name: Build Windows
      run: python build.py --platform windows
      
    - name: Upload Artifact
      uses: actions/upload-artifact@v3
      with:
        name: windows-build
        path: ../build/windows/

  build-web:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup Python
      uses: actions/setup-python@v5
      with:
        python-version: '3.10'
        
    - name: Build Web
      run: python build.py --platform web
      
    - name: Deploy to Pages
      uses: peaceiris/actions-gh-pages@v3
      with:
        github_token: ${{ secrets.GITHUB_TOKEN }}
        publish_dir: ../build/web
```

#### GitLab CI 示例

```yaml
stages:
  - build

build_windows:
  stage: build
  image: ubuntu:22.04
  script:
    - pip install -r requirements.txt
    - python build.py --platform windows
  artifacts:
    paths:
      - ../build/windows/
    expire_in: 1 week
```

### 批量脚本示例

```bash
#!/bin/bash
# full_build.sh - 完整构建脚本

set -e

echo "🏗️  开始完整构建..."

# 清理旧构建
python build.py --clean

# 构建 Web 版本（最快）
python build.py --platform web

# 构建桌面端
python build.py --platform windows
python build.py --platform mac

# 构建移动端
python build.py --platform android

# 构建微信小游戏
python build.py --platform wechat

echo ""
echo "✅ 所有平台构建完成!"
echo "📁 输出目录: ../build/"
ls -la ../build/
```

## 🐛 故障排除

### 常见错误

#### 1. 找不到 Godot 可执行文件

**错误**: `FileNotFoundError: 未找到 Godot 可执行文件`

**解决方案**:
```bash
# 方案1: 添加到 PATH
export PATH="/Applications/Godot.app/Contents/MacOS:$PATH"

# 方案2: 使用参数指定
python build.py --platform windows --godot-path /path/to/godot
```

#### 2. Android SDK 未找到

**错误**: `未设置 ANDROID_SDK_ROOT 环境变量`

**解决方案**:
```bash
# 查找 SDK 位置
find ~/Library/Android/sdk -name "android" -type f 2>/dev/null

# 设置环境变量
echo 'export ANDROID_SDK_ROOT=~/Library/Android/sdk' >> ~/.zshrc
source ~/.zshrc
```

#### 3. iOS 编译失败

**错误**: 需要 macOS 和 Xcode

**解决方案**:
- 确保在 Mac 上执行
- 安装最新版 Xcode: `xcode-select --install`
- 接受许可协议: `sudo xcodebuild -license accept`

#### 4. 微信小游戏包体过大

**问题**: 微信限制主包 4MB，总包 20MB

**解决方案**:
1. 启用资源分包
2. 减少纹理分辨率
3. 使用压缩音频格式
4. 按需加载场景

```gdscript
# 在 Godot 中启用资源分包
func _ready():
    # 动态加载大资源
    var large_texture = preload("res://textures/large_image.png")
```

#### 5. C# 编译错误

**错误**: `.NET 构建失败`

**解决方案**:
```bash
# 确保 .NET SDK 已安装
dotnet --version

# 清理并重建
dotnet clean
dotnet restore
python build.py --platform windows
```

### 日志查看

```bash
# 详细模式（如果实现）
python build.py --platform windows --verbose

# 查看 Godot 输出日志
tail -f ../build/windows/godot_log.txt
```

## 📊 性能优化建议

### 包体大小优化

| 平台 | 目标大小 | 优化方法 |
|------|---------|---------|
| Windows | < 100 MB | UPX 压缩、去除调试符号 |
| macOS | < 150 MB | 代码签名时 Strip |
| Android | < 80 MB | ABI 分包、ProGuard |
| Web | < 30 MB | Gzip、懒加载、CDN |
| 微信 | < 20 MB | 资源分包、按需下载 |

### 运行时性能

1. **纹理压缩**
   - Windows/macOS: BC7 (BPTC)
   - Mobile: ASTC/ETC2
   - Web: 不压缩或手动转码

2. **音频优化**
   - 使用 OGG Vorbis 格式
   - 采样率: 44100 Hz (音乐), 22050 Hz (音效)
   - 单声道用于简单音效

3. **代码优化**
   - 启用 IL2CPP (iOS/Android)
   - 使用 AOT 编译
   - 避免反射和动态类型

## 📝 更新日志

### v1.0.0 (2026-04-09)
- ✅ 初始版本
- ✅ 支持所有主流平台导出
- ✅ 微信小游戏自动转换
- ✅ 一键批量打包
- ✅ CI/CD 集成示例

## 🤝 贡献指南

发现 Bug 或有改进建议？

1. 创建 Issue 描述问题
2. Fork 并创建分支
3. 提交 Pull Request
4. 等待 Code Review

## 📄 许可证

本项目遵循原项目的开源许可证。

---

**快速链接**:
- [Godot 官方文档](https://docs.godotengine.org/)
- [Godot 导出教程](https://docs.godotengine.org/en/stable/tutorials/export/exporting_projects.html)
- [微信小游戏开发文档](https://developers.weixin.qq.com/miniprogram/dev/framework/)
- [C# in Godot](https://docs.godotengine.org/en/stable/tutorials/scripting/csharp/index.html)

---

**需要帮助?** 查看 [常见问题](#故障排除) 或在项目 Issues 中提问。
