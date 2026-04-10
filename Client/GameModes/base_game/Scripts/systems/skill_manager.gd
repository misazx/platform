enum SkillType { ACTIVE, PASSIVE, TOGGLE }
enum TargetType { SELF, SINGLE_ENEMY, AREA_OF_EFFECT, DIRECTION, POSITION }

class_name SkillManager extends Node

signal skill_learned(skill_id: String)
signal skill_used(skill_id: String)
signal skill_leveled_up(skill_id: String, new_level: int)

var _skills: Dictionary = {}
var _skill_instances: Dictionary = {}

func _ready() -> void:
	initialize_default_skills()

func initialize_default_skills() -> void:
	register_skill({
		"id": "dash",
		"name": "冲刺",
		"description": "快速向前冲刺，短暂无敌",
		"type": SkillType.ACTIVE,
		"target_type": TargetType.DIRECTION,
		"cooldown": 3.0,
		"mana_cost": 10.0,
		"max_level": 5,
		"icon_path": "res://GameModes/base_game/Resources/Icons/Skills/dash.png",
		"level_stats": {
			1: {"damage": 10.0, "range": 100.0},
			2: {"damage": 15.0, "range": 120.0},
			3: {"damage": 20.0, "range": 140.0},
			4: {"damage": 25.0, "range": 160.0},
			5: {"damage": 30.0, "range": 200.0}
		},
		"tags": ["movement"]
	})

	register_skill({
		"id": "fireball",
		"name": "火球术",
		"description": "发射一枚火球，对敌人造成火焰伤害",
		"type": SkillType.ACTIVE,
		"target_type": TargetType.SINGLE_ENEMY,
		"cooldown": 5.0,
		"mana_cost": 20.0,
		"max_level": 5,
		"icon_path": "res://GameModes/base_game/Resources/Icons/Skills/fireball.png",
		"level_stats": {
			1: {"damage": 30.0, "radius": 20.0},
			2: {"damage": 45.0, "radius": 25.0},
			3: {"damage": 60.0, "radius": 30.0},
			4: {"damage": 80.0, "radius": 35.0},
			5: {"damage": 100.0, "radius": 40.0}
		},
		"tags": ["magic", "fire"]
	})

	register_skill({
		"id": "iron_skin",
		"name": "铁壁",
		"description": "增加防御力，持续一段时间",
		"type": SkillType.ACTIVE,
		"target_type": TargetType.SELF,
		"cooldown": 8.0,
		"mana_cost": 15.0,
		"max_level": 3,
		"icon_path": "",
		"level_stats": {
			1: {"defense_bonus": 10.0, "duration": 3.0},
			2: {"defense_bonus": 18.0, "duration": 4.0},
			3: {"defense_bonus": 28.0, "duration": 5.0}
		},
		"tags": ["defense"]
	})

	register_skill({
		"id": "heal",
		"name": "治愈",
		"description": "恢复自身生命值",
		"type": SkillType.ACTIVE,
		"target_type": TargetType.SELF,
		"cooldown": 10.0,
		"mana_cost": 25.0,
		"max_level": 3,
		"icon_path": "res://GameModes/base_game/Resources/Icons/Skills/heal.png",
		"level_stats": {
			1: {"heal_amount": 20.0},
			2: {"heal_amount": 35.0},
			3: {"heal_amount": 55.0}
		},
		"tags": ["healing"]
	})

	GD.print("[SkillManager] Initialized with %d skills" % _skills.size())

func register_skill(skill: Dictionary) -> void:
	_skills[skill["id"]] = skill

func learn_skill(skill_id: String) -> bool:
	if not _skills.has(skill_id):
		GD.printerr("[SkillManager] Unknown skill: %s" % skill_id)
		return false
	if _skill_instances.has(skill_id):
		return false

	_skill_instances[skill_id] = {
		"skill_id": skill_id,
		"level": 1,
		"current_cooldown": 0.0,
		"is_unlocked": true,
		"is_active": false
	}
	skill_learned.emit(skill_id)
	GD.print("[SkillManager] Learned skill: %s" % skill_id)
	return true

func use_skill(skill_id: String, caster: Node, target: Node = null) -> bool:
	if not _skill_instances.has(skill_id):
		return false

	var inst := _skill_instances[skill_id]
	if inst["current_cooldown"] > 0 or not inst["is_unlocked"]:
		return false

	inst["current_cooldown"] = _skills[skill_id]["cooldown"]
	skill_used.emit(skill_id)
	GD.print("[SkillManager] Used skill: %s (level %d)" % [skill_id, inst["level"]])
	return true

func level_up_skill(skill_id: String) -> bool:
	if not _skill_instances.has(skill_id):
		return false
	var inst := _skill_instances[skill_id]
	var max_level: int = _skills[skill_id]["max_level"]
	if inst["level"] >= max_level:
		return false
	inst["level"] += 1
	skill_leveled_up.emit(skill_id, inst["level"])
	GD.print("[SkillManager] Skill leveled up: %s -> level %d" % [skill_id, inst["level"]])
	return true

func update_delta(delta: float) -> void:
	for skill_id in _skill_instances:
		var inst := _skill_instances[skill_id]
		if inst["current_cooldown"] > 0:
			inst["current_cooldown"] -= delta

func is_skill_ready(skill_id: String) -> bool:
	if not _skill_instances.has(skill_id):
		return false
	var inst := _skill_instances[skill_id]
	return inst["current_cooldown"] <= 0 and inst["is_unlocked"]

func get_all_skills() -> Dictionary:
	return _skills.duplicate()

func get_learned_skills() -> Dictionary:
	return _skill_instances.duplicate()

func get_skill_level_stats(skill_id: String, level: int) -> Dictionary:
	if not _skills.has(skill_id):
		return {}
	var skill := _skills[skill_id]
	var stats: Dictionary = skill.get("level_stats", {})
	return stats.get(level, {})
