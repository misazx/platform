extends Node
signal database_initialized()

func _ready() -> void:
	initialize_all_databases()

func initialize_all_databases() -> void:
	print("[GameDatabaseManager] Initializing all databases...")

	call_deferred("_on_initialization_complete")
	print("[GameDatabaseManager] All databases initialized via autoload!")

func _on_initialization_complete() -> void:
	database_initialized.emit()

	var report := generate_database_report()
	print(report)

func get_character(id: String) -> Dictionary:
	return CharacterDatabase.get_character(id)

func get_card(id: String) -> Dictionary:
	return CardDatabase.get_card(id)

func get_enemy(id: String) -> Dictionary:
	return EnemyDatabase.get_enemy(id)

func get_relic(id: String) -> Dictionary:
	return RelicDatabase.get_relic(id)

func get_potion(id: String) -> Dictionary:
	return PotionDatabase.get_potion(id)

func get_event(id: String) -> Dictionary:
	return EventDatabase.get_event(id)

func get_all_characters() -> Array:
	return CharacterDatabase.get_all_characters()

func get_all_cards() -> Array:
	return CardDatabase.get_all_cards()

func get_all_enemies() -> Array:
	return EnemyDatabase.get_all_enemies()

func get_all_relics() -> Array:
	return RelicDatabase.get_all_relics()

func get_all_potions() -> Array:
	return PotionDatabase.get_all_potions()

func get_all_events() -> Array:
	return EventDatabase.get_all_events()

func generate_database_report() -> String:
	var lines := []
	lines.append("=== 游戏数据库报告 ===")
	lines.append("生成时间: %s" % Time.get_datetime_string_from_system())
	lines.append("")
	lines.append("--- 角色系统 ---")
	lines.append("总角色数: %d" % CharacterDatabase.get_total_characters())
	for char_data in CharacterDatabase.get_all_characters():
		lines.append("  - %s (%d)" % [char_data["name"], char_data["class"]])
	lines.append("")
	lines.append("--- 卡牌系统 ---")
	lines.append("总卡牌数: %d" % CardDatabase.total_cards())
	lines.append("")
	lines.append("--- 敌人系统 ---")
	lines.append("总敌人数: %d" % EnemyDatabase.get_total_enemies())
	lines.append("  - 普通敌人: %d" % EnemyDatabase.get_normal_enemies().size())
	lines.append("  - 精英敌人: %d" % EnemyDatabase.get_elite_enemies().size())
	lines.append("  - Boss敌人: %d" % EnemyDatabase.get_boss_enemies().size())
	lines.append("")
	lines.append("--- 遗物系统 ---")
	lines.append("总遗物数: %d" % RelicDatabase.get_total_relics())
	lines.append("")
	lines.append("--- 药水系统 ---")
	lines.append("总药水数: %d" % PotionDatabase.get_total_potions())
	lines.append("")
	lines.append("--- 事件系统 ---")
	lines.append("总事件数: %d" % EventDatabase.get_total_events())
	lines.append("")
	lines.append("--- 成就系统 ---")
	lines.append("总成就数: %d" % AchievementSystem.get_total_achievements())
	lines.append("已解锁: %d" % AchievementSystem.get_unlocked_count())
	lines.append("完成度: %.1f%%" % AchievementSystem.get_overall_completion())

	return "\n".join(lines)
