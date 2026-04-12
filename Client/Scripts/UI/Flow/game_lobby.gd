class_name GameLobby extends Control

signal open_package_selector()
signal open_settings()
signal quit_game()

var _bg_texture_rect: TextureRect
var _title_label: Label
var _subtitle_label: Label
var _menu_container: VBoxContainer
var _player_info_bar: HBoxContainer
var _version_label: Label
var _particle_container: Node2D

func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	_build_ui()
	_start_ambient_effects()

func _build_ui() -> void:
	_bg_texture_rect = TextureRect.new()
	_bg_texture_rect.set_anchors_preset(Control.PRESET_FULL_RECT)
	_bg_texture_rect.stretch_mode = TextureRect.STRETCH_SCALE
	_bg_texture_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var bg_tex := load("res://GameModes/base_game/Resources/Images/Backgrounds/glory.png") as Texture2D
	if bg_tex != null:
		_bg_texture_rect.texture = bg_tex
	else:
		_bg_texture_rect.texture = _create_gradient_bg()
	add_child(_bg_texture_rect)

	var overlay := ColorRect.new()
	overlay.set_anchors_preset(Control.PRESET_FULL_RECT)
	overlay.color = Color(0, 0, 0, 0.45)
	overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(overlay)

	_particle_container = Node2D.new()
	_particle_container.z_index = 1
	add_child(_particle_container)

	var center_container := CenterContainer.new()
	center_container.set_anchors_preset(Control.PRESET_FULL_RECT)
	center_container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(center_container)

	var main_vbox := VBoxContainer.new()
	main_vbox.custom_minimum_size = Vector2(500, 600)
	main_vbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	main_vbox.alignment = BoxContainer.ALIGNMENT_CENTER
	center_container.add_child(main_vbox)

	var top_spacer := Control.new()
	top_spacer.custom_minimum_size = Vector2(0, 60)
	top_spacer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	main_vbox.add_child(top_spacer)

	_title_label = Label.new()
	_title_label.text = "杀戮尖塔 2"
	_title_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_title_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_title_label.add_theme_font_size_override("font_size", 52)
	_title_label.modulate = Color(1, 0.9, 0.6)
	main_vbox.add_child(_title_label)

	_subtitle_label = Label.new()
	_subtitle_label.text = "ROGUELIKE CARD GAME"
	_subtitle_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_subtitle_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_subtitle_label.add_theme_font_size_override("font_size", 14)
	_subtitle_label.modulate = Color(0.6, 0.55, 0.5, 0.8)
	main_vbox.add_child(_subtitle_label)

	var title_spacer := Control.new()
	title_spacer.custom_minimum_size = Vector2(0, 50)
	title_spacer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	main_vbox.add_child(title_spacer)

	_menu_container = VBoxContainer.new()
	_menu_container.alignment = BoxContainer.ALIGNMENT_CENTER
	_menu_container.add_theme_constant_override("separation", 14)
	_menu_container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	main_vbox.add_child(_menu_container)

	_add_menu_button("🎮  选择玩法", _on_play_pressed, Color(0.2, 0.6, 0.3))
	_add_menu_button("⚙️  设置", _on_settings_pressed, Color(0.3, 0.35, 0.5))
	_add_menu_button("🚪  退出游戏", _on_quit_pressed, Color(0.5, 0.2, 0.2))

	var bottom_spacer := Control.new()
	bottom_spacer.custom_minimum_size = Vector2(0, 40)
	bottom_spacer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	main_vbox.add_child(bottom_spacer)

	_player_info_bar = HBoxContainer.new()
	_player_info_bar.alignment = BoxContainer.ALIGNMENT_CENTER
	_player_info_bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
	main_vbox.add_child(_player_info_bar)

	var player_icon := Label.new()
	player_icon.text = "👤"
	player_icon.mouse_filter = Control.MOUSE_FILTER_IGNORE
	player_icon.add_theme_font_size_override("font_size", 18)
	_player_info_bar.add_child(player_icon)

	var player_name := Label.new()
	player_name.text = "  玩家  |  💰 0"
	player_name.mouse_filter = Control.MOUSE_FILTER_IGNORE
	player_name.add_theme_font_size_override("font_size", 14)
	player_name.modulate = Color(0.7, 0.7, 0.7)
	_player_info_bar.add_child(player_name)

	_version_label = Label.new()
	_version_label.text = "v0.2.0 Alpha"
	_version_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_version_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_version_label.add_theme_font_size_override("font_size", 11)
	_version_label.modulate = Color(0.4, 0.4, 0.4)
	main_vbox.add_child(_version_label)

func _add_menu_button(text: String, callback: Callable, accent_color: Color) -> void:
	var btn := Button.new()
	btn.text = text
	btn.custom_minimum_size = Vector2(320, 56)
	btn.mouse_filter = Control.MOUSE_FILTER_STOP
	btn.size_flags_horizontal = Control.SIZE_SHRINK_CENTER

	var normal_style := StyleBoxFlat.new()
	normal_style.bg_color = Color(0.1, 0.08, 0.12, 0.92)
	normal_style.corner_radius_top_left = 10
	normal_style.corner_radius_top_right = 10
	normal_style.corner_radius_bottom_left = 10
	normal_style.corner_radius_bottom_right = 10
	normal_style.border_width_left = 2
	normal_style.border_width_right = 2
	normal_style.border_width_top = 2
	normal_style.border_width_bottom = 2
	normal_style.border_color = accent_color
	normal_style.content_margin_top = 12
	normal_style.content_margin_bottom = 12
	normal_style.content_margin_left = 24
	normal_style.content_margin_right = 24
	btn.add_theme_stylebox_override("normal", normal_style)

	var hover_style := normal_style.duplicate() as StyleBoxFlat
	hover_style.bg_color = Color(0.15, 0.12, 0.18, 0.95)
	hover_style.border_color = accent_color.lightened(0.3)
	btn.add_theme_stylebox_override("hover", hover_style)

	var pressed_style := normal_style.duplicate() as StyleBoxFlat
	pressed_style.bg_color = Color(0.08, 0.06, 0.1, 0.98)
	btn.add_theme_stylebox_override("pressed", pressed_style)

	btn.add_theme_font_size_override("font_size", 20)
	btn.modulate = Color(0.95, 0.92, 0.88)

	btn.pressed.connect(callback)
	_menu_container.add_child(btn)

func _on_play_pressed() -> void:
	print("[GameLobby] Play pressed - opening package selector")
	open_package_selector.emit()

func _on_settings_pressed() -> void:
	print("[GameLobby] Settings pressed")
	open_settings.emit()

func _on_quit_pressed() -> void:
	print("[GameLobby] Quit pressed")
	quit_game.emit()

func _start_ambient_effects() -> void:
	var tween := create_tween().set_loops()
	tween.tween_property(_title_label, "modulate:a", 0.7, 2.0).set_ease(Tween.EASE_IN_OUT)
	tween.tween_property(_title_label, "modulate:a", 1.0, 2.0).set_ease(Tween.EASE_IN_OUT)

func _create_gradient_bg() -> ImageTexture:
	var img := Image.create(1280, 720, false, Image.FORMAT_RGBA8)
	for y in range(720):
		var t := float(y) / 720.0
		var r := 0.04 + t * 0.02
		var g := 0.03 + t * 0.01
		var b := 0.08 + t * 0.03
		for x in range(1280):
			img.set_pixel(x, y, Color(r, g, b))
	return ImageTexture.create_from_image(img)

func update_player_info(name: String, gold: int) -> void:
	for child in _player_info_bar.get_children():
		if child is Label and child.text.begins_with("  "):
			child.text = "  %s  |  💰 %d" % [name, gold]
