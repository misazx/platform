class_name ParticleEffect
extends Node2D

enum EffectType { FORM_SWITCH_LIGHT, FORM_SWITCH_SHADOW, FRAGMENT_COLLECT, CHECKPOINT_ACTIVATE, DAMAGE_TAKEN, DEATH, HEAL, ENEMY_DEATH }

@export var effect_type: EffectType = EffectType.FORM_SWITCH_LIGHT
@export var particle_count := 20
@export var lifetime := 1.0

var _particles: Array[Dictionary] = []
var _elapsed := 0.0

func _ready() -> void:
	_spawn_particles()

func _spawn_particles() -> void:
	for i in range(particle_count):
		var particle_data := _create_particle(i)
		_particles.append(particle_data)

func _create_particle(index: int) -> Dictionary:
	var angle := randf() * TAU
	var speed := randf_range(50.0, 200.0)
	var color := Color.WHITE
	var size := randf_range(2.0, 5.0)
	match effect_type:
		EffectType.FORM_SWITCH_LIGHT:
			color = Color(1.0, 0.95, 0.7, randf_range(0.5, 1.0))
			speed = randf_range(30.0, 120.0)
		EffectType.FORM_SWITCH_SHADOW:
			color = Color(0.3, 0.35, 0.7, randf_range(0.5, 1.0))
			speed = randf_range(30.0, 120.0)
		EffectType.FRAGMENT_COLLECT:
			color = Color(0.8, 0.7, 1.0, randf_range(0.6, 1.0))
			speed = randf_range(60.0, 180.0)
		EffectType.CHECKPOINT_ACTIVATE:
			color = Color(1.0, 0.9, 0.5, randf_range(0.5, 1.0))
			speed = randf_range(40.0, 100.0)
		EffectType.DAMAGE_TAKEN:
			color = Color(1.0, 0.3, 0.3, randf_range(0.5, 1.0))
			speed = randf_range(80.0, 200.0)
		EffectType.DEATH:
			color = Color(0.5, 0.5, 0.5, randf_range(0.3, 0.8))
			speed = randf_range(20.0, 80.0)
		EffectType.HEAL:
			color = Color(0.3, 1.0, 0.5, randf_range(0.5, 1.0))
			speed = randf_range(30.0, 100.0)
		EffectType.ENEMY_DEATH:
			color = Color(0.9, 0.3, 0.2, randf_range(0.6, 1.0))
			speed = randf_range(50.0, 150.0)
	return {
		"pos": Vector2.ZERO,
		"vel": Vector2(cos(angle), sin(angle)) * speed,
		"color": color,
		"size": size,
		"life": randf_range(lifetime * 0.5, lifetime),
		"max_life": lifetime
	}

func _process(delta: float) -> void:
	_elapsed += delta
	var all_dead := true
	for p in _particles:
		if p.life > 0:
			all_dead = false
			p.life -= delta
			p.pos += p.vel * delta
			p.vel *= 0.97
			p.size *= 0.98
	if all_dead:
		queue_free()

func _draw() -> void:
	for p in _particles:
		if p.life > 0:
			var alpha_val: float = float(p.life) / float(p.max_life)
			var draw_color := Color(p.color.r, p.color.g, p.color.b, p.color.a * alpha_val)
			draw_circle(p.pos, p.size, draw_color)

func spawn_effect(parent: Node2D, pos: Vector2, type: EffectType, count: int = 20) -> ParticleEffect:
	var effect := ParticleEffect.new()
	effect.effect_type = type
	effect.particle_count = count
	effect.global_position = pos
	parent.add_child(effect)
	return effect

static func create_and_spawn(parent: Node2D, pos: Vector2, type: EffectType, count: int = 20) -> ParticleEffect:
	var helper := ParticleEffect.new()
	return helper.spawn_effect(parent, pos, type, count)
