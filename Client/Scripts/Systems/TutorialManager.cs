using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using RoguelikeGame.Core;

namespace RoguelikeGame.Systems
{
    public class TutorialStep
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string TargetNodePath { get; set; }
        public TutorialHighlightMode HighlightMode { get; set; } = TutorialHighlightMode.Rect;
        public Vector2 HighlightOffset { get; set; }
        public Vector2 HighlightSize { get; set; } = new(200, 100);
        public float DelayBeforeShow { get; set; } = 0.3f;
        public bool RequireInteraction { get; set; }
        public string InteractionSignal { get; set; }
        public TutorialPosition TooltipPosition { get; set; } = TutorialPosition.Auto;
        public List<string> PrerequisiteSteps { get; set; } = new();
        public string NextStepId { get; set; }
        public bool Skippable { get; set; } = true;
        public string IconHint { get; set; }
        public Action OnComplete { get; set; }
        public Action OnStart { get; set; }
        public object Phase { get; set; } = TutorialPhase.MainMenu;
    }

    public enum TutorialHighlightMode
    {
        None,
        Rect,
        Circle,
        Pulse,
        HandPointer
    }

    public enum TutorialPosition
    {
        Auto,
        Bottom,
        Top,
        Left,
        Right,
        Center
    }

    public enum TutorialPhase
    {
        MainMenu,
        CharacterSelect,
        MapNavigation,
        CombatBasics,
        CardPlaying,
        EnergyManagement,
        ShopAndRest,
        EliteAndBoss,
        RelicsAndPotions,
        AdvancedTips
    }

    public partial class TutorialManager : SingletonBase<TutorialManager>
    {
        private readonly Dictionary<string, TutorialStep> _allSteps = new();
        private readonly List<string> _completedSteps = new();
        private readonly HashSet<string> _unlockedPhases = new();

        private Panel _overlayPanel;
        private ColorRect _dimRect;
        private Control _highlightControl;
        private RichTextLabel _tooltipLabel;
        private Button _skipButton;
        private Button _nextButton;
        private AnimationPlayer _animPlayer;

        private TutorialStep _currentStep;
        private int _currentStepIndex = -1;
        private List<string> _activeSequence = new();
        private bool _isTutorialActive = false;
        private bool _isPaused = false;
        private Node _currentTargetNode;

        [Signal]
        public delegate void TutorialStartedEventHandler(string stepId);
        [Signal]
        public delegate void TutorialCompletedEventHandler(string stepId);
        [Signal]
        public delegate void TutorialSkippedEventHandler(string stepId);
        [Signal]
        public delegate void AllTutorialsCompletedEventHandler();
        [Signal]
        public delegate void PhaseUnlockedEventHandler(TutorialPhase phase);

        private const string SaveKey = "tutorial_progress";

        protected override void OnInitialize()
        {
            BuildTutorialSteps();
            LoadProgress();
            GD.Print("[TutorialManager] Initialized");
        }

        #region Tutorial Step Definitions

        private void BuildTutorialSteps()
        {
            AddStep(new TutorialStep
            {
                Id = "welcome",
                Title = "欢迎来到杀戮尖塔 2！",
                Content = "你将踏上一段充满挑战的冒险之旅。通过战斗、收集卡牌和遗物，攀登尖塔的每一层！\n\n点击「下一步」开始学习。",
                HighlightMode = TutorialHighlightMode.None,
                TooltipPosition = TutorialPosition.Center,
                Phase = TutorialPhase.MainMenu,
                Skippable = false
            });

            AddStep(new TutorialStep
            {
                Id = "mainmenu_start",
                Title = "开始冒险",
                Content = "点击「开始游戏」按钮来选择你的角色。每个角色都有独特的卡牌和玩法风格。",
                TargetNodePath = "MainMenuScene/VBoxContainer/StartButton",
                HighlightMode = TutorialHighlightMode.Rect,
                HighlightSize = new(220, 50),
                TooltipPosition = TutorialPosition.Bottom,
                Phase = TutorialPhase.MainMenu,
                RequireInteraction = true,
                NextStepId = "char_select_intro"
            });

            AddStep(new TutorialStep
            {
                Id = "char_select_intro",
                Title = "选择你的角色",
                Content = "每个角色拥有不同的初始生命值、起始卡牌和独特机制。\n\n• 铁甲战士 - 新手友好，攻击力强\n• 静默猎手 - 弃牌组合技专家\n• 故障机器人 - 充能球管理系统\n• 储君 - 星辰与锻造系统",
                HighlightMode = TutorialHighlightMode.None,
                TooltipPosition = TutorialPosition.Center,
                Phase = TutorialPhase.CharacterSelect,
                NextStepId = "char_select_confirm"
            });

            AddStep(new TutorialStep
            {
                Id = "char_select_confirm",
                Title = "确认选择",
                Content = "选中一个角色后，点击「确认」来开始你的冒险！\n\n不要担心选错，每次游戏都是新的体验。",
                TargetNodePath = "CharacterSelect/BottomBar/ConfirmButton",
                HighlightMode = TutorialHighlightMode.Rect,
                HighlightSize = new(160, 45),
                TooltipPosition = TutorialPosition.Top,
                Phase = TutorialPhase.CharacterSelect,
                RequireInteraction = true,
                NextStepId = "map_intro"
            });

            AddStep(new TutorialStep
            {
                Id = "map_intro",
                Title = "地图导航",
                Content = "这是你的冒险地图。从左到右，每列代表一层。\n\n• 红色节点 = 普通怪物\n• 橙色节点 = 精英敌人\n• 深红节点 = Boss战\n• 绿色节点 = 随机事件\n• 蓝色节点 = 商店\n• 浅绿节点 = 休息点",
                HighlightMode = TutorialHighlightMode.None,
                TooltipPosition = TutorialPosition.Center,
                Phase = TutorialPhase.MapNavigation,
                NextStepId = "map_movement"
            });

            AddStep(new TutorialStep
            {
                Id = "map_movement",
                Title = "移动方式",
                Content = "点击一个相邻的节点来移动。\n\n你可以看到所有可选路径，规划好路线是成功的关键！\n\n提示：在Boss前多积累一些力量。",
                TargetNodePath = "MapView/MapContainer",
                HighlightMode = TutorialHighlightMode.Rect,
                HighlightSize = new(900, 500),
                TooltipPosition = TutorialPosition.Bottom,
                Phase = TutorialPhase.MapNavigation,
                RequireInteraction = true,
                NextStepId = "combat_intro"
            });

            AddStep(new TutorialStep
            {
                Id = "combat_intro",
                Title = "进入战斗！",
                Content = "战斗开始了！让我们了解基本界面...",
                HighlightMode = TutorialHighlightMode.None,
                TooltipPosition = TutorialPosition.Center,
                Phase = TutorialPhase.CombatBasics,
                NextStepId = "combat_enemy"
            });

            AddStep(new TutorialStep
            {
                Id = "combat_enemy",
                Title = "敌人区域",
                Content = "这里显示当前敌人的信息：\n\n• 血量条（红色）\n• 意图指示器（显示敌人下回合行动）\n• Buff/Debuff 图标\n\n注意观察敌人的意图，提前做好准备！",
                TargetNodePath = "CombatHUD/EnemyArea",
                HighlightMode = TutorialHighlightMode.Rect,
                HighlightSize = new(1000, 180),
                TooltipPosition = TutorialPosition.Bottom,
                Phase = TutorialPhase.CombatBasics,
                NextStepId = "combat_player"
            });

            AddStep(new TutorialStep
            {
                Id = "combat_player",
                Title = "你的状态",
                Content = "左侧显示你的状态：\n\n• ❤️ 生命值（红色）\n🛡️ 格挡值（蓝色）\n• 手牌数量 / 抽牌堆 / 弃牌堆",
                TargetNodePath = "CombatHUD/PlayerArea",
                HighlightMode = TutorialHighlightMode.Rect,
                HighlightSize = new(250, 120),
                TooltipPosition = TutorialPosition.Right,
                Phase = TutorialPhase.CombatBasics,
                NextStepId = "combat_energy"
            });

            AddStep(new TutorialStep
            {
                Id = "combat_energy",
                Title = "能量系统",
                Content = "⚡ 能量球显示你当前可用的能量点数。\n\n每回合开始时获得 **3点能量**。\n每张卡牌消耗的能量显示在左上角的数字中。\n\n合理分配能量是获胜的关键！",
                TargetNodePath = "CombatHUD/EnergyBar",
                HighlightMode = TutorialHighlightMode.Rect,
                HighlightSize = new(150, 60),
                TooltipPosition = TutorialPosition.Top,
                Phase = TutorialPhase.EnergyManagement,
                NextStepId = "combat_hand"
            });

            AddStep(new TutorialStep
            {
                Id = "combat_hand",
                Title = "手牌区域",
                Content = "这里是你的手牌！\n\n• 🔴 红色边框 = 攻击牌\n• 🔵 蓝色边框 = 技能牌\n• 🟡 黄色边框 = 能力牌\n\n点击卡牌查看详情，再点击一次打出。",
                TargetNodePath = "CombatHUD/HandArea",
                HighlightMode = TutorialHighlightMode.Rect,
                HighlightSize = new(950, 180),
                TooltipPosition = TutorialPosition.Top,
                Phase = TutorialPhase.CardPlaying,
                NextStepId = "combat_play_card"
            });

            AddStep(new TutorialStep
            {
                Id = "combat_play_card",
                Title = "如何出牌",
                Content = "**出牌流程：**\n\n1. 点击一张卡牌选中它\n2. 如果需要，选择目标（敌人或自己）\n3. 卡牌会自动打出并消耗能量\n4. 打出的牌进入弃牌堆\n\n⚠️ 注意：某些牌有特殊关键词，如「消耗」「虚无」等",
                HighlightMode = TutorialHighlightMode.None,
                TooltipPosition = TutorialPosition.Center,
                Phase = TutorialPhase.CardPlaying,
                NextStepId = "combat_end_turn"
            });

            AddStep(new TutorialStep
            {
                Id = "combat_end_turn",
                Title = "结束回合",
                Content = "当你完成出牌后，点击「结束回合」按钮。\n\n• 未打出的牌会被弃置\n• 敌人开始行动\n• 下回合你会抽5张新牌\n\n💡 提示：不必每回合都打光所有牌！",
                TargetNodePath = "CombatHUD/EndTurnButton",
                HighlightMode = TutorialHighlightMode.Rect,
                HighlightSize = new(120, 45),
                TooltipPosition = TutorialPosition.Left,
                Phase = TutorialPhase.CombatBasics,
                RequireInteraction = true,
                NextStepId = "shop_intro"
            });

            AddStep(new TutorialStep
            {
                Id = "shop_intro",
                Title = "商店系统",
                Content = "在商店节点可以：\n\n• 🛒 购买新卡牌加入牌组\n• 💰 移除不想要的卡牌（精简牌组）\n• 🏺 购买药水用于紧急情况\n\n💡 提示：牌组越小越稳定，每张牌被抽到的概率更高！",
                HighlightMode = TutorialHighlightMode.None,
                TooltipPosition = TutorialPosition.Center,
                Phase = TutorialPhase.ShopAndRest,
                NextStepId = "rest_site"
            });

            AddStep(new TutorialStep
            {
                Id = "rest_site",
                Title = "休息点",
                Content = "在休息点你可以选择：\n\n❤️ **恢复生命值** - 回复30%最大生命\n🔀 **升级卡牌** - 选择一张卡牌进行强化\n\n根据当前血量和卡组情况做出明智的选择！",
                HighlightMode = TutorialHighlightMode.None,
                TooltipPosition = TutorialPosition.Center,
                Phase = TutorialPhase.ShopAndRest,
                NextStepId = "elite_boss"
            });

            AddStep(new TutorialStep
            {
                Id = "elite_boss",
                Title = "精英与Boss",
                Content = "⚠️ **精英敌人**（橙色）比普通敌人更强，但掉落更好的奖励。\n\n☠️ **Boss**（深红）是每层的最终挑战！击败Boss后才能进入下一层。\n\n面对强敌前确保你有足够的准备！",
                HighlightMode = TutorialHighlightMode.None,
                TooltipPosition = TutorialPosition.Center,
                Phase = TutorialPhase.EliteAndBoss,
                NextStepId = "relics_intro"
            });

            AddStep(new TutorialStep
            {
                Id = "relics_intro",
                Title = "遗物系统",
                Content = "🏺 **遗物** 是被动增益效果，永久改变你的游戏方式。\n\n获得来源：\n• 击败精英/Boss后的奖励\n• 宝箱节点\n• 事件选项\n• 商店购买\n\n有些遗物之间有强大的协同效应！",
                HighlightMode = TutorialHighlightMode.None,
                TooltipPosition = TutorialPosition.Center,
                Phase = TutorialPhase.RelicsAndPotions,
                NextStepId = "potion_intro"
            });

            AddStep(new TutorialStep
            {
                Id = "potion_intro",
                Title = "药水系统",
                Content = "🧪 **药水** 是一次性使用的强力道具。\n\n最多同时携带 3 瓶药水。\n常见类型：\n• ❤️ 生命药水 - 恢复生命\n• 🛡️ 格挡药水 - 获得格挡\n• ⚔️ 力量药水 - 临时增加力量\n\n合理使用药水可以在关键时刻扭转战局！",
                HighlightMode = TutorialHighlightMode.None,
                TooltipPosition = TutorialPosition.Center,
                Phase = TutorialPhase.RelicsAndPotions,
                NextStepId = "tips_final"
            });

            AddStep(new TutorialStep
            {
                Id = "tips_final",
                Title = "最后的小贴士",
                Content = "🌟 **祝你旅途愉快！**\n\n核心建议：\n1. 保持牌组精简高效\n2. 观察敌人意图再决定策略\n3. 不要贪心，活着才是最重要的\n4. 尝试不同的卡牌组合\n5. 失败也是学习的一部分！\n\n现在，去征服尖塔吧！⚔️",
                HighlightMode = TutorialHighlightMode.None,
                TooltipPosition = TutorialPosition.Center,
                Phase = TutorialPhase.AdvancedTips,
                Skippable = false
            });
        }

        private void AddStep(TutorialStep step)
        {
            _allSteps[step.Id] = step;
        }

        #endregion

        #region Public API

        public void StartTutorial(TutorialPhase phase)
        {
            if (_unlockedPhases.Contains(phase.ToString()))
                return;

            var sequence = GetPhaseSequence(phase);
            if (sequence.Count == 0) return;

            _activeSequence = sequence;
            _currentStepIndex = -1;
            _isTutorialActive = true;

            CreateOverlayIfNeeded();
            ShowNextStep();
            EmitSignal(SignalName.TutorialStarted, phase.ToString());
        }

        public void StartFullTutorial()
        {
            var fullSequence = new List<string>();
            foreach (var phase in Enum.GetValues(typeof(TutorialPhase)))
            {
                fullSequence.AddRange(GetPhaseSequence((TutorialPhase)phase));
            }
            _activeSequence = fullSequence;
            _currentStepIndex = -1;
            _isTutorialActive = true;
            CreateOverlayIfNeeded();
            ShowNextStep();
        }

        public void StartFromStep(string stepId)
        {
            if (!_allSteps.ContainsKey(stepId)) return;

            _activeSequence = new List<string> { stepId };
            var current = _allSteps[stepId];
            while (!string.IsNullOrEmpty(current.NextStepId) && _allSteps.ContainsKey(current.NextStepId))
            {
                current = _allSteps[current.NextStepId];
                _activeSequence.Add(current.Id);
            }

            _currentStepIndex = -1;
            _isTutorialActive = true;
            CreateOverlayIfNeeded();
            ShowNextStep();
        }

        public void SkipCurrentStep()
        {
            if (_currentStep == null || !_currentStep.Skippable) return;

            EmitSignal(SignalName.TutorialSkipped, _currentStep.Id);
            CompleteCurrentStep();
        }

        public void AdvanceToNext()
        {
            if (!_isTutorialActive || _currentStep == null) return;

            if (_currentStep.RequireInteraction)
                return;

            CompleteCurrentStep();
        }

        public void PauseTutorial()
        {
            _isPaused = true;
            if (_overlayPanel != null)
                _overlayPanel.Visible = false;
        }

        public void ResumeTutorial()
        {
            _isPaused = false;
            if (_overlayPanel != null && _isTutorialActive)
                _overlayPanel.Visible = true;
        }

        public void EndTutorial()
        {
            _isTutorialActive = false;
            _currentStep = null;
            _activeSequence.Clear();
            _currentStepIndex = -1;

            if (_overlayPanel != null)
            {
                _overlayPanel.QueueFree();
                _overlayPanel = null;
            }

            EmitSignal(SignalName.AllTutorialsCompleted);
        }

        public bool IsTutorialActive => _isTutorialActive && !_isPaused;
        public bool IsStepCompleted(string stepId) => _completedSteps.Contains(stepId);
        public bool IsPhaseUnlocked(TutorialPhase phase) => _unlockedPhases.Contains(phase.ToString());
        public int CompletedCount => _completedSteps.Count;
        public int TotalSteps => _allSteps.Count;

        public List<TutorialStep> GetPhaseSteps(TutorialPhase phase)
        {
            var result = new List<TutorialStep>();
            foreach (var step in _allSteps.Values)
            {
                if ((TutorialPhase)step.Phase == phase)
                    result.Add(step);
            }
            return result;
        }

        #endregion

        #region Internal Logic

        private List<string> GetPhaseSequence(TutorialPhase phase)
        {
            var result = new List<string>();
            TutorialStep first = null;

            foreach (var step in _allSteps.Values)
            {
                if ((TutorialPhase)step.Phase == phase)
                {
                    if (first == null) first = step;
                    if (!result.Contains(step.Id))
                        result.Add(step.Id);
                }
            }

            if (first != null && !string.IsNullOrEmpty(first.NextStepId))
            {
                var ordered = new List<string> { first.Id };
                var current = first;
                int safety = 0;
                while (!string.IsNullOrEmpty(current?.NextStepId) && safety < 50)
                {
                    if (_allSteps.TryGetValue(current.NextStepId, out var next))
                    {
                        if (!ordered.Contains(next.Id))
                            ordered.Add(next.Id);
                        current = next;
                    }
                    else break;
                    safety++;
                }
                return ordered;
            }

            return result;
        }

        private void CreateOverlayIfNeeded()
        {
            if (_overlayPanel != null && IsInstanceValid(_overlayPanel)) return;

            _overlayPanel = new Panel();
            _overlayPanel.Name = "TutorialOverlay";
            _overlayPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _overlayPanel.MouseFilter = Control.MouseFilterEnum.Stop;
            _overlayPanel.ZIndex = 200;

            _dimRect = new ColorRect();
            _dimRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _dimRect.Color = new Color(0, 0, 0, 0.65f);
            _overlayPanel.AddChild(_dimRect);

            _highlightControl = new Control();
            _highlightControl.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _highlightControl.MouseFilter = Control.MouseFilterEnum.Ignore;
            _overlayPanel.AddChild(_highlightControl);

            _tooltipLabel = new RichTextLabel();
            _tooltipLabel.SetAnchorsPreset(Control.LayoutPreset.CenterBottom);
            _tooltipLabel.CustomMinimumSize = new Vector2(380, 140);
            _tooltipLabel.Position = new Vector2(
                GetViewport().GetVisibleRect().Size.X / 2 - 200,
                GetViewport().GetVisibleRect().Size.Y - 220);
            _tooltipLabel.BbcodeEnabled = true;
            _tooltipLabel.ScrollActive = false;
            _tooltipLabel.FitContent = true;
            var styleBg = new StyleBoxFlat();
            styleBg.BgColor = new Color(0.12f, 0.10f, 0.18f, 0.97f);
            styleBg.BorderColor = new Color(0.6f, 0.5f, 0.3f, 0.8f);
            styleBg.SetBorderWidthAll(2);
            styleBg.SetCornerRadiusAll(10);
            styleBg.ContentMarginLeft = 15;
            styleBg.ContentMarginRight = 15;
            styleBg.ContentMarginTop = 12;
            styleBg.ContentMarginBottom = 12;
            _tooltipLabel.AddThemeStyleboxOverride("normal", styleBg);
            _overlayPanel.AddChild(_tooltipLabel);

            var buttonBar = new HBoxContainer();
            buttonBar.Alignment = BoxContainer.AlignmentMode.Center;
            buttonBar.Position = new Vector2(
                GetViewport().GetVisibleRect().Size.X / 2 - 120,
                GetViewport().GetVisibleRect().Size.Y - 55);

            _skipButton = new Button();
            _skipButton.Text = "跳过教程";
            _skipButton.Pressed += SkipCurrentStep;
            _skipButton.CustomMinimumSize = new Vector2(100, 36);
            buttonBar.AddChild(_skipButton);

            _nextButton = new Button();
            _nextButton.Text = "下一步 ▶";
            _nextButton.Pressed += AdvanceToNext;
            _nextButton.CustomMinimumSize = new Vector2(110, 36);
            var nextStyle = new StyleBoxFlat();
            nextStyle.BgColor = new Color(0.7f, 0.5f, 0.15f);
            nextStyle.SetCornerRadiusAll(6);
            _nextButton.AddThemeStyleboxOverride("normal", nextStyle);
            buttonBar.AddChild(_nextButton);

            _overlayPanel.AddChild(buttonBar);

            GetTree().Root.AddChild(_overlayPanel);
            _overlayPanel.Visible = false;
        }

        private void ShowNextStep()
        {
            _currentStepIndex++;
            if (_currentStepIndex >= _activeSequence.Count)
            {
                EndTutorial();
                return;
            }

            var stepId = _activeSequence[_currentStepIndex];
            if (_completedSteps.Contains(stepId))
            {
                ShowNextStep();
                return;
            }

            _currentStep = _allSteps[stepId];
            _overlayPanel.Visible = true;

            _currentStep.OnStart?.Invoke();

            var treeTimer = GetTree().CreateTimer(_currentStep.DelayBeforeShow);
            treeTimer.Timeout += () =>
            {
                if (_currentStep == null || !_isTutorialActive) return;
                DisplayStepContent(_currentStep);
                EmitSignal(SignalName.TutorialStarted, stepId);
            };

            SetupTargetHighlight(_currentStep);
        }

        private void DisplayStepContent(TutorialStep step)
        {
            string bbcode = $"[center][font_size=18][color=#FFD700]{step.Title}[/color][/font_size]\n\n{step.Content}[/center]";
            _tooltipLabel.Clear();
            _tooltipLabel.AppendText(bbcode);
            _tooltipLabel.Pop();

            _skipButton.Visible = step.Skippable;
            _nextButton.Visible = !step.RequireInteraction;

            AnimateTooltipIn();
        }

        private void SetupTargetHighlight(TutorialStep step)
        {
            ClearHighlight();

            if (string.IsNullOrEmpty(step.TargetNodePath) ||
                step.HighlightMode == TutorialHighlightMode.None)
                return;

            var target = GetNodeOrNull<Node>(step.TargetNodePath);
            if (target == null)
            {
                var viewport = GetViewport();
                if (viewport != null)
                    target = viewport.GetNodeOrNull<Node>(step.TargetNodePath);
            }

            if (target is Control targetCtrl)
            {
                _currentTargetNode = target;
                DrawHighlight(targetCtrl, step);
            }
        }

        private void DrawHighlight(Control target, TutorialStep step)
        {
            var screenPos = target.GetGlobalPosition();
            var size = target.Size;

            if (step.HighlightSize.X > 0 && step.HighlightSize.Y > 0)
                size = step.HighlightSize;

            screenPos += step.HighlightOffset;

            switch (step.HighlightMode)
            {
                case TutorialHighlightMode.Rect:
                    DrawRectHighlight(screenPos, size);
                    break;
                case TutorialHighlightMode.Circle:
                    DrawCircleHighlight(screenPos + size / 2, Mathf.Min(size.X, size.Y) / 2);
                    break;
                case TutorialHighlightMode.Pulse:
                    DrawPulseHighlight(screenPos, size);
                    break;
                case TutorialHighlightMode.HandPointer:
                    DrawHandPointer(screenPos + size / 2);
                    break;
            }

            CutoutDimRect(screenPos, size);
        }

        private void DrawRectHighlight(Vector2 pos, Vector2 size)
        {
            var line = new Line2D();
            line.Width = 3;
            line.DefaultColor = new Color(1f, 0.85f, 0.2f, 0.95f);
            line.AddPoint(pos);
            line.AddPoint(pos + new Vector2(size.X, 0));
            line.AddPoint(pos + size);
            line.AddPoint(pos + new Vector2(0, size.Y));
            line.AddPoint(pos);
            _highlightControl.AddChild(line);
        }

        private void DrawCircleHighlight(Vector2 center, float radius)
        {
            var line = new Line2D();
            line.Width = 3;
            line.DefaultColor = new Color(1f, 0.85f, 0.2f, 0.95f);
            const int segments = 48;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.Tau;
                line.AddPoint(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
            _highlightControl.AddChild(line);
        }

        private void DrawPulseHighlight(Vector2 pos, Vector2 size)
        {
            var tween = CreateTween();
            var line = new Line2D();
            line.Width = 3;
            line.DefaultColor = new Color(1f, 0.85f, 0.2f, 0.95f);
            line.AddPoint(pos);
            line.AddPoint(pos + new Vector2(size.X, 0));
            line.AddPoint(pos + size);
            line.AddPoint(pos + new Vector2(0, size.Y));
            line.AddPoint(pos);
            _highlightControl.AddChild(line);

            tween.TweenProperty(line, "modulate:a", 0.3f, 0.8f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
            tween.TweenProperty(line, "modulate:a", 1.0f, 0.8f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
            tween.SetLoops();
        }

        private void DrawHandPointer(Vector2 targetPos)
        {
            var sprite = new Sprite2D();
            var img = Image.Create(32, 32, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));

            int cx = 16, cy = 20;
            DrawTriangleFilled(img, cx, cy - 14, cx - 8, cy + 6, cx + 8, cy + 6, new Color(1f, 0.9f, 0.4f));
            SetPixelSafe(img, cx, cy + 6, new Color(1f, 0.9f, 0.4f));
            SetPixelSafe(img, cx - 1, cy + 7, new Color(1f, 0.9f, 0.4f));
            SetPixelSafe(img, cx + 1, cy + 7, new Color(1f, 0.9f, 0.4f));
            SetPixelSafe(img, cx, cy + 8, new Color(1f, 0.9f, 0.4f));

            sprite.Texture = ImageTexture.CreateFromImage(img);
            sprite.Position = targetPos + new Vector2(-16, -24);
            _highlightControl.AddChild(sprite);

            var tween = CreateTween();
            tween.TweenProperty(sprite, "position:y", targetPos.Y - 34, 0.6f)
                .SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(sprite, "position:y", targetPos.Y - 24, 0.6f)
                .SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.In);
            tween.SetLoops();
        }

        private void CutoutDimRect(Vector2 pos, Vector2 size)
        {
            var cutout = new ColorRect();
            cutout.Color = Colors.Transparent;
            cutout.Position = pos - new Vector2(2, 2);
            cutout.Size = size + new Vector2(4, 4);
            cutout.MouseFilter = Control.MouseFilterEnum.Pass;
            _overlayPanel.MoveChild(cutout, 1);
        }

        private void ClearHighlight()
        {
            if (_highlightControl != null)
            {
                foreach (var child in _highlightControl.GetChildren())
                    child.QueueFree();
            }
            _currentTargetNode = null;

            if (_dimRect != null)
                _dimRect.Color = new Color(0, 0, 0, 0.65f);
        }

        private void CompleteCurrentStep()
        {
            if (_currentStep == null) return;

            string completedId = _currentStep.Id;
            _currentStep.OnComplete?.Invoke();

            if (!_completedSteps.Contains(completedId))
                _completedSteps.Add(completedId);

            EmitSignal(SignalName.TutorialCompleted, completedId);

            CheckPhaseUnlock(completedId);
            SaveProgress();

            ClearHighlight();

            if (!string.IsNullOrEmpty(_currentStep.NextStepId))
            {
                ShowNextStep();
            }
            else
            {
                _currentStepIndex++;
                if (_currentStepIndex < _activeSequence.Count)
                    ShowNextStep();
                else
                    EndTutorial();
            }
        }

        private void CheckPhaseUnlock(string stepId)
        {
            if (_allSteps.TryGetValue(stepId, out var step))
            {
                var phase = (TutorialPhase)step.Phase;
                string phaseStr = phase.ToString();
                if (!_unlockedPhases.Contains(phaseStr))
                {
                    var phaseSteps = GetPhaseSteps(phase);
                    bool allDone = true;
                    foreach (var ps in phaseSteps)
                    {
                        if (!_completedSteps.Contains(ps.Id))
                        {
                            allDone = false;
                            break;
                        }
                    }
                    if (allDone)
                    {
                        _unlockedPhases.Add(phaseStr);
                        EmitSignal(SignalName.PhaseUnlocked, (int)phase);
                    }
                }
            }
        }

        private void AnimateTooltipIn()
        {
            if (_tooltipLabel == null) return;

            _tooltipLabel.Scale = new Vector2(0.92f, 0.92f);

            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(_tooltipLabel, "modulate:a", 1f, 0.35f)
                .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(_tooltipLabel, "scale", Vector2.One, 0.4f)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        }

        #endregion

        #region Persistence

        private void SaveProgress()
        {
            var config = new ConfigFile();
            config.SetValue(SaveKey, "completed_steps", _completedSteps.ToArray());
            config.SetValue(SaveKey, "unlocked_phases", _unlockedPhases.ToArray());
            config.SetValue(SaveKey, "version", 1);
            config.Save("user://tutorial.cfg");
        }

        private void LoadProgress()
        {
            var config = new ConfigFile();
            var err = config.Load("user://tutorial.cfg");
            if (err != Error.Ok) return;

            var completed = config.GetValue(SaveKey, "completed_steps", new string[0]).AsGodotArray<string>();
            foreach (var s in completed)
                _completedSteps.Add(s);

            var unlocked = config.GetValue(SaveKey, "unlocked_phases", new Godot.Collections.Array<string>()).AsGodotArray<string>();
            foreach (var p in unlocked)
                _unlockedPhases.Add(p);

            GD.Print($"[TutorialManager] Loaded {_completedSteps.Count} completed steps, {_unlockedPhases.Count} unlocked phases");
        }

        public void ResetProgress()
        {
            _completedSteps.Clear();
            _unlockedPhases.Clear();
            SaveProgress();
            GD.Print("[TutorialManager] Progress reset");
        }

        #endregion

        #region Helper Methods (shared with ArtGenerator pattern)

        private static void DrawTriangleFilled(Image img, int x0, int y0, int x1, int y1, int x2, int y2, Color color)
        {
            int minX = Mathf.Min(x0, Mathf.Min(x1, x2));
            int maxX = Mathf.Max(x0, Mathf.Max(x1, x2));
            int minY = Mathf.Min(y0, Mathf.Min(y1, y2));
            int maxY = Mathf.Max(y0, Mathf.Max(y1, y2));
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float d1 = (x1 - x0) * (y - y0) - (x - x0) * (y1 - y0);
                    float d2 = (x2 - x1) * (y - y1) - (x - x1) * (y2 - y1);
                    float d3 = (x0 - x2) * (y - y2) - (x - x2) * (y0 - y2);
                    bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
                    bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
                    if (!(hasNeg && hasPos))
                        SetPixelSafe(img, x, y, color);
                }
            }
        }

        private static void SetPixelSafe(Image img, int x, int y, Color color)
        {
            if (x >= 0 && x < img.GetWidth() && y >= 0 && y < img.GetHeight())
                img.SetPixel(x, y, color);
        }

        #endregion
    }
}
