class_name IPackageUIProvider extends RefCounted

func get_package_id() -> String:
	return ""

func get_entry_scene_path() -> String:
	return ""

func get_character_select_scene_path() -> String:
	return ""

func get_map_scene_path() -> String:
	return ""

func get_achievement_data() -> Array:
	return []

func get_leaderboard_data() -> Array:
	return []

func get_save_slots() -> Array:
	return []

func on_package_activated() -> void:
	pass

func on_package_deactivated() -> void:
	pass


class_name BaseGameUIProvider extends IPackageUIProvider

func get_package_id() -> String:
	return "base_game"

func get_entry_scene_path() -> String:
	return "res://GameModes/base_game/Scenes/CharacterSelect.tscn"

func get_character_select_scene_path() -> String:
	return "res://GameModes/base_game/Scenes/CharacterSelect.tscn"

func get_map_scene_path() -> String:
	return "res://GameModes/base_game/Scenes/MapScene.tscn"

func get_achievement_data() -> Array:
	var data := []
	if AchievementSystem.instance != null:
		data = AchievementSystem.instance.get_all_achievements()
	return data

func get_leaderboard_data() -> Array:
	return [
		{"rank": 1, "name": "高手玩家", "score": 99999, "floor": 50},
		{"rank": 2, "name": "卡牌大师", "score": 85000, "floor": 45},
		{"rank": 3, "name": "尖塔攀登者", "score": 72000, "floor": 40},
		{"rank": 4, "name": "铁甲战士", "score": 60000, "floor": 35},
		{"rank": 5, "name": "静默猎人", "score": 48000, "floor": 30},
	]

func get_save_slots() -> Array:
	var slots := []
	for i in range(3):
		var save_data := {}
		if EnhancedSaveSystem.instance != null:
			save_data = EnhancedSaveSystem.instance.load_game(i + 1)
		slots.append({
			"slot_id": i + 1,
			"data": save_data,
			"has_save": not save_data.is_empty()
		})
	return slots

func on_package_activated() -> void:
	GD.print("[BaseGameUIProvider] Package activated")

func on_package_deactivated() -> void:
	GD.print("[BaseGameUIProvider] Package deactivated")


class_name LightShadowUIProvider extends IPackageUIProvider

func get_package_id() -> String:
	return "light_shadow_traveler"

func get_entry_scene_path() -> String:
	return "res://GameModes/light_shadow_traveler/Scenes/GameScene.tscn"

func get_character_select_scene_path() -> String:
	return ""

func get_map_scene_path() -> String:
	return ""

func get_achievement_data() -> Array:
	return []

func get_leaderboard_data() -> Array:
	return [
		{"rank": 1, "name": "光影大师", "score": 15000, "level": 50},
		{"rank": 2, "name": "暗影行者", "score": 12000, "level": 45},
		{"rank": 3, "name": "光明使者", "score": 9500, "level": 38},
	]

func get_save_slots() -> Array:
	var slots := []
	for i in range(3):
		slots.append({"slot_id": i + 1, "data": {}, "has_save": false})
	return slots

func on_package_activated() -> void:
	GD.print("[LightShadowUIProvider] Package activated")

func on_package_deactivated() -> void:
	GD.print("[LightShadowUIProvider] Package deactivated")


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
	register_provider(BaseGameUIProvider.new())
	register_provider(LightShadowUIProvider.new())

func register_provider(provider: IPackageUIProvider) -> void:
	_providers[provider.get_package_id()] = provider
	GD.print("[PackageUIRegistry] Registered provider: %s" % provider.get_package_id())

func get_provider(package_id: String) -> IPackageUIProvider:
	return _providers.get(package_id) if _providers.has(package_id) else null

func get_all_providers() -> Array:
	return _providers.values()

func has_provider(package_id: String) -> bool:
	return _providers.has(package_id)
