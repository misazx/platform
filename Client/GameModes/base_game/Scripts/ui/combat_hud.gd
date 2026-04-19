class_name CombatHUD extends Control

signal card_played(card_id: String)
signal end_turn()
signal card_played_with_target(card_id: String, target_index: int)
signal show_pile_view_requested(pile_name: String)
signal combat_won()
signal combat_lost()

var _root_container: Control
var _combat_bg: ColorRect
var _floor_label: Label
var _turn_label: Label
var _energy_label: Label
var _phase_label: Label
var _battle_scene_area: Control
var _enemy_sprites: Array = []
var _player_sprite: Control = null
var _enemy_status_area: Control
var _enemies_ui: Array = []
var _player_status_area: VBoxContainer
var _player_avatar: TextureRect
var _player_health_bar: ProgressBar
var _player_health_text: Label
var _player_block_bar: ProgressBar
var _player_block_text: Label
var _hand_area: Control
var _hand_cards: Array = []
var _draw_pile_btn: Button
var _end_turn_btn: Button
var _discard_pile_btn: Button
var _hand_count_label: Label

var _current_energy: int = 3
var _max_energy: int = 3
var _is_player_turn: bool = true
var _is_processing: bool = false
var _is_selecting_target: bool = false
var _pending_card: Control = null

var _is_coop_mode: bool = false
var _combat_system: Node = null
var _coop_player_status_areas: Array = []
var _local_player_index: int = 0
var _player_count: int = 1
var _coop_turn_indicator: Label = null

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_layout()
	_connect_signals()

func _create_layout() -> void:
	_root_container = Control.new()
	_root_container.set_anchors_preset(Control.PRESET_FULL_RECT)
	_root_container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_root_container)

	_create_combat_background()
	_create_top_bar()
	_create_battle_scene_view()
	_create_enemy_status_bar()
	_create_player_status_bar()
	_create_hand_area()
	_create_bottom_buttons()

func _create_combat_background() -> void:
	var bg_texture := load("res://GameModes/base_game/Resources/Images/Backgrounds/hive.png") as Texture2D
	if bg_texture != null:
		var bg_image := TextureRect.new()
		bg_image.set_anchors_preset(Control.PRESET_FULL_RECT)
		bg_image.texture = bg_texture
		bg_image.stretch_mode = TextureRect.STRETCH_SCALE
		bg_image.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_root_container.add_child(bg_image)

		var bg_overlay := ColorRect.new()
		bg_overlay.set_anchors_preset(Control.PRESET_FULL_RECT)
		bg_overlay.color = Color(0, 0, 0, 0.35)
		bg_overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_root_container.add_child(bg_overlay)
	else:
		_combat_bg = ColorRect.new()
		_combat_bg.set_anchors_preset(Control.PRESET_FULL_RECT)
		_combat_bg.color = Color(0.06, 0.04, 0.08, 1)
		_combat_bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_root_container.add_child(_combat_bg)

	var floor_grad := ColorRect.new()
	floor_grad.custom_minimum_size = Vector2(1280, 80)
	floor_grad.color = Color(0.04, 0.03, 0.02, 0.5)
	floor_grad.position = Vector2(0, 430)
	floor_grad.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_root_container.add_child(floor_grad)

func _create_top_bar() -> void:
	var top_bar := HBoxContainer.new()
	top_bar.custom_minimum_size = Vector2(0, 26)
	top_bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
	top_bar.position = Vector2(12, 4)
	_root_container.add_child(top_bar)

	_floor_label = Label.new()
	_floor_label.text = "第 1 层"
	_floor_label.modulate = Color.GRAY
	_floor_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_floor_label.add_theme_font_size_override("font_size", 11)
	top_bar.add_child(_floor_label)

	var spacer1 := Control.new()
	spacer1.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	spacer1.mouse_filter = Control.MOUSE_FILTER_IGNORE
	top_bar.add_child(spacer1)

	_phase_label = Label.new()
	_phase_label.text = "你的回合"
	_phase_label.modulate = Color(0.5, 0.9, 0.5)
	_phase_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_phase_label.add_theme_font_size_override("font_size", 12)
	top_bar.add_child(_phase_label)

	var spacer2 := Control.new()
	spacer2.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	spacer2.mouse_filter = Control.MOUSE_FILTER_IGNORE
	top_bar.add_child(spacer2)

	_turn_label = Label.new()
	_turn_label.text = "回合 1"
	_turn_label.modulate = Color(1, 0.92, 0.3)
	_turn_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_turn_label.add_theme_font_size_override("font_size", 11)
	top_bar.add_child(_turn_label)

	_energy_label = Label.new()
	_energy_label.text = "3/3"
	_energy_label.modulate = Color(1, 0.85, 0.2)
	_energy_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_energy_label.add_theme_font_size_override("font_size", 14)
	top_bar.add_child(UITheme.make_icon_rect("icon_coin", Vector2(16, 16)))
	top_bar.add_child(_energy_label)

func _create_battle_scene_view() -> void:
	_battle_scene_area = Control.new()
	_battle_scene_area.custom_minimum_size = Vector2(1280, 340)
	_battle_scene_area.position = Vector2(0, 32)
	_battle_scene_area.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_root_container.add_child(_battle_scene_area)

	_player_sprite = BattleCharacterSprite.new("ironclad", true)
	_player_sprite.custom_minimum_size = Vector2(140, 170)
	_player_sprite.position = Vector2(60, 140)
	_battle_scene_area.add_child(_player_sprite)

func add_enemy(enemy_name: String, max_hp: int) -> void:
	var sprite := BattleCharacterSprite.new(enemy_name, false)
	sprite.custom_minimum_size = Vector2(160, 200)
	_enemy_sprites.append(sprite)
	_battle_scene_area.add_child(sprite)
	_update_battle_sprite_positions()

	var status_ui := EnemyUnitUI.new(enemy_name, max_hp)
	status_ui.custom_minimum_size = Vector2(140, 65)
	status_ui.set_enemy_index(_enemies_ui.size())
	status_ui.enemy_clicked.connect(_on_enemy_clicked)
	_enemies_ui.append(status_ui)
	_enemy_status_area.add_child(status_ui)
	_update_enemy_status_positions()

func _update_battle_sprite_positions() -> void:
	var count := _enemy_sprites.size()
	if count == 0: return
	
	var screen_width := 1280.0
	var total_width := count * 160 + (count - 1) * 30
	var start_x := (screen_width - total_width) / 2.0 + 120
	
	for i in range(count):
		_enemy_sprites[i].position = Vector2(start_x + i * 190, 20)

func _create_enemy_status_bar() -> void:
	_enemy_status_area = Control.new()
	_enemy_status_area.custom_minimum_size = Vector2(1280, 70)
	_enemy_status_area.position = Vector2(0, 240)
	_enemy_status_area.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_root_container.add_child(_enemy_status_area)

func _update_enemy_status_positions() -> void:
	var count := _enemies_ui.size()
	if count == 0: return
	
	var screen_width := 1280.0
	var total_width := count * 140 + (count - 1) * 20
	var start_x := (screen_width - total_width) / 2.0 + 140
	
	for i in range(count):
		_enemies_ui[i].position = Vector2(start_x + i * 160, 0)

func update_enemy_health(enemy_index: int, current_hp: int, max_hp: int) -> void:
	if enemy_index >= 0 and enemy_index < _enemies_ui.size():
		_enemies_ui[enemy_index].update_health(current_hp, max_hp)

func update_enemy_intent(enemy_index: int, intent_text: String, intent_icon: String = "") -> void:
	if enemy_index >= 0 and enemy_index < _enemies_ui.size():
		_enemies_ui[enemy_index].update_intent(intent_text, intent_icon)

func _create_player_status_bar() -> void:
	_player_status_area = VBoxContainer.new()
	_player_status_area.custom_minimum_size = Vector2(220, 80)
	_player_status_area.position = Vector2(12, 330)
	_player_status_area.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_root_container.add_child(_player_status_area)

	var header_row := HBoxContainer.new()
	header_row.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_player_status_area.add_child(header_row)

	_player_avatar = TextureRect.new()
	_player_avatar.custom_minimum_size = Vector2(28, 28)
	_player_avatar.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_COVERED
	_player_avatar.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var player_texture := load("res://GameModes/base_game/Resources/Icons/Items/iron_sword.png") as Texture2D
	if player_texture != null:
		_player_avatar.texture = player_texture
	header_row.add_child(_player_avatar)

	var name_label := Label.new()
	name_label.text = "铁甲战士"
	name_label.modulate = Color.WHITE
	name_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	name_label.add_theme_font_size_override("font_size", 12)
	header_row.add_child(name_label)

	_player_health_text = Label.new()
	_player_health_text.text = "80/80"
	_player_health_text.modulate = Color(1, 0.4, 0.4)
	_player_health_text.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_player_health_text.add_theme_font_size_override("font_size", 11)
	header_row.add_child(_player_health_text)

	_player_health_bar = ProgressBar.new()
	_player_health_bar.max_value = 100
	_player_health_bar.value = 80
	_player_health_bar.custom_minimum_size = Vector2(210, 16)
	_player_health_bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_player_health_bar.add_theme_stylebox_override("background", UITheme.make_bar_bg_style())
	var health_style := StyleBoxFlat.new()
	health_style.bg_color = Color(0.85, 0.2, 0.2)
	health_style.set_corner_radius_all(4)
	_player_health_bar.add_theme_stylebox_override("fill", health_style)
	_player_status_area.add_child(_player_health_bar)

	var block_row := HBoxContainer.new()
	block_row.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_player_status_area.add_child(block_row)

	_player_block_text = Label.new()
	_player_block_text.text = ""
	_player_block_text.modulate = Color(0.4, 0.6, 1)
	_player_block_text.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_player_block_text.add_theme_font_size_override("font_size", 10)
	block_row.add_child(_player_block_text)

	_player_block_bar = ProgressBar.new()
	_player_block_bar.max_value = 50
	_player_block_bar.value = 0
	_player_block_bar.custom_minimum_size = Vector2(140, 10)
	_player_block_bar.visible = false
	_player_block_bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_player_block_bar.add_theme_stylebox_override("background", UITheme.make_bar_bg_style())
	var block_style := StyleBoxFlat.new()
	block_style.bg_color = Color(0.3, 0.5, 1, 0.9)
	block_style.set_corner_radius_all(3)
	_player_block_bar.add_theme_stylebox_override("fill", block_style)
	block_row.add_child(_player_block_bar)

func _create_hand_area() -> void:
	_hand_area = Control.new()
	_hand_area.custom_minimum_size = Vector2(1240, 130)
	_hand_area.position = Vector2(20, 415)
	_hand_area.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_root_container.add_child(_hand_area)

	_hand_count_label = Label.new()
	_hand_count_label.text = "手牌: 0"
	_hand_count_label.modulate = Color(0.7, 0.7, 0.7, 0.8)
	_hand_count_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_hand_count_label.add_theme_font_size_override("font_size", 10)
	_hand_count_label.position = Vector2(540, -14)
	_hand_area.add_child(_hand_count_label)

	for i in range(10):
		var card := CardUI.new(i)
		card.custom_minimum_size = Vector2(105, 130)
		card.card_pressed.connect(_on_card_pressed)
		_hand_cards.append(card)
		_hand_area.add_child(card)

	_update_card_positions()

func _update_card_positions() -> void:
	var visible_count := 0
	for card in _hand_cards:
		if card.visible:
			visible_count += 1
	if visible_count == 0: return

	var center_x := 600.0
	var base_y := 0.0
	var card_spacing := 112.0
	var start_offset_x := -(visible_count - 1) * card_spacing / 2.0
	var visible_index := 0

	for i in range(_hand_cards.size()):
		if not _hand_cards[i].visible: continue
		var x_offset := start_offset_x + visible_index * card_spacing
		var y_offset := absf(x_offset) * 0.04
		_hand_cards[i].position = Vector2(center_x + x_offset - 55, base_y + y_offset)
		visible_index += 1

func _create_bottom_buttons() -> void:
	var bottom_bar := HBoxContainer.new()
	bottom_bar.custom_minimum_size = Vector2(1280, 42)
	bottom_bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
	bottom_bar.alignment = BoxContainer.ALIGNMENT_CENTER
	bottom_bar.add_theme_constant_override("separation", 25)
	bottom_bar.position = Vector2(0, 555)
	bottom_bar.z_index = 10
	_root_container.add_child(bottom_bar)

	_draw_pile_btn = UITheme.make_small_button("抽牌堆 (45)", "", Vector2(120, 34))
	_draw_pile_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	_draw_pile_btn.pressed.connect(_on_draw_pile_clicked)
	bottom_bar.add_child(_draw_pile_btn)

	_end_turn_btn = UITheme.make_button("⚔ 结束回合", "", Vector2(140, 38))
	_end_turn_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	_end_turn_btn.pressed.connect(_on_end_turn_clicked)
	bottom_bar.add_child(_end_turn_btn)

	_discard_pile_btn = UITheme.make_small_button("弃牌堆 (0)", "", Vector2(120, 34))
	_discard_pile_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	_discard_pile_btn.pressed.connect(_on_discard_pile_clicked)
	bottom_bar.add_child(_discard_pile_btn)

func _connect_signals() -> void:
	_detect_multiplayer_mode()
	_connect_bridge_signals()

func _detect_multiplayer_mode() -> void:
	var mp_bridge = get_node_or_null("/root/MultiplayerBridge")
	if mp_bridge != null and mp_bridge.has_method("is_multiplayer_game"):
		_is_coop_mode = mp_bridge.is_multiplayer_game()
	if _is_coop_mode:
		if mp_bridge.has_method("get_local_player_index"):
			_local_player_index = mp_bridge.get_local_player_index()
		if mp_bridge.has_method("get_player_count"):
			_player_count = mp_bridge.get_player_count()
		print("[CombatHUD] 多人合作模式: player_count=%d local_index=%d" % [_player_count, _local_player_index])

func _initialize_combat_system() -> void:
	if _is_coop_mode:
		var coop_script := load("res://GameModes/base_game/Scripts/combat/coop_combat_system.gd") as GDScript
		if coop_script == null:
			push_error("[CombatHUD] Failed to load CoopCombatEngine script!")
			return
		_combat_system = Node.new()
		_combat_system.set_script(coop_script)
		add_child(_combat_system)

		if _combat_system.has_signal("coop_combat_won"):
			_combat_system.coop_combat_won.connect(func(): combat_won.emit())
		if _combat_system.has_signal("coop_combat_lost"):
			_combat_system.coop_combat_lost.connect(func(): combat_lost.emit())
		if _combat_system.has_signal("coop_turn_started"):
			_combat_system.coop_turn_started.connect(_on_coop_turn_started)
		if _combat_system.has_signal("coop_card_played"):
			_combat_system.coop_card_played.connect(_on_coop_card_played)
		if _combat_system.has_signal("coop_damage_dealt"):
			_combat_system.coop_damage_dealt.connect(_on_coop_damage_dealt)
		if _combat_system.has_signal("coop_block_gained"):
			_combat_system.coop_block_gained.connect(_on_coop_block_gained)

		_create_coop_player_status_areas()

		var mp_bridge = get_node_or_null("/root/MultiplayerBridge")
		var seed_val: int = 0
		if mp_bridge != null and mp_bridge.has_method("get_sync_seed"):
			seed_val = mp_bridge.get_sync_seed()

		var main_node := get_tree().root.get_node_or_null("/root/Main") as Node
		var char_id: String = "ironclad"
		if main_node != null and main_node.has_method("GetSelectedCharacterId"):
			char_id = str(main_node.call("GetSelectedCharacterId"))

		var enemies_data: Array = _get_enemies_for_combat()
		_combat_system.call("initialize_coop_combat", enemies_data, _player_count, seed_val, _local_player_index)
		print("[CombatHUD] Coop combat initialized: enemies=%d players=%d seed=%d" % [enemies_data.size(), _player_count, seed_val])
		_sync_coop_ui_from_system()
	else:
		var combat_script := load("res://GameModes/base_game/Scripts/combat/sts_combat_system.gd") as GDScript
		if combat_script == null:
			push_error("[CombatHUD] Failed to load StsCombatEngine script!")
			return
		_combat_system = Node.new()
		_combat_system.set_script(combat_script)
		add_child(_combat_system)

		if _combat_system.has_signal("combat_won"):
			_combat_system.combat_won.connect(func(): combat_won.emit())
		if _combat_system.has_signal("combat_lost"):
			_combat_system.combat_lost.connect(func(): combat_lost.emit())
		if _combat_system.has_signal("damage_dealt"):
			_combat_system.damage_dealt.connect(_on_damage_dealt)
		if _combat_system.has_signal("block_gained"):
			_combat_system.block_gained.connect(_on_block_gained)
		if _combat_system.has_signal("turn_started"):
			_combat_system.turn_started.connect(_on_turn_started)
		if _combat_system.has_signal("turn_ended"):
			_combat_system.turn_ended.connect(_on_turn_ended)

		var main_node := get_tree().root.get_node_or_null("/root/Main") as Node
		var char_id: String = "ironclad"
		if main_node != null and main_node.has_method("GetSelectedCharacterId"):
			char_id = str(main_node.call("GetSelectedCharacterId"))
		_combat_system.call("initialize_combat", _get_enemies_for_combat(), 0)
		print("[CombatHUD] Single player combat initialized with character: %s" % char_id)
		_sync_single_ui_from_system()

func _connect_bridge_signals() -> void:
	var mp_bridge = get_node_or_null("/root/MultiplayerBridge")
	if mp_bridge == null:
		return
	if mp_bridge.has_signal("bridge_coop_card_played"):
		mp_bridge.bridge_coop_card_played.connect(_on_bridge_coop_card_played)
	if mp_bridge.has_signal("bridge_coop_turn_ended"):
		mp_bridge.bridge_coop_turn_ended.connect(_on_bridge_coop_turn_ended)
	print("[CombatHUD] Bridge signals connected")

func _get_enemies_for_combat() -> Array:
	var main_node := get_tree().root.get_node_or_null("/root/Main") as Node
	var enemy_id: String = ""
	if main_node != null and main_node.has_method("GetLastClickedEnemyId"):
		enemy_id = str(main_node.call("GetLastClickedEnemyId"))

	var enemy_db = get_node_or_null("/root/EnemyDatabase")
	if enemy_db != null and enemy_db.has_method("get_enemy"):
		var enemy_data = enemy_db.call("get_enemy", enemy_id if enemy_id != "" else "cultist")
		if enemy_data != null and not enemy_data.is_empty():
			return [enemy_data]

	return [
		{"id": "cultist", "name": "Cultist", "max_hp": 50, "current_hp": 50, "block": 0, "status_effects": []}
	]

func _create_coop_player_status_areas() -> void:
	for i in range(_player_count):
		var status_area := VBoxContainer.new()
		status_area.custom_minimum_size = Vector2(160, 55)
		status_area.mouse_filter = Control.MOUSE_FILTER_IGNORE
		status_area.position = Vector2(12, 330 + i * 60)

		var header := HBoxContainer.new()
		header.mouse_filter = Control.MOUSE_FILTER_IGNORE
		status_area.add_child(header)

		var name_lbl := Label.new()
		name_lbl.text = "玩家%d" % (i + 1) if i != _local_player_index else "玩家%d (你)" % (i + 1)
		name_lbl.modulate = Color(0.5, 0.9, 1.0) if i == _local_player_index else Color(0.8, 0.8, 0.8)
		name_lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
		name_lbl.add_theme_font_size_override("font_size", 10)
		header.add_child(name_lbl)

		var hp_lbl := Label.new()
		hp_lbl.text = "80/80"
		hp_lbl.modulate = Color(1, 0.4, 0.4)
		hp_lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
		hp_lbl.add_theme_font_size_override("font_size", 9)
		header.add_child(hp_lbl)

		var energy_lbl := Label.new()
		energy_lbl.text = "3/3"
		energy_lbl.modulate = Color(1, 0.85, 0.2)
		energy_lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
		energy_lbl.add_theme_font_size_override("font_size", 9)
		header.add_child(energy_lbl)

		var hp_bar := ProgressBar.new()
		hp_bar.max_value = 80
		hp_bar.value = 80
		hp_bar.custom_minimum_size = Vector2(150, 10)
		hp_bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
		hp_bar.add_theme_stylebox_override("background", UITheme.make_bar_bg_style())
		var hp_style := StyleBoxFlat.new()
		hp_style.bg_color = Color(0.85, 0.2, 0.2)
		hp_style.set_corner_radius_all(3)
		hp_bar.add_theme_stylebox_override("fill", hp_style)
		status_area.add_child(hp_bar)

		_coop_player_status_areas.append({
			"area": status_area,
			"name_label": name_lbl,
			"hp_label": hp_lbl,
			"energy_label": energy_lbl,
			"hp_bar": hp_bar,
			"player_index": i,
		})
		_root_container.add_child(status_area)

	_coop_turn_indicator = Label.new()
	_coop_turn_indicator.text = "玩家1的回合"
	_coop_turn_indicator.modulate = Color(1, 0.92, 0.3)
	_coop_turn_indicator.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_coop_turn_indicator.add_theme_font_size_override("font_size", 13)
	_coop_turn_indicator.position = Vector2(500, 4)
	_root_container.add_child(_coop_turn_indicator)

	_player_status_area.visible = false

func _sync_coop_ui_from_system() -> void:
	if _combat_system == null or not _combat_system.has_method("get_enemies"):
		return

	var enemies: Array = _combat_system.call("get_enemies")
	for i in range(enemies.size()):
		var e: Dictionary = enemies[i]
		if i >= _enemies_ui.size():
			add_enemy(e.get("name", "Enemy"), e.get("max_hp", 50))
		else:
			update_enemy_health(i, e.get("current_hp", 0), e.get("max_hp", 50))
			if e.has("current_intent") and e.current_intent != null:
				var intent: Dictionary = e.current_intent
				update_enemy_intent(i, intent.get("description", ""), intent.get("icon", ""))

	for i in range(_player_count):
		if i < _coop_player_status_areas.size():
			var player_data: Dictionary = {}
			if _combat_system.has_method("get_player_by_index"):
				player_data = _combat_system.call("get_player_by_index", i)
			if not player_data.is_empty():
				_update_coop_player_status(i, player_data)

	if _combat_system.has_method("is_local_player_turn"):
		var is_my_turn: bool = _combat_system.call("is_local_player_turn")
		set_phase(is_my_turn)
		if _coop_turn_indicator != null:
			var current_idx: int = _combat_system.call("get_current_player_index") if _combat_system.has_method("get_current_player_index") else 0
			_coop_turn_indicator.text = "玩家%d的回合" % (current_idx + 1)
			if is_my_turn:
				_coop_turn_indicator.modulate = Color(0.5, 0.9, 0.5)
			else:
				_coop_turn_indicator.modulate = Color(0.9, 0.4, 0.3)

	if _combat_system.has_method("get_local_player"):
		var local_player: Dictionary = _combat_system.call("get_local_player")
		if not local_player.is_empty():
			update_energy(local_player.get("energy", 0), local_player.get("max_energy", 3))
			update_health(local_player.get("current_hp", 0), local_player.get("max_hp", 80))
			update_block(local_player.get("block", 0))
			if local_player.has("hand"):
				update_hand(local_player.hand)

	if _combat_system.has_method("get_turn_number"):
		set_turn_number(_combat_system.call("get_turn_number"))

func _sync_single_ui_from_system() -> void:
	if _combat_system == null:
		return

	if _combat_system.has_method("get_enemies"):
		var enemies: Array = _combat_system.call("get_enemies")
		for i in range(enemies.size()):
			var e: Dictionary = enemies[i]
			if i >= _enemies_ui.size():
				add_enemy(e.get("name", "Enemy"), e.get("max_hp", 50))
			else:
				update_enemy_health(i, e.get("current_hp", 0), e.get("max_hp", 50))

	if _combat_system.has_method("get_player"):
		var player: Dictionary = _combat_system.call("get_player")
		if not player.is_empty():
			update_energy(player.get("energy", 0), player.get("max_energy", 3))
			update_health(player.get("current_hp", 0), player.get("max_hp", 80))
			update_block(player.get("block", 0))
			if player.has("hand"):
				update_hand(player.hand)

	if _combat_system.has_method("get_turn_number"):
		set_turn_number(_combat_system.call("get_turn_number"))

func _update_coop_player_status(player_index: int, player_data: Dictionary) -> void:
	if player_index < 0 or player_index >= _coop_player_status_areas.size():
		return
	var status: Dictionary = _coop_player_status_areas[player_index]
	var hp_lbl: Label = status.get("hp_label")
	var energy_lbl: Label = status.get("energy_label")
	var hp_bar: ProgressBar = status.get("hp_bar")
	if hp_lbl != null:
		hp_lbl.text = "%d/%d" % [player_data.get("current_hp", 0), player_data.get("max_hp", 80)]
	if energy_lbl != null:
		energy_lbl.text = "%d/%d" % [player_data.get("energy", 0), player_data.get("max_energy", 3)]
	if hp_bar != null:
		hp_bar.max_value = player_data.get("max_hp", 80)
		hp_bar.value = player_data.get("current_hp", 0)

func _on_coop_turn_started(player_index: int, turn: int) -> void:
	print("[CombatHUD] Coop turn started: player=%d turn=%d" % [player_index, turn])
	set_turn_number(turn)
	if _coop_turn_indicator != null:
		_coop_turn_indicator.text = "玩家%d的回合" % (player_index + 1)
	if _combat_system != null and _combat_system.has_method("is_local_player_turn"):
		var is_my_turn: bool = _combat_system.call("is_local_player_turn")
		set_phase(is_my_turn)
		if _coop_turn_indicator != null:
			_coop_turn_indicator.modulate = Color(0.5, 0.9, 0.5) if is_my_turn else Color(0.9, 0.4, 0.3)
	_sync_coop_ui_from_system()

func _on_coop_card_played(player_index: int, card: Dictionary, target_index: int) -> void:
	print("[CombatHUD] Coop card played: player=%d card=%s target=%d" % [player_index, card.get("name", "?"), target_index])
	_sync_coop_ui_from_system()

func _on_coop_damage_dealt(target_index: int, target_name: String, amount: int, source_player: int) -> void:
	if target_index >= 0:
		show_enemy_hit_feedback(target_index)
		show_damage(self, amount, _enemies_ui[target_index].global_position if target_index < _enemies_ui.size() else Vector2(500, 200), false)
		update_enemy_health(target_index, _combat_system.call("get_enemies")[target_index].get("current_hp", 0), _combat_system.call("get_enemies")[target_index].get("max_hp", 50))
	elif source_player == -1:
		show_player_hit_feedback()
	_sync_coop_ui_from_system()

func _on_coop_block_gained(player_index: int, amount: int, total: int) -> void:
	if player_index == _local_player_index:
		update_block(total)
	_sync_coop_ui_from_system()

func _on_damage_dealt(target_index: int, target_name: String, amount: int) -> void:
	if target_index >= 0 and target_index < _enemies_ui.size():
		show_enemy_hit_feedback(target_index)
		show_damage(self, amount, _enemies_ui[target_index].global_position, false)
	_sync_single_ui_from_system()

func _on_block_gained(amount: int, total: int) -> void:
	update_block(total)

func _on_turn_started(turn: int) -> void:
	set_turn_number(turn)
	set_phase(true)
	_sync_single_ui_from_system()

func _on_turn_ended(turn: int) -> void:
	set_phase(false)

func _on_bridge_coop_card_played(player_index: int, card_data: String, target_index: int) -> void:
	if not _is_coop_mode or _combat_system == null:
		return
	print("[CombatHUD] Bridge: remote card played player=%d target=%d" % [player_index, target_index])
	var parsed = JSON.parse_string(card_data)
	var card_dict: Dictionary = {}
	if parsed != null and parsed is Dictionary:
		card_dict = parsed as Dictionary
	if _combat_system.has_method("apply_remote_card_play"):
		_combat_system.call("apply_remote_card_play", player_index, card_dict, target_index)

func _on_bridge_coop_turn_ended(player_index: int) -> void:
	if not _is_coop_mode or _combat_system == null:
		return
	print("[CombatHUD] Bridge: remote turn ended player=%d" % player_index)
	if _combat_system.has_method("apply_remote_turn_end"):
		_combat_system.call("apply_remote_turn_end", player_index)

func _on_draw_pile_clicked() -> void:
	print("[CombatHUD] 📥 抽牌堆点击")
	show_pile_view_requested.emit("抽牌堆")

func _on_discard_pile_clicked() -> void:
	print("[CombatHUD] 📤 弃牌堆点击")
	show_pile_view_requested.emit("弃牌堆")

func _on_end_turn_clicked() -> void:
	if _is_processing or not _is_player_turn:
		print("[CombatHUD] ⏳ 无法操作 - 正在处理中")
		return

	print("[CombatHUD] ⚔️ 结束回合按钮点击")
	set_phase(false)
	end_turn.emit()

func set_phase(is_player_turn: bool) -> void:
	_is_player_turn = is_player_turn
	_phase_label.text = "你的回合" if is_player_turn else "敌方回合"
	_phase_label.modulate = Color(0.5, 0.9, 0.5) if is_player_turn else Color(0.9, 0.4, 0.3)
	_end_turn_btn.disabled = not is_player_turn
	_end_turn_btn.modulate = Color.WHITE if is_player_turn else Color(0.5, 0.5, 0.5)

func set_processing(processing: bool) -> void:
	_is_processing = processing

func on_show() -> void:
	visible = true

func on_hide() -> void:
	visible = false

func update_hand(cards: Array) -> void:
	for card in _hand_cards:
		card.visible = false

	for i in range(mini(cards.size(), _hand_cards.size())):
		_hand_cards[i].set_card_data(cards[i])
		_hand_cards[i].visible = true

	_update_card_positions()
	_hand_count_label.text = "手牌: %d" % cards.size()

func update_energy(current: int, max_val: int) -> void:
	_current_energy = current
	_max_energy = max_val
	_energy_label.text = "%d/%d" % [current, max_val]

func update_health(current: int, max_val: int) -> void:
	_player_health_bar.max_value = max_val
	_player_health_bar.value = current
	_player_health_text.text = "%d/%d" % [current, max_val]

func update_block(block: int) -> void:
	_player_block_bar.value = block
	_player_block_bar.visible = block > 0
	_player_block_text.text = "%d" % block if block > 0 else ""

func update_draw_pile(count: int) -> void:
	_draw_pile_btn.text = "抽牌堆 (%d)" % count

func update_discard_pile(count: int) -> void:
	_discard_pile_btn.text = "弃牌堆 (%d)" % count

func set_turn_number(turn: int) -> void:
	_turn_label.text = "回合 %d" % turn

func set_floor_number(floor_num: int) -> void:
	_floor_label.text = "第 %d 层" % floor_num

func _on_card_pressed(card) -> void:
	if _is_processing or not _is_player_turn:
		print("[CombatHUD] ⏳ 无法出牌 - 不是你的回合")
		return

	var card_data = card.card_data if card.has_method("get") else null
	if card_data == null: return

	var cost: int = card_data.get("cost", 1)
	if _current_energy >= cost:
		var type: int = card_data.get("type", 0)
		var needs_target := type == 0
		if needs_target and _enemies_ui.size() > 1:
			var alive_count := 0
			for eui in _enemies_ui:
				if not eui.is_dead:
					alive_count += 1
			if alive_count <= 1:
				var target_idx := _find_first_alive_enemy()
				if target_idx >= 0:
					print("[CombatHUD] 🃏 出牌: %s (费用:%d) -> 敌人%d" % [card_data.get("name", ""), cost, target_idx])
					show_card_play_animation(card_data, target_idx)
					card_played_with_target.emit(card_data.get("id", ""), target_idx)
				else:
					print("[CombatHUD] ❌ 没有存活敌人")
				return
			_is_selecting_target = true
			_pending_card = card
			_phase_label.text = "选择目标"
			_phase_label.modulate = Color(1, 0.5, 0.2)
			_highlight_enemies(true)
			print("[CombatHUD] 🎯 Select target for: %s" % card_data.get("name", ""))
		else:
			var target_idx := _find_first_alive_enemy() if needs_target else -1
			if needs_target and target_idx < 0:
				print("[CombatHUD] ❌ 没有存活敌人")
				return
			print("[CombatHUD] 🃏 出牌: %s (费用:%d)" % [card_data.get("name", ""), cost])
			show_card_play_animation(card_data, target_idx)
			if target_idx >= 0:
				card_played_with_target.emit(card_data.get("id", ""), target_idx)
			else:
				card_played.emit(card_data.get("id", ""))
	else:
		print("[CombatHUD] ❌ 能量不足: %s 需要%d点, 当前%d点" % [card_data.get("name", ""), cost, _current_energy])

func _on_enemy_clicked(enemy_index: int) -> void:
	if _is_selecting_target and _pending_card != null:
		if enemy_index >= 0 and enemy_index < _enemies_ui.size() and _enemies_ui[enemy_index].is_dead:
			print("[CombatHUD] ❌ 不能选择已死亡的敌人")
			return
		var card = _pending_card
		_is_selecting_target = false
		_pending_card = null
		_highlight_enemies(false)
		_phase_label.text = "你的回合"
		_phase_label.modulate = Color(0.5, 0.9, 0.5)
		print("[CombatHUD] 🎯 Target selected: enemy %d" % enemy_index)
		var card_data = card.card_data if card.card_data else {}
		show_card_play_animation(card_data, enemy_index)
		card_played_with_target.emit(card_data.get("id", "") if card.card_data else "", enemy_index)

func _highlight_enemies(highlight: bool) -> void:
	for enemy in _enemies_ui:
		if highlight and enemy.is_dead:
			continue
		enemy.set_selectable(highlight)

func _find_first_alive_enemy() -> int:
	for i in range(_enemies_ui.size()):
		if not _enemies_ui[i].is_dead:
			return i
	return -1

func show_enemy_attack_feedback(enemy_index: int) -> void:
	if enemy_index >= 0 and enemy_index < _enemy_sprites.size():
		_enemy_sprites[enemy_index].play_attack_animation(Vector2(200, 300))
	var shake_tween := create_tween()
	shake_tween.tween_property(_root_container, "position:x", _root_container.position.x + 8, 0.05)
	shake_tween.tween_property(_root_container, "position:x", _root_container.position.x - 6, 0.05)
	shake_tween.tween_property(_root_container, "position:x", _root_container.position.x + 4, 0.05)
	shake_tween.tween_property(_root_container, "position:x", _root_container.position.x, 0.05)

func show_player_hit_feedback() -> void:
	if _player_sprite and _player_sprite.has_method("play_hit_animation"):
		_player_sprite.play_hit_animation()
	var hit_tween := create_tween()
	hit_tween.tween_property(_player_status_area, "modulate", Color(1, 0.3, 0.3), 0.1)
	hit_tween.tween_property(_player_status_area, "modulate", Color.WHITE, 0.2).set_delay(0.1)

func show_enemy_hit_feedback(enemy_index: int) -> void:
	if enemy_index >= 0 and enemy_index < _enemy_sprites.size():
		_enemy_sprites[enemy_index].play_hit_animation()
	if enemy_index >= 0 and enemy_index < _enemies_ui.size():
		var enemy_ui = _enemies_ui[enemy_index]
		var hit_tween: Tween = enemy_ui.create_tween()
		hit_tween.tween_property(enemy_ui, "modulate", Color(1, 0.5, 0.5), 0.08)
		hit_tween.tween_property(enemy_ui, "modulate", Color.WHITE, 0.15).set_delay(0.08)

func show_card_play_animation(card_data: Dictionary, target_enemy_index: int) -> void:
	var icon_path: String = card_data.get("icon_path", "")
	if icon_path == "":
		var type_val: int = card_data.get("type", 0)
		match type_val:
			0: icon_path = "res://GameModes/base_game/Resources/Icons/Cards/strike.png"
			1: icon_path = "res://GameModes/base_game/Resources/Icons/Cards/defend.png"
			2: icon_path = "res://GameModes/base_game/Resources/Icons/Skills/fireball.png"
			_: icon_path = "res://GameModes/base_game/Resources/Icons/Cards/card_0.png"
	var icon_tex := load(icon_path) as Texture2D
	if icon_tex == null: return
	var card_bg := PanelContainer.new()
	card_bg.custom_minimum_size = Vector2(84, 114)
	card_bg.z_index = 99
	card_bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	card_bg.add_theme_stylebox_override("panel", UITheme.make_dark_panel_bg())
	var type_val: int = card_data.get("type", 0)
	match type_val:
		0: card_bg.self_modulate = Color(1.0, 0.6, 0.6)
		1: card_bg.self_modulate = Color(0.6, 0.7, 1.0)
		2: card_bg.self_modulate = Color(1.0, 0.9, 0.6)
		_: card_bg.self_modulate = Color.WHITE
	var card_img := TextureRect.new()
	card_img.texture = icon_tex
	card_img.custom_minimum_size = Vector2(80, 110)
	card_img.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	card_img.z_index = 100
	card_img.mouse_filter = Control.MOUSE_FILTER_IGNORE
	card_bg.add_child(card_img)
	var start_pos := Vector2(540, 400)
	var end_pos: Vector2
	if target_enemy_index >= 0 and target_enemy_index < _enemy_sprites.size():
		end_pos = _enemy_sprites[target_enemy_index].global_position + Vector2(0, -30)
	else:
		end_pos = Vector2(540, 200) if type_val != 1 else Vector2(120, 300)
	card_bg.position = start_pos
	add_child(card_bg)
	var tween := create_tween()
	tween.set_parallel(true)
	tween.tween_property(card_bg, "position", end_pos, 0.4).set_ease(Tween.EASE_IN).set_trans(Tween.TRANS_BACK)
	tween.tween_property(card_bg, "scale", Vector2(1.2, 1.2), 0.2)
	tween.tween_property(card_bg, "modulate", Color(2, 2, 2, 1), 0.2)
	tween.chain()
	tween.tween_property(card_bg, "modulate", Color(1, 1, 1, 0), 0.3).set_delay(0.3)
	tween.tween_property(card_bg, "scale", Vector2(0.5, 0.5), 0.3).set_delay(0.3)
	tween.tween_callback(card_bg.queue_free).set_delay(0.7)


class BattleCharacterSprite:
	extends Control

	var _character_id: String = ""
	var _is_player: bool = false
	var _sprite_display: TextureRect
	var _sprite_frame: PanelContainer
	var _name_overlay: Label

	func _init(character_id: String = "", is_player: bool = false) -> void:
		_character_id = character_id.to_lower().replace("_pack", "").replace("_", " ")
		_is_player = is_player

	func _ready() -> void:
		mouse_filter = Control.MOUSE_FILTER_IGNORE

		_sprite_frame = PanelContainer.new()
		_sprite_frame.custom_minimum_size = Vector2(130, 160) if _is_player else Vector2(150, 190)
		_sprite_frame.mouse_filter = Control.MOUSE_FILTER_IGNORE

		_sprite_frame.add_theme_stylebox_override("panel", UITheme.make_dark_panel_bg())
		_sprite_frame.self_modulate = Color(0.6, 0.75, 1.0) if _is_player else Color(1.0, 0.55, 0.55)
		add_child(_sprite_frame)

		var vbox := VBoxContainer.new()
		vbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
		vbox.set_anchors_preset(Control.PRESET_FULL_RECT)
		vbox.add_theme_constant_override("separation", 3)
		_sprite_frame.add_child(vbox)

		_name_overlay = Label.new()
		_name_overlay.text = "铁甲战士" if _is_player else _character_id
		_name_overlay.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		_name_overlay.modulate = Color(0.6, 0.8, 1) if _is_player else Color(1, 0.5, 0.4)
		_name_overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_name_overlay.add_theme_font_size_override("font_size", 12)
		vbox.add_child(_name_overlay)

		_sprite_display = TextureRect.new()
		_sprite_display.custom_minimum_size = Vector2(0, 110) if _is_player else Vector2(0, 130)
		_sprite_display.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		_sprite_display.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_sprite_display.size_flags_vertical = Control.SIZE_EXPAND_FILL

		var sprite_path := _get_character_sprite_path()
		var texture := load(sprite_path) as Texture2D
		if texture != null:
			_sprite_display.texture = texture
		else:
			_sprite_display.texture = _generate_placeholder_sprite()
		vbox.add_child(_sprite_display)

	func _get_character_sprite_path() -> String:
		if _is_player:
			return "res://GameModes/base_game/Resources/Icons/Items/iron_sword.png"
		match _character_id.to_lower():
			"cultist", "jaw worm", "jawworm":
				return "res://GameModes/base_game/Resources/Icons/Enemies/jawworm.png"
			"lagavulin":
				return "res://GameModes/base_game/Resources/Icons/Enemies/lagavulin.png"
			"theguardian", "the guardian", "guardian":
				return "res://GameModes/base_game/Resources/Icons/Enemies/theguardian.png"
			_:
				return "res://GameModes/base_game/Resources/Icons/Enemies/cultist.png"

	func _generate_placeholder_sprite() -> ImageTexture:
		var img := Image.create_empty(128, 128, false, Image.FORMAT_RGBA8)
		img.fill(Color(0.2, 0.3, 0.5, 1) if _is_player else Color(0.5, 0.2, 0.2, 1))
		return ImageTexture.create_from_image(img)

	func play_attack_animation(target_pos: Vector2) -> void:
		var orig_pos := position
		var direction := (target_pos - position).normalized()
		var tween := create_tween()
		tween.tween_property(self, "position", orig_pos + direction * 50, 0.15).set_ease(Tween.EASE_OUT)
		tween.parallel().tween_property(self, "scale", Vector2(1.15, 1.15), 0.1)
		tween.tween_property(self, "position", orig_pos, 0.15).set_ease(Tween.EASE_IN)
		tween.parallel().tween_property(self, "scale", Vector2.ONE, 0.15)

	func play_hit_animation() -> void:
		var orig_pos := position
		var tween := create_tween()
		tween.tween_property(_sprite_frame, "modulate", Color(2, 0.5, 0.5, 1), 0.06)
		tween.parallel().tween_property(self, "position:x", orig_pos.x - 8, 0.04)
		tween.tween_property(self, "position:x", orig_pos.x + 6, 0.04)
		tween.tween_property(self, "position:x", orig_pos.x - 4, 0.04)
		tween.tween_property(self, "position", orig_pos, 0.06)
		tween.parallel().tween_property(_sprite_frame, "modulate", Color.WHITE, 0.15)

	func play_death_animation() -> void:
		var tween := create_tween()
		tween.tween_property(self, "modulate:a", 0.0, 0.5).set_ease(Tween.EASE_IN)
		tween.tween_property(self, "scale", Vector2(0.8, 0.8), 0.5).set_ease(Tween.EASE_IN)


class EnemyUnitUI:
	extends Control

	signal enemy_clicked(index: int)

	var _name: String = ""
	var _max_hp: int = 0
	var _current_hp: int = 0
	var _enemy_index: int = -1
	var _body: PanelContainer
	var _health_bar: ProgressBar
	var _health_text: Label
	var _intent_label: Label
	var _is_selectable: bool = false

	var is_dead: bool:
		get: return _current_hp <= 0

	func _init(p_name: String = "", p_max_hp: int = 0) -> void:
		_name = p_name.replace("_pack", "").replace("_", " ")
		_max_hp = p_max_hp
		_current_hp = p_max_hp
		custom_minimum_size = Vector2(135, 60)

	func set_enemy_index(index: int) -> void:
		_enemy_index = index

	func set_selectable(selectable: bool) -> void:
		_is_selectable = selectable
		mouse_filter = Control.MOUSE_FILTER_STOP if selectable else Control.MOUSE_FILTER_IGNORE
		if _body != null:
			_body.self_modulate = Color(1.0, 0.85, 0.3) if selectable else Color(1.0, 0.45, 0.45)

	func _ready() -> void:
		mouse_filter = Control.MOUSE_FILTER_IGNORE

		_body = PanelContainer.new()
		_body.custom_minimum_size = Vector2(130, 55)
		_body.mouse_filter = Control.MOUSE_FILTER_IGNORE

		_body.add_theme_stylebox_override("panel", UITheme.make_dark_panel_bg())
		_body.self_modulate = Color(1.0, 0.45, 0.45)
		add_child(_body)

		var vbox := VBoxContainer.new()
		vbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
		vbox.set_anchors_preset(Control.PRESET_FULL_RECT)
		vbox.add_theme_constant_override("separation", 1)
		_body.add_child(vbox)

		var top_row := HBoxContainer.new()
		top_row.mouse_filter = Control.MOUSE_FILTER_IGNORE
		vbox.add_child(top_row)

		_health_text = Label.new()
		_health_text.text = "%d/%d" % [_current_hp, _max_hp]
		_health_text.modulate = Color.LIGHT_GRAY
		_health_text.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_health_text.add_theme_font_size_override("font_size", 10)
		top_row.add_child(_health_text)

		_intent_label = Label.new()
		_intent_label.text = ""
		_intent_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
		_intent_label.modulate = Color(1, 0.78, 0.18)
		_intent_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_intent_label.add_theme_font_size_override("font_size", 10)
		top_row.add_child(_intent_label)

		_health_bar = ProgressBar.new()
		_health_bar.max_value = _max_hp
		_health_bar.value = _current_hp
		_health_bar.custom_minimum_size = Vector2(120, 14)
		_health_bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_health_bar.add_theme_stylebox_override("background", UITheme.make_bar_bg_style())
		var health_style := StyleBoxFlat.new()
		health_style.bg_color = Color(0.8, 0.2, 0.2)
		health_style.set_corner_radius_all(3)
		_health_bar.add_theme_stylebox_override("fill", health_style)
		vbox.add_child(_health_bar)

		gui_input.connect(_on_enemy_gui_input)

	func _on_enemy_gui_input(event: InputEvent) -> void:
		if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT and _is_selectable:
			enemy_clicked.emit(_enemy_index)

	func update_health(current: int, max_val: int) -> void:
		_current_hp = current
		_max_hp = max_val
		_health_bar.max_value = max_val
		_health_bar.value = current
		_health_text.text = "%d/%d" % [current, max_val]
		if current <= 0:
			set_dead()
			return
		if float(current) / max_val <= 0.3:
			var low_health_style := StyleBoxFlat.new()
			low_health_style.bg_color = Color(0.55, 0.12, 0.12)
			low_health_style.set_corner_radius_all(3)
			_health_bar.add_theme_stylebox_override("fill", low_health_style)

	func set_dead() -> void:
		_is_selectable = false
		mouse_filter = Control.MOUSE_FILTER_IGNORE
		_intent_label.text = ""
		_body.add_theme_stylebox_override("panel", UITheme.make_dark_panel_bg())
		_body.self_modulate = Color(0.4, 0.4, 0.4, 0.6)
		var tween := create_tween()
		tween.tween_property(self, "modulate", Color(0.4, 0.4, 0.4, 0.3), 0.6).set_ease(Tween.EASE_IN)
		tween.parallel().tween_property(self, "scale", Vector2(0.85, 0.85), 0.6).set_ease(Tween.EASE_IN)
		tween.tween_callback(func(): _health_text.text = "X")

	func update_intent(text: String, icon: String = "") -> void:
		_intent_label.text = ("%s %s" % [icon, text]) if icon != "" else text


class CardUI:
	extends Control

	signal card_pressed(card_ui)

	var hand_index: int
	var card_data: Dictionary = {}
	var _card_body: PanelContainer
	var _name_label: Label
	var _cost_label: Label
	var _desc_label: Label
	var _icon_rect: TextureRect
	var _base_position: Vector2
	var _is_hovered: bool = false

	func _init(p_hand_index: int = 0) -> void:
		hand_index = p_hand_index

	func _ready() -> void:
		mouse_filter = Control.MOUSE_FILTER_STOP
		custom_minimum_size = Vector2(105, 130)
		_base_position = position

		_card_body = PanelContainer.new()
		_card_body.custom_minimum_size = Vector2(100, 125)
		_card_body.mouse_filter = Control.MOUSE_FILTER_IGNORE
		var style := _create_card_style()
		_card_body.add_theme_stylebox_override("panel", style)
		add_child(_card_body)

		var main_vbox := VBoxContainer.new()
		main_vbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_card_body.add_child(main_vbox)

		var header_hbox := HBoxContainer.new()
		header_hbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
		main_vbox.add_child(header_hbox)

		_name_label = Label.new()
		_name_label.text = "卡牌"
		_name_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		_name_label.modulate = Color.WHITE
		_name_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_name_label.add_theme_font_size_override("font_size", 11)
		header_hbox.add_child(_name_label)

		_cost_label = Label.new()
		_cost_label.text = "0"
		_cost_label.modulate = Color(1, 0.85, 0.2)
		_cost_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_cost_label.add_theme_font_size_override("font_size", 12)
		header_hbox.add_child(_cost_label)

		_icon_rect = TextureRect.new()
		_icon_rect.custom_minimum_size = Vector2(0, 40)
		_icon_rect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		_icon_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
		main_vbox.add_child(_icon_rect)

		_desc_label = Label.new()
		_desc_label.text = ""
		_desc_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		_desc_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		_desc_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_desc_label.custom_minimum_size = Vector2(90, 32)
		_desc_label.add_theme_font_size_override("font_size", 8)
		main_vbox.add_child(_desc_label)

		mouse_entered.connect(_on_mouse_enter)
		mouse_exited.connect(_on_mouse_exit)
		gui_input.connect(_on_gui_input)

	func _create_card_style() -> StyleBoxTexture:
		return UITheme.make_card_panel_style()

	func set_card_data(data: Dictionary) -> void:
		card_data = data
		_name_label.text = data.get("name", "")
		_cost_label.text = str(data.get("cost", 0))
		_desc_label.text = data.get("description", "")

		var type_val: int = data.get("type", 0)
		match type_val:
			0:
				_cost_label.modulate = Color(1, 0.35, 0.35)
			1:
				_cost_label.modulate = Color(0.35, 0.6, 1)
			2:
				_cost_label.modulate = Color(0.9, 0.75, 0.3)
			_:
				_cost_label.modulate = Color(1, 0.85, 0.2)

		var icon_path: String = data.get("icon_path", "")
		if icon_path == "":
			icon_path = _get_card_icon(type_val)
		var icon_tex := load(icon_path) as Texture2D
		if icon_tex != null:
			_icon_rect.texture = icon_tex

		_cost_label.modulate = Color(0.4, 0.85, 0.4) if data.get("cost", 1) == 0 else Color(1, 0.85, 0.2)
		match type_val:
			0:
				_cost_label.modulate = Color(1, 0.35, 0.35)
				_card_body.self_modulate = Color(1.0, 0.7, 0.7)
			1:
				_cost_label.modulate = Color(0.35, 0.6, 1)
				_card_body.self_modulate = Color(0.7, 0.8, 1.0)
			2:
				_cost_label.modulate = Color(0.9, 0.75, 0.3)
				_card_body.self_modulate = Color(1.0, 0.95, 0.7)
			_:
				_cost_label.modulate = Color(1, 0.85, 0.2)
				_card_body.self_modulate = Color.WHITE

	func _get_card_icon(type_val: int) -> String:
		match type_val:
			0: return "res://GameModes/base_game/Resources/Icons/Cards/strike.png"
			1: return "res://GameModes/base_game/Resources/Icons/Cards/defend.png"
			2: return "res://GameModes/base_game/Resources/Icons/Skills/fireball.png"
			_: return "res://GameModes/base_game/Resources/Icons/Cards/card_0.png"

	func _on_mouse_enter() -> void:
		_is_hovered = true
		create_tween().tween_property(self, "position:y", _base_position.y - 20, 0.1).set_ease(Tween.EASE_OUT)

	func _on_mouse_exit() -> void:
		_is_hovered = false
		create_tween().tween_property(self, "position:y", _base_position.y, 0.1).set_ease(Tween.EASE_OUT)

	func _on_gui_input(event: InputEvent) -> void:
		if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
			card_pressed.emit(self)


static func show_damage(parent: Control, amount: int, pos: Vector2, is_player: bool) -> void:
	var color := Color(1, 0.3, 0.3) if is_player else Color(1, 0.85, 0.2)
	parent.add_child(FloatingTextLabel.new("-%d" % amount, pos, color))

static func show_block(parent: Control, amount: int, pos: Vector2) -> void:
	parent.add_child(FloatingTextLabel.new("+%d 🛡️" % amount, pos, Color(0.35, 0.6, 1)))

static func show_heal(parent: Control, amount: int, pos: Vector2) -> void:
	parent.add_child(FloatingTextLabel.new("+%d ❤️" % amount, pos, Color(0.35, 1, 0.4)))

static func show_status(parent: Control, text: String, pos: Vector2) -> void:
	parent.add_child(FloatingTextLabel.new(text, pos, Color(0.9, 0.7, 0.3)))


class FloatingTextLabel:
	extends Control

	var _label: Label
	var _start_pos: Vector2

	func _init(text: String, position: Vector2, color: Color) -> void:
		_start_pos = position
		mouse_filter = Control.MOUSE_FILTER_IGNORE

		_label = Label.new()
		_label.text = text
		_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		_label.modulate = color
		_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_label.add_theme_font_size_override("font_size", 22)

	func _ready() -> void:
		position = _start_pos
		add_child(_label)

		var tween := create_tween()
		tween.set_parallel(true)
		tween.tween_property(self, "position:y", _start_pos.y - 70, 0.9).set_ease(Tween.EASE_OUT)
		tween.tween_property(self, "modulate:a", 0.0, 1.3).set_delay(0.4)

		get_tree().create_timer(1.8).timeout.connect(queue_free)
