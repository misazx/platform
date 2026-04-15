extends Node
enum EventType { CHOICE, COMBAT, SHOP, REST, TREASURE, SPECIAL, CURSE }


signal event_triggered(event_id: String)

var _events: Dictionary = {}
var _type_events: Dictionary = {}

func _ready() -> void:
	load_events_from_config()

func load_events_from_config() -> void:
	var config := ConfigLoader.load_config("events")
	if config.is_empty():
		push_error("[EventDatabase] Failed to load events config!")
		return

	if not config.has("events"):
		return

	for event_cfg in config["events"]:
		var event_data := convert_config_to_data(event_cfg)
		register_event(event_data)

	print("[EventDatabase] Loaded %d events from config (version: %s)" % [_events.size(), config.get("version", "")])

func convert_config_to_data(cfg: Dictionary) -> Dictionary:
	var choices := []
	if cfg.has("choices"):
		for c in cfg["choices"]:
			choices.append({
				"text": c.get("text", ""),
				"description": c.get("description", ""),
				"rewards": Dictionary(c.get("rewards", {})),
				"penalties": Dictionary(c.get("penalties", {})),
				"requires_condition": bool(c.get("requiresCondition", false)),
				"condition_key": c.get("conditionKey", ""),
				"condition_value": c.get("conditionValue", null)
			})

	return {
		"id": cfg.get("id", ""),
		"name": cfg.get("name", ""),
		"description": cfg.get("description", ""),
		"flavor_text": cfg.get("flavorText", ""),
		"type": parse_event_type(cfg.get("type", "choice")),
		"image_path": cfg.get("imagePath", ""),
		"location": cfg.get("location", ""),
		"choices": choices,
		"custom_data": Dictionary(cfg.get("customData", {})),
		"weight": float(cfg.get("weight", 1.0)),
		"one_time": bool(cfg.get("oneTime", false)),
		"has_seen": bool(cfg.get("hasSeen", false))
	}

func parse_event_type(type_str: String) -> int:
	match type_str.to_lower():
		"choice": return EventType.CHOICE
		"combat": return EventType.COMBAT
		"shop": return EventType.SHOP
		"rest": return EventType.REST
		"treasure": return EventType.TREASURE
		"special": return EventType.SPECIAL
		"curse": return EventType.CURSE
		_: return EventType.CHOICE

func register_event(event_data: Dictionary) -> void:
	_events[event_data["id"]] = event_data
	var event_type: int = event_data["type"]
	if not _type_events.has(event_type):
		_type_events[event_type] = []
	_type_events[event_type].append(event_data)

func get_event(event_id: String) -> Dictionary:
	if _events.has(event_id):
		return _events[event_id]
	return {}

func get_all_events() -> Array:
	return _events.values()

func get_events_by_type(type_val: int) -> Array:
	if _type_events.has(type_val):
		return _type_events[type_val]
	return []

func get_events_by_location(location: String) -> Array:
	var result := []
	for ev in _events.values():
		if ev["location"] == location or ev["location"] == "anywhere":
			result.append(ev)
	return result

func get_random_event(location: String) -> Dictionary:
	var available := get_events_by_location(location)
	if available.is_empty():
		return {}
	return available[randi() % available.size()]

func get_total_events() -> int:
	return _events.size()
