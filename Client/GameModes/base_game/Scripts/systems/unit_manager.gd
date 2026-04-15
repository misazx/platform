extends Node

signal unit_spawned(unit_id: String, position: Vector2)
signal unit_destroyed(unit_id: String)
signal unit_damaged(unit_id: String, amount: int)

var _units: Dictionary = {}
var _unit_counter: int = 0

func _ready() -> void:
	pass

func spawn_unit(unit_type: String, position: Vector2, team: int = 0) -> Dictionary:
	_unit_counter += 1
	var unit_id: String = "%s_%d" % [unit_type, _unit_counter]
	var unit_data: Dictionary = {
		"id": unit_id,
		"type": unit_type,
		"position": position,
		"team": team,
		"health": _get_default_health(unit_type),
		"max_health": _get_default_health(unit_type),
		"is_alive": true,
		"damage_dealt": 0
	}
	_units[unit_id] = unit_data
	unit_spawned.emit(unit_id, position)
	print("[UnitManager] Spawned unit: %s at %s" % [unit_id, position])
	return unit_data

func damage_unit(unit_id: String, amount: int) -> bool:
	if not _units.has(unit_id):
		return false
	var unit: Dictionary = _units[unit_id]
	unit["health"] = unit["health"] - amount
	if unit["health"] <= 0:
		unit["health"] = 0
		unit["is_alive"] = false
		unit_destroyed.emit(unit_id)
		print("[UnitManager] Unit destroyed: %s" % unit_id)
	else:
		unit_damaged.emit(unit_id, amount)
	return true

func get_unit(unit_id: String) -> Dictionary:
	if _units.has(unit_id):
		return _units[unit_id]
	return {}

func get_all_units() -> Dictionary:
	return _units.duplicate()

func get_units_by_team(team: int) -> Array:
	var result: Array = []
	for unit: Dictionary in _units.values():
		if unit["team"] == team and unit["is_alive"]:
			result.append(unit)
	return result

func get_alive_count(team: int = -1) -> int:
	var count: int = 0
	for unit: Dictionary in _units.values():
		if unit["is_alive"]:
			if team < 0 or unit["team"] == team:
				count += 1
	return count

func clear_dead_units() -> void:
	var to_remove: Array = []
	for unit_id: String in _units:
		if not _units[unit_id]["is_alive"]:
			to_remove.append(unit_id)
	for unit_id: String in to_remove:
		_units.erase(unit_id)

func clear_all_units() -> void:
	_units.clear()

func _get_default_health(unit_type: String) -> int:
	match unit_type:
		"player": return 80
		"enemy_normal": return 50
		"enemy_elite": return 100
		"enemy_boss": return 300
		_: return 30
