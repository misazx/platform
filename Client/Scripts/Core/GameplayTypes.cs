using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeGame.Core
{
    public enum CardType { Attack = 0, Skill = 1, Power = 2, Status = 3, Curse = 4 }
    public enum CardRarity { Basic = 0, Common = 1, Uncommon = 2, Rare = 3, Special = 4 }
    public enum CardTarget { EnemySingle = 0, EnemyAll = 1, Self = 2, All = 3, None = 4 }
    public enum StsCardType { Attack = 0, Skill = 1, Power = 2, Status = 3, Curse = 4 }

    public partial class CardData : GodotObject
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int Cost { get; set; } = 1;
        public CardType Type { get; set; } = CardType.Attack;
        public CardRarity Rarity { get; set; } = CardRarity.Common;
        public CardTarget Target { get; set; } = CardTarget.EnemySingle;
        public int Damage { get; set; } = 0;
        public int Block { get; set; } = 0;
        public int MagicNumber { get; set; } = 0;
        public bool Ethereal { get; set; } = false;
        public bool Exhaust { get; set; } = false;
        public bool Innate { get; set; } = false;
        public bool Retain { get; set; } = false;
        public string IconPath { get; set; } = "";
    }

    public partial class StsCardData : GodotObject
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int Cost { get; set; } = 1;
        public StsCardType Type { get; set; } = StsCardType.Attack;
        public int Damage { get; set; } = 0;
        public int Block { get; set; } = 0;
        public int MagicNumber { get; set; } = 0;
        public bool Exhaust { get; set; } = false;
        public string IconPath { get; set; } = "";
    }

    public class MapNode
    {
        public int Id { get; set; }
        public int Layer { get; set; }
        public Generation.NodeType Type { get; set; }
        public Vector2 Position { get; set; }
        public List<int> ConnectedNodes { get; set; } = new();
        public bool IsVisited { get; set; } = false;
        public int Status { get; set; } = 0;
        public string EnemyEncounterId { get; set; } = "";
        public string EventId { get; set; } = "";
        public Dictionary<string, object> CustomData { get; set; } = new();
    }

    public class FloorMap
    {
        public List<MapNode> Nodes { get; set; } = new();
        public int CurrentNodeId { get; set; } = -1;
        public int StartNodeId { get; set; } = 0;
    }

    public class CharacterData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int MaxHealth { get; set; } = 80;
        public int StartingGold { get; set; } = 99;
        public List<string> StartingCards { get; set; } = new();
        public List<string> StartingRelics { get; set; } = new();
        public string PortraitPath { get; set; } = "";
    }

    public class RelicData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public Database.RelicTier Tier { get; set; } = Database.RelicTier.Common;
    }

    public class PotionData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class RunStatistics
    {
        public string CharacterId { get; set; } = "";
        public int FloorReached { get; set; }
        public int EnemiesDefeated { get; set; }
        public int DamageDealt { get; set; }
        public int CardsPlayed { get; set; }
        public int RelicsCollected { get; set; }
        public int GoldEarned { get; set; }
        public bool Victory { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public uint Seed { get; set; }
        public List<string> DeckComposition { get; set; } = new();
    }

    public class CardConfigData { }
    public class CharacterConfigData { }
    public class EnemyConfigData { }
    public class RelicConfigData { }
    public class PotionConfigData { }
    public class EventConfigData { }
    public class AudioConfigData { }
    public class EffectConfigData { }

    public static class ConfigLoader
    {
        public static bool UseCompiledConfig { get; set; } = false;
        public static bool CompileConfigToBytes(string configName) { return true; }
        public static T LoadConfig<T>(string configName) where T : class { return null; }
    }
}

namespace RoguelikeGame.Generation
{
    public enum NodeType { Monster = 0, Elite = 1, Boss = 2, Rest = 3, Shop = 4, Event = 5, Treasure = 6 }
    public enum MapNodeStatus { Unreachable = 0, Reachable = 1, Visited = 2, Current = 3 }

    public partial class MapGenerator : Node
    {
        public static MapGenerator Instance => null;
        public void Initialize(uint seed) {}
        public Core.FloorMap GenerateFloor(int floor) => null;
        public bool VisitNode(Core.FloorMap map, Core.MapNode node) => true;
        public void VisitNode(int nodeId) {}
    }
    public partial class DungeonGenerator : Node { public static DungeonGenerator Instance => null; }
}

namespace RoguelikeGame.Database
{
    public enum RelicTier { Starter = 0, Common = 1, Uncommon = 2, Rare = 3, Boss = 4, Shop = 5, Special = 6 }

    public partial class CardDatabase : Node { public static CardDatabase Instance => null; public Core.CardData GetCard(string id) => null; }
    public partial class CharacterDatabase : Node { public static CharacterDatabase Instance => null; public Core.CharacterData GetCharacter(string id) => null; }
    public partial class EnemyDatabase : Node { public static EnemyDatabase Instance => null; public Godot.Collections.Array GetEnemies() => null; }
    public partial class EventDatabase : Node { public static EventDatabase Instance => null; }
    public partial class PotionDatabase : Node { public static PotionDatabase Instance => null; public Core.PotionData GetRandomPotion(RandomNumberGenerator rng) => null; }
    public partial class RelicDatabase : Node { public static RelicDatabase Instance => null; public Core.RelicData GetRandomRelic(RelicTier tier, RandomNumberGenerator rng) => null; }
    public partial class TimelineManager : Node
    {
        public static TimelineManager Instance => null;
        public void AddEventTriggered(string type, string desc = "", int floor = 0, int room = 0) {}
        public void AddCombatStart(int floor, int room, List<string> enemies = null) {}
        public void AddCombatEnd(int floor, int room, bool victory = true, int damageTaken = 0, int damageDealt = 0) {}
        public void AddDeath(int floor, int room, string cause = "") {}
        public void AddEventTriggered(string evt) {}
        public void AddCombatStart(string enemy) {}
    }
    public partial class AchievementSystem : Node { public static AchievementSystem Instance => null; public void RecordRun(Core.RunStatistics stats) {} public void UpdateProgress(string achievementId, int amount) {} }
}

namespace RoguelikeGame.Systems
{
    public partial class ShopManager : Node { public static ShopManager Instance => null; public void Initialize(uint seed) {} public Godot.Collections.Array GenerateShopInventory(string charId, int floor) => null; }
    public partial class WaveManager : Node { public static WaveManager Instance => null; }
    public partial class ItemManager : Node { public static ItemManager Instance => null; }
    public partial class UnitManager : Node { public static UnitManager Instance => null; }
    public partial class ObjectPoolManager : Node { public static ObjectPoolManager Instance => null; }
    public partial class EnhancedSaveSystem : Node { public static EnhancedSaveSystem Instance => null; public Godot.Collections.Dictionary LoadGame(int slot) => null; }
    public partial class TutorialManager : Node { public static TutorialManager Instance => null; }
    public partial class AchievementManager : Node { public static AchievementManager Instance => null; }
}

namespace RoguelikeGame.Audio
{
    public partial class AudioManager : Node { public static AudioManager Instance => null; public float MusicVolume { get; set; } = 1.0f; public float SfxVolume { get; set; } = 1.0f; public void PlayButtonClick() {} public void PlayBGM(string name) {} public void PlaySFX(string name) {} public void StopBGM() {} }
    public partial class AudioGenerator : Node { public static AudioGenerator Instance => null; public void GenerateAllAudioResources() {} }
}

namespace RoguelikeGame.Effects
{
    public partial class ParticleManager : Node { public static ParticleManager Instance => null; }
}

namespace RoguelikeGame.UI
{
    public partial class CombatHUD : Control
    {
        public static CombatHUD Instance => null;
        public void AddEnemy(string id, string name, int hp, int maxHp, int dmg) {}
        public void UpdateEnemyIntent(int idx, string desc, string icon) {}
        public void UpdateEnemyHealth(int idx, int hp, int maxHp) {}
        public void UpdatePlayerHealth(int hp, int maxHp) {}
        public void UpdatePlayerBlock(int block) {}
        public void UpdatePlayerEnergy(int current, int max) {}
        public void UpdateHand(Godot.Collections.Array cards) {}
        public void ShowEnemyAttackFeedback(int idx) {}
        public void ShowPlayerHitFeedback() {}
        public void ShowEnemyHitFeedback(int idx) {}
        public void ShowCardPlayAnimation(Godot.Collections.Dictionary cardData, int targetIdx) {}
    }
}

namespace RoguelikeGame.UI.Panels
{
    public partial class SaveSlotPanel : Control { public event Action<int> SaveSelected; public event Action<int> SaveDeleted; public event Action Closed; }
    public partial class AchievementPanel : Control { public event Action Closed; }
    public partial class MapView : Control { public static MapView Instance => null; public static void ResetPersistentState() {} }
    public partial class CharacterSelect : Control { }
    public partial class ShopPanel : Control { }
    public partial class RestSitePanel : Control { }
    public partial class EventPanel : Control { }
    public partial class TreasurePanel : Control { }
    public partial class RewardPanel : Control { }
    public partial class VictoryScreen : Control { }
    public partial class GameOverScreen : Control { }
}

namespace RoguelikeGame.Packages
{
}

namespace RoguelikeGame.Combat
{
    public partial class StsCombatEngine : Node
    {
        public static StsCombatEngine Instance => null;
        public void InitializeCombat(Godot.Collections.Array enemies, uint seed) {}
        public void PlayCard(string cardId, int targetIdx) {}
        public void EndTurn() {}
        public bool IsCombatOver => true;
        public bool IsPlayerTurn => false;
        public Godot.Collections.Dictionary GetPlayer() => null;
        public Godot.Collections.Array GetEnemies() => null;
        public int GetTurnNumber() => 0;
    }
    public partial class CombatManager : Node { public static CombatManager Instance => null; public void SetPlayerDeck(List<Core.CardData> deck) {} }
    public partial class EncounterGenerator : Node { public static EncounterGenerator Instance => null; public Godot.Collections.Dictionary GenerateEncounter(string id, int nodeType, int floor) => null; public static int GetTotalFloors() => 12; }
}
