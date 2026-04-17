using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using RoguelikeGame.Network.Auth;
using RoguelikeGame.Network.Realtime;

namespace RoguelikeGame.Network.Rooms
{
	public enum GameMode
	{
		PvP,
		PvE,
		Coop
	}

	public enum RoomStatus
	{
		Waiting,
		Full,
		Ready,
		Playing,
		Finished
	}

	public class PlayerInfo
	{
		public string Id { get; set; } = "";
		public string UserId { get; set; } = "";
		public string Username { get; set; } = "";
		public bool IsHost { get; set; }
		public bool IsReady { get; set; }
		public bool IsBot { get; set; }
		public string? BotName { get; set; }
		public string? BotDifficulty { get; set; }
		public string? CharacterId { get; set; }
		public int Score { get; set; }
		public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
	}

	public class RoomInfo
	{
		public string Id { get; set; } = "";
		public string Name { get; set; } = "";
		public string HostId { get; set; } = "";
		public string HostName { get; set; } = "";
		public RoomStatus Status { get; set; }
		public GameMode Mode { get; set; }
		public int MaxPlayers { get; set; } = 4;
		public int CurrentPlayers { get; set; }
		public bool HasPassword { get; set; }
		public string Seed { get; set; } = "";
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public List<PlayerInfo> Players { get; set; } = new();

		public override string ToString()
		{
			return $"{Name} ({CurrentPlayers}/{MaxPlayers}) [{Mode}] - {Status}";
		}
	}

	public class RoomResult
	{
		public bool Success { get; set; }
		public RoomInfo? Room { get; set; }
		public string Message { get; set; } = "";
		public List<RoomInfo>? Rooms { get; set; }
		public int TotalCount { get; set; }
	}

	public partial class RoomManager : Node
	{
		private static RoomManager _instance;

		public static RoomManager Instance => _instance;

		private System.Net.Http.HttpClient _httpClient;
		private string _baseUrl = "";
		private RoomInfo? _currentRoom;
		private List<RoomInfo> _roomCache = new();
		private DateTime _lastCacheUpdate = DateTime.MinValue;
		private TimeSpan _cacheDuration = TimeSpan.FromSeconds(5);

		public RoomInfo? CurrentRoom => _currentRoom;
		public bool InRoom => _currentRoom != null;
		public bool IsHost => InRoom && AuthSystem.Instance?.CurrentUser?.Id == _currentRoom?.HostId;

		[Signal]
		public delegate void RoomCreatedEventHandler(string roomId, string roomName);
		[Signal]
		public delegate void RoomJoinedEventHandler(string roomId, string roomName);
		[Signal]
		public delegate void RoomLeftEventHandler(string roomId);
		[Signal]
		public delegate void RoomUpdatedEventHandler(string roomId);
		[Signal]
		public delegate void PlayerJoinedRoomEventHandler(string playerId, string playerName);
		[Signal]
		public delegate void PlayerLeftRoomEventHandler(string playerId);
		[Signal]
		public delegate void PlayerReadyChangedEventHandler(string playerId, bool isReady);
		[Signal]
		public delegate void GameStartingEventHandler();

		public override void _Ready()
		{
			if (_instance != null && _instance != this)
			{
				QueueFree();
				return;
			}

			_instance = this;
			ProcessMode = ProcessModeEnum.Always;

			_baseUrl = GetServerUrl();
			InitializeHttpClient();

			GD.Print("[RoomManager] 房间管理器已初始化");
		}

		private string GetServerUrl()
		{
			var configNode = GetNodeOrNull("/root/ServerConfig");
			if (configNode != null && configNode.HasMethod("get_server_url"))
			{
				return configNode.Call("get_server_url").AsString();
			}
			return "http://127.0.0.1:5002";
		}

		private void InitializeHttpClient()
		{
			_httpClient = new System.Net.Http.HttpClient
			{
				BaseAddress = new Uri(_baseUrl),
				Timeout = TimeSpan.FromSeconds(30)
			};
		}

		public void SetBaseUrl(string url)
		{
			_baseUrl = url;
			_httpClient.BaseAddress = new Uri(url);
		}

		private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, object? content = null)
		{
			var request = new HttpRequestMessage(method, url);

			if (AuthSystem.Instance?.IsAuthenticated ?? false)
			{
				request.Headers.Authorization =
					new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthSystem.Instance.Token);
			}

			if (content != null)
			{
				var json = JsonSerializer.Serialize(content);
				request.Content = new StringContent(json, Encoding.UTF8, "application/json");
			}

			return request;
		}

		public async Task<RoomResult> CreateRoomAsync(string name, GameMode mode = GameMode.PvP, int maxPlayers = 4, string? password = null)
		{
			try
			{
				var requestData = new
				{
					name,
					mode = mode.ToString(),
					maxPlayers,
					password = password ?? ""
				};

				var request = CreateAuthorizedRequest(HttpMethod.Post, "/api/rooms/create", requestData);
				var response = await _httpClient.SendAsync(request);
				var responseString = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(responseString);

				bool success = result.TryGetProperty("success", out var sEl) && sEl.GetBoolean();

				if (success)
				{
					string roomId = result.TryGetProperty("roomId", out var riEl) ? (riEl.GetString() ?? "") : "";
					string seed = result.TryGetProperty("seed", out var sEl2) ? (sEl2.GetString() ?? "") : "";

					_currentRoom = new RoomInfo
					{
						Id = roomId,
						Name = name,
						HostId = AuthSystem.Instance?.CurrentUser?.Id ?? "",
						HostName = AuthSystem.Instance?.CurrentUser?.Username ?? "",
						Status = RoomStatus.Waiting,
						Mode = mode,
						MaxPlayers = maxPlayers,
						HasPassword = !string.IsNullOrEmpty(password),
						Seed = seed,
						CurrentPlayers = 1,
						Players = new List<PlayerInfo>
						{
							new PlayerInfo
							{
								Id = AuthSystem.Instance?.CurrentUser?.Id ?? "",
								Username = AuthSystem.Instance?.CurrentUser?.Username ?? "",
								IsHost = true,
								IsReady = false,
								JoinedAt = DateTime.UtcNow
							}
						}
					};

					NetworkManager.Instance?.UpdateState(NetworkState.InRoom);

					GD.Print($"[RoomManager] ✓ 房间已创建: {_currentRoom}");

					EmitSignal(SignalName.RoomCreated, _currentRoom?.Id ?? "", _currentRoom?.Name ?? "");

					_ = ConnectHubAndJoinRoom(_currentRoom?.Id ?? "");

					return new RoomResult
					{
						Success = true,
						Room = _currentRoom,
						Message = "房间创建成功"
					};
				}
				else
				{
					string message = result.TryGetProperty("message", out var mEl) ? (mEl.GetString() ?? "创建失败") : "创建失败";
					GD.PrintErr($"[RoomManager] ✗ 创建房间失败: {message}");

					return new RoomResult { Success = false, Message = message };
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[RoomManager] 创建房间异常: {ex.Message}");
				return new RoomResult { Success = false, Message = $"网络错误: {ex.Message}" };
			}
		}

		public async Task<RoomResult> JoinRoomAsync(string roomId, string? password = null)
		{
			if (_currentRoom != null)
			{
				return new RoomResult { Success = false, Message = "已在房间中，请先离开" };
			}

			try
			{
				object requestData = string.IsNullOrEmpty(password)
					? new object()
					: new { password };

				var request = CreateAuthorizedRequest(HttpMethod.Post, $"/api/rooms/{roomId}/join", requestData);
				var response = await _httpClient.SendAsync(request);
				var responseString = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(responseString);

				bool success = result.TryGetProperty("success", out var sEl) && sEl.GetBoolean();

				if (success && result.TryGetProperty("room", out var roomElement))
				{
					_currentRoom = ParseRoomFromJson(roomElement);

					NetworkManager.Instance?.UpdateState(NetworkState.InRoom);

					GD.Print($"[RoomManager] ✓ 已加入房间: {_currentRoom}");

					EmitSignal(SignalName.RoomJoined, _currentRoom?.Id ?? "", _currentRoom?.Name ?? "");

					_ = ConnectHubAndJoinRoom(_currentRoom?.Id ?? "");

					return new RoomResult
					{
						Success = true,
						Room = _currentRoom,
						Message = "加入房间成功"
					};
				}
				else
				{
					string message = result.TryGetProperty("message", out var mEl) ? (mEl.GetString() ?? "加入失败") : "加入失败";
					GD.PrintErr($"[RoomManager] ✗ 加入房间失败: {message}");

					return new RoomResult { Success = false, Message = message };
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[RoomManager] 加入房间异常: {ex.Message}");
				return new RoomResult { Success = false, Message = $"网络错误: {ex.Message}" };
			}
		}

		public async Task<RoomResult> LeaveRoomAsync()
		{
			if (_currentRoom == null)
			{
				return new RoomResult { Success = false, Message = "未在房间中" };
			}

			try
			{
				var leftRoomId = _currentRoom.Id;

				var request = CreateAuthorizedRequest(HttpMethod.Post, $"/api/rooms/{_currentRoom.Id}/leave");
				await _httpClient.SendAsync(request);

				GD.Print($"[RoomManager] 已离开房间: {leftRoomId}");

				EmitSignal(SignalName.RoomLeft, leftRoomId);

				_currentRoom = null;

				NetworkManager.Instance?.UpdateState(NetworkState.Authenticated);

				var hubClient = Realtime.GameHubClient.Instance;
				if (hubClient != null && hubClient.IsConnected)
				{
					await hubClient.LeaveRoomAsync(leftRoomId);
				}

				return new RoomResult { Success = true, Message = "已离开房间" };
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[RoomManager] 离开房间异常: {ex.Message}");
				_currentRoom = null;
				return new RoomResult { Success = true, Message = "已离开房间(本地)" };
			}
		}

		public async Task<RoomResult> GetRoomListAsync(int page = 1, int pageSize = 20)
		{
			if (DateTime.UtcNow - _lastCacheUpdate < _cacheDuration && _roomCache.Count > 0)
			{
				return new RoomResult
				{
					Success = true,
					Rooms = _roomCache,
					TotalCount = _roomCache.Count
				};
			}

			try
			{
				var request = CreateAuthorizedRequest(HttpMethod.Get, $"/api/rooms/list?page={page}&pageSize={pageSize}");
				var response = await _httpClient.SendAsync(request);
				var responseString = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(responseString);

				bool success = result.TryGetProperty("success", out var sEl) && sEl.GetBoolean();

				if (success && result.TryGetProperty("rooms", out var roomsArray))
				{
					_roomCache.Clear();
					foreach (var roomElement in roomsArray.EnumerateArray())
					{
						_roomCache.Add(ParseRoomFromJson(roomElement));
					}

					_lastCacheUpdate = DateTime.UtcNow;

					int total = result.TryGetProperty("total", out var totalElement) ? totalElement.GetInt32() : _roomCache.Count;

					GD.Print($"[RoomManager] 获取到 {_roomCache.Count} 个房间");

					return new RoomResult
					{
						Success = true,
						Rooms = _roomCache,
						TotalCount = total
					};
				}

				return new RoomResult { Success = false, Message = "获取失败" };
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[RoomManager] 获取房间列表异常: {ex.Message}");
				return new RoomResult { Success = false, Message = $"网络错误: {ex.Message}" };
			}
		}

		public async Task<RoomResult> GetRoomDetailsAsync(string roomId)
		{
			try
			{
				var request = CreateAuthorizedRequest(HttpMethod.Get, $"/api/rooms/{roomId}");
				var response = await _httpClient.SendAsync(request);
				var responseString = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(responseString);

				bool success = result.TryGetProperty("success", out var sEl) && sEl.GetBoolean();

				if (success && result.TryGetProperty("room", out var roomElement))
				{
					var room = ParseRoomFromJson(roomElement);

					if (_currentRoom?.Id == roomId)
					{
						_currentRoom = room;
						EmitSignal(SignalName.RoomUpdated, room?.Id ?? "");
					}

					return new RoomResult { Success = true, Room = room };
				}

				return new RoomResult { Success = false, Message = "获取详情失败" };
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[RoomManager] 获取房间详情异常: {ex.Message}");
				return new RoomResult { Success = false, Message = $"网络错误: {ex.Message}" };
			}
		}

		public async Task<bool> SetReadyAsync(bool isReady)
		{
			if (_currentRoom == null) return false;

			try
			{
				var requestData = new { isReady };
				var request = CreateAuthorizedRequest(HttpMethod.Post, $"/api/rooms/{_currentRoom.Id}/ready", requestData);
				var response = await _httpClient.SendAsync(request);
				var responseString = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(responseString);

				bool success = result.TryGetProperty("success", out var sEl) && sEl.GetBoolean();

				if (success)
				{
					var currentUserId = AuthSystem.Instance?.CurrentUser?.Id;
					var currentPlayer = _currentRoom.Players.Find(p => p.Id == currentUserId);
					if (currentPlayer != null)
					{
						currentPlayer.IsReady = isReady;
					}

					GD.Print($"[RoomManager] 准备状态更新为: {isReady}");

					EmitSignal(SignalName.PlayerReadyChanged, currentUserId ?? "", isReady);
				}

				return success;
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[RoomManager] 设置准备状态异常: {ex.Message}");
				return false;
			}
		}

		public async Task<bool> StartGameAsync()
		{
			if (_currentRoom == null || !IsHost) return false;

			try
			{
				var request = CreateAuthorizedRequest(HttpMethod.Post, $"/api/rooms/{_currentRoom.Id}/start");
				var response = await _httpClient.SendAsync(request);
				var responseString = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(responseString);

				bool success = result.TryGetProperty("success", out var sEl) && sEl.GetBoolean();

				if (success)
				{
					_currentRoom.Status = RoomStatus.Playing;

					NetworkManager.Instance?.UpdateState(NetworkState.InGame);

					GD.Print("[RoomManager] 🎮 游戏开始！");

					EmitSignal(SignalName.GameStarting);
				}

				return success;
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[RoomManager] 开始游戏异常: {ex.Message}");
				return false;
			}
		}

		public async Task<RoomResult> AddBotAsync(string difficulty = "Normal")
		{
			if (_currentRoom == null || !IsHost)
			{
				return new RoomResult { Success = false, Message = "仅房主可添加机器人" };
			}

			try
			{
				var requestData = new { difficulty };
				var request = CreateAuthorizedRequest(HttpMethod.Post, $"/api/rooms/{_currentRoom.Id}/add-bot", requestData);
				var response = await _httpClient.SendAsync(request);
				var responseString = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(responseString);

				bool success = result.TryGetProperty("success", out var sEl) && sEl.GetBoolean();

				if (success)
				{
					string botName = result.TryGetProperty("bot", out var botEl) &&
						botEl.TryGetProperty("botName", out var bnEl) ? (bnEl.GetString() ?? "Bot") : "Bot";

					GD.Print($"[RoomManager] 🤖 机器人 {botName} 已加入");

					await RefreshCurrentRoomAsync();

					return new RoomResult { Success = true, Message = $"机器人 {botName} 已加入" };
				}
				else
				{
					string message = result.TryGetProperty("message", out var mEl) ? (mEl.GetString() ?? "添加失败") : "添加失败";
					return new RoomResult { Success = false, Message = message };
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[RoomManager] 添加机器人异常: {ex.Message}");
				return new RoomResult { Success = false, Message = $"网络错误: {ex.Message}" };
			}
		}

		public async Task<RoomResult> RemoveBotAsync(string botId)
		{
			if (_currentRoom == null || !IsHost)
			{
				return new RoomResult { Success = false, Message = "仅房主可移除机器人" };
			}

			try
			{
				var requestData = new { botId };
				var request = CreateAuthorizedRequest(HttpMethod.Post, $"/api/rooms/{_currentRoom.Id}/remove-bot", requestData);
				var response = await _httpClient.SendAsync(request);
				var responseString = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(responseString);

				bool success = result.TryGetProperty("success", out var sEl) && sEl.GetBoolean();

				if (success)
				{
					GD.Print("[RoomManager] 🤖 机器人已移除");

					await RefreshCurrentRoomAsync();

					string message = result.TryGetProperty("message", out var mEl) ? (mEl.GetString() ?? "已移除") : "已移除";
					return new RoomResult { Success = true, Message = message };
				}
				else
				{
					string message = result.TryGetProperty("message", out var mEl) ? (mEl.GetString() ?? "移除失败") : "移除失败";
					return new RoomResult { Success = false, Message = message };
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[RoomManager] 移除机器人异常: {ex.Message}");
				return new RoomResult { Success = false, Message = $"网络错误: {ex.Message}" };
			}
		}

		private async Task RefreshCurrentRoomAsync()
		{
			if (_currentRoom == null) return;

			var result = await GetRoomDetailsAsync(_currentRoom.Id);
			if (result.Success && result.Room != null)
			{
				_currentRoom = result.Room;
				EmitSignal(SignalName.RoomUpdated, _currentRoom.Id);
			}
		}

		private RoomInfo ParseRoomFromJson(JsonElement roomElement)
		{
			var room = new RoomInfo
			{
				Id = roomElement.TryGetProperty("id", out var idEl) ? (idEl.GetString() ?? "") : "",
				Name = roomElement.TryGetProperty("name", out var nameEl) ? (nameEl.GetString() ?? "") : "",
				HostId = roomElement.TryGetProperty("hostId", out var hidEl) ? (hidEl.GetString() ?? "") : "",
				HostName = roomElement.TryGetProperty("hostName", out var hnEl) ? (hnEl.GetString() ?? "") : "",
				Status = Enum.TryParse<RoomStatus>(
					roomElement.TryGetProperty("status", out var stEl) ? stEl.GetString() ?? "Waiting" : "Waiting", out var status) ? status : RoomStatus.Waiting,
				Mode = Enum.TryParse<GameMode>(
					roomElement.TryGetProperty("mode", out var modeEl) ? modeEl.GetString() ?? "PvP" : "PvP", out var mode) ? mode : GameMode.PvP,
				MaxPlayers = roomElement.TryGetProperty("maxPlayers", out var mpEl) ? mpEl.GetInt32() : 4,
				CurrentPlayers = roomElement.TryGetProperty("currentPlayers", out var cpEl) ? cpEl.GetInt32() : 0,
				HasPassword = roomElement.TryGetProperty("hasPassword", out var hpEl) && hpEl.GetBoolean(),
				Seed = roomElement.TryGetProperty("seed", out var seedEl) ? seedEl.GetString() ?? "" : ""
			};

			if (roomElement.TryGetProperty("players", out var playersArray))
			{
				foreach (var playerEl in playersArray.EnumerateArray())
				{
					var playerPrimaryKey = playerEl.TryGetProperty("id", out var pkEl) ? (pkEl.GetString() ?? "") : "";
					var userId = playerEl.TryGetProperty("userId", out var uidEl) ? (uidEl.GetString() ?? "") : "";
					var username = playerEl.TryGetProperty("username", out var unEl) ? (unEl.GetString() ?? "") : "";
					bool isBot = playerEl.TryGetProperty("isBot", out var ibEl) && ibEl.GetBoolean();
					string botName = playerEl.TryGetProperty("botName", out var bnEl) ? (bnEl.GetString() ?? "") : "";
					string botDiff = playerEl.TryGetProperty("botDifficulty", out var bdEl) ? (bdEl.GetString() ?? "") : "";

					if (isBot && !string.IsNullOrEmpty(botName))
					{
						username = $"🤖 {botName}";
					}

					room.Players.Add(new PlayerInfo
					{
						Id = playerPrimaryKey,
						UserId = userId,
						Username = username,
						IsHost = room.HostId == userId,
						IsReady = playerEl.TryGetProperty("isReady", out var readyEl) && readyEl.GetBoolean(),
						IsBot = isBot,
						BotName = botName,
						BotDifficulty = botDiff,
						Score = playerEl.TryGetProperty("score", out var scoreEl) ? scoreEl.GetInt32() : 0,
						JoinedAt = DateTime.UtcNow
					});
				}
			}

			return room;
		}

		private async Task ConnectHubAndJoinRoom(string roomId)
		{
			if (string.IsNullOrEmpty(roomId)) return;

			var hubClient = GameHubClient.Instance;
			if (hubClient == null) return;

			var authToken = AuthSystem.Instance?.Token ?? "";
			if (!hubClient.IsConnected)
			{
				await hubClient.ConnectAsync(authToken);
			}

			if (hubClient.IsConnected)
			{
				await hubClient.JoinRoomAsync(roomId);
			}
		}

		public void ClearCache()
		{
			_roomCache.Clear();
			_lastCacheUpdate = DateTime.MinValue;
		}

		public override void _ExitTree()
		{
			_httpClient?.Dispose();
			_instance = null;
		}
	}
}
