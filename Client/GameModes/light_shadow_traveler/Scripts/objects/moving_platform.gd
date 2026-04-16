class_name MovingPlatform
extends AnimatableBody2D

enum MoveType { HORIZONTAL, VERTICAL, CIRCULAR, PATH }

@export var move_type: MoveType = MoveType.HORIZONTAL
@export var move_range := 200.0
@export var move_speed := 80.0
@export var pause_time := 1.0
@export var platform_width := 120.0
@export var platform_height := 20.0
@export var is_light_platform := false
@export var is_shadow_platform := false
@export var platform_id := ""

var start_position: Vector2
var move_timer := 0.0
var pause_timer := 0.0
var move_direction := 1.0
var is_pausing := false
var collision_shape: CollisionShape2D
var visual: ColorRect

func _ready() -> void:
	start_position = global_position
	_setup_platform()

func set_active(active: bool) -> void:
	if collision_shape:
		collision_shape.disabled = not active
	if visual:
		visual.visible = active
		visual.color = Color(0.7, 0.8, 0.7, 0.9) if active else Color(0.3, 0.3, 0.3, 0.3)
	set_physics_process(active)

func _setup_platform() -> void:
	collision_shape = CollisionShape2D.new()
	collision_shape.name = "CollisionShape2D"
	var shape := RectangleShape2D.new()
	shape.size = Vector2(platform_width, platform_height)
	collision_shape.shape = shape
	add_child(collision_shape)
	visual = ColorRect.new()
	visual.name = "Visual"
	visual.size = Vector2(platform_width, platform_height)
	visual.position = Vector2(-platform_width / 2, -platform_height / 2)
	visual.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(visual)
	if is_light_platform:
		visual.color = Color(1.0, 0.95, 0.7, 0.9)
	elif is_shadow_platform:
		visual.color = Color(0.3, 0.35, 0.6, 0.85)
	else:
		visual.color = Color(0.5, 0.5, 0.5, 1.0)

func _physics_process(delta: float) -> void:
	if is_pausing:
		pause_timer -= delta
		if pause_timer <= 0:
			is_pausing = false
		return
	match move_type:
		MoveType.HORIZONTAL:
			position.x += move_speed * move_direction * delta
			if position.x > start_position.x + move_range:
				move_direction = -1.0
				is_pausing = true
				pause_timer = pause_time
			elif position.x < start_position.x - move_range:
				move_direction = 1.0
				is_pausing = true
				pause_timer = pause_time
		MoveType.VERTICAL:
			position.y += move_speed * move_direction * delta
			if position.y > start_position.y + move_range:
				move_direction = -1.0
				is_pausing = true
				pause_timer = pause_time
			elif position.y < start_position.y - move_range:
				move_direction = 1.0
				is_pausing = true
				pause_timer = pause_time
		MoveType.CIRCULAR:
			move_timer += delta * move_speed / move_range
			position.x = start_position.x + cos(move_timer) * move_range
			position.y = start_position.y + sin(move_timer) * move_range * 0.5

func setup_from_data(data: Dictionary) -> void:
	position = Vector2(data.get("x", 0), data.get("y", 0))
	start_position = position
	platform_width = data.get("w", 120)
	platform_height = data.get("h", 20)
	move_speed = data.get("speed", 80)
	move_range = data.get("range", 200)
	pause_time = data.get("pause", 1.0)
	var type_str: String = data.get("moveType", "horizontal")
	match type_str:
		"vertical":
			move_type = MoveType.VERTICAL
		"circular":
			move_type = MoveType.CIRCULAR
		_:
			move_type = MoveType.HORIZONTAL
	var form_str: String = data.get("formType", "normal")
	match form_str:
		"light":
			is_light_platform = true
		"shadow":
			is_shadow_platform = true
	_setup_platform()
