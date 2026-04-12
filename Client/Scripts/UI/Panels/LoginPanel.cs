using System;
using System.Threading.Tasks;
using Godot;
using RoguelikeGame.Network.Auth;

namespace RoguelikeGame.UI.Panels
{
	public partial class LoginPanel : Control
	{
		private LineEdit _usernameInput;
		private LineEdit _passwordInput;
		private LineEdit _emailInput;
		private Button _loginButton;
		private Button _registerButton;
		private Button _backButton;
		private Label _titleLabel;
		private Label _statusLabel;
		private CheckBox _rememberMeCheck;
		private OptionButton _modeOption;

		public event Action OnLoginSuccess;
		public event Action OnRegisterSuccess;
		public event Action OnBack;

		public override void _Ready()
		{
			CreateUI();
			LoadSavedCredentials();
		}

		private void CreateUI()
		{
			SetAnchorsPreset(Control.LayoutPreset.FullRect);

			var bg = new ColorRect
			{
				Color = new Color(0, 0, 0, 0.93f),
				MouseFilter = MouseFilterEnum.Ignore,
				AnchorsPreset = (int)Control.LayoutPreset.FullRect
			};
			AddChild(bg);

			var mainPanel = new PanelContainer
			{
				CustomMinimumSize = new Vector2(450, 520),
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
				BorderColor = new Color(0.4f, 0.6f, 0.9f, 1f)
			};
			mainPanel.AddThemeStyleboxOverride("panel", panelStyle);
			AddChild(mainPanel);

			var vbox = new VBoxContainer();
			vbox.AddThemeConstantOverride("separation", 15);
			mainPanel.AddChild(vbox);

			_titleLabel = new Label
			{
				Text = "🔐 账号登录",
				HorizontalAlignment = HorizontalAlignment.Center,
				CustomMinimumSize = new Vector2(400, 50)
			};
			_titleLabel.AddThemeFontSizeOverride("font_size", 30);
			_titleLabel.Modulate = new Color(1f, 0.92f, 0.6f);
			vbox.AddChild(_titleLabel);

			vbox.AddChild(new HSeparator());

			var modeRow = new HBoxContainer();
			modeRow.AddThemeConstantOverride("separation", 10);
			vbox.AddChild(modeRow);
			modeRow.AddChild(new Label { Text = "模式:", CustomMinimumSize = new Vector2(60, 35) });
			_modeOption = new OptionButton();
			_modeOption.AddItem("已有账号");
			_modeOption.AddItem("注册新账号");
			_modeOption.CustomMinimumSize = new Vector2(200, 35);
			_modeOption.ItemSelected += OnModeChanged;
			modeRow.AddChild(_modeOption);

			_usernameInput = CreateInputField("用户名 / 邮箱", "请输入用户名");
			vbox.AddChild(_usernameInput);

			_passwordInput = CreateInputField("密码", "请输入密码", true);
			vbox.AddChild(_passwordInput);

			_emailInput = CreateInputField("邮箱 (可选)", "example@email.com");
			_emailInput.Visible = false;
			vbox.AddChild(_emailInput);

			var optionsRow = new HBoxContainer();
			optionsRow.AddThemeConstantOverride("separation", 10);
			vbox.AddChild(optionsRow);

			_rememberMeCheck = new CheckBox
			{
				Text = "记住登录状态",
				ButtonPressed = true,
				CustomMinimumSize = new Vector2(180, 30)
			};
			optionsRow.AddChild(_rememberMeCheck);

			vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });

			_loginButton = new Button
			{
				Text = "✨ 登录",
				CustomMinimumSize = new Vector2(380, 50)
			};
			_loginButton.AddThemeFontSizeOverride("font_size", 20);
			_loginButton.Pressed += OnLoginPressed;
			vbox.AddChild(_loginButton);

			_registerButton = new Button
			{
				Text = "📝 注册新账号",
				CustomMinimumSize = new Vector2(380, 45)
			};
			_registerButton.AddThemeFontSizeOverride("font_size", 16);
			_registerButton.Pressed += OnRegisterPressed;
			_registerButton.Visible = false;
			vbox.AddChild(_registerButton);

			_statusLabel = new Label
			{
				Text = "",
				HorizontalAlignment = HorizontalAlignment.Center,
				CustomMinimumSize = new Vector2(400, 30),
				AutowrapMode = TextServer.AutowrapMode.WordSmart
			};
			_statusLabel.AddThemeFontSizeOverride("font_size", 13);
			vbox.AddChild(_statusLabel);

			vbox.AddChild(new HSeparator());

			_backButton = new Button
			{
				Text = "← 返回",
				CustomMinimumSize = new Vector2(150, 38)
			};
			_backButton.Pressed += () => OnBack?.Invoke();
			var backContainer = new CenterContainer();
			backContainer.AddChild(_backButton);
			vbox.AddChild(backContainer);
		}

		private LineEdit CreateInputField(string placeholder, string emptyMessage, bool secret = false)
		{
			var input = new LineEdit
			{
				PlaceholderText = placeholder,
				Secret = secret,
				CustomMinimumSize = new Vector2(380, 40)
			};
			input.AddThemeFontSizeOverride("font_size", 16);

			var style = new StyleBoxFlat
			{
				BgColor = new Color(0.12f, 0.12f, 0.18f, 0.8f),
				CornerRadiusTopLeft = 8,
				CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8,
				CornerRadiusBottomRight = 8,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = new Color(0.4f, 0.45f, 0.55f, 0.6f)
			};
			input.AddThemeStyleboxOverride("normal", style);

			return input;
		}

		private void OnModeChanged(long index)
		{
			bool isRegisterMode = index == 1;

			_emailInput.Visible = isRegisterMode;
			_loginButton.Visible = !isRegisterMode;
			_registerButton.Visible = isRegisterMode;

			_titleLabel.Text = isRegisterMode ? "📝 注册账号" : "🔐 账号登录";
		}

		private async void OnLoginPressed()
		{
			string username = _usernameInput.Text.Trim();
			string password = _passwordInput.Text;

			if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
			{
				SetStatus("❌ 请输入用户名和密码", Colors.Red);
				return;
			}

			SetStatus("⏳ 正在登录...", Colors.Yellow);
			_loginButton.Disabled = true;

			try
			{
				var result = await AuthSystem.Instance.LoginAsync(username, password);

				if (result.Success)
				{
					SetStatus("✅ 登录成功！", Colors.Green);

					if (_rememberMeCheck.ButtonPressed)
					{
						SaveCredentials(username);
					}

					await Task.Delay(500);
					OnLoginSuccess?.Invoke();
				}
				else
				{
					SetStatus($"❌ 登录失败: {result.Message}", Colors.Red);
				}
			}
			catch (Exception ex)
			{
				SetStatus($"❌ 错误: {ex.Message}", Colors.Red);
			}
			finally
			{
				_loginButton.Disabled = false;
			}
		}

		private async void OnRegisterPressed()
		{
			string username = _usernameInput.Text.Trim();
			string password = _passwordInput.Text;
			string email = _emailInput.Text.Trim();

			if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
			{
				SetStatus("❌ 请填写必填项", Colors.Red);
				return;
			}

			if (password.Length < 6)
			{
				SetStatus("❌ 密码至少需要6个字符", Colors.Red);
				return;
			}

			SetStatus("⏳ 正在注册...", Colors.Yellow);
			_registerButton.Disabled = true;

			try
			{
				var result = await AuthSystem.Instance.RegisterAsync(username, password, email);

				if (result.Success)
				{
					SetStatus("✅ 注册成功！正在自动登录...", Colors.Green);

					await Task.Delay(1000);

					var loginResult = await AuthSystem.Instance.LoginAsync(username, password);
					if (loginResult.Success)
					{
						OnLoginSuccess?.Invoke();
					}
					else
					{
						SetStatus("注册成功但自动登录失败，请手动登录", Colors.Yellow);
						_modeOption.Selected = 0;
						OnModeChanged(0);
					}
				}
				else
				{
					SetStatus($"❌ 注册失败: {result.Message}", Colors.Red);
				}
			}
			catch (Exception ex)
			{
				SetStatus($"❌ 错误: {ex.Message}", Colors.Red);
			}
			finally
			{
				_registerButton.Disabled = false;
			}
		}

		private void SetStatus(string text, Color color)
		{
			_statusLabel.Text = text;
			_statusLabel.Modulate = color;
		}

		private void SaveCredentials(string username)
		{
			try
			{
				var configDir = OS.GetUserDataDir();
				using var file = FileAccess.Open($"{configDir}/last_login.json", FileAccess.ModeFlags.Write);
				file.StoreString(System.Text.Json.JsonSerializer.Serialize(new { username }));
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[LoginPanel] 保存凭据失败: {ex.Message}");
			}
		}

		private void LoadSavedCredentials()
		{
			try
			{
				var configDir = OS.GetUserDataDir();
				var path = $"{configDir}/last_login.json";

				if (!FileAccess.FileExists(path)) return;

				using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
				var json = file.GetAsText();
				var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);

				if (data.TryGetProperty("username", out var userProp))
				{
					_usernameInput.Text = userProp.GetString() ?? "";
				}
			}
			catch (Exception ex)
			{
				GD.Print($"[LoginPanel] 加载凭据失败: {ex.Message}");
			}
		}
	}
}
