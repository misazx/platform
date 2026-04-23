extends Node

func _ready() -> void:
	print("========================================")
	print("=== TEST NODE READY - FROM GDSCRIPT ===")
	print("========================================")
	
	# 测试调用 C# 方法
	var main_node = get_node_or_null("/root/Main")
	if main_node != null:
		print("[TestNode] Found /root/Main node!")
		print("[TestNode] Type: ", main_node.get_class())
		if main_node.has_method("GoToLobby"):
			print("[TestNode] Main has GoToLobby method")
		else:
			print("[TestNode] Main does NOT have GoToLobby method")
	else:
		print("[TestNode] /root/Main NOT found!")
	
	# 测试 autoload 节点
	var room_mgr = get_node_or_null("/root/RoomManager")
	if room_mgr != null:
		print("[TestNode] RoomManager exists: ", room_mgr.get_class())
	else:
		print("[TestNode] RoomManager does NOT exist")
		
	var package_svc = get_node_or_null("/root/PackageService")
	if package_svc != null:
		print("[TestNode] PackageService exists: ", package_svc.get_class())
	else:
		print("[TestNode] PackageService does NOT exist")
	
	print("========================================")
