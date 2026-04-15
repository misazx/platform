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
	var container := VBoxContainer.new()
	container.set_anchors_preset(Control.PRESET_CENTER)
	container.offset_left = -150
	container.offset_top = -100
	container.offset_right = 150
	container.offset_bottom = 100
	container.add_theme_constant_override("separation", 20)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(container)
	var title := Label.new()
	title.text = "💀 失败"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 32)
	title.modulate = Color(0.9, 0.2, 0.2)
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(title)
	var restart_btn := Button.new()
	restart_btn.text = "重新开始"
	restart_btn.custom_minimum_size = Vector2(140, 44)
	restart_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	restart_btn.pressed.connect(func(): restart_pressed.emit())
	container.add_child(restart_btn)
	var menu_btn := Button.new()
	menu_btn.text = "返回主菜单"
	menu_btn.custom_minimum_size = Vector2(140, 44)
	menu_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	menu_btn.pressed.connect(func(): main_menu_pressed.emit())
	container.add_child(menu_btn)
