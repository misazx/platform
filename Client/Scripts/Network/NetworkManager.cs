using System;
using System.Threading.Tasks;
using Godot;
using RoguelikeGame.Network.Core;

namespace RoguelikeGame.Network
{
	public enum NetworkState
	{
		Disconnected,
		Connecting,
		Connected,
		Authenticating,
		Authenticated,
		InLobby,
		InRoom,
		InGame
	}

	public partial class NetworkManager : SingletonBase<NetworkManager>
	{
		private ConnectionManager _connectionManager;

		private NetworkState _state = NetworkState.Disconnected;
		private ConnectionMode _mode = ConnectionMode.Offline;

		private string _currentUserId = "";
		private string _currentRoomId = "";
		private string _serverAddress = "127.0.0.1";
		private int _serverPort = 5000;

		public NetworkState State => _state;
		public ConnectionMode Mode => _mode;
		public bool IsOnline => _mode != ConnectionMode.Offline && _state != NetworkState.Disconnected;
		public string UserId => _currentUserId;
		public string RoomId => _currentRoomId;

		[Signal]
		public delegate void StateChangedEventHandler(NetworkState newState, NetworkState oldState);

		[Signal]
		public delegate void ConnectionSucceededEventHandler();

		[Signal]
		public delegate void ConnectionFailedEventHandler(string error);

		[Signal]
		public delegate void AuthenticationCompletedEventHandler(bool success, string message);

		[Signal]
		public delegate void RoomJoinedEventHandler(string roomId);

		[Signal]
		public delegate void RoomLeftEventHandler();

		[Signal]
		public delegate void GameStartedEventHandler();

		[Signal]
		public delegate void GameEndedEventHandler(bool victory);

		public override void _Ready()
		{
			base._Ready();

			GD.Print("[NetworkManager] 初始化网络管理器...");

			InitializeConnectionManager();
			RegisterEventHandlers();

			GD.Print("[NetworkManager] ✓ 网络管理器就绪");
		}

		private void InitializeConnectionManager()
		{
			_connectionManager = new ConnectionManager();
			AddChild(_connectionManager);
		}

		private void RegisterEventHandlers()
		{
			EventBus.Instance.NetworkConnected += OnNetworkConnected;
			EventBus.Instance.NetworkDisconnected += OnNetworkDisconnected;
			EventBus.Instance.NetworkError += OnNetworkError;
			EventBus.Instance.NetworkDataReceived += OnNetworkDataReceived;
		}

		public async Task<bool> ConnectToLANAsync(string hostAddress = "127.0.0.1", int port = 5000)
		{
			return await ConnectAsync(ConnectionMode.LAN, hostAddress, port);
		}

		public async Task<bool> ConnectToOnlineAsync(string serverUrl = "127.0.0.1", int port = 5000)
		{
			return await ConnectAsync(ConnectionMode.Online, serverUrl, port);
		}

		private async Task<bool> ConnectAsync(ConnectionMode mode, string address, int port)
		{
			if (_state != NetworkState.Disconnected)
			{
				GD.PrintErr("[NetworkManager] 当前状态不允许连接: {_state}");
				return false;
			}

			UpdateState(NetworkState.Connecting);

			_mode = mode;
			_serverAddress = address;
			_serverPort = port;

			GD.Print($"[NetworkManager] 正在通过 {mode} 模式连接到 {address}:{port}...");

			bool success = await _connectionManager.ConnectAsync(mode, address, port);

			if (success)
			{
				UpdateState(NetworkState.Connected);
				EmitSignal(SignalName.ConnectionSucceeded);
				return true;
			}
			else
			{
				UpdateState(NetworkState.Disconnected);
				EmitSignal(SignalName.ConnectionFailed, $"无法连接到 {address}:{port}");
				return false;
			}
		}

		public async Task DisconnectAsync()
		{
			GD.Print("[NetworkManager] 正在断开连接...");

			await _connectionManager.DisconnectAsync();

			_currentUserId = "";
			_currentRoomId = "";
			_mode = ConnectionMode.Offline;

			UpdateState(NetworkState.Disconnected);

			GD.Print("[NetworkManager] ✓ 已断开连接");
		}

		public void SetOfflineMode()
		{
			if (IsOnline)
			{
				DisconnectAsync().Wait();
			}

			_mode = ConnectionMode.Offline;
			UpdateState(NetworkState.Disconnected);

			GD.Print("[NetworkManager] 已切换到单机模式");
		}

		public bool CanPerformAction(string action)
		{
			switch (action.ToLower())
			{
				case "login":
				case "register":
					return _state == NetworkState.Connected;

				case "create_room":
				case "join_room":
				case "list_rooms":
					return _state == NetworkState.Authenticated ||
						   _state == NetworkState.InLobby;

				case "start_game":
					return _state == NetworkState.InRoom;

				case "send_game_data":
					return _state == NetworkState.InGame;

				default:
					return IsOnline;
			}
		}

		private void UpdateState(NetworkState newState)
		{
			var oldState = _state;
			_state = newState;

			GD.Print($"[NetworkManager] 状态变更: {oldState} → {newState}");

			EmitSignal(SignalName.StateChanged, (int)newState, (int)oldState);
		}

		private void OnNetworkConnected()
		{
			GD.Print("[NetworkManager] 网络层已连接");

			if (_state == NetworkState.Connecting)
			{
				UpdateState(NetworkState.Connected);
			}
		}

		private void OnNetworkDisconnected()
		{
			GD.Print("[NetworkManager] 网络层已断开");

			if (_state != NetworkState.Disconnected)
			{
				UpdateState(NetworkState.Disconnected);
				EmitSignal(SignalName.ConnectionLost, "连接意外断开");
			}
		}

		private void OnNetworkError(string error)
		{
			GD.PrintErr($"[NetworkManager] 网络错误: {error}");
			EmitSignal(SignalName.ConnectionFailed, error);
		}

		private void OnNetworkDataReceived(byte[] data)
		{
			GD.Print($"[NetworkManager] 收到数据包: {data.Length} 字节");

			PacketSerializer.DeserializeAndHandle(data);
		}

		public override void _ExitTree()
		{
			if (IsOnline)
			{
				DisconnectAsync().Wait();
			}
		}
	}
}
