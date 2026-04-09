using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace RoguelikeGame.Network.Core
{
	public class ENetConnectionAdapter : IConnectionAdapter
	{
		private TcpClient _tcpClient;
		private NetworkStream _stream;
		private CancellationTokenSource _cts;
		private bool _isConnected = false;
		private string _address = "";
		private int _port = 0;

		public bool IsConnected => _isConnected && _tcpClient?.Connected == true;
		public string ConnectionInfo => IsConnected ? $"{_address}:{_port}" : "未连接";

		public event Action<byte[]> OnDataReceived;
		public event Action OnConnected;
		public event Action OnDisconnected;
		public event Action<string> OnError;

		public async Task<bool> ConnectAsync(string address, int port)
		{
			try
			{
				_address = address;
				_port = port;

				GD.Print($"[ENetAdapter] 正在连接到 {address}:{port}...");

				_tcpClient = new TcpClient();
				_cts = new CancellationTokenSource();

				await _tcpClient.ConnectAsync(address, port, _cts.Token);

				if (_tcpClient.Connected)
				{
					_stream = _tcpClient.GetStream();
					_isConnected = true;

					GD.Print($"[ENetAdapter] ✓ 成功连接到 {address}:{port}");

					OnConnected?.Invoke();

					_ = Task.Run(() => ReceiveLoop(_cts.Token));

					return true;
				}

				return false;
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[ENetAdapter] ✗ 连接失败: {ex.Message}");
				OnError?.Invoke(ex.Message);
				return false;
			}
		}

		public async Task DisconnectAsync()
		{
			try
			{
				_cts?.Cancel();
				_stream?.Close();
				_tcpClient?.Close();

				_isConnected = false;

				GD.Print("[ENetAdapter] 已断开连接");

				OnDisconnected?.Invoke();
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[ENetAdapter] 断开连接时出错: {ex.Message}");
			}
		}

		public async Task SendAsync(byte[] data, DeliveryMode mode = DeliveryMode.Reliable)
		{
			if (!IsConnected || _stream == null)
			{
				GD.PrintErr("[ENetAdapter] 无法发送: 未连接");
				return;
			}

			try
			{
				var lengthBytes = BitConverter.GetBytes(data.Length);
				await _stream.WriteAsync(lengthBytes, 0, lengthBytes.Length);
				await _stream.WriteAsync(data, 0, data.Length);
				await _stream.FlushAsync();
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[ENetAdapter] 发送数据失败: {ex.Message}");
				OnError?.Invoke(ex.Message);
			}
		}

		private async Task ReceiveLoop(CancellationToken token)
		{
			var buffer = new byte[4096];

			try
			{
				while (!token.IsCancellationRequested && IsConnected)
				{
					var lengthBytes = new byte[4];
					int bytesRead = await _stream.ReadAsync(lengthBytes, 0, 4, token);

					if (bytesRead < 4) break;

					int messageLength = BitConverter.ToInt32(lengthBytes, 0);

					if (messageLength <= 0 || messageLength > 1024 * 1024)
					{
						GD.PrintErr($"[ENetAdapter] 无效的消息长度: {messageLength}");
						continue;
					}

					var messageBuffer = new byte[messageLength];
					int totalRead = 0;

					while (totalRead < messageLength)
					{
						bytesRead = await _stream.ReadAsync(
							messageBuffer,
							totalRead,
							messageLength - totalRead,
							token
						);

						if (bytesRead == 0) break;

						totalRead += bytesRead;
					}

					if (totalRead == messageLength)
					{
						OnDataReceived?.Invoke(messageBuffer);
					}
				}
			}
			catch (OperationCanceledException)
			{
				GD.Print("[ENetAdapter] 接收循环已取消");
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[ENetAdapter] 接收数据错误: {ex.Message}");
			}
			finally
			{
				if (_isConnected)
				{
					_isConnected = false;
					OnDisconnected?.Invoke();
				}
			}
		}

		public void Dispose()
		{
			DisconnectAsync().Wait();
			_cts?.Dispose();
		}
	}
}
