using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using RoguelikeGame.Core;

namespace RoguelikeGame.Database
{
    public enum RelicTier
    {
        Starter,
        Common,
        Uncommon,
        Rare,
        Boss,
        Special,
        Shop
    }

    public enum RelicType
    {
        Passive,
        Active,
        Consumable
    }

    public class RelicData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FlavorText { get; set; }
        
        public RelicTier Tier { get; set; } = RelicTier.Common;
        public RelicType Type { get; set; } = RelicType.Passive;
        
        public string ImagePath { get; set; }
        public string IconPath { get; set; }
        
        public List<string> CompatibleCharacters { get; set; } = new();
        public Dictionary<string, object> Effects { get; set; } = new();
        public Dictionary<string, object> Stats { get; set; } = new();
        
        public bool IsCounterpart { get; set; }
        public string CounterpartId { get; set; }
    }

    public partial class RelicDatabase : SingletonBase<RelicDatabase>
    {
        private readonly Dictionary<string, RelicData> _relics = new();
        private readonly Dictionary<RelicTier, List<RelicData>> _tierRelics = new();

        [Signal]
        public delegate void RelicCollectedEventHandler(string relicId);

        protected override void OnInitialize()
        {
            LoadRelicsFromConfig();
        }

        private void LoadRelicsFromConfig()
        {
            var config = ConfigLoader.LoadConfig<RelicConfigData>("relics");
            
            if (config == null)
            {
                GD.PrintErr("[RelicDatabase] Failed to load relics config!");
                return;
            }

            foreach (var relicConfig in config.Relics)
            {
                var relicData = ConvertConfigToData(relicConfig);
                RegisterRelic(relicData);
            }

            GD.Print($"[RelicDatabase] Loaded {_relics.Count} relics from config (version: {config.Version})");
        }

        private RelicData ConvertConfigToData(RelicConfig config)
        {
            return new RelicData
            {
                Id = config.Id,
                Name = config.Name,
                Description = config.Description,
                FlavorText = config.FlavorText,
                Tier = ParseRelicTier(config.Tier),
                Type = ParseRelicType(config.Type),
                ImagePath = config.ImagePath,
                IconPath = config.IconPath,
                CompatibleCharacters = new List<string>(config.CompatibleCharacters),
                Effects = new Dictionary<string, object>(config.Effects),
                Stats = new Dictionary<string, object>(config.Stats),
                IsCounterpart = config.IsCounterpart,
                CounterpartId = config.CounterpartId
            };
        }

        private RelicTier ParseRelicTier(string tier)
        {
            return tier?.ToLower() switch
            {
                "starter" => RelicTier.Starter,
                "common" => RelicTier.Common,
                "uncommon" => RelicTier.Uncommon,
                "rare" => RelicTier.Rare,
                "boss" => RelicTier.Boss,
                "special" => RelicTier.Special,
                "shop" => RelicTier.Shop,
                _ => RelicTier.Common
            };
        }

        private RelicType ParseRelicType(string type)
        {
            return type?.ToLower() switch
            {
                "passive" => RelicType.Passive,
                "active" => RelicType.Active,
                "consumable" => RelicType.Consumable,
                _ => RelicType.Passive
            };
        }

        public void RegisterRelic(RelicData relic)
        {
            _relics[relic.Id] = relic;

            if (!_tierRelics.ContainsKey(relic.Tier))
                _tierRelics[relic.Tier] = new List<RelicData>();
            
            _tierRelics[relic.Tier].Add(relic);
        }

        public RelicData GetRelic(string relicId)
        {
            return _relics.TryGetValue(relicId, out var relic) ? relic : null;
        }

        public List<RelicData> GetAllRelics()
        {
            return new List<RelicData>(_relics.Values);
        }

        public List<RelicData> GetRelicsByTier(RelicTier tier)
        {
            return _tierRelics.TryGetValue(tier, out var relics) 
                ? relics 
                : new List<RelicData>();
        }

        public List<RelicData> GetStarterRelics()
        {
            return GetRelicsByTier(RelicTier.Starter);
        }

        public List<RelicData> GetCommonRelics()
        {
            return GetRelicsByTier(RelicTier.Common);
        }

        public List<RelicData> GetUncommonRelics()
        {
            return GetRelicsByTier(RelicTier.Uncommon);
        }

        public List<RelicData> GetRareRelics()
        {
            return GetRelicsByTier(RelicTier.Rare);
        }

        public List<RelicData> GetBossRelics()
        {
            return GetRelicsByTier(RelicTier.Boss);
        }

        public List<RelicData> GetRelicsForCharacter(string characterId)
        {
            var result = new List<RelicData>();
            foreach (var relic in _relics.Values)
            {
                if (relic.CompatibleCharacters.Contains(characterId) || 
                    relic.CompatibleCharacters.Contains("*"))
                {
                    result.Add(relic);
                }
            }
            return result;
        }

        public RelicData GetRandomRelic(RelicTier tier, RandomNumberGenerator rng = null)
        {
            var relics = GetRelicsByTier(tier);
            if (relics.Count == 0)
                return null;

            if (rng != null)
            {
                int idx = (int)(rng.Randf() * relics.Count);
                return relics[idx];
            }
            
            var randomIndex = (int)(GD.Randi() % relics.Count);
            return relics[randomIndex];
        }

        public int TotalRelics => _relics.Count;
    }
}
