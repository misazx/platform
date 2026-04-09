using System;
using System.Collections.Generic;
using Godot;
using RoguelikeGame.Core;
using RoguelikeGame.Database;
using RoguelikeGame.Network.Session;

namespace RoguelikeGame.Core
{
	public class CombatNetworkSync : Node
	{
		private static CombatNetworkSync _instance;

		public static CombatNetworkSync Instance => _instance;

		private CombatManager _combatManager;
		private GameSessionManager _sessionManager;
		private bool _isNetworkMode = false;
		private bool _isHost = false;
		private Queue<NetworkAction> _pendingActions = new();
		private Dictionary<string, PlayerState> _remotePlayerStates = new();
		private Timer _syncTimer;
		private int _lastConfirmedTurn = -1;

		public bool IsNetworkMode => _isNetworkMode;
		public bool IsHost => _isHost;

		[Signal]
		public delegate void RemoteCardPlayedEventHandler(string playerId, string cardId, int targetIndex);
		[Signal]
		public delegate void RemoteTurnEndedEventHandler(string playerId);
		[Signal]
		public delegate void NetworkStateChangedEventHandler(string message);

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

			GD.Print("[CombatNetworkSync] 战斗网络同步组件已初始化");
		}

		private void InitializeComponents()
		{
			_combatManager = CombatManager.Instance;
			_sessionManager = GameSessionManager.Instance;

			_syncTimer = new Timer
			{
				WaitTime = 0.1,
				OneShot = false,
				Autostart = false
			};
			_syncTimer.Connect("timeout", new Callable(this, nameof(OnSyncTick)));
			AddChild(_syncTimer);
		}

		public void EnableNetworkMode(bool isHost)
		{
			if (GameSessionManager.Instance?.IsInGame ?? false)
			{
				_isNetworkMode = true;
				_isHost = isHost;

				if (_combatManager != null)
				{
					_combatManager.CardPlayed += OnLocalCardPlayed;
					_combatManager.TurnEnded += OnLocalTurnEnded;
				}

				_syncTimer.Start();

				GD.Print($"[CombatNetworkSync] ✓ 网络模式已启用 (主机: {_isHost})");

				EmitSignal(SignalName.NetworkStateChanged, $"网络对战模式已启用 - {(isHost ? "你是房主" : "你是客户端")}");
			}
			else
			{
				GD.PrintErr("[CombatNetworkSync] 无法启用: 不在游戏会话中");
			}
		}

		public void DisableNetworkMode()
		{
			_isNetworkMode = false;
			_isHost = false;

			if (_combatManager != null)
			{
				_combatManager.CardPlayed -= OnLocalCardPlayed;
				_combatManager.TurnEnded -= OnLocalTurnEnded;
			}

			_syncTimer.Stop();
			_pendingActions.Clear();
			_remotePlayerStates.Clear();

			GD.Print("[CombatNetworkSync] 网络模式已禁用");
		}

		private async void OnLocalCardPlayed(string cardId, string targetName)
		{
			if (!_isNetworkMode) return;

			var action = new NetworkAction
			{
				ActionType = "play_card",
				PlayerId = AuthSystem.Instance?.CurrentUser?.Id ?? "",
				Data = new Dictionary<string, object>
				{
					{ "cardId", cardId },
					{ "targetName", targetName },
					{ "timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
				}
			};

			await SendActionToServer(action);
		}

		private async void OnLocalTurnEnded(int turn)
		{
			if (!_isNetworkMode) return;

			var action = new NetworkAction
			{
				ActionType = "end_turn",
				PlayerId = AuthSystem.Instance?.CurrentUser?.Id ?? "",
				Data = new Dictionary<string, object>
				{
					{ "turnNumber", turn },
					{ "timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
				}
			};

			await SendActionToServer(action);
		}

		private async Task SendActionToServer(NetworkAction action)
		{
			try
			{
				_pendingActions.Enqueue(action);

				var packetData = PacketSerializer.SerializePacket(PacketType.GameInput, action);
				await ConnectionManager.Instance.SendAsync(packetData);

				GD.Print($"[CombatNetworkSync] 发送操作: {action.ActionType}");
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[CombatNetworkSync] 发送操作失败: {ex.Message}");
				EmitSignal(SignalName.NetworkStateChanged, $"操作发送失败: {ex.Message}");
			}
		}

		private void OnSyncTick()
		{
			if (!_isNetworkMode || _sessionManager == null) return;

			if (_isHost)
			{
				ProcessPendingActionsAsHost();
			}

			SyncLocalStateToSession();
		}

		private void ProcessPendingActionsAsHost()
		{
			while (_pendingActions.Count > 0)
			{
				var action = _pendingActions.Dequeue();

				bool isValid = ValidateRemoteAction(action);
				if (!isValid) continue;

				ApplyRemoteAction(action);
				BroadcastActionToClients(action);
			}
		}

		private bool ValidateRemoteAction(NetworkAction action)
		{
			if (_combatManager?.State == null) return false;

			switch (action.ActionType.ToLower())
			{
				case "play_card":
					if (!_combatManager.State.CanPlayCards) return false;
					break;

				case "end_turn":
					break;

				default:
					return false;
			}

			return true;
		}

		private void ApplyRemoteAction(NetworkAction action)
		{
			switch (action.ActionType.ToLower())
			{
				case "play_card":
					string cardId = action.Data.GetValueOrDefault("cardId", "")?.ToString() ?? "";
					string targetName = action.Data.GetValueOrDefault("targetName", "")?.ToString() ?? "";

					EmitSignal(SignalName.RemoteCardPlayed, action.PlayerId, cardId, 0);
					GD.Print($"[CombatNetworkSync] 远程玩家出牌: {action.PlayerId} -> {cardId}");
					break;

				case "end_turn":
					EmitSignal(SignalName.RemoteTurnEnded, action.PlayerId);
					GD.Print($"[CombatNetworkSync] 远程玩家结束回合: {action.PlayerId}");
					break;
			}
		}

		private async Task BroadcastActionToClients(NetworkAction action)
		{
			try
			{
				var stateUpdate = PacketSerializer.SerializePacket(
					PacketType.GameStateSync,
					new
					{
						action = action,
						confirmedTurn = _combatManager?.State.TurnNumber ?? 0
					}
				);

				await ConnectionManager.Instance.SendAsync(stateUpdate);
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[CombatNetworkSync] 广播操作失败: {ex.Message}");
			}
		}

		public void ReceiveRemoteState(object stateData)
		{
			if (!_isNetworkMode) return;

			try
			{
				if (stateData is Godot.Collections.Dictionary stateDict)
				{
					if (stateDict.ContainsKey("action"))
					{
						var actionDict = stateDict["action"].AsGodotDictionary();
						var remoteAction = ParseNetworkAction(actionDict);

						if (remoteAction != null && remoteAction.PlayerId != (AuthSystem.Instance?.CurrentUser?.Id ?? ""))
						{
							ApplyRemoteAction(remoteAction);
						}
					}

					if (stateDict.ContainsKey("confirmedTurn"))
					{
						int confirmedTurn = stateDict["confirmedTurn"].AsInt32();
						if (confirmedTurn > _lastConfirmedTurn)
						{
							_lastConfirmedTurn = confirmedTurn;
							GD.Print($"[CombatNetworkSync] 状态已确认至回合 {confirmedTurn}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[CombatNetworkSync] 处理远程状态失败: {ex.Message}");
			}
		}

		private NetworkAction ParseNetworkAction(Godot.Collections.Dictionary dict)
		{
			try
			{
				return new NetworkAction
				{
					ActionType = dict.ContainsKey("actionType") ? dict["actionType"].AsString() : "",
					PlayerId = dict.ContainsKey("playerId") ? dict["playerId"].AsString() : "",
					Data = new Dictionary<string, object>()
				};
			}
			catch
			{
				return null;
			}
		}

		private void SyncLocalStateToSession()
		{
			if (_sessionManager == null || _combatManager?.State == null) return;

			var localUserId = AuthSystem.Instance?.CurrentUser?.Id ?? "";
			var localState = _sessionManager.GetLocalPlayerState();

			if (localState != null)
			{
				localState.CurrentHP = GetPlayerHealth();
				localState.MaxHP = GetPlayerMaxHealth();
				localState.CurrentEnergy = _combatManager.State.CurrentEnergy;
				localState.MaxEnergy = _combatManager.State.MaxEnergy;
				localState.Block = _combatManager.State.CurrentBlock;
				localState.HandCards = GetHandCardIds();
				localState.IsAlive = IsPlayerAlive();

				_remotePlayerStates[localUserId] = localState;
			}
		}

		private int GetPlayerHealth()
		{
			if (_player == null && _combatManager?._player != null)
			{
				_player = _combatManager._player;
			}

			if (_player != null && _player.HasMethod("GetCurrentHealth"))
			{
				var healthVariant = _player.Call("GetCurrentHealth");
				if (healthVariant.VariantType == Variant.Type.Int)
				{
					return healthVariant.AsInt32();
				}
			}

			return _combatManager?.State?.CurrentHP ?? 80;
		}

		private int GetPlayerMaxHealth()
		{
			if (_player != null && _player.HasMethod("GetMaxHealth"))
			{
				var healthVariant = _player.Call("GetMaxHealth");
				if (healthVariant.VariantType == Variant.Type.Int)
				{
					return healthVariant.AsInt32();
				}
			}

			return _combatManager?.State?.MaxHP ?? 80;
		}

		private List<string> GetHandCardIds()
		{
			var cardIds = new List<string>();

			if (_combatManager?.State?.Hand != null)
			{
				foreach (var card in _combatManager.State.Hand)
				{
					cardIds.Add(card.Id);
				}
			}

			return cardIds;
		}

		private bool IsPlayerAlive()
		{
			return GetPlayerHealth() > 0;
		}

		public PlayerState GetRemotePlayerState(string playerId)
		{
			return _remotePlayerStates.GetValueOrDefault(playerId);
		}

		public Dictionary<string, PlayerState> GetAllRemoteStates()
		{
			return new Dictionary<string, PlayerState>(_remotePlayerStates);
		}

		public override void _ExitTree()
		{
			DisableNetworkMode();
			_instance = null;
		}
	}

	public class NetworkAction
	{
		public string ActionType { get; set; } = "";
		public string PlayerId { get; set; } = "";
		public Dictionary<string, object> Data { get; set; } = new();
		public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	}
}
