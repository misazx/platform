using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;

namespace RoguelikeGame.Systems
{
    public class AchievementDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconId { get; set; }
        public AchievementCategory Category { get; set; } = AchievementCategory.General;
        public int Points { get; set; } = 10;
        public bool Hidden { get; set; }
        public string HiddenDescription { get; set; } = "???";
        public List<string> PrerequisiteIds { get; set; } = new();
        public int MaxProgress { get; set; } = 1;
        public int RewardGold { get; set; }
        public string RewardUnlock { get; set; }
        public bool IsSecret { get; set; }

        private int _currentProgress;
        public int CurrentProgress
        {
            get => _currentProgress;
            set => _currentProgress = Mathf.Clamp(value, 0, MaxProgress);
        }

        public bool IsUnlocked => CurrentProgress >= MaxProgress;
        public float ProgressPercent => MaxProgress > 0 ? (float)CurrentProgress / MaxProgress : 0f;
    }

    public enum AchievementCategory
    {
        Combat,
        Exploration,
        Collection,
        Challenge,
        Story,
        General,
        Secret
    }

    public enum AchievementTier
    {
        Bronze,
        Silver,
        Gold,
        Diamond,
        Legendary
    }

    public class AchievementData
    {
        public string Id { get; set; }
        public bool Unlocked { get; set; }
        public int Progress { get; set; }
        public DateTime UnlockTime { get; set; }
        public int RunCountWhenUnlocked { get; set; }
    }

    public partial class AchievementManager : SingletonBase<AchievementManager>
    {
        private readonly Dictionary<string, AchievementDefinition> _definitions = new();
        private readonly Dictionary<string, AchievementData> _progress = new();
        private readonly List<string> _unlockQueue = new();
        private readonly HashSet<string> _secretRevealed = new();

        private Panel _popupPanel;
        private TextureRect _iconDisplay;
        private Label _nameLabel;
        private RichTextLabel _descLabel;
        private Label _pointsLabel;
        private Godot.Timer _displayTimer;
        private AnimationPlayer _popupAnim;
        private int _queueIndex = 0;
        private bool _isShowingPopup = false;

        [Signal]
        public delegate void AchievementUnlockedEventHandler(string achievementId);
        [Signal]
        public delegate void AchievementProgressUpdatedEventHandler(string achievementId, int current, int max);
        [Signal]
        public delegate void AllAchievementsUnlockedEventHandler();
        [Signal]
        public delegate void SecretAchievementRevealedEventHandler(string achievementId);

        protected override void OnInitialize()
        {
            BuildAchievementDefinitions();
            LoadProgress();
            GD.Print($"[AchievementManager] Initialized with {_definitions.Count} achievements");
        }

        #region Achievement Definitions

        private void BuildAchievementDefinitions()
        {
            AddAchievement(new AchievementDefinition
            {
                Id = "first_win",
                Name = "初次胜利",
                Description = "赢得第一场战斗",
                IconId = "trophy_bronze",
                Category = AchievementCategory.Combat,
                Points = 10
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "kill_10_enemies",
                Name = "小试牛刀",
                Description = "累计击败 10 个敌人",
                IconId = "sword",
                Category = AchievementCategory.Combat,
                Points = 15,
                MaxProgress = 10
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "kill_100_enemies",
                Name = "杀戮机器",
                Description = "累计击败 100 个敌人",
                IconId = "skull",
                Category = AchievementCategory.Combat,
                Points = 50,
                MaxProgress = 100,
                PrerequisiteIds = new List<string> { "kill_10_enemies" }
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "kill_500_enemies",
                Name = "尖塔屠夫",
                Description = "累计击败 500 个敌人",
                IconId = "blood",
                Category = AchievementCategory.Challenge,
                Points = 150,
                MaxProgress = 500,
                PrerequisiteIds = new List<string> { "kill_100_enemies" }
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "no_hit_victory",
                Name = "完美无瑕",
                Description = "在不受到任何伤害的情况下赢得一场战斗",
                IconId = "shield_star",
                Category = AchievementCategory.Challenge,
                Points = 40,
                Hidden = true
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "one_hp_win",
                Name = "绝地反击",
                Description = "以 1 点生命值赢得战斗",
                IconId = "heart_broken",
                Category = AchievementCategory.Challenge,
                Points = 35,
                Hidden = true
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "max_damage_single",
                Name = "一击必杀",
                Description = "单次攻击造成 99+ 点伤害",
                IconId = "explosion",
                Category = AchievementCategory.Combat,
                Points = 30
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "block_100",
                Name = "铜墙铁壁",
                Description = "单回合获得 100+ 点格挡",
                IconId = "wall",
                Category = AchievementCategory.Combat,
                Points = 25
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "reach_floor_10",
                Name = "攀登者",
                Description = "到达第 10 层",
                IconId = "stairs",
                Category = AchievementCategory.Exploration,
                Points = 20,
                MaxProgress = 10
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "reach_floor_30",
                Name = "尖塔中段",
                Description = "到达第 30 层",
                IconId = "tower_mid",
                Category = AchievementCategory.Exploration,
                Points = 50,
                MaxProgress = 30,
                PrerequisiteIds = new List<string> { "reach_floor_10" }
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "reach_floor_50",
                Name = "尖塔之巅",
                Description = "到达第 50 层（通关游戏）",
                IconId = "crown",
                Category = AchievementCategory.Story,
                Points = 200,
                MaxProgress = 50,
                PrerequisiteIds = new List<string> { "reach_floor_30" },
                Hidden = true
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "visit_all_events",
                Name = "好奇宝宝",
                Description = "触发所有类型的事件至少一次",
                IconId = "question_mark",
                Category = AchievementCategory.Exploration,
                Points = 30
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "clear_shop",
                Name = "购物狂",
                Description = "在一次商店访问中购买所有商品",
                IconId = "cart_full",
                Category = AchievementCategory.Exploration,
                Points = 20
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "collect_20_relics",
                Name = "收藏家",
                Description = "累计收集 20 个遗物",
                IconId = "relic_pile",
                Category = AchievementCategory.Collection,
                Points = 30,
                MaxProgress = 20
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "collect_50_relics",
                Name = "遗物大师",
                Description = "累计收集 50 个不同的遗物",
                IconId = "museum",
                Category = AchievementCategory.Collection,
                Points = 80,
                MaxProgress = 50,
                PrerequisiteIds = new List<string> { "collect_20_relics" }
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "collect_all_cards_one_char",
                Name = "全卡收集者",
                Description = "用同一个角色收集该角色所有卡牌",
                IconId = "card_collection",
                Category = AchievementCategory.Collection,
                Points = 60,
                Hidden = true
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "play_1000_cards",
                Name = "卡牌大师",
                Description = "累计打出 1000 张卡牌",
                IconId = "cards_spread",
                Category = AchievementCategory.Collection,
                Points = 40,
                MaxProgress = 1000
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "win_no_rare_cards",
                Name = "平凡之路",
                Description = "在不拥有任何稀有卡牌的情况下通关一层",
                IconId = "common_only",
                Category = AchievementCategory.Challenge,
                Points = 45,
                Hidden = true
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "beat_elite_no_damage",
                Name = "精英猎手",
                Description = "无伤击败一个精英敌人",
                IconId = "elite_skull",
                Category = AchievementCategory.Challenge,
                Points = 35
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "defeat_boss_under_1min",
                Name = "速通专家",
                Description = "在1分钟内击败Boss",
                IconId = "timer_fast",
                Category = AchievementCategory.Challenge,
                Points = 40
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "all_characters_played",
                Name = "全能冒险家",
                Description = "使用所有角色各完成一次游戏",
                IconId = "all_chars",
                Category = AchievementCategory.Collection,
                Points = 70,
                MaxProgress = 5
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "complete_tutorial",
                Name = "好学生",
                Description = "完成所有教程内容",
                IconId = "graduation_cap",
                Category = AchievementCategory.General,
                Points = 15
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "first_relic",
                Name = "初获神器",
                Description = "获得第一个遗物",
                IconId = "first_relic_icon",
                Category = AchievementCategory.General,
                Points = 5
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "gold_1000",
                Name = "小有积蓄",
                Description = "单次冒险累计获得 1000 金币",
                IconId = "gold_bag",
                Category = AchievementCategory.Collection,
                Points = 15,
                MaxProgress = 1000
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "gold_5000",
                Name = "富可敌国",
                Description = "单次冒险累计获得 5000 金币",
                IconId = "gold_treasure",
                Category = AchievementCategory.Collection,
                Points = 40,
                MaxProgress = 5000,
                PrerequisiteIds = new List<string> { "gold_1000" }
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "use_all_potion_types",
                Name = "药剂师",
                Description = "使用过所有类型的药水",
                IconId = "potion_mix",
                Category = AchievementCategory.Collection,
                Points = 25
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "perfect_hand",
                Name = "天胡",
                Description = "起手抽到所有理想卡牌（同类型5张）",
                IconId = "lucky_hand",
                Category = AchievementCategory.Secret,
                Points = 55,
                IsSecret = true,
                Hidden = true
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "die_to_first_enemy",
                Name = "出师不利",
                Description = "在第一个敌人面前战败...",
                IconId = "tombstone",
                Category = AchievementCategory.Secret,
                Points = 5,
                IsSecret = true,
                Hidden = true
            });

            AddAchievement(new AchievementDefinition
            {
                Id = "play_100_hours",
                Name = " dedication",
                Description = "累计游戏时间达到 100 小时",
                IconId = "clock",
                Category = AchievementCategory.Challenge,
                Points = 100,
                MaxProgress = 100
            });
        }

        private void AddAchievement(AchievementDefinition def)
        {
            _definitions[def.Id] = def;
            if (!_progress.ContainsKey(def.Id))
            {
                _progress[def.Id] = new AchievementData
                {
                    Id = def.Id,
                    Unlocked = false,
                    Progress = 0
                };
            }
        }

        #endregion

        #region Public API

        public void UpdateProgress(string achievementId, int delta = 1)
        {
            if (!_definitions.TryGetValue(achievementId, out var def)) return;
            if (def.IsUnlocked) return;

            var data = _progress[achievementId];
            data.Progress = Mathf.Min(data.Progress + delta, def.MaxProgress);
            def.CurrentProgress = data.Progress;

            EmitSignal(SignalName.AchievementProgressUpdated, achievementId, data.Progress, def.MaxProgress);

            if (data.Progress >= def.MaxProgress && !data.Unlocked)
            {
                UnlockAchievement(achievementId);
            }

            SaveProgress();
        }

        public void SetProgress(string achievementId, int value)
        {
            if (!_definitions.ContainsKey(achievementId)) return;
            var delta = value - _progress[achievementId].Progress;
            if (delta > 0)
                UpdateProgress(achievementId, delta);
        }

        public void UnlockAchievement(string achievementId)
        {
            if (!_definitions.TryGetValue(achievementId, out var def)) return;

            var data = _progress[achievementId];
            if (data.Unlocked) return;

            data.Unlocked = true;
            data.UnlockTime = DateTime.Now;
            data.Progress = def.MaxProgress;
            def.CurrentProgress = def.MaxProgress;

            if (def.IsSecret && !_secretRevealed.Contains(achievementId))
            {
                _secretRevealed.Add(achievementId);
                EmitSignal(SignalName.SecretAchievementRevealed, achievementId);
            }

            if (!def.Hidden)
            {
                EnqueuePopup(def);
            }

            EmitSignal(SignalName.AchievementUnlocked, achievementId);

            CheckAllComplete();
            SaveProgress();

            GD.Print($"[Achievement] Unlocked: {def.Name} (+{def.Points} points)");
        }

        public void EnqueuePopup(AchievementDefinition def)
        {
            _unlockQueue.Add(def.Id);
            if (!_isShowingPopup)
                ShowNextPopup();
        }

        public bool IsUnlocked(string achievementId)
        {
            return _progress.TryGetValue(achievementId, out var d) && d.Unlocked;
        }

        public int GetProgress(string achievementId)
        {
            return _progress.TryGetValue(achievementId, out var d) ? d.Progress : 0;
        }

        public float GetProgressPercent(string achievementId)
        {
            if (_definitions.TryGetValue(achievementId, out var def))
                return def.ProgressPercent;
            return 0f;
        }

        public AchievementDefinition GetDefinition(string achievementId)
        {
            return _definitions.TryGetValue(achievementId, out var def) ? def : null;
        }

        public List<AchievementDefinition> GetByCategory(AchievementCategory category)
        {
            var result = new List<AchievementDefinition>();
            foreach (var def in _definitions.Values)
            {
                if (def.Category == category && !def.IsSecret)
                    result.Add(def);
            }
            return result;
        }

        public List<AchievementDefinition> GetAllDefinitions(bool includeSecrets = false)
        {
            var result = new List<AchievementDefinition>();
            foreach (var def in _definitions.Values)
            {
                if (includeSecrets || !def.IsSecret)
                    result.Add(def);
            }
            return result;
        }

        public int TotalPoints
        {
            get
            {
                int total = 0;
                foreach (var d in _progress.Values)
                    if (d.Unlocked && _definitions.TryGetValue(d.Id, out var def))
                        total += def.Points;
                return total;
            }
        }

        public int MaxPossiblePoints
        {
            get
            {
                int total = 0;
                foreach (var def in _definitions.Values)
                    if (!def.IsSecret)
                        total += def.Points;
                return total;
            }
        }

        public int UnlockedCount
        {
            get
            {
                int count = 0;
                foreach (var d in _progress.Values)
                    if (d.Unlocked) count++;
                return count;
            }
        }

        public int TotalCount => _definitions.Count;

        #endregion

        #region Popup Display

        private void ShowNextPopup()
        {
            if (_queueIndex >= _unlockQueue.Count)
            {
                _isShowingPopup = false;
                _unlockQueue.Clear();
                _queueIndex = 0;
                return;
            }

            _isShowingPopup = true;
            var achId = _unlockQueue[_queueIndex];
            var def = _definitions[achId];

            CreatePopupIfNeeded();
            SetupPopupContent(def);
            AnimatePopupIn();

            _displayTimer = new Godot.Timer();
            _displayTimer.WaitTime = 3.5f;
            _displayTimer.OneShot = true;
            _displayTimer.Timeout += () =>
            {
                AnimatePopupOut();
                _queueIndex++;
                GetTree().CreateTimer(0.4f).Timeout += ShowNextPopup;
            };
            AddChild(_displayTimer);
            _displayTimer.Start();
        }

        private void CreatePopupIfNeeded()
        {
            if (_popupPanel != null && IsInstanceValid(_popupPanel)) return;

            _popupPanel = new Panel();
            _popupPanel.Name = "AchievementPopup";
            _popupPanel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            _popupPanel.Position = new Vector2(0, -180);
            _popupPanel.Size = new Vector2(420, 110);
            _popupPanel.ZIndex = 190;
            _popupPanel.MouseFilter = Control.MouseFilterEnum.Ignore;

            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.12f, 0.09f, 0.16f, 0.96f);
            style.BorderColor = new Color(0.85f, 0.7f, 0.2f);
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(12);
            style.ShadowSize = 8;
            style.ShadowColor = new Color(0, 0, 0, 0.5f);
            _popupPanel.AddThemeStyleboxOverride("panel", style);

            var mainHBox = new HBoxContainer();
            mainHBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);

            _iconDisplay = new TextureRect();
            _iconDisplay.CustomMinimumSize = new Vector2(72, 72);
            _iconDisplay.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
            _iconDisplay.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            mainHBox.AddChild(_iconDisplay);

            var vbox = new VBoxContainer();
            vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

            _nameLabel = new Label();
            _nameLabel.HorizontalAlignment = HorizontalAlignment.Left;
            var nameFont = _nameLabel.GetThemeFont("font");
            if (nameFont != null)
            {
                var themeData = new Theme();
                themeData.DefaultFont = nameFont;
                _nameLabel.AddThemeFontSizeOverride("font_size", 17);
            }
            else
            {
                _nameLabel.AddThemeFontSizeOverride("font_size", 17);
            }
            _nameLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.2f));
            vbox.AddChild(_nameLabel);

            _descLabel = new RichTextLabel();
            _descLabel.CustomMinimumSize = new Vector2(280, 36);
            _descLabel.BbcodeEnabled = true;
            _descLabel.ScrollActive = false;
            _descLabel.FitContent = true;
            _descLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
            vbox.AddChild(_descLabel);

            _pointsLabel = new Label();
            _pointsLabel.HorizontalAlignment = HorizontalAlignment.Right;
            _pointsLabel.AddThemeFontSizeOverride("font_size", 14);
            _pointsLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.6f, 0.3f));
            vbox.AddChild(_pointsLabel);

            mainHBox.AddChild(vbox);
            _popupPanel.AddChild(mainHBox);

            GetTree().Root.AddChild(_popupPanel);
        }

        private void SetupPopupContent(AchievementDefinition def)
        {
            _nameLabel.Text = $"🏆 {def.Name}";

            string descToShow = def.Description;
            if (def.Hidden && !IsUnlocked(def.Id))
                descToShow = def.HiddenDescription ?? "???";

            _descLabel.Clear();
            _descLabel.PushColor(new Color(0.85f, 0.82f, 0.75f));
            _descLabel.AppendText(descToShow);
            _descLabel.Pop();

            _pointsLabel.Text = $"+{def.Points} 点";

            var tierColor = GetTierColor(def);
            var iconTex = GenerateAchievementIcon(def, tierColor);
            _iconDisplay.Texture = iconTex;
        }

        private ImageTexture GenerateAchievementIcon(AchievementDefinition def, Color tierColor)
        {
            const int size = 64;
            var img = Image.Create(size, size, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));

            DrawCircleFilled(img, size / 2, size / 2, size / 2 - 2, tierColor.Darkened(0.3f));
            DrawCircleFilled(img, size / 2, size / 2, size / 2 - 5, tierColor);

            switch (def.Category)
            {
                case AchievementCategory.Combat:
                    DrawMiniSword(img, size / 2, size / 2, 18, Colors.White);
                    break;
                case AchievementCategory.Exploration:
                    DrawMiniCompass(img, size / 2, size / 2, 18, Colors.White);
                    break;
                case AchievementCategory.Collection:
                    DrawMiniGem(img, size / 2, size / 2, 16, Colors.White);
                    break;
                case AchievementCategory.Challenge:
                    DrawMiniFlame(img, size / 2, size / 2, 18, Colors.White);
                    break;
                case AchievementCategory.Story:
                    DrawMiniStar(img, size / 2, size / 2, 20, Colors.White);
                    break;
                default:
                    DrawMiniTrophy(img, size / 2, size / 2, 18, Colors.White);
                    break;
            }

            return ImageTexture.CreateFromImage(img);
        }

        private Color GetTierColor(AchievementDefinition def)
        {
            return def.Points switch
            {
                >= 100 => new Color(0.9f, 0.7f, 0.2f),
                >= 50 => new Color(0.7f, 0.5f, 0.15f),
                >= 30 => new Color(0.6f, 0.5f, 0.3f),
                >= 15 => new Color(0.55f, 0.45f, 0.2f),
                _ => new Color(0.5f, 0.42f, 0.18f)
            };
        }

        private void AnimatePopupIn()
        {
            _popupPanel.Position = new Vector2(0, -180);
            _popupPanel.Scale = new Vector2(0.85f, 0.85f);

            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(_popupPanel, "position:y", 10f, 0.5f)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(_popupPanel, "modulate:a", 1f, 0.35f)
                .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(_popupPanel, "scale", Vector2.One, 0.4f)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        }

        private void AnimatePopupOut()
        {
            var tween = CreateTween();
            tween.TweenProperty(_popupPanel, "position:y", -180f, 0.35f)
                .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);
            tween.TweenProperty(_popupPanel, "modulate:a", 0f, 0.3f)
                .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
        }

        #endregion

        #region Persistence

        private void SaveProgress()
        {
            var config = new ConfigFile();
            foreach (var kv in _progress)
            {
                config.SetValue("achievements", $"{kv.Key}_unlocked", kv.Value.Unlocked);
                config.SetValue("achievements", $"{kv.Key}_progress", kv.Value.Progress);
                if (kv.Value.UnlockTime != default(DateTime))
                    config.SetValue("achievements", $"{kv.Key}_time", kv.Value.UnlockTime.ToString("O"));
            }
            config.SetValue("achievements", "_secrets_revealed", string.Join(",", new List<string>(_secretRevealed)));
            config.SetValue("achievements", "_version", 2);
            config.Save("user://achievements.cfg");
        }

        private void LoadProgress()
        {
            var config = new ConfigFile();
            var err = config.Load("user://achievements.cfg");
            if (err != Error.Ok) return;

            foreach (var achId in _definitions.Keys)
            {
                var unlocked = config.GetValue("achievements", $"{achId}_unlocked", false).AsBool();
                var progress = (int)config.GetValue("achievements", $"{achId}_progress", 0);

                _progress[achId] = new AchievementData
                {
                    Id = achId,
                    Unlocked = unlocked,
                    Progress = progress
                };

                if (_definitions.TryGetValue(achId, out var def))
                    def.CurrentProgress = progress;
            }

            var secrets = config.GetValue("achievements", "_secrets_revealed", new Godot.Collections.Array<string>())
                .AsGodotArray<string>();
            foreach (var s in secrets)
                _secretRevealed.Add(s);

            int unlockedCount = 0;
            foreach (var p in _progress.Values)
                if (p.Unlocked) unlockedCount++;
            GD.Print($"[AchievementManager] Loaded {unlockedCount}/{_definitions.Count} achievements");
        }

        public void ResetAllProgress()
        {
            foreach (var key in _progress.Keys)
            {
                _progress[key].Unlocked = false;
                _progress[key].Progress = 0;
                if (_definitions.TryGetValue(key, out var def))
                    def.CurrentProgress = 0;
            }
            _secretRevealed.Clear();
            SaveProgress();
            GD.Print("[AchievementManager] All progress reset");
        }

        #endregion

        private void CheckAllComplete()
        {
            foreach (var d in _progress.Values)
                if (!d.Unlocked && !_definitions[d.Id].IsSecret) return;
            EmitSignal(SignalName.AllAchievementsUnlocked);
        }

        #region Drawing Helpers

        private static void DrawCircleFilled(Image img, int cx, int cy, int radius, Color color)
        {
            for (int dy = -radius; dy <= radius; dy++)
                for (int dx = -radius; dx <= radius; dx++)
                    if (dx * dx + dy * dy <= radius * radius)
                        SetPixelSafe(img, cx + dx, cy + dy, color);
        }

        private static void DrawTriangleFilled(Image img, int x0, int y0, int x1, int y1, int x2, int y2, Color color)
        {
            int minX = Mathf.Min(x0, Mathf.Min(x1, x2)), maxX = Mathf.Max(x0, Mathf.Max(x1, x2));
            int minY = Mathf.Min(y0, Mathf.Min(y1, y2)), maxY = Mathf.Max(y0, Mathf.Max(y1, y2));
            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                {
                    float d1 = (x1 - x0) * (y - y0) - (x - x0) * (y1 - y0);
                    float d2 = (x2 - x1) * (y - y1) - (x - x1) * (y2 - y1);
                    float d3 = (x0 - x2) * (y - y2) - (x - x2) * (y0 - y2);
                    if (!((d1 < 0) || (d2 < 0) || (d3 < 0)) && !((d1 > 0) || (d2 > 0) || (d3 > 0)))
                        SetPixelSafe(img, x, y, color);
                }
        }

        private static void SetPixelSafe(Image img, int x, int y, Color color)
        {
            if (x >= 0 && x < img.GetWidth() && y >= 0 && y < img.GetHeight())
                img.SetPixel(x, y, color);
        }

        private static void DrawLineThick(Image img, int x0, int y0, int x1, int y1, Color color, int thickness = 2)
        {
            int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0)) * 2;
            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                int x = (int)Mathf.Lerp(x0, x1, t), y = (int)Mathf.Lerp(y0, y1, t);
                for (int dy = -thickness; dy <= thickness; dy++)
                    for (int dx = -thickness; dx <= thickness; dx++)
                        if (dx * dx + dy * dy <= thickness * thickness)
                            SetPixelSafe(img, x + dx, y + dy, color);
            }
        }

        private static void DrawMiniSword(Image img, int cx, int cy, int size, Color c)
        {
            int h = size / 2;
            DrawLineThick(img, cx - h + 4, cy + h, cx + 2, cy - h + 8, c, 3);
            DrawLineThick(img, cx + 2, cy - h + 8, cx + h - 3, cy - h + 14, c, 2);
            DrawLineThick(img, cx - 5, cy + h - 3, cx + 5, cy + h + 3, c, 3);
        }

        private static void DrawMiniCompass(Image img, int cx, int cy, int r, Color c)
        {
            DrawCircleFilled(img, cx, cy, r - 3, c.Darkened(0.3f));
            DrawCircleOutline(img, cx, cy, r - 2, c, 1);
            DrawTriangleFilled(img, cx, cy - r + 4, cx - 4, cy + 2, cx + 4, cy + 2, c);
            DrawTriangleFilled(img, cx, cy + r - 4, cx - 3, cy - 2, cx + 3, cy - 2, c.Darkened(0.4f));
        }

        private static void DrawCircleOutline(Image img, int cx, int cy, int rad, Color col, int thick = 1)
        {
            for (int dy = -rad; dy <= rad; dy++)
                for (int dx = -rad; dx <= rad; dx++)
                    if (Mathf.Abs(Mathf.Sqrt(dx * dx + dy * dy) - rad) < thick)
                        SetPixelSafe(img, cx + dx, cy + dy, col);
        }

        private static void DrawMiniGem(Image img, int cx, int cy, int size, Color c)
        {
            DrawTriangleFilled(img, cx, cy - size, cx - size, cy + size / 3, cx + size, cy + size / 3, c);
            DrawTriangleFilled(img, cx, cy + size / 3, cx - size, cy + size / 3, cx, cy + size, c.Darkened(0.3f));
            DrawTriangleFilled(img, cx, cy + size / 3, cx + size, cy + size / 3, cx, cy + size, c.Darkened(0.3f));
        }

        private static void DrawMiniFlame(Image img, int cx, int cy, int size, Color c)
        {
            DrawTriangleFilled(img, cx, cy - size, cx - size * 2 / 3, cy + size / 2, cx + size * 2 / 3, cy + size / 2, c);
            DrawTriangleFilled(img, cx, cy - size / 3, cx - size / 2, cy + size * 2 / 3, cx + size / 2, cy + size * 2 / 3, c.Lightened(0.3f));
            DrawTriangleFilled(img, cx, cy + size / 3, cx - size / 4, cy + size, cx + size / 4, cy + size, c.Lightened(0.6f));
        }

        private static void DrawMiniStar(Image img, int cx, int cy, int outerR, Color c)
        {
            int innerR = outerR * 2 / 5;
            for (int i = 0; i < 10; i++)
            {
                float a1 = i * Mathf.Pi / 5f - Mathf.Pi / 2f;
                float a2 = (i + 0.5f) * Mathf.Pi / 5f - Mathf.Pi / 2f;
                int r1 = (i % 2 == 0) ? outerR : innerR;
                int r2 = (i % 2 == 0) ? innerR : outerR;
                DrawLineThick(img, cx + (int)(Mathf.Cos(a1) * r1), cy + (int)(Mathf.Sin(a1) * r1),
                    cx + (int)(Mathf.Cos(a2) * r2), cy + (int)(Mathf.Sin(a2) * r2), c, 2);
            }
        }

        private static void DrawMiniTrophy(Image img, int cx, int cy, int size, Color c)
        {
            DrawEllipseFilled(img, cx, cy - size / 3, size / 2, size / 3, c);
            DrawRectFilled(img, cx - size / 5, cy, size * 2 / 5, size / 2, c);
            DrawRectFilled(img, cx - size / 2, cy + size / 2 - 2, size, size / 5, c.Darkened(0.2f));
            DrawRectFilled(img, cx - size / 2, cy + size / 2 + size / 5 - 2, size / 4, size / 5, c.Darkened(0.2f));
            DrawRectFilled(img, cx + size / 4, cy + size / 2 + size / 5 - 2, size / 4, size / 5, c.Darkened(0.2f));
        }

        private static void DrawEllipseFilled(Image img, int cx, int cy, int rx, int ry, Color color)
        {
            for (int dy = -ry; dy <= ry; dy++)
                for (int dx = -rx; dx <= rx; dx++)
                    if ((float)(dx * dx) / (rx * rx) + (float)(dy * dy) / (ry * ry) <= 1f)
                        SetPixelSafe(img, cx + dx, cy + dy, color);
        }

        private static void DrawRectFilled(Image img, int x, int y, int w, int h, Color color)
        {
            for (int dy = 0; dy < h; dy++)
                for (int dx = 0; dx < w; dx++)
                    SetPixelSafe(img, x + dx, y + dy, color);
        }

        #endregion
    }
}
