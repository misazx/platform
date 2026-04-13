using System;
using Godot;
using RoguelikeGame.Network;
using RoguelikeGame.Packages;

namespace RoguelikeGame.UI.Panels
{
	public partial class GameModeSelectPanel : Control
	{
		private Label _titleLabel;
		private Label _packageInfoLabel;
		private Button _singlePlayerButton;
		private Button _createRoomButton;
		private Button _joinRoomButton;
		private Button _backButton;
		private Label _statusLabel;
		private PanelContainer _multiplayerSection;

		public event Action OnSinglePlayerSelected;
		public event Action OnCreateRoomSelected;
		public event Action OnJoinRoomSelected;
		public event Action OnBack;

		private PackageData _currentPackage;

		public override void _Ready()
		{
			CreateUI();
		}

		public void ShowForPackage(PackageData package)
		{
			_currentPackage = package;
			UpdateUI();
			Visible = true;
		}

		private void CreateUI()
		{
			SetAnchorsPreset(Control.LayoutPreset.FullRect);

			var bg = new ColorRect
			{
				Color = new Color(0, 0, 0, 0.94f),
				MouseFilter = MouseFilterEnum.Ignore,
				AnchorsPreset = (int)Control.LayoutPreset.FullRect
			};
			AddChild(bg);

			var mainPanel = new PanelContainer
			{
				CustomMinimumSize = new Vector2(520, 420),
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
				SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
			};
			mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
			var panelStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.07f, 0.06f, 0.12f, 0.98f),
				CornerRadiusTopLeft = 20,
				CornerRadiusTopRight = 20,
				CornerRadiusBottomLeft = 20,
				CornerRadiusBottomRight = 20,
				BorderWidthLeft = 3,
				BorderWidthRight = 3,
				BorderWidthTop = 3,
				BorderWidthBottom = 3,
				BorderColor = new Color(0.4f, 0.55f, 0.85f, 1f)
			};
			mainPanel.AddThemeStyleboxOverride("panel", panelStyle);
			AddChild(mainPanel);

			var vbox = new VBoxContainer();
			vbox.AddThemeConstantOverride("separation", 15);
			mainPanel.AddChild(vbox);

			_titleLabel = new Label
			{
				Text = "🎮 选择游戏模式",
				HorizontalAlignment = HorizontalAlignment.Center,
				CustomMinimumSize = new Vector2(480, 50)
			};
			_titleLabel.AddThemeFontSizeOverride("font_size", 28);
			_titleLabel.Modulate = new Color(1f, 0.9f, 0.6f);
			vbox.AddChild(_titleLabel);

			vbox.AddChild(new HSeparator());

			_packageInfoLabel = new Label
			{
				Text = "",
				HorizontalAlignment = HorizontalAlignment.Center,
				CustomMinimumSize = new Vector2(460, 40),
				AutowrapMode = TextServer.AutowrapMode.WordSmart
			};
			_packageInfoLabel.AddThemeFontSizeOverride("font_size", 14);
			_packageInfoLabel.Modulate = new Color(0.75f, 0.8f, 0.88f);
			vbox.AddChild(_packageInfoLabel);

			vbox.AddChild(new HSeparator());

			_singlePlayerButton = CreateModeButton(
				"🎯 单人模式",
				"独自探索，不受干扰\n享受完整的单机体验",
				new Color(0.25f, 0.7f, 0.45f)
			);
			_singlePlayerButton.Pressed += () => OnSinglePlayerSelected?.Invoke();
			vbox.AddChild(_singlePlayerButton);

			_multiplayerSection = new VBoxContainer();
			_multiplayerSection.AddThemeConstantOverride("separation", 10);
			vbox.AddChild(_multiplayerSection);

			_createRoomButton = CreateModeButton(
				"🏠 创建房间",
				"创建新房间，邀请好友加入\n等待其他玩家匹配",
				new Color(0.3f, 0.55f, 0.85f)
			);
			_createRoomButton.Pressed += () => OnCreateRoomSelected?.Invoke();
			_multiplayerSection.AddChild(_createRoomButton);

			_joinRoomButton = CreateModeButton(
				"🔍 加入房间",
				"浏览可用房间列表\n快速加入他人的游戏",
				new Color(0.65f, 0.4f, 0.8f)
			);
			_joinRoomButton.Pressed += () => OnJoinRoomSelected?.Invoke();
			_multiplayerSection.AddChild(_joinRoomButton);

			vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 15) });

			_statusLabel = new Label
			{
				Text = "",
				HorizontalAlignment = HorizontalAlignment.Center,
				CustomMinimumSize = new Vector2(460, 28),
				AutowrapMode = TextServer.AutowrapMode.WordSmart
			};
			_statusLabel.AddThemeFontSizeOverride("font_size", 13);
			vbox.AddChild(_statusLabel);

			vbox.AddChild(new HSeparator());

			var backContainer = new CenterContainer();
			vbox.AddChild(backContainer);

			_backButton = new Button { Text = "← 返回", CustomMinimumSize = new Vector2(150, 38) };
			_backButton.Pressed += () => OnBack?.Invoke();
			backContainer.AddChild(_backButton);
		}

		private void UpdateUI()
		{
			if (_currentPackage == null) return;

			string multiIcon = _currentPackage.SupportsMultiplayer ? "🌐" : "🎮";
			_titleLabel.Text = $"{multiIcon} {_currentPackage.Name} - 选择模式";

			_packageInfoLabel.Text =
				$"版本: {_currentPackage.Version} | " +
				$"评分: ⭐{_currentPackage.Score:F1} | " +
				$"{_currentPackage.DownloadCount:N0} 次游玩";

			if (_currentPackage.SupportsMultiplayer)
			{
				_multiplayerSection.Visible = true;
				_statusLabel.Text = $"支持最多 {_currentPackage.MaxPlayers} 人联机对战";
				_statusLabel.Modulate = new Color(0.4f, 0.85f, 0.6f);
			}
			else
			{
				_multiplayerSection.Visible = false;
				_statusLabel.Text = "此玩法仅支持单人模式";
				_statusLabel.Modulate = new Color(0.7f, 0.75f, 0.82f);
			}

			bool needAuth = _currentPackage.SupportsMultiplayer && (NetworkManager.Instance == null || !NetworkManager.Instance.IsOnline);
			if (_currentPackage.SupportsMultiplayer && needAuth)
			{
				_createRoomButton.Disabled = true;
				_joinRoomButton.Disabled = true;
				_statusLabel.Text += "\n⚠️ 多人模式需要先连接服务器";
			}
			else
			{
				_createRoomButton.Disabled = false;
				_joinRoomButton.Disabled = false;
			}
		}

		private static Button CreateModeButton(string title, string description, Color accentColor)
		{
			var button = new Button();
			button.CustomMinimumSize = new Vector2(460, 90);

			var vbox = new VBoxContainer();
			vbox.AddThemeConstantOverride("separation", 5);
			button.AddChild(vbox);

			var titleLabel = new Label
			{
				Text = title,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			titleLabel.AddThemeFontSizeOverride("font_size", 22);
			titleLabel.Modulate = accentColor;
			vbox.AddChild(titleLabel);

			var descLabel = new Label
			{
				Text = description,
				HorizontalAlignment = HorizontalAlignment.Center,
				CustomMinimumSize = new Vector2(430, 36),
				AutowrapMode = TextServer.AutowrapMode.WordSmart
			};
			descLabel.AddThemeFontSizeOverride("font_size", 11);
			descLabel.Modulate = new Color(0.68f, 0.72f, 0.82f);
			vbox.AddChild(descLabel);

			var style = new StyleBoxFlat
			{
				BgColor = new Color(0.12f, 0.12f, 0.18f, 0.92f),
				CornerRadiusTopLeft = 14,
				CornerRadiusTopRight = 14,
				CornerRadiusBottomLeft = 14,
				CornerRadiusBottomRight = 14,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = accentColor * new Color(0.55f, 0.55f, 0.55f, 0.75f)
			};
			button.AddThemeStyleboxOverride("normal", style);

			var hoverStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.16f, 0.16f, 0.24f, 0.96f),
				CornerRadiusTopLeft = 14,
				CornerRadiusTopRight = 14,
				CornerRadiusBottomLeft = 14,
				CornerRadiusBottomRight = 14,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = accentColor * new Color(0.75f, 0.75f, 0.75f, 0.95f)
			};
			button.AddThemeStyleboxOverride("hover", hoverStyle);

			return button;
		}
	}
}
