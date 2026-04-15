class_name VictoryScreen extends Control

signal continue_pressed()

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_layout()

func _create_layout() -> void:
	var bg := ColorRect.new()
	bg.set_anchors_preset(Control.PRESET_FULL_RECT)
	bg.color = Color(0, 0, 0, 0.8)
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
	title.text = "🏆 胜利！"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 32)
	title.modulate = Color(1, 0.85, 0.2)
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(title)
	var stats := Label.new()
	stats.text = "恭喜通关！"
	stats.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	stats.add_theme_font_size_override("font_size", 16)
	stats.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(stats)
	var btn := Button.new()
	btn.text = "继续"
	btn.custom_minimum_size = Vector2(140, 44)
	btn.mouse_filter = Control.MOUSE_FILTER_STOP
	btn.pressed.connect(func(): continue_pressed.emit())
	container.add_child(btn)

func set_victory_stats(stats_data: Dictionary) -> void:
	var panel_node: PanelContainer = get_child(1) as PanelContainer
	if panel_node == null: return
	var container: VBoxContainer = panel_node.get_child(0) as VBoxContainer
	if container == null: return
	var stats_label: Label = container.get_child(1) as Label
	if stats_label != null:
		stats_label.text = "击败敌人: %d | 获得金币: %d | 回合数: %d" % [stats_data.get("enemies_killed", 0), stats_data.get("gold_earned", 0), stats_data.get("turns", 0)]
