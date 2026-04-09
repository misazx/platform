using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;

namespace RoguelikeGame.UI.Panels
{
	public partial class EventPanel : Control
	{
		public event System.Action Closed;

		private static readonly List<EventData> _eventPool = new()
		{
			new EventData
			{
				Id = "golden_shrine",
				Title = "🏛️ 金色神殿",
				Description = "你发现了一座闪耀着金光的神殿。神殿中央有一个古老的祭坛，上面刻着神秘的文字。",
				Choices = new List<EventChoice>
				{
					new() { Text = "🙏 祈祷 - 获得25金币", Effect = "gold:25" },
					new() { Text = "🤲 献祭 - 失去5HP获得遗物", Effect = "hp:-5,relic:random" },
					new() { Text = "🚶 离开", Effect = "none" }
				}
			},
			new EventData
			{
				Id = "mushroom_ring",
				Title = "🍄 蘑菇圈",
				Description = "一片发光的蘑菇围成了一个圆环。空气中弥漫着奇异的孢子。",
				Choices = new List<EventChoice>
				{
					new() { Text = "🍄 吸入孢子 - 获得随机药水", Effect = "potion:random" },
					new() { Text = "🔥 焚烧蘑菇 - 获得15金币", Effect = "gold:15" },
					new() { Text = "🚶 绕道而行", Effect = "none" }
				}
			},
			new EventData
			{
				Id = "wandering_merchant",
				Title = "🧙 流浪商人",
				Description = "一个神秘的商人向你招手。他的货物看起来既诱人又危险。",
				Choices = new List<EventChoice>
				{
					new() { Text = "💰 购买 - 失去30金币获得稀有卡牌", Effect = "gold:-30,card:rare" },
					new() { Text = "🗡️ 威胁 - 50%几率获得50金币，50%几率失去10HP", Effect = "gamble:50gold_10hp" },
					new() { Text = "🚶 拒绝", Effect = "none" }
				}
			},
			new EventData
			{
				Id = "ancient_fountain",
				Title = "⛲ 古老喷泉",
				Description = "一座古老的喷泉，水流清澈见底。你感到一股治愈的力量。",
				Choices = new List<EventChoice>
				{
					new() { Text = "💧 饮用 - 恢复20HP", Effect = "hp:20" },
					new() { Text = "🪙 投币 - 获得遗物", Effect = "gold:-10,relic:random" },
					new() { Text = "🚶 离开", Effect = "none" }
				}
			},
			new EventData
			{
				Id = "dark_altar",
				Title = "🌑 黑暗祭坛",
				Description = "一个散发着不祥气息的祭坛。你能感受到强大的力量，但代价未知。",
				Choices = new List<EventChoice>
				{
					new() { Text = "💀 献血 - 失去10HP升级一张卡牌", Effect = "hp:-10,upgrade:1" },
					new() { Text = "🔮 触碰 - 随机获得正面或负面效果", Effect = "random:buff_debuff" },
					new() { Text = "🚶 远离", Effect = "none" }
				}
			}
		};

		public override void _Ready()
		{
			AnchorsPreset = (int)Control.LayoutPreset.FullRect;
			MouseFilter = MouseFilterEnum.Stop;

			var bg = new ColorRect
			{
				AnchorsPreset = (int)Control.LayoutPreset.FullRect,
				Color = new Color(0.03f, 0.04f, 0.06f, 0.95f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			AddChild(bg);

			var mainPanel = new PanelContainer
			{
				CustomMinimumSize = new Vector2(700, 500),
				MouseFilter = MouseFilterEnum.Ignore
			};
			mainPanel.Position = new Vector2(290, 110);
			var panelStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.08f, 0.06f, 0.12f, 0.98f),
				CornerRadiusTopLeft = 15,
				CornerRadiusTopRight = 15,
				CornerRadiusBottomLeft = 15,
				CornerRadiusBottomRight = 15,
				BorderWidthLeft = 3,
				BorderWidthRight = 3,
				BorderWidthTop = 3,
				BorderWidthBottom = 3,
				BorderColor = new Color(0.3f, 0.5f, 0.8f, 0.9f)
			};
			mainPanel.AddThemeStyleboxOverride("panel", panelStyle);
			AddChild(mainPanel);

			var vbox = new VBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore,
				AnchorsPreset = (int)Control.LayoutPreset.FullRect
			};
			vbox.AddThemeConstantOverride("separation", 10);
			mainPanel.AddChild(vbox);

			var rng = new Random();
			var evt = _eventPool[rng.Next(_eventPool.Count)];

			var titleLabel = new Label
			{
				Text = evt.Title,
				HorizontalAlignment = HorizontalAlignment.Center,
				Modulate = new Color(0.7f, 0.8f, 1f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			titleLabel.AddThemeFontSizeOverride("font_size", 22);
			vbox.AddChild(titleLabel);

			var descLabel = new Label
			{
				Text = evt.Description,
				HorizontalAlignment = HorizontalAlignment.Center,
				Modulate = new Color(0.8f, 0.8f, 0.8f),
				MouseFilter = MouseFilterEnum.Ignore,
				AutowrapMode = TextServer.AutowrapMode.WordSmart,
				CustomMinimumSize = new Vector2(650, 80)
			};
			descLabel.AddThemeFontSizeOverride("font_size", 14);
			vbox.AddChild(descLabel);

			var spacer = new Control { SizeFlagsVertical = Control.SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
			vbox.AddChild(spacer);

			foreach (var choice in evt.Choices)
			{
				var btn = new Button
				{
					Text = choice.Text,
					CustomMinimumSize = new Vector2(500, 45),
					MouseFilter = MouseFilterEnum.Stop
				};
				var capturedChoice = choice;
				btn.Pressed += () => OnChoiceSelected(capturedChoice);
				vbox.AddChild(btn);
			}

			var closeBtn = new Button
			{
				Text = "继续前进",
				CustomMinimumSize = new Vector2(200, 40),
				MouseFilter = MouseFilterEnum.Stop
			};
			closeBtn.Pressed += () => Closed?.Invoke();
			vbox.AddChild(closeBtn);
		}

		private void OnChoiceSelected(EventChoice choice)
		{
			GD.Print($"[EventPanel] Choice: {choice.Text} → Effect: {choice.Effect}");
			ApplyEffect(choice.Effect);
			Closed?.Invoke();
		}

		private void ApplyEffect(string effectStr)
		{
			if (string.IsNullOrEmpty(effectStr) || effectStr == "none") return;

			var run = GameManager.Instance?.CurrentRun;
			if (run == null) return;

			var parts = effectStr.Split(',');
			foreach (var part in parts)
			{
				var kv = part.Split(':');
				if (kv.Length != 2) continue;

				var key = kv[0];
				var value = kv[1];

				switch (key)
				{
					case "gold":
						run.Gold += int.Parse(value);
						GD.Print($"[EventPanel] Gold change: {value}, total: {run.Gold}");
						break;
					case "hp":
						int hpChange = int.Parse(value);
						if (hpChange > 0)
							run.CurrentHealth = System.Math.Min(run.MaxHealth, run.CurrentHealth + hpChange);
						else
							run.CurrentHealth = System.Math.Max(1, run.CurrentHealth + hpChange);
						GD.Print($"[EventPanel] HP change: {hpChange}, now: {run.CurrentHealth}");
						break;
					case "relic":
						GD.Print($"[EventPanel] Relic: {value}");
						break;
					case "potion":
						GD.Print($"[EventPanel] Potion: {value}");
						break;
					case "card":
						GD.Print($"[EventPanel] Card: {value}");
						break;
				}
			}
		}
	}

	public class EventData
	{
		public string Id;
		public string Title;
		public string Description;
		public List<EventChoice> Choices;
	}

	public class EventChoice
	{
		public string Text;
		public string Effect;
	}
}