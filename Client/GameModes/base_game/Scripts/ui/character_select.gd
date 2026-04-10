class_name CharacterSelect extends Control

signal character_selected(character_id: String)

var _character_grid: GridContainer
var _name_label: Label
var _class_label: Label
var _desc_label: RichTextLabel
var _confirm_button: Button
var _back_button: Button

var _selected_index: int = -1
var _characters: Array = []
var _character_cards: Array = []

func _ready() -> void:
	_setup_node_references()
	_setup_signals()
	load_characters()
	GD.print("[CharacterSelect] Ready")

func _setup_node_references() -> void:
	_character_grid = get_node_or_null("CharGrid")
	_name_label = get_node_or_null("DescriptionPanel/DescVBox/NameLabel")
	_class_label = get_node_or_null("DescriptionPanel/DescVBox/ClassLabel")
	_desc_label = get_node_or_null("DescriptionPanel/DescVBox/DescLabel")
	_confirm_button = get_node_or_null("BottomBar/ConfirmButton")
	_back_button = get_node_or_null("HeaderBar/BackButton")

	if _character_grid == null:
		GD.push_error("[CharacterSelect] CharGrid not found")
	if _desc_label == null:
		GD.push_error("[CharacterSelect] DescLabel not found")

func _setup_signals() -> void:
	if _confirm_button != null:
		_confirm_button.pressed.connect(_on_confirm_pressed)
		_confirm_button.disabled = true
	if _back_button != null:
		_back_button.pressed.connect(_on_back_pressed)

func load_characters() -> void:
	var db = CharacterDatabase
	if db == null:
		GD.push_error("[CharacterSelect] CharacterDatabase instance is null!")
		return

	_characters = StsExpansionSystem.get_all_characters()
	GD.print("[CharacterSelect] Loaded %d characters" % _characters.size())

	for child in _character_grid.get_children():
		child.queue_free()
	_character_cards.clear()

	for i in range(_characters.size()):
		var character = _characters[i]
		var card := CharacterCardControl.new(i, character)
		card.card_clicked.connect(_on_card_clicked)
		_character_cards.append(card)
		_character_grid.add_child(card)

	update_description(-1)

func _on_card_clicked(index: int) -> void:
	GD.print("[CharacterSelect] Card clicked: %d" % index)
	select_character(index)

func select_character(index: int) -> void:
	_selected_index = index

	if _confirm_button != null:
		_confirm_button.disabled = false

	for card in _character_cards:
		card.set_selected(card.index == index)

	update_description(index)
	if AudioManager.instance != null:
		AudioManager.instance.play_button_click()

func update_description(index: int) -> void:
	if index < 0 or index >= _characters.size():
		if _name_label != null: _name_label.text = "选择一个角色"
		if _class_label != null: _class_label.text = ""
		if _desc_label != null: _desc_label.text = "点击上方角色卡片查看详情"
		return

	var character = _characters[index]

	if _name_label != null:
		_name_label.text = character.name
	if _class_label != null:
		_class_label.text = _get_class_display_name(character.character)
	if _desc_label != null:
		_desc_label.text = "%s\n\n%s\n\n难度: 简单" % [character.name, character.description]

func _on_confirm_pressed() -> void:
	if _selected_index >= 0 and _selected_index < _characters.size():
		var character_id: String = _characters[_selected_index].id if _characters[_selected_id].has("id") else str(_selected_index)
		GD.print("[CharacterSelect] Confirming character: %s" % character_id)
		if AudioManager.instance != null:
			AudioManager.instance.play_button_click()
		character_selected.emit(character_id)

func _on_back_pressed() -> void:
	GD.print("[CharacterSelect] Back pressed")
	if AudioManager.instance != null:
		AudioManager.instance.play_button_click()
	if Main.instance != null:
		Main.instance.go_to_main_menu()

func _get_class_display_name(char_class: int) -> String:
	match char_class:
		StsExpansionSystem.Character.IRONCLAD: return "战士"
		StsExpansionSystem.Character.SILENT: return "猎人"
		StsExpansionSystem.Character.DEFECT: return "机器人"
		StsExpansionSystem.Character.WATCHER: return "观者"
		_: return "未知"


class_name CharacterCardControl extends PanelContainer

signal card_clicked(index: int)

var index: int
var data: Dictionary
var _selected: bool = false
var _portrait_rect: TextureRect
var _name_lbl: Label

func _init(p_index: int, p_data: Dictionary) -> void:
	index = p_index
	data = p_data

func _ready() -> void:
	custom_minimum_size = Vector2(150, 180)
	mouse_filter = Control.MOUSE_FILTER_STOP

	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.12, 0.1, 0.08)
	style.content_margin_left = 10
	style.content_margin_right = 10
	style.content_margin_top = 10
	style.content_margin_bottom = 10
	style.corner_radius_top_left = 8
	style.corner_radius_top_right = 8
	style.corner_radius_bottom_left = 8
	style.corner_radius_bottom_right = 8
	style.border_width_left = 2
	style.border_width_right = 2
	style.border_width_top = 2
	style.border_width_bottom = 2
	style.border_color = data.get("color", Color.WHITE)
	add_theme_stylebox_override("panel", style)

	var vbox := VBoxContainer.new()
	add_child(vbox)

	_portrait_rect = TextureRect.new()
	_portrait_rect.custom_minimum_size = Vector2(130, 110)
	_portrait_rect.expand_mode = TextureRect.EXPAND_FIT_WIDTH_PROPORTIONAL
	_portrait_rect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	_portrait_rect.modulate = Color(0.5, 0.5, 0.5)
	
	var portrait_path: String = data.get("portrait_path", "")
	if portrait_path != "" and ResourceLoader.exists(portrait_path):
		_portrait_rect.texture = load(portrait_path)
		_portrait_rect.modulate = Color.WHITE
	vbox.add_child(_portrait_rect)

	_name_lbl = Label.new()
	_name_lbl.text = data.get("name", "")
	_name_lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_name_lbl.modulate = Color.WHITE
	vbox.add_child(_name_lbl)

	gui_input.connect(_on_gui_input)
	mouse_entered.connect(_on_mouse_entered)
	mouse_exited.connect(_on_mouse_exited)

func set_selected(selected: bool) -> void:
	_selected = selected
	
	var style = get_theme_stylebox("panel") as StyleBoxFlat
	if style != null:
		var border_width := 4 if selected else 2
		style.border_width_left = border_width
		style.border_width_right = border_width
		style.border_width_top = border_width
		style.border_width_bottom = border_width
		style.border_color = Color.GOLD if selected else data.get("color", Color.WHITE)

	if selected:
		create_tween().tween_property(self, "scale", Vector2(1.05, 1.05), 0.1)
	else:
		create_tween().tween_property(self, "scale", Vector2.ONE, 0.1)

func _on_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		card_clicked.emit(index)

func _on_mouse_entered() -> void:
	if not _selected:
		create_tween().tween_property(self, "scale", Vector2(1.02, 1.02), 0.1)

func _on_mouse_exited() -> void:
	if not _selected:
		create_tween().tween_property(self, "scale", Vector2.ONE, 0.1)
