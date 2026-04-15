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
	var panel := PanelContainer.new()
	panel.set_anchors_preset(Control.PRESET_CENTER)
	panel.offset_left = -200
	panel.offset_top = -160
	panel.offset_right = 200
	panel.offset_bottom = 160
	panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var panel_style := StyleBoxFlat.new()
	panel_style.bg_color = Color(0.1, 0.08, 0.12, 0.95)
	panel_style.corner_radius_top_left = 12
	panel_style.corner_radius_top_right = 12
	panel_style.corner_radius_bottom_left = 12
	panel_style.corner_radius_bottom_right = 12
	panel_style.border_width_left = 2
	panel_style.border_width_right = 2
	panel_style.border_width_top = 2
	panel_style.border_width_bottom = 2
	panel_style.border_color = Color(0.9, 0.7, 0.3, 0.6)
	panel.add_theme_stylebox_override("panel", panel_style)
	add_child(panel)
	var container := VBoxContainer.new()
	container.add_theme_constant_override("separation", 16)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	panel.add_child(container)
	var title := Label.new()
	title.text = "💎 宝箱"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 24)
	title.modulate = Color(0.9, 0.7, 0.3)
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(title)
	var take_btn := Button.new()
	take_btn.text = "打开宝箱"
	take_btn.custom_minimum_size = Vector2(200, 48)
	take_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	take_btn.pressed.connect(func(): treasure_taken.emit({}); visible = false)
	container.add_child(take_btn)
	var skip_btn := Button.new()
	skip_btn.text = "跳过"
	skip_btn.custom_minimum_size = Vector2(140, 40)
	skip_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	skip_btn.pressed.connect(func(): close_pressed.emit(); visible = false)
	container.add_child(skip_btn)

func set_treasure(treasure_data: Dictionary) -> void:
	pass
