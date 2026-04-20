using BTTree = RoguelikeGame.Shared.BehaviorTree.BehaviorTree;
using RoguelikeGame.Shared.BehaviorTree;

namespace RoguelikeGame.Shared.Bots
{
    // 光影旅者 AI 数据模型
    public class PlatformData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Type { get; set; } = "normal"; // normal, light, shadow, shadow_wall
    }

    public class FragmentData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public bool Collected { get; set; }
    }

    public class CheckpointData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Id { get; set; } = "";
        public bool Activated { get; set; }
    }

    public class SwitchData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Id { get; set; } = "";
        public string TargetId { get; set; } = "";
        public bool Activated { get; set; }
    }

    public class GoalData
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class LightShadowEnemyData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Type { get; set; } = "light"; // light, shadow
        public bool IsHostile { get; set; }
    }

    public class LightZoneData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Radius { get; set; }
    }

    public class ShadowZoneData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Radius { get; set; }
    }

    public class LightShadowGameState
    {
        // 玩家状态
        public double PlayerX { get; set; }
        public double PlayerY { get; set; }
        public string PlayerForm { get; set; } = "light"; // light, shadow
        public int PlayerHealth { get; set; }
        public int PlayerMaxHealth { get; set; }
        public double PlayerEnergy { get; set; }
        public bool IsGrounded { get; set; }
        public bool FacingRight { get; set; }

        // 关卡数据
        public List<PlatformData> Platforms { get; set; } = new();
        public List<FragmentData> Fragments { get; set; } = new();
        public List<CheckpointData> Checkpoints { get; set; } = new();
        public List<SwitchData> Switches { get; set; } = new();
        public List<LightShadowEnemyData> Enemies { get; set; } = new();
        public List<LightZoneData> LightZones { get; set; } = new();
        public List<ShadowZoneData> ShadowZones { get; set; } = new();
        public GoalData? Goal { get; set; }

        // AI 内部状态
        public string CurrentTargetType { get; set; } = ""; // fragment, checkpoint, switch, goal, enemy
        public double TargetX { get; set; }
        public double TargetY { get; set; }
        public string? TargetId { get; set; }
    }

    // 黑板键定义
    public static class LightShadowBBKeys
    {
        public const string GameState = "game_state";
        public const string CurrentAction = "current_action";
        public const string MoveDirection = "move_direction"; // -1 left, 0 none, 1 right
        public const string ShouldJump = "should_jump";
        public const string ShouldSwitchForm = "should_switch_form";
        public const string ShouldDash = "should_dash";
        public const string TargetForm = "target_form";
        public const string LastActionTime = "last_action_time";
    }

    // AI 动作定义
    public static class LightShadowActions
    {
        public const string Move = "move";
        public const string Jump = "jump";
        public const string SwitchForm = "switch_form";
        public const string Dash = "dash";
        public const string Wait = "wait";
        public const string Idle = "idle";
    }

    // 光影旅者游戏模式
    public enum LightShadowPlayMode
    {
        Solo,       // 单人模式
        Race,       // 竞速模式 - 优先快速通关
        Coop        // 合作模式 - 配合玩家，收集碎片，激活开关
    }

    // 光影旅者 AI 主类
    public class LightShadowBotAI
    {
        private readonly BotDifficulty _difficulty;
        private readonly LightShadowPlayMode _playMode;
        private const double NearThreshold = 30.0;
        private const double JumpDistanceThreshold = 150.0;
        private const double PlatformReachHeight = 100.0;

        public LightShadowBotAI(BotDifficulty difficulty, LightShadowPlayMode playMode = LightShadowPlayMode.Solo)
        {
            _difficulty = difficulty;
            _playMode = playMode;
        }

        // 条件：是否有未收集的碎片
        public BTCondition HasUncollectedFragments() => new(context =>
        {
            var state = context.Blackboard.Get<LightShadowGameState>(LightShadowBBKeys.GameState);
            return state != null && state.Fragments.Any(f => !f.Collected);
        }, "HasUncollectedFragments");

        // 条件：是否需要切换形态
        public BTCondition NeedsFormSwitch() => new(context =>
        {
            var state = context.Blackboard.Get<LightShadowGameState>(LightShadowBBKeys.GameState);
            if (state == null) return false;

            // 检查附近是否有需要特定形态的平台
            var nearPlatform = FindNearbyPlatformRequiringForm(state);
            if (nearPlatform != null)
            {
                string neededForm = nearPlatform.Type == "light" ? "light" : "shadow";
                return state.PlayerForm != neededForm;
            }

            // 检查是否在危险区域
            if (IsInDangerousZone(state))
                return true;

            return false;
        }, "NeedsFormSwitch");

        // 条件：是否在地面
        public BTCondition IsGrounded() => new(context =>
        {
            var state = context.Blackboard.Get<LightShadowGameState>(LightShadowBBKeys.GameState);
            return state != null && state.IsGrounded;
        }, "IsGrounded");

        // 条件：是否有目标
        public BTCondition HasTarget() => new(context =>
        {
            var state = context.Blackboard.Get<LightShadowGameState>(LightShadowBBKeys.GameState);
            return state != null && !string.IsNullOrEmpty(state.CurrentTargetType);
        }, "HasTarget");

        // 条件：是否到达目标
        public BTCondition IsNearTarget() => new(context =>
        {
            var state = context.Blackboard.Get<LightShadowGameState>(LightShadowBBKeys.GameState);
            if (state == null) return false;

            double dx = state.TargetX - state.PlayerX;
            double dy = state.TargetY - state.PlayerY;
            return Math.Sqrt(dx * dx + dy * dy) < NearThreshold;
        }, "IsNearTarget");

        // 条件：目标在上方需要跳跃
        public BTCondition TargetRequiresJump() => new(context =>
        {
            var state = context.Blackboard.Get<LightShadowGameState>(LightShadowBBKeys.GameState);
            if (state == null) return false;

            return state.PlayerY - state.TargetY > PlatformReachHeight &&
                   Math.Abs(state.TargetX - state.PlayerX) < JumpDistanceThreshold;
        }, "TargetRequiresJump");

        // 条件：是否是竞速模式
        public BTCondition IsRaceMode() => new(context =>
        {
            return _playMode == LightShadowPlayMode.Race;
        }, "IsRaceMode");

        // 条件：是否是合作模式
        public BTCondition IsCoopMode() => new(context =>
        {
            return _playMode == LightShadowPlayMode.Coop;
        }, "IsCoopMode");

        // 条件：是否应该冲刺
        public BTCondition ShouldDash() => new(context =>
        {
            var state = context.Blackboard.Get<LightShadowGameState>(LightShadowBBKeys.GameState);
            if (state == null || state.PlayerForm != "light" || state.PlayerEnergy < 25)
                return false;

            // 简单模式不冲刺
            if (_difficulty == BotDifficulty.Easy)
                return false;

            // 竞速模式更频繁冲刺
            if (_playMode == LightShadowPlayMode.Race && state.PlayerEnergy >= 50)
            {
                double dist = Math.Abs(state.TargetX - state.PlayerX);
                return dist > 150;
            }

            // 检查前方是否有敌人需要躲避
            var enemy = FindNearbyEnemy(state, 200);
            if (enemy != null)
                return true;

            // 检查是否有很长的距离要走
            double distNormal = Math.Abs(state.TargetX - state.PlayerX);
            return distNormal > 300;
        }, "ShouldDash");

        // 动作：选择最近的碎片作为目标
        public BTAction SelectNearestFragment() => new(context =>
        {
            var state = context.Blackboard.Get<LightShadowGameState>(LightShadowBBKeys.GameState);
            if (state == null) return BTNodeStatus.Failure;

            var uncollected = state.Fragments.Where(f => !f.Collected).ToList();
            if (uncollected.Count == 0) return BTNodeStatus.Failure;

            var nearest = uncollected.OrderBy(f =>
            {
                double dx = f.X - state.PlayerX;
                double dy = f.Y - state.PlayerY;
                return Math.Sqrt(dx * dx + dy * dy);
            }).First();

            state.CurrentTargetType = "fragment";
            state.TargetX = nearest.X;
            state.TargetY = nearest.Y;

            context.Blackboard.Set(LightShadowBBKeys.GameState, state);
            return BTNodeStatus.Success;
        }, "SelectNearestFragment");

        // 动作：选择终点作为目标
        public BTAction SelectGoal() => new(context =>
        {
            var state = context.Blackboard.Get<LightShadowGameState>(LightShadowBBKeys.GameState);
            if (state == null || state.Goal == null) return BTNodeStatus.Failure;

            state.CurrentTargetType = "goal";
            state.TargetX = state.Goal.X;
            state.TargetY = state.Goal.Y;

            context.Blackboard.Set(LightShadowBBKeys.GameState, state);
            return BTNodeStatus.Success;
        }, "SelectGoal");

        // 动作：选择检查点
        public BTAction SelectCheckpoint() => new(context =>
        {
            var state = context.Blackboard.Get<LightShadowGameState>(LightShadowBBKeys.GameState);
            if (state == null) return BTNodeStatus.Failure;

            var inactive = state.Checkpoints.Where(c => !c.Activated).ToList();
            if (inactive.Count == 0) return BTNodeStatus.Failure;

            var nearest = inactive.OrderBy(c =>
            {
                double dx = c.X - state.PlayerX;
                double dy = c.Y - state.PlayerY;
                return Math.Sqrt(dx * dx + dy * dy);
            }).First();

            state.CurrentTargetType = "checkpoint";
            state.TargetX = nearest.X;
            state.TargetY = nearest.Y;
            state.TargetId = nearest.Id;

            context.Blackboard.Set(LightShadowBBKeys.GameState, state);
            return BTNodeStatus.Success;
        }, "SelectCheckpoint");

        // 动作：向目标移动
        public BTAction MoveToTarget() => new(context =>
        {
            var state = context.Blackboard.Get<LightShadowGameState>(LightShadowBBKeys.GameState);
            if (state == null) return BTNodeStatus.Failure;

            double dx = state.TargetX - state.PlayerX;

            // 确定移动方向
            int moveDir = dx > 5 ? 1 : dx < -5 ? -1 : 0;

            context.Blackboard.Set(LightShadowBBKeys.MoveDirection, moveDir);
            context.Blackboard.Set(LightShadowBBKeys.CurrentAction, LightShadowActions.Move);

            // 检查是否需要跳跃
            bool needsJump = TargetRequiresJumpHelper(state);
            context.Blackboard.Set(LightShadowBBKeys.ShouldJump, needsJump && state.IsGrounded);

            return BTNodeStatus.Success;
        }, "MoveToTarget");

        // 动作：切换形态
        public BTAction SwitchForm() => new(context =>
        {
            var state = context.Blackboard.Get<LightShadowGameState>(LightShadowBBKeys.GameState);
            if (state == null) return BTNodeStatus.Failure;

            var nearPlatform = FindNearbyPlatformRequiringForm(state);
            string targetForm = state.PlayerForm == "light" ? "shadow" : "light";

            if (nearPlatform != null)
            {
                targetForm = nearPlatform.Type == "light" ? "light" : "shadow";
            }
            else if (IsInDangerousZone(state))
            {
                // 从危险区域切换到安全形态
                targetForm = IsInLightZone(state) ? "light" : "shadow";
            }

            context.Blackboard.Set(LightShadowBBKeys.TargetForm, targetForm);
            context.Blackboard.Set(LightShadowBBKeys.ShouldSwitchForm, true);
            context.Blackboard.Set(LightShadowBBKeys.CurrentAction, LightShadowActions.SwitchForm);

            return BTNodeStatus.Success;
        }, "SwitchForm");

        // 动作：冲刺
        public BTAction Dash() => new(context =>
        {
            var state = context.Blackboard.Get<LightShadowGameState>(LightShadowBBKeys.GameState);
            if (state == null || state.PlayerForm != "light") return BTNodeStatus.Failure;

            context.Blackboard.Set(LightShadowBBKeys.ShouldDash, true);
            context.Blackboard.Set(LightShadowBBKeys.CurrentAction, LightShadowActions.Dash);

            return BTNodeStatus.Success;
        }, "Dash");

        // 动作：收集目标（标记为已收集）
        public BTAction CollectTarget() => new(context =>
        {
            var state = context.Blackboard.Get<LightShadowGameState>(LightShadowBBKeys.GameState);
            if (state == null) return BTNodeStatus.Failure;

            if (state.CurrentTargetType == "fragment")
            {
                var fragment = state.Fragments.FirstOrDefault(f =>
                    Math.Abs(f.X - state.TargetX) < 10 &&
                    Math.Abs(f.Y - state.TargetY) < 10);
                if (fragment != null)
                    fragment.Collected = true;
            }
            else if (state.CurrentTargetType == "checkpoint")
            {
                var checkpoint = state.Checkpoints.FirstOrDefault(c => c.Id == state.TargetId);
                if (checkpoint != null)
                    checkpoint.Activated = true;
            }

            state.CurrentTargetType = "";
            context.Blackboard.Set(LightShadowBBKeys.GameState, state);

            return BTNodeStatus.Success;
        }, "CollectTarget");

        // 动作：待机
        public BTAction Idle() => new(context =>
        {
            context.Blackboard.Set(LightShadowBBKeys.CurrentAction, LightShadowActions.Idle);
            context.Blackboard.Set(LightShadowBBKeys.MoveDirection, 0);
            context.Blackboard.Set(LightShadowBBKeys.ShouldJump, false);
            return BTNodeStatus.Success;
        }, "Idle");

        // 辅助方法
        private PlatformData? FindNearbyPlatformRequiringForm(LightShadowGameState state)
        {
            foreach (var platform in state.Platforms)
            {
                if (platform.Type != "light" && platform.Type != "shadow")
                    continue;

                double dx = platform.X + platform.Width / 2 - state.PlayerX;
                double dy = platform.Y - state.PlayerY;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                if (dist < 200 && dy < 0 && dy > -150)
                    return platform;
            }
            return null;
        }

        private bool IsInDangerousZone(LightShadowGameState state)
        {
            return IsInLightZone(state) && state.PlayerForm == "shadow" ||
                   IsInShadowZone(state) && state.PlayerForm == "light";
        }

        private bool IsInLightZone(LightShadowGameState state)
        {
            foreach (var zone in state.LightZones)
            {
                double dx = zone.X - state.PlayerX;
                double dy = zone.Y - state.PlayerY;
                if (Math.Sqrt(dx * dx + dy * dy) < zone.Radius)
                    return true;
            }
            return false;
        }

        private bool IsInShadowZone(LightShadowGameState state)
        {
            foreach (var zone in state.ShadowZones)
            {
                double dx = zone.X - state.PlayerX;
                double dy = zone.Y - state.PlayerY;
                if (Math.Sqrt(dx * dx + dy * dy) < zone.Radius)
                    return true;
            }
            return false;
        }

        private LightShadowEnemyData? FindNearbyEnemy(LightShadowGameState state, double maxDist)
        {
            foreach (var enemy in state.Enemies)
            {
                double dx = enemy.X - state.PlayerX;
                double dy = enemy.Y - state.PlayerY;
                if (Math.Sqrt(dx * dx + dy * dy) < maxDist)
                    return enemy;
            }
            return null;
        }

        private bool TargetRequiresJumpHelper(LightShadowGameState state)
        {
            return state.PlayerY - state.TargetY > PlatformReachHeight &&
                   Math.Abs(state.TargetX - state.PlayerX) < JumpDistanceThreshold;
        }
    }

    // 行为树工厂
    public static class LightShadowBotAIFactory
    {
        public static BTTree CreateBehaviorTree(BotDifficulty difficulty, LightShadowPlayMode playMode = LightShadowPlayMode.Solo)
        {
            var ai = new LightShadowBotAI(difficulty, playMode);
            var tree = new BTTree($"LightShadowBotAI_{difficulty}_{playMode}");

            if (playMode == LightShadowPlayMode.Race)
            {
                tree.Root = CreateRaceModeTree(ai);
            }
            else if (playMode == LightShadowPlayMode.Coop)
            {
                tree.Root = CreateCoopModeTree(ai);
            }
            else
            {
                tree.Root = CreateSoloModeTree(ai);
            }

            return tree;
        }

        // 单人模式：平衡收集碎片和通关
        private static BTNode CreateSoloModeTree(LightShadowBotAI ai)
        {
            // 应急处理：危险区域切换形态
            var emergencyFormSwitch = new BTSequence("EmergencyFormSwitch")
                .AddChild(ai.NeedsFormSwitch())
                .AddChild(ai.SwitchForm());

            // 收集碎片优先级
            var collectFragments = new BTSequence("CollectFragments")
                .AddChild(ai.HasUncollectedFragments())
                .AddChild(new BTSelector("FragmentSelector")
                    .AddChild(new BTSequence("CollectNearbyFragment")
                        .AddChild(ai.HasTarget())
                        .AddChild(ai.IsNearTarget())
                        .AddChild(ai.CollectTarget()))
                    .AddChild(new BTSequence("MoveToFragment")
                        .AddChild(ai.HasTarget())
                        .AddChild(ai.ShouldDash())
                        .AddChild(ai.Dash())
                        .AddChild(ai.MoveToTarget()))
                    .AddChild(new BTSequence("MoveToFragmentNoDash")
                        .AddChild(ai.HasTarget())
                        .AddChild(ai.MoveToTarget()))
                    .AddChild(ai.SelectNearestFragment()));

            // 激活检查点
            var activateCheckpoint = new BTSequence("ActivateCheckpoint")
                .AddChild(new BTSelector("CheckpointAction")
                    .AddChild(new BTSequence("CollectNearbyCheckpoint")
                        .AddChild(ai.HasTarget())
                        .AddChild(ai.IsNearTarget())
                        .AddChild(ai.CollectTarget()))
                    .AddChild(new BTSequence("MoveToCheckpoint")
                        .AddChild(ai.HasTarget())
                        .AddChild(ai.MoveToTarget()))
                    .AddChild(ai.SelectCheckpoint()));

            // 向终点移动
            var goToGoal = new BTSequence("GoToGoal")
                .AddChild(new BTSelector("GoalAction")
                    .AddChild(new BTSequence("MoveToGoal")
                        .AddChild(ai.HasTarget())
                        .AddChild(ai.ShouldDash())
                        .AddChild(ai.Dash())
                        .AddChild(ai.MoveToTarget()))
                    .AddChild(new BTSequence("MoveToGoalNoDash")
                        .AddChild(ai.HasTarget())
                        .AddChild(ai.MoveToTarget()))
                    .AddChild(ai.SelectGoal()));

            // 根选择器：按优先级执行
            return new BTSelector("Root_Solo")
                .AddChild(emergencyFormSwitch)
                .AddChild(collectFragments)
                .AddChild(activateCheckpoint)
                .AddChild(goToGoal)
                .AddChild(ai.Idle());
        }

        // 竞速模式：优先快速通关，忽略碎片
        private static BTNode CreateRaceModeTree(LightShadowBotAI ai)
        {
            // 应急处理：危险区域切换形态
            var emergencyFormSwitch = new BTSequence("EmergencyFormSwitch")
                .AddChild(ai.NeedsFormSwitch())
                .AddChild(ai.SwitchForm());

            // 快速向终点移动，尽可能冲刺
            var goToGoalFast = new BTSequence("GoToGoalFast")
                .AddChild(new BTSelector("GoalActionFast")
                    .AddChild(new BTSequence("MoveToGoalWithDash")
                        .AddChild(ai.HasTarget())
                        .AddChild(ai.ShouldDash())
                        .AddChild(ai.Dash())
                        .AddChild(ai.MoveToTarget()))
                    .AddChild(new BTSequence("MoveToGoalRace")
                        .AddChild(ai.HasTarget())
                        .AddChild(ai.MoveToTarget()))
                    .AddChild(ai.SelectGoal()));

            // 根选择器：竞速模式优先速度
            return new BTSelector("Root_Race")
                .AddChild(emergencyFormSwitch)
                .AddChild(goToGoalFast)
                .AddChild(ai.Idle());
        }

        // 合作模式：积极收集碎片，激活开关，配合玩家
        private static BTNode CreateCoopModeTree(LightShadowBotAI ai)
        {
            // 应急处理：危险区域切换形态
            var emergencyFormSwitch = new BTSequence("EmergencyFormSwitch")
                .AddChild(ai.NeedsFormSwitch())
                .AddChild(ai.SwitchForm());

            // 合作模式：优先收集所有碎片
            var collectFragmentsCoop = new BTSequence("CollectFragments_Coop")
                .AddChild(ai.HasUncollectedFragments())
                .AddChild(new BTSelector("FragmentSelector_Coop")
                    .AddChild(new BTSequence("CollectNearbyFragment_Coop")
                        .AddChild(ai.HasTarget())
                        .AddChild(ai.IsNearTarget())
                        .AddChild(ai.CollectTarget()))
                    .AddChild(new BTSequence("MoveToFragment_Coop")
                        .AddChild(ai.HasTarget())
                        .AddChild(ai.MoveToTarget()))
                    .AddChild(ai.SelectNearestFragment()));

            // 激活检查点
            var activateCheckpoint = new BTSequence("ActivateCheckpoint_Coop")
                .AddChild(new BTSelector("CheckpointAction_Coop")
                    .AddChild(new BTSequence("CollectNearbyCheckpoint_Coop")
                        .AddChild(ai.HasTarget())
                        .AddChild(ai.IsNearTarget())
                        .AddChild(ai.CollectTarget()))
                    .AddChild(new BTSequence("MoveToCheckpoint_Coop")
                        .AddChild(ai.HasTarget())
                        .AddChild(ai.MoveToTarget()))
                    .AddChild(ai.SelectCheckpoint()));

            // 向终点移动（合作模式较慢，确保不落下玩家）
            var goToGoalCoop = new BTSequence("GoToGoal_Coop")
                .AddChild(new BTSelector("GoalAction_Coop")
                    .AddChild(new BTSequence("MoveToGoalCoop")
                        .AddChild(ai.HasTarget())
                        .AddChild(ai.MoveToTarget()))
                    .AddChild(ai.SelectGoal()));

            // 根选择器：合作模式优先收集
            return new BTSelector("Root_Coop")
                .AddChild(emergencyFormSwitch)
                .AddChild(collectFragmentsCoop)
                .AddChild(activateCheckpoint)
                .AddChild(goToGoalCoop)
                .AddChild(ai.Idle());
        }
    }
}
