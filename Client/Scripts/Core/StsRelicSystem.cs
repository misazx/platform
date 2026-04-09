using System;
using System.Collections.Generic;
using Godot;

namespace RoguelikeGame.Core
{
	public enum StsRelicRarity
	{
		Common,
		Uncommon,
		Rare,
		Boss,
		Special
	}

	public enum StsRelicTrigger
	{
		OnCombatStart,
		OnCombatEnd,
		OnTurnStart,
		OnTurnEnd,
		OnCardPlayed,
		OnAttack,
		OnDamageDealt,
		OnDamageTaken,
		OnKill,
		OnPickup,
		OnRest,
		OnChestOpen
	}

	public class StsRelicData
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string FlavorText { get; set; }
		public StsRelicRarity Rarity { get; set; }
		public string IconPath { get; set; }
		public Action<StsCombatEngine> OnCombatStart { get; set; }
		public Action<StsCombatEngine> OnCombatEnd { get; set; }
		public Action<StsCombatEngine> OnTurnStart { get; set; }
		public Action<StsCombatEngine> OnTurnEnd { get; set; }
		public Action<StsCombatEngine, StsCardData> OnCardPlayed { get; set; }
		public Action<StsCombatEngine, int> OnDamageDealt { get; set; }
		public Action<StsCombatEngine, int> OnDamageTaken { get; set; }
		public Action<StsCombatEngine> OnKill { get; set; }

		public static StsRelicData CreateBurningBlood() => new()
		{
			Id = "Burning_Blood",
			Name = "燃烧之血",
			Description = "战斗结束时回复 6 点生命。",
			FlavorText = "铁甲战士的起始遗物",
			Rarity = StsRelicRarity.Special,
			OnCombatEnd = (engine) =>
			{
				int heal = Math.Min(6, engine.Player.MaxHp - engine.Player.CurrentHp);
				engine.Player.CurrentHp += heal;
				GD.Print($"[Relic] Burning Blood healed {heal} HP");
			}
		};

		public static StsRelicData CreateAnchor() => new()
		{
			Id = "Anchor",
			Name = "锚",
			Description = "战斗开始时获得 10 点格挡。",
			Rarity = StsRelicRarity.Common,
			OnCombatStart = (engine) =>
			{
				engine.Player.Block += 10;
				GD.Print("[Relic] Anchor granted 10 block");
			}
		};

		public static StsRelicData CreateLantern() => new()
		{
			Id = "Lantern",
			Name = "灯笼",
			Description = "每场战斗的第 1 回合获得 1 点能量。",
			Rarity = StsRelicRarity.Common,
			OnCombatStart = (engine) =>
			{
				engine.Player.Energy += 1;
				GD.Print("[Relic] Lantern granted +1 energy");
			}
		};

		public static StsRelicData CreateBagOfPreparation() => new()
		{
			Id = "Bag_of_Preparation",
			Name = "备战袋",
			Description = "每场战斗的前 2 回合多抽 2 张牌。",
			Rarity = StsRelicRarity.Common,
			OnTurnStart = (engine) =>
			{
				if (engine.TurnNumber <= 2)
				{
					engine.DrawCards(2);
					GD.Print("[Relic] Bag of Preparation drew 2 extra cards");
				}
			}
		};

		public static StsRelicData CreateVajra() => new()
		{
			Id = "Vajra",
			Name = "金刚杵",
			Description = "战斗开始时获得 1 点力量。",
			Rarity = StsRelicRarity.Common,
			OnCombatStart = (engine) =>
			{
				engine.Player.Strength += 1;
				GD.Print("[Relic] Vajra granted +1 Strength");
			}
		};

		public static StsRelicData CreateOddMushroom() => new()
		{
			Id = "Odd_Mushroom",
			Name = "奇怪蘑菇",
			Description = "受到的脆弱效果降低 25%（50% → 25%）。",
			Rarity = StsRelicRarity.Uncommon,
			OnDamageTaken = (engine, damage) =>
			{
				GD.Print("[Relic] Odd Mushroom reduces Vulnerable damage");
			}
		};

		public static StsRelicData CreateShuriken() => new()
		{
			Id = "Shuriken",
			Name = "手里剑",
			Description = "每打出 3 张攻击牌，获得 1 点力量。",
			Rarity = StsRelicRarity.Uncommon,
			OnCardPlayed = (engine, card) =>
			{
				if (card.Type == StsCardType.Attack)
				{
					GD.Print("[Relic] Shuriken tracks attack cards");
				}
			}
		};

		public static StsRelicData CreateOrichalcum() => new()
		{
			Id = "Orichalcum",
			Name = "山铜",
			Description = "回合结束时如果格挡为 0，获得 6 点格挡。",
			Rarity = StsRelicRarity.Uncommon,
			OnTurnEnd = (engine) =>
			{
				if (engine.Player.Block == 0)
				{
					engine.Player.Block = 6;
					GD.Print("[Relic] Orichalcum granted 6 block");
				}
			}
		};

		public static StsRelicData CreateDeadBranch() => new()
		{
			Id = "Dead_Branch",
			Name = "枯枝",
			Description = "每消耗一张牌，随机增加一张牌到手牌。",
			Rarity = StsRelicRarity.Rare,
			OnCardPlayed = (engine, card) =>
			{
				if (card.Exhaust)
				{
					GD.Print("[Relic] Dead Branch adds random card on Exhaust");
				}
			}
		};

		public static StsRelicData CreateRunicCube() => new()
		{
			Id = "Runic_Cube",
			Name = "符文立方体",
			Description = "受到伤害时抽 1 张牌。",
			Rarity = StsRelicRarity.Rare,
			OnDamageTaken = (engine, damage) =>
			{
				if (damage > 0)
				{
					engine.DrawCards(1);
					GD.Print("[Relic] Runic Cube drew 1 card on damage");
				}
			}
		};
	}

	public class StsRelicManager
	{
		private static readonly Dictionary<string, StsRelicData> AllRelics = new();
		private readonly List<StsRelicData> _ownedRelics = new();

		static StsRelicManager()
		{
			RegisterCoreRelics();
		}

		private static void RegisterCoreRelics()
		{
			Register(StsRelicData.CreateBurningBlood());
			Register(StsRelicData.CreateAnchor());
			Register(StsRelicData.CreateLantern());
			Register(StsRelicData.CreateBagOfPreparation());
			Register(StsRelicData.CreateVajra());
			Register(StsRelicData.CreateOddMushroom());
			Register(StsRelicData.CreateShuriken());
			Register(StsRelicData.CreateOrichalcum());
			Register(StsRelicData.CreateDeadBranch());
			Register(StsRelicData.CreateRunicCube());
		}

		private static void Register(StsRelicData relic)
		{
			AllRelics[relic.Id] = relic;
		}

		public static StsRelicData GetRelic(string id)
		{
			return AllRelics.TryGetValue(id, out var relic) ? relic : null;
		}

		public void AddRelic(string id)
		{
			var relic = GetRelic(id);
			if (relic != null && !_ownedRelics.Contains(relic))
			{
				_ownedRelics.Add(relic);
				GD.Print($"[RelicManager] Acquired: {relic.Name}");
			}
		}

		public void RemoveRelic(string id)
		{
			var relic = GetRelic(id);
			if (relic != null)
			{
				_ownedRelics.Remove(relic);
				GD.Print($"[RelicManager] Lost: {relic.Name}");
			}
		}

		public IReadOnlyList<StsRelicData> GetOwnedRelics() => _ownedRelics.AsReadOnly();

		public void TriggerOnCombatStart(StsCombatEngine engine)
		{
			foreach (var relic in _ownedRelics)
				relic.OnCombatStart?.Invoke(engine);
		}

		public void TriggerOnCombatEnd(StsCombatEngine engine)
		{
			foreach (var relic in _ownedRelics)
				relic.OnCombatEnd?.Invoke(engine);
		}

		public void TriggerOnTurnStart(StsCombatEngine engine)
		{
			foreach (var relic in _ownedRelics)
				relic.OnTurnStart?.Invoke(engine);
		}

		public void TriggerOnTurnEnd(StsCombatEngine engine)
		{
			foreach (var relic in _ownedRelics)
				relic.OnTurnEnd?.Invoke(engine);
		}

		public void TriggerOnCardPlayed(StsCombatEngine engine, StsCardData card)
		{
			foreach (var relic in _ownedRelics)
				relic.OnCardPlayed?.Invoke(engine, card);
		}

		public void TriggerOnDamageDealt(StsCombatEngine engine, int damage)
		{
			foreach (var relic in _ownedRelics)
				relic.OnDamageDealt?.Invoke(engine, damage);
		}

		public void TriggerOnDamageTaken(StsCombatEngine engine, int damage)
		{
			foreach (var relic in _ownedRelics)
				relic.OnDamageTaken?.Invoke(engine, damage);
		}

		public void TriggerOnKill(StsCombatEngine engine)
		{
			foreach (var relic in _ownedRelics)
				relic.OnKill?.Invoke(engine);
		}

		public static List<StsRelicData> GetRandomRelics(int count, System.Random rng, StsRelicRarity? rarity = null)
		{
			var pool = new List<StsRelicData>();
			foreach (var relic in AllRelics.Values)
			{
				if (rarity == null || relic.Rarity == rarity)
					pool.Add(relic);
			}

			var result = new List<StsRelicData>();
			for (int i = 0; i < count && pool.Count > 0; i++)
			{
				int index = rng.Next(pool.Count);
				result.Add(pool[index]);
				pool.RemoveAt(index);
			}

			return result;
		}
	}
}