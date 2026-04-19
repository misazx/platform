class_name MultiplayerBridge extends Node

signal bridge_coop_card_played(player_index: int, card_data: String, target_index: int)
signal bridge_coop_turn_ended(player_index: int)
signal bridge_coop_position_updated(user_id: String, x: float, y: float, form: String)
signal bridge_coop_switch_updated(user_id: String, switch_id: String, activated: bool)
signal bridge_coop_puzzle_solved(puzzle_id: String)
signal bridge_coop_player_died(user_id: String)
signal bridge_coop_player_revived()
signal bridge_race_position_updated(racer_id: String, x: float, y: float, form: String)
signal bridge_race_checkpoint_reached(racer_id: String, checkpoint_id: String)
signal bridge_race_finished(racer_id: String, finish_time: float)
signal bridge_game_started(sync_seed: int)
signal bridge_game_ended(victory: bool)
signal bridge_state_synced(room_id: String, current_turn: int)
signal bridge_turn_changed(new_turn: int)
signal bridge_player_action_received(player_id: String, action_type: String)
signal bridge_player_joined_room(player_id: String, player_name: String)
signal bridge_player_left_room(player_id: String, player_name: String)
signal bridge_player_ready_changed(player_id: String, is_ready: bool)
signal bridge_room_state_updated(state_json: String)

static var instance: MultiplayerBridge = null

var _is_connected: bool = false

func _ready() -> void:
	if instance != null and instance != self:
		queue_free()
		return
	instance = self
	process_mode = Node.PROCESS_MODE_ALWAYS
	_connect_csharp_signals()

func _connect_csharp_signals() -> void:
	var hub_client = get_node_or_null("/root/GameHubClient")
	if hub_client != null:
		_connect_hub_client(hub_client)
	else:
		print("[MultiplayerBridge] GameHubClient not found, will retry on next frame")
		call_deferred("_retry_connect_hub")

	var session_manager = get_node_or_null("/root/GameSessionManager")
	if session_manager != null:
		_connect_session_manager(session_manager)
	else:
		print("[MultiplayerBridge] GameSessionManager not found, will retry on next frame")
		call_deferred("_retry_connect_session")

func _retry_connect_hub() -> void:
	var hub_client = get_node_or_null("/root/GameHubClient")
	if hub_client != null:
		_connect_hub_client(hub_client)
		print("[MultiplayerBridge] GameHubClient connected on retry")
	else:
		get_tree().create_timer(2.0).timeout.connect(_retry_connect_hub)

func _retry_connect_session() -> void:
	var session_manager = get_node_or_null("/root/GameSessionManager")
	if session_manager != null:
		_connect_session_manager(session_manager)
		print("[MultiplayerBridge] GameSessionManager connected on retry")
	else:
		get_tree().create_timer(2.0).timeout.connect(_retry_connect_session)

func _connect_hub_client(hub_client: Node) -> void:
	if _is_connected:
		return

	if hub_client.has_signal("OnCoopCardPlayed"):
		hub_client.OnCoopCardPlayed.connect(_on_coop_card_played)
	if hub_client.has_signal("OnCoopTurnEnded"):
		hub_client.OnCoopTurnEnded.connect(_on_coop_turn_ended)
	if hub_client.has_signal("OnCoopPositionUpdate"):
		hub_client.OnCoopPositionUpdate.connect(_on_coop_position_update)
	if hub_client.has_signal("OnCoopSwitchUpdate"):
		hub_client.OnCoopSwitchUpdate.connect(_on_coop_switch_update)
	if hub_client.has_signal("OnCoopPuzzleSolved"):
		hub_client.OnCoopPuzzleSolved.connect(_on_coop_puzzle_solved)
	if hub_client.has_signal("OnCoopPlayerDied"):
		hub_client.OnCoopPlayerDied.connect(_on_coop_player_died)
	if hub_client.has_signal("OnCoopPlayerRevived"):
		hub_client.OnCoopPlayerRevived.connect(_on_coop_player_revived)
	if hub_client.has_signal("OnRacePositionUpdate"):
		hub_client.OnRacePositionUpdate.connect(_on_race_position_update)
	if hub_client.has_signal("OnRaceCheckpointReached"):
		hub_client.OnRaceCheckpointReached.connect(_on_race_checkpoint_reached)
	if hub_client.has_signal("OnRaceFinished"):
		hub_client.OnRaceFinished.connect(_on_race_finished)
	if hub_client.has_signal("OnPlayerJoinedRoom"):
		hub_client.OnPlayerJoinedRoom.connect(_on_player_joined_room)
	if hub_client.has_signal("OnPlayerLeftRoom"):
		hub_client.OnPlayerLeftRoom.connect(_on_player_left_room)
	if hub_client.has_signal("OnPlayerReadyChanged"):
		hub_client.OnPlayerReadyChanged.connect(_on_player_ready_changed)
	if hub_client.has_signal("OnRoomStateUpdate"):
		hub_client.OnRoomStateUpdate.connect(_on_room_state_update)

	_is_connected = true
	print("[MultiplayerBridge] Hub client signals connected")

func _connect_session_manager(session_manager: Node) -> void:
	if session_manager.has_signal("GameStarted"):
		session_manager.GameStarted.connect(_on_game_started)
	if session_manager.has_signal("GameEnded"):
		session_manager.GameEnded.connect(_on_game_ended)
	if session_manager.has_signal("StateSynced"):
		session_manager.StateSynced.connect(_on_state_synced)
	if session_manager.has_signal("TurnChanged"):
		session_manager.TurnChanged.connect(_on_turn_changed)
	if session_manager.has_signal("PlayerActionReceived"):
		session_manager.PlayerActionReceived.connect(_on_player_action_received)

	print("[MultiplayerBridge] Session manager signals connected")

func _on_coop_card_played(player_index: int, card_data: String, target_index: int) -> void:
	bridge_coop_card_played.emit(player_index, card_data, target_index)

func _on_coop_turn_ended(player_index: int) -> void:
	bridge_coop_turn_ended.emit(player_index)

func _on_coop_position_update(user_id: String, x: float, y: float, form: String) -> void:
	bridge_coop_position_updated.emit(user_id, x, y, form)

func _on_coop_switch_update(user_id: String, switch_id: String, activated: bool) -> void:
	bridge_coop_switch_updated.emit(user_id, switch_id, activated)

func _on_coop_puzzle_solved(puzzle_id: String) -> void:
	bridge_coop_puzzle_solved.emit(puzzle_id)

func _on_coop_player_died(user_id: String) -> void:
	bridge_coop_player_died.emit(user_id)

func _on_coop_player_revived() -> void:
	bridge_coop_player_revived.emit()

func _on_race_position_update(racer_id: String, x: float, y: float, form: String) -> void:
	bridge_race_position_updated.emit(racer_id, x, y, form)

func _on_race_checkpoint_reached(racer_id: String, checkpoint_id: String) -> void:
	bridge_race_checkpoint_reached.emit(racer_id, checkpoint_id)

func _on_race_finished(racer_id: String, finish_time: float) -> void:
	bridge_race_finished.emit(racer_id, finish_time)

func _on_game_started(sync_seed: int) -> void:
	bridge_game_started.emit(sync_seed)
	print("[MultiplayerBridge] Game started with seed: %d - transitioning to map" % sync_seed)
	var main_node := get_tree().root.get_node_or_null("/root/Main") as Node
	if main_node != null and main_node.has_method("GoToMap"):
		main_node.call("GoToMap")

func _on_game_ended(victory: bool) -> void:
	bridge_game_ended.emit(victory)

func _on_state_synced(room_id: String, current_turn: int) -> void:
	bridge_state_synced.emit(room_id, current_turn)

func _on_turn_changed(new_turn: int) -> void:
	bridge_turn_changed.emit(new_turn)

func _on_player_action_received(player_id: String, action_type: String) -> void:
	bridge_player_action_received.emit(player_id, action_type)

func _on_player_joined_room(player_id: String, player_name: String) -> void:
	bridge_player_joined_room.emit(player_id, player_name)

func _on_player_left_room(player_id: String, player_name: String) -> void:
	bridge_player_left_room.emit(player_id, player_name)

func _on_player_ready_changed(player_id: String, is_ready: bool) -> void:
	bridge_player_ready_changed.emit(player_id, is_ready)

func _on_room_state_update(state_json: String) -> void:
	bridge_room_state_updated.emit(state_json)

func send_coop_card_play(player_index: int, card_data: Dictionary, target_index: int) -> void:
	var hub_client = get_node_or_null("/root/GameHubClient")
	if hub_client == null:
		push_error("[MultiplayerBridge] GameHubClient not found!")
		return
	var room_id: String = hub_client.call("get_current_room_id") if hub_client.has_method("get_current_room_id") else ""
	if room_id == "":
		push_warning("[MultiplayerBridge] No room ID, cannot send card play")
		return
	var card_json: String = JSON.stringify(card_data)
	hub_client.call("send_coop_card_play_sync", room_id, player_index, card_json, target_index)
	print("[MultiplayerBridge] Sent coop card play: player=%d card=%s target=%d" % [player_index, card_data.get("name", "?"), target_index])

func send_coop_turn_end(player_index: int) -> void:
	var hub_client = get_node_or_null("/root/GameHubClient")
	if hub_client == null:
		push_error("[MultiplayerBridge] GameHubClient not found!")
		return
	var room_id: String = hub_client.call("get_current_room_id") if hub_client.has_method("get_current_room_id") else ""
	if room_id == "":
		push_warning("[MultiplayerBridge] No room ID, cannot send turn end")
		return
	hub_client.call("send_coop_turn_end_sync", room_id, player_index)
	print("[MultiplayerBridge] Sent coop turn end: player=%d" % player_index)

func send_coop_position(x: float, y: float, form: String) -> void:
	var hub_client = get_node_or_null("/root/GameHubClient")
	if hub_client == null:
		return
	var room_id: String = hub_client.call("get_current_room_id") if hub_client.has_method("get_current_room_id") else ""
	if room_id == "":
		return
	hub_client.call("send_coop_position_async", room_id, x, y, form)

func send_coop_switch(switch_id: String, activated: bool) -> void:
	var hub_client = get_node_or_null("/root/GameHubClient")
	if hub_client == null:
		return
	var room_id: String = hub_client.call("get_current_room_id") if hub_client.has_method("get_current_room_id") else ""
	if room_id == "":
		return
	hub_client.call("send_coop_switch_async", room_id, switch_id, activated)

func send_coop_puzzle_solved(puzzle_id: String) -> void:
	var hub_client = get_node_or_null("/root/GameHubClient")
	if hub_client == null:
		return
	var room_id: String = hub_client.call("get_current_room_id") if hub_client.has_method("get_current_room_id") else ""
	if room_id == "":
		return
	hub_client.call("send_coop_puzzle_solved_async", room_id, puzzle_id)

func send_coop_player_died() -> void:
	var hub_client = get_node_or_null("/root/GameHubClient")
	if hub_client == null:
		return
	var room_id: String = hub_client.call("get_current_room_id") if hub_client.has_method("get_current_room_id") else ""
	if room_id == "":
		return
	hub_client.call("send_coop_player_died_async", room_id)

func send_coop_player_revived() -> void:
	var hub_client = get_node_or_null("/root/GameHubClient")
	if hub_client == null:
		return
	var room_id: String = hub_client.call("get_current_room_id") if hub_client.has_method("get_current_room_id") else ""
	if room_id == "":
		return
	hub_client.call("send_coop_player_revived_async", room_id)

func send_race_position(racer_id: String, x: float, y: float, form: String) -> void:
	var hub_client = get_node_or_null("/root/GameHubClient")
	if hub_client == null:
		return
	var room_id: String = hub_client.call("get_current_room_id") if hub_client.has_method("get_current_room_id") else ""
	if room_id == "":
		return
	hub_client.call("send_race_position_async", room_id, racer_id, x, y, form)

func send_race_checkpoint(racer_id: String, checkpoint_id: String) -> void:
	var hub_client = get_node_or_null("/root/GameHubClient")
	if hub_client == null:
		return
	var room_id: String = hub_client.call("get_current_room_id") if hub_client.has_method("get_current_room_id") else ""
	if room_id == "":
		return
	hub_client.call("send_race_checkpoint_async", room_id, racer_id, checkpoint_id)

func send_race_finish(racer_id: String, finish_time: float) -> void:
	var hub_client = get_node_or_null("/root/GameHubClient")
	if hub_client == null:
		return
	var room_id: String = hub_client.call("get_current_room_id") if hub_client.has_method("get_current_room_id") else ""
	if room_id == "":
		return
	hub_client.call("send_race_finish_async", room_id, racer_id, finish_time)

func is_multiplayer_game() -> bool:
	var bridge = get_node_or_null("/root/MultiplayerSeedBridge")
	if bridge != null:
		return bridge.is_multiplayer_game()
	return false

func get_local_player_index() -> int:
	var session_manager = get_node_or_null("/root/GameSessionManager")
	if session_manager != null and session_manager.has_method("get_local_player_index"):
		return int(session_manager.call("get_local_player_index"))
	return 0

func get_player_count() -> int:
	var session_manager = get_node_or_null("/root/GameSessionManager")
	if session_manager != null and session_manager.has_method("get_player_count"):
		return int(session_manager.call("get_player_count"))
	return 1

func get_sync_seed() -> int:
	var bridge = get_node_or_null("/root/MultiplayerSeedBridge")
	if bridge != null:
		return bridge.get_effective_seed(0)
	return 0
