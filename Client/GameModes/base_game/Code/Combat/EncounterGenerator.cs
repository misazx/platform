using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using RoguelikeGame.Generation;

namespace RoguelikeGame.Core
{
	public static class EncounterGenerator
	{
		private static readonly Dictionary<string, (int baseHp, int minDmg, int maxDmg)> _enemyStats = new()
		{
			{ "Cultist", (50, 6, 8) },
			{ "JawWorm", (60, 9, 12) },
			{ "Louse", (35, 5, 7) },
			{ "Slaver", (52, 10, 13) },
			{ "FungiBeast", (44, 7, 10) },
			{ "Gremlin_Nob", (82, 14, 18) },
			{ "Lagavulin", (109, 18, 24) },
			{ "The_Guardian", (200, 20, 28) },
			{ "Hexaghost", (250, 22, 30) },
			{ "The_Collector", (180, 16, 22) },
			{ "The_Automaton", (175, 15, 21) },
			{ "Donu_and_Deca", (150, 17, 23) },
			{ "The_Awakener", (300, 25, 35) },
			{ "Sentry", (48, 11, 14) },
			{ "ShelledParasite", (42, 6, 9) },
			{ "AcidSlime_L", (35, 4, 6) },
			{ "AcidSlime_M", (25, 3, 5) },
			{ "AcidSlime_S", (14, 2, 3) },
			{ "SpikeSlime_L", (38, 5, 8) },
			{ "SpikeSlime_M", (26, 4, 6) },
			{ "SpikeSlime_S", (15, 3, 4) },
			{ "RedSlaver", (56, 12, 15) },
			{ "TaskMaster", (58, 13, 16) },
			{ "FungusLing", (32, 6, 8) },
		};

		public class EncounterResult
		{
			public List<StsEnemyData> Enemies { get; set; } = new();
			public int GoldReward { get; set; }
			public string Description { get; set; }
		}

		public static EncounterResult GenerateEncounter(string enemyEncounterId, NodeType nodeType, int floorNumber)
		{
			var rng = new Random();
			GD.Print($"[EncounterGenerator] Generating: type={nodeType} floor={floorNumber} id={enemyEncounterId}");

			float hpScale = GetHpScale(floorNumber);
			float dmgScale = GetDamageScale(floorNumber);

			switch (nodeType)
			{
				case NodeType.Boss:
					return GenerateBossEncounter(enemyEncounterId, floorNumber, hpScale, dmgScale, rng);
				case NodeType.Elite:
					return GenerateEliteEncounter(enemyEncounterId, floorNumber, hpScale, dmgScale, rng);
				case NodeType.Monster:
				default:
					return GenerateNormalEncounter(enemyEncounterId, floorNumber, hpScale, dmgScale, rng);
			}
		}

		private static EncounterResult GenerateBossEncounter(string enemyId, int floor, float hpScale, float dmgScale, Random rng)
		{
			string bossId = enemyId ?? GetBossForFloor(floor);
			if (!_enemyStats.TryGetValue(bossId, out var stats))
				stats = (200, 20, 28);

			int hp = Math.Max((int)(stats.baseHp * hpScale), stats.baseHp);
			int dmg = Math.Max((int)(stats.minDmg * dmgScale), stats.minDmg);

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
				GoldReward = 80 + floor * 15 + rng.Next(0, 40),
				Description = $"Boss: {boss.Name} (HP:{hp})"
			};
		}

		private static EncounterResult GenerateEliteEncounter(string enemyId, int floor, float hpScale, float dmgScale, Random rng)
		{
			string eliteId = enemyId ?? PickElite(floor, rng);
			if (!_enemyStats.TryGetValue(eliteId, out var stats))
				stats = (82, 14, 18);

			int hp = Math.Max((int)(stats.baseHp * hpScale), stats.baseHp);
			int dmg = Math.Max((int)(stats.minDmg * dmgScale), stats.minDmg);

			int eliteCount = floor >= 3 && rng.NextDouble() < 0.3f ? 2 : 1;
			var enemies = new List<StsEnemyData>();

			for (int i = 0; i < eliteCount; i++)
			{
				enemies.Add(new StsEnemyData
				{
					Id = eliteId + (eliteCount > 1 ? $"_{i+1}" : ""),
					Name = eliteId.Replace("_", " ") + (eliteCount > 1 ? $" #{i+1}" : ""),
					MaxHp = hp,
					CurrentHp = hp,
					Block = 0,
					CurrentIntent = StsEnemyIntent.CreateAttack(dmg)
				});
			}

			return new EncounterResult
			{
				Enemies = enemies,
				GoldReward = 35 + floor * 8 + rng.Next(0, 25),
				Description = $"Elite: {enemies[0].Name}{(eliteCount > 1 ? $" x{eliteCount}" : "")}"
			};
		}

		private static EncounterResult GenerateNormalEncounter(string enemyId, int floor, float hpScale, float dmgScale, Random rng)
		{
			int enemyCount = GetEnemyCountForFloor(floor, rng);
			var normalPool = _normalPools[floor % _normalPools.Length];
			var enemies = new List<StsEnemyData>();

			for (int i = 0; i < enemyCount; i++)
			{
				string eId = enemyId != null && i == 0 ? enemyId : normalPool[rng.Next(normalPool.Length)];
				if (!_enemyStats.TryGetValue(eId, out var stats))
					stats = (50, 6, 8);

				int hp = Math.Max((int)(stats.baseHp * hpScale), stats.baseHp);
				int dmg = Math.Max((int)(stats.minDmg * dmgScale), stats.minDmg);
				bool isLeader = i == 0 && enemyCount > 1;

				if (isLeader)
				{
					hp = (int)(hp * 1.3f);
					dmg = (int)(dmg * 1.2f);
				}

				enemies.Add(new StsEnemyData
				{
					Id = eId + (enemyCount > 1 ? $"_{i+1}" : ""),
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
				GoldReward = 15 + floor * 3 + rng.Next(0, 15),
				Description = $"{enemyCount}x Normal: {string.Join(", ", enemies.Select(e => e.Name))}"
			};
		}

		private static readonly string[][] _normalPools =
		{
			new[] { "Cultist", "JawWorm", "Louse" },
			new[] { "Cultist", "JawWorm", "Louse", "Slaver", "FungiBeast" },
			new[] { "JawWorm", "Slaver", "FungiBeast", "Sentry", "ShelledParasite" },
			new[] { "Slaver", "FungiBeast", "Sentry", "ShelledParasite", "AcidSlime_L" },
			new[] { "Slaver", "TaskMaster", "Sentry", "AcidSlime_L", "SpikeSlime_L" },
		};

		private static readonly string[] _elitePool = { "Gremlin_Nob", "Lagavulin" };

		private static string GetBossForFloor(int floor) => floor switch
		{
			1 => "The_Guardian",
			2 => "The_Collector",
			3 => "The_Automaton",
			_ => "The_Awakener"
		};

		private static string PickElite(int floor, Random rng) => _elitePool[rng.Next(_elitePool.Length)];

		private static int GetEnemyCountForFloor(int floor, Random rng) => floor switch
		{
			<= 1 => rng.Next(1, 3),
			<= 2 => rng.Next(1, 3),
			_ => rng.Next(1, 4)
		};

		private static float GetHpScale(int floor) => 1.0f + (floor - 1) * 0.08f;
		private static float GetDamageScale(int floor) => 1.0f + (floor - 1) * 0.05f;

		public static int GetBaseHealth(int floor) => 72 + (floor - 1) * 6;
		public static int GetBaseEnergy(int floor) => 3;
		public static int GetCardRewardCount(int floor) => 3;
	}
}
