extends Node

signal multiplayer_seed_received(seed_value: int)

var _multiplayer_seed: int = -1
var _is_multiplayer: bool = false

static var instance: Node = null

func _ready() -> void:
	if instance != null and instance != self:
		queue_free()
		return
	instance = self

	var session_manager = get_node_or_null("/root/GameSessionManager")
	if session_manager != null:
		if not session_manager.MultiplayerSeedReceived.is_connected(_on_seed_received):
			session_manager.MultiplayerSeedReceived.connect(_on_seed_received)

func _on_seed_received(seed_value: int) -> void:
	_multiplayer_seed = seed_value
	_is_multiplayer = true
	multiplayer_seed_received.emit(seed_value)
	print("[MultiplayerSeedBridge] 多人游戏种子已接收: %d" % seed_value)

func get_multiplayer_seed() -> int:
	return _multiplayer_seed

func is_multiplayer_game() -> bool:
	return _is_multiplayer

func get_effective_seed(fallback_seed: int = 0) -> int:
	if _is_multiplayer and _multiplayer_seed >= 0:
		return _multiplayer_seed
	if fallback_seed != 0:
		return fallback_seed
	return randi()

func reset() -> void:
	_multiplayer_seed = -1
	_is_multiplayer = false
