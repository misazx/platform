using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using RoguelikeGame.Core;
using RoguelikeGame.Network.Auth;
using RoguelikeGame.Network.Rooms;
using RoguelikeGame.Network.Core;

namespace RoguelikeGame.Network.Session
{
	public class GameState
	{
		public string RoomId { get; set; } = "";
		public uint SyncSeed { get; set; }
		public int CurrentTurn { get; set; } = 0;
		public string CurrentPhase { get; set; } = "";
		public Dictionary<string, PlayerState> Players { get; set; } = new();
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

	public partial class GameSessionManager : Node
	{
		private static GameSessionManager _instance;

		public static GameSessionManager Instance => _instance;

		private GameState _gameState;
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
		public delegate void StateSyncedEventHandler(string roomId, int currentTurn);

		[Signal]
		public delegate void TurnChangedEventHandler(int newTurn);

		[Signal]
		public delegate void PlayerActionReceivedEventHandler(string playerId, string actionType);

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
					SyncSeed = (uint)DateTime.UtcNow.Ticks,
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

				var endData = JsonSerializer.Serialize(new
				{
					type = "game_end",
					roomId = _gameState.RoomId,
					victory,
					finalTurn = _gameState.CurrentTurn,
					timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
				});

				var bytes = System.Text.Encoding.UTF8.GetBytes(endData);
				if (NetworkManager.Instance != null)
				{
					var connMgr = NetworkManager.Instance.GetChild(0) as ConnectionManager;
					if (connMgr != null) await connMgr.SendAsync(bytes);
				}

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

			var playerAction = new
			{
				playerId = AuthSystem.Instance?.CurrentUser?.Id ?? "",
				cardId,
				action = actionType,
				target,
				timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
			};

			var jsonData = JsonSerializer.Serialize(new
			{
				type = "game_input",
				action = playerAction
			});

			var bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
			if (NetworkManager.Instance != null)
			{
				var connMgr = NetworkManager.Instance.GetChild(0) as ConnectionManager;
				if (connMgr != null) await connMgr.SendAsync(bytes);
			}

			GD.Print($"[GameSessionManager] 发送玩家操作: {actionType} - {cardId}");

			EmitSignal(SignalName.PlayerActionReceived, playerAction.playerId, actionType);
		}

		public async Task SendEndTurnAsync()
		{
			await SendPlayerActionAsync("", "end_turn");
		}

		private async Task OnSyncTick()
		{
			if (!IsInGame) return;

			_gameState.LastUpdated = DateTime.UtcNow;
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
						var playersDict = (Godot.Collections.Dictionary)stateDict["players"];
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

					EmitSignal(SignalName.StateSynced, _gameState.RoomId, _gameState.CurrentTurn);

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
