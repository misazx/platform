extends Node

enum CardType { ATTACK, SKILL, POWER, STATUS, CURSE }
enum CardRarity { BASIC, COMMON, UNCOMMON, RARE, SPECIAL }
enum CardTarget { SELF, SINGLE_ENEMY, ALL_ENEMIES, RANDOM_ENEMY, NONE }

signal card_registered(card_id: String)

var _cards: Dictionary = {}
var _character_cards: Dictionary = {}
var _type_cards: Dictionary = {}
static var instance: Node = null

func _ready() -> void:
	if instance != null and instance != self:
		queue_free()
		return
	instance = self
	_load_cards_from_config()

func _load_cards_from_config() -> void:
	var config := ConfigLoader.load_config("cards")
	if config == null or not config.has("cards"):
		push_error("[CardDatabase] Failed to load cards config!")
		return
	
	for card_config in config.cards:
		var card_data := _convert_config_to_data(card_config)
		register_card(card_data)
	
	print("[CardDatabase] Loaded %d cards from config" % _cards.size())

func _convert_config_to_data(config: Dictionary) -> Dictionary:
	return {
		"id": config.get("id", ""),
		"name": config.get("name", ""),
		"description": config.get("description", ""),
		"cost": config.get("cost", 1),
		"type": _parse_card_type(config.get("type", "attack")),
		"rarity": _parse_card_rarity(config.get("rarity", "common")),
		"target": _parse_card_target(config.get("target", "singleenemy")),
		"damage": config.get("damage", 0),
		"block": config.get("block", 0),
		"magic_number": config.get("magic_number", 0),
		"upgraded": config.get("upgraded", false),
		"keywords": config.get("keywords", []).duplicate(),
		"custom_data": config.get("custom_data", {}).duplicate(),
		"character_id": config.get("character_id", ""),
		"icon_path": config.get("icon_path", ""),
		"color": config.get("color", "#FFFFFF"),
		"is_exhaust": config.get("is_exhaust", false),
		"is_ethereal": config.get("is_ethereal", false),
		"is_innate": config.get("is_innate", false)
	}

func _parse_card_type(type_str: String) -> int:
	match type_str.to_lower():
		"attack": return CardType.ATTACK
		"skill": return CardType.SKILL
		"power": return CardType.POWER
		"status": return CardType.STATUS
		"curse": return CardType.CURSE
		_: return CardType.ATTACK

func _parse_card_rarity(rarity_str: String) -> int:
	match rarity_str.to_lower():
		"basic": return CardRarity.BASIC
		"common": return CardRarity.COMMON
		"uncommon": return CardRarity.UNCOMMON
		"rare": return CardRarity.RARE
		"special": return CardRarity.SPECIAL
		_: return CardRarity.COMMON

func _parse_card_target(target_str: String) -> int:
	match target_str.to_lower():
		"self": return CardTarget.SELF
		"singleenemy": return CardTarget.SINGLE_ENEMY
		"allenemies": return CardTarget.ALL_ENEMIES
		"randomenemy": return CardTarget.RANDOM_ENEMY
		"none": return CardTarget.NONE
		_: return CardTarget.SINGLE_ENEMY

func register_card(card_data: Dictionary) -> void:
	var id: String = card_data.get("id", "")
	_cards[id] = card_data
	
	var char_id: String = card_data.get("character_id", "")
	if not _character_cards.has(char_id):
		_character_cards[char_id] = []
	_character_cards[char_id].append(card_data)
	
	var type_val: int = card_data.get("type", CardType.ATTACK)
	if not _type_cards.has(type_val):
		_type_cards[type_val] = []
	_type_cards[type_val].append(card_data)
	
	card_registered.emit(id)

func get_card(card_id: String) -> Dictionary:
	return _cards.get(card_id, {}) if _cards.has(card_id) else {}

func get_all_cards() -> Array:
	return _cards.values().duplicate()

func get_character_cards(character_id: String) -> Array:
	return _character_cards.get(character_id, []).duplicate()

func get_cards_by_type(type_val: int) -> Array:
	return _type_cards.get(type_val, []).duplicate()

func get_cards_by_rarity(rarity: int) -> Array:
	var result := []
	for card in _cards.values():
		if card.get("rarity", 0) == rarity:
			result.append(card)
	return result

func search_cards(query: String) -> Array:
	var result := []
	var query_lower := query.to_lower()
	for card in _cards.values():
		var name: String = card.get("name", "")
		var desc: String = card.get("description", "")
		if query_lower in name.to_lower() or query_lower in desc.to_lower():
			result.append(card)
	return result

func total_cards() -> int:
	return _cards.size()
