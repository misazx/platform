class_name ObjectPoolManager extends Node

var _pools: Dictionary = {}

func _ready() -> void:
	pass

func create_pool(pool_name: String, scene: PackedScene, initial_size: int = 5) -> void:
	if _pools.has(pool_name):
		return
	var pool := {
		"scene": scene,
		"available": [],
		"in_use": []
	}
	for i in range(initial_size):
		var instance = scene.instantiate()
		instance.visible = false
		instance.process_mode = Node.PROCESS_MODE_DISABLED
		pool["available"].append(instance)
	_pools[pool_name] = pool
	GD.print("[ObjectPoolManager] Created pool '%s' with %d objects" % [pool_name, initial_size])

func get_object(pool_name: String) -> Node:
	if not _pools.has(pool_name):
		GD.printerr("[ObjectPoolManager] Pool not found: %s" % pool_name)
		return null
	var pool := _pools[pool_name]
	var obj: Node
	if pool["available"].size() > 0:
		obj = pool["available"].pop_back()
	else:
		obj = pool["scene"].instantiate()
	obj.visible = true
	obj.process_mode = Node.PROCESS_MODE_INHERIT
	pool["in_use"].append(obj)
	return obj

func return_object(pool_name: String, obj: Node) -> void:
	if not _pools.has(pool_name):
		return
	var pool := _pools[pool_name]
	var idx := pool["in_use"].find(obj)
	if idx >= 0:
		pool["in_use"].remove_at(idx)
	obj.visible = false
	obj.process_mode = Node.PROCESS_MODE_DISABLED
	pool["available"].append(obj)

func clear_pool(pool_name: String) -> void:
	if not _pools.has(pool_name):
		return
	var pool := _pools[pool_name]
	for obj in pool["available"]:
		obj.queue_free()
	for obj in pool["in_use"]:
		obj.queue_free()
	_pools.erase(pool_name)

func get_pool_stats(pool_name: String) -> Dictionary:
	if not _pools.has(pool_name):
		return {"available": 0, "in_use": 0}
	var pool := _pools[pool_name]
	return {
		"available": pool["available"].size(),
		"in_use": pool["in_use"].size(),
		"total": pool["available"].size() + pool["in_use"].size()
	}

func warmup_pool(pool_name: String, count: int) -> void:
	if not _pools.has(pool_name):
		return
	var pool := _pools[pool_name]
	while pool["available"].size() < count:
		var obj = pool["scene"].instantiate()
		obj.visible = false
		obj.process_mode = Node.PROCESS_MODE_DISABLED
		pool["available"].append(obj)
	GD.print("[ObjectPoolManager] Warmed up pool '%s' to %d objects" % [pool_name, count])
