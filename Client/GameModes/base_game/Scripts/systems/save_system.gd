extends Node
signal game_saved(slot: int)
signal game_loaded(slot: int)

const SAVE_PATH := "user://saves/"
const MAX_SLOTS := 3

var _auto_save := true
var _auto_save_interval := 300.0
var _auto_save_timer := 0.0
var _current_game_data: Dictionary = {}

func _ready() -> void:
	ensure_save_directory()

func ensure_save_directory() -> void:
	var dir := DirAccess.open("user://")
	if dir != null:
		dir.make_dir_recursive("saves")

func save_game(slot: int, game_data: Dictionary) -> bool:
	var path := SAVE_PATH + "save_%d.json" % slot
	game_data["save_time"] = Time.get_datetime_string_from_system()
	game_data["save_version"] = 1

	var json_str := JSON.stringify(game_data, "\t")
	var file := FileAccess.open(path, FileAccess.WRITE)
	if file == null:
		push_error("[SaveSystem] Failed to open save file: %s" % path)
		return false
	file.store_string(json_str)
	file.close()
	game_saved.emit(slot)
	print("[SaveSystem] Game saved to slot %d" % slot)
	return true

func load_game(slot: int) -> Dictionary:
	var path := SAVE_PATH + "save_%d.json" % slot
	if not FileAccess.file_exists(path):
		push_error("[SaveSystem] Save file not found: %s" % path)
		return {}

	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		return {}
	var json_str := file.get_as_text()
	file.close()

	var json := JSON.new()
	var error := json.parse(json_str)
	if error != OK:
		push_error("[SaveSystem] Error parsing save file: %s" % json.get_error_message())
		return {}

	_current_game_data = json.get_data()
	game_loaded.emit(slot)
	print("[SaveSystem] Game loaded from slot %d" % slot)
	return _current_game_data

func has_save(slot: int) -> bool:
	return FileAccess.file_exists(SAVE_PATH + "save_%d.json" % slot)

func delete_save(slot: int) -> bool:
	var path := SAVE_PATH + "save_%d.json" % slot
	if FileAccess.file_exists(path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(path))
		print("[SaveSystem] Deleted save slot %d" % slot)
		return true
	return false

func get_save_slots_info() -> Array:
	var slots := []
	for i in range(MAX_SLOTS):
		slots.append({"slot": i, "has_data": has_save(i)})
	return slots

func set_auto_save(enabled: bool) -> void:
	_auto_save = enabled

func _process(delta: float) -> void:
	if _auto_save:
		_auto_save_timer += delta
		if _auto_save_timer >= _auto_save_interval:
			_auto_save_timer = 0.0
			if not _current_game_data.is_empty():
				save_game(0, _current_game_data)
