class_name Player extends CharacterBody2D

signal health_changed(current_health: int, max_health: int)
signal died()

@export var max_health: int = 80
@export var attack: int = 10
@export var defense: int = 5
@export var speed: float = 200.0
@export var dash_speed: float = 500.0
@export var dash_duration: float = 0.2
@export var dash_cooldown: float = 1.0

var current_health: int
var is_dashing := false
var is_invincible := false
var _dash_timer := 0.0
var _dash_cooldown_timer := 0.0
var _dash_direction := Vector2.ZERO

func _ready() -> void:
	current_health = max_health
	health_changed.emit(current_health, max_health)
	print("[Player] Ready with %d/%d HP" % [current_health, max_health])

func _physics_process(delta: float) -> void:
	var dt := delta

	if _dash_cooldown_timer > 0:
		_dash_cooldown_timer -= dt

	if is_dashing:
		_dash_timer -= dt
		if _dash_timer <= 0:
			is_dashing = false
			is_invincible = false

	var velocity := Vector2.ZERO

	if is_dashing:
		velocity = _dash_direction * dash_speed
	else:
		var input_dir := Vector2.ZERO
		if Input.is_action_pressed("ui_up"):
			input_dir.y -= 1
		if Input.is_action_pressed("ui_down"):
			input_dir.y += 1
		if Input.is_action_pressed("ui_left"):
			input_dir.x -= 1
		if Input.is_action_pressed("ui_right"):
			input_dir.x += 1

		if input_dir != Vector2.ZERO:
			input_dir = input_dir.normalized()
			velocity = input_dir * speed

	velocity = move_and_slide()

	if Input.is_action_just_pressed("ui_accept") and _dash_cooldown_timer <= 0:
		dash(input_dir)

func dash(direction: Vector2) -> void:
	if direction == Vector2.ZERO:
		return
	is_dashing = true
	is_invincible = true
	_dash_direction = direction.normalized()
	_dash_timer = dash_duration
	_dash_cooldown_timer = dash_cooldown
	print("[Player] Dashing!")

func take_damage(amount: int) -> void:
	if is_invincible:
		return
	current_health = maxi(0, current_health - amount)
	health_changed.emit(current_health, max_health)
	print("[Player] Took %d damage! HP: %d/%d" % [amount, current_health, max_health])
	if current_health <= 0:
		die()

func heal(amount: int) -> void:
	current_health = mini(max_health, current_health + amount)
	health_changed.emit(current_health, max_health)

func die() -> void:
	died.emit()
	print("[Player] Died!")

func get_defense() -> int:
	return defense

func get_current_health() -> int:
	return current_health

func get_max_health() -> int:
	return max_health
