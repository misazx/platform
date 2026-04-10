enum PotionType { ATTACK, DEFENSE, UTILITY, SPECIAL }

class_name PotionDatabase extends Node

signal potion_used(potion_id: String)

var _potions: Dictionary = {}
var _type_potions: Dictionary = {}

func _ready() -> void:
	load_potions_from_config()

func load_potions_from_config() -> void:
	var config := ConfigLoader.load_config("potions")
	if config.is_empty():
		GD.printerr("[PotionDatabase] Failed to load potions config!")
		return

	if not config.has("potions"):
		return

	for potion_cfg in config["potions"]:
		var potion_data := convert_config_to_data(potion_cfg)
		register_potion(potion_data)

	GD.print("[PotionDatabase] Loaded %d potions from config (version: %s)" % [_potions.size(), config.get("version", "")])

func convert_config_to_data(cfg: Dictionary) -> Dictionary:
	return {
		"id": cfg.get("id", ""),
		"name": cfg.get("name", ""),
		"description": cfg.get("description", ""),
		"type": parse_potion_type(cfg.get("type", "utility")),
		"price": int(cfg.get("price", 50)),
		"rarity": int(cfg.get("rarity", 1)),
		"image_path": cfg.get("imagePath", ""),
		"color": cfg.get("color", "#FF00FF"),
		"effects": Dictionary(cfg.get("effects", {})),
		"tags": Array(cfg.get("tags", [])),
		"is_stackable": bool(cfg.get("isStackable", false)),
		"max_stack": int(cfg.get("maxStack", 3))
	}

func parse_potion_type(type_str: String) -> int:
	match type_str.to_lower():
		"attack": return PotionType.ATTACK
		"defense": return PotionType.DEFENSE
		"utility": return PotionType.UTILITY
		"special": return PotionType.SPECIAL
		_: return PotionType.UTILITY

func register_potion(potion_data: Dictionary) -> void:
	_potions[potion_data["id"]] = potion_data
	var potion_type: int = potion_data["type"]
	if not _type_potions.has(potion_type):
		_type_potions[potion_type] = []
	_type_potions[potion_type].append(potion_data)

func get_potion(potion_id: String) -> Dictionary:
	if _potions.has(potion_id):
		return _potions[potion_id]
	return {}

func get_all_potions() -> Array:
	return _potions.values()

func get_potions_by_type(type_val: int) -> Array:
	if _type_potions.has(type_val):
		return _type_potions[type_val]
	return []

func get_random_potion() -> Dictionary:
	if _potions.is_empty():
		return {}
	var all_values := _potions.values()
	return all_values[randi() % all_values.size()]

func get_total_potions() -> int:
	return _potions.size()
