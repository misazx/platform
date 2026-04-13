class_name Checkpoint
extends Area2D

signal checkpoint_activated(checkpoint_id: String)

@export var checkpoint_id := ""
@export var is_start := false

var is_activated := false
var sprite: Sprite2D
var glow: PointLight2D
var collision_shape: CollisionShape2D

func _ready() -> void:
	_setup_visuals()
	body_entered.connect(_on_body_entered)
	if is_start:
		_activate()

func _setup_visuals() -> void:
	collision_shape = CollisionShape2D.new()
	var shape := RectangleShape2D.new()
	shape.size = Vector2(30, 50)
	collision_shape.shape = shape
	add_child(collision_shape)
	sprite = Sprite2D.new()
	add_child(sprite)
	var img := Image.create(30, 50, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	for dy in range(-20, 21):
		for dx in range(-10, 11):
			var dist := sqrt(dx * dx * 0.3 + dy * dy) / 20.0
			if dist < 1.0:
				var alpha := (1.0 - dist) * 0.8
				img.set_pixel(15 + dx, 25 + dy, Color(0.5, 0.8, 0.5, alpha))
	for dy in range(-5, 0):
		for dx in range(-3, 4):
			img.set_pixel(15 + dx, 8 + dy, Color(1.0, 0.9, 0.5, 1.0))
	sprite.texture = ImageTexture.create_from_image(img)
	glow = PointLight2D.new()
	glow.color = Color(0.5, 0.8, 0.5, 0.3)
	glow.energy = 0.3
	add_child(glow)

func _on_body_entered(body: Node2D) -> void:
	if body is PlayerCharacter and not is_activated:
		_activate()

func _activate() -> void:
	is_activated = true
	glow.color = Color(1.0, 0.9, 0.5, 0.6)
	glow.energy = 0.8
	checkpoint_activated.emit(checkpoint_id)
	if sprite:
		var img := Image.create(30, 50, false, Image.FORMAT_RGBA8)
		img.fill(Color(0, 0, 0, 0))
		for dy in range(-20, 21):
			for dx in range(-10, 11):
				var dist := sqrt(dx * dx * 0.3 + dy * dy) / 20.0
				if dist < 1.0:
					var alpha := (1.0 - dist) * 0.9
					img.set_pixel(15 + dx, 25 + dy, Color(1.0, 0.9, 0.5, alpha))
		for dy in range(-8, -2):
			for dx in range(-4, 5):
				if abs(dx) < 3:
					img.set_pixel(15 + dx, 8 + dy, Color(1.0, 1.0, 0.8, 1.0))
		sprite.texture = ImageTexture.create_from_image(img)

func get_spawn_position() -> Vector2:
	return global_position + Vector2(0, -20)
