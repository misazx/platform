extends SceneTree

func _init():
	print("=== 光影旅者 - 验证测试开始 ===")
	print("")

	# 测试 1: 检查所有脚本是否能加载
	_test_script_loading()

	# 测试 2: 检查关卡数据
	_test_level_data()

	# 测试 3: 检查配置文件
	_test_config_files()

	# 测试 4: 验证资源路径
	_test_resource_paths()

	print("")
	print("=== 验证测试完成 ===")

	# 退出
	call_deferred("quit")

func _test_script_loading():
	print("[测试 1] 检查脚本加载...")

	var scripts = [
		"res://GameModes/light_shadow_traveler/Scripts/player/player.gd",
		"res://GameModes/light_shadow_traveler/Scripts/levels/level_manager.gd",
		"res://GameModes/light_shadow_traveler/Scripts/systems/game_scene.gd",
		"res://GameModes/light_shadow_traveler/Scripts/ui/game_hud.gd",
		"res://GameModes/light_shadow_traveler/Scripts/ui/ui_theme.gd",
		"res://GameModes/light_shadow_traveler/Scripts/platforms/form_platform.gd",
		"res://GameModes/light_shadow_traveler/Scripts/enemies/form_enemy.gd",
		"res://GameModes/light_shadow_traveler/Scripts/collectibles/memory_fragment.gd",
		"res://GameModes/light_shadow_traveler/Scripts/systems/coop_mode_manager.gd",
		"res://GameModes/light_shadow_traveler/Scripts/systems/race_mode_manager.gd",
		"res://GameModes/light_shadow_traveler/Scripts/systems/bot_controller.gd"
	]

	var success = 0
	var failed = 0

	for script_path in scripts:
		if ResourceLoader.exists(script_path):
			print("  ✓ " + script_path)
			success += 1
		else:
			print("  ✗ " + script_path + " (不存在)")
			failed += 1

	print("  结果: " + str(success) + " 通过, " + str(failed) + " 失败")
	print("")

func _test_level_data():
	print("[测试 2] 检查关卡数据...")

	var level_file = "res://GameModes/light_shadow_traveler/Config/Data/levels.json"
	if not FileAccess.file_exists(level_file):
		print("  ✗ 关卡数据文件不存在")
		return

	var file = FileAccess.open(level_file, FileAccess.READ)
	var json_text = file.get_as_text()
	file.close()

	var json = JSON.new()
	var parse_result = json.parse(json_text)

	if parse_result != OK:
		print("  ✗ 关卡数据解析失败: " + json.get_error_message())
		return

	var data = json.data as Dictionary
	if data.is_empty():
		print("  ✗ 关卡数据为空")
		return

	var chapters = data.get("chapters", []) as Array
	print("  ✓ 找到 " + str(chapters.size()) + " 个章节")

	for i in range(chapters.size()):
		var chapter = chapters[i] as Dictionary
		var chapter_name = chapter.get("name", "未知")
		var levels = chapter.get("levels", []) as Array
		print("    - " + chapter_name + ": " + str(levels.size()) + " 个关卡")

	print("")

func _test_config_files():
	print("[测试 3] 检查配置文件...")

	var configs = [
		"res://GameModes/light_shadow_traveler/Config/config.json",
		"res://GameModes/light_shadow_traveler/Config/Data/package_config.json"
	]

	for config_path in configs:
		if FileAccess.file_exists(config_path):
			print("  ✓ " + config_path)
		else:
			print("  ✗ " + config_path + " (不存在)")

	print("")

func _test_resource_paths():
	print("[测试 4] 检查资源结构...")

	var resource_dirs = [
		"res://GameModes/light_shadow_traveler/Resources/Backgrounds",
		"res://GameModes/light_shadow_traveler/Resources/Characters",
		"res://GameModes/light_shadow_traveler/Resources/UI"
	]

	for dir_path in resource_dirs:
		if DirAccess.dir_exists_absolute(dir_path):
			print("  ✓ " + dir_path)
		else:
			print("  ~ " + dir_path + " (目录不存在，但游戏会使用程序化生成)")

	print("")

func _process(delta):
	return false
