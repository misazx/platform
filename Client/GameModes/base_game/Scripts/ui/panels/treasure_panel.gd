class_name TreasurePanel extends Control

signal treasure_taken(treasure_data: Dictionary)
signal close_pressed()

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_layout()

func _create_layout() -> void:
	var bg := ColorRect.new()
	bg.set_anchors_preset(Control.PRESET_FULL_RECT)
	bg.color = Color(0, 0, 0, 0.7)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	var container := VBoxContainer.new()
	container.set_anchors_preset(Control.PRESET_CENTER)
	container.offset_left = -150
	container.offset_top = -100
	container.offset_right = 150
	container.offset_bottom = 100
	container.add_theme_constant_override("separation", 15)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(container)
	var title := Label.new()
	title.text = "💎 宝箱"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 22)
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(title)
	var take_btn := Button.new()
	take_btn.text = "打开宝箱"
	take_btn.custom_minimum_size = Vector2(140, 40)
	take_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	take_btn.pressed.connect(func(): treasure_taken.emit({}); visible = false)
	container.add_child(take_btn)
	var skip_btn := Button.new()
	skip_btn.text = "跳过"
	skip_btn.custom_minimum_size = Vector2(100, 32)
	skip_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	skip_btn.pressed.connect(func(): close_pressed.emit(); visible = false)
	container.add_child(skip_btn)

func set_treasure(treasure_data: Dictionary) -> void:
	pass
