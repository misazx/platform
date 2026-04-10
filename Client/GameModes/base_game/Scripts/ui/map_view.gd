class_name MapView extends Control

var _node_layer: Control
var _connection_layer: Node2D
var _floor_label: Label
var _gold_label: Label
var _back_button: Button
var _node_uis: Array = []
var _current_map: Dictionary = {}
var _visited_node_ids: Array = []
var _reachable_node_ids: Array = []

static var _persistent_visited: Array = []
static var _persistent_reachable: Array = []

signal node_selected(node_data)

static func reset_persistent_state() -> void:
	_persistent_visited.clear()
	_persistent_reachable.clear()
	GD.print("[MapView] Persistent state reset for new game")

func _ready() -> void:
	GD.print("========== [MapView] _Ready() START ==========")
	
	_setup_node_references()
	_setup_signals()
	
	GD.print("[MapView] Creating immediate test visuals...")
	_create_test_visuals()
	
	GD.print("[MapView] Scheduling map load via CallDeferred...")
	call_deferred("_load_map_from_game_manager")
	
	GD.print("========== [MapView] _Ready() END ==========")

func _create_test_visuals() -> void:
	if _node_layer == null:
		GD.push_error("[MapView] CreateTestVisuals: _node_layer is NULL!")
		return

	var test_rect := ColorRect.new()
	test_rect.name = "TestDebugRect"
	test_rect.color = Color(0, 1, 0, 0.5)
	test_rect.position = Vector2(100, 200)
	test_rect.size = Vector2(80, 80)
	test_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_node_layer.add_child(test_rect)
	GD.print("[MapView] Test rect added at %s, size %s" % [test_rect.position, test_rect.size])

	var test_label := Label.new()
	test_label.text = "MAP NODES HERE"
	test_label.position = Vector2(80, 180)
	test_label.z_index = 100
	test_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	test_label.add_theme_font_size_override("font_size", 16)
	test_label.modulate = Color.YELLOW
	_node_layer.add_child(test_label)

func _setup_node_references() -> void:
	var map_container = get_node_or_null("MapContainer")
	
	if map_container != null:
		GD.print("[MapView] MapContainer FOUND: %s, size=%s" % [map_container.name, map_container.size])
		
		_node_layer = map_container.get_node_or_null("NodeLayer")
		if _node_layer == null:
			GD.push_warning("[MapView] NodeLayer not found, creating...")
			_node_layer = Control.new()
			_node_layer.name = "NodeLayer"
			_node_layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
			map_container.add_child(_node_layer)
		_node_layer.set_anchors_preset(Control.PRESET_FULL_RECT)
		GD.print("[MapView] NodeLayer ready: %s" % _node_layer.name)
		
		_connection_layer = map_container.get_node_or_null("ConnectionLayer")
		if _connection_layer == null:
			_connection_layer = Node2D.new()
			_connection_layer.name = "ConnectionLayer"
			map_container.add_child(_connection_layer)
		GD.print("[MapView] ConnectionLayer ready: %s" % _connection_layer.name)
	else:
		GD.push_error("[MapView] CRITICAL: MapContainer NOT found!")
		_node_layer = Control.new()
		_node_layer.name = "NodeLayer"
		_node_layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_connection_layer = Node2D.new()
		_connection_layer.name = "ConnectionLayer"
		add_child(_node_layer)
		add_child(_connection_layer)

	_floor_label = get_node_or_null("HeaderBar/FloorLabel")
	_gold_label = get_node_or_null("HeaderBar/GoldLabel")
	_back_button = get_node_or_null("HeaderBar/BackButton")
	
	if _back_button != null:
		_back_button.pressed.connect(_on_back_pressed)
	
	var header_bar = get_node_or_null("HeaderBar")
	if header_bar != null and header_bar.get_node_or_null("SaveButton") == null:
		var save_btn := Button.new()
		save_btn.name = "SaveButton"
		save_btn.text = "💾 存档"
		save_btn.custom_minimum_size = Vector2(90, 36)
		save_btn.mouse_filter = Control.MOUSE_FILTER_STOP
		save_btn.position = Vector2(400, 5)
		save_btn.pressed.connect(_on_save_pressed)
		header_bar.add_child(save_btn)
		GD.print("[MapView] SaveButton added to HeaderBar")
	
	GD.print("[MapView] Setup done - Layer:%s BackBtn:%s" % [_node_layer != null, _back_button != null])

func _on_save_pressed() -> void:
	GD.print("[MapView] Quick save pressed")

func _on_back_pressed() -> void:
	GD.print("[MapView] Back pressed")
	if Main.instance != null:
		Main.instance.go_to_main_menu()

func _setup_signals() -> void:
	pass

func _load_map_from_game_manager() -> void:
	GD.print("---------- [MapView] LoadMapFromGameManager() START ----------")
	
	_generate_demo_map()
	
	GD.print("---------- [MapView] LoadMapFromGameManager() END ----------")

func _generate_demo_map() -> void:
	GD.print("[MapView] GenerateDemoMap() start")
	
	var demo_map := {
		"floor_number": 1,
		"floor_name": "The Exordium",
		"width": 800,
		"height": 500,
		"nodes": [],
		"start_node_id": 0,
		"boss_node_id": -1
	}
	
	var id := 0
	
	demo_map.nodes.append({
		"id": id, "position": Vector2i(40, 250), "type": 0,
		"status": 1, "connected_nodes": [1, 2]
	})
	demo_map.start_node_id = id
	id += 1
	
	var enemies := ["Cultist", "JawWorm", "Louse", "Gremlin_Nob", "Cultist", "JawWorm"]
	var enemy_idx := 0
	
	var layer1 = [{"x": 160, "y": 120, "type": 1}, {"x": 160, "y": 350, "type": 4}]
	var layer2 = [{"x": 300, "y": 80, "type": 1}, {"x": 300, "y": 250, "type": 3}, {"x": 300, "y": 400, "type": 1}]
	var layer3 = [{"x": 440, "y": 150, "type": 2}, {"x": 440, "y": 350, "type": 5}]
	var layer4 = [{"x": 580, "y": 100, "type": 6}, {"x": 580, "y": 300, "type": 1}]
	var boss_node = {"x": 720, "y": 250, "type": 7}
	
	for n in layer1:
		demo_map.nodes.append({"id": id, "position": Vector2i(n.x, n.y), "type": n.type,
			"status": 0, "enemy_id": enemies[enemy_idx % enemies.size()] if n.type == 1 else "", "connected_nodes": []})
		if n.type == 1: enemy_idx += 1
		id += 1
	
	for n in layer2:
		demo_map.nodes.append({"id": id, "position": Vector2i(n.x, n.y), "type": n.type,
			"status": 0, "enemy_id": enemies[enemy_idx % enemies.size()] if n.type == 1 else ("Gremlin_Nob" if n.type == 2 else ""), "connected_nodes": []})
		if n.type == 1: enemy_idx += 1
		id += 1
	
	for n in layer3:
		demo_map.nodes.append({"id": id, "position": Vector2i(n.x, n.y), "type": n.type,
			"status": 0, "enemy_id": "Gremlin_Nob" if n.type == 2 else "", "connected_nodes": []})
		id += 1
	
	for n in layer4:
		demo_map.nodes.append({"id": id, "position": Vector2i(n.x, n.y), "type": n.type,
			"status": 0, "enemy_id": enemies[enemy_idx % enemies.size()] if n.type == 1 else "", "connected_nodes": []})
		if n.type == 1: enemy_idx += 1
		id += 1
	
	demo_map.nodes.append({"id": id, "position": Vector2i(boss_node.x, boss_node.y), "type": boss_node.type,
		"status": 0, "enemy_id": "The_Guardian", "connected_nodes": []})
	demo_map.boss_node_id = id
	id += 1
	
	demo_map.nodes[0].connected_nodes = [1, 2]
	demo_map.nodes[1].connected_nodes = [0, 3, 4]
	demo_map.nodes[2].connected_nodes = [0, 4, 5]
	demo_map.nodes[3].connected_nodes = [1, 6]
	demo_map.nodes[4].connected_nodes = [1, 2, 6, 7]
	demo_map.nodes[5].connected_nodes = [2, 7]
	demo_map.nodes[6].connected_nodes = [3, 4, 8]
	demo_map.nodes[7].connected_nodes = [4, 5, 9]
	demo_map.nodes[8].connected_nodes = [6, 10]
	demo_map.nodes[9].connected_nodes = [7, 10]
	demo_map.nodes[10].connected_nodes = [8, 9]
	
	set_map(demo_map)
	update_floor(1)
	update_gold(99)
	
	GD.print("[MapView] Demo map created with %d nodes" % demo_map.nodes.size())

func set_map(map_data: Dictionary) -> void:
	GD.print("[MapView] SetMap() called, nodes: %d" % map_data.nodes.size() if map_data.has("nodes") else 0)
	
	_current_map = map_data
	_clear_nodes()
	
	if not map_data.has("nodes") or map_data.nodes.is_empty():
		GD.push_warning("[MapView] SetMap: empty/null node list!")
		return
	
	_visited_node_ids.clear()
	_reachable_node_ids.clear()
	_persistent_visited.clear()
	_persistent_reachable.clear()
	
	var start_node = null
	for node in map_data.nodes:
		if node.id == map_data.start_node_id:
			start_node = node
			break
	
	if start_node != null:
		_visited_node_ids.append(start_node.id)
		_persistent_visited.append(start_node.id)
		start_node["is_visited"] = true
		start_node["status"] = 2
		for cid in start_node.connected_nodes:
			if not cid in _visited_node_ids:
				_reachable_node_ids.append(cid)
				_persistent_reachable.append(cid)
				for connected in map_data.nodes:
					if connected.id == cid:
						connected["status"] = 1
						break
		GD.print("[MapView] Fresh init: start=%d, reachable=%s" % [start_node.id, str(_reachable_node_ids)])
	
	for node in map_data.nodes:
		var is_reachable := node.id in _reachable_node_ids
		var is_visited := node.id in _visited_node_ids
		var node_ui := MapNodeUI.new(node, is_reachable, is_visited)
		node_ui.node_clicked.connect(_on_node_clicked)
		_node_uis.append(node_ui)
		_node_layer.add_child(node_ui)
		GD.print("[MapView] +Node %d (%d) at (%d,%d) reachable=%s visited=%s" % 
			[node.id, node.type, node.position.x, node.position.y, is_reachable, is_visited])
	
	_draw_connections()
	GD.print("[MapView] SetMap complete: %d nodes rendered" % _node_uis.size())

func _clear_nodes() -> void:
	for n in _node_uis:
		n.queue_free()
	_node_uis.clear()
	if _connection_layer != null:
		for c in _connection_layer.get_children():
			c.queue_free()

func _draw_connections() -> void:
	if _connection_layer == null: return
	for node_ui in _node_uis:
		var node_data = node_ui.node_data
		if not node_data.has("connected_nodes"): continue
		for cid in node_data.connected_nodes:
			var target = null
			for other_ui in _node_uis:
				if other_ui.node_data.id == cid:
					target = other_ui
					break
			if target != null:
				var line := Line2D.new()
				line.width = 3.0
				line.default_color = Color(0.6, 0.55, 0.45, 0.8)
				line.z_index = 0
				line.add_point(node_ui.get_center_position())
				line.add_point(target.get_center_position())
				_connection_layer.add_child(line)

func _on_node_clicked(node_ui) -> void:
	var node_data = node_ui.node_data
	GD.print("[MapView] Clicked: type=%d id=%d reachable=%s visited=%s" % 
		[node_data.type, node_data.id, node_data.id in _reachable_node_ids, node_data.id in _visited_node_ids])
	
	if node_data.id in _visited_node_ids:
		GD.print("[MapView] Node already visited, ignoring")
		return
	
	if not node_data.id in _reachable_node_ids:
		GD.print("[MapView] Node not reachable, ignoring")
		return
	
	_visited_node_ids.append(node_data.id)
	_persistent_visited.append(node_data.id)
	_reachable_node_ids.erase(node_data.id)
	_persistent_reachable.erase(node_data.id)
	node_data["is_visited"] = true
	node_data["status"] = 2
	
	for cid in node_data.connected_nodes:
		if not cid in _visited_node_ids:
			_reachable_node_ids.append(cid)
			_persistent_reachable.append(cid)
			for connected in _current_map.nodes:
				if connected.id == cid and connected.status == 0:
					connected["status"] = 1
					break
	
	_update_node_visuals()
	
	for n in _node_uis:
		n.highlighted = (n == node_ui)
	node_selected.emit(node_data)
	
	match node_data.type:
		1, 2, 7:
			pass
		3:
			pass
		5:
			pass
		4:
			pass
		6:
			pass
		_:
			GD.print("[MapView] Node type %d not implemented yet" % node_data.type)

func _update_node_visuals() -> void:
	for n in _node_uis:
		var is_reachable := n.node_data.id in _reachable_node_ids
		var is_visited := n.node_data.id in _visited_node_ids
		n.set_reachable_state(is_reachable, is_visited)

func update_floor(f: int) -> void:
	if _floor_label != null: _floor_label.text = "第 %d 层" % f

func update_gold(g: int) -> void:
	if _gold_label != null: _gold_label.text = "💰 %d" % g

func visit_node(node_data) -> void:
	for ui in _node_uis:
		if ui.node_data == node_data:
			ui.visited = true
			break

func on_show() -> void:
	visible = true
	call_deferred("_load_map_from_game_manager")

func on_hide() -> void:
	visible = false


class_name MapNodeUI extends Control

signal node_clicked(node_ui)

var node_data: Dictionary
var visited: bool = false:
	set(value):
		visited = value
		_update_appearance()
var highlighted: bool = false:
	set(value):
		highlighted = value
		var s := 1.25 if value else (0.9 if visited else 1.0)
		scale = Vector2(s, s)

var _visited_internal: bool = false
var _highlighted_internal: bool = false
var _reachable: bool = false
var _bg_rect: ColorRect
var _icon_label: Label
var _tooltip: Label

func _init(data: Dictionary, reachable: bool = false, visited: bool = false) -> void:
	node_data = data
	_reachable = reachable
	_visited_internal = visited

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	
	var x: float = float(node_data.position.x)
	var y: float = float(node_data.position.y)
	position = Vector2(x, y)
	size = Vector2(56, 56)
	
	var node_color := _get_color(node_data.type)
	var border_color := node_color.lightened(0.3)
	
	_bg_rect = ColorRect.new()
	_bg_rect.name = "BG"
	_bg_rect.color = Color(0.15, 0.13, 0.10, 0.95)
	_bg_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_bg_rect.set_anchors_preset(Control.PRESET_FULL_RECT)
	add_child(_bg_rect)
	
	var border_style := StyleBoxFlat.new()
	border_style.bg_color = Color(0.15, 0.13, 0.10, 0.95)
	border_style.corner_radius_top_left = 12
	border_style.corner_radius_top_right = 12
	border_style.corner_radius_bottom_left = 12
	border_style.corner_radius_bottom_right = 12
	border_style.border_width_left = 3
	border_style.border_width_right = 3
	border_style.border_width_top = 3
	border_style.border_width_bottom = 3
	border_style.border_color = border_color
	add_theme_stylebox_override("panel", border_style)
	
	_icon_label = Label.new()
	_icon_label.text = _get_icon_char(node_data.type)
	_icon_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_icon_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	_icon_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_icon_label.set_anchors_preset(Control.PRESET_FULL_RECT)
	_icon_label.add_theme_font_size_override("font_size", 22)
	_icon_label.modulate = node_color
	add_child(_icon_label)
	
	_tooltip = Label.new()
	_tooltip.text = _get_tooltip(node_data.type)
	_tooltip.visible = false
	_tooltip.z_index = 200
	_tooltip.position = Vector2(28, -50)
	_tooltip.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_tooltip.add_theme_font_size_override("font_size", 14)
	add_child(_tooltip)
	
	gui_input.connect(_on_gui_input)
	mouse_entered.connect(_on_mouse_entered)
	mouse_exited.connect(_on_mouse_exit)
	
	_apply_initial_state()
	
	GD.print("[MapNodeUI] Ready: type=%d pos=(%d,%d) color=%s reachable=%s visited=%s" % 
		[node_data.type, node_data.position.x, node_data.position.y, node_color, _reachable, _visited_internal])

func _apply_initial_state() -> void:
	if _visited_internal:
		_update_appearance()
	elif _reachable:
		modulate = Color(1, 1, 1, 1)
	else:
		modulate = Color(0.4, 0.4, 0.4, 0.6)
		mouse_filter = Control.MOUSE_FILTER_IGNORE

func set_reachable_state(reachable: bool, visited: bool) -> void:
	_reachable = reachable
	_visited_internal = visited
	
	if visited:
		_update_appearance()
	elif reachable:
		modulate = Color(1, 1, 1, 1)
		mouse_filter = Control.MOUSE_FILTER_STOP
		scale = Vector2.ONE
	else:
		modulate = Color(0.4, 0.4, 0.4, 0.6)
		mouse_filter = Control.MOUSE_FILTER_IGNORE
		scale = Vector2.ONE

func _get_icon_char(type_val: int) -> String:
	match type_val:
		1: return "⚔"
		2: return "★"
		7: return "👑"
		4: return "?"
		3: return "$"
		5: return "♥"
		6: return "◆"
		_: return "●"

func _get_color(type_val: int) -> Color:
	match type_val:
		1: return Color(0.9, 0.35, 0.35)
		2: return Color(1, 0.65, 0.2)
		7: return Color(0.95, 0.2, 0.2)
		4: return Color(0.55, 0.8, 0.35)
		3: return Color(0.4, 0.7, 1)
		5: return Color(0.35, 0.6, 0.35)
		6: return Color(1, 0.88, 0.3)
		_: return Color(0.6, 0.6, 0.6)

func _get_tooltip(type_val: int) -> String:
	match type_val:
		1: return "普通敌人"
		2: return "精英敌人"
		7: return "Boss"
		4: return "事件"
		3: return "商店"
		5: return "休息点"
		6: return "宝箱"
		_: return "起点"

func _update_appearance() -> void:
	if not _visited_internal: return
	var s := StyleBoxFlat.new()
	s.bg_color = Color(0.15, 0.22, 0.15, 0.8)
	s.corner_radius_top_left = 12
	s.corner_radius_top_right = 12
	s.corner_radius_bottom_left = 12
	s.corner_radius_bottom_right = 12
	s.border_width_left = 2
	s.border_width_right = 2
	s.border_width_top = 2
	s.border_width_bottom = 2
	s.border_color = Color(0.35, 0.5, 0.35, 0.7)
	add_theme_stylebox_override("panel", s)
	scale = Vector2(0.9, 0.9)
	modulate = Color(0.8, 0.8, 0.8, 0.85)

func _on_mouse_enter() -> void:
	if not _reachable and not _visited_internal: return
	_tooltip.visible = true
	if not highlighted and not _visited_internal:
		create_tween().tween_property(self, "scale", Vector2(1.15, 1.15), 0.1)

func _on_mouse_exit() -> void:
	_tooltip.visible = false
	if not highlighted and not _visited_internal and _reachable:
		create_tween().tween_property(self, "scale", Vector2.ONE, 0.1)

func _on_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		node_clicked.emit(self)

func get_center_position() -> Vector2:
	return position + size / 2
