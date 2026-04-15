extends Node
enum RelicTier { STARTER, COMMON, UNCOMMON, RARE, BOSS, SPECIAL, SHOP }
enum RelicType { PASSIVE, ACTIVE, CONSUMABLE }


signal relic_collected(relic_id: String)

var _relics: Dictionary = {}
var _tier_relics: Dictionary = {}

func _ready() -> void:
	load_relics_from_config()

func load_relics_from_config() -> void:
	var config := ConfigLoader.load_config("relics")
	if config.is_empty():
		push_error("[RelicDatabase] Failed to load relics config!")
		return

	if not config.has("relics"):
		return

	for relic_cfg in config["relics"]:
		var relic_data := convert_config_to_data(relic_cfg)
		register_relic(relic_data)

	print("[RelicDatabase] Loaded %d relics from config (version: %s)" % [_relics.size(), config.get("version", "")])

func convert_config_to_data(cfg: Dictionary) -> Dictionary:
	return {
		"id": cfg.get("id", ""),
		"name": cfg.get("name", ""),
		"description": cfg.get("description", ""),
		"flavor_text": cfg.get("flavorText", ""),
		"tier": parse_relic_tier(cfg.get("tier", "common")),
		"type": parse_relic_type(cfg.get("type", "passive")),
		"image_path": cfg.get("imagePath", ""),
		"icon_path": cfg.get("iconPath", ""),
		"compatible_characters": Array(cfg.get("compatibleCharacters", [])),
		"effects": Dictionary(cfg.get("effects", {})),
		"stats": Dictionary(cfg.get("stats", {})),
		"is_counterpart": bool(cfg.get("isCounterpart", false)),
		"counterpart_id": cfg.get("counterpartId", "")
	}

func parse_relic_tier(tier_str: String) -> int:
	match tier_str.to_lower():
		"starter": return RelicTier.STARTER
		"common": return RelicTier.COMMON
		"uncommon": return RelicTier.UNCOMMON
		"rare": return RelicTier.RARE
		"boss": return RelicTier.BOSS
		"special": return RelicTier.SPECIAL
		"shop": return RelicTier.SHOP
		_: return RelicTier.COMMON

func parse_relic_type(type_str: String) -> int:
	match type_str.to_lower():
		"passive": return RelicType.PASSIVE
		"active": return RelicType.ACTIVE
		"consumable": return RelicType.CONSUMABLE
		_: return RelicType.PASSIVE

func register_relic(relic_data: Dictionary) -> void:
	_relics[relic_data["id"]] = relic_data
	var tier: int = relic_data["tier"]
	if not _tier_relics.has(tier):
		_tier_relics[tier] = []
	_tier_relics[tier].append(relic_data)
	relic_collected.emit(relic_data["id"])

func get_relic(relic_id: String) -> Dictionary:
	if _relics.has(relic_id):
		return _relics[relic_id]
	return {}

func get_all_relics() -> Array:
	return _relics.values()

func get_relics_by_tier(tier: int) -> Array:
	if _tier_relics.has(tier):
		return _tier_relics[tier]
	return []

func get_starter_relics() -> Array:
	return get_relics_by_tier(RelicTier.STARTER)

func get_common_relics() -> Array:
	return get_relics_by_tier(RelicTier.COMMON)

func get_uncommon_relics() -> Array:
	return get_relics_by_tier(RelicTier.UNCOMMON)

func get_rare_relics() -> Array:
	return get_relics_by_tier(RelicTier.RARE)

func get_boss_relics() -> Array:
	return get_relics_by_tier(RelicTier.BOSS)

func get_relics_for_character(character_id: String) -> Array:
	var result := []
	for relic in _relics.values():
		var compat: Array = relic["compatible_characters"]
		if character_id in compat or "*" in compat:
			result.append(relic)
	return result

func get_random_relic(tier: int) -> Dictionary:
	var relics := get_relics_by_tier(tier)
	if relics.is_empty():
		return {}
	return relics[randi() % relics.size()]

func get_total_relics() -> int:
	return _relics.size()
