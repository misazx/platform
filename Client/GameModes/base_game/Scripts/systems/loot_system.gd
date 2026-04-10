class_name LootSystem extends Node

signal loot_dropped(position: Vector2, item_ids: PackedStringArray)

@export var global_drop_chance: float = 0.3
@export var rarity_multiplier: float = 1.0

var _loot_tables: Dictionary = {}

func _ready() -> void:
	load_loot_tables()

func load_loot_tables() -> void:
	register_loot_table({
		"id": "common_enemy",
		"name": "Common Enemy Drops",
		"min_items": 0,
		"max_items": 2,
		"entries": [
			{"item_id": "health_potion_small", "weight": 10.0, "chance": 0.3},
			{"item_id": "gold_coin", "weight": 20.0, "min_count": 1, "max_count": 5}
		]
	})

	register_loot_table({
		"id": "elite_enemy",
		"name": "Elite Enemy Drops",
		"min_items": 1,
		"max_items": 3,
		"entries": [
			{"item_id": "health_potion_large", "weight": 15.0, "chance": 0.5},
			{"item_id": "iron_sword", "weight": 5.0, "chance": 0.2},
			{"item_id": "gold_coin", "weight": 30.0, "min_count": 5, "max_count": 15}
		]
	})

	register_loot_table({
		"id": "boss",
		"name": "Boss Drops",
		"min_items": 3,
		"max_items": 5,
		"entries": [
			{"item_id": "health_potion_large", "weight": 20.0, "min_count": 2, "max_count": 3},
			{"item_id": "steel_armor", "weight": 10.0, "chance": 1.0},
			{"item_id": "legendary_weapon", "weight": 5.0, "chance": 0.5},
			{"item_id": "gold_coin", "weight": 50.0, "min_count": 20, "max_count": 50}
		]
	})

	GD.print("[LootSystem] Loaded %d loot tables" % _loot_tables.size())

func register_loot_table(table: Dictionary) -> void:
	_loot_tables[table["id"]] = table

func get_loot_table(table_id: String) -> Dictionary:
	if _loot_tables.has(table_id):
		return _loot_tables[table_id]
	return {}

func generate_loot(table_id: String, luck_modifier: float = 0.0) -> Array:
	var table := get_loot_table(table_id)
	if table.is_empty():
		GD.printerr("[LootSystem] Loot table not found: %s" % table_id)
		return []

	var drops := []
	var item_count := randi_range(table["min_items"], table["max_items"])

	var weighted_entries := []
	var total_weight := 0.0
	for entry in table["entries"]:
		total_weight += entry["weight"]
		weighted_entries.append({"entry": entry, "cumulative_weight": total_weight})

	for i in range(item_count):
		var selected := select_weighted_entry(weighted_entries, total_weight)
		if selected == null:
			continue
		if randf() > selected.get("chance", 1.0) + luck_modifier:
			continue
		var count := randi_range(selected.get("min_count", 1), selected.get("max_count", 1))
		drops.append({"item_id": selected["item_id"], "count": count})

	GD.print("[LootSystem] Generated %d items from %s" % [drops.size(), table_id])
	return drops

func select_weighted_entry(entries: Array, total_weight: float) -> Dictionary:
	if entries.is_empty():
		return null
	var roll := randf() * total_weight
	for e in entries:
		if roll <= e["cumulative_weight"]:
			return e["entry"]
	return entries[-1]["entry"]

func drop_loot_at_position(table_id: String, position: Vector2, luck_modifier: float = 0.0) -> void:
	var drops := generate_loot(table_id, luck_modifier)
	var drop_ids := PackedStringArray()
	for drop in drops:
		for i in range(drop["count"]):
			var offset := Vector2(randf_range(-30.0, 30.0), randf_range(-30.0, 30.0))
			drop_ids.append(drop["item_id"])

	loot_dropped.emit(position, drop_ids)
	GD.print("[LootSystem] Dropped %d items at %s" % [drops.size(), position])

func drop_loot_from_enemy(table_id: String, enemy: Node, luck_modifier: float = 0.0) -> void:
	if enemy == null:
		return
	var pos: Vector2 = enemy.global_position if enemy.has_method("get_global_position") else Vector2.ZERO
	drop_loot_at_position(table_id, pos, luck_modifier)
