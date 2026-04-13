using Godot;
using System;
using System.Threading.Tasks;
using RoguelikeGame.Core;
using RoguelikeGame.Packages;
using RoguelikeGame.Network;
using RoguelikeGame.Network.Auth;
using RoguelikeGame.UI.Panels;

namespace RoguelikeGame.UI
{
	public partial class EnhancedMainMenu : Control
	{
		private Button _newGameButton;
		private Button _continueButton;
		private Button _packageStoreButton;
		private Button _multiplayerButton;
		private Button _leaderboardButton;
		private Button _quitButton;
		private Label _titleLabel;
		private Label _versionLabel;
		private Label _userStatusLabel;
		private Button _authButton;
		private PackageStoreUI _packageStoreUI;
		private MultiplayerPanel _multiplayerPanel;
		private LeaderboardPanel _leaderboardPanel;
		private GameModeSelectPanel _gameModeSelectPanel;

		private PackageData _currentLaunchingPackage;

		[Export]
		public string GameTitle { get; set; } = "Roguelike Game";

		public override void _Ready()
		{
			SetupUI();
			ConnectSignals();
			InitializePackageSystem();
		}

		private void SetupUI()
		{
			SetAnchorsPreset(Control.LayoutPreset.FullRect);

			var bgTexture = TryLoadBackground();
			if (bgTexture != null)
			{
				var bgRect = new TextureRect
				{
					Texture = bgTexture,
					StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
					AnchorsPreset = (int)Control.LayoutPreset.FullRect,
					MouseFilter = MouseFilterEnum.Ignore
				};
				AddChild(bgRect);
			}
			else
			{
				var bgColor = new ColorRect
				{
					Color = new Color(0.05f, 0.05f, 0.1f, 1f),
					AnchorsPreset = (int)Control.LayoutPreset.FullRect,
					MouseFilter = MouseFilterEnum.Ignore
				};
				AddChild(bgColor);
			}

			var centerContainer = new CenterContainer();
			centerContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
			AddChild(centerContainer);

			var vbox = new VBoxContainer();
			vbox.Alignment = BoxContainer.AlignmentMode.Center;
			vbox.AddThemeConstantOverride("separation", 18);
			centerContainer.AddChild(vbox);

			_titleLabel = new Label();
			_titleLabel.Text = GameTitle;
			_titleLabel.AddThemeFontSizeOverride("font_size", 52);
			_titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
			_titleLabel.Modulate = new Color(1f, 0.95f, 0.8f);
			vbox.AddChild(_titleLabel);

			CreateUserBar(vbox);

			var subtitleLabel = new Label
			{
				Text = "✨ 多玩法包系统 v1.0 ✨",
				HorizontalAlignment = HorizontalAlignment.Center,
				MouseFilter = MouseFilterEnum.Ignore
			};
			subtitleLabel.AddThemeFontSizeOverride("font_size", 16);
			subtitleLabel.Modulate = new Color(0.7f, 0.8f, 1f);
			vbox.AddChild(subtitleLabel);

			var spacer1 = new Control();
			spacer1.CustomMinimumSize = new Vector2(0, 40);
			vbox.AddChild(spacer1);

			_newGameButton = CreateStyledButton("🎮 开始游戏");
			vbox.AddChild(_newGameButton);

			_packageStoreButton = CreateStyledButton("📦 游戏包商店");
		_packageStoreButton.Modulate = new Color(0.9f, 0.85f, 1f);
		vbox.AddChild(_packageStoreButton);

		_multiplayerButton = CreateStyledButton("🌐 多人游戏");
		_multiplayerButton.Modulate = new Color(0.7f, 0.85f, 1f);
		vbox.AddChild(_multiplayerButton);

		_leaderboardButton = CreateStyledButton("🏆 排行榜");
		_leaderboardButton.Modulate = new Color(1f, 0.85f, 0.3f);
		vbox.AddChild(_leaderboardButton);

		_continueButton = CreateStyledButton("📂 继续游戏");
			_continueButton.Disabled = true;
			vbox.AddChild(_continueButton);

			var spacer2 = new Control();
			spacer2.CustomMinimumSize = new Vector2(0, 30);
			vbox.AddChild(spacer2);

			_quitButton = CreateStyledButton("🚪 退出游戏");
			_quitButton.Modulate = new Color(0.8f, 0.8f, 0.85f);
			vbox.AddChild(_quitButton);

			_versionLabel = new Label
			{
				Text = $"版本: {ProjectSettings.GetSetting("application/config/version", "1.0.0")} | 已安装包: 0",
				HorizontalAlignment = HorizontalAlignment.Center,
				MouseFilter = MouseFilterEnum.Ignore,
				CustomMinimumSize = new Vector2(400, 20)
			};
			_versionLabel.AddThemeFontSizeOverride("font_size", 12);
			_versionLabel.Modulate = new Color(0.6f, 0.65f, 0.7f);
			vbox.AddChild(_versionLabel);

			CreatePackageStoreOverlay();
		}

		private Button CreateStyledButton(string text)
		{
			var button = new Button();
			button.Text = text;
			button.CustomMinimumSize = new Vector2(280, 55);
			button.AddThemeFontSizeOverride("font_size", 19);

			var style = new StyleBoxFlat
			{
				BgColor = new Color(0.15f, 0.15f, 0.25f, 0.9f),
				CornerRadiusTopLeft = 10,
				CornerRadiusTopRight = 10,
				CornerRadiusBottomLeft = 10,
				CornerRadiusBottomRight = 10,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = new Color(0.4f, 0.5f, 0.7f, 0.7f),
				ContentMarginTop = 12,
				ContentMarginBottom = 12
			};
			button.AddThemeStyleboxOverride("normal", style);

			var hoverStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.2f, 0.22f, 0.35f, 0.95f),
				CornerRadiusTopLeft = 10,
				CornerRadiusTopRight = 10,
				CornerRadiusBottomLeft = 10,
				CornerRadiusBottomRight = 10,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = new Color(0.5f, 0.6f, 0.9f, 0.9f)
			};
			button.AddThemeStyleboxOverride("hover", hoverStyle);

			return button;
		}

		private void CreatePackageStoreOverlay()
		{
			_packageStoreUI = new PackageStoreUI();
			_packageStoreUI.Visible = false;
			_packageStoreUI.PackageLaunchRequested += OnPackageLaunchedFromStore;
			AddChild(_packageStoreUI);

			_multiplayerPanel = new MultiplayerPanel();
			_multiplayerPanel.Visible = false;
			_multiplayerPanel.OnLANSelected += OnMultiplayerLANSelected;
			_multiplayerPanel.OnOnlineSelected += OnMultiplayerOnlineSelected;
			_multiplayerPanel.OnBack += OnMultiplayerBack;
			AddChild(_multiplayerPanel);

			_leaderboardPanel = new LeaderboardPanel();
			_leaderboardPanel.Visible = false;
			_leaderboardPanel.OnBack += () => _leaderboardPanel.Visible = false;
			AddChild(_leaderboardPanel);

			_gameModeSelectPanel = new GameModeSelectPanel();
			_gameModeSelectPanel.Visible = false;
			_gameModeSelectPanel.OnSinglePlayerSelected += OnSinglePlayerSelected;
			_gameModeSelectPanel.OnCreateRoomSelected += OnCreateRoomSelected;
			_gameModeSelectPanel.OnJoinRoomSelected += OnJoinRoomSelected;
			_gameModeSelectPanel.OnBack += () => _gameModeSelectPanel.Visible = false;
			AddChild(_gameModeSelectPanel);
		}

		private void ConnectSignals()
		{
			_newGameButton.Pressed += OnNewGamePressed;
			_packageStoreButton.Pressed += OnPackageStorePressed;
			_multiplayerButton.Pressed += OnMultiplayerPressed;
			_leaderboardButton.Pressed += OnLeaderboardPressed;
			_continueButton.Pressed += OnContinuePressed;
			_quitButton.Pressed += OnQuitPressed;

			if (PackageManager.Instance != null)
			{
				PackageManager.Instance.PackageListUpdated += UpdatePackageCount;
			}

			if (AuthSystem.Instance != null)
			{
				AuthSystem.Instance.Connect(AuthSystem.SignalName.LoginCompleted, Callable.From((bool success, string msg) =>
				{
					if (success) UpdateAuthUI();
				}));
				AuthSystem.Instance.Connect(AuthSystem.SignalName.Logout, Callable.From(() =>
				{
					UpdateAuthUI();
				}));
			}
		}

		private async void InitializePackageSystem()
		{
			GD.Print("[EnhancedMainMenu] Initializing package system...");
			await Task.Delay(500); // 等待PackageManager初始化

			UpdatePackageCount();

			GD.Print("[EnhancedMainMenu] Package system ready!");
		}

		private void UpdatePackageCount()
		{
			if (PackageManager.Instance == null) return;

			int installedCount = 0;
			foreach (var p in PackageManager.Instance.InstalledPackages)
			{
				if (p.Value.Status == PackageStatus.Installed)
					installedCount++;
			}

			int availableCount = PackageManager.Instance.AvailablePackages.Count;

			_versionLabel.Text =
				$"版本: {ProjectSettings.GetSetting("application/config/version", "1.0.0")} | " +
				$"已安装: {installedCount}/{availableCount} 个包";
		}

		private void OnNewGamePressed()
		{
			GD.Print("[EnhancedMainMenu] Starting base game");

			var pkg = PackageManager.Instance?.GetPackage("base_game");
			if (pkg != null)
			{
				_currentLaunchingPackage = pkg;
				_gameModeSelectPanel.ShowForPackage(pkg);
			}
			else
			{
				Hide();
				GameInitializer.QuickStart();
			}
		}

		private void OnPackageStorePressed()
		{
			GD.Print("[EnhancedMainMenu] Opening package store");
			_packageStoreUI.Visible = true;
			_packageStoreUI.RefreshPackageList();
		}

		private void OnContinuePressed()
		{
			GD.Print("[EnhancedMainMenu] Continue not implemented yet");
		}

		private void OnQuitPressed()
		{
			GD.Print("[EnhancedMainMenu] Quitting game");
			GetTree().Quit();
		}

		private void OnPackageLaunchedFromStore(PackageData package)
		{
			GD.Print($"[EnhancedMainMenu] Launching package: {package.Name}");
			_packageStoreUI.Visible = false;
			_currentLaunchingPackage = package;
			_gameModeSelectPanel.ShowForPackage(package);
		}

		private void OnMultiplayerPressed()
		{
			GD.Print("[EnhancedMainMenu] Opening multiplayer panel");
			_multiplayerPanel.Visible = true;
		}

		private async void OnMultiplayerLANSelected(string address, int port)
		{
			GD.Print($"[EnhancedMainMenu] LAN mode selected: {address}:{port}");

			if (NetworkManager.Instance == null) return;

			bool connected = await NetworkManager.Instance.ConnectToLANAsync(address, port);
			if (connected)
			{
				GD.Print("[EnhancedMainMenu] LAN connected, showing login");
				ShowLoginPanel();
			}
		}

		private async void OnMultiplayerOnlineSelected(string address, int port)
		{
			GD.Print($"[EnhancedMainMenu] Online mode selected: {address}:{port}");

			if (NetworkManager.Instance == null || AuthSystem.Instance == null) return;

			AuthSystem.Instance.SetBaseUrl($"http://{address}:{port}");

			bool connected = await NetworkManager.Instance.ConnectToOnlineAsync(address, port);
			if (connected)
			{
				GD.Print("[EnhancedMainMenu] Online connected, showing login");
				ShowLoginPanel();
			}
		}

		private void OnMultiplayerBack()
		{
			GD.Print("[EnhancedMainMenu] Back from multiplayer panel");
			_multiplayerPanel.Visible = false;
		}

		private void ShowLoginPanel()
		{
			var loginPanel = new LoginPanel();
			loginPanel.OnLoginSuccess += OnLoginSuccess;
			loginPanel.OnBack += () => { loginPanel.QueueFree(); };
			AddChild(loginPanel);
		}

		private void OnLoginSuccess()
		{
			GD.Print("[EnhancedMainMenu] Login successful, entering lobby");
			EnterLobby();
		}

		private void EnterLobby()
		{
			var lobbyPanel = new LobbyPanel();
			lobbyPanel.OnStartGame += OnStartNetworkedGame;
			lobbyPanel.OnLeave += OnLeaveLobby;
			AddChild(lobbyPanel);
		}

		private void OnStartNetworkedGame()
		{
			GD.Print("[EnhancedMainMenu] Starting networked game");
			_multiplayerPanel.Visible = false;
			Hide();
		}

		private void OnLeaveLobby()
		{
			GD.Print("[EnhancedMainMenu] Leaving lobby");
		}

		private void CreateUserBar(VBoxContainer parent)
		{
			var userBar = new HBoxContainer();
			userBar.AddThemeConstantOverride("separation", 10);
			parent.AddChild(userBar);

			_userStatusLabel = new Label
			{
				Text = "",
				HorizontalAlignment = HorizontalAlignment.Left,
				CustomMinimumSize = new Vector2(300, 28),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_userStatusLabel.AddThemeFontSizeOverride("font_size", 13);
			userBar.AddChild(_userStatusLabel);

			userBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });

			_authButton = new Button
			{
				Text = "🔐 登录 / 注册",
				CustomMinimumSize = new Vector2(140, 32)
			};
			_authButton.AddThemeFontSizeOverride("font_size", 13);
			_authButton.Pressed += OnAuthButtonPressed;
			userBar.AddChild(_authButton);

			UpdateAuthUI();
		}

		private void UpdateAuthUI()
		{
			if (AuthSystem.Instance?.IsAuthenticated == true && AuthSystem.Instance.CurrentUser != null)
			{
				var user = AuthSystem.Instance.CurrentUser;
				_userStatusLabel.Text = $"👤 {user.Username} | Lv.{user.Level} | 胜场: {user.GamesWon}";
				_userStatusLabel.Modulate = new Color(0.4f, 0.9f, 0.6f);
				_authButton.Text = "🚪 登出";
			}
			else
			{
				_userStatusLabel.Text = "未登录 - 单人模式可用，多人模式需登录";
				_userStatusLabel.Modulate = new Color(0.6f, 0.62f, 0.68f);
				_authButton.Text = "🔐 登录 / 注册";
			}
		}

		private void OnAuthButtonPressed()
		{
			if (AuthSystem.Instance?.IsAuthenticated == true)
			{
				AuthSystem.Instance.PerformLogout();
				UpdateAuthUI();
			}
			else
			{
				ShowLoginPanel();
			}
		}

		private void OnLeaderboardPressed()
		{
			GD.Print("[EnhancedMainMenu] Opening leaderboard");
			_leaderboardPanel.Visible = true;
		}

		private void OnSinglePlayerSelected()
		{
			GD.Print("[EnhancedMainMenu] Single player mode selected");
			_gameModeSelectPanel.Visible = false;
			Hide();

			if (_currentLaunchingPackage != null && PackageManager.Instance?.CanLaunchPackage(_currentLaunchingPackage.Id) == true)
			{
				PackageManager.Instance.LaunchPackage(_currentLaunchingPackage.Id);
			}
			else if (_currentLaunchingPackage?.Id == "base_game")
			{
				GameInitializer.QuickStart();
			}
		}

		private void OnCreateRoomSelected()
		{
			GD.Print("[EnhancedMainMenu] Create room selected");
			_gameModeSelectPanel.Visible = false;

			if (!EnsureAuthenticated()) return;

			EnterLobby();
		}

		private void OnJoinRoomSelected()
		{
			GD.Print("[EnhancedMainMenu] Join room selected");
			_gameModeSelectPanel.Visible = false;

			if (!EnsureAuthenticated()) return;

			EnterLobby();
		}

		private bool EnsureAuthenticated()
		{
			if (AuthSystem.Instance?.IsAuthenticated == true) return true;

			ShowLoginPanel();
			return false;
		}

		private Texture2D TryLoadBackground()
		{
			try
			{
				string[] possiblePaths =
				{
					"res://GameModes/base_game/Resources/Images/Backgrounds/glory.png",
					"res://GameModes/base_game/Resources/Images/Backgrounds/overgrowth.png"
				};

				foreach (var path in possiblePaths)
				{
					if (ResourceLoader.Exists(path))
					{
						return GD.Load<Texture2D>(path);
					}
				}
			}
			catch { }

			return null;
		}
	}
}
