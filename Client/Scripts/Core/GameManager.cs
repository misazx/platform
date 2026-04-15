using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using RoguelikeGame.Systems;
using RoguelikeGame.Core;
using RoguelikeGame.Database;
using RoguelikeGame.Generation;
using RoguelikeGame.Combat;
using RoguelikeGame.UI.Panels;

namespace RoguelikeGame.Core
{
	public enum GamePhase
	{
		MainMenu,
		CharacterSelect,
		MapNavigation,
		Combat,
		Event,
		Shop,
		RestSite,
		BossReward,
		Victory,
		GameOver
	}

	public class RunData
	{
		public uint Seed { get; set; }
		public string CharacterId { get; set; }

		public int CurrentFloor { get; set; } = 1;
		public int CurrentRoom { get; set; } = 0;

		public int Gold { get; set; } = 99;
		public int MaxHealth { get; set; } = 80;
		public int CurrentHealth { get; set; } = 80;

		public List<CardData> Deck { get; set; } = new();
		public List<string> Relics { get; set; } = new();
		public List<string> Potions { get; set; } = new();

		public Dictionary<string, object> CustomData { get; set; } = new();

		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public bool IsVictory { get; set; }
		public int TotalDamageDealt { get; set; }
		public int TotalEnemiesDefeated { get; set; }
		public int TotalCardsPlayed { get; set; }
	}

	public partial class GameManager : SingletonBase<GameManager>
	{
		private RunData _currentRun;
		private FloorMap _currentMap;
		private MapNode _currentNode;
		private GamePhase _currentPhase = GamePhase.MainMenu;
		private RandomNumberGenerator _rng;

		[Signal]
		public delegate void PhaseChangedEventHandler(int phase);

		[Signal]
		public delegate void RunStartedEventHandler(string characterId);

		[Signal]
		public delegate void RunEndedEventHandler(bool victory);

		[Signal]
		public delegate void FloorChangedEventHandler(int floor);

		public RunData CurrentRun => _currentRun;
		public GamePhase CurrentPhase => _currentPhase;
		public FloorMap CurrentMap => _currentMap;
		public MapNode CurrentNode => _currentNode;

		protected override void OnInitialize()
		{
			InitializeSystems();
		}

		private void InitializeSystems()
		{
			GD.Print("[GameManager] Initializing game systems...");
		}

		public void StartNewRun(string characterId, uint seed = 0)
		{
			GD.Print($"[GameManager] StartNewRun called with character: {characterId}");

			var characterData = CharacterDatabase.Instance.GetCharacter(characterId);
			if (characterData == null)
			{
				GD.PrintErr($"[GameManager] Character not found: {characterId}");
				return;
			}

			GD.Print($"[GameManager] Found character: {characterData.Name}");

			_rng = new RandomNumberGenerator();
			_rng.Seed = seed == 0 ? (ulong)GD.Randi() : seed;

			_currentRun = new RunData
			{
				Seed = seed == 0 ? (uint)_rng.Randi() : seed,
				CharacterId = characterId,

				MaxHealth = characterData.MaxHealth,
				CurrentHealth = characterData.MaxHealth,
				Gold = characterData.StartingGold,

				StartTime = DateTime.Now
			};

			GD.Print("[GameManager] RunData created, initializing deck...");
			InitializeStartingDeck(characterId);

			GD.Print($"[GameManager] MapGenerator.Instance: {MapGenerator.Instance != null}");
			GD.Print($"[GameManager] ShopManager.Instance: {ShopManager.Instance != null}");

			if (MapGenerator.Instance != null)
				MapGenerator.Instance.Initialize((uint)_rng.Randi());
			else
				GD.PushError("[GameManager] MapGenerator.Instance is NULL!");

			if (ShopManager.Instance != null)
				ShopManager.Instance.Initialize((uint)_rng.Randi());

			if (CombatManager.Instance != null && _currentRun.Deck.Count > 0)
				CombatManager.Instance.SetPlayerDeck(_currentRun.Deck);

			ChangePhase(GamePhase.MapNavigation);

			GD.Print("[GameManager] Calling GenerateCurrentFloor...");
			GenerateCurrentFloor();

			EmitSignal(SignalName.RunStarted, characterId);

			TimelineManager.Instance?.AddEventTriggered(
				"run_started",
				$"开始新游戏: {characterData.Name}",
				1,
				0
			);

			GD.Print($"[GameManager] New run started with {characterData.Name} (Seed: {_currentRun.Seed})");
		}

		private void InitializeStartingDeck(string characterId)
		{
			var characterData = CharacterDatabase.Instance.GetCharacter(characterId);
			if (characterData == null)
				return;

			_currentRun.Deck.Clear();

			foreach (var cardId in characterData.StartingCards)
			{
				var card = CardDatabase.Instance.GetCard(cardId);
				if (card != null)
				{
					if (cardId.Contains("strike") || cardId.Contains("defend"))
					{
						for (int i = 0; i < 5; i++)
							_currentRun.Deck.Add(card);
					}
					else
						_currentRun.Deck.Add(card);
				}
			}

			GD.Print($"[GameManager] Initialized deck with {_currentRun.Deck.Count} cards");
		}

		private void GenerateCurrentFloor()
		{
			GD.Print("[GameManager] GenerateCurrentFloor() called");

			if (MapGenerator.Instance == null)
			{
				GD.PushError("[GameManager] MapGenerator.Instance is NULL in GenerateCurrentFloor!");
				return;
			}

			_currentMap = MapGenerator.Instance.GenerateFloor(_currentRun.CurrentFloor);
			GD.Print($"[GameManager] Map generated: {_currentMap != null}, nodes: {_currentMap?.Nodes?.Count ?? 0}");

			if (_currentMap == null)
			{
				GD.PushError("[GameManager] GenerateFloor returned null!");
				return;
			}

			_currentNode = FindStartNode(_currentMap);

			if (_currentNode != null)
				_currentNode.Status = MapNodeStatus.Current;

			EmitSignal(SignalName.FloorChanged, _currentRun.CurrentFloor);
			GD.Print($"[GameManager] Floor {_currentRun.CurrentFloor} generation complete");
		}

		private MapNode FindStartNode(FloorMap map)
		{
			return map.Nodes.FirstOrDefault(n => n.Id == map.StartNodeId);
		}

		public void VisitNode(MapNode node)
		{
			if (!MapGenerator.Instance.VisitNode(_currentMap, node))
				return;

			_currentNode = node;

			switch (node.Type)
			{
				case NodeType.Monster:
				case NodeType.Elite:
				case NodeType.Boss:
					StartCombat(node);
					break;

				case NodeType.Event:
					TriggerEvent(node);
					break;

				case NodeType.Shop:
					OpenShop();
					break;

				case NodeType.Rest:
					OpenRestSite();
					break;

				case NodeType.Treasure:
					OpenTreasureChest(node);
					break;
			}
		}

		public void StartCombat(MapNode node)
		{
			ChangePhase(GamePhase.Combat);

			TimelineManager.Instance?.AddCombatStart(
				_currentRun.CurrentFloor,
				_currentRun.CurrentRoom,
				new List<string> { node.EnemyEncounterId ?? "Unknown Enemy" }
			);

			GD.Print($"[GameManager] Starting combat against: {node.EnemyEncounterId}");
		}

		public void EndCombat(bool victory)
		{
			if (victory)
			{
				AwardCombatRewards(_currentNode.Type);
				_currentRun.TotalEnemiesDefeated++;

				TimelineManager.Instance?.AddCombatEnd(
					_currentRun.CurrentFloor,
					_currentRun.CurrentRoom,
					victory: true,
					damageTaken: 0,
					damageDealt: 100
				);

				AchievementSystem.Instance?.UpdateProgress("kill_100_enemies", 1);

				if (_currentNode.Type == NodeType.Boss)
					AdvanceToNextFloor();
				else
					ChangePhase(GamePhase.MapNavigation);
			}
			else
			{
				EndRun(false);
			}
		}

		private void AwardCombatRewards(NodeType nodeType)
		{
			switch (nodeType)
			{
				case NodeType.Monster:
					_currentRun.Gold += (int)_rng.RandiRange(10, 16);
					if (_rng.Randf() < 0.3f)
						AddRandomPotion();
					break;

				case NodeType.Elite:
					_currentRun.Gold += (int)_rng.RandiRange(25, 36);
					AddRandomRelic(Database.RelicTier.Common);
					if (_rng.Randf() < 0.5f)
						AddRandomPotion();
					break;

				case NodeType.Boss:
					_currentRun.Gold += (int)_rng.RandiRange(95, 106);
					AddRandomRelic(Database.RelicTier.Special);
					AddRandomPotion();
					break;
			}
		}

		public void TriggerEvent(MapNode node)
		{
			ChangePhase(GamePhase.Event);
			GD.Print($"[GameManager] Triggering event: {node.EventId ?? "Unknown"}");
		}

		public void CompleteEvent()
		{
			ChangePhase(GamePhase.MapNavigation);
		}

		public void OpenShop()
		{
			ChangePhase(GamePhase.Shop);

			if (ShopManager.Instance != null)
			{
				var inventory = ShopManager.Instance.GenerateShopInventory(
					_currentRun.CharacterId,
					_currentRun.CurrentFloor
				);
			}

			TimelineManager.Instance?.AddEventTriggered(
				"shop_visit",
				"访问商店",
				_currentRun.CurrentFloor,
				_currentRun.CurrentRoom
			);
		}

		public void CloseShop()
		{
			ChangePhase(GamePhase.MapNavigation);
		}

		public void OpenRestSite()
		{
			ChangePhase(GamePhase.RestSite);

			TimelineManager.Instance?.AddEventTriggered(
				"rest_site",
				"在篝火处休息",
				_currentRun.CurrentFloor,
				_currentRun.CurrentRoom
			);
		}

		public void CompleteRest()
		{
			ChangePhase(GamePhase.MapNavigation);
		}

		public void OpenTreasureChest(MapNode node)
		{
			string chestType = node.CustomData.GetValueOrDefault("chest_type", "small").ToString();

			if (chestType == "large")
			{
				AddRandomRelic(Database.RelicTier.Rare);
				_currentRun.Gold += (int)_rng.RandiRange(30, 51);
			}
			else
			{
				AddRandomPotion();
				_currentRun.Gold += (int)_rng.RandiRange(15, 26);
			}

			ChangePhase(GamePhase.MapNavigation);
		}

		public void AdvanceToNextFloor()
		{
			_currentRun.CurrentFloor++;
			_currentRun.CurrentRoom = 0;

			if (_currentRun.CurrentFloor > EncounterGenerator.GetTotalFloors())
			{
				EndRun(true);
				return;
			}

			RoguelikeGame.UI.MapView.ResetPersistentState();
			GenerateCurrentFloor();
			ChangePhase(GamePhase.MapNavigation);

			GD.Print($"[GameManager] Advanced to floor {_currentRun.CurrentFloor}");
		}

		public void ChangePhase(GamePhase newPhase)
		{
			_currentPhase = newPhase;
			EmitSignal(SignalName.PhaseChanged, (int)newPhase);
			GD.Print($"[GameManager] Phase changed to: {newPhase}");
		}

		public void AddCardToDeck(CardData card)
		{
			_currentRun.Deck.Add(card);
			GD.Print($"[GameManager] Added card to deck: {card.Name}");
		}

		public void RemoveCardFromDeck(CardData card)
		{
			_currentRun.Deck.Remove(card);
			GD.Print($"[GameManager] Removed card from deck: {card.Name}");
		}

		public void AddRelic(string relicId)
		{
			if (!_currentRun.Relics.Contains(relicId))
			{
				_currentRun.Relics.Add(relicId);
				GD.Print($"[GameManager] Acquired relic: {relicId}");
			}
		}

		public void AddPotion(string potionId)
		{
			if (_currentRun.Potions.Count < 3)
			{
				_currentRun.Potions.Add(potionId);
				GD.Print($"[GameManager] Acquired potion: {potionId}");
			}
		}

		public void SpendGold(int amount)
		{
			_currentRun.Gold -= amount;
			GD.Print($"[GameManager] Spent {amount} gold (Remaining: {_currentRun.Gold})");
		}

		public void EarnGold(int amount)
		{
			_currentRun.Gold += amount;
			GD.Print($"[GameManager] Earned {amount} gold (Total: {_currentRun.Gold})");
		}

		private void AddRandomRelic(Database.RelicTier tier)
		{
			var relic = RelicDatabase.Instance.GetRandomRelic(tier, _rng);
			if (relic != null)
				AddRelic(relic.Id);
		}

		private void AddRandomPotion()
		{
			var potion = PotionDatabase.Instance.GetRandomPotion(_rng);
			if (potion != null)
				AddPotion(potion.Id);
		}

		public void EndRun(bool victory)
		{
			_currentRun.IsVictory = victory;
			_currentRun.EndTime = DateTime.Now;

			var stats = new RunStatistics
			{
				CharacterId = _currentRun.CharacterId,
				FloorReached = _currentRun.CurrentFloor,
				EnemiesDefeated = _currentRun.TotalEnemiesDefeated,
				DamageDealt = _currentRun.TotalDamageDealt,
				CardsPlayed = _currentRun.TotalCardsPlayed,
				RelicsCollected = _currentRun.Relics.Count,
				GoldEarned = _currentRun.Gold,
				Victory = victory,
				StartTime = _currentRun.StartTime,
				EndTime = _currentRun.EndTime,
				Seed = _currentRun.Seed,
				DeckComposition = _currentRun.Deck.Select(c => c.Id).ToList()
			};

			AchievementSystem.Instance.RecordRun(stats);

			if (victory)
			{
				TimelineManager.Instance?.AddEventTriggered(
					"victory",
					$"胜利! 伤害: {_currentRun.TotalDamageDealt}, 卡牌: {_currentRun.TotalCardsPlayed}",
					_currentRun.CurrentFloor,
					_currentRun.CurrentRoom
				);

				ChangePhase(GamePhase.Victory);
			}
			else
			{
				TimelineManager.Instance?.AddDeath(
					_currentRun.CurrentFloor,
					_currentRun.CurrentRoom,
					cause: "被敌人击败"
				);

				ChangePhase(GamePhase.GameOver);
			}

			EmitSignal(SignalName.RunEnded, victory);

			GD.Print($"[GameManager] Run ended - Victory: {victory}");
		}

		public void ReturnToMainMenu()
		{
			_currentRun = null;
			_currentMap = null;
			_currentNode = null;

			ChangePhase(GamePhase.MainMenu);
		}

		public override void _Input(InputEvent @event)
		{
		}
	}
}
