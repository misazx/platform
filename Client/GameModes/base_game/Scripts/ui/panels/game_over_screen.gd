class_name GameOverScreen extends Control

signal restart_pressed()
signal main_menu_pressed()

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_layout()

func _create_layout() -> void:
	var bg := ColorRect.new()
	bg.set_anchors_preset(Control.PRESET_FULL_RECT)
	bg.color = Color(0, 0, 0, 0.85)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	var panel := PanelContainer.new()
	panel.set_anchors_preset(Control.PRESET_CENTER)
	panel.offset_left = -220
	panel.offset_top = -180
	panel.offset_right = 220
	panel.offset_bottom = 180
	panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var panel_style := StyleBoxFlat.new()
	panel_style.bg_color = Color(0.12, 0.05, 0.05, 0.95)
	panel_style.corner_radius_top_left = 12
	panel_style.corner_radius_top_right = 12
	panel_style.corner_radius_bottom_left = 12
	panel_style.corner_radius_bottom_right = 12
	panel_style.border_width_left = 2
	panel_style.border_width_right = 2
	panel_style.border_width_top = 2
	panel_style.border_width_bottom = 2
	panel_style.border_color = Color(0.9, 0.2, 0.2, 0.6)
	panel.add_theme_stylebox_override("panel", panel_style)
	add_child(panel)
	var container := VBoxContainer.new()
	container.add_theme_constant_override("separation", 20)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	panel.add_child(container)
	var title := Label.new()
	title.text = "💀 失败"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 36)
	title.modulate = Color(0.9, 0.2, 0.2)
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(title)
	var restart_btn := Button.new()
	restart_btn.text = "重新开始"
	restart_btn.custom_minimum_size = Vector2(200, 48)
	restart_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	restart_btn.pressed.connect(func(): restart_pressed.emit())
	container.add_child(restart_btn)
	var menu_btn := Button.new()
	menu_btn.text = "返回主菜单"
	menu_btn.custom_minimum_size = Vector2(200, 48)
	menu_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	menu_btn.pressed.connect(func(): main_menu_pressed.emit())
	container.add_child(menu_btn)
