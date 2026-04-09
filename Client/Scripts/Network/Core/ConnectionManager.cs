using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace RoguelikeGame.Network.Core
{
	public enum ConnectionMode
	{
		Offline,
		LAN,
		Bluetooth,
		Online
	}

	public partial class ConnectionManager : Node
	{
		private static ConnectionManager _instance;

		public static ConnectionManager Instance => _instance;

		public ConnectionMode ActiveMode { get; private set; } = ConnectionMode.Offline;
		public IConnectionMode ActiveAdapter { get; private set; }
		public bool IsConnected => ActiveAdapter?.IsConnected ?? false;

		private Dictionary<ConnectionMode, IConnectionMode> _adapters;

		public override void _Ready()
		{
			if (_instance != null && _instance != this)
			{
				QueueFree();
				return;
			}

			_instance = this;
			ProcessMode = ProcessModeEnum.Always;

			InitializeAdapters();
		}

		private void InitializeAdapters()
		{
			_adapters = new Dictionary<ConnectionMode, IConnectionMode>
			{
				{ ConnectionMode.LAN, new ENetConnectionAdapter() },
				{ ConnectionMode.Online, new ENetConnectionAdapter() }
			};

			GD.Print("[ConnectionManager] 初始化完成，可用协议:");
			foreach (var mode in _adapters.Keys)
			{
				GD.Print($"  - {mode}");
			}
		}

		public async Task<bool> ConnectAsync(ConnectionMode mode, string address, int port)
		{
			if (!_adapters.TryGetValue(mode, out var adapter))
			{
				GD.PrintErr($"[ConnectionManager] 不支持的连接模式: {mode}");
				return false;
			}

			if (ActiveAdapter != null && ActiveAdapter.IsConnected)
			{
				GD.Print("[ConnectionManager] 已有活动连接，先断开...");
				await ActiveAdapter.DisconnectAsync();
			}

			ActiveMode = mode;
			ActiveAdapter = adapter;

			adapter.OnDataReceived += HandleDataReceived;
			adapter.OnConnected += HandleConnected;
			adapter.OnDisconnected += HandleDisconnected;
			adapter.OnError += HandleError;

			bool success = await adapter.ConnectAsync(address, port);

			if (success)
			{
				GD.Print($"[ConnectionManager] ✓ 已通过 {mode} 模式连接到 {address}:{port}");
			}
			else
			{
				GD.Print($"[ConnectionManager] ✗ {mode} 模式连接失败");
				ActiveAdapter = null;
				ActiveMode = ConnectionMode.Offline;
			}

			return success;
		}

		public async Task DisconnectAsync()
		{
			if (ActiveAdapter != null)
			{
				await ActiveAdapter.DisconnectAsync();

				ActiveAdapter.OnDataReceived -= HandleDataReceived;
				ActiveAdapter.OnConnected -= HandleConnected;
				ActiveAdapter.OnDisconnected -= HandleDisconnected;
				ActiveAdapter.OnError -= HandleError;

				ActiveAdapter = null;
				ActiveMode = ConnectionMode.Offline;

				GD.Print("[ConnectionManager] 已断开所有连接");
			}
		}

		public async Task SendAsync(byte[] data, DeliveryMode mode = DeliveryMode.Reliable)
		{
			if (ActiveAdapter != null && ActiveAdapter.IsConnected)
			{
				await ActiveAdapter.SendAsync(data, mode);
			}
			else
			{
				GD.PrintErr("[ConnectionManager] 无法发送: 未连接");
			}
		}

		private void HandleDataReceived(byte[] data)
		{
			GD.Print($"[ConnectionManager] 收到数据: {data.Length} 字节");

			EventBus.Instance.EmitSignal(
				EventBus.SignalName.NetworkDataReceived,
				data
			);
		}

		private void HandleConnected()
		{
			GD.Print("[ConnectionManager] 连接建立事件触发");

			EventBus.Instance.EmitSignal(EventBus.SignalName.NetworkConnected);
		}

		private void HandleDisconnected()
		{
			GD.Print("[ConnectionManager] 连接断开事件触发");

			EventBus.Instance.EmitSignal(EventBus.SignalName.NetworkDisconnected);
		}

		private void HandleError(string error)
		{
			GD.PrintErr($"[ConnectionManager] 网络错误: {error}");

			EventBus.Instance.EmitSignal(
				EventBus.SignalName.NetworkError,
				error
			);
		}

		public override void _ExitTree()
		{
			DisconnectAsync().Wait();
			_instance = null;
		}
	}
}
