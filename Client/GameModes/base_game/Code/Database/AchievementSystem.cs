using Godot;
using System;
using System.Collections.Generic;

namespace RoguelikeGame.Database
{
    public enum AchievementType
    {
        Progression,
        Combat,
        Collection,
        Challenge,
        Secret
    }

    public class AchievementData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FlavorText { get; set; }
        
        public AchievementType Type { get; set; } = AchievementType.Progression;
        
        public bool IsUnlocked { get; set; }
        public DateTime UnlockTime { get; set; }
        
        public int Points { get; set; } = 10;
        public bool IsHidden { get; set; }
        
        public string IconPath { get; set; }
        public string ImagePath { get; set; }
        
        public Dictionary<string, object> Requirements { get; set; } = new();
        public Dictionary<string, object> Rewards { get; set; } = new();
        
        public float CompletionProgress { get; set; }
        public int CurrentValue { get; set; }
        public int TargetValue { get; set; }
    }

    public class RunStatistics
    {
        public string CharacterId { get; set; }
        public int FloorReached { get; set; }
        public int EnemiesDefeated { get; set; }
        public int DamageDealt { get; set; }
        public int CardsPlayed { get; set; }
        public int RelicsCollected { get; set; }
        public int GoldEarned { get; set; }
        public bool Victory { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public uint Seed { get; set; }
        public List<string> DeckComposition { get; set; } = new();
    }

    public partial class AchievementSystem : Node
    {
        public static AchievementSystem Instance { get; private set; }

        private readonly Dictionary<string, AchievementData> _achievements = new();
        private readonly List<RunStatistics> _runHistory = new();
        private readonly Dictionary<AchievementType, List<AchievementData>> _typeAchievements = new();

        [Signal]
        public delegate void AchievementUnlockedEventHandler(string achievementId);

        [Signal]
        public delegate void AchievementProgressUpdatedEventHandler(string achievementId, float progress);

        [Signal]
        public delegate void RunCompletedEventHandler(string statsJson);

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;

            InitializeAchievements();
        }

        private void InitializeAchievements()
        {
            RegisterAchievement(new AchievementData
            {
                Id = "first_victory",
                Name = "初次胜利",
                Description = "完成一次游戏通关。",
                FlavorText = "这只是开始。",

                Type = AchievementType.Progression,

                Points = 50,
                IsHidden = false,

                IconPath = "res://GameModes/base_game/Resources/Icons/Achievements/FirstVictory.png",

                Requirements = new Dictionary<string, object>
                {
                    { "victory", true }
                },

                Rewards = new Dictionary<string, object>
                {
                    { "unlock_character", "watcher" },
                    { "gold_bonus", 100 }
                },

                TargetValue = 1
            });

            RegisterAchievement(new AchievementData
            {
                Id = "kill_100_enemies",
                Name = "百人斩",
                Description = "累计击败 100 个敌人。",
                FlavorText = "你的剑已经饥渴难耐。",

                Type = AchievementType.Combat,

                Points = 30,
                IsHidden = false,

                IconPath = "res://GameModes/base_game/Resources/Icons/Achievements/Kill100.png",

                Requirements = new Dictionary<string, object>
                {
                    { "total_kills", 100 }
                },

                TargetValue = 100,
                CurrentValue = 0
            });

            RegisterAchievement(new AchievementData
            {
                Id = "collect_all_relics",
                Name = "收藏家",
                Description = "收集所有遗物。",
                FlavorText = "每一件遗物都有它的故事。",

                Type = AchievementType.Collection,

                Points = 100,
                IsHidden = false,

                IconPath = "res://GameModes/base_game/Resources/Icons/Achievements/AllRelics.png",

                Requirements = new Dictionary<string, object>
                {
                    { "relics_collected", "all" }
                },

                TargetValue = 50
            });

            RegisterAchievement(new AchievementData
            {
                Id = "no_damage_run",
                Name = "无伤通关",
                Description = "在不受到任何伤害的情况下完成一整层。",
                FlavorText = "完美的战斗艺术。",

                Type = AchievementType.Challenge,

                Points = 200,
                IsHidden = true,

                IconPath = "res://GameModes/base_game/Resources/Icons/Achievements/NoDamage.png",

                TargetValue = 1
            });

            GD.Print($"[AchievementSystem] Initialized with {_achievements.Count} achievements");
        }

        public void RegisterAchievement(AchievementData achievement)
        {
            _achievements[achievement.Id] = achievement;

            if (!_typeAchievements.ContainsKey(achievement.Type))
                _typeAchievements[achievement.Type] = new List<AchievementData>();
            
            _typeAchievements[achievement.Type].Add(achievement);
        }

        public AchievementData GetAchievement(string achievementId)
        {
            return _achievements.TryGetValue(achievementId, out var achievement) ? achievement : null;
        }

        public List<AchievementData> GetAllAchievements()
        {
            return new List<AchievementData>(_achievements.Values);
        }

        public List<AchievementData> GetUnlockedAchievements()
        {
            var result = new List<AchievementData>();
            foreach (var achievement in _achievements.Values)
            {
                if (achievement.IsUnlocked)
                    result.Add(achievement);
            }
            return result;
        }

        public List<AchievementData> GetLockedAchievements()
        {
            var result = new List<AchievementData>();
            foreach (var achievement in _achievements.Values)
            {
                if (!achievement.IsUnlocked && !achievement.IsHidden)
                    result.Add(achievement);
            }
            return result;
        }

        public void UpdateProgress(string achievementId, int value)
        {
            if (!_achievements.TryGetValue(achievementId, out var achievement))
                return;

            achievement.CurrentValue += value;
            achievement.CompletionProgress = (float)achievement.CurrentValue / achievement.TargetValue;

            EmitSignal(SignalName.AchievementProgressUpdated, achievementId, achievement.CompletionProgress);

            if (achievement.CurrentValue >= achievement.TargetValue && !achievement.IsUnlocked)
            {
                UnlockAchievement(achievementId);
            }
        }

        public void UnlockAchievement(string achievementId)
        {
            if (!_achievements.TryGetValue(achievementId, out var achievement))
                return;

            if (achievement.IsUnlocked)
                return;

            achievement.IsUnlocked = true;
            achievement.UnlockTime = DateTime.Now;

            EmitSignal(SignalName.AchievementUnlocked, achievementId);

            GD.Print($"[AchievementSystem] Unlocked: {achievement.Name}");
        }

        public void RecordRun(RunStatistics runStats)
        {
            _runHistory.Add(runStats);

            UpdateProgress("kill_100_enemies", runStats.EnemiesDefeated);

            if (runStats.Victory)
            {
                UnlockAchievement("first_victory");
            }

            EmitSignal(SignalName.RunCompleted, runStats.CharacterId);
        }

        public List<RunStatistics> GetRunHistory(int count = 10)
        {
            var start = Math.Max(0, _runHistory.Count - count);
            return _runHistory.GetRange(start, Math.Min(count, _runHistory.Count - start));
        }

        public int TotalRuns => _runHistory.Count;
        public int TotalAchievements => _achievements.Count;
        public int UnlockedCount => GetUnlockedAchievements().Count;
        public float OverallCompletion => (float)UnlockedCount / TotalAchievements * 100f;
    }
}
