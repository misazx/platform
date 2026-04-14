extends Node2D

var _velocity := Vector2.ZERO
var _life := 0.0
var _max_life := 8.0
var _color := Color.WHITE
var _size := 2.0
var _bob_phase := 0.0

func _ready() -> void:
	_velocity = Vector2(randf_range(-15, 15), randf_range(-25, -5))
	_life = randf_range(3.0, _max_life)
	_max_life = _life
	_bob_phase = randf() * TAU
	_size = randf_range(1.5, 3.5)
	_color = Color(randf_range(0.3, 0.8), randf_range(0.3, 0.8), randf_range(0.3, 0.8), randf_range(0.2, 0.5))

func _process(delta: float) -> void:
	_life -= delta
	if _life <= 0:
		_reset_particle()
		return
	_bob_phase += delta * 1.5
	position += _velocity * delta
	position.y += sin(_bob_phase) * 0.5

func _draw() -> void:
	var alpha: float = _color.a * min(_life / 2.0, 1.0)
	draw_circle(Vector2.ZERO, _size, Color(_color.r, _color.g, _color.b, alpha))

func _reset_particle() -> void:
	position = Vector2(randf_range(-500, 2000), randf_range(-300, 700))
	_velocity = Vector2(randf_range(-15, 15), randf_range(-25, -5))
	_life = randf_range(3.0, _max_life)
	_max_life = _life
