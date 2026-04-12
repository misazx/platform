using Godot;
using System;
using System.Threading.Tasks;
using RoguelikeGame.Core;
using RoguelikeGame.UI.Panels;
using RoguelikeGame.Network;
using RoguelikeGame.Network.Auth;
using RoguelikeGame.Network.Rooms;

namespace RoguelikeGame.UI
{
	public partial class MainMenuNetworkIntegrator : Node
	{
		private static MainMenuNetworkIntegrator _instance;

		public static MainMenuNetworkIntegrator Instance => _instance;

		private Control _mainMenu;
		private Button _multiplayerButton;
		private MultiplayerPanel _multiplayerPanel;
		private LoginPanel _loginPanel;
		private LobbyPanel _lobbyPanel;
		private ConnectionStatusIndicator _connectionIndicator;
		private bool _isInitialized = false;

		public event Action OnMultiplayerModeEntered;
		public event Action OnSinglePlayerModeEntered;

		public override void _Ready()
		{
			if (_instance != null && _instance != this)
			{
				QueueFree();
				return;
			}

			_instance = this;
			ProcessMode = ProcessModeEnum.Always;

			GD.Print("[MainMenuNetworkIntegrator] 初始化主菜单网络集成...");
		}

		public async Task InitializeAsync(Control mainMenu)
		{
			if (_isInitialized) return;

			_mainMenu = mainMenu;

			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

			AddMultiplayerButtonToMenu();
			CreateNetworkPanels();
			SetupEventHandlers();

			_isInitialized = true;

			GD.Print("[MainMenuNetworkIntegrator] ✓ 主菜单网络集成完成");
		}

		private void AddMultiplayerButtonToMenu()
		{
			if (_mainMenu == null) return;

			var vbox = FindVBoxContainer(_mainMenu);
			if (vbox == null)
			{
				GD.PushWarning("[MainMenuNetworkIntegrator] 未找到VBoxContainer，使用备用方案");
				vbox = (VBoxContainer)_mainMenu;
			}

			int insertIndex = 1; // 在"开始游戏"按钮之后

			_multiplayerButton = new Button
			{
				Text = "🌐 多人游戏",
				CustomMinimumSize = new Vector2(280, 55)
			};
			_multiplayerButton.AddThemeFontSizeOverride("font_size", 19);

			var style = new StyleBoxFlat
			{
				BgColor = new Color(0.15f, 0.18f, 0.28f, 0.9f),
				CornerRadiusTopLeft = 10,
				CornerRadiusTopRight = 10,
				CornerRadiusBottomLeft = 10,
				CornerRadiusBottomRight = 10,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = new Color(0.4f, 0.5f, 0.9f, 0.7f),
				ContentMarginTop = 12,
				ContentMarginBottom = 12
			};
			_multiplayerButton.AddThemeStyleboxOverride("normal", style);

			var hoverStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.2f, 0.24f, 0.38f, 0.95f),
				CornerRadiusTopLeft = 10,
				CornerRadiusTopRight = 10,
				CornerRadiusBottomLeft = 10,
				CornerRadiusBottomRight = 10,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = new Color(0.5f, 0.6f, 1.0f, 0.9f)
			};
			_multiplayerButton.AddThemeStyleboxOverride("hover", hoverStyle);

			_multiplayerButton.Pressed += OnMultiplayerPressed;

			if (insertIndex < vbox.GetChildCount())
			{
				vbox.AddChild(_multiplayerButton);
				vbox.MoveChild(_multiplayerButton, insertIndex);
			}
			else
			{
				vbox.AddChild(_multiplayerButton);
			}

			_connectionIndicator = new ConnectionStatusIndicator();
			var indicatorContainer = new HBoxContainer();
			indicatorContainer.AddChild(new Control { CustomMinimumSize = new Vector2(100, 0) });
			indicatorContainer.AddChild(_connectionIndicator);
			vbox.AddChild(indicatorContainer);

			GD.Print("[MainMenuNetworkIntegrator] ✓ 已添加多人游戏按钮");
		}

		private void CreateNetworkPanels()
		{
			_multiplayerPanel = new MultiplayerPanel();
			_multiplayerPanel.Visible = false;
			_multiplayerPanel.OnLANSelected += OnLANSelectedHandler;
			_multiplayerPanel.OnOnlineSelected += OnOnlineSelectedHandler;
			_multiplayerPanel.OnBluetoothSelected += OnBluetoothSelectedHandler;
			_multiplayerPanel.OnBack += () => ShowMainMenu();
			GetTree().Root.AddChild(_multiplayerPanel);

			_loginPanel = new LoginPanel();
			_loginPanel.Visible = false;
			_loginPanel.OnLoginSuccess += OnLoginSuccessHandler;
			_loginPanel.OnBack += () => _multiplayerPanel.Visible = true;
			GetTree().Root.AddChild(_loginPanel);

			_lobbyPanel = new LobbyPanel();
			_lobbyPanel.Visible = false;
			_lobbyPanel.OnLogout += OnLogoutFromLobby;
			_lobbyPanel.OnBackToMenu += ShowMainMenu;
			GetTree().Root.AddChild(_lobbyPanel);

			GD.Print("[MainMenuNetworkIntegrator] ✓ 已创建所有网络面板");
		}

		private void SetupEventHandlers()
		{
			if (NetworkManager.Instance != null)
			{
				NetworkManager.Instance.Connect(
					NetworkManager.SignalName.StateChanged,
					Callable.From((int newState, int oldState) => UpdateConnectionIndicator())
				);
			}
		}

		private VBoxContainer FindVBoxContainer(Node node)
		{
			foreach (var child in node.GetChildren())
			{
				if (child is VBoxContainer vbox)
				{
					return vbox;
				}
				var result = FindVBoxContainer(child);
				if (result != null) return result;
			}
			return null;
		}

		private void OnMultiplayerPressed()
		{
			GD.Print("[MainMenuNetworkIntegrator] 多人游戏按钮点击");

			_mainMenu?.Hide();
			_multiplayerPanel.Visible = true;

			OnMultiplayerModeEntered?.Invoke();
		}

		private async void OnLANSelectedHandler(string address, int port)
		{
			GD.Print($"[MainMenuNetworkIntegrator] LAN模式选择: {address}:{port}");

			bool connected = await NetworkManager.Instance.ConnectToLANAsync(address, port);

			if (connected)
			{
				_multiplayerPanel.Visible = false;
				_lobbyPanel.Visible = true;
				_lobbyPanel.Update();
			}
			else
			{
				GD.PrintErr("[MainMenuNetworkIntegrator] LAN连接失败");
			}
		}

		private async void OnOnlineSelectedHandler(string address, int port)
		{
			GD.Print($"[MainMenuNetworkIntegrator] 在线模式选择: {address}:{port}");

			AuthSystem.Instance.SetBaseUrl($"http://{address}:{port}");
			RoomManager.Instance.SetBaseUrl($"http://{address}:{port}");

			bool connected = await NetworkManager.Instance.ConnectToOnlineAsync(address, port);

			if (connected)
			{
				_multiplayerPanel.Visible = false;
				_loginPanel.Visible = true;
			}
			else
			{
				GD.PrintErr("[MainMenuNetworkIntegrator] 在线连接失败");
			}
		}

		private void OnBluetoothSelectedHandler()
		{
			GD.Print("[MainMenuNetworkIntegrator] 蓝牙模式选择");

			// TODO: 实现蓝牙连接逻辑
		}

		private void OnLoginSuccessHandler()
		{
			GD.Print("[MainMenuNetworkIntegrator] 登录成功");

			_loginPanel.Visible = false;
			_lobbyPanel.Visible = true;
			_lobbyPanel.Update();
		}

		private void OnLogoutFromLobby()
		{
			GD.Print("[MainMenuNetworkIntegrator] 从大厅登出");

			AuthSystem.Instance.PerformLogout();

			_lobbyPanel.Visible = false;
			_multiplayerPanel.Visible = true;
		}

		public void ShowMainMenu()
		{
			_multiplayerPanel?.Hide();
			_loginPanel?.Hide();
			_lobbyPanel?.Hide();
			_mainMenu?.Show();
		}

		public void ShowLobby()
		{
			_mainMenu?.Hide();
			_multiplayerPanel?.Hide();
			_loginPanel?.Hide();
			if (_lobbyPanel != null)
			{
				_lobbyPanel.Visible = true;
				_lobbyPanel.Update();
			}
		}

		private void UpdateConnectionIndicator()
		{
			if (_connectionIndicator != null && NetworkManager.Instance != null)
			{
				_connectionIndicator.UpdateStatus(NetworkManager.Instance.State);
			}
		}

		public override void _ExitTree()
		{
			_instance = null;
		}
	}
}
