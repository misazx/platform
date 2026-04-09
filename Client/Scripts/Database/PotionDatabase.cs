using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using RoguelikeGame.Core;

namespace RoguelikeGame.Database
{
    public enum PotionType
    {
        Attack,
        Defense,
        Utility,
        Special
    }

    public class PotionData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        public PotionType Type { get; set; } = PotionType.Utility;
        
        public int Price { get; set; } = 50;
        public int Rarity { get; set; } = 1;
        
        public string ImagePath { get; set; }
        public string Color { get; set; } = "#FF00FF";
        
        public Dictionary<string, object> Effects { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        
        public bool IsStackable { get; set; }
        public int MaxStack { get; set; } = 3;
    }

    public partial class PotionDatabase : Node
    {
        public static PotionDatabase Instance { get; private set; }

        private readonly Dictionary<string, PotionData> _potions = new();
        private readonly Dictionary<PotionType, List<PotionData>> _typePotions = new();

        [Signal]
        public delegate void PotionUsedEventHandler(string potionId);

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;

            LoadPotionsFromConfig();
        }

        private void LoadPotionsFromConfig()
        {
            var config = ConfigLoader.LoadConfig<PotionConfigData>("potions");
            
            if (config == null)
            {
                GD.PrintErr("[PotionDatabase] Failed to load potions config!");
                return;
            }

            foreach (var potionConfig in config.Potions)
            {
                var potionData = ConvertConfigToData(potionConfig);
                RegisterPotion(potionData);
            }

            GD.Print($"[PotionDatabase] Loaded {_potions.Count} potions from config (version: {config.Version})");
        }

        private PotionData ConvertConfigToData(PotionConfig config)
        {
            return new PotionData
            {
                Id = config.Id,
                Name = config.Name,
                Description = config.Description,
                Type = ParsePotionType(config.Type),
                Price = config.Price,
                Rarity = config.Rarity,
                ImagePath = config.ImagePath,
                Color = config.Color,
                Effects = new Dictionary<string, object>(config.Effects),
                Tags = new List<string>(config.Tags),
                IsStackable = config.IsStackable,
                MaxStack = config.MaxStack
            };
        }

        private PotionType ParsePotionType(string type)
        {
            return type?.ToLower() switch
            {
                "attack" => PotionType.Attack,
                "defense" => PotionType.Defense,
                "utility" => PotionType.Utility,
                "special" => PotionType.Special,
                _ => PotionType.Utility
            };
        }

        public void RegisterPotion(PotionData potion)
        {
            _potions[potion.Id] = potion;

            if (!_typePotions.ContainsKey(potion.Type))
                _typePotions[potion.Type] = new List<PotionData>();
            
            _typePotions[potion.Type].Add(potion);
        }

        public PotionData GetPotion(string potionId)
        {
            return _potions.TryGetValue(potionId, out var potion) ? potion : null;
        }

        public List<PotionData> GetAllPotions()
        {
            return new List<PotionData>(_potions.Values);
        }

        public List<PotionData> GetPotionsByType(PotionType type)
        {
            return _typePotions.TryGetValue(type, out var potions) 
                ? potions 
                : new List<PotionData>();
        }

        public PotionData GetRandomPotion(RandomNumberGenerator rng = null)
        {
            if (_potions.Count == 0)
                return null;

            if (rng != null)
            {
                int idx = (int)(rng.Randf() * _potions.Count);
                return _potions.Values.ElementAt(idx);
            }
            
            var randomIndex = (int)(GD.Randi() % _potions.Count);
            return _potions.Values.ToList()[randomIndex];
        }

        public int TotalPotions => _potions.Count;
    }
}
