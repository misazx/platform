class_name EncounterGenerator extends Node

signal encounter_generated(encounter: Dictionary)

@export var base_enemy_count: int = 1
@export var max_elite_chance: float = 0.08
@export var elite_chance_increment: float = 0.02

var _rng: RandomNumberGenerator

func _ready() -> void:
	_rng = RandomNumberGenerator.new()

func generate_encounter(floor_num: int, room_type: String, seed_val: uint) -> Dictionary:
	_rng.seed = seed_val + floor_num
	var encounter := {
		"floor": floor_num,
		"type": room_type,
		"enemies": [],
		"is_elite": false,
		"is_boss": room_type == "boss",
		"difficulty_rating": calculate_difficulty(floor_num),
		"seed": seed_val
	}

	if room_type == "boss":
		encounter["enemies"] = generate_boss_encounter(floor_num)
		encounter["is_boss"] = true
	elif _rng.randf() < get_elite_chance(floor_num):
		encounter["enemies"] = generate_elite_encounter(floor_num)
		encounter["is_elite"] = true
	else:
		encounter["enemies"] = generate_normal_encounter(floor_num)

	encounter_generated.emit(encounter)
	GD.print("[EncounterGenerator] Generated encounter on floor %d (%s): %d enemies" % [floor_num, room_type, encounter["enemies"].size()])
	return encounter

func generate_normal_encounter(floor_num: int) -> Array:
	var normal_enemies := EnemyDatabase.get_normal_enemies()
	if normal_enemies.is_empty():
		return []
	var count := base_enemy_count + floori(floor_num / 5.0)
	count = mini(count, 4)
	var result := []
	for i in range(count):
		result.append(normal_enemies[_rng.randi() % normal_enemies.size()])
	return result

func generate_elite_encounter(floor_num: int) -> Array:
	var elite_enemies := EnemyDatabase.get_elite_enemies()
	if elite_enemies.is_empty():
		return generate_normal_encounter(floor_num)
	var result := [elite_enemies[_rng.randi() % elite_enemies.size()]]
	var normal_enemies := EnemyDatabase.get_normal_enemies()
	if not normal_enemies.is_empty() and _rng.randf() < 0.6:
		result.append(normal_enemies[_rng.randi() % normal_enemies.size()])
	return result

func generate_boss_encounter(floor_num: int) -> Array:
	var boss_enemies := EnemyDatabase.get_boss_enemies()
	if boss_enemies.is_empty():
		return generate_elite_encounter(floor_num)
	return [boss_enemies[floor_num % boss_enemies.size()]]

func get_elite_chance(floor_num: int) -> float:
	return min(max_elite_chance + elite_chance_increment * float(floor_num), 0.3)

func calculate_difficulty(floor_num: int) -> float:
	return 1.0 + float(floor_num) * 0.15
