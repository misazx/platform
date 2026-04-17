@tool
extends EditorPlugin

var _dock: Control
var _package_id_edit: LineEdit
var _package_name_edit: LineEdit
var _create_btn: Button
var _validate_btn: Button
var _build_btn: Button
var _log_label: RichTextLabel

func _enter_tree() -> void:
	_dock = _create_dock_ui()
	add_control_to_dock(DOCK_SLOT_LEFT_UL, _dock)
	print("[PackageDevTools] Plugin entered tree")

func _exit_tree() -> void:
	if _dock != null:
		remove_control_from_docks(_dock)
		_dock.queue_free()
	print("[PackageDevTools] Plugin exited tree")

func _create_dock_ui() -> Control:
	var vbox := VBoxContainer.new()
	vbox.custom_minimum_size = Vector2(280, 400)

	var title := Label.new()
	title.text = "📦 包开发工具"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 16)
	vbox.add_child(title)

	var sep := HSeparator.new()
	vbox.add_child(sep)

	var form := GridContainer.new()
	form.columns = 2
	form.add_theme_constant_override("h_separation", 8)
	form.add_theme_constant_override("v_separation", 6)
	vbox.add_child(form)

	var id_label := Label.new()
	id_label.text = "包ID:"
	form.add_child(id_label)

	_package_id_edit = LineEdit.new()
	_package_id_edit.placeholder_text = "my_package"
	_package_id_edit.custom_minimum_size = Vector2(160, 0)
	form.add_child(_package_id_edit)

	var name_label := Label.new()
	name_label.text = "包名:"
	form.add_child(name_label)

	_package_name_edit = LineEdit.new()
	_package_name_edit.placeholder_text = "我的玩法包"
	form.add_child(_package_name_edit)

	var btn_vbox := VBoxContainer.new()
	btn_vbox.add_theme_constant_override("separation", 6)
	vbox.add_child(btn_vbox)

	_create_btn = Button.new()
	_create_btn.text = "🔨 创建包脚手架"
	_create_btn.pressed.connect(_on_create_package)
	btn_vbox.add_child(_create_btn)

	_validate_btn = Button.new()
	_validate_btn.text = "✅ 验证包结构"
	_validate_btn.pressed.connect(_on_validate_package)
	btn_vbox.add_child(_validate_btn)

	_build_btn = Button.new()
	_build_btn.text = "📦 打包导出"
	_build_btn.pressed.connect(_on_build_package)
	btn_vbox.add_child(_build_btn)

	_log_label = RichTextLabel.new()
	_log_label.custom_minimum_size = Vector2(260, 200)
	_log_label.bbcode_enabled = true
	_log_label.size_flags_vertical = Control.SIZE_EXPAND_FILL
	vbox.add_child(_log_label)

	return vbox

func _log(text: String, color: Color = Color.WHITE) -> void:
	_log_label.append_text("[color=#%s]%s[/color]\n" % [color.to_html(), text])

func _on_create_package() -> void:
	var pkg_id: String = _package_id_edit.text.strip_edges()
	var pkg_name: String = _package_name_edit.text.strip_edges()

	if pkg_id == "":
		_log("❌ 包ID不能为空", Color.RED)
		return

	if not pkg_id.is_valid_identifier():
		_log("❌ 包ID只能包含字母、数字和下划线", Color.RED)
		return

	var base_path := "res://GameModes/%s" % pkg_id
	if DirAccess.dir_exists_absolute(base_path):
		_log("❌ 包目录已存在: %s" % base_path, Color.RED)
		return

	_create_scaffold(pkg_id, pkg_name)
	_log("✅ 包脚手架创建成功: %s" % pkg_id, Color.GREEN)

func _create_scaffold(pkg_id: String, pkg_name: String) -> void:
	var base := "res://GameModes/%s" % pkg_id

	var dirs := [
		"%s/Scripts" % base,
		"%s/Scenes" % base,
		"%s/Config/Data" % base,
		"%s/Resources/Audio/BGM" % base,
		"%s/Resources/Audio/SFX" % base,
		"%s/Resources/Icons" % base,
		"%s/Resources/Images" % base,
	]

	for dir in dirs:
		DirAccess.make_dir_recursive_absolute(dir)

	var config := {
		"id": pkg_id,
		"name": pkg_name,
		"description": "",
		"version": "0.1.0",
		"type": "expansion",
		"author": "",
		"entryScene": "res://GameModes/%s/Scenes/Main.tscn" % pkg_id,
		"characterSelectScene": "",
		"mapScene": "",
		"configFile": "res://GameModes/%s/Config/Data/package_config.json" % pkg_id,
		"requiredBaseVersion": "1.0.0",
		"dependencies": [],
		"tags": [],
		"features": [],
		"isFree": true,
		"price": 0,
		"rating": 0.0,
		"downloadCount": 0,
		"includeInBuild": false,
	}

	var config_path := "%s/Config/Data/package_config.json" % base
	var file := FileAccess.open(config_path, FileAccess.WRITE)
	if file != null:
		file.store_string(JSON.stringify(config, "\t"))

	var entry_script := "extends Node2D\n\nfunc _ready() -> void:\n\tpass\n"
	var script_path := "%s/Scripts/main.gd" % base
	file = FileAccess.open(script_path, FileAccess.WRITE)
	if file != null:
		file.store_string(entry_script)

	_update_registry(pkg_id, pkg_name, config)

func _update_registry(pkg_id: String, pkg_name: String, config: Dictionary) -> void:
	var registry_path := "res://Config/Data/package_registry.json"
	var registry := {}

	if FileAccess.file_exists(registry_path):
		var file := FileAccess.open(registry_path, FileAccess.READ)
		if file != null:
			var json := JSON.new()
			if json.parse(file.get_as_text()) == OK:
				registry = json.data if json.data is Dictionary else {}

	if not registry.has("packages"):
		registry["packages"] = []

	var packages: Array = registry["packages"]
	packages.append(config)

	var file := FileAccess.open(registry_path, FileAccess.WRITE)
	if file != null:
		file.store_string(JSON.stringify(registry, "\t"))

	_log("📝 已更新 package_registry.json", Color.CYAN)

func _on_validate_package() -> void:
	var pkg_id: String = _package_id_edit.text.strip_edges()
	if pkg_id == "":
		_log("❌ 请输入包ID", Color.RED)
		return

	var base_path := "res://GameModes/%s" % pkg_id
	var errors := 0
	var warnings := 0

	if not DirAccess.dir_exists_absolute(base_path):
		_log("❌ 包目录不存在: %s" % base_path, Color.RED)
		return

	_log("🔍 验证包: %s" % pkg_id, Color.CYAN)

	var required_dirs := ["Scripts", "Scenes", "Config/Data"]
	for dir in required_dirs:
		if not DirAccess.dir_exists_absolute("%s/%s" % [base_path, dir]):
			_log("  ❌ 缺少目录: %s" % dir, Color.RED)
			errors += 1
		else:
			_log("  ✅ 目录存在: %s" % dir, Color.GREEN)

	var config_path := "%s/Config/Data/package_config.json" % base_path
	if not FileAccess.file_exists(config_path):
		_log("  ❌ 缺少包配置文件", Color.RED)
		errors += 1
	else:
		var file := FileAccess.open(config_path, FileAccess.READ)
		if file != null:
			var json := JSON.new()
			if json.parse(file.get_as_text()) == OK and json.data is Dictionary:
				var data := json.data as Dictionary
				for key in ["id", "name", "version", "entryScene"]:
					if not data.has(key) or str(data[key]) == "":
						_log("  ⚠️ 配置缺少字段: %s" % key, Color.YELLOW)
						warnings += 1
				_log("  ✅ 包配置文件有效", Color.GREEN)
			else:
				_log("  ❌ 包配置JSON解析失败", Color.RED)
				errors += 1

	if errors == 0:
		_log("✅ 验证通过 (0错误, %d警告)" % warnings, Color.GREEN)
	else:
		_log("❌ 验证失败 (%d错误, %d警告)" % [errors, warnings], Color.RED)

func _on_build_package() -> void:
	var pkg_id: String = _package_id_edit.text.strip_edges()
	if pkg_id == "":
		_log("❌ 请输入包ID", Color.RED)
		return

	_log("📦 打包包: %s" % pkg_id, Color.CYAN)

	var base_path := "res://GameModes/%s" % pkg_id
	if not DirAccess.dir_exists_absolute(base_path):
		_log("❌ 包目录不存在", Color.RED)
		return

	var output_path := "user://exports/%s.zip" % pkg_id
	DirAccess.make_dir_recursive_absolute("user://exports")

	_log("  📁 源目录: %s" % base_path, Color.WHITE)
	_log("  📦 输出: %s" % output_path, Color.WHITE)
	_log("  ✅ 打包完成（模拟）", Color.GREEN)
	_log("  💡 实际打包需要配合CI/CD流程", Color.YELLOW)
