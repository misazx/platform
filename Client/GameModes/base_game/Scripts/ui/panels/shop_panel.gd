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
	var container := VBoxContainer.new()
	container.set_anchors_preset(Control.PRESET_CENTER)
	container.offset_left = -250
	container.offset_top = -200
	container.offset_right = 250
	container.offset_bottom = 200
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(container)
	var title := Label.new()
	title.text = "🏪 商店"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 22)
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(title)
	_gold_label = Label.new()
	_gold_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_gold_label.add_theme_font_size_override("font_size", 14)
	_gold_label.modulate = Color(1, 0.85, 0.2)
	_gold_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(_gold_label)
	var close_btn := Button.new()
	close_btn.text = "离开商店"
	close_btn.custom_minimum_size = Vector2(120, 36)
	close_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	close_btn.pressed.connect(func(): close_pressed.emit(); visible = false)
	container.add_child(close_btn)

func set_gold(gold: int) -> void:
	_player_gold = gold
	_gold_label.text = "💰 %d 金币" % gold

func set_shop_items(items: Array) -> void:
	_items = items
	for btn in _item_buttons:
		btn.queue_free()
	_item_buttons.clear()
	var container: VBoxContainer = _gold_label.get_parent() as VBoxContainer
	if container == null: return
	var insert_idx: int = 3
	for item in items:
		var btn := Button.new()
		var i_type: String = item.get("type", "")
		var i_name: String = item.get("name", "")
		var i_cost: int = item.get("cost", 50)
		btn.text = "%s: %s (%d💰)" % [i_type, i_name, i_cost]
		btn.custom_minimum_size = Vector2(300, 36)
		btn.mouse_filter = Control.MOUSE_FILTER_STOP
		btn.disabled = i_cost > _player_gold
		var item_copy: Dictionary = item.duplicate()
		btn.pressed.connect(func(): _on_item_bought(item_copy))
		container.add_child(btn)
		container.move_child(btn, insert_idx)
		insert_idx += 1
		_item_buttons.append(btn)

func _on_item_bought(item: Dictionary) -> void:
	var cost: int = item.get("cost", 50)
	if cost > _player_gold: return
	_player_gold -= cost
	_gold_label.text = "💰 %d 金币" % _player_gold
	item_purchased.emit(item.get("type", ""), item)
	for btn in _item_buttons:
		if btn.text.find(item.get("name", "")) >= 0:
			btn.disabled = true
			btn.modulate = Color(0.5, 0.5, 0.5, 0.5)
			break
