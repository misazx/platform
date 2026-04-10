# C# → GDScript 玩法代码迁移方案

## 一、现状分析

### 1.1 代码规模
| 模块 | 文件数 | 代码行数 |
|------|--------|----------|
| Combat/ | 4 | ~2,100 |
| Cards/ | 2 | ~760 |
| Config/ | 2 | ~720 |
| Database/ | 8 | ~2,200 |
| Effects/ | 1 | ~477 |
| Enemies/ | 1 | ~383 |
| Entities/ | 2 | ~241 |
| Generation/ | 3 | ~920 |
| Relics/ | 2 | ~698 |
| Systems/ | 13 | ~5,500 |
| Audio/ | 2 | ~470 |
| UI/ | 12 | ~3,800 |
| Tests/ | 3 | ~880 (排除) |
| **合计** | **55** (可转换) | **~19,145** |

### 1.2 依赖关系（关键）
- **Godot API**: 58文件依赖 → GDScript 原生支持 ✅
- **RoguelikeGame.Core**: 42文件依赖 → 通过 autoload 单例访问 ✅
- **RoguelikeGame.Database**: 12文件依赖 → 同上 ✅
- **System.Linq**: 10文件 → GDScript 用 Array/Dict 方法替代 ⚠️
- **System.Text.Json**: 3文件 → Godot JSON 替代 ✅
- **Network/**: 6文件 → 已排除编译，暂不转换 ❌

### 1.3 Autoload 分层

```
┌─────────────────────────────────────────────┐
│  C# 共享基础设施层 (保留C#, 不迁移)           │
│  EventBus / GameManager / ResourceManager   │
│  UIManager / PackageManager / NetworkManager│
│  AuthSystem / ArtGenerator / ResourceGen    │
├─────────────────────────────────────────────┤
│  GDScript 玩法层 (C#→GDScript 迁移目标)      │
│  CardDatabase / EnemyDatabase / ...         │
│  CombatManager / MapGenerator / ShopManager │
│  AudioManager / ParticleManager / ...       │
│  所有 UI Panels / CombatHUD / MapView       │
└─────────────────────────────────────────────┘
```

## 二、架构设计

### 2.1 C# ↔ GDScript 通信桥接

GDScript 访问 C# 单例：
```gdscript
# GDScript 中调用 C# autoload
var event_bus = EventBus  # 直接用全局名
event_bus.emit_signal("game_state_changed", new_state)

var card_db = CardDatabase  # autoload 全局名
var cards = card_db.get_all_cards()
```

C# 调用 GDScript（通过信号）：
```csharp
// C# 中通过 EventBus 信号桥接
EventBus.Instance.EmitSignal(SignalName.GameStateUpdated, state);
```

### 2.2 目录结构规范

```
GameModes/base_game/
├── Code/                    # ← 删除(C#源码)
├── Scripts/                 # ← 新建(GDScript玩法代码)
│   ├── combat/
│   │   ├── combat_manager.gd
│   │   ├── sts_combat_system.gd
│   │   └── encounter_generator.gd
│   ├── cards/
│   │   ├── card_database.gd
│   │   └── sts_card_database.gd
│   ├── database/
│   │   ├── character_database.gd
│   │   ├── enemy_database.gd
│   │   ├── event_database.gd
│   │   ├── potion_database.gd
│   │   ├── relic_database.gd
│   │   ├── achievement_system.gd
│   │   ├── game_database_manager.gd
│   │   └── timeline_manager.gd
│   ├── systems/
│   │   ├── damage_system.gd
│   │   ├── shop_manager.gd
│   │   ├── loot_system.gd
│   │   ├── rest_site_manager.gd
│   │   ├── item_manager.gd
│   │   ├── achievement_manager.gd
│   │   ├── skill_manager.gd
│   │   ├── save_system.gd
│   │   ├── enhanced_save_system.gd
│   │   ├── unit_manager.gd
│   │   ├── wave_manager.gd
│   │   ├── object_pool_manager.gd
│   │   └── tutorial_manager.gd
│   ├── generation/
│   │   ├── dungeon_generator.gd
│   │   ├── map_generator.gd
│   │   └── room_generator.gd
│   ├── relics/
│   │   ├── sts_relic_system.gd
│   │   └── sts_expansion_system.gd
│   ├── audio/
│   │   ├── audio_manager.gd
│   │   └── audio_generator.gd
│   ├── effects/
│   │   └── particle_manager.gd
│   ├── enemies/
│   │   └── enemy_ai.gd
│   ├── entities/
│   │   ├── player.gd
│   │   └── bullet.gd
│   ├── ui/
│   │   ├── combat_hud.gd
│   │   ├── character_select.gd
│   │   ├── map_view.gd
│   │   ├── reward_screen.gd
│   │   └── hud.gd
│   └── panels/
│       ├── achievement_panel.gd
│       ├── achievement_popup.gd
│       ├── event_panel.gd
│       ├── game_over_screen.gd
│       ├── pile_view_panel.gd
│       ├── rest_site_panel.gd
│       ├── reward_panel.gd
│       ├── save_slot_panel.gd
│       ├── shop_panel.gd
│       ├── treasure_panel.gd
│       ├── tutorial_overlay.gd
│       └── victory_screen.gd
├── Config/                   # JSON配置(不变)
├── Scenes/                   # 场景(更新脚本引用)
└── Resources/                # 资源(不变)
```

### 2.3 迁移规则

| 规则 | 说明 |
|------|------|
| class_name | GDScript 类使用 `class_name` 注册为全局名 |
| @export | C# [Export] → GDScript @export |
| signal | C# [Signal] delegate → GDScript signal |
| _ready() | C# _Ready() → GDScript _ready() |
| _process() | C# _Process() → GDScript _process(delta) |
| 单例模式 | C# static Instance → GDScript autoload 自动单例 |
| List<T> | C# List → GDScript Array |
| Dictionary<K,V> | C# Dict → GDScript Dictionary |
| LINQ | C# .Where/.Select → GDScript Array.filter/map |
| JSON | C# System.Text.Json → GDScript JSON |
| async/await | C# async → GDScript await (有限支持) |

## 三、执行计划（分批）

### 批次1：核心数据层（Database + Config）
```
CardDatabase.gd, CharacterDatabase.gd, EnemyDatabase.gd,
EventDatabase.gd, PotionDatabase.gd, RelicDatabase.gd,
AchievementSystem.gd, GameDatabaseManager.gd,
TimelineManager.gd, ConfigLoader.gd, ConfigModels.gd
```
→ 11个文件，无UI依赖，纯数据逻辑

### 批次2：核心系统层（Systems 核心）
```
DamageSystem.gd, ShopManager.gd, LootSystem.gd,
RestSiteManager.gd, ItemManager.gd, SkillManager.gd,
SaveSystem.gd, EnhancedSaveSystem.gd, UnitManager.gd,
WaveManager.gd, ObjectPoolManager.gd, AchievementManager.gd
```
→ 12个文件，系统逻辑

### 批次3：战斗系统（Combat）
```
CombatManager.gd, StsCombatSystem.gd, EncounterGenerator.gd
```
→ 3个文件，最复杂的模块

### 批次4：生成系统（Generation）
```
DungeonGenerator.gd, MapGenerator.gd, RoomGenerator.gd
```
→ 3个文件

### 批次5：实体+效果+音频
```
Player.gd, Bullet.gd, EnemyAI.gd,
ParticleManager.gd, AudioManager.gd, AudioGenerator.gd
```
→ 6个文件

### 批次6：圣物+卡牌
```
StsRelicSystem.gd, StsExpansionSystem.gd,
CardDatabase.gd(Sts), StsCardDatabase.gd
```
→ 4个文件

### 批次7：UI层（最大批量）
```
CombatHUD.gd, MapView.gd, CharacterSelect.gd,
RewardScreen.gd, HUD.gd,
+ 12个 Panel
```
→ 18个文件

### 批次8：集成验证
- 更新 project.godot autoload (.cs → .gd)
- 更新场景 .tscn 脚本引用
- 排除 .cs 编译
- 运行测试验证

## 四、热更新机制

### 4.1 Godot 原生热重载
GDScript 在编辑器运行时按 F5 即可热重载所有脚本：
- 不需要重新编译 C#
- 不需要重启游戏
- 修改 .gd 文件后自动生效

### 4.2 包热更新流程
```
1. 用户下载包 → 解压到 GameModes/<package_id>/
2. PackageManager 加载包的 autoload 配置
3. Godot 自动注册 GDScript autoload
4. 切换场景时加载新的 GDScript 脚本
5. 无需重启游戏引擎
```

## 五、风险与缓解

| 风险 | 缓解措施 |
|------|---------|
| GDScript 性能低于 C# | 仅玩法逻辑用GDScript，性能敏感部分保留C# |
| 类型安全减弱 | 使用 class_name + @export 强类型注解 |
| C#/GDScript互调复杂 | 统一通过 EventBus 信号通信 |
| 大量文件转换出错 | 分批转换，每批验证编译 |
