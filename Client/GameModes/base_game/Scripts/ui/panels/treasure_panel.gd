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
	panel.add_theme_stylebox_override("panel", UITheme.make_panel_bg(Color(0.9, 0.7, 0.3, 0.6)))
	add_child(panel)
	var container := VBoxContainer.new()
	container.add_theme_constant_override("separation", 16)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	panel.add_child(container)
	var title_row: HBoxContainer = UITheme.make_icon_label("icon_star", "宝箱", Vector2(24, 24))
	title_row.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	var title_label: Label = title_row.get_child(1) as Label
	title_label.add_theme_font_size_override("font_size", 24)
	title_label.modulate = Color(0.9, 0.7, 0.3)
	container.add_child(title_row)
	var take_btn: Button = UITheme.make_button("打开宝箱", "icon_star", Vector2(200, 48))
	take_btn.pressed.connect(func(): treasure_taken.emit({}); visible = false)
	container.add_child(take_btn)
	var skip_btn: Button = UITheme.make_button("跳过", "", Vector2(140, 40))
	skip_btn.pressed.connect(func(): close_pressed.emit(); visible = false)
	container.add_child(skip_btn)

func set_treasure(treasure_data: Dictionary) -> void:
	pass
