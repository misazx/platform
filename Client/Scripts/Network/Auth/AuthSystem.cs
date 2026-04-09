using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;

namespace RoguelikeGame.Network.Auth
{
	public class AuthResult
	{
		public bool Success { get; set; }
		public string Token { get; set; } = "";
		public string UserId { get; set; } = "";
		public string Message { get; set; } = "";
		public DateTime ExpiresAt { get; set; }
	}

	public class UserInfo
	{
		public string Id { get; set; } = "";
		public string Username { get; set; } = "";
		public string? Email { get; set; }
		public int Level { get; set; } = 1;
		public int Experience { get; set; } = 0;
		public int TotalGamesPlayed { get; set; } = 0;
		public int GamesWon { get; set; } = 0;
	}

	public partial class AuthSystem : Node
	{
		private static AuthSystem _instance;

		public static AuthSystem Instance => _instance;

		private System.Net.Http.HttpClient _httpClient;
		private string _baseUrl = "http://127.0.0.1:5000";
		private string _currentToken = "";
		private UserInfo? _currentUser;
		private DateTime _tokenExpiry = DateTime.MinValue;

		public bool IsAuthenticated => !string.IsNullOrEmpty(_currentToken) && DateTime.UtcNow < _tokenExpiry;
		public UserInfo? CurrentUser => _currentUser;
		public string Token => _currentToken;

		[Signal]
		public delegate void LoginCompletedEventHandler(bool success, string message);

		[Signal]
		public delegate void RegisterCompletedEventHandler(bool success, string message);

		[Signal]
		public delegate void LogoutEventHandler();

		[Signal]
		public delegate void SessionExpiredEventHandler();

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
			LoadSavedSession();

			GD.Print("[AuthSystem] 认证系统已初始化");
		}

		private void InitializeHttpClient()
		{
			_httpClient = new HttpClient
			{
				BaseAddress = new Uri(_baseUrl),
				Timeout = TimeSpan.FromSeconds(30)
			};
		}

		public void SetBaseUrl(string url)
		{
			_baseUrl = url;
			_httpClient.BaseAddress = new Uri(url);

			GD.Print($"[AuthSystem] 服务器地址更新为: {_baseUrl}");
		}

		public async Task<AuthResult> RegisterAsync(string username, string password, string? email = null)
		{
			try
			{
				var requestData = new
				{
					username,
					password,
					email = email ?? ""
				};

				var json = JsonSerializer.Serialize(requestData);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				GD.Print($"[AuthSystem] 正在注册用户: {username}");

				var response = await _httpClient.PostAsync("/api/auth/register", content);
				var responseString = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(responseString);

				bool success = result.GetProperty("success").GetBoolean();
				string message = result.GetProperty("message").GetString() ?? "";

				if (success)
				{
					string token = result.GetProperty("token").GetString() ?? "";

					GD.Print($"[AuthSystem] ✓ 注册成功: {username}");

					EmitSignal(SignalName.RegisterCompleted, true, message);

					return new AuthResult
					{
						Success = true,
						Token = token,
						Message = message,
						ExpiresAt = DateTime.UtcNow.AddDays(7)
					};
				}
				else
				{
					GD.Print($"[AuthSystem] ✗ 注册失败: {message}");
					EmitSignal(SignalName.RegisterCompleted, false, message);

					return new AuthResult
					{
						Success = false,
						Message = message
					};
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[AuthSystem] 注册异常: {ex.Message}");
				EmitSignal(SignalName.RegisterCompleted, false, $"网络错误: {ex.Message}");

				return new AuthResult
				{
					Success = false,
					Message = $"网络错误: {ex.Message}"
				};
			}
		}

		public async Task<AuthResult> LoginAsync(string username, string password)
		{
			try
			{
				var requestData = new { username, password };
				var json = JsonSerializer.Serialize(requestData);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				GD.Print($"[AuthSystem] 正在登录: {username}");

				var response = await _httpClient.PostAsync("/api/auth/login", content);
				var responseString = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(responseString);

				bool success = result.GetProperty("success").GetBoolean();
				string message = result.GetProperty("message").GetString() ?? "";

				if (success)
				{
					string token = result.GetProperty("token").GetString() ?? "";
					string userId = result.GetProperty("userId").GetString() ?? "";

					await SetSession(token, userId);

					GD.Print($"[AuthSystem] ✓ 登录成功: {username} (ID: {userId})");

					EmitSignal(SignalName.LoginCompleted, true, message);

					return new AuthResult
					{
						Success = true,
						Token = token,
						UserId = userId,
						Message = message,
						ExpiresAt = _tokenExpiry
					};
				}
				else
				{
					GD.Print($"[AuthSystem] ✗ 登录失败: {message}");
					EmitSignal(SignalName.LoginCompleted, false, message);

					return new AuthResult
					{
						Success = false,
						Message = message
					};
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[AuthSystem] 登录异常: {ex.Message}");
				EmitSignal(SignalName.LoginCompleted, false, $"网络错误: {ex.Message}");

				return new AuthResult
				{
					Success = false,
					Message = $"网络错误: {ex.Message}"
				};
			}
		}

		public async Task<UserInfo?> GetCurrentUserAsync()
		{
			if (!IsAuthenticated) return null;

			try
			{
				_httpClient.DefaultRequestHeaders.Authorization =
					new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _currentToken);

				var response = await _httpClient.GetAsync("/api/auth/me");
				var responseString = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(responseString);

				if (result.TryGetProperty("user", out var userElement))
				{
					_currentUser = new UserInfo
					{
						Id = userElement.GetProperty("id").GetString() ?? "",
						Username = userElement.GetProperty("username").GetString() ?? "",
						Email = userElement.GetProperty("email").GetString(),
						Level = userElement.GetProperty("level").GetInt32(),
						Experience = userElement.GetProperty("experience").GetInt32(),
						TotalGamesPlayed = userElement.GetProperty("totalGamesPlayed").GetInt32(),
						GamesWon = userElement.GetProperty("gamesWon").GetInt32()
					};

					return _currentUser;
				}

				return null;
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[AuthSystem] 获取用户信息失败: {ex.Message}");
				return null;
			}
		}

		public async Task<bool> ValidateTokenAsync()
		{
			if (!IsAuthenticated) return false;

			var userInfo = await GetCurrentUserAsync();
			return userInfo != null;
		}

		public void Logout()
		{
			GD.Print("[AuthSystem] 用户登出");

			ClearSession();

			EmitSignal(SignalName.Logout);
		}

		private async Task SetSession(string token, string userId)
		{
			_currentToken = token;
			_tokenExpiry = DateTime.UtcNow.AddDays(7);

			SaveSessionToDisk(token, userId, _tokenExpiry);

			NetworkManager.Instance?.UpdateState(NetworkState.Authenticated);

			await GetCurrentUserAsync();
		}

		private void ClearSession()
		{
			_currentToken = "";
			_tokenExpiry = DateTime.MinValue;
			_currentUser = null;

			if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
			{
				_httpClient.DefaultRequestHeaders.Remove("Authorization");
			}

			DeleteSessionFromDisk();

			if (NetworkManager.Instance?.State == NetworkState.Authenticated ||
			    NetworkManager.Instance?.State == NetworkState.InLobby)
			{
				NetworkManager.Instance.UpdateState(NetworkState.Connected);
			}
		}

		private void SaveSessionToDisk(string token, string userId, DateTime expiry)
		{
			try
			{
				var sessionData = new { token, userId, expiryTicks = expiry.Ticks };
				var json = JsonSerializer.Serialize(sessionData);
				var configDir = OS.GetUserDataDir();
				var savePath = $"{config_dir}/auth_session.json";

				using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);
				file.StoreString(json);

				GD.Print("[AuthSystem] 会话已保存到本地");
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[AuthSystem] 保存会话失败: {ex.Message}");
			}
		}

		private void LoadSavedSession()
		{
			try
			{
				var configDir = OS.GetUserDataDir();
				var savePath = $"{configDir}/auth_session.json";

				if (!FileAccess.FileExists(savePath)) return;

				using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
				var json = file.GetAsText();
				var sessionData = JsonSerializer.Deserialize<JsonElement>(json);

				string savedToken = sessionData.GetProperty("token").GetString() ?? "";
				long expiryTicks = sessionData.GetProperty("expiryTicks").GetInt64();
				var savedExpiry = new DateTime(expiryTicks);

				if (DateTime.UtcNow < savedExpiry)
				{
					_currentToken = savedToken;
					_tokenExpiry = savedExpiry;

					GD.Print("[AuthSystem] 已加载本地会话");

					_ = ValidateTokenAsync();
				}
				else
				{
					GD.Print("[AuthSystem] 本地会话已过期");
					DeleteSessionFromDisk();
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[AuthSystem] 加载会话失败: {ex.Message}");
			}
		}

		private void DeleteSessionFromDisk()
		{
			try
			{
				var configDir = OS.GetUserDataDir();
				var savePath = $"{configDir}/auth_session.json";

				if (FileAccess.FileExists(savePath))
				{
					DirAccess.RemoveAbsolute(savePath);
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[AuthSystem] 删除会话文件失败: {ex.Message}");
			}
		}

		public override void _Process(double delta)
		{
			if (IsAuthenticated && DateTime.UtcNow >= _tokenExpiry.AddMinutes(-5))
			{
				GD.Print("[AuthSystem] 即将过期，尝试续期...");
			}

			if (IsAuthenticated && DateTime.UtcNow >= _tokenExpiry)
			{
				GD.Print("[AuthSystem] 会话已过期");
				EmitSignal(SignalName.SessionExpired);
				Logout();
			}
		}

		public override void _ExitTree()
		{
			_httpClient?.Dispose();
			_instance = null;
		}
	}
}
