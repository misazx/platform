class_name ShadowZone
extends Area2D

signal zone_entered(zone_type: String)
signal zone_exited(zone_type: String)

@export var zone_radius := 150.0
@export var heal_rate := 5.0
@export var speed_boost := 1.3

var _dark_light: PointLight2D
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
	_dark_light = PointLight2D.new()
	_dark_light.color = Color(0.15, 0.18, 0.35, 0.5)
	_dark_light.energy = -1.0
	_dark_light.texture = _make_dark_texture()
	add_child(_dark_light)
	var visual := ColorRect.new()
	visual.size = Vector2(zone_radius * 2, zone_radius * 2)
	visual.position = Vector2(-zone_radius, -zone_radius)
	visual.color = Color(0.1, 0.12, 0.25, 0.1)
	visual.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(visual)

func _make_dark_texture() -> Texture2D:
	var img := Image.create(64, 64, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	for dy in range(-32, 32):
		for dx in range(-32, 32):
			var dist := sqrt(dx * dx + dy * dy) / 32.0
			if dist < 1.0:
				img.set_pixel(32 + dx, 32 + dy, Color(0.1, 0.12, 0.25, (1.0 - dist) * 0.5))
	return ImageTexture.create_from_image(img)

func _process(delta: float) -> void:
	_pulse_time += delta * 0.8
	if _dark_light:
		_dark_light.energy = -1.0 + sin(_pulse_time) * 0.2
	for body in _bodies_in_zone:
		if body is PlayerCharacter and body.is_shadow_form():
			body.heal(1) 
			if Engine.get_frames_drawn() % 60 != 0 :
				pass

func _on_body_entered(body: Node2D) -> void:
	if body is PlayerCharacter:
		_bodies_in_zone.append(body)
		zone_entered.emit("shadow")

func _on_body_exited(body: Node2D) -> void:
	_bodies_in_zone.erase(body)
	if body is PlayerCharacter:
		zone_exited.emit("shadow")
