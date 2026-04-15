extends Node

signal achievement_unlocked(achievement_id: String)
signal achievement_progress_updated(achievement_id: String, progress: float)

var _achievements: Dictionary = {}
var _unlocked_ids: Array = []

func _ready() -> void:
	initialize_achievements()

func initialize_achievements() -> void:
	register_achievement({
		"id": "first_blood",
		"name": "初血",
		"description": "在战斗中首次击败敌人",
		"points": 10,
		"is_hidden": false,
		"icon_path": "",
		"target_value": 1,
		"current_value": 0,
		"is_unlocked": false,
		"completion_progress": 0.0
	})

	register_achievement({
		"id": "combo_master",
		"name": "连击大师",
		"description": "在一回合内打出5张攻击牌",
		"points": 25,
		"is_hidden": false,
		"icon_path": "",
		"target_value": 5,
		"current_value": 0,
		"is_unlocked": false,
		"completion_progress": 0.0
	})

	register_achievement({
		"id": "hoarder",
		"name": "囤积癖",
		"description": "单次游戏收集超过500金币",
		"points": 40,
		"is_hidden": false,
		"icon_path": "",
		"target_value": 500,
		"current_value": 0,
		"is_unlocked": false,
		"completion_progress": 0.0
	})

	register_achievement({
		"id": "speedrunner",
		"name": "速通者",
		"description": "在20层以内通关游戏",
		"points": 100,
		"is_hidden": true,
		"icon_path": "",
		"target_value": 20,
		"current_value": 0,
		"is_unlocked": false,
		"completion_progress": 0.0
	})

	print("[AchievementManager] Initialized with %d achievements" % _achievements.size())

func register_achievement(ach: Dictionary) -> void:
	_achievements[ach["id"]] = ach

func update_progress(achievement_id: String, value: int) -> void:
	if not _achievements.has(achievement_id):
		return
	var ach: Dictionary = _achievements[achievement_id]
	ach["current_value"] += value
	if ach["target_value"] > 0:
		ach["completion_progress"] = float(ach["current_value"]) / float(ach["target_value"])
	else:
		ach["completion_progress"] = 1.0
	achievement_progress_updated.emit(achievement_id, ach["completion_progress"])
	if ach["current_value"] >= ach["target_value"] and not ach["is_unlocked"]:
		unlock_achievement(achievement_id)

func unlock_achievement(achievement_id: String) -> void:
	if not _achievements.has(achievement_id):
		return
	var ach: Dictionary = _achievements[achievement_id]
	if ach["is_unlocked"]:
		return
	ach["is_unlocked"] = true
	_unlocked_ids.append(achievement_id)
	achievement_unlocked.emit(achievement_id)
	print("[AchievementManager] Unlocked: %s" % ach["name"])

func is_unlocked(achievement_id: String) -> bool:
	return achievement_id in _unlocked_ids

func get_achievement(achievement_id: String) -> Dictionary:
	if _achievements.has(achievement_id):
		return _achievements[achievement_id]
	return {}

func get_all_achievements() -> Array:
	return _achievements.values()

func get_unlocked_count() -> int:
	return _unlocked_ids.size()

func get_total_count() -> int:
	return _achievements.size()

func get_completion_percentage() -> float:
	if get_total_count() == 0:
		return 0.0
	return float(get_unlocked_count()) / float(get_total_count()) * 100.0
