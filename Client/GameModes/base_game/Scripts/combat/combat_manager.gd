extends Node
enum CombatPhase { PLAYER_TURN, ENEMY_TURN, CARD_SELECTION, TARGET_SELECTION, GAME_OVER, VICTORY }
enum PlayerAction { PLAY_CARD, END_TURN, USE_POTION, CHECK_DECK, CHECK_DISCARD_PILE, NONE }


signal turn_started(turn: int)
signal turn_ended(turn: int)
signal card_played(card_id: String, target_name: String)
signal energy_changed(current: int, max_val: int)
signal block_changed(block: int)
signal phase_changed(new_phase: int)
signal combat_victory()
signal combat_defeat()

var _state: Dictionary = {}
var _current_phase := CombatPhase.PLAYER_TURN
var _player: Node = null
var _enemies: Array = []
var _rng: RandomNumberGenerator
var _player_deck: Array = []

func _ready() -> void:
	pass

func initialize_combat(player: Node, enemies: Array, seed_val: int) -> void:
	_player = player
	_enemies = enemies if enemies else []
	_rng = RandomNumberGenerator.new()
	var effective_seed: int = seed_val
	var bridge = get_node_or_null("/root/MultiplayerSeedBridge")
	if bridge != null and bridge.is_multiplayer_game() and seed_val == 0:
		effective_seed = bridge.get_effective_seed(0)
	_rng.seed = effective_seed

	_state = {
		"current_energy": 3,
		"max_energy": 3,
		"current_block": 0,
		"turn_number": 1,
		"hand": [],
		"draw_pile": [],
		"discard_pile": [],
		"exhaust_pile": [],
		"buffs": {},
		"debuffs": {},
		"has_acted": false
	}

	var deck_cards := CardDatabase.get_character_cards("ironclad")
	if deck_cards.is_empty():
		deck_cards = CardDatabase.get_all_cards()
	for card in deck_cards:
		for i in range(3):
			_player_deck.append(card)

	_state["draw_pile"] = _player_deck.duplicate()
	_state["draw_pile"].shuffle()
	_current_phase = CombatPhase.PLAYER_TURN
	start_player_turn()

	print("[CombatManager] Combat initialized with %d enemies, %d cards in deck" % [_enemies.size(), _player_deck.size()])
	phase_changed.emit(_current_phase)

func start_player_turn() -> void:
	_state["current_energy"] = _state["max_energy"]
	_state["current_block"] = 0
	_state["has_acted"] = false
	draw_cards(5)
	turn_started.emit(_state["turn_number"])
	print("[CombatManager] Turn %d started (energy: %d)" % [_state["turn_number"], _state["current_energy"]])

func end_turn() -> void:
	if _current_phase != CombatPhase.PLAYER_TURN:
		return
	discard_hand()
	_current_phase = CombatPhase.ENEMY_TURN
	turn_ended.emit(_state["turn_number"])
	call_deferred("_process_enemy_turn")

func _process_enemy_turn() -> void:
	await get_tree().create_timer(1.0).timeout
	_state["turn_number"] += 1
	_current_phase = CombatPhase.PLAYER_TURN
	start_player_turn()

func play_card(card_id: String, target_index: int = -1) -> bool:
	if _state["current_energy"] <= 0 or _state["hand"].is_empty():
		return false

	var card_data := find_card_in_hand(card_id)
	if card_data.is_empty():
		return false

	var cost: int = card_data["cost"]
	if cost > _state["current_energy"]:
		return false

	_state["current_energy"] -= cost
	_state["hand"].erase(card_data)
	_state["discard_pile"].append(card_data)

	var target_name := ""
	if target_index >= 0 and target_index < _enemies.size():
		target_name = str(_enemies[target_index])
	elif not _enemies.is_empty():
		target_name = str(_enemies[0])

	card_played.emit(card_id, target_name)
	energy_changed.emit(_state["current_energy"], _state["max_energy"])

	if cost == 0 and card_data.get("type", 0) == 2:
		_state["exhaust_pile"].append(card_data)

	_state["has_acted"] = true
	print("[CombatManager] Played card: %s (cost: %d)" % [card_id, cost])
	return true

func find_card_in_hand(card_id: String) -> Dictionary:
	for card in _state["hand"]:
		if card["id"] == card_id:
			return card
	return {}

func draw_cards(count: int) -> void:
	for i in range(count):
		if _state["draw_pile"].is_empty():
			if not _state["discard_pile"].is_empty():
				_state["draw_pile"].append_array(_state["discard_pile"])
				_state["discard_pile"].clear()
				_state["draw_pile"].shuffle()
			else:
				break
		if not _state["draw_pile"].is_empty():
			var card: Dictionary = _state["draw_pile"].pop_back()
			_state["hand"].append(card)

func discard_hand() -> void:
	while not _state["hand"].is_empty():
		var card: Dictionary = _state["hand"].pop_back()
		_state["discard_pile"].append(card)

func add_block(amount: int) -> void:
	_state["current_block"] += amount
	block_changed.emit(_state["current_block"])

func take_damage(amount: int) -> void:
	var blocked := mini(amount, _state["current_block"])
	_state["current_block"] -= blocked
	var damage_taken := amount - blocked
	if damage_taken > 0:
		block_changed.emit(_state["current_block"])
	block_changed.emit(_state["current_block"])

func get_state() -> Dictionary:
	return _state.duplicate(true)

func can_play_cards() -> bool:
	return _state["current_energy"] > 0 and not _state["hand"].is_empty()

func end_combat(victory: bool) -> void:
	if victory:
		_current_phase = CombatPhase.VICTORY
		combat_victory.emit()
	else:
		_current_phase = CombatPhase.GAME_OVER
		combat_defeat.emit()
	print("[CombatManager] Combat ended: %s" % ("Victory" if victory else "Defeat"))
