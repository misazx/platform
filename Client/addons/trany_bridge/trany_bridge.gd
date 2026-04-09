@tool
extends EditorPlugin

## TranyCode AI Bridge - Editor Plugin for Godot 4.x
## 负责开启本地 TCP 服务器并监听 AI 的指令

var server: TCPServer
var peer: StreamPeerTCP
var port: int = 11411
var error_buffer: Array = []
var debugger: TranyDebugger

class TranyDebugger extends EditorDebuggerPlugin:
	var bridge_ref
	
	func _setup_session(session_id: int) -> void:
		var session = get_session(session_id)
		session.started.connect(_on_session_started.bind(session))
		session.stopped.connect(_on_session_stopped.bind(session))

	func _on_session_started(session: EditorDebuggerSession) -> void:
		session.add_session_tab(Control.new()) # 占位
		
	func _on_session_stopped(session: EditorDebuggerSession) -> void:
		pass

	func _capture(message: String, data: Array, session_id: int) -> bool:
		# 捕获错误和警告
		if message == "debug:error" or message == "debug:warning":
			var err_data = {
				"time": Time.get_time_string_from_system(),
				"type": message.split(":")[1],
				"content": data[0],
				"stack": data[1] if data.size() > 1 else []
			}
			bridge_ref.error_buffer.append(err_data)
			if bridge_ref.error_buffer.size() > 50:
				bridge_ref.error_buffer.remove_at(0)
		return false

func _enter_tree() -> void:
	server = TCPServer.new()
	var err = server.listen(port)
	if err == OK:
		print("🚀 TranyCode AI Bridge: Listening on port ", port)
	else:
		print("❌ TranyCode AI Bridge: Failed to start server on port ", port)
	
	debugger = TranyDebugger.new()
	debugger.bridge_ref = self
	add_debugger_plugin(debugger)

func _exit_tree() -> void:
	if server:
		server.stop()
	remove_debugger_plugin(debugger)
	print("🛑 TranyCode AI Bridge: Stopped.")

func _process(_delta: float) -> void:
	if not server or not server.is_listening():
		return
	
	if server.is_connection_available():
		peer = server.take_connection()
		print("🔌 TranyCode AI Bridge: AI Connected.")
		
	if peer and peer.get_status() == StreamPeerTCP.STATUS_CONNECTED:
		if peer.get_available_bytes() > 0:
			var raw_data = peer.get_utf8_string(peer.get_available_bytes())
			var response = _handle_command(raw_data)
			peer.put_utf8_string(response + "\n")

func _handle_command(json_str: String) -> String:
	var json = JSON.new()
	var error = json.parse(json_str)
	if error != OK:
		return JSON.stringify({"error": "Invalid JSON"})
	
	var data = json.data
	var cmd = data.get("command", "")
	var root = get_editor_interface().get_edited_scene_root()
	
	match cmd:
		"ping":
			return JSON.stringify({"status": "ok", "version": "4.6-ready"})
		
		"get_scene_root":
			if root:
				return JSON.stringify({
					"name": root.name,
					"class": root.get_class(),
					"filename": root.scene_file_path
				})
			return JSON.stringify({"error": "No scene open"})
		
		"get_tree":
			if not root: return JSON.stringify({"error": "No scene open"})
			return JSON.stringify(_serialize_node(root))
			
		"get_selected_nodes":
			var selection = get_editor_interface().get_selection().get_selected_nodes()
			var names = []
			for n in selection: names.append(n.name)
			return JSON.stringify({"selected": names})

		"select_node":
			var path = data.get("path", "")
			var node = root.get_node_or_null(path) if root else null
			if node:
				get_editor_interface().get_selection().clear()
				get_editor_interface().get_selection().add_node(node)
				get_editor_interface().inspect_node(node)
				_flash_node(node) # 选中时自动闪烁
				return JSON.stringify({"status": "selected", "node": node.name})
			return JSON.stringify({"error": "Node not found"})

		"flash_node":
			var path = data.get("path", "")
			var node = root.get_node_or_null(path) if root else null
			if node:
				_flash_node(node)
				return JSON.stringify({"status": "flashed"})
			return JSON.stringify({"error": "Node not found"})

		"connect_signal":
			var from_path = data.get("from", "")
			var to_path = data.get("to", ".") 
			var signal_name = data.get("signal", "")
			var method_name = data.get("method", "")
			
			var from_node = root.get_node_or_null(from_path) if root else null
			var to_node = root.get_node_or_null(to_path) if root else null
			
			if from_node and to_node:
				if from_node.has_signal(signal_name):
					var err = from_node.connect(signal_name, Callable(to_node, method_name))
					if err == OK:
						return JSON.stringify({"status": "connected_runtime", "tip": "Permanent connection via AI is in progress."})
					return JSON.stringify({"error": "Connect failed"})
				return JSON.stringify({"error": "Signal not found"})
			return JSON.stringify({"error": "Node(s) not found"})

		"get_properties":
			var path = data.get("path", "")
			var node = root.get_node_or_null(path) if root else null
			if node:
				return JSON.stringify(_get_node_props(node))
			return JSON.stringify({"error": "Node not found"})

		"set_property":
			var path = data.get("path", "")
			var prop = data.get("property", "")
			var val = data.get("value")
			var node = root.get_node_or_null(path) if root else null
			if node:
				node.set(prop, val)
				return JSON.stringify({"status": "updated", "property": prop, "new_value": val})
			return JSON.stringify({"error": "Node not found"})

		"add_node":
			var parent_path = data.get("parent", ".")
			var node_type = data.get("type", "Node")
			var node_name = data.get("name", "NewNode")
			
			var parent = root.get_node_or_null(parent_path) if root else null
			if parent:
				var new_node = ClassDB.instantiate(node_type)
				new_node.name = node_name
				parent.add_child(new_node)
				new_node.owner = root # 确保能在场景中保存
				return JSON.stringify({"status": "created", "path": str(new_node.get_path())})
			return JSON.stringify({"error": "Parent not found"})

		"get_screenshot":
			var viewport = get_editor_interface().get_editor_viewport_2d()
			if not viewport: return JSON.stringify({"error": "Viewport not found"})
			
			var img = viewport.get_texture().get_image()
			var path = "user://editor_capture.png"
			img.save_png(path)
			
			return JSON.stringify({
				"status": "captured",
				"path": ProjectSettings.globalize_path(path),
				"tip": "AI can read this file directly from local disk."
			})

		"reparent_node":
			var node_path = data.get("path", "")
			var new_parent_path = data.get("new_parent", "")
			var node = root.get_node_or_null(node_path) if root else null
			var new_parent = root.get_node_or_null(new_parent_path) if root else null
			
			if node and new_parent:
				node.get_parent().remove_child(node)
				new_parent.add_child(node)
				node.owner = root
				return JSON.stringify({"status": "reparented"})
			return JSON.stringify({"error": "Node or parent not found"})

		"attach_script":
			var node_path = data.get("path", "")
			var script_path = data.get("script", "")
			var node = root.get_node_or_null(node_path) if root else null
			
			if node:
				var script = load(script_path)
				if script and script is Script:
					node.set_script(script)
					return JSON.stringify({"status": "script_attached"})
				return JSON.stringify({"error": "Failed to load script"})
			return JSON.stringify({"error": "Node not found"})

		"validate_script":
			var script_content = data.get("content", "")
			var temp_script = GDScript.new()
			temp_script.source_code = script_content
			var err = temp_script.reload() # 触发 Godot 内部编译校验
			
			if err == OK:
				return JSON.stringify({"is_valid": true})
			else:
				return JSON.stringify({
					"is_valid": false, 
					"error_code": err,
					"msg": "Godot compiler detected syntax errors."
				})

		"get_ui_layout":
			if not root: return JSON.stringify({"error": "No scene open"})
			return JSON.stringify({"layout": _collect_ui_rects(root)})

		"get_globals":
			var globals = {}
			for i in range(get_tree().root.get_child_count()):
				var child = get_tree().root.get_child(i)
				if child.name != "EditorNode" and not child.name.contains("Bridge"):
					globals[child.name] = _get_node_props(child)
			return JSON.stringify(globals)

		"get_errors":
			return JSON.stringify({"errors": error_buffer})

		"clear_errors":
			error_buffer.clear()
			return JSON.stringify({"status": "cleared"})

		"run_test":
			var test_suite = data.get("test_suite", "all")
			var test_file = data.get("test_file", "")
			
			# 如果 GUT 已经作为单例或节点存在，尝试直接调用
			# 这里实现一个简单的逻辑：如果项目里有 GUT，尝试执行它
			var gut = get_tree().root.find_child("Gut", true, false)
			if gut:
				# 如果找到了 GUT 节点，触发它的测试
				if test_file:
					gut.add_script(test_file)
				gut.test_scripts()
				return JSON.stringify({"status": "triggered", "msg": "GUT test triggered in editor."})
			
			# 如果没有找到活动节点，则返回错误，让 Python 端回退到 CLI 模式
			return JSON.stringify({"error": "GUT node not found in scene tree. Please ensure GUT is running or use CLI mode."})

		"audit_ui_containers":
			if not root: return JSON.stringify({"error": "No scene open"})
			return JSON.stringify({"issues": _audit_ui_usage(root)})

		_:
			return JSON.stringify({"error": "Unknown command: " + cmd})

# --- Helper Functions ---

func _audit_ui_usage(node: Node) -> Array:
	var issues = []
	if node is Control:
		var c = node as Control
		var parent = c.get_parent()
		if parent and not parent is Container and not parent is Viewport and not c is Container:
			issues.append({
				"node": c.name,
				"path": str(root_path(c)),
				"issue": "Absolute Positioning",
				"fix_hint": "Wrap in a Container."
			})
		if c.anchors_preset == -1 and c.layout_direction == Control.LAYOUT_DIRECTION_INHERITED:
			issues.append({
				"node": c.name,
				"path": str(root_path(c)),
				"issue": "Unset Anchors",
				"fix_hint": "Use anchors_preset."
			})
	for child in node.get_children():
		issues.append_array(_audit_ui_usage(child))
	return issues

func _collect_ui_rects(node: Node) -> Array:
	var rects = []
	if node is Control:
		var c = node as Control
		if c.is_visible_in_tree():
			rects.append({
				"name": c.name,
				"path": str(root_path(c)),
				"rect": [c.global_position.x, c.global_position.y, c.size.x, c.size.y]
			})
	for child in node.get_children():
		rects.append_array(_collect_ui_rects(child))
	return rects

func root_path(node: Node) -> String:
	var root = get_editor_interface().get_edited_scene_root()
	if not root: return str(node.get_path())
	return str(root.get_path_to(node))

func _serialize_node(node: Node) -> Dictionary:
	var dict = {"name": node.name, "class": node.get_class(), "children": []}
	for child in node.get_children():
		dict["children"].append(_serialize_node(child))
	return dict

func _get_node_props(node: Node) -> Dictionary:
	var props = {}
	for p in node.get_property_list():
		if p["usage"] & PROPERTY_USAGE_EDITOR:
			var val = node.get(p["name"])
			if typeof(val) in [TYPE_VECTOR2, TYPE_VECTOR3, TYPE_COLOR, TYPE_STRING, TYPE_INT, TYPE_FLOAT, TYPE_BOOL]:
				props[p["name"]] = val
	return props

func _flash_node(node: Node) -> void:
	if not node is CanvasItem: return
	var ci = node as CanvasItem
	var original_modulate = ci.modulate
	var tween = create_tween()
	for i in range(3):
		tween.tween_property(ci, "modulate", Color.RED, 0.1)
		tween.tween_property(ci, "modulate", original_modulate, 0.1)
