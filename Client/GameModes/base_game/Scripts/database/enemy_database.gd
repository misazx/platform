enum EnemyType { NORMAL, ELITE, BOSS }
enum EnemyBehavior { AGGRESSIVE, DEFENSIVE, SUPPORT, SUMMONER }

class_name EnemyDatabase extends Node

signal enemy_registered(enemy_id: String)

var _enemies: Dictionary = {}
var _type_enemies: Dictionary = {}

func _ready() -> void:
	load_enemies_from_config()

func load_enemies_from_config() -> void:
	var config := ConfigLoader.load_config("enemies")
	if config.is_empty():
		GD.printerr("[EnemyDatabase] Failed to load enemies config!")
		return

	if not config.has("enemies"):
		return

	for enemy_cfg in config["enemies"]:
		var enemy_data := convert_config_to_data(enemy_cfg)
		register_enemy(enemy_data)

	GD.print("[EnemyDatabase] Loaded %d enemies from config (version: %s)" % [_enemies.size(), config.get("version", "")])

func convert_config_to_data(cfg: Dictionary) -> Dictionary:
	return {
		"id": cfg.get("id", ""),
		"name": cfg.get("name", ""),
		"description": cfg.get("description", ""),
		"max_health": int(cfg.get("maxHealth", 50)),
		"attack_damage": int(cfg.get("attackDamage", 10)),
		"block_amount": int(cfg.get("blockAmount", 5)),
		"type": parse_enemy_type(cfg.get("type", "normal")),
		"behavior": parse_enemy_behavior(cfg.get("behavior", "aggressive")),
		"portrait_path": cfg.get("portraitPath", ""),
		"icon_path": cfg.get("iconPath", ""),
		"abilities": Array(cfg.get("abilities", [])),
		"drops": Array(cfg.get("drops", [])),
		"stats": Dictionary(cfg.get("stats", {})),
		"custom_data": Dictionary(cfg.get("customData", {})),
		"difficulty_rating": float(cfg.get("difficultyRating", 1.0)),
		"encounter_location": cfg.get("encounterLocation", "")
	}

func parse_enemy_type(type_str: String) -> int:
	match type_str.to_lower():
		"normal": return EnemyType.NORMAL
		"elite": return EnemyType.ELITE
		"boss": return EnemyType.BOSS
		_: return EnemyType.NORMAL

func parse_enemy_behavior(behavior_str: String) -> int:
	match behavior_str.to_lower():
		"aggressive": return EnemyBehavior.AGGRESSIVE
		"defensive": return EnemyBehavior.DEFENSIVE
		"support": return EnemyBehavior.SUPPORT
		"summoner": return EnemyBehavior.SUMMONER
		_: return EnemyBehavior.AGGRESSIVE

func register_enemy(enemy_data: Dictionary) -> void:
	_enemies[enemy_data["id"]] = enemy_data
	var enemy_type: int = enemy_data["type"]
	if not _type_enemies.has(enemy_type):
		_type_enemies[enemy_type] = []
	_type_enemies[enemy_type].append(enemy_data)
	enemy_registered.emit(enemy_data["id"])

func get_enemy(enemy_id: String) -> Dictionary:
	if _enemies.has(enemy_id):
		return _enemies[enemy_id]
	return {}

func get_all_enemies() -> Array:
	return _enemies.values()

func get_enemies_by_type(type_val: int) -> Array:
	if _type_enemies.has(type_val):
		return _type_enemies[type_val]
	return []

func get_normal_enemies() -> Array:
	return get_enemies_by_type(EnemyType.NORMAL)

func get_elite_enemies() -> Array:
	return get_enemies_by_type(EnemyType.ELITE)

func get_boss_enemies() -> Array:
	return get_enemies_by_type(EnemyType.BOSS)

func get_enemies_by_location(location: String) -> Array:
	var result := []
	for enemy in _enemies.values():
		if enemy["encounter_location"] == location:
			result.append(enemy)
	return result

func get_total_enemies() -> int:
	return _enemies.size()
