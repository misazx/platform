extends Node

enum ItemRarity { COMMON, UNCOMMON, RARE, EPIC, LEGENDARY }
enum ItemType { WEAPON, ARMOR, CONSUMABLE, PASSIVE, ACTIVE }

signal item_obtained(item_id: String)
signal item_used(item_id: String)

var _items: Dictionary = {}
var _item_pool: Array = []

func _ready() -> void:
	initialize_item_pool()

func initialize_item_pool() -> void:
	_item_pool = [
		{"id": "health_potion_small", "name": "小型生命药水", "type": ItemType.CONSUMABLE, "rarity": ItemRarity.COMMON, "icon_path": "res://GameModes/base_game/Resources/Images/Potions/HealthPotion.png"},
		{"id": "health_potion_large", "name": "大型生命药水", "type": ItemType.CONSUMABLE, "rarity": ItemRarity.UNCOMMON, "icon_path": "res://GameModes/base_game/Resources/Images/Potions/HealthPotion.png"},
		{"id": "gold_coin", "name": "金币", "type": ItemType.PASSIVE, "rarity": ItemRarity.COMMON, "icon_path": ""},
		{"id": "iron_sword", "name": "铁剑", "type": ItemType.WEAPON, "rarity": ItemRarity.UNCOMMON, "icon_path": ""},
		{"id": "steel_armor", "name": "钢甲", "type": ItemType.ARMOR, "rarity": ItemRarity.RARE, "icon_path": ""},
		{"id": "legendary_weapon", "name": "传说武器", "type": ItemType.WEAPON, "rarity": ItemRarity.LEGENDARY, "icon_path": ""}
	]
	print("[ItemManager] Initialized with %d items in pool" % _item_pool.size())

func get_item_data(item_id: String) -> Dictionary:
	for item: Dictionary in _item_pool:
		if item["id"] == item_id:
			return item
	return {}

func spawn_item(item_id: String, position: Vector2) -> void:
	var data: Dictionary = get_item_data(item_id)
	if not data.is_empty():
		item_obtained.emit(item_id)
		print("[ItemManager] Spawned item: %s at %s" % [item_id, position])

func add_item_to_inventory(item_id: String) -> bool:
	var data: Dictionary = get_item_data(item_id)
	if data.is_empty():
		return false
	_items[item_id] = data
	item_obtained.emit(item_id)
	print("[ItemManager] Added to inventory: %s" % item_id)
	return true

func remove_item(item_id: String) -> void:
	_items.erase(item_id)

func get_inventory() -> Dictionary:
	return _items.duplicate()

func has_item(item_id: String) -> bool:
	return _items.has(item_id)

func get_random_item(rarity: int = -1) -> Dictionary:
	var candidates: Array = []
	for item: Dictionary in _item_pool:
		if rarity < 0 or item["rarity"] == rarity:
			candidates.append(item)
	if candidates.is_empty():
		return {}
	return candidates[randi() % candidates.size()]
