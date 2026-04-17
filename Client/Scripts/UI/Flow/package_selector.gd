class_name PackageSelector extends Control

signal package_selected(package_id: String)
signal back_pressed()

var _bg_overlay: ColorRect
var _main_container: VBoxContainer
var _package_grid: GridContainer
var _package_cards: Array = []
var _back_button: Button
var _category_bar: HBoxContainer
var _current_category: String = "all"

func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)
	mouse_filter = Control.MOUSE_FILTER_STOP
	_build_ui()
	_load_packages()

func _build_ui() -> void:
	_bg_overlay = ColorRect.new()
	_bg_overlay.set_anchors_preset(Control.PRESET_FULL_RECT)
	_bg_overlay.color = Color(0, 0, 0, 0.85)
	_bg_overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_bg_overlay)

	var center := CenterContainer.new()
	center.set_anchors_preset(Control.PRESET_FULL_RECT)
	center.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(center)

	_main_container = VBoxContainer.new()
	_main_container.custom_minimum_size = Vector2(1000, 650)
	_main_container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_main_container.alignment = BoxContainer.ALIGNMENT_CENTER
	_main_container.add_theme_constant_override("separation", 16)
	center.add_child(_main_container)

	var header := HBoxContainer.new()
	header.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_main_container.add_child(header)

	_back_button = Button.new()
	_back_button.text = "← 返回大厅"
	_back_button.custom_minimum_size = Vector2(140, 40)
	_back_button.mouse_filter = Control.MOUSE_FILTER_STOP
	_back_button.pressed.connect(_on_back_pressed)
	header.add_child(_back_button)

	var spacer := Control.new()
	spacer.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	spacer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	header.add_child(spacer)

	var title := Label.new()
	title.text = "🎮 选择玩法"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	title.add_theme_font_size_override("font_size", 28)
	title.modulate = Color(1, 0.9, 0.6)
	header.add_child(title)

	var spacer2 := Control.new()
	spacer2.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	spacer2.mouse_filter = Control.MOUSE_FILTER_IGNORE
	header.add_child(spacer2)

	var placeholder := Control.new()
	placeholder.custom_minimum_size = Vector2(140, 40)
	placeholder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	header.add_child(placeholder)

	_category_bar = HBoxContainer.new()
	_category_bar.alignment = BoxContainer.ALIGNMENT_CENTER
	_category_bar.add_theme_constant_override("separation", 8)
	_category_bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_main_container.add_child(_category_bar)

	_add_category_button("全部", "all")
	_add_category_button("🎮 官方", "official")
	_add_category_button("👥 社区", "community")
	_add_category_button("💎 DLC", "dlc")

	var scroll := ScrollContainer.new()
	scroll.custom_minimum_size = Vector2(960, 460)
	scroll.mouse_filter = Control.MOUSE_FILTER_STOP
	scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
	_main_container.add_child(scroll)

	_package_grid = GridContainer.new()
	_package_grid.columns = 3
	_package_grid.add_theme_constant_override("h_separation", 16)
	_package_grid.add_theme_constant_override("v_separation", 16)
	_package_grid.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_package_grid.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	scroll.add_child(_package_grid)

func _add_category_button(text: String, category_id: String) -> void:
	var btn := Button.new()
	btn.text = text
	btn.custom_minimum_size = Vector2(100, 36)
	btn.mouse_filter = Control.MOUSE_FILTER_STOP
	btn.toggle_mode = true
	btn.button_pressed = (category_id == "all")

	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.12, 0.1, 0.15, 0.9)
	style.corner_radius_top_left = 8
	style.corner_radius_top_right = 8
	style.corner_radius_bottom_left = 8
	style.corner_radius_bottom_right = 8
	style.border_width_left = 1
	style.border_width_right = 1
	style.border_width_top = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.3, 0.3, 0.4, 0.6)
	btn.add_theme_stylebox_override("normal", style)

	var pressed_style := style.duplicate() as StyleBoxFlat
	pressed_style.bg_color = Color(0.2, 0.18, 0.28, 0.95)
	pressed_style.border_color = Color(0.6, 0.5, 0.9, 0.9)
	btn.add_theme_stylebox_override("pressed", pressed_style)

	btn.pressed.connect(func(): _on_category_selected(category_id))
	_category_bar.add_child(btn)

func _on_category_selected(category_id: String) -> void:
	_current_category = category_id
	_refresh_grid()

func _load_packages() -> void:
	_refresh_grid()

func _refresh_grid() -> void:
	for child in _package_grid.get_children():
		child.queue_free()
	_package_cards.clear()

	var packages: Array = []
	var svc := get_node_or_null("/root/PackageService")
	if svc != null:
		if _current_category == "all":
			packages = svc.get_all_packages()
		else:
			packages = svc.get_packages_by_category(_current_category)
	else:
		var registry := _get_package_registry()
		packages = registry.get("packages", [])

	for pkg in packages:
		if _current_category != "all":
			var tags: Array = pkg.get("tags", [])
			if not _current_category in tags:
				continue

		var card := _create_package_card(pkg)
		_package_grid.add_child(card)
		_package_cards.append(card)

	if packages.is_empty():
		var empty_label := Label.new()
		empty_label.text = "暂无可用玩法包"
		empty_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		empty_label.modulate = Color.GRAY
		empty_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		empty_label.add_theme_font_size_override("font_size", 18)
		_package_grid.add_child(empty_label)

func _create_package_card(pkg: Dictionary) -> PanelContainer:
	var card := PanelContainer.new()
	card.custom_minimum_size = Vector2(290, 200)
	card.mouse_filter = Control.MOUSE_FILTER_STOP

	var is_installed: bool = pkg.get("isFree", true)
	var accent := Color(0.2, 0.6, 0.3) if is_installed else Color(0.6, 0.4, 0.2)

	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.08, 0.06, 0.1, 0.95)
	style.corner_radius_top_left = 12
	style.corner_radius_top_right = 12
	style.corner_radius_bottom_left = 12
	style.corner_radius_bottom_right = 12
	style.border_width_left = 2
	style.border_width_right = 2
	style.border_width_top = 2
	style.border_width_bottom = 2
	style.border_color = accent
	style.content_margin_top = 12
	style.content_margin_bottom = 12
	style.content_margin_left = 14
	style.content_margin_right = 14
	card.add_theme_stylebox_override("panel", style)

	var vbox := VBoxContainer.new()
	vbox.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.add_theme_constant_override("separation", 6)
	card.add_child(vbox)

	var header_row := HBoxContainer.new()
	header_row.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.add_child(header_row)

	var name_label := Label.new()
	name_label.text = pkg.get("name", "未知")
	name_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	name_label.add_theme_font_size_override("font_size", 18)
	name_label.modulate = Color(0.95, 0.9, 0.8)
	header_row.add_child(name_label)

	var version_label := Label.new()
	version_label.text = "v%s" % pkg.get("version", "1.0")
	version_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	version_label.add_theme_font_size_override("font_size", 11)
	version_label.modulate = Color(0.5, 0.5, 0.5)
	header_row.add_child(version_label)

	var desc_label := Label.new()
	desc_label.text = pkg.get("description", "")
	desc_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	desc_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	desc_label.custom_minimum_size = Vector2(250, 60)
	desc_label.add_theme_font_size_override("font_size", 11)
	desc_label.modulate = Color(0.7, 0.7, 0.7)
	vbox.add_child(desc_label)

	var tags_row := HBoxContainer.new()
	tags_row.mouse_filter = Control.MOUSE_FILTER_IGNORE
	tags_row.add_theme_constant_override("separation", 4)
	vbox.add_child(tags_row)

	var tags: Array = pkg.get("tags", [])
	for tag in tags:
		if tag in ["official", "community", "dlc", "expansion"]:
			continue
		var tag_label := Label.new()
		tag_label.text = " %s " % tag
		tag_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		tag_label.add_theme_font_size_override("font_size", 9)
		tag_label.modulate = Color(0.6, 0.6, 0.8)
		tags_row.add_child(tag_label)

	var bottom_row := HBoxContainer.new()
	bottom_row.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.add_child(bottom_row)

	var rating_label := Label.new()
	rating_label.text = "⭐ %.1f" % pkg.get("rating", 0)
	rating_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	rating_label.add_theme_font_size_override("font_size", 12)
	rating_label.modulate = Color(1, 0.85, 0.3)
	bottom_row.add_child(rating_label)

	var spacer3 := Control.new()
	spacer3.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	spacer3.mouse_filter = Control.MOUSE_FILTER_IGNORE
	bottom_row.add_child(spacer3)

	var status_label := Label.new()
	status_label.text = "✅ 已安装" if is_installed else "💰 ¥%.0f" % pkg.get("price", 0)
	status_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	status_label.add_theme_font_size_override("font_size", 12)
	status_label.modulate = Color(0.3, 0.8, 0.4) if is_installed else Color(1, 0.7, 0.3)
	bottom_row.add_child(status_label)

	var pkg_id: String = pkg.get("id", "")
	card.gui_input.connect(func(event: InputEvent):
		if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
			package_selected.emit(pkg_id)
	)

	card.mouse_entered.connect(func():
		var hover_style := style.duplicate() as StyleBoxFlat
		hover_style.bg_color = Color(0.12, 0.1, 0.15, 0.98)
		hover_style.border_color = accent.lightened(0.3)
		card.add_theme_stylebox_override("panel", hover_style)
	)

	card.mouse_exited.connect(func():
		card.add_theme_stylebox_override("panel", style)
	)

	return card

func _get_package_registry() -> Dictionary:
	var config_path := "res://Config/Data/package_registry.json"
	if not ResourceLoader.exists(config_path):
		return {"packages": []}
	var file := FileAccess.open(config_path, FileAccess.READ)
	if file == null:
		return {"packages": []}
	var json := JSON.new()
	var err := json.parse(file.get_as_text())
	if err != OK:
		return {"packages": []}
	return json.data if json.data is Dictionary else {"packages": []}

func _on_back_pressed() -> void:
	print("[PackageSelector] Back pressed")
	back_pressed.emit()
