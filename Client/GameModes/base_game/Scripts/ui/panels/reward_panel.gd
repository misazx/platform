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
	panel.add_theme_stylebox_override("panel", UITheme.make_panel_bg(Color(1, 0.85, 0.3, 0.6)))
	add_child(panel)
	var container := VBoxContainer.new()
	container.add_theme_constant_override("separation", 8)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	panel.add_child(container)
	var title_row := HBoxContainer.new()
	title_row.add_theme_constant_override("separation", 8)
	title_row.mouse_filter = Control.MOUSE_FILTER_IGNORE
	title_row.alignment = BoxContainer.ALIGNMENT_CENTER
	container.add_child(title_row)
	title_row.add_child(UITheme.make_icon_rect("icon_star", Vector2(28, 28)))
	var title := Label.new()
	title.text = "战斗奖励"
	title.add_theme_font_size_override("font_size", 22)
	title.modulate = Color(1, 0.85, 0.3)
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	title_row.add_child(title)
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
	_skip_btn = UITheme.make_button("跳过", "", Vector2(140, 40))
	_skip_btn.pressed.connect(func(): skip_pressed.emit(); visible = false)
	container.add_child(_skip_btn)

func set_rewards(rewards: Array, gold: int) -> void:
	_rewards = rewards
	_gold_label.text = "+%d 金币" % gold
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
		var r_type: String = reward.get("type", "")
		var r_name: String = reward.get("name", "")
		var icon_name: String = ""
		var btn_text: String = ""
		match r_type:
			"card":
				icon_name = "icon_sword"
				btn_text = "卡牌: %s" % r_name
			"relic":
				icon_name = "icon_star"
				btn_text = "遗物: %s" % r_name
			"potion":
				icon_name = "icon_heart"
				btn_text = "药水: %s" % r_name
			"gold":
				icon_name = "icon_coin"
				btn_text = "金币: %d" % reward.get("amount", 0)
			_:
				btn_text = r_name
		var btn := UITheme.make_button(btn_text, icon_name, Vector2(420, 44))
		var reward_copy := reward.duplicate()
		btn.pressed.connect(func(): reward_chosen.emit(r_type, reward_copy); visible = false)
		scroll_vbox.add_child(btn)
		_reward_buttons.append(btn)

func show_rewards() -> void:
	visible = true

func hide_rewards() -> void:
	visible = false
