using System;
using System.Text.Json;
using Godot;

namespace RoguelikeGame.Network.Core
{
	public enum PacketType
	{
		Ping = 0,
		Pong = 1,
		AuthRequest = 10,
		AuthResponse = 11,
		RoomCreate = 20,
		RoomJoin = 21,
		RoomLeave = 22,
		RoomList = 23,
		RoomInfo = 24,
		GameStart = 30,
		GameStateSync = 31,
		GameInput = 32,
		GameEnd = 33,
		Error = 99
	}

	public class NetworkPacket
	{
		public PacketType Type { get; set; }
		public string SenderId { get; set; } = "";
		public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		public object Payload { get; set; }

		public byte[] Serialize()
		{
			var options = new JsonSerializerOptions
			{
				WriteIndented = false,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};

			return JsonSerializer.SerializeToUtf8Bytes(this, options);
		}

		public static NetworkPacket Deserialize(byte[] data)
		{
			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};

			return JsonSerializer.Deserialize<NetworkPacket>(data, options);
		}

		public static NetworkPacket Create(PacketType type, object payload = null, string senderId = "")
		{
			return new NetworkPacket
			{
				Type = type,
				SenderId = senderId,
				Payload = payload
			};
		}
	}

	public static class PacketSerializer
	{
		public static byte[] SerializePacket(PacketType type, object payload = null)
		{
			var packet = NetworkPacket.Create(type, payload);
			return packet.Serialize();
		}

		public static NetworkPacket DeserializePacket(byte[] data)
		{
			return NetworkPacket.Deserialize(data);
		}

		public static void DeserializeAndHandle(byte[] data)
		{
			try
			{
				var packet = DeserializePacket(data);

				GD.Print($"[PacketSerializer] 处理数据包: {packet.Type}");

				switch (packet.Type)
				{
					case PacketType.Ping:
						HandlePing(packet);
						break;

					case PacketType.AuthResponse:
						HandleAuthResponse(packet);
						break;

					case PacketType.RoomInfo:
						HandleRoomInfo(packet);
						break;

					case PacketType.GameStateSync:
						HandleGameStateSync(packet);
						break;

					case PacketType.Error:
						HandleError(packet);
						break;

					default:
						GD.Print($"[PacketSerializer] 未处理的数据包类型: {packet.Type}");
						break;
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[PacketSerializer] 数据包处理错误: {ex.Message}");
			}
		}

		private static void HandlePing(NetworkPacket packet)
		{
			var pongPacket = NetworkPacket.Create(PacketType.Pong);
			ConnectionManager.Instance?.SendAsync(pongPacket.Serialize());
		}

		private static void HandleAuthResponse(NetworkPacket packet)
		{
			if (packet.Payload is JsonElement payload)
			{
				bool success = payload.GetProperty("success").GetBoolean();
				string message = payload.GetProperty("message").GetString() ?? "";

				NetworkManager.Instance?.EmitSignal(
					NetworkManager.SignalName.AuthenticationCompleted,
					success,
					message
				);
			}
		}

		private static void HandleRoomInfo(NetworkPacket packet)
		{
			if (packet.Payload is JsonElement payload)
			{
				string roomId = payload.GetProperty("roomId").GetString() ?? "";

				NetworkManager.Instance?.EmitSignal(
					NetworkManager.SignalName.RoomJoined,
					roomId
				);
			}
		}

		private static void HandleGameStateSync(NetworkPacket packet)
		{
			EventBus.Instance.EmitSignal(
				EventBus.SignalName.GameStateUpdated,
				packet.Payload
			);
		}

		private static void HandleError(NetworkPacket packet)
		{
			if (packet.Payload is JsonElement payload)
			{
				string errorMessage = payload.GetProperty("message").GetString() ?? "未知错误";

				GD.PrintErr($"[PacketSerializer] 服务器错误: {errorMessage}");

				NetworkManager.Instance?.EmitSignal(
					NetworkManager.SignalName.ConnectionFailed,
					errorMessage
				);
			}
		}
	}
}
