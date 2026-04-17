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
	panel.add_theme_stylebox_override("panel", UITheme.make_panel_bg(Color(0.9, 0.2, 0.2, 0.6)))
	add_child(panel)
	var container := VBoxContainer.new()
	container.add_theme_constant_override("separation", 20)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	panel.add_child(container)
	var title_row: HBoxContainer = UITheme.make_icon_label("icon_skull", "失败", Vector2(28, 28))
	title_row.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	var title_label: Label = title_row.get_child(1) as Label
	title_label.add_theme_font_size_override("font_size", 36)
	title_label.modulate = Color(0.9, 0.2, 0.2)
	container.add_child(title_row)
	var restart_btn: Button = UITheme.make_button("重新开始", "", Vector2(200, 48))
	restart_btn.pressed.connect(func(): restart_pressed.emit())
	container.add_child(restart_btn)
	var menu_btn: Button = UITheme.make_button("返回主菜单", "", Vector2(200, 48))
	menu_btn.pressed.connect(func(): main_menu_pressed.emit())
	container.add_child(menu_btn)
