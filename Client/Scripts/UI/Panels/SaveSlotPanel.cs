using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;
using RoguelikeGame.Systems;

namespace RoguelikeGame.UI.Panels
{
	public partial class SaveSlotPanel : Control
	{
		public event System.Action<int> SaveSelected;
		public event System.Action<int> SaveDeleted;
		public event System.Action Closed;

		public SaveSlotPanel()
		{
		}

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
				CustomMinimumSize = new Vector2(650, 500),
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
				SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
				MouseFilter = MouseFilterEnum.Ignore
			};
			mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
			var panelStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.1f, 0.08f, 0.1f, 0.98f),
				CornerRadiusTopLeft = 15,
				CornerRadiusTopRight = 15,
				CornerRadiusBottomLeft = 15,
				CornerRadiusBottomRight = 15,
				BorderWidthLeft = 3,
				BorderWidthRight = 3,
				BorderWidthTop = 3,
				BorderWidthBottom = 3,
				BorderColor = new Color(0.4f, 0.5f, 0.6f, 0.9f),
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
				Text = "选择存档",
				HorizontalAlignment = HorizontalAlignment.Center,
				Modulate = new Color(0.8f, 0.9f, 1f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			titleLabel.AddThemeFontSizeOverride("font_size", 24);
			vbox.AddChild(titleLabel);

			for (int i = 1; i <= 3; i++)
			{
				var slotBtn = CreateSaveSlotButton(i);
				vbox.AddChild(slotBtn);
			}

			var spacer = new Control { CustomMinimumSize = new Vector2(0, 15), MouseFilter = MouseFilterEnum.Ignore };
			vbox.AddChild(spacer);

			var closeBtn = new Button
			{
				Text = "返回",
				CustomMinimumSize = new Vector2(180, 42),
				MouseFilter = MouseFilterEnum.Stop,
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
			};
			closeBtn.Pressed += () => Closed?.Invoke();
			vbox.AddChild(closeBtn);
		}

		private Control CreateSaveSlotButton(int slotId)
		{
			var hasSave = EnhancedSaveSystem.Instance?.HasSave(slotId) ?? false;
			var slotInfo = hasSave ? EnhancedSaveSystem.Instance?.LoadGame(slotId) : null;

			var panel = new PanelContainer
			{
				CustomMinimumSize = new Vector2(580, 90),
				MouseFilter = MouseFilterEnum.Stop,
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
			};

			var style = new StyleBoxFlat
			{
				BgColor = hasSave ? new Color(0.15f, 0.18f, 0.2f, 0.95f) : new Color(0.08f, 0.08f, 0.08f, 0.7f),
				CornerRadiusTopLeft = 10,
				CornerRadiusTopRight = 10,
				CornerRadiusBottomLeft = 10,
				CornerRadiusBottomRight = 10,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = hasSave ? new Color(0.3f, 0.5f, 0.7f) : new Color(0.2f, 0.2f, 0.2f)
			};
			panel.AddThemeStyleboxOverride("panel", style);

			var hbox = new HBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore,
				CustomMinimumSize = new Vector2(560, 80)
			};
			hbox.AddThemeConstantOverride("separation", 15);
			panel.AddChild(hbox);

			var slotNumLabel = new Label
			{
				Text = $"存档 {slotId}",
				Modulate = hasSave ? new Color(0.9f, 0.95f, 1f) : new Color(0.4f, 0.4f, 0.4f),
				CustomMinimumSize = new Vector2(80, 0),
				VerticalAlignment = VerticalAlignment.Center,
				MouseFilter = MouseFilterEnum.Ignore
			};
			slotNumLabel.AddThemeFontSizeOverride("font_size", 18);
			hbox.AddChild(slotNumLabel);

			if (hasSave && slotInfo != null)
			{
				var charLabel = new Label
				{
					Text = $"{slotInfo.CharacterId}",
					Modulate = new Color(0.6f, 0.9f, 0.7f),
					CustomMinimumSize = new Vector2(80, 0),
					VerticalAlignment = VerticalAlignment.Center,
					MouseFilter = MouseFilterEnum.Ignore
				};
				charLabel.AddThemeFontSizeOverride("font_size", 14);
				hbox.AddChild(charLabel);

				var infoLabel = new Label
				{
					Text = $"第 {slotInfo.CurrentFloor} 层  ❤️ {slotInfo.CurrentHealth}/{slotInfo.MaxHealth}  💰 {slotInfo.Gold}",
					Modulate = new Color(0.85f, 0.85f, 0.85f),
					VerticalAlignment = VerticalAlignment.Center,
					MouseFilter = MouseFilterEnum.Ignore
				};
				infoLabel.AddThemeFontSizeOverride("font_size", 13);
				hbox.AddChild(infoLabel);

				var deleteBtn = new Button
				{
					Text = "删除",
					CustomMinimumSize = new Vector2(70, 35),
					Modulate = new Color(0.9f, 0.4f, 0.4f),
					MouseFilter = MouseFilterEnum.Stop
				};
				deleteBtn.Pressed += () => SaveDeleted?.Invoke(slotId);
				hbox.AddChild(deleteBtn);
			}
			else
			{
				var emptyLabel = new Label
				{
					Text = "空存档",
					Modulate = new Color(0.5f, 0.5f, 0.5f),
					VerticalAlignment = VerticalAlignment.Center,
					MouseFilter = MouseFilterEnum.Ignore
				};
				emptyLabel.AddThemeFontSizeOverride("font_size", 14);
				hbox.AddChild(emptyLabel);
			}

			panel.GuiInput += (ev) =>
			{
				if (ev is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
				{
					SaveSelected?.Invoke(slotId);
				}
			};

			return panel;
		}
	}
}
