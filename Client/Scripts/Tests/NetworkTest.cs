using System;
using System.Threading.Tasks;
using Godot;
using RoguelikeGame.Network;
using RoguelikeGame.Network.Core;

public partial class NetworkTest : Control
{
	private Label _statusLabel;
	private Button _connectButton;
	private Button _disconnectButton;
	private Button _sendTestButton;
	private LineEdit _addressInput;
	private SpinBox _portInput;
	private RichTextLabel _logOutput;

	public override void _Ready()
	{
		CreateUI();
		SetupEventHandlers();

		GD.Print("[NetworkTest] 网络测试面板已加载");
	}

	private void CreateUI()
	{
		AnchorsPreset = (int)Control.LayoutPreset.FullRect;

		var mainPanel = new PanelContainer
		{
			CustomMinimumSize = new Vector2(600, 500),
			SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
			SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
		};
		mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);

		var style = new StyleBoxFlat
		{
			BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f),
			CornerRadiusTopLeft = 15,
			CornerRadiusTopRight = 15,
			CornerRadiusBottomLeft = 15,
			CornerRadiusBottomRight = 15,
			BorderWidthLeft = 2,
			BorderWidthRight = 2,
			BorderWidthTop = 2,
			BorderWidthBottom = 2,
			BorderColor = new Color(0.3f, 0.6f, 1.0f)
		};
		mainPanel.AddThemeStyleboxOverride("panel", style);

		var vbox = new VBoxContainer { SeparationOffset = 10 };
		mainPanel.AddChild(vbox);

		var titleLabel = new Label
		{
			Text = "🌐 网络系统测试面板",
			HorizontalAlignment = HorizontalAlignment.Center
		};
		titleLabel.AddThemeFontSizeOverride("font_size", 24);
		vbox.AddChild(titleLabel);

		_statusLabel = new Label { Text = "状态: 未连接" };
		vbox.AddChild(_statusLabel);

		var addressContainer = new HBoxContainer { SeparationOffset = 10 };
		vbox.AddChild(addressContainer);

		addressContainer.AddChild(new Label { Text = "地址:" });
		_addressInput = new LineEdit { Text = "127.0.0.1", CustomMinimumSize = new Vector2(150, 0) };
		addressContainer.AddChild(_addressInput);

		addressContainer.AddChild(new Label { Text = "端口:" });
		_portInput = new SpinBox { MinValue = 1000, MaxValue = 65535, Value = 5000, CustomMinimumSize = new Vector2(80, 0) };
		addressContainer.AddChild(_portInput);

		var buttonContainer = new HBoxContainer { SeparationOffset = 10 };
		vbox.AddChild(buttonContainer);

		_connectButton = new Button { Text = "连接服务器", CustomMinimumSize = new Vector2(120, 40) };
		_connectButton.Pressed += OnConnectPressed;
		buttonContainer.AddChild(_connectButton);

		_disconnectButton = new Button { Text = "断开连接", Disabled = true, CustomMinimumSize = new Vector2(120, 40) };
		_disconnectButton.Pressed += OnDisconnectPressed;
		buttonContainer.AddChild(_disconnectButton);

		_sendTestButton = new Button { Text = "发送测试数据", Disabled = true, CustomMinimumSize = new Vector2(140, 40) };
		_sendTestButton.Pressed += OnSendTestPressed;
		buttonContainer.AddChild(_sendTestButton);

		vbox.AddChild(new HSeparator());

		var logLabel = new Label { Text = "日志输出:" };
		vbox.AddChild(logLabel);

		_logOutput = new RichTextLabel
		{
			CustomMinimumSize = new Vector2(0, 250),
			FitContent = true
		};
		vbox.AddChild(_logOutput);

		AddChild(mainPanel);
	}

	private void SetupEventHandlers()
	{
		if (NetworkManager.Instance == null) return;

		NetworkManager.Instance.Connect(
			NetworkManager.SignalName.StateChanged,
			new Callable(this, nameof(OnStateChanged))
		);

		NetworkManager.Instance.Connect(
			NetworkManager.SignalName.ConnectionSucceeded,
			new Callable(this, nameof(OnConnectionSucceeded))
		);

		NetworkManager.Instance.Connect(
			NetworkManager.SignalName.ConnectionFailed,
			new Callable(this, nameof(OnConnectionFailed))
		);
	}

	private async void OnConnectPressed()
	{
		string address = _addressInput.Text;
		int port = (int)_portInput.Value;

		Log($"正在连接到 {address}:{port}...");

		bool success = await NetworkManager.Instance.ConnectToLANAsync(address, port);

		if (success)
		{
			Log("✓ 连接成功！");
		}
		else
		{
			Log("✗ 连接失败");
		}
	}

	private async void OnDisconnectPressed()
	{
		Log("正在断开连接...");
		await NetworkManager.Instance.DisconnectAsync();
		Log("已断开连接");
	}

	private async void OnSendTestPressed()
	{
		var testData = new
		{
			type = "test_message",
			content = "Hello from client!",
			timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
			testData = new[] { 1, 2, 3, 4, 5 }
		};

		var packetData = PacketSerializer.SerializePacket(PacketType.Ping, testData);

		await ConnectionManager.Instance.SendAsync(packetData);

		Log($"发送测试数据: {packetData.Length} 字节");
	}

	private void OnStateChanged(int newState, int oldState)
	{
		var state = (NetworkState)newState;
		UpdateStatusUI(state);
		Log($"状态变更: {(NetworkState)oldState} → {state}");
	}

	private void OnConnectionSucceeded()
	{
		Log("事件触发: 连接成功");
	}

	private void OnConnectionFailed(string error)
	{
		Log($"事件触发: 连接失败 - {error}");
	}

	private void UpdateStatusUI(NetworkState state)
	{
		string statusText = $"状态: {state}";
		_statusLabel.Text = statusText;

		bool isConnected = state != NetworkState.Disconnected && state != NetworkState.Connecting;

		_connectButton.Disabled = isConnected;
		_disconnectButton.Disabled = !isConnected;
		_sendTestButton.Disabled = !isConnected;

		switch (state)
		{
			case NetworkState.Disconnected:
				_statusLabel.Modulate = Colors.Gray;
				break;
			case NetworkState.Connecting:
				_statusLabel.Modulate = Colors.Yellow;
				break;
			case NetworkState.Connected:
			case NetworkState.Authenticated:
			case NetworkState.InLobby:
			case NetworkState.InRoom:
			case NetworkState.InGame:
				_statusLabel.Modulate = Colors.Green;
				break;
			default:
				_statusLabel.Modulate = Colors.White;
				break;
		}
	}

	private void Log(string message)
	{
		string timestamp = DateTime.Now.ToString("HH:mm:ss");
		string logLine = $"[{timestamp}] {message}\n";

		GD.Print($"[NetworkTest] {message}");

		_logOutput.AppendText(logLine);
		_logOutput.ScrollToLine(__logOutput.GetLineCount() - 1);
	}
}
