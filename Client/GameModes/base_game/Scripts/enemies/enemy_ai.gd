class_name EnemyAI extends Node

signal action_decided(action_type: String, target_index: int)

enum BehaviorType { AGGRESSIVE, DEFENSIVE, SUPPORT, SUMMONER, BOSS }

var _enemy_data: Dictionary = {}
var _behavior: int = BehaviorType.AGGRESSIVE
var _action_queue: Array = []
var _turn_count: int = 0
var _hp_threshold_low: float = 0.3
var _hp_threshold_critical: float = 0.15

func _ready() -> void:
	pass

func initialize(enemy_id: String) -> void:
	_enemy_data = EnemyDatabase.get_enemy(enemy_id) if EnemyDatabase != null else {}
	if not _enemy_data.is_empty():
		_behavior = _enemy_data.get("behavior", BehaviorType.AGGRESSIVE) as int
		if _enemy_data.has("behavior_type"):
			match _enemy_data["behavior_type"]:
				"defensive": _behavior = BehaviorType.DEFENSIVE
				"support": _behavior = BehaviorType.SUPPORT
				"summoner": _behavior = BehaviorType.SUMMONER
				"boss": _behavior = BehaviorType.BOSS
				_: _behavior = BehaviorType.AGGRESSIVE
	print("[EnemyAI] Initialized for %s (behavior: %d)" % [_enemy_data.get("name", enemy_id), _behavior])

func decide_action(player_hp: int, player_block: int, turn_number: int) -> Dictionary:
	_turn_count = turn_number
	var base_damage: int = _enemy_data.get("attack_damage", 10)
	var base_block: int = _enemy_data.get("block_amount", 6)
	var enemy_hp: int = _enemy_data.get("current_hp", 50)
	var enemy_max_hp: int = _enemy_data.get("max_hp", 50)
	var hp_ratio: float = float(enemy_hp) / float(maxi(enemy_max_hp, 1))
	var action := {"type": "attack", "target": 0, "value": base_damage}

	match _behavior:
		BehaviorType.AGGRESSIVE:
			action = _decide_aggressive(base_damage, player_hp, player_block, hp_ratio, turn_number)
		BehaviorType.DEFENSIVE:
			action = _decide_defensive(base_damage, base_block, player_hp, player_block, hp_ratio, turn_number)
		BehaviorType.SUPPORT:
			action = _decide_support(base_damage, base_block, hp_ratio, turn_number)
		BehaviorType.SUMMONER:
			action = _decide_summoner(base_damage, hp_ratio, turn_number)
		BehaviorType.BOSS:
			action = _decide_boss(base_damage, base_block, player_hp, hp_ratio, turn_number)

	action_decided.emit(action["type"], action.get("target", 0))
	return action

func _decide_aggressive(base_damage: int, player_hp: int, player_block: int, hp_ratio: float, turn: int) -> Dictionary:
	if hp_ratio < _hp_threshold_critical and randf() < 0.3:
		return {"type": "attack", "target": 0, "value": int(base_damage * 1.5)}
	if player_block == 0 and randf() < 0.7:
		return {"type": "attack", "target": 0, "value": base_damage}
	if turn % 3 == 0 and randf() < 0.4:
		return {"type": "buff", "target": -1, "value": 2}
	return {"type": "attack", "target": 0, "value": base_damage}

func _decide_defensive(base_damage: int, base_block: int, player_hp: int, player_block: int, hp_ratio: float, turn: int) -> Dictionary:
	if hp_ratio < _hp_threshold_low:
		if randf() < 0.6:
			return {"type": "block", "target": -1, "value": base_block + 3}
		else:
			return {"type": "attack", "target": 0, "value": base_damage}
	if turn % 2 == 0:
		return {"type": "block", "target": -1, "value": base_block}
	if player_block < 5:
		return {"type": "attack", "target": 0, "value": base_damage}
	if randf() < 0.5:
		return {"type": "block", "target": -1, "value": base_block}
	return {"type": "attack", "target": 0, "value": base_damage}

func _decide_support(base_damage: int, base_block: int, hp_ratio: float, turn: int) -> Dictionary:
	if turn % 3 == 0:
		return {"type": "buff", "target": -1, "value": 3}
	if hp_ratio < _hp_threshold_low and randf() < 0.5:
		return {"type": "block", "target": -1, "value": base_block}
	if randf() < 0.3:
		return {"type": "debuff", "target": 0, "value": 1}
	return {"type": "attack", "target": 0, "value": base_damage}

func _decide_summoner(base_damage: int, hp_ratio: float, turn: int) -> Dictionary:
	if turn <= 2 and randf() < 0.6:
		return {"type": "summon", "target": -1, "value": 1}
	if hp_ratio < _hp_threshold_low and randf() < 0.4:
		return {"type": "buff", "target": -1, "value": 2}
	return {"type": "attack", "target": 0, "value": base_damage}

func _decide_boss(base_damage: int, base_block: int, player_hp: int, hp_ratio: float, turn: int) -> Dictionary:
	var phase: int = 1 if hp_ratio > 0.5 else 2
	if phase == 1:
		match turn % 4:
			0: return {"type": "attack", "target": 0, "value": base_damage}
			1: return {"type": "block", "target": -1, "value": base_block + 5}
			2: return {"type": "buff", "target": -1, "value": 2}
			_: return {"type": "attack", "target": 0, "value": int(base_damage * 0.8)}
	else:
		match turn % 3:
			0: return {"type": "attack", "target": 0, "value": int(base_damage * 1.3)}
			1: return {"type": "debuff", "target": 0, "value": 2}
			_: return {"type": "attack", "target": 0, "value": base_damage}

func get_behavior() -> int:
	return _behavior

func set_behavior(behav: int) -> void:
	_behavior = behav

func get_turn_count() -> int:
	return _turn_count
