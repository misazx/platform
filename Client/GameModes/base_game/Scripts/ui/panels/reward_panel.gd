class_name RewardPanel extends Control

signal reward_chosen(reward_type: String, reward_data: Dictionary)
signal skip_pressed()

var _rewards: Array = []
var _reward_buttons: Array = []
var _gold_label: Label
var _skip_btn: Button

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_layout()

func _create_layout() -> void:
	var bg := ColorRect.new()
	bg.set_anchors_preset(Control.PRESET_FULL_RECT)
	bg.color = Color(0, 0, 0, 0.7)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	var panel := PanelContainer.new()
	panel.set_anchors_preset(Control.PRESET_CENTER)
	panel.offset_left = -250
	panel.offset_top = -220
	panel.offset_right = 250
	panel.offset_bottom = 220
	panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var panel_style := StyleBoxFlat.new()
	panel_style.bg_color = Color(0.1, 0.08, 0.12, 0.95)
	panel_style.corner_radius_top_left = 12
	panel_style.corner_radius_top_right = 12
	panel_style.corner_radius_bottom_left = 12
	panel_style.corner_radius_bottom_right = 12
	panel_style.border_width_left = 2
	panel_style.border_width_right = 2
	panel_style.border_width_top = 2
	panel_style.border_width_bottom = 2
	panel_style.border_color = Color(1, 0.85, 0.3, 0.6)
	panel.add_theme_stylebox_override("panel", panel_style)
	add_child(panel)
	var container := VBoxContainer.new()
	container.add_theme_constant_override("separation", 8)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	panel.add_child(container)
	var title := Label.new()
	title.text = "🏆 战斗奖励"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 22)
	title.modulate = Color(1, 0.85, 0.3)
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(title)
	_gold_label = Label.new()
	_gold_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_gold_label.add_theme_font_size_override("font_size", 16)
	_gold_label.modulate = Color(1, 0.85, 0.2)
	_gold_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(_gold_label)
	var scroll := ScrollContainer.new()
	scroll.custom_minimum_size = Vector2(440, 260)
	scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
	scroll.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(scroll)
	var scroll_vbox := VBoxContainer.new()
	scroll_vbox.add_theme_constant_override("separation", 6)
	scroll_vbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	scroll.add_child(scroll_vbox)
	_skip_btn = Button.new()
	_skip_btn.text = "跳过"
	_skip_btn.custom_minimum_size = Vector2(140, 40)
	_skip_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	_skip_btn.pressed.connect(func(): skip_pressed.emit(); visible = false)
	container.add_child(_skip_btn)

func set_rewards(rewards: Array, gold: int) -> void:
	_rewards = rewards
	_gold_label.text = "💰 +%d 金币" % gold
	for btn in _reward_buttons:
		btn.queue_free()
	_reward_buttons.clear()
	var panel_node: PanelContainer = get_child(1) as PanelContainer
	if panel_node == null: return
	var main_vbox: VBoxContainer = panel_node.get_child(0) as VBoxContainer
	if main_vbox == null: return
	var scroll: ScrollContainer = main_vbox.get_child(2) as ScrollContainer
	if scroll == null: return
	var scroll_vbox: VBoxContainer = scroll.get_child(0) as VBoxContainer
	if scroll_vbox == null: return
	for reward in rewards:
		var btn := Button.new()
		var r_type: String = reward.get("type", "")
		var r_name: String = reward.get("name", "")
		match r_type:
			"card": btn.text = "🃏 卡牌: %s" % r_name
			"relic": btn.text = "🏺 遗物: %s" % r_name
			"potion": btn.text = "🧪 药水: %s" % r_name
			"gold": btn.text = "💰 金币: %d" % reward.get("amount", 0)
			_: btn.text = r_name
		btn.custom_minimum_size = Vector2(420, 44)
		btn.mouse_filter = Control.MOUSE_FILTER_STOP
		var reward_copy := reward.duplicate()
		btn.pressed.connect(func(): reward_chosen.emit(r_type, reward_copy); visible = false)
		scroll_vbox.add_child(btn)
		_reward_buttons.append(btn)

func show_rewards() -> void:
	visible = true

func hide_rewards() -> void:
	visible = false
