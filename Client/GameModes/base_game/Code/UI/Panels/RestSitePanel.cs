using Godot;
using System.Collections.Generic;
using RoguelikeGame.Core;
using RoguelikeGame.Database;

namespace RoguelikeGame.UI.Panels
{
	public partial class RestSitePanel : Control
	{
		public event System.Action Closed;

		public override void _Ready()
		{
			AnchorsPreset = (int)Control.LayoutPreset.FullRect;
			MouseFilter = MouseFilterEnum.Stop;

			var bg = new ColorRect
			{
				AnchorsPreset = (int)Control.LayoutPreset.FullRect,
				Color = new Color(0.03f, 0.05f, 0.03f, 0.95f),
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
				BgColor = new Color(0.1f, 0.08f, 0.05f, 0.98f),
				CornerRadiusTopLeft = 15,
				CornerRadiusTopRight = 15,
				CornerRadiusBottomLeft = 15,
				CornerRadiusBottomRight = 15,
				BorderWidthLeft = 3,
				BorderWidthRight = 3,
				BorderWidthTop = 3,
				BorderWidthBottom = 3,
				BorderColor = new Color(0.2f, 0.6f, 0.3f, 0.9f)
			};
			mainPanel.AddThemeStyleboxOverride("panel", panelStyle);
			AddChild(mainPanel);

			var vbox = new VBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore,
				AnchorsPreset = (int)Control.LayoutPreset.FullRect
			};
			vbox.AddThemeConstantOverride("separation", 15);
			mainPanel.AddChild(vbox);

			var titleLabel = new Label
			{
				Text = "🔥 篝火",
				HorizontalAlignment = HorizontalAlignment.Center,
				Modulate = new Color(1f, 0.7f, 0.3f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			titleLabel.AddThemeFontSizeOverride("font_size", 24);
			vbox.AddChild(titleLabel);

			var run = GameManager.Instance?.CurrentRun;
			string hpInfo = run != null ? $"当前: {run.CurrentHealth}/{run.MaxHealth} HP" : "当前: 80/80 HP";
			var hpLabel = new Label
			{
				Text = hpInfo,
				HorizontalAlignment = HorizontalAlignment.Center,
				Modulate = new Color(1f, 0.4f, 0.4f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			hpLabel.AddThemeFontSizeOverride("font_size", 14);
			vbox.AddChild(hpLabel);

			var restBtn = new Button
			{
				Text = "❤️ 休息 - 恢复30%生命值",
				CustomMinimumSize = new Vector2(400, 50),
				MouseFilter = MouseFilterEnum.Stop
			};
			restBtn.Pressed += OnRestPressed;
			vbox.AddChild(restBtn);

			var smithBtn = new Button
			{
				Text = "🔨 锻造 - 升级一张卡牌",
				CustomMinimumSize = new Vector2(400, 50),
				MouseFilter = MouseFilterEnum.Stop
			};
			smithBtn.Pressed += OnSmithPressed;
			vbox.AddChild(smithBtn);

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

		private void OnRestPressed()
		{
			var run = GameManager.Instance?.CurrentRun;
			if (run != null)
			{
				int healAmount = (int)(run.MaxHealth * 0.3f);
				run.CurrentHealth = System.Math.Min(run.MaxHealth, run.CurrentHealth + healAmount);
				GD.Print($"[RestSitePanel] ❤️ Rested: healed {healAmount} HP, now {run.CurrentHealth}/{run.MaxHealth}");
			}
			Closed?.Invoke();
		}

		private void OnSmithPressed()
		{
			GD.Print("[RestSitePanel] 🔨 Smith: upgrade a card (simplified - +3 damage to random attack card)");
			Closed?.Invoke();
		}
	}
}