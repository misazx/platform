using Godot;
using RoguelikeGame.Core;
using System;
using System.Linq;using System.Collections.Generic;
using System.Linq;
using RoguelikeGame.Database;

namespace RoguelikeGame.Systems
{
    public enum ShopItemType
    {
        Card,
        Relic,
        Potion,
        CardRemoval,
        CardUpgrade
    }

    public class ShopItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ShopItemType Type { get; set; }
        public int Price { get; set; }
        
        public object Data { get; set; } // Can be CardData, RelicData, or PotionData
        
        public bool IsPurchased { get; set; }
        public bool IsAvailable { get; set; } = true;
        
        public string Description { get; set; }
        public string IconPath { get; set; }
    }

    public partial class ShopManager : Node
    {
        public static ShopManager Instance { get; private set; }

        private readonly List<ShopItem> _currentItems = new();
        private RandomNumberGenerator _rng;
        
        [Export]
        public int BaseCardPrice { get; set; } = 50;
        
        [Export]
        public int BaseRelicPrice { get; set; } = 150;
        
        [Export]
        public int BasePotionPrice { get; set; } = 80;
        
        [Export]
        public int CardRemovalPrice { get; set; } = 100;
        
        [Export]
        public int CardUpgradePrice { get; set; } = 100;

        [Signal]
        public delegate void ItemPurchasedEventHandler(string itemId);
        
        [Signal]
        public delegate void ShopEnteredEventHandler();

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;
        }

        public void Initialize(uint seed)
        {
            _rng = new RandomNumberGenerator();
            _rng.Seed = (ulong)seed;
        }

        public List<ShopItem> GenerateShopInventory(string characterId, int floor)
        {
            _currentItems.Clear();
            
            // Generate 3 card offers
            for (int i = 0; i < 3; i++)
            {
                var card = GenerateCardOffer(characterId);
                if (card != null)
                {
                    var item = CreateCardItem(card);
                    _currentItems.Add(item);
                }
            }
            
            // Generate 1-2 relic offers
            int relicCount = (int)_rng.RandiRange(1, 2);
            for (int i = 0; i < relicCount; i++)
            {
                var relic = GenerateRelicOffer(characterId);
                if (relic != null)
                {
                    var item = CreateRelicItem(relic);
                    _currentItems.Add(item);
                }
            }
            
            // Generate 1-2 potion offers
            int potionCount = (int)_rng.RandiRange(1, 2);
            for (int i = 0; i < potionCount; i++)
            {
                var potion = GeneratePotionOffer();
                if (potion != null)
                {
                    var item = CreatePotionItem(potion);
                    _currentItems.Add(item);
                }
            }
            
            // Always offer card removal and upgrade
            _currentItems.Add(CreateServiceItem("card_removal", "移除一张牌", CardRemovalPrice));
            _currentItems.Add(CreateServiceItem("card_upgrade", "升级一张牌", CardUpgradePrice));
            
            EmitSignal(SignalName.ShopEntered);
            
            GD.Print($"[ShopManager] Generated shop with {_currentItems.Count} items");
            
            return _currentItems;
        }

        private CardData GenerateCardOffer(string characterId)
        {
            var allCards = CardDatabase.Instance.GetCharacterCards(characterId);
            
            if (allCards.Count == 0)
                allCards = CardDatabase.Instance.GetAllCards();
            
            if (allCards.Count == 0)
                return null;
            
            var availableCards = allCards.Where(c => 
                c.Rarity != CardRarity.Basic && 
                c.Rarity != CardRarity.Special
            ).ToList();
            
            if (availableCards.Count == 0)
                return allCards[(int)(_rng.Randf() * allCards.Count)];

            return availableCards[(int)(_rng.Randf() * availableCards.Count)];
        }

        private RelicData GenerateRelicOffer(string characterId)
        {
            var relics = RelicDatabase.Instance.GetRelicsForCharacter(characterId);
            
            // Filter out starter relics and boss relics
            var availableRelics = relics.Where(r => 
                r.Tier != Database.RelicTier.Starter && 
                r.Tier != Database.RelicTier.Boss &&
                r.Tier != Database.RelicTier.Special
            ).ToList();
            
            if (availableRelics.Count == 0)
                return null;
            
            return _rng != null ? availableRelics[(int)(_rng.Randf() * availableRelics.Count)] : null;
        }

        private PotionData GeneratePotionOffer()
        {
            var potions = PotionDatabase.Instance.GetAllPotions();
            if (potions.Count == 0)
                return null;
            
            return potions[(int)(_rng.Randf() * potions.Count)];
        }

        private ShopItem CreateCardItem(CardData card)
        {
            int price = CalculateCardPrice(card);
            
            return new ShopItem
            {
                Id = $"card_{card.Id}",
                Name = card.Name,
                Type = ShopItemType.Card,
                Price = price,
                Data = card,
                Description = FormatCardDescription(card),
                IconPath = card.IconPath ?? "res://Icons/Cards/default.png"
            };
        }

        private ShopItem CreateRelicItem(RelicData relic)
        {
            int price = CalculateRelicPrice(relic);
            
            return new ShopItem
            {
                Id = $"relic_{relic.Id}",
                Name = relic.Name,
                Type = ShopItemType.Relic,
                Price = price,
                Data = relic,
                Description = relic.Description,
                IconPath = relic.ImagePath ?? "res://Icons/Relics/default.png"
            };
        }

        private ShopItem CreatePotionItem(PotionData potion)
        {
            int price = CalculatePotionPrice(potion);
            
            return new ShopItem
            {
                Id = $"potion_{potion.Id}",
                Name = potion.Name,
                Type = ShopItemType.Potion,
                Price = price,
                Data = potion,
                Description = potion.Description,
                IconPath = potion.ImagePath ?? "res://Icons/Potions/default.png"
            };
        }

        private ShopItem CreateServiceItem(string serviceId, string name, int price)
        {
            return new ShopItem
            {
                Id = $"service_{serviceId}",
                Name = name,
                Type = serviceId.Contains("removal") ? ShopItemType.CardRemoval : ShopItemType.CardUpgrade,
                Price = price,
                Data = serviceId,
                Description = GetServiceDescription(serviceId),
                IconPath = "res://Icons/Services/default.png"
            };
        }

        private int CalculateCardPrice(CardData card)
        {
            int basePrice = BaseCardPrice;
            
            switch (card.Rarity)
            {
                case CardRarity.Common:
                    basePrice = 50;
                    break;
                case CardRarity.Uncommon:
                    basePrice = 75;
                    break;
                case CardRarity.Rare:
                    basePrice = 120;
                    break;
                case CardRarity.Special:
                    basePrice = 200;
                    break;
            }
            
            // Add cost modifier
            basePrice += card.Cost * 10;
            
            // Add some random variation
            basePrice += (int)_rng.RandiRange(-10, 10);
            
            return Math.Max(25, basePrice); // Minimum price
        }

        private int CalculateRelicPrice(RelicData relic)
        {
            switch (relic.Tier)
            {
                case Database.RelicTier.Common:
                    return BaseRelicPrice - 50;
                case Database.RelicTier.Uncommon:
                    return BaseRelicPrice;
                case Database.RelicTier.Rare:
                    return BaseRelicPrice + 50;
                default:
                    return BaseRelicPrice;
            }
        }

        private int CalculatePotionPrice(PotionData potion)
        {
            return BasePotionPrice + (potion.Rarity * 15) + (int)_rng.RandiRange(-10, 15);
        }

        private string FormatCardDescription(CardData card)
        {
            var desc = new System.Text.StringBuilder();
            desc.AppendLine(card.Description);
            desc.AppendLine();
            desc.Append($"费用: {card.Cost}");
            
            if (card.Damage > 0)
                desc.AppendLine($"\n伤害: {card.Damage}");
            
            if (card.Block > 0)
                desc.AppendLine($"\n格挡: {card.Block}");
            
            return desc.ToString();
        }

        private string GetServiceDescription(string serviceId)
        {
            return serviceId switch
            {
                "card_removal" => "从你的牌组中永久移除一张牌",
                "card_upgrade" => "升级你牌组中的一张牌",
                _ => "服务描述"
            };
        }

        public bool PurchaseItem(ShopItem item, Node player)
        {
            if (!item.IsAvailable || item.IsPurchased)
                return false;
            
            // Check if player has enough gold
            if (player.HasMethod("GetGold"))
            {
                int currentGold = (int)player.Call("GetGold");
                if (currentGold < item.Price)
                {
                    GD.Print("[ShopManager] Not enough gold!");
                    return false;
                }
                
                // Deduct gold
                player.Call("SpendGold", item.Price);
            }
            
            // Process purchase based on type
            ProcessPurchase(item, player);
            
            item.IsPurchased = true;
            item.IsAvailable = false;
            
            EmitSignal(SignalName.ItemPurchased, item.Id);
            
            TimelineManager.Instance?.AddEventTriggered(
                "shop_purchase",
                $"购买了: {item.Name}",
                GameManager.Instance?.CurrentRun?.CurrentFloor ?? 1,
                GameManager.Instance?.CurrentRun?.CurrentRoom ?? 0
            );
            
            GD.Print($"[ShopManager] Purchased: {item.Name} for {item.Price} gold");
            
            return true;
        }

        private void ProcessPurchase(ShopItem item, Node player)
        {
            switch (item.Type)
            {
                case ShopItemType.Card:
                    if (item.Data is CardData card && player.HasMethod("AddCardToDeck"))
                    {
                        player.Call("AddCardToDeck", card.Id);
                    }
                    break;
                    
                case ShopItemType.Relic:
                    if (item.Data is RelicData relic && player.HasMethod("AddRelic"))
                    {
                        player.Call("AddRelic", relic.Id);
                    }
                    break;
                    
                case ShopItemType.Potion:
                    if (item.Data is PotionData potion && player.HasMethod("AddPotion"))
                    {
                        player.Call("AddPotion", potion.Id);
                    }
                    break;
                    
                case ShopItemType.CardRemoval:
                    // This would open a UI to select a card to remove
                    GD.Print("[ShopManager] Card removal selected");
                    break;
                    
                case ShopItemType.CardUpgrade:
                    // This would open a UI to select a card to upgrade
                    GD.Print("[ShopManager] Card upgrade selected");
                    break;
            }
        }

        public List<ShopItem> GetCurrentInventory() => new List<ShopItem>(_currentItems);
        
        public List<ShopItem> GetAvailableItems() => _currentItems.Where(i => i.IsAvailable).ToList();
        
        public int TotalItems => _currentItems.Count;
    }
}
