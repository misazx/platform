using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;
using RoguelikeGame.Database;

namespace RoguelikeGame.UI.Panels
{
	public partial class ShopPanel : Control
	{
		public event System.Action Closed;

		private int _playerGold = 99;
		private List<ShopItemData> _shopItems = new();
		private VBoxContainer _itemsList;
		private Label _goldLabel;
		private Label _titleLabel;

		public override void _Ready()
		{
			AnchorsPreset = (int)Control.LayoutPreset.FullRect;
			MouseFilter = MouseFilterEnum.Stop;

			var bg = new ColorRect
			{
				AnchorsPreset = (int)Control.LayoutPreset.FullRect,
				Color = new Color(0.05f, 0.03f, 0.08f, 0.95f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			AddChild(bg);

			var mainPanel = new PanelContainer
			{
				CustomMinimumSize = new Vector2(800, 600),
				MouseFilter = MouseFilterEnum.Ignore
			};
			mainPanel.Position = new Vector2(240, 60);
			var panelStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.12f, 0.08f, 0.15f, 0.98f),
				CornerRadiusTopLeft = 15,
				CornerRadiusTopRight = 15,
				CornerRadiusBottomLeft = 15,
				CornerRadiusBottomRight = 15,
				BorderWidthLeft = 3,
				BorderWidthRight = 3,
				BorderWidthTop = 3,
				BorderWidthBottom = 3,
				BorderColor = new Color(0.8f, 0.65f, 0.2f, 0.9f)
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

			var header = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
			vbox.AddChild(header);

			_titleLabel = new Label
			{
				Text = "🏪 商店",
				Modulate = new Color(1f, 0.85f, 0.3f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_titleLabel.AddThemeFontSizeOverride("font_size", 22);
			header.AddChild(_titleLabel);

			var spacer = new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
			header.AddChild(spacer);

			_goldLabel = new Label
			{
				Text = $"💰 {_playerGold}",
				Modulate = new Color(1f, 0.85f, 0.2f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_goldLabel.AddThemeFontSizeOverride("font_size", 18);
			header.AddChild(_goldLabel);

			_itemsList = new VBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};
			_itemsList.AddThemeConstantOverride("separation", 6);
			vbox.AddChild(_itemsList);

			var closeBtn = new Button
			{
				Text = "离开商店",
				CustomMinimumSize = new Vector2(200, 40),
				MouseFilter = MouseFilterEnum.Stop
			};
			closeBtn.Pressed += () => Closed?.Invoke();
			vbox.AddChild(closeBtn);

			GenerateShopItems();
			RefreshDisplay();
		}

		private void GenerateShopItems()
		{
			_shopItems.Clear();

			var cardDb = CardDatabase.Instance;
			if (cardDb != null)
			{
				var allCards = cardDb.GetAllCards();
				var rng = new Random();
				for (int i = 0; i < 5 && allCards.Count > 0; i++)
				{
					int idx = rng.Next(allCards.Count);
					var card = allCards[idx];
					int price = card.Rarity == CardRarity.Rare ? 150 : card.Rarity == CardRarity.Uncommon ? 100 : 50;
					_shopItems.Add(new ShopItemData { Type = ShopItemType.Card, ItemId = card.Id, Price = price, Name = card.Name, Description = card.Description });
					allCards.RemoveAt(idx);
				}
			}

			_shopItems.Add(new ShopItemData { Type = ShopItemType.RemoveCard, Price = 75, Name = "移除一张牌", Description = "从牌组中移除一张卡牌" });

			var gm = GameManager.Instance;
			if (gm != null)
			{
				_playerGold = gm.CurrentRun?.Gold ?? 99;
				_goldLabel.Text = $"💰 {_playerGold}";
			}
		}

		private void RefreshDisplay()
		{
			foreach (var child in _itemsList.GetChildren())
				child.QueueFree();

			foreach (var item in _shopItems)
			{
				var row = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
				_itemsList.AddChild(row);

				var typeIcon = item.Type switch
				{
					ShopItemType.Card => "🃏",
					ShopItemType.Relic => "🏺",
					ShopItemType.Potion => "🧪",
					ShopItemType.RemoveCard => "🗑️",
					_ => "?"
				};

				var nameLabel = new Label
				{
					Text = $"{typeIcon} {item.Name}",
					CustomMinimumSize = new Vector2(200, 30),
					Modulate = item.Sold ? new Color(0.4f, 0.4f, 0.4f) : Colors.White,
					MouseFilter = MouseFilterEnum.Ignore
				};
				nameLabel.AddThemeFontSizeOverride("font_size", 14);
				row.AddChild(nameLabel);

				var descLabel = new Label
				{
					Text = item.Description,
					CustomMinimumSize = new Vector2(300, 30),
					Modulate = new Color(0.7f, 0.7f, 0.7f),
					MouseFilter = MouseFilterEnum.Ignore
				};
				descLabel.AddThemeFontSizeOverride("font_size", 11);
				row.AddChild(descLabel);

				var spacer = new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
				row.AddChild(spacer);

				var buyBtn = new Button
				{
					Text = item.Sold ? "已购买" : $"{item.Price} 💰",
					CustomMinimumSize = new Vector2(100, 30),
					Disabled = item.Sold || _playerGold < item.Price,
					MouseFilter = MouseFilterEnum.Stop
				};
				buyBtn.Pressed += () => OnItemPurchased(item);
				row.AddChild(buyBtn);
			}
		}

		private void OnItemPurchased(ShopItemData item)
		{
			if (item.Sold || _playerGold < item.Price) return;

			_playerGold -= item.Price;
			item.Sold = true;
			_goldLabel.Text = $"💰 {_playerGold}";

			var gm = GameManager.Instance;
			if (gm?.CurrentRun != null)
			{
				gm.CurrentRun.Gold = _playerGold;
			}

			GD.Print($"[ShopPanel] ✅ Purchased: {item.Name} for {item.Price} gold");
			RefreshDisplay();
		}
	}

	public enum ShopItemType { Card, Relic, Potion, RemoveCard }

	public class ShopItemData
	{
		public ShopItemType Type;
		public string ItemId;
		public int Price;
		public string Name;
		public string Description;
		public bool Sold;
	}
}