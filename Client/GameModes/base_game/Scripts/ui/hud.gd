class_name HUD extends CanvasLayer

var _floor_label: Label
var _room_label: Label
var _wave_label: Label
var _enemies_label: Label
var _health_bar: ProgressBar

func _ready() -> void:
	_setup_ui()
	_connect_signals()

func _setup_ui() -> void:
	var margin_container := MarginContainer.new()
	margin_container.set_anchors_preset(Control.PRESET_TOP_LEFT)
	margin_container.add_theme_constant_override("margin_left", 20)
	margin_container.add_theme_constant_override("margin_top", 20)
	add_child(margin_container)

	var vbox := VBoxContainer.new()
	vbox.add_theme_constant_override("separation", 10)
	margin_container.add_child(vbox)

	_floor_label = Label.new()
	_floor_label.text = "Floor: 1"
	_floor_label.add_theme_font_size_override("font_size", 20)
	vbox.add_child(_floor_label)

	_room_label = Label.new()
	_room_label.text = "Room: 0/10"
	_room_label.add_theme_font_size_override("font_size", 18)
	vbox.add_child(_room_label)

	_wave_label = Label.new()
	_wave_label.text = "Wave: 0"
	_wave_label.add_theme_font_size_override("font_size", 18)
	vbox.add_child(_wave_label)

	_enemies_label = Label.new()
	_enemies_label.text = "Enemies: 0"
	_enemies_label.add_theme_font_size_override("font_size", 16)
	vbox.add_child(_enemies_label)

	_health_bar = ProgressBar.new()
	_health_bar.min_value = 0
	_health_bar.max_value = 100
	_health_bar.value = 100
	_health_bar.custom_minimum_size = Vector2(200, 20)
	vbox.add_child(_health_bar)

func _connect_signals() -> void:
	pass

func on_floor_changed(floor_num: int) -> void:
	_floor_label.text = "Floor: %d" % floor_num

func on_room_changed(room: int) -> void:
	_room_label.text = "Room: %d/10" % room

func on_wave_started(wave: int) -> void:
	_wave_label.text = "Wave: %d" % wave

func on_wave_completed(wave: int) -> void:
	_wave_label.text = "Wave %d Complete!" % wave

func on_player_health_changed(health: int) -> void:
	var max_health := 100
	_health_bar.max_value = max_health
	_health_bar.value = health

func update_enemy_count(count: int) -> void:
	_enemies_label.text = "Enemies: %d" % count
