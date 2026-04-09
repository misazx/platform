using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RoguelikeGame.Core
{
    public class CardConfigData
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("cards")]
        public List<CardConfig> Cards { get; set; } = new();
    }

    public class CardConfig
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("cost")]
        public int Cost { get; set; } = 1;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "Attack";

        [JsonPropertyName("rarity")]
        public string Rarity { get; set; } = "Common";

        [JsonPropertyName("target")]
        public string Target { get; set; } = "SingleEnemy";

        [JsonPropertyName("damage")]
        public int Damage { get; set; }

        [JsonPropertyName("block")]
        public int Block { get; set; }

        [JsonPropertyName("magicNumber")]
        public int MagicNumber { get; set; }

        [JsonPropertyName("upgraded")]
        public bool Upgraded { get; set; }

        [JsonPropertyName("keywords")]
        public List<string> Keywords { get; set; } = new();

        [JsonPropertyName("customData")]
        public Dictionary<string, object> CustomData { get; set; } = new();

        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; }

        [JsonPropertyName("iconPath")]
        public string IconPath { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; } = "#FFFFFF";

        [JsonPropertyName("isExhaust")]
        public bool IsExhaust { get; set; }

        [JsonPropertyName("isEthereal")]
        public bool IsEthereal { get; set; }

        [JsonPropertyName("isInnate")]
        public bool IsInnate { get; set; }
    }

    public class CharacterConfigData
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("characters")]
        public List<CharacterConfig> Characters { get; set; } = new();
    }

    public class CharacterConfig
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("class")]
        public string Class { get; set; }

        [JsonPropertyName("style")]
        public string Style { get; set; }

        [JsonPropertyName("maxHealth")]
        public int MaxHealth { get; set; } = 80;

        [JsonPropertyName("startingGold")]
        public int StartingGold { get; set; } = 99;

        [JsonPropertyName("portraitPath")]
        public string PortraitPath { get; set; }

        [JsonPropertyName("backgroundColor")]
        public string BackgroundColor { get; set; } = "#FF0000";

        [JsonPropertyName("startingCards")]
        public List<string> StartingCards { get; set; } = new();

        [JsonPropertyName("uniqueMechanics")]
        public List<string> UniqueMechanics { get; set; } = new();

        [JsonPropertyName("stats")]
        public Dictionary<string, object> Stats { get; set; } = new();

        [JsonPropertyName("customData")]
        public Dictionary<string, object> CustomData { get; set; } = new();

        [JsonPropertyName("difficultyRating")]
        public float DifficultyRating { get; set; } = 3f;

        [JsonPropertyName("difficultyDescription")]
        public string DifficultyDescription { get; set; }
    }

    public class EnemyConfigData
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("enemies")]
        public List<EnemyConfig> Enemies { get; set; } = new();
    }

    public class EnemyConfig
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("maxHealth")]
        public int MaxHealth { get; set; } = 50;

        [JsonPropertyName("attackDamage")]
        public int AttackDamage { get; set; } = 10;

        [JsonPropertyName("blockAmount")]
        public int BlockAmount { get; set; } = 5;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "Normal";

        [JsonPropertyName("behavior")]
        public string Behavior { get; set; } = "Aggressive";

        [JsonPropertyName("portraitPath")]
        public string PortraitPath { get; set; }

        [JsonPropertyName("iconPath")]
        public string IconPath { get; set; }

        [JsonPropertyName("abilities")]
        public List<string> Abilities { get; set; } = new();

        [JsonPropertyName("drops")]
        public List<string> Drops { get; set; } = new();

        [JsonPropertyName("stats")]
        public Dictionary<string, object> Stats { get; set; } = new();

        [JsonPropertyName("customData")]
        public Dictionary<string, object> CustomData { get; set; } = new();

        [JsonPropertyName("difficultyRating")]
        public float DifficultyRating { get; set; }

        [JsonPropertyName("encounterLocation")]
        public string EncounterLocation { get; set; }
    }

    public class RelicConfigData
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("relics")]
        public List<RelicConfig> Relics { get; set; } = new();
    }

    public class RelicConfig
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("flavorText")]
        public string FlavorText { get; set; }

        [JsonPropertyName("tier")]
        public string Tier { get; set; } = "Common";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "Passive";

        [JsonPropertyName("imagePath")]
        public string ImagePath { get; set; }

        [JsonPropertyName("iconPath")]
        public string IconPath { get; set; }

        [JsonPropertyName("compatibleCharacters")]
        public List<string> CompatibleCharacters { get; set; } = new();

        [JsonPropertyName("effects")]
        public Dictionary<string, object> Effects { get; set; } = new();

        [JsonPropertyName("stats")]
        public Dictionary<string, object> Stats { get; set; } = new();

        [JsonPropertyName("isCounterpart")]
        public bool IsCounterpart { get; set; }

        [JsonPropertyName("counterpartId")]
        public string CounterpartId { get; set; }
    }

    public class PotionConfigData
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("potions")]
        public List<PotionConfig> Potions { get; set; } = new();
    }

    public class PotionConfig
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "Utility";

        [JsonPropertyName("price")]
        public int Price { get; set; } = 50;

        [JsonPropertyName("rarity")]
        public int Rarity { get; set; } = 1;

        [JsonPropertyName("imagePath")]
        public string ImagePath { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; } = "#FF00FF";

        [JsonPropertyName("effects")]
        public Dictionary<string, object> Effects { get; set; } = new();

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("isStackable")]
        public bool IsStackable { get; set; }

        [JsonPropertyName("maxStack")]
        public int MaxStack { get; set; } = 3;
    }

    public class EventConfigData
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("events")]
        public List<EventConfig> Events { get; set; } = new();
    }

    public class EventConfig
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("flavorText")]
        public string FlavorText { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "Choice";

        [JsonPropertyName("imagePath")]
        public string ImagePath { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("choices")]
        public List<EventChoiceConfig> Choices { get; set; } = new();

        [JsonPropertyName("customData")]
        public Dictionary<string, object> CustomData { get; set; } = new();

        [JsonPropertyName("weight")]
        public float Weight { get; set; } = 1.0f;

        [JsonPropertyName("oneTime")]
        public bool OneTime { get; set; }

        [JsonPropertyName("hasSeen")]
        public bool HasSeen { get; set; }
    }

    public class EventChoiceConfig
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("rewards")]
        public Dictionary<string, object> Rewards { get; set; } = new();

        [JsonPropertyName("penalties")]
        public Dictionary<string, object> Penalties { get; set; } = new();

        [JsonPropertyName("requiresCondition")]
        public bool RequiresCondition { get; set; }

        [JsonPropertyName("conditionKey")]
        public string ConditionKey { get; set; }

        [JsonPropertyName("conditionValue")]
        public object ConditionValue { get; set; }
    }

    public class AudioConfigData
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("audioSettings")]
        public AudioSettingsConfig AudioSettings { get; set; }

        [JsonPropertyName("soundEffects")]
        public List<SoundEffectConfig> SoundEffects { get; set; } = new();
    }

    public class AudioSettingsConfig
    {
        [JsonPropertyName("sfxPlayerCount")]
        public int SfxPlayerCount { get; set; } = 8;

        [JsonPropertyName("bgmVolume")]
        public float BgmVolume { get; set; } = -12.0f;

        [JsonPropertyName("sfxVolume")]
        public float SfxVolume { get; set; } = 0.0f;

        [JsonPropertyName("sfxPath")]
        public string SfxPath { get; set; } = "res://GameModes/base_game/Resources/Audio/SFX/";

        [JsonPropertyName("bgmPath")]
        public string BgmPath { get; set; } = "res://GameModes/base_game/Resources/Audio/BGM/";
    }

    public class SoundEffectConfig
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("defaultPitchScale")]
        public float DefaultPitchScale { get; set; } = 1.0f;

        [JsonPropertyName("volumeDb")]
        public float VolumeDb { get; set; } = 0.0f;
    }

    public class EffectConfigData
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("effects")]
        public List<EffectConfig> Effects { get; set; } = new();
    }

    public class EffectConfig
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("duration")]
        public float Duration { get; set; }

        [JsonPropertyName("config")]
        public Dictionary<string, object> Config { get; set; } = new();
    }
}
