class_name BotController
extends Node

signal bot_action_received(action_data: Dictionary)

enum BotMode { SOLO, RACE, COOP }

var _bot_user_id: String = ""
var _bot_name: String = "Bot"
var _player_index: int = 0
var _is_active: bool = false
var _bot_mode: BotMode = BotMode.SOLO

var _move_direction: int = 0
var _should_jump: bool = false
var _should_switch_form: bool = false
var _should_dash: bool = false
var _target_form: String = "light"
var _current_action: String = ""

var _bot_character: PlayerCharacter = null
var _last_action_time: int = 0

func _ready() -> void:
	set_process(false)

func _process(delta: float) -> void:
	if not _is_active:
		return

	_apply_bot_input(delta)

func initialize_bot(bot_user_id: String, bot_name: String, player_index: int, mode: BotMode = BotMode.SOLO) -> void:
	_bot_user_id = bot_user_id
	_bot_name = bot_name
	_player_index = player_index
	_bot_mode = mode
	_is_active = true
	set_process(true)
	GD.Print("[BotController] 机器人初始化: ", bot_name, " (", bot_user_id, ") 模式: ", _get_mode_name(mode))

func set_bot_character(character: PlayerCharacter) -> void:
	_bot_character = character
	GD.Print("[BotController] 机器人角色已设置")

func on_server_action(action_json: String) -> void:
	var json = JSON.new()
	var parse_result = json.parse(action_json)
	if parse_result != OK:
		GD.PrintErr("[BotController] 解析机器人动作失败: ", json.get_error_message())
		return

	var data: Dictionary = json.data as Dictionary
	if data.is_empty():
		return

	var received_bot_user_id: String = data.get("botUserId", "")
	if received_bot_user_id != _bot_user_id and received_bot_user_id != "":
		return

	_move_direction = data.get("moveDirection", 0)
	_should_jump = data.get("shouldJump", false)
	_should_switch_form = data.get("shouldSwitchForm", false)
	_should_dash = data.get("shouldDash", false)
	_target_form = data.get("targetForm", "light")
	_current_action = data.get("action", "")

	bot_action_received.emit(data)

func _apply_bot_input(delta: float) -> void:
	if not is_instance_valid(_bot_character):
		return

	match _bot_mode:
		BotMode.RACE:
			_apply_race_mode_input(delta)
		BotMode.COOP:
			_apply_coop_mode_input(delta)
		_:
			_apply_solo_mode_input(delta)

func _apply_solo_mode_input(delta: float) -> void:
	if _should_switch_form:
		_bot_character._switch_form()
		_should_switch_form = false

	if _should_jump:
		_simulate_jump()
		_should_jump = false

	if _should_dash and _bot_character.is_light_form():
		_simulate_dash()
		_should_dash = false

	_simulate_movement(_move_direction)

func _apply_race_mode_input(delta: float) -> void:
	if _should_switch_form:
		_bot_character._switch_form()
		_should_switch_form = false

	if _should_jump:
		_simulate_jump()
		_should_jump = false

	if _should_dash and _bot_character.is_light_form():
		_simulate_dash()
		_should_dash = false

	_simulate_movement(_move_direction)

func _apply_coop_mode_input(delta: float) -> void:
	if _should_switch_form:
		_bot_character._switch_form()
		_should_switch_form = false

	if _should_jump:
		_simulate_jump()
		_should_jump = false

	if _should_dash and _bot_character.is_light_form():
		_simulate_dash()
		_should_dash = false

	_simulate_movement(_move_direction)

func _simulate_movement(direction: int) -> void:
	if direction == 0:
		return

	var input_dir: float = float(direction)
	var speed: float = _bot_character.light_speed if _bot_character.is_light_form() else _bot_character.shadow_speed

	_bot_character.velocity.x = input_dir * speed

	if input_dir != 0:
		_bot_character.sprite.flip_h = input_dir > 0
		_bot_character.facing_right = input_dir > 0

	_bot_character.move_and_slide()

func _simulate_jump() -> void:
	if not _bot_character.is_on_floor():
		return

	var jump_force: float = _bot_character.light_jump_force if _bot_character.is_light_form() else _bot_character.shadow_jump_force
	_bot_character.velocity.y = jump_force

func _simulate_dash() -> void:
	if _bot_character.form_energy < 25.0:
		return

	_bot_character._start_dash()

func _get_mode_name(mode: BotMode) -> String:
	match mode:
		BotMode.SOLO:
			return "单人"
		BotMode.RACE:
			return "竞速"
		BotMode.COOP:
			return "合作"
		_:
			return "未知"

func get_bot_user_id() -> String:
	return _bot_user_id

func get_bot_name() -> String:
	return _bot_name

func get_bot_mode() -> BotMode:
	return _bot_mode

func is_active() -> bool:
	return _is_active

func cleanup() -> void:
	_is_active = false
	set_process(false)
	_bot_character = null
	_move_direction = 0
	_should_jump = false
	_should_switch_form = false
	_should_dash = false
