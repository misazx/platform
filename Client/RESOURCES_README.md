# 🎮 游戏资源说明文档

## 📁 资源目录结构

```
trae-game/
├── Images/                    # 图像资源
│   ├── Characters/           # 角色肖像 (6个)
│   ├── Enemies/              # 敌人图像 (4+个)
│   ├── Relics/               # 遗物图像 (20个)
│   ├── Potions/              # 药水图像 (15个)
│   ├── Events/               # 事件图像 (10个)
│   └── Backgrounds/          # 背景图像 (4个)
│
├── Icons/                    # 图标资源
│   ├── Cards/                # 卡牌图标 (30个)
│   ├── Relics/               # 遗物图标 (20个)
│   ├── Potions/              # 药水图标
│   ├── Enemies/              # 敌人图标
│   ├── Rest/                 # 休息点图标 (5个)
│   ├── Achievements/         # 成就图标 (4个)
│   ├── Skills/               # 技能图标 (4个)
│   ├── Items/                # 物品图标 (4个)
│   └── Services/             # 服务图标
│
└── Audio/                    # 音频资源
    ├── BGM/                  # 背景音乐 (9首)
    └── SFX/                  # 音效 (12个)
```

## 🎨 美术资源

### 角色肖像 (Characters)
- **Ironclad.png** - 铁甲战士（红色主题）
- **Silent.png** - 静默猎人（绿色主题）
- **Defect.png** - 缺陷机器人（蓝色主题）
- **Watcher.png** - 守望者（紫色主题）
- **Necromancer.png** - 死灵法师（黑色主题）
- **Heir.png** - 继承者（金色主题）

### 背景图像 (Backgrounds)
- **glory.png** - 荣耀章节（暖色调）
- **hive.png** - 蜂巢章节（棕色调）
- **overgrowth.png** - 疯长章节（绿色调）
- **underdocks.png** - 地下码头章节（蓝色调）

### UI图标
#### 休息点图标 (Rest)
- heal.png - 治疗（绿色心形）
- upgrade.png - 升级（黄色箭头）
- recall.png - 召回（蓝色循环）
- smith.png - 锻造（橙色锤子）

#### 成就图标 (Achievements)
- FirstVictory.png - 首次胜利（金色奖杯）
- Kill100.png - 击杀100（红色骷髅）
- AllRelics.png - 收集所有遗物（紫色菱形）
- NoDamage.png - 无伤通关（绿色盾牌）

#### 技能图标 (Skills)
- fireball.png - 火球（橙红色）
- heal.png - 治疗（绿色心形）
- dash.png - 冲刺（蓝色箭头）
- iron_skin.png - 铁肤（灰色盾牌）

### 敌人图像 (Enemies)
- **Cultist.png** - 邪教徒（人形）
- **JawWorm.png** - 颚虫（野兽）
- **Lagavulin.png** - 拉加林（构造体）
- **TheGuardian.png** - 守护者（Boss）

### 遗物图像 (Relics)
- 20个程序化生成的遗物图像
- 每个遗物有独特的颜色和形状组合

### 药水图像 (Potions)
- 15个程序化生成的药水图像
- 每个药水有独特的颜色

## 🎵 音频资源

### 背景音乐 (BGM)
| 文件名 | 场景 | 特点 |
|--------|------|------|
| main_menu.wav | 主菜单 | 平静、C大调、80BPM |
| combat_normal.wav | 普通战斗 | 紧张、D小调、120BPM |
| combat_elite.wav | 精英战斗 | 激烈、A小调、140BPM |
| combat_boss.wav | Boss战斗 | 史诗、E小调、100BPM |
| shop.wav | 商店 | 平和、F大调、90BPM |
| rest.wav | 休息点 | 放松、G大调、60BPM |
| map.wav | 地图界面 | 冒险、C大调、70BPM |
| victory.wav | 胜利界面 | 凯旋、C大调、100BPM |
| game_over.wav | 游戏结束 | 悲伤、A小调、60BPM |

### 音效 (SFX)
| 文件名 | 用途 | 波形 |
|--------|------|------|
| card_play.wav | 打出卡牌 | 正弦波、800Hz |
| card_draw.wav | 抽牌 | 正弦波、600Hz |
| attack.wav | 攻击 | 锯齿波、200Hz |
| block.wav | 格挡 | 方波、400Hz |
| damage.wav | 受伤 | 白噪声、150Hz |
| enemy_hit.wav | 敌人受击 | 锯齿波、300Hz |
| enemy_death.wav | 敌人死亡 | 白噪声、100Hz |
| potion_use.wav | 使用药水 | 正弦波、700Hz |
| relic_activate.wav | 遗物激活 | 正弦波、900Hz |
| gold_pickup.wav | 拾取金币 | 正弦波、1000Hz |
| button_click.wav | 按钮点击 | 正弦波、500Hz |
| shop_buy.wav | 商店购买 | 正弦波、800Hz |

## 🔧 资源生成系统

### 自动生成
游戏启动时会自动检查资源完整性，缺失的资源会自动生成。

### 手动生成脚本
```bash
# 生成音频资源
python3 generate_audio.py

# 生成图像资源（需要PIL库）
python3 generate_images.py
```

### 强制重新生成
在游戏代码中调用：
```csharp
ResourceInitializer.Instance.ForceRegenerateResources();
```

## 📝 使用示例

### 加载角色肖像
```csharp
var portrait = ResourceLoader.Load<Texture2D>("res://Images/Characters/Ironclad.png");
```

### 播放背景音乐
```csharp
AudioManager.Instance.PlayBGM("combat_normal");
```

### 播放音效
```csharp
AudioManager.Instance.PlaySFX("card_play");
```

### 加载图标
```csharp
var icon = ResourceLoader.Load<Texture2D>("res://Icons/Rest/heal.png");
```

## 🎯 资源特点

### 程序化生成优势
1. **自动化** - 无需手动创建每个资源
2. **一致性** - 所有资源风格统一
3. **可扩展** - 轻松添加新资源
4. **轻量级** - 不占用大量存储空间
5. **版本控制** - 可重现的资源生成

### 音频特点
- 使用数学函数合成
- 不同场景有独特的音乐风格
- 音效简洁清晰，适合游戏反馈

### 图像特点
- 使用算法绘制
- 颜色主题鲜明
- 适合卡牌游戏风格

## 🔄 资源更新

如需更新资源：
1. 修改对应的生成器脚本
2. 运行生成脚本或重启游戏
3. 资源会自动更新

## 📊 资源统计

| 类型 | 数量 | 格式 | 大小估算 |
|------|------|------|----------|
| 角色肖像 | 6 | PNG | ~500KB |
| 背景图像 | 4 | PNG | ~2MB |
| UI图标 | 20+ | PNG | ~1MB |
| 敌人图像 | 4+ | PNG | ~300KB |
| 遗物图像 | 20 | PNG | ~1MB |
| 药水图像 | 15 | PNG | ~500KB |
| 事件图像 | 10 | PNG | ~1MB |
| 卡牌图标 | 30 | PNG | ~1MB |
| 背景音乐 | 9 | WAV | ~50MB |
| 音效 | 12 | WAV | ~2MB |
| **总计** | **130+** | - | **~60MB** |

## 🎨 自定义资源

### 添加新角色
1. 在 `ResourceGenerator.cs` 中添加角色定义
2. 指定颜色和特征
3. 运行生成器

### 添加新音效
1. 在 `generate_audio.py` 中添加配置
2. 指定频率、时长和波形
3. 运行生成脚本

### 替换资源
可以直接替换生成的文件，游戏会优先使用现有文件。

## 💡 提示

1. **性能优化** - 音频资源较大，建议使用OGG格式
2. **内存管理** - 及时释放不用的资源
3. **缓存策略** - 常用资源可以缓存
4. **版本兼容** - 更新资源后注意版本号

---

**生成时间**: 2024年
**生成工具**: 程序化生成系统
**版本**: 1.0.0
