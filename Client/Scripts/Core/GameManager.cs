using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using RoguelikeGame.Systems;
using RoguelikeGame.Core;
using RoguelikeGame.Database;
using RoguelikeGame.Generation;

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

		protected override void OnInitialize()
		{
			GD.Print("[GameManager] Initializing game systems...");
		}

		public void StartNewRun(string characterId, uint seed = 0)
		{
			GD.Print($"[GameManager] StartNewRun called with character: {characterId}");

			_rng = new RandomNumberGenerator();
			_rng.Seed = seed == 0 ? (ulong)GD.Randi() : seed;

			_currentRun = new RunData
			{
				Seed = seed == 0 ? (uint)_rng.Randi() : seed,
				CharacterId = characterId,
				MaxHealth = 80,
				CurrentHealth = 80,
				Gold = 99,
				StartTime = DateTime.Now
			};

			ChangePhase(GamePhase.MapNavigation);
			EmitSignal(SignalName.RunStarted, characterId);
			GD.Print($"[GameManager] New run started with {characterId} (Seed: {_currentRun.Seed})");
		}

		public void EndCombat(bool victory)
		{
			if (victory)
			{
				_currentRun.TotalEnemiesDefeated++;
				ChangePhase(GamePhase.MapNavigation);
			}
			else
			{
				EndRun(false);
			}
		}

		public void AdvanceToNextFloor()
		{
			_currentRun.CurrentFloor++;
			_currentRun.CurrentRoom = 0;

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
			_currentRun?.Deck.Add(card);
		}

		public void AddRelic(string relicId)
		{
			if (_currentRun != null && !_currentRun.Relics.Contains(relicId))
				_currentRun.Relics.Add(relicId);
		}

		public void AddPotion(string potionId)
		{
			if (_currentRun != null && _currentRun.Potions.Count < 3)
				_currentRun.Potions.Add(potionId);
		}

		public void SpendGold(int amount)
		{
			if (_currentRun != null)
				_currentRun.Gold -= amount;
		}

		public void EarnGold(int amount)
		{
			if (_currentRun != null)
				_currentRun.Gold += amount;
		}

		public void EndRun(bool victory)
		{
			if (_currentRun == null) return;

			_currentRun.IsVictory = victory;
			_currentRun.EndTime = DateTime.Now;

			ChangePhase(victory ? GamePhase.Victory : GamePhase.GameOver);
			EmitSignal(SignalName.RunEnded, victory);
			GD.Print($"[GameManager] Run ended - Victory: {victory}");
		}

		public void ReturnToMainMenu()
		{
			_currentRun = null;
			ChangePhase(GamePhase.MainMenu);
		}
	}
}
