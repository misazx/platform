class_name PileViewPanel extends Control

signal close_pressed()

var _title_label: Label
var _card_list: VBoxContainer

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
	container.offset_left = -200
	container.offset_top = -250
	container.offset_right = 200
	container.offset_bottom = 250
	container.add_theme_constant_override("separation", 8)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(container)
	_title_label = Label.new()
	_title_label.text = "牌堆"
	_title_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_title_label.add_theme_font_size_override("font_size", 18)
	_title_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(_title_label)
	var scroll := ScrollContainer.new()
	scroll.custom_minimum_size = Vector2(380, 400)
	scroll.mouse_filter = Control.MOUSE_FILTER_STOP
	container.add_child(scroll)
	_card_list = VBoxContainer.new()
	_card_list.mouse_filter = Control.MOUSE_FILTER_IGNORE
	scroll.add_child(_card_list)
	var close_btn := Button.new()
	close_btn.text = "关闭"
	close_btn.custom_minimum_size = Vector2(100, 32)
	close_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	close_btn.pressed.connect(func(): close_pressed.emit(); visible = false)
	container.add_child(close_btn)

func set_pile_data(pile_name: String, cards: Array) -> void:
	_title_label.text = pile_name
	for child in _card_list.get_children():
		child.queue_free()
	for card in cards:
		var label := Label.new()
		label.text = "%s (费用:%d)" % [card.get("name", "???"), card.get("cost", 0)]
		label.add_theme_font_size_override("font_size", 12)
		label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_card_list.add_child(label)
