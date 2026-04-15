class_name StsRelicSystem extends Node

enum RelicRarity { COMMON, UNCOMMON, RARE, BOSS, SPECIAL }
enum RelicTrigger { ON_COMBAT_START, ON_COMBAT_END, ON_TURN_START, ON_TURN_END, ON_CARD_PLAYED, ON_ATTACK, ON_DAMAGE_DEALT, ON_DAMAGE_TAKEN, ON_KILL, ON_PICKUP, ON_REST, ON_CHEST_OPEN }

class RelicData:
	var id: String = ""
	var name: String = ""
	var description: String = ""
	var flavor_text: String = ""
	var rarity: int = RelicRarity.COMMON
	var icon_path: String = ""
	var on_combat_start: Callable
	var on_combat_end: Callable
	var on_turn_start: Callable
	var on_turn_end: Callable
	var on_card_played: Callable
	var on_damage_dealt: Callable
	var on_damage_taken: Callable
	var on_kill: Callable

	func _init(p_id: String = "", p_name: String = "", p_desc: String = "") -> void:
		id = p_id
		name = p_name
		description = p_desc
		on_combat_start = Callable()
		on_combat_end = Callable()
		on_turn_start = Callable()
		on_turn_end = Callable()
		on_card_played = Callable()
		on_damage_dealt = Callable()
		on_damage_taken = Callable()
		on_kill = Callable()

static func create_burning_blood() -> RelicData:
	var relic := RelicData.new("Burning_Blood", "燃烧之血", "战斗结束时回复 6 点生命。")
	relic.flavor_text = "铁甲战士的起始遗物"
	relic.rarity = RelicRarity.SPECIAL
	relic.on_combat_end = func(engine):
		var heal := mini(6, engine.player.max_hp - engine.player.current_hp)
		engine.player.current_hp += heal
		print("[Relic] Burning Blood healed %d HP" % heal)
	return relic

static func create_anchor() -> RelicData:
	var relic := RelicData.new("Anchor", "锚", "战斗开始时获得 10 点格挡。")
	relic.rarity = RelicRarity.COMMON
	relic.on_combat_start = func(engine):
		engine.player.block += 10
		print("[Relic] Anchor granted 10 block")
	return relic

static func create_lantern() -> RelicData:
	var relic := RelicData.new("Lantern", "灯笼", "每场战斗的第 1 回合获得 1 点能量。")
	relic.rarity = RelicRarity.COMMON
	relic.on_combat_start = func(engine):
		engine.player.energy += 1
		print("[Relic] Lantern granted +1 energy")
	return relic

static func create_bag_of_preparation() -> RelicData:
	var relic := RelicData.new("Bag_of_Preparation", "备战袋", "每场战斗的前 2 回合多抽 2 张牌。")
	relic.rarity = RelicRarity.COMMON
	relic.on_turn_start = func(engine):
		if engine.turn_number <= 2:
			engine.draw_cards(2)
			print("[Relic] Bag of Preparation drew 2 extra cards")
	return relic

static func create_vajra() -> RelicData:
	var relic := RelicData.new("Vajra", "金刚杵", "战斗开始时获得 1 点力量。")
	relic.rarity = RelicRarity.COMMON
	relic.on_combat_start = func(engine):
		engine.player.strength += 1
		print("[Relic] Vajra granted +1 Strength")
	return relic

static func create_odd_mushroom() -> RelicData:
	var relic := RelicData.new("Odd_Mushroom", "奇怪蘑菇", "受到的脆弱效果降低 25%（50% → 25%）。")
	relic.rarity = RelicRarity.UNCOMMON
	relic.on_damage_taken = func(engine, damage):
		print("[Relic] Odd Mushroom reduces Vulnerable damage")
	return relic

static func create_shuriken() -> RelicData:
	var relic := RelicData.new("Shuriken", "手里剑", "每打出 3 张攻击牌，获得 1 点力量。")
	relic.rarity = RelicRarity.UNCOMMON
	relic.on_card_played = func(engine, card):
		if card.type == CardDatabase.CardType.ATTACK:
			print("[Relic] Shuriken tracks attack cards")
	return relic

static func create_orichalcum() -> RelicData:
	var relic := RelicData.new("Orichalcum", "山铜", "回合结束时如果格挡为 0，获得 6 点格挡。")
	relic.rarity = RelicRarity.UNCOMMON
	relic.on_turn_end = func(engine):
		if engine.player.block == 0:
			engine.player.block = 6
			print("[Relic] Orichalcum granted 6 block")
	return relic

static func create_dead_branch() -> RelicData:
	var relic := RelicData.new("Dead_Branch", "枯枝", "每消耗一张牌，随机增加一张牌到手牌。")
	relic.rarity = RelicRarity.RARE
	relic.on_card_played = func(engine, card):
		if card.exhaust:
			print("[Relic] Dead Branch adds random card on Exhaust")
	return relic

static func create_runic_cube() -> RelicData:
	var relic := RelicData.new("Runic_Cube", "符文立方体", "受到伤害时抽 1 张牌。")
	relic.rarity = RelicRarity.RARE
	relic.on_damage_taken = func(engine, damage):
		if damage > 0:
			engine.draw_cards(1)
			print("[Relic] Runic Cube drew 1 card on damage")
	return relic


class RelicManager extends Node:

signal relic_added(relic_id: String)
signal relic_removed(relic_id: String)

var _all_relics: Dictionary = {}
var _owned_relics: Array = []

func _ready() -> void:
	_register_core_relics()

func _register_core_relics() -> void:
	_register(StsRelicSystem.create_burning_blood())
	_register(StsRelicSystem.create_anchor())
	_register(StsRelicSystem.create_lantern())
	_register(StsRelicSystem.create_bag_of_preparation())
	_register(StsRelicSystem.create_vajra())
	_register(StsRelicSystem.create_odd_mushroom())
	_register(StsRelicSystem.create_shuriken())
	_register(StsRelicSystem.create_orichalcum())
	_register(StsRelicSystem.create_dead_branch())
	_register(StsRelicSystem.create_runic_cube())

func _register(relic: RelicData) -> void:
	_all_relics[relic.id] = relic

func get_relic(id: String) -> RelicData:
	return _all_relics.get(id) if _all_relics.has(id) else null

func add_relic(id: String) -> void:
	var relic := get_relic(id)
	if relic != null and not _owned_relics.has(relic):
		_owned_relics.append(relic)
		relic_added.emit(id)
		print("[RelicManager] Acquired: %s" % relic.name)

func remove_relic(id: String) -> void:
	var relic := get_relic(id)
	if relic != null:
		_owned_relics.erase(relic)
		relic_removed.emit(id)
		print("[RelicManager] Lost: %s" % relic.name)

func get_owned_relics() -> Array:
	return _owned_relics.duplicate()

func trigger_on_combat_start(engine) -> void:
	for relic in _owned_relics:
		if relic.on_combat_start.is_valid():
			relic.on_combat_start.call(engine)

func trigger_on_combat_end(engine) -> void:
	for relic in _owned_relics:
		if relic.on_combat_end.is_valid():
			relic.on_combat_end.call(engine)

func trigger_on_turn_start(engine) -> void:
	for relic in _owned_relics:
		if relic.on_turn_start.is_valid():
			relic.on_turn_start.call(engine)

func trigger_on_turn_end(engine) -> void:
	for relic in _owned_relics:
		if relic.on_turn_end.is_valid():
			relic.on_turn_end.call(engine)

func trigger_on_card_played(engine, card) -> void:
	for relic in _owned_relics:
		if relic.on_card_played.is_valid():
			relic.on_card_played.call(engine, card)

func trigger_on_damage_dealt(engine, damage: int) -> void:
	for relic in _owned_relics:
		if relic.on_damage_dealt.is_valid():
			relic.on_damage_dealt.call(engine, damage)

func trigger_on_damage_taken(engine, damage: int) -> void:
	for relic in _owned_relics:
		if relic.on_damage_taken.is_valid():
			relic.on_damage_taken.call(engine, damage)

func trigger_on_kill(engine) -> void:
	for relic in _owned_relics:
		if relic.on_kill.is_valid():
			relic.on_kill.call(engine)

func get_random_relics(count: int, rng: RandomNumberGenerator, rarity: int = -1) -> Array:
	var pool := []
	for relic in _all_relics.values():
		if rarity == -1 or relic.rarity == rarity:
			pool.append(relic)
	
	var result := []
	pool = pool.duplicate()
	pool.shuffle()
	for i in range(mini(count, pool.size())):
		result.append(pool[i])
	return result
