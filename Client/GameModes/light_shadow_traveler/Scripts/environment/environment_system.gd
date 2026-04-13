class_name EnvironmentSystem
extends Node2D

enum EnvTheme { FOREST, STUDIO, CONCERT_HALL, LIBRARY, TEMPLE }

@export var current_theme: EnvTheme = EnvTheme.FOREST

var _bg_layers: Array[ParallaxLayer] = []
var _env_particles: Array[Node2D] = []
var _light_zones: Array[Area2D] = []
var _shadow_zones: Array[Area2D] = []
var _time_of_day := 0.0
var _ambient_light: PointLight2D

func _ready() -> void:
	_setup_environment()

func _setup_environment() -> void:
	var parallax := ParallaxBackground.new()
	parallax.name = "EnvParallax"
	add_child(parallax)
	match current_theme:
		EnvTheme.FOREST:
			_create_forest_bg(parallax)
		EnvTheme.STUDIO:
			_create_studio_bg(parallax)
		EnvTheme.CONCERT_HALL:
			_create_concert_bg(parallax)
		EnvTheme.LIBRARY:
			_create_library_bg(parallax)
		EnvTheme.TEMPLE:
			_create_temple_bg(parallax)
	_ambient_light = PointLight2D.new()
	_ambient_light.name = "AmbientLight"
	_ambient_light.energy = 0.3
	add_child(_ambient_light)
	_spawn_env_particles()

func _create_forest_bg(parallax: ParallaxBackground) -> void:
	var layer1 := _make_parallax_layer(Color(0.1, 0.15, 0.25), 0.1, Vector2(0, -100))
	parallax.add_child(layer1)
	_bg_layers.append(layer1)
	var layer2 := _make_parallax_layer(Color(0.05, 0.12, 0.08), 0.3, Vector2(0, -50))
	parallax.add_child(layer2)
	_bg_layers.append(layer2)
	var layer3 := _make_parallax_layer(Color(0.08, 0.18, 0.1), 0.5, Vector2(0, 0))
	parallax.add_child(layer3)
	_bg_layers.append(layer3)

func _create_studio_bg(parallax: ParallaxBackground) -> void:
	var layer1 := _make_parallax_layer(Color(0.15, 0.12, 0.1), 0.1, Vector2(0, -100))
	parallax.add_child(layer1)
	_bg_layers.append(layer1)
	var layer2 := _make_parallax_layer(Color(0.18, 0.14, 0.12), 0.3, Vector2(0, -50))
	parallax.add_child(layer2)
	_bg_layers.append(layer2)

func _create_concert_bg(parallax: ParallaxBackground) -> void:
	var layer1 := _make_parallax_layer(Color(0.08, 0.06, 0.12), 0.1, Vector2(0, -100))
	parallax.add_child(layer1)
	_bg_layers.append(layer1)
	var layer2 := _make_parallax_layer(Color(0.1, 0.08, 0.15), 0.3, Vector2(0, -50))
	parallax.add_child(layer2)
	_bg_layers.append(layer2)

func _create_library_bg(parallax: ParallaxBackground) -> void:
	var layer1 := _make_parallax_layer(Color(0.12, 0.1, 0.08), 0.1, Vector2(0, -100))
	parallax.add_child(layer1)
	_bg_layers.append(layer1)
	var layer2 := _make_parallax_layer(Color(0.15, 0.12, 0.1), 0.3, Vector2(0, -50))
	parallax.add_child(layer2)
	_bg_layers.append(layer2)

func _create_temple_bg(parallax: ParallaxBackground) -> void:
	var layer1 := _make_parallax_layer(Color(0.1, 0.1, 0.15), 0.1, Vector2(0, -100))
	parallax.add_child(layer1)
	_bg_layers.append(layer1)
	var layer2 := _make_parallax_layer(Color(0.15, 0.12, 0.2), 0.3, Vector2(0, -50))
	parallax.add_child(layer2)
	_bg_layers.append(layer2)
	var layer3 := _make_parallax_layer(Color(0.2, 0.18, 0.25), 0.5, Vector2(0, 0))
	parallax.add_child(layer3)
	_bg_layers.append(layer3)

func _make_parallax_layer(color: Color, motion_scale: float, offset: Vector2) -> ParallaxLayer:
	var layer := ParallaxLayer.new()
	layer.motion_scale = Vector2(motion_scale, motion_scale * 0.3)
	layer.motion_offset = offset
	var rect := ColorRect.new()
	rect.color = color
	rect.size = Vector2(4000, 1200)
	rect.position = Vector2(-1000, -600)
	rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	layer.add_child(rect)
	return layer

func _spawn_env_particles() -> void:
	match current_theme:
		EnvTheme.FOREST:
			_spawn_floating_particles(Color(0.3, 0.6, 0.3, 0.4), 30, 20.0, 60.0)
		EnvTheme.STUDIO:
			_spawn_floating_particles(Color(0.8, 0.7, 0.5, 0.3), 20, 15.0, 40.0)
		EnvTheme.CONCERT_HALL:
			_spawn_floating_particles(Color(0.5, 0.4, 0.7, 0.3), 15, 10.0, 30.0)
		EnvTheme.LIBRARY:
			_spawn_floating_particles(Color(0.7, 0.6, 0.4, 0.3), 25, 8.0, 25.0)
		EnvTheme.TEMPLE:
			_spawn_floating_particles(Color(0.6, 0.5, 0.8, 0.4), 35, 25.0, 70.0)

func _spawn_floating_particles(color: Color, count: int, min_speed: float, max_speed: float) -> void:
	for i in count:
		var particle := _create_env_particle(color, min_speed, max_speed)
		add_child(particle)
		_env_particles.append(particle)

func _create_env_particle(color: Color, min_speed: float, max_speed: float) -> Node2D:
	var node := Node2D.new()
	node.set_script(preload("res://GameModes/light_shadow_traveler/Scripts/environment/env_particle.gd"))
	node.position = Vector2(randf_range(-500, 2000), randf_range(-300, 700))
	return node

func add_light_zone(pos: Vector2, radius: float, energy: float) -> void:
	var zone := Area2D.new()
	var shape := CircleShape2D.new()
	shape.radius = radius
	var col := CollisionShape2D.new()
	col.shape = shape
	zone.add_child(col)
	zone.position = pos
	var light := PointLight2D.new()
	light.color = Color(1.0, 0.95, 0.8, 0.3)
	light.energy = energy
	light.texture = _make_zone_glow()
	zone.add_child(light)
	add_child(zone)
	_light_zones.append(zone)

func add_shadow_zone(pos: Vector2, radius: float) -> void:
	var zone := Area2D.new()
	var shape := CircleShape2D.new()
	shape.radius = radius
	var col := CollisionShape2D.new()
	col.shape = shape
	zone.add_child(col)
	zone.position = pos
	var dark := PointLight2D.new()
	dark.color = Color(0.1, 0.1, 0.2, 0.5)
	dark.energy = -1.5
	dark.texture = _make_zone_glow()
	zone.add_child(dark)
	add_child(zone)
	_shadow_zones.append(zone)

func _make_zone_glow() -> Texture2D:
	var img := Image.create(64, 64, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	for dy in range(-32, 32):
		for dx in range(-32, 32):
			var dist := sqrt(dx * dx + dy * dy) / 32.0
			if dist < 1.0:
				img.set_pixel(32 + dx, 32 + dy, Color(1, 1, 1, (1.0 - dist) * 0.5))
	return ImageTexture.create_from_image(img)

func _process(delta: float) -> void:
	_time_of_day += delta * 0.02
	if _ambient_light:
		match current_theme:
			EnvTheme.FOREST:
				_ambient_light.color = Color(0.3, 0.5, 0.3, 0.15 + sin(_time_of_day) * 0.05)
			EnvTheme.STUDIO:
				_ambient_light.color = Color(0.5, 0.4, 0.3, 0.1)
			EnvTheme.CONCERT_HALL:
				_ambient_light.color = Color(0.2, 0.15, 0.3, 0.08)
			EnvTheme.LIBRARY:
				_ambient_light.color = Color(0.4, 0.35, 0.25, 0.1)
			EnvTheme.TEMPLE:
				_ambient_light.color = Color(0.3, 0.3, 0.5, 0.12 + sin(_time_of_day * 0.5) * 0.04)

func setup_from_chapter(chapter_id: String) -> void:
	match chapter_id:
		"forgotten_forest":
			current_theme = EnvTheme.FOREST
		"faded_studio":
			current_theme = EnvTheme.STUDIO
		"silent_concert_hall":
			current_theme = EnvTheme.CONCERT_HALL
		"sleeping_library":
			current_theme = EnvTheme.LIBRARY
		"light_shadow_temple":
			current_theme = EnvTheme.TEMPLE
		_:
			current_theme = EnvTheme.FOREST
