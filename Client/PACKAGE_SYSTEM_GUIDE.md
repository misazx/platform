# 🎮 游戏包管理系统 - 完整开发指南

## 📋 系统概述

本系统为 Roguelike 游戏提供完善的**多玩法包管理架构**，支持：
- ✅ 将当前游戏作为"基础包"
- ✅ 动态下载、安装、卸载扩展包
- ✅ 主界面展示所有可用游戏包
- ✅ 包依赖管理和冲突检测
- ✅ 扩展接口支持自定义内容（卡牌、角色、敌人等）
- ✅ 付费/免费包支持
- ✅ 社区Mod支持

---

## 🏗️ 架构设计

### 核心组件

```
Scripts/Packages/
├── PackageModels.cs          # 数据模型定义
├── PackageManager.cs         # 核心管理器（单例）
├── IPackageExtension.cs      # 扩展接口 + 基类
└── Samples/
    └── SampleExtensions.cs   # 示例扩展实现

Scripts/UI/
└── PackageStoreUI.cs         # 商店界面
    └── EnhancedMainMenu.cs   # 增强主菜单

Config/Data/
└── package_registry.json     # 包注册表配置
```

### 类关系图

```
PackageManager (单例 Node)
├── 管理所有包的生命周期
├── 提供下载/安装/卸载API
└── 触发信号通知UI更新

PackageData (数据模型)
├── 定义包的元数据
├── 包含下载URL、依赖关系等
└── 支持自定义数据扩展

IPackageExtension (接口)
└── PackageExtensionBase (抽象基类)
    ├── BaseGameExtension (基础游戏)
    ├── FrostExpansion (示例：冰霜扩展)
    └── ShadowRealmExtension (示例：暗影领域)

PackageStoreUI (UI层)
├── 展示所有可用包
├── 支持搜索和分类筛选
└── 处理用户交互（下载/启动）
```

---

## 🚀 快速开始

### 1. 初始化 PackageManager

在 `Main.cs` 或 `GameInitializer.cs` 中添加：

```csharp
// 创建 PackageManager 节点
var packageManager = new PackageManager();
AddChild(packageManager);
```

### 2. 使用增强版主菜单

替换原有 MainMenu：

```csharp
var mainMenu = new EnhancedMainMenu();
mainMenu.GameTitle = "我的Roguelike游戏";
GetTree().Root.AddChild(mainMenu);
```

### 3. 配置包注册表

编辑 [Config/Data/package_registry.json](Config/Data/package_registry.json) 添加你的包。

---

## 📦 如何创建新包

### 步骤 1: 定义包数据

在 `package_registry.json` 中添加：

```json
{
  "id": "my_expansion",
  "name": "我的扩展包",
  "description": "这是一个很棒的扩展！",
  "version": "1.0.0",
  "type": 1,
  "author": "YourName",
  "downloadUrl": "https://example.com/my_expansion.zip",
  "fileSize": 150000000,
  "dependencies": ["base_game"],
  "tags": ["expansion", "custom"],
  "features": [
    "特性1",
    "特性2"
  ],
  "isFree": true,
  "entryScene": "res://Scenes/MyExpansionScene.tscn",
  "configFile": "res://Config/Data/my_expansion_cards.json"
}
```

**字段说明：**
- `id`: 唯一标识符（必填）
- `type`: 0=基础游戏, 1=扩展, 2=自定义地图, 3=Mod
- `dependencies`: 依赖的其他包ID列表
- `tags`: 用于搜索的标签
- `customData`: 自定义扩展数据

### 步骤 2: 创建包扩展类

继承 `PackageExtensionBase`：

```csharp
using RoguelikeGame.Packages;
using RoguelikeGame.Core;
using RoguelikeGame.Database;

namespace MyGame.Packages
{
    public class MyExpansion : PackageExtensionBase
    {
        public override string PackageId => "my_expansion";

        public override void OnInitialize()
        {
            base.OnInitialize();
            GD.Print("[MyExpansion] 初始化中...");
        }

        public override void RegisterCustomCards()
        {
            var myCards = new List<CardConfig>
            {
                new CardConfig
                {
                    Id = "my_card_1",
                    Name = "我的卡牌",
                    Description = "描述文本",
                    Cost = 1,
                    Type = "Attack",
                    Damage = 8,
                    Rarity = "Common",
                    Color = "#FF6B6B"
                }
                // ... 更多卡牌
            };

            foreach (var card in myCards)
            {
                CardDatabase.Instance?.RegisterCard(card);
            }
        }

        public override void RegisterCustomCharacters()
        {
            // 注册新角色
        }

        public override void RegisterCustomEnemies()
        {
            // 注册新敌人
        }

        public override void RegisterCustomEvents()
        {
            // 注册新事件
        }

        public override void RegisterCustomRelics()
        {
            // 注册新圣物
        }

        public override void OnLaunch()
        {
            base.OnLaunch();
            // 启动时的特殊逻辑
        }

        // 存档系统支持
        public override Dictionary<string, object> GetSaveData()
        {
            return new Dictionary<string, object>
            {
                { "playerProgress", someData },
                { "unlocks", unlockList }
            };
        }

        public override void LoadSaveData(Dictionary<string, object> data)
        {
            // 加载存档
        }

        // 冲突检测
        public override List<string> GetConflictingPackages()
        {
            return new List<string> { "conflicting_package_id" };
        }
    }
}
```

### 步骤 3: 打包资源

将扩展内容打包成 ZIP 文件，包含：
```
my_expansion.zip
├── my_expansion.dll          # 编译后的扩展程序集
├── config/
│   └── cards.json           # 卡牌配置
├── scenes/
│   └── MyScene.tscn         # 场景文件
├── images/
│   └── icons/               # 图标资源
└── audio/
    └── sfx/                 # 音效资源
```

### 步骤 4: 上传到CDN

将 ZIP 文件上传到服务器，更新 `package_registry.json` 中的 `downloadUrl`。

---

## 🔧 核心 API 参考

### PackageManager API

```csharp
// 获取实例
PackageManager.Instance

// 包查询
var package = PackageManager.Instance.GetPackage("package_id");
var state = PackageManager.Instance.GetPackageState("package_id");
bool isInstalled = PackageManager.Instance.IsPackageInstalled("package_id");
bool canLaunch = PackageManager.Instance.CanLaunchPackage("package_id");

// 分类获取
var officialPackages = PackageManager.Instance.GetPackagesByCategory("official");
var featured = PackageManager.Instance.GetFeaturedPackages();
var searchResults = PackageManager.Instance.SearchPackages("冰霜");

// 操作方法
await PackageManager.Instance.DownloadPackageAsync("package_id");
await PackageManager.Instance.InstallPackageAsync("package_id");
PackageManager.Instance.UninstallPackage("package_id");
PackageManager.Instance.LaunchPackage("package_id");

// 刷新包列表
await PackageManager.Instance.RefreshPackageListAsync();

// 信号
PackageManager.Instance.PackageListUpdated += handler;
PackageManager.Instance.PackageDownloadProgress += (id, progress) => { };
PackageManager.Instance.PackageDownloadCompleted += (id, success) => { };
PackageManager.Instance.PackageInstalled += id => { };
PackageManager.Instance.PackageUninstalled += id => { };
PackageManager.Instance.PackageError += (id, error) => { };
```

### PackageStoreUI API

```csharp
var storeUI = new PackageStoreUI();
AddChild(storeUI);

// 事件
storeUI.PackageSelected += pkg => { };
storeUI.PackageLaunchRequested += pkg => { };
storeUI.DownloadRequested += pkg => { };

// 方法
storeUI.RefreshPackageList();
storeUI.Visible = true; // 显示商店
```

---

## 🎨 UI 定制

### 修改商店样式

在 `PackageStoreUI.cs` 中修改 `CreatePackageCard()` 方法：

```csharp
// 更改卡片颜色
var cardStyle = new StyleBoxFlat
{
    BgColor = new Color(0.2f, 0.15f, 0.25f, 0.95f), // 你的颜色
    CornerRadiusTopLeft = 15,                            // 圆角大小
    BorderColor = new Color(0.8f, 0.5f, 0.2f, 0.9f)   // 边框颜色
};
```

### 添加新分类标签

```csharp
CreateCategoryButton("新分类", "new_category");
```

并在 `package_registry.json` 的 `categories` 数组中添加对应项。

---

## 💾 存储结构

### 本地文件位置

```
user://packages/                      # 包安装目录
├── frost_expansion/                  # 已安装的包
│   ├── (解压后的文件)
├── frost_expansion.zip               # 下载的压缩包
└── shadow_realm/

user://package_registry.json          # 本地缓存的注册表
user://package_states.json            # 安装状态记录
```

### 状态持久化

系统自动保存：
- ✅ 安装状态（已下载/已安装/错误）
- ✅ 安装版本和路径
- ✅ 最后游玩时间
- ✅ 下载进度
- ✅ 各包的自定义存档数据

---

## 🔐 安全性考虑

### 1. 包验证

建议在 `PackageManager.cs` 的 `InstallPackageAsync()` 中添加：

```csharp
// 验证包签名或校验和
if (!VerifyPackageHash(packageId, expectedHash))
{
    throw new SecurityException("Package verification failed!");
}
```

### 2. 沙箱执行

扩展代码应在受限环境中运行：
- 限制文件系统访问
- 禁止网络请求（除非明确允许）
- 限制内存和CPU使用

### 3. 依赖版本检查

```csharp
public bool ValidateDependencies()
{
    var package = GetPackage(PackageId);
    foreach (var dep in package.Dependencies)
    {
        var depState = GetPackageState(dep);
        if (depState?.Status != PackageStatus.Installed)
            return false;

        // 版本兼容性检查
        if (!IsVersionCompatible(depState.InstalledVersion, dep.RequiredVersion))
            return false;
    }
    return true;
}
```

---

## 📊 性能优化建议

### 1. 增量更新

只下载变更的文件而非整个包：
```csharp
async Task UpdatePackageAsync(string packageId)
{
    var patchInfo = await GetPatchInfo(packageId);
    await DownloadPatchFiles(patchInfo.ChangedFiles);
}
```

### 2. 异步预加载

在后台预加载包资源：
```csharp
async Task PreloadPackageAssets(string packageId)
{
    var state = GetPackageState(packageId);
    await Task.Run(() =>
    {
        PreloadScenes(state.InstalledPath);
        PreloadTextures(state.InstalledPath);
    });
}
```

### 3. 缓存策略

缓存包图标和缩略图：
```csharp
private Dictionary<string, Texture2D> _iconCache = new();

Texture2D GetCachedIcon(string path)
{
    if (_iconCache.TryGetValue(path, out var cached))
        return cached;

    var texture = LoadTexture(path);
    _iconCache[path] = texture;
    return texture;
}
```

---

## 🧪 测试指南

### 单元测试

```csharp
[Test]
public void TestPackageInstallation()
{
    var manager = new PackageManager();
    manager.DownloadPackageAsync("test_package").Wait();

    Assert.IsTrue(manager.IsPackageInstalled("test_package"));
}

[Test]
public void TestDependencyValidation()
{
    var extension = new FrostExtension();
    Assert.IsTrue(extension.ValidateDependencies());
}
```

### 手动测试清单

- [ ] 下载大文件包（>100MB）
- [ ] 断网重试机制
- [ ] 并发下载多个包
- [ ] 卸载后重新安装
- [ ] 依赖缺失提示
- [ ] 冲突包检测
- [ ] 存档数据完整性
- [ ] UI响应速度（100+包）

---

## ❓ 常见问题

### Q: 如何处理离线模式？

```csharp
public async Task LoadRegistryAsync()
{
    try
    {
        // 尝试在线获取
        var onlineRegistry = await FetchOnlineRegistry();
        SaveToLocal(onlineRegistry);
    }
    catch
    {
        // 回退到本地缓存
       	if (FileAccess.FileExists(REGISTRY_FILE))
       	{
       		LoadLocalRegistry();
       	}
       	else
       	{
       		GD.PrintWarn("No registry available, using built-in packages only");
       	}
    }
}
```

### Q: 如何支持模组创作者？

提供工具链：
1. **SDK文档** - IPackageExtension 接口说明
2. **模板项目** - SampleExtensions.cs 作为起点
3. **验证工具** - 包格式检查器
4. **发布平台** - 自动化提交流程

### Q: 如何实现内购？

集成支付系统：
```csharp
async Task PurchasePackage(string packageId)
{
    var package = GetPackage(packageId);
    if (package.IsFree)
    {
        await DownloadPackageAsync(packageId);
        return;
    }

    var paymentResult = await PaymentService.ProcessPurchase(
        packageId,
        package.Price
    );

    if (paymentResult.Success)
    {
        await DownloadPackageAsync(packageId);
    }
}
```

---

## 📈 后续扩展路线图

### v1.1 - 增强功能
- [ ] 云存档同步
- [ ] 成就系统集成
- [ ] 包评分和评论
- [ ] 自动更新机制

### v1.2 - 社交功能
- [ ] 多人协作包
- [ ] 好友进度分享
- [ ] 包创作者市场
- [ ] Mod 工作坊集成

### v2.0 - 高级特性
- [ ] 运行时热加载
- [ ] 包依赖可视化
- [ ] A/B 测试框架
- [ ] 分析数据收集

---

## 🛠️ 开发工具推荐

- **JSON编辑器**: VS Code + JSON插件
- **包测试**: Godot内置调试器
- **性能分析**: Godot Monitor
- **版本控制**: Git + LFS（用于大文件）

---

## 📝 最佳实践

1. **语义化版本**: 使用 `MAJOR.MINOR.PATCH` 格式
2. **向后兼容**: 保持API稳定
3. **文档完善**: 为每个包编写README
4. **测试覆盖**: 核心路径必须有测试
5. **渐进式加载**: 大包分块下载
6. **错误恢复**: 所有操作可重试
7. **用户反馈**: 显示清晰的进度和错误信息

---

## 🎯 总结

这个包管理系统提供了：
- ✅ **完整的生命周期管理** - 从下载到卸载
- ✅ **灵活的扩展机制** - 通过接口注入自定义内容
- ✅ **用户友好的界面** - 商店式浏览体验
- ✅ **开发者友好** - 清晰的API和丰富的示例
- ✅ **生产就绪** - 错误处理、安全性、性能优化

现在你可以专注于创作精彩的 gameplay 内容，而不用担心技术基础设施！

**祝你开发愉快！🚀**
