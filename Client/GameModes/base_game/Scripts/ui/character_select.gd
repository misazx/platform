class_name CharacterSelect extends Control

signal character_selected(character_id: String)

var _character_grid: GridContainer
var _name_label: Label
var _class_label: Label
var _desc_label: RichTextLabel
var _confirm_button: Button
var _back_button: Button

var _selected_index: int = -1
var _characters: Array = []
var _character_cards: Array = []

var _is_multiplayer: bool = false
var _multiplayer_status_label: Label = null

func _ready() -> void:
	_setup_node_references()
	_setup_signals()
	_detect_multiplayer_mode()
	load_characters()
	print("[CharacterSelect] Ready")

func _detect_multiplayer_mode() -> void:
	var mp_bridge = get_node_or_null("/root/MultiplayerBridge")
	if mp_bridge != null and mp_bridge.has_method("is_multiplayer_game"):
		_is_multiplayer = mp_bridge.is_multiplayer_game()
	if _is_multiplayer:
		_multiplayer_status_label = Label.new()
		_multiplayer_status_label.text = "🌐 多人合作模式 - 选择你的角色"
		_multiplayer_status_label.modulate = Color(0.5, 0.9, 1.0)
		_multiplayer_status_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		_multiplayer_status_label.add_theme_font_size_override("font_size", 14)
		_multiplayer_status_label.set_anchors_preset(Control.PRESET_TOP_WIDE)
		_multiplayer_status_label.position = Vector2(0, 5)
		_multiplayer_status_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		add_child(_multiplayer_status_label)
		_connect_multiplayer_signals()
		print("[CharacterSelect] Multiplayer mode detected")

func _connect_multiplayer_signals() -> void:
	var mp_bridge = get_node_or_null("/root/MultiplayerBridge")
	if mp_bridge == null:
		return
	if mp_bridge.has_signal("bridge_player_ready_changed"):
		mp_bridge.bridge_player_ready_changed.connect(_on_remote_player_ready_changed)

func _setup_node_references() -> void:
	_character_grid = get_node_or_null("CharGrid")
	_name_label = get_node_or_null("DescriptionPanel/DescVBox/NameLabel")
	_class_label = get_node_or_null("DescriptionPanel/DescVBox/ClassLabel")
	_desc_label = get_node_or_null("DescriptionPanel/DescVBox/DescLabel")
	_confirm_button = get_node_or_null("BottomBar/ConfirmButton")
	_back_button = get_node_or_null("HeaderBar/BackButton")

	if _character_grid == null:
		push_error("[CharacterSelect] CharGrid not found")
	if _desc_label == null:
		push_error("[CharacterSelect] DescLabel not found")

func _setup_signals() -> void:
	if _confirm_button != null:
		_confirm_button.pressed.connect(_on_confirm_pressed)
		_confirm_button.disabled = true
	if _back_button != null:
		_back_button.pressed.connect(_on_back_pressed)

func load_characters() -> void:
	var db = CharacterDatabase
	if db == null:
		push_error("[CharacterSelect] CharacterDatabase instance is null!")
		return

	_characters = CharacterDatabase.get_all_characters()
	if _characters.is_empty():
		var all_chars := [
			StsExpansionSystem.CharacterData.create_ironclad(),
			StsExpansionSystem.CharacterData.create_silent(),
			StsExpansionSystem.CharacterData.create_defect(),
			StsExpansionSystem.CharacterData.create_watcher()
		]
		_characters = all_chars
	print("[CharacterSelect] Loaded %d characters" % _characters.size())

	for child in _character_grid.get_children():
		child.queue_free()
	_character_cards.clear()

	for i in range(_characters.size()):
		var character = _characters[i]
		var card := CharacterCardControl.new(i, character)
		card.card_clicked.connect(_on_card_clicked)
		_character_cards.append(card)
		_character_grid.add_child(card)

	update_description(-1)

func _on_card_clicked(index: int) -> void:
	print("[CharacterSelect] Card clicked: %d" % index)
	select_character(index)

func select_character(index: int) -> void:
	_selected_index = index

	if _confirm_button != null:
		_confirm_button.disabled = false

	for card in _character_cards:
		card.set_selected(card.index == index)

	update_description(index)
	if AudioManager.has_method("play_button_click"):
		AudioManager.play_button_click()

func update_description(index: int) -> void:
	if index < 0 or index >= _characters.size():
		if _name_label != null: _name_label.text = "选择一个角色"
		if _class_label != null: _class_label.text = ""
		if _desc_label != null: _desc_label.text = "点击上方角色卡片查看详情"
		return

	var character = _characters[index]
	var char_name: String = ""
	var char_desc: String = ""
	var char_class: int = 0

	if character is Dictionary:
		char_name = character.get("name", "")
		char_desc = character.get("description", "")
		char_class = character.get("class", 0)
	else:
		if "name" in character: char_name = str(character.name)
		if "description" in character: char_desc = str(character.description)
		if "character" in character: char_class = int(character.character)

	if _name_label != null:
		_name_label.text = char_name
	if _class_label != null:
		_class_label.text = _get_class_display_name(char_class)
	if _desc_label != null:
		_desc_label.text = "%s\n\n%s\n\n难度: 简单" % [char_name, char_desc]

func _on_confirm_pressed() -> void:
	if _selected_index >= 0 and _selected_index < _characters.size():
		var selected_char = _characters[_selected_index]
		var character_id: String = ""
		var character_name: String = ""
		var max_hp: int = 80
		var starting_gold: int = 99
		var starting_deck: Array = []
		var starting_relic: String = ""

		if selected_char is Dictionary:
			character_id = selected_char.get("id", str(_selected_index))
			character_name = selected_char.get("name", "")
			max_hp = int(selected_char.get("max_health", 80))
			starting_gold = int(selected_char.get("starting_gold", 99))
			starting_deck = selected_char.get("starting_cards", [])
			starting_relic = selected_char.get("starting_relic_id", "")
		else:
			if "id" in selected_char: character_id = str(selected_char.id)
			if "name" in selected_char: character_name = str(selected_char.name)
			if "max_hp" in selected_char: max_hp = int(selected_char.max_hp)
			if "starting_gold" in selected_char: starting_gold = int(selected_char.starting_gold)
			if "starting_deck" in selected_char: starting_deck = selected_char.starting_deck
			if "starting_relic_id" in selected_char: starting_relic = str(selected_char.starting_relic_id)

		print("[CharacterSelect] Confirming character: %s (%s)" % [character_id, character_name])
		if AudioManager.has_method("play_button_click"):
			AudioManager.play_button_click()

		character_selected.emit(character_id)

		var game_manager := get_tree().root.get_node_or_null("/root/Main") as Node
		if game_manager != null:
			var deck_strings: PackedStringArray = PackedStringArray()
			for card_id: String in starting_deck:
				deck_strings.append(card_id)
			if game_manager.has_method("SetSelectedCharacter"):
				game_manager.call("SetSelectedCharacter", character_id, character_name, max_hp, starting_gold, deck_strings, starting_relic)
			if _is_multiplayer:
				_notify_character_ready(character_id)
				if _multiplayer_status_label != null:
					_multiplayer_status_label.text = "✅ 角色已选择: %s - 等待其他玩家..." % character_name
					_multiplayer_status_label.modulate = Color(0.5, 1.0, 0.5)
				if _confirm_button != null:
					_confirm_button.disabled = true
					_confirm_button.text = "已确认"
			else:
				if game_manager.has_method("GoToMap"):
					game_manager.call("GoToMap")
		else:
			get_tree().change_scene_to_file("res://GameModes/base_game/Scenes/MapScene.tscn")

func _notify_character_ready(character_id: String) -> void:
	var hub_client = get_node_or_null("/root/GameHubClient")
	if hub_client != null and hub_client.has_method("notify_ready_changed_async"):
		var room_id: String = hub_client.call("get_current_room_id") if hub_client.has_method("get_current_room_id") else ""
		if room_id != "":
			hub_client.call("notify_ready_changed_async", room_id, true)
	print("[CharacterSelect] Notified server: character=%s ready=true" % character_id)

func _on_remote_player_ready_changed(player_id: String, is_ready: bool) -> void:
	print("[CharacterSelect] Remote player %s ready: %s" % [player_id, str(is_ready)])
	if _multiplayer_status_label != null:
		var status_text: String = "✅ 角色已选择 - 等待其他玩家..."
		if is_ready:
			status_text = "✅ 对手已准备 - 等待游戏开始..."
		_multiplayer_status_label.text = status_text

func _on_back_pressed() -> void:
	print("[CharacterSelect] Back pressed")
	if AudioManager.has_method("play_button_click"):
		AudioManager.play_button_click()
	var main_node := get_tree().root.get_node_or_null("/root/Main") as Node
	if main_node != null and main_node.has_method("go_to_main_menu"):
		main_node.call("go_to_main_menu")
	elif get_tree().current_scene != null:
		get_tree().change_scene_to_file("res://Scenes/Main.tscn")

func _get_class_display_name(char_class: int) -> String:
	match char_class:
		StsExpansionSystem.Character.IRONCLAD: return "战士"
		StsExpansionSystem.Character.SILENT: return "猎人"
		StsExpansionSystem.Character.DEFECT: return "机器人"
		StsExpansionSystem.Character.WATCHER: return "观者"
		_: return "未知"


class CharacterCardControl:
	extends PanelContainer

	signal card_clicked(index: int)

	var index: int
	var data: Dictionary
	var _selected: bool = false
	var _portrait_rect: TextureRect
	var _name_lbl: Label

	func _init(p_index: int, p_data: Dictionary) -> void:
		index = p_index
		if p_data is Dictionary:
			data = p_data
		else:
			data = _convert_to_dict(p_data)

	func _convert_to_dict(obj) -> Dictionary:
		var d: Dictionary = {}
		if "id" in obj: d["id"] = str(obj.id) if obj.has("id") else str(index)
		if "name" in obj: d["name"] = str(obj.name)
		if "description" in obj: d["description"] = str(obj.description)
		if "max_hp" in obj: d["max_health"] = int(obj.max_hp)
		if "starting_gold" in obj: d["starting_gold"] = int(obj.starting_gold)
		if "starting_deck" in obj: d["starting_cards"] = obj.starting_deck
		if "starting_relic_id" in obj: d["starting_relic_id"] = str(obj.starting_relic_id)
		if "portrait_path" in obj: d["portrait_path"] = str(obj.portrait_path)
		if "color" in obj: d["color"] = obj.color
		if "character" in obj: d["class"] = int(obj.character)
		if d.is_empty():
			d = {"id": str(index), "name": "Unknown"}
		return d

	func _ready() -> void:
		custom_minimum_size = Vector2(150, 180)
		mouse_filter = Control.MOUSE_FILTER_STOP

		var style: StyleBoxTexture = UITheme.make_card_panel_style()
		add_theme_stylebox_override("panel", style)
		self_modulate = _get_data_color(data)

		var vbox := VBoxContainer.new()
		add_child(vbox)

		_portrait_rect = TextureRect.new()
		_portrait_rect.custom_minimum_size = Vector2(130, 110)
		_portrait_rect.expand_mode = TextureRect.EXPAND_FIT_WIDTH_PROPORTIONAL
		_portrait_rect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		_portrait_rect.modulate = Color(0.5, 0.5, 0.5)

		var portrait_path: String = _get_data_portrait(data)
		if portrait_path != "" and ResourceLoader.exists(portrait_path):
			_portrait_rect.texture = load(portrait_path)
			_portrait_rect.modulate = Color.WHITE
		vbox.add_child(_portrait_rect)

		_name_lbl = Label.new()
		_name_lbl.text = data.get("name", "")
		_name_lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		_name_lbl.modulate = Color.WHITE
		vbox.add_child(_name_lbl)

		gui_input.connect(_on_gui_input)
		mouse_entered.connect(_on_mouse_entered)
		mouse_exited.connect(_on_mouse_exited)

	func set_selected(selected: bool) -> void:
		_selected = selected
		self_modulate = Color.GOLD if selected else _get_data_color(data)
		if selected:
			create_tween().tween_property(self, "scale", Vector2(1.05, 1.05), 0.1)
		else:
			create_tween().tween_property(self, "scale", Vector2.ONE, 0.1)

	func _on_gui_input(event: InputEvent) -> void:
		if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
			card_clicked.emit(index)

	func _on_mouse_entered() -> void:
		if not _selected:
			create_tween().tween_property(self, "scale", Vector2(1.02, 1.02), 0.1)

	func _on_mouse_exited() -> void:
		if not _selected:
			create_tween().tween_property(self, "scale", Vector2.ONE, 0.1)

	func _get_data_portrait(d: Dictionary) -> String:
		if d.has("portrait_path"):
			var v = d["portrait_path"]
			if v is String: return v
		if d.has("portraitPath"):
			var v2 = d["portraitPath"]
			if v2 is String: return v2
		return ""

	func _get_data_color(d: Dictionary) -> Color:
		if d.has("color"):
			var v = d["color"]
			if v is Color: return v
		if d.has("background_color"):
			var bg: String = str(d["background_color"])
			if bg.begins_with("#"):
				return Color.from_string(bg, Color.WHITE)
		return Color.WHITE
