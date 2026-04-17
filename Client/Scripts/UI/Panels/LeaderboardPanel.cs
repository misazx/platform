using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using RoguelikeGame.Network.Auth;
using RoguelikeGame.Packages;

namespace RoguelikeGame.UI.Panels
{
	public partial class LeaderboardPanel : Control
	{
		private OptionButton _packageSelector;
		private ItemList _rankList;
		private Label _titleLabel;
		private Label _statsLabel;
		private Label _userRankLabel;
		private Button _refreshButton;
		private Button _backButton;
		private Label _statusLabel;

		public event Action OnBack;

		private string _baseUrl = "";
		private System.Net.Http.HttpClient _httpClient;

		public override void _Ready()
		{
			_baseUrl = GetServerUrl();
			_httpClient = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(10) };
			CreateUI();
			PopulatePackageSelector();
			LoadLeaderboard();
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

		private void CreateUI()
		{
			SetAnchorsPreset(Control.LayoutPreset.FullRect);

			var bg = new ColorRect
			{
				Color = new Color(0.02f, 0.02f, 0.06f, 0.96f),
				MouseFilter = MouseFilterEnum.Ignore,
				AnchorsPreset = (int)Control.LayoutPreset.FullRect
			};
			AddChild(bg);

			var mainContainer = new VBoxContainer();
			mainContainer.AddThemeConstantOverride("separation", 12);
			mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
			mainContainer.CustomMinimumSize = new Vector2(750, 600);
			AddChild(mainContainer);

			var headerRow = new HBoxContainer();
			headerRow.AddThemeConstantOverride("separation", 15);
			mainContainer.AddChild(headerRow);

			_titleLabel = new Label
			{
				Text = "🏆 排行榜",
				CustomMinimumSize = new Vector2(300, 50)
			};
			_titleLabel.AddThemeFontSizeOverride("font_size", 30);
			_titleLabel.Modulate = new Color(1f, 0.85f, 0.3f);
			headerRow.AddChild(_titleLabel);

			headerRow.AddChild(new Label { Text = "选择玩法:", CustomMinimumSize = new Vector2(70, 35) });

			_packageSelector = new OptionButton();
			_packageSelector.CustomMinimumSize = new Vector2(220, 38);
			_packageSelector.ItemSelected += OnPackageChanged;
			headerRow.AddChild(_packageSelector);

			_refreshButton = new Button { Text = "🔄 刷新", CustomMinimumSize = new Vector2(90, 36) };
			_refreshButton.Pressed += () => LoadLeaderboard();
			headerRow.AddChild(_refreshButton);

			mainContainer.AddChild(new HSeparator());

			var statsRow = new HBoxContainer();
			statsRow.AddThemeConstantOverride("separation", 20);
			mainContainer.AddChild(statsRow);

			_statsLabel = new Label
			{
				Text = "",
				CustomMinimumSize = new Vector2(400, 28)
			};
			_statsLabel.AddThemeFontSizeOverride("font_size", 14);
			_statsLabel.Modulate = new Color(0.7f, 0.8f, 0.9f);
			statsRow.AddChild(_statsLabel);

			_userRankLabel = new Label
			{
				Text = "",
				CustomMinimumSize = new Vector2(250, 28),
				HorizontalAlignment = HorizontalAlignment.Right
			};
			_userRankLabel.AddThemeFontSizeOverride("font_size", 14);
			_userRankLabel.Modulate = new Color(0.4f, 0.9f, 0.6f);
			statsRow.AddChild(_userRankLabel);

			mainContainer.AddChild(new HSeparator());

			var listHeader = new HBoxContainer();
			listHeader.AddThemeConstantOverride("separation", 10);
			mainContainer.AddChild(listHeader);

			var headerLabels = new[] { "排名", "玩家", "分数", "层数", "击杀", "时间", "结果" };
			var headerWidths = new[] { 60, 150, 120, 60, 60, 80, 80 };
			for (int i = 0; i < headerLabels.Length; i++)
			{
				var lbl = new Label
				{
					Text = headerLabels[i],
					CustomMinimumSize = new Vector2(headerWidths[i], 28),
					HorizontalAlignment = HorizontalAlignment.Center
				};
				lbl.AddThemeFontSizeOverride("font_size", 13);
				lbl.Modulate = new Color(0.6f, 0.65f, 0.8f);
				listHeader.AddChild(lbl);
			}

			_rankList = new ItemList
			{
				CustomMinimumSize = new Vector2(720, 360),
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};
			mainContainer.AddChild(_rankList);

			_statusLabel = new Label
			{
				Text = "",
				HorizontalAlignment = HorizontalAlignment.Center,
				CustomMinimumSize = new Vector2(700, 25)
			};
			_statusLabel.AddThemeFontSizeOverride("font_size", 12);
			mainContainer.AddChild(_statusLabel);

			mainContainer.AddChild(new HSeparator());

			var backContainer = new CenterContainer();
			mainContainer.AddChild(backContainer);

			_backButton = new Button { Text = "← 返回主菜单", CustomMinimumSize = new Vector2(180, 42) };
			_backButton.Pressed += () => OnBack?.Invoke();
			backContainer.AddChild(_backButton);
		}

		private void PopulatePackageSelector()
		{
			_packageSelector.Clear();

			if (PackageManager.Instance == null) return;

			int selectedIndex = -1;
			int idx = 0;
			foreach (var kvp in PackageManager.Instance.AvailablePackages)
			{
				var pkg = kvp.Value;
				if (pkg.HasLeaderboard && PackageManager.Instance.IsPackageInstalled(pkg.Id))
				{
					string icon = pkg.SupportsMultiplayer ? "🌐" : "🎮";
					_packageSelector.AddItem($"{icon} {pkg.Name}");
					_packageSelector.SetItemMetadata(idx, pkg.Id);

					if (selectedIndex < 0 && pkg.Id == "base_game")
						selectedIndex = idx;

					idx++;
				}
			}

			if (_packageSelector.ItemCount > 0 && selectedIndex >= 0)
				_packageSelector.Selected = selectedIndex;
		}

		private void OnPackageChanged(long index)
		{
			if (index >= 0 && index < _packageSelector.ItemCount)
			{
				LoadLeaderboard();
			}
		}

		private async void LoadLeaderboard()
		{
			try
			{
				_statusLabel.Text = "⏳ 正在加载排行榜...";
				_statusLabel.Modulate = Colors.Yellow;

				string packageId = GetCurrentPackageId();
				if (string.IsNullOrEmpty(packageId)) return;

				string url = $"{_baseUrl}/api/leaderboard/{packageId}/top?top=50";

				if (AuthSystem.Instance?.IsAuthenticated == true)
				{
					_httpClient.DefaultRequestHeaders.Authorization =
						new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthSystem.Instance.Token);
				}
				else
				{
					if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
						_httpClient.DefaultRequestHeaders.Remove("Authorization");
				}

				var response = await _httpClient.GetAsync(url);
				var content = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(content);

				_rankList.Clear();

				if (!(result.TryGetProperty("success", out var sEl) && sEl.GetBoolean()))
				{
					_statusLabel.Text = "❌ 加载失败";
					_statusLabel.Modulate = Colors.Red;
					return;
				}

				if (result.TryGetProperty("data", out var dataArray) && dataArray.ValueKind == JsonValueKind.Array)
				{
					foreach (var entry in dataArray.EnumerateArray())
					{
						int rank = entry.TryGetProperty("rank", out var rEl) ? rEl.GetInt32() : 0;
						string username = entry.TryGetProperty("username", out var unEl) ? (unEl.GetString() ?? "???") : "???";
						long score = entry.TryGetProperty("score", out var scEl) ? scEl.GetInt64() : 0;
						int floor = entry.TryGetProperty("floorReached", out var f) ? f.GetInt32() : 0;
						int kills = entry.TryGetProperty("killCount", out var k) ? k.GetInt32() : 0;
						int timeSec = entry.TryGetProperty("playTimeSeconds", out var t) ? t.GetInt32() : 0;
						bool victory = entry.TryGetProperty("isVictory", out var v) && v.GetBoolean();

						string medal = rank switch
						{
							1 => "🥇",
							2 => "🥈",
							3 => "🥉",
							_ => $"#{rank}"
						};

						string victoryIcon = victory ? "✅" : "❌";

						FormatTime(timeSec, out string timeStr);

						string itemText =
							$"{medal,-6} {username,-14} {score,12}    {floor,3}   {kills,4}   {timeStr,8}   {victoryIcon}";

						_rankList.AddItem(itemText);

						Color rowColor = rank switch
						{
							1 => new Color(1f, 0.85f, 0.2f),
							2 => new Color(0.85f, 0.85f, 0.85f),
							3 => new Color(0.8f, 0.6f, 0.3f),
							_ => new Color(0.85f, 0.87f, 0.92f)
						};

						_rankList.SetItemCustomFgColor(_rankList.ItemCount - 1, rowColor);
					}
				}

				if (result.TryGetProperty("stats", out var stats))
				{
					int totalGames = stats.TryGetProperty("totalGames", out var tg) ? tg.GetInt32() : 0;
					int players = stats.TryGetProperty("uniquePlayers", out var up) ? up.GetInt32() : 0;
					int victories = stats.TryGetProperty("victories", out var v) ? v.GetInt32() : 0;
					long highScore = stats.TryGetProperty("highestScore", out var hs) ? hs.GetInt64() : 0;

					_statsLabel.Text = $"📊 总场次: {totalGames} | 玩家: {players} | 胜利: {victories} | 最高分: {highScore}";
				}

				if (AuthSystem.Instance?.IsAuthenticated == true && !string.IsNullOrEmpty(AuthSystem.Instance.CurrentUser?.Id))
				{
					LoadUserRank(packageId);
				}
				else
				{
					_userRankLabel.Text = "";
				}

				_statusLabel.Text = $"✅ 已加载 ({DateTime.Now:HH:mm:ss})";
				_statusLabel.Modulate = new Color(0.3f, 0.9f, 0.4f);
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[Leaderboard] 加载失败: {ex.Message}");
				_statusLabel.Text = "❌ 无法连接到服务器";
				_statusLabel.Modulate = Colors.Red;
			}
		}

		private async Task LoadUserRank(string packageId)
		{
			try
			{
				string userId = AuthSystem.Instance.CurrentUser?.Id ?? "";
				string url = $"{_baseUrl}/api/leaderboard/{packageId}/user/{userId}";

				var response = await _httpClient.GetAsync(url);
				var content = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JsonElement>(content);

				if (result.TryGetProperty("bestRank", out var rankEl) && rankEl.ValueKind != JsonValueKind.Null)
				{
					int rank = rankEl.GetInt32();
					_userRankLabel.Text = rank > 0 ? $"🎯 你的排名: #{rank}" : "暂无记录";
				}
				else
				{
					_userRankLabel.Text = "暂无记录";
				}
			}
			catch
			{
				_userRankLabel.Text = "";
			}
		}

		private string GetCurrentPackageId()
		{
			int idx = _packageSelector.Selected;
			if (idx >= 0)
			{
				var meta = _packageSelector.GetItemMetadata(idx);
				if (meta.VariantType == Variant.Type.String)
					return meta.AsString();
			}
			return "base_game";
		}

		private static void FormatTime(int totalSeconds, out string result)
		{
			if (totalSeconds < 60)
			{ result = $"{totalSeconds}s"; return; }
			int m = totalSeconds / 60;
			int s = totalSeconds % 60;
			result = $"{m}m{s}s";
		}

		public override void _ExitTree()
		{
			_httpClient?.Dispose();
			base._ExitTree();
		}
	}
}
