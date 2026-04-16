using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using RoguelikeGame.Network.Auth;

namespace RoguelikeGame.Network.Friends
{
	public class FriendInfo
	{
		public string Id { get; set; } = "";
		public string Username { get; set; } = "";
		public int Level { get; set; }
		public bool IsOnline { get; set; }
		public DateTime LastSeen { get; set; } = DateTime.UtcNow;
	}

	public partial class FriendManager : Node
	{
		private static FriendManager _instance;

		public static FriendManager Instance => _instance;

		private System.Net.Http.HttpClient _httpClient;
		private List<FriendInfo> _friends = new();
		private List<FriendInfo> _pendingRequests = new();

		public IReadOnlyList<FriendInfo> Friends => _friends.AsReadOnly();
		public IReadOnlyList<FriendInfo> PendingRequests => _pendingRequests.AsReadOnly();

		[Signal]
		public delegate void FriendsListUpdatedEventHandler();
		[Signal]
		public delegate void FriendRequestReceivedEventHandler(string friendId, string friendName);
		[Signal]
		public delegate void FriendRequestAcceptedEventHandler(string friendId);
		[Signal]
		public delegate void FriendRemovedEventHandler(string friendId);

		public override void _Ready()
		{
			if (_instance != null && _instance != this)
			{
				QueueFree();
				return;
			}

			_instance = this;
			ProcessMode = ProcessModeEnum.Always;

			InitializeHttpClient();

			GD.Print("[FriendManager] 好友管理器已初始化");
		}

		private void InitializeHttpClient()
		{
			_httpClient = new System.Net.Http.HttpClient
			{
				BaseAddress = new Uri("http://127.0.0.1:5002"),
				Timeout = TimeSpan.FromSeconds(15)
			};
		}

		public async Task<List<FriendInfo>> GetFriendsListAsync()
		{
			try
			{
				if (AuthSystem.Instance?.IsAuthenticated != true) return new();

				SetAuthorizationHeader();

				var response = await _httpClient.GetAsync("/api/friends/list");
				var content = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(content);

				if (result.GetProperty("success").GetBoolean() && result.TryGetProperty("friends", out var friendsArray))
				{
					_friends.Clear();
					foreach (var f in friendsArray.EnumerateArray())
					{
						_friends.Add(new FriendInfo
						{
							Id = f.GetProperty("id").GetString() ?? "",
							Username = f.GetProperty("username").GetString() ?? "",
							Level = f.TryGetProperty("level", out var lv) ? lv.GetInt32() : 1,
							IsOnline = f.TryGetProperty("isOnline", out var online) && online.GetBoolean()
						});
					}

					EmitSignal(SignalName.FriendsListUpdated);
				}

				return _friends;
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[FriendManager] 获取好友列表失败: {ex.Message}");
				return _friends;
			}
		}

		public async Task<bool> SendFriendRequestAsync(string usernameOrUserId)
		{
			try
			{
				SetAuthorizationHeader();

				var requestData = new { targetUsername = usernameOrUserId };
				var json = JsonSerializer.Serialize(requestData);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync("/api/friends/request", content);
				var responseContent = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

				bool success = result.GetProperty("success").GetBoolean();

				if (success)
				{
					GD.Print($"[FriendManager] ✓ 好友请求已发送: {usernameOrUserId}");
				}

				return success;
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[FriendManager] 发送好友请求失败: {ex.Message}");
				return false;
			}
		}

		public async Task<bool> AcceptFriendRequestAsync(string requestId)
		{
			try
			{
				SetAuthorizationHeader();

				var response = await _httpClient.PostAsync($"/api/friends/{requestId}/accept", null);
				var responseContent = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

				bool success = result.GetProperty("success").GetBoolean();

				if (success)
				{
					GD.Print($"[FriendManager] ✓ 已接受好友请求");
					await GetFriendsListAsync(); // 刷新列表
				}

				return success;
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[FriendManager] 接受好友请求失败: {ex.Message}");
				return false;
			}
		}

		public async Task<bool> RemoveFriendAsync(string friendId)
		{
			try
			{
				SetAuthorizationHeader();

				var response = await _httpClient.DeleteAsync($"/api/friends/{friendId}");
				var responseContent = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

				bool success = result.GetProperty("success").GetBoolean();

				if (success)
				{
					_friends.RemoveAll(f => f.Id == friendId);
					EmitSignal(SignalName.FriendRemoved, friendId);
					GD.Print("[FriendManager] ✓ 已移除好友");
				}

				return success;
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[FriendManager] 移除好友失败: {ex.Message}");
				return false;
			}
		}

		public async Task<bool> InviteToRoomAsync(string friendId, string roomId)
		{
			try
			{
				SetAuthorizationHeader();

				var requestData = new { friendId, roomId };
				var json = JsonSerializer.Serialize(requestData);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync("/api/friends/invite", content);

				return response.IsSuccessStatusCode;
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[FriendManager] 邀请好友失败: {ex.Message}");
				return false;
			}
		}

		private void SetAuthorizationHeader()
		{
			if (!string.IsNullOrEmpty(AuthSystem.Instance?.Token))
			{
				_httpClient.DefaultRequestHeaders.Authorization =
					new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthSystem.Instance.Token);
			}
		}

		public override void _ExitTree()
		{
			_httpClient?.Dispose();
			_instance = null;
		}
	}
}
