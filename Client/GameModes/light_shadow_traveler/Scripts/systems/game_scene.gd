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

func _ready() -> void:
	_setup_scene()
	_setup_input_actions()
	_load_and_build_level("ff_01")

func _setup_scene() -> void:
	level_manager = LevelManager.new()
	add_child(level_manager)
	level_manager.level_loaded.connect(_on_level_loaded)
	level_manager.level_completed.connect(_on_level_completed)
	level_manager.level_failed.connect(_on_level_failed)
	hud = GameHUD.new()
	add_child(hud)
	environment = EnvironmentSystem.new()
	add_child(environment)
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
	var layer := ParallaxLayer.new()
	layer.motion_mirroring = Vector2(1280, 0)
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
	var data := level_manager.get_level_data(level_id)
	if data.is_empty():
		push_error("[GameScene] No data for level: " + level_id)
		return
	_build_level(data)
	_spawn_player(data)
	_update_hud(data)
	_show_tutorial(data)

func _build_level(data: Dictionary) -> void:
	var platforms := data.get("platforms", []) as Array
	for p_data in platforms:
		var platform := FormPlatform.new()
		platform.setup_from_data(p_data as Dictionary)
		if p_data.get("type", "normal") in ["shadow", "shadow_wall"]:
			platform.add_to_group("shadow_platform")
		level_root.add_child(platform)
	var enemies := data.get("enemies", []) as Array
	for e_data in enemies:
		var enemy := FormEnemy.new()
		enemy.setup_from_data(e_data as Dictionary)
		level_root.add_child(enemy)
	var fragments := data.get("fragments", []) as Array
	for i in range(fragments.size()):
		var f_data := fragments[i] as Dictionary
		var fragment := MemoryFragment.new()
		fragment.position = Vector2(f_data.get("x", 0), f_data.get("y", 0))
		fragment.fragment_id = level_root.name + "_frag_" + str(i)
		fragment.collected.connect(_on_fragment_collected)
		level_root.add_child(fragment)
	var light_sources := data.get("lightSources", []) as Array
	for l_data in light_sources:
		var light_source := MovableLightSource.new()
		light_source.setup_from_data(l_data as Dictionary)
		level_root.add_child(light_source)
	var moving_platforms := data.get("movingPlatforms", []) as Array
	for m_data in moving_platforms:
		var mplatform := MovingPlatform.new()
		mplatform.setup_from_data(m_data as Dictionary)
		level_root.add_child(mplatform)
	var switches := data.get("switches", []) as Array
	for s_data in switches:
		var sw := LightShadowSwitch.new()
		sw.position = Vector2(s_data.get("x", 0), s_data.get("y", 0))
		sw.setup_from_data(s_data as Dictionary)
		sw.switch_activated.connect(_on_switch_activated)
		sw.switch_deactivated.connect(_on_switch_deactivated)
		level_root.add_child(sw)
	var checkpoints := data.get("checkpoints", []) as Array
	for c_data in checkpoints:
		var cp := Checkpoint.new()
		cp.position = Vector2(c_data.get("x", 0), c_data.get("y", 0))
		cp.checkpoint_id = c_data.get("id", "")
		if c_data.get("isStart", false):
			cp.is_start = true
		cp.checkpoint_activated.connect(_on_checkpoint_activated)
		level_root.add_child(cp)
	var end_pos := data.get("endPos", {}) as Dictionary
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
		var goal_img := Image.create(40, 60, false, Image.FORMAT_RGBA8)
		goal_img.fill(Color(0, 0, 0, 0))
		for dy in range(-25, 26):
			for dx in range(-15, 16):
				var dist := sqrt(dx * dx * 0.5 + dy * dy) / 25.0
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

func _spawn_player(data: Dictionary) -> void:
	player = PlayerCharacter.new()
	player.name = "Player"
	player.add_to_group("player")
	var start_pos := data.get("startPos", {}) as Dictionary
	player.position = Vector2(start_pos.get("x", 100), start_pos.get("y", 400))
	player.form_changed.connect(_on_form_changed)
	player.health_changed.connect(_on_health_changed)
	player.player_died.connect(_on_player_died)
	player.fragment_collected.connect(_on_fragment_count_changed)
	player.energy_changed.connect(_on_energy_changed)
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
	ParticleEffect.spawn_at(level_root, player.global_position, ParticleEffect.EffectType.DEATH, 30)
	hud.show_damage_flash()
	hud.stop_timer()
	level_manager.fail_level()
	get_tree().create_timer(1.5).timeout.connect(func():
		_respawn_at_checkpoint()
	)

func _on_energy_changed(current: float, max_val: float) -> void:
	hud.update_energy(current, max_val)

func _on_fragment_collected(_fragment: MemoryFragment) -> void:
	level_manager.add_fragment()
	var count := level_manager.get_fragment_count(level_manager.current_level_id)
	var data := level_manager.get_level_data(level_manager.current_level_id)
	hud.update_fragments(count, data.get("fragments", []).size())

func _on_fragment_count_changed(count: int) -> void:
	var data := level_manager.get_level_data(level_manager.current_level_id)
	hud.update_fragments(count, data.get("fragments", []).size())

func _on_goal_reached(body: Node2D) -> void:
	if body is PlayerCharacter:
		level_manager.complete_level()
		var next_id := _get_next_level_id()
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
	ParticleEffect.spawn_at(level_root, last_checkpoint_pos, ParticleEffect.EffectType.HEAL, 20)

func _on_checkpoint_activated(checkpoint_id: String) -> void:
	for child in level_root.get_children():
		if child is Checkpoint and child.checkpoint_id == checkpoint_id:
			last_checkpoint_pos = child.get_spawn_position()
			break
	ParticleEffect.spawn_at(level_root, last_checkpoint_pos, ParticleEffect.EffectType.CHECKPOINT_ACTIVATE, 25)
	hud.show_tutorial("检查点已激活", 1.5)

func _on_switch_activated(switch_id: String) -> void:
	active_switches[switch_id] = true
	var target_id := ""
	for child in level_root.get_children():
		if child is LightShadowSwitch and child.switch_id == switch_id:
			target_id = child.target_id
			break
	if target_id != "":
		for child in level_root.get_children():
			if child is FormPlatform and target_id.begins_with("sw_platform"):
				child.set_active(true)

func _on_switch_deactivated(switch_id: String) -> void:
	active_switches.erase(switch_id)

func _get_next_level_id() -> String:
	var all_ids := level_manager.levels_data.keys()
	all_ids.sort()
	var current_idx := all_ids.find(level_manager.current_level_id)
	if current_idx >= 0 and current_idx < all_ids.size() - 1:
		return all_ids[current_idx + 1]
	return ""

var current_level_id := ""

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
