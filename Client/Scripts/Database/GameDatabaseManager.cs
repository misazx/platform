using Godot;
using System;
using System.Collections.Generic;

namespace RoguelikeGame.Database
{
    public partial class GameDatabaseManager : Node
    {
        public static GameDatabaseManager Instance { get; private set; }

        [Signal]
        public delegate void DatabaseInitializedEventHandler();

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;

            InitializeAllDatabases();
        }

        private void InitializeAllDatabases()
        {
            GD.Print("[GameDatabaseManager] Initializing all databases...");

            var databases = new List<Node>
            {
                new CharacterDatabase(),
                new CardDatabase(),
                new EnemyDatabase(),
                new RelicDatabase(),
                new PotionDatabase(),
                new EventDatabase(),
                new AchievementSystem(),
                new TimelineManager()
            };

            foreach (var database in databases)
            {
                GetTree().Root.CallDeferred("add_child", database);
            }

            CallDeferred(nameof(OnInitializationComplete));
        }

        private void OnInitializationComplete()
        {
            EmitSignal(SignalName.DatabaseInitialized);
            
            GD.Print("========================================");
            GD.Print("[GameDatabaseManager] All databases initialized!");
            GD.Print($"  - Characters: {CharacterDatabase.Instance.TotalCharacters}");
            GD.Print($"  - Cards: {CardDatabase.Instance.TotalCards}");
            GD.Print($"  - Enemies: {EnemyDatabase.Instance.TotalEnemies}");
            GD.Print($"  - Relics: {RelicDatabase.Instance.TotalRelics}");
            GD.Print($"  - Potions: {PotionDatabase.Instance.TotalPotions}");
            GD.Print($"  - Events: {EventDatabase.Instance.TotalEvents}");
            GD.Print($"  - Achievements: {AchievementSystem.Instance.TotalAchievements}");
            GD.Print("========================================");
        }

        public CharacterData GetCharacter(string id) => CharacterDatabase.Instance.GetCharacter(id);
        
        public CardData GetCard(string id) => CardDatabase.Instance.GetCard(id);
        
        public EnemyData GetEnemy(string id) => EnemyDatabase.Instance.GetEnemy(id);
        
        public RelicData GetRelic(string id) => RelicDatabase.Instance.GetRelic(id);
        
        public PotionData GetPotion(string id) => PotionDatabase.Instance.GetPotion(id);
        
        public EventData GetEvent(string id) => EventDatabase.Instance.GetEvent(id);

        public List<CharacterData> GetAllCharacters() => CharacterDatabase.Instance.GetAllCharacters();
        
        public List<CardData> GetAllCards() => CardDatabase.Instance.GetAllCards();
        
        public List<EnemyData> GetAllEnemies() => EnemyDatabase.Instance.GetAllEnemies();
        
        public List<RelicData> GetAllRelics() => RelicDatabase.Instance.GetAllRelics();
        
        public List<PotionData> GetAllPotions() => PotionDatabase.Instance.GetAllPotions();
        
        public List<EventData> GetAllEvents() => EventDatabase.Instance.GetAllEvents();

        public string GenerateDatabaseReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== 游戏数据库报告 ===");
            report.AppendLine($"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            report.AppendLine("--- 角色系统 ---");
            report.AppendLine($"总角色数: {CharacterDatabase.Instance.TotalCharacters}");
            foreach (var character in CharacterDatabase.Instance.GetAllCharacters())
            {
                report.AppendLine($"  - {character.Name} ({character.Class})");
            }
            report.AppendLine();
            report.AppendLine("--- 卡牌系统 ---");
            report.AppendLine($"总卡牌数: {CardDatabase.Instance.TotalCards}");
            var cardTypes = Enum.GetValues(typeof(CardType));
            foreach (CardType type in cardTypes)
            {
                var count = CardDatabase.Instance.GetCardsByType(type).Count;
                if (count > 0)
                    report.AppendLine($"  - {type}: {count} 张");
            }
            report.AppendLine();
            report.AppendLine("--- 敌人系统 ---");
            report.AppendLine($"总敌人数: {EnemyDatabase.Instance.TotalEnemies}");
            report.AppendLine($"  - 普通敌人: {EnemyDatabase.Instance.GetNormalEnemies().Count}");
            report.AppendLine($"  - 精英敌人: {EnemyDatabase.Instance.GetEliteEnemies().Count}");
            report.AppendLine($"  - Boss敌人: {EnemyDatabase.Instance.GetBossEnemies().Count}");
            report.AppendLine();
            report.AppendLine("--- 遗物系统 ---");
            report.AppendLine($"总遗物数: {RelicDatabase.Instance.TotalRelics}");
            var relicTiers = Enum.GetValues(typeof(RelicTier));
            foreach (RelicTier tier in relicTiers)
            {
                var count = RelicDatabase.Instance.GetRelicsByTier(tier).Count;
                if (count > 0)
                    report.AppendLine($"  - {tier}: {count} 个");
            }
            report.AppendLine();
            report.AppendLine("--- 药水系统 ---");
            report.AppendLine($"总药水数: {PotionDatabase.Instance.TotalPotions}");
            report.AppendLine();
            report.AppendLine("--- 事件系统 ---");
            report.AppendLine($"总事件数: {EventDatabase.Instance.TotalEvents}");
            report.AppendLine();
            report.AppendLine("--- 成就系统 ---");
            report.AppendLine($"总成就数: {AchievementSystem.Instance.TotalAchievements}");
            report.AppendLine($"已解锁: {AchievementSystem.Instance.UnlockedCount}");
            report.AppendLine($"完成度: {AchievementSystem.Instance.OverallCompletion:F1}%");

            return report.ToString();
        }
    }
}
