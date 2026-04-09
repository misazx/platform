using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;
using RoguelikeGame.Database;

namespace RoguelikeGame.UI.Panels
{
	public partial class RewardPanel : Control
	{
		public event System.Action Closed;

		private int _goldReward;
		private List<CardData> _cardChoices;
		private bool _goldClaimed = false;
		private bool _cardClaimed = false;

		public RewardPanel(int goldReward, List<CardData> cardChoices)
		{
			_goldReward = goldReward;
			_cardChoices = cardChoices;
		}

		public override void _Ready()
		{
			AnchorsPreset = (int)Control.LayoutPreset.FullRect;
			MouseFilter = MouseFilterEnum.Stop;

			var bg = new ColorRect
			{
				AnchorsPreset = (int)Control.LayoutPreset.FullRect,
				Color = new Color(0, 0, 0, 0.85f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			AddChild(bg);

			var mainPanel = new PanelContainer
			{
				CustomMinimumSize = new Vector2(620, 460),
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
				SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
				MouseFilter = MouseFilterEnum.Ignore
			};
			mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
			var panelStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.1f, 0.08f, 0.12f, 0.98f),
				CornerRadiusTopLeft = 15,
				CornerRadiusTopRight = 15,
				CornerRadiusBottomLeft = 15,
				CornerRadiusBottomRight = 15,
				BorderWidthLeft = 3,
				BorderWidthRight = 3,
				BorderWidthTop = 3,
				BorderWidthBottom = 3,
				BorderColor = new Color(0.8f, 0.65f, 0.2f, 0.9f),
				ContentMarginTop = 20,
				ContentMarginBottom = 20,
				ContentMarginLeft = 25,
				ContentMarginRight = 25
			};
			mainPanel.AddThemeStyleboxOverride("panel", panelStyle);
			AddChild(mainPanel);

			var vbox = new VBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore,
				Alignment = BoxContainer.AlignmentMode.Center
			};
			vbox.AddThemeConstantOverride("separation", 10);
			mainPanel.AddChild(vbox);

			var titleLabel = new Label
			{
				Text = "🏆 战斗奖励",
				HorizontalAlignment = HorizontalAlignment.Center,
				Modulate = new Color(1f, 0.85f, 0.3f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			titleLabel.AddThemeFontSizeOverride("font_size", 22);
			vbox.AddChild(titleLabel);

			var goldBtn = new Button
			{
				Text = $"💰 获得 {_goldReward} 金币",
				CustomMinimumSize = new Vector2(500, 42),
				MouseFilter = MouseFilterEnum.Stop,
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
			};
			goldBtn.Pressed += () =>
			{
				if (_goldClaimed) return;
				_goldClaimed = true;
				goldBtn.Text = $"✅ 已获得 {_goldReward} 金币";
				goldBtn.Disabled = true;
				var run = GameManager.Instance?.CurrentRun;
				if (run != null) run.Gold += _goldReward;
				GD.Print($"[RewardPanel] Gold: +{_goldReward}");
			};
			vbox.AddChild(goldBtn);

			var cardLabel = new Label
			{
				Text = "选择一张卡牌加入牌组:",
				Modulate = new Color(0.8f, 0.8f, 0.8f),
				MouseFilter = MouseFilterEnum.Ignore,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			cardLabel.AddThemeFontSizeOverride("font_size", 14);
			vbox.AddChild(cardLabel);

			if (_cardChoices != null)
			{
				foreach (var card in _cardChoices)
				{
					var cardBtn = new Button
					{
						Text = $"🃏 {card.Name} ({card.Type}) - {card.Description}",
						CustomMinimumSize = new Vector2(540, 40),
						MouseFilter = MouseFilterEnum.Stop,
						SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
					};
					var capturedCard = card;
					cardBtn.Pressed += () =>
					{
						if (_cardClaimed) return;
						_cardClaimed = true;
						GD.Print($"[RewardPanel] Card chosen: {capturedCard.Name}");
						cardBtn.Text = $"✅ 已选择 {capturedCard.Name}";
						cardBtn.Disabled = true;
						if (!_goldClaimed)
						{
							_goldClaimed = true;
							var run2 = GameManager.Instance?.CurrentRun;
							if (run2 != null) run2.Gold += _goldReward;
						}
						GetTree().CreateTimer(1.0f).Timeout += () => Closed?.Invoke();
					};
					vbox.AddChild(cardBtn);
				}
			}

			var skipBtn = new Button
			{
				Text = "跳过卡牌奖励",
				CustomMinimumSize = new Vector2(200, 38),
				MouseFilter = MouseFilterEnum.Stop,
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
			};
			skipBtn.Pressed += () =>
			{
				GD.Print("[RewardPanel] Skipped card reward");
				if (!_goldClaimed)
				{
					_goldClaimed = true;
					var run3 = GameManager.Instance?.CurrentRun;
					if (run3 != null) run3.Gold += _goldReward;
				}
				Closed?.Invoke();
			};
			vbox.AddChild(skipBtn);

			var spacer = new Control { CustomMinimumSize = new Vector2(0, 15), MouseFilter = MouseFilterEnum.Ignore };
			vbox.AddChild(spacer);

			var continueBtn = new Button
			{
				Text = "继续",
				CustomMinimumSize = new Vector2(200, 42),
				MouseFilter = MouseFilterEnum.Stop,
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
			};
			continueBtn.Pressed += () => Closed?.Invoke();
			vbox.AddChild(continueBtn);
		}
	}
}