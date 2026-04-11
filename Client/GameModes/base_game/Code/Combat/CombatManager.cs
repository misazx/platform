using Godot;
using RoguelikeGame.Core;
using System;
using System.Collections.Generic;
using RoguelikeGame.Database;

namespace RoguelikeGame.Core
{
	public enum CombatPhase
	{
		PlayerTurn,
		EnemyTurn,
		CardSelection,
		TargetSelection,
		GameOver,
		Victory
	}

	public enum PlayerAction
	{
		PlayCard,
		EndTurn,
		UsePotion,
		CheckDeck,
		CheckDiscardPile,
		None
	}

	public class CombatState
	{
		public int CurrentEnergy { get; set; }
		public int MaxEnergy { get; set; } = 3;
		public int CurrentBlock { get; set; }
		public int TurnNumber { get; set; }
		
		public List<CardData> Hand { get; set; } = new();
		public List<CardData> DrawPile { get; set; } = new();
		public List<CardData> DiscardPile { get; set; } = new();
		public List<CardData> ExhaustPile { get; set; } = new();
		
		public Dictionary<string, int> Buffs { get; set; } = new();
		public Dictionary<string, int> Debuffs { get; set; } = new();
		
		public bool HasActed { get; set; }
		public bool CanPlayCards => CurrentEnergy > 0 && Hand.Count > 0;
	}

	public partial class CombatManager : SingletonBase<CombatManager>
	{
		private CombatState _state;
		private CombatPhase _currentPhase = CombatPhase.PlayerTurn;
		private Node _player;
		private List<Node> _enemies = new();
		private RandomNumberGenerator _rng;
		private List<CardData> _playerDeck = new();

		[Signal]
		public delegate void TurnStartedEventHandler(int turn);
		
		[Signal]
		public delegate void TurnEndedEventHandler(int turn);
		
		[Signal]
		public delegate void CardPlayedEventHandler(string cardId, string targetName);
		
		[Signal]
		public delegate void EnergyChangedEventHandler(int current, int max);
		
		[Signal]
		public delegate void BlockChangedEventHandler(int block);
		
		[Signal]
		public delegate void PhaseChangedEventHandler(int newPhase);

		public CombatState State => _state;
		public CombatPhase CurrentPhase => _currentPhase;
		public bool IsInCombat => _state != null && _player != null;

		protected override void OnInitialize()
		{
			GD.Print("[CombatManager] Initialized");
		}

		public void InitializeCombat(Node player, List<Node> enemies, uint seed)
		{
			GD.Print($"[CombatManager] InitializeCombat called - player: {player != null}, enemies: {enemies?.Count ?? 0}");
			
			_player = player;
			_enemies = enemies ?? new List<Node>();
			_rng = new RandomNumberGenerator();
			_rng.Seed = (ulong)seed;
			
			_state = new CombatState
			{
				MaxEnergy = 3,
				CurrentEnergy = 3,
				CurrentBlock = 0,
				TurnNumber = 0,
				Hand = new(),
				DrawPile = new(),
				DiscardPile = new(),
				ExhaustPile = new()
			};
			
			InitializeDrawPile();
			
			if (_state.DrawPile.Count > 0)
			{
				StartNewTurn();
				GD.Print($"[CombatManager] Combat started with {_enemies.Count} enemies, {_state.DrawPile.Count} cards in deck");
			}
			else
			{
				GD.Print("[CombatManager] No cards in deck, combat not started");
			}
		}

		public void SetPlayerDeck(List<CardData> deck)
		{
			_playerDeck = deck ?? new List<CardData>();
			GD.Print($"[CombatManager] Player deck set with {_playerDeck.Count} cards");
		}

		private void InitializeDrawPile()
		{
			_state.DrawPile.Clear();
			
			if (_playerDeck.Count > 0)
			{
				foreach (var card in _playerDeck)
				{
					_state.DrawPile.Add(card);
				}
				GD.Print($"[CombatManager] Loaded {_state.DrawPile.Count} cards from player deck");
			}
			else if (_player != null && _player.HasMethod("GetDeck"))
			{
				var deckVariant = _player.Call("GetDeck");
				if (deckVariant.VariantType == Variant.Type.Array)
				{
					var arr = deckVariant.AsGodotArray();
					foreach (var item in arr)
					{
						if (item.VariantType == Variant.Type.Object && item.Obj is CardData card)
							_state.DrawPile.Add(card);
					}
					GD.Print($"[CombatManager] Loaded {_state.DrawPile.Count} cards from player");
				}
			}
			else if (GameManager.Instance?.CurrentRun?.Deck != null)
			{
				foreach (var card in GameManager.Instance.CurrentRun.Deck)
				{
					_state.DrawPile.Add(card);
				}
				GD.Print($"[CombatManager] Loaded {_state.DrawPile.Count} cards from GameManager");
			}
			
			if (_state.DrawPile.Count > 0)
			{
				ShuffleList(_state.DrawPile);
			}
			else
			{
				GD.PushWarning("[CombatManager] No cards available for combat!");
			}
		}

		private void ShuffleList<T>(List<T> list)
		{
			if (list == null || list.Count <= 1) return;
			
			for (int i = list.Count - 1; i > 0; i--)
			{
				int j = (int)(_rng.Randf() * (i + 1));
				(list[i], list[j]) = (list[j], list[i]);
			}
		}

		public void StartNewTurn()
		{
			if (_state == null) return;
			
			_state.TurnNumber++;
			_state.CurrentEnergy = _state.MaxEnergy;
			_state.CurrentBlock = 0;
			_state.HasActed = false;
			_state.Hand.Clear();
			
			DrawCards(5);
			
			_currentPhase = CombatPhase.PlayerTurn;
			
			EmitSignal(SignalName.TurnStarted, _state.TurnNumber);
			EmitSignal(SignalName.EnergyChanged, _state.CurrentEnergy, _state.MaxEnergy);
			EmitSignal(SignalName.BlockChanged, _state.CurrentBlock);
			EmitSignal(SignalName.PhaseChanged, (int)_currentPhase);
			
			GD.Print($"[CombatManager] Turn {_state.TurnNumber} started - Energy: {_state.CurrentEnergy}, Hand: {_state.Hand.Count}");
		}

		public void DrawCards(int count)
		{
			if (_state == null) return;
			
			for (int i = 0; i < count; i++)
			{
				if (_state.DrawPile.Count == 0)
				{
					ReshuffleDiscardPile();
					if (_state.DrawPile.Count == 0)
						break;
				}
				
				var card = _state.DrawPile[0];
				_state.DrawPile.RemoveAt(0);
				_state.Hand.Add(card);
			}
			
			GD.Print($"[CombatManager] Drew {count} cards, hand size: {_state.Hand.Count}");
		}

		private void ReshuffleDiscardPile()
		{
			if (_state.DiscardPile.Count > 0)
			{
				_state.DrawPile.AddRange(_state.DiscardPile);
				_state.DiscardPile.Clear();
				ShuffleList(_state.DrawPile);
				GD.Print("[CombatManager] Reshuffled discard pile into draw pile");
			}
		}

		public bool CanPlayCard(CardData card, Node target = null)
		{
			if (_state == null || _currentPhase != CombatPhase.PlayerTurn)
				return false;
			
			if (!_state.Hand.Contains(card))
				return false;
			
			if (_state.CurrentEnergy < card.Cost)
				return false;
			
			return true;
		}

		public bool PlayCard(CardData card, Node target = null)
		{
			if (!CanPlayCard(card, target))
			{
				GD.PrintErr($"[CombatManager] Cannot play card: {card?.Name ?? "null"}");
				return false;
			}
			
			_state.CurrentEnergy -= card.Cost;
			_state.Hand.Remove(card);
			
			ExecuteCardEffect(card, target);
			HandleCardDisposal(card);
			
			_state.HasActed = true;
			
			EmitSignal(SignalName.CardPlayed, card.Id, target?.Name ?? "");
			EmitSignal(SignalName.EnergyChanged, _state.CurrentEnergy, _state.MaxEnergy);
			
			GD.Print($"[CombatManager] Played: {card.Name}");
			return true;
		}

		private void ExecuteCardEffect(CardData card, Node target)
		{
			if (card.Damage > 0 && target != null)
			{
				ApplyDamageToTarget(target, CalculateDamage(card));
			}
			
			if (card.Block > 0)
			{
				AddBlock(card.Block);
			}
		}

		private int CalculateDamage(CardData card)
		{
			int damage = card.Damage;
			if (_state.Buffs.ContainsKey("strength"))
				damage += _state.Buffs["strength"];
			return damage;
		}

		private void ApplyDamageToTarget(Node target, int damage)
		{
			if (target != null && target.HasMethod("TakeDamage"))
			{
				target.Call("TakeDamage", damage, _player);
			}
		}

		public void AddBlock(int amount)
		{
			if (_state == null) return;
			_state.CurrentBlock += amount;
			EmitSignal(SignalName.BlockChanged, _state.CurrentBlock);
		}

		public void TakeDamage(int damage)
		{
			if (_state == null) return;
			
			int remainingDamage = damage;
			
			if (_state.CurrentBlock > 0)
			{
				int blocked = Math.Min(_state.CurrentBlock, remainingDamage);
				_state.CurrentBlock -= blocked;
				remainingDamage -= blocked;
				EmitSignal(SignalName.BlockChanged, _state.CurrentBlock);
			}
			
			if (remainingDamage > 0 && _player != null && _player.HasMethod("TakeDamage"))
			{
				_player.Call("TakeDamage", remainingDamage);
			}
		}

		private void HandleCardDisposal(CardData card)
		{
			if (card.IsExhaust)
			{
				_state.ExhaustPile.Add(card);
			}
			else
			{
				_state.DiscardPile.Add(card);
			}
		}

		public void EndTurn()
		{
			if (_state == null || _currentPhase != CombatPhase.PlayerTurn)
				return;
			
			foreach (var card in new List<CardData>(_state.Hand))
			{
				_state.DiscardPile.Add(card);
			}
			_state.Hand.Clear();
			
			_currentPhase = CombatPhase.EnemyTurn;
			
			EmitSignal(SignalName.TurnEnded, _state.TurnNumber);
			EmitSignal(SignalName.PhaseChanged, (int)_currentPhase);
			
			CallDeferred(nameof(ProcessEnemyTurn));
		}

		private async void ProcessEnemyTurn()
		{
			await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);
			
			foreach (var enemy in _enemies)
			{
				if (IsInstanceValid(enemy) && enemy.HasMethod("PerformTurn"))
				{
					enemy.Call("PerformTurn", _player);
				}
			}
			
			await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);
			
			StartNewTurn();
		}

		public void OnEnemyDied(Node enemy)
		{
			_enemies.Remove(enemy);
			
			if (_enemies.Count == 0)
			{
				Victory();
			}
		}

		public void OnPlayerDeath()
		{
			_currentPhase = CombatPhase.GameOver;
			EmitSignal(SignalName.PhaseChanged, (int)_currentPhase);
		}

		public void Victory()
		{
			_currentPhase = CombatPhase.Victory;
			EmitSignal(SignalName.PhaseChanged, (int)_currentPhase);
			GD.Print("[CombatManager] VICTORY!");
		}

		public List<CardData> GetHand() => _state != null ? new List<CardData>(_state.Hand) : new List<CardData>();
		public int DrawPileCount => _state?.DrawPile.Count ?? 0;
		public int DiscardPileCount => _state?.DiscardPile.Count ?? 0;
		public int ExhaustPileCount => _state?.ExhaustPile.Count ?? 0;
	}
}
