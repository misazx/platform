class_name RaceModeManager extends Node

const GHOST_OPACITY: float = 0.35
const SYNC_RATE_HZ: float = 3.0
const SYNC_INTERVAL: float = 1.0 / SYNC_RATE_HZ
const CHECKPOINT_BOOST_DURATION: float = 2.0
const CHECKPOINT_BOOST_SPEED: float = 150.0
const MAX_RACERS: int = 4
const FINISH_Y_OFFSET: float = -50.0

signal racer_position_updated(racer_id: String, position: Vector2, form: String)
signal racer_finished(racer_id: String, finish_time: float, placement: int)
signal racer_checkpoint(racer_id: String, checkpoint_id: String)
signal race_started()
signal race_finished(results: Array)
signal ghost_created(racer_id: String, ghost_node: Node2D)

var _racers: Dictionary = {}
var _local_racer_id: String = ""
var _ghost_nodes: Dictionary = {}
var _finish_order: Array = []
var _race_active: bool = false
var _race_start_time: float = 0.0
var _sync_timer: float = 0.0
var _checkpoints_reached: Dictionary = {}

func _ready() -> void:
	pass

func _process(delta: float) -> void:
	if not _race_active: return
	_sync_timer += delta
	if _sync_timer >= SYNC_INTERVAL:
		_sync_timer = 0.0
		_broadcast_local_position()

func initialize_race(racer_ids: Array, local_id: String) -> void:
	_racers.clear()
	_ghost_nodes.clear()
	_finish_order.clear()
	_checkpoints_reached.clear()
	_local_racer_id = local_id
	_race_active = false

	for rid in racer_ids:
		_racers[rid] = {
			"id": rid,
			"name": "玩家" + str(racer_ids.find(rid) + 1),
			"position": Vector2.ZERO,
			"form": "light",
			"finished": false,
			"finish_time": 0.0,
			"placement": 0,
			"checkpoints": [],
		}
		_checkpoints_reached[rid] = []
		if rid != _local_racer_id:
			_create_ghost(rid)

func start_race() -> void:
	_race_active = true
	_race_start_time = Time.get_ticks_msec() / 1000.0
	race_started.emit()

func _create_ghost(racer_id: String) -> void:
	var ghost := Node2D.new()
	ghost.name = "Ghost_%s" % racer_id

	var sprite := Sprite2D.new()
	sprite.name = "GhostSprite"
	sprite.modulate = Color(1.0, 1.0, 1.0, GHOST_OPACITY)
	ghost.add_child(sprite)

	var label := Label.new()
	label.name = "GhostLabel"
	label.text = _racers[racer_id].name
	label.position = Vector2(0, -30)
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.add_theme_font_size_override("font_size", 10)
	ghost.add_child(label)

	add_child(ghost)
	_ghost_nodes[racer_id] = ghost
	ghost_created.emit(racer_id, ghost)

func update_remote_racer(racer_id: String, position: Vector2, form: String) -> void:
	if not _racers.has(racer_id): return
	_racers[racer_id].position = position
	_racers[racer_id].form = form

	if _ghost_nodes.has(racer_id):
		var ghost: Node2D = _ghost_nodes[racer_id]
		ghost.position = ghost.position.lerp(position, 0.3)

		var sprite: Node2D = ghost.get_node_or_null("GhostSprite")
		if sprite != null:
			if form == "shadow":
				sprite.modulate = Color(0.5, 0.4, 0.9, GHOST_OPACITY)
			else:
				sprite.modulate = Color(1.0, 0.95, 0.6, GHOST_OPACITY)

	racer_position_updated.emit(racer_id, position, form)

func _broadcast_local_position() -> void:
	if not _racers.has(_local_racer_id): return
	var local_racer: Dictionary = _racers[_local_racer_id]
	racer_position_updated.emit(_local_racer_id, local_racer.position, local_racer.form)

func update_local_position(position: Vector2, form: String) -> void:
	if not _racers.has(_local_racer_id): return
	_racers[_local_racer_id].position = position
	_racers[_local_racer_id].form = form

func on_local_checkpoint(checkpoint_id: String) -> void:
	if not _racers.has(_local_racer_id): return
	if _checkpoints_reached[_local_racer_id].has(checkpoint_id): return

	_checkpoints_reached[_local_racer_id].append(checkpoint_id)
	racer_checkpoint.emit(_local_racer_id, checkpoint_id)

func on_remote_checkpoint(racer_id: String, checkpoint_id: String) -> void:
	if not _checkpoints_reached.has(racer_id): return
	if _checkpoints_reached[racer_id].has(checkpoint_id): return
	_checkpoints_reached[racer_id].append(checkpoint_id)
	racer_checkpoint.emit(racer_id, checkpoint_id)

func on_local_finish() -> void:
	if not _racers.has(_local_racer_id): return
	if _racers[_local_racer_id].finished: return

	var finish_time: float = Time.get_ticks_msec() / 1000.0 - _race_start_time
	var placement: int = _finish_order.size() + 1

	_racers[_local_racer_id].finished = true
	_racers[_local_racer_id].finish_time = finish_time
	_racers[_local_racer_id].placement = placement
	_finish_order.append(_local_racer_id)

	racer_finished.emit(_local_racer_id, finish_time, placement)

	if _finish_order.size() >= _racers.size():
		_end_race()
	elif placement == 1:
		pass

func on_remote_finish(racer_id: String, finish_time: float) -> void:
	if not _racers.has(racer_id): return
	if _racers[racer_id].finished: return

	var placement: int = _finish_order.size() + 1
	_racers[racer_id].finished = true
	_racers[racer_id].finish_time = finish_time
	_racers[racer_id].placement = placement
	_finish_order.append(racer_id)

	racer_finished.emit(racer_id, finish_time, placement)

	if _finish_order.size() >= _racers.size():
		_end_race()

func _end_race() -> void:
	_race_active = false
	var results: Array = []
	for rid in _finish_order:
		results.append({
			"id": rid,
			"name": _racers[rid].name,
			"finish_time": _racers[rid].finish_time,
			"placement": _racers[rid].placement,
			"checkpoints": _racers[rid].checkpoints.size(),
		})
	race_finished.emit(results)

func get_race_state() -> Dictionary:
	var racers_state: Array = []
	for rid in _racers:
		var r: Dictionary = _racers[rid]
		racers_state.append({
			"id": r.id,
			"position": r.position,
			"form": r.form,
			"finished": r.finished,
			"finish_time": r.finish_time,
			"placement": r.placement,
		})
	return {
		"active": _race_active,
		"elapsed": Time.get_ticks_msec() / 1000.0 - _race_start_time if _race_active else 0.0,
		"racers": racers_state,
		"finish_order": _finish_order,
	}

func cleanup() -> void:
	_race_active = false
	for ghost in _ghost_nodes.values():
		if is_instance_valid(ghost):
			ghost.queue_free()
	_ghost_nodes.clear()
	_racers.clear()
	_finish_order.clear()
