class_name EncounterGenerator extends Node

enum NodeType { MONSTER, ELITE, BOSS, REST, SHOP, EVENT, TREASURE }

class ActConfig:
	var name: String = ""
	var floors: int = 3
	var normal_pool: PackedStringArray = []
	var elite_pool: PackedStringArray = []
	var boss: String = ""
	var hp_multiplier: float = 1.0
	var dmg_multiplier: float = 1.0

var _acts: Array = []
var _enemy_stats: Dictionary = {}
var _encounter_templates: Dictionary = {}

func _ready() -> void:
	_init_acts()
	_init_enemy_stats()
	_init_encounter_templates()

func _init_acts() -> void:
	var act1 := ActConfig.new()
	act1.name = "第一幕：深渊之门"
	act1.floors = 3
	act1.normal_pool = PackedStringArray(["Cultist", "JawWorm", "Louse", "FungiBeast"])
	act1.elite_pool = PackedStringArray(["Gremlin_Nob", "Lagavulin"])
	act1.boss = "The_Guardian"
	act1.hp_multiplier = 1.0
	act1.dmg_multiplier = 1.0
	_acts.append(act1)

	var act2 := ActConfig.new()
	act2.name = "第二幕：暗影走廊"
	act2.floors = 3
	act2.normal_pool = PackedStringArray(["Slaver", "FungiBeast", "Sentry", "ShelledParasite", "AcidSlime_L"])
	act2.elite_pool = PackedStringArray(["Gremlin_Nob", "Lagavulin", "Sentry"])
	act2.boss = "The_Collector"
	act2.hp_multiplier = 1.2
	act2.dmg_multiplier = 1.15
	_acts.append(act2)

	var act3 := ActConfig.new()
	act3.name = "第三幕：熔炉之心"
	act3.floors = 3
	act3.normal_pool = PackedStringArray(["RedSlaver", "TaskMaster", "Sentry", "AcidSlime_L", "SpikeSlime_L", "ShelledParasite"])
	act3.elite_pool = PackedStringArray(["Lagavulin", "Sentry"])
	act3.boss = "The_Automaton"
	act3.hp_multiplier = 1.45
	act3.dmg_multiplier = 1.3
	_acts.append(act3)

	var act4 := ActConfig.new()
	act4.name = "第四幕：尖塔之巅"
	act4.floors = 2
	act4.normal_pool = PackedStringArray(["RedSlaver", "TaskMaster", "SpikeSlime_L", "FungusLing"])
	act4.elite_pool = PackedStringArray(["Gremlin_Nob", "Lagavulin"])
	act4.boss = "The_Awakener"
	act4.hp_multiplier = 1.7
	act4.dmg_multiplier = 1.5
	_acts.append(act4)

func _init_enemy_stats() -> void:
	_enemy_stats = {
		"Cultist": {"base_hp": 48, "min_dmg": 6, "max_dmg": 9},
		"JawWorm": {"base_hp": 44, "min_dmg": 7, "max_dmg": 11},
		"Louse": {"base_hp": 31, "min_dmg": 5, "max_dmg": 7},
		"Slaver": {"base_hp": 46, "min_dmg": 9, "max_dmg": 13},
		"RedSlaver": {"base_hp": 52, "min_dmg": 11, "max_dmg": 15},
		"FungiBeast": {"base_hp": 40, "min_dmg": 6, "max_dmg": 10},
		"Gremlin_Nob": {"base_hp": 82, "min_dmg": 12, "max_dmg": 16},
		"Lagavulin": {"base_hp": 109, "min_dmg": 14, "max_dmg": 20},
		"Sentry": {"base_hp": 42, "min_dmg": 8, "max_dmg": 12},
		"ShelledParasite": {"base_hp": 40, "min_dmg": 6, "max_dmg": 9},
		"AcidSlime_L": {"base_hp": 36, "min_dmg": 5, "max_dmg": 8},
		"AcidSlime_M": {"base_hp": 24, "min_dmg": 4, "max_dmg": 6},
		"AcidSlime_S": {"base_hp": 14, "min_dmg": 3, "max_dmg": 4},
		"SpikeSlime_L": {"base_hp": 38, "min_dmg": 5, "max_dmg": 8},
		"SpikeSlime_M": {"base_hp": 26, "min_dmg": 4, "max_dmg": 7},
		"SpikeSlime_S": {"base_hp": 16, "min_dmg": 3, "max_dmg": 5},
		"TaskMaster": {"base_hp": 54, "min_dmg": 10, "max_dmg": 14},
		"FungusLing": {"base_hp": 30, "min_dmg": 5, "max_dmg": 8},
		"The_Guardian": {"base_hp": 240, "min_dmg": 14, "max_dmg": 20},
		"Hexaghost": {"base_hp": 250, "min_dmg": 16, "max_dmg": 22},
		"The_Collector": {"base_hp": 282, "min_dmg": 14, "max_dmg": 20},
		"The_Automaton": {"base_hp": 300, "min_dmg": 16, "max_dmg": 22},
		"Donu_and_Deca": {"base_hp": 250, "min_dmg": 14, "max_dmg": 20},
		"The_Awakener": {"base_hp": 300, "min_dmg": 18, "max_dmg": 26},
		"Slime_Boss": {"base_hp": 150, "min_dmg": 10, "max_dmg": 16},
	}

func _init_encounter_templates() -> void:
	_encounter_templates = {
		"easy_1": ["Cultist"],
		"easy_2": ["JawWorm"],
		"easy_3": ["Louse", "Louse"],
		"medium_1": ["Cultist", "Louse"],
		"medium_2": ["JawWorm", "Louse"],
		"medium_3": ["Slaver", "FungiBeast"],
		"medium_4": ["AcidSlime_L", "SpikeSlime_L"],
		"hard_1": ["Slaver", "RedSlaver"],
		"hard_2": ["TaskMaster", "FungusLing", "FungusLing"],
		"hard_3": ["RedSlaver", "ShelledParasite"],
		"elite_gremlin": ["Gremlin_Nob"],
		"elite_lagavulin": ["Lagavulin"],
		"elite_sentries": ["Sentry", "Sentry"],
		"boss_guardian": ["The_Guardian"],
		"boss_collector": ["The_Collector"],
		"boss_automaton": ["The_Automaton"],
		"boss_awakener": ["The_Awakener"],
		"boss_slime": ["Slime_Boss"],
	}

func get_act_for_floor(floor_num: int) -> int:
	if floor_num <= 3: return 0
	if floor_num <= 6: return 1
	if floor_num <= 9: return 2
	return 3

func get_current_act(floor_num: int) -> ActConfig:
	return _acts[get_act_for_floor(floor_num)]

func get_floor_in_act(floor_num: int) -> int:
	return ((floor_num - 1) % 3) + 1

func is_boss_floor(floor_num: int) -> bool:
	var act_idx: int = get_act_for_floor(floor_num)
	var act_start: int = act_idx * 3 + 1
	var act_end: int = act_start + _acts[act_idx].floors - 1
	return floor_num == act_end

func generate_encounter(enemy_encounter_id: String, node_type: int, floor_number: int) -> Dictionary:
	var rng := RandomNumberGenerator.new()
	rng.seed = hash(enemy_encounter_id + str(floor_number))
	var act: ActConfig = get_current_act(floor_number)
	var floor_in_act: int = get_floor_in_act(floor_number)
	var hp_scale: float = act.hp_multiplier * (1.0 + (floor_in_act - 1) * 0.1)
	var dmg_scale: float = act.dmg_multiplier * (1.0 + (floor_in_act - 1) * 0.06)
	print("[EncounterGenerator] Act %s: %s, Floor %d/%d, Type=%d" % [get_act_for_floor(floor_number), act.name, floor_in_act, act.floors, node_type])
	match node_type:
		NodeType.BOSS:
			return _generate_boss_encounter(act, floor_in_act, hp_scale, dmg_scale, rng)
		NodeType.ELITE:
			return _generate_elite_encounter(act, floor_in_act, hp_scale, dmg_scale, rng)
		_:
			return _generate_normal_encounter(enemy_encounter_id, act, floor_in_act, hp_scale, dmg_scale, rng)

func _generate_boss_encounter(act: ActConfig, floor_in_act: int, hp_scale: float, dmg_scale: float, rng: RandomNumberGenerator) -> Dictionary:
	var boss_id: String = act.boss
	var stats: Dictionary = _enemy_stats.get(boss_id, {"base_hp": 240, "min_dmg": 14, "max_dmg": 20})
	var hp: int = int(stats.base_hp * hp_scale)
	var dmg: int = int(stats.min_dmg * dmg_scale)
	var boss := _create_enemy_data(boss_id, hp, dmg)
	return {
		"enemies": [boss],
		"gold_reward": 80 + floor_in_act * 20 + rng.randi_range(0, 50),
		"description": "Boss: %s (HP:%d)" % [boss.name, hp],
		"act_name": act.name
	}

func _generate_elite_encounter(act: ActConfig, floor_in_act: int, hp_scale: float, dmg_scale: float, rng: RandomNumberGenerator) -> Dictionary:
	var template_key: String = ["elite_gremlin", "elite_lagavulin", "elite_sentries"][rng.randi() % 3]
	var template: Array = _encounter_templates.get(template_key, [act.elite_pool[rng.randi() % act.elite_pool.size()]])
	var enemies: Array = []
	for i in range(template.size()):
		var e_id: String = template[i]
		var stats: Dictionary = _enemy_stats.get(e_id, {"base_hp": 82, "min_dmg": 12, "max_dmg": 16})
		var hp: int = int(stats.base_hp * hp_scale)
		var dmg: int = int(stats.min_dmg * dmg_scale)
		var suffix: String = "_%d" % (i + 1) if template.size() > 1 else ""
		var enemy := _create_enemy_data(e_id + suffix, hp, dmg)
		enemy.name = e_id.replace("_", " ") + (" #%d" % (i + 1) if template.size() > 1 else "")
		enemies.append(enemy)
	return {
		"enemies": enemies,
		"gold_reward": 35 + floor_in_act * 12 + rng.randi_range(0, 30),
		"description": "Elite: %s" % ", ".join(enemies.map(func(e): return e.name)),
		"act_name": act.name
	}

func _generate_normal_encounter(enemy_id: String, act: ActConfig, floor_in_act: int, hp_scale: float, dmg_scale: float, rng: RandomNumberGenerator) -> Dictionary:
	var template_keys: Array
	match floor_in_act:
		1: template_keys = ["easy_1", "easy_2", "easy_3", "medium_1"]
		2: template_keys = ["medium_1", "medium_2", "medium_3", "medium_4"]
		_: template_keys = ["medium_3", "medium_4", "hard_1", "hard_2", "hard_3"]
	var template_key: String = template_keys[rng.randi() % template_keys.size()]
	var template: Array = _encounter_templates.get(template_key, [])
	if template.is_empty():
		var count: int = 1 + rng.randi() % (2 if floor_in_act <= 1 else 3)
		for i in range(count):
			template.append(act.normal_pool[rng.randi() % act.normal_pool.size()])
	var enemies: Array = []
	for i in range(template.size()):
		var e_id: String = enemy_id if (enemy_id != "" and i == 0) else template[i]
		var stats: Dictionary = _enemy_stats.get(e_id, {"base_hp": 50, "min_dmg": 6, "max_dmg": 8})
		var hp: int = int(stats.base_hp * hp_scale)
		var dmg: int = int(stats.min_dmg * dmg_scale)
		var is_leader: bool = i == 0 and template.size() > 1
		if is_leader:
			hp = int(hp * 1.2)
			dmg = int(dmg * 1.1)
		var suffix: String = "_%d" % (i + 1) if template.size() > 1 else ""
		var enemy := _create_enemy_data(e_id + suffix, hp, dmg)
		enemies.append(enemy)
	return {
		"enemies": enemies,
		"gold_reward": 12 + floor_in_act * 5 + rng.randi_range(0, 18),
		"description": ", ".join(enemies.map(func(e): return e.name)),
		"act_name": act.name
	}

func _create_enemy_data(id: String, hp: int, dmg: int) -> Dictionary:
	return {
		"id": id,
		"name": id.replace("_", " "),
		"max_hp": hp,
		"current_hp": hp,
		"block": 0,
		"status_effects": [],
		"current_intent": {"type": 0, "value": dmg, "value2": 0, "description": "准备攻击 %d" % dmg, "icon": "⚔"}
	}

func get_base_health(floor_num: int) -> int:
	return 72 + (floor_num - 1) * 6

func get_base_energy(floor_num: int) -> int:
	return 3

func get_card_reward_count(floor_num: int) -> int:
	return 3

func get_total_floors() -> int:
	var total: int = 0
	for act in _acts:
		total += act.floors
	return total

func get_act_name(floor_num: int) -> String:
	return get_current_act(floor_num).name
