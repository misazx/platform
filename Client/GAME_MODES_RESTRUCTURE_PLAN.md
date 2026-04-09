# 🎮 游戏玩法目录重构方案

## 📋 当前问题

当前项目采用**扁平化结构**，所有资源混在一起：
```
❌ 当前结构:
├── Config/Data/          # 所有配置混合
├── Scenes/               # 所有场景混合
├── Audio/                # 所有音频混合
├── Images/               # 所有图片混合
├── Icons/                # 所有图标混合
└── Scripts/              # 所有代码混合
```

**问题**:
- ❌ 无法区分不同玩法的资源
- ❌ 新增扩展包时容易冲突
- ❌ 不符合包管理系统架构
- ❌ 难以维护和扩展

---

## ✨ 目标架构

### 新目录结构（玩法隔离）

```
📁 /Users/zhuyong/trae-game/
│
├── 📁 GameModes/                        # 🎯 顶层玩法文件夹（新增）
│   │
│   ├── 📁 base_game/                     # 🔹 基础游戏（杀戮尖塔复刻版）
│   │   │
│   │   ├── 📁 Code/                      # C# 代码（玩法特定）
│   │   │   ├── CombatLogic.cs            # 战斗逻辑
│   │   │   ├── CardEffects.cs           # 卡牌效果
│   │   │   ├── EnemyBehaviors.cs        # 敌人行为
│   │   │   ├── EventHandlers.cs          # 事件处理
│   │   │   └── RelicAbilities.cs        # 圣物能力
│   │   │
│   │   ├── 📁 Config/                    # 配置数据
│   │   │   ├── cards.json               # 卡牌定义
│   │   │   ├── characters.json          # 角色定义
│   │   │   ├── enemies.json             # 敌人定义
│   │   │   ├── events.json              # 事件定义
│   │   │   ├── relics.json              # 圣物定义
│   │   │   ├── potions.json             # 药水定义
│   │   │   ├── effects.json             # 效果定义
│   │   │   ├── audio.json               # 音频配置
│   │   │   └── game_config.json         # ⭐ 游戏配置（新增）
│   │   │
│   │   ├── 📁 Scenes/                   # 场景文件
│   │   │   ├── CombatScene.tscn         # 战斗场景
│   │   │   ├── CharacterSelect.tscn     # 角色选择
│   │   │   ├── MainMenu.tscn            # 主菜单
│   │   │   ├── MapScene.tscn            # 地图场景
│   │   │   ├── EventPanel.tscn          # 事件面板
│   │   │   ├── ShopPanel.tscn           # 商店面板
│   │   │   ├── RestSitePanel.tscn      # 休息点面板
│   │   │   ├── SettingsPanel.tscn      # 设置面板
│   │   │   ├── GameOverScreen.tscn      # 游戏结束
│   │   │   ├── VictoryScreen.tscn       # 胜利画面
│   │   │   └── TutorialOverlay.tscn     # 教程覆盖层
│   │   │
│   │   ├── 📁 Resources/                # 🖼️ 资源文件（统一管理）
│   │   │   │
│   │   │   ├── 📁 Images/              # 图片资源
│   │   │   │   ├── Backgrounds/         # 背景
│   │   │   │   │   ├── glory.png
│   │   │   │   │   ├── hive.png
│   │   │   │   │   ├── overgrowth.png
│   │   │   │   │   └── underdocks.png
│   │   │   │   │
│   │   │   │   ├── Characters/          # 角色
│   │   │   │   │   ├── Ironclad.png
│   │   │   │   │   ├── Silent.png
│   │   │   │   │   ├── Defect.png
│   │   │   │   │   └── Watcher.png
│   │   │   │   │
│   │   │   │   ├── Enemies/             # 敌人
│   │   │   │   ├── Events/              # 事件
│   │   │   │   ├── Potions/             # 药水
│   │   │   │   └── Relics/              # 圣物
│   │   │   │
│   │   │   ├── 📁 Audio/               # 音频资源
│   │   │   │   ├── BGM/                 # 背景音乐
│   │   │   │   │   ├── Preview.ogg
│   │   │   │   │   ├── combat_*.wav
│   │   │   │   │   ├── main_menu.wav
│   │   │   │   │   ├── map.wav
│   │   │   │   │   ├── rest.wav
│   │   │   │   │   ├── shop.wav
│   │   │   │   │   ├── victory.wav
│   │   │   │   │   └── jingles_*.ogg
│   │   │   │   │
│   │   │   │   └── SFX/                 # 音效
│   │   │   │       ├── attack.wav
│   │   │   │       ├── block.wav
│   │   │   │       ├── damage.wav
│   │   │   │       └── ...
│   │   │   │
│   │   │   └── 📁 Icons/               # 图标资源
│   │   │       ├── Cards/               # 卡牌图标
│   │   │       ├── Enemies/             # 敌人图标
│   │   │       ├── Items/               # 物品图标
│   │   │       ├── Relics/              # 圣物图标
│   │   │       ├── Skills/              # 技能图标
│   │   │       ├── Achievements/        # 成就图标
│   │   │       ├── Rest/                # 休息图标
│   │   │       └── Services/            # 服务图标
│   │   │
│   │   └── 📄 package_config.json       # 包元数据配置
│   │
│   ├── 📁 frost_expansion/              # ❄️ 冰霜扩展（未来）
│   │   ├── Code/
│   │   ├── Config/
│   │   ├── Scenes/
│   │   ├── Resources/
│   │   └── package_config.json
│   │
│   ├── 📁 shadow_realm/                 # 🌑 暗影领域（未来）
│   │   ├── Code/
│   │   ├── Config/
│   │   ├── Scenes/
│   │   ├── Resources/
│   │   └── package_config.json
│   │
│   └── 📁 mech_warriors/                 # 🤖 机甲战士（未来）
│       ├── Code/
│       ├── Config/
│       ├── Scenes/
│       ├── Resources/
│       └── package_config.json
│
├── 📁 Scripts/                          # 🔧 框架代码（保持不变）
│   ├── Core/                            # 核心引擎
│   │   ├── GameManager.cs
│   │   ├── EventBus.cs
│   │   ├── GameInitializer.cs
│   │   └── ...
│   │
│   ├── Systems/                         # 系统模块
│   │   ├── UnitManager.cs
│   │   ├── WaveManager.cs
│   │   ├── ItemManager.cs
│   │   └── ...
│   │
│   ├── Database/                        # 数据库层
│   │   ├── CardDatabase.cs
│   │   ├── EnemyDatabase.cs
│   │   └── ...
│   │
│   ├── Packages/                        # 📦 包管理系统
│   │   ├── PackageManager.cs
│   │   ├── PackageModels.cs
│   │   ├── PackageConfig.cs
│   │   └── IPackageExtension.cs
│   │
│   ├── UI/                              # 通用UI组件
│   │   ├── PackageStoreUI.cs
│   │   ├── EnhancedMainMenu.cs
│   │   └── ...
│   │
│   └── Main.cs                          # 入口点
│
├── 📁 Tools/                            # 🔧 开发工具
│   ├── build_packages.py                # 打包工具
│   ├── local_cdn_server.py              # CDN服务器
│   └── start_test_environment.sh         # 测试启动脚本
│
├── 📁 test_cdn/                        # 🧪 测试CDN
│   └── packages/
│       ├── registry.json
│       └── base_game.zip
│
└── RoguelikeGame.csproj                  # 项目文件
```

---

## 🔄 迁移策略

### Phase 1: 创建新结构（安全，不破坏现有）

```bash
# 1. 创建顶层目录
mkdir -p GameModes

# 2. 创建 base_game 子目录
mkdir -p GameModes/base_game/{Code,Config,Scenes,Resources/{Images/{Backgrounds,Characters,Enemies,Events,Potions,Relics},Audio/{BGM,SFX},Icons/{Cards,Enemies,Items,Relics,Skills,Achievements,Rest,Services}}}
```

### Phase 2: 移动玩法特定文件

#### 配置文件
```bash
cp Config/Data/*.json GameModes/base_game/Config/
```

#### 场景文件
```bash
cp Scenes/*.tscn GameModes/base_game/Scenes/
```

#### 图片资源
```bash
cp -r Images/* GameModes/base_game/Resources/Images/
```

#### 音频资源
```bash
cp -r Audio/* GameModes/base_game/Resources/Audio/
```

#### 图标资源
```bash
cp -r Icons/* GameModes/base_game/Resources/Icons/
```

#### 包配置
```bash
cp Packages/base_game/base_game_config.json GameModes/base_game/package_config.json
```

### Phase 3: 创建符号链接（保持兼容）

为了不破坏现有代码，先使用符号链接：

```bash
# 在原位置创建链接到新位置
ln -s ../GameModes/base_game/Config Config/Data_base_game
ln -s ../GameModes/base_game/Scenes Scenes_base_game
# ... 其他类似
```

### Phase 4: 更新引用路径（渐进式）

#### 更新 PackageManager
```csharp
// 旧路径
var configPath = Path.Combine(state.InstalledPath, "base_game_config.json");

// 新路径
var configPath = Path.Combine("res://GameModes/base_game/", "package_config.json");
```

#### 更新 ConfigLoader
```csharp
// 动态加载不同玩法的配置
string basePath = $"res://GameModes/{currentPackageId}/";
LoadCards($"{basePath}Config/cards.json");
```

---

## 📐 架构优势

### ✅ **玩法隔离**
- 每个玩法完全独立
- 资源不会冲突
- 易于添加/删除

### ✅ **包管理友好**
- 直接对应包下载后的目录结构
- 一键打包整个玩法文件夹
- 支持热插拔

### ✅ **团队协作**
- 不同成员负责不同玩法
- 减少合并冲突
- 清晰的责任边界

### ✅ **扩展性强**
- 添加新玩法只需新建文件夹
- 共享框架代码（Scripts/）
- 自定义资源和代码

---

## ⚠️ 注意事项

### Godot 资源路径

Godot 使用 `res://` 前缀访问资源。移动后需确保：

1. **场景中的节点引用**
   ```gdscript
   # 旧: $TextureRect.texture = load("res://Images/Backgrounds/glory.png")
   # 新: $TextureRect.texture = load("res://GameModes/base_game/Resources/Images/Backgrounds/glory.png")
   ```

2. **C# 代码中的资源加载**
   ```csharp
   // 旧
   var texture = GD.Load<Texture2D>("res://Images/Characters/Ironclad.png");
   
   // 新
   var texture = GD.Load<Texture2D>($"res://GameModes/{currentMode}/Resources/Images/Characters/Ironclad.png");
   ```

3. **配置文件路径**
   ```csharp
   // 旧
   using var file = FileAccess.Open("res://Config/Data/cards.json", ...);
   
   // 新
   string modePath = $"res://GameModes/{PackageManager.Instance.CurrentPackageId}";
   using var file = FileAccess.Open($"{modePath}/Config/cards.json", ...);
   ```

---

## 🛠️ 实施建议

### 推荐做法：渐进式迁移

**Step 1**: 先创建目录结构（今天）
**Step 2**: 复制文件到新位置（测试用）
**Step 3**: 更新包管理系统指向新路径
**Step 4**: 删除旧文件（确认无问题后）

### 工具支持

我可以帮你：
1. ✅ 创建完整的目录结构
2. ✅ 编写自动化迁移脚本
3. ✅ 更新关键文件的路径引用
4. ✅ 生成迁移检查清单

---

## 📊 对比表

| 方面 | 旧结构 | 新结构 |
|------|--------|--------|
| **资源查找** | ❌ 困难（混在一起） | ✅ 快速（按玩法分类） |
| **添加扩展** | ⚠️ 风险高（可能覆盖） | ✅ 安全（独立文件夹） |
| **团队协作** | ❌ 冲突多 | ✅ 无冲突 |
| **包管理** | ⚠️ 需手动筛选 | ✅ 整体打包 |
| **代码清晰度** | ⚠️ 中等 | ✅ 高度清晰 |
| **可维护性** | ⚠️ 一般 | ✅ 优秀 |

---

## 🎯 下一步行动

你希望我现在：

**A) 创建完整的新目录结构** 
   - 创建所有文件夹
   - 复制base_game相关文件
   - 生成迁移脚本

**B) 只创建目录骨架**
   - 只建空文件夹
   - 你手动移动文件

**C) 先生成详细迁移计划**
   - 分析每个文件的依赖关系
   - 列出需要修改的所有路径
   - 制定分步实施方案

请选择一个方案，或者告诉我你的具体需求！🚀
