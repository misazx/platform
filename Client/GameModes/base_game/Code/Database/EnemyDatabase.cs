using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;

namespace RoguelikeGame.Database
{
    public enum EnemyType
    {
        Normal,
        Elite,
        Boss
    }

    public enum EnemyBehavior
    {
        Aggressive,
        Defensive,
        Support,
        Summoner
    }

    public class EnemyData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        public int MaxHealth { get; set; } = 50;
        public int AttackDamage { get; set; } = 10;
        public int BlockAmount { get; set; } = 5;
        
        public EnemyType Type { get; set; } = EnemyType.Normal;
        public EnemyBehavior Behavior { get; set; } = EnemyBehavior.Aggressive;
        
        public string PortraitPath { get; set; }
        public string IconPath { get; set; }
        
        public List<string> Abilities { get; set; } = new();
        public List<string> Drops { get; set; } = new();
        
        public Dictionary<string, object> Stats { get; set; } = new();
        public Dictionary<string, object> CustomData { get; set; } = new();
        
        public float DifficultyRating { get; set; }
        public string EncounterLocation { get; set; }
    }

    public partial class EnemyDatabase : SingletonBase<EnemyDatabase>
    {
        private readonly Dictionary<string, EnemyData> _enemies = new();
        private readonly Dictionary<EnemyType, List<EnemyData>> _typeEnemies = new();

        [Signal]
        public delegate void EnemyRegisteredEventHandler(string enemyId);

        protected override void OnInitialize()
        {
            LoadEnemiesFromConfig();
        }

        private void LoadEnemiesFromConfig()
        {
            var config = ConfigLoader.LoadConfig<EnemyConfigData>("enemies");
            
            if (config == null)
            {
                GD.PrintErr("[EnemyDatabase] Failed to load enemies config!");
                return;
            }

            foreach (var enemyConfig in config.Enemies)
            {
                var enemyData = ConvertConfigToData(enemyConfig);
                RegisterEnemy(enemyData);
            }

            GD.Print($"[EnemyDatabase] Loaded {_enemies.Count} enemies from config (version: {config.Version})");
        }

        private EnemyData ConvertConfigToData(EnemyConfig config)
        {
            return new EnemyData
            {
                Id = config.Id,
                Name = config.Name,
                Description = config.Description,
                MaxHealth = config.MaxHealth,
                AttackDamage = config.AttackDamage,
                BlockAmount = config.BlockAmount,
                Type = ParseEnemyType(config.Type),
                Behavior = ParseEnemyBehavior(config.Behavior),
                PortraitPath = config.PortraitPath,
                IconPath = config.IconPath,
                Abilities = new List<string>(config.Abilities),
                Drops = new List<string>(config.Drops),
                Stats = new Dictionary<string, object>(config.Stats),
                CustomData = new Dictionary<string, object>(config.CustomData),
                DifficultyRating = config.DifficultyRating,
                EncounterLocation = config.EncounterLocation
            };
        }

        private EnemyType ParseEnemyType(string type)
        {
            return type?.ToLower() switch
            {
                "normal" => EnemyType.Normal,
                "elite" => EnemyType.Elite,
                "boss" => EnemyType.Boss,
                _ => EnemyType.Normal
            };
        }

        private EnemyBehavior ParseEnemyBehavior(string behavior)
        {
            return behavior?.ToLower() switch
            {
                "aggressive" => EnemyBehavior.Aggressive,
                "defensive" => EnemyBehavior.Defensive,
                "support" => EnemyBehavior.Support,
                "summoner" => EnemyBehavior.Summoner,
                _ => EnemyBehavior.Aggressive
            };
        }

        public void RegisterEnemy(EnemyData enemy)
        {
            _enemies[enemy.Id] = enemy;

            if (!_typeEnemies.ContainsKey(enemy.Type))
                _typeEnemies[enemy.Type] = new List<EnemyData>();
            
            _typeEnemies[enemy.Type].Add(enemy);

            EmitSignal(SignalName.EnemyRegistered, enemy.Id);
        }

        public EnemyData GetEnemy(string enemyId)
        {
            return _enemies.TryGetValue(enemyId, out var enemy) ? enemy : null;
        }

        public List<EnemyData> GetAllEnemies()
        {
            return new List<EnemyData>(_enemies.Values);
        }

        public List<EnemyData> GetEnemiesByType(EnemyType type)
        {
            return _typeEnemies.TryGetValue(type, out var enemies) 
                ? enemies 
                : new List<EnemyData>();
        }

        public List<EnemyData> GetNormalEnemies()
        {
            return GetEnemiesByType(EnemyType.Normal);
        }

        public List<EnemyData> GetEliteEnemies()
        {
            return GetEnemiesByType(EnemyType.Elite);
        }

        public List<EnemyData> GetBossEnemies()
        {
            return GetEnemiesByType(EnemyType.Boss);
        }

        public List<EnemyData> GetEnemiesByLocation(string location)
        {
            var result = new List<EnemyData>();
            foreach (var enemy in _enemies.Values)
            {
                if (enemy.EncounterLocation == location)
                    result.Add(enemy);
            }
            return result;
        }

        public int TotalEnemies => _enemies.Count;
    }
}
