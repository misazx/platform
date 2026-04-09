using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;

namespace RoguelikeGame.UI.Panels
{
	public partial class TreasurePanel : Control
	{
		public event System.Action Closed;

		public override void _Ready()
		{
			AnchorsPreset = (int)Control.LayoutPreset.FullRect;
			MouseFilter = MouseFilterEnum.Stop;

			var bg = new ColorRect
			{
				AnchorsPreset = (int)Control.LayoutPreset.FullRect,
				Color = new Color(0.04f, 0.03f, 0.02f, 0.95f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			AddChild(bg);

			var mainPanel = new PanelContainer
			{
				CustomMinimumSize = new Vector2(600, 400),
				MouseFilter = MouseFilterEnum.Ignore
			};
			mainPanel.Position = new Vector2(340, 160);
			var panelStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.1f, 0.08f, 0.04f, 0.98f),
				CornerRadiusTopLeft = 15,
				CornerRadiusTopRight = 15,
				CornerRadiusBottomLeft = 15,
				CornerRadiusBottomRight = 15,
				BorderWidthLeft = 3,
				BorderWidthRight = 3,
				BorderWidthTop = 3,
				BorderWidthBottom = 3,
				BorderColor = new Color(0.8f, 0.6f, 0.15f, 0.9f)
			};
			mainPanel.AddThemeStyleboxOverride("panel", panelStyle);
			AddChild(mainPanel);

			var vbox = new VBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore,
				AnchorsPreset = (int)Control.LayoutPreset.FullRect
			};
			vbox.AddThemeConstantOverride("separation", 12);
			mainPanel.AddChild(vbox);

			var titleLabel = new Label
			{
				Text = "💎 宝箱!",
				HorizontalAlignment = HorizontalAlignment.Center,
				Modulate = new Color(1f, 0.85f, 0.3f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			titleLabel.AddThemeFontSizeOverride("font_size", 24);
			vbox.AddChild(titleLabel);

			var rng = new Random();
			int goldAmount = rng.Next(20, 50);

			var rewards = new List<string>
			{
				$"💰 {goldAmount} 金币",
				"🏺 随机遗物",
				"🧪 随机药水"
			};

			foreach (var reward in rewards)
			{
				var btn = new Button
				{
					Text = $"获取 {reward}",
					CustomMinimumSize = new Vector2(400, 45),
					MouseFilter = MouseFilterEnum.Stop
				};
				btn.Pressed += () =>
				{
					GD.Print($"[TreasurePanel] Obtained: {reward}");
					var run = GameManager.Instance?.CurrentRun;
					if (run != null && reward.Contains("金币"))
						run.Gold += goldAmount;
				};
				vbox.AddChild(btn);
			}

			var spacer = new Control { SizeFlagsVertical = Control.SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
			vbox.AddChild(spacer);

			var closeBtn = new Button
			{
				Text = "继续前进",
				CustomMinimumSize = new Vector2(200, 40),
				MouseFilter = MouseFilterEnum.Stop
			};
			closeBtn.Pressed += () => Closed?.Invoke();
			vbox.AddChild(closeBtn);
		}
	}
}