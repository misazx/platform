# 🎮 Roguelike Game Framework - 完整版

基于 Godot 4.6 + C# 的完整 Roguelike 肉鸽游戏框架

## 📋 项目结构

```
Scripts/
├── Core/                    # 核心系统
│   ├── EventBus.cs         # 事件总线系统
│   ├── GameManager.cs      # 游戏主管理器
│   ├── GameInitializer.cs  # 游戏初始化器
│   └── RandomGenerator.cs  # 种子随机生成器
│
├── Systems/                 # 游戏系统
│   ├── UnitManager.cs      # 单位管理器（对象池）
│   ├── WaveManager.cs      # 波次管理系统
│   ├── ItemManager.cs      # 物品管理系统 ✨
│   ├── DamageSystem.cs     # 伤害系统 ✨
│   ├── LootSystem.cs       # 战利品系统 ✨
│   ├── SkillManager.cs     # 技能系统 ✨
│   ├── SaveSystem.cs       # 存档系统 ✨
│   └── ObjectPoolManager.cs # 对象池管理器 ✨
│
├── Generation/              # 程序化生成
│   ├── RoomGenerator.cs    # 房间生成器
│   └── DungeonGenerator.cs # 地牢生成器（支持4种布局）
│
├── Entities/                # 游戏实体
│   ├── Player.cs           # 玩家角色
│   └── EnemyAI.cs          # 敌人AI系统 ✨
│
└── UI/                      # 用户界面
    ├── HUD.cs              # 游戏界面
    └── MainMenu.cs         # 主菜单
```

## 🚀 快速开始

### 1. 初始化游戏

在主场景中添加 `GameInitializer` 节点：

```csharp
// 自动初始化
// 或者手动调用
GameInitializer.QuickStart(seed: 12345);
```

### 2. 开始新游戏

```csharp
GameManager.Instance.StartNewGame(seed: -1);
```

## 🎯 核心功能

### 1. **事件总线系统 (EventBus)**

解耦系统间通信：

```csharp
// 订阅事件
EventBus.Instance.Subscribe<int>(GameEvents.PlayerHealthChanged, (health) => {
    UpdateHealthUI(health);
});

// 发布事件
EventBus.Instance.Publish(GameEvents.PlayerHealthChanged, currentHealth);
```

### 2. **物品系统 (ItemManager)** ✨ 新增

完整的物品管理系统：

```csharp
// 注册物品
ItemManager.Instance.RegisterItem(new ItemData {
    Id = "health_potion",
    Name = "Health Potion",
    Type = ItemType.Consumable,
    Rarity = ItemRarity.Common,
    MaxStack = 5,
    Stats = new Dictionary<string, float> {
        { "heal_amount", 25f }
    }
});

// 生成物品
ItemManager.Instance.SpawnItem("health_potion", position);

// 拾取物品
ItemManager.Instance.PickupItem(item, player);
```

**物品类型**：
- `Weapon` - 武器
- `Armor` - 护甲
- `Consumable` - 消耗品
- `Passive` - 被动物品
- `Active` - 主动物品

**稀有度**：
- `Common` - 普通
- `Uncommon` - 优秀
- `Rare` - 稀有
- `Epic` - 史诗
- `Legendary` - 传说

### 3. **伤害系统 (DamageSystem)** ✨ 新增

完整的伤害计算系统：

```csharp
// 创建伤害信息
var damageInfo = DamageSystem.Instance.CreateDamageInfo(
    amount: 30,
    type: DamageType.Physical,
    source: attacker
);

// 应用伤害
var result = DamageSystem.Instance.ApplyDamage(damageInfo);

// 检查结果
GD.Print($"伤害: {result.OriginalDamage} -> {result.FinalDamage}");
GD.Print($"暴击: {result.WasCritical}");
GD.Print($"闪避: {result.WasDodged}");
```

**伤害类型**：
- `Physical` - 物理伤害
- `Magical` - 魔法伤害
- `Fire` - 火焰伤害
- `Ice` - 冰霜伤害
- `Lightning` - 闪电伤害
- `Poison` - 毒素伤害
- `True` - 真实伤害

### 4. **战利品系统 (LootSystem)** ✨ 新增

随机掉落系统：

```csharp
// 注册战利品表
LootSystem.Instance.RegisterLootTable(new LootTable {
    Id = "common_enemy",
    Name = "Common Enemy Drops",
    MinItems = 0,
    MaxItems = 2,
    Entries = new List<LootEntry> {
        new LootEntry {
            ItemId = "health_potion_small",
            Weight = 10f,
            Chance = 0.3f
        }
    }
});

// 掉落战利品
LootSystem.Instance.DropLootAtPosition("common_enemy", position);

// 从敌人掉落
LootSystem.Instance.DropLootFromEnemy("elite_enemy", enemy);
```

### 5. **技能系统 (SkillManager)** ✨ 新增

技能学习和使用系统：

```csharp
// 学习技能
SkillManager.Instance.LearnSkill("fireball", player);

// 升级技能
SkillManager.Instance.UpgradeSkill("fireball", player);

// 使用技能
SkillManager.Instance.UseSkill("fireball", caster, target, direction);

// 检查技能状态
bool isReady = SkillManager.Instance.IsSkillReady("fireball");
float cooldown = SkillManager.Instance.GetCooldownRemaining("fireball");
```

**技能类型**：
- `Active` - 主动技能
- `Passive` - 被动技能
- `Toggle` - 切换技能

**目标类型**：
- `Self` - 自身
- `SingleEnemy` - 单个敌人
- `AreaOfEffect` - 范围效果
- `Direction` - 方向
- `Position` - 位置

### 6. **敌人AI系统 (EnemyAI)** ✨ 新增

智能敌人AI：

```csharp
// AI类型
public enum AIType {
    Aggressive,  // 攻击型
    Passive,     // 被动型
    Neutral,     // 中立型
    Boss         // Boss
}

// AI状态
public enum AIState {
    Idle,    // 空闲
    Patrol,  // 巡逻
    Chase,   // 追击
    Attack,  // 攻击
    Flee,    // 逃跑
    Dead     // 死亡
}
```

**特性**：
- 自动寻路和追踪
- 攻击范围检测
- 逃跑机制
- 巡逻行为
- 战利品掉落

### 7. **存档系统 (SaveSystem)** ✨ 新增

完整的存档系统：

```csharp
// 创建存档数据
var saveData = SaveSystem.Instance.CreateSaveData();

// 保存游戏
SaveSystem.Instance.SaveGame(slot: 0, saveData);

// 加载游戏
var loadedData = SaveSystem.Instance.LoadGame(slot: 0);
SaveSystem.Instance.ApplySaveData(loadedData);

// 快速存档/读档
SaveSystem.Instance.QuickSave();
SaveSystem.Instance.QuickLoad();

// 检查存档
bool hasSave = SaveSystem.Instance.HasSave(slot: 0);
var saveTime = SaveSystem.Instance.GetSaveTime(slot: 0);

// 删除存档
SaveSystem.Instance.DeleteSave(slot: 0);
```

**存档内容**：
- 玩家属性和位置
- 物品栏
- 已学习技能
- 当前楼层和房间
- 游戏种子

### 8. **对象池管理器 (ObjectPoolManager)** ✨ 新增

高性能对象池：

```csharp
// 创建对象池
ObjectPoolManager.Instance.CreatePool<Bullet>(
    poolName: "bullets",
    initialSize: 50,
    maxSize: 100,
    createFunc: () => new Bullet(),
    resetAction: (bullet) => bullet.Reset()
);

// 从池中获取对象
var bullet = ObjectPoolManager.Instance.Get<Bullet>("bullets");

// 归还对象到池
ObjectPoolManager.Instance.Return("bullets", bullet);

// 归还所有对象
ObjectPoolManager.Instance.ReturnAll("bullets");

// 清空池
ObjectPoolManager.Instance.ClearPool("bullets");
```

**预置对象池**：
- `bullets` - 子弹池
- `effects` - 特效池
- `damage_numbers` - 伤害数字池

### 9. **单位管理器 (UnitManager)**

管理游戏中的所有单位：

```csharp
// 注册单位
UnitManager.Instance.RegisterUnit("Goblin", new UnitData {
    Name = "Goblin",
    Type = UnitType.Enemy,
    MaxHealth = 30,
    Attack = 5,
    Defense = 2,
    Speed = 100f
});

// 生成单位
var enemy = UnitManager.Instance.SpawnUnit("Goblin", position);

// 获取范围内的单位
var nearbyEnemies = UnitManager.Instance.GetUnitsInRange(
    center: playerPos,
    radius: 100f,
    filterType: UnitType.Enemy
);
```

### 10. **波次管理器 (WaveManager)**

管理敌人波次：

```csharp
// 初始化
WaveManager.Instance.Initialize(seed: 12345);

// 开始下一波
WaveManager.Instance.StartNextWave();

// 跳转到指定波次
WaveManager.Instance.SkipToWave(5);
```

### 11. **地牢生成器 (DungeonGenerator)**

程序化生成地牢：

```csharp
// 生成地牢
var dungeon = DungeonGenerator.Instance.GenerateDungeon(
    floor: 1,
    seed: 12345
);

// 获取房间信息
var room = DungeonGenerator.Instance.GetRoom(roomId);
var connectedRooms = DungeonGenerator.Instance.GetConnectedRooms(roomId);
```

**地牢布局类型**：
- `Linear` - 线性布局
- `Branching` - 分支布局
- `Loop` - 环形布局
- `Grid` - 网格布局

### 12. **玩家系统 (Player)**

玩家角色控制：

```csharp
// 玩家属性
public int MaxHealth { get; set; } = 100;
public int Attack { get; set; } = 10;
public int Defense { get; set; } = 5;
public float Speed { get; set; } = 200f;
public float DashSpeed { get; set; } = 500f;

// 玩家方法
player.TakeDamage(damage: 10);
player.Heal(amount: 20);
```

## 🎨 完整示例

### 创建一个敌人

```csharp
// 1. 注册敌人
UnitManager.Instance.RegisterUnit("Goblin", new UnitData {
    Name = "Goblin",
    Type = UnitType.Enemy,
    MaxHealth = 30,
    Attack = 5,
    Defense = 2,
    Speed = 100f,
    ScenePath = "res://Scenes/Units/Goblin.tscn"
});

// 2. 生成敌人
var enemy = UnitManager.Instance.SpawnUnit("Goblin", position);

// 3. 敌人会自动：
// - 巡逻
// - 检测玩家
// - 追击和攻击
// - 死亡时掉落战利品
```

### 创建一个物品

```csharp
// 1. 注册物品
ItemManager.Instance.RegisterItem(new ItemData {
    Id = "magic_sword",
    Name = "Magic Sword",
    Description = "+20 Attack, +5% Critical",
    Type = ItemType.Weapon,
    Rarity = ItemRarity.Rare,
    Stats = new Dictionary<string, float> {
        { "attack", 20f },
        { "critical_chance", 0.05f }
    }
});

// 2. 添加到战利品表
LootSystem.Instance.RegisterLootTable(new LootTable {
    Id = "elite_enemy",
    Entries = new List<LootEntry> {
        new LootEntry {
            ItemId = "magic_sword",
            Weight = 5f,
            Chance = 0.1f
        }
    }
});

// 3. 物品会自动从敌人掉落
```

## 📊 配置参数

### GameManager
- `MaxActiveUnits`: 最大活动单位数 (默认: 100)
- `TotalRooms`: 每层楼房间数 (默认: 10)

### WaveManager
- `MaxWaves`: 最大波次数 (默认: 10)
- `BaseDifficulty`: 基础难度 (默认: 1.0)
- `DifficultyScaling`: 难度缩放 (默认: 1.2)

### DungeonGenerator
- `MinRooms`: 最小房间数 (默认: 10)
- `MaxRooms`: 最大房间数 (默认: 20)
- `RoomSpacing`: 房间间距 (默认: 3)
- `BranchChance`: 分支概率 (默认: 0.3)
- `LoopChance`: 环路概率 (默认: 0.2)

### DamageSystem
- `CriticalMultiplier`: 暴击倍数 (默认: 2.0)
- `CriticalChance`: 暴击率 (默认: 0.1)
- `KnockbackMultiplier`: 击退倍数 (默认: 1.0)

### SaveSystem
- `AutoSave`: 自动存档 (默认: true)
- `AutoSaveInterval`: 自动存档间隔 (默认: 300秒)

### EnemyAI
- `DetectionRange`: 检测范围 (默认: 200)
- `AttackRange`: 攻击范围 (默认: 50)
- `AttackCooldown`: 攻击冷却 (默认: 1.5秒)
- `MoveSpeed`: 移动速度 (默认: 100)
- `PatrolRadius`: 巡逻半径 (默认: 100)
- `FleeHealthPercent`: 逃跑血量百分比 (默认: 0.2)

## 🔧 扩展框架

### 添加新的物品类型

1. 在 `ItemType` 枚举中添加新类型
2. 在 `ItemManager` 中添加处理逻辑

### 添加新的技能

1. 在 `SkillManager.LoadSkillDefinitions()` 中注册技能
2. 在 `ExecuteSkillEffect()` 中实现技能效果

### 自定义敌人AI

继承 `EnemyAI` 并重写状态方法

## 📝 最佳实践

1. **使用种子系统**: 确保可重复的游戏体验
2. **事件驱动**: 使用 EventBus 解耦系统
3. **对象池**: 使用 ObjectPoolManager 减少 GC
4. **状态管理**: 通过 GameManager 统一管理游戏状态
5. **模块化**: 每个系统独立，易于测试和维护
6. **性能优化**: 使用对象池和批量处理

## 🐛 调试

启用详细日志：

```csharp
// 所有管理器都会输出日志
// 查看控制台输出以调试
GD.Print("[GameManager] State changed: Playing");
```

## 📚 依赖

- Godot 4.6+
- .NET 8.0
- C# 12.0

## 🎯 系统清单

✅ **核心系统**
- EventBus - 事件总线
- GameManager - 游戏管理
- RandomGenerator - 随机生成

✅ **游戏系统**
- UnitManager - 单位管理
- WaveManager - 波次管理
- ItemManager - 物品管理 ✨
- DamageSystem - 伤害系统 ✨
- LootSystem - 战利品系统 ✨
- SkillManager - 技能系统 ✨
- SaveSystem - 存档系统 ✨
- ObjectPoolManager - 对象池 ✨

✅ **生成系统**
- RoomGenerator - 房间生成
- DungeonGenerator - 地牢生成

✅ **实体系统**
- Player - 玩家
- EnemyAI - 敌人AI ✨

✅ **UI系统**
- HUD - 游戏界面
- MainMenu - 主菜单

## 📄 许可证

MIT License

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

---

**完整的 Roguelike 游戏框架已准备就绪！** 🎮🎲✨
