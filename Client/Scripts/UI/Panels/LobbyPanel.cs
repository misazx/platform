using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using RoguelikeGame.Network;
using RoguelikeGame.Network.Auth;
using RoguelikeGame.Network.Rooms;

namespace RoguelikeGame.UI.Panels
{
	public partial class LobbyPanel : Control
	{
		private Button _createRoomButton;
		private Button _refreshButton;
		private Button _logoutButton;
		private Button _backButton;
		private ItemList _roomList;
		private Label _welcomeLabel;
		private Label _playerInfoLabel;
		private LineEdit _roomNameInput;
		private OptionButton _gameModeOption;
		private SpinBox _maxPlayersSpin;
		private RichTextLabel _chatOutput;
		private LineEdit _chatInput;
		private Label _onlineCountLabel;
		private Control _friendsPanel;
		private ConnectionStatusIndicator _connectionIndicator;

		public event Action<RoomInfo> OnJoinRoom;
		public event Action OnCreateRoom;
		public event Action OnLogout;
		public event Action OnBackToMenu;
		public event Action<string, string> OnChatMessage;
		public event Action OnStartGame;
		public event Action OnLeave;
		public event Action OnRoomCreatedAndJoined;

		private List<RoomInfo> _displayedRooms = new List<RoomInfo>();

		public override void _Ready()
		{
			CreateUI();
			SetupEventHandlers();
			RefreshUserInfo();
			LoadRoomList();
		}

		private void CreateUI()
		{
			SetAnchorsPreset(Control.LayoutPreset.FullRect);

			var bg = new ColorRect
			{
				Color = new Color(0.02f, 0.02f, 0.05f, 0.95f),
				MouseFilter = MouseFilterEnum.Ignore,
				AnchorsPreset = (int)Control.LayoutPreset.FullRect
			};
			AddChild(bg);

			var mainContainer = new HSplitContainer();
			mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			AddChild(mainContainer);

			CreateLeftSidebar(mainContainer);
			CreateMainContent(mainContainer);
		}

		private void CreateLeftSidebar(HSplitContainer parent)
		{
			var sidebar = new VBoxContainer();
			sidebar.AddThemeConstantOverride("separation", 10);
			sidebar.CustomMinimumSize = new Vector2(250, 0);
			parent.AddChild(sidebar);

			var headerBox = new VBoxContainer();
			headerBox.AddThemeConstantOverride("separation", 5);
			sidebar.AddChild(headerBox);

			_welcomeLabel = new Label
			{
				Text = "欢迎, 玩家",
				HorizontalAlignment = HorizontalAlignment.Center,
				CustomMinimumSize = new Vector2(230, 30)
			};
			_welcomeLabel.AddThemeFontSizeOverride("font_size", 20);
			_welcomeLabel.Modulate = new Color(1f, 0.9f, 0.6f);
			headerBox.AddChild(_welcomeLabel);

			_playerInfoLabel = new Label
			{
				Text = "等级: 1 | 场次: 0",
				HorizontalAlignment = HorizontalAlignment.Center,
				CustomMinimumSize = new Vector2(230, 25)
			};
			_playerInfoLabel.AddThemeFontSizeOverride("font_size", 12);
			_playerInfoLabel.Modulate = new Color(0.75f, 0.8f, 0.85f);
			headerBox.AddChild(_playerInfoLabel);

			sidebar.AddChild(new HSeparator());

			_connectionIndicator = new ConnectionStatusIndicator();
			sidebar.AddChild(_connectionIndicator);

			_onlineCountLabel = new Label
			{
				Text = "在线玩家: --",
				CustomMinimumSize = new Vector2(230, 25)
			};
			_onlineCountLabel.AddThemeFontSizeOverride("font_size", 12);
			sidebar.AddChild(_onlineCountLabel);

			sidebar.AddChild(new HSeparator());

			_friendsPanel = new Control
			{
				Name = "FriendsPanel",
				CustomMinimumSize = new Vector2(200, 150)
			};
			sidebar.AddChild(_friendsPanel);

			sidebar.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });

			_logoutButton = new Button
			{
				Text = "🚪 登出",
				CustomMinimumSize = new Vector2(220, 40)
			};
			_logoutButton.Pressed += () => OnLogout?.Invoke();
			sidebar.AddChild(_logoutButton);

			_backButton = new Button
			{
				Text = "← 返回菜单",
				CustomMinimumSize = new Vector2(220, 35)
			};
			_backButton.Modulate = new Color(0.8f, 0.8f, 0.85f);
			_backButton.Pressed += () => OnBackToMenu?.Invoke();
			sidebar.AddChild(_backButton);
		}

		private void CreateMainContent(HSplitContainer parent)
		{
			var mainArea = new VBoxContainer();
			mainArea.AddThemeConstantOverride("separation", 10);
			parent.AddChild(mainArea);

			var topBar = new HBoxContainer();
			topBar.AddThemeConstantOverride("separation", 15);
			mainArea.AddChild(topBar);

			var titleLabel = new Label
			{
				Text = "🏠 游戏大厅",
				CustomMinimumSize = new Vector2(300, 45)
			};
			titleLabel.AddThemeFontSizeOverride("font_size", 28);
			titleLabel.Modulate = new Color(1f, 0.92f, 0.7f);
			topBar.AddChild(titleLabel);

			_refreshButton = new Button
			{
				Text = "🔄 刷新列表",
				CustomMinimumSize = new Vector2(120, 38)
			};
			_refreshButton.Pressed += async () => await LoadRoomList();
			topBar.AddChild(_refreshButton);

			mainArea.AddChild(new HSeparator());

			var createRoomSection = new VBoxContainer();
			createRoomSection.AddThemeConstantOverride("separation", 8);
			mainArea.AddChild(createRoomSection);

			var createHeader = new HBoxContainer();
			createHeader.AddThemeConstantOverride("separation", 10);
			createRoomSection.AddChild(createHeader);

			createHeader.AddChild(new Label { Text = "🎮 创建房间:", CustomMinimumSize = new Vector2(100, 30) });

			_roomNameInput = new LineEdit
			{
				PlaceholderText = "房间名称",
				Text = $"我的房间_{DateTime.Now:HHmm}",
				CustomMinimumSize = new Vector2(180, 32)
			};
			createHeader.AddChild(_roomNameInput);

			createHeader.AddChild(new Label { Text = "模式:", CustomMinimumSize = new Vector2(50, 30) });

			_gameModeOption = new OptionButton();
			_gameModeOption.AddItem("PvP 对战");
			_gameModeOption.AddItem("PvE 合作");
			_gameModeOption.AddItem("Coop 团队");
			_gameModeOption.CustomMinimumSize = new Vector2(100, 32);
			createHeader.AddChild(_gameModeOption);

			createHeader.AddChild(new Label { Text = "人数:", CustomMinimumSize = new Vector2(50, 30) });

			_maxPlayersSpin = new SpinBox
			{
				MinValue = 2,
				MaxValue = 4,
				Value = 4,
				CustomMinimumSize = new Vector2(60, 32)
			};
			createHeader.AddChild(_maxPlayersSpin);

			_createRoomButton = new Button
			{
				Text = "✨ 创建房间",
				CustomMinimumSize = new Vector2(130, 36)
			};
			_createRoomButton.Pressed += OnCreateRoomPressed;
			createHeader.AddChild(_createRoomButton);

			mainArea.AddChild(new HSeparator());

			var roomListLabel = new Label
			{
				Text = "📋 可用房间列表:",
				CustomMinimumSize = new Vector2(400, 28)
			};
			roomListLabel.AddThemeFontSizeOverride("font_size", 16);
			mainArea.AddChild(roomListLabel);

			_roomList = new ItemList
			{
				CustomMinimumSize = new Vector2(0, 280),
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};
			_roomList.Connect("item_selected", new Callable(this, nameof(OnRoomItemSelected)));
			mainArea.AddChild(_roomList);

			mainArea.AddChild(new HSeparator());

			var chatSection = new VBoxContainer();
			chatSection.AddThemeConstantOverride("separation", 5);
			mainArea.AddChild(chatSection);

			_chatOutput = new RichTextLabel
			{
				CustomMinimumSize = new Vector2(0, 120),
				FitContent = true
			};
			chatSection.AddChild(_chatOutput);

			var chatRow = new HBoxContainer();
			chatRow.AddThemeConstantOverride("separation", 8);
			chatSection.AddChild(chatRow);

			_chatInput = new LineEdit
			{
				PlaceholderText = "输入消息 (Enter发送)",
				CustomMinimumSize = new Vector2(500, 32)
			};
			_chatInput.Connect("text_submitted", new Callable(this, nameof(OnChatSubmitted)));
			chatRow.AddChild(_chatInput);

			var sendButton = new Button
			{
				Text = "发送",
				CustomMinimumSize = new Vector2(70, 32)
			};
			sendButton.Pressed += () => SendChatMessage();
			chatRow.AddChild(sendButton);
		}

		private void SetupEventHandlers()
		{
			if (RoomManager.Instance != null)
			{
				RoomManager.Instance.RoomCreated += OnRoomCreatedHandler;
				RoomManager.Instance.RoomJoined += OnRoomJoinedHandler;
			}
		}

		private void RefreshUserInfo()
		{
			if (AuthSystem.Instance?.CurrentUser != null)
			{
				var user = AuthSystem.Instance.CurrentUser;
				_welcomeLabel.Text = $"欢迎, {user.Username}";
				_playerInfoLabel.Text = $"等级: {user.Level} | 胜场: {user.GamesWon}";
			}
		}

		public async Task LoadRoomList()
		{
			try
			{
				_roomList.Clear();
				_displayedRooms.Clear();

				_roomList.AddItem("⏳ 正在加载房间列表...");

				var result = await RoomManager.Instance.GetRoomListAsync();

				_roomList.Clear();

				if (result.Success && result.Rooms != null && result.Rooms.Count > 0)
				{
					_displayedRooms = result.Rooms;

					foreach (var room in result.Rooms)
					{
						string statusIcon = room.Status switch
						{
							RoomStatus.Waiting => "✅",
							RoomStatus.Full => "🔒",
							RoomStatus.Ready => "⚡",
							_ => "❓"
						};

						string modeIcon = room.Mode switch
						{
							GameMode.PvP => "⚔️",
							GameMode.PvE => "🛡️",
							GameMode.Coop => "🤝",
							_ => "📌"
						};

						string itemText =
							$"{statusIcon} {modeIcon} {room.Name}\n" +
							$"   👥 {room.CurrentPlayers}/{room.MaxPlayers} | 🎭 {room.HostName}";

						_roomList.AddItem(itemText);
					}

					_onlineCountLabel.Text = $"在线房间: {result.TotalCount}";
				}
				else
				{
					_roomList.AddItem("😴 暂无可用房间，快来创建一个吧！");
					_onlineCountLabel.Text = "在线房间: 0";
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[LobbyPanel] 加载房间列表失败: {ex.Message}");
				_roomList.Clear();
				_displayedRooms.Clear();
				_roomList.AddItem($"❌ 加载失败: {ex.Message}");
			}
		}

		private void OnRoomItemSelected(long index)
		{
			GD.Print($"[LobbyPanel] 选择房间索引: {index}");

			if (index >= 0 && index < _displayedRooms.Count && RoomManager.Instance?.CurrentRoom == null)
			{
				var selectedRoom = _displayedRooms[(int)index];
				GD.Print($"[LobbyPanel] 选中房间: {selectedRoom.Name} ({selectedRoom.Id})");
				OnJoinRoom?.Invoke(selectedRoom);
			}
		}

		private async void OnCreateRoomPressed()
		{
			string name = _roomNameInput.Text.Trim();
			if (string.IsNullOrEmpty(name))
			{
				name = $"房间_{DateTime.Now:HHmmss}";
			}

			int modeIndex = _gameModeOption.Selected;
			var mode = modeIndex switch
			{
				0 => GameMode.PvP,
				1 => GameMode.PvE,
				_ => GameMode.Coop
			};

			int maxPlayers = (int)_maxPlayersSpin.Value;

			GD.Print($"[LobbyPanel] 创建房间: {name}, 模式: {mode}, 最大人数: {maxPlayers}");

			_createRoomButton.Disabled = true;

			var result = await RoomManager.Instance.CreateRoomAsync(name, mode, maxPlayers);

			if (result.Success)
			{
				AddSystemMessage($"✅ 房间 '{name}' 创建成功！");
				OnRoomCreatedAndJoined?.Invoke();
			}
			else
			{
				AddSystemMessage($"❌ 创建失败: {result.Message}");
				_createRoomButton.Disabled = false;
			}
		}

		private void OnChatSubmitted(string text)
		{
			SendChatMessage();
		}

		private void SendChatMessage()
		{
			string message = _chatInput.Text.Trim();
			if (!string.IsNullOrEmpty(message))
			{
				OnChatMessage?.Invoke(AuthSystem.Instance?.CurrentUser?.Username ?? "Unknown", message);
				AddChatMessage(AuthSystem.Instance?.CurrentUser?.Username ?? "你", message, true);
				_chatInput.Text = "";
			}
		}

		public void AddChatMessage(string sender, string message, bool isLocal = false)
		{
			Color color = isLocal ? new Color(0.4f, 0.8f, 1f) : new Color(1f, 0.9f, 0.6f);
			string formattedMsg = $"[color=#{color.ToHtml(false)}]{sender}[/color]: {message}\n";

			_chatOutput.AppendText(formattedMsg);
			_chatOutput.ScrollToLine(_chatOutput.GetLineCount() - 1);
		}

		public void AddSystemMessage(string message)
		{
			_chatOutput.AppendText($"[color=gray]系统: {message}[/color]\n");
			_chatOutput.ScrollToLine(_chatOutput.GetLineCount() - 1);
		}

		private void OnRoomCreatedHandler(string roomId, string roomName)
		{
			AddSystemMessage($"🏠 房间已创建: {roomName}");
			LoadRoomList();
		}

		private void OnRoomJoinedHandler(string roomId, string roomName)
		{
			AddSystemMessage($"✅ 已加入房间: {roomName}");
		}

		public void Update()
		{
			RefreshUserInfo();
			UpdateConnectionStatusDisplay();
		}

		private void UpdateConnectionStatusDisplay()
		{
			if (_connectionIndicator is ConnectionStatusIndicator indicator)
			{
				indicator.UpdateStatus(NetworkManager.Instance?.State ?? NetworkState.Disconnected);
			}
		}
	}
}
