class_name ShadowTrap
extends Area2D

enum TrapType { DAMAGE, SLOW, TELEPORT, REVEAL }

@export var trap_type: TrapType = TrapType.DAMAGE
@export var damage_amount := 1
@export var slow_factor := 0.3
@export var teleport_target := Vector2.ZERO
@export var trigger_delay := 0.3
@export var cooldown_time := 3.0
@export var is_visible := true

var _is_triggered := false
var _cooldown_timer := 0.0
var _trigger_timer := 0.0
var _sprite: Sprite2D
var _collision: CollisionShape2D
var _glow: PointLight2D

func _ready() -> void:
	_setup_visuals()
	body_entered.connect(_on_body_entered)

func _setup_visuals() -> void:
	_collision = CollisionShape2D.new()
	var shape := RectangleShape2D.new()
	shape.size = Vector2(30, 10)
	_collision.shape = shape
	add_child(_collision)
	_sprite = Sprite2D.new()
	add_child(_sprite)
	var img := Image.create(30, 10, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	for y in range(10):
		for x in range(30):
			var alpha := 0.3 + 0.2 * sin(x * 0.5)
			img.set_pixel(x, y, Color(0.3, 0.1, 0.5, alpha))
	_sprite.texture = ImageTexture.create_from_image(img)
	_sprite.visible = is_visible
	_glow = PointLight2D.new()
	_glow.color = Color(0.3, 0.1, 0.5, 0.3)
	_glow.energy = 0.3
	add_child(_glow)

func _process(delta: float) -> void:
	if _cooldown_timer > 0:
		_cooldown_timer -= delta
		if _cooldown_timer <= 0:
			_is_triggered = false
			if _sprite:
				_sprite.modulate.a = 1.0
	if _is_triggered and _trigger_timer > 0:
		_trigger_timer -= delta

func _on_body_entered(body: Node2D) -> void:
	if _is_triggered or _trigger_timer > 0:
		return
	if not body is PlayerCharacter:
		return
	var player := body as PlayerCharacter
	if player.is_stealth_active():
		return
	_trigger_timer = trigger_delay
	_activate_trap(player)

func _activate_trap(player: PlayerCharacter) -> void:
	_is_triggered = true
	_cooldown_timer = cooldown_time
	if _sprite:
		_sprite.modulate.a = 0.3
	match trap_type:
		TrapType.DAMAGE:
			player.take_damage(damage_amount)
			ParticleEffect.spawn_at(get_parent(), global_position, ParticleEffect.EffectType.DAMAGE_TAKEN, 10)
		TrapType.SLOW:
			player.velocity *= slow_factor
		TrapType.TELEPORT:
			player.global_position = teleport_target
			ParticleEffect.spawn_at(get_parent(), global_position, ParticleEffect.EffectType.FORM_SWITCH_SHADOW, 15)
		TrapType.REVEAL:
			_reveal_nearby_fragments()

func _reveal_nearby_fragments() -> void:
	for child in get_parent().get_children():
		if child is MemoryFragment and child.is_hidden:
			var dist := global_position.distance_to(child.global_position)
			if dist < 300:
				child.reveal()
