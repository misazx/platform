class_name ConfigLoader

const CONFIG_DATA_PATH := "res://GameModes/base_game/Config/Data/"
const CONFIG_COMPILED_PATH := "res://GameModes/base_game/Config/Compiled/"
const JSON_EXTENSION := ".json"
const BYTES_EXTENSION := ".bytes"

static var _use_compiled_config: bool = false
static var _config_cache: Dictionary = {}

static func set_use_compiled_config(value: bool) -> void:
	_use_compiled_config = value
	_config_cache.clear()

static func get_use_compiled_config() -> bool:
	return _use_compiled_config

static func load_config(config_name: String) -> Dictionary:
	var cache_key := config_name + "_dict"
	if _config_cache.has(cache_key):
		return _config_cache[cache_key]

	var config := {}
	if _use_compiled_config:
		config = load_from_bytes(config_name)

	if config.is_empty():
		config = load_from_json(config_name)

	if not config.is_empty():
		_config_cache[cache_key] = config

	return config

static func load_from_json(config_name: String) -> Dictionary:
	var path := CONFIG_DATA_PATH + config_name + JSON_EXTENSION
	if not ResourceLoader.exists(path):
		GD.printerr("[ConfigLoader] JSON config not found: %s" % path)
		return {}

	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		GD.printerr("[ConfigLoader] Failed to open JSON file: %s" % path)
		return {}

	var json_content := file.get_as_text()
	file.close()

	var json := JSON.new()
	var error := json.parse(json_content)
	if error != OK:
		GD.printerr("[ConfigLoader] Error parsing JSON config %s: %s" % [config_name, json.get_error_message()])
		return {}

	var data := json.get_data()
	if data is Dictionary:
		GD.print("[ConfigLoader] Loaded JSON config: %s" % config_name)
		return data

	return {}

static func load_from_bytes(config_name: String) -> Dictionary:
	var path := CONFIG_COMPILED_PATH + config_name + BYTES_EXTENSION
	if not ResourceLoader.exists(path):
		return {}

	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		return {}

	var compressed_data := file.get_buffer(file.get_length())
	file.close()

	var decompressed := decompress_buffer(compressed_data)
	if decompressed == "":
		return {}

	var json := JSON.new()
	var error := json.parse(decompressed)
	if error != OK:
		return {}

	var data := json.get_data()
	if data is Dictionary:
		GD.print("[ConfigLoader] Loaded bytes config: %s" % config_name)
		return data

	return {}

static func compile_config_to_bytes(config_name: String) -> bool:
	var json_path := CONFIG_DATA_PATH + config_name + JSON_EXTENSION
	var bytes_path := CONFIG_COMPILED_PATH + config_name + BYTES_EXTENSION

	if not ResourceLoader.exists(json_path):
		GD.printerr("[ConfigLoader] Source JSON not found: %s" % json_path)
		return false

	var json_file := FileAccess.open(json_path, FileAccess.READ)
	if json_file == null:
		return false

	var json_content := json_file.get_as_text()
	json_file.close()

	var compressed := compress_string(json_content)
	var bytes_file := FileAccess.open(bytes_path, FileAccess.WRITE)
	if bytes_file == null:
		return false

	bytes_file.store_buffer(compressed)
	bytes_file.close()

	GD.print("[ConfigLoader] Compiled config to bytes: %s (%d bytes)" % [config_name, compressed.size()])
	return true

static func compile_all_configs() -> bool:
	var config_names := ["cards", "characters", "enemies", "relics", "potions", "events", "audio", "effects"]
	var success_count := 0
	for name in config_names:
		if compile_config_to_bytes(name):
			success_count += 1

	GD.print("[ConfigLoader] Compiled %d/%d configs" % [success_count, config_names.size()])
	return success_count == config_names.size()

static func compress_string(text: String) -> PackedByteArray:
	return text.to_utf8().compress(FileAccess.COMPRESSION_GZIP)

static func decompress_buffer(data: PackedByteArray) -> String:
	var decompressed := data.decompress(-1, FileAccess.COMPRESSION_GZIP)
	if decompressed.size() == 0:
		return ""
	return decompressed.get_string_from_utf8()

static func clear_cache() -> void:
	_config_cache.clear()
	GD.print("[ConfigLoader] Cache cleared")

static func validate_config(config: Dictionary) -> bool:
	if config.is_empty():
		GD.printerr("[ConfigLoader] Config validation failed: config is empty")
		return false
	return true
