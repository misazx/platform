using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Systems;

namespace RoguelikeGame.UI.Panels
{
	public partial class AchievementPanel : Control
	{
		public event System.Action Closed;

		public AchievementPanel()
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
				CustomMinimumSize = new Vector2(700, 550),
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
				SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
				MouseFilter = MouseFilterEnum.Ignore
			};
			mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
			var panelStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.08f, 0.06f, 0.1f, 0.98f),
				CornerRadiusTopLeft = 15,
				CornerRadiusTopRight = 15,
				CornerRadiusBottomLeft = 15,
				CornerRadiusBottomRight = 15,
				BorderWidthLeft = 3,
				BorderWidthRight = 3,
				BorderWidthTop = 3,
				BorderWidthBottom = 3,
				BorderColor = new Color(0.7f, 0.6f, 0.9f, 0.9f),
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
			vbox.AddThemeConstantOverride("separation", 10);
			mainPanel.AddChild(vbox);

			var titleLabel = new Label
			{
				Text = "🏆 成就",
				HorizontalAlignment = HorizontalAlignment.Center,
				Modulate = new Color(0.9f, 0.85f, 1f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			titleLabel.AddThemeFontSizeOverride("font_size", 24);
			vbox.AddChild(titleLabel);

			var scrollContainer = new ScrollContainer
			{
				CustomMinimumSize = new Vector2(640, 400),
				MouseFilter = MouseFilterEnum.Ignore
			};
			vbox.AddChild(scrollContainer);

			var contentVbox = new VBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore
			};
			contentVbox.AddThemeConstantOverride("separation", 8);
			scrollContainer.AddChild(contentVbox);

			var categories = new[]
			{
				AchievementCategory.Combat,
				AchievementCategory.Exploration,
				AchievementCategory.Collection,
				AchievementCategory.Challenge,
				AchievementCategory.General
			};

			foreach (var category in categories)
			{
				var categorySection = CreateCategorySection(category);
				if (categorySection != null)
					contentVbox.AddChild(categorySection);
			}

			var spacer = new Control { CustomMinimumSize = new Vector2(0, 15), MouseFilter = MouseFilterEnum.Ignore };
			vbox.AddChild(spacer);

			var closeBtn = new Button
			{
				Text = "关闭",
				CustomMinimumSize = new Vector2(180, 42),
				MouseFilter = MouseFilterEnum.Stop,
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
			};
			closeBtn.Pressed += () => Closed?.Invoke();
			vbox.AddChild(closeBtn);
		}

		private Control CreateCategorySection(AchievementCategory category)
		{
			var definitions = GetCategoryDefinitions(category);
			if (definitions.Count == 0) return null;

			var section = new VBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore
			};
			section.AddThemeConstantOverride("separation", 6);

			var catLabel = new Label
			{
				Text = GetCategoryName(category),
				Modulate = new Color(0.7f, 0.8f, 0.95f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			catLabel.AddThemeFontSizeOverride("font_size", 16);
			section.AddChild(catLabel);

			var hbox = new HBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore
			};
			hbox.AddThemeConstantOverride("separation", 8);
			section.AddChild(hbox);

			int rowCount = 0;
			foreach (var def in definitions)
			{
				if (rowCount >= 4)
				{
					rowCount = 0;
					var newHbox = new HBoxContainer
					{
						MouseFilter = MouseFilterEnum.Ignore
					};
					newHbox.AddThemeConstantOverride("separation", 8);
					section.AddChild(newHbox);
					hbox = newHbox;
				}

				var achBtn = CreateAchievementButton(def);
				if (achBtn != null)
				{
					hbox.AddChild(achBtn);
					rowCount++;
				}
			}

			var separator = new HSeparator
			{
				CustomMinimumSize = new Vector2(0, 12),
				Modulate = new Color(0.3f, 0.3f, 0.4f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			section.AddChild(separator);

			return section;
		}

		private List<AchievementDefinition> GetCategoryDefinitions(AchievementCategory category)
		{
			var result = new List<AchievementDefinition>();
			if (AchievementManager.Instance == null) return result;

			var allDefs = typeof(AchievementManager).GetField("_definitions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			if (allDefs == null) return result;

			var dict = allDefs.GetValue(AchievementManager.Instance) as Dictionary<string, AchievementDefinition>;
			if (dict == null) return result;

			foreach (var kvp in dict)
			{
				if (kvp.Value.Category == category)
					result.Add(kvp.Value);
			}

			return result;
		}

		private string GetCategoryName(AchievementCategory category) => category switch
		{
			AchievementCategory.Combat => "⚔️ 战斗",
			AchievementCategory.Exploration => "🗺️ 探索",
			AchievementCategory.Collection => "📦 收集",
			AchievementCategory.Challenge => "🎯 挑战",
			AchievementCategory.General => "⭐ 通用",
			_ => "成就"
		};

		private Control CreateAchievementButton(AchievementDefinition def)
		{
			if (def == null) return null;

			bool isUnlocked = def.IsUnlocked;

			var btn = new Button
			{
				CustomMinimumSize = new Vector2(140, 55),
				MouseFilter = MouseFilterEnum.Stop,
				TooltipText = isUnlocked ? def.Description : (def.Hidden ? "???" : $"未解锁 - {def.Description}")
			};

			var btnStyle = new StyleBoxFlat
			{
				BgColor = isUnlocked ? new Color(0.2f, 0.25f, 0.35f, 0.9f) : new Color(0.1f, 0.1f, 0.12f, 0.6f),
				CornerRadiusTopLeft = 8,
				CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8,
				CornerRadiusBottomRight = 8,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = isUnlocked ? new Color(0.4f, 0.6f, 0.85f) : new Color(0.2f, 0.2f, 0.25f)
			};
			btn.AddThemeStyleboxOverride("panel", btnStyle);

			btn.Text = isUnlocked ? $"🏆 {def.Name}" : (def.Hidden ? "???" : $"🔒 {def.Name}");

			return btn;
		}
	}
}
