# 🎮 杀戮尖塔2 - 游戏UI资源与结构完整文档

> **文档用途**: 本文档用于向AI设计工具（如豆包、MidJourney等）提供完整的游戏UI结构和资源清单，以便生成美观的UI设计方案
>
> **游戏类型**: Roguelike卡牌构筑游戏（类似杀戮尖塔Slay the Spire）
>
> **引擎**: Godot 4.x (C#)
>
> **分辨率基准**: 1280x720 (可适配1920x1080)

---

## 📋 目录

1. [UI系统架构总览](#1-ui系统架构总览)
2. [主界面与导航系统](#2-主界面与导航系统)
3. [核心游戏界面](#3-核心游戏界面)
4. [面板与弹窗系统](#4-面板与弹窗系统)
5. [战斗HUD详细结构](#5-combat-hud详细结构)
6. [地图系统](#6-map系统)
7. [图片资源清单](#7-图片资源清单)
8. [图标资源分类](#8-图标资源分类)
9. [颜色方案与样式规范](#9-颜色方案与样式规范)
10. [字体与排版规范](#10-字体与排版规范)
11. [动画与交互效果](#11-动画与交互效果)
12. [音频资源关联](#12-音频资源关联)

---

## 1. UI系统架构总览

### 1.1 界面层级结构

```
Main (根节点 - Node)
├── UILayer (CanvasLayer, Layer=10) - 最顶层UI
├── HUD (CanvasLayer, Layer=5) - 游戏内HUD层
├── EffectLayer (Node2D, ZIndex=100) - 特效层
└── AudioListeners (Node) - 音频监听器
```

### 1.2 界面状态机

```
MainMenuScene (主菜单)
    ↓ [开始新游戏]
CharacterSelect (角色选择)
    ↓ [确认角色]
MapView (地图界面)
    ├── → CombatScene (战斗场景)
    │       └── → RewardPanel (奖励面板)
    ├── → ShopPanel (商店)
    ├── → RestSitePanel (休息点/篝火)
    ├── → EventPanel (事件)
    └── → TreasurePanel (宝箱)
    ↓ [胜利]
VictoryScreen (胜利画面)
    ↓ [失败]
GameOverScreen (失败画面)
    ↓ [返回]
MainMenuScene (主菜单)
```

### 1.3 核心UI脚本文件位置

**主UI控制器:**
- `Client/Scripts/UI/MainMenuScene.cs` - 主菜单逻辑
- `Client/Scripts/Main.cs` - 主控制器（管理所有界面切换）

**游戏模式UI:**
- `GameModes/base_game/Code/UI/HUD.cs` - 通用HUD（楼层、房间、波次、血量）
- `GameModes/base_game/Code/UI/CombatHUD.cs` - **战斗HUD（最复杂）**
- `GameModes/base_game/Code/UI/MapView.cs` - 地图视图
- `GameModes/base_game/Code/UI/CharacterSelect.cs` - 角色选择
- `GameModes/base_game/Code/UI/RewardScreen.cs` - 奖励屏幕

**面板类 (Panels目录):**
- `Panels/AchievementPanel.cs` - 成就面板
- `Panels/AchievementPopup.cs` - 成就弹窗通知
- `Panels/EventPanel.cs` - 事件面板
- `Panels/GameOverScreen.cs` - 游戏结束
- `Panels/PileViewPanel.cs` - 牌堆查看器
- `Panels/RestSitePanel.cs` - 休息点面板
- `Panels/RewardPanel.cs` - 战斗奖励面板
- `Panels/SaveSlotPanel.cs` - 存档槽位
- `Panels/ShopPanel.cs` - 商店面板
- `Panels/TreasurePanel.cs` - 宝箱面板
- `Panels/TutorialOverlay.cs` - 教程覆盖层
- `Panels/VictoryScreen.cs` - 胜利屏幕
- `Panels/SettingsPanel.cs` - 设置面板

---

## 2. 主界面与导航系统

### 2.1 MainMenuScene (主菜单)

**场景文件**: `Client/Scenes/MainMenu.tscn`
**尺寸**: 全屏 (1280x720)
**背景色**: 深紫黑色 `Color(0.04, 0.03, 0.08, 1)`

#### 节点树结构:

```
MainMenuScene (Control, FullRect)
├── Background (ColorRect, FullRect) - 深色背景
│   └── color = Color(0.04, 0.03, 0.08, 1)
├── ParticleContainer (Node2D) - 粒子特效容器
├── TitleLabel (Label) - 游戏标题
│   ├── position = 居中偏上 (offset_top=100)
│   ├── size = 400x70
│   ├── text = "杀戮尖塔 2"
│   └── font_size = 48 (LabelSettings)
├── VBoxContainer (垂直布局容器) - 按钮组
│   ├── anchors = 底部居中
│   ├── size = 240x170
│   ├── separation = 16px
│   ├── StartButton (Button, 240x52) - "开始游戏"
│   ├── ContinueButton (Button, 240x52) - "继续游戏"
│   ├── AchievementButton (Button, 240x52) - "成就"
│   ├── SettingsButton (Button, 240x52) - "设置"
│   └── QuitButton (Button, 240x52) - "退出游戏"
└── VersionLabel (Label) - 版本号
    ├── position = 右下角
    ├── text = "v0.1.0 Alpha"
    └── alignment = 右对齐
```

#### 设计要点:
- ✨ **标题**: 大字号(48px)，居中显示，可添加发光效果
- 🔘 **按钮**: 统一尺寸240x52px，垂直排列，间距16px
- 🎨 **配色**: 深色背景 + 金色/白色文字按钮
- 💫 **动效建议**: 标题淡入动画、按钮hover缩放、背景粒子飘浮

---

### 2.2 SettingsPanel (设置面板)

**场景文件**: `Client/Scenes/SettingsPanel.tscn`
**类型**: 模态弹窗 (Modal Dialog)
**尺寸**: 500x400 (居中)

#### 节点树:

```
SettingsPanel (Control, FullRect)
├── Background (ColorRect, FullRect) - 半透明遮罩
│   └── color = Color(0, 0, 0, 0.7)
├── Panel (PanelContainer, 500x400) - 主面板
│   └── VBox (VBoxContainer)
│       ├── TitleLabel (Label) - "设置" (居中)
│       ├── MusicLabel (Label) - "音乐音量"
│       ├── MusicSlider (HSlider) - 默认值80%
│       ├── SFXLabel (Label) - "音效音量"
│       ├── SFXSlider (HSlider) - 默认值100%
│       └── CloseButton (Button) - "关闭"
```

#### 设计要点:
- 🎛️ **滑块控件**: HSlider，带数值显示
- 🌑 **遮罩层**: 70%不透明度黑色背景
- 📦 **面板**: 圆角边框，深色主题

---

## 3. 核心游戏界面

### 3.1 CharacterSelect (角色选择)

**场景文件**: `GameModes/base_game/Scenes/CharacterSelect.tscn`
**背景色**: `Color(0.05, 0.04, 0.1, 1)` (深蓝黑)

#### 完整节点树:

```
CharacterSelect (Control, FullRect)
├── Background (ColorRect, FullRect)
├── HeaderBar (HBoxContainer, 顶部栏, height=50)
│   ├── BackButton (Button, 100x40) - "返回"
│   ├── Spacer (弹性占位)
│   ├── TitleLabel (Label) - "选择角色" (居中)
│   └── Spacer2 (弹性占位)
├── CharGrid (GridContainer, 角色网格)
│   ├── position = 居中
│   ├── size = 960x400
│   └── columns = 5 (每行5个角色卡片)
├── DescriptionPanel (Panel, 底部描述区)
│   └── DescVBox (VBoxContainer)
│       ├── NameLabel (Label) - 角色名称 (居中)
│       ├── ClassLabel (Label) - 职业类别 (居中)
│       └── DescLabel (RichTextLabel) - 角色描述 (支持BBCode)
│           ├── size = 自动高度(min 70px)
│           └── bbcode_enabled = true
└── BottomBar (HBoxContainer, 底部操作栏)
    ├── SpacerLeft (弹性占位)
    ├── ConfirmButton (Button, 160x45) - "确认选择"
    └── SpacerRight (弹性占位)
```

#### 可选角色 (从资源文件夹推断):

| 角色 | 文件名 | 描述 |
|------|--------|------|
| 铁甲战士 | Ironclad.png | 近战战士 |
| 静默猎人 | Silent.png | 敏捷刺客 |
| 故障机器人 | Defect.png | 能量法师 |
| 观察者 | Watcher.png | 念力行者 |
| 继承者 | Heir.png | 新角色 |
| 死灵法师 | Necromancer.png | 召唤师 |

#### 设计要点:
- 🃏 **角色卡片**: 网格布局，5列，带hover高亮效果
- 📝 **描述面板**: 支持富文本(BBCode)，底部固定
- 🔘 **操作栏**: 固定在底部，居中的确认按钮

---

## 4. 面板与弹窗系统

### 4.1 通用面板模板

所有面板遵循统一的设计模式:

```
[PanelName] (Control, FullRect)
├── Background (ColorRect) - 半透明遮罩 (alpha=0.7-0.85)
├── Panel (PanelContainer) - 主内容区 (居中)
│   └── VBox (VBoxContainer)
│       ├── TitleLabel (标题)
│       ├── [内容区域...]
│       └── CloseButton/ActionButton (关闭/确认按钮)
```

### 4.2 EventPanel (事件面板)

**尺寸**: 600x400
**用途**: 显示随机事件和选择

```
EventPanel
├── Background (ColorRect, alpha=0.7)
├── Panel (PanelContainer, 600x400)
│   └── VBox
│       ├── TitleLabel - "事件"
│       ├── DescLabel (RichTextLabel, 500x150) - 事件描述
│       │   └── bbcode_enabled = true
│       ├── ChoicesContainer (VBoxContainer) - 动态选项按钮
│       └── CloseButton - "关闭"
```

### 4.3 ShopPanel (商店面板)

**尺寸**: 800x500

```
ShopPanel
├── Background (ColorRect, alpha=0.7)
├── Panel (PanelContainer, 800x500)
│   └── VBox
│       ├── Header (HBoxContainer)
│       │   ├── TitleLabel - "商店"
│       │   └── GoldLabel - "💰 99"
│       ├── ItemsGrid (GridContainer, columns=4) - 商品网格
│       └── CloseButton - "离开商店"
```

### 4.4 RestSitePanel (休息点/篝火)

**尺寸**: 500x400

```
RestSitePanel
├── Background (ColorRect, alpha=0.7)
├── Panel (PanelContainer, 500x400)
│   └── VBox
│       ├── TitleLabel - "篝火" (居中)
│       ├── DescLabel - "选择一个行动" (居中)
│       ├── RestButton - "休息 (回复30%生命)"
│       ├── SmithButton - "锻造 (升级一张卡)"
│       └── CloseButton - "离开"
```

**关联图标资源** (`Icons/Rest/`):
- `default.png` - 默认图标
- `heal.png` - 休息/治疗图标
- `recall.png` - 回忆/移除图标
- `smith.png` - 锻造/升级图标
- `upgrade.png` - 升级图标

### 4.5 VictoryScreen & GameOverScreen

**VictoryScreen (胜利)**:
- 尺寸: 500x400
- 背景色: `Color(0, 0.1, 0.2, 0.9)` (深青色)
- 内容:
  - "胜利!" 标题
  - "你成功登顶了尖塔!" 描述
  - "继续" 按钮
  - "返回主菜单" 按钮

**GameOverScreen (失败)**:
- 尺寸: 400x300
- 背景色: `Color(0.1, 0, 0, 0.9)` (深红色)
- 内容:
  - "游戏结束" 标题
  - "你倒下了..." 描述
  - "重新开始" 按钮
  - "返回主菜单" 按钮

### 4.6 AchievementPopup (成就通知)

**类型**: 临时通知Toast
**尺寸**: 400x80
**位置**: 顶部居中 (top=50)

```
AchievementPopup
└── Panel (PanelContainer, 400x80)
    └── HBox
        ├── IconRect (TextureRect, 48x48) - 成就图标
        └── VBox
            ├── TitleLabel - "成就解锁"
            └── DescLabel - 成就描述
```

**成就图标资源** (`Icons/Achievements/`):
- `FirstVictory.png` - 首胜
- `Kill100.png` - 击杀100敌人
- `NoDamage.png` - 无伤通关
- `AllRelics.png` - 收集全部遗物

### 4.7 TutorialOverlay (教程覆盖层)

**尺寸**: 700x500

```
TutorialOverlay
├── Background (ColorRect, alpha=0.8)
├── Panel (PanelContainer, 700x500)
│   └── VBox
│       ├── TitleLabel - "教程"
│       ├── ContentLabel (RichTextLabel, 600x300) - 教程内容
│       │   └── bbcode_enabled = true
│       ├── NextButton - "下一步"
│       └── SkipButton - "跳过教程"
```

### 4.8 RewardPanel (战斗奖励面板)

**尺寸**: 620x460
**特点**: 金色边框强调

```
RewardPanel (动态创建)
├── Background (ColorRect, alpha=0.85)
├── MainPanel (PanelContainer, 620x460)
│   ├── style: 圆角15px, 金色边框(3px), BgColor=(0.1,0.08,0.12)
│   └── VBox
│       ├── TitleLabel - "🏆 战斗奖励" (金色, 22px)
│       ├── GoldButton - "💰 获得 XX 金币" (500x42)
│       ├── CardLabel - "选择一张卡牌加入牌组:"
│       ├── [CardChoice Buttons] x3 (540x40 each)
│       ├── SkipButton - "跳过卡牌奖励" (200x38)
│       └── ContinueButton - "继续" (200x42)
```

### 4.9 PileViewPanel (牌堆查看器)

**尺寸**: 900x550
**用途**: 查看 抽牌堆/弃牌堆/手牌 的完整列表

```
PileViewPanel
├── Background (ColorRect, alpha=0.8)
├── MainPanel (PanelContainer, 900x550)
│   └── VBox
│       ├── TitleLabel - "{牌堆名} (X张)"
│       ├── ScrollContainer (860x420)
│       │   └── GridContainer (columns=5)
│       │       └── [CardPanel] x N (155x90 each)
│       │           ├── NameLabel + CostLabel
│       │           └── DescLabel
│       └── CloseButton - "关闭" (150x36)
```

---

## 5. Combat HUD 详细结构 ⭐️

**这是游戏最复杂的UI系统！**

**脚本文件**: `CombatHUD.cs`
**场景文件**: `CombatScene.tscn` (空壳，内容通过代码动态生成)
**尺寸**: 1280x720 (全屏)

### 5.1 整体布局分区

```
┌─────────────────────────────────────────────────────────────┐
│ TopBar (顶部信息栏) - height=26, y=4                       │
│ [第1层] ............ [你的回合] ............ [回合1] [⚡3/3] │
├─────────────────────────────────────────────────────────────┤
│ BattleSceneArea (战斗场景) - 1280x340, y=32                │
│   ┌─────────┐                              ┌──┬──┬──┐      │
│   │ Player  │                              │E1│E2│E3│      │
│   │ Sprite  │                              └──┴──┴──┘      │
│   └─────────┘                                                   │
├─────────────────────────────────────────────────────────────┤
│ EnemyStatusArea (敌人状态栏) - 1280x70, y=240               │
│        [Enemy1 HP Bar]     [Enemy2 HP Bar]    [Enemy3...]   │
├───────────────────────────┬─────────────────────────────────┤
│ PlayerStatusArea          │ HandArea (手牌区)                │
│ (玩家状态区)               │ 1240x130, y=415                 │
│ 220x80, y=330             │  [手牌: 5]                        │
│ ┌─────────────────┐      │ [C1][C2][C3][C4][C5]              │
│ │⚔️ 铁甲战士 80/80 │      │                                  │
│ │ ████████████░░░░ │      │                                  │
│ │ 🛡️ 12            │      │                                  │
│ │ [Relic][Relic]   │      │                                  │
│ └─────────────────┘      │                                  │
├───────────────────────────┴─────────────────────────────────┤
│ BottomBar (底部按钮栏) - 1280x42, y=555                      │
│   [📥 抽牌堆(45)]  [⚔️ 结束回合]  [📤 弃牌堆(0)]           │
└─────────────────────────────────────────────────────────────┘
```

### 5.2 详细组件说明

#### A. TopBar (顶部信息栏)

**位置**: (12, 4), **尺寸**: 1256x26

```csharp
HBoxContainer
├── FloorLabel (Label) - "第 1 层"
│   ├── color = Gray
│   └── font_size = 11
├── Spacer (ExpandFill)
├── PhaseLabel (Label) - "你的回合" / "敌方回合"
│   ├── color = Green(0.5, 0.9, 0.5) / Red(0.9, 0.4, 0.3)
│   └── font_size = 12
├── Spacer2 (ExpandFill)
├── TurnLabel (Label) - "回合 1"
│   ├── color = Gold(1, 0.92, 0.3)
│   └── font_size = 11
└── EnergyLabel (Label) - "⚡ 3/3"
    ├── color = Gold(1, 0.85, 0.2)
    └── font_size = 14
```

#### B. BattleCharacterSprite (战斗角色精灵)

**玩家精灵**:
- **尺寸**: 140x170
- **位置**: (60, 140)
- **边框颜色**: 蓝色 `(0.25, 0.4, 0.7, 0.8)`
- **内部结构**:
  ```
  PanelContainer (130x160, 圆角12px)
  └── VBoxContainer
      ├── NameLabel - "铁甲战士" (蓝色)
      └── TextureRect (110px高, KeepAspectCentered)
          └── texture = iron_sword.png (或角色立绘)
  ```

**敌人精灵**:
- **尺寸**: 160x200
- **位置**: 动态计算（居中分布，间距30px）
- **边框颜色**: 红色 `(0.7, 0.25, 0.25, 0.85)`
- **内部结构**: 同上，但名称为红色

#### C. EnemyUnitUI (敌人状态单元)

**尺寸**: 135x60 (body: 130x55)

```
EnemyUnitUI (Control)
└── Body (PanelContainer, 130x55, 圆角8px)
    ├── style: BgColor=(0.1,0.08,0.07), BorderColor=Red(0.65,0.22,0.22)
    └── VBoxContainer
        ├── TopRow (HBoxContainer)
        │   ├── HealthText (Label) - "45/65" (LightGray, 10px)
        │   └── IntentLabel (Label) - "攻击 12" (Gold, 10px, 右对齐)
        └── HealthBar (ProgressBar, 120x14)
            └── fill: Red(0.8, 0.2, 0.2), 圆角3px
            └── 低血量(<30%): DarkRed(0.55, 0.12, 0.12)
```

**目标选择状态**:
- 边框变为金色 `(1, 0.8, 0.2)`，宽度3px
- 可点击交互

#### D. PlayerStatusBar (玩家状态栏)

**位置**: (12, 330), **尺寸**: 220x80

```
VBoxContainer
├── HeaderRow (HBoxContainer)
│   ├── PlayerAvatar (TextureRect, 28x28) - iron_sword.png
│   ├── NameLabel - "铁甲战士" (White, 12px)
│   └── HealthText - "80/80" (Red, 11px)
├── HealthBar (ProgressBar, 210x16)
│   └── fill: Red(0.85, 0.2, 0.2), 圆角4px
├── BlockRow (HBoxContainer)
│   ├── BlockText - "🛡️ 12" (Blue, 10px)
│   └── BlockBar (ProgressBar, 140x10, 可见性动态控制)
│       └── fill: Blue(0.3, 0.5, 1, 0.9), 圆角3px
└── RelicsRow (HBoxContainer, separation=3px)
    └── [RelicIconTextureRect] x N (动态添加)
```

#### E. CardUI (卡牌UI) ⭐核心组件

**尺寸**: 105x130 (card body: 100x125)

```
CardUI (Control)
└── CardBody (PanelContainer, 100x125, 圆角7px)
    ├── style: BgColor=(0.15,0.12,0.1), BorderColor=ByType
    └── MainVBox (VBoxContainer)
        ├── HeaderHBox (HBoxContainer)
        │   ├── NameLabel - 卡牌名称 (White, 11px, 居中)
        │   └── CostLabel - 费用 (Gold/Green by cost, 12px)
        ├── IconRect (TextureRect, 40px高, KeepAspectCentered)
        │   └── texture = card icon (by type)
        ├── DescLabel - 卡牌描述 (Gray, 8px, 自动换行, 90x32)
        └── TypeLabel - 类型标签 (Gray, 8px, 居中)
            └── text: "⚔️ 攻击" / "🛡️ 技能" / "✨ 能力"
            └── color: Red / Blue / Gold
```

**卡牌类型配色方案**:

| 类型 | 边框颜色 | 类型标签颜色 | 图标默认路径 |
|------|----------|--------------|--------------|
| **Attack (攻击)** | `Red(0.75, 0.25, 0.25)` | `Red(1, 0.35, 0.35)` | `Icons/Cards/strike.png` |
| **Skill (技能)** | `Blue(0.25, 0.45, 0.8)` | `Blue(0.35, 0.6, 1)` | `Icons/Cards/defend.png` |
| **Power (能力)** | `Gold(0.8, 0.65, 0.2)` | `Gold(0.9, 0.75, 0.3)` | `Icons/Skills/fireball.png` |

**费用颜色**:
- **费用=0**: 绿色 `(0.4, 0.85, 0.4)` (免费!)
- **费用>0**: 金色 `(1, 0.85, 0.2)`

**交互行为**:
- **MouseEnter**: 卡牌上移20px (0.1s ease-out)
- **MouseExit**: 卡牌归位 (0.1s ease-out)
- **Click**: 触发出牌逻辑

**手牌布局算法**:
- 中心点: X=600
- 卡牌间距: 112px
- 扇形展开: Y偏移 = |X偏移| * 0.04 (弧形)
- 最多显示: 10张卡牌

#### F. BottomButtons (底部操作栏)

**位置**: (0, 555), **尺寸**: 1280x42, **ZIndex**: 10

```
HBoxContainer (居中对齐, separation=25px)
├── DrawPileBtn (Button, 120x34)
│   ├── text: "📥 抽牌堆 (45)"
│   ├── style: BgColor=DarkGreen(0.18,0.28,0.18), Border=Green(0.35,0.45,0.28)
│   └── 圆角: 8px, 边框: 2px
├── EndTurnBtn (Button, 140x38) ⭐主要按钮
│   ├── text: "⚔️ 结束回合"
│   ├── style: BgColor=DarkYellow(0.4,0.3,0.1), Border=Gold(1,0.75,0.2)
│   └── 圆角: 10px, 边框: 3px (加粗强调)
└── DiscardPileBtn (Button, 120x34)
    ├── text: "📤 弃牌堆 (0)"
    ├── style: BgColor=DarkRed(0.28,0.18,0.18), Border=Brown(0.45,0.3,0.28)
    └── 圆角: 8px, 边框: 2px
```

### 5.3 浮动文字系统 (FloatingText)

**用途**: 显示伤害/治疗/格挡等即时反馈

**实现类**: `FloatingTextLabel` (继承Control)

**类型**:
- **伤害**: `"-12"` - 红色 `(1, 0.3, 0.3)` - 字号22
- **格挡**: `"+5 🛡️"` - 蓝色 `(0.35, 0.6, 1)` - 字号18
- **治疗**: `"+8 ❤️"` - 绿色 `(0.35, 1, 0.4)` - 字号18
- **状态**: `"能量不足!"` - 橙色 `(0.9, 0.7, 0.3)` - 字号22

**动画**:
- 向上漂浮 70px (0.9s, ease-out)
- 淡出 1.3s (延迟0.4s后开始)
- 生命周期: 1.8s (自动销毁)
- **阴影**: 黑色75%不透明度, 偏移(2,2)

### 5.4 出牌动画系统

**触发条件**: 点击卡牌后执行

**动画流程**:
1. **起始位置**: 手牌中央 (540, 400)
2. **目标位置**:
   - 攻击牌+多敌人: 敌人精灵位置 + (0, -30)
   - 技能牌: 玩家左侧 (120, 300)
   - 能力牌: 中央上方 (540, 200)
3. **动画参数**:
   - 移动: 0.4s (ease-in, back过渡)
   - 缩放: 1.2x (0.2s)
   - 闪烁: 白色x2亮度 (0.2s)
   - 消失: 淡出+缩小到0.5x (0.3s, 延迟0.3s)
   - 总时长: 0.7s (自动QueueFree)

**出牌卡片外观**:
- 尺寸: 84x114 (比手牌稍大)
- 背景: 深灰 `(0.12, 0.1, 0.08)`
- 边框: 2px (按卡牌类型着色)
- ZIndex: 99 (卡片) / 100 (图标)

---

## 6. Map 系统

### 6.1 MapView (地图视图)

**场景文件**: `MapScene.tscn`
**背景**: overgrowth.png (纹理拉伸)

#### 节点结构:

```
MapView (Control, FullRect)
├── Background (TextureRect, FullRect)
│   └── texture = overgrowth.png (stretch_mode=Scale)
├── HeaderBar (HBoxContainer, height=45)
│   ├── BackButton (Button, 90x36) - "返回地图"
│   ├── FloorLabel (Label) - "第 1 层"
│   ├── GoldLabel (Label) - "💰 99" (右对齐)
│   └── SaveButton (Button, 90x36) - "💾 存档" (动态添加)
├── MapContainer (Control, 1000x500, 居中)
│   ├── GridBg (ColorRect, 半透明白色3%)
│   ├── NodeLayer (Control, FullRect) - 地图节点容器
│   │   └── [MapNodeUI] x N (动态生成)
│   ├── ConnectionLayer (Node2D) - 连线绘制层
│   │   └── [Line2D] x N (节点间连线)
│   └── [TestDebugRect] - 调试用 (开发阶段)
└── TooltipPanel (Panel, 默认隐藏)
    └── TooltipLabel (RichTextLabel, 220x60)
```

### 6.2 MapNodeUI (地图节点UI)

**尺寸**: 56x56
**形状**: 圆角方形 (圆角12px)

#### 视觉状态:

| 状态 | 透明度 | 缩放 | 可交互 | 边框颜色 | 背景色 |
|------|--------|------|--------|----------|--------|
| **可达 (Reachable)** | 100% | 1.0x → hover 1.15x | ✅ | 按类型 | 默认深色 |
| **已访问 (Visited)** | 85% | 0.9x | ❌ | 绿灰色 | 深绿色调 |
| **锁定 (Locked)** | 60% | 1.0x | ❌ | 灰色 | 默认深色 |
| **高亮 (Highlighted)** | 100% | 1.25x | - | 金色 | - |

#### 节点类型配置:

| 类型 | 图标字符 | 颜色 | Tooltip文字 | 示例 |
|------|----------|------|-------------|------|
| **Monster (怪物)** | ⚔ | 红 `(0.9, 0.35, 0.35)` | "普通敌人" | Cultist, JawWorm |
| **Elite (精英)** | ★ | 橙 `(1, 0.65, 0.2)` | "精英敌人" | Gremlin Nob |
| **Boss (Boss)** | 👑 | 深红 `(0.95, 0.2, 0.2)` | "Boss" | The Guardian |
| **Event (事件)** | ? | 绿 `(0.55, 0.8, 0.35)` | "事件" | 随机事件 |
| **Shop (商店)** | $ | 蓝 `(0.4, 0.7, 1)` | "商店" | 商人 |
| **Rest (休息)** | ♥ | 绿 `(0.35, 0.6, 0.35)` | "休息点" | 篝火 |
| **Treasure (宝箱)** | ◆ | 金 `(1, 0.88, 0.3)` | "宝箱" | 宝箱 |
| **Unknown (未知)** | ● | 灰 `(0.6, 0.6, 0.6)` | "起点" | 起点 |

#### 内部结构:

```
MapNodeUI (Control, 56x56)
├── BGRect (ColorRect, FullRect)
│   └── color = (0.15, 0.13, 0.10, 0.95)
├── IconLabel (Label, FullRect)
│   ├── text = 图标字符 (⚔★?等)
│   ├── font_size = 22
│   └── color = 按节点类型
└── Tooltip (Label, 不可见)
    ├── position = (28, -50) (节点上方)
    ├── font_size = 14
    └── z_index = 200
```

#### 连线样式:
- **Line2D**: 宽度3px
- **颜色**: 棕灰色 `(0.6, 0.55, 0.45, 0.8)`
- **ZIndex**: 0 (在节点下层)

---

## 7. 图片资源清单

### 7.1 背景图 (Backgrounds)

**路径**: `Resources/Images/Backgrounds/`

| 文件名 | 用途 | 推荐使用场景 |
|--------|------|--------------|
| `overgrowth.png` | 草地/自然 | 地图界面背景 |
| `hive.png` | 蜂巢/暗洞 | 战斗场景背景 |
| `glory.png` | 光荣/胜利 | 胜利画面背景 |
| `underdocks.png` | 地下码头 | 特殊关卡背景 |

**规格建议**: 1920x1080 或更高，支持平铺/拉伸

---

### 7.2 角色立绘 (Characters)

**路径**: `Resources/Images/Characters/`

| 文件名 | 角色 | 用途 |
|--------|------|------|
| `Ironclad.png` | 铁甲战士 | 战斗精灵、状态栏头像 |
| `Silent.png` | 静默猎人 | 战斗精灵 |
| `Defect.png` | 故障机器人 | 战斗精灵 |
| `Watcher.png` | 观察者 | 战斗精灵 |
| `Heir.png` | 继承者 | 新角色 |
| `Necromancer.png` | 死灵法师 | 新角色 |

**规格建议**: 透明PNG, 200x300px 左右，全身或半身像

---

### 7.3 敌人精灵 (Enemies)

**路径**: 
- `Resources/Images/Enemies/` (大图)
- `Resources/Icons/Enemies/` (小图标)

| 文件名 | 敌人名称 | 类型 | 描述 |
|--------|----------|------|------|
| `Cultist.png` | 邪教徒 | 普通 | 初期敌人 |
| `JawWorm.png` / `jaw_worm.png` | 颚虫 | 普通 | 初期敌人 |
| `Lagavulin.png` | 拉加维林 | 精英/Boss | 中期精英 |
| `TheGuardian.png` / `the_guardian.png` | 守护者 | Boss | 关卡Boss |

**规格**:
- 大图: 160x200px (战斗场景)
- 小图标: 64x64px (状态栏)

---

### 7.4 事件插图 (Events)

**路径**: `Resources/Images/Events/`

| 文件名 | 用途 |
|--------|------|
| `BigFish.png` | 大鱼事件 |
| `CursedTome.png` | 被诅咒的 Tome 事件 |
| `ShiningLight.png` | 闪耀之光事件 |
| `Event_0.png` ~ `Event_9.png` | 通用事件插槽 (10个) |

**规格建议**: 400x300px, 带氛围感的插画

---

### 7.5 药水图标 (Potions)

**路径**: `Resources/Images/Potions/`

**命名药水** (5个):
| 文件名 | 效果 |
|--------|------|
| `FirePotion.png` | 火焰药水 (伤害) |
| `BlockPotion.png` | 格挡药水 (护甲) |
| `EnergyPotion.png` | 能量药水 (回复能量) |
| `StrengthPotion.png` | 力量药水 (Buff) |
| `HealthPotion.png` | 生命药水 (治疗) |

**通用药水** (15个):
- `Potion_0.png` ~ `Potion_14.png`

**规格**: 64x64px, 透明背景, 易识别的瓶子/液体造型

---

### 7.6 遗物图标 (Relics)

**路径**:
- `Resources/Images/Relics/` (大图)
- `Resources/Icons/Relics/` (小图标 + UI组件素材)

**特殊遗物** (4个大图):
| 文件名 | 名称 | 描述 |
|--------|------|------|
| `Anchor.png` | 船锚 | 增加格挡 |
| `IceCream.png` | 冰淇淋 | 回合结束时治疗 |
| `Lantern.png` | 灯塔 | 照亮地图 |
| `BurningBlood.png` | 燃烧之血 | 战斗结束后治疗 |

**通用遗物** (20个):
- `Relic_0.png` ~ `Relic_19.png` (大图)
- `relic_0.png` ~ `relic_19.png` (小图标)

**UI组件素材库** (Relics目录下的按钮/复选框/箭头等):
- **按钮样式**: button_square_*, button_round_*, button_rectangle_* (各多种变体)
- **复选框**: check_square_*, check_round_* (各种状态组合)
- **箭头**: arrow_basic_*, arrow_decorative_* (四方向)
- **滑动条**: slide_horizontal_*, slide_vertical_* (各种颜色/宽度)
- **图标**: icon_circle, icon_cross, icon_square, icon_checkmark 等
- **星星**: star.png, star_outline.png

**规格**:
- 大图: 80x80px
- 小图标: 48x48px
- UI组件: 各种尺寸

---

## 8. 图标资源分类

### 8.1 卡牌图标 (Cards)

**路径**: `Resources/Icons/Cards/`

**基础卡牌** (5张):
| 文件名 | 卡名 | 类型 |
|--------|------|------|
| `strike.png` | 打击 | Attack |
| `defend.png` | 防御 | Skill |
| `bash.png` | 重击 | Attack |
| `cleave.png` | 横扫 | Attack |
| `iron_wave.png` | 铁浪 | Attack |

**职业特定**:
- `strike_ironclad.png` - 铁甲战士打击
- `defend_ironclad.png` - 铁甲战士防御

**通用卡牌** (30张):
- `card_0.png` ~ `card_29.png`

**规格**: 80x110px (接近卡牌比例)

---

### 8.2 技能图标 (Skills)

**路径**: `Resources/Icons/Skills/`

| 文件名 | 技能名 | 描述 |
|--------|--------|------|
| `fireball.png` | 火球术 | 能力卡默认图标 |
| `heal.png` | 治疗 | 治疗技能 |
| `dash.png` | 冲刺 | 移动技能 |
| `iron_skin.png` | 铁肤 | 防御技能 |

**规格**: 64x64px

---

### 8.3 物品图标 (Items)

**路径**: `Resources/Icons/Items/`

| 文件名 | 物品名 | 用途 |
|--------|--------|------|
| `iron_sword.png` | 铁剑 | 玩家默认头像/武器 |
| `steel_armor.png` | 钢甲 | 防具图标 |
| `health_potion_small.png` | 小血药 | 回复少量生命 |
| `health_potion_large.png` | 大血药 | 回复大量生命 |

**规格**: 48x48px ~ 64x64px

---

### 8.4 休息点图标 (Rest)

**路径**: `Resources/Icons/Rest/`

| 文件名 | 用途 |
|--------|------|
| `default.png` | 默认休息图标 |
| `heal.png` | 休息/治疗动作 |
| `recall.png` | 回忆/移除卡牌 |
| `smith.png` | 锻造/升级卡牌 |
| `upgrade.png` | 升级 |

**规格**: 64x64px

---

### 8.5 成就图标 (Achievements)

**路径**: `Resources/Icons/Achievements/`

| 文件名 | 成就名称 | 条件 |
|--------|----------|------|
| `FirstVictory.png` | 首次胜利 | 第一次通关 |
| `Kill100.png` | 百斩 | 累计击杀100敌人 |
| `NoDamage.png` | 无伤 | 无伤通关某关卡 |
| `AllRelics.png` | 收藏家 | 收集所有遗物 |

**规格**: 64x64px, 带徽章/奖杯风格

---

### 8.6 服务/其他图标 (Services)

**路径**: `Resources/Icons/Services/`

- `default.png` - 默认服务图标

---

## 9. 颜色方案与样式规范

### 9.1 全局色彩体系

#### 主色调 (Primary Colors):

| 用途 | 颜色值 | RGB | 说明 |
|------|--------|-----|------|
| **背景深色** | `#0A0A14` | (0.04, 0.03, 0.08) | 主菜单/通用背景 |
| **面板背景** | `#1A151F` | (0.1, 0.08, 0.12) | 面板/弹窗背景 |
| **卡片背景** | `#261E1A` | (0.15, 0.12, 0.1) | 卡牌/元素背景 |
| **边框默认** | `#8C6B40` | (0.55, 0.42, 0.25) | 通用边框 |

#### 功能色 (Functional Colors):

| 用途 | 颜色值 | RGB | 使用场景 |
|------|--------|-----|----------|
| **攻击/伤害** | `#D94A4A` | (0.85, 0.29, 0.29) | 攻击卡、HP条、伤害数字 |
| **技能/防御** | `#5973FF` | (0.35, 0.45, 1.0) | 技能卡、护盾条 |
| **能力/Buff** | `#E6B233` | (0.9, 0.7, 0.2) | 能力卡、能量、金币 |
| **治疗** | `#5AFF66` | (0.35, 1.0, 0.4) | 治疗数字、回复效果 |
| **普通敌人** | `#E65959` | (0.9, 0.35, 0.35) | 怪物节点 |
| **精英敌人** | `#FFA633` | (1.0, 0.65, 0.2) | 精英节点 |
| **Boss** | `#F23333` | (0.95, 0.2, 0.2) | Boss节点 |
| **事件** | `#8CCC5A` | (0.55, 0.8, 0.35) | 事件节点 |
| **商店** | `#66B3FF` | (0.4, 0.7, 1.0) | 商店节点 |
| **休息** | `#5A995A` | (0.35, 0.6, 0.35) | 休息节点 |
| **宝箱** | `#FFE14D` | (1.0, 0.88, 0.3) | 宝箱节点 |

#### 状态色 (Status Colors):

| 状态 | 颜色值 | RGB | 说明 |
|------|--------|-----|------|
| **可用/正常** | `#FFFFFF` | (1, 1, 1) | 白色 |
| **禁用/锁定** | `#666666` | (0.4, 0.4, 0.4) | 灰色 |
| **你的回合** | `#80E680` | (0.5, 0.9, 0.5) | 亮绿色 |
| **敌方回合** | `#E6664D` | (0.9, 0.4, 0.3) | 橙红色 |
| **选中/高亮** | `#FFCC33` | (1.0, 0.8, 0.2) | 金色 |
| **低血量警告** | `#8C1F1F` | (0.55, 0.12, 0.12) | 深红色 |

---

### 9.2 圆角规范

| 元素类型 | 圆角半径 | 示例 |
|----------|----------|------|
| **大型面板** | 15px | RewardPanel, PileViewPanel |
| **中型面板** | 12px | 地图节点、角色精灵框架 |
| **小型面板** | 8~10px | 敌人状态单元、按钮 |
| **卡片/元素** | 7px | CardUI |
| **进度条** | 3~4px | HP条、Block条 |
| **微型元素** | 3px | 进度条填充 |

---

### 9.3 边框规范

| 用途 | 宽度 | 颜色 | 示例 |
|------|------|------|------|
| **通用边框** | 2px | Brown(0.55,0.42,0.25) | 卡牌默认 |
| **强调边框** | 3px | Gold(0.8,0.65,0.2) | 奖励面板 |
| **主按钮** | 3px | BrightGold(1,0.75,0.2) | 结束回合 |
| **次级按钮** | 2px | 按功能色 | 抽牌/弃牌堆 |
| **目标选中** | 3px | Gold(1,0.8,0.2) | 敌人可选择 |

---

### 9.4 阴影与光效

**当前实现**: 主要依赖Modulate（颜色调制）和简单Tween动画

**建议增强** (供AI设计参考):
- 📌 **面板投影**: 下方偏移 (0, 4px), 模糊10px, 黑色30%
- 📌 **卡片悬浮**: 下方偏移 (0, 8px), 模糊16px, 黑色40%
- 📌 **按钮按下**: 内嵌阴影 (inset), 偏移(0, 2px)
- 📌 **发光效果**: 重要元素外发光 (如能量、低血量警告)
- 📌 **粒子特效**: 主菜单背景、胜利/失败画面

---

## 10. 字体与排版规范

### 10.1 字号层级

| 层级 | 字号 | 用途 | 示例 |
|------|------|------|------|
| **超大标题** | 48px | 游戏标题 | "杀戮尖塔 2" |
| **大标题** | 22~24px | 面板标题 | "🏆 战斗奖励" |
| **中标题** | 16~18px | 区块标题 | "选择一张卡牌加入牌组:" |
| **正文** | 11~14px | 一般文本 | 卡牌描述、按钮文字 |
| **小字** | 8~10px | 辅助信息 | 类型标签、费用数字 |
| **微型** | 10~11px | 状态信息 | 回合数、层数、HP数字 |

### 10.2 对齐方式

| 元素 | 对齐 | 说明 |
|------|------|------|
| **标题** | 居中 (Center) | 面板标题、屏幕标题 |
| **按钮文字** | 居中 (Center) | 所有按钮 |
| **标签** | 居左 (Left) | 表单字段名 |
| **数值** | 居右 (Right) | 金币、HP、费用 |
| **正文** | 两端/单词换行 | 描述文本 (AutowrapMode.WordSmart) |

### 10.3 特殊排版

- **BBCode支持**: RichTextLabel 启用 BBCode，可用于：
  - `[color=red]红色文字[/color]`
  - `[b]粗体[/b]`
  - `[i]斜体[/i]`
- **Emoji使用**: 已在多处使用 (⚡💰📥📤⚔️🛡️❤️🏆✅等)
- **字体阴影**: FloatingText 使用 shadow_offset(2,2), color=Black(75%)

---

## 11. 动画与交互效果

### 11.1 过渡动画库

**引擎**: Godot Tween系统

#### 常用动画参数:

| 动画类型 | 时长 | 缓动函数 | 使用场景 |
|----------|------|----------|----------|
| **悬停上浮** | 0.1s | Ease-Out | 卡牌hover |
| **模态淡入** | 0.2~0.3s | Ease-InOut | 面板出现 |
| **点击反馈** | 0.1s | (缩放0.95→1) | 按钮按下 |
| **出牌飞行** | 0.4s | Ease-In + Back | 卡牌飞向目标 |
| **伤害闪烁** | 0.08s→0.12s | (白闪) | 受击反馈 |
| **浮动文字** | 0.9s (上浮) + 1.3s (淡出) | Ease-Out | 伤害/治疗数字 |
| **节点高亮** | 0.1s | Ease-Out | 地图节点hover |
| **死亡消失** | 0.5s | Ease-In | 敌人死亡 (淡出+缩小) |

### 11.2 交互动效清单

#### 卡牌系统:
- ✅ **Hover**: 上移20px + 轻微放大
- ✅ **Click**: 出牌动画 (飞向目标 + 闪光 + 消失)
- ✅ **Drag** (可选): 拖拽出手牌区域

#### 战斗系统:
- ✅ **敌人攻击**: 敌人精灵前冲后撤 (0.24s)
- ✅ **玩家受击**: 玩家精灵白闪 (0.2s)
- ✅ **敌人受击**: 敌人精灵白闪 (0.2s)
- ✅ **敌人死亡**: 淡出+缩小到0.8x (0.5s)

#### 地图系统:
- ✅ **节点Hover**: 放大到1.15x (0.1s)
- ✅ **节点选中**: 放大到1.25x (持续高亮)
- ✅ **节点访问**: 缩小到0.9x + 变灰绿色

#### 界面切换:
- ✅ **面板弹出**: 从中心缩放出现 (可选)
- ✅ **场景切换**: 淡入淡出过渡 (待实现)

---

## 12. 音频资源关联

### 12.1 背景音乐 (BGM)

**路径**: `Resources/Audio/BGM/`

| 文件名 | 用途 | 触发时机 |
|--------|------|----------|
| `main_menu.wav` | 主菜单音乐 | MainMenuScene |
| `map.wav` / `map.wav.import` | 地图探索音乐 | MapView |
| `combat_normal.wav` | 普通战斗BGM | CombatScene (普通敌) |
| `combat_elite.wav` | 精英战斗BGM | CombatScene (精英) |
| `combat_boss.wav` | Boss战BGM | CombatScene (Boss) |
| `rest.wav` | 休息点音乐 | RestSitePanel |
| `shop.wav` | 商店音乐 | ShopPanel |
| `victory.wav` | 胜利音乐 | VictoryScreen |
| `game_over.wav` | 失败音乐 | GameOverScreen |
| `Preview.ogg` | 预览/测试用 | 开发调试 |

**Kenney音效包** (备选):
- `jingles_HIT00.ogg` ~ `jingles_HIT16.ogg` (17个) - 打击音效
- `jingles_NES00.ogg` ~ `jingles_NES16.ogg` (17个) - 8-bit风格音效
- `jingles_PIZZI00.ogg` ~ `jingles_PIZZI16.ogg` (17个) - 其他风格音效
- `jingles_SAX00.ogg` ~ `jingles_SAX16.ogg` (17个) - 萨克斯风格
- `jingles_STEEL00.ogg` - 钢琴风格

### 12.2 音效 (SFX)

**路径**: `Resources/Audio/SFX/`

#### 核心音效:

| 文件名 | 用途 | 触发时机 |
|--------|------|----------|
| `button_click.wav` | 按钮点击 | 所有按钮交互 |
| `attack.wav` | 攻击音效 | 玩家攻击 |
| `damage.wav` | 受伤音效 | 角色/敌人受伤 |
| `block.wav` | 格挡音效 | 产生护甲 |
| `heal.wav` | 治疗音效 | 回复生命 |
| `enemy_death.wav` | 敌人死亡 | 敌人HP归零 |
| `enemy_hit.wav` | 敌人被击中 | 敌人受伤 |
| `card_draw.wav` | 抽牌音效 | 抽牌 |
| `card_play.wav` | 出牌音效 | 打出卡牌 |
| `gold_pickup.wav` | 拾取金币 | 获得金币奖励 |
| `potion_use.wav` | 使用药水 | 使用药水 |
| `relic_activate.wav` | 遗物激活 | 遗物触发效果 |
| `victory.wav` | 胜利音效 | 战斗胜利 |
| `game_over.wav` | 失败音效 | 战斗失败 |

#### UI交互音效 (Kenney UI Sounds):

**后退/取消**:
- `back_002.ogg`, `back_003.ogg`

**关闭**:
- `close_001.ogg`, `close_003.ogg`

**确认/选择**:
- `confirmation_004.ogg`
- `select_004.ogg`, `select_006.ogg`, `select_007.ogg`

**错误/无效操作**:
- `error_004.ogg`, `error_006.ogg`

**放下/丢弃**:
- `drop_002.ogg`

**滚动**:
- `scroll_004.ogg`

**切换/开关**:
- `switch_004.ogg`, `toggle_001.ogg`, `toggle_004.ogg`

**最大化/最小化**:
- `maximize_001.ogg` ~ `maximize_009.ogg` (多个变体)
- `minimize_002.ogg` ~ `minimize_008.ogg` (多个变体)

**故障/科技感**:
- `glitch_001.ogg`, `glitch_004.ogg`

**刮擦/摩擦**:
- `scratch_001.ogg` ~ `scratch_005.ogg`

**滴答/计时**:
- `tick_004.ogg`

#### 环境音效 (Footsteps):

**地毯**: `footstep_carpet_000.ogg` ~ `footstep_carpet_004.ogg` (5个)
**混凝土**: `footstep_concrete_000.ogg` ~ `footstep_concrete_004.ogg` (5个)
**草地**: `footstep_grass_000.ogg` ~ `footstep_grass_004.ogg` (5个)
**雪地**: `footstep_snow_000.ogg` ~ `footstep_snow_004.ogg` (5个)
**木板**: `footstep_wood_000.ogg` ~ `footstep_wood_004.ogg` (5个)

#### 打击/碰撞音效 (Impacts):

**玻璃**:
- `impactGlass_light/hedium/heavy_000~004` (15个)

**金属**:
- `impactMetal_light/hedium/heavy_000~004` (15个)

**通用**:
- `impactGeneric_light_000~004` (5个)

**采矿**:
- `impactMining_000~004` (5个)

**木板**:
- `impactPlank_medium_000~004` (5个)

**盘子**:
- `impactPlate_light/hedium/heavy_000~004` (15个)

**拳头**:
- `impactPunch_heavy_000~004` (5个)

**铃铛**:
- `impactBell_heavy/light_000~004` (10个)

---

## 📊 资源统计汇总

### 图片资源总数: **约150+ 张**

| 分类 | 数量 | 主要用途 |
|------|------|----------|
| 背景图 | 4 | 场景背景 |
| 角色立绘 | 6 | 战斗精灵 |
| 敌人精灵 | 4~8 | 战斗/状态栏 |
| 事件插图 | 13 | 事件面板 |
| 药水图标 | 20 | 药水系统 |
| 遗物图标 | 24+ | 遗物系统 + UI素材库 |
| 卡牌图标 | 35 | 卡牌面 |
| 技能图标 | 4 | 技能卡 |
| 物品图标 | 4 | 装备/道具 |
| 休息点图标 | 5 | 休息点动作 |
| 成就图标 | 4 | 成就系统 |
| UI组件素材 | 100+ | 按钮/复选框/箭头/滑动条等 |

### 音频资源总数: **约200+ 个**

| 分类 | 数量 |
|------|------|
| BGM | 12 + 68(kenney) |
| 核心SFX | 16 |
| UI交互音效 | 50+ |
| 环境音效 | 25 |
| 打击音效 | 75+ |

### UI场景文件: **15 个**

### UI脚本文件: **20+ 个**

---

## 🎨 给AI设计工具的提示词 (Prompt Suggestions)

### 用于生成UI设计的提示词示例:

#### 1. 主菜单设计:
```
Design a dark fantasy game main menu for a roguelike card game called "杀戮尖塔 2" (Slay the Spire 2).
Style: Dark purple-black background (#0A0A14), golden accents, particle effects.
Layout: Center-aligned title (48px, golden glow), vertical button stack (240x52px each, 16px spacing).
Buttons: "Start Game", "Continue", "Achievements", "Settings", "Quit".
Atmosphere: Mysterious, epic, slightly ominous. Add floating particles/embers in background.
Resolution: 1920x1080 (scalable to 1280x720).
```

#### 2. 战斗HUD设计:
```
Create a detailed combat HUD interface for a deck-building roguelike game.
Layout (1280x720):
- Top bar (26px height): Floor label (gray), Phase indicator (green/red), Turn counter (gold), Energy display (gold lightning icon)
- Battle area (340px height): Player character sprite (left, blue border), Enemy sprites (right, red borders, up to 3)
- Enemy status bars (70px height): HP bars with intent indicators
- Player status (left bottom, 220x80): Avatar, name, HP bar (red), Shield bar (blue), relic icons
- Hand card area (bottom center, 1240x130): Fan-spread cards (105x130 each, max 10)
- Bottom action bar (42px height): Draw pile button (green), End Turn button (golden, prominent!), Discard pile button (red)

Card design: Dark background (#1A151F), colored borders by type (Attack=Red, Skill=Blue, Power=Gold), cost indicator (top-right, gold for cost>0, green for 0), icon, description.

Color scheme: Very dark backgrounds (#0A0A14 to #1A151F), high contrast text, functional colors for game elements.
Include: Damage floating numbers (red, floating up), block indicators (blue), heal numbers (green).
```

#### 3. 地图界面设计:
```
Design a node-based map interface for a roguelike game dungeon crawler.
Background: Textured dark fantasy (overgrowth/forest ruin aesthetic).
Layout: Full screen with top header bar (back button, floor label, gold display, save button).
Central map area (1000x500): Grid of interconnected nodes.

Node types and styling (56x56 rounded squares, 12px radius):
- Monster nodes: Red (#E65959), sword icon "⚔"
- Elite nodes: Orange (#FFA633), star icon "★"
- Boss node: Dark red (#F23333), crown icon "👑"
- Event nodes: Green (#8CCC5A), question mark "?"
- Shop nodes: Blue (#66B3FF), dollar sign "$"
- Rest nodes: Green (#5A995A), heart "♥"
- Treasure nodes: Gold (#FFE14D), diamond "◆"

Connections: Brown-gray lines (#99998A, 3px width) between nodes.
Node states: Normal (opaque), Reachable (bright, hover scale 1.15x), Visited (dimmed 85%, green-tinted, scaled 0.9x), Locked (60% opacity, grayed out).

Interactivity: Click reachable nodes to navigate. Show tooltip on hover with node type name.
Atmosphere: Exploration, adventure, slight mystery.
```

#### 4. 卡牌设计:
```
Design a set of collectible cards for a roguelike deck-builder game.
Card size: 105x130 pixels (portrait orientation).
Base style: Dark fantasy, slightly worn/parchment texture feel.
Background: Very dark brown (#1A151F), subtle inner border glow.

Layout (from top to bottom):
1. Header row: Card name (white, 11px, centered) + Cost (top-right, circle badge, gold if cost>0 else green)
2. Icon area: Centered, 40px height, keep aspect ratio
3. Description text: Small (8px), gray, word-wrapped, 2-3 lines max
4. Type footer: Single line, centered, colored label ("Attack"/"Skill"/Power")

Type variations:
- ATTACK cards: Red border (#D94A4A), red type label "⚔️ 攻击", aggressive visual motif
- SKILL cards: Blue border (#5973FF), blue type label "🛡️ 技能", defensive/magic motif
- POWER cards: Golden border (#E6B233), gold type label "✨ 能力", mystical/glowing motif

Special states:
- Hover: Lift up 20px with smooth animation, slight drop shadow
-Playable: Full opacity, normal brightness
- Unplayable (too much cost): Dimmed to 60%, gray overlay
- Target selection mode: Pulsing golden glow border

Create 3 example cards showing different types with fantasy artwork style icons.
```

---

## 🔧 技术实现备注

### 代码生成优先级 (如果需要AI辅助编码):

1. **P0 - 必须有**: CombatHUD, CardUI, MapView, MapNodeUI (核心玩法)
2. **P1 - 重要**: CharacterSelect, RewardPanel, ShopPanel, RestSitePanel (主要流程)
3. **P2 - 完善**: EventPanel, SettingsPanel, AchievementPopup, TutorialOverlay (辅助功能)
4. **P3 - 增强**: VictoryScreen, GameOverScreen, PileViewPanel, TreasurePanel (收尾体验)

### 性能优化建议:

- ✅ 使用对象池 (ObjectPool) 管理FloatingText和CardUI实例
- ✅ 纹理图集 (TextureAtlas) 合并小图标以减少DrawCall
- ✅ 节点可见性管理 (Visible属性) 替代频繁Add/Remove
- ✅ 地图连线使用Line2D批量绘制而非单独Sprite
- ✅ 卡牌动画使用Tween复用池

### 可访问性 (Accessibility):

- ⚠️ 当前缺少: 高对比度模式、色盲友好配色、键盘完整导航、屏幕阅读器支持
- 建议: 为功能色添加图标辅助 (如 ⚔️🛡️✨ 已经部分实现)

---

## 📝 文档维护信息

- **生成日期**: 2026-04-13
- **游戏版本**: v0.1.0 Alpha
- **文档版本**: 1.0
- **适用工具**: 豆包 (Doubao)、MidJourney、Stable Diffusion、DALL-E、ChatGPT等AI设计/生成工具
- **更新频率**: 随UI代码变更同步更新

---

## ❓ 常见问题 FAQ

**Q: 这个文档可以直接给AI用来生成代码吗？**
A: 可以！第5节(CombatHUD)和第6节(Map)包含了足够详细的节点树和样式信息，配合Godot C#语法，AI可以生成90%+准确的UI代码。

**Q: 如何快速找到某个UI元素的实现？**
A: 使用文档内的搜索功能(Ctrl+F)搜索组件名称，然后参考对应的"节点树结构"和"脚本文件位置"。

**Q: 颜色值可以直接复制使用吗？**
A: 可以！所有颜色都提供了RGB十进制值(0-1范围，适用于Godot)和十六进制值(适用于CSS/Web)。

**Q: 图片资源的推荐规格是什么？**
A: 见每个资源分类下的"规格建议"。一般原则: 图标64x64px, 立绘200-300px, 背景1920x1080px, 透明PNG格式。

**Q: 如何扩展新的卡牌/敌人/遗物？**
A: 按照现有命名规范(`card_xx.png`, `relic_xx.png`)添加资源到对应文件夹，然后在JSON配置文件(cards.json, relics.json, enemies.json)中注册数据即可。

---

*文档结束 - 祝你创作出惊艳的UI设计！ 🎮✨*
