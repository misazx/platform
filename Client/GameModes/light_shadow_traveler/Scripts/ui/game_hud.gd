class_name GameHUD
extends CanvasLayer

signal form_button_pressed

var _health_hearts: Array[TextureRect] = []
var _fragment_label: Label
var _form_indicator: PanelContainer
var _form_label: Label
var _level_label: Label
var _tutorial_label: Label
var _energy_bar: ProgressBar
var _energy_label: Label
var _timer_label: Label
var _progress_bar: ProgressBar
var _progress_label: Label
var _damage_overlay: ColorRect
var _combo_label: Label
var _screen_flash: ColorRect

var _elapsed_time := 0.0
var _is_timing := false
var _damage_flash_timer := 0.0
var _combo_count := 0
var _combo_timer := 0.0

func _ready() -> void:
	layer = 10
	_setup_hud()

func _setup_hud() -> void:
	var top_bar := HBoxContainer.new()
	top_bar.name = "TopBar"
	top_bar.anchors_preset = Control.PRESET_TOP_WIDE
	top_bar.offset_bottom = 40
	top_bar.add_theme_constant_override("separation", 8)
	add_child(top_bar)
	for i in range(3):
		var heart := TextureRect.new()
		heart.name = "Heart%d" % i
		heart.custom_minimum_size = Vector2(28, 28)
		heart.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTER
		heart.texture = _make_heart_texture(true)
		_health_hearts.append(heart)
		top_bar.add_child(heart)
	_fragment_label = Label.new()
	_fragment_label.name = "FragmentLabel"
	_fragment_label.custom_minimum_size = Vector2(80, 28)
	_fragment_label.add_theme_color_override("font_color", Color(0.8, 0.7, 1.0))
	_fragment_label.add_theme_font_size_override("font_size", 16)
	top_bar.add_child(_fragment_label)
	_timer_label = Label.new()
	_timer_label.name = "TimerLabel"
	_timer_label.custom_minimum_size = Vector2(80, 28)
	_timer_label.add_theme_color_override("font_color", Color(0.9, 0.9, 0.9))
	_timer_label.add_theme_font_size_override("font_size", 16)
	_timer_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	top_bar.add_child(_timer_label)
	var spacer := Control.new()
	spacer.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	top_bar.add_child(spacer)
	_level_label = Label.new()
	_level_label.name = "LevelLabel"
	_level_label.add_theme_color_override("font_color", Color(0.9, 0.9, 0.9))
	_level_label.add_theme_font_size_override("font_size", 14)
	top_bar.add_child(_level_label)
	_form_indicator = PanelContainer.new()
	_form_indicator.name = "FormIndicator"
	_form_indicator.custom_minimum_size = Vector2(120, 36)
	_form_indicator.anchors_preset = Control.PRESET_TOP_LEFT
	_form_indicator.offset_left = 10
	_form_indicator.offset_top = 48
	_form_indicator.mouse_filter = Control.MOUSE_FILTER_STOP
	_form_indicator.gui_input.connect(_on_form_button_pressed)
	add_child(_form_indicator)
	_form_label = Label.new()
	_form_label.name = "FormLabel"
	_form_label.text = "光形态"
	_form_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_form_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	_form_label.add_theme_color_override("font_color", Color(1.0, 0.95, 0.8))
	_form_label.add_theme_font_size_override("font_size", 14)
	_form_indicator.add_child(_form_label)
	_energy_bar = ProgressBar.new()
	_energy_bar.name = "EnergyBar"
	_energy_bar.anchors_preset = Control.PRESET_TOP_LEFT
	_energy_bar.offset_left = 10
	_energy_bar.offset_top = 90
	_energy_bar.custom_minimum_size = Vector2(120, 12)
	_energy_bar.max_value = 100.0
	_energy_bar.value = 100.0
	_energy_bar.show_percentage = false
	var style_bg := StyleBoxFlat.new()
	style_bg.bg_color = Color(0.2, 0.2, 0.2, 0.8)
	_energy_bar.add_theme_stylebox_override("background", style_bg)
	var style_fill := StyleBoxFlat.new()
	style_fill.bg_color = Color(1.0, 0.95, 0.7, 0.9)
	_energy_bar.add_theme_stylebox_override("fill", style_fill)
	add_child(_energy_bar)
	_energy_label = Label.new()
	_energy_label.name = "EnergyLabel"
	_energy_label.anchors_preset = Control.PRESET_TOP_LEFT
	_energy_label.offset_left = 10
	_energy_label.offset_top = 104
	_energy_label.add_theme_color_override("font_color", Color(0.8, 0.8, 0.8))
	_energy_label.add_theme_font_size_override("font_size", 10)
	_energy_label.text = "能量"
	add_child(_energy_label)
	_progress_bar = ProgressBar.new()
	_progress_bar.name = "LevelProgress"
	_progress_bar.anchors_preset = Control.PRESET_BOTTOM_WIDE
	_progress_bar.offset_top = -16
	_progress_bar.offset_bottom = -4
	_progress_bar.max_value = 100.0
	_progress_bar.value = 0.0
	_progress_bar.show_percentage = false
	var prog_bg := StyleBoxFlat.new()
	prog_bg.bg_color = Color(0.15, 0.15, 0.15, 0.8)
	_progress_bar.add_theme_stylebox_override("background", prog_bg)
	var prog_fill := StyleBoxFlat.new()
	prog_fill.bg_color = Color(0.5, 0.8, 0.5, 0.8)
	_progress_bar.add_theme_stylebox_override("fill", prog_fill)
	add_child(_progress_bar)
	_progress_label = Label.new()
	_progress_label.name = "ProgressLabel"
	_progress_label.anchors_preset = Control.PRESET_BOTTOM_WIDE
	_progress_label.offset_top = -32
	_progress_label.offset_bottom = -16
	_progress_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_progress_label.add_theme_color_override("font_color", Color(0.7, 0.7, 0.7))
	_progress_label.add_theme_font_size_override("font_size", 11)
	add_child(_progress_label)
	_tutorial_label = Label.new()
	_tutorial_label.name = "TutorialLabel"
	_tutorial_label.anchors_preset = Control.PRESET_CENTER_BOTTOM
	_tutorial_label.offset_bottom = -60
	_tutorial_label.offset_top = -90
	_tutorial_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_tutorial_label.add_theme_color_override("font_color", Color(1.0, 1.0, 1.0))
	_tutorial_label.add_theme_font_size_override("font_size", 18)
	_tutorial_label.visible = false
	add_child(_tutorial_label)
	_damage_overlay = ColorRect.new()
	_damage_overlay.name = "DamageOverlay"
	_damage_overlay.anchors_preset = Control.PRESET_FULL_RECT
	_damage_overlay.color = Color(1.0, 0.0, 0.0, 0.0)
	_damage_overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_damage_overlay)
	_screen_flash = ColorRect.new()
	_screen_flash.name = "ScreenFlash"
	_screen_flash.anchors_preset = Control.PRESET_FULL_RECT
	_screen_flash.color = Color(1.0, 1.0, 1.0, 0.0)
	_screen_flash.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_screen_flash)
	_combo_label = Label.new()
	_combo_label.name = "ComboLabel"
	_combo_label.anchors_preset = Control.PRESET_CENTER_TOP
	_combo_label.offset_top = 120
	_combo_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_combo_label.add_theme_color_override("font_color", Color(1.0, 0.9, 0.3))
	_combo_label.add_theme_font_size_override("font_size", 24)
	_combo_label.visible = false
	add_child(_combo_label)

func _make_heart_texture(filled: bool) -> ImageTexture:
	var img := Image.create(24, 24, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	var color := Color(0.9, 0.2, 0.3) if filled else Color(0.3, 0.3, 0.3, 0.5)
	for dy in range(-10, 11):
		for dx in range(-10, 11):
			var dist := sqrt(dx * dx + dy * dy)
			if dist < 10:
				var alpha := 1.0 if filled else 0.3
				if not filled and dist < 8:
					alpha = 0.1
				img.set_pixel(12 + dx, 12 + dy, Color(color.r, color.g, color.b, alpha))
	return ImageTexture.create_from_image(img)

func _process(delta: float) -> void:
	if _is_timing:
		_elapsed_time += delta
		_update_timer_display()
	if _damage_flash_timer > 0:
		_damage_flash_timer -= delta
		var alpha := _damage_flash_timer / 0.3
		_damage_overlay.color = Color(1.0, 0.0, 0.0, alpha * 0.3)
	if _combo_timer > 0:
		_combo_timer -= delta
		if _combo_timer <= 0:
			_combo_count = 0
			_combo_label.visible = false

func _update_timer_display() -> void:
	var minutes := int(_elapsed_time) / 60
	var seconds := int(_elapsed_time) % 60
	var ms := int((_elapsed_time - int(_elapsed_time)) * 100)
	_timer_label.text = "%d:%02d.%02d" % [minutes, seconds, ms]

func update_health(current: int, max_val: int) -> void:
	for i in range(_health_hearts.size()):
		if i < max_val:
			_health_hearts[i].visible = true
			_health_hearts[i].texture = _make_heart_texture(i < current)
		else:
			_health_hearts[i].visible = false

func update_fragments(current: int, total: int) -> void:
	_fragment_label.text = "碎片 %d/%d" % [current, total]

func update_form(form_name: String) -> void:
	match form_name:
		"light":
			_form_label.text = "☀ 光形态"
			_form_label.add_theme_color_override("font_color", Color(1.0, 0.95, 0.8))
			var style := _form_indicator.get_theme_stylebox("panel") as StyleBoxFlat
			if style:
				style.bg_color = Color(0.3, 0.28, 0.15, 0.8)
			if _energy_bar:
				var fill := _energy_bar.get_theme_stylebox("fill") as StyleBoxFlat
				if fill:
					fill.bg_color = Color(1.0, 0.95, 0.7, 0.9)
		"shadow":
			_form_label.text = "☽ 影形态"
			_form_label.add_theme_color_override("font_color", Color(0.5, 0.55, 0.8))
			var style := _form_indicator.get_theme_stylebox("panel") as StyleBoxFlat
			if style:
				style.bg_color = Color(0.15, 0.15, 0.25, 0.8)
			if _energy_bar:
				var fill := _energy_bar.get_theme_stylebox("fill") as StyleBoxFlat
				if fill:
					fill.bg_color = Color(0.3, 0.35, 0.7, 0.9)

func update_energy(current: float, max_val: float) -> void:
	if _energy_bar:
		_energy_bar.value = current
		_energy_bar.max_value = max_val

func update_level_name(level_name: String) -> void:
	_level_label.text = level_name

func update_progress(progress: float) -> void:
	if _progress_bar:
		_progress_bar.value = progress * 100.0
	if _progress_label:
		_progress_label.text = "关卡进度 %d%%" % int(progress * 100)

func show_tutorial(text: String, duration: float = 3.0) -> void:
	_tutorial_label.text = text
	_tutorial_label.visible = true
	_tutorial_label.modulate.a = 0.0
	var tween := create_tween()
	tween.tween_property(_tutorial_label, "modulate:a", 1.0, 0.3)
	tween.tween_interval(duration)
	tween.tween_property(_tutorial_label, "modulate:a", 0.0, 0.5)
	tween.tween_callback(func(): _tutorial_label.visible = false)

func show_damage_flash() -> void:
	_damage_flash_timer = 0.3

func show_screen_flash(color: Color, duration: float = 0.2) -> void:
	_screen_flash.color = Color(color.r, color.g, color.b, 0.3)
	var tween := create_tween()
	tween.tween_property(_screen_flash, "color:a", 0.0, duration)

func show_combo(count: int) -> void:
	_combo_count = count
	_combo_timer = 2.0
	_combo_label.text = "连击 x%d" % count
	_combo_label.visible = true
	_combo_label.modulate.a = 0.0
	var tween := create_tween()
	tween.tween_property(_combo_label, "modulate:a", 1.0, 0.2)
	tween.tween_property(_combo_label, "scale", Vector2(1.2, 1.2), 0.1)
	tween.tween_property(_combo_label, "scale", Vector2(1.0, 1.0), 0.1)

func start_timer() -> void:
	_is_timing = true
	_elapsed_time = 0.0

func stop_timer() -> void:
	_is_timing = false

func get_elapsed_time() -> float:
	return _elapsed_time

func hide_tutorial() -> void:
	_tutorial_label.visible = false

func _on_form_button_pressed(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		form_button_pressed.emit()
