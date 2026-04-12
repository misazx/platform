class_name FormEnemy
extends CharacterBody2D

enum EnemyAttribute { LIGHT, SHADOW }
enum EnemyState { PATROL, CHASE, IDLE, STUNNED }

@export var attribute: EnemyAttribute = EnemyAttribute.SHADOW
@export var move_speed := 80.0
@export var chase_speed := 150.0
@export var patrol_range := 150.0
@export var detection_range := 250.0
@export var attack_range := 30.0
@export var knockback_force := 300.0
@export var damage := 1
@export var is_boss := false

var state: EnemyState = EnemyState.PATROL
var start_position: Vector2
var patrol_direction := 1.0
var state_timer := 0.0
var is_hostile := true
var facing_right := true

@onready var sprite: Sprite2D = $Sprite2D
@onready var detection_area: Area2D = $DetectionArea
@onready var attack_area: Area2D = $AttackArea
@onready var health_bar: ProgressBar = $HealthBar

func _ready() -> void:
	start_position = global_position
	_setup_visuals()
	_setup_areas()
	_update_hostility()

func _setup_visuals() -> void:
	if not sprite:
		sprite = Sprite2D.new()
		add_child(sprite)
	var img := Image.create(32, 32, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	var body_color := Color(1.0, 0.9, 0.7) if attribute == EnemyAttribute.LIGHT else Color(0.3, 0.35, 0.6)
	for dy in range(-10, 11):
		for dx in range(-8, 9):
			var dist := sqrt(dx * dx + dy * dy * 0.8)
			if dist < 10:
				var alpha := 1.0 - dist / 10.0
				img.set_pixel(16 + dx, 16 + dy, Color(body_color.r, body_color.g, body_color.b, alpha))
	if attribute == EnemyAttribute.LIGHT:
		img.set_pixel(13, 8, Color(1, 0.8, 0.3, 1))
		img.set_pixel(19, 8, Color(1, 0.8, 0.3, 1))
	else:
		img.set_pixel(13, 8, Color(0.5, 0.5, 1.0, 1))
		img.set_pixel(19, 8, Color(0.5, 0.5, 1.0, 1))
	if is_boss:
		var boss_img := Image.create(48, 48, false, Image.FORMAT_RGBA8)
		boss_img.fill(Color(0, 0, 0, 0))
		for dy in range(-16, 17):
			for dx in range(-14, 15):
				var dist := sqrt(dx * dx + dy * dy * 0.7)
				if dist < 16:
					var alpha := 1.0 - dist / 16.0
					boss_img.set_pixel(24 + dx, 24 + dy, Color(body_color.r * 1.2, body_color.g * 1.1, body_color.b * 0.8, alpha))
		sprite.texture = ImageTexture.create_from_image(boss_img)
	else:
		sprite.texture = ImageTexture.create_from_image(img)

func _setup_areas() -> void:
	if not detection_area:
		detection_area = Area2D.new()
		detection_area.name = "DetectionArea"
		add_child(detection_area)
		var det_shape := CollisionShape2D.new()
		var det_circle := CircleShape2D.new()
		det_circle.radius = detection_range
		det_shape.shape = det_circle
		detection_area.add_child(det_shape)
	if not attack_area:
		attack_area = Area2D.new()
		attack_area.name = "AttackArea"
		add_child(attack_area)
		var atk_shape := CollisionShape2D.new()
		var atk_circle := CircleShape2D.new()
		atk_circle.radius = attack_range
		atk_shape.shape = atk_circle
		attack_area.add_child(atk_shape)

func _physics_process(delta: float) -> void:
	_update_hostility()
	match state:
		EnemyState.PATROL:
			_patrol(delta)
		EnemyState.CHASE:
			_chase(delta)
		EnemyState.IDLE:
			_idle(delta)
	if not is_on_floor():
		velocity.y += 980.0 * delta
	move_and_slide()

func _update_hostility() -> void:
	var player := _get_player()
	if player == null:
		is_hostile = true
		return
	match attribute:
		EnemyAttribute.LIGHT:
			is_hostile = player.is_shadow_form()
		EnemyAttribute.SHADOW:
			is_hostile = player.is_light_form()
	if is_hostile:
		modulate = Color(1.0, 0.7, 0.7, 1.0) if attribute == EnemyAttribute.SHADOW else Color(1.0, 1.0, 0.7, 1.0)
	else:
		modulate = Color(0.7, 0.7, 0.7, 0.6)

func _patrol(delta: float) -> void:
	velocity.x = move_speed * patrol_direction
	if global_position.x > start_position.x + patrol_range:
		patrol_direction = -1.0
	elif global_position.x < start_position.x - patrol_range:
		patrol_direction = 1.0
	sprite.flip_h = patrol_direction < 0
	facing_right = patrol_direction > 0
	if is_hostile:
		var player := _get_player()
		if player and global_position.distance_to(player.global_position) < detection_range:
			state = EnemyState.CHASE

func _chase(delta: float) -> void:
	var player := _get_player()
	if player == null or not is_hostile:
		state = EnemyState.PATROL
		return
	var dist := global_position.distance_to(player.global_position)
	if dist > detection_range * 1.5 or not is_hostile:
		state = EnemyState.PATROL
		return
	var dir: float = sign(player.global_position.x - global_position.x)
	velocity.x = chase_speed * dir
	sprite.flip_h = dir < 0
	facing_right = dir > 0
	if dist < attack_range:
		_attack_player(player)

func _idle(delta: float) -> void:
	velocity.x = 0
	state_timer -= delta
	if state_timer <= 0:
		state = EnemyState.PATROL

func _attack_player(player: PlayerCharacter) -> void:
	if player and not player.is_dead:
		player.take_damage(damage)
		var knockback := Vector2(knockback_force * (1 if player.global_position.x > global_position.x else -1), -200)
		player.velocity = knockback
	state = EnemyState.IDLE
	state_timer = 1.0

func _get_player() -> PlayerCharacter:
	var players := get_tree().get_nodes_in_group("player")
	if players.size() > 0:
		return players[0] as PlayerCharacter
	return null

func setup_from_data(data: Dictionary) -> void:
	position = Vector2(data.get("x", 0), data.get("y", 0))
	var attr_str: String = data.get("attribute", "shadow")
	match attr_str:
		"light":
			attribute = EnemyAttribute.LIGHT
		_:
			attribute = EnemyAttribute.SHADOW
	var type_str: String = data.get("type", "")
	if type_str.find("boss") >= 0 or type_str.find("guardian") >= 0:
		is_boss = true
		move_speed = 60.0
		chase_speed = 120.0
		detection_range = 350.0
		damage = 2
	_setup_visuals()
