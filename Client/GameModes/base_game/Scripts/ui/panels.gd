class_name VictoryScreen extends Control

signal closed

var _run_data: Dictionary = {}

func _init(run_data: Dictionary = {}) -> void:
	_run_data = run_data

func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)
	mouse_filter = Control.MOUSE_FILTER_STOP

	var bg := ColorRect.new()
	bg.set_anchors_preset(Control.PRESET_FULL_RECT)
	bg.color = Color(0, 0, 0, 0.9)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)

	var main_panel := PanelContainer.new()
	main_panel.custom_minimum_size = Vector2(500, 400)
	main_panel.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	main_panel.size_flags_vertical = Control.SIZE_SHRINK_CENTER
	main_panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	main_panel.set_anchors_preset(Control.PRESET_CENTER)

	var panel_style := StyleBoxFlat.new()
	panel_style.bg_color = Color(0.08, 0.12, 0.08, 0.98)
	panel_style.corner_radius_top_left = 15
	panel_style.corner_radius_top_right = 15
	panel_style.corner_radius_bottom_left = 15
	panel_style.corner_radius_bottom_right = 15
	panel_style.border_width_left = 3
	panel_style.border_width_right = 3
	panel_style.border_width_top = 3
	panel_style.border_width_bottom = 3
	panel_style.border_color = Color(0.2, 0.8, 0.3, 0.9)
	panel_style.content_margin_top = 25
	panel_style.content_margin_bottom = 25
	panel_style.content_margin_left = 30
	panel_style.content_margin_right = 30
	main_panel.add_theme_stylebox_override("panel", panel_style)
	add_child(main_panel)

	var vbox := VBoxContainer.new()
	vbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.alignment = BoxContainer.ALIGNMENT_CENTER
	vbox.add_theme_constant_override("separation", 12)
	main_panel.add_child(vbox)

	var title_label := Label.new()
	title_label.text = "🏆 通关成功!"
	title_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title_label.modulate = Color(1, 0.9, 0.3)
	title_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	title_label.add_theme_font_size_override("font_size", 28)
	vbox.add_child(title_label)

	var stats_label := Label.new()
	stats_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	stats_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	stats_label.add_theme_font_size_override("font_size", 16)

	if not _run_data.is_empty():
		stats_label.text = "成功登顶!\n击败 %d 个敌人\n收集 %d 金币\n用时: 00:00" % [_run_data.get("enemies_defeated", 0), _run_data.get("gold", 0)]
	else:
		stats_label.text = "成功登顶!"
	vbox.add_child(stats_label)

	var spacer := Control.new()
	spacer.custom_minimum_size = Vector2(0, 20)
	spacer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.add_child(spacer)

	var menu_btn := Button.new()
	menu_btn.text = "返回主菜单"
	menu_btn.custom_minimum_size = Vector2(200, 42)
	menu_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	menu_btn.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	menu_btn.pressed.connect(func():
		closed.emit()
		if Main.instance != null:
			Main.instance.go_to_main_menu()
	)
	vbox.add_child(menu_btn)


class_name PileViewPanel extends Control

signal closed

var _title: String = ""
var _cards: Array = []

func _init(p_title: String = "", p_cards: Array = []) -> void:
	_title = p_title
	_cards = p_cards

func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)
	mouse_filter = Control.MOUSE_FILTER_STOP

	var bg := ColorRect.new()
	bg.set_anchors_preset(Control.PRESET_FULL_RECT)
	bg.color = Color(0, 0, 0, 0.8)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)

	var main_panel := PanelContainer.new()
	main_panel.custom_minimum_size = Vector2(900, 550)
	main_panel.position = Vector2(190, 85)
	main_panel.mouse_filter = Control.MOUSE_FILTER_IGNORE

	var panel_style := StyleBoxFlat.new()
	panel_style.bg_color = Color(0.1, 0.08, 0.12, 0.98)
	panel_style.corner_radius_top_left = 15
	panel_style.corner_radius_top_right = 15
	panel_style.corner_radius_bottom_left = 15
	panel_style.corner_radius_bottom_right = 15
	panel_style.border_width_left = 2
	panel_style.border_width_right = 2
	panel_style.border_width_top = 2
	panel_style.border_width_bottom = 2
	panel_style.border_color = Color(0.5, 0.4, 0.3, 0.9)
	main_panel.add_theme_stylebox_override("panel", panel_style)
	add_child(main_panel)

	var vbox := VBoxContainer.new()
	vbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.set_anchors_preset(Control.PRESET_FULL_RECT)
	vbox.add_theme_constant_override("separation", 8)
	main_panel.add_child(vbox)

	var title_label := Label.new()
	title_label.text = "%s (%d张)" % [_title, _cards.size()]
	title_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title_label.modulate = Color(0.9, 0.8, 0.6)
	title_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	title_label.add_theme_font_size_override("font_size", 18)
	vbox.add_child(title_label)

	var scroll_container := ScrollContainer.new()
	scroll_container.custom_minimum_size = Vector2(860, 420)
	scroll_container.mouse_filter = Control.MOUSE_FILTER_STOP
	scroll_container.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
	vbox.add_child(scroll_container)

	var grid_container := GridContainer.new()
	grid_container.columns = 5
	grid_container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	grid_container.add_theme_constant_override("h_separation", 8)
	grid_container.add_theme_constant_override("v_separation", 8)
	scroll_container.add_child(grid_container)

	for card in _cards:
		var card_panel := _create_card_panel(card)
		grid_container.add_child(card_panel)

	if _cards.is_empty():
		var empty_label := Label.new()
		empty_label.text = "空"
		empty_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		empty_label.modulate = Color.GRAY
		empty_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		empty_label.add_theme_font_size_override("font_size", 16)
		vbox.add_child(empty_label)

	var close_btn := Button.new()
	close_btn.text = "关闭"
	close_btn.custom_minimum_size = Vector2(150, 36)
	close_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	close_btn.pressed.connect(func(): closed.emit())
	vbox.add_child(close_btn)

func _create_card_panel(card) -> PanelContainer:
	var card_panel := PanelContainer.new()
	card_panel.custom_minimum_size = Vector2(155, 90)
	card_panel.mouse_filter = Control.MOUSE_FILTER_IGNORE

	var card_type: int = card.get("type", CardDatabase.CardType.ATTACK)
	var border_color := Color(0.5, 0.4, 0.3, 0.9)
	match card_type:
		CardDatabase.CardType.ATTACK: border_color = Color(0.75, 0.25, 0.25, 0.9)
		CardDatabase.CardType.SKILL: border_color = Color(0.25, 0.45, 0.8, 0.9)
		CardDatabase.CardType.POWER: border_color = Color(0.8, 0.65, 0.2, 0.9)

	var card_style := StyleBoxFlat.new()
	card_style.bg_color = Color(0.12, 0.1, 0.08, 0.98)
	card_style.corner_radius_top_left = 8
	card_style.corner_radius_top_right = 8
	card_style.corner_radius_bottom_left = 8
	card_style.corner_radius_bottom_right = 8
	card_style.border_width_left = 2
	card_style.border_width_right = 2
	card_style.border_width_top = 2
	card_style.border_width_bottom = 2
	card_style.border_color = border_color
	card_panel.add_theme_stylebox_override("panel", card_style)

	var card_vbox := VBoxContainer.new()
	card_vbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	card_vbox.set_anchors_preset(Control.PRESET_FULL_RECT)
	card_vbox.add_theme_constant_override("separation", 2)
	card_panel.add_child(card_vbox)

	var name_row := HBoxContainer.new()
	name_row.mouse_filter = Control.MOUSE_FILTER_IGNORE
	card_vbox.add_child(name_row)

	var name_lbl := Label.new()
	name_lbl.text = card.get("name", "")
	name_lbl.modulate = Color.WHITE
	name_lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
	name_lbl.add_theme_font_size_override("font_size", 11)
	name_row.add_child(name_lbl)

	var cost_lbl := Label.new()
	cost_lbl.text = str(card.get("cost", 1))
	cost_lbl.modulate = Color(1, 0.85, 0.2)
	cost_lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cost_lbl.add_theme_font_size_override("font_size", 11)
	name_row.add_child(cost_lbl)

	var desc_lbl := Label.new()
	desc_lbl.text = card.get("description", "")
	desc_lbl.modulate = Color(0.7, 0.7, 0.7)
	desc_lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
	desc_lbl.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	desc_lbl.custom_minimum_size = Vector2(140, 30)
	desc_lbl.add_theme_font_size_override("font_size", 9)
	card_vbox.add_child(desc_lbl)

	return card_panel


class_name TutorialOverlay extends Control

var _content_label: RichTextLabel
var _next_button: Button
var _pages: Array = []
var _current_page: int = 0

func _ready() -> void:
	_content_label = get_node_or_null("Panel/VBox/ContentLabel")
	_next_button = get_node_or_null("Panel/VBox/NextButton")
	var skip_btn = get_node_or_null("Panel/VBox/SkipButton")

	if _next_button != null:
		_next_button.pressed.connect(_on_next_pressed)
	if skip_btn != null:
		skip_btn.pressed.connect(hide)

	_initialize_tutorial_pages()

func _initialize_tutorial_pages() -> void:
	_pages.clear()
	_pages.append("[b]欢迎来到杀戮尖塔![/b]\n\n这是一款卡牌构建 roguelike 游戏。\n\n你的目标是攀登尖塔，击败沿途的敌人。")
	_pages.append("[b]卡牌战斗[/b]\n\n每回合你会抽取手牌，消耗能量打出卡牌。\n\n攻击卡造成伤害，技能卡提供各种效果。")
	_pages.append("[b]地图导航[/b]\n\n选择你的路线，遭遇敌人、事件、商店和休息点。\n\n精英敌人更强但奖励更丰厚！")
	_pages.append("[b]遗物与卡牌[/b]\n\n击败Boss获得遗物，提供永久增益。\n\n谨慎选择添加到牌组的卡牌！")

	show_page(0)

func show_tutorial() -> void:
	_current_page = 0
	show_page(0)
	show()

func show_page(index: int) -> void:
	if index >= 0 and index < _pages.size() and _content_label != null:
		_content_label.text = _pages[index]
		if _next_button != null:
			_next_button.text = "完成" if index == _pages.size() - 1 else "下一步"

func _on_next_pressed() -> void:
	_current_page += 1
	if _current_page >= _pages.size():
		hide()
	else:
		show_page(_current_page)


class_name AchievementPanel extends Control

signal closed

func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)
	mouse_filter = Control.MOUSE_FILTER_STOP

	var bg := ColorRect.new()
	bg.set_anchors_preset(Control.PRESET_FULL_RECT)
	bg.color = Color(0, 0, 0, 0.9)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)

	var main_panel := PanelContainer.new()
	main_panel.custom_minimum_size = Vector2(700, 550)
	main_panel.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	main_panel.size_flags_vertical = Control.SIZE_SHRINK_CENTER
	main_panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	main_panel.set_anchors_preset(Control.PRESET_CENTER)

	var panel_style := StyleBoxFlat.new()
	panel_style.bg_color = Color(0.08, 0.06, 0.1, 0.98)
	panel_style.corner_radius_top_left = 15
	panel_style.corner_radius_top_right = 15
	panel_style.corner_radius_bottom_left = 15
	panel_style.corner_radius_bottom_right = 15
	panel_style.border_width_left = 3
	panel_style.border_width_right = 3
	panel_style.border_width_top = 3
	panel_style.border_width_bottom = 3
	panel_style.border_color = Color(0.7, 0.6, 0.9, 0.9)
	panel_style.content_margin_top = 25
	panel_style.content_margin_bottom = 25
	panel_style.content_margin_left = 30
	panel_style.content_margin_right = 30
	main_panel.add_theme_stylebox_override("panel", panel_style)
	add_child(main_panel)

	var vbox := VBoxContainer.new()
	vbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.alignment = BoxContainer.ALIGNMENT_CENTER
	vbox.add_theme_constant_override("separation", 10)
	main_panel.add_child(vbox)

	var title_label := Label.new()
	title_label.text = "🏆 成就"
	title_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title_label.modulate = Color(0.9, 0.85, 1)
	title_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	title_label.add_theme_font_size_override("font_size", 24)
	vbox.add_child(title_label)

	var scroll_container := ScrollContainer.new()
	scroll_container.custom_minimum_size = Vector2(640, 400)
	scroll_container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.add_child(scroll_container)

	var content_vbox := VBoxContainer.new()
	content_vbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	content_vbox.add_theme_constant_override("separation", 8)
	scroll_container.add_child(content_vbox)

	var categories := ["⚔️ 战斗", "🗺️ 探索", "📦 收集", "🎯 挑战", "⭐ 通用"]
	for cat_name in categories:
		var cat_section := _create_category_section(cat_name)
		if cat_section != null:
			content_vbox.add_child(cat_section)

	var spacer := Control.new()
	spacer.custom_minimum_size = Vector2(0, 15)
	spacer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.add_child(spacer)

	var close_btn := Button.new()
	close_btn.text = "关闭"
	close_btn.custom_minimum_size = Vector2(180, 42)
	close_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	close_btn.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	close_btn.pressed.connect(func(): closed.emit())
	vbox.add_child(close_btn)

func _create_category_section(category_name: String) -> VBoxContainer:
	var section := VBoxContainer.new()
	section.mouse_filter = Control.MOUSE_FILTER_IGNORE
	section.add_theme_constant_override("separation", 6)

	var cat_label := Label.new()
	cat_label.text = category_name
	cat_label.modulate = Color(0.7, 0.8, 0.95)
	cat_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cat_label.add_theme_font_size_override("font_size", 16)
	section.add_child(cat_label)

	var separator := HSeparator.new()
	separator.custom_minimum_size = Vector2(0, 12)
	separator.modulate = Color(0.3, 0.3, 0.4)
	separator.mouse_filter = Control.MOUSE_FILTER_IGNORE
	section.add_child(separator)

	return section


class_name RestSitePanel extends Control

signal rest_selected(option: int)
signal skipped

var _main_container: VBoxContainer
var _options: Array = []

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_ui()

func _create_ui() -> void:
	_main_container = VBoxContainer.new()
	_main_container.set_anchors_preset(Control.PRESET_CENTER)
	_main_container.custom_minimum_size = Vector2(600, 450)
	_main_container.mouse_filter = Control.MOUSE_FILTER_IGNORE

	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.06, 0.1, 0.08, 0.97)
	style.corner_radius_top_left = 15
	style.corner_radius_top_right = 15
	style.corner_radius_bottom_left = 15
	style.corner_radius_bottom_right = 15
	style.border_width_left = 3
	style.border_width_right = 3
	style.border_width_top = 3
	style.border_width_bottom = 3
	style.border_color = Color(0.35, 0.6, 0.4, 0.8)
	_main_container.add_theme_stylebox_override("panel", style)
	add_child(_main_container)

	var title := Label.new()
	title.text = "🏕️ 营火"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	title.add_theme_font_size_override("font_size", 28)
	title.modulate = Color(0.6, 0.9, 0.6)
	_main_container.add_child(title)

	var desc := Label.new()
	desc.text = "你发现了一处安全的营火地。"
	desc.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	desc.mouse_filter = Control.MOUSE_FILTER_IGNORE
	desc.add_theme_font_size_override("font_size", 16)
	_main_container.add_child(desc)

	var spacer := Control.new()
	spacer.custom_minimum_size = Vector2(0, 20)
	spacer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_main_container.add_child(spacer)

	_options = [
		{"text": "🛌 休息 (回复30%最大生命)", "option": 0},
		{"text": "⬆️ 升级一张卡牌", "option": 1},
		{"text": "💊 炼成一张药水 (需移除一张卡牌)", "option": 2},
		{"text": "🚫 跳过", "option": 3}
	]

	for opt in _options:
		var btn := Button.new()
		btn.text = opt["text"]
		btn.custom_minimum_size = Vector2(500, 50)
		btn.pressed.connect(func(): rest_selected.emit(opt["option"]))
		_main_container.add_child(btn)

	var skip_btn := Button.new()
	skip_btn.text = "跳过休息点"
	skip_btn.custom_minimum_size = Vector2(200, 40)
	skip_btn.pressed.connect(func(): skipped.emit())
	_main_container.add_child(skip_btn)


class_name RewardPanel extends Control

signal card_chosen(card_data)
signal relic_chosen(relic_data)
signal reward_skipped

var _card_options: Array = []
var _relic_options: Array = []
var _container: VBoxContainer

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_ui()

func _create_ui() -> void:
	_container = VBoxContainer.new()
	_container.set_anchors_preset(Control.PRESET_CENTER)
	_container.custom_minimum_size = Vector2(700, 500)
	_container.mouse_filter = Control.MOUSE_FILTER_IGNORE

	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.1, 0.08, 0.12, 0.97)
	style.corner_radius_top_left = 12
	style.corner_radius_top_right = 12
	style.corner_radius_bottom_left = 12
	style.corner_radius_bottom_right = 12
	style.border_width_left = 2
	style.border_width_right = 2
	style.border_width_top = 2
	style.border_width_bottom = 2
	style.border_color = Color(1, 0.85, 0.3, 0.7)
	_container.add_theme_stylebox_override("panel", style)
	add_child(_container)

	var title := Label.new()
	title.text = "🎁 奖励选择"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	title.add_theme_font_size_override("font_size", 24)
	title.modulate = Color(1, 0.85, 0.3)
	_container.add_child(title)

func setup_rewards(cards: Array, relics: Array) -> void:
	_card_options = cards
	_relic_options = relics

	for card in cards:
		var btn := Button.new()
		btn.text = "🃏 %s (%d费)" % [card.get("name", ""), card.get("cost", 0)]
		btn.custom_minimum_size = Vector2(550, 45)
		btn.pressed.connect(func(): card_chosen.emit(card))
		_container.add_child(btn)

	for relic in relics:
		var btn := Button.new()
		btn.text = "💎 %s" % relic.get("name", "")
		btn.custom_minimum_size = Vector2(550, 40)
		btn.pressed.connect(func(): relic_chosen.emit(relic))
		_container.add_child(btn)

	var skip_btn := Button.new()
	skip_btn.text = "跳过奖励"
	skip_btn.custom_minimum_size = Vector2(200, 40)
	skip_btn.pressed.connect(func(): reward_skipped.emit())
	_container.add_child(skip_btn)


class_name SaveSlotPanel extends Control

signal slot_selected(slot_id: int)
signal cancelled

var _slots: Array = []
var _container: VBoxContainer

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_ui()

func _create_ui() -> void:
	_container = VBoxContainer.new()
	_container.set_anchors_preset(Control.PRESET_CENTER)
	_container.custom_minimum_size = Vector2(500, 450)
	_container.mouse_filter = Control.MOUSE_FILTER_IGNORE

	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.08, 0.08, 0.12, 0.97)
	style.corner_radius_top_left = 12
	style.corner_radius_top_right = 12
	style.corner_radius_bottom_left = 12
	style.corner_radius_bottom_right = 12
	style.border_width_left = 2
	style.border_width_right = 2
	style.border_width_top = 2
	style.border_width_bottom = 2
	style.border_color = Color(0.5, 0.5, 0.7, 0.8)
	_container.add_theme_stylebox_override("panel", style)
	add_child(_container)

	var title := Label.new()
	title.text = "💾 存档管理"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	title.add_theme_font_size_override("font_size", 24)
	_container.add_child(title)

	for i in range(3):
		var slot_data := _get_slot_data(i)
		var btn := Button.new()
		btn.text = "存档 %d - %s" % [i + 1, slot_data]
		btn.custom_minimum_size = Vector2(400, 50)
		btn.pressed.connect(func(): slot_selected.emit(i))
		_container.add_child(btn)

	var cancel_btn := Button.new()
	cancel_btn.text = "取消"
	cancel_btn.custom_minimum_size = Vector2(200, 40)
	cancel_btn.pressed.connect(func(): cancelled.emit())
	_container.add_child(cancel_btn)

func _get_slot_data(slot_idx: int) -> String:
	match slot_idx:
		0: return "空"
		1: return "空"
		2: return "空"
		_: return "未知"


class_name ShopPanel extends Control

signal item_purchased(item_data, price: int)
signal shop_exit

var _player_gold: int = 99
var _items: Array = []
var _container: VBoxContainer

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_ui()

func _create_ui() -> void:
	_container = VBoxContainer.new()
	_container.set_anchors_preset(Control.PRESET_FULL_RECT)
	_container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_container)

	var header := Label.new()
	header.text = "🏪 商店"
	header.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	header.mouse_filter = Control.MOUSE_FILTER_IGNORE
	header.add_theme_font_size_override("font_size", 28)
	_container.add_child(header)

	var gold_bar := HBoxContainer.new()
	gold_bar.alignment = BoxContainer.ALIGNMENT_END
	gold_bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_container.add_child(gold_bar)

	var gold_icon := Label.new()
	gold_icon.text = "💰"
	gold_icon.mouse_filter = Control.MOUSE_FILTER_IGNORE
	gold_icon.add_theme_font_size_override("font_size", 22)
	gold_bar.add_child(gold_icon)

	var gold_label := Label.new()
	gold_label.name = "GoldLabel"
	gold_label.text = " %d" % _player_gold
	gold_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	gold_label.add_theme_font_size_override("font_size", 18)
	gold_label.modulate = Color(1, 0.9, 0.4)
	gold_bar.add_child(gold_label)

func setup_shop(player_gold: int, items: Array) -> void:
	_player_gold = player_gold
	_items = items
	
	var gold_label = _container.get_node_or_null("GoldLabel") if _container != null else null
	if gold_label != null:
		gold_label.text = " %d" % _player_gold

	for item in items:
		var price: int = item.get("price", 50)
		var btn := Button.new()
		btn.text = "%s - 💰%d" % [item.get("name", ""), price]
		btn.custom_minimum_size = Vector2(500, 45)
		btn.pressed.connect(func():
			if _player_gold >= price:
				_player_gold -= price
				item_purchased.emit(item, price)
				btn.disabled = true
				btn.modulate = Color(0.5, 0.5, 0.5)
				if gold_label != null:
					gold_label.text = " %d" % _player_gold
		)
		_container.add_child(btn)

	var exit_btn := Button.new()
	exit_btn.text = "离开商店"
	exit_btn.custom_minimum_size = Vector2(200, 42)
	exit_btn.pressed.connect(func(): shop_exit.emit())
	_container.add_child(exit_btn)


class_name TreasurePanel extends Control

signal treasure_taken(treasure_id: String)
signal treasure_skipped

var _treasures: Array = []
var _container: VBoxContainer

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_ui()

func _create_ui() -> void:
	_container = VBoxContainer.new()
	_container.set_anchors_preset(Control.PRESET_CENTER)
	_container.custom_minimum_size = Vector2(550, 400)
	_container.mouse_filter = Control.MOUSE_FILTER_IGNORE

	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.12, 0.1, 0.06, 0.97)
	style.corner_radius_top_left = 12
	style.corner_radius_top_right = 12
	style.corner_radius_bottom_left = 12
	style.corner_radius_bottom_right = 12
	style.border_width_left = 3
	style.border_width_right = 3
	style.border_width_top = 3
	style.border_width_bottom = 3
	style.border_color = Color(1, 0.88, 0.3, 0.8)
	_container.add_theme_stylebox_override("panel", style)
	add_child(_container)

	var title := Label.new()
	title.text = "🎁 宝箱"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	title.add_theme_font_size_override("font_size", 26)
	title.modulate = Color(1, 0.88, 0.3)
	_container.add_child(title)

func setup_treasures(treasures: Array) -> void:
	_treasures = treasures

	for treasure in treasures:
		var btn := Button.new()
		btn.text = "◆ %s" % treasure.get("name", "未知宝物")
		btn.custom_minimum_size = Vector2(480, 50)
		btn.pressed.connect(func(): treasure_taken.emit(treasure.get("id", "")))
		_container.add_child(btn)

	var skip_btn := Button.new()
	skip_btn.text = "离开"
	skip_btn.custom_minimum_size = Vector2(180, 38)
	skip_btn.pressed.connect(func(): treasure_skipped.emit())
	_container.add_child(skip_btn)


class_name GameOverScreen extends Control

signal retry_pressed
signal menu_pressed

var _is_victory: bool = false
var _score: int = 0
var _container: VBoxContainer

func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_ui()

func _create_ui() -> void:
	var bg := ColorRect.new()
	bg.set_anchors_preset(Control.PRESET_FULL_RECT)
	bg.color = Color(0, 0, 0, 0.92)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)

	_container = VBoxContainer.new()
	_container.set_anchors_preset(Control.PRESET_CENTER)
	_container.custom_minimum_size = Vector2(500, 420)
	_container.alignment = BoxContainer.ALIGNMENT_CENTER
	_container.mouse_filter = Control.MOUSE_FILTER_IGNORE

	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.1, 0.05, 0.08, 0.98)
	style.corner_radius_top_left = 15
	style.corner_radius_top_right = 15
	style.corner_radius_bottom_left = 15
	style.corner_radius_bottom_right = 15
	style.border_width_left = 3
	style.border_width_right = 3
	style.border_width_top = 3
	style.border_width_bottom = 3
	style.border_color = Color(0.8, 0.2, 0.2, 0.9)
	_container.add_theme_stylebox_override("panel", style)
	add_child(_container)

func setup_game_over(is_victory: bool, score: int = 0) -> void:
	_is_victory = is_victory
	_score = score

	for child in _container.get_children():
		child.queue_free()

	var title := Label.new()
	title.text = "💀 战斗失败!" if not is_victory else "🏆 胜利!"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	title.add_theme_font_size_override("font_size", 32)
	title.modulate = Color(0.9, 0.3, 0.3) if not is_victory else Color(1, 0.85, 0.3)
	_container.add_child(title)

	var score_label := Label.new()
	score_label.text = "得分: %d" % _score
	score_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	score_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	score_label.add_theme_font_size_override("font_size", 18)
	_container.add_child(score_label)

	var spacer := Control.new()
	spacer.custom_minimum_size = Vector2(0, 25)
	spacer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_container.add_child(spacer)

	var retry_btn := Button.new()
	retry_btn.text = "重新挑战"
	retry_btn.custom_minimum_size = Vector2(220, 48)
	retry_btn.pressed.connect(func(): retry_pressed.emit())
	_container.add_child(retry_btn)

	var menu_btn := Button.new()
	menu_btn.text = "返回主菜单"
	menu_btn.custom_minimum_size = Vector2(220, 48)
	menu_btn.pressed.connect(func(): menu_pressed.emit())
	_container.add_child(menu_btn)


class_name EventPanel extends Control

signal option_selected(option_index: int)
signal event_closed

var _event_data: Dictionary = {}
var _container: VBoxContainer

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_ui()

func _create_ui() -> void:
	_container = VBoxContainer.new()
	_container.set_anchors_preset(Control.PRESET_CENTER)
	_container.custom_minimum_size = Vector2(650, 480)
	_container.mouse_filter = Control.MOUSE_FILTER_IGNORE

	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.08, 0.1, 0.12, 0.97)
	style.corner_radius_top_left = 12
	style.corner_radius_top_right = 12
	style.corner_radius_bottom_left = 12
	style.corner_radius_bottom_right = 12
	style.border_width_left = 2
	style.border_width_right = 2
	style.border_width_top = 2
	style.border_width_bottom = 2
	style.border_color = Color(0.55, 0.8, 0.35, 0.8)
	_container.add_theme_stylebox_override("panel", style)
	add_child(_container)

func show_event(event_data: Dictionary) -> void:
	_event_data = event_data

	for child in _container.get_children():
		child.queue_free()

	var title := Label.new()
	title.text = event_data.get("title", "未知事件")
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	title.add_theme_font_size_override("font_size", 24)
	title.modulate = Color(0.55, 0.8, 0.35)
	_container.add_child(title)

	var desc := Label.new()
	desc.text = event_data.get("description", "")
	desc.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	desc.mouse_filter = Control.MOUSE_FILTER_IGNORE
	desc.custom_minimum_size = Vector2(580, 100)
	_container.add_child(desc)

	var options: Array = event_data.get("options", [])
	for i in range(options.size()):
		var opt = options[i]
		var btn := Button.new()
		btn.text = opt.get("text", "选项 %d" % (i + 1))
		btn.custom_minimum_size = Vector2(560, 45)
		btn.pressed.connect(func(): option_selected.emit(i))
		_container.add_child(btn)


class_name AchievementPopup extends Control

var _label: Label
var _timer: float = 0.0
var _lifespan: float = 3.0

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	custom_minimum_size = Vector2(350, 60)
	
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.1, 0.08, 0.15, 0.95)
	style.corner_radius_top_left = 10
	style.corner_radius_top_right = 10
	style.corner_radius_bottom_left = 10
	style.corner_radius_bottom_right = 10
	style.border_width_left = 2
	style.border_width_right = 2
	style.border_width_top = 2
	style.border_width_bottom = 2
	style.border_color = Color(0.7, 0.6, 0.9, 0.9)
	add_theme_stylebox_override("panel", style)

	_label = Label.new()
	_label.text = "🏆 成就解锁!"
	_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_label.add_theme_font_size_override("font_size", 16)
	_label.modulate = Color(0.9, 0.85, 1)
	add_child(_label)

	position = Vector2(
		get_viewport().get_visible_rect().size.x / 2 - 175,
		-get_viewport().get_visible_rect().size.y / 2 - 80
	)

func show_achievement(name: String, description: String) -> void:
	_label.text = "🏆 %s: %s" % [name, description]
	visible = true
	_timer = 0.0
	var tween := create_tween()
	tween.tween_property(self, "modulate:a", 1.0, 0.3).from(0.0)

func _process(delta: float) -> void:
	if not visible: return
	_timer += delta
	if _timer >= _lifespan:
		var tween := create_tween()
		tween.tween_property(self, "modulate:a", 0.0, 0.3)
		tween.tween_callback(queue_free)
		set_process(false)
