class_name UITheme
extends Node

static func make_dark_panel_bg() -> StyleBoxTexture:
	var style := StyleBoxTexture.new()
	var img := Image.create(64, 64, false, Image.FORMAT_RGBA8)
	var bg_color := Color(0.08, 0.1, 0.15, 0.95)
	for y in range(64):
		for x in range(64):
			img.set_pixel(x, y, bg_color)
	var tex := ImageTexture.create_from_image(img)
	style.texture = tex
	style.expand_margin_left = 16
	style.expand_margin_right = 16
	style.expand_margin_top = 16
	style.expand_margin_bottom = 16
	return style

static func make_bar_bg_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.1, 0.1, 0.15, 0.9)
	style.corner_radius_top_left = 4
	style.corner_radius_top_right = 4
	style.corner_radius_bottom_left = 4
	style.corner_radius_bottom_right = 4
	return style

static func make_card_panel_style() -> StyleBoxTexture:
	var style := StyleBoxTexture.new()
	var img := Image.create(64, 64, false, Image.FORMAT_RGBA8)
	var bg_color := Color(0.12, 0.15, 0.2, 0.9)
	for y in range(64):
		for x in range(64):
			img.set_pixel(x, y, bg_color)
	var tex := ImageTexture.create_from_image(img)
	style.texture = tex
	style.expand_margin_left = 8
	style.expand_margin_right = 8
	style.expand_margin_top = 8
	style.expand_margin_bottom = 8
	return style

static func make_button_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.2, 0.25, 0.35, 0.95)
	style.corner_radius_top_left = 6
	style.corner_radius_top_right = 6
	style.corner_radius_bottom_left = 6
	style.corner_radius_bottom_right = 6
	return style

static func make_button_hover_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.3, 0.35, 0.45, 0.95)
	style.corner_radius_top_left = 6
	style.corner_radius_top_right = 6
	style.corner_radius_bottom_left = 6
	style.corner_radius_bottom_right = 6
	return style

static func make_button_pressed_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.4, 0.45, 0.55, 0.95)
	style.corner_radius_top_left = 6
	style.corner_radius_top_right = 6
	style.corner_radius_bottom_left = 6
	style.corner_radius_bottom_right = 6
	return style
