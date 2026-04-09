using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;

namespace RoguelikeGame.UI
{
	public partial class RewardScreen : Control
	{
		private VBoxContainer _mainContainer;
		private Label _titleLabel;
		private HBoxContainer _goldRow;
		private VBoxContainer _cardRewards;
		private VBoxContainer _relicRewards;
		private Button _skipButton;

		private List<StsCardData> _cards = new();
		private List<StsRelicData> _relics = new();
		private int _gold = 0;

		public event Action<StsCardData> CardSelected;
		public event Action<StsRelicData> RelicSelected;
		public event Action Skipped;

		public override void _Ready()
		{
			MouseFilter = MouseFilterEnum.Stop;
			CreateUI();
		}

		private void CreateUI()
		{
			_mainContainer = new VBoxContainer
			{
				AnchorsPreset = (int)Control.LayoutPreset.Center,
				CustomMinimumSize = new Vector2(800, 600),
				MouseFilter = MouseFilterEnum.Ignore
			};

			var style = new StyleBoxFlat
			{
				BgColor = new Color(0.08f, 0.06f, 0.1f, 0.95f),
				CornerRadiusTopLeft = 12,
				CornerRadiusTopRight = 12,
				CornerRadiusBottomLeft = 12,
				CornerRadiusBottomRight = 12,
				BorderWidthLeft = 3,
				BorderWidthRight = 3,
				BorderWidthTop = 3,
				BorderWidthBottom = 3,
				BorderColor = new Color(1f, 0.85f, 0.3f, 0.6f)
			};
			_mainContainer.AddThemeStyleboxOverride("panel", style);
			AddChild(_mainContainer);

			_titleLabel = new Label
			{
				Text = "🎉 战斗胜利！",
				HorizontalAlignment = HorizontalAlignment.Center,
				MouseFilter = MouseFilterEnum.Ignore
			};
			_titleLabel.AddThemeFontSizeOverride("font_size", 28);
			_titleLabel.Modulate = new Color(1f, 0.85f, 0.3f);
			_mainContainer.AddChild(_titleLabel);

			var spacer1 = new Control { CustomMinimumSize = new Vector2(0, 20), MouseFilter = MouseFilterEnum.Ignore };
			_mainContainer.AddChild(spacer1);

			_goldRow = new HBoxContainer
			{
				Alignment = BoxContainer.AlignmentMode.Center,
				MouseFilter = MouseFilterEnum.Ignore
			};
			_mainContainer.AddChild(_goldRow);

			var goldIcon = new Label
			{
				Text = "💰",
				MouseFilter = MouseFilterEnum.Ignore
			};
			goldIcon.AddThemeFontSizeOverride("font_size", 24);
			_goldRow.AddChild(goldIcon);

			var goldLabel = new Label
			{
				Text = " +20 金币",
				MouseFilter = MouseFilterEnum.Ignore
			};
			goldLabel.AddThemeFontSizeOverride("font_size", 20);
			goldLabel.Modulate = new Color(1f, 0.9f, 0.4f);
			_goldRow.AddChild(goldLabel);

			var spacer2 = new Control { CustomMinimumSize = new Vector2(0, 30), MouseFilter = MouseFilterEnum.Ignore };
			_mainContainer.AddChild(spacer2);

			var cardLabel = new Label
			{
				Text = "选择一张卡牌添加到卡组：",
				MouseFilter = MouseFilterEnum.Ignore
			};
			cardLabel.AddThemeFontSizeOverride("font_size", 18);
			_mainContainer.AddChild(cardLabel);

			_cardRewards = new VBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore
			};
			_mainContainer.AddChild(_cardRewards);

			var spacer3 = new Control { CustomMinimumSize = new Vector2(0, 20), MouseFilter = MouseFilterEnum.Ignore };
			_mainContainer.AddChild(spacer3);

			var relicLabel = new Label
			{
				Text = "遗物奖励：",
				MouseFilter = MouseFilterEnum.Ignore
			};
			relicLabel.AddThemeFontSizeOverride("font_size", 18);
			_mainContainer.AddChild(relicLabel);

			_relicRewards = new VBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore
			};
			_mainContainer.AddChild(_relicRewards);

			var spacer4 = new Control { CustomMinimumSize = new Vector2(0, 30), MouseFilter = MouseFilterEnum.Ignore };
			_mainContainer.AddChild(spacer4);

			_skipButton = new Button
			{
				Text = "跳过奖励",
				CustomMinimumSize = new Vector2(200, 45)
			};
			_skipButton.Pressed += () => Skipped?.Invoke();
			_mainContainer.AddChild(_skipButton);
		}

		public void SetupRewards(List<StsCardData> cards, List<StsRelicData> relics, int gold)
		{
			_cards = cards;
			_relics = relics;
			_gold = gold;

			UpdateGoldDisplay();
			UpdateCardDisplay();
			UpdateRelicDisplay();
		}

		private void UpdateGoldDisplay()
		{
			var goldLabel = _goldRow.GetChild<Label>(1);
			goldLabel.Text = $" +{_gold} 金币";
		}

		private void UpdateCardDisplay()
		{
			foreach (var child in _cardRewards.GetChildren())
				child.QueueFree();

			foreach (var card in _cards)
			{
				var cardButton = CreateCardButton(card);
				_cardRewards.AddChild(cardButton);
			}
		}

		private Button CreateCardButton(StsCardData card)
		{
			var btn = new Button
			{
				CustomMinimumSize = new Vector2(600, 80),
				Text = $"{card.Name} ({card.Cost}费) - {card.Description}"
			};

			var rarityColor = card.Rarity switch
			{
				StsCardRarity.Rare => new Color(1f, 0.6f, 0.2f),
				StsCardRarity.Uncommon => new Color(0.3f, 0.7f, 1f),
				_ => new Color(0.9f, 0.9f, 0.9f)
			};

			var style = new StyleBoxFlat
			{
				BgColor = new Color(0.15f, 0.12f, 0.1f),
				CornerRadiusTopLeft = 8,
				CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8,
				CornerRadiusBottomRight = 8,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = rarityColor
			};
			btn.AddThemeStyleboxOverride("normal", style);

			btn.Pressed += () =>
			{
				CardSelected?.Invoke(card);
				QueueFree();
			};

			return btn;
		}

		private void UpdateRelicDisplay()
		{
			foreach (var child in _relicRewards.GetChildren())
				child.QueueFree();

			if (_relics.Count == 0)
			{
				var noRelicLabel = new Label
				{
					Text = "无遗物奖励",
					Modulate = Colors.Gray,
					MouseFilter = MouseFilterEnum.Ignore
				};
				_relicRewards.AddChild(noRelicLabel);
				return;
			}

			foreach (var relic in _relics)
			{
				var relicButton = CreateRelicButton(relic);
				_relicRewards.AddChild(relicButton);
			}
		}

		private Button CreateRelicButton(StsRelicData relic)
		{
			var btn = new Button
			{
				CustomMinimumSize = new Vector2(600, 60),
				Text = $"💎 {relic.Name} - {relic.Description}"
			};

			var rarityColor = relic.Rarity switch
			{
				StsRelicRarity.Rare => new Color(1f, 0.6f, 0.2f),
				StsRelicRarity.Uncommon => new Color(0.3f, 0.7f, 1f),
				_ => new Color(0.9f, 0.9f, 0.9f)
			};

			btn.Modulate = rarityColor;

			btn.Pressed += () =>
			{
				RelicSelected?.Invoke(relic);
				QueueFree();
			};

			return btn;
		}

		public static RewardScreen ShowRewards(Control parent, List<StsCardData> cards, List<StsRelicData> relics, int gold)
		{
			var screen = new RewardScreen();
			screen.SetupRewards(cards, relics, gold);
			parent.AddChild(screen);
			return screen;
		}
	}

	public partial class ShopScreen : Control
	{
		private VBoxContainer _mainContainer;
		private HBoxContainer _goldBar;
		private Label _goldLabel;
		private HBoxContainer _cardRow;
		private HBoxContainer _relicRow;
		private Button _leaveButton;

		private int _playerGold = 99;

		public event Action<StsCardData, int> CardPurchased;
		public event Action<StsRelicData, int> RelicPurchased;
		public event Action ShopClosed;

		public override void _Ready()
		{
			MouseFilter = MouseFilterEnum.Stop;
			CreateUI();
		}

		private void CreateUI()
		{
			_mainContainer = new VBoxContainer
			{
				AnchorsPreset = (int)Control.LayoutPreset.FullRect,
				MouseFilter = MouseFilterEnum.Ignore
			};
			AddChild(_mainContainer);

			_goldBar = new HBoxContainer
			{
				CustomMinimumSize = new Vector2(0, 50),
				Alignment = BoxContainer.AlignmentMode.End,
				MouseFilter = MouseFilterEnum.Ignore
			};
			_mainContainer.AddChild(_goldBar);

			var goldIcon = new Label { Text = "💰", MouseFilter = MouseFilterEnum.Ignore };
			goldIcon.AddThemeFontSizeOverride("font_size", 24);
			_goldBar.AddChild(goldIcon);

			_goldLabel = new Label { Text = " 99", MouseFilter = MouseFilterEnum.Ignore };
			_goldLabel.AddThemeFontSizeOverride("font_size", 20);
			_goldLabel.Modulate = new Color(1f, 0.9f, 0.4f);
			_goldBar.AddChild(_goldLabel);

			var spacer = new Control { MouseFilter = MouseFilterEnum.Ignore };
			_goldBar.AddChild(spacer);
			_goldBar.AddChild(spacer);

			var titleLabel = new Label
			{
				Text = "🏪 商店",
				HorizontalAlignment = HorizontalAlignment.Center,
				MouseFilter = MouseFilterEnum.Ignore
			};
			titleLabel.AddThemeFontSizeOverride("font_size", 32);
			_mainContainer.AddChild(titleLabel);

			var cardLabel = new Label
			{
				Text = "卡牌 (点击购买)",
				MouseFilter = MouseFilterEnum.Ignore
			};
			cardLabel.AddThemeFontSizeOverride("font_size", 18);
			_mainContainer.AddChild(cardLabel);

			_cardRow = new HBoxContainer
			{
				CustomMinimumSize = new Vector2(0, 200),
				Alignment = BoxContainer.AlignmentMode.Center,
				MouseFilter = MouseFilterEnum.Ignore
			};
			_mainContainer.AddChild(_cardRow);

			var relicLabel = new Label
			{
				Text = "遗物",
				MouseFilter = MouseFilterEnum.Ignore
			};
			relicLabel.AddThemeFontSizeOverride("font_size", 18);
			_mainContainer.AddChild(relicLabel);

			_relicRow = new HBoxContainer
			{
				CustomMinimumSize = new Vector2(0, 120),
				Alignment = BoxContainer.AlignmentMode.Center,
				MouseFilter = MouseFilterEnum.Ignore
			};
			_mainContainer.AddChild(_relicRow);

			_leaveButton = new Button
			{
				Text = "离开商店",
				CustomMinimumSize = new Vector2(200, 50)
			};
			_leaveButton.Pressed += () => ShopClosed?.Invoke();
			_mainContainer.AddChild(_leaveButton);
		}

		public void SetupShop(int playerGold)
		{
			_playerGold = playerGold;
			UpdateGoldDisplay();
			GenerateShopItems();
		}

		private void UpdateGoldDisplay()
		{
			_goldLabel.Text = $" {_playerGold}";
		}

		private void GenerateShopItems()
		{
			var rng = new Random();

			var cards = StsCardDatabase.GetRandomRewardCards(5, rng);
			foreach (var card in cards)
			{
				int price = card.Rarity switch
				{
					StsCardRarity.Rare => 150,
					StsCardRarity.Uncommon => 100,
					_ => 50
				};
				var cardItem = CreateShopCard(card, price);
				_cardRow.AddChild(cardItem);
			}

			var relics = StsRelicManager.GetRandomRelics(3, rng);
			foreach (var relic in relics)
			{
				int price = relic.Rarity switch
				{
					StsRelicRarity.Rare => 200,
					StsRelicRarity.Uncommon => 150,
					_ => 100
				};
				var relicItem = CreateShopRelic(relic, price);
				_relicRow.AddChild(relicItem);
			}
		}

		private Control CreateShopCard(StsCardData card, int price)
		{
			var container = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };

			var btn = new Button
			{
				CustomMinimumSize = new Vector2(140, 180),
				Text = $"{card.Name}\n{card.Cost}费\n{card.Description}"
			};

			btn.Pressed += () =>
			{
				if (_playerGold >= price)
				{
					_playerGold -= price;
					UpdateGoldDisplay();
					CardPurchased?.Invoke(card, price);
					btn.Disabled = true;
					btn.Modulate = new Color(0.5f, 0.5f, 0.5f);
				}
			};

			container.AddChild(btn);

			var priceLabel = new Label
			{
				Text = $"💰 {price}",
				HorizontalAlignment = HorizontalAlignment.Center,
				MouseFilter = MouseFilterEnum.Ignore
			};
			priceLabel.Modulate = new Color(1f, 0.9f, 0.4f);
			container.AddChild(priceLabel);

			return container;
		}

		private Control CreateShopRelic(StsRelicData relic, int price)
		{
			var container = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };

			var btn = new Button
			{
				CustomMinimumSize = new Vector2(140, 80),
				Text = $"💎 {relic.Name}\n{relic.Description}"
			};

			btn.Pressed += () =>
			{
				if (_playerGold >= price)
				{
					_playerGold -= price;
					UpdateGoldDisplay();
					RelicPurchased?.Invoke(relic, price);
					btn.Disabled = true;
					btn.Modulate = new Color(0.5f, 0.5f, 0.5f);
				}
			};

			container.AddChild(btn);

			var priceLabel = new Label
			{
				Text = $"💰 {price}",
				HorizontalAlignment = HorizontalAlignment.Center,
				MouseFilter = MouseFilterEnum.Ignore
			};
			priceLabel.Modulate = new Color(1f, 0.9f, 0.4f);
			container.AddChild(priceLabel);

			return container;
		}

		public static ShopScreen ShowShop(Control parent, int playerGold)
		{
			var screen = new ShopScreen();
			screen.SetupShop(playerGold);
			parent.AddChild(screen);
			return screen;
		}
	}
}