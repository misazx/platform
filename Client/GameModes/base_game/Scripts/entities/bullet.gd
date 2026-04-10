class_name Bullet extends Area2D

signal hit_target(target: Node)

@export var speed: float = 400.0
@export var damage: int = 10
@export var lifetime: float = 3.0
@export var direction: Vector2 = Vector2.RIGHT

var _timer := 0.0
var _shooter: Node

func _ready() -> void:
	body_entered.connect(_on_body_entered)

func init(shooter_node: Node, dir: Vector2, dmg: int) -> void:
	_shooter = shooter_node
	direction = dir.normalized()
	damage = dmg
	rotation = direction.angle()
	_timer = 0.0

func _process(delta: float) -> void:
	position += direction * speed * delta
	_timer += delta
	if _timer >= lifetime:
		queue_free()

func _on_body_entered(body: Node) -> void:
	if body != _shooter and body.has_method("take_damage"):
		body.call("take_damage", damage)
		hit_target.emit(body)
		queue_free()
