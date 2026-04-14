using Godot;
using RoguelikeGame.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeGame.Core
{
	public enum StsCardType
	{
		Attack,
		Skill,
		Power,
		Status,
		Curse
	}

	public enum StsCardRarity
	{
		Basic,
		Common,
		Uncommon,
		Rare,
		Special
	}

	public enum StsCardTarget
	{
		EnemySingle,
		EnemyAll,
		Self,
		All,
		None
	}

	public enum StsDamageType
	{
		Normal,
		Attack, // 受力量加成
		Thorns  // 反伤
	}

	public class StsCardData
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int Cost { get; set; } = 1;
		public StsCardType Type { get; set; } = StsCardType.Attack;
		public StsCardRarity Rarity { get; set; } = StsCardRarity.Common;
		public StsCardTarget Target { get; set; } = StsCardTarget.EnemySingle;
		public int Damage { get; set; } = 0;
		public int Block { get; set; } = 0;
		public int MagicNumber { get; set; } = 0;
		public bool Ethereal { get; set; } = false;
		public bool Exhaust { get; set; } = false;
		public bool Innate { get; set; } = false;
		public bool Retain { get; set; } = false;
		public StsDamageType DamageType { get; set; } = StsDamageType.Normal;
		public List<string> Keywords { get; set; } = new();

		public static StsCardData CreateStrike() => new()
		{
			Id = "Strike_R",
			Name = "打击",
			Description = "造成 6 点伤害。",
			Cost = 1,
			Type = StsCardType.Attack,
			Rarity = StsCardRarity.Basic,
			Damage = 6,
			DamageType = StsDamageType.Attack,
			Keywords = new() { "攻击" }
		};

		public static StsCardData CreateDefend() => new()
		{
			Id = "Defend_R",
			Name = "防御",
			Description = "获得 5 点格挡。",
			Cost = 1,
			Type = StsCardType.Skill,
			Rarity = StsCardRarity.Basic,
			Block = 5,
			Keywords = new() { "技能", "格挡" }
		};

		public static StsCardData CreateBash() => new()
		{
			Id = "Bash",
			Name = "痛击",
			Description = "造成 8 点伤害。\n施加 2 层脆弱。",
			Cost = 2,
			Type = StsCardType.Attack,
			Rarity = StsCardRarity.Common,
			Damage = 8,
			MagicNumber = 2,
			DamageType = StsDamageType.Attack,
			Keywords = new() { "攻击", "脆弱" }
		};
	}

	public class StsStatusEffect
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int Stacks { get; set; }
		public int Duration { get; set; }
		public bool IsBuff { get; set; }

		public static StsStatusEffect CreateWeak(int stacks) => new()
		{
			Id = "weak",
			Name = "虚弱",
			Description = "攻击伤害降低25%",
			Stacks = stacks,
			Duration = -1,
			IsBuff = false
		};

		public static StsStatusEffect CreateVulnerable(int stacks) => new()
		{
			Id = "vulnerable",
			Name = "脆弱",
			Description = "受到的伤害提升50%",
			Stacks = stacks,
			Duration = -1,
			IsBuff = false
		};

		public static StsStatusEffect CreateStrength(int stacks) => new()
		{
			Id = "strength",
			Name = "力量",
			Description = $"每张攻击牌伤害+{stacks}",
			Stacks = stacks,
			Duration = -1,
			IsBuff = true
		};

		public static StsStatusEffect CreateDexterity(int stacks) => new()
		{
			Id = "dexterity",
			Name = "敏捷",
			Description = $"每张防御牌格挡+{stacks}",
			Stacks = stacks,
			Duration = -1,
			IsBuff = true
		};

		public static StsStatusEffect CreatePoison(int stacks) => new()
		{
			Id = "poison",
			Name = "中毒",
			Description = $"回合结束时受到{stacks}点伤害",
			Stacks = stacks,
			Duration = -1,
			IsBuff = false
		};
	}

	public class StsEnemyData
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public int MaxHp { get; set; }
		public int CurrentHp { get; set; }
		public int Block { get; set; } = 0;
		public List<StsStatusEffect> StatusEffects { get; set; } = new();
		public StsEnemyIntent CurrentIntent { get; set; }

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

		public void AddStatus(StsStatusEffect effect)
		{
			var existing = StatusEffects.Find(s => s.Id == effect.Id);
			if (existing != null)
			{
				existing.Stacks += effect.Stacks;
			}
			else
			{
				StatusEffects.Add(effect);
			}
		}

		public int GetStatusStacks(string statusId)
		{
			var effect = StatusEffects.Find(s => s.Id == statusId);
			return effect?.Stacks ?? 0;
		}

		public bool HasStatus(string statusId) => GetStatusStacks(statusId) > 0;

		public bool IsDead => CurrentHp <= 0;
	}

	public class StsEnemyIntent
	{
		public enum IntentType
		{
			Attack,
			AttackDebuff,
			AttackBuff,
			Defend,
			DefendBuff,
			Buff,
			Debuff,
			StrongDebuff,
			Sleep,
			Magic,
			Escape,
			Unknown
		}

		public IntentType Type { get; set; }
		public int Value { get; set; }
		public int Value2 { get; set; }
		public string Description { get; set; }
		public string Icon { get; set; }

		public static StsEnemyIntent CreateAttack(int damage) => new()
		{
			Type = IntentType.Attack,
			Value = damage,
			Description = $"准备攻击 {damage}",
			Icon = "⚔"
		};

		public static StsEnemyIntent CreateDefend(int block) => new()
		{
			Type = IntentType.Defend,
			Value = block,
			Description = $"获得 {block} 格挡",
			Icon = "🛡"
		};

		public static StsEnemyIntent CreateBuff(string buffName, int stacks) => new()
		{
			Type = IntentType.Buff,
			Value = stacks,
			Description = $"获得 {buffName} +{stacks}",
			Icon = "⬆"
		};

		public static StsEnemyIntent CreateDebuff(string debuffName, int stacks) => new()
		{
			Type = IntentType.Debuff,
			Value = stacks,
			Description = $"施加 {debuffName} {stacks}层",
			Icon = "↓"
		};
	}

	public class StsPlayerState
	{
		public int MaxHp { get; set; } = 80;
		public int CurrentHp { get; set; } = 80;
		public int Gold { get; set; } = 99;
		public int Energy { get; set; } = 3;
		public int MaxEnergy { get; set; } = 3;
		public int Block { get; set; } = 0;
		public int Strength { get; set; } = 0;
		public int Dexterity { get; set; } = 0;
		public List<StsStatusEffect> StatusEffects { get; set; } = new();
		public List<StsCardData> Hand { get; set; } = new();
		public List<StsCardData> DrawPile { get; set; } = new();
		public List<StsCardData> DiscardPile { get; set; } = new();
		public List<StsCardData> ExhaustPile { get; set; } = new();

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

		public void AddBlock(int block)
		{
			Block += block;
		}

		public void AddStatus(StsStatusEffect effect)
		{
			var existing = StatusEffects.Find(s => s.Id == effect.Id);
			if (existing != null)
			{
				existing.Stacks += effect.Stacks;
			}
			else
			{
				StatusEffects.Add(effect);
			}
		}

		public int GetStatusStacks(string statusId)
		{
			var effect = StatusEffects.Find(s => s.Id == statusId);
			return effect?.Stacks ?? 0;
		}

		public bool HasStatus(string statusId)
		{
			return GetStatusStacks(statusId) > 0;
		}

		public bool IsDead => CurrentHp <= 0;
	}

	public partial class StsCombatEngine : SingletonBase<StsCombatEngine>
	{
		private StsPlayerState _player;
		private List<StsEnemyData> _enemies = new();
		private RandomNumberGenerator _rng = new();
		private int _turnNumber = 0;
		private StsRelicManager _relicManager;
		public event Action<int, string, int> OnDamageDealt;
		public event Action<int, int> OnBlockGained;
		public event Action<string, StsStatusEffect> OnStatusApplied;
		public event Action<int> OnTurnStarted;
		public event Action<int> OnTurnEnded;
		public event Action OnCombatWon;
		public event Action OnCombatLost;

		public StsPlayerState Player => _player;
		public IReadOnlyList<StsEnemyData> Enemies => _enemies.AsReadOnly();
		public int TurnNumber => _turnNumber;
		public bool IsPlayerTurn { get; private set; } = true;
		public bool IsCombatOver { get; private set; } = false;
		public StsRelicManager RelicManager => _relicManager;

		protected override void OnInitialize()
		{
		}

		public void InitializeCombat(List<StsEnemyData> enemies, uint seed)
		{
			_rng.Seed = seed;
			_enemies = enemies ?? new List<StsEnemyData>();
			_player = new StsPlayerState();
			_turnNumber = 0;
			_relicManager = new StsRelicManager();

			InitializeDeck();
			StartNewTurn();

			_relicManager?.TriggerOnCombatStart(this);

			GD.Print($"[StsCombatEngine] Combat initialized with {_enemies.Count} enemies");
		}

		private void InitializeDeck()
		{
			_player.DrawPile.Clear();
			_player.DiscardPile.Clear();
			_player.ExhaustPile.Clear();

			for (int i = 0; i < 5; i++)
				_player.DrawPile.Add(StsCardData.CreateStrike());

			for (int i = 0; i < 5; i++)
				_player.DrawPile.Add(StsCardData.CreateDefend());

			ShuffleDrawPile();
		}

		private void ShuffleDrawPile()
		{
			for (int i = _player.DrawPile.Count - 1; i > 0; i--)
			{
				int j = _rng.RandiRange(0, i);
				(_player.DrawPile[i], _player.DrawPile[j]) = (_player.DrawPile[j], _player.DrawPile[i]);
			}
		}

		public void StartNewTurn()
		{
			IsPlayerTurn = true;
			_turnNumber++;
			_player.Energy = _player.MaxEnergy;
			_player.Block = 0;

			ProcessEndTurnStatusEffects(_player);

			DrawCards(5);

			foreach (var enemy in _enemies)
			{
				if (!enemy.IsDead)
					GenerateEnemyIntent(enemy);
			}

			_relicManager?.TriggerOnTurnStart(this);
			OnTurnStarted?.Invoke(_turnNumber);
		}

		private void ProcessEndTurnStatusEffects(StsPlayerState unit)
		{
			foreach (var effect in unit.StatusEffects.ToList())
			{
				if (effect.Name == "Poison" && effect.Stacks > 0)
				{
					int poisonDmg = effect.Stacks;
					unit.ApplyDamage(poisonDmg);
					effect.Stacks = System.Math.Max(0, effect.Stacks - 1);
					OnDamageDealt?.Invoke(-1, "Player", poisonDmg);
					_relicManager?.TriggerOnDamageTaken(this, poisonDmg);
					GD.Print($"[StsCombatEngine] Poison deals {poisonDmg} to player");
				}
				if (effect.Name == "Metallicize" && effect.Stacks > 0)
				{
					unit.AddBlock(effect.Stacks);
					OnBlockGained?.Invoke(effect.Stacks, unit.Block);
					GD.Print($"[StsCombatEngine] Metallicize gives {effect.Stacks} block");
				}
				if (effect.Name == "DemonForm" && effect.Stacks > 0)
				{
					var str = StsStatusEffect.CreateStrength(effect.Stacks);
					unit.AddStatus(str);
					GD.Print($"[StsCombatEngine] Demon Form gives +{effect.Stacks} strength");
				}
				if (effect.Name == "Brutality" && effect.Stacks > 0)
				{
					unit.ApplyDamage(effect.Stacks);
					OnDamageDealt?.Invoke(-1, "Player", effect.Stacks);
					DrawCards(1);
					GD.Print($"[StsCombatEngine] Brutality: lose {effect.Stacks} HP, draw 1");
				}
				effect.Duration--;
			}
			unit.StatusEffects.RemoveAll(effect => effect.Duration == 0 || (effect.Name == "Poison" && effect.Stacks <= 0));
		}

		private void ProcessEndTurnEnemyStatusEffects(StsEnemyData enemy)
		{
			if (enemy.IsDead) return;

			foreach (var effect in enemy.StatusEffects.ToList())
			{
				if (effect.Name == "Poison" && effect.Stacks > 0)
				{
					int poisonDmg = effect.Stacks;
					enemy.ApplyDamage(poisonDmg);
					effect.Stacks = System.Math.Max(0, effect.Stacks - 1);
					int idx = _enemies.IndexOf(enemy);
					OnDamageDealt?.Invoke(idx, enemy.Name, poisonDmg);
					GD.Print($"[StsCombatEngine] Poison deals {poisonDmg} to {enemy.Name}");
				}
				if (effect.Name == "Vulnerable" || effect.Name == "Weak")
				{
					effect.Stacks = System.Math.Max(0, effect.Stacks - 1);
				}
				effect.Duration--;
			}
			enemy.StatusEffects.RemoveAll(effect => effect.Duration == 0 || (effect.Name == "Poison" && effect.Stacks <= 0));
		}

		public void DrawCards(int count)
		{
			for (int i = 0; i < count; i++)
			{
				if (_player.DrawPile.Count == 0)
				{
					RefillDrawPileFromDiscard();
				}

				if (_player.DrawPile.Count > 0)
				{
					var card = _player.DrawPile[^1];
					_player.DrawPile.RemoveAt(_player.DrawPile.Count - 1);
					_player.Hand.Add(card);
				}
			}

			GD.Print($"[StsCombatEngine] Drew {count} cards | Hand: {_player.Hand.Count}");
		}

		private void RefillDrawPileFromDiscard()
		{
			_player.DrawPile.AddRange(_player.DiscardPile);
			_player.DiscardPile.Clear();
			ShuffleDrawPile();
			GD.Print("[StsCombatEngine] Reshuffled discard pile into draw pile");
		}

		public bool CanPlayCard(StsCardData card)
		{
			if (IsCombatOver) return false;
			if (card == null) return false;
			if (_player.Energy < card.Cost) return false;
			if (!HasValidTarget(card)) return false;
			return true;
		}

		public bool HasValidTarget(StsCardData card)
		{
			switch (card.Target)
			{
				case StsCardTarget.None:
				case StsCardTarget.Self:
				case StsCardTarget.All:
					return true;
				case StsCardTarget.EnemySingle:
				case StsCardTarget.EnemyAll:
					return _enemies.Exists(e => !e.IsDead);
				default:
					return true;
			}
		}

		public StsPlayResult PlayCard(StsCardData card, int targetIndex = -1)
		{
			var result = new StsPlayResult();

			if (!CanPlayCard(card))
			{
				result.Success = false;
				result.Reason = "无法出牌";
				return result;
			}

			_player.Energy -= card.Cost;
			_player.Hand.Remove(card);

			ExecuteCardEffect(card, targetIndex, result);

			if (card.Exhaust || card.Ethereal)
			{
				_player.ExhaustPile.Add(card);
			}
			else if (!card.Retain)
			{
				_player.DiscardPile.Add(card);
			}

			CheckCombatEnd();

			_relicManager?.TriggerOnCardPlayed(this, card);

			GD.Print($"[StsCombatEngine] Played: {card.Name} | Result: {(result.Success ? "Success" : result.Reason)}");

			return result;
		}

		private void ExecuteCardEffect(StsCardData card, int targetIndex, StsPlayResult result)
		{
			switch (card.Type)
			{
				case StsCardType.Attack:
					ExecuteAttackCard(card, targetIndex, result);
					break;
				case StsCardType.Skill:
					ExecuteSkillCard(card, result);
					break;
				case StsCardType.Power:
					ExecutePowerCard(card, result);
					break;
				default:
					break;
			}
		}

		private void ExecuteAttackCard(StsCardData card, int targetIndex, StsPlayResult result)
		{
			int actualDamage = CalculateDamage(card.Damage, card.DamageType);
			List<StsEnemyData> targets = GetTargets(card.Target, targetIndex);

			foreach (var enemy in targets)
			{
				if (enemy.IsDead) continue;

				enemy.ApplyDamage(actualDamage);
				int enemyIdx = _enemies.IndexOf(enemy);
				OnDamageDealt?.Invoke(enemyIdx, enemy.Name, actualDamage);
				_relicManager?.TriggerOnDamageDealt(this, actualDamage);
				result.DamageDealt += actualDamage;

				if (enemy.IsDead)
				{
					_relicManager?.TriggerOnKill(this);
				}

				if (!enemy.IsDead && card.MagicNumber > 0)
				{
					var effect = GetCardStatusEffect(card);
					if (effect != null)
					{
						enemy.AddStatus(effect);
						OnStatusApplied?.Invoke(enemy.Name, effect);
						result.StatusApplied.Add(effect);
					}
				}
			}

			result.Success = true;
		}

		private StsStatusEffect GetCardStatusEffect(StsCardData card)
		{
			string nameLower = card.Name.ToLower();
			if (nameLower.Contains("虚弱") || nameLower.Contains("weak") || nameLower.Contains("晾衣") || nameLower.Contains("clothesline"))
				return StsStatusEffect.CreateWeak(card.MagicNumber);
			if (nameLower.Contains("脆弱") || nameLower.Contains("vulnerable") || nameLower.Contains("痛击") || nameLower.Contains("bash") || nameLower.Contains("上勾") || nameLower.Contains("uppercut"))
				return StsStatusEffect.CreateVulnerable(card.MagicNumber);
			return StsStatusEffect.CreateVulnerable(card.MagicNumber);
		}

		private void ExecuteSkillCard(StsCardData card, StsPlayResult result)
		{
			if (card.Block > 0)
			{
				int actualBlock = card.Block + _player.Dexterity;
				_player.AddBlock(actualBlock);
				OnBlockGained?.Invoke(actualBlock, _player.Block);
				result.BlockGained = actualBlock;
			}

			string nameLower = card.Name.ToLower();

			if (nameLower.Contains("抽牌") || nameLower.Contains("draw") || nameLower.Contains("耸肩") || nameLower.Contains("shrug") || nameLower.Contains("恍惚") || nameLower.Contains("trance"))
			{
				int drawCount = card.MagicNumber > 0 ? card.MagicNumber : 1;
				DrawCards(drawCount);
			}

			if (nameLower.Contains("放血") || nameLower.Contains("bloodletting"))
			{
				_player.Energy += card.MagicNumber;
			}

			if (nameLower.Contains("挖掘") || nameLower.Contains("entrench"))
			{
				_player.Block *= 2;
				OnBlockGained?.Invoke(_player.Block, _player.Block);
			}

			if (nameLower.Contains("武装") || nameLower.Contains("armaments"))
			{
				// TODO: upgrade cards in hand
			}

			if (nameLower.Contains("屈伸") || nameLower.Contains("flex"))
			{
				var str = StsStatusEffect.CreateStrength(card.MagicNumber);
				_player.AddStatus(str);
				OnStatusApplied?.Invoke("Player", str);
			}

			if (nameLower.Contains("愤怒") || nameLower.Contains("rage"))
			{
				var str = StsStatusEffect.CreateStrength(card.MagicNumber);
				_player.AddStatus(str);
				OnStatusApplied?.Invoke("Player", str);
			}

			result.Success = true;
		}

		private void ExecutePowerCard(StsCardData card, StsPlayResult result)
		{
			string nameLower = card.Name.ToLower();

			if (nameLower.Contains("燃烧") || nameLower.Contains("inflame"))
			{
				var str = StsStatusEffect.CreateStrength(card.MagicNumber);
				_player.AddStatus(str);
				OnStatusApplied?.Invoke("Player", str);
			}
			else if (nameLower.Contains("金属化") || nameLower.Contains("metallicize"))
			{
				var metal = new StsStatusEffect { Name = "Metallicize", Stacks = card.MagicNumber, Duration = -1 };
				_player.AddStatus(metal);
				OnStatusApplied?.Invoke("Player", metal);
			}
			else if (nameLower.Contains("恶魔") || nameLower.Contains("demon"))
			{
				var demon = new StsStatusEffect { Name = "DemonForm", Stacks = card.MagicNumber, Duration = -1 };
				_player.AddStatus(demon);
				OnStatusApplied?.Invoke("Player", demon);
			}
			else if (nameLower.Contains("壁垒") || nameLower.Contains("barricade"))
			{
				var barricade = new StsStatusEffect { Name = "Barricade", Stacks = 1, Duration = -1 };
				_player.AddStatus(barricade);
				OnStatusApplied?.Invoke("Player", barricade);
			}
			else if (nameLower.Contains("残暴") || nameLower.Contains("brutality"))
			{
				var brutal = new StsStatusEffect { Name = "Brutality", Stacks = card.MagicNumber, Duration = -1 };
				_player.AddStatus(brutal);
				OnStatusApplied?.Invoke("Player", brutal);
			}
			else if (nameLower.Contains("无痛") || nameLower.Contains("feel no pain"))
			{
				var fnp = new StsStatusEffect { Name = "FeelNoPain", Stacks = card.MagicNumber, Duration = -1 };
				_player.AddStatus(fnp);
				OnStatusApplied?.Invoke("Player", fnp);
			}
			else
			{
				var str = StsStatusEffect.CreateStrength(card.MagicNumber > 0 ? card.MagicNumber : 2);
				_player.AddStatus(str);
				OnStatusApplied?.Invoke("Player", str);
			}

			result.Success = true;
		}

		public int CalculateDamage(int baseDamage, StsDamageType type = StsDamageType.Normal)
		{
			float damage = baseDamage;

			if (type == StsDamageType.Attack)
			{
				damage += _player.Strength;
			}

			float weakMultiplier = 1f;
			if (_player.GetStatusStacks("weak") > 0)
			{
				weakMultiplier = 0.75f;
			}

			return Math.Max(0, (int)(damage * weakMultiplier));
		}

		public int CalculateDamageToEnemy(int baseDamage, StsPlayerState target)
		{
			float damage = baseDamage;

			if (target.HasStatus("vulnerable"))
			{
				damage *= 1.5f;
			}

			return (int)damage;
		}

		private List<StsEnemyData> GetTargets(StsCardTarget target, int targetIndex)
		{
			var targets = new List<StsEnemyData>();

			switch (target)
			{
				case StsCardTarget.EnemySingle:
					if (targetIndex >= 0 && targetIndex < _enemies.Count && !_enemies[targetIndex].IsDead)
						targets.Add(_enemies[targetIndex]);
					else
					{
						var alive = _enemies.FirstOrDefault(e => !e.IsDead);
						if (alive != null) targets.Add(alive);
					}
					break;
				case StsCardTarget.EnemyAll:
					targets.AddRange(_enemies.Where(e => !e.IsDead));
					break;
				default:
					break;
			}

			return targets;
		}

		private void GenerateEnemyIntent(StsEnemyData enemy)
		{
			float roll = _rng.Randf();

			var baseId = enemy.Id.Split('_')[0];
			if (enemy.Id.StartsWith("AcidSlime")) baseId = "AcidSlime";
			else if (enemy.Id.StartsWith("SpikeSlime")) baseId = "SpikeSlime";

			enemy.CurrentIntent = baseId switch
			{
				"Cultist" => GenerateCultistIntent(enemy, roll),
				"JawWorm" => GenerateJawWormIntent(enemy, roll),
				"Louse" => GenerateLouseIntent(enemy, roll),
				"Slaver" => GenerateSlaverIntent(enemy, roll),
				"RedSlaver" => GenerateRedSlaverIntent(enemy, roll),
				"FungiBeast" => GenerateFungiBeastIntent(enemy, roll),
				"Gremlin" => GenerateGremlinNobIntent(enemy, roll),
				"Lagavulin" => GenerateLagavulinIntent(enemy, roll),
				"Sentry" => GenerateSentryIntent(enemy, roll),
				"ShelledParasite" => GenerateShelledParasiteIntent(enemy, roll),
				"AcidSlime" => GenerateAcidSlimeIntent(enemy, roll),
				"SpikeSlime" => GenerateSpikeSlimeIntent(enemy, roll),
				"TaskMaster" => GenerateTaskMasterIntent(enemy, roll),
				"FungusLing" => GenerateFungusLingIntent(enemy, roll),
				"The" => GenerateBossIntent(enemy, roll),
				"Hexaghost" => GenerateBossIntent(enemy, roll),
				"Donu" => GenerateBossIntent(enemy, roll),
				_ => GenerateDefaultIntent(roll)
			};
		}

		private StsEnemyIntent GenerateSlaverIntent(StsEnemyData enemy, float roll)
		{
			if (_turnNumber % 4 == 1)
				return StsEnemyIntent.CreateDebuff("虚弱", 2);
			else if (roll < 0.5f)
				return StsEnemyIntent.CreateAttack(12 + _turnNumber / 3);
			else
				return StsEnemyIntent.CreateDefend(6);
		}

		private StsEnemyIntent GenerateRedSlaverIntent(StsEnemyData enemy, float roll)
		{
			if (_turnNumber % 3 == 0)
				return StsEnemyIntent.CreateDebuff("脆弱", 2);
			else if (roll < 0.6f)
				return StsEnemyIntent.CreateAttack(13 + _turnNumber / 3);
			else
				return StsEnemyIntent.CreateAttack(8);
		}

		private StsEnemyIntent GenerateFungiBeastIntent(StsEnemyData enemy, float roll)
		{
			if (_turnNumber % 3 == 0)
				return StsEnemyIntent.CreateBuff("力量", 2);
			else
				return StsEnemyIntent.CreateAttack(6 + _turnNumber / 2);
		}

		private StsEnemyIntent GenerateGremlinNobIntent(StsEnemyData enemy, float roll)
		{
			if (_turnNumber == 1)
				return StsEnemyIntent.CreateBuff("力量", 2);
			else if (roll < 0.6f)
				return StsEnemyIntent.CreateAttack(14 + _turnNumber);
			else
				return StsEnemyIntent.CreateAttack(10 + _turnNumber / 2);
		}

		private StsEnemyIntent GenerateLagavulinIntent(StsEnemyData enemy, float roll)
		{
			int cycle = _turnNumber % 3;
			if (cycle == 0)
				return StsEnemyIntent.CreateDebuff("力量-1", 1);
			else if (cycle == 1)
				return StsEnemyIntent.CreateDefend(12);
			else
				return StsEnemyIntent.CreateAttack(18 + _turnNumber / 2);
		}

		private StsEnemyIntent GenerateSentryIntent(StsEnemyData enemy, float roll)
		{
			if (_turnNumber % 2 == 1)
				return StsEnemyIntent.CreateDefend(8);
			else
				return StsEnemyIntent.CreateAttack(9 + _turnNumber / 3);
		}

		private StsEnemyIntent GenerateShelledParasiteIntent(StsEnemyData enemy, float roll)
		{
			if (_turnNumber % 3 == 0)
				return StsEnemyIntent.CreateDebuff("脆弱", 1);
			else if (roll < 0.5f)
				return StsEnemyIntent.CreateAttack(8 + _turnNumber / 2);
			else
				return StsEnemyIntent.CreateDefend(10);
		}

		private StsEnemyIntent GenerateAcidSlimeIntent(StsEnemyData enemy, float roll)
		{
			if (roll < 0.3f)
				return StsEnemyIntent.CreateDebuff("虚弱", 1);
			else
				return StsEnemyIntent.CreateAttack(5 + _turnNumber / 3);
		}

		private StsEnemyIntent GenerateSpikeSlimeIntent(StsEnemyData enemy, float roll)
		{
			if (roll < 0.3f)
				return StsEnemyIntent.CreateDebuff("脆弱", 1);
			else
				return StsEnemyIntent.CreateAttack(5 + _turnNumber / 3);
		}

		private StsEnemyIntent GenerateTaskMasterIntent(StsEnemyData enemy, float roll)
		{
			if (_turnNumber % 3 == 0)
				return StsEnemyIntent.CreateBuff("力量", 1);
			else if (roll < 0.5f)
				return StsEnemyIntent.CreateAttack(10 + _turnNumber / 2);
			else
				return StsEnemyIntent.CreateDefend(8);
		}

		private StsEnemyIntent GenerateFungusLingIntent(StsEnemyData enemy, float roll)
		{
			if (_turnNumber % 2 == 0)
				return StsEnemyIntent.CreateBuff("力量", 1);
			else
				return StsEnemyIntent.CreateAttack(6 + _turnNumber / 2);
		}

		private StsEnemyIntent GenerateBossIntent(StsEnemyData enemy, float roll)
		{
			int phase = enemy.CurrentHp > enemy.MaxHp / 2 ? 1 : 2;
			int cycle = _turnNumber % 4;

			if (phase == 1)
			{
				return cycle switch
				{
					0 => StsEnemyIntent.CreateAttack(16 + _turnNumber / 2),
					1 => StsEnemyIntent.CreateDefend(14),
					2 => StsEnemyIntent.CreateBuff("力量", 2),
					_ => StsEnemyIntent.CreateAttack(12 + _turnNumber / 3)
				};
			}
			else
			{
				return cycle switch
				{
					0 => StsEnemyIntent.CreateAttack(20 + _turnNumber),
					1 => StsEnemyIntent.CreateDebuff("脆弱", 2),
					2 => StsEnemyIntent.CreateAttack(15 + _turnNumber / 2),
					_ => StsEnemyIntent.CreateBuff("力量", 3)
				};
			}
		}

		private StsEnemyIntent GenerateCultistIntent(StsEnemyData enemy, float roll)
		{
			if (_turnNumber % 3 == 0)
			{
				return StsEnemyIntent.CreateBuff("仪式", 3);
			}
			else
			{
				int damage = _rng.RandiRange(6, 9);
				return StsEnemyIntent.CreateAttack(damage);
			}
		}

		private StsEnemyIntent GenerateJawWormIntent(StsEnemyData enemy, float roll)
		{
			if (roll < 0.25f)
			{
				return StsEnemyIntent.CreateDefend(6 + _turnNumber / 2);
			}
			else if (roll < 0.6f)
			{
				return StsEnemyIntent.CreateAttack(12 + _turnNumber);
			}
			else
			{
				return StsEnemyIntent.CreateAttack(5 + _turnNumber / 2);
			}
		}

		private StsEnemyIntent GenerateLouseIntent(StsEnemyData enemy, float roll)
		{
			if (roll < 0.33f)
			{
				return StsEnemyIntent.CreateDefend(_rng.RandiRange(4, 8));
			}
			else if (roll < 0.66f)
			{
				return StsEnemyIntent.CreateAttack(_rng.RandiRange(4, 7));
			}
			else
			{
				return StsEnemyIntent.CreateBuff("成长", 2);
			}
		}

		private StsEnemyIntent GenerateDefaultIntent(float roll)
		{
			return StsEnemyIntent.CreateAttack(_rng.RandiRange(5, 10));
		}

		public void EndTurn()
		{
			IsPlayerTurn = false;
			_player.Energy = 0;

			_relicManager?.TriggerOnTurnEnd(this);

			foreach (var card in _player.Hand.ToList())
			{
				if (!card.Retain)
				{
					_player.Hand.Remove(card);
					_player.DiscardPile.Add(card);
				}
			}

			ProcessEndTurnStatusEffects(_player);

			ExecuteEnemyTurns();

			OnTurnEnded?.Invoke(_turnNumber);

			if (!_player.IsDead)
			{
				StartNewTurn();
			}
			else
			{
				OnCombatLost?.Invoke();
				GD.Print("[StsCombatEngine] Player defeated!");
			}
		}

		private void ExecuteEnemyTurns()
		{
			foreach (var enemy in _enemies)
			{
				if (enemy.IsDead) continue;

				ProcessEndTurnEnemyStatusEffects(enemy);

				if (enemy.IsDead) continue;

				ExecuteEnemyAction(enemy);
			}

			CheckCombatEnd();
		}

		private void ExecuteEnemyAction(StsEnemyData enemy)
		{
			if (enemy.CurrentIntent == null) return;

			switch (enemy.CurrentIntent.Type)
			{
				case StsEnemyIntent.IntentType.Attack:
					int damage = CalculateDamageToEnemy(enemy.CurrentIntent.Value, _player);
					_player.ApplyDamage(damage);
					OnDamageDealt?.Invoke(-1, "玩家", damage);
					GD.Print($"[StsCombatEngine] {enemy.Name} attacks for {damage}");
					break;

				case StsEnemyIntent.IntentType.Defend:
					enemy.Block += enemy.CurrentIntent.Value;
					OnBlockGained?.Invoke(enemy.CurrentIntent.Value, enemy.Block);
					GD.Print($"[StsCombatEngine] {enemy.Name} gains {enemy.CurrentIntent.Value} block");
					break;

				case StsEnemyIntent.IntentType.Buff:
					var buff = StsStatusEffect.CreateStrength(enemy.CurrentIntent.Value);
					enemy.AddStatus(buff);
					OnStatusApplied?.Invoke(enemy.Name, buff);
					GD.Print($"[StsCombatEngine] {enemy.Name} gains {buff.Name} +{buff.Stacks}");
					break;

				case StsEnemyIntent.IntentType.Debuff:
					var debuff = StsStatusEffect.CreateWeak(enemy.CurrentIntent.Value);
					_player.AddStatus(debuff);
					OnStatusApplied?.Invoke("玩家", debuff);
					GD.Print($"[StsCombatEngine] Player receives {debuff.Name} {debuff.Stacks}");
					break;
			}
		}

		private void CheckCombatEnd()
		{
			if (IsCombatOver) return;

			bool allEnemiesDead = _enemies.All(e => e.IsDead);
			if (allEnemiesDead)
			{
				IsCombatOver = true;
				_relicManager?.TriggerOnCombatEnd(this);
				OnCombatWon?.Invoke();
				GD.Print("[StsCombatEngine] Victory!");
				return;
			}

			if (_player.IsDead)
			{
				IsCombatOver = true;
				OnCombatLost?.Invoke();
				GD.Print("[StsCombatEngine] Player defeated!");
			}
		}

		public class StsPlayResult
		{
			public bool Success { get; set; }
			public string Reason { get; set; }
			public int DamageDealt { get; set; }
			public int BlockGained { get; set; }
			public List<StsStatusEffect> StatusApplied { get; set; } = new();
		}
	}
}