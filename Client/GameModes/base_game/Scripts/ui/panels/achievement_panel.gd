class_name AchievementPanel extends Control

signal close_pressed()

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
	container.add_theme_constant_override("separation", 10)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(container)
	var title_row: HBoxContainer = UITheme.make_icon_label("icon_star", "成就", Vector2(20, 20))
	title_row.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	var title_label: Label = title_row.get_child(1) as Label
	title_label.add_theme_font_size_override("font_size", 20)
	container.add_child(title_row)
	var scroll := ScrollContainer.new()
	scroll.custom_minimum_size = Vector2(380, 400)
	scroll.mouse_filter = Control.MOUSE_FILTER_STOP
	container.add_child(scroll)
	var close_btn := Button.new()
	close_btn.text = "关闭"
	close_btn.custom_minimum_size = Vector2(100, 32)
	close_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	close_btn.pressed.connect(func(): close_pressed.emit(); visible = false)
	container.add_child(close_btn)

func set_achievements(achievements: Array) -> void:
	pass
