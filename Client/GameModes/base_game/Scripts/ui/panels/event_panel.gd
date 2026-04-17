class_name EventPanel extends Control

signal choice_made(choice_index: int)
signal close_pressed()

var _event_data: Dictionary = {}
var _choice_buttons: Array = []

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_layout()

func _create_layout() -> void:
	var bg := ColorRect.new()
	bg.set_anchors_preset(Control.PRESET_FULL_RECT)
	bg.color = Color(0, 0, 0, 0.75)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	var panel := PanelContainer.new()
	panel.set_anchors_preset(Control.PRESET_CENTER)
	panel.offset_left = -280
	panel.offset_top = -240
	panel.offset_right = 280
	panel.offset_bottom = 240
	panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	panel.add_theme_stylebox_override("panel", UITheme.make_panel_bg(Color(0.6, 0.5, 0.8, 0.6)))
	add_child(panel)
	var container := VBoxContainer.new()
	container.add_theme_constant_override("separation", 10)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	panel.add_child(container)
	var title_row: HBoxContainer = UITheme.make_icon_label("icon_star", "事件", Vector2(22, 22))
	title_row.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	var title_label: Label = title_row.get_child(1) as Label
	title_label.add_theme_font_size_override("font_size", 22)
	title_label.modulate = Color(0.8, 0.7, 1.0)
	container.add_child(title_row)
	var scroll := ScrollContainer.new()
	scroll.custom_minimum_size = Vector2(500, 300)
	scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
	scroll.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(scroll)
	var scroll_vbox := VBoxContainer.new()
	scroll_vbox.add_theme_constant_override("separation", 8)
	scroll_vbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	scroll.add_child(scroll_vbox)
	var close_btn: Button = UITheme.make_button("离开", "", Vector2(120, 40))
	close_btn.pressed.connect(func(): close_pressed.emit(); visible = false)
	container.add_child(close_btn)

func set_event(event_data: Dictionary) -> void:
	_event_data = event_data
	for btn in _choice_buttons:
		btn.queue_free()
	_choice_buttons.clear()
	var panel_node: PanelContainer = get_child(1) as PanelContainer
	if panel_node == null: return
	var main_vbox: VBoxContainer = panel_node.get_child(0) as VBoxContainer
	if main_vbox == null: return
	var title_hbox: HBoxContainer = main_vbox.get_child(0) as HBoxContainer
	if title_hbox != null:
		var t_label: Label = title_hbox.get_child(1) as Label
		if t_label != null:
			t_label.text = event_data.get("title", "事件")
	var scroll: ScrollContainer = main_vbox.get_child(1) as ScrollContainer
	if scroll == null: return
	var scroll_vbox: VBoxContainer = scroll.get_child(0) as VBoxContainer
	if scroll_vbox == null: return
	var choices: Array = event_data.get("choices", [])
	for i in range(choices.size()):
		var btn: Button = UITheme.make_button(choices[i].get("text", "选项 %d" % (i + 1)), "", Vector2(460, 44))
		var idx := i
		btn.pressed.connect(func(): choice_made.emit(idx); visible = false)
		scroll_vbox.add_child(btn)
		_choice_buttons.append(btn)
