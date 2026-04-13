class_name GameHUD
extends CanvasLayer

var _health_icons: Array[TextureRect] = []
var _fragment_label: Label
var _form_indicator: PanelContainer
var _form_label: Label
var _level_label: Label
var _tutorial_label: RichTextLabel
var _max_health := 3
var _current_form := "light"

func _ready() -> void:
	layer = 10
	_build_ui()

func _build_ui() -> void:
	var root := Control.new()
	root.name = "HUDRoot"
	root.set_anchors_preset(Control.PRESET_FULL_RECT)
	root.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(root)
	var top_bar := HBoxContainer.new()
	top_bar.name = "TopBar"
	top_bar.anchors_preset = Control.PRESET_TOP_WIDE
	top_bar.offset_bottom = 50
	top_bar.add_theme_constant_override("separation", 20)
	top_bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
	root.add_child(top_bar)
	var health_container := HBoxContainer.new()
	health_container.name = "HealthContainer"
	health_container.add_theme_constant_override("separation", 4)
	top_bar.add_child(health_container)
	for i in range(_max_health):
		var icon := TextureRect.new()
		icon.custom_minimum_size = Vector2(28, 28)
		icon.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		icon.texture = _make_heart_texture(true)
		_health_icons.append(icon)
		health_container.add_child(icon)
	_fragment_label = Label.new()
	_fragment_label.name = "FragmentLabel"
	_fragment_label.text = "◆ 0/3"
	_fragment_label.add_theme_font_size_override("font_size", 20)
	_fragment_label.add_theme_color_override("font_color", Color(0.8, 0.7, 1.0))
	top_bar.add_child(_fragment_label)
	_level_label = Label.new()
	_level_label.name = "LevelLabel"
	_level_label.text = ""
	_level_label.add_theme_font_size_override("font_size", 18)
	_level_label.add_theme_color_override("font_color", Color(0.8, 0.8, 0.8))
	top_bar.add_child(_level_label)
	_form_indicator = PanelContainer.new()
	_form_indicator.name = "FormIndicator"
	_form_indicator.custom_minimum_size = Vector2(120, 36)
	_form_indicator.anchors_preset = Control.PRESET_CENTER_TOP
	_form_indicator.offset_top = 55
	_form_indicator.position.x -= 60
	var form_style := StyleBoxFlat.new()
	form_style.bg_color = Color(1.0, 0.95, 0.8, 0.9)
	form_style.corner_radius_top_left = 18
	form_style.corner_radius_top_right = 18
	form_style.corner_radius_bottom_left = 18
	form_style.corner_radius_bottom_right = 18
	form_style.border_width_left = 2
	form_style.border_width_right = 2
	form_style.border_width_top = 2
	form_style.border_width_bottom = 2
	form_style.border_color = Color(1.0, 0.9, 0.6)
	_form_indicator.add_theme_stylebox_override("panel", form_style)
	root.add_child(_form_indicator)
	_form_label = Label.new()
	_form_label.name = "FormLabel"
	_form_label.text = "☀ 光形态"
	_form_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_form_label.add_theme_font_size_override("font_size", 16)
	_form_label.add_theme_color_override("font_color", Color(0.4, 0.35, 0.2))
	_form_indicator.add_child(_form_label)
	_tutorial_label = RichTextLabel.new()
	_tutorial_label.name = "TutorialLabel"
	_tutorial_label.anchors_preset = Control.PRESET_CENTER_BOTTOM
	_tutorial_label.offset_top = -120
	_tutorial_label.offset_bottom = -60
	_tutorial_label.offset_left = 200
	_tutorial_label.offset_right = -200
	_tutorial_label.bbcode_enabled = true
	_tutorial_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_tutorial_label.add_theme_font_size_override("normal_font_size", 18)
	var tutorial_style := StyleBoxFlat.new()
	tutorial_style.bg_color = Color(0, 0, 0, 0.6)
	tutorial_style.corner_radius_top_left = 8
	tutorial_style.corner_radius_top_right = 8
	tutorial_style.corner_radius_bottom_left = 8
	tutorial_style.corner_radius_bottom_right = 8
	tutorial_style.content_margin_top = 8
	tutorial_style.content_margin_bottom = 8
	tutorial_style.content_margin_left = 12
	tutorial_style.content_margin_right = 12
	_tutorial_label.add_theme_stylebox_override("normal", tutorial_style)
	root.add_child(_tutorial_label)

func _make_heart_texture(filled: bool) -> ImageTexture:
	var img := Image.create(28, 28, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	var color := Color(1.0, 0.3, 0.35) if filled else Color(0.3, 0.3, 0.3, 0.5)
	for dy in range(-10, 11):
		for dx in range(-10, 11):
			var nx := (dx + 3) / 7.0
			var ny := (dy + 3) / 7.0
			var heart := nx * nx + pow(ny - sqrt(abs(nx)), 2)
			if heart < 1.8:
				img.set_pixel(14 + dx, 14 + dy, color)
	return ImageTexture.create_from_image(img)

func update_health(health: int, max_health: int) -> void:
	_max_health = max_health
	for i in range(_health_icons.size()):
		if i < _max_health:
			_health_icons[i].visible = true
			_health_icons[i].texture = _make_heart_texture(i < health)
		else:
			_health_icons[i].visible = false

func update_fragments(count: int, total: int = 3) -> void:
	_fragment_label.text = "◆ " + str(count) + "/" + str(total)

func update_form(form: String) -> void:
	_current_form = form
	var form_style := _form_indicator.get_theme_stylebox("panel") as StyleBoxFlat
	if form == "light":
		_form_label.text = "☀ 光形态"
		_form_label.add_theme_color_override("font_color", Color(0.4, 0.35, 0.2))
		form_style.bg_color = Color(1.0, 0.95, 0.8, 0.9)
		form_style.border_color = Color(1.0, 0.9, 0.6)
	else:
		_form_label.text = "☾ 影形态"
		_form_label.add_theme_color_override("font_color", Color(0.6, 0.65, 0.9))
		form_style.bg_color = Color(0.2, 0.22, 0.4, 0.9)
		form_style.border_color = Color(0.4, 0.45, 0.8)

func update_level_name(name: String) -> void:
	_level_label.text = name

func show_tutorial(text: String, duration: float = 3.0) -> void:
	_tutorial_label.text = "[center]" + text + "[/center]"
	_tutorial_label.visible = true
	var timer := get_tree().create_timer(duration)
	timer.timeout.connect(func(): _tutorial_label.visible = false)

func hide_tutorial() -> void:
	_tutorial_label.visible = false
