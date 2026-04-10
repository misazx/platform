enum DungeonLayout { LINEAR, BRANCHING, LOOP, GRID }

class_name DungeonGenerator extends Node

signal dungeon_generated(dungeon_json: String)

@export var min_rooms: int = 10
@export var max_rooms: int = 20
@export var room_spacing: int = 3
@export var branch_chance: float = 0.3
@export var loop_chance: float = 0.2

var _rng: RandomNumberGenerator
var _current_dungeon: Dictionary

func _ready() -> void:
	pass

func generate_dungeon(floor: int, seed_val: uint) -> Dictionary:
	_rng = RandomNumberGenerator.new()
	_rng.seed = seed_val + floor

	_current_dungeon = {
		"floor": floor,
		"seed": seed_val,
		"rooms": {},
		"start_room_id": 0,
		"boss_room_id": 0,
		"shop_room_ids": [],
		"treasure_room_ids": [],
		"layout": choose_layout(floor)
	}

	var room_count := _rng.randi_range(min_rooms, max_rooms + 1)
	GD.print("[DungeonGenerator] Generating floor %d with %d rooms" % [floor, room_count])

	generate_rooms(room_count)
	connect_rooms()
	assign_special_rooms()

	var json_str := JSON.stringify(_current_dungeon, "\t")
	dungeon_generated.emit(json_str)
	GD.print("[DungeonGenerator] Floor %d generated successfully" % floor)
	return _current_dungeon

func choose_layout(floor: int) -> int:
	var roll := _rng.randf()
	if floor <= 5:
		return DungeonLayout.LINEAR
	elif roll < branch_chance:
		return DungeonLayout.BRANCHING
	elif roll < branch_chance + loop_chance:
		return DungeonLayout.LOOP
	else:
		return DungeonLayout.GRID

func generate_rooms(count: int) -> void:
	for i in range(count):
		var room := {
			"id": i,
			"x": _rng.randi_range(50, 750),
			"y": _rng.randi_range(100, 500),
			"type": choose_room_type(i, count),
			"connections": [],
			"visited": false,
			"cleared": false,
			"loot_dropped": false
		}
		_current_dungeon["rooms"][i] = room

func choose_room_type(index: int, total: int) -> String:
	if index == 0:
		return "start"
	if index == total - 1:
		return "boss"
	var roll := _rng.randf()
	if roll < 0.35:
		return "combat"
	elif roll < 0.55:
		return "event"
	elif roll < 0.70:
		return "shop"
	elif roll < 0.85:
		return "treasure"
	else:
		return "rest"

func connect_rooms() -> void:
	var rooms: Dictionary = _current_dungeon["rooms"]
	var sorted_keys := rooms.keys()
	sorted_keys.sort()
	for i in range(sorted_keys.size() - 1):
		var current_id: int = sorted_keys[i]
		var next_id: int = sorted_keys[i + 1]
		rooms[current_id]["connections"].append(next_id)
		rooms[next_id]["connections"].append(current_id)

		if _rng.randf() < branch_chance and i < sorted_keys.size() - 2:
			var branch_target: int = sorted_keys[i + 2]
			rooms[current_id]["connections"].append(branch_target)
			rooms[branch_target]["connections"].append(current_id)

	_current_dungeon["start_room_id"] = 0
	_current_dungeon["boss_room_id"] = total_rooms() - 1

func assign_special_rooms() -> void:
	var rooms: Dictionary = _current_dungeon["rooms"]
	var shop_count := maxi(1, total_rooms() / 8)
	var treasure_count := maxi(1, total_rooms() / 10)
	var candidates := []
	for rid in rooms:
		var r: Dictionary = rooms[rid]
		if r["type"] == "shop":
			candidates.append(int(rid))
	for i in range(mini(shop_count, candidates.size())):
		_current_dungeon["shop_room_ids"].append(candidates[i])
	candidates.clear()
	for rid in rooms:
		var r: Dictionary = rooms[rid]
		if r["type"] == "treasure":
			candidates.append(int(rid))
	for i in range(mini(treasure_count, candidates.size())):
		_current_dungeon["treasure_room_ids"].append(candidates[i])

func get_current_dungeon() -> Dictionary:
	return _current_dungeon

func total_rooms() -> int:
	return _current_dungeon["rooms"].size()
