enum ShopItemType { CARD, RELIC, POTION, CARD_REMOVAL, CARD_UPGRADE }

class_name ShopManager extends Node

signal item_purchased(item_id: String)
signal shop_entered()

@export var base_card_price: int = 50
@export var base_relic_price: int = 150
@export var base_potion_price: int = 80
@export var card_removal_price: int = 100
@export var card_upgrade_price: int = 100

var _current_items: Array = []
var _rng: RandomNumberGenerator

func _ready() -> void:
	pass

func initialize(seed: int) -> void:
	_rng = RandomNumberGenerator.new()
	_rng.seed = seed

func generate_shop_inventory(character_id: String, floor: int) -> Array:
	_current_items.clear()

	for i in range(3):
		var card := generate_card_offer(character_id)
		if not card.is_empty():
			_current_items.append(create_card_item(card))

	var relic_count := _rng.randi_range(1, 2)
	for i in range(relic_count):
		var relic := generate_relic_offer(character_id)
		if not relic.is_empty():
			_current_items.append(create_relic_item(relic))

	var potion_count := _rng.randi_range(1, 3)
	for i in range(potion_count):
		var potion := generate_potion_offer()
		if not potion.is_empty():
			_current_items.append(create_potion_item(potion))

	shop_entered.emit()
	GD.print("[ShopManager] Generated %d shop items for floor %d" % [_current_items.size(), floor])
	return _current_items.duplicate()

func generate_card_offer(character_id: String) -> Dictionary:
	var all_cards := CardDatabase.get_character_cards(character_id)
	if all_cards.is_empty():
		return {}
	return all_cards[_rng.randi() % all_cards.size()]

func generate_relic_offer(character_id: String) -> Dictionary:
	var relics := RelicDatabase.get_relics_for_character(character_id)
	if relics.is_empty():
		return {}
	return relics[_rng.randi() % relics.size()]

func generate_potion_offer() -> Dictionary:
	var potions := PotionDatabase.get_all_potions()
	if potions.is_empty():
		return {}
	return potions[_rng.randi() % potions.size()]

func create_card_item(card_data: Dictionary) -> Dictionary:
	return {
		"id": card_data["id"],
		"name": card_data["name"],
		"type": ShopItemType.CARD,
		"price": base_card_price,
		"data": card_data,
		"is_purchased": false,
		"is_available": true,
		"description": card_data["description"],
		"icon_path": card_data.get("icon_path", "")
	}

func create_relic_item(relic_data: Dictionary) -> Dictionary:
	return {
		"id": relic_data["id"],
		"name": relic_data["name"],
		"type": ShopItemType.RELIC,
		"price": base_relic_price,
		"data": relic_data,
		"is_purchased": false,
		"is_available": true,
		"description": relic_data["description"],
		"icon_path": relic_data.get("icon_path", "")
	}

func create_potion_item(potion_data: Dictionary) -> Dictionary:
	return {
		"id": potion_data["id"],
		"name": potion_data["name"],
		"type": ShopItemType.POTION,
		"price": base_potion_price,
		"data": potion_data,
		"is_purchased": false,
		"is_available": true,
		"description": potion_data["description"],
		"icon_path": potion_data.get("image_path", "")
	}

func purchase_item(item_id: String) -> bool:
	for item in _current_items:
		if item["id"] == item_id and not item["is_purchased"]:
			item["is_purchased"] = true
			item_purchased.emit(item_id)
			GD.print("[ShopManager] Purchased item: %s" % item_id)
			return true
	return false

func get_current_items() -> Array:
	return _current_items.duplicate()

func get_available_items() -> Array:
	var available := []
	for item in _current_items:
		if item["is_available"] and not item["is_purchased"]:
			available.append(item)
	return available
