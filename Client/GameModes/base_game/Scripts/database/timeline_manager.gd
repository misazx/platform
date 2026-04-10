enum TimelineEventType { COMBAT_START, COMBAT_END, CARD_PLAYED, RELIC_OBTAINED, POTION_USED, EVENT_TRIGGERED, SHOP_VISIT, REST_USED, BOSS_DEFEATED, DEATH, VICTORY, LEVEL_UP, CUSTOM }

class_name TimelineManager extends Node

signal timeline_entry_added(entry_id: String)

var _timeline: Array = []
var _typed_entries: Dictionary = {}

func _ready() -> void:
	pass

func add_entry(type_val: int, title: String, description: String = "", data: Dictionary = {}, floor: int = 0, room: int = 0) -> Dictionary:
	var entry := {
		"id": str(Time.get_ticks_msec()) + "_" + str(randi()),
		"type": type_val,
		"timestamp": Time.get_datetime_string_from_system(),
		"title": title,
		"description": description,
		"data": data if not data.is_empty() else {},
		"floor": floor,
		"room": room
	}

	_timeline.append(entry)
	if not _typed_entries.has(type_val):
		_typed_entries[type_val] = []
	_typed_entries[type_val].append(entry)
	timeline_entry_added.emit(entry["id"])
	return entry

func add_combat_start(floor: int, room: int, enemies: Array) -> void:
	add_entry(TimelineEventType.COMBAT_START, "战斗开始 - 第%d层 房间%d" % [floor, room], "遭遇敌人: %s" % [", ".join(enemies)], {"enemies": enemies}, floor, room)

func add_combat_end(floor: int, room: int, victory: bool, damage_taken: int, damage_dealt: int) -> void:
	var title := "战斗胜利" if victory else "战斗失败"
	add_entry(TimelineEventType.COMBAT_END, title, "造成 %d 点伤害，承受 %d 点伤害" % [damage_dealt, damage_taken], {"victory": victory, "damage_taken": damage_taken, "damage_dealt": damage_dealt}, floor, room)

func add_card_played(card_name: String, cost: int, floor: int, room: int) -> void:
	add_entry(TimelineEventType.CARD_PLAYED, "打出卡牌: %s" % card_name, "消耗 %d 点能量" % cost, {"card_name": card_name, "cost": cost}, floor, room)

func add_relic_obtained(relic_name: String, source: String, floor: int, room: int) -> void:
	add_entry(TimelineEventType.RELIC_OBTAINED, "获得遗物: %s" % relic_name, "来源: %s" % source, {"relic_name": relic_name, "source": source}, floor, room)

func add_potion_used(potion_name: String, floor: int, room: int) -> void:
	add_entry(TimelineEventType.POTION_USED, "使用药水: %s" % potion_name, "", {"potion_name": potion_name}, floor, room)

func add_event_triggered(event_name: String, choice: String, floor: int, room: int) -> void:
	add_entry(TimelineEventType.EVENT_TRIGGERED, "触发事件: %s" % event_name, "选择: %s" % choice, {"event_name": event_name, "choice": choice}, floor, room)

func add_boss_defeated(boss_name: String, floor: int) -> void:
	add_entry(TimelineEventType.BOSS_DEFEATED, "击败Boss: %s" % boss_name, "第 %d 层Boss战胜利" % floor, {"boss_name": boss_name}, floor, 0)

func add_death(floor: int, room: int, cause: String) -> void:
	add_entry(TimelineEventType.DEATH, "死亡", "死亡原因: %s" % cause, {"cause": cause}, floor, room)

func add_victory(seed: uint, total_damage: int, total_cards_played: int) -> void:
	add_entry(TimelineEventType.VICTORY, "游戏通关！", "种子: %d, 总伤害: %d, 出牌数: %d" % [seed, total_damage, total_cards_played], {"seed": seed, "total_damage": total_damage, "total_cards_played": total_cards_played})

func get_all_entries() -> Array:
	return _timeline.duplicate()

func get_entries_by_type(type_val: int) -> Array:
	if _typed_entries.has(type_val):
		return _typed_entries[type_val]
	return []

func get_recent_entries(count: int = 20) -> Array:
	var start := maxi(0, _timeline.size() - count)
	return _timeline.slice(start, mini(count, _timeline.size() - start))

func get_floor_timeline(floor: int) -> Array:
	var result := []
	for entry in _timeline:
		if entry["floor"] == floor:
			result.append(entry)
	return result

func clear_timeline() -> void:
	_timeline.clear()
	_typed_entries.clear()
	GD.print("[TimelineManager] Timeline cleared")

func get_total_entries() -> int:
	return _timeline.size()
