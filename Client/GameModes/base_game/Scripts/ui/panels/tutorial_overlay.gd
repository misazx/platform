class_name TutorialOverlay extends Control

signal tutorial_completed()

var _step_labels: Array = []
var _current_step: int = 0
var _steps: Array = []

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_layout()

func _create_layout() -> void:
	var bg := ColorRect.new()
	bg.set_anchors_preset(Control.PRESET_FULL_RECT)
	bg.color = Color(0, 0, 0, 0.6)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	var container := VBoxContainer.new()
	container.set_anchors_preset(Control.PRESET_CENTER)
	container.offset_left = -200
	container.offset_top = -120
	container.offset_right = 200
	container.offset_bottom = 120
	container.add_theme_constant_override("separation", 12)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(container)
	var title := Label.new()
	title.text = "📖 教程"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 20)
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(title)
	var next_btn := Button.new()
	next_btn.text = "下一步"
	next_btn.custom_minimum_size = Vector2(120, 36)
	next_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	next_btn.pressed.connect(_on_next_step)
	container.add_child(next_btn)

func set_tutorial_steps(steps: Array) -> void:
	_steps = steps
	_current_step = 0
	_show_step(0)

func _show_step(index: int) -> void:
	if index >= _steps.size():
		tutorial_completed.emit()
		visible = false
		return
	var panel_node: PanelContainer = get_child(1) as PanelContainer
	if panel_node == null: return
	var container: VBoxContainer = panel_node.get_child(0) as VBoxContainer
	if container == null: return
	for label in _step_labels:
		label.queue_free()
	_step_labels.clear()
	var step: Dictionary = _steps[index]
	var title_label := Label.new()
	title_label.text = step.get("title", "")
	title_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title_label.add_theme_font_size_override("font_size", 18)
	title_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(title_label)
	container.move_child(title_label, 1)
	_step_labels.append(title_label)
	var desc_label := Label.new()
	desc_label.text = step.get("description", "")
	desc_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	desc_label.custom_minimum_size = Vector2(460, 100)
	desc_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(desc_label)
	container.move_child(desc_label, 2)
	_step_labels.append(desc_label)

func _on_next_step() -> void:
	_current_step += 1
	_show_step(_current_step)
