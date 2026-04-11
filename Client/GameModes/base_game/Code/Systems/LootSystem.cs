using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using RoguelikeGame.Core;

namespace RoguelikeGame.Systems
{
    public class LootTable
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<LootEntry> Entries { get; set; } = new();
        public int MinItems { get; set; } = 1;
        public int MaxItems { get; set; } = 3;
    }

    public class LootEntry
    {
        public string ItemId { get; set; }
        public float Weight { get; set; } = 1.0f;
        public int MinCount { get; set; } = 1;
        public int MaxCount { get; set; } = 1;
        public float Chance { get; set; } = 1.0f;
        public ItemRarity? MinRarity { get; set; }
        public ItemRarity? MaxRarity { get; set; }
    }

    public class LootDrop
    {
        public string ItemId { get; set; }
        public int Count { get; set; }
        public ItemRarity Rarity { get; set; }
    }

    public partial class LootSystem : SingletonBase<LootSystem>
    {
        private readonly Dictionary<string, LootTable> _lootTables = new();
        private RandomGenerator _rng;

        [Export]
        public float GlobalDropChance { get; set; } = 0.3f;

        [Export]
        public float RarityMultiplier { get; set; } = 1.0f;

        [Signal]
        public delegate void LootDroppedEventHandler(Vector2 position, string[] itemIds);

        protected override void OnInitialize()
        {
            LoadLootTables();
        }

        private void LoadLootTables()
        {
            RegisterLootTable(new LootTable
            {
                Id = "common_enemy",
                Name = "Common Enemy Drops",
                MinItems = 0,
                MaxItems = 2,
                Entries = new List<LootEntry>
                {
                    new LootEntry
                    {
                        ItemId = "health_potion_small",
                        Weight = 10f,
                        Chance = 0.3f
                    },
                    new LootEntry
                    {
                        ItemId = "gold_coin",
                        Weight = 20f,
                        MinCount = 1,
                        MaxCount = 5
                    }
                }
            });

            RegisterLootTable(new LootTable
            {
                Id = "elite_enemy",
                Name = "Elite Enemy Drops",
                MinItems = 1,
                MaxItems = 3,
                Entries = new List<LootEntry>
                {
                    new LootEntry
                    {
                        ItemId = "health_potion_large",
                        Weight = 15f,
                        Chance = 0.5f
                    },
                    new LootEntry
                    {
                        ItemId = "iron_sword",
                        Weight = 5f,
                        Chance = 0.2f
                    },
                    new LootEntry
                    {
                        ItemId = "gold_coin",
                        Weight = 30f,
                        MinCount = 5,
                        MaxCount = 15
                    }
                }
            });

            RegisterLootTable(new LootTable
            {
                Id = "boss",
                Name = "Boss Drops",
                MinItems = 3,
                MaxItems = 5,
                Entries = new List<LootEntry>
                {
                    new LootEntry
                    {
                        ItemId = "health_potion_large",
                        Weight = 20f,
                        MinCount = 2,
                        MaxCount = 3
                    },
                    new LootEntry
                    {
                        ItemId = "steel_armor",
                        Weight = 10f,
                        Chance = 1.0f
                    },
                    new LootEntry
                    {
                        ItemId = "legendary_weapon",
                        Weight = 5f,
                        Chance = 0.5f
                    },
                    new LootEntry
                    {
                        ItemId = "gold_coin",
                        Weight = 50f,
                        MinCount = 20,
                        MaxCount = 50
                    }
                }
            });

            GD.Print($"[LootSystem] Loaded {_lootTables.Count} loot tables");
        }

        public void Initialize(uint seed)
        {
            _rng = new RandomGenerator(seed);
        }

        public void RegisterLootTable(LootTable table)
        {
            _lootTables[table.Id] = table;
        }

        public LootTable GetLootTable(string tableId)
        {
            return _lootTables.TryGetValue(tableId, out var table) ? table : null;
        }

        public LootDrop[] GenerateLoot(string tableId, float luckModifier = 0f)
        {
            if (_rng == null)
            {
                _rng = new RandomGenerator((uint)GD.Randi());
            }

            var table = GetLootTable(tableId);
            if (table == null)
            {
                GD.PrintErr($"[LootSystem] Loot table not found: {tableId}");
                return Array.Empty<LootDrop>();
            }

            var drops = new List<LootDrop>();
            int itemCount = _rng.Next(table.MinItems, table.MaxItems + 1);

            var weightedEntries = new List<(LootEntry entry, float cumulativeWeight)>();
            float totalWeight = 0f;

            foreach (var entry in table.Entries)
            {
                totalWeight += entry.Weight;
                weightedEntries.Add((entry, totalWeight));
            }

            for (int i = 0; i < itemCount; i++)
            {
                var selectedEntry = SelectWeightedEntry(weightedEntries, totalWeight);
                if (selectedEntry == null)
                    continue;

                if (_rng.NextFloat() > selectedEntry.Chance + luckModifier)
                    continue;

                int count = _rng.Next(selectedEntry.MinCount, selectedEntry.MaxCount + 1);

                var itemData = ItemManager.Instance?.GetItemData(selectedEntry.ItemId);
                if (itemData != null)
                {
                    drops.Add(new LootDrop
                    {
                        ItemId = selectedEntry.ItemId,
                        Count = count,
                        Rarity = itemData.Rarity
                    });
                }
            }

            GD.Print($"[LootSystem] Generated {drops.Count} items from {tableId}");
            return drops.ToArray();
        }

        private LootEntry SelectWeightedEntry(
            List<(LootEntry entry, float cumulativeWeight)> entries,
            float totalWeight)
        {
            if (entries.Count == 0)
                return null;

            float roll = _rng.NextFloat() * totalWeight;

            foreach (var (entry, cumulativeWeight) in entries)
            {
                if (roll <= cumulativeWeight)
                    return entry;
            }

            return entries[entries.Count - 1].entry;
        }

        public void DropLootAtPosition(string tableId, Vector2 position, float luckModifier = 0f)
        {
            var drops = GenerateLoot(tableId, luckModifier);

            foreach (var drop in drops)
            {
                for (int i = 0; i < drop.Count; i++)
                {
                    var offset = new Vector2(
                        _rng.NextFloat(-30f, 30f),
                        _rng.NextFloat(-30f, 30f)
                    );

                    ItemManager.Instance?.SpawnItem(drop.ItemId, position + offset);
                }
            }

            var dropIds = drops.Select(d => d.ItemId).ToArray();
            EmitSignal(SignalName.LootDropped, position, dropIds);
            EventBus.Instance.Publish("LootDropped", string.Join(",", dropIds));

            GD.Print($"[LootSystem] Dropped {drops.Length} items at {position}");
        }

        public void DropLootFromEnemy(string tableId, Node enemy, float luckModifier = 0f)
        {
            if (enemy == null)
                return;

            var position = (Vector2)enemy.Get("Position");
            DropLootAtPosition(tableId, position, luckModifier);
        }

        public LootDrop[] GenerateRandomLoot(int minItems, int maxItems, ItemRarity minRarity, ItemRarity maxRarity)
        {
            if (_rng == null)
            {
                _rng = new RandomGenerator((uint)GD.Randi());
            }

            var drops = new List<LootDrop>();
            int itemCount = _rng.Next(minItems, maxItems + 1);

            var allItems = new List<ItemData>();
            // Get all items from ItemManager that match rarity criteria

            for (int i = 0; i < itemCount; i++)
            {
                // Select random item matching criteria
                // For now, just add a placeholder
            }

            return drops.ToArray();
        }
    }
}
