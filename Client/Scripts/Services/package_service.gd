extends Node

signal package_list_updated()
signal package_activated(package_id: String)
signal package_deactivated(package_id: String)
signal package_error(package_id: String, error_msg: String)

enum PackageStatus { UNAVAILABLE, AVAILABLE, DOWNLOADING, DOWNLOADED, INSTALLING, INSTALLED, ERROR }
enum PackageType { BASE_GAME, EXPANSION, COMMUNITY, DLC }

var _registry: Dictionary = {}
var _packages: Dictionary = {}
var _states: Dictionary = {}
var _active_package_id: String = ""
var _config_cache: Dictionary = {}

static var instance: Node = null

func _ready() -> void:
	if instance != null and instance != self:
		queue_free()
		return
	instance = self
	_initialize()

func _initialize() -> void:
	_load_registry()
	_load_states()
	_register_builtin_packages()
	package_list_updated.emit()
	print("[PackageService] Initialized with %d packages" % _packages.size())

func _load_registry() -> void:
	var config_path := "res://Config/Data/package_registry.json"
	if not ResourceLoader.exists(config_path):
		_registry = {"version": "1.0.0", "packages": [], "categories": [], "featuredPackages": []}
		return

	var file := FileAccess.open(config_path, FileAccess.READ)
	if file == null:
		_registry = {"version": "1.0.0", "packages": [], "categories": [], "featuredPackages": []}
		return

	var json := JSON.new()
	if json.parse(file.get_as_text()) != OK:
		_registry = {"version": "1.0.0", "packages": [], "categories": [], "featuredPackages": []}
		return

	_registry = json.data if json.data is Dictionary else {"version": "1.0.0", "packages": []}

func _load_states() -> void:
	var state_path := "user://package_states.json"
	if not FileAccess.file_exists(state_path):
		return
	var file := FileAccess.open(state_path, FileAccess.READ)
	if file == null:
		return
	var json := JSON.new()
	if json.parse(file.get_as_text()) != OK:
		return
	var data := json.data as Dictionary
	if data == null:
		return
	var states: Array = data.get("states", [])
	for s in states:
		var pid: String = s.get("packageId", "")
		if pid != "":
			_states[pid] = {
				"status": s.get("status", PackageStatus.INSTALLED),
				"version": s.get("installedVersion", ""),
				"path": s.get("installedPath", ""),
				"last_played": s.get("lastPlayed", ""),
			}

func _save_states() -> void:
	var states_arr := []
	for pid in _states:
		var s: Dictionary = _states[pid]
		states_arr.append({
			"packageId": pid,
			"status": s.get("status", PackageStatus.INSTALLED),
			"installedVersion": s.get("version", ""),
			"installedPath": s.get("path", ""),
			"lastPlayed": s.get("last_played", ""),
		})
	var root := {"states": states_arr}
	var json_text := JSON.stringify(root, "\t")
	var file := FileAccess.open("user://package_states.json", FileAccess.WRITE)
	if file != null:
		file.store_string(json_text)

func _register_builtin_packages() -> void:
	var packages: Array = _registry.get("packages", [])
	for pkg in packages:
		var pid: String = pkg.get("id", "")
		if pid == "":
			continue
		_packages[pid] = pkg
		if not _states.has(pid):
			_states[pid] = {
				"status": PackageStatus.INSTALLED,
				"version": pkg.get("version", "1.0.0"),
				"path": "res://GameModes/%s" % pid,
				"last_played": "",
			}
	_print("[PackageService] Registered %d builtin packages" % _packages.size())

func get_package(package_id: String) -> Dictionary:
	return _packages.get(package_id, {})

func get_all_packages() -> Array:
	return _packages.values()

func get_packages_by_category(category_id: String) -> Array:
	var categories: Array = _registry.get("categories", [])
	for cat in categories:
		if cat.get("id", "") == category_id:
			var ids: Array = cat.get("packageIds", [])
			var result := []
			for pid in ids:
				if _packages.has(pid):
					result.append(_packages[pid])
			return result
	return []

func get_featured_packages() -> Array:
	var featured: Array = _registry.get("featuredPackages", [])
	var result := []
	for pid in featured:
		if _packages.has(pid):
			result.append(_packages[pid])
	return result []

func get_categories() -> Array:
	return _registry.get("categories", [])

func get_package_state(package_id: String) -> Dictionary:
	return _states.get(package_id, {})

func is_package_installed(package_id: String) -> bool:
	if not _states.has(package_id):
		return false
	var s: Dictionary = _states[package_id]
	return s.get("status", PackageStatus.UNAVAILABLE) == PackageStatus.INSTALLED

func is_package_builtin(package_id: String) -> bool:
	if not _packages.has(package_id):
		return false
	var pkg: Dictionary = _packages[package_id]
	return pkg.get("isFree", true) and pkg.get("type", "") == "base_game"

func can_launch(package_id: String) -> bool:
	if not is_package_installed(package_id):
		return false
	var pkg := get_package(package_id)
	if pkg.is_empty():
		return false
	var deps: Array = pkg.get("dependencies", [])
	for dep in deps:
		if not is_package_installed(dep):
			return false
	return true

func activate_package(package_id: String) -> bool:
	if not can_launch(package_id):
		package_error.emit(package_id, "无法激活：未安装或依赖未满足")
		return false

	if _active_package_id == package_id:
		return true

	if _active_package_id != "":
		deactivate_package(_active_package_id)

	_active_package_id = package_id

	if _states.has(package_id):
		_states[package_id]["last_played"] = Time.get_datetime_string_from_system()

	_load_package_config(package_id)
	_save_states()

	package_activated.emit(package_id)
	print("[PackageService] Activated package: %s" % package_id)
	return true

func deactivate_package(package_id: String) -> void:
	if _active_package_id != package_id:
		return

	_config_cache.erase(package_id)
	_active_package_id = ""

	package_deactivated.emit(package_id)
	print("[PackageService] Deactivated package: %s" % package_id)

func get_active_package_id() -> String:
	return _active_package_id

func get_active_package() -> Dictionary:
	if _active_package_id == "":
		return {}
	return get_package(_active_package_id)

func _load_package_config(package_id: String) -> void:
	if _config_cache.has(package_id):
		return

	var pkg := get_package(package_id)
	if pkg.is_empty():
		return

	var config_path: String = pkg.get("configFile", "")
	if config_path == "":
		var pkg_path: String = _states.get(package_id, {}).get("path", "")
		if pkg_path != "":
			config_path = "%s/Config/Data/package_config.json" % pkg_path

	if config_path == "" or not ResourceLoader.exists(config_path):
		_config_cache[package_id] = {}
		return

	var file := FileAccess.open(config_path, FileAccess.READ)
	if file == null:
		_config_cache[package_id] = {}
		return

	var json := JSON.new()
	if json.parse(file.get_as_text()) != OK:
		_config_cache[package_id] = {}
		return

	_config_cache[package_id] = json.data if json.data is Dictionary else {}

func get_package_config(package_id: String) -> Dictionary:
	if not _config_cache.has(package_id):
		_load_package_config(package_id)
	return _config_cache.get(package_id, {})

func get_gameplay_setting(package_id: String, key: String, default_value: Variant = null) -> Variant:
	var config := get_package_config(package_id)
	if config.is_empty():
		return default_value
	var gameplay: Dictionary = config.get("gameplay", {})
	return gameplay.get(key, default_value)

func set_gameplay_setting(package_id: String, key: String, value: Variant) -> void:
	var config := get_package_config(package_id)
	if config.is_empty():
		return
	if not config.has("gameplay"):
		config["gameplay"] = {}
	config["gameplay"][key] = value
	_save_package_config(package_id, config)

func _save_package_config(package_id: String, config: Dictionary) -> void:
	var pkg := get_package(package_id)
	var config_path: String = pkg.get("configFile", "")
	if config_path == "":
		return

	if config_path.begins_with("res://"):
		print("[PackageService] Cannot write to res:// path: %s" % config_path)
		return

	var json_text := JSON.stringify(config, "\t")
	var file := FileAccess.open(config_path, FileAccess.WRITE)
	if file != null:
		file.store_string(json_text)

func get_entry_scene(package_id: String) -> String:
	var pkg := get_package(package_id)
	return pkg.get("entryScene", "")

func get_character_select_scene(package_id: String) -> String:
	var pkg := get_package(package_id)
	return pkg.get("characterSelectScene", "")

func get_map_scene(package_id: String) -> String:
	var pkg := get_package(package_id)
	return pkg.get("mapScene", "")

func get_save_slots(package_id: String) -> Array:
	var slots := []
	for i in range(3):
		var save_path := "user://saves/%s_slot_%d.json" % [package_id, i + 1]
		var has_save := FileAccess.file_exists(save_path)
		var save_data := {}
		if has_save:
			var file := FileAccess.open(save_path, FileAccess.READ)
			if file != null:
				var json := JSON.new()
				if json.parse(file.get_as_text()) == OK and json.data is Dictionary:
					save_data = json.data
		slots.append({"slot_id": i + 1, "has_save": has_save, "data": save_data})
	return slots

func get_achievements(package_id: String) -> Array:
	var config := get_package_config(package_id)
	if config.is_empty():
		return _default_achievements()
	var ach: Array = config.get("achievements", [])
	if ach.is_empty():
		return _default_achievements()
	return ach

func get_leaderboard(package_id: String) -> Array:
	var config := get_package_config(package_id)
	return config.get("leaderboard", _default_leaderboard())

func get_include_in_build_packages() -> Array:
	var result := []
	for pid in _packages:
		var pkg: Dictionary = _packages[pid]
		if pkg.get("includeInBuild", false):
			result.append(pid)
	return result

func search_packages(query: String) -> Array:
	var lower_query := query.to_lower()
	var result := []
	for pkg in _packages.values():
		var name: String = pkg.get("name", "").to_lower()
		var desc: String = pkg.get("description", "").to_lower()
		var tags: Array = pkg.get("tags", [])
		if name.contains(lower_query) or desc.contains(lower_query):
			result.append(pkg)
			continue
		for tag in tags:
			if tag.to_lower().contains(lower_query):
				result.append(pkg)
				break
	return result

func _default_achievements() -> Array:
	return [
		{"id": "first_victory", "name": "初次胜利", "desc": "首次通关游戏", "unlocked": false},
		{"id": "kill_100", "name": "百人斩", "desc": "击败100个敌人", "unlocked": false},
		{"id": "all_relics", "name": "收藏家", "desc": "收集所有遗物", "unlocked": false},
		{"id": "no_damage", "name": "无伤通关", "desc": "不受伤完成一场战斗", "unlocked": false},
	]

func _default_leaderboard() -> Array:
	return [
		{"rank": 1, "name": "高手玩家", "score": 99999, "floor": 50},
		{"rank": 2, "name": "卡牌大师", "score": 85000, "floor": 45},
		{"rank": 3, "name": "尖塔攀登者", "score": 72000, "floor": 40},
	]
