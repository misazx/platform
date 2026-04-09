using Godot;
using RoguelikeGame.Core;

namespace RoguelikeGame.UI.Panels
{
	public partial class VictoryScreen : Control
	{
		public event System.Action Closed;

		public VictoryScreen() { }

		public VictoryScreen(RunData runData)
		{
			_runData = runData;
		}

		private RunData _runData;

		public override void _Ready()
		{
			AnchorsPreset = (int)Control.LayoutPreset.FullRect;
			MouseFilter = MouseFilterEnum.Stop;

			var bg = new ColorRect
			{
				AnchorsPreset = (int)Control.LayoutPreset.FullRect,
				Color = new Color(0, 0, 0, 0.9f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			AddChild(bg);

			var mainPanel = new PanelContainer
			{
				CustomMinimumSize = new Vector2(500, 400),
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
				SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
				MouseFilter = MouseFilterEnum.Ignore
			};
			mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
			var panelStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.08f, 0.12f, 0.08f, 0.98f),
				CornerRadiusTopLeft = 15,
				CornerRadiusTopRight = 15,
				CornerRadiusBottomLeft = 15,
				CornerRadiusBottomRight = 15,
				BorderWidthLeft = 3,
				BorderWidthRight = 3,
				BorderWidthTop = 3,
				BorderWidthBottom = 3,
				BorderColor = new Color(0.2f, 0.8f, 0.3f, 0.9f),
				ContentMarginTop = 25,
				ContentMarginBottom = 25,
				ContentMarginLeft = 30,
				ContentMarginRight = 30
			};
			mainPanel.AddThemeStyleboxOverride("panel", panelStyle);
			AddChild(mainPanel);

			var vbox = new VBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore,
				Alignment = BoxContainer.AlignmentMode.Center
			};
			vbox.AddThemeConstantOverride("separation", 12);
			mainPanel.AddChild(vbox);

			var titleLabel = new Label
			{
				Text = "🏆 通关成功!",
				HorizontalAlignment = HorizontalAlignment.Center,
				Modulate = new Color(1f, 0.9f, 0.3f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			titleLabel.AddThemeFontSizeOverride("font_size", 28);
			vbox.AddChild(titleLabel);

			var statsLabel = new Label
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				MouseFilter = MouseFilterEnum.Ignore
			};
			statsLabel.AddThemeFontSizeOverride("font_size", 16);

			if (_runData != null)
			{
				var playTime = _runData.EndTime - _runData.StartTime;
				statsLabel.Text = $"成功登顶!\n击败 {_runData.TotalEnemiesDefeated} 个敌人\n收集 {_runData.Gold} 金币\n用时: {playTime:mm\\:ss}";
			}
			else
			{
				statsLabel.Text = "成功登顶!";
			}
			vbox.AddChild(statsLabel);

			var spacer = new Control { CustomMinimumSize = new Vector2(0, 20), MouseFilter = MouseFilterEnum.Ignore };
			vbox.AddChild(spacer);

			var menuBtn = new Button
			{
				Text = "返回主菜单",
				CustomMinimumSize = new Vector2(200, 42),
				MouseFilter = MouseFilterEnum.Stop,
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
			};
			menuBtn.Pressed += () =>
			{
				Closed?.Invoke();
				Main.Instance?.GoToMainMenu();
			};
			vbox.AddChild(menuBtn);
		}
	}
}
