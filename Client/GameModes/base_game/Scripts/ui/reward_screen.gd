class_name RewardScreen extends Control

signal card_selected(card_data)
signal relic_selected(relic_data)
signal skipped

var _main_container: VBoxContainer
var _title_label: Label
var _gold_row: HBoxContainer
var _card_rewards: VBoxContainer
var _relic_rewards: VBoxContainer
var _skip_button: Button

var _cards: Array = []
var _relics: Array = []
var _gold: int = 0

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_ui()

func _create_ui() -> void:
	_main_container = VBoxContainer.new()
	_main_container.set_anchors_preset(Control.PRESET_CENTER)
	_main_container.custom_minimum_size = Vector2(800, 600)
	_main_container.mouse_filter = Control.MOUSE_FILTER_IGNORE

	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.08, 0.06, 0.1, 0.95)
	style.corner_radius_top_left = 12
	style.corner_radius_top_right = 12
	style.corner_radius_bottom_left = 12
	style.corner_radius_bottom_right = 12
	style.border_width_left = 3
	style.border_width_right = 3
	style.border_width_top = 3
	style.border_width_bottom = 3
	style.border_color = Color(1, 0.85, 0.3, 0.6)
	_main_container.add_theme_stylebox_override("panel", style)
	add_child(_main_container)

	_title_label = Label.new()
	_title_label.text = "🎉 战斗胜利！"
	_title_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_title_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_title_label.add_theme_font_size_override("font_size", 28)
	_title_label.modulate = Color(1, 0.85, 0.3)
	_main_container.add_child(_title_label)

	var spacer1 := Control.new()
	spacer1.custom_minimum_size = Vector2(0, 20)
	spacer1.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_main_container.add_child(spacer1)

	_gold_row = HBoxContainer.new()
	_gold_row.alignment = BoxContainer.ALIGNMENT_CENTER
	_gold_row.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_main_container.add_child(_gold_row)

	var gold_icon := Label.new()
	gold_icon.text = "💰"
	gold_icon.mouse_filter = Control.MOUSE_FILTER_IGNORE
	gold_icon.add_theme_font_size_override("font_size", 24)
	_gold_row.add_child(gold_icon)

	var gold_label := Label.new()
	gold_label.text = " +20 金币"
	gold_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	gold_label.add_theme_font_size_override("font_size", 20)
	gold_label.modulate = Color(1, 0.9, 0.4)
	_gold_row.add_child(gold_label)

	var spacer2 := Control.new()
	spacer2.custom_minimum_size = Vector2(0, 30)
	spacer2.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_main_container.add_child(spacer2)

	var card_lbl := Label.new()
	card_lbl.text = "选择一张卡牌添加到卡组："
	card_lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
	card_lbl.add_theme_font_size_override("font_size", 18)
	_main_container.add_child(card_lbl)

	_card_rewards = VBoxContainer.new()
	_card_rewards.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_main_container.add_child(_card_rewards)

	var spacer3 := Control.new()
	spacer3.custom_minimum_size = Vector2(0, 20)
	spacer3.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_main_container.add_child(spacer3)

	var relic_lbl := Label.new()
	relic_lbl.text = "遗物奖励："
	relic_lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
	relic_lbl.add_theme_font_size_override("font_size", 18)
	_main_container.add_child(relic_lbl)

	_relic_rewards = VBoxContainer.new()
	_relic_rewards.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_main_container.add_child(_relic_rewards)

	var spacer4 := Control.new()
	spacer4.custom_minimum_size = Vector2(0, 30)
	spacer4.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_main_container.add_child(spacer4)

	_skip_button = Button.new()
	_skip_button.text = "跳过奖励"
	_skip_button.custom_minimum_size = Vector2(200, 45)
	_skip_button.pressed.connect(func(): skipped.emit())
	_main_container.add_child(_skip_button)

func setup_rewards(cards: Array, relics: Array, gold: int) -> void:
	_cards = cards
	_relics = relics
	_gold = gold

	_update_gold_display()
	_update_card_display()
	_update_relic_display()

func _update_gold_display() -> void:
	if _gold_row.get_child_count() > 1:
		var gold_label = _gold_row.get_child(1)
		if gold_label is Label:
			gold_label.text = " +%d 金币" % _gold

func _update_card_display() -> void:
	for child in _card_rewards.get_children():
		child.queue_free()

	for card in _cards:
		var card_btn := _create_card_button(card)
		_card_rewards.add_child(card_btn)

func _create_card_button(card) -> Button:
	var btn := Button.new()
	btn.custom_minimum_size = Vector2(600, 80)
	btn.text = "%s (%d费) - %s" % [card.name, card.cost, card.description]

	var rarity_color := Color(0.9, 0.9, 0.9)
	if card.has("rarity"):
		match card.rarity:
			StsCardDatabase.CardRarity.RARE: rarity_color = Color(1, 0.6, 0.2)
			StsCardDatabase.CardRarity.UNCOMMON: rarity_color = Color(0.3, 0.7, 1)
			CardDatabase.CardRarity.RARE: rarity_color = Color(1, 0.6, 0.2)
			CardDatabase.CardRarity.UNCOMMON: rarity_color = Color(0.3, 0.7, 1)

	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.15, 0.12, 0.1)
	style.corner_radius_top_left = 8
	style.corner_radius_top_right = 8
	style.corner_radius_bottom_left = 8
	style.corner_radius_bottom_right = 8
	style.border_width_left = 2
	style.border_width_right = 2
	style.border_width_top = 2
	style.border_width_bottom = 2
	style.border_color = rarity_color
	btn.add_theme_stylebox_override("normal", style)

	btn.pressed.connect(func():
		card_selected.emit(card)
		queue_free()
	)

	return btn

func _update_relic_display() -> void:
	for child in _relic_rewards.get_children():
		child.queue_free()

	if _relics.is_empty():
		var no_relic_label := Label.new()
		no_relic_label.text = "无遗物奖励"
		no_relic_label.modulate = Color.GRAY
		no_relic_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_relic_rewards.add_child(no_relic_label)
		return

	for relic in _relics:
		var relic_btn := _create_relic_button(relic)
		_relic_rewards.add_child(relic_btn)

func _create_relic_button(relic) -> Button:
	var btn := Button.new()
	btn.custom_minimum_size = Vector2(600, 60)
	btn.text = "💎 %s - %s" % [relic.name, relic.description]

	var rarity_color := Color(0.9, 0.9, 0.9)
	if relic.has("rarity"):
		match relic.rarity:
			StsRelicSystem.RelicRarity.RARE: rarity_color = Color(1, 0.6, 0.2)
			StsRelicSystem.RelicRarity.UNCOMMON: rarity_color = Color(0.3, 0.7, 1)
			RelicDatabase.RelicTier.RARE: rarity_color = Color(1, 0.6, 0.2)
			RelicDatabase.RelicTier.UNCOMMON: rarity_color = Color(0.3, 0.7, 1)

	btn.modulate = rarity_color

	btn.pressed.connect(func():
		relic_selected.emit(relic)
		queue_free()
	)

	return btn

static func show_rewards(parent: Control, cards: Array, relics: Array, gold: int) -> RewardScreen:
	var screen := RewardScreen.new()
	screen.setup_rewards(cards, relics, gold)
	parent.add_child(screen)
	return screen


class ShopScreen:
	extends Control

signal card_purchased(card_data, price: int)
signal relic_purchased(relic_data, price: int)
signal shop_closed

var _main_container: VBoxContainer
var _gold_bar: HBoxContainer
var _gold_label: Label
var _card_row: HBoxContainer
var _relic_row: HBoxContainer
var _leave_button: Button
var _player_gold: int = 99

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_ui()

func _create_ui() -> void:
	_main_container = VBoxContainer.new()
	_main_container.set_anchors_preset(Control.PRESET_FULL_RECT)
	_main_container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_main_container)

	_gold_bar = HBoxContainer.new()
	_gold_bar.custom_minimum_size = Vector2(0, 50)
	_gold_bar.alignment = BoxContainer.ALIGNMENT_END
	_gold_bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_main_container.add_child(_gold_bar)

	var gold_icon := Label.new()
	gold_icon.text = "💰"
	gold_icon.mouse_filter = Control.MOUSE_FILTER_IGNORE
	gold_icon.add_theme_font_size_override("font_size", 24)
	_gold_bar.add_child(gold_icon)

	_gold_label = Label.new()
	_gold_label.text = " 99"
	_gold_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_gold_label.add_theme_font_size_override("font_size", 20)
	_gold_label.modulate = Color(1, 0.9, 0.4)
	_gold_bar.add_child(_gold_label)

	var title_lbl := Label.new()
	title_lbl.text = "🏪 商店"
	title_lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title_lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
	title_lbl.add_theme_font_size_override("font_size", 32)
	_main_container.add_child(title_lbl)

	var card_lbl := Label.new()
	card_lbl.text = "卡牌 (点击购买)"
	card_lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
	card_lbl.add_theme_font_size_override("font_size", 18)
	_main_container.add_child(card_lbl)

	_card_row = HBoxContainer.new()
	_card_row.custom_minimum_size = Vector2(0, 200)
	_card_row.alignment = BoxContainer.ALIGNMENT_CENTER
	_card_row.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_main_container.add_child(_card_row)

	var relic_lbl := Label.new()
	relic_lbl.text = "遗物"
	relic_lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
	relic_lbl.add_theme_font_size_override("font_size", 18)
	_main_container.add_child(relic_lbl)

	_relic_row = HBoxContainer.new()
	_relic_row.custom_minimum_size = Vector2(0, 120)
	_relic_row.alignment = BoxContainer.ALIGNMENT_CENTER
	_relic_row.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_main_container.add_child(_relic_row)

	_leave_button = Button.new()
	_leave_button.text = "离开商店"
	_leave_button.custom_minimum_size = Vector2(200, 50)
	_leave_button.pressed.connect(func(): shop_closed.emit())
	_main_container.add_child(_leave_button)

func setup_shop(player_gold: int) -> void:
	_player_gold = player_gold
	_update_gold_display()
	_generate_shop_items()

func _update_gold_display() -> void:
	_gold_label.text = " %d" % _player_gold

func _generate_shop_items() -> void:
	var rng := RandomNumberGenerator.new()
	rng.randomize()

	var cards := []
	var card_db_cards := CardDatabase.get_all_cards()
	if card_db_cards.is_empty():
		card_db_cards = StsCardDatabase.new().get_all_cards()
	cards = card_db_cards.duplicate()
	cards.shuffle()
	var result_cards := []
	for i in range(mini(5, cards.size())):
		result_cards.append(cards[i])
	for card in result_cards:
		var price := 50
		if card.has("rarity"):
			match card.rarity:
			StsCardDatabase.CardRarity.RARE: price = 150
			StsCardDatabase.CardRarity.UNCOMMON: price = 100
			CardDatabase.CardRarity.RARE: price = 150
			CardDatabase.CardRarity.UNCOMMON: price = 100
		var card_item := _create_shop_card(card, price)
		_card_row.add_child(card_item)

	var relics := []
	var relic_db_relics := RelicDatabase.get_all_relics()
	relics = relic_db_relics.duplicate()
	relics.shuffle()
	var result_relics := []
	for i in range(mini(3, relics.size())):
		result_relics.append(relics[i])
	for relic in result_relics:
		var price := 100
		if relic.has("rarity"):
			match relic.rarity:
			StsRelicSystem.RelicRarity.RARE: price = 200
			StsRelicSystem.RelicRarity.UNCOMMON: price = 150
			RelicDatabase.RelicTier.RARE: price = 200
			RelicDatabase.RelicTier.UNCOMMON: price = 150
		var relic_item := _create_shop_relic(relic, price)
		_relic_row.add_child(relic_item)

func _create_shop_card(card, price: int) -> VBoxContainer:
	var container := VBoxContainer.new()
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE

	var btn := Button.new()
	btn.custom_minimum_size = Vector2(140, 180)
	btn.text = "%s\n%d费\n%s" % [card.name, card.cost, card.description]

	btn.pressed.connect(func():
		if _player_gold >= price:
			_player_gold -= price
			_update_gold_display()
			card_purchased.emit(card, price)
			btn.disabled = true
			btn.modulate = Color(0.5, 0.5, 0.5)
	)

	container.add_child(btn)

	var price_label := Label.new()
	price_label.text = "💰 %d" % price
	price_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	price_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	price_label.modulate = Color(1, 0.9, 0.4)
	container.add_child(price_label)

	return container

func _create_shop_relic(relic, price: int) -> VBoxContainer:
	var container := VBoxContainer.new()
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE

	var btn := Button.new()
	btn.custom_minimum_size = Vector2(140, 80)
	btn.text = "💎 %s\n%s" % [relic.name, relic.description]

	btn.pressed.connect(func():
		if _player_gold >= price:
			_player_gold -= price
			_update_gold_display()
			relic_purchased.emit(relic, price)
			btn.disabled = true
			btn.modulate = Color(0.5, 0.5, 0.5)
	)

	container.add_child(btn)

	var price_label := Label.new()
	price_label.text = "💰 %d" % price
	price_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	price_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	price_label.modulate = Color(1, 0.9, 0.4)
	container.add_child(price_label)

	return container

static func show_shop(parent: Control, player_gold: int) -> ShopScreen:
	var screen := ShopScreen.new()
	screen.setup_shop(player_gold)
	parent.add_child(screen)
	return screen
