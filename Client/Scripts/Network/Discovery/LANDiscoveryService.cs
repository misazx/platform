using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace RoguelikeGame.Network.Discovery
{
	public class LANHostInfo
	{
		public string HostName { get; set; } = "";
		public IPAddress Address { get; set; }
		public int Port { get; set; }
		public int CurrentPlayers { get; set; }
		public int MaxPlayers { get; set; }
		public string GameMode { get; set; } = "";
		public DateTime LastSeen { get; set; } = DateTime.UtcNow;
		public long PingMs { get; set; }

		public override string ToString()
		{
			return $"{HostName} @ {Address}:{Port} ({CurrentPlayers}/{MaxPlayers}) [{GameMode}]";
		}
	}

	public partial class LANDiscoveryService : Node
	{
		private static LANDiscoveryService _instance;

		public static LANDiscoveryService Instance => _instance;

		private UdpClient _udpClient;
		private CancellationTokenSource _cts;
		private Godot.Timer _broadcastTimer;
		private Godot.Timer _cleanupTimer;

		private Dictionary<string, LANHostInfo> _discoveredHosts = new();
		private bool _isHosting = false;
		private bool _isSearching = false;

		private const int BROADCAST_PORT = 8888;
		private const int BROADCAST_INTERVAL_MS = 2000;
		private const int HOST_TIMEOUT_SECONDS = 30;

		public IReadOnlyDictionary<string, LANHostInfo> DiscoveredHosts => _discoveredHosts;
		public bool IsHosting => _isHosting;
		public bool IsSearching => _isSearching;

		[Signal]
		public delegate void HostDiscoveredEventHandler(string hostId, string hostName, string address, int port);

		[Signal]
		public delegate void HostLostEventHandler(string hostId);

		[Signal]
		public delegate void SearchStartedEventHandler();

		[Signal]
		public delegate void SearchStoppedEventHandler();

		public override void _Ready()
		{
			if (_instance != null && _instance != this)
			{
				QueueFree();
				return;
			}

			_instance = this;
			ProcessMode = ProcessModeEnum.Always;

			InitializeTimers();

			GD.Print("[LANDiscovery] 局域网发现服务已初始化");
		}

		private void InitializeTimers()
		{
			_broadcastTimer = new Godot.Timer
			{
				WaitTime = BROADCAST_INTERVAL_MS / 1000.0,
				OneShot = false,
				Autostart = false
			};
			_broadcastTimer.Connect("timeout", new Callable(this, nameof(OnBroadcastTick)));
			AddChild(_broadcastTimer);

			_cleanupTimer = new Godot.Timer
			{
				WaitTime = 5.0,
				OneShot = false,
				Autostart = false
			};
			_cleanupTimer.Connect("timeout", new Callable(this,(nameof(CleanupExpiredHosts))));
			AddChild(_cleanupTimer);
		}

		public async Task StartHostingAsync(int port = 5000, int maxPlayers = 4, string gameMode = "PvP")
		{
			if (_isHosting)
			{
				GD.Print("[LANDiscovery] 已在托管中");
				return;
			}

			try
			{
				_udpClient = new UdpClient(BROADCAST_PORT);
				_udpClient.EnableBroadcast = true;

				_isHosting = true;

				GD.Print($"[LANDiscovery] ✓ 开始在端口 {BROADCAST_PORT} 广播主机信息");

				_broadcastTimer.Start();
				_cleanupTimer.Start();

				_ = Task.Run(() => ListenForRequestsAsync(_cts.Token));
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[LANDiscovery] 启动托管失败: {ex.Message}");
			}
		}

		public void StopHosting()
		{
			_isHosting = false;
			_broadcastTimer.Stop();
			_cleanupTimer.Stop();

			SendShutdownNotification();

			_udpClient?.Close();
			_udpClient?.Dispose();
			_udpClient = null;

			GD.Print("[LANDiscovery] 停止托管");
		}

		public async Task StartSearchAsync()
		{
			if (_isSearching)
			{
				GD.Print("[LANDiscovery] 已在搜索中");
				return;
			}

			try
			{
				if (_udpClient == null)
				{
					_udpClient = new UdpClient();
					_udpClient.EnableBroadcast = true;
				}

				_isSearching = true;
				_cts = new CancellationTokenSource();

				EmitSignal(SignalName.SearchStarted);

				GD.Print("[LANDiscovery] 开始搜索局域网主机...");

				await SendDiscoveryRequest();

				_cleanupTimer.Start();

				_ = Task.Run(() => ListenForResponsesAsync(_cts.Token));
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[LANDiscovery] 启动搜索失败: {ex.Message}");
			}
		}

		public void StopSearch()
		{
			_isSearching = false;
			_cts?.Cancel();
			_cleanupTimer.Stop();

			_udpClient?.Close();
			_udpClient?.Dispose();
			_udpClient = null;

			_discoveredHosts.Clear();

			EmitSignal(SignalName.SearchStopped);

			GD.Print("[LANDiscovery] 停止搜索");
		}

		private async Task SendDiscoveryRequest()
		{
			if (_udpClient == null) return;

			var requestData = System.Text.Encoding.UTF8.GetBytes("DISCOVER_ROGUELIKE_GAME");
			var endpoint = new IPEndPoint(IPAddress.Broadcast, BROADCAST_PORT);

			try
			{
				await _udpClient.SendAsync(requestData, requestData.Length, endpoint);
				GD.Print("[LANDiscovery] 发送发现请求");
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[LANDiscovery] 发送发现请求失败: {ex.Message}");
			}
		}

		private async Task ListenForResponsesAsync(CancellationToken token)
		{
			while (!token.IsCancellationRequested && _isSearching)
			{
				try
				{
					var result = await _udpClient.ReceiveAsync();
					var response = System.Text.Encoding.UTF8.GetString(result.Buffer);

					if (response.StartsWith("HOST_INFO:"))
					{
						ProcessHostResponse(response, result.RemoteEndPoint);
					}
				}
				catch (ObjectDisposedException)
				{
					break;
				}
				catch (Exception ex)
				{
					if (!token.IsCancellationRequested)
					{
						GD.PrintErr($"[LANDiscovery] 接收响应错误: {ex.Message}");
					}
				}
			}
		}

		private void ProcessHostResponse(string response, IPEndPoint endpoint)
		{
			try
			{
				var dataPart = response.Substring("HOST_INFO:".Length);
				var parts = dataPart.Split('|');

				if (parts.Length >= 3)
				{
					var hostId = $"{endpoint.Address}:{endpoint.Port}";
					bool isNew = !_discoveredHosts.ContainsKey(hostId);

					var hostInfo = new LANHostInfo
					{
						HostName = parts[0],
						Address = endpoint.Address,
						Port = int.Parse(parts[1]),
						CurrentPlayers = int.Parse(parts[2]),
						MaxPlayers = parts.Length > 3 ? int.Parse(parts[3]) : 4,
						GameMode = parts.Length > 4 ? parts[4] : "PvP",
						LastSeen = DateTime.UtcNow
					};

					_discoveredHosts[hostId] = hostInfo;

					if (isNew)
					{
						GD.Print($"[LANDiscovery] 发现新主机: {hostInfo}");
						EmitSignal(SignalName.HostDiscovered, hostId, hostInfo.HostName, hostInfo.Address.ToString(), hostInfo.Port);
					}
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[LANDiscovery] 解析主机响应失败: {ex.Message}");
			}
		}

		private void OnBroadcastTick()
		{
			if (!_isHosting || _udpClient == null) return;

			try
			{
				var currentPlayers = 1;
				var username = "Player";
				var port = 5000;

				var broadcastMessage = $"HOST_INFO:{username}|{port}|{currentPlayers}|4|PvP";
				var data = System.Text.Encoding.UTF8.GetBytes(broadcastMessage);
				var endpoint = new IPEndPoint(IPAddress.Broadcast, BROADCAST_PORT);

				_udpClient.Send(data, data.Length, endpoint);
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[LANDiscovery] 广播失败: {ex.Message}");
			}
		}

		private async Task ListenForRequestsAsync(CancellationToken token)
		{
			while (!token.IsCancellationRequested && _isHosting)
			{
				try
				{
					var result = await _udpClient.ReceiveAsync();
					var request = System.Text.Encoding.UTF8.GetString(result.Buffer);

					if (request.Contains("DISCOVER_ROGUELIKE_GAME"))
					{
						await SendHostResponse(result.RemoteEndPoint);
					}
				}
				catch (ObjectDisposedException)
				{
					break;
				}
				catch (Exception ex)
				{
					if (!token.IsCancellationRequested)
					{
						GD.PrintErr($"[LANDiscovery] 监听请求错误: {ex.Message}");
					}
				}
			}
		}

		private async Task SendHostResponse(IPEndPoint targetEndpoint)
		{
			if (_udpClient == null) return;

			try
			{
				var currentPlayers = 1;
				var username = "Player";
				var port = 5000;

				var response = $"HOST_INFO:{username}|{port}|{currentPlayers}|4|PvP";
				var data = System.Text.Encoding.UTF8.GetBytes(response);

				await _udpClient.SendAsync(data, data.Length, targetEndpoint);
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[LANDiscovery] 发送响应失败: {ex.Message}");
			}
		}

		private void CleanupExpiredHosts()
		{
			var now = DateTime.UtcNow;
			var expiredKeys = _discoveredHosts
				.Where(h => (now - h.Value.LastSeen).TotalSeconds > HOST_TIMEOUT_SECONDS)
				.Select(h => h.Key)
				.ToList();

			foreach (var key in expiredKeys)
			{
				var lostHost = _discoveredHosts[key];
				_discoveredHosts.Remove(key);

				GD.Print($"[LANDiscovery] 主机超时移除: {lostHost}");
				EmitSignal(SignalName.HostLost, key);
			}
		}

		private void SendShutdownNotification()
		{
			if (_udpClient == null) return;

			try
			{
				var shutdownMsg = "HOST_SHUTDOWN";
				var data = System.Text.Encoding.UTF8.GetBytes(shutdownMsg);
				var endpoint = new IPEndPoint(IPAddress.Broadcast, BROADCAST_PORT);

				_udpClient.Send(data, data.Length, endpoint);
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[LANDiscovery] 发送关闭通知失败: {ex.Message}");
			}
		}

		public List<LANHostInfo> GetAvailableHosts()
		{
			return _discoveredHosts.Values.ToList();
		}

		public override void _ExitTree()
		{
			StopHosting();
			StopSearch();
			_instance = null;
		}
	}
}
