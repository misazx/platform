using System.Collections.Generic;

namespace RoguelikeGame.Core
{
	public enum StsCharacter
	{
		Ironclad,
		Silent,
		Defect,
		Watcher
	}

	public class StsCharacterData
	{
		public StsCharacter Character { get; set; }
		public string Name { get; set; }
		public int MaxHp { get; set; }
		public int StartingGold { get; set; }
		public string StartingRelicId { get; set; }
		public List<string> StartingDeck { get; set; } = new();
		public string Description { get; set; }

		public static StsCharacterData CreateIronclad() => new()
		{
			Character = StsCharacter.Ironclad,
			Name = "铁甲战士",
			MaxHp = 80,
			StartingGold = 99,
			StartingRelicId = "Burning_Blood",
			Description = "红发的战士，擅长力量和生命回复",
			StartingDeck = new List<string>
			{
				"Strike_R", "Strike_R", "Strike_R", "Strike_R", "Strike_R",
				"Defend_R", "Defend_R", "Defend_R", "Defend_R",
				"Bash"
			}
		};

		public static StsCharacterData CreateSilent() => new()
		{
			Character = StsCharacter.Silent,
			Name = "静默猎人",
			MaxHp = 70,
			StartingGold = 99,
			StartingRelicId = "Snake_Ring",
			Description = "致命的猎人，擅长毒素和防御",
			StartingDeck = new List<string>
			{
				"Strike_G", "Strike_G", "Strike_G", "Strike_G", "Strike_G",
				"Defend_G", "Defend_G", "Defend_G", "Defend_G", "Defend_G",
				"Survivor"
			}
		};

		public static StsCharacterData CreateDefect() => new()
		{
			Character = StsCharacter.Defect,
			Name = "缺陷机器人",
			MaxHp = 75,
			StartingGold = 99,
			StartingRelicId = "Cracked_Core",
			Description = "觉醒的机器人，擅长充能和冰霜",
			StartingDeck = new List<string>
			{
				"Strike_B", "Strike_B", "Strike_B", "Strike_B",
				"Defend_B", "Defend_B", "Defend_B", "Defend_B",
				"Zap", "Dualcast"
			}
		};

		public static StsCharacterData CreateWatcher() => new()
		{
			Character = StsCharacter.Watcher,
			Name = "观者",
			MaxHp = 72,
			StartingGold = 99,
			StartingRelicId = "Holy_Water",
			Description = "盲眼的圣女，擅长姿态和预见",
			StartingDeck = new List<string>
			{
				"Strike_P", "Strike_P", "Strike_P", "Strike_P",
				"Defend_P", "Defend_P", "Defend_P", "Defend_P",
				"Eruption", "Vigilance"
			}
		};

		public static StsCharacterData GetCharacterData(StsCharacter character)
		{
			return character switch
			{
				StsCharacter.Ironclad => CreateIronclad(),
				StsCharacter.Silent => CreateSilent(),
				StsCharacter.Defect => CreateDefect(),
				StsCharacter.Watcher => CreateWatcher(),
				_ => CreateIronclad()
			};
		}
	}

	public class StsBossData
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public int MaxHp { get; set; }
		public int CurrentHp { get; set; }
		public int Block { get; set; }
		public List<StsStatusEffect> StatusEffects { get; set; } = new();
		public StsEnemyIntent CurrentIntent { get; set; }
		public int Phase { get; set; } = 1;
		public bool IsBoss => true;

		public static StsBossData CreateTheGuardian() => new()
		{
			Id = "The_Guardian",
			Name = "守护者",
			MaxHp = 240,
			CurrentHp = 240
		};

		public static StsBossData CreateHexaghost() => new()
		{
			Id = "Hexaghost",
			Name = "六火幽灵",
			MaxHp = 250,
			CurrentHp = 250
		};

		public static StsBossData CreateSlimeBoss() => new()
		{
			Id = "Slime_Boss",
			Name = "史莱姆王",
			MaxHp = 140,
			CurrentHp = 140
		};

		public void ApplyDamage(int damage)
		{
			if (Block > 0)
			{
				if (damage >= Block)
				{
					damage -= Block;
					Block = 0;
				}
				else
				{
					Block -= damage;
					damage = 0;
				}
			}
			CurrentHp -= damage;
		}

		public bool IsDead => CurrentHp <= 0;
	}

	public class StsPotionData
	{
		public enum PotionType
		{
			Attack,
			Skill,
			Power,
			Unknown
		}

		public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public PotionType Type { get; set; }
		public int Value { get; set; }
		public bool CanDiscard { get; set; } = true;
		public bool RequiresTarget { get; set; } = false;

		public static StsPotionData CreateHealthPotion() => new()
		{
			Id = "Health_Potion",
			Name = "治疗药水",
			Description = "回复 15 点生命。",
			Type = PotionType.Skill,
			Value = 15
		};

		public static StsPotionData CreateFirePotion() => new()
		{
			Id = "Fire_Potion",
			Name = "火焰药水",
			Description = "造成 20 点伤害。",
			Type = PotionType.Attack,
			Value = 20,
			RequiresTarget = true
		};

		public static StsPotionData CreateBlockPotion() => new()
		{
			Id = "Block_Potion",
			Name = "格挡药水",
			Description = "获得 12 点格挡。",
			Type = PotionType.Skill,
			Value = 12
		};

		public static StsPotionData CreateStrengthPotion() => new()
		{
			Id = "Strength_Potion",
			Name = "力量药水",
			Description = "获得 2 点力量。",
			Type = PotionType.Power,
			Value = 2
		};

		public static StsPotionData CreateDexterityPotion() => new()
		{
			Id = "Dexterity_Potion",
			Name = "敏捷药水",
			Description = "获得 2 点敏捷。",
			Type = PotionType.Power,
			Value = 2
		};

		public static StsPotionData CreateEnergyPotion() => new()
		{
			Id = "Energy_Potion",
			Name = "能量药水",
			Description = "获得 2 点能量。",
			Type = PotionType.Skill,
			Value = 2
		};

		public static StsPotionData CreateExplosivePotion() => new()
		{
			Id = "Explosive_Potion",
			Name = "爆炸药水",
			Description = "对所有敌人造成 10 点伤害。",
			Type = PotionType.Attack,
			Value = 10
		};

		public static StsPotionData CreateFearPotion() => new()
		{
			Id = "Fear_Potion",
			Name = "恐惧药水",
			Description = "施加 3 层脆弱。",
			Type = PotionType.Skill,
			Value = 3,
			RequiresTarget = true
		};

		public static List<StsPotionData> GetRandomPotions(int count, System.Random rng)
		{
			var allPotions = new List<StsPotionData>
			{
				CreateHealthPotion(),
				CreateFirePotion(),
				CreateBlockPotion(),
				CreateStrengthPotion(),
				CreateDexterityPotion(),
				CreateEnergyPotion(),
				CreateExplosivePotion(),
				CreateFearPotion()
			};

			var result = new List<StsPotionData>();
			for (int i = 0; i < count && allPotions.Count > 0; i++)
			{
				int index = rng.Next(allPotions.Count);
				result.Add(allPotions[index]);
				allPotions.RemoveAt(index);
			}

			return result;
		}
	}

	public class StsEventData
	{
		public string Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string ImagePath { get; set; }
		public List<StsEventOption> Options { get; set; } = new();

		public static StsEventData CreateBigFish() => new()
		{
			Id = "Big_Fish",
			Title = "大鱼",
			Description = "你发现了一条巨大的鱼躺在地上。它看起来很新鲜。",
			Options = new List<StsEventOption>
			{
				new() { Text = "吃掉它", Effect = "回复 5 点生命，获得 1 张伤口牌", GoldReward = 0 },
				new() { Text = "离开", Effect = "获得 50 金币", GoldReward = 50 },
				new() { Text = "无视", Effect = "什么也没发生", GoldReward = 0 }
			}
		};

		public static StsEventData CreateTheCleric() => new()
		{
			Id = "The_Cleric",
			Title = "牧师",
			Description = "一位友善的牧师向你招手。",
			Options = new List<StsEventOption>
			{
				new() { Text = "治疗 (花费 50 金币)", Effect = "回复 25% 最大生命", GoldCost = 50 },
				new() { Text = "移除卡牌 (花费 75 金币)", Effect = "从卡组中移除一张牌", GoldCost = 75 },
				new() { Text = "离开", Effect = "什么也没发生", GoldReward = 0 }
			}
		};

		public static StsEventData CreateGoldenShrine() => new()
		{
			Id = "Golden_Shrine",
			Title = "金色神殿",
			Description = "一个闪耀着金光的神殿出现在你面前。",
			Options = new List<StsEventOption>
			{
				new() { Text = "祈祷", Effect = "获得 100 金币，获得 1 张悔恨牌", GoldReward = 100 },
				new() { Text = "偷窃", Effect = "获得 275 金币，受到 5 点伤害", GoldReward = 275, DamageTaken = 5 },
				new() { Text = "离开", Effect = "什么也没发生", GoldReward = 0 }
			}
		};

		public static StsEventData CreateDeadAdventurer() => new()
		{
			Id = "Dead_Adventurer",
			Title = "死去的冒险者",
			Description = "你发现了一具冒险者的尸体。",
			Options = new List<StsEventOption>
			{
				new() { Text = "搜查尸体", Effect = "获得 50 金币", GoldReward = 50 },
				new() { Text = "埋葬", Effect = "获得 1 点最大生命", MaxHpBonus = 1 },
				new() { Text = "离开", Effect = "什么也没发生", GoldReward = 0 }
			}
		};

		public static StsEventData CreateTheMushrooms() => new()
		{
			Id = "The_Mushrooms",
			Title = "蘑菇",
			Description = "你发现了一片奇怪的蘑菇。",
			Options = new List<StsEventOption>
			{
				new() { Text = "吃下蘑菇", Effect = "回复 15 点生命，获得 1 张奇怪蘑菇遗物", GoldReward = 0 },
				new() { Text = "无视", Effect = "什么也没发生", GoldReward = 0 }
			}
		};

		public static List<StsEventData> GetRandomEvents(int count, System.Random rng)
		{
			var allEvents = new List<StsEventData>
			{
				CreateBigFish(),
				CreateTheCleric(),
				CreateGoldenShrine(),
				CreateDeadAdventurer(),
				CreateTheMushrooms()
			};

			var result = new List<StsEventData>();
			for (int i = 0; i < count && allEvents.Count > 0; i++)
			{
				int index = rng.Next(allEvents.Count);
				result.Add(allEvents[index]);
				allEvents.RemoveAt(index);
			}

			return result;
		}
	}

	public class StsEventOption
	{
		public string Text { get; set; }
		public string Effect { get; set; }
		public int GoldReward { get; set; }
		public int GoldCost { get; set; }
		public int DamageTaken { get; set; }
		public int MaxHpBonus { get; set; }
		public int HealAmount { get; set; }
		public string CardReward { get; set; }
		public string RelicReward { get; set; }
	}
}