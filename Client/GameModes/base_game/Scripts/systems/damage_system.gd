extends Node
enum DamageType { PHYSICAL, MAGICAL, FIRE, ICE, LIGHTNING, POISON, TRUE }


signal damage_dealt(source: Node, target: Node, amount: int)
signal damage_taken(target: Node, amount: int)
signal critical_hit(source: Node, target: Node, amount: int)

@export var critical_multiplier: float = 2.0
@export var critical_chance: float = 0.1
@export var knockback_multiplier: float = 1.0

var _type_resistances: Dictionary = {}
var _damage_modifiers: Array = []

func calculate_damage(info: Dictionary) -> Dictionary:
	var result := {
		"original_damage": info.get("amount", 0),
		"final_damage": info.get("amount", 0),
		"damage_blocked": 0,
		"was_critical": false,
		"was_dodged": false,
		"killed_target": false,
		"modifiers": {}
	}

	var target: Node = info.get("target")
	if target == null:
		return result

	var final_damage := float(info.get("amount", 0))
	var source: Node = info.get("source")

	if source != null and source.has_method("get_critical_chance"):
		var crit_chance: float = source.call("get_critical_chance")
		if randf() < crit_chance:
			info["is_critical"] = true
			result["was_critical"] = true
			final_damage *= critical_multiplier

	if target.has_method("get_defense"):
		var defense: int = target.call("get_defense")
		var damage_reduction := defense / (defense + 100.0)
		var blocked := int(round(final_damage * damage_reduction))
		result["damage_blocked"] = blocked
		final_damage -= blocked

	if target.has_method("get_resistance"):
		var resistance: float = target.call("get_resistance", int(info.get("type", DamageType.PHYSICAL)))
		final_damage *= (1.0 - resistance)

	if target.has_method("get_dodge_chance"):
		var dodge_chance: float = target.call("get_dodge_chance")
		if randf() < dodge_chance:
			result["was_dodged"] = true
			final_damage = 0

	final_damage = max(1.0, final_damage)
	result["final_damage"] = int(round(final_damage))
	return result

func apply_damage(info: Dictionary) -> Dictionary:
	var target: Node = info.get("target")
	if target == null:
		push_error("[DamageSystem] Target is null")
		return {"original_damage": 0, "final_damage": 0, "damage_blocked": 0, "was_critical": false, "was_dodged": false, "killed_target": false, "modifiers": {}}

	var result := calculate_damage(info)
	if result["was_dodged"]:
		print("[DamageSystem] Attack dodged!")
		return result

	if target.has_method("take_damage"):
		target.call("take_damage", result["final_damage"])
	elif target is Node and "current_health" in target:
		var current_health: int = target["current_health"]
		var new_health := maxi(0, current_health - result["final_damage"])
		target["current_health"] = new_health
		if new_health <= 0:
			result["killed_target"] = true
			if target.has_method("die"):
				target.call("die")

	damage_dealt.emit(info.get("source"), target, result["final_damage"])
	damage_taken.emit(target, result["final_damage"])

	if result["was_critical"]:
		critical_hit.emit(info.get("source"), target, result["final_damage"])

	print("[DamageSystem] %d -> %d damage (blocked: %d, crit: %s)" % [result["original_damage"], result["final_damage"], result["damage_blocked"], result["was_critical"]])
	return result

func create_damage_info(amount: int, type_val: int = DamageType.PHYSICAL, source: Node = null) -> Dictionary:
	return {
		"amount": amount,
		"type": type_val,
		"source": source,
		"target": null,
		"is_critical": false,
		"knockback_force": 0.0,
		"knockback_direction": Vector2.ZERO,
		"custom_data": {}
	}

func add_damage_modifier(modifier_id: String) -> void:
	if modifier_id not in _damage_modifiers:
		_damage_modifiers.append(modifier_id)
		print("[DamageSystem] Added damage modifier: %s" % modifier_id)

func remove_damage_modifier(modifier_id: String) -> void:
	_damage_modifiers.erase(modifier_id)
	print("[DamageSystem] Removed damage modifier: %s" % modifier_id)

func get_damage_multiplier(type_val: int) -> float:
	return 1.0
