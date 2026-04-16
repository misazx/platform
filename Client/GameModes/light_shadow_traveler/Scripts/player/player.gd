class_name PlayerCharacter
extends CharacterBody2D

signal form_changed(new_form: String)
signal health_changed(new_health: int, max_health: int)
signal player_died
signal fragment_collected(count: int)
signal checkpoint_reached(checkpoint_id: String)
signal energy_changed(current: float, max_val: float)

enum Form { LIGHT, SHADOW }

const COYOTE_TIME := 0.1
const INPUT_BUFFER_TIME := 0.1
const FORM_SWITCH_COOLDOWN := 0.3
const LIGHT_DASH_SPEED := 600.0
const LIGHT_DASH_DURATION := 0.15
const LIGHT_DASH_ENERGY := 25.0
const SHADOW_STEALTH_SPEED_MULT := 0.4
const SHADOW_STEALTH_ENERGY_RATE := 15.0
const MAX_FORM_ENERGY := 100.0
const ENERGY_REGEN_RATE := 12.0
const LIGHT_WALL_JUMP_FORCE := -350.0

@export var light_speed := 280.0
@export var light_jump_force := -550.0
@export var shadow_speed := 350.0
@export var shadow_jump_force := -620.0
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
var form_energy := MAX_FORM_ENERGY
var is_dashing := false
var dash_timer := 0.0
var is_stealthing := false
var can_wall_jump := false
var wall_jump_dir := 0.0
var _trail_timer := 0.0

var _light_color := Color(1.0, 0.95, 0.85, 1.0)
var _shadow_color := Color(0.3, 0.35, 0.6, 0.85)
var _light_glow_tex: Texture2D
var _shadow_glow_tex: Texture2D
var _glow_intensity: float = 0.0

var sprite: Sprite2D
var glow: PointLight2D
var collision: CollisionShape2D
var anim_player: AnimationPlayer
var ray_floor: RayCast2D
var ray_wall_left: RayCast2D
var ray_wall_right: RayCast2D
var stealth_visual: ColorRect
var dash_trail: Node2D
var _attack_area: Area2D

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
	ray_wall_left = RayCast2D.new()
	ray_wall_left.name = "RayWallLeft"
	ray_wall_left.target_position = Vector2(-16, 0)
	ray_wall_left.enabled = true
	add_child(ray_wall_left)
	ray_wall_right = RayCast2D.new()
	ray_wall_right.name = "RayWallRight"
	ray_wall_right.target_position = Vector2(16, 0)
	ray_wall_right.enabled = true
	add_child(ray_wall_right)
	stealth_visual = ColorRect.new()
	stealth_visual.name = "StealthOverlay"
	stealth_visual.size = Vector2(40, 40)
	stealth_visual.position = Vector2(-20, -20)
	stealth_visual.color = Color(0.2, 0.25, 0.5, 0.0)
	stealth_visual.mouse_filter = Control.MOUSE_FILTER_IGNORE
	stealth_visual.z_index = 1
	add_child(stealth_visual)
	dash_trail = Node2D.new()
	dash_trail.name = "DashTrail"
	add_child(dash_trail)
	_attack_area = Area2D.new()
	_attack_area.name = "AttackArea"
	var atk_shape := CollisionShape2D.new()
	var atk_circle := CircleShape2D.new()
	atk_circle.radius = 20.0
	atk_shape.shape = atk_circle
	_attack_area.add_child(atk_shape)
	_attack_area.monitoring = false
	_attack_area.body_entered.connect(_on_attack_body_entered)
	add_child(_attack_area)
	_create_player_sprite()

func _create_player_sprite() -> void:
	var form_path: String = "res://GameModes/light_shadow_traveler/Resources/Characters/light_form.png" if current_form == Form.LIGHT else "res://GameModes/light_shadow_traveler/Resources/Characters/shadow_form.png"
	if ResourceLoader.exists(form_path):
		var tex: Texture2D = load(form_path) as Texture2D
		if tex:
			sprite.texture = tex
			sprite.scale = Vector2(0.2, 0.2)
			return
	var img := Image.create(32, 32, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	var body_color: Color = _light_color if current_form == Form.LIGHT else _shadow_color
	for dy in range(-10, 11):
		for dx in range(-8, 9):
			var dist: float = sqrt(dx * dx + dy * dy * 0.6)
			if dist < 10:
				var alpha: float = 1.0 - dist / 10.0
				img.set_pixel(16 + dx, 16 + dy, Color(body_color.r, body_color.g, body_color.b, alpha))
	for dy in range(-6, -2):
		for dx in range(-4, 5):
			if abs(dx) < 3:
				img.set_pixel(16 + dx, 10 + dy, Color(1, 1, 1, 0.9))
	img.set_pixel(14, 8, Color(0.1, 0.1, 0.2, 1.0))
	img.set_pixel(18, 8, Color(0.1, 0.1, 0.2, 1.0))
	sprite.texture = ImageTexture.create_from_image(img)
	sprite.hframes = 1

func _physics_process(delta: float) -> void:
	if is_dead:
		return
	_update_timers(delta)
	_handle_input()
	_apply_gravity(delta)
	_check_fall_death()
	_handle_jump()
	_handle_wall_jump()
	_handle_form_switch()
	_handle_dash(delta)
	_handle_stealth(delta)
	_regen_energy(delta)
	_move(delta)
	_update_coyote_time()
	_update_wall_detection()
	_update_visuals(delta)
	_update_dash_trail(delta)
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
	if is_dashing:
		dash_timer -= delta
		if dash_timer <= 0:
			is_dashing = false
	if _attack_area:
		_attack_area.monitoring = false

func _handle_input() -> void:
	if Input.is_action_just_pressed("jump"):
		jump_buffer_timer = INPUT_BUFFER_TIME
	if Input.is_action_just_pressed("switch_form") and form_switch_timer <= 0:
		_switch_form()
	if Input.is_action_just_pressed("move_right"):
		facing_right = true
	if Input.is_action_just_pressed("move_left"):
		facing_right = false
	if Input.is_action_just_pressed("light_dash") and current_form == Form.LIGHT and form_energy >= LIGHT_DASH_ENERGY and not is_dashing:
		_start_dash()
	if Input.is_action_pressed("shadow_stealth") and current_form == Form.SHADOW and form_energy > 0:
		is_stealthing = true
	else:
		if is_stealthing:
			is_stealthing = false
			stealth_visual.color = Color(0.2, 0.25, 0.5, 0.0)

func _switch_form() -> void:
	form_switch_timer = FORM_SWITCH_COOLDOWN
	var new_form: Form = Form.SHADOW if current_form == Form.LIGHT else Form.LIGHT
	current_form = new_form
	is_stealthing = false
	is_dashing = false
	stealth_visual.color = Color(0.2, 0.25, 0.5, 0.0)
	_update_form_visuals()
	form_changed.emit("shadow" if current_form == Form.SHADOW else "light")
	ParticleEffect.create_and_spawn(get_parent(), global_position, ParticleEffect.EffectType.FORM_SWITCH_LIGHT if current_form == Form.LIGHT else ParticleEffect.EffectType.FORM_SWITCH_SHADOW, 15)

func _start_dash() -> void:
	is_dashing = true
	dash_timer = LIGHT_DASH_DURATION
	if _attack_area:
		_attack_area.monitoring = true
	form_energy -= LIGHT_DASH_ENERGY
	energy_changed.emit(form_energy, MAX_FORM_ENERGY)
	velocity.y = 0.0
	ParticleEffect.create_and_spawn(get_parent(), global_position, ParticleEffect.EffectType.FORM_SWITCH_LIGHT, 20)

func _handle_dash(delta: float) -> void:
	if not is_dashing:
		return
	var dash_dir: float = 1.0 if facing_right else -1.0
	velocity.x = LIGHT_DASH_SPEED * dash_dir
	velocity.y = 0.0
	move_and_slide()

func _handle_stealth(delta: float) -> void:
	if not is_stealthing:
		return
	form_energy -= SHADOW_STEALTH_ENERGY_RATE * delta
	if form_energy <= 0:
		form_energy = 0
		is_stealthing = false
		stealth_visual.color = Color(0.2, 0.25, 0.5, 0.0)
	energy_changed.emit(form_energy, MAX_FORM_ENERGY)
	stealth_visual.color = Color(0.2, 0.25, 0.5, 0.4)
	if glow:
		glow.energy = 0.1

func _regen_energy(delta: float) -> void:
	if is_dashing or is_stealthing:
		return
	var regen: float = ENERGY_REGEN_RATE * delta
	if current_form == Form.LIGHT:
		regen *= 1.5
	form_energy = min(form_energy + regen, MAX_FORM_ENERGY)
	energy_changed.emit(form_energy, MAX_FORM_ENERGY)

func _update_wall_detection() -> void:
	if is_on_floor():
		can_wall_jump = false
		return
	if ray_wall_left.is_colliding() and not facing_right:
		can_wall_jump = true
		wall_jump_dir = 1.0
	elif ray_wall_right.is_colliding() and facing_right:
		can_wall_jump = true
		wall_jump_dir = -1.0
	else:
		can_wall_jump = false

func _handle_wall_jump() -> void:
	if not can_wall_jump:
		return
	if jump_buffer_timer > 0 and current_form == Form.LIGHT:
		velocity.x = light_speed * wall_jump_dir
		velocity.y = LIGHT_WALL_JUMP_FORCE
		jump_buffer_timer = 0.0
		coyote_timer = 0.0
		facing_right = wall_jump_dir > 0
		ParticleEffect.create_and_spawn(get_parent(), global_position, ParticleEffect.EffectType.FORM_SWITCH_LIGHT, 8)

func is_light_form() -> bool:
	return current_form == Form.LIGHT

func is_shadow_form() -> bool:
	return current_form == Form.SHADOW

func is_stealth_active() -> bool:
	return is_stealthing

func _apply_gravity(delta: float) -> void:
	if is_dashing:
		return
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
			var jump_force: float = light_jump_force if current_form == Form.LIGHT else shadow_jump_force
			velocity.y = jump_force
			jump_buffer_timer = 0.0
			coyote_timer = 0.0
			is_glidering = false

func _handle_form_switch() -> void:
	pass

func _move(_delta: float) -> void:
	if is_dashing:
		return
	var speed: float = light_speed if current_form == Form.LIGHT else shadow_speed
	if is_stealthing:
		speed *= SHADOW_STEALTH_SPEED_MULT
	var input_dir: float = Input.get_axis("move_left", "move_right")
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
		if _light_glow_tex == null:
			_light_glow_tex = _create_glow_texture(Color(1.0, 0.95, 0.8))
		if _light_glow_tex:
			glow.texture = _light_glow_tex
	else:
		glow.color = Color(0.3, 0.35, 0.7, 0.4)
		glow.energy = 0.6
		if _shadow_glow_tex == null:
			_shadow_glow_tex = _create_glow_texture(Color(0.3, 0.35, 0.7))
		if _shadow_glow_tex:
			glow.texture = _shadow_glow_tex

func _create_glow_texture(color: Color) -> Texture2D:
	var img := Image.create(64, 64, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	for dy in range(-32, 32):
		for dx in range(-32, 32):
			var dist: float = sqrt(dx * dx + dy * dy) / 32.0
			if dist < 1.0:
				var alpha: float = (1.0 - dist) * 0.5
				img.set_pixel(32 + dx, 32 + dy, Color(color.r, color.g, color.b, alpha))
	return ImageTexture.create_from_image(img)

func _update_visuals(delta: float) -> void:
	modulate.a = 1.0
	if is_invincible:
		modulate.a = 0.5 + sin(Engine.get_frames_drawn() * 0.5) * 0.3
	if is_glidering:
		_glow_intensity = min(_glow_intensity + delta * 3.0, 1.0)
	else:
		_glow_intensity = max(_glow_intensity - delta * 3.0, 0.0)
	if glow and not is_stealthing:
		glow.energy = (1.2 if current_form == Form.LIGHT else 0.6) + _glow_intensity * 0.5
	if is_dashing:
		pass

func _update_dash_trail(delta: float) -> void:
	if not is_dashing:
		for child in dash_trail.get_children():
			child.modulate.a -= delta * 5.0
			if child.modulate.a <= 0:
				child.queue_free()
		return
	_trail_timer += delta
	if _trail_timer > 0.03:
		_trail_timer = 0.0
		var trail_sprite := Sprite2D.new()
		trail_sprite.texture = sprite.texture
		trail_sprite.flip_h = sprite.flip_h
		trail_sprite.global_position = global_position
		trail_sprite.modulate = Color(1.0, 0.95, 0.7, 0.5)
		dash_trail.add_child(trail_sprite)

func take_damage(amount: int = 1) -> void:
	if is_invincible or is_dead or is_stealthing:
		return
	current_health -= amount
	health_changed.emit(current_health, max_health)
	ParticleEffect.create_and_spawn(get_parent(), global_position, ParticleEffect.EffectType.DAMAGE_TAKEN, 15)
	if current_health <= 0:
		die()
	else:
		is_invincible = true
		invincible_timer = invincible_time
		velocity.y = -200.0
		velocity.x = -100.0 * (1 if facing_right else -1)

func _on_attack_body_entered(body: Node2D) -> void:
	if body is FormEnemy:
		var enemy: FormEnemy = body as FormEnemy
		if enemy.is_hostile:
			enemy.take_damage(1)
			var knockback_dir: float = 1.0 if enemy.global_position.x > global_position.x else -1.0
			enemy.velocity = Vector2(knockback_dir * 200.0, -150.0)

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
	ParticleEffect.create_and_spawn(get_parent(), global_position, ParticleEffect.EffectType.HEAL, 12)

func collect_fragment() -> void:
	fragments_collected += 1
	fragment_collected.emit(fragments_collected)

func reach_checkpoint(checkpoint_id: String) -> void:
	checkpoint_reached.emit(checkpoint_id)

func get_form_name() -> String:
	return "light" if current_form == Form.LIGHT else "shadow"
