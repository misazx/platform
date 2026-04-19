class_name MovingPlatform
extends AnimatableBody2D

enum MoveType { HORIZONTAL, VERTICAL, CIRCULAR, PATH }

const NORMAL_PLAT_PATH: String = "res://GameModes/light_shadow_traveler/Resources/Platforms/normal_platform.png"
const LIGHT_PLAT_PATH: String = "res://GameModes/light_shadow_traveler/Resources/Platforms/light_platform.png"
const SHADOW_PLAT_PATH: String = "res://GameModes/light_shadow_traveler/Resources/Platforms/shadow_platform.png"

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
var _is_setup := false

var collision_shape: CollisionShape2D
var visual: ColorRect
var plat_visual: TextureRect

func _ready() -> void:
	start_position = global_position
	if not _is_setup:
		_setup_platform()

func set_active(active: bool) -> void:
	if collision_shape:
		collision_shape.disabled = not active
	if plat_visual and is_instance_valid(plat_visual):
		plat_visual.visible = active
		plat_visual.modulate.a = 1.0 if active else 0.3
	if visual and visual.visible:
		visual.visible = active
	set_physics_process(active)

func _setup_platform() -> void:
	_is_setup = true
	_clear_old_visuals()
	var shape := RectangleShape2D.new()
	shape.size = Vector2(platform_width, platform_height)
	if collision_shape == null:
		collision_shape = CollisionShape2D.new()
		collision_shape.name = "CollisionShape2D"
		add_child(collision_shape)
	collision_shape.shape = shape
	if visual == null:
		visual = ColorRect.new()
		visual.name = "Visual"
		add_child(visual)
	visual.size = Vector2(platform_width, platform_height)
	visual.position = Vector2(-platform_width / 2.0, -platform_height / 2.0)
	visual.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var plat_path: String = NORMAL_PLAT_PATH
	var plat_color: Color = Color(0.5, 0.5, 0.5, 1.0)
	if is_light_platform:
		plat_path = LIGHT_PLAT_PATH
		plat_color = Color(1.0, 0.95, 0.7, 0.9)
	elif is_shadow_platform:
		plat_path = SHADOW_PLAT_PATH
		plat_color = Color(0.3, 0.35, 0.6, 0.85)
	if ResourceLoader.exists(plat_path):
		var tex: Texture2D = load(plat_path) as Texture2D
		if tex:
			plat_visual = TextureRect.new()
			plat_visual.name = "PlatformSprite"
			plat_visual.texture = tex
			plat_visual.stretch_mode = TextureRect.STRETCH_TILE
			plat_visual.size = Vector2(platform_width, platform_height)
			plat_visual.position = Vector2(-platform_width / 2.0, -platform_height / 2.0)
			plat_visual.mouse_filter = Control.MOUSE_FILTER_IGNORE
			plat_visual.texture_repeat = CanvasItem.TEXTURE_REPEAT_MIRROR
			add_child(plat_visual)
			visual.visible = false
		else:
			visual.color = plat_color
			visual.visible = true
	else:
		visual.color = plat_color
		visual.visible = true

func _clear_old_visuals() -> void:
	for child in get_children():
		if child is TextureRect and child.name == "PlatformSprite":
			child.queue_free()
			break

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
	platform_width = float(data.get("w", 120))
	platform_height = float(data.get("h", 20))
	move_speed = float(data.get("speed", 80))
	move_range = float(data.get("range", 200))
	pause_time = float(data.get("pause", 1.0))
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
