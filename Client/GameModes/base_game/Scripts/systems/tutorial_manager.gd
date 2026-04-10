class_name TutorialManager extends Node

signal tutorial_step_changed(step_id: String)
signal tutorial_completed(tutorial_id: String)
signal tutorial_skipped(tutorial_id: String)

var _completed_tutorials: Array = []
var _current_tutorial: String = ""
var _current_step_index := 0
var _tutorials: Dictionary = {}

func _ready() -> void:
	load_tutorials()

func load_tutorials() -> void:
	_tutorials["combat_basics"] = {
		"name": "战斗基础",
		"description": "学习基本的战斗操作",
		"steps": [
			{"id": "combat_intro", "title": "欢迎来到战斗!", "text": "这里是杀戮尖塔的战斗界面。你将在左侧看到你的角色，右侧是敌人。", "highlight": null},
			{"id": "energy_system", "title": "能量系统", "text": "每回合你有3点能量。打出卡牌需要消耗对应数量的能量。", "highlight": "energy_bar"},
			{"id": "card_hand", "title": "手牌", "text": "每回合开始时你会抽取5张牌到手中。点击卡牌可以打出它。", "highlight": "hand_container"},
			{"id": "end_turn", "title": "结束回合", "text": "当你完成所有操作后，点击'结束回合'按钮让敌人行动。", "highlight": "end_turn_button"},
			{"id": "block_system", "title": "格挡", "text": '格挡可以抵消本回合受到的伤害。某些卡牌可以提供格挡值。', "highlight": "block_label"}
		]
	}

	_tutorials["card_types"] = {
		"name": "卡牌类型",
		"description": "了解不同类型的卡牌",
		"steps": [
			{"id": "attack_cards", "title": "攻击牌", "text": "红色边框的是攻击牌，可以对敌人造成伤害。", "highlight": null},
			{"id": "skill_cards", "title": "技能牌", "text": "绿色边框的是技能牌，提供防御、增益等效果。", "highlight": null},
			{"id": "power_cards", "title": "能力牌", "text": "金色边框的是能力牌，效果持续整场战斗。", "highlight": null},
			{"id": "card_upgrades", "title": "卡牌升级", "text": "升级后的卡牌更强大，但消耗更多能量。", "highlight": null}
		]
	}

	_tutorials["relics_intro"] = {
		"name": "遗物介绍",
		"description": "了解遗物系统的基础知识",
		"steps": [
			{"id": "what_are_relics", "title": "什么是遗物?", "text": "遗物是强大的物品，可以在整个游戏中提供被动加成或主动能力。", "highlight": null},
			{"id": "relic_tiers", "title": "遗物稀有度", "text": "遗物分为普通、罕见、稀有、Boss和特殊等级。越稀有的遗物越强大。", "highlight": null},
			{"id": "relic_sources", "title": "获取遗物", "text": "你可以通过击败精英敌人、Boss、商店购买、事件奖励等方式获得遗物。", "highlight": null}
		]
	}

	GD.print("[TutorialManager] Loaded %d tutorials" % _tutorials.size())

func start_tutorial(tutorial_id: String) -> bool:
	if not _tutorials.has(tutorial_id):
		GD.printerr("[TutorialManager] Tutorial not found: %s" % tutorial_id)
		return false
	if tutorial_id in _completed_tutorials:
		return false
	_current_tutorial = tutorial_id
	_current_step_index = 0
	tutorial_step_changed.emit(get_current_step()["id"])
	GD.print("[TutorialManager] Started tutorial: %s" % tutorial_id)
	return true

func next_step() -> Dictionary:
	if _current_tutorial == "":
		return {}
	var steps: Array = _tutorials[_current_tutorial]["steps"]
	_current_step_index += 1
	if _current_step_index >= steps.size():
		complete_tutorial(_current_tutorial)
		return {}
	var step := steps[_current_step_index]
	tutorial_step_changed.emit(step["id"])
	return step

func previous_step() -> Dictionary:
	if _current_tutorial == "":
		return {}
	if _current_step_index <= 0:
		return {}
	_current_step_index -= 1
	var steps: Array = _tutorials[_current_tutorial]["steps"]
	var step := steps[_current_step_index]
	tutorial_step_changed.emit(step["id"])
	return step

func skip_tutorial() -> void:
	if _current_tutorial != "":
		tutorial_skipped.emit(_current_tutorial)
		_current_tutorial = ""
		_current_step_index = 0

func complete_tutorial(tutorial_id: String) -> void:
	if tutorial_id not in _completed_tutorials:
		_completed_tutorials.append(tutorial_id)
	tutorial_completed.emit(tutorial_id)
	_current_tutorial = ""
	_current_step_index = 0
	GD.print("[TutorialManager] Completed tutorial: %s" % tutorial_id)

func get_current_step() -> Dictionary:
	if _current_tutorial == "":
		return {}
	var steps: Array = _tutorials[_current_tutorial]["steps"]
	if _current_step_index >= steps.size():
		return {}
	return steps[_current_step_index]

func get_tutorial_progress(tutorial_id: String) -> Dictionary:
	if not _tutorials.has(tutorial_id):
		return {"total": 0, "current": 0, "completed": tutorial_id in _completed_tutorials}
	var tut := _tutorials[tutorial_id]
	return {
		"total": tut["steps"].size(),
		"current": _current_step_index if _current_tutorial == tutorial_id else tut["steps"].size(),
		"completed": tutorial_id in _completed_tutorials
	}

func is_tutorial_completed(tutorial_id: String) -> bool:
	return tutorial_id in _completed_tutorials

func get_all_tutorials() -> Dictionary:
	return _tutorials.duplicate()
