class_name AchievementPopup extends Control

var _title_label: Label
var _desc_label: Label
var _icon_rect: TextureRect

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	_create_layout()

func _create_layout() -> void:
	var panel := PanelContainer.new()
	panel.set_anchors_preset(Control.PRESET_CENTER_TOP)
	panel.offset_left = -180
	panel.offset_top = 20
	panel.offset_right = 180
	panel.offset_bottom = 100
	panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var style: StyleBoxTexture = UITheme.make_wood_panel_bg()
	panel.add_theme_stylebox_override("panel", style)
	add_child(panel)
	var hbox := HBoxContainer.new()
	hbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	panel.add_child(hbox)
	_icon_rect = UITheme.make_icon_rect("icon_star", Vector2(32, 32))
	hbox.add_child(_icon_rect)
	var vbox := VBoxContainer.new()
	vbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	hbox.add_child(vbox)
	_title_label = Label.new()
	_title_label.text = "成就解锁！"
	_title_label.add_theme_font_size_override("font_size", 13)
	_title_label.modulate = Color(1, 0.85, 0.2)
	_title_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.add_child(_title_label)
	_desc_label = Label.new()
	_desc_label.text = ""
	_desc_label.add_theme_font_size_override("font_size", 11)
	_desc_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.add_child(_desc_label)

func show_achievement(title: String, description: String, icon_path: String = "") -> void:
	_title_label.text = "%s" % title
	_desc_label.text = description
	if icon_path != "":
		var tex: Texture2D = load(icon_path) as Texture2D
		if tex != null:
			_icon_rect.texture = tex
	visible = true
	var tween: Tween = create_tween()
	tween.tween_property(self, "modulate:a", 1.0, 0.3).from(0.0)
	tween.tween_interval(3.0)
	tween.tween_property(self, "modulate:a", 0.0, 0.5)
	tween.tween_callback(func(): visible = false)
