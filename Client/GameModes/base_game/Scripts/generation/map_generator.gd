class_name MapGenerator extends Node

signal map_generated(map_data: Dictionary)

@export var map_width: int = 1200
@export var map_height: int = 800
@export var node_radius: float = 25.0

var _current_map: Dictionary

func _ready() -> void:
	pass

func generate_map(dungeon_data: Dictionary) -> Dictionary:
	_current_map = {
		"width": map_width,
		"height": map_height,
		"nodes": [],
		"paths": [],
		"current_floor": dungeon_data.get("floor", 1),
		"background": "res://GameModes/base_game/Resources/Images/Backgrounds/overgrowth.png",
		"player_position": Vector2.ZERO,
		"visited_nodes": []
	}

	create_map_nodes(dungeon_data)
	create_map_paths(dungeon_data)

	map_generated.emit(_current_map)
	GD.print("[MapGenerator] Map generated with %d nodes, %d paths" % [_current_map["nodes"].size(), _current_map["paths"].size()])
	return _current_map

func create_map_nodes(dungeon_data: Dictionary) -> void:
	var rooms: Dictionary = dungeon_data.get("rooms", {})
	for room_id in rooms:
		var room: Dictionary = rooms[room_id]
		var node := {
			"id": int(room_id),
			"position": Vector2(float(room["x"]), float(room["y"])),
			"type": room["type"],
			"visited": bool(room.get("visited", false)),
			"cleared": bool(room.get("cleared", false)),
			"radius": node_radius,
			"icon": get_node_icon(room["type"]),
			"label": get_node_label(room["type"], int(room_id))
		}
		_current_map["nodes"].append(node)

		if room["type"] == "start":
			_current_map["player_position"] = node["position"]

func create_map_paths(dungeon_data: Dictionary) -> void:
	var rooms: Dictionary = dungeon_data.get("rooms", {})
	for room_id in rooms:
		var room: Dictionary = rooms[room_id]
		var connections: Array = room.get("connections", [])
		for conn_id in connections:
			var path_exists := false
			for path in _current_map["paths"]:
				if (path["from"] == int(room_id) and path["to"] == int(conn_id)) or \
				   (path["from"] == int(conn_id) and path["to"] == int(room_id)):
					path_exists = true
					break
			if not path_exists:
				_current_map["paths"].append({
					"from": int(room_id),
					"to": int(conn_id),
					"style": "normal"
				})

func get_node_icon(type_str: String) -> String:
	match type_str:
		"start": return "res://GameModes/base_game/Resources/Icons/Services/start.png"
		"combat": return "res://GameModes/base_game/Resources/Icons/Skills/sword.png"
		"event": return "res://GameModes/base_game/Resources/Icons/Items/question_mark.png"
		"shop": return "res://GameModes/base_game/Resources/Icons/Services/shop.png"
		"rest": return "res://GameModes/base_game/Resources/Icons/Rest/campfire.png"
		"treasure": return "res://GameModes/base_game/Resources/Icons/Items/chest.png"
		"boss": return "res://GameModes/base_game/Resources/Icons/Enemies/boss.png"
		_: return ""

func get_node_label(type_str: String, id: int) -> String:
	match type_str:
		"start": return "起点"
		"combat": return "战斗"
		"event": return "事件"
		"shop": return "商店"
		"rest": return "休息"
		"treasure": return "宝箱"
		"boss": return "Boss"
		_: return "房间%d" % id

func get_current_map() -> Dictionary:
	return _current_map

func visit_node(node_id: int) -> void:
	for node in _current_map["nodes"]:
		if node["id"] == node_id:
			node["visited"] = true
			_current_map["visited_nodes"].append(node_id)
			break
