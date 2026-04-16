class_name StsExpansionSystem extends RefCounted

enum Character { IRONCLAD, SILENT, DEFECT, WATCHER }

class CharacterData:
	var character: int = Character.IRONCLAD
	var name: String = ""
	var max_hp: int = 80
	var starting_gold: int = 99
	var starting_relic_id: String = ""
	var starting_deck: Array = []
	var description: String = ""
	var portrait_path: String = ""
	var color: Color = Color.WHITE

	static func create_ironclad() -> CharacterData:
		var data := CharacterData.new()
		data.character = Character.IRONCLAD
		data.name = "铁甲战士"
		data.max_hp = 80
		data.starting_gold = 99
		data.starting_relic_id = "Burning_Blood"
		data.description = "红发的战士，擅长力量和生命回复"
		data.portrait_path = "res://GameModes/base_game/Resources/Images/Characters/Ironclad.png"
		data.color = Color(1, 0.27, 0.27)
		data.starting_deck = ["Strike_R", "Strike_R", "Strike_R", "Strike_R", "Strike_R",
			"Defend_R", "Defend_R", "Defend_R", "Defend_R", "Bash"]
		return data

	static func create_silent() -> CharacterData:
		var data := CharacterData.new()
		data.character = Character.SILENT
		data.name = "静默猎人"
		data.max_hp = 70
		data.starting_gold = 99
		data.starting_relic_id = "Snake_Ring"
		data.description = "致命的猎人，擅长毒素和防御"
		data.portrait_path = "res://GameModes/base_game/Resources/Images/Characters/Silent.png"
		data.color = Color(0, 0.67, 0)
		data.starting_deck = ["Strike_G", "Strike_G", "Strike_G", "Strike_G", "Strike_G",
			"Defend_G", "Defend_G", "Defend_G", "Defend_G", "Defend_G", "Survivor"]
		return data

	static func create_defect() -> CharacterData:
		var data := CharacterData.new()
		data.character = Character.DEFECT
		data.name = "缺陷机器人"
		data.max_hp = 75
		data.starting_gold = 99
		data.starting_relic_id = "Cracked_Core"
		data.description = "觉醒的机器人，擅长充能和冰霜"
		data.portrait_path = "res://GameModes/base_game/Resources/Images/Characters/Defect.png"
		data.color = Color(0.27, 0.27, 1)
		data.starting_deck = ["Strike_B", "Strike_B", "Strike_B", "Strike_B",
			"Defend_B", "Defend_B", "Defend_B", "Defend_B", "Zap", "Dualcast"]
		return data

	static func create_watcher() -> CharacterData:
		var data := CharacterData.new()
		data.character = Character.WATCHER
		data.name = "观者"
		data.max_hp = 72
		data.starting_gold = 99
		data.starting_relic_id = "Holy_Water"
		data.description = "盲眼的圣女，擅长姿态和预见"
		data.portrait_path = "res://GameModes/base_game/Resources/Images/Characters/Watcher.png"
		data.color = Color(0.67, 0.27, 1)
		data.starting_deck = ["Strike_P", "Strike_P", "Strike_P", "Strike_P",
			"Defend_P", "Defend_P", "Defend_P", "Defend_P", "Eruption", "Vigilance"]
		return data

	static func get_character_data(character: int) -> CharacterData:
		match character:
			Character.IRONCLAD: return create_ironclad()
			Character.SILENT: return create_silent()
			Character.DEFECT: return create_defect()
			Character.WATCHER: return create_watcher()
			_: return create_ironclad()


class BossData:
	var id: String = ""
	var name: String = ""
	var max_hp: int = 0
	var current_hp: int = 0
	var block: int = 0
	var status_effects: Array = []
	var current_intent: Dictionary = {}
	var phase: int = 1
	var is_boss: bool = true

	static func create_the_guardian() -> BossData:
		var data := BossData.new()
		data.id = "The_Guardian"
		data.name = "守护者"
		data.max_hp = 240
		data.current_hp = 240
		return data

	static func create_hexaghost() -> BossData:
		var data := BossData.new()
		data.id = "Hexaghost"
		data.name = "六火幽灵"
		data.max_hp = 250
		data.current_hp = 250
		return data

	static func create_slime_boss() -> BossData:
		var data := BossData.new()
		data.id = "Slime_Boss"
		data.name = "史莱姆王"
		data.max_hp = 140
		data.current_hp = 140
		return data

	func apply_damage(damage: int) -> void:
		if block > 0:
			if damage >= block:
				damage -= block
				block = 0
			else:
				block -= damage
				damage = 0
		current_hp -= damage

	func is_dead() -> bool:
		return current_hp <= 0


class PotionData:
	enum PotionType { ATTACK, SKILL, POWER, UNKNOWN }
	
	var id: String = ""
	var name: String = ""
	var description: String = ""
	var type: int = PotionType.UNKNOWN
	var value: int = 0
	var can_discard: bool = true
	var requires_target: bool = false

	static func create_health_potion() -> PotionData:
		var potion := PotionData.new()
		potion.id = "Health_Potion"
		potion.name = "治疗药水"
		potion.description = "回复 15 点生命。"
		potion.type = PotionType.SKILL
		potion.value = 15
		return potion

	static func create_fire_potion() -> PotionData:
		var potion := PotionData.new()
		potion.id = "Fire_Potion"
		potion.name = "火焰药水"
		potion.description = "造成 20 点伤害。"
		potion.type = PotionType.ATTACK
		potion.value = 20
		potion.requires_target = true
		return potion

	static func create_block_potion() -> PotionData:
		var potion := PotionData.new()
		potion.id = "Block_Potion"
		potion.name = "格挡药水"
		potion.description = "获得 12 点格挡。"
		potion.type = PotionType.SKILL
		potion.value = 12
		return potion

	static func create_strength_potion() -> PotionData:
		var potion := PotionData.new()
		potion.id = "Strength_Potion"
		potion.name = "力量药水"
		potion.description = "获得 2 点力量。"
		potion.type = PotionType.POWER
		potion.value = 2
		return potion

	static func create_dexterity_potion() -> PotionData:
		var potion := PotionData.new()
		potion.id = "Dexterity_Potion"
		potion.name = "敏捷药水"
		potion.description = "获得 2 点敏捷。"
		potion.type = PotionType.POWER
		potion.value = 2
		return potion

	static func create_energy_potion() -> PotionData:
		var potion := PotionData.new()
		potion.id = "Energy_Potion"
		potion.name = "能量药水"
		potion.description = "获得 2 点能量。"
		potion.type = PotionType.SKILL
		potion.value = 2
		return potion

	static func create_explosive_potion() -> PotionData:
		var potion := PotionData.new()
		potion.id = "Explosive_Potion"
		potion.name = "爆炸药水"
		potion.description = "对所有敌人造成 10 点伤害。"
		potion.type = PotionType.ATTACK
		potion.value = 10
		return potion

	static func create_fear_potion() -> PotionData:
		var potion := PotionData.new()
		potion.id = "Fear_Potion"
		potion.name = "恐惧药水"
		potion.description = "施加 3 层脆弱。"
		potion.type = PotionType.SKILL
		potion.value = 3
		potion.requires_target = true
		return potion

	static func get_random_potions(count: int, rng: RandomNumberGenerator) -> Array:
		var all_potions := [
			create_health_potion(), create_fire_potion(), create_block_potion(),
			create_strength_potion(), create_dexterity_potion(), create_energy_potion(),
			create_explosive_potion(), create_fear_potion()
		]
		all_potions.shuffle()
		var result := []
		for i in range(mini(count, all_potions.size())):
			result.append(all_potions[i])
		return result


class EventData:
	var id: String = ""
	var title: String = ""
	var description: String = ""
	var image_path: String = ""
	var options: Array = []

	static func create_big_fish() -> EventData:
		var event := EventData.new()
		event.id = "Big_Fish"
		event.title = "大鱼"
		event.description = "你发现了一条巨大的鱼躺在地上。它看起来很新鲜。"
		event.options = [_create_option("吃掉它", "回复 5 点生命，获得 1 张伤口牌", 0, 0, 0, 0, 0),
			_create_option("离开", "获得 50 金币", 50, 0, 0, 0, 0),
			_create_option("无视", "什么也没发生", 0, 0, 0, 0, 0)]
		return event

	static func create_the_cleric() -> EventData:
		var event := EventData.new()
		event.id = "The_Cleric"
		event.title = "牧师"
		event.description = "一位友善的牧师向你招手。"
		event.options = [_create_option("治疗 (花费 50 金币)", "回复 25% 最大生命", 0, 50, 0, 0, 0),
			_create_option("移除卡牌 (花费 75 金币)", "从卡组中移除一张牌", 0, 75, 0, 0, 0),
			_create_option("离开", "什么也没发生", 0, 0, 0, 0, 0)]
		return event

	static func create_golden_shrine() -> EventData:
		var event := EventData.new()
		event.id = "Golden_Shrine"
		event.title = "金色神殿"
		event.description = "一个闪耀着金光的神殿出现在你面前。"
		event.options = [_create_option("祈祷", "获得 100 金币，获得 1 张悔恨牌", 100, 0, 0, 0, 0),
			_create_option("偷窃", "获得 275 金币，受到 5 点伤害", 275, 0, 5, 0, 0),
			_create_option("离开", "什么也没发生", 0, 0, 0, 0, 0)]
		return event

	static func create_dead_adventurer() -> EventData:
		var event := EventData.new()
		event.id = "Dead_Adventurer"
		event.title = "死去的冒险者"
		event.description = "你发现了一具冒险者的尸体。"
		event.options = [_create_option("搜查尸体", "获得 50 金币", 50, 0, 0, 0, 0),
			_create_option("埋葬", "获得 1 点最大生命", 0, 0, 0, 1, 0),
			_create_option("离开", "什么也没发生", 0, 0, 0, 0, 0)]
		return event

	static func create_the_mushrooms() -> EventData:
		var event := EventData.new()
		event.id = "The_Mushrooms"
		event.title = "蘑菇"
		event.description = "你发现了一片奇怪的蘑菇。"
		event.options = [_create_option("吃下蘑菇", "回复 15 点生命，获得 1 张奇怪蘑菇遗物", 0, 0, 0, 0, 0),
			_create_option("无视", "什么也没发生", 0, 0, 0, 0, 0)]
		return event

	static func _create_option(text: String, effect: String, gold_reward: int, gold_cost: int,
			damage: int, max_hp_bonus: int, heal: int) -> Dictionary:
		return {
			"text": text,
			"effect": effect,
			"gold_reward": gold_reward,
			"gold_cost": gold_cost,
			"damage_taken": damage,
			"max_hp_bonus": max_hp_bonus,
			"heal_amount": heal
		}

	static func get_random_events(count: int, rng: RandomNumberGenerator) -> Array:
		var all_events := [create_big_fish(), create_the_cleric(), create_golden_shrine(),
			create_dead_adventurer(), create_the_mushrooms()]
		all_events.shuffle()
		var result := []
		for i in range(mini(count, all_events.size())):
			result.append(all_events[i])
		return result
