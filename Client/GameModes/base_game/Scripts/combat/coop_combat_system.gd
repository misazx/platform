class_name CoopCombatEngine extends Node

enum CoopTurnPhase { PLAYER_1_TURN, PLAYER_2_TURN, PLAYER_3_TURN, PLAYER_4_TURN, ENEMY_TURN }
enum CoopCardType { ATTACK, SKILL, POWER }
enum CoopCardTarget { ENEMY_SINGLE, ENEMY_ALL, SELF, ALL, NONE }
enum CoopIntentType { ATTACK, ATTACK_DEBUFF, ATTACK_BUFF, DEFEND, DEFEND_BUFF, BUFF, DEBUFF, STRONG_DEBUFF, SLEEP, MAGIC, ESCAPE, UNKNOWN }

const MAX_COOP_PLAYERS: int = 4
const DEFAULT_ENERGY: int = 3
const DRAW_PER_TURN: int = 5
const SYNC_INTERVAL: float = 0.5

signal coop_turn_started(player_index: int, turn: int)
signal coop_turn_ended(player_index: int, turn: int)
signal coop_card_played(player_index: int, card: Dictionary, target_index: int)
signal coop_damage_dealt(target_index: int, target_name: String, amount: int, source_player: int)
signal coop_block_gained(player_index: int, amount: int, total: int)
signal coop_status_applied(target_name: String, effect: Dictionary, source_player: int)
signal coop_combat_won()
signal coop_combat_lost()
signal coop_state_synced(state: Dictionary)

var _players: Array = []
var _enemies: Array = []
var _rng: RandomNumberGenerator = RandomNumberGenerator.new()
var _turn_number: int = 0
var _current_player_index: int = 0
var _current_phase: int = CoopTurnPhase.PLAYER_1_TURN
var _is_combat_over: bool = false
var _local_player_index: int = 0
var _relic_managers: Array = []
var _sync_timer: float = 0.0

var _bot_player_indices: Array = []
var _bot_user_ids: Dictionary = {}
var _is_bot_turn_processing: bool = false

func _ready() -> void:
	pass

func set_bot_info(bot_indices: Array, bot_user_ids: Dictionary) -> void:
	_bot_player_indices = bot_indices
	_bot_user_ids = bot_user_ids

func is_bot_player(player_index: int) -> bool:
	return _bot_player_indices.has(player_index)

func _notify_bot_ai_if_needed() -> void:
	if not is_bot_player(_current_player_index): return
	if _is_combat_over: return
	if _is_bot_turn_processing: return

	_is_bot_turn_processing = true
	var bot_user_id: String = _bot_user_ids.get(_current_player_index, "")
	if bot_user_id == "": return

	var player: Dictionary = _players[_current_player_index]
	var hand_data: Array = []
	for card in player.hand:
		hand_data.append({
			"id": card.get("id", ""),
			"name": card.get("name", ""),
			"cost": card.get("cost", 0),
			"type": card.get("type", 0),
			"damage": card.get("damage", 0),
			"block": card.get("block", 0),
			"target": card.get("target", 0),
		})

	var enemies_data: Array = []
	for e in _enemies:
		if not _is_enemy_dead(e):
			enemies_data.append({
				"id": e.get("id", ""),
				"name": e.get("name", ""),
				"current_hp": e.current_hp,
				"max_hp": e.max_hp,
				"block": e.get("block", 0),
			})

	var potions_data: Array = []
	for p in player.get("potions", []):
		potions_data.append({"id": p.get("id", ""), "name": p.get("name", "")})

	var game_state := {
		"hand": hand_data,
		"player_hp": player.current_hp,
		"player_max_hp": player.max_hp,
		"player_energy": player.energy,
		"player_block": player.block,
		"enemies": enemies_data,
		"potions": potions_data,
	}

	var hub_client = get_node_or_null("/root/GameHubClient")
	if hub_client != null:
		var room_id: String = ""
		var session_mgr = get_node_or_null("/root/GameSessionManager")
		if session_mgr != null:
			room_id = session_mgr.GetCurrentRoomId()
		if room_id != "":
			var json_state: String = JSON.stringify(game_state)
			hub_client.UpdateBotGameStateAsync(room_id, bot_user_id, json_state)

func initialize_coop_combat(enemies: Array, player_count: int, seed_val: int, local_index: int = 0) -> void:
	var effective_seed: int = seed_val
	var bridge = get_node_or_null("/root/MultiplayerSeedBridge")
	if bridge != null and bridge.is_multiplayer_game() and seed_val == 0:
		effective_seed = bridge.get_effective_seed(0)
	_rng.seed = effective_seed
	_enemies = enemies
	_local_player_index = local_index
	_turn_number = 0
	_current_player_index = 0
	_is_combat_over = false
	_players.clear()
	_relic_managers.clear()

	var effective_count: int = mini(player_count, MAX_COOP_PLAYERS)
	for i in range(effective_count):
		_players.append(_create_coop_player_state(i))
		_relic_managers.append(null)

	_current_phase = CoopTurnPhase.PLAYER_1_TURN
	_start_coop_turn()

func _create_coop_player_state(index: int) -> Dictionary:
	return {
		"index": index,
		"name": "玩家%d" % (index + 1),
		"max_hp": 80,
		"current_hp": 80,
		"energy": DEFAULT_ENERGY,
		"max_energy": DEFAULT_ENERGY,
		"block": 0,
		"strength": 0,
		"dexterity": 0,
		"status_effects": [],
		"hand": [],
		"draw_pile": [],
		"discard_pile": [],
		"exhaust_pile": [],
		"is_ready": false,
	}

func get_local_player() -> Dictionary:
	if _local_player_index >= 0 and _local_player_index < _players.size():
		return _players[_local_player_index]
	return {}

func get_player_by_index(index: int) -> Dictionary:
	if index >= 0 and index < _players.size():
		return _players[index]
	return {}

func get_enemies() -> Array:
	return _enemies

func get_current_player_index() -> int:
	return _current_player_index

func is_local_player_turn() -> bool:
	return _current_player_index == _local_player_index and not _is_combat_over

func get_turn_number() -> int:
	return _turn_number

func _start_coop_turn() -> void:
	_turn_number += 1
	_current_player_index = 0
	_current_phase = CoopTurnPhase.PLAYER_1_TURN

	for player in _players:
		player.energy = player.max_energy
		player.block = 0
		_process_start_turn_status_effects(player)

	for i in range(_players.size()):
		_draw_cards_for_player(i, DRAW_PER_TURN)

	for enemy in _enemies:
		if not _is_enemy_dead(enemy):
			_generate_enemy_intent(enemy)

	coop_turn_started.emit(_current_player_index, _turn_number)
	_notify_bot_ai_if_needed()

func _draw_cards_for_player(player_index: int, count: int) -> void:
	var player: Dictionary = _players[player_index]
	for i in range(count):
		if player.draw_pile.is_empty():
			_refill_draw_pile(player)
		if not player.draw_pile.is_empty():
			var card = player.draw_pile.pop_back()
			player.hand.append(card)

func _refill_draw_pile(player: Dictionary) -> void:
	player.draw_pile.append_array(player.discard_pile)
	player.discard_pile.clear()
	_shuffle_player_draw_pile(player)

func _shuffle_player_draw_pile(player: Dictionary) -> void:
	for i in range(player.draw_pile.size() - 1, 0, -1):
		var j: int = _rng.randi_range(0, i)
		var temp = player.draw_pile[i]
		player.draw_pile[i] = player.draw_pile[j]
		player.draw_pile[j] = temp

func can_play_card_coop(player_index: int, card: Dictionary) -> bool:
	if _is_combat_over: return false
	if _current_player_index != player_index: return false
	if card.is_empty(): return false
	var player: Dictionary = _players[player_index]
	if player.energy < card.cost: return false
	if not _has_valid_target(card): return false
	return true

func play_card_coop(player_index: int, card: Dictionary, target_index: int = -1) -> Dictionary:
	var result := {"success": false, "reason": "", "damage_dealt": 0, "block_gained": 0, "status_applied": []}
	if not can_play_card_coop(player_index, card):
		result.reason = "无法出牌"
		return result

	var player: Dictionary = _players[player_index]
	player.energy -= card.cost
	player.hand.erase(card)
	_execute_coop_card_effect(player_index, card, target_index, result)

	if card.exhaust or card.ethereal:
		player.exhaust_pile.append(card)
	elif not card.retain:
		player.discard_pile.append(card)

	_check_combat_end()
	coop_card_played.emit(player_index, card, target_index)
	return result

func _execute_coop_card_effect(player_index: int, card: Dictionary, target_index: int, result: Dictionary) -> void:
	var player: Dictionary = _players[player_index]
	var card_type: int = card.get("type", 0)
	match card_type:
		CoopCardType.ATTACK:
			_execute_coop_attack(player_index, card, target_index, result)
		CoopCardType.SKILL:
			_execute_coop_skill(player_index, card, result)
		CoopCardType.POWER:
			_execute_coop_power(player_index, card, result)

func _execute_coop_attack(player_index: int, card: Dictionary, target_index: int, result: Dictionary) -> void:
	var player: Dictionary = _players[player_index]
	var actual_damage: int = _calculate_player_damage(player, card.damage)
	var targets: Array = _get_targets(card.target, target_index)

	for enemy in targets:
		if _is_enemy_dead(enemy): continue
		_apply_damage_to_enemy(enemy, actual_damage)
		var enemy_idx: int = _enemies.find(enemy)
		coop_damage_dealt.emit(enemy_idx, enemy.name, actual_damage, player_index)
		result.damage_dealt += actual_damage
		if not _is_enemy_dead(enemy) and card.magic_number > 0:
			var effect := _get_card_status_effect(card)
			if not effect.is_empty():
				_add_status(enemy, effect)
				coop_status_applied.emit(enemy.name, effect, player_index)
				result.status_applied.append(effect)
	result.success = true

func _execute_coop_skill(player_index: int, card: Dictionary, result: Dictionary) -> void:
	var player: Dictionary = _players[player_index]
	if card.block > 0:
		var actual_block: int = card.block + player.dexterity
		player.block += actual_block
		coop_block_gained.emit(player_index, actual_block, player.block)
		result.block_gained = actual_block
	result.success = true

func _execute_coop_power(player_index: int, card: Dictionary, result: Dictionary) -> void:
	var player: Dictionary = _players[player_index]
	var name_lower: String = card.name.to_lower()
	if "燃烧" in name_lower or "inflame" in name_lower:
		_add_status(player, _create_strength(card.magic_number))
	elif "金属化" in name_lower or "metallicize" in name_lower:
		var metal := {"id": "metallicize", "name": "Metallicize", "stacks": card.magic_number, "duration": -1, "is_buff": true}
		_add_status(player, metal)
	else:
		_add_status(player, _create_strength(card.magic_number if card.magic_number > 0 else 2))
	result.success = true

func _calculate_player_damage(player: Dictionary, base_damage: int) -> int:
	var damage: float = base_damage + player.strength
	var weak_stacks: int = _get_status_stacks(player, "weak")
	if weak_stacks > 0:
		damage *= 0.75
	return maxi(0, int(damage))

func _apply_damage_to_enemy(enemy: Dictionary, damage: int) -> void:
	if enemy.block > 0:
		if damage >= enemy.block:
			damage -= enemy.block
			enemy.block = 0
		else:
			enemy.block -= damage
			damage = 0
	enemy.current_hp -= damage

func end_coop_player_turn(player_index: int) -> void:
	if _current_player_index != player_index: return
	if _is_combat_over: return

	coop_turn_ended.emit(player_index, _turn_number)

	var player: Dictionary = _players[player_index]
	player.energy = 0
	var cards_to_discard: Array = []
	for card in player.hand:
		if not card.retain:
			cards_to_discard.append(card)
	for card in cards_to_discard:
		player.hand.erase(card)
		player.discard_pile.append(card)

	_process_end_turn_status_effects(player)

	_current_player_index += 1
	if _current_player_index >= _players.size():
		_current_phase = CoopTurnPhase.ENEMY_TURN
		_execute_enemy_turns()
		if not _is_combat_over:
			_start_coop_turn()
	else:
		_current_phase = _current_player_index
		coop_turn_started.emit(_current_player_index, _turn_number)
		_notify_bot_ai_if_needed()

func _execute_enemy_turns() -> void:
	for enemy in _enemies:
		if _is_enemy_dead(enemy): continue
		_process_end_turn_status_effects(enemy)
		if _is_enemy_dead(enemy): continue
		if _is_combat_over: return
		_execute_coop_enemy_action(enemy)
		if _all_players_dead():
			break
	_check_combat_end()

func _execute_coop_enemy_action(enemy: Dictionary) -> void:
	if _is_enemy_dead(enemy): return
	if enemy.current_intent == null or enemy.current_intent.is_empty(): return

	var target_player_index: int = _select_target_player(enemy)

	match enemy.current_intent.type:
		CoopIntentType.ATTACK:
			var damage: int = _calculate_enemy_damage(enemy.current_intent.value)
			_apply_damage_to_coop_player(target_player_index, damage)
			coop_damage_dealt.emit(-1, _players[target_player_index].name, damage, -1)
		CoopIntentType.ATTACK_DEBUFF:
			var damage: int = _calculate_enemy_damage(enemy.current_intent.value)
			_apply_damage_to_coop_player(target_player_index, damage)
			coop_damage_dealt.emit(-1, _players[target_player_index].name, damage, -1)
			var debuff := _create_weak(enemy.current_intent.value2)
			_add_status(_players[target_player_index], debuff)
		CoopIntentType.BUFF:
			var buff = _create_strength(enemy.current_intent.value)
			_add_status(enemy, buff)
		CoopIntentType.DEBUFF, CoopIntentType.STRONG_DEBUFF:
			var debuff := _create_weak(enemy.current_intent.value)
			_add_status(_players[target_player_index], debuff)
		CoopIntentType.DEFEND, CoopIntentType.DEFEND_BUFF:
			enemy.block = enemy.get("block", 0) + enemy.current_intent.value

func _select_target_player(enemy: Dictionary) -> int:
	var alive_indices: Array = []
	for i in range(_players.size()):
		if _players[i].current_hp > 0:
			alive_indices.append(i)
	if alive_indices.is_empty():
		return 0
	return alive_indices[_rng.randi_range(0, alive_indices.size() - 1)]

func _calculate_enemy_damage(base_damage: int) -> int:
	return base_damage

func _apply_damage_to_coop_player(player_index: int, damage: int) -> void:
	var player: Dictionary = _players[player_index]
	if player.block > 0:
		if damage >= player.block:
			damage -= player.block
			player.block = 0
		else:
			player.block -= damage
			damage = 0
	player.current_hp -= damage

func _all_players_dead() -> bool:
	for player in _players:
		if player.current_hp > 0:
			return false
	return true

func _has_valid_target(card: Dictionary) -> bool:
	var target: int = card.get("target", 0)
	match target:
		CoopCardTarget.ALL, CoopCardTarget.SELF, CoopCardTarget.NONE:
			return true
		CoopCardTarget.ENEMY_SINGLE, CoopCardTarget.ENEMY_ALL:
			for e in _enemies:
				if not _is_enemy_dead(e):
					return true
			return false
		_:
			return true

func _get_targets(target: int, target_index: int) -> Array:
	var targets: Array = []
	match target:
		CoopCardTarget.ENEMY_SINGLE:
			if target_index >= 0 and target_index < _enemies.size() and not _is_enemy_dead(_enemies[target_index]):
				targets.append(_enemies[target_index])
			else:
				for e in _enemies:
					if not _is_enemy_dead(e):
						targets.append(e)
						break
		CoopCardTarget.ENEMY_ALL:
			for e in _enemies:
				if not _is_enemy_dead(e):
					targets.append(e)
	return targets

func _generate_enemy_intent(enemy: Dictionary) -> void:
	var roll: float = _rng.randf()
	var base_id: String = enemy.id.split("_")[0]
	var damage_scale: float = 1.0 + (_players.size() - 1) * 0.3

	match base_id:
		"Cultist":
			if _turn_number % 3 == 0:
				enemy.current_intent = {"type": CoopIntentType.BUFF, "value": 3, "value2": 0, "description": "仪式 +3", "icon": "⬆"}
			else:
				var dmg: int = int((_rng.randi_range(6, 9)) * damage_scale)
				enemy.current_intent = {"type": CoopIntentType.ATTACK, "value": dmg, "value2": 0, "description": "攻击 %d" % dmg, "icon": "⚔"}
		_:
			var dmg: int = int((_rng.randi_range(5, 10)) * damage_scale)
			enemy.current_intent = {"type": CoopIntentType.ATTACK, "value": dmg, "value2": 0, "description": "攻击 %d" % dmg, "icon": "⚔"}

func _get_card_status_effect(card: Dictionary) -> Dictionary:
	var name_lower: String = card.name.to_lower()
	if "虚弱" in name_lower or "weak" in name_lower:
		return _create_weak(card.magic_number)
	if "脆弱" in name_lower or "vulnerable" in name_lower or "痛击" in name_lower:
		return _create_vulnerable(card.magic_number)
	return _create_vulnerable(card.magic_number)

func _check_combat_end() -> void:
	if _is_combat_over: return
	var all_dead: bool = true
	for e in _enemies:
		if not _is_enemy_dead(e):
			all_dead = false
			break
	if all_dead:
		_is_combat_over = true
		coop_combat_won.emit()
		return
	if _all_players_dead():
		_is_combat_over = true
		coop_combat_lost.emit()

func _is_enemy_dead(enemy: Dictionary) -> bool:
	return enemy.current_hp <= 0

func _process_start_turn_status_effects(unit: Dictionary) -> void:
	var effects: Array = unit.get("status_effects", [])
	for effect in effects:
		if effect.name == "Metallicize" and effect.stacks > 0:
			unit.block += effect.stacks
		if effect.name == "DemonForm" and effect.stacks > 0:
			_add_status(unit, _create_strength(effect.stacks))

func _process_end_turn_status_effects(unit: Dictionary) -> void:
	var effects: Array = unit.get("status_effects", [])
	var effects_copy: Array = effects.duplicate()
	for effect in effects_copy:
		if effect.name == "Poison" and effect.stacks > 0:
			var poison_dmg: int = effect.stacks
			if unit.block > 0:
				if poison_dmg >= unit.block:
					poison_dmg -= unit.block
					unit.block = 0
				else:
					unit.block -= poison_dmg
					poison_dmg = 0
			unit.current_hp -= poison_dmg
			effect.stacks = maxi(0, effect.stacks - 1)
		if effect.name == "Vulnerable" or effect.name == "Weak":
			effect.stacks = maxi(0, effect.stacks - 1)
		if effect.name == "Ritual" and effect.stacks > 0:
			_add_status(unit, _create_strength(effect.stacks))
		effect.duration -= 1
	var to_remove: Array = []
	for effect in effects:
		if effect.duration == 0 or (effect.name == "Poison" and effect.stacks <= 0):
			to_remove.append(effect)
	for effect in to_remove:
		effects.erase(effect)

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
	return {"id": "weak", "name": "Weak", "stacks": stacks, "duration": -1, "is_buff": false}

func _create_vulnerable(stacks: int) -> Dictionary:
	return {"id": "vulnerable", "name": "Vulnerable", "stacks": stacks, "duration": -1, "is_buff": false}

func _create_strength(stacks: int) -> Dictionary:
	return {"id": "strength", "name": "Strength", "stacks": stacks, "duration": -1, "is_buff": true}

func get_sync_state() -> Dictionary:
	var players_state: Array = []
	for p in _players:
		players_state.append({
			"index": p.index,
			"name": p.name,
			"current_hp": p.current_hp,
			"max_hp": p.max_hp,
			"block": p.block,
			"energy": p.energy,
			"max_energy": p.max_energy,
			"hand_count": p.hand.size(),
		})
	var enemies_state: Array = []
	for e in _enemies:
		enemies_state.append({
			"id": e.id,
			"name": e.name,
			"current_hp": e.current_hp,
			"max_hp": e.max_hp,
			"block": e.get("block", 0),
			"intent": e.current_intent,
		})
	return {
		"turn": _turn_number,
		"current_player_index": _current_player_index,
		"phase": _current_phase,
		"players": players_state,
		"enemies": enemies_state,
		"combat_over": _is_combat_over,
	}

func apply_remote_card_play(player_index: int, card_data: Dictionary, target_index: int) -> void:
	_is_bot_turn_processing = false
	var player: Dictionary = _players[player_index]
	player.energy -= card_data.cost
	player.hand.erase(card_data)
	_execute_coop_card_effect(player_index, card_data, target_index, {})
	if card_data.exhaust or card_data.ethereal:
		player.exhaust_pile.append(card_data)
	elif not card_data.retain:
		player.discard_pile.append(card_data)
	_check_combat_end()
	coop_card_played.emit(player_index, card_data, target_index)

func apply_remote_turn_end(player_index: int) -> void:
	_is_bot_turn_processing = false
	end_coop_player_turn(player_index)
