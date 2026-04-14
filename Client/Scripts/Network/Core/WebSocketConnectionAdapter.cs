using System;
using System.Threading.Tasks;
using Godot;

namespace RoguelikeGame.Network.Core
{
	public partial class WebSocketConnectionAdapter : Node, IConnectionAdapter
	{
		private WebSocketPeer _webSocketPeer;
		private bool _isConnected = false;
		private string _targetUrl = "";

		public new bool IsConnected => _isConnected && _webSocketPeer?.GetReadyState() == WebSocketPeer.State.Open;
		public string ConnectionInfo => IsConnected ? _targetUrl : "未连接";

		public event Action<byte[]> OnDataReceived;
		public event Action OnConnected;
		public event Action OnDisconnected;
		public event Action<string> OnError;

		public override void _Ready()
		{
			_webSocketPeer = new WebSocketPeer();
			GD.Print("[WebSocketAdapter] WebSocket连接适配器已初始化");
		}

		public async Task<bool> ConnectAsync(string address, int port)
		{
			try
			{
				_targetUrl = $"ws://{address}:{port}/hubs/game?id={Guid.NewGuid()}";

				GD.Print($"[WebSocketAdapter] 正在通过WebSocket连接: {_targetUrl}");

				var err = _webSocketPeer.ConnectToUrl(_targetUrl);

				if (err != Error.Ok)
				{
					GD.PrintErr($"[WebSocketAdapter] WebSocket连接失败: {err}");
					OnError?.Invoke($"连接错误: {err}");
					return false;
				}

				_isConnected = true;

				GD.Print($"[WebSocketAdapter] WebSocket连接成功");

				OnConnected?.Invoke();

				return true;
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[WebSocketAdapter] 连接异常: {ex.Message}");
				OnError?.Invoke(ex.Message);
				return false;
			}
		}

		public async Task DisconnectAsync()
		{
			try
			{
				if (_webSocketPeer != null)
				{
					_webSocketPeer.Close();
					_isConnected = false;

					GD.Print("[WebSocketAdapter] WebSocket已断开");
					OnDisconnected?.Invoke();
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[WebSocketAdapter] 断开连接异常: {ex.Message}");
			}
		}

		public async Task SendAsync(byte[] data, DeliveryMode mode = DeliveryMode.Reliable)
		{
			if (!IsConnected || _webSocketPeer == null)
			{
				GD.PrintErr("[WebSocketAdapter] 无法发送: 未连接");
				return;
			}

			try
			{
				var err = _webSocketPeer.Send(data, WebSocketPeer.WriteMode.Text);
				if (err != Error.Ok)
				{
					GD.PrintErr($"[WebSocketAdapter] 发送数据失败: {err}");
					OnError?.Invoke($"发送错误: {err}");
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[WebSocketAdapter] 发送数据异常: {ex.Message}");
				OnError?.Invoke(ex.Message);
			}
		}

		public override void _Process(double delta)
		{
			if (_webSocketPeer == null) return;

			_webSocketPeer.Poll();

			while (_webSocketPeer.GetAvailablePacketCount() > 0)
			{
				try
				{
					var data = _webSocketPeer.GetPacket();
					if (data.Length > 0)
					{
						OnDataReceived?.Invoke(data);
					}
				}
				catch (Exception ex)
				{
					GD.PrintErr($"[WebSocketAdapter] 接收数据异常: {ex.Message}");
				}
			}

			var state = _webSocketPeer.GetReadyState();
			switch (state)
			{
				case WebSocketPeer.State.Closed:
					if (_isConnected)
					{
						_isConnected = false;
						OnDisconnected?.Invoke();
					}
					break;

				case WebSocketPeer.State.Closing:
					GD.Print("[WebSocketAdapter] 连接正在关闭...");
					break;
			}
		}
	}
}
