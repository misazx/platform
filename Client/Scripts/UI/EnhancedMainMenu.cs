using Godot;
using System;
using RoguelikeGame.Core;
using RoguelikeGame.Packages;

namespace RoguelikeGame.UI
{
	public partial class EnhancedMainMenu : Control
	{
		private Button _newGameButton;
		private Button _continueButton;
		private Button _packageStoreButton;
		private Button _quitButton;
		private Label _titleLabel;
		private Label _versionLabel;
		private PackageStoreUI _packageStoreUI;

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
		}

		private void ConnectSignals()
		{
			_newGameButton.Pressed += OnNewGamePressed;
			_packageStoreButton.Pressed += OnPackageStorePressed;
			_continueButton.Pressed += OnContinuePressed;
			_quitButton.Pressed += OnQuitPressed;

			if (PackageManager.Instance != null)
			{
				PackageManager.Instance.PackageListUpdated += UpdatePackageCount;
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

			int installedCount = PackageManager.Instance.InstalledPackages.Count(p =>
				p.Value.Status == PackageStatus.Installed);

			int availableCount = PackageManager.Instance.AvailablePackages.Count;

			_versionLabel.Text =
				$"版本: {ProjectSettings.GetSetting("application/config/version", "1.0.0")} | " +
				$"已安装: {installedCount}/{availableCount} 个包";
		}

		private void OnNewGamePressed()
		{
			GD.Print("[EnhancedMainMenu] Starting base game");
			Hide();

			if (PackageManager.Instance != null &&
			    PackageManager.Instance.CanLaunchPackage("base_game"))
			{
				PackageManager.Instance.LaunchPackage("base_game");
			}
			else
			{
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
			Hide();
		}

		private Texture2D TryLoadBackground()
		{
			try
			{
				string[] possiblePaths =
				{
					"res://Images/Backgrounds/glory.png",
					"res://Images/Backgrounds/overgrowth.png"
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
