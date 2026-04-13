class_name PlayerCharacter
extends CharacterBody2D

signal form_changed(new_form: String)
signal health_changed(new_health: int, max_health: int)
signal player_died
signal fragment_collected(count: int)
signal checkpoint_reached(checkpoint_id: String)

enum Form { LIGHT, SHADOW }

const COYOTE_TIME := 0.1
const INPUT_BUFFER_TIME := 0.1
const FORM_SWITCH_COOLDOWN := 0.3

@export var light_speed := 250.0
@export var light_jump_force := -420.0
@export var shadow_speed := 320.0
@export var shadow_jump_force := -500.0
@export var shadow_glide_gravity := 200.0
@export var shadow_glide_max_fall := 100.0
@export var gravity := 980.0
@export var max_health := 3
@export var invincible_time := 1.5

var current_form: Form = Form.LIGHT
var current_health: int
var is_invincible := false
var is_glidering := false
var is_dead := false
var fragments_collected := 0
var coyote_timer := 0.0
var jump_buffer_timer := 0.0
var form_switch_timer := 0.0
var invincible_timer := 0.0
var was_on_floor := false
var facing_right := true

var _light_color := Color(1.0, 0.95, 0.85, 1.0)
var _shadow_color := Color(0.3, 0.35, 0.6, 0.85)
var _glow_intensity := 0.0

var sprite: Sprite2D
var glow: PointLight2D
var collision: CollisionShape2D
var anim_player: AnimationPlayer
var ray_floor: RayCast2D

func _ready() -> void:
	current_health = max_health
	_setup_visuals()
	_update_form_visuals()
	form_changed.emit("light")
	health_changed.emit(current_health, max_health)

func _setup_visuals() -> void:
	sprite = Sprite2D.new()
	add_child(sprite)
	glow = PointLight2D.new()
	glow.name = "Glow"
	add_child(glow)
	collision = CollisionShape2D.new()
	var shape = CapsuleShape2D.new()
	shape.radius = 12.0
	shape.height = 28.0
	collision.shape = shape
	add_child(collision)
	ray_floor = RayCast2D.new()
	ray_floor.name = "RayCastFloor"
	ray_floor.target_position = Vector2(0, 20)
	ray_floor.enabled = true
	add_child(ray_floor)
	_create_player_sprite()

func _create_player_sprite() -> void:
	var img := Image.create(32, 32, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	var body_color := _light_color if current_form == Form.LIGHT else _shadow_color
	for dy in range(-10, 11):
		for dx in range(-8, 9):
			var dist := sqrt(dx * dx + dy * dy * 0.6)
			if dist < 10:
				var alpha := 1.0 - dist / 10.0
				img.set_pixel(16 + dx, 16 + dy, Color(body_color.r, body_color.g, body_color.b, alpha))
	for dy in range(-6, -2):
		for dx in range(-4, 5):
			if abs(dx) < 3:
				img.set_pixel(16 + dx, 10 + dy, Color(1, 1, 1, 0.9))
	img.set_pixel(14, 8, Color(0.1, 0.1, 0.2, 1.0))
	img.set_pixel(18, 8, Color(0.1, 0.1, 0.2, 1.0))
	var tex := ImageTexture.create_from_image(img)
	sprite.texture = tex
	sprite.hframes = 1

func _physics_process(delta: float) -> void:
	if is_dead:
		return
	_update_timers(delta)
	_handle_input()
	_apply_gravity(delta)
	_check_fall_death()
	_handle_jump()
	_handle_form_switch()
	_move(delta)
	_update_coyote_time()
	_update_visuals(delta)
	was_on_floor = is_on_floor()

func _check_fall_death() -> void:
	if global_position.y > 1000:
		die()

func _update_timers(delta: float) -> void:
	if jump_buffer_timer > 0:
		jump_buffer_timer -= delta
	if form_switch_timer > 0:
		form_switch_timer -= delta
	if is_invincible:
		invincible_timer -= delta
		if invincible_timer <= 0:
			is_invincible = false
			modulate.a = 1.0

func _handle_input() -> void:
	if Input.is_action_just_pressed("jump"):
		jump_buffer_timer = INPUT_BUFFER_TIME
	if Input.is_action_just_pressed("switch_form") and form_switch_timer <= 0:
		_switch_form()
	if Input.is_action_just_pressed("move_right"):
		facing_right = true
	if Input.is_action_just_pressed("move_left"):
		facing_right = false

func _switch_form() -> void:
	form_switch_timer = FORM_SWITCH_COOLDOWN
	var new_form := Form.SHADOW if current_form == Form.LIGHT else Form.LIGHT
	current_form = new_form
	_update_form_visuals()
	form_changed.emit("shadow" if current_form == Form.SHADOW else "light")

func _apply_gravity(delta: float) -> void:
	if is_on_floor():
		is_glidering = false
		return
	if current_form == Form.SHADOW and Input.is_action_pressed("jump") and velocity.y > 0:
		is_glidering = true
		velocity.y += shadow_glide_gravity * delta
		velocity.y = min(velocity.y, shadow_glide_max_fall)
	else:
		is_glidering = false
		velocity.y += gravity * delta

func _handle_jump() -> void:
	if jump_buffer_timer > 0:
		if is_on_floor() or coyote_timer > 0:
			var jump_force := light_jump_force if current_form == Form.LIGHT else shadow_jump_force
			velocity.y = jump_force
			jump_buffer_timer = 0.0
			coyote_timer = 0.0
			is_glidering = false

func _handle_form_switch() -> void:
	pass

func _move(_delta: float) -> void:
	var speed := light_speed if current_form == Form.LIGHT else shadow_speed
	var input_dir := Input.get_axis("move_left", "move_right")
	velocity.x = input_dir * speed
	if input_dir != 0:
		sprite.flip_h = input_dir < 0
		facing_right = input_dir > 0
	move_and_slide()

func _update_coyote_time() -> void:
	if was_on_floor and not is_on_floor() and velocity.y >= 0:
		coyote_timer = COYOTE_TIME
	elif is_on_floor():
		coyote_timer = 0.0
	else:
		coyote_timer -= get_physics_process_delta_time()

func _update_form_visuals() -> void:
	_create_player_sprite()
	if not is_instance_valid(glow):
		return
	if current_form == Form.LIGHT:
		glow.color = Color(1.0, 0.95, 0.8, 0.6)
		glow.energy = 1.2
		var tex := _create_glow_texture(Color(1.0, 0.95, 0.8))
		if tex:
			glow.texture = tex
	else:
		glow.color = Color(0.3, 0.35, 0.7, 0.4)
		glow.energy = 0.6
		var tex := _create_glow_texture(Color(0.3, 0.35, 0.7))
		if tex:
			glow.texture = tex

func _create_glow_texture(color: Color) -> Texture2D:
	var img := Image.create(64, 64, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	for dy in range(-32, 32):
		for dx in range(-32, 32):
			var dist := sqrt(dx * dx + dy * dy) / 32.0
			if dist < 1.0:
				var alpha := (1.0 - dist) * 0.5
				img.set_pixel(32 + dx, 32 + dy, Color(color.r, color.g, color.b, alpha))
	return ImageTexture.create_from_image(img)

func _update_visuals(delta: float) -> void:
	if is_invincible:
		modulate.a = 0.5 + sin(Engine.get_frames_drawn() * 0.5) * 0.3
	if is_glidering:
		_glow_intensity = min(_glow_intensity + delta * 3.0, 1.0)
	else:
		_glow_intensity = max(_glow_intensity - delta * 3.0, 0.0)
	if glow:
		glow.energy = (1.2 if current_form == Form.LIGHT else 0.6) + _glow_intensity * 0.5

func take_damage(amount: int = 1) -> void:
	if is_invincible or is_dead:
		return
	current_health -= amount
	health_changed.emit(current_health, max_health)
	if current_health <= 0:
		die()
	else:
		is_invincible = true
		invincible_timer = invincible_time
		velocity.y = -200.0
		velocity.x = -100.0 * (1 if facing_right else -1)

func die() -> void:
	is_dead = true
	velocity = Vector2.ZERO
	player_died.emit()
	var tween := create_tween()
	tween.tween_property(self, "modulate:a", 0.0, 0.5)
	tween.tween_callback(queue_free)

func heal(amount: int = 1) -> void:
	current_health = min(current_health + amount, max_health)
	health_changed.emit(current_health, max_health)

func collect_fragment() -> void:
	fragments_collected += 1
	fragment_collected.emit(fragments_collected)

func reach_checkpoint(checkpoint_id: String) -> void:
	checkpoint_reached.emit(checkpoint_id)

func get_form_name() -> String:
	return "light" if current_form == Form.LIGHT else "shadow"

func is_light_form() -> bool:
	return current_form == Form.LIGHT

func is_shadow_form() -> bool:
	return current_form == Form.SHADOW
