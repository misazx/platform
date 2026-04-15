extends Node

signal wave_started(wave_number: int)
signal wave_completed(wave_number: int)
signal all_waves_completed()
signal enemy_spawned(enemy_data: Dictionary)

@export var base_enemy_count: int = 3
@export var enemy_count_per_wave: float = 1.5
@export var max_waves: int = 10

var _current_wave: int = 0
var _enemies_remaining: int = 0
var _is_active: bool = false
var _rng: RandomNumberGenerator

func _ready() -> void:
	_rng = RandomNumberGenerator.new()

func start_waves(seed: int) -> void:
	_rng.seed = seed
	_current_wave = 0
	_is_active = true
	print("[WaveManager] Starting waves (max: %d)" % max_waves)
	start_next_wave()

func start_next_wave() -> void:
	if _current_wave >= max_waves:
		_is_active = false
		all_waves_completed.emit()
		print("[WaveManager] All waves completed!")
		return

	_current_wave += 1
	var enemy_count: int = int(base_enemy_count + (_current_wave - 1) * enemy_count_per_wave)
	_enemies_remaining = enemy_count

	wave_started.emit(_current_wave)
	print("[WaveManager] Wave %d started (%d enemies)" % [_current_wave, enemy_count])

	for i in range(enemy_count):
		spawn_wave_enemy()

func spawn_wave_enemy() -> void:
	var enemy_types: PackedStringArray = ["enemy_normal", "enemy_normal", "enemy_elite"]
	var type_idx: int = 0
	if _current_wave > 3:
		type_idx = mini(2, _rng.randi() % (_current_wave / 3 + 1))
	var enemy_type: String = enemy_types[type_idx]

	var all_enemies: Array = []
	if EnemyDatabase != null and EnemyDatabase.has_method("get_all_enemies"):
		all_enemies = EnemyDatabase.get_all_enemies()
	var health_data: Dictionary = {}
	if all_enemies.size() > 0:
		health_data = all_enemies[_rng.randi() % all_enemies.size()]
	var enemy_data: Dictionary = {
		"type": enemy_type,
		"wave": _current_wave,
		"health": health_data.get("max_health", 50) if not health_data.is_empty() else 50,
		"name": health_data.get("name", "Enemy") if not health_data.is_empty() else "Enemy"
	}

	enemy_spawned.emit(enemy_data)

func on_enemy_killed() -> void:
	_enemies_remaining -= 1
	if _enemies_remaining <= 0:
		wave_completed.emit(_current_wave)
		print("[WaveManager] Wave %d completed!" % _current_wave)
		call_deferred("start_next_wave")

func get_current_wave() -> int:
	return _current_wave

func get_enemies_remaining() -> int:
	return _enemies_remaining

func is_active() -> bool:
	return _is_active

func get_progress() -> float:
	if max_waves <= 0:
		return 100.0
	return float(_current_wave) / float(max_waves) * 100.0
