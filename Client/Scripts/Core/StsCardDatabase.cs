using System.Collections.Generic;

namespace RoguelikeGame.Core
{
	public static class StsCardDatabase
	{
		private static readonly Dictionary<string, StsCardData> AllCards = new();

		static StsCardDatabase()
		{
			RegisterIroncladCards();
		}

		private static void RegisterIroncladCards()
		{
			Register(StsCardData.CreateStrike());
			Register(StsCardData.CreateDefend());
			Register(StsCardData.CreateBash());

			RegisterAttackCards();
			RegisterSkillCards();
			RegisterPowerCards();
		}

		private static void RegisterAttackCards()
		{
			Register(new StsCardData
			{
				Id = "Cleave",
				Name = "顺劈",
				Description = "对所有敌人造成 8 点伤害。",
				Cost = 1,
				Type = StsCardType.Attack,
				Rarity = StsCardRarity.Common,
				Target = StsCardTarget.EnemyAll,
				Damage = 8,
				DamageType = StsDamageType.Attack
			});

			Register(new StsCardData
			{
				Id = "Iron_Wave",
				Name = "铁波",
				Description = "造成 5 点伤害。\n获得 5 点格挡。",
				Cost = 1,
				Type = StsCardType.Attack,
				Rarity = StsCardRarity.Common,
				Target = StsCardTarget.EnemySingle,
				Damage = 5,
				Block = 5,
				DamageType = StsDamageType.Attack
			});

			Register(new StsCardData
			{
				Id = "Pommel_Strike",
				Name = "柄击",
				Description = "造成 9 点伤害。\n抽 1 张牌。",
				Cost = 1,
				Type = StsCardType.Attack,
				Rarity = StsCardRarity.Common,
				Target = StsCardTarget.EnemySingle,
				Damage = 9,
				MagicNumber = 1,
				DamageType = StsDamageType.Attack
			});

			Register(new StsCardData
			{
				Id = "Twin_Strike",
				Name = "双击",
				Description = "造成 5 点伤害两次。",
				Cost = 1,
				Type = StsCardType.Attack,
				Rarity = StsCardRarity.Common,
				Target = StsCardTarget.EnemySingle,
				Damage = 5,
				MagicNumber = 2,
				DamageType = StsDamageType.Attack
			});

			Register(new StsCardData
			{
				Id = "Anger",
				Name = "愤怒",
				Description = "造成 6 点伤害。\n将一张愤怒放入弃牌堆。",
				Cost = 0,
				Type = StsCardType.Attack,
				Rarity = StsCardRarity.Common,
				Target = StsCardTarget.EnemySingle,
				Damage = 6,
				DamageType = StsDamageType.Attack
			});

			Register(new StsCardData
			{
				Id = "Clothesline",
				Name = "晾衣绳",
				Description = "造成 12 点伤害。\n施加 2 层虚弱。",
				Cost = 2,
				Type = StsCardType.Attack,
				Rarity = StsCardRarity.Common,
				Target = StsCardTarget.EnemySingle,
				Damage = 12,
				MagicNumber = 2,
				DamageType = StsDamageType.Attack
			});

			Register(new StsCardData
			{
				Id = "Heavy_Blade",
				Name = "重刃",
				Description = "造成 14 点伤害。\n力量对此牌的影响为 3 倍。",
				Cost = 2,
				Type = StsCardType.Attack,
				Rarity = StsCardRarity.Common,
				Target = StsCardTarget.EnemySingle,
				Damage = 14,
				MagicNumber = 3,
				DamageType = StsDamageType.Attack
			});

			Register(new StsCardData
			{
				Id = "Body_Slam",
				Name = "身体撞击",
				Description = "造成等同于当前格挡的伤害。",
				Cost = 1,
				Type = StsCardType.Attack,
				Rarity = StsCardRarity.Common,
				Target = StsCardTarget.EnemySingle,
				Damage = 0,
				DamageType = StsDamageType.Attack
			});

			Register(new StsCardData
			{
				Id = "Sword_Boomerang",
				Name = "剑回旋镖",
				Description = "随机造成 3 次 3 点伤害。",
				Cost = 1,
				Type = StsCardType.Attack,
				Rarity = StsCardRarity.Common,
				Target = StsCardTarget.EnemyAll,
				Damage = 3,
				MagicNumber = 3,
				DamageType = StsDamageType.Attack
			});

			Register(new StsCardData
			{
				Id = "Perfected_Strike",
				Name = "完美打击",
				Description = "造成 6 点伤害。\n手牌中每有一张\"打击\"牌，伤害+2。",
				Cost = 2,
				Type = StsCardType.Attack,
				Rarity = StsCardRarity.Common,
				Target = StsCardTarget.EnemySingle,
				Damage = 6,
				MagicNumber = 2,
				DamageType = StsDamageType.Attack
			});

			Register(new StsCardData
			{
				Id = "Uppercut",
				Name = "上勾拳",
				Description = "造成 13 点伤害。\n施加 1 层虚弱。\n施加 1 层脆弱。",
				Cost = 2,
				Type = StsCardType.Attack,
				Rarity = StsCardRarity.Uncommon,
				Target = StsCardTarget.EnemySingle,
				Damage = 13,
				MagicNumber = 1,
				DamageType = StsDamageType.Attack
			});

			Register(new StsCardData
			{
				Id = "Carnage",
				Name = "杀戮",
				Description = "消耗。造成 20 点伤害。",
				Cost = 2,
				Type = StsCardType.Attack,
				Rarity = StsCardRarity.Uncommon,
				Target = StsCardTarget.EnemySingle,
				Damage = 20,
				Exhaust = true,
				DamageType = StsDamageType.Attack
			});

			Register(new StsCardData
			{
				Id = "Reckless_Charge",
				Name = "鲁莽冲锋",
				Description = "造成 7 点伤害。\n将一张眩晕放入弃牌堆。",
				Cost = 0,
				Type = StsCardType.Attack,
				Rarity = StsCardRarity.Uncommon,
				Target = StsCardTarget.EnemySingle,
				Damage = 7,
				DamageType = StsDamageType.Attack
			});

			Register(new StsCardData
			{
				Id = "Bludgeon",
				Name = "重击",
				Description = "造成 32 点伤害。",
				Cost = 3,
				Type = StsCardType.Attack,
				Rarity = StsCardRarity.Rare,
				Target = StsCardTarget.EnemySingle,
				Damage = 32,
				DamageType = StsDamageType.Attack
			});

			Register(new StsCardData
			{
				Id = "Impervious",
				Name = "坚不可摧",
				Description = "获得 30 点格挡。",
				Cost = 2,
				Type = StsCardType.Skill,
				Rarity = StsCardRarity.Rare,
				Target = StsCardTarget.Self,
				Block = 30,
				Exhaust = true
			});
		}

		private static void RegisterSkillCards()
		{
			Register(new StsCardData
			{
				Id = "Shrug_It_Off",
				Name = "耸肩",
				Description = "获得 8 点格挡。\n抽 1 张牌。",
				Cost = 1,
				Type = StsCardType.Skill,
				Rarity = StsCardRarity.Common,
				Target = StsCardTarget.Self,
				Block = 8,
				MagicNumber = 1
			});

			Register(new StsCardData
			{
				Id = "Armaments",
				Name = "武装",
				Description = "获得 5 点格挡。\n将手牌中的一张牌升级。",
				Cost = 1,
				Type = StsCardType.Skill,
				Rarity = StsCardRarity.Common,
				Target = StsCardTarget.Self,
				Block = 5,
				MagicNumber = 1
			});

			Register(new StsCardData
			{
				Id = "Flex",
				Name = "屈伸",
				Description = "获得 2 点力量。\n回合结束时失去 2 点力量。",
				Cost = 0,
				Type = StsCardType.Skill,
				Rarity = StsCardRarity.Common,
				Target = StsCardTarget.Self,
				MagicNumber = 2
			});

			Register(new StsCardData
			{
				Id = "Battle_Trance",
				Name = "战斗恍惚",
				Description = "抽 3 张牌。\n本回合不能再抽牌。",
				Cost = 0,
				Type = StsCardType.Skill,
				Rarity = StsCardRarity.Uncommon,
				Target = StsCardTarget.Self,
				MagicNumber = 3
			});

			Register(new StsCardData
			{
				Id = "Bloodletting",
				Name = "放血",
				Description = "失去 3 点生命。\n获得 2 点能量。",
				Cost = 0,
				Type = StsCardType.Skill,
				Rarity = StsCardRarity.Uncommon,
				Target = StsCardTarget.Self,
				MagicNumber = 2
			});

			Register(new StsCardData
			{
				Id = "Inflame",
				Name = "燃烧",
				Description = "获得 2 点力量。",
				Cost = 1,
				Type = StsCardType.Skill,
				Rarity = StsCardRarity.Uncommon,
				Target = StsCardTarget.Self,
				MagicNumber = 2
			});

			Register(new StsCardData
			{
				Id = "Rage",
				Name = "愤怒",
				Description = "每打出一张攻击牌，获得 3 点格挡。",
				Cost = 0,
				Type = StsCardType.Skill,
				Rarity = StsCardRarity.Uncommon,
				Target = StsCardTarget.Self,
				MagicNumber = 3
			});

			Register(new StsCardData
			{
				Id = "Flame_Barrier",
				Name = "火焰屏障",
				Description = "获得 12 点格挡。\n受到攻击时造成 4 点伤害。",
				Cost = 2,
				Type = StsCardType.Skill,
				Rarity = StsCardRarity.Uncommon,
				Target = StsCardTarget.Self,
				Block = 12,
				MagicNumber = 4
			});

			Register(new StsCardData
			{
				Id = "Ghostly_Armor",
				Name = "幽灵护甲",
				Description = "虚无。获得 10 点格挡。",
				Cost = 1,
				Type = StsCardType.Skill,
				Rarity = StsCardRarity.Uncommon,
				Target = StsCardTarget.Self,
				Block = 10,
				Ethereal = true
			});

			Register(new StsCardData
			{
				Id = "Entrench",
				Name = "挖掘",
				Description = "将当前格挡翻倍。",
				Cost = 2,
				Type = StsCardType.Skill,
				Rarity = StsCardRarity.Uncommon,
				Target = StsCardTarget.Self
			});
		}

		private static void RegisterPowerCards()
		{
			Register(new StsCardData
			{
				Id = "Inflame_Power",
				Name = "燃烧",
				Description = "获得 2 点力量。",
				Cost = 1,
				Type = StsCardType.Power,
				Rarity = StsCardRarity.Uncommon,
				Target = StsCardTarget.Self,
				MagicNumber = 2
			});

			Register(new StsCardData
			{
				Id = "Metallicize",
				Name = "金属化",
				Description = "回合结束时获得 3 点格挡。",
				Cost = 1,
				Type = StsCardType.Power,
				Rarity = StsCardRarity.Uncommon,
				Target = StsCardTarget.Self,
				MagicNumber = 3
			});

			Register(new StsCardData
			{
				Id = "Combust",
				Name = "燃烧",
				Description = "回合开始时失去 1 点生命，对所有敌人造成 5 点伤害。",
				Cost = 1,
				Type = StsCardType.Power,
				Rarity = StsCardRarity.Uncommon,
				Target = StsCardTarget.Self,
				Damage = 5,
				MagicNumber = 1
			});

			Register(new StsCardData
			{
				Id = "Dark_Embrace",
				Name = "黑暗契约",
				Description = "每消耗一张牌，抽 1 张牌。",
				Cost = 2,
				Type = StsCardType.Power,
				Rarity = StsCardRarity.Uncommon,
				Target = StsCardTarget.Self,
				MagicNumber = 1
			});

			Register(new StsCardData
			{
				Id = "Feel_No_Pain",
				Name = "无痛",
				Description = "每消耗一张牌，获得 3 点格挡。",
				Cost = 1,
				Type = StsCardType.Power,
				Rarity = StsCardRarity.Uncommon,
				Target = StsCardTarget.Self,
				MagicNumber = 3
			});

			Register(new StsCardData
			{
				Id = "Demon_Form",
				Name = "恶魔形态",
				Description = "回合开始时获得 2 点力量。",
				Cost = 3,
				Type = StsCardType.Power,
				Rarity = StsCardRarity.Rare,
				Target = StsCardTarget.Self,
				MagicNumber = 2
			});

			Register(new StsCardData
			{
				Id = "Barricade",
				Name = "壁垒",
				Description = "格挡不再在回合开始时清除。",
				Cost = 3,
				Type = StsCardType.Power,
				Rarity = StsCardRarity.Rare,
				Target = StsCardTarget.Self
			});

			Register(new StsCardData
			{
				Id = "Brutality",
				Name = "残暴",
				Description = "回合开始时失去 1 点生命，抽 1 张牌。",
				Cost = 1,
				Type = StsCardType.Power,
				Rarity = StsCardRarity.Rare,
				Target = StsCardTarget.Self,
				MagicNumber = 1
			});
		}

		private static void Register(StsCardData card)
		{
			AllCards[card.Id] = card;
		}

		public static StsCardData GetCard(string id)
		{
			return AllCards.TryGetValue(id, out var card) ? card : null;
		}

		public static List<StsCardData> GetAllCards()
		{
			return new List<StsCardData>(AllCards.Values);
		}

		public static List<StsCardData> GetCardsByRarity(StsCardRarity rarity)
		{
			var cards = new List<StsCardData>();
			foreach (var card in AllCards.Values)
			{
				if (card.Rarity == rarity)
					cards.Add(card);
			}
			return cards;
		}

		public static List<StsCardData> GetCardsByType(StsCardType type)
		{
			var cards = new List<StsCardData>();
			foreach (var card in AllCards.Values)
			{
				if (card.Type == type)
					cards.Add(card);
			}
			return cards;
		}

		public static List<StsCardData> GetStarterDeck()
		{
			var deck = new List<StsCardData>();

			for (int i = 0; i < 5; i++)
				deck.Add(GetCard("Strike_R"));

			for (int i = 0; i < 4; i++)
				deck.Add(GetCard("Defend_R"));

			deck.Add(GetCard("Bash"));

			return deck;
		}

		public static List<StsCardData> GetRandomRewardCards(int count, System.Random rng)
		{
			var cards = new List<StsCardData>();
			var pool = new List<StsCardData>(AllCards.Values);

			for (int i = 0; i < count && pool.Count > 0; i++)
			{
				int index = rng.Next(pool.Count);
				cards.Add(pool[index]);
				pool.RemoveAt(index);
			}

			return cards;
		}
	}
}