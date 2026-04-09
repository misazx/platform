using Godot;
using System.Collections.Generic;
using System.Linq;
using RoguelikeGame.Core;
using RoguelikeGame.Database;
using RoguelikeGame.Audio;

namespace RoguelikeGame.UI
{
	[GlobalClass]
	public partial class CombatHUD : Control, IUIScreen
	{
		private Control _rootContainer;
		private ColorRect _combatBg;

		private Label _floorLabel;
		private Label _turnLabel;
		private Label _energyLabel;
		private Label _phaseLabel;

		private Control _battleSceneArea;
		private List<BattleCharacterSprite> _enemySprites = new();
		private BattleCharacterSprite _playerSprite;

		private Control _enemyStatusArea;
		private List<EnemyUnitUI> _enemies = new();

		private Control _playerStatusArea;
		private TextureRect _playerAvatar;
		private ProgressBar _playerHealthBar;
		private Label _playerHealthText;
		private ProgressBar _playerBlockBar;
		private Label _playerBlockText;
		private HBoxContainer _relicsRow;

		private Control _handArea;
		private List<CardUI> _handCards = new();

		private Button _drawPileBtn;
		private Button _endTurnBtn;
		private Button _discardPileBtn;
		private Label _handCountLabel;

		private int _currentEnergy = 3;
		private int _maxEnergy = 3;
		private bool _isPlayerTurn = true;
		private bool _isProcessing = false;
		private bool _isSelectingTarget = false;
		private CardUI _pendingCard = null;

		[Signal]
		public delegate void CardPlayedEventHandler(string cardId);
		[Signal]
		public delegate void EndTurnEventHandler();
		[Signal]
		public delegate void CardPlayedWithTargetEventHandler(string cardId, int targetIndex);

		public override void _Ready()
		{
			MouseFilter = MouseFilterEnum.Stop;
			CreateLayout();
			ConnectSignals();
		}

		private void CreateLayout()
		{
			_rootContainer = new Control
			{
				AnchorsPreset = (int)Control.LayoutPreset.FullRect,
				MouseFilter = MouseFilterEnum.Ignore
			};
			AddChild(_rootContainer);

			CreateCombatBackground();
			CreateTopBar();
			CreateBattleSceneView();
			CreateEnemyStatusBar();
			CreatePlayerStatusBar();
			CreateHandArea();
			CreateBottomButtons();
		}

		private void CreateCombatBackground()
		{
			var bgTexture = GD.Load<Texture2D>("res://Images/Backgrounds/hive.png");
			if (bgTexture != null)
			{
				var bgImage = new TextureRect
				{
					AnchorsPreset = (int)Control.LayoutPreset.FullRect,
					Texture = bgTexture,
					StretchMode = TextureRect.StretchModeEnum.Scale,
					MouseFilter = MouseFilterEnum.Ignore
				};
				_rootContainer.AddChild(bgImage);

				var bgOverlay = new ColorRect
				{
					AnchorsPreset = (int)Control.LayoutPreset.FullRect,
					Color = new Color(0, 0, 0, 0.35f),
					MouseFilter = MouseFilterEnum.Ignore
				};
				_rootContainer.AddChild(bgOverlay);
			}
			else
			{
				_combatBg = new ColorRect
				{
					AnchorsPreset = (int)Control.LayoutPreset.FullRect,
					Color = new Color(0.06f, 0.04f, 0.08f, 1f),
					MouseFilter = MouseFilterEnum.Ignore
				};
				_rootContainer.AddChild(_combatBg);
			}

			var floorGrad = new ColorRect
			{
				CustomMinimumSize = new Vector2(1280, 80),
				Color = new Color(0.04f, 0.03f, 0.02f, 0.5f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			floorGrad.Position = new Vector2(0, 430);
			_rootContainer.AddChild(floorGrad);
		}

		private void CreateTopBar()
		{
			var topBar = new HBoxContainer
			{
				CustomMinimumSize = new Vector2(0, 26),
				MouseFilter = MouseFilterEnum.Ignore
			};
			topBar.Position = new Vector2(12, 4);
			_rootContainer.AddChild(topBar);

			_floorLabel = new Label
			{
				Text = "第 1 层",
				Modulate = Colors.Gray,
				MouseFilter = MouseFilterEnum.Ignore
			};
			_floorLabel.AddThemeFontSizeOverride("font_size", 11);
			topBar.AddChild(_floorLabel);

			var spacer = new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
			topBar.AddChild(spacer);

			_phaseLabel = new Label
			{
				Text = "你的回合",
				Modulate = new Color(0.5f, 0.9f, 0.5f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_phaseLabel.AddThemeFontSizeOverride("font_size", 12);
			topBar.AddChild(_phaseLabel);

			var spacer2 = new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
			topBar.AddChild(spacer2);

			_turnLabel = new Label
			{
				Text = "回合 1",
				Modulate = new Color(1f, 0.92f, 0.3f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_turnLabel.AddThemeFontSizeOverride("font_size", 11);
			topBar.AddChild(_turnLabel);

			_energyLabel = new Label
			{
				Text = "⚡ 3/3",
				Modulate = new Color(1f, 0.85f, 0.2f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_energyLabel.AddThemeFontSizeOverride("font_size", 14);
			topBar.AddChild(_energyLabel);
		}

		private void CreateBattleSceneView()
		{
			_battleSceneArea = new Control
			{
				CustomMinimumSize = new Vector2(1280, 340),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_battleSceneArea.Position = new Vector2(0, 32);
			_rootContainer.AddChild(_battleSceneArea);

			_playerSprite = new BattleCharacterSprite("ironclad", true)
			{
				CustomMinimumSize = new Vector2(140, 170)
			};
			_playerSprite.Position = new Vector2(60, 140);
			_battleSceneArea.AddChild(_playerSprite);
		}

		public void AddEnemy(string enemyName, int maxHp)
		{
			var sprite = new BattleCharacterSprite(enemyName, false)
			{
				CustomMinimumSize = new Vector2(160, 200)
			};
			_enemySprites.Add(sprite);
			_battleSceneArea.AddChild(sprite);
			UpdateBattleSpritePositions();

			var statusUI = new EnemyUnitUI(enemyName, maxHp)
			{
				CustomMinimumSize = new Vector2(140, 65)
			};
			_enemies.Add(statusUI);
			statusUI.SetEnemyIndex(_enemies.Count - 1);
			statusUI.EnemyClicked += OnEnemyClicked;
			_enemyStatusArea.AddChild(statusUI);
			UpdateEnemyStatusPositions();
		}

		private void UpdateBattleSpritePositions()
		{
			int count = _enemySprites.Count;
			if (count == 0) return;

			float screenWidth = 1280;
			float totalWidth = count * 160 + (count - 1) * 30;
			float startX = (screenWidth - totalWidth) / 2f + 120;

			for (int i = 0; i < count; i++)
			{
				_enemySprites[i].Position = new Vector2(startX + i * 190, 20);
			}
		}

		private void CreateEnemyStatusBar()
		{
			_enemyStatusArea = new Control
			{
				CustomMinimumSize = new Vector2(1280, 70),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_enemyStatusArea.Position = new Vector2(0, 240);
			_rootContainer.AddChild(_enemyStatusArea);
		}

		private void UpdateEnemyStatusPositions()
		{
			int count = _enemies.Count;
			if (count == 0) return;

			float screenWidth = 1280;
			float totalWidth = count * 140 + (count - 1) * 20;
			float startX = (screenWidth - totalWidth) / 2f + 140;

			for (int i = 0; i < count; i++)
			{
				_enemies[i].Position = new Vector2(startX + i * 160, 0);
			}
		}

		public void UpdateEnemyHealth(int enemyIndex, int currentHp, int maxHp)
		{
			if (enemyIndex >= 0 && enemyIndex < _enemies.Count)
				_enemies[enemyIndex].UpdateHealth(currentHp, maxHp);
		}

		public void UpdateEnemyIntent(int enemyIndex, string intentText, string intentIcon)
		{
			if (enemyIndex >= 0 && enemyIndex < _enemies.Count)
				_enemies[enemyIndex].UpdateIntent(intentText, intentIcon);
		}

		private void CreatePlayerStatusBar()
		{
			_playerStatusArea = new VBoxContainer
			{
				CustomMinimumSize = new Vector2(220, 80),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_playerStatusArea.Position = new Vector2(12, 330);
			_rootContainer.AddChild(_playerStatusArea);

			var headerRow = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
			_playerStatusArea.AddChild(headerRow);

			_playerAvatar = new TextureRect
			{
				CustomMinimumSize = new Vector2(28, 28),
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
				MouseFilter = MouseFilterEnum.Ignore
			};

			var playerTexture = GD.Load<Texture2D>("res://Icons/Items/iron_sword.png");
			if (playerTexture != null)
				_playerAvatar.Texture = playerTexture;

			headerRow.AddChild(_playerAvatar);

			var nameLabel = new Label
			{
				Text = "铁甲战士",
				Modulate = Colors.White,
				MouseFilter = MouseFilterEnum.Ignore
			};
			nameLabel.AddThemeFontSizeOverride("font_size", 12);
			headerRow.AddChild(nameLabel);

			_playerHealthText = new Label
			{
				Text = "80/80",
				Modulate = new Color(1f, 0.4f, 0.4f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_playerHealthText.AddThemeFontSizeOverride("font_size", 11);
			headerRow.AddChild(_playerHealthText);

			_playerHealthBar = new ProgressBar
			{
				MaxValue = 100,
				Value = 80,
				CustomMinimumSize = new Vector2(210, 16),
				MouseFilter = MouseFilterEnum.Ignore
			};
			var healthStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.85f, 0.2f, 0.2f),
				CornerRadiusTopLeft = 4,
				CornerRadiusTopRight = 4,
				CornerRadiusBottomLeft = 4,
				CornerRadiusBottomRight = 4
			};
			_playerHealthBar.AddThemeStyleboxOverride("fill", healthStyle);
			_playerStatusArea.AddChild(_playerHealthBar);

			var blockRow = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
			_playerStatusArea.AddChild(blockRow);

			_playerBlockText = new Label
			{
				Text = "",
				Modulate = new Color(0.4f, 0.6f, 1f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_playerBlockText.AddThemeFontSizeOverride("font_size", 10);
			blockRow.AddChild(_playerBlockText);

			_playerBlockBar = new ProgressBar
			{
				MaxValue = 50,
				Value = 0,
				CustomMinimumSize = new Vector2(140, 10),
				Visible = false,
				MouseFilter = MouseFilterEnum.Ignore
			};
			var blockStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.3f, 0.5f, 1f, 0.9f),
				CornerRadiusTopLeft = 3,
				CornerRadiusTopRight = 3,
				CornerRadiusBottomLeft = 3,
				CornerRadiusBottomRight = 3
			};
			_playerBlockBar.AddThemeStyleboxOverride("fill", blockStyle);
			blockRow.AddChild(_playerBlockBar);

			_relicsRow = new HBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore
			};
			_relicsRow.AddThemeConstantOverride("separation", 3);
			_playerStatusArea.AddChild(_relicsRow);
		}

		private void CreateHandArea()
		{
			_handArea = new Control
			{
				CustomMinimumSize = new Vector2(1240, 130),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_handArea.Position = new Vector2(20, 415);
			_rootContainer.AddChild(_handArea);

			_handCountLabel = new Label
			{
				Text = "手牌: 0",
				Modulate = new Color(0.7f, 0.7f, 0.7f, 0.8f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_handCountLabel.AddThemeFontSizeOverride("font_size", 10);
			_handCountLabel.Position = new Vector2(540, -14);
			_handArea.AddChild(_handCountLabel);

			for (int i = 0; i < 10; i++)
			{
				var card = new CardUI(i)
				{
					CustomMinimumSize = new Vector2(105, 130)
				};
				card.CardPressed += OnCardPressed;
				_handCards.Add(card);
				_handArea.AddChild(card);
			}

			UpdateCardPositions();
		}

		private void UpdateCardPositions()
		{
			int count = _handCards.Count(c => c.Visible);
			if (count == 0) return;

			float centerX = 600;
			float baseY = 0;
			float cardSpacing = 112;
			float startOffsetX = -(float)(count - 1) * cardSpacing / 2f;
			int visibleIndex = 0;

			for (int i = 0; i < _handCards.Count; i++)
			{
				if (!_handCards[i].Visible) continue;

				float xOffset = startOffsetX + visibleIndex * cardSpacing;
				float yOffset = Mathf.Abs(xOffset) * 0.04f;
				_handCards[i].Position = new Vector2(centerX + xOffset - 55, baseY + yOffset);
				visibleIndex++;
			}
		}

		private void CreateBottomButtons()
		{
			var bottomBar = new HBoxContainer
			{
				CustomMinimumSize = new Vector2(1280, 42),
				MouseFilter = MouseFilterEnum.Ignore,
				Alignment = BoxContainer.AlignmentMode.Center
			};
			bottomBar.AddThemeConstantOverride("separation", 25);
			bottomBar.Position = new Vector2(0, 555);
			bottomBar.ZIndex = 10;
			_rootContainer.AddChild(bottomBar);

			_drawPileBtn = new Button
			{
				Text = "📥 抽牌堆 (45)",
				CustomMinimumSize = new Vector2(120, 34),
				Flat = true,
				MouseFilter = MouseFilterEnum.Stop
			};
			var drawBtnStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.18f, 0.28f, 0.18f),
				CornerRadiusTopLeft = 8,
				CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8,
				CornerRadiusBottomRight = 8,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = new Color(0.35f, 0.45f, 0.28f)
			};
			_drawPileBtn.AddThemeStyleboxOverride("normal", drawBtnStyle);
			_drawPileBtn.Pressed += OnDrawPileClicked;
			bottomBar.AddChild(_drawPileBtn);

			_endTurnBtn = new Button
			{
				Text = "⚔️ 结束回合",
				CustomMinimumSize = new Vector2(140, 38),
				Flat = true,
				MouseFilter = MouseFilterEnum.Stop
			};
			var endTurnStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.4f, 0.3f, 0.1f),
				CornerRadiusTopLeft = 10,
				CornerRadiusTopRight = 10,
				CornerRadiusBottomLeft = 10,
				CornerRadiusBottomRight = 10,
				BorderWidthLeft = 3,
				BorderWidthRight = 3,
				BorderWidthTop = 3,
				BorderWidthBottom = 3,
				BorderColor = new Color(1f, 0.75f, 0.2f, 0.95f)
			};
			_endTurnBtn.AddThemeStyleboxOverride("normal", endTurnStyle);
			_endTurnBtn.Pressed += OnEndTurnClicked;
			bottomBar.AddChild(_endTurnBtn);

			_discardPileBtn = new Button
			{
				Text = "📤 弃牌堆 (0)",
				CustomMinimumSize = new Vector2(120, 34),
				Flat = true,
				MouseFilter = MouseFilterEnum.Stop
			};
			var discardBtnStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.28f, 0.18f, 0.18f),
				CornerRadiusTopLeft = 8,
				CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8,
				CornerRadiusBottomRight = 8,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = new Color(0.45f, 0.3f, 0.28f)
			};
			_discardPileBtn.AddThemeStyleboxOverride("normal", discardBtnStyle);
			_discardPileBtn.Pressed += OnDiscardPileClicked;
			bottomBar.AddChild(_discardPileBtn);
		}

		private void ConnectSignals() { }

		private void OnDrawPileClicked()
		{
			GD.Print("[CombatHUD] 📥 抽牌堆点击");
			AudioManager.Instance?.PlayButtonClick();
			ShowPileViewRequested?.Invoke("抽牌堆");
		}

		private void OnDiscardPileClicked()
		{
			GD.Print("[CombatHUD] 📤 弃牌堆点击");
			AudioManager.Instance?.PlayButtonClick();
			ShowPileViewRequested?.Invoke("弃牌堆");
		}

		public event System.Action<string> ShowPileViewRequested;

		private void OnEndTurnClicked()
		{
			if (_isProcessing || !_isPlayerTurn)
			{
				GD.Print("[CombatHUD] ⏳ 无法操作 - 正在处理中");
				return;
			}

			GD.Print("[CombatHUD] ⚔️ 结束回合按钮点击");
			AudioManager.Instance?.PlayButtonClick();
			SetPhase(false);
			EmitSignal(SignalName.EndTurn);
		}

		public void SetPhase(bool isPlayerTurn)
		{
			_isPlayerTurn = isPlayerTurn;
			_phaseLabel.Text = isPlayerTurn ? "你的回合" : "敌方回合";
			_phaseLabel.Modulate = isPlayerTurn ? new Color(0.5f, 0.9f, 0.5f) : new Color(0.9f, 0.4f, 0.3f);
			_endTurnBtn.Disabled = !isPlayerTurn;
			_endTurnBtn.Modulate = isPlayerTurn ? Colors.White : new Color(0.5f, 0.5f, 0.5f);
		}

		public void SetProcessing(bool processing)
		{
			_isProcessing = processing;
		}

		public void OnShow()
		{
			Visible = true;
			AudioManager.Instance?.PlayBGM("combat_ambient");
		}

		public void OnHide() => Visible = false;

		public void UpdateHand(List<CardData> cards)
		{
			foreach (var card in _handCards)
				card.Visible = false;

			for (int i = 0; i < cards.Count && i < _handCards.Count; i++)
			{
				_handCards[i].SetCardData(cards[i]);
				_handCards[i].Visible = true;
			}

			UpdateCardPositions();
			_handCountLabel.Text = $"手牌: {cards.Count}";
		}

		public void UpdateEnergy(int current, int max)
		{
			_currentEnergy = current;
			_maxEnergy = max;
			_energyLabel.Text = $"⚡ {current}/{max}";
		}

		public void UpdateHealth(int current, int max)
		{
			_playerHealthBar.MaxValue = max;
			_playerHealthBar.Value = current;
			_playerHealthText.Text = $"{current}/{max}";
		}

		public void UpdateBlock(int block)
		{
			_playerBlockBar.Value = block;
			_playerBlockBar.Visible = block > 0;
			_playerBlockText.Text = block > 0 ? $"🛡️ {block}" : "";
		}

		public void UpdateDrawPile(int count)
		{
			_drawPileBtn.Text = $"📥 抽牌堆 ({count})";
		}

		public void UpdateDiscardPile(int count)
		{
			_discardPileBtn.Text = $"📤 弃牌堆 ({count})";
		}

		public void SetTurnNumber(int turn) => _turnLabel.Text = $"回合 {turn}";
		public void SetFloorNumber(int floor) => _floorLabel.Text = $"第 {floor} 层";

		private void OnCardPressed(CardUI card)
		{
			if (_isProcessing || !_isPlayerTurn)
			{
				GD.Print("[CombatHUD] ⏳ 无法出牌 - 不是你的回合");
				return;
			}

			if (card.CardData != null && _currentEnergy >= card.CardData.Cost)
			{
				bool needsTarget = card.CardData.Type == CardType.Attack;
				if (needsTarget && _enemies.Count > 1)
				{
					_isSelectingTarget = true;
					_pendingCard = card;
					_phaseLabel.Text = "选择目标";
					_phaseLabel.Modulate = new Color(1f, 0.5f, 0.2f);
					HighlightEnemies(true);
					GD.Print($"[CombatHUD] 🎯 Select target for: {card.CardData.Name}");
				}
				else
				{
					GD.Print($"[CombatHUD] 🃏 出牌: {card.CardData.Name} (费用:{card.CardData.Cost})");
					ShowCardPlayAnimation(card.CardData, needsTarget && _enemies.Count > 0 ? 0 : -1);
					AudioManager.Instance?.PlaySFX("card_play");
					EmitSignal(SignalName.CardPlayed, card.CardData.Id);
				}
			}
			else if (card.CardData != null)
			{
				GD.Print($"[CombatHUD] ❌ 能量不足: {card.CardData.Name} 需要{card.CardData.Cost}点, 当前{_currentEnergy}点");
				FloatingText.ShowStatus(this, "能量不足!", new Vector2(600, 400));
			}
		}

		public void OnEnemyClicked(int enemyIndex)
		{
			if (_isSelectingTarget && _pendingCard != null)
			{
				var card = _pendingCard;
				_isSelectingTarget = false;
				_pendingCard = null;
				HighlightEnemies(false);
				_phaseLabel.Text = "你的回合";
				_phaseLabel.Modulate = new Color(0.5f, 0.9f, 0.5f);

				GD.Print($"[CombatHUD] 🎯 Target selected: enemy {enemyIndex} for {card.CardData.Name}");
					ShowCardPlayAnimation(card.CardData, enemyIndex);
					AudioManager.Instance?.PlaySFX("card_play");
				EmitSignal(SignalName.CardPlayedWithTarget, card.CardData.Id, enemyIndex);
			}
		}

		private void HighlightEnemies(bool highlight)
		{
			foreach (var enemy in _enemies)
			{
				enemy.SetSelectable(highlight);
			}
		}

		private void ShowCardPlayAnimation(CardData cardData, int targetEnemyIndex)
		{
			string iconPath = !string.IsNullOrEmpty(cardData.IconPath) ? cardData.IconPath : "res://Icons/Cards/card_0.png";
			if (cardData.Type == CardType.Attack) iconPath = "res://Icons/Cards/strike.png";
			else if (cardData.Type == CardType.Skill) iconPath = "res://Icons/Cards/defend.png";
			else if (cardData.Type == CardType.Power) iconPath = "res://Icons/Skills/fireball.png";
			var iconTex = GD.Load<Texture2D>(iconPath);
			if (iconTex == null) return;

			var cardImg = new TextureRect
			{
				Texture = iconTex,
				CustomMinimumSize = new Vector2(80, 110),
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				ZIndex = 100,
				MouseFilter = MouseFilterEnum.Ignore
			};

			var cardBg = new PanelContainer
			{
				CustomMinimumSize = new Vector2(84, 114),
				ZIndex = 99,
				MouseFilter = MouseFilterEnum.Ignore
			};
			var bgStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.12f, 0.1f, 0.08f, 0.95f),
				CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
				CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
				BorderWidthLeft = 2, BorderWidthRight = 2,
				BorderWidthTop = 2, BorderWidthBottom = 2,
				BorderColor = cardData.Type switch
				{
					CardType.Attack => new Color(0.8f, 0.3f, 0.3f),
					CardType.Skill => new Color(0.3f, 0.5f, 0.9f),
					CardType.Power => new Color(0.9f, 0.7f, 0.2f),
					_ => new Color(0.5f, 0.5f, 0.5f)
				}
			};
			cardBg.AddThemeStyleboxOverride("panel", bgStyle);

			var startPos = new Vector2(540, 400);
			Vector2 endPos;

			if (targetEnemyIndex >= 0 && targetEnemyIndex < _enemySprites.Count)
			{
				var enemySprite = _enemySprites[targetEnemyIndex];
				endPos = enemySprite.GlobalPosition + new Vector2(0, -30);
			}
			else
			{
				endPos = cardData.Type == CardType.Skill
					? new Vector2(120, 300)
					: new Vector2(540, 200);
			}

			cardBg.Position = startPos;
			cardImg.Position = new Vector2(2, 2);
			cardBg.AddChild(cardImg);
			AddChild(cardBg);

			var tween = CreateTween();
			tween.SetParallel(true);

			tween.TweenProperty(cardBg, "position", endPos, 0.4f)
				.SetEase(Tween.EaseType.In)
				.SetTrans(Tween.TransitionType.Back);

			tween.TweenProperty(cardBg, "scale", new Vector2(1.2f, 1.2f), 0.2f);
			tween.TweenProperty(cardBg, "modulate", new Color(2f, 2f, 2f, 1f), 0.2f);

			tween.Chain();
			tween.TweenProperty(cardBg, "modulate", new Color(1f, 1f, 1f, 0f), 0.3f)
				.SetDelay(0.3f);
			tween.TweenProperty(cardBg, "scale", new Vector2(0.5f, 0.5f), 0.3f)
				.SetDelay(0.3f);

			tween.TweenCallback(Callable.From(() => cardBg.QueueFree())).SetDelay(0.7f);
		}

		public void ShowEnemyAttackFeedback(int enemyIndex)
		{
			if (enemyIndex >= 0 && enemyIndex < _enemySprites.Count)
				_enemySprites[enemyIndex].PlayAttackAnimation(new Vector2(200, 300));
		}

		public void ShowPlayerHitFeedback()
		{
			_playerSprite?.PlayHitAnimation();
		}

		public void ShowEnemyHitFeedback(int enemyIndex)
		{
			if (enemyIndex >= 0 && enemyIndex < _enemySprites.Count)
				_enemySprites[enemyIndex].PlayHitAnimation();
		}
	}

	public partial class BattleCharacterSprite : Control
	{
		private string _characterId;
		private bool _isPlayer;
		private TextureRect _spriteDisplay;
		private PanelContainer _spriteFrame;
		private Label _nameOverlay;

		public BattleCharacterSprite(string characterId, bool isPlayer)
		{
			_characterId = characterId.ToLower().Replace("_pack", "").Replace("_", " ");
			_isPlayer = isPlayer;
		}

		public override void _Ready()
		{
			MouseFilter = MouseFilterEnum.Ignore;

			_spriteFrame = new PanelContainer
			{
				CustomMinimumSize = _isPlayer ? new Vector2(130, 160) : new Vector2(150, 190),
				MouseFilter = MouseFilterEnum.Ignore
			};

			var frameStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.08f, 0.06f, 0.05f, 0.7f),
				CornerRadiusTopLeft = 12,
				CornerRadiusTopRight = 12,
				CornerRadiusBottomLeft = 12,
				CornerRadiusBottomRight = 12,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = _isPlayer
					? new Color(0.25f, 0.4f, 0.7f, 0.8f)
					: new Color(0.7f, 0.25f, 0.25f, 0.85f)
			};
			_spriteFrame.AddThemeStyleboxOverride("panel", frameStyle);
			AddChild(_spriteFrame);

			var vbox = new VBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore,
				AnchorsPreset = (int)Control.LayoutPreset.FullRect
			};
			vbox.AddThemeConstantOverride("separation", 3);
			_spriteFrame.AddChild(vbox);

			_nameOverlay = new Label
			{
				Text = _isPlayer ? "铁甲战士" : _characterId,
				HorizontalAlignment = HorizontalAlignment.Center,
				Modulate = _isPlayer ? new Color(0.6f, 0.8f, 1f) : new Color(1f, 0.5f, 0.4f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_nameOverlay.AddThemeFontSizeOverride("font_size", 12);
			vbox.AddChild(_nameOverlay);

			_spriteDisplay = new TextureRect
			{
				CustomMinimumSize = new Vector2(0, _isPlayer ? 110 : 130),
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				MouseFilter = MouseFilterEnum.Ignore,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};

			string spritePath = GetCharacterSpritePath();
			var texture = GD.Load<Texture2D>(spritePath);
			if (texture != null)
				_spriteDisplay.Texture = texture;
			else
				_spriteDisplay.Texture = GeneratePlaceholderSprite();

			vbox.AddChild(_spriteDisplay);
		}

		private string GetCharacterSpritePath()
		{
			return _isPlayer switch
			{
				true => "res://Icons/Items/iron_sword.png",
				false => _characterId.ToLower() switch
				{
					"cultist" or "jaw worm" or "jawworm" => "res://Icons/Enemies/jawworm.png",
					"lagavulin" => "res://Icons/Enemies/lagavulin.png",
					"theguardian" or "the guardian" or "guardian" => "res://Icons/Enemies/theguardian.png",
					_ => "res://Icons/Enemies/cultist.png"
				}
			};
		}

		private ImageTexture GeneratePlaceholderSprite()
		{
			var img = Image.CreateEmpty(128, 128, false, Image.Format.Rgba8);
			img.Fill(_isPlayer ? new Color(0.2f, 0.3f, 0.5f, 1f) : new Color(0.5f, 0.2f, 0.2f, 1f));
			return ImageTexture.CreateFromImage(img);
		}

		public void PlayAttackAnimation(Vector2 targetPos)
		{
			var tween = CreateTween();
			tween.TweenProperty(this, "position:x", Position.X + 40, 0.12f).SetEase(Tween.EaseType.Out);
			tween.TweenProperty(this, "position:x", Position.X, 0.12f).SetDelay(0.12f).SetEase(Tween.EaseType.In);
		}

		public void PlayHitAnimation()
		{
			var tween = CreateTween();
			tween.TweenProperty(_spriteFrame, "modulate", new Color(2f, 2f, 2f, 1f), 0.08f);
			tween.TweenProperty(_spriteFrame, "modulate", Colors.White, 0.12f).SetDelay(0.08f);
		}

		public void PlayDeathAnimation()
		{
			var tween = CreateTween();
			tween.TweenProperty(this, "modulate:a", 0f, 0.5f).SetEase(Tween.EaseType.In);
			tween.TweenProperty(this, "scale", new Vector2(0.8f, 0.8f), 0.5f).SetEase(Tween.EaseType.In);
		}
	}

	public partial class EnemyUnitUI : Control
	{
		private string _name;
		private int _maxHp;
		private int _currentHp;
		private int _enemyIndex = -1;

		private PanelContainer _body;
		private ProgressBar _healthBar;
		private Label _healthText;
		private Label _intentLabel;
		private bool _isSelectable = false;

		public event System.Action<int> EnemyClicked;

		public EnemyUnitUI(string name, int maxHp)
		{
			_name = name.Replace("_pack", "").Replace("_", " ");
			_maxHp = maxHp;
			_currentHp = maxHp;
			CustomMinimumSize = new Vector2(135, 60);
		}

		public void SetEnemyIndex(int index) => _enemyIndex = index;

		public void SetSelectable(bool selectable)
		{
			_isSelectable = selectable;
			MouseFilter = selectable ? MouseFilterEnum.Stop : MouseFilterEnum.Ignore;
			if (_body != null)
			{
				var style = _body.GetThemeStylebox("panel") as StyleBoxFlat;
				if (style != null)
				{
					style.BorderColor = selectable
						? new Color(1f, 0.8f, 0.2f, 1f)
						: new Color(0.65f, 0.22f, 0.22f, 0.88f);
					style.BorderWidthLeft = selectable ? 3 : 2;
					style.BorderWidthRight = selectable ? 3 : 2;
					style.BorderWidthTop = selectable ? 3 : 2;
					style.BorderWidthBottom = selectable ? 3 : 2;
				}
			}
		}

		public override void _Ready()
		{
			MouseFilter = MouseFilterEnum.Ignore;

			_body = new PanelContainer
			{
				CustomMinimumSize = new Vector2(130, 55),
				MouseFilter = MouseFilterEnum.Ignore
			};

			var bodyStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.1f, 0.08f, 0.07f, 0.92f),
				CornerRadiusTopLeft = 8,
				CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8,
				CornerRadiusBottomRight = 8,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = new Color(0.65f, 0.22f, 0.22f, 0.88f)
			};
			_body.AddThemeStyleboxOverride("panel", bodyStyle);
			AddChild(_body);

			var vbox = new VBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore,
				AnchorsPreset = (int)Control.LayoutPreset.FullRect
			};
			vbox.AddThemeConstantOverride("separation", 1);
			_body.AddChild(vbox);

			var topRow = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
			vbox.AddChild(topRow);

			_healthText = new Label
			{
				Text = $"{_currentHp}/{_maxHp}",
				Modulate = Colors.LightGray,
				MouseFilter = MouseFilterEnum.Ignore
			};
			_healthText.AddThemeFontSizeOverride("font_size", 10);
			topRow.AddChild(_healthText);

			_intentLabel = new Label
			{
				Text = "",
				HorizontalAlignment = HorizontalAlignment.Right,
				Modulate = new Color(1f, 0.78f, 0.18f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_intentLabel.AddThemeFontSizeOverride("font_size", 10);
			topRow.AddChild(_intentLabel);

			_healthBar = new ProgressBar
			{
				MaxValue = _maxHp,
				Value = _currentHp,
				CustomMinimumSize = new Vector2(120, 14),
				MouseFilter = MouseFilterEnum.Ignore
			};
			var healthStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.8f, 0.2f, 0.2f),
				CornerRadiusTopLeft = 3,
				CornerRadiusTopRight = 3,
				CornerRadiusBottomLeft = 3,
				CornerRadiusBottomRight = 3
			};
			_healthBar.AddThemeStyleboxOverride("fill", healthStyle);
			vbox.AddChild(_healthBar);

			GuiInput += OnEnemyGuiInput;
		}

		private void OnEnemyGuiInput(InputEvent @event)
		{
			if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left && _isSelectable)
			{
				EnemyClicked?.Invoke(_enemyIndex);
			}
		}

		public void UpdateHealth(int current, int max)
		{
			_currentHp = current;
			_maxHp = max;
			_healthBar.MaxValue = max;
			_healthBar.Value = current;
			_healthText.Text = $"{current}/{max}";

			if ((float)current / max <= 0.3f)
			{
				var lowHealthStyle = new StyleBoxFlat
				{
					BgColor = new Color(0.55f, 0.12f, 0.12f),
					CornerRadiusTopLeft = 3,
					CornerRadiusTopRight = 3,
					CornerRadiusBottomLeft = 3,
					CornerRadiusBottomRight = 3
				};
				_healthBar.AddThemeStyleboxOverride("fill", lowHealthStyle);
			}
		}

		public void UpdateIntent(string text, string icon = "")
		{
			_intentLabel.Text = string.IsNullOrEmpty(icon) ? text : $"{icon} {text}";
		}
	}

	public partial class CardUI : Control
	{
		public event System.Action<CardUI> CardPressed;
		public int HandIndex { get; }
		public CardData CardData { get; private set; }

		private PanelContainer _cardBody;
		private Label _nameLabel;
		private Label _costLabel;
		private Label _descLabel;
		private TextureRect _iconRect;
		private Vector2 _basePosition;
		private bool _isHovered = false;

		public CardUI(int handIndex)
		{
			HandIndex = handIndex;
		}

		public override void _Ready()
		{
			MouseFilter = MouseFilterEnum.Stop;
			CustomMinimumSize = new Vector2(105, 130);
			_basePosition = Position;

			_cardBody = new PanelContainer
			{
				CustomMinimumSize = new Vector2(100, 125),
				MouseFilter = MouseFilterEnum.Ignore
			};
			var style = CreateCardStyle();
			_cardBody.AddThemeStyleboxOverride("panel", style);
			AddChild(_cardBody);

			var mainVBox = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
			_cardBody.AddChild(mainVBox);

			var headerHBox = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
			mainVBox.AddChild(headerHBox);

			_nameLabel = new Label
			{
				Text = "卡牌",
				HorizontalAlignment = HorizontalAlignment.Center,
				Modulate = Colors.White,
				MouseFilter = MouseFilterEnum.Ignore
			};
			_nameLabel.AddThemeFontSizeOverride("font_size", 11);
			headerHBox.AddChild(_nameLabel);

			_costLabel = new Label
			{
				Text = "0",
				Modulate = new Color(1f, 0.85f, 0.2f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_costLabel.AddThemeFontSizeOverride("font_size", 12);
			headerHBox.AddChild(_costLabel);

			_iconRect = new TextureRect
			{
				CustomMinimumSize = new Vector2(0, 40),
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				MouseFilter = MouseFilterEnum.Ignore
			};
			mainVBox.AddChild(_iconRect);

			_descLabel = new Label
			{
				Text = "",
				HorizontalAlignment = HorizontalAlignment.Center,
				AutowrapMode = TextServer.AutowrapMode.WordSmart,
				MouseFilter = MouseFilterEnum.Ignore,
				CustomMinimumSize = new Vector2(90, 32)
			};
			_descLabel.AddThemeFontSizeOverride("font_size", 8);
			mainVBox.AddChild(_descLabel);

			var typeLabel = new Label
			{
				Text = "",
				HorizontalAlignment = HorizontalAlignment.Center,
				Modulate = Colors.Gray,
				MouseFilter = MouseFilterEnum.Ignore
			};
			typeLabel.Name = "TypeLabel";
			typeLabel.AddThemeFontSizeOverride("font_size", 8);
			mainVBox.AddChild(typeLabel);

			MouseEntered += OnMouseEnter;
			MouseExited += OnMouseExit;
			GuiInput += OnGuiInput;
		}

		private StyleBoxFlat CreateCardStyle()
		{
			return new StyleBoxFlat
			{
				BgColor = new Color(0.15f, 0.12f, 0.1f, 0.98f),
				CornerRadiusTopLeft = 7,
				CornerRadiusTopRight = 7,
				CornerRadiusBottomLeft = 7,
				CornerRadiusBottomRight = 7,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = new Color(0.55f, 0.42f, 0.25f, 0.9f)
			};
		}

		public void SetCardData(CardData data)
		{
			CardData = data;
			_nameLabel.Text = data.Name;
			_costLabel.Text = data.Cost.ToString();
			_descLabel.Text = data.Description;

			var typeLabel = GetNodeOrNull<Label>("TypeLabel");
			if (typeLabel != null)
			{
				typeLabel.Text = data.Type switch
				{
					CardType.Attack => "⚔️ 攻击",
					CardType.Skill => "🛡️ 技能",
					CardType.Power => "✨ 能力",
					_ => data.Type.ToString()
				};
				typeLabel.Modulate = data.Type switch
				{
					CardType.Attack => new Color(1f, 0.35f, 0.35f),
					CardType.Skill => new Color(0.35f, 0.6f, 1f),
					CardType.Power => new Color(0.9f, 0.75f, 0.3f),
					_ => Colors.Gray
				};
			}

			string iconPath = !string.IsNullOrEmpty(data.IconPath) ? data.IconPath : GetCardIcon(data.Type);
			var iconTex = GD.Load<Texture2D>(iconPath);
			if (iconTex != null)
				_iconRect.Texture = iconTex;

			_costLabel.Modulate = data.Cost == 0 ? new Color(0.4f, 0.85f, 0.4f) : new Color(1f, 0.85f, 0.2f);

			var style = CreateCardStyle();
			style.BorderColor = data.Type switch
			{
				CardType.Attack => new Color(0.75f, 0.25f, 0.25f, 0.95f),
				CardType.Skill => new Color(0.25f, 0.45f, 0.8f, 0.95f),
				CardType.Power => new Color(0.8f, 0.65f, 0.2f, 0.95f),
				_ => style.BorderColor
			};
			_cardBody.AddThemeStyleboxOverride("panel", style);
		}

		private string GetCardIcon(CardType type) => type switch
		{
			CardType.Attack => "res://Icons/Cards/strike.png",
			CardType.Skill => "res://Icons/Cards/defend.png",
			CardType.Power => "res://Icons/Skills/fireball.png",
			_ => "res://Icons/Cards/card_0.png"
		};

		private void OnMouseEnter()
		{
			_isHovered = true;
			var tween = CreateTween();
			tween.TweenProperty(this, "position:y", _basePosition.Y - 20, 0.1f).SetEase(Tween.EaseType.Out);
		}

		private void OnMouseExit()
		{
			_isHovered = false;
			var tween = CreateTween();
			tween.TweenProperty(this, "position:y", _basePosition.Y, 0.1f).SetEase(Tween.EaseType.Out);
		}

		private void OnGuiInput(InputEvent @event)
		{
			if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
			{
				CardPressed?.Invoke(this);
			}
		}
	}

	public static class FloatingText
	{
		public static void ShowDamage(Control parent, int amount, Vector2 position, bool isPlayer)
		{
			var color = isPlayer ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 0.85f, 0.2f);
			var text = new FloatingTextLabel($"-{amount}", position, color, 1.8f);
			parent.AddChild(text);
		}

		public static void ShowBlock(Control parent, int amount, Vector2 position)
		{
			var text = new FloatingTextLabel($"+{amount} 🛡️", position, new Color(0.35f, 0.6f, 1f), 1.8f);
			parent.AddChild(text);
		}

		public static void ShowHeal(Control parent, int amount, Vector2 position)
		{
			var text = new FloatingTextLabel($"+{amount} ❤️", position, new Color(0.35f, 1f, 0.4f), 1.8f);
			parent.AddChild(text);
		}

		public static void ShowStatus(Control parent, string text, Vector2 position)
		{
			var label = new FloatingTextLabel(text, position, new Color(0.9f, 0.7f, 0.3f), 2.2f);
			parent.AddChild(label);
		}
	}

	public partial class FloatingTextLabel : Control
	{
		private Label _label;
		private Vector2 _startPos;

		public FloatingTextLabel(string text, Vector2 position, Color color, float lifetime = 2f)
		{
			_startPos = position;
			MouseFilter = MouseFilterEnum.Ignore;

			_label = new Label
			{
				Text = text,
				HorizontalAlignment = HorizontalAlignment.Center,
				Modulate = color,
				MouseFilter = MouseFilterEnum.Ignore
			};
			_label.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.75f));
			_label.AddThemeConstantOverride("shadow_offset_x", 2);
			_label.AddThemeConstantOverride("shadow_offset_y", 2);
			_label.AddThemeFontSizeOverride("font_size", 22);
		}

		public override void _Ready()
		{
			Position = _startPos;
			AddChild(_label);

			var tween = CreateTween();
			tween.SetParallel(true);
			tween.TweenProperty(this, "position:y", _startPos.Y - 70, 0.9f).SetEase(Tween.EaseType.Out);
			tween.TweenProperty(this, "modulate:a", 0f, 1.3f).SetDelay(0.4f);

			GetTree().CreateTimer(1.8f).Timeout += QueueFree;
		}
	}
}