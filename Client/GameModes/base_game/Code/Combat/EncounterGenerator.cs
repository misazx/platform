using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using RoguelikeGame.Generation;

namespace RoguelikeGame.Core
{
	public static class EncounterGenerator
	{
		public class ActConfig
		{
			public string Name;
			public int Floors;
			public string[] NormalPool;
			public string[] ElitePool;
			public string Boss;
			public float HpMultiplier;
			public float DmgMultiplier;
		}

		private static readonly ActConfig[] _acts = new[]
		{
			new ActConfig
			{
				Name = "第一幕：深渊之门",
				Floors = 3,
				NormalPool = new[] { "Cultist", "JawWorm", "Louse", "FungiBeast" },
				ElitePool = new[] { "Gremlin_Nob", "Lagavulin" },
				Boss = "The_Guardian",
				HpMultiplier = 1.0f,
				DmgMultiplier = 1.0f
			},
			new ActConfig
			{
				Name = "第二幕：暗影走廊",
				Floors = 3,
				NormalPool = new[] { "Slaver", "FungiBeast", "Sentry", "ShelledParasite", "AcidSlime_L" },
				ElitePool = new[] { "Gremlin_Nob", "Lagavulin", "Sentry" },
				Boss = "The_Collector",
				HpMultiplier = 1.2f,
				DmgMultiplier = 1.15f
			},
			new ActConfig
			{
				Name = "第三幕：熔炉之心",
				Floors = 3,
				NormalPool = new[] { "RedSlaver", "TaskMaster", "Sentry", "AcidSlime_L", "SpikeSlime_L", "ShelledParasite" },
				ElitePool = new[] { "Lagavulin", "Sentry" },
				Boss = "The_Automaton",
				HpMultiplier = 1.45f,
				DmgMultiplier = 1.3f
			},
			new ActConfig
			{
				Name = "第四幕：尖塔之巅",
				Floors = 2,
				NormalPool = new[] { "RedSlaver", "TaskMaster", "SpikeSlime_L", "FungusLing" },
				ElitePool = new[] { "Gremlin_Nob", "Lagavulin" },
				Boss = "The_Awakener",
				HpMultiplier = 1.7f,
				DmgMultiplier = 1.5f
			}
		};

		private static readonly Dictionary<string, (int baseHp, int minDmg, int maxDmg)> _enemyStats = new()
		{
			{ "Cultist", (48, 6, 9) },
			{ "JawWorm", (55, 9, 12) },
			{ "Louse", (32, 5, 7) },
			{ "Slaver", (50, 10, 14) },
			{ "RedSlaver", (56, 12, 16) },
			{ "FungiBeast", (44, 7, 11) },
			{ "Gremlin_Nob", (82, 14, 18) },
			{ "Lagavulin", (109, 16, 22) },
			{ "Sentry", (48, 9, 13) },
			{ "ShelledParasite", (44, 7, 10) },
			{ "AcidSlime_L", (38, 5, 8) },
			{ "AcidSlime_M", (25, 4, 6) },
			{ "AcidSlime_S", (14, 3, 4) },
			{ "SpikeSlime_L", (40, 6, 9) },
			{ "SpikeSlime_M", (28, 4, 7) },
			{ "SpikeSlime_S", (16, 3, 5) },
			{ "TaskMaster", (58, 11, 15) },
			{ "FungusLing", (32, 6, 9) },
			{ "The_Guardian", (240, 16, 22) },
			{ "Hexaghost", (260, 18, 24) },
			{ "The_Collector", (200, 14, 20) },
			{ "The_Automaton", (190, 16, 22) },
			{ "Donu_and_Deca", (160, 15, 20) },
			{ "The_Awakener", (320, 20, 28) },
			{ "Slime_Boss", (150, 12, 18) },
		};

		private static readonly Dictionary<string, string[]> _encounterTemplates = new()
		{
			{ "easy_1", new[] { "Cultist" } },
			{ "easy_2", new[] { "JawWorm" } },
			{ "easy_3", new[] { "Louse", "Louse" } },
			{ "medium_1", new[] { "Cultist", "Louse" } },
			{ "medium_2", new[] { "JawWorm", "Louse" } },
			{ "medium_3", new[] { "Slaver", "FungiBeast" } },
			{ "medium_4", new[] { "AcidSlime_L", "SpikeSlime_L" } },
			{ "hard_1", new[] { "Slaver", "RedSlaver" } },
			{ "hard_2", new[] { "TaskMaster", "FungusLing", "FungusLing" } },
			{ "hard_3", new[] { "RedSlaver", "ShelledParasite" } },
			{ "elite_gremlin", new[] { "Gremlin_Nob" } },
			{ "elite_lagavulin", new[] { "Lagavulin" } },
			{ "elite_sentries", new[] { "Sentry", "Sentry" } },
			{ "boss_guardian", new[] { "The_Guardian" } },
			{ "boss_collector", new[] { "The_Collector" } },
			{ "boss_automaton", new[] { "The_Automaton" } },
			{ "boss_awakener", new[] { "The_Awakener" } },
			{ "boss_slime", new[] { "Slime_Boss" } },
		};

		public class EncounterResult
		{
			public List<StsEnemyData> Enemies { get; set; } = new();
			public int GoldReward { get; set; }
			public string Description { get; set; }
			public string ActName { get; set; }
		}

		public static int GetActForFloor(int floor) => floor switch
		{
			<= 3 => 0,
			<= 6 => 1,
			<= 9 => 2,
			_ => 3
		};

		public static ActConfig GetCurrentAct(int floor) => _acts[GetActForFloor(floor)];

		public static int GetFloorInAct(int floor) => ((floor - 1) % 3) + 1;

		public static bool IsBossFloor(int floor)
		{
			int actIdx = GetActForFloor(floor);
			int actStart = actIdx * 3 + 1;
			int actEnd = actStart + _acts[actIdx].Floors - 1;
			return floor == actEnd;
		}

		public static EncounterResult GenerateEncounter(string enemyEncounterId, NodeType nodeType, int floorNumber)
		{
			var rng = new Random();
			var act = GetCurrentAct(floorNumber);
			int floorInAct = GetFloorInAct(floorNumber);

			GD.Print($"[EncounterGenerator] Act {actIdx(GetActForFloor(floorNumber))}: {act.Name}, Floor {floorInAct}/{act.Floors}, Type={nodeType}");

			float hpScale = act.HpMultiplier * (1.0f + (floorInAct - 1) * 0.1f);
			float dmgScale = act.DmgMultiplier * (1.0f + (floorInAct - 1) * 0.06f);

			switch (nodeType)
			{
				case NodeType.Boss:
					return GenerateBossEncounter(act, floorInAct, hpScale, dmgScale, rng);
				case NodeType.Elite:
					return GenerateEliteEncounter(act, floorInAct, hpScale, dmgScale, rng);
				case NodeType.Monster:
				default:
					return GenerateNormalEncounter(enemyEncounterId, act, floorInAct, hpScale, dmgScale, rng);
			}
		}

		private static int actIdx(int i) => i;

		private static EncounterResult GenerateBossEncounter(ActConfig act, int floorInAct, float hpScale, float dmgScale, Random rng)
		{
			string bossId = act.Boss;
			if (!_enemyStats.TryGetValue(bossId, out var stats))
				stats = (240, 16, 22);

			int hp = (int)(stats.baseHp * hpScale);
			int dmg = (int)(stats.minDmg * dmgScale);

			var boss = new StsEnemyData
			{
				Id = bossId,
				Name = bossId.Replace("_", " "),
				MaxHp = hp,
				CurrentHp = hp,
				Block = 0,
				CurrentIntent = StsEnemyIntent.CreateAttack(dmg)
			};

			return new EncounterResult
			{
				Enemies = new List<StsEnemyData> { boss },
				GoldReward = 80 + floorInAct * 20 + rng.Next(0, 50),
				Description = $"Boss: {boss.Name} (HP:{hp})",
				ActName = act.Name
			};
		}

		private static EncounterResult GenerateEliteEncounter(ActConfig act, int floorInAct, float hpScale, float dmgScale, Random rng)
		{
			string templateKey = rng.Next(3) switch
			{
				0 => "elite_gremlin",
				1 => "elite_lagavulin",
				_ => "elite_sentries"
			};

			if (!_encounterTemplates.TryGetValue(templateKey, out var template))
				template = new[] { act.ElitePool[rng.Next(act.ElitePool.Length)] };

			var enemies = new List<StsEnemyData>();
			for (int i = 0; i < template.Length; i++)
			{
				string eId = template[i];
				if (!_enemyStats.TryGetValue(eId, out var stats))
					stats = (82, 14, 18);

				int hp = (int)(stats.baseHp * hpScale);
				int dmg = (int)(stats.minDmg * dmgScale);

				enemies.Add(new StsEnemyData
				{
					Id = eId + (template.Length > 1 ? $"_{i + 1}" : ""),
					Name = eId.Replace("_", " ") + (template.Length > 1 ? $" #{i + 1}" : ""),
					MaxHp = hp,
					CurrentHp = hp,
					Block = 0,
					CurrentIntent = StsEnemyIntent.CreateAttack(dmg)
				});
			}

			return new EncounterResult
			{
				Enemies = enemies,
				GoldReward = 35 + floorInAct * 12 + rng.Next(0, 30),
				Description = $"Elite: {string.Join(", ", enemies.Select(e => e.Name))}",
				ActName = act.Name
			};
		}

		private static EncounterResult GenerateNormalEncounter(string enemyId, ActConfig act, int floorInAct, float hpScale, float dmgScale, Random rng)
		{
			string[] templateKeys = floorInAct switch
			{
				1 => new[] { "easy_1", "easy_2", "easy_3", "medium_1" },
				2 => new[] { "medium_1", "medium_2", "medium_3", "medium_4" },
				_ => new[] { "medium_3", "medium_4", "hard_1", "hard_2", "hard_3" }
			};

			string templateKey = templateKeys[rng.Next(templateKeys.Length)];

			if (!_encounterTemplates.TryGetValue(templateKey, out var template))
			{
				int count = floorInAct <= 1 ? rng.Next(1, 3) : rng.Next(1, 4);
				template = new string[count];
				for (int i = 0; i < count; i++)
					template[i] = act.NormalPool[rng.Next(act.NormalPool.Length)];
			}

			var enemies = new List<StsEnemyData>();
			for (int i = 0; i < template.Length; i++)
			{
				string eId = enemyId != null && i == 0 ? enemyId : template[i];
				if (!_enemyStats.TryGetValue(eId, out var stats))
					stats = (50, 6, 8);

				int hp = (int)(stats.baseHp * hpScale);
				int dmg = (int)(stats.minDmg * dmgScale);
				bool isLeader = i == 0 && template.Length > 1;

				if (isLeader)
				{
					hp = (int)(hp * 1.25f);
					dmg = (int)(dmg * 1.15f);
				}

				enemies.Add(new StsEnemyData
				{
					Id = eId + (template.Length > 1 ? $"_{i + 1}" : ""),
					Name = eId.Replace("_", " "),
					MaxHp = hp,
					CurrentHp = hp,
					Block = 0,
					CurrentIntent = StsEnemyIntent.CreateAttack(dmg)
				});
			}

			return new EncounterResult
			{
				Enemies = enemies,
				GoldReward = 12 + floorInAct * 5 + rng.Next(0, 18),
				Description = $"{string.Join(", ", enemies.Select(e => e.Name))}",
				ActName = act.Name
			};
		}

		public static int GetBaseHealth(int floor) => 72 + (floor - 1) * 6;
		public static int GetBaseEnergy(int floor) => 3;
		public static int GetCardRewardCount(int floor) => 3;
		public static int GetTotalFloors() => _acts.Sum(a => a.Floors);
		public static string GetActName(int floor) => GetCurrentAct(floor).Name;
	}
}
