class_name StsCombatEngine extends Node

enum CardType { ATTACK, SKILL, POWER, STATUS, CURSE }
enum CardRarity { BASIC, COMMON, UNCOMMON, RARE, SPECIAL }
enum CardTarget { ENEMY_SINGLE, ENEMY_ALL, SELF, ALL, NONE }
enum DamageType { NORMAL, ATTACK, THORNS }
enum IntentType { ATTACK, ATTACK_DEBUFF, ATTACK_BUFF, DEFEND, DEFEND_BUFF, BUFF, DEBUFF, STRONG_DEBUFF, SLEEP, MAGIC, ESCAPE, UNKNOWN }

signal damage_dealt(target_index: int, target_name: String, amount: int)
signal block_gained(amount: int, total: int)
signal status_applied(target_name: String, effect: Dictionary)
signal turn_started(turn: int)
signal turn_ended(turn: int)
signal combat_won()
signal combat_lost()

var _player: Dictionary = {}
var _enemies: Array = []
var _rng: RandomNumberGenerator = RandomNumberGenerator.new()
var _turn_number: int = 0
var _relic_manager: Node = null
var is_player_turn: bool = true
var is_combat_over: bool = false

func _ready() -> void:
	pass

func get_player() -> Dictionary:
	return _player

func get_enemies() -> Array:
	return _enemies

func get_turn_number() -> int:
	return _turn_number

func initialize_combat(enemies: Array, seed_val: int) -> void:
	_rng.seed = seed_val
	_enemies = enemies
	_player = _create_player_state()
	_turn_number = 0
	_relic_manager = _create_relic_manager()
	is_combat_over = false
	_initialize_deck()
	_start_new_turn()
	if _relic_manager:
		_relic_manager.trigger_on_combat_start(self)
	print("[StsCombatEngine] Combat initialized with %d enemies" % _enemies.size())

func _create_player_state() -> Dictionary:
	return {
		"max_hp": 80,
		"current_hp": 80,
		"gold": 99,
		"energy": 3,
		"max_energy": 3,
		"block": 0,
		"strength": 0,
		"dexterity": 0,
		"status_effects": [],
		"hand": [],
		"draw_pile": [],
		"discard_pile": [],
		"exhaust_pile": [],
	}

func _create_relic_manager() -> Node:
	var manager := Node.new()
	manager.set_script(load("res://GameModes/base_game/Scripts/relics/sts_relic_system.gd"))
	add_child(manager)
	if manager.has_method("_register_core_relics"):
		manager._register_core_relics()
	return manager

func _initialize_deck() -> void:
	_player.draw_pile.clear()
	_player.discard_pile.clear()
	_player.exhaust_pile.clear()
	for i in range(5):
		_player.draw_pile.append(_create_strike())
	for i in range(5):
		_player.draw_pile.append(_create_defend())
	_player.draw_pile.append(_create_bash())
	_shuffle_draw_pile()

func _create_strike() -> Dictionary:
	return {
		"id": "Strike_R", "name": "打击", "description": "造成 6 点伤害。",
		"cost": 1, "type": CardType.ATTACK, "rarity": CardRarity.BASIC,
		"target": CardTarget.ENEMY_SINGLE, "damage": 6, "block": 0,
		"magic_number": 0, "ethereal": false, "exhaust": false,
		"innate": false, "retain": false, "damage_type": DamageType.ATTACK,
		"keywords": ["攻击"]
	}

func _create_defend() -> Dictionary:
	return {
		"id": "Defend_R", "name": "防御", "description": "获得 5 点格挡。",
		"cost": 1, "type": CardType.SKILL, "rarity": CardRarity.BASIC,
		"target": CardTarget.SELF, "damage": 0, "block": 5,
		"magic_number": 0, "ethereal": false, "exhaust": false,
		"innate": false, "retain": false, "damage_type": DamageType.NORMAL,
		"keywords": ["技能", "格挡"]
	}

func _create_bash() -> Dictionary:
	return {
		"id": "Bash", "name": "痛击", "description": "造成 8 点伤害。\n施加 2 层脆弱。",
		"cost": 2, "type": CardType.ATTACK, "rarity": CardRarity.COMMON,
		"target": CardTarget.ENEMY_SINGLE, "damage": 8, "block": 0,
		"magic_number": 2, "ethereal": false, "exhaust": false,
		"innate": false, "retain": false, "damage_type": DamageType.ATTACK,
		"keywords": ["攻击", "脆弱"]
	}

func _shuffle_draw_pile() -> void:
	for i in range(_player.draw_pile.size() - 1, 0, -1):
		var j: int = _rng.randi_range(0, i)
		var temp = _player.draw_pile[i]
		_player.draw_pile[i] = _player.draw_pile[j]
		_player.draw_pile[j] = temp

func _start_new_turn() -> void:
	is_player_turn = true
	_turn_number += 1
	_player.energy = _player.max_energy
	_player.block = 0
	_process_start_turn_status_effects(_player)
	draw_cards(5)
	for enemy in _enemies:
		if not _is_enemy_dead(enemy):
			_generate_enemy_intent(enemy)
	if _relic_manager:
		_relic_manager.trigger_on_turn_start(self)
	turn_started.emit(_turn_number)

func _process_start_turn_status_effects(unit: Dictionary) -> void:
	var effects: Array = unit.get("status_effects", [])
	for effect in effects:
		if effect.name == "Metallicize" and effect.stacks > 0:
			unit.block += effect.stacks
			block_gained.emit(effect.stacks, unit.block)
		if effect.name == "DemonForm" and effect.stacks > 0:
			_add_status(unit, _create_strength(effect.stacks))
		if effect.name == "Brutality" and effect.stacks > 0:
			_apply_damage_to_unit(unit, effect.stacks)
			damage_dealt.emit(-1, "Player", effect.stacks)
			draw_cards(1)

func draw_cards(count: int) -> void:
	for i in range(count):
		if _player.draw_pile.is_empty():
			_refill_draw_pile_from_discard()
		if not _player.draw_pile.is_empty():
			var card = _player.draw_pile.pop_back()
			_player.hand.append(card)
	print("[StsCombatEngine] Drew %d cards | Hand: %d" % [count, _player.hand.size()])

func _refill_draw_pile_from_discard() -> void:
	_player.draw_pile.append_array(_player.discard_pile)
	_player.discard_pile.clear()
	_shuffle_draw_pile()
	print("[StsCombatEngine] Reshuffled discard pile into draw pile")

func can_play_card(card: Dictionary) -> bool:
	if is_combat_over: return false
	if card.is_empty(): return false
	if _player.energy < card.cost: return false
	if not _has_valid_target(card): return false
	return true

func _has_valid_target(card: Dictionary) -> bool:
	match card.target:
		CardTarget.NONE, CardTarget.SELF, CardTarget.ALL:
			return true
		CardTarget.ENEMY_SINGLE, CardTarget.ENEMY_ALL:
			for e in _enemies:
				if not _is_enemy_dead(e):
					return true
			return false
		_:
			return true

func play_card(card: Dictionary, target_index: int = -1) -> Dictionary:
	var result := {"success": false, "reason": "", "damage_dealt": 0, "block_gained": 0, "status_applied": []}
	if not can_play_card(card):
		result.reason = "无法出牌"
		return result
	_player.energy -= card.cost
	_player.hand.erase(card)
	_execute_card_effect(card, target_index, result)
	if card.exhaust or card.ethereal:
		_player.exhaust_pile.append(card)
	elif not card.retain:
		_player.discard_pile.append(card)
	_check_combat_end()
	if _relic_manager:
		_relic_manager.trigger_on_card_played(self, card)
	print("[StsCombatEngine] Played: %s | Result: %s" % [card.name, "Success" if result.success else result.reason])
	return result

func _execute_card_effect(card: Dictionary, target_index: int, result: Dictionary) -> void:
	match card.type:
		CardType.ATTACK:
			_execute_attack_card(card, target_index, result)
		CardType.SKILL:
			_execute_skill_card(card, result)
		CardType.POWER:
			_execute_power_card(card, result)

func _execute_attack_card(card: Dictionary, target_index: int, result: Dictionary) -> void:
	var actual_damage: int = _calculate_damage(card.damage, card.damage_type)
	var targets: Array = _get_targets(card.target, target_index)
	for enemy in targets:
		if _is_enemy_dead(enemy): continue
		_apply_damage_to_enemy(enemy, actual_damage)
		var enemy_idx: int = _enemies.find(enemy)
		damage_dealt.emit(enemy_idx, enemy.name, actual_damage)
		if _relic_manager:
			_relic_manager.trigger_on_damage_dealt(self, actual_damage)
		result.damage_dealt += actual_damage
		if _is_enemy_dead(enemy):
			if _relic_manager:
				_relic_manager.trigger_on_kill(self)
		if not _is_enemy_dead(enemy) and card.magic_number > 0:
			var effect = _get_card_status_effect(card)
			if not effect.is_empty():
				_add_status(enemy, effect)
				status_applied.emit(enemy.name, effect)
				result.status_applied.append(effect)
	result.success = true

func _get_card_status_effect(card: Dictionary) -> Dictionary:
	var name_lower: String = card.name.to_lower()
	if "虚弱" in name_lower or "weak" in name_lower or "晾衣" in name_lower or "clothesline" in name_lower:
		return _create_weak(card.magic_number)
	if "脆弱" in name_lower or "vulnerable" in name_lower or "痛击" in name_lower or "bash" in name_lower or "上勾" in name_lower or "uppercut" in name_lower:
		return _create_vulnerable(card.magic_number)
	return _create_vulnerable(card.magic_number)

func _execute_skill_card(card: Dictionary, result: Dictionary) -> void:
	if card.block > 0:
		var actual_block: int = card.block + _player.dexterity
		_player.block += actual_block
		block_gained.emit(actual_block, _player.block)
		result.block_gained = actual_block
	var name_lower: String = card.name.to_lower()
	if "抽牌" in name_lower or "draw" in name_lower or "耸肩" in name_lower or "shrug" in name_lower or "恍惚" in name_lower or "trance" in name_lower:
		var draw_count: int = card.magic_number if card.magic_number > 0 else 1
		draw_cards(draw_count)
	if "放血" in name_lower or "bloodletting" in name_lower:
		_player.energy += card.magic_number
	if "挖掘" in name_lower or "entrench" in name_lower:
		_player.block *= 2
		block_gained.emit(_player.block, _player.block)
	if "屈伸" in name_lower or "flex" in name_lower:
		_add_status(_player, _create_strength(card.magic_number))
		status_applied.emit("Player", _create_strength(card.magic_number))
	if "愤怒" in name_lower or "rage" in name_lower:
		_add_status(_player, _create_strength(card.magic_number))
		status_applied.emit("Player", _create_strength(card.magic_number))
	result.success = true

func _execute_power_card(card: Dictionary, result: Dictionary) -> void:
	var name_lower: String = card.name.to_lower()
	if "燃烧" in name_lower or "inflame" in name_lower:
		_add_status(_player, _create_strength(card.magic_number))
		status_applied.emit("Player", _create_strength(card.magic_number))
	elif "金属化" in name_lower or "metallicize" in name_lower:
		var metal := {"id": "metallicize", "name": "Metallicize", "stacks": card.magic_number, "duration": -1, "is_buff": true}
		_add_status(_player, metal)
		status_applied.emit("Player", metal)
	elif "恶魔" in name_lower or "demon" in name_lower:
		var demon := {"id": "demon_form", "name": "DemonForm", "stacks": card.magic_number, "duration": -1, "is_buff": true}
		_add_status(_player, demon)
		status_applied.emit("Player", demon)
	elif "壁垒" in name_lower or "barricade" in name_lower:
		var barricade := {"id": "barricade", "name": "Barricade", "stacks": 1, "duration": -1, "is_buff": true}
		_add_status(_player, barricade)
		status_applied.emit("Player", barricade)
	elif "残暴" in name_lower or "brutality" in name_lower:
		var brutal := {"id": "brutality", "name": "Brutality", "stacks": card.magic_number, "duration": -1, "is_buff": true}
		_add_status(_player, brutal)
		status_applied.emit("Player", brutal)
	elif "无痛" in name_lower or "feel no pain" in name_lower:
		var fnp := {"id": "feel_no_pain", "name": "FeelNoPain", "stacks": card.magic_number, "duration": -1, "is_buff": true}
		_add_status(_player, fnp)
		status_applied.emit("Player", fnp)
	else:
		_add_status(_player, _create_strength(card.magic_number if card.magic_number > 0 else 2))
		status_applied.emit("Player", _create_strength(card.magic_number if card.magic_number > 0 else 2))
	result.success = true

func _calculate_damage(base_damage: int, damage_type: int = DamageType.NORMAL) -> int:
	var damage: float = base_damage
	if damage_type == DamageType.ATTACK:
		damage += _player.strength
	var weak_multiplier: float = 1.0
	if _get_status_stacks(_player, "weak") > 0:
		weak_multiplier = 0.75
	return maxi(0, int(damage * weak_multiplier))

func _calculate_enemy_damage_to_player(base_damage: int) -> int:
	var damage: float = base_damage
	if _player_has_status("vulnerable"):
		damage *= 1.5
	if _player_has_status("weak"):
		damage *= 0.75
	return maxi(0, int(damage))

func _get_targets(target: int, target_index: int) -> Array:
	var targets: Array = []
	match target:
		CardTarget.ENEMY_SINGLE:
			if target_index >= 0 and target_index < _enemies.size() and not _is_enemy_dead(_enemies[target_index]):
				targets.append(_enemies[target_index])
			else:
				for e in _enemies:
					if not _is_enemy_dead(e):
						targets.append(e)
						break
		CardTarget.ENEMY_ALL:
			for e in _enemies:
				if not _is_enemy_dead(e):
					targets.append(e)
	return targets

func _generate_enemy_intent(enemy: Dictionary) -> void:
	var roll: float = _rng.randf()
	var base_id: String = enemy.id.split("_")[0]
	if enemy.id.begins_with("AcidSlime"):
		base_id = "AcidSlime"
	elif enemy.id.begins_with("SpikeSlime"):
		base_id = "SpikeSlime"
	match base_id:
		"Cultist":
			enemy.current_intent = _gen_cultist_intent(roll)
		"JawWorm":
			enemy.current_intent = _gen_jaw_worm_intent(roll)
		"Louse":
			enemy.current_intent = _gen_louse_intent(roll)
		"Slaver":
			enemy.current_intent = _gen_slaver_intent(roll)
		"RedSlaver":
			enemy.current_intent = _gen_red_slaver_intent(roll)
		"FungiBeast":
			enemy.current_intent = _gen_fungi_beast_intent(roll)
		"Gremlin":
			enemy.current_intent = _gen_gremlin_nob_intent(roll)
		"Lagavulin":
			enemy.current_intent = _gen_lagavulin_intent(roll)
		"Sentry":
			enemy.current_intent = _gen_sentry_intent(roll)
		"ShelledParasite":
			enemy.current_intent = _gen_shelled_parasite_intent(roll)
		"AcidSlime":
			enemy.current_intent = _gen_acid_slime_intent(roll)
		"SpikeSlime":
			enemy.current_intent = _gen_spike_slime_intent(roll)
		"TaskMaster":
			enemy.current_intent = _gen_task_master_intent(roll)
		"FungusLing":
			enemy.current_intent = _gen_fungus_ling_intent(roll)
		"The", "Hexaghost", "Donu":
			enemy.current_intent = _gen_boss_intent(enemy, roll)
		_:
			enemy.current_intent = _create_attack_intent(_rng.randi_range(5, 10))

func _gen_cultist_intent(roll: float) -> Dictionary:
	if _turn_number % 3 == 0:
		return _create_buff_intent("仪式", 3)
	var damage: int = _rng.randi_range(6, 9)
	return _create_attack_intent(damage)

func _gen_jaw_worm_intent(roll: float) -> Dictionary:
	if roll < 0.25:
		return _create_defend_intent(6 + _turn_number / 3)
	elif roll < 0.6:
		return _create_attack_intent(10 + _turn_number / 3)
	else:
		return _create_attack_intent(5 + _turn_number / 4)

func _gen_louse_intent(roll: float) -> Dictionary:
	if roll < 0.33:
		return _create_defend_intent(_rng.randi_range(4, 8))
	elif roll < 0.66:
		return _create_attack_intent(_rng.randi_range(4, 7))
	else:
		return _create_buff_intent("成长", 2)

func _gen_slaver_intent(roll: float) -> Dictionary:
	if _turn_number % 4 == 1:
		return _create_debuff_intent("虚弱", 2)
	elif roll < 0.5:
		return _create_attack_intent(10 + _turn_number / 4)
	else:
		return _create_defend_intent(6)

func _gen_red_slaver_intent(roll: float) -> Dictionary:
	if _turn_number % 3 == 0:
		return _create_debuff_intent("脆弱", 2)
	elif roll < 0.6:
		return _create_attack_intent(11 + _turn_number / 4)
	else:
		return _create_attack_intent(8)

func _gen_fungi_beast_intent(roll: float) -> Dictionary:
	if _turn_number % 3 == 0:
		return _create_buff_intent("力量", 2)
	else:
		return _create_attack_intent(6 + _turn_number / 3)

func _gen_gremlin_nob_intent(roll: float) -> Dictionary:
	if _turn_number == 1:
		return _create_buff_intent("力量", 2)
	elif roll < 0.6:
		return _create_attack_intent(12 + _turn_number / 2)
	else:
		return _create_attack_intent(8 + _turn_number / 3)

func _gen_lagavulin_intent(roll: float) -> Dictionary:
	var cycle: int = _turn_number % 3
	if cycle == 0:
		return _create_debuff_intent("力量-1", 1)
	elif cycle == 1:
		return _create_defend_intent(12)
	else:
		return _create_attack_intent(15 + _turn_number / 3)

func _gen_sentry_intent(roll: float) -> Dictionary:
	if _turn_number % 2 == 1:
		return _create_defend_intent(8)
	else:
		return _create_attack_intent(8 + _turn_number / 4)

func _gen_shelled_parasite_intent(roll: float) -> Dictionary:
	if _turn_number % 3 == 0:
		return _create_debuff_intent("脆弱", 1)
	elif roll < 0.5:
		return _create_attack_intent(7 + _turn_number / 3)
	else:
		return _create_defend_intent(10)

func _gen_acid_slime_intent(roll: float) -> Dictionary:
	if roll < 0.3:
		return _create_debuff_intent("虚弱", 1)
	else:
		return _create_attack_intent(5 + _turn_number / 4)

func _gen_spike_slime_intent(roll: float) -> Dictionary:
	if roll < 0.3:
		return _create_debuff_intent("脆弱", 1)
	else:
		return _create_attack_intent(5 + _turn_number / 4)

func _gen_task_master_intent(roll: float) -> Dictionary:
	if _turn_number % 3 == 0:
		return _create_buff_intent("力量", 1)
	elif roll < 0.5:
		return _create_attack_intent(9 + _turn_number / 3)
	else:
		return _create_defend_intent(8)

func _gen_fungus_ling_intent(roll: float) -> Dictionary:
	if _turn_number % 2 == 0:
		return _create_buff_intent("力量", 1)
	else:
		return _create_attack_intent(6 + _turn_number / 3)

func _gen_boss_intent(enemy: Dictionary, roll: float) -> Dictionary:
	var phase: int = 1 if enemy.current_hp > enemy.max_hp / 2 else 2
	var cycle: int = _turn_number % 4
	if phase == 1:
		match cycle:
			0: return _create_attack_intent(14 + _turn_number / 3)
			1: return _create_defend_intent(14)
			2: return _create_buff_intent("力量", 2)
			_: return _create_attack_intent(10 + _turn_number / 4)
	else:
		match cycle:
			0: return _create_attack_intent(18 + _turn_number / 2)
			1: return _create_debuff_intent("脆弱", 2)
			2: return _create_attack_intent(13 + _turn_number / 3)
			_: return _create_buff_intent("力量", 3)

func _create_attack_intent(damage: int) -> Dictionary:
	return {"type": IntentType.ATTACK, "value": damage, "value2": 0, "description": "准备攻击 %d" % damage, "icon": "⚔"}

func _create_defend_intent(block: int) -> Dictionary:
	return {"type": IntentType.DEFEND, "value": block, "value2": 0, "description": "获得 %d 格挡" % block, "icon": "🛡"}

func _create_buff_intent(buff_name: String, stacks: int) -> Dictionary:
	return {"type": IntentType.BUFF, "value": stacks, "value2": 0, "description": "获得 %s +%d" % [buff_name, stacks], "icon": "⬆"}

func _create_debuff_intent(debuff_name: String, stacks: int) -> Dictionary:
	return {"type": IntentType.DEBUFF, "value": stacks, "value2": 0, "description": "施加 %s %d层" % [debuff_name, stacks], "icon": "↓"}

func _create_attack_debuff_intent(damage: int, debuff_name: String, stacks: int) -> Dictionary:
	return {"type": IntentType.ATTACK_DEBUFF, "value": damage, "value2": stacks, "description": "攻击 %d + 施加 %s %d层" % [damage, debuff_name, stacks], "icon": "⚔↓"}

func end_turn() -> void:
	is_player_turn = false
	_player.energy = 0
	if _relic_manager:
		_relic_manager.trigger_on_turn_end(self)
	var cards_to_discard: Array = []
	for card in _player.hand:
		if not card.retain:
			cards_to_discard.append(card)
	for card in cards_to_discard:
		_player.hand.erase(card)
		_player.discard_pile.append(card)
	_process_end_turn_status_effects(_player)
	_execute_enemy_turns()
	turn_ended.emit(_turn_number)
	if is_combat_over:
		return
	if not _is_player_dead():
		_start_new_turn()
	else:
		is_combat_over = true
		combat_lost.emit()
		print("[StsCombatEngine] Player defeated!")

func _execute_enemy_turns() -> void:
	for enemy in _enemies:
		if _is_enemy_dead(enemy): continue
		_process_end_turn_enemy_status_effects(enemy)
		if _is_enemy_dead(enemy): continue
		if is_combat_over: return
		_execute_enemy_action(enemy)
		if _is_player_dead():
			break
	_check_combat_end()

func _execute_enemy_action(enemy: Dictionary) -> void:
	if _is_enemy_dead(enemy): return
	if enemy.current_intent == null or enemy.current_intent.is_empty(): return
	match enemy.current_intent.type:
		IntentType.ATTACK:
			var damage: int = _calculate_enemy_damage_to_player(enemy.current_intent.value)
			_apply_damage_to_player(damage)
			damage_dealt.emit(-1, "玩家", damage)
			print("[StsCombatEngine] %s attacks for %d" % [enemy.name, damage])
		IntentType.ATTACK_DEBUFF:
			var damage: int = _calculate_enemy_damage_to_player(enemy.current_intent.value)
			_apply_damage_to_player(damage)
			damage_dealt.emit(-1, "玩家", damage)
			var debuff = _create_weak(enemy.current_intent.value2)
			_add_status(_player, debuff)
			status_applied.emit("玩家", debuff)
			print("[StsCombatEngine] %s attacks for %d and applies debuff" % [enemy.name, damage])
		IntentType.ATTACK_BUFF:
			var damage: int = _calculate_enemy_damage_to_player(enemy.current_intent.value)
			_apply_damage_to_player(damage)
			damage_dealt.emit(-1, "玩家", damage)
			var buff = _create_strength(enemy.current_intent.value2)
			_add_status(enemy, buff)
			status_applied.emit(enemy.name, buff)
			print("[StsCombatEngine] %s attacks for %d and buffs self" % [enemy.name, damage])
		IntentType.DEFEND:
			enemy.block = enemy.get("block", 0) + enemy.current_intent.value
			block_gained.emit(enemy.current_intent.value, enemy.block)
			print("[StsCombatEngine] %s gains %d block" % [enemy.name, enemy.current_intent.value])
		IntentType.DEFEND_BUFF:
			enemy.block = enemy.get("block", 0) + enemy.current_intent.value
			block_gained.emit(enemy.current_intent.value, enemy.block)
			var buff = _create_strength(enemy.current_intent.value2)
			_add_status(enemy, buff)
			status_applied.emit(enemy.name, buff)
		IntentType.BUFF:
			var buff_name: String = enemy.current_intent.description
			if "仪式" in buff_name:
				var ritual := {"id": "ritual", "name": "Ritual", "stacks": enemy.current_intent.value, "duration": -1, "is_buff": true}
				_add_status(enemy, ritual)
				status_applied.emit(enemy.name, ritual)
			elif "成长" in buff_name:
				var growth := {"id": "growth", "name": "Growth", "stacks": enemy.current_intent.value, "duration": -1, "is_buff": true}
				_add_status(enemy, growth)
				status_applied.emit(enemy.name, growth)
			else:
				var buff = _create_strength(enemy.current_intent.value)
				_add_status(enemy, buff)
				status_applied.emit(enemy.name, buff)
			print("[StsCombatEngine] %s gains buff" % enemy.name)
		IntentType.DEBUFF, IntentType.STRONG_DEBUFF:
			var debuff_name: String = enemy.current_intent.description
			if "脆弱" in debuff_name:
				var debuff = _create_vulnerable(enemy.current_intent.value)
				_add_status(_player, debuff)
				status_applied.emit("玩家", debuff)
			else:
				var debuff = _create_weak(enemy.current_intent.value)
				_add_status(_player, debuff)
				status_applied.emit("玩家", debuff)
			print("[StsCombatEngine] Player receives debuff from %s" % enemy.name)
		IntentType.SLEEP:
			print("[StsCombatEngine] %s is sleeping" % enemy.name)
		IntentType.MAGIC:
			print("[StsCombatEngine] %s uses magic" % enemy.name)
		IntentType.ESCAPE:
			print("[StsCombatEngine] %s tries to escape" % enemy.name)

func _process_end_turn_status_effects(unit: Dictionary) -> void:
	var effects: Array = unit.get("status_effects", [])
	var effects_copy: Array = effects.duplicate()
	for effect in effects_copy:
		if effect.name == "Poison" and effect.stacks > 0:
			var poison_dmg: int = effect.stacks
			_apply_damage_to_unit(unit, poison_dmg)
			effect.stacks = maxi(0, effect.stacks - 1)
			var idx: int = -1
			if unit == _player:
				idx = -1
			else:
				idx = _enemies.find(unit)
			damage_dealt.emit(idx, unit.get("name", "Unknown"), poison_dmg)
			if _relic_manager and unit == _player:
				_relic_manager.trigger_on_damage_taken(self, poison_dmg)
		if effect.name == "Vulnerable" or effect.name == "Weak":
			effect.stacks = maxi(0, effect.stacks - 1)
		if effect.name == "Ritual" and effect.stacks > 0:
			var str = _create_strength(effect.stacks)
			_add_status(unit, str)
		if effect.name == "Growth" and effect.stacks > 0:
			var str = _create_strength(effect.stacks)
			_add_status(unit, str)
		effect.duration -= 1
	effects.erase(null)
	var to_remove: Array = []
	for effect in effects:
		if effect.duration == 0 or (effect.name == "Poison" and effect.stacks <= 0):
			to_remove.append(effect)
	for effect in to_remove:
		effects.erase(effect)

func _process_end_turn_enemy_status_effects(enemy: Dictionary) -> void:
	if _is_enemy_dead(enemy): return
	_process_end_turn_status_effects(enemy)

func _apply_damage_to_player(damage: int) -> void:
	if _player.block > 0:
		if damage >= _player.block:
			damage -= _player.block
			_player.block = 0
		else:
			_player.block -= damage
			damage = 0
	_player.current_hp -= damage
	if _relic_manager and damage > 0:
		_relic_manager.trigger_on_damage_taken(self, damage)

func _apply_damage_to_enemy(enemy: Dictionary, damage: int) -> void:
	if enemy.get("block", 0) > 0:
		if damage >= enemy.block:
			damage -= enemy.block
			enemy.block = 0
		else:
			enemy.block -= damage
			damage = 0
	if enemy.has_status and enemy.get("has_status", false):
		if _enemy_has_status(enemy, "vulnerable"):
			damage = int(damage * 1.5)
	enemy.current_hp -= damage

func _apply_damage_to_unit(unit: Dictionary, damage: int) -> void:
	if unit.block > 0:
		if damage >= unit.block:
			damage -= unit.block
			unit.block = 0
		else:
			unit.block -= damage
			damage = 0
	unit.current_hp -= damage

func _check_combat_end() -> void:
	if is_combat_over: return
	var all_dead: bool = true
	for e in _enemies:
		if not _is_enemy_dead(e):
			all_dead = false
			break
	if all_dead:
		is_combat_over = true
		if _relic_manager:
			_relic_manager.trigger_on_combat_end(self)
		combat_won.emit()
		print("[StsCombatEngine] Victory!")
		return
	if _is_player_dead():
		is_combat_over = true
		combat_lost.emit()
		print("[StsCombatEngine] Player defeated!")

func _is_enemy_dead(enemy: Dictionary) -> bool:
	return enemy.current_hp <= 0

func _is_player_dead() -> bool:
	return _player.current_hp <= 0

func _player_has_status(status_id: String) -> bool:
	return _get_status_stacks(_player, status_id) > 0

func _enemy_has_status(enemy: Dictionary, status_id: String) -> bool:
	return _get_status_stacks(enemy, status_id) > 0

func _get_status_stacks(unit: Dictionary, status_id: String) -> int:
	for effect in unit.get("status_effects", []):
		if effect.id == status_id:
			return effect.stacks
	return 0

func _add_status(unit: Dictionary, effect: Dictionary) -> void:
	if effect.is_empty(): return
	var effects: Array = unit.get("status_effects", [])
	for existing in effects:
		if existing.id == effect.id:
			existing.stacks += effect.stacks
			return
	effects.append(effect.duplicate())
	unit.status_effects = effects

func _create_weak(stacks: int) -> Dictionary:
	return {"id": "weak", "name": "Weak", "description": "攻击伤害降低25%", "stacks": stacks, "duration": -1, "is_buff": false}

func _create_vulnerable(stacks: int) -> Dictionary:
	return {"id": "vulnerable", "name": "Vulnerable", "description": "受到的伤害提升50%", "stacks": stacks, "duration": -1, "is_buff": false}

func _create_strength(stacks: int) -> Dictionary:
	return {"id": "strength", "name": "Strength", "description": "每张攻击牌伤害+%d" % stacks, "stacks": stacks, "duration": -1, "is_buff": true}

func _create_dexterity(stacks: int) -> Dictionary:
	return {"id": "dexterity", "name": "Dexterity", "description": "每张防御牌格挡+%d" % stacks, "stacks": stacks, "duration": -1, "is_buff": true}

func _create_poison(stacks: int) -> Dictionary:
	return {"id": "poison", "name": "Poison", "description": "回合结束时受到%d点伤害" % stacks, "stacks": stacks, "duration": -1, "is_buff": false}

func get_alive_enemies() -> Array:
	var alive: Array = []
	for e in _enemies:
		if not _is_enemy_dead(e):
			alive.append(e)
	return alive

func get_first_alive_enemy_index() -> int:
	for i in range(_enemies.size()):
		if not _is_enemy_dead(_enemies[i]):
			return i
	return -1
