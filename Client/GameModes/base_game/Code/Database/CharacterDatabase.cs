using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;

namespace RoguelikeGame.Database
{
    public enum CharacterClass
    {
        Ironclad,
        Silent,
        Defect,
        Watcher,
        Necromancer,
        Heir
    }

    public enum PlayStyle
    {
        Aggressive,
        Defensive,
        Hybrid,
        Combo,
        Control
    }

    public class CharacterData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public CharacterClass Class { get; set; }
        public PlayStyle Style { get; set; }
        
        public int MaxHealth { get; set; } = 80;
        public int StartingGold { get; set; } = 99;
        
        public string PortraitPath { get; set; }
        public string BackgroundColor { get; set; } = "#FF0000";
        
        public List<string> StartingCards { get; set; } = new();
        public List<string> UniqueMechanics { get; set; } = new();
        
        public Dictionary<string, object> Stats { get; set; } = new();
        public Dictionary<string, object> CustomData { get; set; } = new();
        
        public float DifficultyRating { get; set; } = 3f;
        public string DifficultyDescription { get; set; }
    }

    public partial class CharacterDatabase : SingletonBase<CharacterDatabase>
    {
        private readonly Dictionary<string, CharacterData> _characters = new();
        private readonly List<CharacterData> _playableCharacters = new();

        [Signal]
        public delegate void CharacterSelectedEventHandler(string characterId);

        protected override void OnInitialize()
        {
            LoadCharactersFromConfig();
        }

        private void LoadCharactersFromConfig()
        {
            var config = ConfigLoader.LoadConfig<CharacterConfigData>("characters");
            
            if (config == null)
            {
                GD.PrintErr("[CharacterDatabase] Failed to load characters config!");
                return;
            }

            foreach (var charConfig in config.Characters)
            {
                var charData = ConvertConfigToData(charConfig);
                RegisterCharacter(charData);
            }

            GD.Print($"[CharacterDatabase] Loaded {_characters.Count} characters from config (version: {config.Version})");
        }

        private CharacterData ConvertConfigToData(CharacterConfig config)
        {
            return new CharacterData
            {
                Id = config.Id,
                Name = config.Name,
                Title = config.Title,
                Description = config.Description,
                Class = ParseCharacterClass(config.Class),
                Style = ParsePlayStyle(config.Style),
                MaxHealth = config.MaxHealth,
                StartingGold = config.StartingGold,
                PortraitPath = config.PortraitPath,
                BackgroundColor = config.BackgroundColor,
                StartingCards = new List<string>(config.StartingCards),
                UniqueMechanics = new List<string>(config.UniqueMechanics),
                Stats = new Dictionary<string, object>(config.Stats),
                CustomData = new Dictionary<string, object>(config.CustomData),
                DifficultyRating = config.DifficultyRating,
                DifficultyDescription = config.DifficultyDescription
            };
        }

        private CharacterClass ParseCharacterClass(string charClass)
        {
            return charClass?.ToLower() switch
            {
                "ironclad" => CharacterClass.Ironclad,
                "silent" => CharacterClass.Silent,
                "defect" => CharacterClass.Defect,
                "watcher" => CharacterClass.Watcher,
                "necromancer" => CharacterClass.Necromancer,
                "heir" => CharacterClass.Heir,
                _ => CharacterClass.Ironclad
            };
        }

        private PlayStyle ParsePlayStyle(string style)
        {
            return style?.ToLower() switch
            {
                "aggressive" => PlayStyle.Aggressive,
                "defensive" => PlayStyle.Defensive,
                "hybrid" => PlayStyle.Hybrid,
                "combo" => PlayStyle.Combo,
                "control" => PlayStyle.Control,
                _ => PlayStyle.Hybrid
            };
        }

        public void RegisterCharacter(CharacterData character)
        {
            _characters[character.Id] = character;
            _playableCharacters.Add(character);
            
            GD.Print($"[CharacterDatabase] Registered character: {character.Name}");
        }

        public CharacterData GetCharacter(string characterId)
        {
            return _characters.TryGetValue(characterId, out var character) ? character : null;
        }

        public List<CharacterData> GetAllCharacters()
        {
            return new List<CharacterData>(_playableCharacters);
        }

        public List<CharacterData> GetCharactersByStyle(PlayStyle style)
        {
            var result = new List<CharacterData>();
            foreach (var character in _playableCharacters)
            {
                if (character.Style == style)
                    result.Add(character);
            }
            return result;
        }

        public List<CharacterData> GetBeginnerFriendlyCharacters(float maxDifficulty = 3f)
        {
            var result = new List<CharacterData>();
            foreach (var character in _playableCharacters)
            {
                if (character.DifficultyRating <= maxDifficulty)
                    result.Add(character);
            }
            return result;
        }

        public int TotalCharacters => _characters.Count;
    }
}
