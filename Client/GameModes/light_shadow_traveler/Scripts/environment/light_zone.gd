class_name LightZone
extends Area2D

signal zone_entered(zone_type: String)
signal zone_exited(zone_type: String)

@export var zone_radius := 150.0
@export var light_energy := 1.5
@export var is_permanent := true
@export var pulse_speed := 1.0

var _light: PointLight2D
var _collision: CollisionShape2D
var _pulse_time := 0.0
var _bodies_in_zone: Array[Node2D] = []

func _ready() -> void:
	_setup_zone()
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)

func _setup_zone() -> void:
	_collision = CollisionShape2D.new()
	var shape := CircleShape2D.new()
	shape.radius = zone_radius
	_collision.shape = shape
	add_child(_collision)
	_light = PointLight2D.new()
	_light.color = Color(1.0, 0.95, 0.8, 0.4)
	_light.energy = light_energy
	_light.texture = _make_glow_texture()
	add_child(_light)
	var visual := ColorRect.new()
	visual.size = Vector2(zone_radius * 2, zone_radius * 2)
	visual.position = Vector2(-zone_radius, -zone_radius)
	visual.color = Color(1.0, 0.95, 0.8, 0.05)
	visual.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(visual)

func _make_glow_texture() -> Texture2D:
	var img := Image.create(64, 64, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	for dy in range(-32, 32):
		for dx in range(-32, 32):
			var dist := sqrt(dx * dx + dy * dy) / 32.0
			if dist < 1.0:
				img.set_pixel(32 + dx, 32 + dy, Color(1, 0.95, 0.8, (1.0 - dist) * 0.5))
	return ImageTexture.create_from_image(img)

func _process(delta: float) -> void:
	_pulse_time += delta * pulse_speed
	if _light:
		_light.energy = light_energy + sin(_pulse_time) * 0.3
	for body in _bodies_in_zone:
		if body is PlayerCharacter and body.is_shadow_form():
			if not body.is_stealth_active():
				body.take_damage(1)
				return

func _on_body_entered(body: Node2D) -> void:
	if body is PlayerCharacter:
		_bodies_in_zone.append(body)
		zone_entered.emit("light")
		if body.is_shadow_form() and not body.is_stealth_active():
			body.take_damage(1)

func _on_body_exited(body: Node2D) -> void:
	_bodies_in_zone.erase(body)
	if body is PlayerCharacter:
		zone_exited.emit("light")
