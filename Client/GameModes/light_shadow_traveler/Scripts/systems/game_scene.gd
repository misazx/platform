class_name GameScene
extends Node2D

var player: PlayerCharacter
var level_manager: LevelManager
var hud: GameHUD
var current_difficulty := "normal"
var camera: Camera2D
var level_root: Node2D
var bg_parallax: ParallaxBackground
var last_checkpoint_pos := Vector2(100, 400)
var active_switches: Dictionary = {}
var environment: EnvironmentSystem
var current_level_id := ""
var current_chapter_id := ""
var _nearby_light: MovableLightSource = null
var race_manager: RaceModeManager
var coop_manager: CoopModeManager
var _game_mode := "solo"
var _level_select: LevelSelect

func _ready() -> void:
	_setup_scene()
	_setup_input_actions()
	_load_and_build_level("ff_01")

func _physics_process(_delta: float) -> void:
	if not is_instance_valid(player) or player.is_dead:
		return
	_handle_push_light()
	_sync_multiplayer_state()

func _setup_scene() -> void:
	level_manager = LevelManager.new()
	add_child(level_manager)
	level_manager.level_loaded.connect(_on_level_loaded)
	level_manager.level_completed.connect(_on_level_completed)
	level_manager.level_failed.connect(_on_level_failed)
	level_manager.all_fragments_collected.connect(_on_all_fragments_collected)
	hud = GameHUD.new()
	add_child(hud)
	environment = EnvironmentSystem.new()
	add_child(environment)
	race_manager = RaceModeManager.new()
	add_child(race_manager)
	race_manager.race_started.connect(_on_race_started)
	race_manager.race_finished.connect(_on_race_finished)
	race_manager.racer_finished.connect(_on_racer_finished)
	race_manager.racer_checkpoint.connect(_on_racer_checkpoint)
	race_manager.ghost_created.connect(_on_ghost_created)
	coop_manager = CoopModeManager.new()
	add_child(coop_manager)
	coop_manager.coop_partner_died.connect(_on_coop_partner_died)
	coop_manager.coop_partner_revived.connect(_on_coop_partner_revived)
	coop_manager.coop_switch_activated.connect(_on_coop_switch_activated)
	coop_manager.coop_puzzle_solved.connect(_on_coop_puzzle_solved)
	coop_manager.coop_level_completed.connect(_on_coop_level_completed)
	coop_manager.coop_state_synced.connect(_on_coop_state_synced)
	race_manager.racer_position_updated.connect(_on_racer_position_updated)
	coop_manager.coop_partner_position_updated.connect(_on_coop_partner_position_updated)
	_connect_multiplayer_bridge()
	bg_parallax = ParallaxBackground.new()
	bg_parallax.name = "Background"
	add_child(bg_parallax)
	_create_background()
	camera = Camera2D.new()
	camera.name = "Camera"
	camera.zoom = Vector2(1.5, 1.5)
	camera.position_smoothing_enabled = true
	camera.position_smoothing_speed = 8.0
	add_child(camera)
	level_root = Node2D.new()
	level_root.name = "LevelRoot"
	add_child(level_root)

func _setup_input_actions() -> void:
	_ensure_action("move_left", [KEY_A, KEY_LEFT])
	_ensure_action("move_right", [KEY_D, KEY_RIGHT])
	_ensure_action("jump", [KEY_SPACE, KEY_W, KEY_UP])
	_ensure_action("switch_form", [KEY_SHIFT, KEY_TAB])
	_ensure_action("push_light", [KEY_E, KEY_J])
	_ensure_action("light_dash", [KEY_Q, KEY_K])
	_ensure_action("shadow_stealth", [KEY_S, KEY_DOWN])

func _ensure_action(action_name: String, keycodes: Array) -> void:
	if InputMap.has_action(action_name):
		return
	InputMap.add_action(action_name)
	for keycode in keycodes:
		var event := InputEventKey.new()
		event.keycode = keycode
		InputMap.action_add_event(action_name, event)

func _create_background() -> void:
	_update_background("forgotten_forest")

func _update_background(chapter_id: String) -> void:
	for child in bg_parallax.get_children():
		child.queue_free()
	var bg_map: Dictionary = {
		"forgotten_forest": "res://GameModes/light_shadow_traveler/Resources/Backgrounds/forest_bg.png",
		"faded_studio": "res://GameModes/light_shadow_traveler/Resources/Backgrounds/studio_bg.png",
		"silent_concert_hall": "res://GameModes/light_shadow_traveler/Resources/Backgrounds/concert_hall_bg.png",
		"sleeping_library": "res://GameModes/light_shadow_traveler/Resources/Backgrounds/library_bg.png",
		"light_shadow_temple": "res://GameModes/light_shadow_traveler/Resources/Backgrounds/temple_bg.png",
	}
	var bg_path: String = bg_map.get(chapter_id, "")
	var layer := ParallaxLayer.new()
	layer.motion_mirroring = Vector2(1280, 0)
	if bg_path != "" and ResourceLoader.exists(bg_path):
		var bg_tex: Texture2D = load(bg_path) as Texture2D
		if bg_tex:
			var bg_sprite := Sprite2D.new()
			bg_sprite.texture = bg_tex
			bg_sprite.scale = Vector2(1280.0 / bg_tex.get_width(), 720.0 / bg_tex.get_height())
			bg_sprite.centered = false
			layer.add_child(bg_sprite)
			bg_parallax.add_child(layer)
			return
	var bg_rect := ColorRect.new()
	bg_rect.size = Vector2(1280, 720)
	bg_rect.color = Color(0.08, 0.12, 0.08)
	layer.add_child(bg_rect)
	var stars_rect := ColorRect.new()
	stars_rect.size = Vector2(1280, 300)
	stars_rect.position = Vector2(0, 0)
	stars_rect.color = Color(0.05, 0.08, 0.15, 0.5)
	layer.add_child(stars_rect)
	bg_parallax.add_child(layer)

func _load_and_build_level(level_id: String) -> void:
	_clear_level()
	level_manager.load_level(level_id)

func _on_level_loaded(level_id: String) -> void:
	var data: Dictionary = level_manager.get_level_data(level_id)
	if data.is_empty():
		push_error("[GameScene] No data for level: " + level_id)
		return
	var chapter_id: String = data.get("chapter_id", "")
	if chapter_id != current_chapter_id:
		current_chapter_id = chapter_id
		_update_background(chapter_id)
	_build_level(data)
	_spawn_player(data)
	_update_hud(data)
	_show_tutorial(data)

func _build_level(data: Dictionary) -> void:
	var platforms: Array = data.get("platforms", []) as Array
	var plat_idx := 0
	for p_data in platforms:
		var platform := FormPlatform.new()
		platform.setup_from_data(p_data as Dictionary)
		var p_id: String = p_data.get("id", "")
		if p_id == "":
			p_id = "sw_platform_" + str(plat_idx)
		platform.platform_id = p_id
		platform.name = p_id
		if p_data.get("type", "normal") in ["shadow", "shadow_wall"]:
			platform.add_to_group("shadow_platform")
		level_root.add_child(platform)
		plat_idx += 1
	var moving_platforms: Array = data.get("movingPlatforms", []) as Array
	var mp_idx := 0
	for m_data in moving_platforms:
		var mplatform := MovingPlatform.new()
		mplatform.setup_from_data(m_data as Dictionary)
		var m_id: String = m_data.get("id", "")
		if m_id == "":
			m_id = "moving_plat_" + str(mp_idx)
		mplatform.platform_id = m_id
		mplatform.name = m_id
		level_root.add_child(mplatform)
		mp_idx += 1
	var enemies: Array = data.get("enemies", []) as Array
	for e_data in enemies:
		var enemy := FormEnemy.new()
		enemy.setup_from_data(e_data as Dictionary)
		level_root.add_child(enemy)
	var fragments: Array = data.get("fragments", []) as Array
	for i in range(fragments.size()):
		var f_data: Dictionary = fragments[i] as Dictionary
		var fragment := MemoryFragment.new()
		fragment.position = Vector2(f_data.get("x", 0), f_data.get("y", 0))
		fragment.fragment_id = level_root.name + "_frag_" + str(i)
		fragment.collected.connect(_on_fragment_collected)
		level_root.add_child(fragment)
	var light_sources: Array = data.get("lightSources", []) as Array
	for l_data in light_sources:
		var light_source := MovableLightSource.new()
		light_source.setup_from_data(l_data as Dictionary)
		level_root.add_child(light_source)
	var switches: Array = data.get("switches", []) as Array
	for s_data in switches:
		var sw := LightShadowSwitch.new()
		sw.position = Vector2(s_data.get("x", 0), s_data.get("y", 0))
		sw.setup_from_data(s_data as Dictionary)
		sw.switch_activated.connect(_on_switch_activated)
		sw.switch_deactivated.connect(_on_switch_deactivated)
		level_root.add_child(sw)
	var checkpoints: Array = data.get("checkpoints", []) as Array
	for c_data in checkpoints:
		var cp := Checkpoint.new()
		cp.position = Vector2(c_data.get("x", 0), c_data.get("y", 0))
		cp.checkpoint_id = c_data.get("id", "")
		if c_data.get("isStart", false):
			cp.is_start = true
		cp.checkpoint_activated.connect(_on_checkpoint_activated)
		level_root.add_child(cp)
	var end_pos: Dictionary = data.get("endPos", {}) as Dictionary
	if not end_pos.is_empty():
		var goal := Area2D.new()
		goal.name = "LevelGoal"
		goal.position = Vector2(end_pos.get("x", 1200), end_pos.get("y", 400))
		var shape_node := CollisionShape2D.new()
		var shape := RectangleShape2D.new()
		shape.size = Vector2(40, 60)
		shape_node.shape = shape
		goal.add_child(shape_node)
		var goal_sprite := Sprite2D.new()
		var goal_path := "res://GameModes/light_shadow_traveler/Resources/UI/goal_portal.png"
		if ResourceLoader.exists(goal_path):
			var goal_tex: Texture2D = load(goal_path) as Texture2D
			if goal_tex:
				goal_sprite.texture = goal_tex
				goal_sprite.scale = Vector2(50.0 / goal_tex.get_width(), 70.0 / goal_tex.get_height())
		if goal_sprite.texture == null:
			var goal_img := Image.create(40, 60, false, Image.FORMAT_RGBA8)
			goal_img.fill(Color(0, 0, 0, 0))
			for dy in range(-25, 26):
				for dx in range(-15, 16):
					var dist: float = sqrt(dx * dx * 0.5 + dy * dy) / 25.0
					if dist < 1.0:
						goal_img.set_pixel(20 + dx, 30 + dy, Color(0.9, 0.85, 1.0, (1.0 - dist) * 0.7))
			goal_sprite.texture = ImageTexture.create_from_image(goal_img)
		goal.add_child(goal_sprite)
		var goal_glow := PointLight2D.new()
		goal_glow.color = Color(0.9, 0.85, 1.0, 0.5)
		goal_glow.energy = 1.0
		goal.add_child(goal_glow)
		goal.body_entered.connect(_on_goal_reached)
		level_root.add_child(goal)
	var shadow_zones: Array = data.get("shadowZones", []) as Array
	for sz_data in shadow_zones:
		var sz := ShadowZone.new()
		sz.position = Vector2(sz_data.get("x", 0), sz_data.get("y", 0))
		sz.zone_radius = sz_data.get("radius", 150.0)
		sz.heal_rate = sz_data.get("healRate", 5.0)
		sz.speed_boost = sz_data.get("speedBoost", 1.3)
		sz.zone_entered.connect(_on_zone_entered)
		sz.zone_exited.connect(_on_zone_exited)
		level_root.add_child(sz)
	var light_zones: Array = data.get("lightZones", []) as Array
	for lz_data in light_zones:
		var lz := LightZone.new()
		lz.position = Vector2(lz_data.get("x", 0), lz_data.get("y", 0))
		lz.zone_radius = lz_data.get("radius", 150.0)
		lz.light_energy = lz_data.get("energy", 1.5)
		lz.is_permanent = lz_data.get("permanent", true)
		lz.damage_rate = lz_data.get("damageRate", 2.0)
		lz.zone_entered.connect(_on_zone_entered)
		lz.zone_exited.connect(_on_zone_exited)
		level_root.add_child(lz)

func _spawn_player(data: Dictionary) -> void:
	player = PlayerCharacter.new()
	player.name = "Player"
	player.add_to_group("player")
	var start_pos: Dictionary = data.get("startPos", {}) as Dictionary
	player.position = Vector2(start_pos.get("x", 100), start_pos.get("y", 400))
	player.form_changed.connect(_on_form_changed)
	player.health_changed.connect(_on_health_changed)
	player.player_died.connect(_on_player_died)
	player.fragment_collected.connect(_on_fragment_count_changed)
	player.energy_changed.connect(_on_energy_changed)
	player.checkpoint_reached.connect(_on_checkpoint_activated)
	level_root.add_child(player)
	if is_instance_valid(camera) and camera.get_parent():
		camera.get_parent().remove_child(camera)
	else:
		camera = Camera2D.new()
		camera.name = "Camera"
		camera.zoom = Vector2(1.5, 1.5)
		camera.position_smoothing_enabled = true
		camera.position_smoothing_speed = 8.0
	player.add_child(camera)

func _update_hud(data: Dictionary) -> void:
	hud.update_health(player.max_health, player.max_health)
	hud.update_fragments(0, data.get("fragments", []).size())
	hud.update_form("light")
	hud.update_level_name(data.get("name", ""))
	hud.update_energy(player.form_energy, PlayerCharacter.MAX_FORM_ENERGY)
	hud.start_timer()
	if not hud.form_button_pressed.is_connected(_on_hud_form_button_pressed):
		hud.form_button_pressed.connect(_on_hud_form_button_pressed)

func _on_hud_form_button_pressed() -> void:
	if player and not player.is_dead:
		player._switch_form()

func _show_tutorial(data: Dictionary) -> void:
	var focus: String = data.get("tutorialFocus", "")
	match focus:
		"move":
			hud.show_tutorial("使用 A/D 或 ←/→ 移动", 4.0)
		"jump":
			hud.show_tutorial("按 空格 或 W 跳跃", 4.0)
		"form_switch":
			hud.show_tutorial("按 Shift 切换光/影形态", 4.0)
		"light_form":
			hud.show_tutorial("光形态可踩上金色光平台", 4.0)
		"shadow_form":
			hud.show_tutorial("影形态可踩上蓝紫色影平台", 4.0)
		"shadow_wall":
			hud.show_tutorial("影形态可穿过薄影墙", 4.0)
		"mixed_forms":
			hud.show_tutorial("快速切换光/影形态通过交替平台", 4.0)
		"push_light":
			hud.show_tutorial("按 E 推动光源改变影子方向", 4.0)
		"switch_mechanic":
			hud.show_tutorial("踩上开关激活对应平台", 4.0)
		"enemy_dodge":
			hud.show_tutorial("躲避敌人！不同形态可免疫对应敌人", 4.0)
		"boss":
			hud.show_tutorial("Boss战！利用光影形态切换取胜", 4.0)
		"combined":
			hud.show_tutorial("综合运用所有技巧", 4.0)
		"precision_jump":
			hud.show_tutorial("精确跳跃！影形态可滑翔减速", 4.0)

func _clear_level() -> void:
	for child in level_root.get_children():
		child.queue_free()

func _on_form_changed(form: String) -> void:
	hud.update_form(form)

func _on_health_changed(health: int, max_health: int) -> void:
	hud.update_health(health, max_health)

func _on_player_died() -> void:
	ParticleEffect.create_and_spawn(level_root, player.global_position, ParticleEffect.EffectType.DEATH, 30)
	hud.show_damage_flash()
	hud.stop_timer()
	level_manager.fail_level()
	if _game_mode == "race":
		pass
	elif _game_mode == "coop":
		coop_manager.on_local_died()
		var bridge: MultiplayerBridge = MultiplayerBridge.instance
		if bridge:
			bridge.send_coop_player_died()
	get_tree().create_timer(1.5).timeout.connect(func():
		_respawn_at_checkpoint()
	)

func _on_energy_changed(current: float, max_val: float) -> void:
	hud.update_energy(current, max_val)

func _on_fragment_collected(_fragment: MemoryFragment) -> void:
	level_manager.add_fragment()
	var count: int = level_manager.get_fragment_count(level_manager.current_level_id)
	var data: Dictionary = level_manager.get_level_data(level_manager.current_level_id)
	hud.update_fragments(count, data.get("fragments", []).size())

func _on_fragment_count_changed(count: int) -> void:
	var data: Dictionary = level_manager.get_level_data(level_manager.current_level_id)
	hud.update_fragments(count, data.get("fragments", []).size())

func _on_all_fragments_collected(total: int) -> void:
	hud.show_tutorial("所有记忆碎片已收集！", 2.0)
	ParticleEffect.create_and_spawn(level_root, player.global_position if is_instance_valid(player) else Vector2.ZERO, ParticleEffect.EffectType.FRAGMENT_COLLECT, 30)

func _on_goal_reached(body: Node2D) -> void:
	if body is PlayerCharacter:
		if _game_mode == "race":
			race_manager.on_local_finish()
			var bridge: MultiplayerBridge = MultiplayerBridge.instance
			if bridge:
				var finish_time: float = Time.get_ticks_msec() / 1000.0 - race_manager._race_start_time
				bridge.send_race_finish(race_manager._local_racer_id, finish_time)
		elif _game_mode == "coop":
			coop_manager.on_level_completed(level_manager.current_level_id)
		level_manager.complete_level()
		var next_id: String = _get_next_level_id()
		if next_id != "":
			get_tree().create_timer(1.0).timeout.connect(func():
				_load_and_build_level(next_id)
			)
		else:
			hud.show_tutorial("恭喜通关！所有关卡已完成！", 10.0)

func _on_level_completed(_level_id: String) -> void:
	hud.show_tutorial("关卡完成！", 2.0)

func _on_level_failed(_level_id: String) -> void:
	hud.show_tutorial("再试一次！", 2.0)

func _respawn_at_checkpoint() -> void:
	if is_instance_valid(player):
		player.queue_free()
	player = PlayerCharacter.new()
	player.name = "Player"
	player.add_to_group("player")
	player.position = last_checkpoint_pos
	player.form_changed.connect(_on_form_changed)
	player.health_changed.connect(_on_health_changed)
	player.player_died.connect(_on_player_died)
	player.fragment_collected.connect(_on_fragment_count_changed)
	player.energy_changed.connect(_on_energy_changed)
	player.checkpoint_reached.connect(_on_checkpoint_activated)
	level_root.add_child(player)
	if is_instance_valid(camera) and camera.get_parent():
		camera.get_parent().remove_child(camera)
	else:
		camera = Camera2D.new()
		camera.name = "Camera"
		camera.zoom = Vector2(1.5, 1.5)
		camera.position_smoothing_enabled = true
		camera.position_smoothing_speed = 8.0
	player.add_child(camera)
	hud.update_health(player.max_health, player.max_health)
	hud.update_form("light")
	hud.update_energy(player.form_energy, PlayerCharacter.MAX_FORM_ENERGY)
	hud.start_timer()
	ParticleEffect.create_and_spawn(level_root, last_checkpoint_pos, ParticleEffect.EffectType.HEAL, 20)

func _on_checkpoint_activated(checkpoint_id: String) -> void:
	for child in level_root.get_children():
		if child is Checkpoint and child.checkpoint_id == checkpoint_id:
			last_checkpoint_pos = child.get_spawn_position()
			break
	ParticleEffect.create_and_spawn(level_root, last_checkpoint_pos, ParticleEffect.EffectType.CHECKPOINT_ACTIVATE, 25)
	hud.show_tutorial("检查点已激活", 1.5)
	if _game_mode == "race":
		race_manager.on_local_checkpoint(checkpoint_id)
		var bridge: MultiplayerBridge = MultiplayerBridge.instance
		if bridge:
			bridge.send_race_checkpoint(race_manager._local_racer_id, checkpoint_id)

func _on_switch_activated(switch_id: String) -> void:
	active_switches[switch_id] = true
	_activate_switch_target(switch_id, true)
	if _game_mode == "coop":
		coop_manager.on_local_switch_activated(switch_id)
		var bridge: MultiplayerBridge = MultiplayerBridge.instance
		if bridge:
			bridge.send_coop_switch(switch_id, true)

func _on_switch_deactivated(switch_id: String) -> void:
	active_switches.erase(switch_id)
	_activate_switch_target(switch_id, false)
	if _game_mode == "coop":
		coop_manager.on_switch_deactivated(switch_id)
		var bridge: MultiplayerBridge = MultiplayerBridge.instance
		if bridge:
			bridge.send_coop_switch(switch_id, false)

func _activate_switch_target(switch_id: String, active: bool) -> void:
	var target_id := ""
	for child in level_root.get_children():
		if child is LightShadowSwitch and child.switch_id == switch_id:
			target_id = child.target_id
			break
	if target_id == "":
		return
	for child in level_root.get_children():
		if child is FormPlatform:
			if child.name == target_id or child.platform_id == target_id:
				child.set_active(active)
		elif child is MovingPlatform:
			if child.name == target_id:
				child.set_active(active)

func _get_next_level_id() -> String:
	var all_ids: Array = level_manager.levels_data.keys()
	all_ids.sort()
	var current_idx: int = all_ids.find(level_manager.current_level_id)
	if current_idx >= 0 and current_idx < all_ids.size() - 1:
		return all_ids[current_idx + 1]
	return ""


func load_level_by_id(level_id: String) -> void:
	current_level_id = level_id
	_load_and_build_level(level_id)

func set_difficulty(difficulty: String) -> void:
	current_difficulty = difficulty
	match difficulty:
		"casual":
			if player:
				player.max_health = 99
				player.current_health = 99
		"hardcore":
			if player:
				player.max_health = 1
				player.current_health = 1
		_:
			if player:
				player.max_health = 3
				player.current_health = 3

func _handle_push_light() -> void:
	if not Input.is_action_pressed("push_light"):
		if _nearby_light != null:
			_nearby_light.stop_push()
			_nearby_light = null
		return
	if _nearby_light == null or not is_instance_valid(_nearby_light):
		_nearby_light = _find_nearby_pushable_light()
	if _nearby_light == null:
		return
	var push_dir: float = 0.0
	if Input.is_action_pressed("move_left"):
		push_dir = -1.0
	elif Input.is_action_pressed("move_right"):
		push_dir = 1.0
	if push_dir != 0.0:
		_nearby_light.start_push(push_dir)
	else:
		_nearby_light.stop_push()

func _find_nearby_pushable_light() -> MovableLightSource:
	var lights := get_tree().get_nodes_in_group("pushable_light")
	if lights.is_empty():
		for child in level_root.get_children():
			if child is MovableLightSource:
				if not child.is_in_group("pushable_light"):
					child.add_to_group("pushable_light")
				lights.append(child)
	var closest: MovableLightSource = null
	var closest_dist: float = 80.0
	for light in lights:
		var ml: MovableLightSource = light as MovableLightSource
		if ml and ml.is_pushable:
			var dist: float = player.global_position.distance_to(ml.global_position)
			if dist < closest_dist:
				closest_dist = dist
				closest = ml
	return closest

func start_race_mode(racer_ids: Array, local_id: String) -> void:
	_game_mode = "race"
	race_manager.initialize_race(racer_ids, local_id)
	race_manager.start_race()

func start_coop_mode(local_role: int) -> void:
	_game_mode = "coop"
	coop_manager.initialize_coop(local_role)

func _on_race_started() -> void:
	hud.show_tutorial("竞速开始！", 2.0)

func _on_race_finished(results: Array) -> void:
	var first: Dictionary = results[0] if results.size() > 0 else {}
	var winner_name: String = first.get("name", "未知")
	hud.show_tutorial("竞速结束！冠军: " + winner_name, 5.0)

func _on_racer_finished(racer_id: String, finish_time: float, placement: int) -> void:
	if racer_id == race_manager._local_racer_id:
		hud.show_tutorial("你获得第" + str(placement) + "名！", 3.0)

func _on_racer_checkpoint(racer_id: String, checkpoint_id: String) -> void:
	if racer_id == race_manager._local_racer_id:
		if is_instance_valid(player):
			player.light_speed += RaceModeManager.CHECKPOINT_BOOST_SPEED
			player.shadow_speed += RaceModeManager.CHECKPOINT_BOOST_SPEED
			get_tree().create_timer(RaceModeManager.CHECKPOINT_BOOST_DURATION).timeout.connect(func():
				if is_instance_valid(player):
					player.light_speed -= RaceModeManager.CHECKPOINT_BOOST_SPEED
					player.shadow_speed -= RaceModeManager.CHECKPOINT_BOOST_SPEED
			)

func _on_ghost_created(racer_id: String, ghost_node: Node2D) -> void:
	level_root.add_child(ghost_node)

func _on_coop_partner_died() -> void:
	hud.show_tutorial("搭档已阵亡！靠近搭档加速复活", 3.0)

func _on_coop_partner_revived() -> void:
	hud.show_tutorial("搭档已复活！", 2.0)
	if is_instance_valid(player):
		player.heal(CoopModeManager.SHARED_HEAL_AMOUNT)
	var bridge: MultiplayerBridge = MultiplayerBridge.instance
	if bridge:
		bridge.send_coop_player_revived()

func _on_coop_switch_activated(switch_id: String, activated_by: String) -> void:
	if activated_by == "remote":
		_activate_switch_target(switch_id, true)

func _on_coop_puzzle_solved(puzzle_id: String, solved_by: String) -> void:
	hud.show_tutorial("谜题已解决: " + puzzle_id, 2.0)
	if solved_by == "local":
		var bridge: MultiplayerBridge = MultiplayerBridge.instance
		if bridge:
			bridge.send_coop_puzzle_solved(puzzle_id)

func _on_coop_level_completed(level_id: String) -> void:
	level_manager.complete_level()

func _on_zone_entered(zone_type: String) -> void:
	if zone_type == "shadow":
		if is_instance_valid(player) and player.is_shadow_form():
			hud.show_tutorial("暗影区域 - 持续恢复生命", 1.5)
	elif zone_type == "light":
		if is_instance_valid(player) and player.is_shadow_form():
			hud.show_tutorial("光明区域 - 影形态受伤！", 1.5)

func _on_zone_exited(zone_type: String) -> void:
	pass

func show_level_select() -> void:
	if _level_select and is_instance_valid(_level_select):
		_level_select.visible = true
		return
	_level_select = LevelSelect.new()
	_level_select.level_selected.connect(_on_level_selected)
	_level_select.back_pressed.connect(_on_level_select_back)
	_level_select.populate_levels(level_manager)
	add_child(_level_select)

func _on_level_selected(level_id: String) -> void:
	if _level_select and is_instance_valid(_level_select):
		_level_select.visible = false
	_load_and_build_level(level_id)

func _on_level_select_back() -> void:
	if _level_select and is_instance_valid(_level_select):
		_level_select.visible = false

func _connect_multiplayer_bridge() -> void:
	var bridge: MultiplayerBridge = MultiplayerBridge.instance
	if bridge == null:
		return
	bridge.bridge_race_position_updated.connect(_on_bridge_race_position)
	bridge.bridge_race_checkpoint_reached.connect(_on_bridge_race_checkpoint)
	bridge.bridge_race_finished.connect(_on_bridge_race_finish)
	bridge.bridge_coop_position_updated.connect(_on_bridge_coop_position)
	bridge.bridge_coop_switch_updated.connect(_on_bridge_coop_switch)
	bridge.bridge_coop_puzzle_solved.connect(_on_bridge_coop_puzzle)
	bridge.bridge_coop_player_died.connect(_on_bridge_coop_player_died)
	bridge.bridge_coop_player_revived.connect(_on_bridge_coop_player_revived)

func _sync_multiplayer_state() -> void:
	if not is_instance_valid(player):
		return
	var pos: Vector2 = player.global_position
	var form: String = "light" if player.is_light_form() else "shadow"
	if _game_mode == "race":
		race_manager.update_local_position(pos, form)
	elif _game_mode == "coop":
		coop_manager.update_local_state(pos, form)
		_check_coop_proximity_revive()

func _on_racer_position_updated(racer_id: String, position: Vector2, form: String) -> void:
	if _game_mode != "race":
		return
	var bridge: MultiplayerBridge = MultiplayerBridge.instance
	if bridge:
		bridge.send_race_position(racer_id, position.x, position.y, form)

func _on_coop_partner_position_updated(position: Vector2, form: String) -> void:
	pass

func _on_coop_state_synced(state: Dictionary) -> void:
	if _game_mode != "coop":
		return
	var bridge: MultiplayerBridge = MultiplayerBridge.instance
	if bridge and is_instance_valid(player):
		bridge.send_coop_position(player.global_position.x, player.global_position.y, "light" if player.is_light_form() else "shadow")

func _on_bridge_race_position(racer_id: String, x: float, y: float, form: String) -> void:
	if _game_mode == "race":
		race_manager.update_remote_racer(racer_id, Vector2(x, y), form)

func _on_bridge_race_checkpoint(racer_id: String, checkpoint_id: String) -> void:
	if _game_mode == "race":
		race_manager.on_remote_checkpoint(racer_id, checkpoint_id)

func _on_bridge_race_finish(racer_id: String, finish_time: float) -> void:
	if _game_mode == "race":
		race_manager.on_remote_finish(racer_id, finish_time)

func _on_bridge_coop_position(user_id: String, x: float, y: float, form: String) -> void:
	if _game_mode == "coop":
		coop_manager.update_partner_state(Vector2(x, y), form)

func _on_bridge_coop_switch(user_id: String, switch_id: String, activated: bool) -> void:
	if _game_mode == "coop":
		if activated:
			coop_manager.on_remote_switch_activated(switch_id)
		else:
			coop_manager.on_switch_deactivated(switch_id)

func _on_bridge_coop_puzzle(puzzle_id: String) -> void:
	if _game_mode == "coop":
		coop_manager.on_remote_puzzle_solved(puzzle_id)

func _on_bridge_coop_player_died(user_id: String) -> void:
	if _game_mode == "coop":
		coop_manager.on_partner_died()

func _on_bridge_coop_player_revived() -> void:
	if _game_mode == "coop":
		coop_manager._partner_alive = true
		coop_manager._is_reviving = false
		coop_manager._revive_timer = 0.0
		hud.show_tutorial("搭档已复活！", 2.0)
		if is_instance_valid(player):
			player.heal(CoopModeManager.SHARED_HEAL_AMOUNT)

const COOP_REVIVE_PROXIMITY: float = 80.0

func _check_coop_proximity_revive() -> void:
	if _game_mode != "coop":
		return
	if not coop_manager.is_partner_alive() and is_instance_valid(player):
		var partner_pos: Vector2 = coop_manager._partner_position
		var dist: float = player.global_position.distance_to(partner_pos)
		if dist < COOP_REVIVE_PROXIMITY:
			coop_manager.on_local_near_partner()
