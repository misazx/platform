using System;
using System.Threading.Tasks;
using Godot;
using RoguelikeGame.Network;
using RoguelikeGame.Network.Core;
using RoguelikeGame.UI.Panels;

namespace RoguelikeGame.UI.Panels
{
	public partial class MultiplayerPanel : Control
	{
		private Button _lanButton;
		private Button _onlineButton;
		private Button _bluetoothButton;
		private Button _backButton;
		private Label _titleLabel;
		private Label _statusLabel;
		private LineEdit _serverAddressInput;
		private SpinBox _portInput;
		private ConnectionStatusIndicator _connectionIndicator;

		public event Action<string, int> OnLANSelected;
		public event Action<string, int> OnOnlineSelected;
		public event Action OnBluetoothSelected;
		public event Action OnBack;

		public override void _Ready()
		{
			CreateUI();
			SetupEventHandlers();
			UpdateConnectionStatus();
		}

		private void CreateUI()
		{
			SetAnchorsPreset(Control.LayoutPreset.FullRect);

			var bg = new ColorRect
			{
				Color = new Color(0, 0, 0, 0.92f),
				MouseFilter = MouseFilterEnum.Ignore,
				AnchorsPreset = (int)Control.LayoutPreset.FullRect
			};
			AddChild(bg);

			var mainPanel = new PanelContainer
			{
				CustomMinimumSize = new Vector2(650, 550),
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
				SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
			};
			mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
			var panelStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.08f, 0.07f, 0.12f, 0.98f),
				CornerRadiusTopLeft = 20,
				CornerRadiusTopRight = 20,
				CornerRadiusBottomLeft = 20,
				CornerRadiusBottomRight = 20,
				BorderWidthLeft = 3,
				BorderWidthRight = 3,
				BorderWidthTop = 3,
				BorderWidthBottom = 3,
				BorderColor = new Color(0.3f, 0.5f, 0.9f, 1f)
			};
			mainPanel.AddThemeStyleboxOverride("panel", panelStyle);
			AddChild(mainPanel);

			var vbox = new VBoxContainer();
			vbox.AddThemeConstantOverride("separation", 15);
			mainPanel.AddChild(vbox);

			var headerContainer = new HBoxContainer();
			vbox.AddChild(headerContainer);

			_titleLabel = new Label
			{
				Text = "🌐 多人游戏",
				HorizontalAlignment = HorizontalAlignment.Center,
				CustomMinimumSize = new Vector2(500, 50)
			};
			_titleLabel.AddThemeFontSizeOverride("font_size", 32);
			_titleLabel.Modulate = new Color(1f, 0.95f, 0.7f);
			headerContainer.AddChild(_titleLabel);

			_connectionIndicator = new ConnectionStatusIndicator();
			headerContainer.AddChild(_connectionIndicator);

			vbox.AddChild(new HSeparator());

			_statusLabel = new Label
			{
				Text = "选择连接方式开始多人游戏",
				HorizontalAlignment = HorizontalAlignment.Center,
				CustomMinimumSize = new Vector2(500, 30)
			};
			_statusLabel.AddThemeFontSizeOverride("font_size", 14);
			_statusLabel.Modulate = new Color(0.8f, 0.85f, 0.9f);
			vbox.AddChild(_statusLabel);

			vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 15) });

			_lanButton = CreateModeButton(
				"🏠 局域网对战",
				"与同一网络下的好友对战\n低延迟 · 无需互联网",
				new Color(0.2f, 0.6f, 0.4f)
			);
			_lanButton.Pressed += OnLANPressed;
			vbox.AddChild(_lanButton);

			_onlineButton = CreateModeButton(
				"🌍 在线多人游戏",
				"与全球玩家匹配对战\n需要账号登录",
				new Color(0.3f, 0.5f, 0.8f)
			);
			_onlineButton.Pressed += OnOnlinePressed;
			vbox.AddChild(_onlineButton);

			_bluetoothButton = CreateModeButton(
				"📱 蓝牙近距离联机",
				"无需WiFi即可联机\n适合移动设备",
				new Color(0.6f, 0.3f, 0.7f)
			);
			_bluetoothButton.Pressed += OnBluetoothPressed;
			vbox.AddChild(_bluetoothButton);

			vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });

			var serverConfigContainer = new VBoxContainer();
			serverConfigContainer.AddThemeConstantOverride("separation", 8);
			serverConfigContainer.AddThemeConstantOverride("separation", 8);
			vbox.AddChild(serverConfigContainer);

			var addressRow = new HBoxContainer();
			addressRow.AddThemeConstantOverride("separation", 10);
			addressRow.AddChild(new Label { Text = "服务器地址:", CustomMinimumSize = new Vector2(100, 0) });
			_serverAddressInput = new LineEdit
			{
				Text = "127.0.0.1",
				PlaceholderText = "IP地址或域名",
				CustomMinimumSize = new Vector2(200, 35)
			};
			addressRow.AddChild(_serverAddressInput);
			addressRow.AddChild(new Label { Text = "端口:", CustomMinimumSize = new Vector2(40, 0) });
			_portInput = new SpinBox
			{
				MinValue = 1000,
				MaxValue = 65535,
				Value = 5000,
				CustomMinimumSize = new Vector2(80, 35)
			};
			addressRow.AddChild(_portInput);
			serverConfigContainer.AddChild(addressRow);

			vbox.AddChild(new HSeparator());

			_backButton = new Button
			{
				Text = "← 返回主菜单",
				CustomMinimumSize = new Vector2(200, 45)
			};
			_backButton.AddThemeFontSizeOverride("font_size", 16);
			_backButton.Pressed += () => OnBack?.Invoke();
			var backContainer = new CenterContainer();
			backContainer.AddChild(_backButton);
			vbox.AddChild(backContainer);
		}

		private Button CreateModeButton(string title, string description, Color accentColor)
		{
			var button = new Button();
			button.CustomMinimumSize = new Vector2(550, 90);

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
				CustomMinimumSize = new Vector2(500, 40),
				AutowrapMode = TextServer.AutowrapMode.WordSmart
			};
			descLabel.AddThemeFontSizeOverride("font_size", 12);
			descLabel.Modulate = new Color(0.75f, 0.78f, 0.85f);
			vbox.AddChild(descLabel);

			var style = new StyleBoxFlat
			{
				BgColor = new Color(0.12f, 0.12f, 0.18f, 0.9f),
				CornerRadiusTopLeft = 12,
				CornerRadiusTopRight = 12,
				CornerRadiusBottomLeft = 12,
				CornerRadiusBottomRight = 12,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = accentColor * new Color(0.6f, 0.6f, 0.6f, 0.8f)
			};
			button.AddThemeStyleboxOverride("normal", style);

			var hoverStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.15f, 0.15f, 0.22f, 0.95f),
				CornerRadiusTopLeft = 12,
				CornerRadiusTopRight = 12,
				CornerRadiusBottomLeft = 12,
				CornerRadiusBottomRight = 12,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = accentColor
			};
			button.AddThemeStyleboxOverride("hover", hoverStyle);

			return button;
		}

		private void SetupEventHandlers()
		{
			if (NetworkManager.Instance != null)
			{
				NetworkManager.Instance.Connect(
					NetworkManager.SignalName.StateChanged,
					Callable.From((int newState, int oldState) => UpdateConnectionStatus())
				);
			}
		}

		private void UpdateConnectionStatus()
		{
			if (NetworkManager.Instance == null) return;

			var state = NetworkManager.Instance.State;
			string statusText;
			Color statusColor;

			switch (state)
			{
				case NetworkState.Disconnected:
					statusText = "未连接";
					statusColor = Colors.Gray;
					break;
				case NetworkState.Connecting:
					statusText = "正在连接...";
					statusColor = Colors.Yellow;
					break;
				case NetworkState.Connected:
					statusText = "已连接到服务器";
					statusColor = Colors.Green;
					break;
				case NetworkState.Authenticated:
					statusText = "已认证 ✓";
					statusColor = new Color(0.3f, 0.9f, 0.5f);
					break;
				default:
					statusText = state.ToString();
					statusColor = Colors.White;
					break;
			}

			_statusLabel.Text = $"状态: {statusText}";
			_statusLabel.Modulate = statusColor;

			_connectionIndicator?.UpdateStatus(state);
		}

		private async void OnLANPressed()
		{
			string address = _serverAddressInput.Text;
			int port = (int)_portInput.Value;

			GD.Print($"[MultiplayerPanel] 选择局域网模式: {address}:{port}");

			OnLANSelected?.Invoke(address, port);
		}

		private async void OnOnlinePressed()
		{
			string address = _serverAddressInput.Text;
			int port = (int)_portInput.Value;

			GD.Print($"[MultiplayerPanel] 选择在线模式: {address}:{port}");

			OnOnlineSelected?.Invoke(address, port);
		}

		private async void OnBluetoothPressed()
		{
			GD.Print("[MultiplayerPanel] 选择蓝牙模式");

			OnBluetoothSelected?.Invoke();
		}
	}
}
