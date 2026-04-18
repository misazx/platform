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

static func _make_stylebox(tex_name: String, margin_left: int = 10, margin_right: int = 10, margin_top: int = 10, margin_bottom: int = 10, content_left: int = 8, content_right: int = 8, content_top: int = 6, content_bottom: int = 6) -> StyleBoxTexture:
	var style := StyleBoxTexture.new()
	var tex: Texture2D = get_icon(tex_name)
	if tex != null:
		style.texture = tex
	style.texture_margin_left = margin_left
	style.texture_margin_right = margin_right
	style.texture_margin_top = margin_top
	style.texture_margin_bottom = margin_bottom
	style.content_margin_left = content_left
	style.content_margin_right = content_right
	style.content_margin_top = content_top
	style.content_margin_bottom = content_bottom
	style.axis_stretch_horizontal = StyleBoxTexture.AXIS_STRETCH_MODE_TILE
	style.axis_stretch_vertical = StyleBoxTexture.AXIS_STRETCH_MODE_TILE
	return style

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
	var normal_style: StyleBoxTexture = _make_stylebox("btn_wide_normal", 10, 10, 8, 8, 8, 8, 4, 4)
	btn.add_theme_stylebox_override("normal", normal_style)
	var hover_style: StyleBoxTexture = _make_stylebox("btn_wide_hover", 10, 10, 8, 8, 8, 8, 4, 4)
	btn.add_theme_stylebox_override("hover", hover_style)
	var pressed_style: StyleBoxTexture = _make_stylebox("btn_wide_pressed", 10, 10, 8, 8, 8, 8, 4, 4)
	btn.add_theme_stylebox_override("pressed", pressed_style)
	var disabled_style: StyleBoxTexture = _make_stylebox("btn_wide_disabled", 10, 10, 8, 8, 8, 8, 4, 4)
	btn.add_theme_stylebox_override("disabled", disabled_style)
	btn.add_theme_color_override("font_color", Color(0.95, 0.9, 0.8))
	btn.add_theme_color_override("font_hover_color", Color(1, 1, 1))
	btn.add_theme_color_override("font_pressed_color", Color(0.8, 0.75, 0.65))
	btn.add_theme_color_override("font_disabled_color", Color(0.5, 0.45, 0.4, 0.6))
	btn.add_theme_font_size_override("font_size", 14)
	return btn

static func make_small_button(text: String, icon_name: String = "", min_size: Vector2 = Vector2(80, 32)) -> Button:
	var btn := Button.new()
	btn.custom_minimum_size = min_size
	btn.mouse_filter = Control.MOUSE_FILTER_STOP
	if icon_name != "":
		btn.icon = get_icon(icon_name)
		btn.icon_alignment = HORIZONTAL_ALIGNMENT_LEFT
		btn.expand_icon = true
	btn.text = text
	var normal_style: StyleBoxTexture = _make_stylebox("btn_sq26_s1_normal", 6, 6, 6, 6, 4, 4, 4, 4)
	btn.add_theme_stylebox_override("normal", normal_style)
	var hover_style: StyleBoxTexture = _make_stylebox("btn_sq26_s1_hover", 6, 6, 6, 6, 4, 4, 4, 4)
	btn.add_theme_stylebox_override("hover", hover_style)
	var pressed_style: StyleBoxTexture = _make_stylebox("btn_sq26_s1_pressed", 6, 6, 6, 6, 4, 4, 4, 4)
	btn.add_theme_stylebox_override("pressed", pressed_style)
	var disabled_style: StyleBoxTexture = _make_stylebox("btn_sq26_s1_disabled", 6, 6, 6, 6, 4, 4, 4, 4)
	btn.add_theme_stylebox_override("disabled", disabled_style)
	btn.add_theme_color_override("font_color", Color(0.95, 0.9, 0.8))
	btn.add_theme_color_override("font_hover_color", Color(1, 1, 1))
	btn.add_theme_color_override("font_pressed_color", Color(0.8, 0.75, 0.65))
	btn.add_theme_font_size_override("font_size", 12)
	return btn

static func make_panel_bg(border_color: Color = Color(0.55, 0.45, 0.3, 0.8)) -> StyleBoxTexture:
	var style: StyleBoxTexture = _make_stylebox("panel_light", 24, 24, 24, 24, 20, 20, 18, 18)
	return style

static func make_dark_panel_bg() -> StyleBoxTexture:
	var style: StyleBoxTexture = _make_stylebox("dialog_box_big", 10, 10, 10, 10, 8, 8, 8, 8)
	return style

static func make_wood_panel_bg() -> StyleBoxTexture:
	var style: StyleBoxTexture = _make_stylebox("panel_wood", 8, 8, 8, 8, 6, 6, 6, 6)
	return style

static func make_medium_panel_bg() -> StyleBoxTexture:
	var style: StyleBoxTexture = _make_stylebox("panel_medium", 6, 6, 6, 6, 4, 4, 4, 4)
	return style

static func make_bar_bg_style() -> StyleBoxTexture:
	var style: StyleBoxTexture = _make_stylebox("dialog_box_big", 10, 10, 10, 10, 2, 2, 2, 2)
	return style

static func make_card_panel_style() -> StyleBoxTexture:
	var style: StyleBoxTexture = _make_stylebox("dialog_box_small", 10, 10, 10, 10, 6, 6, 6, 6)
	return style

static func make_icon_button(icon_name: String, min_size: Vector2 = Vector2(36, 36)) -> Button:
	var btn := Button.new()
	btn.custom_minimum_size = min_size
	btn.mouse_filter = Control.MOUSE_FILTER_STOP
	btn.icon = get_icon(icon_name)
	btn.expand_icon = true
	var normal_style: StyleBoxTexture = _make_stylebox("btn_sq26_s2_normal", 6, 6, 6, 6, 4, 4, 4, 4)
	btn.add_theme_stylebox_override("normal", normal_style)
	var hover_style: StyleBoxTexture = _make_stylebox("btn_sq26_s2_hover", 6, 6, 6, 6, 4, 4, 4, 4)
	btn.add_theme_stylebox_override("hover", hover_style)
	var pressed_style: StyleBoxTexture = _make_stylebox("btn_sq26_s2_pressed", 6, 6, 6, 6, 4, 4, 4, 4)
	btn.add_theme_stylebox_override("pressed", pressed_style)
	var disabled_style: StyleBoxTexture = _make_stylebox("btn_sq26_s2_disabled", 6, 6, 6, 6, 4, 4, 4, 4)
	btn.add_theme_stylebox_override("disabled", disabled_style)
	return btn
