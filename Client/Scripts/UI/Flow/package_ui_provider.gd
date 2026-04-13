extends Node

var _providers: Dictionary = {}
static var instance: Node = null

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

func _create_provider_from_config(pkg: Dictionary) -> Dictionary:
	var provider := {}
	provider["package_id"] = pkg.get("id", "")
	provider["package_name"] = pkg.get("name", "")
	provider["entry_scene"] = pkg.get("entryScene", "")
	provider["character_select_scene"] = pkg.get("characterSelectScene", "")
	provider["map_scene"] = pkg.get("mapScene", "")
	provider["is_builtin"] = pkg.get("isFree", true) and pkg.get("type", "") == "base_game"
	if provider["package_id"] == "":
		return {}
	return provider

func register_provider(provider: Dictionary) -> void:
	_providers[provider["package_id"]] = provider
	print("[PackageUIRegistry] Registered provider: %s" % provider["package_id"])

func get_provider(package_id: String) -> Dictionary:
	if _providers.has(package_id):
		return _providers[package_id]
	return {}

func get_all_providers() -> Array:
	return _providers.values()

func has_provider(package_id: String) -> bool:
	return _providers.has(package_id)
