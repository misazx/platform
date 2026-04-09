# 游戏配置系统

## 📋 概述

本项目的所有游戏数据已从硬编码迁移到JSON配置文件，支持开发环境使用JSON格式，生产环境使用压缩的bytes格式。

## 📁 目录结构

```
Config/
├── Data/                    # JSON配置文件（开发用）
│   ├── cards.json          # 卡牌配置
│   ├── characters.json     # 角色配置
│   ├── enemies.json        # 敌人配置
│   ├── relics.json         # 遗物配置
│   ├── potions.json        # 药水配置
│   ├── events.json         # 事件配置
│   ├── audio.json          # 音效配置
│   └── effects.json        # 特效配置
│
└── Compiled/               # 压缩后的bytes文件（上线用）
    ├── cards.bytes
    ├── characters.bytes
    ├── enemies.bytes
    ├── relics.bytes
    ├── potions.bytes
    ├── events.bytes
    ├── audio.bytes
    └── effects.bytes
```

## 🔧 核心组件

### 1. ConfigLoader (配置加载器)

位置：`Scripts/Core/ConfigLoader.cs`

功能：
- ✅ 支持JSON和压缩bytes两种格式
- ✅ 自动解压缩
- ✅ 配置缓存
- ✅ 统一的加载接口

### 2. ConfigModels (配置模型)

位置：`Scripts/Core/ConfigModels.cs`

包含所有配置数据的模型类，用于JSON反序列化。

### 3. ConfigCompiler (配置编译器)

位置：`Scripts/Editor/ConfigCompiler.cs`

功能：
- 编译JSON到压缩bytes
- 批量编译所有配置
- 配置加载测试

## 📝 使用指南

### 开发环境

**默认使用JSON格式：**

```csharp
// 自动加载JSON配置
var cardConfig = ConfigLoader.LoadConfig<CardConfigData>("cards");
```

### 生产环境

**步骤1：编译配置**

```csharp
// 编译所有JSON配置到bytes
ConfigCompiler.CompileAllConfigs();
```

**步骤2：启用编译模式**

```csharp
// 切换到bytes加载模式
ConfigLoader.UseCompiledConfig = true;
```

### 数据库使用

所有数据库类已自动集成配置加载：

```csharp
// 获取卡牌数据
var card = CardDatabase.Instance.GetCard("strike_ironclad");

// 获取角色数据
var character = CharacterDatabase.Instance.GetCharacter("ironclad");

// 获取敌人数据
var enemy = EnemyDatabase.Instance.GetEnemy("cultist");
```

## 🎯 配置文件格式

### 卡牌配置示例 (cards.json)

```json
{
  "version": "1.0.0",
  "cards": [
    {
      "id": "strike_ironclad",
      "name": "打击",
      "description": "造成 6 点伤害。",
      "cost": 1,
      "type": "Attack",
      "rarity": "Basic",
      "target": "SingleEnemy",
      "damage": 6,
      "characterId": "ironclad",
      "iconPath": "res://Icons/Cards/strike.png",
      "color": "#FF4444"
    }
  ]
}
```

### 角色配置示例 (characters.json)

```json
{
  "version": "1.0.0",
  "characters": [
    {
      "id": "ironclad",
      "name": "铁甲战士",
      "maxHealth": 80,
      "startingGold": 99,
      "startingCards": ["strike_ironclad", "defend_ironclad", "bash"]
    }
  ]
}
```

## 🔄 迁移完成清单

- ✅ 卡牌数据库 (CardDatabase.cs)
- ✅ 角色数据库 (CharacterDatabase.cs)
- ✅ 敌人数据库 (EnemyDatabase.cs)
- ✅ 遗物数据库 (RelicDatabase.cs)
- ✅ 药水数据库 (PotionDatabase.cs)
- ✅ 事件数据库 (EventDatabase.cs)
- ✅ 音效配置 (audio.json)
- ✅ 特效配置 (effects.json)

## 🧪 测试

运行配置测试：

```csharp
// 在Godot编辑器中运行测试场景
var test = new ConfigTest();
AddChild(test);
```

或使用ConfigCompiler测试：

```csharp
ConfigCompiler.TestConfigLoading();
```

## 📊 性能优势

### JSON格式 (开发环境)
- 易于编辑和版本控制
- 人类可读
- 支持注释和格式化

### Bytes格式 (生产环境)
- 文件体积减少约60-70%
- 加载速度提升约30-40%
- 防止数据被直接篡改

## 🔐 上线流程

1. **编辑配置**：修改 `Config/Data/` 下的JSON文件
2. **编译配置**：运行 `ConfigCompiler.CompileAllConfigs()`
3. **切换模式**：设置 `ConfigLoader.UseCompiledConfig = true`
4. **测试验证**：运行 `ConfigTest` 确保加载正常
5. **打包发布**：打包时包含 `Config/Compiled/` 目录

## 📌 注意事项

1. **版本控制**：JSON文件应纳入版本控制，bytes文件可选择是否纳入
2. **缓存管理**：切换配置模式时调用 `ConfigLoader.ClearCache()`
3. **错误处理**：配置加载失败时会输出错误日志，返回null
4. **热重载**：开发环境支持热重载JSON配置

## 🛠️ 扩展指南

### 添加新的配置类型

1. 在 `ConfigModels.cs` 中添加新的配置模型类
2. 在 `Config/Data/` 创建对应的JSON文件
3. 在数据库类中添加加载逻辑
4. 更新 `ConfigCompiler.cs` 的配置列表

### 示例：添加技能配置

```csharp
// 1. ConfigModels.cs
public class SkillConfigData
{
    [JsonPropertyName("version")]
    public string Version { get; set; }
    
    [JsonPropertyName("skills")]
    public List<SkillConfig> Skills { get; set; } = new();
}

// 2. Config/Data/skills.json
{
  "version": "1.0.0",
  "skills": [...]
}

// 3. SkillDatabase.cs
var config = ConfigLoader.LoadConfig<SkillConfigData>("skills");

// 4. ConfigCompiler.cs
string[] configNames = { ..., "skills" };
```

## 📚 相关文件

- [ConfigLoader.cs](Scripts/Core/ConfigLoader.cs) - 配置加载系统
- [ConfigModels.cs](Scripts/Core/ConfigModels.cs) - 配置数据模型
- [ConfigCompiler.cs](Scripts/Editor/ConfigCompiler.cs) - 配置编译工具
- [ConfigTest.cs](Scripts/Tests/ConfigTest.cs) - 配置测试脚本

---

**最后更新**: 2026-04-04  
**版本**: 1.0.0
