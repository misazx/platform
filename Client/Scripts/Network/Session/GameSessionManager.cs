using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using RoguelikeGame.Core;
using RoguelikeGame.Network.Rooms;

namespace RoguelikeGame.Network.Session
{
	public class GameState
	{
		public string RoomId { get; set; } = "";
		public uint SyncSeed { get; set; }
		public int CurrentTurn { get; set; } = 0;
		public string CurrentPhase { get; set; } = "";
		public Dictionary<string, PlayerState> Players { get; set; } = new();
		public List<CardAction> PendingActions { get; set; } = new();
		public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
	}

	public class PlayerState
	{
		public string UserId { get; set; } = "";
		public string CharacterId { get; set; } = "";
		public int CurrentHP { get; set; }
		public int MaxHP { get; set; }
		public int CurrentEnergy { get; set; }
		public int MaxEnergy { get; set; }
		public int Gold { get; set; }
		public int Block { get; set; }
		public List<string> HandCards { get; set; } = new();
		public List<string> DrawPile { get; set; } = new();
		public List<string> DiscardPile { get; set; } = new();
		public bool IsAlive { get; set; } = true;
		public Dictionary<string, int> Buffs { get; set; } = new();
		public Dictionary<string, int> Debuffs { get; set; } = new();
	}

	public class CardAction
	{
		public string ActionId { get; set; } = Guid.NewGuid().ToString();
		public string PlayerId { get; set; } = "";
		public string CardId { get; set; } = "";
		public string Action { get; set; } = ""; // play, end_turn, etc.
		public object? Target { get; set; }
		public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		public bool IsValidated { get; set; } = false;
	}

	public partial class GameSessionManager : Node
	{
		private static GameSessionManager _instance;

		public static GameSessionManager Instance => _instance;

		private GameState _gameState;
		private Queue<CardAction> _actionQueue;
		private Timer _syncTimer;
		private bool _isHost;
		private int _syncIntervalMs = 100;

		public GameState CurrentGameState => _gameState;
		public bool IsInGame => NetworkManager.Instance?.State == NetworkState.InGame;
		public bool IsHost => _isHost;

		[Signal]
		public delegate void GameStartedEventHandler(uint syncSeed);
		[Signal]
		public delegate void GameEndedEventHandler(bool victory);
		[Signal]
		public delegate void StateSyncedEventHandler(GameState state);
		[Signal]
		public delegate void TurnChangedEventHandler(int newTurn);
		[Signal]
		public delegate void PlayerActionReceivedEventHandler(CardAction action);

		public override void _Ready()
		{
			if (_instance != null && _instance != this)
			{
				QueueFree();
				return;
			}

			_instance = this;
			ProcessMode = ProcessModeEnum.Always;

			InitializeComponents();

			GD.Print("[GameSessionManager] 游戏会话管理器已初始化");
		}

		private void InitializeComponents()
		{
			_gameState = new GameState();
			_actionQueue = new Queue<CardAction>();

			_syncTimer = new Timer
			{
				WaitTime = _syncIntervalMs / 1000.0,
				OneShot = false,
				Autostart = false
			};
			_syncTimer.Connect("timeout", new Callable(this, nameof(OnSyncTick)));
			AddChild(_syncTimer);
		}

		public async Task StartGameSessionAsync(RoomInfo room)
		{
			if (room == null)
			{
				GD.PrintErr("[GameSessionManager] 无法开始: 房间信息为空");
				return;
			}

			try
			{
				GD.Print($"[GameSessionManager] 正在初始化游戏会话: {room.Name}");

				_isHost = room.HostId == AuthSystem.Instance?.CurrentUser?.Id;

				_gameState = new GameState
				{
					RoomId = room.Id,
					SyncSeed = uint.Parse(room.Seed.Replace("-", "").Substring(0, 8),
						System.Globalization.NumberStyles.HexNumber),
					CurrentTurn = 0,
					CurrentPhase = "setup"
				};

				foreach (var player in room.Players)
				{
					_gameState.Players[player.Id] = new PlayerState
					{
						UserId = player.Id,
						CharacterId = player.CharacterId ?? "",
						CurrentHP = 80,
						MaxHP = 80,
						CurrentEnergy = 3,
						MaxEnergy = 3,
						Gold = 99,
						Block = 0,
						IsAlive = true
					};
				}

				bool success = await RoomManager.Instance.StartGameAsync();

				if (success)
				{
					_syncTimer.Start();

					GD.Print($"[GameSessionManager] ✓ 游戏会话已启动 (种子: {_gameState.SyncSeed})");

					EmitSignal(SignalName.GameStarted, _gameState.SyncSeed);

					if (_isHost)
					{
						await BroadcastInitialGameState();
					}
				}
				else
				{
					GD.PrintErr("[GameSessionManager] ✗ 服务器未确认游戏开始");
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[GameSessionManager] 启动游戏会话异常: {ex.Message}");
			}
		}

		public async Task EndGameSessionAsync(bool victory)
		{
			try
			{
				GD.Print($"[GameSessionManager] 正在结束游戏会话 (胜利: {victory})");

				_syncTimer.Stop();

				var endPacket = PacketSerializer.SerializePacket(PacketType.GameEnd, new
				{
					roomId = _gameState.RoomId,
					victory,
					finalTurn = _gameState.CurrentTurn,
					timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
				});

				await ConnectionManager.Instance.SendAsync(endPacket);

				NetworkManager.Instance.UpdateState(NetworkState.InLobby);

				EmitSignal(SignalName.GameEnded, victory);

				CleanupSession();

				GD.Print("[GameSessionManager] ✓ 游戏会话已结束");
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[GameSessionManager] 结束游戏会话异常: {ex.Message}");
			}
		}

		public async Task SendPlayerActionAsync(string cardId, string actionType, object? target = null)
		{
			if (!IsInGame) return;

			var playerAction = new CardAction
			{
				PlayerId = AuthSystem.Instance?.CurrentUser?.Id ?? "",
				CardId = cardId,
				Action = actionType,
				Target = target
			};

			_actionQueue.Enqueue(playerAction);

			var actionPacket = PacketSerializer.SerializePacket(PacketType.GameInput, playerAction);

			await ConnectionManager.Instance.SendAsync(actionPacket);

			GD.Print($"[GameSessionManager] 发送玩家操作: {actionType} - {cardId}");

			EmitSignal(SignalName.PlayerActionReceived, playerAction);
		}

		public async Task SendEndTurnAsync()
		{
			await SendPlayerActionAsync("", "end_turn");
		}

		private async Task OnSyncTick()
		{
			if (!IsInGame) return;

			if (_isHost && _actionQueue.Count > 0)
			{
				await ProcessAndBroadcastActions();
			}

			_gameState.LastUpdated = DateTime.UtcNow;
		}

		private async Task ProcessAndBroadcastActions()
		{
			while (_actionQueue.Count > 0)
			{
				var action = _actionQueue.Dequeue();

				action.IsValidated = ValidateAction(action);

				ApplyActionToState(action);

				var stateUpdate = PacketSerializer.SerializePacket(
					PacketType.GameStateSync,
					new
					{
						state = _gameState,
						lastAction = action
					}
				);

				await ConnectionManager.Instance.SendAsync(stateUpdate);
			}
		}

		private bool ValidateAction(CardAction action)
		{
			if (!_gameState.Players.ContainsKey(action.PlayerId))
			{
				GD.PrintErr($"[GameSessionManager] 无效操作: 玩家不存在 - {action.PlayerId}");
				return false;
			}

			var player = _gameState.Players[action.PlayerId];

			switch (action.Action.ToLower())
			{
				case "play":
					if (!player.IsAlive) return false;
					if (player.HandCards.Count <= 0) return false;
					if (player.CurrentEnergy < 1) return false;
					break;

				case "end_turn":
					if (!_isHost) return false;
					break;

				default:
					GD.PrintWarn($"[GameSessionManager] 未知操作类型: {action.Action}");
					return false;
			}

			return true;
		}

		private void ApplyActionToState(CardAction action)
		{
			if (!_gameState.Players.ContainsKey(action.PlayerId)) return;

			var player = _gameState.Players[action.PlayerId];

			switch (action.Action.ToLower())
			{
				case "play":
					player.HandCards.Remove(action.CardId);
					player.DiscardPile.Add(action.CardId);
					player.CurrentEnergy--;
					break;

				case "end_turn":
					_gameState.CurrentTurn++;
					EmitSignal(SignalName.TurnChanged, _gameState.CurrentTurn);
					break;
			}
		}

		public void ApplyRemoteState(object stateData)
		{
			try
			{
				if (stateData is Godot.Collections.Dictionary stateDict)
				{
					if (stateDict.ContainsKey("currentTurn"))
					{
						int newTurn = stateDict["currentTurn"].AsInt32();
						if (newTurn != _gameState.CurrentTurn)
						{
							_gameState.CurrentTurn = newTurn;
							EmitSignal(SignalName.TurnChanged, newTurn);
						}
					}

					if (stateDict.ContainsKey("players"))
					{
						var playersDict = stateDict["players"].AsGodotDictionary();
						foreach (var playerId in playersDict.Keys)
						{
							string key = playerId.AsString();
							if (_gameState.Players.ContainsKey(key))
							{
								UpdatePlayerState(_gameState.Players[key], playersDict[key]);
							}
						}
					}

					_gameState.LastUpdated = DateTime.UtcNow;

					EmitSignal(SignalName.StateSynced, _gameState);

					GD.Print("[GameSessionManager] 远程状态已同步");
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[GameSessionManager] 应用远程状态失败: {ex.Message}");
			}
		}

		private void UpdatePlayerState(PlayerState playerState, object data)
		{
			if (data is Godot.Collections.Dictionary dict)
			{
				if (dict.ContainsKey("currentHp")) playerState.CurrentHP = dict["currentHp"].AsInt32();
				if (dict.ContainsKey("maxHp")) playerState.MaxHP = dict["maxHp"].AsInt32();
				if (dict.ContainsKey("currentEnergy")) playerState.CurrentEnergy = dict["currentEnergy"].AsInt32();
				if (dict.ContainsKey("gold")) playerState.Gold = dict["gold"].AsInt32();
				if (dict.ContainsKey("block")) playerState.Block = dict["block"].AsInt32();
				if (dict.ContainsKey("isAlive")) playerState.IsAlive = dict["isAlive"].AsBool();
			}
		}

		private async Task BroadcastInitialGameState()
		{
			var initialPacket = PacketSerializer.SerializePacket(
				PacketType.GameStart,
				new
				{
					seed = _gameState.SyncSeed,
					initialState = _gameState,
					message = "游戏已开始，请同步初始状态"
				}
			);

			await ConnectionManager.Instance.SendAsync(initialPacket);

			GD.Print("[GameSessionManager] 已广播初始游戏状态");
		}

		public PlayerState? GetLocalPlayerState()
		{
			var localUserId = AuthSystem.Instance?.CurrentUser?.Id;
			if (localUserId == null || !_gameState.Players.ContainsKey(localUserId))
			{
				return null;
			}

			return _gameState.Players[localUserId];
		}

		public PlayerState? GetPlayerState(string userId)
		{
			return _gameState.Players.GetValueOrDefault(userId);
		}

		public List<PlayerState> GetAllAlivePlayers()
		{
			var alivePlayers = new List<PlayerState>();
			foreach (var player in _gameState.Players.Values)
			{
				if (player.IsAlive)
				{
					alivePlayers.Add(player);
				}
			}
			return alivePlayers;
		}

		public bool CheckVictoryCondition()
		{
			int aliveCount = 0;
			foreach (var player in _gameState.Players.Values)
			{
				if (player.IsAlive) aliveCount++;
			}

			if (_isHost && aliveCount <= 1)
			{
				var localPlayer = GetLocalPlayerState();
				return localPlayer?.IsAlive ?? false;
			}

			return false;
		}

		public bool CheckDefeatCondition()
		{
			var localPlayer = GetLocalPlayerState();
			return localPlayer != null && !localPlayer.IsAlive;
		}

		private void CleanupSession()
		{
			_gameState = new GameState();
			_actionQueue.Clear();
			_isHost = false;
		}

		public override void _ExitTree()
		{
			_syncTimer?.Stop();
			CleanupSession();
			_instance = null;
		}
	}
}
