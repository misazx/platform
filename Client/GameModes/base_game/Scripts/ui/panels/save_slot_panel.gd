class_name SaveSlotPanel extends Control

signal save_requested(slot_index: int)
signal load_requested(slot_index: int)
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
	container.offset_left = -180
	container.offset_top = -150
	container.offset_right = 180
	container.offset_bottom = 150
	container.add_theme_constant_override("separation", 12)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(container)
	var title_row: HBoxContainer = UITheme.make_icon_label("icon_coin", "存档", Vector2(20, 20))
	title_row.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	var title_label: Label = title_row.get_child(1) as Label
	title_label.add_theme_font_size_override("font_size", 20)
	container.add_child(title_row)
	for i in range(3):
		var slot_row := HBoxContainer.new()
		slot_row.mouse_filter = Control.MOUSE_FILTER_IGNORE
		container.add_child(slot_row)
		var slot_label := Label.new()
		slot_label.text = "存档 %d: 空" % (i + 1)
		slot_label.custom_minimum_size = Vector2(200, 30)
		slot_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		slot_row.add_child(slot_label)
		var save_btn: Button = UITheme.make_button("保存", "", Vector2(60, 28))
		var idx := i
		save_btn.pressed.connect(func(): save_requested.emit(idx))
		slot_row.add_child(save_btn)
		var load_btn: Button = UITheme.make_button("读取", "", Vector2(60, 28))
		load_btn.pressed.connect(func(): load_requested.emit(idx))
		slot_row.add_child(load_btn)
	var close_btn: Button = UITheme.make_button("关闭", "", Vector2(100, 32))
	close_btn.pressed.connect(func(): close_pressed.emit(); visible = false)
	container.add_child(close_btn)

func update_slot_info(slot_index: int, info: Dictionary) -> void:
	pass
