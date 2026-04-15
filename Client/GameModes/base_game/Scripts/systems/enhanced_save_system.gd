extends Node

const MAX_SLOTS: int = 3
const SAVE_DIR: String = "user://saves/"
const SCREENSHOT_DIR: String = "user://screenshots/"

func _ready() -> void:
	_ensure_directories_exist()

func _ensure_directories_exist() -> void:
	var dir: DirAccess = DirAccess.open("user://")
	if dir != null:
		dir.make_dir_recursive("saves")
		dir.make_dir_recursive("screenshots")

func has_save(slot_id: int) -> bool:
	return FileAccess.file_exists(_get_save_path(slot_id))

func save_game(slot_id: int, run_data: Dictionary) -> Dictionary:
	var slot: Dictionary = {
		"slot_id": slot_id,
		"save_time": Time.get_datetime_string_from_system(),
		"character_id": run_data.get("character_id", ""),
		"current_floor": run_data.get("current_floor", 1),
		"current_health": run_data.get("current_health", -1),
		"max_health": run_data.get("max_health", -1),
		"gold": run_data.get("gold", 0),
		"total_kills": run_data.get("total_kills", 0),
		"total_damage_dealt": run_data.get("total_damage_dealt", 0),
		"deck_ids": Array(run_data.get("deck_ids", [])),
		"relic_ids": Array(run_data.get("relic_ids", [])),
		"potion_ids": Array(run_data.get("potion_ids", [])),
		"custom_data": Dictionary(run_data.get("custom_data", {})),
		"seed": run_data.get("seed", randi()),
		"screenshot_path": ""
	}

	var path: String = _get_save_path(slot_id)
	var json_str: String = JSON.stringify(slot, "\t")
	var file: FileAccess = FileAccess.open(path, FileAccess.WRITE)
	if file == null:
		push_error("[EnhancedSaveSystem] Failed to save slot %d" % slot_id)
		return {}
	file.store_string(json_str)
	file.close()
	print("[EnhancedSaveSystem] Saved slot %d" % slot_id)
	return slot

func load_game(slot_id: int) -> Dictionary:
	var path: String = _get_save_path(slot_id)
	if not FileAccess.file_exists(path):
		return {}
	var file: FileAccess = FileAccess.open(path, FileAccess.READ)
	if file == null:
		return {}
	var json_str: String = file.get_as_text()
	file.close()
	var json: JSON = JSON.new()
	json.parse(json_str)
	var data: Variant = json.get_data()
	print("[EnhancedSaveSystem] Loaded slot %d" % slot_id)
	return data if data else {}

func delete_save(slot_id: int) -> bool:
	var path: String = _get_save_path(slot_id)
	if FileAccess.file_exists(path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(path))
		return true
	return false

func get_all_save_slots() -> Array:
	var slots: Array = []
	for i: int in range(MAX_SLOTS):
		var info: Dictionary = {"slot_id": i, "has_data": has_save(i)}
		if has_save(i):
			var data: Dictionary = load_game(i)
			info.merge(data)
		slots.append(info)
	return slots

func clear_all_saves() -> void:
	for i: int in range(MAX_SLOTS):
		delete_save(i)
	print("[EnhancedSaveSystem] All saves cleared")

func _get_save_path(slot_id: int) -> String:
	return SAVE_DIR + "slot_%d.json" % slot_id
