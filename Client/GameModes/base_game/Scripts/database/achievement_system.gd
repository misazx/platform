enum AchievementType { PROGRESSION, COMBAT, COLLECTION, CHALLENGE, SECRET }

class_name AchievementSystem extends Node

signal achievement_unlocked(achievement_id: String)
signal achievement_progress_updated(achievement_id: String, progress: float)
signal run_completed(stats_json: String)

var _achievements: Dictionary = {}
var _run_history: Array = []
var _type_achievements: Dictionary = {}

func _ready() -> void:
	initialize_achievements()

func initialize_achievements() -> void:
	register_achievement({
		"id": "first_victory",
		"name": "初次胜利",
		"description": "完成一次游戏通关。",
		"flavor_text": "这只是开始。",
		"type": AchievementType.PROGRESSION,
		"points": 50,
		"is_hidden": false,
		"icon_path": "res://GameModes/base_game/Resources/Icons/Achievements/FirstVictory.png",
		"requirements": {"victory": true},
		"rewards": {"unlock_character": "watcher", "gold_bonus": 100},
		"target_value": 1,
		"current_value": 0,
		"is_unlocked": false,
		"completion_progress": 0.0
	})

	register_achievement({
		"id": "kill_100_enemies",
		"name": "百人斩",
		"description": "累计击败 100 个敌人。",
		"flavor_text": "你的剑已经饥渴难耐。",
		"type": AchievementType.COMBAT,
		"points": 30,
		"is_hidden": false,
		"icon_path": "res://GameModes/base_game/Resources/Icons/Achievements/Kill100.png",
		"requirements": {"total_kills": 100},
		"target_value": 100,
		"current_value": 0,
		"is_unlocked": false,
		"completion_progress": 0.0
	})

	register_achievement({
		"id": "collect_all_relics",
		"name": "收藏家",
		"description": "收集所有遗物。",
		"flavor_text": "每一件遗物都有它的故事。",
		"type": AchievementType.COLLECTION,
		"points": 100,
		"is_hidden": false,
		"icon_path": "res://GameModes/base_game/Resources/Icons/Achievements/AllRelics.png",
		"requirements": {"relics_collected": "all"},
		"target_value": 50,
		"current_value": 0,
		"is_unlocked": false,
		"completion_progress": 0.0
	})

	register_achievement({
		"id": "no_damage_run",
		"name": "无伤通关",
		"description": "在不受到任何伤害的情况下完成一整层。",
		"flavor_text": "完美的战斗艺术。",
		"type": AchievementType.CHALLENGE,
		"points": 200,
		"is_hidden": true,
		"icon_path": "res://GameModes/base_game/Resources/Icons/Achievements/NoDamage.png",
		"target_value": 1,
		"current_value": 0,
		"is_unlocked": false,
		"completion_progress": 0.0
	})

	GD.print("[AchievementSystem] Initialized with %d achievements" % _achievements.size())

func register_achievement(achievement: Dictionary) -> void:
	_achievements[achievement["id"]] = achievement
	var ach_type: int = achievement["type"]
	if not _type_achievements.has(ach_type):
		_type_achievements[ach_type] = []
	_type_achievements[ach_type].append(achievement)

func get_achievement(achievement_id: String) -> Dictionary:
	if _achievements.has(achievement_id):
		return _achievements[achievement_id]
	return {}

func get_all_achievements() -> Array:
	return _achievements.values()

func get_unlocked_achievements() -> Array:
	var result := []
	for ach in _achievements.values():
		if ach["is_unlocked"]:
			result.append(ach)
	return result

func get_locked_achievements() -> Array:
	var result := []
	for ach in _achievements.values():
		if not ach["is_unlocked"] and not ach["is_hidden"]:
			result.append(ach)
	return result

func update_progress(achievement_id: String, value: int) -> void:
	if not _achievements.has(achievement_id):
		return

	var ach := _achievements[achievement_id]
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

	var ach := _achievements[achievement_id]
	if ach["is_unlocked"]:
		return

	ach["is_unlocked"] = true
	ach["unlock_time"] = Time.get_datetime_string_from_system()
	achievement_unlocked.emit(achievement_id)
	GD.print("[AchievementSystem] Unlocked: %s" % ach["name"])

func record_run(run_stats: Dictionary) -> void:
	_run_history.append(run_stats)
	update_progress("kill_100_enemies", run_stats.get("enemies_defeated", 0))

	if run_stats.get("victory", false):
		unlock_achievement("first_victory")

	run_completed.emit(run_stats.get("character_id", ""))

func get_run_history(count: int = 10) -> Array:
	var start := maxi(0, _run_history.size() - count)
	return _run_history.slice(start, mini(count, _run_history.size() - start))

func get_total_runs() -> int:
	return _run_history.size()

func get_total_achievements() -> int:
	return _achievements.size()

func get_unlocked_count() -> int:
	return get_unlocked_achievements().size()

func get_overall_completion() -> float:
	if get_total_achievements() == 0:
		return 0.0
	return float(get_unlocked_count()) / float(get_total_achievements()) * 100.0
