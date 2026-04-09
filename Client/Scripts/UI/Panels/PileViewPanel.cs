using Godot;
using System.Collections.Generic;
using RoguelikeGame.Database;

namespace RoguelikeGame.UI.Panels
{
	public partial class PileViewPanel : Control
	{
		public event System.Action Closed;

		private string _title;
		private List<CardData> _cards;

		public PileViewPanel(string title, List<CardData> cards)
		{
			_title = title;
			_cards = cards;
		}

		public override void _Ready()
		{
			AnchorsPreset = (int)Control.LayoutPreset.FullRect;
			MouseFilter = MouseFilterEnum.Stop;

			var bg = new ColorRect
			{
				AnchorsPreset = (int)Control.LayoutPreset.FullRect,
				Color = new Color(0, 0, 0, 0.8f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			AddChild(bg);

			var mainPanel = new PanelContainer
			{
				CustomMinimumSize = new Vector2(900, 550),
				MouseFilter = MouseFilterEnum.Ignore
			};
			mainPanel.Position = new Vector2(190, 85);
			var panelStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.1f, 0.08f, 0.12f, 0.98f),
				CornerRadiusTopLeft = 15,
				CornerRadiusTopRight = 15,
				CornerRadiusBottomLeft = 15,
				CornerRadiusBottomRight = 15,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = new Color(0.5f, 0.4f, 0.3f, 0.9f)
			};
			mainPanel.AddThemeStyleboxOverride("panel", panelStyle);
			AddChild(mainPanel);

			var vbox = new VBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore,
				AnchorsPreset = (int)Control.LayoutPreset.FullRect
			};
			vbox.AddThemeConstantOverride("separation", 8);
			mainPanel.AddChild(vbox);

			var titleLabel = new Label
			{
				Text = $"{_title} ({_cards?.Count ?? 0}张)",
				HorizontalAlignment = HorizontalAlignment.Center,
				Modulate = new Color(0.9f, 0.8f, 0.6f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			titleLabel.AddThemeFontSizeOverride("font_size", 18);
			vbox.AddChild(titleLabel);

			var scrollContainer = new ScrollContainer
			{
				CustomMinimumSize = new Vector2(860, 420),
				MouseFilter = MouseFilterEnum.Stop,
				HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled
			};
			vbox.AddChild(scrollContainer);

			var gridContainer = new GridContainer
			{
				Columns = 5,
				MouseFilter = MouseFilterEnum.Ignore
			};
			gridContainer.AddThemeConstantOverride("h_separation", 8);
			gridContainer.AddThemeConstantOverride("v_separation", 8);
			scrollContainer.AddChild(gridContainer);

			if (_cards != null)
			{
				foreach (var card in _cards)
				{
					var cardPanel = new PanelContainer
					{
						CustomMinimumSize = new Vector2(155, 90),
						MouseFilter = MouseFilterEnum.Ignore
					};
					var cardStyle = new StyleBoxFlat
					{
						BgColor = new Color(0.12f, 0.1f, 0.08f, 0.98f),
						CornerRadiusTopLeft = 8,
						CornerRadiusTopRight = 8,
						CornerRadiusBottomLeft = 8,
						CornerRadiusBottomRight = 8,
						BorderWidthLeft = 2,
						BorderWidthRight = 2,
						BorderWidthTop = 2,
						BorderWidthBottom = 2,
						BorderColor = card.Type switch
						{
							CardType.Attack => new Color(0.75f, 0.25f, 0.25f, 0.9f),
							CardType.Skill => new Color(0.25f, 0.45f, 0.8f, 0.9f),
							CardType.Power => new Color(0.8f, 0.65f, 0.2f, 0.9f),
							_ => new Color(0.5f, 0.4f, 0.3f, 0.9f)
						}
					};
					cardPanel.AddThemeStyleboxOverride("panel", cardStyle);
					gridContainer.AddChild(cardPanel);

					var cardVBox = new VBoxContainer
					{
						MouseFilter = MouseFilterEnum.Ignore,
						AnchorsPreset = (int)Control.LayoutPreset.FullRect
					};
					cardVBox.AddThemeConstantOverride("separation", 2);
					cardPanel.AddChild(cardVBox);

					var nameRow = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
					cardVBox.AddChild(nameRow);

					var nameLabel = new Label
					{
						Text = card.Name,
						Modulate = Colors.White,
						MouseFilter = MouseFilterEnum.Ignore
					};
					nameLabel.AddThemeFontSizeOverride("font_size", 11);
					nameRow.AddChild(nameLabel);

					var costLabel = new Label
					{
						Text = card.Cost.ToString(),
						Modulate = new Color(1f, 0.85f, 0.2f),
						MouseFilter = MouseFilterEnum.Ignore
					};
					costLabel.AddThemeFontSizeOverride("font_size", 11);
					nameRow.AddChild(costLabel);

					var descLabel = new Label
					{
						Text = card.Description,
						Modulate = new Color(0.7f, 0.7f, 0.7f),
						MouseFilter = MouseFilterEnum.Ignore,
						AutowrapMode = TextServer.AutowrapMode.WordSmart,
						CustomMinimumSize = new Vector2(140, 30)
					};
					descLabel.AddThemeFontSizeOverride("font_size", 9);
					cardVBox.AddChild(descLabel);
				}
			}

			if (_cards == null || _cards.Count == 0)
			{
				var emptyLabel = new Label
				{
					Text = "空",
					HorizontalAlignment = HorizontalAlignment.Center,
					Modulate = Colors.Gray,
					MouseFilter = MouseFilterEnum.Ignore
				};
				emptyLabel.AddThemeFontSizeOverride("font_size", 16);
				vbox.AddChild(emptyLabel);
			}

			var closeBtn = new Button
			{
				Text = "关闭",
				CustomMinimumSize = new Vector2(150, 36),
				MouseFilter = MouseFilterEnum.Stop
			};
			closeBtn.Pressed += () => Closed?.Invoke();
			vbox.AddChild(closeBtn);
		}
	}
}