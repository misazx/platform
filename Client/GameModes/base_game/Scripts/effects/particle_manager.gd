extends Node

const MAX_PARTICLES: int = 500

var _particles: Array = []
var _particle_pool: Array = []

func _ready() -> void:
	pass

func emit(position: Vector2, config: Dictionary) -> void:
	var count: int = config.get("count", 10)
	var color: Color = config.get("color", Color.WHITE)
	var size_range: Vector2 = config.get("size_range", Vector2(2, 6))
	var lifetime_range: Vector2 = config.get("lifetime_range", Vector2(0.3, 1.0))
	var spread: float = config.get("spread", 360.0)
	var speed_range: Vector2 = config.get("speed_range", Vector2(50, 150))

	for i: int in range(count):
		var particle: Dictionary = create_particle(position, color, size_range, lifetime_range, spread, speed_range)
		_particles.append(particle)

func create_particle(pos: Vector2, col: Color, size_rng: Vector2, life_rng: Vector2, spr: float, spd_rng: Vector2) -> Dictionary:
	var particle: Dictionary = {
		"node": null,
		"position": pos,
		"color": col,
		"size": randf_range(size_rng.x, size_rng.y),
		"lifetime": randf_range(life_rng.x, life_rng.y),
		"age": 0.0,
		"velocity": Vector2.from_angle(deg_to_rad(randf_range(-spr / 2.0, spr / 2.0))) * randf_range(spd_rng.x, spd_rng.y),
		"active": true
	}
	return particle

func emit_damage_numbers(position: Vector2, amount: int, is_crit: bool = false) -> void:
	emit(position, {
		"count": 1,
		"color": Color.RED if not is_crit else Color.ORANGE,
		"size_range": Vector2(12 if is_crit else 8, 20 if is_crit else 14),
		"lifetime_range": Vector2(0.8, 1.2),
		"spread": 0,
		"speed_range": Vector2(0, 0)
	})

func emit_heal_effect(position: Vector2, amount: int) -> void:
	emit(position, {
		"count": 8,
		"color": Color.GREEN,
		"size_range": Vector2(3, 6),
		"lifetime_range": Vector2(0.4, 0.8),
		"spread": 180,
		"speed_range": Vector2(30, 80)
	})

func emit_block_effect(position: Vector2, amount: int) -> void:
	emit(position, {
		"count": 6,
		"color": Color.CYAN,
		"size_range": Vector2(4, 8),
		"lifetime_range": Vector2(0.3, 0.6),
		"spread": 90,
		"speed_range": Vector2(20, 60)
	})

func _process(delta: float) -> void:
	var i: int = 0
	while i < _particles.size():
		var p: Dictionary = _particles[i]
		p["age"] = p["age"] + delta
		p["position"] = p["position"] + p["velocity"] * delta
		p["velocity"] = p["velocity"] * 0.95
		if p["age"] >= p["lifetime"]:
			_particles.remove_at(i)
		else:
			i += 1

func clear_all() -> void:
	_particles.clear()
