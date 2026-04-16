class_name FormPlatform
extends AnimatableBody2D

enum PlatformType { NORMAL, LIGHT, SHADOW, SHADOW_WALL }

@export var platform_type: PlatformType = PlatformType.NORMAL
@export var platform_width := 200.0
@export var platform_height := 40.0
@export var platform_id := ""

var is_active := true
var _original_color: Color
var _inactive_color := Color(0.3, 0.3, 0.3, 0.3)
var _light_glow_tex: Texture2D
var _shadow_glow_tex: Texture2D

var collision_shape: CollisionShape2D
var visual: ColorRect
var glow: PointLight2D

func _ready() -> void:
	_setup_platform()
	_update_visuals()

func _setup_platform() -> void:
	if not collision_shape:
		collision_shape = CollisionShape2D.new()
		collision_shape.name = "CollisionShape2D"
		add_child(collision_shape)
	var shape := RectangleShape2D.new()
	shape.size = Vector2(platform_width, platform_height)
	collision_shape.shape = shape
	collision_shape.one_way_collision = false
	if not visual:
		visual = ColorRect.new()
		visual.name = "Visual"
		add_child(visual)
	visual.size = Vector2(platform_width, platform_height)
	visual.position = Vector2(-platform_width / 2, -platform_height / 2)
	visual.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var plat_path: String = ""
	match platform_type:
		PlatformType.NORMAL:
			_original_color = Color(0.5, 0.5, 0.5, 1.0)
			plat_path = "res://GameModes/light_shadow_traveler/Resources/Platforms/normal_platform.png"
		PlatformType.LIGHT:
			_original_color = Color(1.0, 0.95, 0.7, 0.9)
			plat_path = "res://GameModes/light_shadow_traveler/Resources/Platforms/light_platform.png"
		PlatformType.SHADOW:
			_original_color = Color(0.3, 0.35, 0.6, 0.85)
			plat_path = "res://GameModes/light_shadow_traveler/Resources/Platforms/shadow_platform.png"
		PlatformType.SHADOW_WALL:
			_original_color = Color(0.25, 0.28, 0.5, 0.9)
			plat_path = "res://GameModes/light_shadow_traveler/Resources/Platforms/shadow_platform.png"
	if plat_path != "" and ResourceLoader.exists(plat_path):
		var tex: Texture2D = load(plat_path) as Texture2D
		if tex:
			var plat_visual := TextureRect.new()
			plat_visual.name = "PlatformSprite"
			plat_visual.texture = tex
			plat_visual.stretch_mode = TextureRect.STRETCH_SCALE
			plat_visual.size = Vector2(platform_width, platform_height)
			plat_visual.position = Vector2(-platform_width / 2, -platform_height / 2)
			plat_visual.mouse_filter = Control.MOUSE_FILTER_IGNORE
			add_child(plat_visual)
			visual.visible = false
	if not glow:
		glow = PointLight2D.new()
		glow.name = "PlatformGlow"
		add_child(glow)

func _process(_delta: float) -> void:
	_update_activity()
	_update_visuals()

func _update_activity() -> void:
	match platform_type:
		PlatformType.NORMAL:
			is_active = true
		PlatformType.LIGHT:
			var player := _get_player()
			is_active = player != null and player.is_light_form()
		PlatformType.SHADOW:
			var player := _get_player()
			is_active = player != null and player.is_shadow_form()
		PlatformType.SHADOW_WALL:
			var player := _get_player()
			is_active = player != null and player.is_shadow_form()
	collision_shape.disabled = not is_active

func _update_visuals() -> void:
	var plat_visual: TextureRect = null
	for child in get_children():
		if child is TextureRect and child.name == "PlatformSprite":
			plat_visual = child as TextureRect
			break
	if plat_visual:
		plat_visual.visible = is_active
		plat_visual.modulate.a = 1.0 if is_active else 0.3
	if visual and visual.visible:
		if is_active:
			visual.color = _original_color
			visual.modulate.a = 1.0
		else:
			visual.color = _inactive_color
			visual.modulate.a = 0.3
	if glow and is_instance_valid(glow):
		if is_active:
			match platform_type:
				PlatformType.LIGHT:
					glow.color = Color(1.0, 0.95, 0.7, 0.4)
					glow.energy = 0.8
					if _light_glow_tex == null:
						_light_glow_tex = _make_glow_tex(Color(1.0, 0.95, 0.7))
					if _light_glow_tex:
						glow.texture = _light_glow_tex
				PlatformType.SHADOW, PlatformType.SHADOW_WALL:
					glow.color = Color(0.3, 0.35, 0.7, 0.3)
					glow.energy = 0.5
					if _shadow_glow_tex == null:
						_shadow_glow_tex = _make_glow_tex(Color(0.3, 0.35, 0.7))
					if _shadow_glow_tex:
						glow.texture = _shadow_glow_tex
				_:
					glow.energy = 0.0
		else:
			glow.energy = 0.0

func _make_glow_tex(color: Color) -> Texture2D:
	var img := Image.create(48, 48, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	for dy in range(-24, 24):
		for dx in range(-24, 24):
			var dist := sqrt(dx * dx + dy * dy) / 24.0
			if dist < 1.0:
				img.set_pixel(24 + dx, 24 + dy, Color(color.r, color.g, color.b, (1.0 - dist) * 0.4))
	return ImageTexture.create_from_image(img)

func _get_player() -> PlayerCharacter:
	var players := get_tree().get_nodes_in_group("player")
	if players.size() > 0:
		return players[0] as PlayerCharacter
	return null

func set_active(active: bool) -> void:
	is_active = active
	if collision_shape:
		collision_shape.disabled = not active
	if active:
		platform_type = PlatformType.NORMAL
		_original_color = Color(0.7, 0.8, 0.7, 0.9)
	_update_visuals()

func setup_from_data(data: Dictionary) -> void:
	position = Vector2(data.get("x", 0), data.get("y", 0))
	platform_width = data.get("w", 200)
	platform_height = data.get("h", 40)
	var type_str: String = data.get("type", "normal")
	match type_str:
		"light":
			platform_type = PlatformType.LIGHT
		"shadow":
			platform_type = PlatformType.SHADOW
		"shadow_wall":
			platform_type = PlatformType.SHADOW_WALL
		_:
			platform_type = PlatformType.NORMAL
	_setup_platform()
	_update_visuals()
