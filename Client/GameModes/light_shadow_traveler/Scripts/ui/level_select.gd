class_name LevelSelectScreen
extends Control

signal level_selected(level_id: String)
signal back_pressed

var _level_manager: LevelManager
var _chapter_container: VBoxContainer
var _scroll_container: ScrollContainer

func _ready() -> void:
	_build_ui()

func _build_ui() -> void:
	anchors_preset = Control.PRESET_FULL_RECT
	var bg := ColorRect.new()
	bg.color = Color(0.04, 0.06, 0.1, 1.0)
	bg.anchors_preset = Control.PRESET_FULL_RECT
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	var title := Label.new()
	title.text = "光影旅者 - 关卡选择"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 32)
	title.add_theme_color_override("font_color", Color(0.9, 0.85, 1.0))
	title.anchors_preset = Control.PRESET_CENTER_TOP
	title.offset_top = 30
	title.offset_left = -200
	title.offset_right = 200
	title.offset_bottom = 70
	add_child(title)
	var back_btn := Button.new()
	back_btn.text = "← 返回"
	back_btn.position = Vector2(20, 20)
	back_btn.custom_minimum_size = Vector2(100, 40)
	back_btn.pressed.connect(func(): back_pressed.emit())
	add_child(back_btn)
	_scroll_container = ScrollContainer.new()
	_scroll_container.anchors_preset = Control.PRESET_FULL_RECT
	_scroll_container.offset_top = 80
	_scroll_container.offset_bottom = -20
	_scroll_container.offset_left = 40
	_scroll_container.offset_right = -40
	add_child(_scroll_container)
	_chapter_container = VBoxContainer.new()
	_chapter_container.add_theme_constant_override("separation", 20)
	_scroll_container.add_child(_chapter_container)

func populate_levels(lm: LevelManager) -> void:
	_level_manager = lm
	for child in _chapter_container.get_children():
		child.queue_free()
	var chapters := lm.get_chapters()
	for chapter in chapters:
		var chapter_dict := chapter as Dictionary
		var chapter_id: String = chapter_dict.get("id", "")
		var chapter_name: String = chapter_dict.get("name", "")
		var chapter_panel := PanelContainer.new()
		var style: StyleBoxTexture = UITheme.make_dark_panel_bg()
		var bg_color := Color(chapter_dict.get("bgColor", "#2D5A27"))
		chapter_panel.add_theme_stylebox_override("panel", style)
		chapter_panel.self_modulate = bg_color.lightened(0.3)
		_chapter_container.add_child(chapter_panel)
		var chapter_vbox := VBoxContainer.new()
		chapter_vbox.add_theme_constant_override("separation", 8)
		chapter_panel.add_child(chapter_vbox)
		var header := Label.new()
		header.text = chapter_name + " - " + chapter_dict.get("description", "")
		header.add_theme_font_size_override("font_size", 20)
		header.add_theme_color_override("font_color", bg_color.lightened(0.5))
		chapter_vbox.add_child(header)
		var grid := GridContainer.new()
		grid.columns = 5
		grid.add_theme_constant_override("h_separation", 10)
		grid.add_theme_constant_override("v_separation", 10)
		chapter_vbox.add_child(grid)
		var levels := lm.get_chapter_levels(chapter_id)
		for level_data in levels:
			var level_id: String = level_data.get("id", "")
			var level_name: String = level_data.get("name", "")
			var btn := Button.new()
			btn.text = level_name
			btn.custom_minimum_size = Vector2(100, 50)
			if lm.is_level_completed(level_id):
				btn.add_theme_color_override("font_color", Color(0.5, 1.0, 0.5))
			var lid := level_id
			btn.pressed.connect(func(): level_selected.emit(lid))
			grid.add_child(btn)
