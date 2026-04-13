class_name LightShadowSwitch
extends Area2D

signal switch_activated(switch_id: String)
signal switch_deactivated(switch_id: String)

enum SwitchType { LIGHT_ONLY, SHADOW_ONLY, BOTH }
enum SwitchTarget { PLATFORM, DOOR, LIGHT_SOURCE, ENEMY }

@export var switch_id := ""
@export var switch_type: SwitchType = SwitchType.BOTH
@export var target_type: SwitchTarget = SwitchTarget.PLATFORM
@export var target_id := ""
@export var is_toggle := false
@export var stay_active := true

var is_active := false
var sprite: Sprite2D
var glow: PointLight2D
var collision_shape: CollisionShape2D
var _cooldown_timer := 0.0

func _ready() -> void:
	_setup_visuals()
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)

func _setup_visuals() -> void:
	collision_shape = CollisionShape2D.new()
	var shape := CircleShape2D.new()
	shape.radius = 25.0
	collision_shape.shape = shape
	add_child(collision_shape)
	sprite = Sprite2D.new()
	add_child(sprite)
	_draw_switch_sprite()
	glow = PointLight2D.new()
	glow.color = Color(0.6, 0.8, 1.0, 0.4)
	glow.energy = 0.5
	add_child(glow)

func _draw_switch_sprite() -> void:
	var img := Image.create(32, 32, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	var base_color := Color(0.6, 0.8, 1.0)
	match switch_type:
		SwitchType.LIGHT_ONLY:
			base_color = Color(1.0, 0.95, 0.7)
		SwitchType.SHADOW_ONLY:
			base_color = Color(0.3, 0.35, 0.7)
	for dy in range(-12, 13):
		for dx in range(-12, 13):
			var dist := sqrt(dx * dx + dy * dy) / 12.0
			if dist < 1.0:
				var alpha := (1.0 - dist) * 0.8
				img.set_pixel(16 + dx, 16 + dy, Color(base_color.r, base_color.g, base_color.b, alpha))
	for dy in range(-4, -1):
		for dx in range(-2, 3):
			img.set_pixel(16 + dx, 10 + dy, Color(1, 1, 1, 0.9))
	sprite.texture = ImageTexture.create_from_image(img)

func _physics_process(delta: float) -> void:
	if _cooldown_timer > 0:
		_cooldown_timer -= delta

func _on_body_entered(body: Node2D) -> void:
	if _cooldown_timer > 0:
		return
	if not body is PlayerCharacter:
		return
	var player := body as PlayerCharacter
	var can_activate := false
	match switch_type:
		SwitchType.LIGHT_ONLY:
			can_activate = player.is_light_form()
		SwitchType.SHADOW_ONLY:
			can_activate = player.is_shadow_form()
		SwitchType.BOTH:
			can_activate = true
	if can_activate:
		_activate()

func _on_body_exited(_body: Node2D) -> void:
	if not stay_active and is_active:
		_deactivate()

func _activate() -> void:
	if is_active and is_toggle:
		_deactivate()
		return
	is_active = true
	_cooldown_timer = 0.5
	glow.energy = 1.2
	glow.color = Color(1.0, 0.9, 0.5, 0.7)
	switch_activated.emit(switch_id)

func _deactivate() -> void:
	is_active = false
	glow.energy = 0.5
	glow.color = Color(0.6, 0.8, 1.0, 0.4)
	switch_deactivated.emit(switch_id)

func setup_from_data(data: Dictionary) -> void:
	switch_id = data.get("id", "")
	target_id = data.get("targetId", "")
	var type_str: String = data.get("switchType", "both")
	match type_str:
		"light_only":
			switch_type = SwitchType.LIGHT_ONLY
		"shadow_only":
			switch_type = SwitchType.SHADOW_ONLY
		_:
			switch_type = SwitchType.BOTH
	var target_str: String = data.get("targetType", "platform")
	match target_str:
		"door":
			target_type = SwitchTarget.DOOR
		"light_source":
			target_type = SwitchTarget.LIGHT_SOURCE
		"enemy":
			target_type = SwitchTarget.ENEMY
		_:
			target_type = SwitchTarget.PLATFORM
	is_toggle = data.get("isToggle", false)
	stay_active = data.get("stayActive", true)
	_draw_switch_sprite()
