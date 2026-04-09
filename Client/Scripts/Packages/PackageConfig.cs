using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;

namespace RoguelikeGame.Packages
{
	public class PackageConfig
	{
		[JsonPropertyName("packageId")]
		public string PackageId { get; set; }

		[JsonPropertyName("version")]
		public string Version { get; set; } = "1.0.0";

		[JsonPropertyName("displayName")]
		public string DisplayName { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; }

		[JsonPropertyName("author")]
		public string Author { get; set; }

		[JsonPropertyName("entryScene")]
		public string EntryScene { get; set; }

		[JsonPropertyName("configFile")]
		public string ConfigFile { get; set; }

		[JsonPropertyName("gameplay")]
		public GameplayConfig Gameplay { get; set; } = new();

		[JsonPropertyName("difficulty")]
		public DifficultyConfig Difficulty { get; set; } = new();

		[JsonPropertyName("content")]
		public ContentConfig Content { get; set; } = new();

		[JsonPropertyName("ui")]
		public UIConfig UI { get; set; } = new();

		[JsonPropertyName("audio")]
		public AudioConfig Audio { get; set; } = new();

		[JsonPropertyName("customSettings")]
		public Dictionary<string, object> CustomSettings { get; set; } = new();

		[JsonPropertyName("lastModified")]
		public DateTime LastModified { get; set; } = DateTime.Now;
	}

	public class GameplayConfig
	{
		[JsonPropertyName("startingGold")]
		public int StartingGold { get; set; } = 99;

		[JsonPropertyName("maxHealth")]
		public int MaxHealth { get; set; } = 80;

		[JsonPropertyName("drawCardsPerTurn")]
		public int DrawCardsPerTurn { get; set; } = 5;

		[JsonPropertyName("energyPerTurn")]
		public int EnergyPerTurn { get; set; } = 3;

		[JsonPropertyName("maxHandSize")]
		public int MaxHandSize { get; set; } = 10;

		[JsonPropertyName("potionSlots")]
		public int PotionSlots { get; set; } = 3;

		[JsonPropertyName("relicSlots")]
		public int RelicSlots { get; set; } = 8;

		[JsonPropertyName("cardRewardCount")]
		public int CardRewardCount { get; set; } = 3;

		[JsonPropertyName("enablePermanentUpgrades")]
		public bool EnablePermanentUpgrades { get; set; } = true;

		[JsonPropertyName("enableEvents")]
		public bool EnableEvents { get; set; } = true;

		[JsonPropertyName("enableShops")]
		public bool EnableShops { get; set; } = true;

		[JsonPropertyName("enableRestSites")]
		public bool EnableRestSites { get; set; } = true;

		[JsonPropertyName("enableEliteEncounters")]
		public bool EnableEliteEncounters { get; set; } = true;

		[JsonPropertyName("enableBossRush")]
		public bool EnableBossRush { get; set; } = false;

		[JsonPropertyName("enableDailyChallenge")]
		public bool EnableDailyChallenge { get; set; } = false;

		[JsonPropertyName("enableAscensionMode")]
		public bool EnableAscensionMode { get; set; } = true;

		[JsonPropertyName("maxAscensionLevel")]
		public int MaxAscensionLevel { get; set; } = 20;
	}

	public class DifficultyConfig
	{
		[JsonPropertyName("defaultDifficulty")]
		public string DefaultDifficulty { get; set; } = "normal";

		[JsonPropertyName("availableDifficulties")]
		public List<string> AvailableDifficulties { get; set; } = new()
		{
			"easy", "normal", "hard", "insane"
		};

		[JsonPropertyName("enemyScaling")]
		public Dictionary<string, float> EnemyScaling { get; set; } = new()
		{
			{ "easy", 0.7f },
			{ "normal", 1.0f },
			{ "hard", 1.4f },
			{ "insane", 2.0f }
		};

		[JsonPropertyName("goldMultiplier")]
		public Dictionary<string, float> GoldMultiplier { get; set; } = new()
		{
			{ "easy", 1.5f },
			{ "normal", 1.0f },
			{ "hard", 0.8f },
			{ "insane", 0.5f }
		};

		[JsonPropertyName("hpModifier")]
		public Dictionary<string, int> HpModifier { get; set; } = new()
		{
			{ "easy", 20 },
			{ "normal", 0 },
			{ "hard", -15 },
			{ "insane", -30 }
		};
	}

	public class ContentConfig
	{
		[JsonPropertyName("enabledCharacters")]
		public List<string> EnabledCharacters { get; set; } = new()
		{
			"ironclad", "silent", "defect", "watcher"
		};

		[JsonPropertyName("disabledCharacters")]
		public List<string> DisabledCharacters { get; set; } = new();

		[JsonPropertyName("enabledCardPools")]
		public List<string> EnabledCardPools { get; set; } = new()
		{
			"colorless", "curse", "special"
		};

		[JsonPropertyName("customCardSets")]
		public List<string> CustomCardSets { get; set; } = new();

		[JsonPropertyName("enabledRelicPools")]
		public List<string> EnabledRelicPools { get; set; } = new()
		{
			"common", "uncommon", "rare", "boss", "shop", "special"
		};

		[JsonPropertyName("enabledEventPools")]
		public List<string> EnabledEventPools { get; set; } = new()
		{
			"normal", "elite", "boss", "shrine", "chest"
		};

		[JsonPropertyName("maxFloorCount")]
		public int MaxFloorCount { get; set; } = 55;

		[JsonPropertyName("actStructure")]
		public List<ActConfig> ActStructure { get; set; } = new()
		{
			new() { ActNumber = 1, FloorRange = "1-17", BossId = "guardian" },
			new() { ActNumber = 2, FloorRange = "18-43", BossId = "collector" },
			new() { ActNumber = 3, FloorRange = "44-55", BossId = "heart" }
		};
	}

	public class ActConfig
	{
		[JsonPropertyName("actNumber")]
		public int ActNumber { get; set; }

		[JsonPropertyName("floorRange")]
		public string FloorRange { get; set; }

		[JsonPropertyName("bossId")]
		public string BossId { get; set; }
	}

	public class UIConfig
	{
		[JsonPropertyName("theme")]
		public string Theme { get; set; } = "dark";

		[JsonPropertyName("language")]
		public string Language { get; set; } = "zh-CN";

		[JsonPropertyName("showDamageNumbers")]
		public bool ShowDamageNumbers { get; set; } = true;

		[JsonPropertyName("showBlockNumbers")]
		public bool ShowBlockNumbers { get; set; } = true;

		[JsonPropertyName("showIntentIcons")]
		public bool ShowIntentIcons { get; set; } = true;

		[JsonPropertyName("animationSpeed")]
		public float AnimationSpeed { get; set; } = 1.0f;

		[JsonPropertyName("cardFlipAnimation")]
		public bool CardFlipAnimation { get; set; } = true;

		[JsonPropertyName("screenShakeIntensity")]
		public float ScreenShakeIntensity { get; set; } = 0.5f;

		[JsonPropertyName("particleEffects")]
		public bool ParticleEffects { get; set; } = true;

		[JsonPropertyName("autoPauseOnFocusLost")]
		public bool AutoPauseOnFocusLost { get; set; } = true;
	}

	public class AudioConfig
	{
		[JsonPropertyName("masterVolume")]
		public float MasterVolume { get; set; } = 1.0f;

		[JsonPropertyName("bgmVolume")]
		public float BgmVolume { get; set; } = 0.8f;

		[JsonPropertyName("sfxVolume")]
		public float SfxVolume { get; set; } = 1.0f;

		[JsonPropertyName("ambientVolume")]
		public float AmbientVolume { get; set; } = 0.6f;

		[JsonPropertyName("voiceVolume")]
		public float VoiceVolume { get; set; } = 1.0f;

		[JsonPropertyName("enableDynamicMusic")]
		public bool EnableDynamicMusic { get; set; } = true;

		[JsonPropertyName("combatMusicIntensity")]
		public bool CombatMusicIntensity { get; set; } = true;
	}
}
