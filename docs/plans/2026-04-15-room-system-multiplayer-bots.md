# 房间系统 + 多人玩法 + 机器人系统 实施计划

## 概述

实现完整的房间系统（创建/加入/房间内视图/自动销毁）、多人玩法模式（协作/竞速/对抗）、行为树机器人框架。

## 优先级

P0 = 紧急Bug修复 → P1 = 房间系统核心 → P2 = 机器人框架 → P3 = 多人玩法扩展

---

## Task 1: [P0] 修复game_lobby返回菜单无反应

**Files:**
- Modify: `Client/Scripts/UI/Flow/game_lobby.gd`
- Modify: `Client/Scripts/Main.cs`

**Step 1:** 检查game_lobby.gd的quit_game信号连接
**Step 2:** 在Main.cs的GoToLobby中确保quit_game信号连接到QuitGame方法
**Step 3:** 验证编译通过

---

## Task 2: [P0] 修复LobbyPanel加入房间逻辑

**Files:**
- Modify: `Client/Scripts/UI/Panels/LobbyPanel.cs`

**Step 1:** 修复OnRoomItemSelected，传递选中的roomId而非null
**Step 2:** 添加JoinRoomAsync调用，使用选中的roomId
**Step 3:** 验证编译通过

---

## Task 3: [P1] 创建RoomPanel全屏房间视图

**Files:**
- Create: `Client/Scripts/UI/Panels/RoomPanel.cs`

**Step 1:** 创建RoomPanel类，继承PanelContainer
**Step 2:** 实现UI布局：顶部房间信息、左侧玩家列表、右侧聊天区、底部操作按钮
**Step 3:** 实现准备/取消准备按钮逻辑
**Step 4:** 实现退出房间按钮逻辑
**Step 5:** 实现房主专属：开始游戏按钮、添加机器人按钮
**Step 6:** 实现房间聊天（通过SignalR GameHub）
**Step 7:** 验证编译通过

---

## Task 4: [P1] 修改Main.cs房间流程 - 创建/加入后进入RoomPanel

**Files:**
- Modify: `Client/Scripts/Main.cs`

**Step 1:** OnCreateRoomRequested改为：创建房间后进入RoomPanel
**Step 2:** OnJoinRoomRequested改为：加入房间后进入RoomPanel
**Step 3:** RoomPanel退出时返回game_lobby
**Step 4:** 验证编译通过

---

## Task 5: [P1] 服务端房间自动销毁 - RoomCleanupService

**Files:**
- Create: `Server/RoguelikeGame.Server/Services/RoomCleanupService.cs`
- Modify: `Server/RoguelikeGame.Server/Program.cs`

**Step 1:** 创建RoomCleanupService (BackgroundService)
**Step 2:** 每30秒扫描：空房间立即销毁、等待超30分钟销毁、游戏中超2小时强制结束
**Step 3:** 房主离线超5分钟自动转让
**Step 4:** 在Program.cs注册服务
**Step 5:** 验证编译通过

---

## Task 6: [P1] 增强GameHub - 房间实时通信

**Files:**
- Modify: `Server/RoguelikeGame.Server/Hubs/GameHub.cs`

**Step 1:** 添加RoomChat方法（房间频道聊天）
**Step 2:** 添加PlayerReadyChanged方法（准备状态变更通知）
**Step 3:** 添加GameStarting方法（游戏开始通知，广播给房间所有人）
**Step 4:** 添加PlayerJoinedRoom/PlayerLeftRoom（带用户名信息的加入/离开通知）
**Step 5:** 添加BotAdded/BotRemoved通知
**Step 6:** 验证编译通过

---

## Task 7: [P2] 行为树框架 - 共享代码层

**Files:**
- Create: `Shared/RoguelikeGame.BehaviorTree/BehaviorTree.cs`
- Create: `Shared/RoguelikeGame.BehaviorTree/Nodes/BTNode.cs`
- Create: `Shared/RoguelikeGame.BehaviorTree/Nodes/BTSelector.cs`
- Create: `Shared/RoguelikeGame.BehaviorTree/Nodes/BTSequence.cs`
- Create: `Shared/RoguelikeGame.BehaviorTree/Nodes/BTCondition.cs`
- Create: `Shared/RoguelikeGame.BehaviorTree/Nodes/BTAction.cs`
- Create: `Shared/RoguelikeGame.BehaviorTree/Nodes/BTDecorator.cs`
- Create: `Shared/RoguelikeGame.BehaviorTree/Blackboard.cs`

**Step 1:** 创建共享项目RoguelikeGame.BehaviorTree (.NET Standard 2.1)
**Step 2:** 实现BTNode基类（Execute/Reset/Abort）
**Step 3:** 实现BTSelector（优先级选择器）
**Step 4:** 实现BTSequence（顺序执行器）
**Step 5:** 实现BTCondition（条件节点）
**Step 6:** 实现BTAction（动作节点，支持async）
**Step 7:** 实现BTDecorator（装饰器：反转、重复、延时）
**Step 8:** 实现Blackboard（共享数据黑板）
**Step 9:** 实现BehaviorTree（根节点+黑板+执行上下文）
**Step 10:** 验证编译通过

---

## Task 8: [P2] 机器人管理器 - 客户端/服务端共享

**Files:**
- Create: `Shared/RoguelikeGame.BehaviorTree/BotManager.cs`
- Create: `Shared/RoguelikeGame.BehaviorTree/BotProfile.cs`
- Modify: `Server/RoguelikeGame.Server/Controllers/RoomController.cs`
- Modify: `Server/RoguelikeGame.Server/Models/Room.cs`

**Step 1:** 添加BotProfile模型（名称、难度、行为树配置）
**Step 2:** RoomPlayer添加IsBot标识
**Step 3:** RoomController添加AddBot/RemoveBot端点
**Step 4:** BotManager实现：创建/销毁机器人、tick驱动行为树
**Step 5:** 客户端BotManager：可选本地运行机器人
**Step 6:** 验证编译通过

---

## Task 9: [P2] 包1机器人 - 卡牌战斗AI

**Files:**
- Create: `Client/GameModes/base_game/Code/AI/CardBotAI.cs`

**Step 1:** 实现卡牌评估条件节点（手牌价值、敌人意图、血量状况）
**Step 2:** 实现出牌动作节点（选择最优牌、选择目标）
**Step 3:** 实现防御策略（低血量时优先防御）
**Step 4:** 实现药水使用策略
**Step 5:** 验证编译通过

---

## Task 10: [P3] 包1多人模式 - Coop组队协作

**Files:**
- Modify: `Client/GameModes/base_game/Scripts/combat/sts_combat_system.gd`
- Modify: `Server/RoguelikeGame.Server/Hubs/GameHub.cs`

**Step 1:** Coop模式：共享Boss生命值，独立手牌和能量
**Step 2:** 轮流出牌机制（按座位顺序）
**Step 3:** 遗物效果对全队生效
**Step 4:** 网络同步：出牌操作通过SignalR广播
**Step 5:** 验证编译通过

---

## Task 11: [P3] 包2多人模式 - Race竞速跑图

**Files:**
- Modify: `Client/GameModes/light_shadow_traveler/Scripts/player/player.gd`
- Modify: `Server/RoguelikeGame.Server/Hubs/GameHub.cs`

**Step 1:** Race模式：每人独立关卡实例
**Step 2:** 实时同步其他玩家位置（低频2-5Hz）
**Step 3:** 显示其他玩家半透明幽灵
**Step 4:** 先到检查点者获得加速奖励
**Step 5:** 验证编译通过

---

## Task 12: [P3] 包2多人模式 - Coop光影协作

**Files:**
- Modify: `Client/GameModes/light_shadow_traveler/Scripts/level/level_manager.gd`

**Step 1:** Coop模式：一人光形态一人影形态
**Step 2:** 光/影平台限制（光玩家只能站光平台）
**Step 3:** 双人开关谜题
**Step 4:** 网络同步：位置+形态+开关状态
**Step 5:** 验证编译通过

---

## Task 13: 编译验证 + Git提交推送

**Step 1:** 客户端 dotnet build
**Step 2:** 服务端 dotnet build
**Step 3:** git add -A && git commit
**Step 4:** git push
