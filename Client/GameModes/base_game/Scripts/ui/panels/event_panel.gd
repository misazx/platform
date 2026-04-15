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
	var container := VBoxContainer.new()
	container.set_anchors_preset(Control.PRESET_CENTER)
	container.offset_left = -200
	container.offset_top = -180
	container.offset_right = 200
	container.offset_bottom = 180
	container.add_theme_constant_override("separation", 10)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(container)
	var title := Label.new()
	title.text = "❓ 事件"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 20)
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(title)
	var close_btn := Button.new()
	close_btn.text = "离开"
	close_btn.custom_minimum_size = Vector2(100, 32)
	close_btn.mouse_filter = Control.MOUSE_FILTER_STOP
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
	var title_label: Label = main_vbox.get_child(0) as Label
	if title_label != null:
		title_label.text = event_data.get("title", "❓ 事件")
	var scroll: ScrollContainer = main_vbox.get_child(1) as ScrollContainer
	if scroll == null: return
	var scroll_vbox: VBoxContainer = scroll.get_child(0) as VBoxContainer
	if scroll_vbox == null: return
	var choices: Array = event_data.get("choices", [])
	for i in range(choices.size()):
		var btn := Button.new()
		btn.text = choices[i].get("text", "选项 %d" % (i + 1))
		btn.custom_minimum_size = Vector2(460, 44)
		btn.mouse_filter = Control.MOUSE_FILTER_STOP
		var idx := i
		btn.pressed.connect(func(): choice_made.emit(idx); visible = false)
		scroll_vbox.add_child(btn)
		_choice_buttons.append(btn)
