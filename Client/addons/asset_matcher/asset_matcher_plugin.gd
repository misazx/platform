@tool
extends EditorPlugin

var button: Button
var process_id: int = -1
var progress_dialog: AcceptDialog
var progress_label: Label
var is_running: bool = false
var log_file_path: String = ""
var check_timer: Timer
var elapsed_time: float = 0.0

func _enter_tree():
	button = Button.new()
	button.text = "Download Assets"
	button.tooltip_text = "AI Smart Download Kenney Free Assets"
	button.custom_minimum_size = Vector2(120, 30)
	button.pressed.connect(_on_button_pressed)
	add_control_to_container(CONTAINER_TOOLBAR, button)
	
	check_timer = Timer.new()
	check_timer.wait_time = 2.0
	check_timer.one_shot = false
	check_timer.timeout.connect(_check_process)
	add_child(check_timer)

func _exit_tree():
	if check_timer:
		check_timer.stop()
		check_timer.queue_free()
	if button:
		remove_control_from_container(CONTAINER_TOOLBAR, button)
		button.queue_free()
	_close_progress()

func _find_python3() -> String:
	var paths = [
		"/Library/Frameworks/Python.framework/Versions/3.14/bin/python3",
		"/Library/Frameworks/Python.framework/Versions/3.13/bin/python3",
		"/Library/Frameworks/Python.framework/Versions/3.12/bin/python3",
		"/Library/Frameworks/Python.framework/Versions/3.11/bin/python3",
		"/Library/Frameworks/Python.framework/Versions/3.10/bin/python3",
		"/opt/homebrew/bin/python3",
		"/usr/local/bin/python3",
		"/usr/bin/python3",
	]
	for p in paths:
		if FileAccess.file_exists(p):
			return p
	return "python3"

func _on_button_pressed():
	if is_running:
		_show_message("Download in progress, please wait...")
		return
	
	var script_path = ProjectSettings.globalize_path("res://ai_asset_matcher.py")
	if not FileAccess.file_exists(script_path):
		_show_message("Script not found:\n" + script_path + "\n\nPlease make sure ai_asset_matcher.py is in the project root.")
		return
	
	log_file_path = ProjectSettings.globalize_path("res://.asset_download_log.txt")
	
	var output_file = FileAccess.open(log_file_path, FileAccess.WRITE)
	if output_file:
		output_file.close()
	
	is_running = true
	elapsed_time = 0.0
	button.text = "Downloading..."
	button.disabled = true
	
	var python_path = _find_python3()
	print("[AssetMatcher] Python: " + python_path)
	print("[AssetMatcher] Script: " + script_path)
	
	process_id = OS.create_process(python_path, [script_path, "--download"])
	print("[AssetMatcher] Process ID: " + str(process_id))
	
	if process_id == -1:
		_close_progress()
		_show_message("Failed to start script.\n\nPython path: " + python_path + "\nScript: " + script_path + "\n\nPlease run manually in terminal:\ncd " + ProjectSettings.globalize_path("res://") + "\npython3 ai_asset_matcher.py --download")
		_reset_button()
		return
	
	_show_progress("Starting download...")
	check_timer.start()

func _check_process():
	if process_id == -1:
		return
	
	elapsed_time += check_timer.wait_time
	
	var running = OS.is_process_running(process_id)
	
	if not running:
		check_timer.stop()
		_close_progress()
		
		var log_content = ""
		if log_file_path != "" and FileAccess.file_exists(log_file_path):
			var f = FileAccess.open(log_file_path, FileAccess.READ)
			if f:
				log_content = f.get_as_text()
				f.close()
		
		if log_content == "" and elapsed_time < 5.0:
			_show_message("Script exited immediately (no output).\n\nThis usually means Python3 was not found or the script has errors.\n\nPlease run manually in terminal:\npython3 ai_asset_matcher.py --download")
		elif log_content.find("Done") >= 0 or log_content.find("copied") >= 0:
			_show_message("Asset download and matching complete!\n\nPlease run Project > ReImport All in Godot to refresh resources.")
			EditorInterface.get_resource_filesystem().scan()
		else:
			_show_message("Script finished.\n\nOutput:\n" + log_content.left(500) + "\n\nRun manually if needed:\npython3 ai_asset_matcher.py --download")
		
		_reset_button()
		return
	
	if progress_label:
		var log_tail = _read_log_tail()
		if log_tail != "":
			progress_label.text = "Downloading... (" + str(int(elapsed_time)) + "s)\n\n" + log_tail
		else:
			progress_label.text = "Downloading... (" + str(int(elapsed_time)) + "s)\n\nWaiting for output..."

func _read_log_tail() -> String:
	if log_file_path == "" or not FileAccess.file_exists(log_file_path):
		return ""
	var f = FileAccess.open(log_file_path, FileAccess.READ)
	if not f:
		return ""
	var content = f.get_as_text()
	f.close()
	if content == "":
		return ""
	var lines = content.split("\n")
	var start_idx = lines.size() - 4
	if start_idx < 0:
		start_idx = 0
	var result = ""
	for i in range(start_idx, lines.size()):
		if lines[i] != "":
			result += lines[i] + "\n"
	return result

func _show_progress(msg: String):
	if not progress_dialog:
		progress_dialog = AcceptDialog.new()
		progress_dialog.title = "AI Asset Downloader"
		progress_dialog.min_size = Vector2(450, 220)
		progress_dialog.confirmed.connect(_on_progress_confirmed)
		
		progress_label = Label.new()
		progress_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		progress_label.custom_minimum_size = Vector2(400, 140)
		progress_dialog.add_child(progress_label)
		
		var base_control = get_editor_interface().get_base_control()
		base_control.add_child(progress_dialog)
	
	progress_label.text = msg
	progress_dialog.get_ok_button().text = "Run in Background"
	progress_dialog.popup_centered()

func _on_progress_confirmed():
	pass

func _close_progress():
	if progress_dialog and progress_dialog.visible:
		progress_dialog.hide()

func _show_message(msg: String):
	var dialog = AcceptDialog.new()
	dialog.title = "AI Asset Downloader"
	dialog.dialog_text = msg
	dialog.min_size = Vector2(500, 200)
	var base_control = get_editor_interface().get_base_control()
	base_control.add_child(dialog)
	dialog.popup_centered()
	dialog.confirmed.connect(_free_dialog.bind(dialog))
	dialog.canceled.connect(_free_dialog.bind(dialog))

func _free_dialog(dialog: AcceptDialog):
	dialog.queue_free()

func _reset_button():
	is_running = false
	process_id = -1
	elapsed_time = 0.0
	button.text = "Download Assets"
	button.disabled = false
