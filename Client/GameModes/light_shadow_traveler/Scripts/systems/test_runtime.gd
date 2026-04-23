extends Node

var _test_results: Dictionary = {}
var _tests_passed: int = 0
var _tests_failed: int = 0

func _ready() -> void:
	print("========================================")
	print("=== TEST RUNTIME READY - 光影旅者 ===")
	print("========================================")

	_test_multiplayer_bridge()
	_test_auto_nodes()
	_test_game_mode_config()

	_print_summary()

func _test_multiplayer_bridge() -> void:
	print("\n[测试] 检查 MultiplayerBridge...")
	var bridge = MultiplayerBridge.instance
	if bridge != null:
		print("  ✓ MultiplayerBridge 实例存在")
		_tests_passed += 1
		_test_results["MultiplayerBridge"] = "通过"
	else:
		print("  ✗ MultiplayerBridge 未找到（可能未初始化）")
		_tests_failed += 1
		_test_results["MultiplayerBridge"] = "未找到"

func _test_auto_nodes() -> void:
	print("\n[测试] 检查自动加载节点...")

	var room_mgr = get_node_or_null("/root/RoomManager")
	if room_mgr != null:
		print("  ✓ RoomManager 存在")
		_tests_passed += 1
		_test_results["RoomManager"] = "通过"
	else:
		print("  ✗ RoomManager 未找到")
		_tests_failed += 1
		_test_results["RoomManager"] = "未找到"

	var auth_sys = get_node_or_null("/root/AuthSystem")
	if auth_sys != null:
		print("  ✓ AuthSystem 存在")
		_tests_passed += 1
		_test_results["AuthSystem"] = "通过"
	else:
		print("  ✗ AuthSystem 未找到")
		_tests_failed += 1
		_test_results["AuthSystem"] = "未找到"

	var pkg_svc = get_node_or_null("/root/PackageService")
	if pkg_svc != null:
		print("  ✓ PackageService 存在")
		_tests_passed += 1
		_test_results["PackageService"] = "通过"
	else:
		print("  ✗ PackageService 未找到")
		_tests_failed += 1
		_test_results["PackageService"] = "未找到"

func _test_game_mode_config() -> void:
	print("\n[测试] 检查游戏模式配置...")

	var config_path = "res://GameModes/light_shadow_traveler/Config/Data/package_config.json"
	if FileAccess.file_exists(config_path):
		print("  ✓ package_config.json 存在")
		_tests_passed += 1
		_test_results["PackageConfig"] = "通过"
	else:
		print("  ✗ package_config.json 未找到")
		_tests_failed += 1
		_test_results["PackageConfig"] = "未找到"

	var levels_path = "res://GameModes/light_shadow_traveler/Config/Data/levels.json"
	if FileAccess.file_exists(levels_path):
		print("  ✓ levels.json 存在")
		_tests_passed += 1
		_test_results["LevelsConfig"] = "通过"
	else:
		print("  ✗ levels.json 未找到")
		_tests_failed += 1
		_test_results["LevelsConfig"] = "未找到"

func _print_summary() -> void:
	print("\n========================================")
	print("=== 测试总结 ===")
	print("========================================")
	print("  通过: ", _tests_passed)
	print("  失败: ", _tests_failed)
	print("  总计: ", _tests_passed + _tests_failed)
	print("========================================")

	for test_name in _test_results:
		print("  ", test_name, ": ", _test_results[test_name])

	print("========================================")

func get_test_results() -> Dictionary:
	return _test_results.duplicate()

func get_tests_passed() -> int:
	return _tests_passed

func get_tests_failed() -> int:
	return _tests_failed
