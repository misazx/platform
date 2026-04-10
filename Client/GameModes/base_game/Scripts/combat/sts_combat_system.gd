class_name StsCombatSystem extends Node

signal combat_initialized()
signal turn_started(player_energy: int)
signal card_effect_executed(effect_type: String, value: int)
signal enemy_action_completed(enemy_name: String)
signal combat_ended(result: String)

var _combat_active := false
var _player_energy := 3
var _max_energy := 3
var _player_block := 0
var _turn_count := 0
var _enemies: Array = []

func _ready() -> void:
	pass

func start_combat(enemies: Array, seed_val: uint) -> void:
	_enemies = enemies
	_combat_active = true
	_player_energy = _max_energy
	_player_block = 0
	_turn_count = 1
	combat_initialized.emit()
	GD.Print("[StsCombat] Combat started with %d enemies" % enemies.size())
	turn_started.emit(_player_energy)

func execute_card_effect(card_id: String, targets: Array) -> Dictionary:
	var card := CardDatabase.get_card(card_id)
	if card.is_empty():
		return {"success": false}

	var effect_type := "attack"
	var value := card.get("damage", 0)
	if value <= 0:
		value = card.get("block", 0)
		effect_type = "block"

	match card.get("type", 0):
		0:
			effect_type = "attack"
			value = card.get("damage", 0)
		1:
			effect_type = "block"
			value = card.get("block", 0)
		2:
			effect_type = "power"
			value = card.get("magic_number", 0)

	_player_energy -= card.get("cost", 1)
	if effect_type == "block":
		_player_block += value

	card_effect_executed.emit(effect_type, value)
	return {"success": true, "effect_type": effect_type, "value": value}

func process_enemy_turns() -> void:
	for enemy in _enemies:
		await get_tree().create_timer(0.5).timeout
		enemy_action_completed.emit(enemy.get("name", "Enemy"))
	_turn_count += 1

func end_combat(result: String) -> void:
	_combat_active = false
	combat_ended.emit(result)
	GD.print("[StsCombat] Combat ended: %s" % result)

func is_combat_active() -> bool:
	return _combat_active

func get_player_energy() -> int:
	return _player_energy

func get_player_block() -> int:
	return _player_block

func get_enemies() -> Array:
	return _enemies
