enum RestOption { HEAL, UPGRADE, SMITH, RECALL }

class_name RestSiteManager extends Node

signal rest_completed(option: int)

func _ready() -> void:
	pass

func get_available_options(player: Node) -> Array:
	var options := [RestOption.HEAL, RestOption.UPGRADE]
	if has_recall_option(player):
		options.append(RestOption.RECALL)
	return options

func has_recall_option(player: Node) -> bool:
	if player != null and player.has_method("has_relic") and player.call("has_relic", "dream_catcher"):
		return true
	return false

func perform_rest_action(option: int, player: Node) -> void:
	match option:
		RestOption.HEAL:
			heal_player(player)
		RestOption.UPGRADE:
			open_upgrade_ui(player)
		RestOption.RECALL:
			recall_cards(player)

	rest_completed.emit(option)
	GD.print("[RestSiteManager] Performed rest action: %d" % option)

func heal_player(player: Node) -> void:
	var heal_amount := 30
	if player != null and player.has_method("heal"):
		player.call("heal", heal_amount)
		GD.print("[RestSiteManager] Healed %d HP" % heal_amount)

func open_upgrade_ui(player: Node) -> void:
	GD.print("[RestSiteManager] Opening upgrade UI...")
	if player != null and player.has_method("upgrade_random_card"):
		player.call("upgrade_random_card")

func recall_cards(player: Node) -> void:
	if player != null and player.has_method("recall_discarded_cards"):
		player.call("recall_discarded_cards")
		GD.print("[RestSiteManager] Recalled discarded cards")

func get_rest_option_description(option: int) -> String:
	match option:
		RestOption.HEAL: return "回复 30% 最大生命值"
		RestOption.UPGRADE: return "升级一张牌"
		RestOption.RECALL: return "将弃牌堆中的所有牌收回手牌"
		RestOption.SMITH: return "移除一张牌（需要特定遗物）"
		_: return "未知选项"

func get_rest_option_icon(option: int) -> String:
	match option:
		RestOption.HEAL: return "res://GameModes/base_game/Resources/Icons/Rest/heal.png"
		RestOption.UPGRADE: return "res://GameModes/base_game/Resources/Icons/Rest/upgrade.png"
		RestOption.RECALL: return "res://GameModes/base_game/Resources/Icons/Rest/recall.png"
		RestOption.SMITH: return "res://GameModes/base_game/Resources/Icons/Rest/smith.png"
		_: return "res://GameModes/base_game/Resources/Icons/Rest/default.png"
