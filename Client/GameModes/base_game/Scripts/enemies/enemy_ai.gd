class_name EnemyAI extends Node

signal action_decided(action_type: String, target_index: int)

var _enemy_data: Dictionary = {}
var _behavior := "aggressive"
var _action_queue: Array = []

func _ready() -> void:
	pass

func initialize(enemy_id: String) -> void:
	_enemy_data = EnemyDatabase.get_enemy(enemy_id)
	if not _enemy_data.is_empty():
		_behavior = "aggressive"
		if _enemy_data.has("behavior"):
			match _enemy_data["behavior"]:
				1: _behavior = "defensive"
				2: _behavior = "support"
				3: _behavior = "summoner"
		GD.Print("[EnemyAI] Initialized for %s (behavior: %s)" % [_enemy_data.get("name", ""), _behavior])

func decide_action(player_hp: int, player_block: int, turn_number: int) -> Dictionary:
	var action := {"type": "attack", "target": 0, "value": _enemy_data.get("attack_damage", 10)}

	match _behavior:
		"aggressive":
			action["type"] = "attack"
			action["value"] = _enemy_data.get("attack_damage", 10)
		"defensive":
			if player_block < 5 or randf() < 0.4:
				action["type"] = "attack"
				action["value"] = _enemy_data.get("attack_damage", 8)
			else:
				action["type"] = "block"
				action["value"] = _enemy_data.get("block_amount", 6)
		"support":
			if randf() < 0.3:
				action["type"] = "buff"
				action["value"] = 5
			else:
				action["type"] = "attack"
				action["value"] = _enemy_data.get("attack_damage", 8)
		"summoner":
			if turn_number <= 1 and randf() < 0.5:
				action["type"] = "summon"
				action["value"] = 1
			else:
				action["type"] = "attack"
				action["value"] = _enemy_data.get("attack_damage", 7)

	action_decided.emit(action["type"], action["target"])
	return action

func get_behavior() -> String:
	return _behavior

func set_behavior(behav: String) -> void:
	_behavior = behav
