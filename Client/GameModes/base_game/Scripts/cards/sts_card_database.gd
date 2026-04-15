class_name StsCardDatabase extends RefCounted

enum CardType { ATTACK, SKILL, POWER }
enum CardRarity { BASIC, COMMON, UNCOMMON, RARE, SPECIAL }
enum CardTarget { ENEMY_SINGLE, ENEMY_ALL, SELF, NONE }
enum DamageType { ATTACK, MAGIC }

class CardData:
	var id: String = ""
	var name: String = ""
	var description: String = ""
	var cost: int = 1
	var type: int = CardType.ATTACK
	var rarity: int = CardRarity.COMMON
	var target: int = CardTarget.ENEMY_SINGLE
	var damage: int = 0
	var block: int = 0
	var magic_number: int = 0
	var damage_type: int = DamageType.ATTACK
	var exhaust: bool = false
	var ethereal: bool = false
	var innate: bool = false
	var upgraded: bool = false

	func _init(p_id: String = "", p_name: String = "", p_desc: String = "") -> void:
		id = p_id
		name = p_name
		description = p_desc

static func create_strike() -> CardData:
	var card := CardData.new("Strike_R", "打击", "造成 6 点伤害。")
	card.cost = 1
	card.type = CardType.ATTACK
	card.rarity = CardRarity.BASIC
	card.target = CardTarget.ENEMY_SINGLE
	card.damage = 6
	return card

static func create_defend() -> CardData:
	var card := CardData.new("Defend_R", "防御", "获得 5 点格挡。")
	card.cost = 1
	card.type = CardType.SKILL
	card.rarity = CardRarity.BASIC
	card.target = CardTarget.SELF
	card.block = 5
	return card

static func create_bash() -> CardData:
	var card := CardData.new("Bash", "重击", "造成 8 点伤害。\n施加 2 层脆弱。")
	card.cost = 2
	card.type = CardType.ATTACK
	card.rarity = CardRarity.BASIC
	card.target = CardTarget.ENEMY_SINGLE
	card.damage = 8
	card.magic_number = 2
	return card

var _all_cards: Dictionary = {}

func _ready() -> void:
	_register_ironclad_cards()

func _register_ironclad_cards() -> void:
	_register(create_strike())
	_register(create_defend())
	_register(create_bash())
	_register_attack_cards()
	_register_skill_cards()
	_register_power_cards()

func _register_attack_cards() -> void:
	_register_card("Cleave", "顺劈", "对所有敌人造成 8 点伤害。", 1, CardType.ATTACK, CardRarity.COMMON,
		CardTarget.ENEMY_ALL, 8, 0, 0)
	_register_card("Iron_Wave", "铁波", "造成 5 点伤害。\n获得 5 点格挡。", 1, CardType.ATTACK, CardRarity.COMMON,
		CardTarget.ENEMY_SINGLE, 5, 5, 0)
	_register_card("Pommel_Strike", "柄击", "造成 9 点伤害。\n抽 1 张牌。", 1, CardType.ATTACK, CardRarity.COMMON,
		CardTarget.ENEMY_SINGLE, 9, 0, 1)
	_register_card("Twin_Strike", "双击", "造成 5 点伤害两次。", 1, CardType.ATTACK, CardRarity.COMMON,
		CardTarget.ENEMY_SINGLE, 5, 0, 2)
	_register_card("Anger", "愤怒", "造成 6 点伤害。\n将一张愤怒放入弃牌堆。", 0, CardType.ATTACK, CardRarity.COMMON,
		CardTarget.ENEMY_SINGLE, 6, 0, 0)
	_register_card("Clothesline", "晾衣绳", "造成 12 点伤害。\n施加 2 层虚弱。", 2, CardType.ATTACK, CardRarity.COMMON,
		CardTarget.ENEMY_SINGLE, 12, 0, 2)
	_register_card("Heavy_Blade", "重刃", "造成 14 点伤害。\n力量对此牌的影响为 3 倍。", 2, CardType.ATTACK, CardRarity.COMMON,
		CardTarget.ENEMY_SINGLE, 14, 0, 3)
	_register_card("Body_Slam", "身体撞击", "造成等同于当前格挡的伤害。", 1, CardType.ATTACK, CardRarity.COMMON,
		CardTarget.ENEMY_SINGLE, 0, 0, 0)
	_register_card("Sword_Boomerang", "剑回旋镖", "随机造成 3 次 3 点伤害。", 1, CardType.ATTACK, CardRarity.COMMON,
		CardTarget.ENEMY_ALL, 3, 0, 3)
	_register_card("Perfected_Strike", "完美打击", "造成 6 点伤害。\n手牌中每有一张\"打击\"牌，伤害+2。", 2, CardType.ATTACK, CardRarity.COMMON,
		CardTarget.ENEMY_SINGLE, 6, 0, 2)
	_register_card("Uppercut", "上勾拳", "造成 13 点伤害。\n施加 1 层虚弱。\n施加 1 层脆弱。", 2, CardType.ATTACK, CardRarity.UNCOMMON,
		CardTarget.ENEMY_SINGLE, 13, 0, 1)
	_register_card("Carnage", "杀戮", "消耗。造成 20 点伤害。", 2, CardType.ATTACK, CardRarity.UNCOMMON,
		CardTarget.ENEMY_SINGLE, 20, 0, 0, true)
	_register_card("Reckless_Charge", "鲁莽冲锋", "造成 7 点伤害。\n将一张眩晕放入弃牌堆。", 0, CardType.ATTACK, CardRarity.UNCOMMON,
		CardTarget.ENEMY_SINGLE, 7, 0, 0)
	_register_card("Bludgeon", "重击", "造成 32 点伤害。", 3, CardType.ATTACK, CardRarity.RARE,
		CardTarget.ENEMY_SINGLE, 32, 0, 0)
	_register_card("Impervious", "坚不可摧", "获得 30 点格挡。", 2, CardType.SKILL, CardRarity.RARE,
		CardTarget.SELF, 0, 30, 0, false, true)

func _register_skill_cards() -> void:
	_register_card("Shrug_It_Off", "耸肩", "获得 8 点格挡。\n抽 1 张牌。", 1, CardType.SKILL, CardRarity.COMMON,
		CardTarget.SELF, 0, 8, 1)
	_register_card("Armaments", "武装", "获得 5 点格挡。\n将手牌中的一张牌升级。", 1, CardType.SKILL, CardRarity.COMMON,
		CardTarget.SELF, 0, 5, 1)
	_register_card("Flex", "屈伸", "获得 2 点力量。\n回合结束时失去 2 点力量。", 0, CardType.SKILL, CardRarity.COMMON,
		CardTarget.SELF, 0, 0, 2)
	_register_card("Battle_Trance", "战斗恍惚", "抽 3 张牌。\n本回合不能再抽牌。", 0, CardType.SKILL, CardRarity.UNCOMMON,
		CardTarget.SELF, 0, 0, 3)
	_register_card("Bloodletting", "放血", "失去 3 点生命。\n获得 2 点能量。", 0, CardType.SKILL, CardRarity.UNCOMMON,
		CardTarget.SELF, 0, 0, 2)
	_register_card("Inflame", "燃烧", "获得 2 点力量。", 1, CardType.SKILL, CardRarity.UNCOMMON,
		CardTarget.SELF, 0, 0, 2)
	_register_card("Rage", "愤怒", "每打出一张攻击牌，获得 3 点格挡。", 0, CardType.SKILL, CardRarity.UNCOMMON,
		CardTarget.SELF, 0, 0, 3)
	_register_card("Flame_Barrier", "火焰屏障", "获得 12 点格挡。\n受到攻击时造成 4 点伤害。", 2, CardType.SKILL, CardRarity.UNCOMMON,
		CardTarget.SELF, 0, 12, 4)
	_register_card("Ghostly_Armor", "幽灵护甲", "虚无。获得 10 点格挡。", 1, CardType.SKILL, CardRarity.UNCOMMON,
		CardTarget.SELF, 0, 10, 0, false, false, true)
	_register_card("Entrench", "挖掘", "将当前格挡翻倍。", 2, CardType.SKILL, CardRarity.UNCOMMON,
		CardTarget.SELF, 0, 0, 0)

func _register_power_cards() -> void:
	_register_card("Inflame_Power", "燃烧", "获得 2 点力量。", 1, CardType.POWER, CardRarity.UNCOMMON,
		CardTarget.SELF, 0, 0, 2)
	_register_card("Metallicize", "金属化", "回合结束时获得 3 点格挡。", 1, CardType.POWER, CardRarity.UNCOMMON,
		CardTarget.SELF, 0, 0, 3)
	_register_card("Combust", "燃烧", "回合开始时失去 1 点生命，对所有敌人造成 5 点伤害。", 1, CardType.POWER, CardRarity.UNCOMMON,
		CardTarget.SELF, 5, 0, 1)
	_register_card("Dark_Embrace", "黑暗契约", "每消耗一张牌，抽 1 张牌。", 2, CardType.POWER, CardRarity.UNCOMMON,
		CardTarget.SELF, 0, 0, 1)
	_register_card("Feel_No_Pain", "无痛", "每消耗一张牌，获得 3 点格挡。", 1, CardType.POWER, CardRarity.UNCOMMON,
		CardTarget.SELF, 0, 0, 3)
	_register_card("Demon_Form", "恶魔形态", "回合开始时获得 2 点力量。", 3, CardType.POWER, CardRarity.RARE,
		CardTarget.SELF, 0, 0, 2)
	_register_card("Barricade", "壁垒", "格挡不再在回合开始时清除。", 3, CardType.POWER, CardRarity.RARE,
		CardTarget.SELF, 0, 0, 0)
	_register_card("Brutality", "残暴", "回合开始时失去 1 点生命，抽 1 张牌。", 1, CardType.POWER, CardRarity.RARE,
		CardTarget.SELF, 0, 0, 1)

func _register_card(id: String, name: String, desc: String, cost: int, type: int, rarity: int,
		target: int, damage: int, block: int, magic: int, exhaust: bool = false,
		ethereal: bool = false, innate: bool = false) -> void:
	var card := CardData.new(id, name, desc)
	card.cost = cost
	card.type = type
	card.rarity = rarity
	card.target = target
	card.damage = damage
	card.block = block
	card.magic_number = magic
	card.exhaust = exhaust
	card.ethereal = ethereal
	card.innate = innate
	_register(card)

func _register(card: CardData) -> void:
	_all_cards[card.id] = card

func get_card(id: String) -> CardData:
	return _all_cards.get(id) if _all_cards.has(id) else null

func get_all_cards() -> Array:
	return _all_cards.values().duplicate()

func get_cards_by_rarity(rarity: int) -> Array:
	var result := []
	for card in _all_cards.values():
		if card.rarity == rarity:
			result.append(card)
	return result

func get_cards_by_type(type: int) -> Array:
	var result := []
	for card in _all_cards.values():
		if card.type == type:
			result.append(card)
	return result

func get_starter_deck() -> Array:
	var deck := []
	for i in range(5):
		deck.append(get_card("Strike_R"))
	for i in range(4):
		deck.append(get_card("Defend_R"))
	deck.append(get_card("Bash"))
	return deck

func get_random_reward_cards(count: int, rng: RandomNumberGenerator) -> Array:
	var pool: Array = _all_cards.values().duplicate()
	pool.shuffle()
	var result := []
	for i in range(mini(count, pool.size())):
		result.append(pool[i])
	return result
