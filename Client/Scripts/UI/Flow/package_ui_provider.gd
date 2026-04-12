class_name PackageUIRegistry extends Node

var _providers: Dictionary = {}
static var instance: PackageUIRegistry = null

func _ready() -> void:
	if instance != null and instance != self:
		queue_free()
		return
	instance = self
	_register_default_providers()

func _register_default_providers() -> void:
	var config_path := "res://Config/Data/package_registry.json"
	var packages := _load_packages_from_config(config_path)
	for pkg in packages:
		var provider := _create_provider_from_config(pkg)
		if provider != null:
			_providers[provider.package_id] = provider
			print("[PackageUIRegistry] Registered provider: %s" % provider.package_id)

func _load_packages_from_config(path: String) -> Array:
	if not ResourceLoader.exists(path):
		return []
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		return []
	var json := JSON.new()
	if json.parse(file.get_as_text()) != OK:
		return []
	var data := json.data as Dictionary
	if data == null:
		return []
	return data.get("packages", [])

func _create_provider_from_config(pkg: Dictionary) -> PackageProvider:
	var provider := PackageProvider.new()
	provider.package_id = pkg.get("id", "")
	provider.package_name = pkg.get("name", "")
	provider.entry_scene = pkg.get("entryScene", "")
	provider.character_select_scene = pkg.get("characterSelectScene", "")
	provider.map_scene = pkg.get("mapScene", "")
	provider.is_builtin = pkg.get("isFree", true) and pkg.get("type", "") == "base_game"
	if provider.package_id == "":
		return null
	return provider

func register_provider(provider: PackageProvider) -> void:
	_providers[provider.package_id] = provider
	print("[PackageUIRegistry] Registered provider: %s" % provider.package_id)

func get_provider(package_id: String) -> PackageProvider:
	if _providers.has(package_id):
		return _providers[package_id]
	return null

func get_all_providers() -> Array:
	return _providers.values()

func has_provider(package_id: String) -> bool:
	return _providers.has(package_id)


class PackageProvider extends RefCounted:

	var package_id: String = ""
	var package_name: String = ""
	var entry_scene: String = ""
	var character_select_scene: String = ""
	var map_scene: String = ""
	var is_builtin: bool = false

	func get_save_slots() -> Array:
		var slots := []
		for i in range(3):
			slots.append({"slot_id": i + 1, "data": {}, "has_save": false})
		return slots

	func get_achievement_data() -> Array:
		return [
			{"id": "first_victory", "name": "初次胜利", "desc": "首次通关游戏", "unlocked": false},
			{"id": "kill_100", "name": "百人斩", "desc": "击败100个敌人", "unlocked": false},
			{"id": "all_relics", "name": "收藏家", "desc": "收集所有遗物", "unlocked": false},
			{"id": "no_damage", "name": "无伤通关", "desc": "不受伤完成一场战斗", "unlocked": false},
		]

	func get_leaderboard_data() -> Array:
		return [
			{"rank": 1, "name": "高手玩家", "score": 99999, "floor": 50},
			{"rank": 2, "name": "卡牌大师", "score": 85000, "floor": 45},
			{"rank": 3, "name": "尖塔攀登者", "score": 72000, "floor": 40},
			{"rank": 4, "name": "铁甲战士", "score": 60000, "floor": 35},
			{"rank": 5, "name": "静默猎人", "score": 48000, "floor": 30},
		]

	func on_package_activated() -> void:
		print("[PackageProvider] Package activated: %s" % package_id)

	func on_package_deactivated() -> void:
		print("[PackageProvider] Package deactivated: %s" % package_id)
