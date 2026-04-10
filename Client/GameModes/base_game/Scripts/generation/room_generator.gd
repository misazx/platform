class_name RoomGenerator extends Node

var _rng: RandomNumberGenerator

func _init(rng: RandomNumberGenerator = null) -> void:
	_rng = rng if rng else RandomNumberGenerator.new()

func generate_room(id: int, x: int, y: int, type_str: String) -> Dictionary:
	return {
		"id": id,
		"x": x,
		"y": y,
		"type": type_str,
		"connections": [],
		"visited": false,
		"cleared": false,
		"loot_dropped": false,
		"rewards_claimed": false
	}

func generate_start_room(id: int) -> Dictionary:
	return generate_room(id, 100, 400, "start")

func generate_boss_room(id: int, width: int) -> Dictionary:
	return generate_room(id, width - 100, 400, "boss")

func generate_random_room(id: int, bounds: Rect2) -> Dictionary:
	var x := _rng.randi_range(int(bounds.position.x + 50), int(bounds.end.x - 50))
	var y := _rng.randi_range(int(bounds.position.y + 80), int(bounds.end.y - 80))
	var types := ["combat", "event", "shop", "rest", "treasure"]
	var weights := [35, 20, 10, 20, 15]
	var roll := _rng.randf()
	var cumulative := 0.0
	var chosen_type := "combat"
	for i in range(types.size()):
		cumulative += float(weights[i]) / 100.0
		if roll <= cumulative:
			chosen_type = types[i]
			break
	return generate_room(id, x, y, chosen_type)

func connect_rooms(room_a: Dictionary, room_b: Dictionary) -> void:
	if int(room_b["id"]) not in room_a["connections"]:
		room_a["connections"].append(int(room_b["id"]))
	if int(room_a["id"]) not in room_b["connections"]:
		room_b["connections"].append(int(room_a["id"]))

func has_connection(room_a: Dictionary, room_b: Dictionary) -> bool:
	return int(room_b["id"]) in room_a["connections"]

func get_distance(room_a: Dictionary, room_b: Dictionary) -> float:
	return Vector2(float(room_a["x"]), float(room_a["y"])).distance_to(Vector2(float(room_b["x"]), float(room_b["y"]))

func get_room_type_color(type_str: String) -> Color:
	match type_str:
		"start": return Color.GREEN
		"combat": return Color.RED
		"event": return Color.PURPLE
		"shop": return Color.YELLOW
		"rest": return Color.CYAN
		"treasure": return Color.ORANGE
		"boss": return Color.DARK_RED
		_: return Color.WHITE
