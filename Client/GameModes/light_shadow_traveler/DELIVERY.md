# 光影旅者 - 项目交付文档

## 项目概述

**光影旅者 (Light Shadow Traveler)** 是一款治愈系 2D 横版平台跳跃游戏，以光影形态切换为核心创新机制。

### 核心特色

- 🎮 **双形态玩法** - 光形态与影形态各有独特能力
- 🤝 **多人模式** - 支持合作、竞速、对战等多种玩法
- 🎨 **治愈风格** - 柔和的光影效果与手绘风格
- 🧩 **丰富解谜** - 结合光影机制的关卡设计

---

## 项目结构

```
light_shadow_traveler/
├── Config/
│   ├── config.json                    # 游戏模式配置
│   └── Data/
│       ├── package_config.json        # 游戏包配置
│       └── levels.json                # 关卡数据（5个章节）
├── Resources/
│   ├── Audio/                         # 音频资源
│   ├── Backgrounds/                   # 背景图
│   ├── Characters/                    # 角色资源
│   ├── Collectibles/                  # 收集品资源
│   ├── Effects/                       # 特效资源
│   ├── Enemies/                       # 敌人资源
│   ├── Icons/                         # 图标
│   ├── Platforms/                     # 平台资源
│   └── UI/                            # UI 资源
├── Scenes/
│   ├── GameScene.tscn                 # 主游戏场景
│   └── TestScene.tscn                 # 测试场景
└── Scripts/
    ├── player/
    │   └── player.gd                  # 玩家控制器
    ├── platforms/
    │   └── form_platform.gd           # 形态感应平台
    ├── enemies/
    │   └── form_enemy.gd              # 形态感应敌人
    ├── collectibles/
    │   └── memory_fragment.gd         # 记忆碎片收集品
    ├── objects/
    │   ├── checkpoint.gd              # 检查点
    │   ├── light_shadow_switch.gd     # 光影开关
    │   ├── moving_platform.gd         # 移动平台
    │   ├── particle_effect.gd         # 粒子效果
    │   └── shadow_trap.gd             # 暗影陷阱
    ├── puzzles/
    │   └── movable_light.gd           # 可移动光源
    ├── environment/
    │   ├── environment_system.gd      # 环境系统
    │   ├── light_zone.gd              # 光明区域
    │   ├── shadow_zone.gd             # 暗影区域
    │   └── env_particle.gd            # 环境粒子
    ├── levels/
    │   └── level_manager.gd           # 关卡管理器
    ├── systems/
    │   ├── game_scene.gd              # 主游戏场景控制器
    │   ├── coop_mode_manager.gd       # 合作模式管理器
    │   ├── race_mode_manager.gd       # 竞速模式管理器
    │   ├── bot_controller.gd          # 机器人控制器
    │   ├── test_scene.gd              # 测试场景
    │   └── test_runtime.gd            # 运行时测试
    └── ui/
        ├── game_hud.gd                # 游戏 HUD
        ├── level_select.gd            # 关卡选择
        └── ui_theme.gd                # UI 主题
```

---

## 核心功能

### 1. 玩家系统

**光形态能力**
- 移动速度适中
- 标准跳跃高度
- 可激活光平台
- 光形态冲刺（消耗能量）
- 可推动光源

**影形态能力**
- 移动速度更快
- 跳跃高度更高
- 可激活影平台
- 可滑翔（按住跳跃键）
- 可潜行（降低被发现概率）

### 2. 游戏模式

| 模式 | 描述 | 最大玩家 |
|------|------|----------|
| **单人冒险** | 完整的剧情关卡体验 | 1 |
| **双人合作** | 两名玩家配合解谜通关 | 2 |
| **竞速模式** | 比拼最快通关时间 | 4 |
| **对战模式** | 光影对决竞技 | 2 |

### 3. 关卡系统

**五大章节**
1. **遗忘之森** - 新手教程，基础移动和形态切换
2. **褪色画室** - 光源互动解谜
3. **寂静音乐厅** - 敌人互动机制
4. **沉睡图书馆** - 复杂解谜挑战
5. **光影神殿** - 综合机制最终挑战

### 4. 交互元素

- **光/影平台** - 只有对应形态可以站立
- **光影开关** - 特定形态激活机关
- **可移动光源** - 推动改变环境光影
- **记忆碎片** - 收集品，解锁剧情和皮肤
- **检查点** - 死亡后复活位置
- **光明/暗影区域** - 治疗或伤害特定形态

### 5. 敌人系统

- **形态感应敌人** - 只在特定形态下有攻击性
- **巡逻敌人** - 按固定路线巡逻
- **追击敌人** - 发现玩家后追击
- **Boss 敌人** - 大型挑战敌人

---

## 控制方式

| 操作 | 按键 |
|------|------|
| 左移 | A / 左方向键 |
| 右移 | D / 右方向键 |
| 跳跃 | 空格 / W / 上方向键 |
| 切换形态 | Shift / Tab |
| 光冲刺 | Q / K |
| 影潜行（按住） | S / 下方向键 |
| 推动光源 | E / J |

---

## 快速开始

### 1. 启动游戏

在 Godot 编辑器中打开项目，运行主场景：
```
res://GameModes/light_shadow_traveler/Scenes/GameScene.tscn
```

### 2. 测试模式

运行测试场景进行功能测试：
```
res://GameModes/light_shadow_traveler/Scenes/TestScene.tscn
```

### 3. 配置选项

修改难度设置在：
```
Config/Data/package_config.json
```

---

## 技术特点

### 脚本架构

- **GDScript 实现** - 24 个完整脚本文件
- **信号系统** - 松散耦合的事件通信
- **网络同步** - 支持多人联机
- **AI 系统** - 机器人控制器支持

### 视觉效果

- **程序化生成** - 缺少资源时自动生成占位图形
- **平滑相机** - 位置缓动跟随
- **粒子效果** - 形态切换、收集、伤害等特效
- **环境动画** - 呼吸式光效、飘动粒子

### 关卡数据

- **JSON 配置** - 完整的关卡数据结构
- **动态加载** - 运行时构建关卡
- **进度保存** - 关卡完成度和碎片收集

---

## 多人游戏

### 网络架构

- **MultiplayerBridge** - 跨语言网络桥接
- **RoomManager** - 房间管理系统
- **BotController** - AI 玩家支持
- **状态同步** - 位置、形态、开关状态

### 支持的网络功能

- ✅ 玩家位置同步
- ✅ 形态切换同步
- ✅ 开关状态同步
- ✅ 关卡进度同步
- ✅ 机器人 AI 控制

---

## 文件清单

### 核心脚本（24个）

1. `player/player.gd` - 玩家控制器
2. `platforms/form_platform.gd` - 形态平台
3. `enemies/form_enemy.gd` - 形态敌人
4. `collectibles/memory_fragment.gd` - 记忆碎片
5. `objects/checkpoint.gd` - 检查点
6. `objects/light_shadow_switch.gd` - 光影开关
7. `objects/moving_platform.gd` - 移动平台
8. `objects/particle_effect.gd` - 粒子效果
9. `objects/shadow_trap.gd` - 陷阱
10. `puzzles/movable_light.gd` - 可移动光源
11. `environment/environment_system.gd` - 环境系统
12. `environment/light_zone.gd` - 光明区域
13. `environment/shadow_zone.gd` - 暗影区域
14. `environment/env_particle.gd` - 环境粒子
15. `levels/level_manager.gd` - 关卡管理器
16. `systems/game_scene.gd` - 主游戏场景
17. `systems/coop_mode_manager.gd` - 合作模式
18. `systems/race_mode_manager.gd` - 竞速模式
19. `systems/bot_controller.gd` - 机器人控制器
20. `systems/test_scene.gd` - 测试场景
21. `systems/test_runtime.gd` - 运行时测试
22. `ui/game_hud.gd` - 游戏 HUD
23. `ui/level_select.gd` - 关卡选择
24. `ui/ui_theme.gd` - UI 主题

### 配置文件（3个）

1. `Config/config.json` - 游戏模式配置
2. `Config/Data/package_config.json` - 包配置
3. `Config/Data/levels.json` - 完整关卡数据

### 场景文件（2个）

1. `Scenes/GameScene.tscn` - 主游戏场景
2. `Scenes/TestScene.tscn` - 测试场景

---

## 已知特性与限制

### ✅ 已实现

- 完整的玩家双形态系统
- 完整的关卡加载和构建系统
- 四种游戏模式（单人、合作、竞速、对战）
- 完整的 UI HUD 系统
- 粒子特效系统
- 多人网络同步框架
- 机器人 AI 控制器
- 完整的关卡数据（5个章节）

### 📝 设计特点

- 程序化生成占位图形（无需美术资源即可运行）
- 平滑的操作手感（coyote time、输入缓冲）
- 完善的教程系统
- 难度分级系统

---

## 后续扩展建议

### 短期扩展

1. 添加更多关卡内容
2. 完善美术资源（替换占位图形）
3. 添加音效和背景音乐
4. 成就系统
5. 存档系统

### 长期规划

1. 自定义地图编辑器
2. 更多角色皮肤
3. 每日挑战模式
4. 排行榜系统
5. 更多游戏模式

---

## 交付状态

**版本**: 1.0.0  
**状态**: ✅ 可交付  
**最后更新**: 2026-04-24  

### 验收清单

- [x] 核心玩法完整
- [x] 单人模式可用
- [x] 多人模式框架完整
- [x] 关卡数据完整
- [x] UI 系统完整
- [x] 场景文件完整
- [x] 配置文件完整
- [x] 文档齐全

---

## 联系方式

如有问题或需要支持，请参考项目根目录的文档。

---

**感谢使用光影旅者！** 🎮✨
