class_name MovableLightSource
extends RigidBody2D

signal light_moved(source: MovableLightSource)
signal shadow_platforms_updated

@export var light_radius := 150.0
@export var light_color := Color(1.0, 0.95, 0.8, 0.6)
@export var light_energy := 1.0
@export var is_pushable := true

var is_being_pushed := false
var _push_direction := 0.0

@onready var light: PointLight2D = $LightSource
@onready var sprite: Sprite2D = $Sprite2D
@onready var interaction_area: Area2D = $InteractionArea

func _ready() -> void:
	_setup_visuals()
	_setup_light()
	_setup_physics()

func _setup_visuals() -> void:
	if not sprite:
		sprite = Sprite2D.new()
		add_child(sprite)
	var img := Image.create(28, 36, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	for dy in range(4):
		for dx in range(-6, 7):
			img.set_pixel(14 + dx, 28 + dy, Color(0.6, 0.5, 0.4, 1.0))
	for dy in range(-8, 1):
		for dx in range(-4, 5):
			var dist := abs(dx) / 4.0
			if dist < 1.0:
				img.set_pixel(14 + dx, 20 + dy, Color(0.7, 0.65, 0.55, 1.0 - dist * 0.3))
	for dy in range(-16, -8):
		for dx in range(-6, 7):
			var dist := sqrt(dx * dx + (dy + 12) * (dy + 12)) / 8.0
			if dist < 1.0:
				var alpha := (1.0 - dist) * 0.9
				img.set_pixel(14 + dx, 20 + dy, Color(1.0, 0.95, 0.7, alpha))
	sprite.texture = ImageTexture.create_from_image(img)

func _setup_light() -> void:
	if not light:
		light = PointLight2D.new()
		light.name = "LightSource"
		add_child(light)
	light.color = light_color
	light.energy = light_energy
	light.texture = _create_light_texture()
	light.height = 10

func _create_light_texture() -> Texture2D:
	var size := int(light_radius * 2)
	var img := Image.create(size, size, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	var center := size / 2
	for dy in range(-center, center):
		for dx in range(-center, center):
			var dist := sqrt(dx * dx + dy * dy) / float(center)
			if dist < 1.0:
				var alpha := (1.0 - dist) * 0.6
				img.set_pixel(center + dx, center + dy, Color(light_color.r, light_color.g, light_color.b, alpha))
	return ImageTexture.create_from_image(img)

func _setup_physics() -> void:
	if not interaction_area:
		interaction_area = Area2D.new()
		interaction_area.name = "InteractionArea"
		add_child(interaction_area)
		var shape_node := CollisionShape2D.new()
		var shape := CircleShape2D.new()
		shape.radius = 30.0
		shape_node.shape = shape
		interaction_area.add_child(shape_node)
	gravity_scale = 0.0
	linear_damp = 5.0
	contact_monitor = true
	max_contacts_reported = 4

func _physics_process(delta: float) -> void:
	if is_being_pushed and is_pushable:
		velocity.x = _push_direction * 100.0
		velocity.y = 0
	else:
		velocity.x = lerp(velocity.x, 0.0, delta * 5.0)
	if is_pushable:
		move_and_slide()
		if velocity.length_squared() > 1.0:
			light_moved.emit(self)
			_update_shadow_platforms()

func _update_shadow_platforms() -> void:
	shadow_platforms_updated.emit()
	var shadow_platforms := get_tree().get_nodes_in_group("shadow_platform")
	for platform in shadow_platforms:
		if platform.has_method("update_shadow_from_light"):
			platform.update_shadow_from_light(global_position, light_radius)

func start_push(direction: float) -> void:
	if is_pushable:
		is_being_pushed = true
		_push_direction = direction

func stop_push() -> void:
	is_being_pushed = false
	_push_direction = 0.0

func setup_from_data(data: Dictionary) -> void:
	position = Vector2(data.get("x", 0), data.get("y", 0))
	light_radius = data.get("radius", 150.0)
	light_energy = data.get("energy", 1.0)
	_setup_light()
