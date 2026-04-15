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
	var container := VBoxContainer.new()
	container.set_anchors_preset(Control.PRESET_CENTER)
	container.offset_left = -200
	container.offset_top = -150
	container.offset_right = 200
	container.offset_bottom = 150
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(container)
	var title := Label.new()
	title.text = "🏆 战斗奖励"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 20)
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(title)
	_gold_label = Label.new()
	_gold_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_gold_label.add_theme_font_size_override("font_size", 14)
	_gold_label.modulate = Color(1, 0.85, 0.2)
	_gold_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(_gold_label)
	_skip_btn = Button.new()
	_skip_btn.text = "跳过"
	_skip_btn.custom_minimum_size = Vector2(120, 36)
	_skip_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	_skip_btn.pressed.connect(func(): skip_pressed.emit(); visible = false)
	container.add_child(_skip_btn)

func set_rewards(rewards: Array, gold: int) -> void:
	_rewards = rewards
	_gold_label.text = "💰 +%d 金币" % gold
	for btn in _reward_buttons:
		btn.queue_free()
	_reward_buttons.clear()
	var container: VBoxContainer = _gold_label.get_parent() as VBoxContainer
	if container == null: return
	var insert_idx: int = 3
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
		btn.custom_minimum_size = Vector2(280, 40)
		btn.mouse_filter = Control.MOUSE_FILTER_STOP
		var reward_copy: Dictionary = reward.duplicate()
		btn.pressed.connect(func(): reward_chosen.emit(r_type, reward_copy); visible = false)
		container.add_child(btn)
		container.move_child(btn, insert_idx)
		insert_idx += 1
		_reward_buttons.append(btn)

func show_rewards() -> void:
	visible = true

func hide_rewards() -> void:
	visible = false
