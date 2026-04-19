class_name CoopModeManager extends Node

const MAX_COOP_PLAYERS: int = 2
const SYNC_RATE_HZ: float = 5.0
const SYNC_INTERVAL: float = 1.0 / SYNC_RATE_HZ
const SWITCH_COOLDOWN: float = 0.5
const REVIVE_TIME: float = 3.0
const SHARED_HEAL_AMOUNT: int = 1

enum CoopRole { LIGHT_PLAYER, SHADOW_PLAYER }

signal coop_partner_position_updated(position: Vector2, form: String)
signal coop_switch_activated(switch_id: String, activated_by: String)
signal coop_partner_died()
signal coop_partner_revived()
signal coop_puzzle_solved(puzzle_id: String)
signal coop_state_synced(state: Dictionary)
signal coop_level_completed(level_id: String)

var _local_role: int = CoopRole.LIGHT_PLAYER
var _partner_role: int = CoopRole.SHADOW_PLAYER
var _local_position: Vector2 = Vector2.ZERO
var _local_form: String = "light"
var _partner_position: Vector2 = Vector2.ZERO
var _partner_form: String = "shadow"
var _partner_node: Node2D = null
var _partner_alive: bool = true
var _local_alive: bool = true
var _switch_states: Dictionary = {}
var _puzzle_states: Dictionary = {}
var _sync_timer: float = 0.0
var _is_active: bool = false
var _revive_timer: float = 0.0
var _is_reviving: bool = false

func _ready() -> void:
	set_process(false)

func _process(delta: float) -> void:
	if not _is_active: return

	_sync_timer += delta
	if _sync_timer >= SYNC_INTERVAL:
		_sync_timer = 0.0
		coop_state_synced.emit(get_sync_state())

	if _is_reviving and not _partner_alive:
		_revive_timer += delta
		if _revive_timer >= REVIVE_TIME:
			_is_reviving = false
			_revive_timer = 0.0
			coop_partner_revived.emit()

func initialize_coop(local_role: int) -> void:
	_local_role = local_role
	_partner_role = CoopRole.SHADOW_PLAYER if local_role == CoopRole.LIGHT_PLAYER else CoopRole.LIGHT_PLAYER
	_local_form = "light" if _local_role == CoopRole.LIGHT_PLAYER else "shadow"
	_partner_form = "shadow" if _local_role == CoopRole.LIGHT_PLAYER else "light"
	_local_position = Vector2.ZERO
	_partner_position = Vector2.ZERO
	_partner_alive = true
	_local_alive = true
	_switch_states.clear()
	_puzzle_states.clear()
	_is_active = true
	_is_reviving = false
	_revive_timer = 0.0

	_create_partner_avatar()

func _create_partner_avatar() -> void:
	if _partner_node != null and is_instance_valid(_partner_node):
		_partner_node.queue_free()

	_partner_node = Node2D.new()
	_partner_node.name = "CoopPartner"

	var sprite := Sprite2D.new()
	sprite.name = "PartnerSprite"
	if _partner_form == "shadow":
		sprite.modulate = Color(0.5, 0.4, 0.9, 0.7)
	else:
		sprite.modulate = Color(1.0, 0.95, 0.6, 0.7)
	_partner_node.add_child(sprite)

	var label := Label.new()
	label.name = "PartnerLabel"
	label.text = "搭档"
	label.position = Vector2(0, -30)
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.add_theme_font_size_override("font_size", 10)
	_partner_node.add_child(label)

	add_child(_partner_node)

func update_local_state(position: Vector2, form: String) -> void:
	_local_position = position
	_local_form = form

func update_partner_state(position: Vector2, form: String) -> void:
	_partner_position = position
	_partner_form = form

	if _partner_node != null and is_instance_valid(_partner_node):
		_partner_node.position = _partner_node.position.lerp(position, 0.4)

		var sprite: Node2D = _partner_node.get_node_or_null("PartnerSprite")
		if sprite != null:
			if form == "shadow":
				sprite.modulate = Color(0.5, 0.4, 0.9, 0.7)
			else:
				sprite.modulate = Color(1.0, 0.95, 0.6, 0.7)

	coop_partner_position_updated.emit(position, form)

func on_local_switch_activated(switch_id: String) -> void:
	if _switch_states.has(switch_id) and _switch_states[switch_id]:
		return
	_switch_states[switch_id] = true
	coop_switch_activated.emit(switch_id, "local")

func on_remote_switch_activated(switch_id: String) -> void:
	if _switch_states.has(switch_id) and _switch_states[switch_id]:
		return
	_switch_states[switch_id] = true
	coop_switch_activated.emit(switch_id, "remote")

func on_switch_deactivated(switch_id: String) -> void:
	_switch_states[switch_id] = false

func on_local_puzzle_solved(puzzle_id: String) -> void:
	if _puzzle_states.has(puzzle_id): return
	_puzzle_states[puzzle_id] = true
	coop_puzzle_solved.emit(puzzle_id)

func on_remote_puzzle_solved(puzzle_id: String) -> void:
	if _puzzle_states.has(puzzle_id): return
	_puzzle_states[puzzle_id] = true
	coop_puzzle_solved.emit(puzzle_id)

func on_partner_died() -> void:
	_partner_alive = false
	_is_reviving = true
	_revive_timer = 0.0
	coop_partner_died.emit()

func on_local_died() -> void:
	_local_alive = false
	coop_partner_died.emit()

func on_local_near_partner() -> void:
	if not _partner_alive and _is_reviving:
		_revive_timer += SYNC_INTERVAL
		if _revive_timer >= REVIVE_TIME:
			_partner_alive = true
			_is_reviving = false
			_revive_timer = 0.0
			coop_partner_revived.emit()

func on_level_completed(level_id: String) -> void:
	coop_level_completed.emit(level_id)

func get_local_role() -> int:
	return _local_role

func get_partner_role() -> int:
	return _partner_role

func is_local_light_player() -> bool:
	return _local_role == CoopRole.LIGHT_PLAYER

func is_local_shadow_player() -> bool:
	return _local_role == CoopRole.SHADOW_PLAYER

func is_partner_alive() -> bool:
	return _partner_alive

func get_switch_state(switch_id: String) -> bool:
	return _switch_states.get(switch_id, false)

func get_sync_state() -> Dictionary:
	return {
		"local_position": _local_position,
		"local_form": _local_form,
		"local_alive": _local_alive,
		"partner_position": _partner_position,
		"partner_form": _partner_form,
		"partner_alive": _partner_alive,
		"switch_states": _switch_states.duplicate(),
		"puzzle_states": _puzzle_states.duplicate(),
	}

func apply_remote_state(state: Dictionary) -> void:
	if state.has("local_position"):
		update_partner_state(state.local_position, state.get("local_form", _partner_form))
	if state.has("partner_alive"):
		_partner_alive = state.partner_alive
	if state.has("switch_states"):
		for switch_id: String in state.switch_states:
			if not _switch_states.get(switch_id, false) and state.switch_states[switch_id]:
				coop_switch_activated.emit(switch_id, "remote")
		_switch_states = state.switch_states.duplicate()
	if state.has("puzzle_states"):
		for puzzle_id: String in state.puzzle_states:
			if not _puzzle_states.get(puzzle_id, false) and state.puzzle_states[puzzle_id]:
				coop_puzzle_solved.emit(puzzle_id)
		_puzzle_states = state.puzzle_states.duplicate()

func cleanup() -> void:
	_is_active = false
	if _partner_node != null and is_instance_valid(_partner_node):
		_partner_node.queue_free()
	_partner_node = null
	_switch_states.clear()
	_puzzle_states.clear()
