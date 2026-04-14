using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using RoguelikeGame.Network.Core;
using RoguelikeGame.Network.Discovery;

namespace RoguelikeGame.Network
{
	public partial class MultiProtocolManager : Node
	{
		private static MultiProtocolManager _instance;

		public static MultiProtocolManager Instance => _instance;

		private ConnectionManager _connectionManager;
		private LANDiscoveryService _lanDiscovery;

		private ConnectionMode _preferredMode = ConnectionMode.Offline;
		private List<ConnectionMode> _availableModes = new();

		public event Action<LANHostInfo> OnHostDiscovered;
		public event Action<string> OnHostLost;
		public event Action<ConnectionMode> OnModeChanged;

		public IReadOnlyList<ConnectionMode> AvailableModes => _availableModes.AsReadOnly();

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
			DetectAvailableModes();

			GD.Print("[MultiProtocolManager] 多协议管理器初始化完成");
		}

		private void InitializeComponents()
		{
			_connectionManager = ConnectionManager.Instance;
			_lanDiscovery = LANDiscoveryService.Instance;

			_lanDiscovery.HostDiscovered += OnHostDiscoveredHandler;
			_lanDiscovery.HostLost += OnHostLostHandler;
		}

		private void DetectAvailableModes()
		{
			_availableModes.Clear();
			_availableModes.Add(ConnectionMode.Offline);

			if (OS.HasFeature("pc") || OS.HasFeature("mobile"))
			{
				_availableModes.Add(ConnectionMode.LAN);
				_availableModes.Add(ConnectionMode.Online);
			}

			if (OS.HasFeature("mobile") || OS.HasFeature("web"))
			{
				_availableModes.Add(ConnectionMode.Bluetooth);
			}

			GD.Print("[MultiProtocolManager] 可用协议模式:");
			foreach (var mode in _availableModes)
			{
				GD.Print($"  ✓ {mode}");
			}
		}

		public async Task<bool> ConnectToBestAvailableAsync(string targetAddress = "127.0.0.1", int targetPort = 5000)
		{
			GD.Print("[MultiProtocolManager] 尝试最佳可用连接...");

			var priorityOrder = new[]
			{
				ConnectionMode.LAN,
				ConnectionMode.Online,
				ConnectionMode.Bluetooth
			};

			foreach (var mode in priorityOrder)
			{
				if (!_availableModes.Contains(mode)) continue;

				bool success = await ConnectWithModeAsync(mode, targetAddress, targetPort);
				if (success)
				{
					SetPreferredMode(mode);
					return true;
				}

				GD.Print($"[MultiProtocolManager] {mode} 模式不可用，尝试下一个...");
			}

			GD.PrintErr("[MultiProtocolManager] 所有协议模式均无法连接");
			return false;
		}

		public async Task<bool> ConnectWithModeAsync(ConnectionMode mode, string address, int port)
		{
			if (!_availableModes.Contains(mode))
			{
				GD.PrintErr($"[MultiProtocolManager] 不支持的模式: {mode}");
				return false;
			}

			switch (mode)
			{
				case ConnectionMode.LAN:
					return await ConnectLANAsync(address, port);

				case ConnectionMode.Bluetooth:
					return await ConnectBluetoothAsync(address, port);

				case ConnectionMode.Online:
					return await ConnectOnlineAsync(address, port);

				default:
					GD.PrintErr($"[MultiProtocolManager] 无效的连接模式: {mode}");
					return false;
			}
		}

		private async Task<bool> ConnectLANAsync(string address, int port)
		{
			GD.Print($"[MultiProtocolManager] 尝试局域网连接到 {address}:{port}...");

			bool success = await NetworkManager.Instance.ConnectToLANAsync(address, port);

			if (success)
			{
				GD.Print("[MultiProtocolManager] ✓ 局域网连接成功");
			}

			return success;
		}

		private async Task<bool> ConnectBluetoothAsync(string address, int port)
		{
			GD.Print($"[MultiProtocolManager] 尝试蓝牙/WebSocket连接...");

			try
			{
				var wsAdapter = new WebSocketConnectionAdapter();
				AddChild(wsAdapter);

				bool success = await wsAdapter.ConnectAsync(address, port);

				if (success)
				{
					GD.Print("[MultiProtocolManager] ✓ 蓝牙/WebSocket连接成功");
				}
				else
				{
					wsAdapter.QueueFree();
				}

				return success;
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[MultiProtocolManager] 蓝牙连接失败: {ex.Message}");
				return false;
			}
		}

		private async Task<bool> ConnectOnlineAsync(string address, int port)
		{
			GD.Print($"[MultiProtocolManager] 尝试外网连接到 {address}:{port}...");

			bool success = await NetworkManager.Instance.ConnectToOnlineAsync(address, port);

			if (success)
			{
				GD.Print("[MultiProtocolManager] ✓ 外网连接成功");
			}

			return success;
		}

		public async Task StartLANHostingAsync(int port = 5000, int maxPlayers = 4)
		{
			GD.Print("[MultiProtocolManager] 启动局域网托管...");

			await _lanDiscovery.StartHostingAsync(port, maxPlayers);

			await NetworkManager.Instance.ConnectToLANAsync("127.0.0.1", port);

			SetPreferredMode(ConnectionMode.LAN);

			GD.Print("[MultiProtocolManager] ✓ 局域网托管已启动");
		}

		public async Task StartLANSearchAsync()
		{
			GD.Print("[MultiProtocolManager] 开始搜索局域网主机...");

			await _lanDiscovery.StartSearchAsync();

			GD.Print("[MultiProtocolManager] 局域网搜索已开始");
		}

		public void StopAllOperations()
		{
			GD.Print("[MultiProtocolManager] 停止所有网络操作...");

			_lanDiscovery.StopHosting();
			_lanDiscovery.StopSearch();

			if (NetworkManager.Instance?.IsOnline ?? false)
			{
				NetworkManager.Instance.DisconnectAsync().Wait();
			}

			_preferredMode = ConnectionMode.Offline;

			GD.Print("[MultiProtocolManager] 所有操作已停止");
		}

		public List<LANHostInfo> GetDiscoveredHosts()
		{
			return _lanDiscovery.GetAvailableHosts();
		}

		public void SetPreferredMode(ConnectionMode mode)
		{
			_preferredMode = mode;
			OnModeChanged?.Invoke(mode);

			GD.Print($"[MultiProtocolManager] 首选模式切换为: {mode}");
		}

		public ConnectionMode GetBestModeForEnvironment()
		{
			if (OS.HasFeature("editor") || OS.HasFeature("debug"))
			{
				return ConnectionMode.LAN;
			}

			if (OS.HasFeature("mobile"))
			{
				var hosts = GetDiscoveredHosts();
				if (hosts.Count > 0)
				{
					return ConnectionMode.LAN;
				}
				return ConnectionMode.Bluetooth;
			}

			if (OS.HasFeature("pc"))
			{
				return ConnectionMode.LAN;
			}

			return ConnectionMode.Online;
		}

		private void OnHostDiscoveredHandler(string hostId, string hostName, string address, int port)
		{
			GD.Print($"[MultiProtocolManager] 发现主机: {hostName} @ {address}:{port}");
			var hostInfo = new LANHostInfo { HostName = hostName, Port = port };
			OnHostDiscovered?.Invoke(hostInfo);
		}

		private void OnHostLostHandler(string hostId)
		{
			GD.Print($"[MultiProtocolManager] 主机丢失: {hostId}");
			OnHostLost?.Invoke(hostId);
		}

		public override void _ExitTree()
		{
			StopAllOperations();
			_instance = null;
		}
	}
}
