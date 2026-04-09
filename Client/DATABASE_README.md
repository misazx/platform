# 🎮 Roguelike Card Game Database System

基于杀戮尖塔2（Slay the Spire 2）风格的完整卡牌Roguelike游戏数据库系统

## 📋 数据库模块总览

```
Scripts/Database/
├── CharacterDatabase.cs      # 角色数据库 ✨
├── CardDatabase.cs          # 卡牌数据库 ✨
├── EnemyDatabase.cs         # 敌人数据库 ✨
├── RelicDatabase.cs         # 遗物/物品数据库 ✨
├── PotionDatabase.cs        # 药水数据库 ✨
├── EventDatabase.cs         # 事件数据库 ✨
├── AchievementSystem.cs     # 成就/统计系统 ✨
├── TimelineManager.cs       # 时间线/历史记录 ✨
└── GameDatabaseManager.cs   # 数据库管理器 ✨
```

## 🎯 核心功能

### 1. **角色数据库 (CharacterDatabase)** ✨

完整的角色管理系统：

```csharp
// 获取所有角色
var characters = CharacterDatabase.Instance.GetAllCharacters();

// 获取特定角色
var ironclad = CharacterDatabase.Instance.GetCharacter("ironclad");

// 按风格筛选
var aggressiveCharacters = CharacterDatabase.Instance.GetCharactersByStyle(PlayStyle.Aggressive);

// 获取新手友好角色
var beginnerChars = CharacterDatabase.Instance.GetBeginnerFriendlyCharacters(3f);
```

**已注册角色**：
- **铁甲战士** (Ironclad) - 新手友好，激进打法
- **静默猎手** (Silent) - 弃牌组合技专家
- **故障机器人** (Defect) - 充能球管理大师
- **储君** (Watcher) - 长线成长策略
- **亡灵契约师** (Necromancer) - 高风险高回报

### 2. **卡牌数据库 (CardDatabase)** ✨

全面的卡牌管理系统：

```csharp
// 注册新卡牌
CardDatabase.Instance.RegisterCard(new CardData {
    Id = "my_card",
    Name = "我的卡牌",
    Type = CardType.Attack,
    Damage = 10
});

// 搜索卡牌
var attackCards = CardDatabase.Instance.GetCardsByType(CardType.Attack);
var characterCards = CardDatabase.Instance.GetCharacterCards("ironclad");
var searchResults = CardDatabase.Instance.SearchCards("打击");
```

**卡牌类型**：
- `Attack` - 攻击卡
- `Skill` - 技能卡
- `Power` - 能力卡
- `Status` - 状态卡
- `Curse` - 诅咒卡

**稀有度**：
- `Basic` - 基础卡
- `Common` - 普通
- `Uncommon` - 稀有
- `Rare` - 史诗
- `Special` - 特殊

### 3. **敌人数据库 (EnemyDatabase)** ✨

详细的敌人数据系统：

```csharp
// 获取所有敌人
var enemies = EnemyDatabase.Instance.GetAllEnemies();

// 按类型获取
var normalEnemies = EnemyDatabase.Instance.GetNormalEnemies();
var eliteEnemies = EnemyDatabase.Instance.GetEliteEnemies();
var bossEnemies = EnemyDatabase.Instance.GetBossEnemies();

// 按位置获取
var exordiumEnemies = EnemyDatabase.Instance.GetEnemiesByLocation("exordium");
```

**敌人类型**：
- `Normal` - 普通敌人
- `Elite` - 精英敌人
- `Boss` - Boss敌人

**敌人行为模式**：
- `Aggressive` - 攻击型
- `Defensive` - 防御型
- `Support` - 辅助型
- `Summoner` - 召唤型

### 4. **遗物数据库 (RelicDatabase)** ✨

丰富的遗物收集系统：

```csharp
// 获取遗物
var relic = RelicDatabase.Instance.GetRelic("burning_blood");

// 按稀有度获取
var commonRelics = RelicDatabase.Instance.GetCommonRelics();
var rareRelics = RelicDatabase.Instance.GetRareRelics();
var bossRelics = RelicDatabase.Instance.GetBossRelics();

// 获取角色兼容遗物
var ironcladRelics = RelicDatabase.Instance.GetRelicsForCharacter("ironclad");

// 随机获取
var randomRare = RelicDatabase.Instance.GetRandomRelic(RelicTier.Rare, rng);
```

**遗物稀有度**：
- `Starter` - 初始遗物
- `Common` - 普通
- `Uncommon` - 稀有
- `Rare` - 史诗
- `Boss` - Boss遗物
- `Special` - 特殊
- `Shop` - 商店专属

### 5. **药水数据库 (PotionDatabase)** ✨

实用的药水系统：

```csharp
// 获取药水
var potion = PotionDatabase.Instance.GetPotion("health_potion");

// 按类型获取
var attackPotions = PotionDatabase.Instance.GetPotionsByType(PotionType.Attack);
var defensePotions = PotionDatabase.Instance.GetPotionsByType(PotionType.Defense);

// 随机获取
var randomPotion = PotionDatabase.Instance.GetRandomPotion(rng);
```

**药水类型**：
- `Attack` - 攻击类
- `Defense` - 防御类
- `Utility` - 功能类
- `Special` - 特殊类

### 6. **事件数据库 (EventDatabase)** ✨

随机事件系统：

```csharp
// 获取事件
var event_ = EventDatabase.Instance.GetEvent("big_fish");

// 按类型获取
var choiceEvents = EventDatabase.Instance.GetEventsByType(EventType.Choice);

// 按位置获取
var exordiumEvents = EventDatabase.Instance.GetEventsByLocation("exordium");

// 随机触发
var randomEvent = EventDatabase.Instance.GetRandomEvent("exordium", rng);
```

**事件类型**：
- `Choice` - 选择事件
- `Combat` - 战斗事件
- `Shop` - 商店事件
- `Rest` - 休息事件
- `Treasure` - 宝藏事件
- `Special` - 特殊事件
- `Curse` - 诅咒事件

### 7. **成就系统 (AchievementSystem)** ✨

完整的成就追踪：

```csharp
// 解锁成就
AchievementSystem.Instance.UnlockAchievement("first_victory");

// 更新进度
AchievementSystem.Instance.UpdateProgress("kill_100_enemies", 1);

// 记录游戏运行
AchievementSystem.Instance.RecordRun(new RunStatistics {
    CharacterId = "ironclad",
    FloorReached = 20,
    EnemiesDefeated = 150,
    Victory = true
});

// 查询成就
var unlocked = AchievementSystem.Instance.GetUnlockedAchievements();
var completion = AchievementSystem.Instance.OverallCompletion;
```

**成就类型**：
- `Progression` - 进度类
- `Combat` - 战斗类
- `Collection` - 收集类
- `Challenge` - 挑战类
- `Secret` - 秘密类

### 8. **时间线管理器 (TimelineManager)** ✨

详细的游戏历史记录：

```csharp
// 记录战斗开始
TimelineManager.Instance.AddCombatStart(floor: 3, room: 5, enemies);

// 记录战斗结束
TimelineManager.Instance.AddCombatEnd(floor, room, victory: true, damageTaken: 20, damageDealt: 150);

// 记录出牌
TimelineManager.Instance.AddCardPlayed("打击", cost: 1, floor, room);

// 记录获得遗物
TimelineManager.Instance.AddRelicObtained("燃烧之血", "boss_drop", floor, room);

// 记录使用药水
TimelineManager.Instance.AddPotionUsed("生命药水", floor, room);

// 记录触发事件
TimelineManager.Instance.AddEventTriggered("大鱼", "吃掉它", floor, room);

// 记录击败Boss
TimelineManager.Instance.AddBossDefeated("守护者", floor: 10);

// 记录死亡或胜利
TimelineManager.Instance.AddDeath(floor, room, cause: "被Boss击败");
TimelineManager.Instance.Victory(seed, totalDamage: 5000, totalCardsPlayed: 200);
```

**时间线事件类型**：
- `CombatStart/End` - 战斗开始/结束
- `CardPlayed` - 出牌
- `RelicObtained` - 获得遗物
- `PotionUsed` - 使用药水
- `EventTriggered` - 触发事件
- `ShopVisit` - 访问商店
- `RestUsed` - 使用休息点
- `BossDefeated` - 击败Boss
- `Death/Victory` - 死亡/胜利

### 9. **数据库管理器 (GameDatabaseManager)** ✨

统一管理所有数据库：

```csharp
// 初始化所有数据库
var manager = new GameDatabaseManager();

// 统一访问接口
var character = manager.GetCharacter("ironclad");
var card = manager.GetCard("strike_ironclad");
var enemy = manager.GetEnemy("cultist");
var relic = manager.GetRelic("anchor");
var potion = manager.GetPotion("health_potion");
var event_ = manager.GetEvent("big_fish");

// 批量查询
var allCharacters = manager.GetAllCharacters();
var allCards = manager.GetAllCards();
var allEnemies = manager.GetAllEnemies();
var allRelics = manager.GetAllRelics();
var allPotions = manager.GetAllPotions();
var allEvents = manager.GetAllEvents();

// 生成报告
var report = manager.GenerateDatabaseReport();
GD.Print(report);
```

## 🚀 快速开始

### 1. 初始化数据库系统

在主场景中添加 `GameDatabaseManager` 节点，它会自动初始化所有子数据库。

```csharp
// 或手动初始化
GetTree().Root.AddChild(new GameDatabaseManager());
```

### 2. 使用数据库

```csharp
// 示例：创建一个新角色
CharacterDatabase.Instance.RegisterCharacter(new CharacterData {
    Id = "custom_character",
    Name = "自定义角色",
    Description: "一个强大的自定义角色",
    MaxHealth: 100,
    StartingGold: 99,
    Style = PlayStyle.Hybrid,
    DifficultyRating = 4f
});

// 示例：添加新卡牌
CardDatabase.Instance.RegisterCard(new CardData {
    Id = "custom_strike",
    Name: "强力打击",
    Description: "造成 15 点伤害。",
    Cost: 2,
    Type = CardType.Attack,
    Damage = 15,
    CharacterId = "custom_character"
});
```

## 📊 数据库统计

初始化后的完整统计：

```
=== 游戏数据库报告 ===
生成时间: 2026-04-03 XX:XX:XX

--- 角色系统 ---
总角色数: 5
  - 铁甲战士 (Ironclad)
  - 静默猎手 (Silent)
  - 故障机器人 (Defect)
  - 储君 (Watcher)
  - 亡灵契约师 (Necromancer)

--- 卡牌系统 ---
总卡牌数: 5+ (可扩展)

--- 敌人系统 ---
总敌人数: 4+
  - 普通敌人: 2
  - 精英敌人: 1
  - Boss敌人: 1

--- 遗物系统 ---
总遗物数: 4+

--- 药水系统 ---
总药水数: 5+

--- 事件系统 ---
总事件数: 3+

--- 成就系统 ---
总成就数: 4+
已解锁: X
完成度: XX.X%
```

## 🔧 扩展指南

### 添加新角色

```csharp
CharacterDatabase.Instance.RegisterCharacter(new CharacterData {
    Id = "new_character",
    Name = "新角色",
    // ... 其他属性
});
```

### 添加新卡牌

```csharp
CardDatabase.Instance.RegisterCard(new CardData {
    Id = "new_card",
    Name = "新卡牌",
    Type = CardType.Attack,
    // ... 其他属性
});
```

### 添加新敌人

```csharp
EnemyDatabase.Instance.RegisterEnemy(new EnemyData {
    Id = "new_enemy",
    Name = "新敌人",
    Type = EnemyType.Normal,
    // ... 其他属性
});
```

### 添加新遗物

```csharp
RelicDatabase.Instance.RegisterRelic(new RelicData {
    Id = "new_relic",
    Name = "新遗物",
    Tier = RelicTier.Rare,
    // ... 其他属性
});
```

## 💡 最佳实践

1. **数据驱动**: 所有游戏内容都通过数据库配置
2. **统一管理**: 通过 GameDatabaseManager 统一访问
3. **扩展性**: 易于添加新的角色、卡牌、敌人等
4. **搜索功能**: 支持按类型、稀有度等条件搜索
5. **历史记录**: 完整的时间线追踪游戏过程

## 📚 与现有系统集成

### 结合物品系统

```csharp
// 将数据库中的遗物转换为游戏内物品
var relicData = RelicDatabase.Instance.GetRelic("burning_blood");
ItemManager.Instance.RegisterItem(new ItemData {
    Id = relicData.Id,
    Name = relicData.Name,
    Type = ItemType.Passive,
    Rarity = ItemRarity.Common,
    Stats = relicData.Effects
});
```

### 结合存档系统

```csharp
// 在存档中保存成就进度
var saveData = SaveSystem.Instance.CreateSaveData();
saveData.CustomData["achievements"] = AchievementSystem.Instance.GetAllAchievements();
saveData.CustomData["timeline"] = TimelineManager.Instance.GetAllEntries();
```

## 🎮 杀戮尖塔2特色功能

✅ **5个独特角色** - 每个都有独特的机制和玩法
✅ **完整卡池** - 攻击、技能、能力、状态、诅咒
✅ **丰富敌人** - 普通、精英、Boss三种难度
✅ **遗物收集** - 多种稀有度和效果
✅ **随机事件** - 选择驱动的叙事体验
✅ **成就系统** - 追踪玩家进度
✅ **时间线记录** - 详细的游戏历史

## 🔥 下一步建议

1. **创建UI界面** - 显示角色选择、卡牌收藏等
2. **添加更多内容** - 更多角色、卡牌、敌人
3. **实现游戏逻辑** - 战斗系统、商店系统等
4. **美术资源** - 卡牌图片、角色立绘等
5. **平衡调整** - 数值平衡和测试

---

**完整的杀戮尖塔2风格数据库系统已准备就绪！** 🃏🎲✨
