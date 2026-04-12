using System;
using System.Threading.Tasks;
using Godot;
using RoguelikeGame.Network.Auth;
using RoguelikeGame.Network.Rooms;
using RoguelikeGame.Network.Session;

namespace RoguelikeGame.Network
{
	public enum GameModeType
	{
		SinglePlayer,
		LANMultiplayer,
		OnlineMultiplayer
	}

	public partial class OfflineModeManager : Node
	{
		private static OfflineModeManager _instance;

		public static OfflineModeManager Instance => _instance;

		private GameModeType _currentMode = GameModeType.SinglePlayer;
		private bool _isTransitioning = false;

		public GameModeType CurrentMode => _currentMode;
		public bool IsSinglePlayer => _currentMode == GameModeType.SinglePlayer;
		public bool IsMultiplayer => _currentMode != GameModeType.SinglePlayer;

		[Signal]
		public delegate void ModeChangedEventHandler(GameModeType newMode, GameModeType oldMode);
		[Signal]
		public delegate void ModeTransitionStartedEventHandler(GameModeType targetMode);
		[Signal]
		public delegate void ModeTransitionCompletedEventHandler(bool success);

		public override void _Ready()
		{
			if (_instance != null && _instance != this)
			{
				QueueFree();
				return;
			}

			_instance = this;
			ProcessMode = ProcessModeEnum.Always;

			GD.Print("[OfflineModeManager] 单机模式兼容层已初始化");
		}

		public async Task<bool> SwitchToSinglePlayerAsync()
		{
			if (_isTransitioning) return false;

			var oldMode = _currentMode;

			if (oldMode == GameModeType.SinglePlayer) return true;

			EmitSignal(SignalName.ModeTransitionStarted, (int)GameModeType.SinglePlayer);
			_isTransitioning = true;

			try
			{
				GD.Print("[OfflineModeManager] 正在切换到单机模式...");

				if (NetworkManager.Instance?.IsOnline ?? false)
				{
					await NetworkManager.Instance.DisconnectAsync();
				}

				NetworkManager.Instance?.SetOfflineMode();

				_currentMode = GameModeType.SinglePlayer;

				GD.Print("[OfflineModeManager] ✓ 已切换到单机模式");

				EmitSignal(SignalName.ModeChanged, (int)_currentMode, (int)oldMode);
				EmitSignal(SignalName.ModeTransitionCompleted, true);

				return true;
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[OfflineModeManager] 切换到单机模式失败: {ex.Message}");
				EmitSignal(SignalName.ModeTransitionCompleted, false);
				return false;
			}
			finally
			{
				_isTransitioning = false;
			}
		}

		public async Task<bool> SwitchToLANAsync()
		{
			if (_isTransitioning) return false;

			var oldMode = _currentMode;

			EmitSignal(SignalName.ModeTransitionStarted, (int)GameModeType.LANMultiplayer);
			_isTransitioning = true;

			try
			{
				GD.Print("[OfflineModeManager] 正在切换到局域网多人模式...");

				bool connected = await NetworkManager.Instance.ConnectToLANAsync();

				if (connected)
				{
					_currentMode = GameModeType.LANMultiplayer;

					GD.Print("[OfflineModeManager] ✓ 已切换到局域网多人模式");

					EmitSignal(SignalName.ModeChanged, (int)_currentMode, (int)oldMode);
					EmitSignal(SignalName.ModeTransitionCompleted, true);

					return true;
				}
				else
				{
					GD.PrintErr("[OfflineModeManager] ✗ 局域网连接失败");

					EmitSignal(SignalName.ModeTransitionCompleted, false);
					return false;
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[OfflineModeManager] 切换到局域网模式失败: {ex.Message}");
				EmitSignal(SignalName.ModeTransitionCompleted, false);
				return false;
			}
			finally
			{
				_isTransitioning = false;
			}
		}

		public async Task<bool> SwitchToOnlineAsync(string serverUrl = "127.0.0.1", int port = 5000)
		{
			if (_isTransitioning) return false;

			var oldMode = _currentMode;

			EmitSignal(SignalName.ModeTransitionStarted, (int)GameModeType.OnlineMultiplayer);
			_isTransitioning = true;

			try
			{
				GD.Print("[OfflineModeManager] 正在切换到在线多人模式...");

				AuthSystem.Instance.SetBaseUrl($"http://{serverUrl}:{port}");
				RoomManager.Instance.SetBaseUrl($"http://{serverUrl}:{port}");

				bool connected = await NetworkManager.Instance.ConnectToOnlineAsync(serverUrl, port);

				if (connected)
				{
					_currentMode = GameModeType.OnlineMultiplayer;

					GD.Print("[OfflineModeManager] ✓ 已切换到在线多人模式");

					EmitSignal(SignalName.ModeChanged, (int)_currentMode, (int)oldMode);
					EmitSignal(SignalName.ModeTransitionCompleted, true);

					return true;
				}
				else
				{
					GD.PrintErr("[OfflineModeManager] ✗ 在线连接失败");

					EmitSignal(SignalName.ModeTransitionCompleted, false);
					return false;
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[OfflineModeManager] 切换到在线模式失败: {ex.Message}");
				EmitSignal(SignalName.ModeTransitionCompleted, false);
				return false;
			}
			finally
			{
				_isTransitioning = false;
			}
		}

		public T ExecuteWithModeCheck<T>(Func<T> singlePlayerAction, Func<T> multiplayerAction)
		{
			if (IsSinglePlayer)
			{
				return singlePlayerAction();
			}
			else
			{
				return multiplayerAction();
			}
		}

		public async Task<T> ExecuteAsyncWithModeCheck<T>(Func<Task<T>> singlePlayerAction, Func<Task<T>> multiplayerAction)
		{
			if (IsSinglePlayer)
			{
				return await singlePlayerAction();
			}
			else
			{
				return await multiplayerAction();
			}
		}

		public void ExecuteWithModeCheck(Action singlePlayerAction, Action multiplayerAction)
		{
			if (IsSinglePlayer)
			{
				singlePlayerAction();
			}
			else
			{
				multiplayerAction();
			}
		}

		public bool CanUseNetworkFeatures()
		{
			return IsMultiplayer && NetworkManager.Instance?.IsOnline == true && AuthSystem.Instance?.IsAuthenticated == true;
		}

		public bool ShouldSyncState()
		{
			return IsMultiplayer && GameSessionManager.Instance?.IsInGame == true;
		}

		public string GetModeDisplayName()
		{
			switch (_currentMode)
			{
				case GameModeType.SinglePlayer:
					return "🎮 单机模式";
				case GameModeType.LANMultiplayer:
					return "🌐 局域网联机";
				case GameModeType.OnlineMultiplayer:
					return "🌍 在线多人";
				default:
					return "未知模式";
			}
		}

		public override void _ExitTree()
		{
			_instance = null;
		}
	}
}
