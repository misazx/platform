using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;

namespace RoguelikeGame.Systems
{
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Passive,
        Active
    }

    public class ItemData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemType Type { get; set; }
        public ItemRarity Rarity { get; set; }
        public int MaxStack { get; set; } = 1;
        public Dictionary<string, float> Stats { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public string IconPath { get; set; }
        public string ScenePath { get; set; }
        public Dictionary<string, object> CustomData { get; set; } = new();
    }

    public class ItemEffect
    {
        public string EffectId { get; set; }
        public string Description { get; set; }
        public virtual void ApplyEffect(Node target)
        {
        GD.Print($"[ItemEffect] Applying {EffectId} to {target.Name}");
        }

        public virtual void RemoveEffect(Node target)
        {
        GD.Print($"[ItemEffect] Removing {EffectId} from {target.Name}");
        }
    }

    public class StatModifierEffect : ItemEffect
    {
        public string StatName { get; set; }
        public float Value { get; set; }
        public bool IsPercentage { get; set; }

        public override void ApplyEffect(Node target)
        {
                if (target.HasMethod("ModifyStat"))
                {
                    target.Call("ModifyStat", StatName, Value, IsPercentage);
                GD.Print($"[StatModifier] {StatName} +{Value}{(IsPercentage ? "%" : "")}");
                }
        }

        public override void RemoveEffect(Node target)
        {
                if (target.HasMethod("ModifyStat"))
                {
                    float removeValue = IsPercentage ? -Value : -Value;
                    target.Call("ModifyStat", StatName, removeValue, IsPercentage);
                }
        }
    }

    public class HealEffect : ItemEffect
    {
        public int HealAmount { get; set; }

        public override void ApplyEffect(Node target)
        {
                if (target.HasMethod("Heal"))
                {
                    target.Call("Heal", HealAmount);
                    GD.Print($"[HealEffect] Healed {HealAmount} HP");
                }
        }
    }

    public partial class ItemManager : SingletonBase<ItemManager>
    {
        private readonly Dictionary<string, ItemData> _itemDefinitions = new();
        private readonly Dictionary<string, PackedScene> _itemScenes = new();
        private readonly List<Node> _droppedItems = new();

        [Export]
        public int MaxDroppedItems { get; set; } = 50;

        [Signal]
        public delegate void ItemPickedUpEventHandler(Node item, Node picker);

        [Signal]
        public delegate void ItemDroppedEventHandler(Node item, Vector2 position);

        protected override void OnInitialize()
        {
            LoadItemDefinitions();
        }

        private void LoadItemDefinitions()
        {
            RegisterItem(new ItemData
            {
                Id = "health_potion_small",
                Name = "Small Health Potion",
                Description = "Restores 25 HP",
                Type = ItemType.Consumable,
                Rarity = ItemRarity.Common,
                MaxStack = 5,
                Stats = new Dictionary<string, float>
                {
                    { "heal_amount", 25f }
                },
                IconPath = "res://GameModes/base_game/Resources/Icons/Items/health_potion_small.png",
                ScenePath = "res://GameModes/base_game/Scenes/Items/HealthPotion.tscn"
            });

            RegisterItem(new ItemData
            {
                Id = "health_potion_large",
                Name = "Large Health Potion",
                Description = "Restores 50 HP",
                Type = ItemType.Consumable,
                Rarity = ItemRarity.Uncommon,
                MaxStack = 3,
                Stats = new Dictionary<string, float>
                {
                    { "heal_amount", 50f }
                },
                IconPath = "res://GameModes/base_game/Resources/Icons/Items/health_potion_large.png"
            });

            RegisterItem(new ItemData
            {
                Id = "iron_sword",
                Name = "Iron Sword",
                Description = "+10 Attack",
                Type = ItemType.Weapon,
                Rarity = ItemRarity.Common,
                Stats = new Dictionary<string, float>
                {
                    { "attack", 10f },
                    { "speed", -5f }
                },
                IconPath = "res://GameModes/base_game/Resources/Icons/Items/iron_sword.png"
            });

            RegisterItem(new ItemData
            {
                Id = "steel_armor",
                Name = "Steel Armor",
                Description = "+15 Defense",
                Type = ItemType.Armor,
                Rarity = ItemRarity.Rare,
                Stats = new Dictionary<string, float>
                {
                    { "defense", 15f },
                    { "speed", -10f }
                },
                IconPath = "res://GameModes/base_game/Resources/Icons/Items/steel_armor.png"
            });

            GD.Print($"[ItemManager] Loaded {_itemDefinitions.Count} item definitions");
        }

        public void RegisterItem(ItemData itemData)
        {
            _itemDefinitions[itemData.Id] = itemData;
        }

        public ItemData GetItemData(string itemId)
        {
            return _itemDefinitions.TryGetValue(itemId, out var data) ? data : null;
        }

        public Node SpawnItem(string itemId, Vector2 position, Node parent = null)
        {
            var itemData = GetItemData(itemId);
            if (itemData == null)
            {
                GD.PrintErr($"[ItemManager] Item not found: {itemId}");
                return null;
            }

            if (!_itemScenes.TryGetValue(itemId, out var scene))
            {
                if (string.IsNullOrEmpty(itemData.ScenePath))
                {
                    scene = CreateDefaultItemScene(itemData);
                }
                else if (ResourceLoader.Exists(itemData.ScenePath))
                {
                    scene = ResourceLoader.Load<PackedScene>(itemData.ScenePath);
                }
                else
                {
                    scene = CreateDefaultItemScene(itemData);
                }
                _itemScenes[itemId] = scene;
            }

            var item = scene.Instantiate();
            item.Set("Position", position);
            item.Set("ItemDataId", itemId);

            parent ??= GetTree().CurrentScene;
            parent.AddChild(item);

            _droppedItems.Add(item);

            EmitSignal(SignalName.ItemDropped, itemId, position);
            EventBus.Instance.Publish(GameEvents.ItemDropped, itemId);

            GD.Print($"[ItemManager] Spawned {itemId} at {position}");
            return item;
        }

        private PackedScene CreateDefaultItemScene(ItemData itemData)
        {
            var scene = new PackedScene();
            var area = new Area2D();
            var sprite = new Sprite2D();
            var collision = new CollisionShape2D();
            
            var circleShape = new CircleShape2D();
            circleShape.Radius = 16f;
            collision.Shape = circleShape;

            area.AddChild(sprite);
            area.AddChild(collision);

            scene.Pack(area);
            return scene;
        }

        public void PickupItem(Node item, Node picker)
        {
            if (!_droppedItems.Contains(item))
                return;

            _droppedItems.Remove(item);

            var itemDataStr = item.Get("ItemDataId").AsString();
            var itemData = GetItemData(itemDataStr);
            if (picker.HasMethod("AddItem"))
            {
                picker.Call("AddItem", itemDataStr);
            }

            EmitSignal(SignalName.ItemPickedUp, itemDataStr, picker.Name);
            EventBus.Instance.Publish(GameEvents.ItemPickedUp, itemDataStr);

            item.QueueFree();

            GD.Print($"[ItemManager] {picker.Name} picked up {itemData.Name}");
        }

        public List<Node> GetDroppedItemsInRange(Vector2 center, float radius)
        {
            var result = new List<Node>();
            var radiusSquared = radius * radius;

            foreach (var item in _droppedItems)
            {
                var pos = (Vector2)item.Get("Position");
                if (pos.DistanceSquaredTo(center) <= radiusSquared)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public void ClearDroppedItems()
        {
            foreach (var item in _droppedItems.ToArray())
            {
                item.QueueFree();
            }
            _droppedItems.Clear();
            GD.Print("[ItemManager] Cleared all dropped items");
        }
    }
}
