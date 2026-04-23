extends Node2D

var _test_label: Label = null
var _test_timer: Timer = null
var _test_count: int = 0

func _ready() -> void:
	print("=== TEST SCENE READY ===")
	print("光影旅者 - 测试场景")

	_setup_test_ui()
	_start_test_sequence()

func _setup_test_ui() -> void:
	var bg = ColorRect.new()
	bg.anchors_preset = Control.PRESET_FULL_RECT
	bg.color = Color(0.05, 0.08, 0.12, 1.0)
	add_child(bg)

	_test_label = Label.new()
	_test_label.text = "测试场景加载中..."
	_test_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_test_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	_test_label.add_theme_font_size_override("font_size", 28)
	_test_label.add_theme_color_override("font_color", Color(0.9, 0.85, 1.0))
	_test_label.anchors_preset = Control.PRESET_CENTER
	add_child(_test_label)

	var info_label = Label.new()
	info_label.text = "按任意键返回游戏主场景"
	info_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	info_label.add_theme_font_size_override("font_size", 16)
	info_label.add_theme_color_override("font_color", Color(0.6, 0.6, 0.7))
	info_label.anchors_preset = Control.PRESET_CENTER_BOTTOM
	info_label.offset_top = -50
	add_child(info_label)

func _start_test_sequence() -> void:
	_test_timer = Timer.new()
	_test_timer.wait_time = 1.0
	_test_timer.timeout.connect(_on_test_tick)
	add_child(_test_timer)
	_test_timer.start()

func _on_test_tick() -> void:
	_test_count += 1
	match _test_count:
		1:
			_test_label.text = "检查脚本..."
		2:
			_test_label.text = "加载关卡数据..."
		3:
			_test_label.text = "初始化系统..."
		4:
			_test_label.text = "准备完成！"
		5:
			_test_label.text = "跳转到主游戏场景..."
			_test_timer.stop()
			get_tree().create_timer(0.5).timeout.connect(_go_to_game_scene)

func _go_to_game_scene() -> void:
	get_tree().change_scene_to_file("res://GameModes/light_shadow_traveler/Scenes/GameScene.tscn")

func _input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed:
		print("[TestScene] 按键按下，跳转到游戏场景")
		get_tree().change_scene_to_file("res://GameModes/light_shadow_traveler/Scenes/GameScene.tscn")
