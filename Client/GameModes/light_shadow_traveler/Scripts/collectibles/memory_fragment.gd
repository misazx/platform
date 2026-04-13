class_name MemoryFragment
extends Area2D

signal collected(fragment: MemoryFragment)

@export var fragment_id := ""
@export var is_hidden := false

var is_collected := false
var _bob_time := 0.0
var _initial_y := 0.0
var sprite: Sprite2D
var glow: PointLight2D
var collect_area: CollisionShape2D

func _ready() -> void:
	_initial_y = position.y
	_setup_visuals()
	body_entered.connect(_on_body_entered)
	if is_hidden:
		visible = false
		monitoring = false

func _setup_visuals() -> void:
	sprite = Sprite2D.new()
	add_child(sprite)
	var img := Image.create(24, 24, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	for dy in range(-10, 11):
		for dx in range(-10, 11):
			var dist := sqrt(dx * dx + dy * dy)
			if dist < 10:
				var alpha := (1.0 - dist / 10.0) * 0.9
				var r := 0.8 + 0.2 * sin(dx * 0.5)
				var g := 0.7 + 0.3 * cos(dy * 0.3)
				var b := 1.0
				img.set_pixel(12 + dx, 12 + dy, Color(r, g, b, alpha))
	for i in range(5):
		var angle := i * TAU / 5.0 - PI / 2.0
		var px := 12 + int(cos(angle) * 7)
		var py := 12 + int(sin(angle) * 7)
		for dy in range(-1, 2):
			for dx in range(-1, 2):
				if px + dx >= 0 and px + dx < 24 and py + dy >= 0 and py + dy < 24:
					img.set_pixel(px + dx, py + dy, Color(1, 0.9, 0.5, 1.0))
	sprite.texture = ImageTexture.create_from_image(img)
	glow = PointLight2D.new()
	glow.name = "FragmentGlow"
	add_child(glow)
	glow.color = Color(0.8, 0.7, 1.0, 0.5)
	glow.energy = 0.8
	glow.texture = _make_glow()
	collect_area = CollisionShape2D.new()
	collect_area.name = "CollectArea"
	add_child(collect_area)
	var shape := CircleShape2D.new()
	shape.radius = 15.0
	collect_area.shape = shape

func _make_glow() -> Texture2D:
	var img := Image.create(32, 32, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	for dy in range(-16, 16):
		for dx in range(-16, 16):
			var dist := sqrt(dx * dx + dy * dy) / 16.0
			if dist < 1.0:
				img.set_pixel(16 + dx, 16 + dy, Color(0.8, 0.7, 1.0, (1.0 - dist) * 0.4))
	return ImageTexture.create_from_image(img)

func _process(delta: float) -> void:
	if is_collected:
		return
	_bob_time += delta * 2.0
	position.y = _initial_y + sin(_bob_time) * 5.0
	if glow:
		glow.energy = 0.6 + sin(_bob_time * 1.5) * 0.3

func _on_body_entered(body: Node2D) -> void:
	if is_collected:
		return
	if body is PlayerCharacter:
		is_collected = true
		collected.emit(self)
		var player := body as PlayerCharacter
		player.collect_fragment()
		_play_collect_animation()

func _play_collect_animation() -> void:
	var tween := create_tween()
	tween.set_parallel(true)
	tween.tween_property(self, "scale", Vector2(1.5, 1.5), 0.2)
	tween.tween_property(self, "modulate:a", 0.0, 0.3)
	tween.chain().tween_callback(queue_free)

func reveal() -> void:
	if is_hidden and not is_collected:
		is_hidden = false
		visible = true
		monitoring = true
		var tween := create_tween()
		modulate.a = 0.0
		tween.tween_property(self, "modulate:a", 1.0, 0.5)
