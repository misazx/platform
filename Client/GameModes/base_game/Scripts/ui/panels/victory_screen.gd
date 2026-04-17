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
	var panel := PanelContainer.new()
	panel.set_anchors_preset(Control.PRESET_CENTER)
	panel.offset_left = -250
	panel.offset_top = -200
	panel.offset_right = 250
	panel.offset_bottom = 200
	panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	panel.add_theme_stylebox_override("panel", UITheme.make_panel_bg(Color(1, 0.85, 0.2, 0.6)))
	add_child(panel)
	var container := VBoxContainer.new()
	container.add_theme_constant_override("separation", 20)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	panel.add_child(container)
	var title_row: HBoxContainer = UITheme.make_icon_label("icon_star", "胜利！", Vector2(28, 28))
	title_row.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	var title_label: Label = title_row.get_child(1) as Label
	title_label.add_theme_font_size_override("font_size", 36)
	title_label.modulate = Color(1, 0.85, 0.2)
	container.add_child(title_row)
	var stats := Label.new()
	stats.text = "恭喜通关！"
	stats.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	stats.add_theme_font_size_override("font_size", 18)
	stats.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(stats)
	var btn: Button = UITheme.make_button("继续", "", Vector2(200, 48))
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
