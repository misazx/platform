using System;
using System.Threading.Tasks;
using Godot;

namespace RoguelikeGame.Network.Core
{
	public enum DeliveryMode
	{
		Unreliable,
		Reliable,
		Unordered
	}

	public interface IConnectionAdapter : IDisposable
	{
		bool IsConnected { get; }
		string ConnectionInfo { get; }

		Task<bool> ConnectAsync(string address, int port);
		Task DisconnectAsync();
		Task SendAsync(byte[] data, DeliveryMode mode = DeliveryMode.Reliable);

		event Action<byte[]> OnDataReceived;
		event Action OnConnected;
		event Action OnDisconnected;
		event Action<string> OnError;
	}
}
