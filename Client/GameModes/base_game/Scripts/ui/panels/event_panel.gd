class_name EventPanel extends Control

signal choice_made(choice_index: int, choice_data: Dictionary)
signal close_pressed()

var _event_data: Dictionary = {}
var _choice_buttons: Array = []
var _available_events: Array = []

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_layout()
	_initialize_events()

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
	var title_row: HBoxContainer = UITheme.make_icon_label("icon_star", "神秘事件", Vector2(22, 22))
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

func _initialize_events() -> void:
	_available_events = [
		{
			"id": "event_shining_light",
			"title": "闪耀之光",
			"description": "前方传来一阵耀眼的光芒...",
			"choices": [
				{"text": "✨ 走向光芒 - 恢复20%生命值，获得1张随机稀有卡牌", "effect": "heal_and_card", "heal_percent": 0.2, "card_rarity": 3},
				{"text": "🛡 转身离开 - 无事发生", "effect": "nothing"},
				{"text": "⚔ 冲向光芒 - 受到15点伤害，获得50金币", "effect": "risk_gold", "damage": 15, "gold": 50}
			]
		},
		{
			"id": "event_mysterious_voice",
			"title": "神秘声音",
			"description": "黑暗中传来低语声：'想要力量吗？'",
			"choices": [
				{"text": "👂 倾听 - 获得2点力量，受到5点伤害", "effect": "strength_damage", "strength": 2, "damage": 5},
				{"text": "🙏 祈祷 - 恢复15点生命值", "effect": "heal_flat", "heal_amount": 15},
				{"text": "🚶 忽略 - 继续前进", "effect": "nothing"}
			]
		},
		{
			"id": "event_world_of_goo",
			"title": "粘液世界",
			"description": "地面覆盖着奇怪的粘液...",
			"choices": [
				{"text": "🧪 触碰粘液 - 获得随机药水，可能中毒", "effect": "potion_risk", "poison_chance": 0.3},
				{"text": "🔥 烧毁粘液 - 失去10金币，清除所有负面状态", "effect": "cleanse_gold", "gold_cost": 10},
				{"text": "⏭ 小心绕过 - 无事发生", "effect": "nothing"}
			]
		},
		{
			"id": "event_big_fish",
			"title": "大鱼",
			"description": "一条巨大的鱼挡住了去路...",
			"choices": [
				{"text": "🎣 尝试钓鱼 - 有几率获得食物或被攻击", "effect": "fishing", "food_chance": 0.6},
				{"text": "🗡 攻击它 - 战斗开始！敌人：Big Fish (HP: 75)", "effect": "combat", "enemy_id": "Big_Fish"},
				{"text": "🏊 游过去 - 受到10点伤害", "effect": "swim_damage", "damage": 10}
			]
		},
		{
			"id": "event_bonfire_spirits",
			"title": "篝火精灵",
			"description": "一群小精灵在篝火旁跳舞...",
			"choices": [
				{"text": "🔥 加入舞蹈 - 恢复25%生命值", "effect": "heal_percent", "heal_percent": 0.25},
				{"text": "🎁 向它们献上礼物 - 失去30金币，获得随机遗物", "effect": "relic_gift", "gold_cost": 30},
				{"text": "💤 安静观看 - 下次战斗抽牌+2", "effect": "draw_bonus", "bonus_draw": 2}
			]
		},
		{
			"id": "event_cursed_tome",
			"title": "诅咒之书",
			"description": "一本古老的书籍散发着不祥的气息...",
			"choices": [
				{"text": "📖 阅读它 - 移除手牌中一张牌，获得一张随机稀有卡", "effect": "remove_and_gain", "card_rarity": 3},
				{"text": "🔒 封印它 - 需要25金币，获得100经验值", "effect": "seal_tome", "gold_cost": 25, "exp": 100},
				{"text": "📕 销毁它 - 获得力量+2，但最大生命-10", "effect": "power_hp_trade", "strength": 2, "max_hp_loss": 10}
			]
		},
		{
			"id": "event_falling_wizard",
			"title": "坠落的巫师",
			"description": "一个巫师从天而降，看起来晕头转向...",
			"choices": [
				{"text": "❓ 帮助他 - 他会给你一张随机能力牌作为感谢", "effect": "gift_power_card", "card_type": 2},
				{"text": "💰 抢劫他 - 获得40金币，但下次遭遇精英敌人", "effect": "rob_gold", "gold": 40, "elite_next": true},
				{"text": "😆 嘲笑他 - 他生气地离开，什么都没发生", "effect": "nothing"}
			]
		},
		{
			"id": "event_vampires",
			"title": "吸血鬼",
			"description": "几个吸血鬼在阴影中注视着你...",
			"choices": [
				{"text": "🩸 献出血液 - 失去15%生命值，获得2张随机攻击牌", "effect": "blood_for_cards", "hp_loss_percent": 0.15, "cards": 2, "card_type": 0},
				{"text": "🌞 使用阳光 - 如果你有火焰遗物，驱散它们并获得奖励", "effect": "sunlight_check"},
				{"text": "🏃 快速逃跑 - 成功逃跑，无事发生", "effect": "escape_success"}
			]
		}
	]

func set_event(event_data: Dictionary) -> void:
	if not event_data.is_empty():
		_event_data = event_data
	else:
		var random_idx: int = randi() % _available_events.size()
		_event_data = _available_events[random_idx].duplicate()
	_display_event()

func set_random_event() -> void:
	var random_idx: int = randi() % _available_events.size()
	_event_data = _available_events[random_idx].duplicate()
	_display_event()

func _display_event() -> void:
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
			t_label.text = _event_data.get("title", "事件")
	var scroll: ScrollContainer = main_vbox.get_child(1) as ScrollContainer
	if scroll == null: return
	var scroll_vbox: VBoxContainer = scroll.get_child(0) as VBoxContainer
	if scroll_vbox == null: return
	var desc_label: RichTextLabel = RichTextLabel.new()
	desc_label.bbcode_enabled = true
	desc_label.text = "[center]%s[/center]" % _event_data.get("description", "")
	desc_label.fit_content = true
	desc_label.custom_minimum_size = Vector2(460, 60)
	desc_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	scroll_vbox.add_child(desc_label)
	var choices: Array = _event_data.get("choices", [])
	for i in range(choices.size()):
		var choice: Dictionary = choices[i]
		var btn: Button = UITheme.make_button(choice.get("text", "选项 %d" % (i + 1)), "", Vector2(460, 44))
		var idx := i
		btn.pressed.connect(func(): _on_choice_selected(idx))
		scroll_vbox.add_child(btn)
		_choice_buttons.append(btn)

func _on_choice_selected(index: int) -> void:
	var choices: Array = _event_data.get("choices", [])
	if index < 0 or index >= choices.size():
		return
	var choice_data: Dictionary = choices[index].duplicate()
	print("[EventPanel] Choice selected: %s - %s" % [index, choice_data.get("text", "")])
	choice_made.emit(index, choice_data)
	close_pressed.emit()
	visible = false
