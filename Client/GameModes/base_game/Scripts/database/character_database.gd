extends Node
enum CharacterClass { IRONCLAD, SILENT, DEFECT, WATCHER, NECROMANCER, HEIR }
enum PlayStyle { AGGRESSIVE, DEFENSIVE, HYBRID, COMBO, CONTROL }


signal character_selected(character_id: String)

var _characters: Dictionary = {}
var _playable_characters: Array = []

func _ready() -> void:
	load_characters_from_config()

func load_characters_from_config() -> void:
	var config := ConfigLoader.load_config("characters")
	if config.is_empty():
		push_error("[CharacterDatabase] Failed to load characters config!")
		return

	if not config.has("characters"):
		return

	for char_cfg in config["characters"]:
		var char_data := convert_config_to_data(char_cfg)
		register_character(char_data)

	print("[CharacterDatabase] Loaded %d characters from config (version: %s)" % [_characters.size(), config.get("version", "")])

func convert_config_to_data(cfg: Dictionary) -> Dictionary:
	return {
		"id": cfg.get("id", ""),
		"name": cfg.get("name", ""),
		"title": cfg.get("title", ""),
		"description": cfg.get("description", ""),
		"class": parse_character_class(cfg.get("class", "ironclad")),
		"style": parse_play_style(cfg.get("style", "hybrid")),
		"max_health": int(cfg.get("maxHealth", 80)),
		"starting_gold": int(cfg.get("startingGold", 99)),
		"portrait_path": cfg.get("portraitPath", ""),
		"background_color": cfg.get("backgroundColor", "#FF0000"),
		"starting_cards": Array(cfg.get("startingCards", [])),
		"unique_mechanics": Array(cfg.get("uniqueMechanics", [])),
		"stats": Dictionary(cfg.get("stats", {})),
		"custom_data": Dictionary(cfg.get("customData", {})),
		"difficulty_rating": float(cfg.get("difficultyRating", 3.0)),
		"difficulty_description": cfg.get("difficultyDescription", "")
	}

func parse_character_class(class_str: String) -> int:
	match class_str.to_lower():
		"ironclad": return CharacterClass.IRONCLAD
		"silent": return CharacterClass.SILENT
		"defect": return CharacterClass.DEFECT
		"watcher": return CharacterClass.WATCHER
		"necromancer": return CharacterClass.NECROMANCER
		"heir": return CharacterClass.HEIR
		_: return CharacterClass.IRONCLAD

func parse_play_style(style_str: String) -> int:
	match style_str.to_lower():
		"aggressive": return PlayStyle.AGGRESSIVE
		"defensive": return PlayStyle.DEFENSIVE
		"hybrid": return PlayStyle.HYBRID
		"combo": return PlayStyle.COMBO
		"control": return PlayStyle.CONTROL
		_: return PlayStyle.HYBRID

func register_character(char_data: Dictionary) -> void:
	_characters[char_data["id"]] = char_data
	_playable_characters.append(char_data)
	print("[CharacterDatabase] Registered character: %s" % char_data["name"])

func get_character(character_id: String) -> Dictionary:
	if _characters.has(character_id):
		return _characters[character_id]
	return {}

func get_all_characters() -> Array:
	return _playable_characters.duplicate()

func get_characters_by_style(style: int) -> Array:
	var result := []
	for character in _playable_characters:
		if character["style"] == style:
			result.append(character)
	return result

func get_beginner_friendly_characters(max_difficulty: float = 3.0) -> Array:
	var result := []
	for character in _playable_characters:
		if character["difficulty_rating"] <= max_difficulty:
			result.append(character)
	return result

func get_total_characters() -> int:
	return _characters.size()
