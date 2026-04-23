class_name RestPanel extends Control

signal rest_choice_made(choice_data: Dictionary)
signal close_pressed()

var _player_hp: int = 80
var _player_max_hp: int = 80
var _choice_buttons: Array = []
var _hp_label: Label = null

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
	panel.offset_top = -260
	panel.offset_right = 280
	panel.offset_bottom = 260
	panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	panel.add_theme_stylebox_override("panel", UITheme.make_panel_bg(Color(0.35, 0.6, 0.4, 0.6)))
	add_child(panel)
	var container := VBoxContainer.new()
	container.add_theme_constant_override("separation", 12)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	panel.add_child(container)
	var title_row: HBoxContainer = UITheme.make_icon_label("icon_heart", "篝火休憩", Vector2(22, 22))
	title_row.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	var title_label: Label = title_row.get_child(1) as Label
	title_label.add_theme_font_size_override("font_size", 22)
	title_label.modulate = Color(0.6, 1.0, 0.6)
	container.add_child(title_row)
	_hp_label = Label.new()
	_hp_label.text = "当前生命: %d / %d" % [_player_hp, _player_max_hp]
	_hp_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_hp_label.add_theme_font_size_override("font_size", 16)
	_hp_label.modulate = Color(1, 0.8, 0.8)
	container.add_child(_hp_label)
	var desc_label: Label = Label.new()
	desc_label.text = "选择一项休憩活动来恢复体力"
	desc_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	desc_label.add_theme_font_size_override("font_size", 14)
	desc_label.modulate = Color(0.85, 0.85, 0.85)
	container.add_child(desc_label)
	var scroll := ScrollContainer.new()
	scroll.custom_minimum_size = Vector2(500, 280)
	scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
	scroll.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(scroll)
	var scroll_vbox := VBoxContainer.new()
	scroll_vbox.add_theme_constant_override("separation", 10)
	scroll_vbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	scroll.add_child(scroll_vbox)
	var recover_btn: Button = UITheme.make_button("♨ 休息恢复 - 恢复30%生命值", "", Vector2(460, 48))
	recover_btn.pressed.connect(func(): _on_choice_made("recover"))
	scroll_vbox.add_child(recover_btn)
	_choice_buttons.append(recover_btn)
	var upgrade_btn: Button = UITheme.make_button("⬆ 升级卡牌 - 选择一张牌进行强化", "", Vector2(460, 48))
	upgrade_btn.pressed.connect(func(): _on_choice_made("upgrade"))
	scroll_vbox.add_child(upgrade_btn)
	_choice_buttons.append(upgrade_btn)
	var smith_btn: Button = UITheme.make_button("🔧 打造就移除 - 移除一张牌", "", Vector2(460, 48))
	smith_btn.pressed.connect(func(): _on_choice_made("remove"))
	scroll_vbox.add_child(smith_btn)
	_choice_buttons.append(smith_btn)
	var close_btn: Button = UITheme.make_button("离开休息点", "", Vector2(120, 40))
	close_btn.pressed.connect(func(): close_pressed.emit(); visible = false)
	container.add_child(close_btn)

func set_player_stats(hp: int, max_hp: int) -> void:
	_player_hp = hp
	_player_max_hp = max_hp
	if _hp_label != null:
		_hp_label.text = "当前生命: %d / %d" % [_player_hp, _player_max_hp]

func _on_choice_made(choice: String) -> void:
	print("[RestPanel] Choice made: %s" % choice)
	match choice:
		"recover":
			var heal_amount: int = maxi(10, int(_player_max_hp * 0.3))
			_player_hp = mini(_player_max_hp, _player_hp + heal_amount)
			if _hp_label != null:
				_hp_label.text = "当前生命: %d / %d" % [_player_hp, _player_max_hp]
			rest_choice_made.emit({"choice": choice, "heal_amount": heal_amount, "new_hp": _player_hp})
		"upgrade":
			rest_choice_made.emit({"choice": choice})
		"remove":
			rest_choice_made.emit({"choice": choice})
	for btn in _choice_buttons:
		btn.disabled = true
		btn.modulate = Color(0.5, 0.5, 0.5, 0.5)
