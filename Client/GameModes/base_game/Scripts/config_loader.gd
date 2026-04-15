extends Node

const CONFIG_DATA_PATH: String = "res://GameModes/base_game/Config/Data/"
const CONFIG_COMPILED_PATH: String = "res://GameModes/base_game/Config/Compiled/"
const JSON_EXTENSION: String = ".json"
const BYTES_EXTENSION: String = ".bytes"

var _use_compiled_config: bool = false
var _config_cache: Dictionary = {}

func set_use_compiled_config(value: bool) -> void:
	_use_compiled_config = value
	_config_cache.clear()

func get_use_compiled_config() -> bool:
	return _use_compiled_config

func load_config(config_name: String) -> Dictionary:
	var cache_key: String = config_name + "_dict"
	if _config_cache.has(cache_key):
		return _config_cache[cache_key]

	var config: Dictionary = {}
	if _use_compiled_config:
		config = load_from_bytes(config_name)

	if config.is_empty():
		config = load_from_json(config_name)

	if not config.is_empty():
		_config_cache[cache_key] = config

	return config

func load_from_json(config_name: String) -> Dictionary:
	var path: String = CONFIG_DATA_PATH + config_name + JSON_EXTENSION
	if not ResourceLoader.exists(path):
		push_error("[ConfigLoader] JSON config not found: %s" % path)
		return {}

	var file: FileAccess = FileAccess.open(path, FileAccess.READ)
	if file == null:
		push_error("[ConfigLoader] Failed to open JSON file: %s" % path)
		return {}

	var json_content: String = file.get_as_text()
	file.close()

	var json: JSON = JSON.new()
	var error: int = json.parse(json_content)
	if error != OK:
		push_error("[ConfigLoader] Error parsing JSON config %s: %s" % [config_name, json.get_error_message()])
		return {}

	var data: Variant = json.get_data()
	if data is Dictionary:
		print("[ConfigLoader] Loaded JSON config: %s" % config_name)
		return data

	return {}

func load_from_bytes(config_name: String) -> Dictionary:
	var path: String = CONFIG_COMPILED_PATH + config_name + BYTES_EXTENSION
	if not ResourceLoader.exists(path):
		return {}

	var file: FileAccess = FileAccess.open(path, FileAccess.READ)
	if file == null:
		return {}

	var compressed_data: PackedByteArray = file.get_buffer(file.get_length())
	file.close()

	var decompressed: String = decompress_buffer(compressed_data)
	if decompressed == "":
		return {}

	var json: JSON = JSON.new()
	var error: int = json.parse(decompressed)
	if error != OK:
		return {}

	var data: Variant = json.get_data()
	if data is Dictionary:
		print("[ConfigLoader] Loaded bytes config: %s" % config_name)
		return data

	return {}

func compile_config_to_bytes(config_name: String) -> bool:
	var json_path: String = CONFIG_DATA_PATH + config_name + JSON_EXTENSION
	var bytes_path: String = CONFIG_COMPILED_PATH + config_name + BYTES_EXTENSION

	if not ResourceLoader.exists(json_path):
		push_error("[ConfigLoader] Source JSON not found: %s" % json_path)
		return false

	var json_file: FileAccess = FileAccess.open(json_path, FileAccess.READ)
	if json_file == null:
		return false

	var json_content: String = json_file.get_as_text()
	json_file.close()

	var compressed: PackedByteArray = compress_string(json_content)
	var bytes_file: FileAccess = FileAccess.open(bytes_path, FileAccess.WRITE)
	if bytes_file == null:
		return false

	bytes_file.store_buffer(compressed)
	bytes_file.close()

	print("[ConfigLoader] Compiled config to bytes: %s (%d bytes)" % [config_name, compressed.size()])
	return true

func compile_all_configs() -> bool:
	var config_names: Array = ["cards", "characters", "enemies", "relics", "potions", "events", "audio", "effects"]
	var success_count: int = 0
	for name: String in config_names:
		if compile_config_to_bytes(name):
			success_count += 1

	print("[ConfigLoader] Compiled %d/%d configs" % [success_count, config_names.size()])
	return success_count == config_names.size()

func compress_string(text: String) -> PackedByteArray:
	return text.to_utf8_buffer().compress(FileAccess.COMPRESSION_GZIP)

func decompress_buffer(data: PackedByteArray) -> String:
	var decompressed: PackedByteArray = data.decompress(-1, FileAccess.COMPRESSION_GZIP)
	if decompressed.size() == 0:
		return ""
	return decompressed.get_string_from_utf8()

func clear_cache() -> void:
	_config_cache.clear()
	print("[ConfigLoader] Cache cleared")

func validate_config(config: Dictionary) -> bool:
	if config.is_empty():
		push_error("[ConfigLoader] Config validation failed: config is empty")
		return false
	return true
