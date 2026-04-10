using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using RoguelikeGame.Core;
using RoguelikeGame.Network;
using RoguelikeGame.Network.Auth;
using RoguelikeGame.Network.Core;
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

				_syncTimer.Start();

				GD.Print($"[CombatNetworkSync] ✓ 网络模式已启用 (主机: {_isHost})");

				EmitSignal("network_state_changed", $"网络对战模式已启用 - {(isHost ? "你是房主" : "你是客户端")}");
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

			_syncTimer.Stop();
			_pendingActions.Clear();
			_remotePlayerStates.Clear();

			GD.Print("[CombatNetworkSync] 网络模式已禁用");
		}

		private async Task SendActionToServer(NetworkAction action)
		{
			try
			{
				_pendingActions.Enqueue(action);

				var jsonData = JsonSerializer.Serialize(new { type = "combat_action", action });
				var bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

				if (NetworkManager.Instance != null)
				{
					var connMgr = NetworkManager.Instance.GetChild(0) as RoguelikeGame.Network.Core.ConnectionManager;
					if (connMgr != null) await connMgr.SendAsync(bytes);
				}

				GD.Print($"[CombatNetworkSync] 发送操作: {action.ActionType}");
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[CombatNetworkSync] 发送操作失败: {ex.Message}");
				EmitSignal("network_state_changed", $"操作发送失败: {ex.Message}");
			}
		}

		private void OnSyncTick()
		{
			if (!_isNetworkMode || _sessionManager == null) return;

			if (_isHost && _pendingActions.Count > 0)
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
			}
		}

		private bool ValidateRemoteAction(NetworkAction action)
		{
			switch (action.ActionType.ToLower())
			{
				case "play_card":
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
					EmitSignal("remote_card_played", action.PlayerId, cardId, 0);
					GD.Print($"[CombatNetworkSync] 远程玩家出牌: {action.PlayerId} -> {cardId}");
					break;

				case "end_turn":
					EmitSignal("remote_turn_ended", action.PlayerId);
					GD.Print($"[CombatNetworkSync] 远程玩家结束回合: {action.PlayerId}");
					break;
			}
		}

		public void ReceiveRemoteState(object stateData)
		{
			if (!_isNetworkMode) return;

			try
			{
				if (stateData is Godot.Collections.Dictionary stateDict)
				{
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

		private void SyncLocalStateToSession()
		{
			if (_sessionManager == null || _combatManager?.State == null) return;

			var localUserId = AuthSystem.Instance?.CurrentUser?.Id ?? "";
			var localState = _sessionManager.GetLocalPlayerState();

			if (localState != null)
			{
				localState.CurrentHP = 80;
				localState.MaxHP = 80;
				localState.CurrentEnergy = 3;
				localState.MaxEnergy = 3;
				localState.Block = 0;
				localState.IsAlive = true;

				_remotePlayerStates[localUserId] = localState;
			}
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
