using System.Collections.Generic;
using RoguelikeGame.Packages;
using RoguelikeGame.Core;
using RoguelikeGame.Database;

namespace RoguelikeGame.Packages.Samples
{
	public class FrostExpansion : PackageExtensionBase
	{
		public override string PackageId => "frost_expansion";

		private List<CardConfig> _frostCards = new();
		private List<CharacterConfig> _frostCharacters = new();

		public override void OnInitialize()
		{
			base.OnInitialize();
			GD.Print("[FrostExpansion] Initializing frost expansion content...");
		}

		public override void RegisterCustomCards()
		{
			_frostCards = new List<CardConfig>
			{
				new CardConfig
				{
					Id = "frost_strike",
					Name = "冰霜打击",
					Description = "造成 7 点伤害。施加 2 层寒冷。",
					Cost = 1,
					Type = "Attack",
					Damage = 7,
					Rarity = "Common",
					CharacterId = "ironclad",
					Color = "#A8D8FF",
					CustomData = new Dictionary<string, object>
					{
						{ "frostDamage", 7 },
						{ "chillStacks", 2 }
					}
				},
				new CardConfig
				{
					Id = "blizzard",
					Name = "暴风雪",
					Description = "对所有敌人造成 5 点伤害。施加 3 层寒冷。",
					Cost = 2,
					Type = "Attack",
					Damage = 5,
					Rarity = "Rare",
					Target = "AllEnemies",
					CharacterId = "silent",
					Color = "#E0F4FF",
					CustomData = new Dictionary<string, object>
					{
						{ "aoe", true },
						{ "chillStacks", 3 }
					}
				},
				new CardConfig
				{
					Id = "ice_barrier",
					Name = "冰霜屏障",
					Description = "获得 12 点格挡。本回合受到的伤害减少 25%。",
					Cost = 1,
					Type = "Skill",
					Block = 12,
					Rarity = "Uncommon",
					CharacterId = "defect",
					Color = "#B8E6F0"
				},
				new CardConfig
				{
					Id = "frozen_lance",
					Name = "冰冻长矛",
					Description = "造成 14 点伤害。如果敌人有 5 层以上寒冷，伤害翻倍。",
					Cost = 2,
					Type = "Attack",
					Damage = 14,
					Rarity = "Rare",
					CharacterId = "watcher",
					Color = "#D4F1FF",
					CustomData = new Dictionary<string, object>
					{
						{ "bonusCondition", "chill >= 5" },
						{ "bonusMultiplier", 2.0 }
					}
				}
			};

			foreach (var card in _frostCards)
			{
				if (CardDatabase.Instance != null)
				{
					CardDatabase.Instance.RegisterCard(card);
				}
			}

			GD.Print($"[FrostExpansion] Registered {_frostCards.Count} frost cards");
		}

		public override void RegisterCustomCharacters()
		{
			var frostMage = new CharacterConfig
			{
				Id = "frost_mage",
				Name = "冰霜法师",
				Title = "寒冬使者",
				Description = "掌控冰雪之力的强大法师，能够冻结敌人并召唤暴风雪。",
				Class = "FrostMage",
				MaxHealth = 75,
				StartingGold = 99,
				CustomData = new Dictionary<string, object>
				{
					{ "theme", "frost" },
					{ "specialAbility", "freeze" },
					{ "startingRelic", "frost_core" }
				}
			};

			_frostCharacters.Add(frostMage);

			if (CharacterDatabase.Instance != null)
			{
				CharacterDatabase.Instance.RegisterCharacter(frostMage);
			}

			GD.Print("[FrostExpansion] Registered frost mage character");
		}

		public override void RegisterCustomRelics()
		{
			// 可以在这里注册新的圣物
			GD.Print("[FrostExpansion] Custom relics would be registered here");
		}

		public override Dictionary<string, object> GetSaveData()
		{
			return new Dictionary<string, object>
			{
				{ "totalChillApplied", 0 },
				{ "enemiesFrozen", 0 },
				{ "blizzardsCast", 0 }
			};
		}

		public override void LoadSaveData(Dictionary<string, object> data)
		{
			if (data.ContainsKey("totalChillApplied"))
			{
				GD.Print($"[FrostExpansion] Loaded save data - Total chill applied: {data["totalChillApplied"]}");
			}
		}

		public override List<string> GetConflictingPackages()
		{
			return new List<string> { "fire_expansion" };
		}
	}

	public class ShadowRealmExtension : PackageExtensionBase
	{
		public override string PackageId => "shadow_realm";

		public override void RegisterCustomCards()
		{
			var shadowCards = new List<CardConfig>
			{
				new CardConfig
				{
					Id = "shadow_strike",
					Name = "暗影打击",
					Description = "造成 9 点伤害。如果在潜行状态，额外造成 6 点伤害。",
					Cost = 1,
					Type = "Attack",
					Damage = 9,
					Rarity = "Common",
					Color = "#2C2C54"
				},
				new CardConfig
				{
					Id = "vanish",
					Name = "消失",
					Description = "进入潜行状态。获得 2 层敏捷。",
					Cost = 1,
					Type = "Skill",
					Rarity = "Common",
					Color = "#474787"
				},
				new CardConfig
				{
					Id = "shadow_form",
					Name = "暗影形态",
					Description = "变身进入暗影形态。本回合所有攻击卡牌消耗减少 1 点。",
					Cost = 0,
					Type = "Power",
					Rarity = "Rare",
					Color = "#1B1464",
					IsEthereal = true
				}
			};

			foreach (var card in shadowCards)
			{
				CardDatabase.Instance?.RegisterCard(card);
			}

			GD.Print($"[ShadowRealm] Registered {shadowCards.Count} shadow cards");
		}

		public override void OnLaunch()
		{
			base.OnLaunch();
			GD.Print("[ShadowRealm] Applying shadow realm visual effects...");
		}
	}
}
