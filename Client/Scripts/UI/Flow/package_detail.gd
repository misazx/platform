class_name PackageDetail extends Control

signal start_game_requested(package_id: String)
signal continue_game_requested(package_id: String, slot_id: int)
signal back_pressed()
signal create_room_requested(package_id: String)
signal join_room_requested(package_id: String)

var _package_id: String = ""
var _package_data: Dictionary = {}
var _provider: Dictionary = {}

var _bg_overlay: ColorRect
var _main_panel: PanelContainer
var _header_area: HBoxContainer
var _content_area: TabContainer
var _back_button: Button
var _start_button: Button

var _overview_tab: VBoxContainer
var _saves_tab: VBoxContainer
var _achievements_tab: VBoxContainer
var _leaderboard_tab: ScrollContainer
var _leaderboard_status: Label
var _mode_select_panel: PanelContainer

func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)
	mouse_filter = Control.MOUSE_FILTER_STOP
	_build_ui()

func _build_ui() -> void:
	_bg_overlay = ColorRect.new()
	_bg_overlay.set_anchors_preset(Control.PRESET_FULL_RECT)
	_bg_overlay.color = Color(0, 0, 0, 0.88)
	_bg_overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_bg_overlay)

	var center := CenterContainer.new()
	center.set_anchors_preset(Control.PRESET_FULL_RECT)
	center.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(center)

	_main_panel = PanelContainer.new()
	_main_panel.custom_minimum_size = Vector2(900, 620)
	_main_panel.mouse_filter = Control.MOUSE_FILTER_IGNORE

	var panel_style := StyleBoxFlat.new()
	panel_style.bg_color = Color(0.08, 0.06, 0.1, 0.98)
	panel_style.corner_radius_top_left = 16
	panel_style.corner_radius_top_right = 16
	panel_style.corner_radius_bottom_left = 16
	panel_style.corner_radius_bottom_right = 16
	panel_style.border_width_left = 2
	panel_style.border_width_right = 2
	panel_style.border_width_top = 2
	panel_style.border_width_bottom = 2
	panel_style.border_color = Color(0.4, 0.35, 0.55, 0.8)
	panel_style.content_margin_top = 16
	panel_style.content_margin_bottom = 16
	panel_style.content_margin_left = 20
	panel_style.content_margin_right = 20
	_main_panel.add_theme_stylebox_override("panel", panel_style)
	center.add_child(_main_panel)

	var main_vbox := VBoxContainer.new()
	main_vbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	main_vbox.add_theme_constant_override("separation", 12)
	_main_panel.add_child(main_vbox)

	_header_area = HBoxContainer.new()
	_header_area.mouse_filter = Control.MOUSE_FILTER_IGNORE
	main_vbox.add_child(_header_area)

	_back_button = Button.new()
	_back_button.text = "← 返回"
	_back_button.custom_minimum_size = Vector2(100, 38)
	_back_button.mouse_filter = Control.MOUSE_FILTER_STOP
	_back_button.pressed.connect(_on_back_pressed)
	_header_area.add_child(_back_button)

	var title_spacer := Control.new()
	title_spacer.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	title_spacer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_header_area.add_child(title_spacer)

	var title_label := Label.new()
	title_label.name = "PackageTitle"
	title_label.text = "玩法详情"
	title_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	title_label.add_theme_font_size_override("font_size", 24)
	title_label.modulate = Color(1, 0.9, 0.6)
	_header_area.add_child(title_label)

	var btn_spacer := Control.new()
	btn_spacer.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	btn_spacer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_header_area.add_child(btn_spacer)

	_start_button = Button.new()
	_start_button.text = "▶ 开始游戏"
	_start_button.custom_minimum_size = Vector2(130, 38)
	_start_button.mouse_filter = Control.MOUSE_FILTER_STOP

	var start_style := StyleBoxFlat.new()
	start_style.bg_color = Color(0.15, 0.4, 0.2, 0.95)
	start_style.corner_radius_top_left = 8
	start_style.corner_radius_top_right = 8
	start_style.corner_radius_bottom_left = 8
	start_style.corner_radius_bottom_right = 8
	start_style.border_width_left = 2
	start_style.border_width_right = 2
	start_style.border_width_top = 2
	start_style.border_width_bottom = 2
	start_style.border_color = Color(0.3, 0.7, 0.4)
	_start_button.add_theme_stylebox_override("normal", start_style)
	_start_button.pressed.connect(_on_start_pressed)
	_header_area.add_child(_start_button)

	_content_area = TabContainer.new()
	_content_area.custom_minimum_size = Vector2(850, 480)
	_content_area.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_content_area.tab_alignment = TabBar.ALIGNMENT_CENTER
	main_vbox.add_child(_content_area)

	_overview_tab = VBoxContainer.new()
	_overview_tab.name = "📋 概览"
	_overview_tab.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_overview_tab.add_theme_constant_override("separation", 8)
	_content_area.add_child(_overview_tab)

	_saves_tab = VBoxContainer.new()
	_saves_tab.name = "💾 存档"
	_saves_tab.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_saves_tab.add_theme_constant_override("separation", 8)
	_content_area.add_child(_saves_tab)

	_achievements_tab = VBoxContainer.new()
	_achievements_tab.name = "🏆 成就"
	_achievements_tab.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_achievements_tab.add_theme_constant_override("separation", 8)
	_content_area.add_child(_achievements_tab)

	_leaderboard_tab = ScrollContainer.new()
	_leaderboard_tab.name = "📊 排行榜"
	_leaderboard_tab.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_content_area.add_child(_leaderboard_tab)

func setup(package_id: String, package_data: Dictionary) -> void:
	_package_id = package_id
	_package_data = package_data

	if PackageUIRegistry.instance != null:
		_provider = PackageUIRegistry.instance.get_provider(package_id)

	var title_label = _header_area.get_node_or_null("PackageTitle")
	if title_label != null:
		title_label.text = package_data.get("name", "未知玩法")

	_populate_overview()
	_populate_saves()
	_populate_achievements()
	_fetch_leaderboard_from_server()

func _populate_overview() -> void:
	for child in _overview_tab.get_children():
		child.queue_free()

	var desc_label := Label.new()
	desc_label.text = _package_data.get("description", "暂无描述")
	desc_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	desc_label.custom_minimum_size = Vector2(800, 80)
	desc_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	desc_label.add_theme_font_size_override("font_size", 14)
	desc_label.modulate = Color(0.85, 0.85, 0.85)
	_overview_tab.add_child(desc_label)

	var features_label := Label.new()
	features_label.text = "✨ 特色功能"
	features_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	features_label.add_theme_font_size_override("font_size", 16)
	features_label.modulate = Color(1, 0.9, 0.6)
	_overview_tab.add_child(features_label)

	var features: Array = _package_data.get("features", [])
	for feature in features:
		var feat_label := Label.new()
		feat_label.text = "  • %s" % feature
		feat_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		feat_label.add_theme_font_size_override("font_size", 13)
		feat_label.modulate = Color(0.75, 0.75, 0.75)
		_overview_tab.add_child(feat_label)

	var spacer := Control.new()
	spacer.custom_minimum_size = Vector2(0, 15)
	spacer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_overview_tab.add_child(spacer)

	var info_grid := GridContainer.new()
	info_grid.columns = 2
	info_grid.add_theme_constant_override("h_separation", 20)
	info_grid.add_theme_constant_override("v_separation", 8)
	info_grid.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_overview_tab.add_child(info_grid)

	_add_info_row(info_grid, "版本", "v%s" % _package_data.get("version", "1.0"))
	_add_info_row(info_grid, "作者", _package_data.get("author", "未知"))
	_add_info_row(info_grid, "评分", "⭐ %.1f" % _package_data.get("rating", 0))
	_add_info_row(info_grid, "下载量", "%d" % _package_data.get("downloadCount", 0))
	_add_info_row(info_grid, "类型", "免费" if _package_data.get("isFree", true) else "付费 ¥%.0f" % _package_data.get("price", 0))

func _add_info_row(grid: GridContainer, label_text: String, value_text: String) -> void:
	var label := Label.new()
	label.text = label_text
	label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	label.add_theme_font_size_override("font_size", 13)
	label.modulate = Color(0.6, 0.6, 0.6)
	grid.add_child(label)

	var value := Label.new()
	value.text = value_text
	value.mouse_filter = Control.MOUSE_FILTER_IGNORE
	value.add_theme_font_size_override("font_size", 13)
	value.modulate = Color(0.9, 0.9, 0.9)
	grid.add_child(value)

func _populate_saves() -> void:
	for child in _saves_tab.get_children():
		child.queue_free()

	var saves_title := Label.new()
	saves_title.text = "💾 存档管理"
	saves_title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	saves_title.add_theme_font_size_override("font_size", 16)
	saves_title.modulate = Color(1, 0.9, 0.6)
	_saves_tab.add_child(saves_title)

	var save_slots: Array = []
	if not _provider.is_empty():
		save_slots = _provider.get("save_slots", [])

	if save_slots.is_empty():
		for i in range(3):
			save_slots.append({"slot_id": i + 1, "data": {}, "has_save": false})

	for slot in save_slots:
		var slot_panel := _create_save_slot(slot)
		_saves_tab.add_child(slot_panel)

func _create_save_slot(slot: Dictionary) -> PanelContainer:
	var panel := PanelContainer.new()
	panel.custom_minimum_size = Vector2(800, 70)
	panel.mouse_filter = Control.MOUSE_FILTER_IGNORE

	var has_save: bool = slot.get("has_save", false)
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.1, 0.08, 0.12, 0.9) if has_save else Color(0.06, 0.05, 0.08, 0.8)
	style.corner_radius_top_left = 8
	style.corner_radius_top_right = 8
	style.corner_radius_bottom_left = 8
	style.corner_radius_bottom_right = 8
	style.border_width_left = 1
	style.border_width_right = 1
	style.border_width_top = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.3, 0.5, 0.35) if has_save else Color(0.2, 0.2, 0.25)
	panel.add_theme_stylebox_override("panel", style)

	var hbox := HBoxContainer.new()
	hbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	hbox.add_theme_constant_override("separation", 12)
	panel.add_child(hbox)

	var slot_label := Label.new()
	slot_label.text = "存档 %d" % slot.get("slot_id", 0)
	slot_label.custom_minimum_size = Vector2(80, 0)
	slot_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	slot_label.add_theme_font_size_override("font_size", 14)
	slot_label.modulate = Color(0.9, 0.9, 0.9)
	hbox.add_child(slot_label)

	var info_label := Label.new()
	if has_save:
		var data: Dictionary = slot.get("data", {})
		info_label.text = "角色: %s | 层数: %d | 金币: %d" % [
			data.get("character_id", "未知"),
			data.get("current_floor", 0),
			data.get("gold", 0)]
		info_label.modulate = Color(0.8, 0.8, 0.8)
	else:
		info_label.text = "空存档"
		info_label.modulate = Color(0.4, 0.4, 0.4)
	info_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	info_label.add_theme_font_size_override("font_size", 13)
	info_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	hbox.add_child(info_label)

	if has_save:
		var continue_btn := Button.new()
		continue_btn.text = "▶ 继续"
		continue_btn.custom_minimum_size = Vector2(90, 36)
		continue_btn.mouse_filter = Control.MOUSE_FILTER_STOP
		continue_btn.pressed.connect(func():
			continue_game_requested.emit(_package_id, slot.get("slot_id", 1))
		)
		hbox.add_child(continue_btn)

		var delete_btn := Button.new()
		delete_btn.text = "🗑️"
		delete_btn.custom_minimum_size = Vector2(40, 36)
		delete_btn.mouse_filter = Control.MOUSE_FILTER_STOP
		delete_btn.modulate = Color(0.8, 0.3, 0.3)
		hbox.add_child(delete_btn)
	else:
		var new_btn := Button.new()
		new_btn.text = "➕ 新游戏"
		new_btn.custom_minimum_size = Vector2(100, 36)
		new_btn.mouse_filter = Control.MOUSE_FILTER_STOP
		new_btn.pressed.connect(func():
			start_game_requested.emit(_package_id)
		)
		hbox.add_child(new_btn)

	return panel

func _populate_achievements() -> void:
	for child in _achievements_tab.get_children():
		child.queue_free()

	var achievements_title := Label.new()
	achievements_title.text = "🏆 成就"
	achievements_title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	achievements_title.add_theme_font_size_override("font_size", 16)
	achievements_title.modulate = Color(1, 0.9, 0.6)
	_achievements_tab.add_child(achievements_title)

	var achievements: Array = []
	if not _provider.is_empty():
		achievements = _provider.get("achievement_data", [])

	if achievements.is_empty():
		var default_achievements := [
			{"id": "first_victory", "name": "初次胜利", "desc": "首次通关游戏", "unlocked": false},
			{"id": "kill_100", "name": "百人斩", "desc": "击败100个敌人", "unlocked": false},
			{"id": "all_relics", "name": "收藏家", "desc": "收集所有遗物", "unlocked": false},
			{"id": "no_damage", "name": "无伤通关", "desc": "不受伤完成一场战斗", "unlocked": false},
		]
		achievements = default_achievements

	for ach in achievements:
		var ach_panel := _create_achievement_item(ach)
		_achievements_tab.add_child(ach_panel)

func _create_achievement_item(ach: Dictionary) -> PanelContainer:
	var panel := PanelContainer.new()
	panel.custom_minimum_size = Vector2(800, 50)
	panel.mouse_filter = Control.MOUSE_FILTER_IGNORE

	var unlocked: bool = ach.get("unlocked", false)
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.1, 0.1, 0.05, 0.9) if unlocked else Color(0.06, 0.05, 0.08, 0.8)
	style.corner_radius_top_left = 6
	style.corner_radius_top_right = 6
	style.corner_radius_bottom_left = 6
	style.corner_radius_bottom_right = 6
	style.border_width_left = 1
	style.border_width_right = 1
	style.border_width_top = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.7, 0.6, 0.2) if unlocked else Color(0.2, 0.2, 0.25)
	panel.add_theme_stylebox_override("panel", style)

	var hbox := HBoxContainer.new()
	hbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	hbox.add_theme_constant_override("separation", 10)
	panel.add_child(hbox)

	var icon_label := Label.new()
	icon_label.text = "🏆" if unlocked else "🔒"
	icon_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	icon_label.add_theme_font_size_override("font_size", 18)
	hbox.add_child(icon_label)

	var name_label := Label.new()
	name_label.text = ach.get("name", "未知成就")
	name_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	name_label.add_theme_font_size_override("font_size", 14)
	name_label.modulate = Color(1, 0.9, 0.5) if unlocked else Color(0.5, 0.5, 0.5)
	name_label.custom_minimum_size = Vector2(120, 0)
	hbox.add_child(name_label)

	var desc_label := Label.new()
	desc_label.text = ach.get("desc", "")
	desc_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	desc_label.add_theme_font_size_override("font_size", 12)
	desc_label.modulate = Color(0.7, 0.7, 0.7) if unlocked else Color(0.35, 0.35, 0.35)
	desc_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	hbox.add_child(desc_label)

	return panel

func _populate_leaderboard() -> void:
	for child in _leaderboard_tab.get_children():
		child.queue_free()

	var leaderboard_vbox := VBoxContainer.new()
	leaderboard_vbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	leaderboard_vbox.add_theme_constant_override("separation", 6)
	_leaderboard_tab.add_child(leaderboard_vbox)

	var lb_title := Label.new()
	lb_title.text = "📊 排行榜"
	lb_title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	lb_title.add_theme_font_size_override("font_size", 16)
	lb_title.modulate = Color(1, 0.9, 0.6)
	leaderboard_vbox.add_child(lb_title)

	_leaderboard_status = Label.new()
	_leaderboard_status.text = "⏳ 正在加载..."
	_leaderboard_status.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_leaderboard_status.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_leaderboard_status.add_theme_font_size_override("font_size", 13)
	_leaderboard_status.modulate = Color.YELLOW
	leaderboard_vbox.add_child(_leaderboard_status)

func _fetch_leaderboard_from_server() -> void:
	_populate_leaderboard()

	var http := HTTPRequest.new()
	add_child(http)
	http.request_completed.connect(_on_leaderboard_received)

	var url := "http://127.0.0.1:5000/api/leaderboard/%s/top?top=20" % _package_id
	var err := http.request(url)
	if err != Error.OK:
		_update_leaderboard_status("❌ 无法连接服务器")
		http.queue_free()

func _on_leaderboard_received(result: int, code: int, _headers: PackedStringArray, body: PackedByteArray) -> void:
	var http_node := get_child(get_child_count() - 1) as HTTPRequest
	if http_node != null:
		http_node.queue_free()

	if result != HTTPRequest.RESULT_SUCCESS or code != 200:
		_update_leaderboard_status("❌ 加载失败 (HTTP %d)" % code)
		return

	var json := JSON.new()
	if json.parse(body.get_string_from_utf8()) != Error.OK:
		_update_leaderboard_status("❌ 数据解析失败")
		return

	var data: Dictionary = json.data
	if not data.get("success", false):
		_update_leaderboard_status("❌ 服务器返回错误")
		return

	var entries: Array = data.get("data", [])
	if entries.is_empty():
		_update_leaderboard_status("暂无排行数据")
		return

	_update_leaderboard_status("")

	var leaderboard_vbox := _leaderboard_tab.get_child(0) as VBoxContainer
	if leaderboard_vbox == null:
		return

	var header_row := HBoxContainer.new()
	header_row.mouse_filter = Control.MOUSE_FILTER_IGNORE
	header_row.add_theme_constant_override("separation", 10)
	leaderboard_vbox.add_child(header_row)

	for h in ["排名", "玩家", "分数", "层数", "击杀", "结果"]:
		var h_label := Label.new()
		h_label.text = h
		h_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		h_label.add_theme_font_size_override("font_size", 12)
		h_label.modulate = Color(0.5, 0.5, 0.5)
		h_label.custom_minimum_size = Vector2(80, 0) if h != "排名" else Vector2(50, 0)
		header_row.add_child(h_label)

	for entry in entries:
		var row := HBoxContainer.new()
		row.mouse_filter = Control.MOUSE_FILTER_IGNORE
		row.add_theme_constant_override("separation", 10)
		leaderboard_vbox.add_child(row)

		var rank: int = entry.get("rank", 0)
		var rank_label := Label.new()
		match rank:
			1: rank_label.text = "🥇"
			2: rank_label.text = "🥈"
			3: rank_label.text = "🥉"
			_: rank_label.text = "#%d" % rank
		rank_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		rank_label.add_theme_font_size_override("font_size", 14)
		rank_label.custom_minimum_size = Vector2(50, 0)
		match rank:
			1: rank_label.modulate = Color(1, 0.85, 0.2)
			2: rank_label.modulate = Color(0.8, 0.8, 0.85)
			3: rank_label.modulate = Color(0.8, 0.55, 0.3)
			_: rank_label.modulate = Color(0.7, 0.7, 0.7)
		row.add_child(rank_label)

		var name_label := Label.new()
		name_label.text = entry.get("username", "???")
		name_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		name_label.add_theme_font_size_override("font_size", 14)
		name_label.custom_minimum_size = Vector2(80, 0)
		row.add_child(name_label)

		var score_label := Label.new()
		score_label.text = "%d" % entry.get("score", 0)
		score_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		score_label.add_theme_font_size_override("font_size", 14)
		score_label.custom_minimum_size = Vector2(80, 0)
		score_label.modulate = Color(1, 0.9, 0.5)
		row.add_child(score_label)

		var floor_label := Label.new()
		floor_label.text = "%d" % entry.get("floorReached", 0)
		floor_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		floor_label.add_theme_font_size_override("font_size", 14)
		floor_label.custom_minimum_size = Vector2(80, 0)
		floor_label.modulate = Color(0.6, 0.8, 0.6)
		row.add_child(floor_label)

		var kill_label := Label.new()
		kill_label.text = "%d" % entry.get("killCount", 0)
		kill_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		kill_label.add_theme_font_size_override("font_size", 14)
		kill_label.custom_minimum_size = Vector2(80, 0)
		row.add_child(kill_label)

		var victory: bool = entry.get("isVictory", false)
		var result_label := Label.new()
		result_label.text = "✅" if victory else "❌"
		result_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		result_label.add_theme_font_size_override("font_size", 14)
		result_label.custom_minimum_size = Vector2(80, 0)
		row.add_child(result_label)

func _update_leaderboard_status(text: String) -> void:
	if _leaderboard_status != null:
		_leaderboard_status.text = text
		if text.begins_with("❌"):
			_leaderboard_status.modulate = Color.RED
		elif text.begins_with("⏳"):
			_leaderboard_status.modulate = Color.YELLOW
		else:
			_leaderboard_status.modulate = Color.GRAY

func _on_back_pressed() -> void:
	print("[PackageDetail] Back pressed")
	back_pressed.emit()

func _on_start_pressed() -> void:
	var supports_multi: bool = _package_data.get("supportsMultiplayer", false)
	if supports_multi:
		_show_mode_select()
	else:
		print("[PackageDetail] Single player only, starting: %s" % _package_id)
		start_game_requested.emit(_package_id)

func _show_mode_select() -> void:
	if _mode_select_panel != null:
		_mode_select_panel.queue_free()
		_mode_select_panel = null

	_mode_select_panel = PanelContainer.new()
	_mode_select_panel.set_anchors_preset(Control.PRESET_CENTER)
	_mode_select_panel.custom_minimum_size = Vector2(420, 340)
	_mode_select_panel.z_index = 100
	_mode_select_panel.mouse_filter = Control.MOUSE_FILTER_STOP

	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.07, 0.06, 0.12, 0.98)
	style.corner_radius_top_left = 20
	style.corner_radius_top_right = 20
	style.corner_radius_bottom_left = 20
	style.corner_radius_bottom_right = 20
	style.border_width_left = 3
	style.border_width_right = 3
	style.border_width_top = 3
	style.border_width_bottom = 3
	style.border_color = Color(0.4, 0.55, 0.85)
	_mode_select_panel.add_theme_stylebox_override("panel", style)
	add_child(_mode_select_panel)

	var vbox := VBoxContainer.new()
	vbox.add_theme_constant_override("separation", 14)
	_mode_select_panel.add_child(vbox)

	var title := Label.new()
	title.text = "🎮 选择游戏模式"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 24)
	title.modulate = Color(1, 0.9, 0.6)
	vbox.add_child(title)

	var max_p: int = _package_data.get("maxPlayers", 4)
	var info := Label.new()
	info.text = "支持最多 %d 人联机" % max_p
	info.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	info.add_theme_font_size_override("font_size", 13)
	info.modulate = Color(0.4, 0.85, 0.6)
	vbox.add_child(info)

	vbox.add_child(HSeparator.new())

	var single_btn := Button.new()
	single_btn.text = "🎯 单人模式"
	single_btn.custom_minimum_size = Vector2(360, 55)
	single_btn.add_theme_font_size_override("font_size", 18)
	single_btn.pressed.connect(func():
		_close_mode_select()
		start_game_requested.emit(_package_id)
	)
	vbox.add_child(single_btn)

	var create_btn := Button.new()
	create_btn.text = "🏠 创建房间"
	create_btn.custom_minimum_size = Vector2(360, 55)
	create_btn.add_theme_font_size_override("font_size", 18)
	create_btn.pressed.connect(func():
		_close_mode_select()
		create_room_requested.emit(_package_id)
	)
	vbox.add_child(create_btn)

	var join_btn := Button.new()
	join_btn.text = "🔍 加入房间"
	join_btn.custom_minimum_size = Vector2(360, 55)
	join_btn.add_theme_font_size_override("font_size", 18)
	join_btn.pressed.connect(func():
		_close_mode_select()
		join_room_requested.emit(_package_id)
	)
	vbox.add_child(join_btn)

	var auth_system = _get_auth_system()
	if auth_system == null or not auth_system.IsAuthenticated:
		create_btn.disabled = true
		join_btn.disabled = true
		create_btn.tooltip_text = "需要先登录"
		join_btn.tooltip_text = "需要先登录"

	var cancel_btn := Button.new()
	cancel_btn.text = "取消"
	cancel_btn.custom_minimum_size = Vector2(120, 36)
	cancel_btn.pressed.connect(_close_mode_select)
	vbox.add_child(cancel_btn)

func _close_mode_select() -> void:
	if _mode_select_panel != null:
		_mode_select_panel.queue_free()
		_mode_select_panel = null

func _get_auth_system():
	var node = get_node_or_null("/root/AuthSystem")
	if node != null:
		return node
	return null
