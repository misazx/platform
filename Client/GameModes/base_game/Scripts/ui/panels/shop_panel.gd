class_name ShopPanel extends Control

signal item_purchased(item_type: String, item_data: Dictionary)
signal close_pressed()

var _items: Array = []
var _item_buttons: Array = []
var _gold_label: Label
var _player_gold: int = 0

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_layout()

func _create_layout() -> void:
	var bg := ColorRect.new()
	bg.set_anchors_preset(Control.PRESET_FULL_RECT)
	bg.color = Color(0, 0, 0, 0.75)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	var panel := PanelContainer.new()
	panel.set_anchors_preset(Control.PRESET_CENTER)
	panel.offset_left = -300
	panel.offset_top = -280
	panel.offset_right = 300
	panel.offset_bottom = 280
	panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	panel.add_theme_stylebox_override("panel", UITheme.make_panel_bg(Color(0.3, 0.8, 0.5, 0.6)))
	add_child(panel)
	var container := VBoxContainer.new()
	container.add_theme_constant_override("separation", 10)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	panel.add_child(container)
	var title_row: HBoxContainer = UITheme.make_icon_label("icon_coin", "商店", Vector2(24, 24))
	title_row.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	var title_label: Label = title_row.get_child(1) as Label
	title_label.add_theme_font_size_override("font_size", 24)
	title_label.modulate = Color(0.3, 0.8, 0.5)
	container.add_child(title_row)
	var gold_row: HBoxContainer = UITheme.make_icon_label("icon_coin", "0 金币", Vector2(16, 16))
	gold_row.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	_gold_label = gold_row.get_child(1) as Label
	_gold_label.add_theme_font_size_override("font_size", 16)
	_gold_label.modulate = Color(1, 0.85, 0.2)
	container.add_child(gold_row)
	var scroll := ScrollContainer.new()
	scroll.custom_minimum_size = Vector2(540, 360)
	scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
	scroll.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(scroll)
	var scroll_vbox := VBoxContainer.new()
	scroll_vbox.add_theme_constant_override("separation", 8)
	scroll_vbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	scroll.add_child(scroll_vbox)
	var close_btn: Button = UITheme.make_button("离开商店", "", Vector2(140, 40))
	close_btn.pressed.connect(func(): close_pressed.emit(); visible = false)
	container.add_child(close_btn)

func set_gold(gold: int) -> void:
	_player_gold = gold
	_gold_label.text = "%d 金币" % gold

func set_shop_items(items: Array) -> void:
	_items = items
	for btn in _item_buttons:
		btn.queue_free()
	_item_buttons.clear()
	var panel_node: PanelContainer = get_child(1) as PanelContainer
	if panel_node == null: return
	var main_vbox: VBoxContainer = panel_node.get_child(0) as VBoxContainer
	if main_vbox == null: return
	var scroll: ScrollContainer = main_vbox.get_child(2) as ScrollContainer
	if scroll == null: return
	var scroll_vbox: VBoxContainer = scroll.get_child(0) as VBoxContainer
	if scroll_vbox == null: return
	for item in items:
		var i_type: String = item.get("type", "")
		var i_name: String = item.get("name", "")
		var i_cost: int = item.get("cost", 50)
		var btn: Button = UITheme.make_button("%s: %s (%d)" % [i_type, i_name, i_cost], "icon_coin", Vector2(500, 44))
		btn.disabled = i_cost > _player_gold
		var item_copy: Dictionary = item.duplicate()
		btn.pressed.connect(func(): _on_item_bought(item_copy))
		scroll_vbox.add_child(btn)
		_item_buttons.append(btn)

func _on_item_bought(item: Dictionary) -> void:
	var cost: int = item.get("cost", 50)
	if cost > _player_gold: return
	_player_gold -= cost
	_gold_label.text = "%d 金币" % _player_gold
	item_purchased.emit(item.get("type", ""), item)
	for btn in _item_buttons:
		if btn.text.find(item.get("name", "")) >= 0:
			btn.disabled = true
			btn.modulate = Color(0.5, 0.5, 0.5, 0.5)
			break
