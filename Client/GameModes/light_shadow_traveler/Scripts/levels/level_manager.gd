class_name LevelManager
extends Node

signal level_loaded(level_id: String)
signal level_completed(level_id: String)
signal level_failed(level_id: String)
signal all_fragments_collected(count: int)

const LEVELS_CONFIG_PATH := "res://GameModes/light_shadow_traveler/Config/Data/levels.json"
const PACKAGE_CONFIG_PATH := "res://GameModes/light_shadow_traveler/Config/Data/package_config.json"

var current_chapter_id := ""
var current_level_id := ""
var current_level_data: Dictionary = {}
var levels_data: Dictionary = {}
var completed_levels: Dictionary = {}
var collected_fragments: Dictionary = {}

func _ready() -> void:
	_load_levels_config()

func _load_levels_config() -> void:
	var config_path := LEVELS_CONFIG_PATH
	if not FileAccess.file_exists(config_path):
		push_error("[LevelManager] Levels config not found: " + config_path)
		return
	var file := FileAccess.open(config_path, FileAccess.READ)
	var json_text := file.get_as_text()
	file.close()
	var parsed: Variant = JSON.parse_string(json_text)
	if not parsed:
		push_error("[LevelManager] Failed to parse levels JSON")
		return
	var data: Dictionary = parsed as Dictionary
	if data.is_empty():
		return
	var chapters: Array = data.get("chapters", []) as Array
	for chapter in chapters:
		var chapter_dict: Dictionary = chapter as Dictionary
		var chapter_id: String = chapter_dict.get("id", "")
		var levels: Array = chapter_dict.get("levels", []) as Array
		for level in levels:
			var level_dict: Dictionary = level as Dictionary
			var level_id: String = level_dict.get("id", "")
			levels_data[level_id] = level_dict
			levels_data[level_id]["chapter_id"] = chapter_id
	print("[LevelManager] Loaded ", levels_data.size(), " levels")

func load_level(level_id: String) -> bool:
	if not levels_data.has(level_id):
		push_error("[LevelManager] Level not found: " + level_id)
		return false
	current_level_id = level_id
	current_level_data = levels_data[level_id]
	current_chapter_id = current_level_data.get("chapter_id", "")
	if not collected_fragments.has(level_id):
		collected_fragments[level_id] = 0
	level_loaded.emit(level_id)
	print("[LevelManager] Level loaded: ", level_id)
	return true

func complete_level() -> void:
	completed_levels[current_level_id] = true
	level_completed.emit(current_level_id)
	print("[LevelManager] Level completed: ", current_level_id)

func fail_level() -> void:
	level_failed.emit(current_level_id)
	print("[LevelManager] Level failed: ", current_level_id)

func add_fragment() -> void:
	if not collected_fragments.has(current_level_id):
		collected_fragments[current_level_id] = 0
	collected_fragments[current_level_id] += 1
	var total := 0
	for count in collected_fragments.values():
		total += count
	all_fragments_collected.emit(total)

func get_chapter_levels(chapter_id: String) -> Array[Dictionary]:
	var result: Array[Dictionary] = []
	for level_id in levels_data:
		if levels_data[level_id].get("chapter_id", "") == chapter_id:
			result.append(levels_data[level_id])
	return result

func get_level_data(level_id: String) -> Dictionary:
	return levels_data.get(level_id, {})

func is_level_completed(level_id: String) -> bool:
	return completed_levels.get(level_id, false)

func get_fragment_count(level_id: String) -> int:
	return collected_fragments.get(level_id, 0)

func get_total_fragments() -> int:
	var total := 0
	for count in collected_fragments.values():
		total += count
	return total

func get_chapters() -> Array:
	var config_path := PACKAGE_CONFIG_PATH
	if not FileAccess.file_exists(config_path):
		return []
	var file := FileAccess.open(config_path, FileAccess.READ)
	var json_text := file.get_as_text()
	file.close()
	var parsed: Variant = JSON.parse_string(json_text)
	if not parsed:
		return []
	var result: Array = (parsed as Dictionary).get("chapters", []) as Array
	return result

func save_progress() -> Dictionary:
	return {
		"completed_levels": completed_levels.duplicate(),
		"collected_fragments": collected_fragments.duplicate()
	}

func load_progress(data: Dictionary) -> void:
	completed_levels = data.get("completed_levels", {})
	collected_fragments = data.get("collected_fragments", {})
