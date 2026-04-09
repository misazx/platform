using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;

namespace RoguelikeGame.Database
{
    public enum CardType
    {
        Attack,
        Skill,
        Power,
        Status,
        Curse
    }

    public enum CardRarity
    {
        Basic,
        Common,
        Uncommon,
        Rare,
        Special
    }

    public enum CardTarget
    {
        Self,
        SingleEnemy,
        AllEnemies,
        RandomEnemy,
        None
    }

    public class CardData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Cost { get; set; } = 1;
        public CardType Type { get; set; } = CardType.Attack;
        public CardRarity Rarity { get; set; } = CardRarity.Common;
        public CardTarget Target { get; set; } = CardTarget.SingleEnemy;
        
        public int Damage { get; set; }
        public int Block { get; set; }
        public int MagicNumber { get; set; }
        public bool Upgraded { get; set; }
        
        public List<string> Keywords { get; set; } = new();
        public Dictionary<string, object> CustomData { get; set; } = new();
        
        public string CharacterId { get; set; }
        public string IconPath { get; set; }
        public string Color { get; set; } = "#FFFFFF";
        
        public bool IsExhaust { get; set; }
        public bool IsEthereal { get; set; }
        public bool IsInnate { get; set; }
    }

    public partial class CardDatabase : Node
    {
        public static CardDatabase Instance { get; private set; }

        private readonly Dictionary<string, CardData> _cards = new();
        private readonly Dictionary<string, List<CardData>> _characterCards = new();
        private readonly Dictionary<CardType, List<CardData>> _typeCards = new();

        [Signal]
        public delegate void CardRegisteredEventHandler(string cardId);

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;

            LoadCardsFromConfig();
        }

        private void LoadCardsFromConfig()
        {
            var config = ConfigLoader.LoadConfig<CardConfigData>("cards");
            
            if (config == null)
            {
                GD.PrintErr("[CardDatabase] Failed to load cards config!");
                return;
            }

            foreach (var cardConfig in config.Cards)
            {
                var cardData = ConvertConfigToData(cardConfig);
                RegisterCard(cardData);
            }

            GD.Print($"[CardDatabase] Loaded {_cards.Count} cards from config (version: {config.Version})");
        }

        private CardData ConvertConfigToData(CardConfig config)
        {
            return new CardData
            {
                Id = config.Id,
                Name = config.Name,
                Description = config.Description,
                Cost = config.Cost,
                Type = ParseCardType(config.Type),
                Rarity = ParseCardRarity(config.Rarity),
                Target = ParseCardTarget(config.Target),
                Damage = config.Damage,
                Block = config.Block,
                MagicNumber = config.MagicNumber,
                Upgraded = config.Upgraded,
                Keywords = new List<string>(config.Keywords),
                CustomData = new Dictionary<string, object>(config.CustomData),
                CharacterId = config.CharacterId,
                IconPath = config.IconPath,
                Color = config.Color,
                IsExhaust = config.IsExhaust,
                IsEthereal = config.IsEthereal,
                IsInnate = config.IsInnate
            };
        }

        private CardType ParseCardType(string type)
        {
            return type?.ToLower() switch
            {
                "attack" => CardType.Attack,
                "skill" => CardType.Skill,
                "power" => CardType.Power,
                "status" => CardType.Status,
                "curse" => CardType.Curse,
                _ => CardType.Attack
            };
        }

        private CardRarity ParseCardRarity(string rarity)
        {
            return rarity?.ToLower() switch
            {
                "basic" => CardRarity.Basic,
                "common" => CardRarity.Common,
                "uncommon" => CardRarity.Uncommon,
                "rare" => CardRarity.Rare,
                "special" => CardRarity.Special,
                _ => CardRarity.Common
            };
        }

        private CardTarget ParseCardTarget(string target)
        {
            return target?.ToLower() switch
            {
                "self" => CardTarget.Self,
                "singleenemy" => CardTarget.SingleEnemy,
                "allenemies" => CardTarget.AllEnemies,
                "randomenemy" => CardTarget.RandomEnemy,
                "none" => CardTarget.None,
                _ => CardTarget.SingleEnemy
            };
        }

        public void RegisterCard(CardData card)
        {
            _cards[card.Id] = card;

            if (!_characterCards.ContainsKey(card.CharacterId))
                _characterCards[card.CharacterId] = new List<CardData>();
            
            _characterCards[card.CharacterId].Add(card);

            if (!_typeCards.ContainsKey(card.Type))
                _typeCards[card.Type] = new List<CardData>();
            
            _typeCards[card.Type].Add(card);

            EmitSignal(SignalName.CardRegistered, card.Id);
        }

        public CardData GetCard(string cardId)
        {
            return _cards.TryGetValue(cardId, out var card) ? card : null;
        }

        public List<CardData> GetAllCards()
        {
            return new List<CardData>(_cards.Values);
        }

        public List<CardData> GetCharacterCards(string characterId)
        {
            return _characterCards.TryGetValue(characterId, out var cards) 
                ? cards 
                : new List<CardData>();
        }

        public List<CardData> GetCardsByType(CardType type)
        {
            return _typeCards.TryGetValue(type, out var cards) 
                ? cards 
                : new List<CardData>();
        }

        public List<CardData> GetCardsByRarity(CardRarity rarity)
        {
            var result = new List<CardData>();
            foreach (var card in _cards.Values)
            {
                if (card.Rarity == rarity)
                    result.Add(card);
            }
            return result;
        }

        public List<CardData> SearchCards(string query)
        {
            var result = new List<CardData>();
            foreach (var card in _cards.Values)
            {
                if (card.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    card.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(card);
                }
            }
            return result;
        }

        public int TotalCards => _cards.Count;
    }
}
