class_name UITheme extends RefCounted

const UI_BASE: String = "res://GameModes/base_game/Resources/UI/"

static var _cache: Dictionary = {}

static func get_icon(name: String) -> Texture2D:
	if _cache.has(name):
		return _cache[name] as Texture2D
	var path: String = UI_BASE + name + ".png"
	if not ResourceLoader.exists(path):
		return null
	var tex: Texture2D = ResourceLoader.load(path) as Texture2D
	if tex != null:
		_cache[name] = tex
	return tex

static func make_icon_rect(name: String, size: Vector2 = Vector2(24, 24)) -> TextureRect:
	var rect := TextureRect.new()
	rect.texture = get_icon(name)
	rect.custom_minimum_size = size
	rect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	rect.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	return rect

static func make_icon_label(icon_name: String, text: String, icon_size: Vector2 = Vector2(20, 20)) -> HBoxContainer:
	var hbox := HBoxContainer.new()
	hbox.add_theme_constant_override("separation", 4)
	hbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var icon := make_icon_rect(icon_name, icon_size)
	hbox.add_child(icon)
	var label := Label.new()
	label.text = text
	label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	hbox.add_child(label)
	return hbox

static func make_button(text: String, icon_name: String = "", min_size: Vector2 = Vector2(140, 40)) -> Button:
	var btn := Button.new()
	btn.custom_minimum_size = min_size
	btn.mouse_filter = Control.MOUSE_FILTER_STOP
	if icon_name != "":
		btn.icon = get_icon(icon_name)
		btn.icon_alignment = HORIZONTAL_ALIGNMENT_LEFT
		btn.expand_icon = true
	btn.text = text
	var normal_style := StyleBoxFlat.new()
	normal_style.bg_color = Color(0.18, 0.14, 0.08, 0.95)
	normal_style.corner_radius_top_left = 8
	normal_style.corner_radius_top_right = 8
	normal_style.corner_radius_bottom_left = 8
	normal_style.corner_radius_bottom_right = 8
	normal_style.border_width_left = 2
	normal_style.border_width_right = 2
	normal_style.border_width_top = 2
	normal_style.border_width_bottom = 2
	normal_style.border_color = Color(0.55, 0.45, 0.3, 0.8)
	normal_style.content_margin_left = 8
	normal_style.content_margin_right = 8
	normal_style.content_margin_top = 4
	normal_style.content_margin_bottom = 4
	btn.add_theme_stylebox_override("normal", normal_style)
	var hover_style := normal_style.duplicate() as StyleBoxFlat
	hover_style.bg_color = Color(0.25, 0.2, 0.12, 0.95)
	hover_style.border_color = Color(0.7, 0.6, 0.35, 1.0)
	btn.add_theme_stylebox_override("hover", hover_style)
	var pressed_style := normal_style.duplicate() as StyleBoxFlat
	pressed_style.bg_color = Color(0.12, 0.1, 0.06, 0.95)
	pressed_style.border_color = Color(0.4, 0.35, 0.2, 1.0)
	btn.add_theme_stylebox_override("pressed", pressed_style)
	var disabled_style := normal_style.duplicate() as StyleBoxFlat
	disabled_style.bg_color = Color(0.1, 0.08, 0.06, 0.6)
	disabled_style.border_color = Color(0.3, 0.25, 0.15, 0.4)
	btn.add_theme_stylebox_override("disabled", disabled_style)
	return btn

static func make_panel_bg(border_color: Color = Color(0.55, 0.45, 0.3, 0.8)) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.12, 0.09, 0.06, 0.95)
	style.corner_radius_top_left = 12
	style.corner_radius_top_right = 12
	style.corner_radius_bottom_left = 12
	style.corner_radius_bottom_right = 12
	style.border_width_left = 3
	style.border_width_right = 3
	style.border_width_top = 3
	style.border_width_bottom = 3
	style.border_color = border_color
	style.shadow_color = Color(0, 0, 0, 0.3)
	style.shadow_size = 4
	style.content_margin_left = 12
	style.content_margin_right = 12
	style.content_margin_top = 12
	style.content_margin_bottom = 12
	return style
