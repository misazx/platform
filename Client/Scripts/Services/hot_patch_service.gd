extends Node

signal update_available(package_id: String, version: String, size: int)
signal update_progress(package_id: String, progress: float)
signal update_completed(package_id: String, version: String)
signal update_failed(package_id: String, error: String)
signal all_updates_checked(results: Dictionary)

enum UpdateStatus { IDLE, CHECKING, DOWNLOADING, APPLYING, ROLLBACK, ERROR }

const HOTFIX_DIR: String = "user://hotfix/"
const BACKUP_DIR: String = "user://hotfix_backup/"
const UPDATE_RECORD_FILE: String = "user://hotfix/update_records.json"
const MANIFEST_CACHE_FILE: String = "user://hotfix/manifest_cache.json"

var _status: int = UpdateStatus.IDLE
var _current_package_id: String = ""
var _update_records: Dictionary = {}
var _manifest_cache: Dictionary = {}
var _pending_updates: Dictionary = {}
var _http_pool: Array = []

static var instance: HotPatchService = null

func _ready() -> void:
	if instance != null and instance != self:
		queue_free()
		return
	instance = self
	_ensure_directories()
	_load_update_records()
	_load_manifest_cache()

func _ensure_directories() -> void:
	if not DirAccess.dir_exists_absolute(HOTFIX_DIR):
		DirAccess.make_dir_recursive_absolute(HOTFIX_DIR)
	if not DirAccess.dir_exists_absolute(BACKUP_DIR):
		DirAccess.make_dir_recursive_absolute(BACKUP_DIR)
	var temp_dir: String = HOTFIX_DIR + "temp/"
	if not DirAccess.dir_exists_absolute(temp_dir):
		DirAccess.make_dir_recursive_absolute(temp_dir)

func _load_update_records() -> void:
	if not FileAccess.file_exists(UPDATE_RECORD_FILE):
		_update_records = {}
		return
	var file: FileAccess = FileAccess.open(UPDATE_RECORD_FILE, FileAccess.READ)
	if file == null:
		_update_records = {}
		return
	var json: JSON = JSON.new()
	if json.parse(file.get_as_text()) != OK:
		_update_records = {}
		return
	var data: Dictionary = json.data as Dictionary
	if data == null:
		_update_records = {}
		return
	_update_records = data

func _save_update_records() -> void:
	var json_text: String = JSON.stringify(_update_records, "\t")
	var file: FileAccess = FileAccess.open(UPDATE_RECORD_FILE, FileAccess.WRITE)
	if file != null:
		file.store_string(json_text)

func _load_manifest_cache() -> void:
	if not FileAccess.file_exists(MANIFEST_CACHE_FILE):
		_manifest_cache = {}
		return
	var file: FileAccess = FileAccess.open(MANIFEST_CACHE_FILE, FileAccess.READ)
	if file == null:
		_manifest_cache = {}
		return
	var json: JSON = JSON.new()
	if json.parse(file.get_as_text()) != OK:
		_manifest_cache = {}
		return
	_manifest_cache = json.data as Dictionary if json.data is Dictionary else {}

func _save_manifest_cache() -> void:
	var json_text: String = JSON.stringify(_manifest_cache, "\t")
	var file: FileAccess = FileAccess.open(MANIFEST_CACHE_FILE, FileAccess.WRITE)
	if file != null:
		file.store_string(json_text)

func get_status() -> int:
	return _status

func get_current_package_id() -> String:
	return _current_package_id

func get_installed_version(package_id: String) -> String:
	if _update_records.has(package_id):
		var record: Dictionary = _update_records[package_id]
		return record.get("installed_version", "")
	var pkg_svc = get_node_or_null("/root/PackageService")
	if pkg_svc != null:
		var pkg: Dictionary = pkg_svc.get_package(package_id)
		return pkg.get("version", "1.0.0")
	return "1.0.0"

func get_pending_update(package_id: String) -> Dictionary:
	return _pending_updates.get(package_id, {})

func has_pending_update(package_id: String) -> bool:
	return _pending_updates.has(package_id)

func check_for_updates(cdn_base_url: String = "") -> void:
	if _status != UpdateStatus.IDLE:
		update_failed.emit("", "更新检查正在进行中")
		return

	var base_url: String = cdn_base_url
	if base_url == "":
		base_url = _get_cdn_base_url()

	if base_url == "":
		update_failed.emit("", "未配置CDN地址")
		return

	_status = UpdateStatus.CHECKING
	_current_package_id = ""

	var pkg_svc = get_node_or_null("/root/PackageService")
	if pkg_svc == null:
		_status = UpdateStatus.IDLE
		update_failed.emit("", "PackageService不可用")
		return

	var packages: Array = pkg_svc.get_all_packages()
	var check_count: int = 0
	var completed_count: int = 0
	var results: Dictionary = {}

	for pkg in packages:
		var pid: String = pkg.get("id", "")
		if pid == "":
			continue
		check_count += 1
		_check_package_update(pid, base_url, func(pid_result: String, has_update: bool, manifest: Dictionary):
			completed_count += 1
			results[pid_result] = {"has_update": has_update, "manifest": manifest}
			if has_update and not manifest.is_empty():
				_manifest_cache[pid_result] = manifest
				_pending_updates[pid_result] = manifest
				var version: String = manifest.get("version", "")
				var size: int = manifest.get("total_size", 0)
				update_available.emit(pid_result, version, size)

			if completed_count >= check_count:
				_save_manifest_cache()
				_status = UpdateStatus.IDLE
				_current_package_id = ""
				all_updates_checked.emit(results)
		)

	if check_count == 0:
		_status = UpdateStatus.IDLE
		all_updates_checked.emit(results)

func check_package_update(package_id: String, cdn_base_url: String = "") -> void:
	if _status != UpdateStatus.IDLE:
		update_failed.emit(package_id, "更新操作正在进行中")
		return

	var base_url: String = cdn_base_url
	if base_url == "":
		base_url = _get_cdn_base_url()

	if base_url == "":
		update_failed.emit(package_id, "未配置CDN地址")
		return

	_status = UpdateStatus.CHECKING
	_current_package_id = package_id

	_check_package_update(package_id, base_url, func(pid: String, has_update: bool, manifest: Dictionary):
		_status = UpdateStatus.IDLE
		_current_package_id = ""
		if has_update and not manifest.is_empty():
			_manifest_cache[pid] = manifest
			_pending_updates[pid] = manifest
			_save_manifest_cache()
			var version: String = manifest.get("version", "")
			var size: int = manifest.get("total_size", 0)
			update_available.emit(pid, version, size)
		else:
			update_available.emit(pid, "", 0)
	)

func _check_package_update(package_id: String, base_url: String, callback: Callable) -> void:
	var manifest_url: String = "%s/updates/%s/manifest.json" % [base_url.rstrip("/"), package_id]
	var http: HTTPRequest = HTTPRequest.new()
	http.name = "HTTP_CheckUpdate_%s" % package_id
	add_child(http)

	var current_version: String = get_installed_version(package_id)

	http.request_completed.connect(func(result: int, code: int, _headers: PackedStringArray, body: PackedByteArray):
		http.queue_free()

		if result != HTTPRequest.RESULT_SUCCESS or code != 200:
			callback.call(package_id, false, {})
			return

		var json: JSON = JSON.new()
		if json.parse(body.get_string_from_utf8()) != OK:
			callback.call(package_id, false, {})
			return

		var manifest: Dictionary = json.data as Dictionary
		if manifest.is_empty():
			callback.call(package_id, false, {})
			return

		var remote_version: String = manifest.get("version", "")
		if remote_version == "" or not _is_version_newer(remote_version, current_version):
			callback.call(package_id, false, {})
			return

		callback.call(package_id, true, manifest)
	)

	var err: int = http.request(manifest_url)
	if err != OK:
		http.queue_free()
		callback.call(package_id, false, {})

func download_and_apply_update(package_id: String, cdn_base_url: String = "") -> void:
	if _status != UpdateStatus.IDLE:
		update_failed.emit(package_id, "更新操作正在进行中")
		return

	if not _pending_updates.has(package_id):
		update_failed.emit(package_id, "没有可用的更新")
		return

	var manifest: Dictionary = _pending_updates[package_id]
	var base_url: String = cdn_base_url
	if base_url == "":
		base_url = _get_cdn_base_url()

	if base_url == "":
		update_failed.emit(package_id, "未配置CDN地址")
		return

	_status = UpdateStatus.DOWNLOADING
	_current_package_id = package_id

	_download_update_files(package_id, manifest, base_url)

func _download_update_files(package_id: String, manifest: Dictionary, base_url: String) -> void:
	var files: Array = manifest.get("files", [])
	if files.is_empty():
		_status = UpdateStatus.IDLE
		update_failed.emit(package_id, "更新清单中没有文件")
		return

	var total_files: int = files.size()
	var downloaded_files: int = 0
	var failed_files: int = 0
	var temp_dir: String = HOTFIX_DIR + "temp/%s/" % package_id

	if not DirAccess.dir_exists_absolute(temp_dir):
		DirAccess.make_dir_recursive_absolute(temp_dir)

	var download_url: String = manifest.get("download_url", "")
	if download_url != "":
		_download_full_package(package_id, download_url, base_url, manifest)
		return

	for file_info in files:
		var file_path: String = file_info.get("path", "")
		var file_hash: String = file_info.get("hash", "")
		if file_path == "":
			continue

		_download_single_file(package_id, file_path, file_hash, base_url, temp_dir,
			func(success: bool):
				if success:
					downloaded_files += 1
				else:
					failed_files += 1

				var progress: float = float(downloaded_files + failed_files) / float(total_files)
				update_progress.emit(package_id, progress)

				if downloaded_files + failed_files >= total_files:
					if failed_files > 0:
						_status = UpdateStatus.ERROR
						update_failed.emit(package_id, "%d个文件下载失败" % failed_files)
						return
					_apply_update(package_id, manifest, temp_dir)
		)

func _download_full_package(package_id: String, download_url: String, base_url: String, manifest: Dictionary) -> void:
	var full_url: String = download_url
	if not full_url.begins_with("http"):
		full_url = "%s/%s" % [base_url.rstrip("/"), download_url.lstrip("/")]

	var temp_dir: String = HOTFIX_DIR + "temp/%s/" % package_id
	var temp_file: String = temp_dir + "update.zip"

	var http: HTTPRequest = HTTPRequest.new()
	http.name = "HTTP_DownloadFull_%s" % package_id
	http.download_file = temp_file
	add_child(http)

	http.request_completed.connect(func(result: int, code: int, _headers: PackedStringArray, _body: PackedByteArray):
		http.queue_free()

		if result != HTTPRequest.RESULT_SUCCESS or code != 200:
			_status = UpdateStatus.ERROR
			update_failed.emit(package_id, "完整包下载失败 (HTTP %d)" % code)
			return

		update_progress.emit(package_id, 1.0)
		_apply_update_from_zip(package_id, manifest, temp_file, temp_dir)
	)

	var err: int = http.request(full_url)
	if err != OK:
		http.queue_free()
		_status = UpdateStatus.ERROR
		update_failed.emit(package_id, "下载请求失败")

func _download_single_file(package_id: String, file_path: String, expected_hash: String, base_url: String, temp_dir: String, callback: Callable) -> void:
	var url: String = "%s/updates/%s/files/%s" % [base_url.rstrip("/"), package_id, file_path.lstrip("/")]
	var local_path: String = temp_dir + file_path.get_file()

	var dir: String = local_path.get_base_dir()
	if not DirAccess.dir_exists_absolute(dir):
		DirAccess.make_dir_recursive_absolute(dir)

	var http: HTTPRequest = HTTPRequest.new()
	http.name = "HTTP_DownloadFile_%s_%s" % [package_id, file_path.get_file()]
	http.download_file = local_path
	add_child(http)

	http.request_completed.connect(func(result: int, code: int, _headers: PackedStringArray, _body: PackedByteArray):
		http.queue_free()

		if result != HTTPRequest.RESULT_SUCCESS or code != 200:
			callback.call(false)
			return

		if expected_hash != "" and FileAccess.file_exists(local_path):
			var actual_hash: String = _compute_file_hash(local_path)
			if actual_hash != expected_hash:
				callback.call(false)
				return

		callback.call(true)
	)

	var err: int = http.request(url)
	if err != OK:
		http.queue_free()
		callback.call(false)

func _apply_update(package_id: String, manifest: Dictionary, temp_dir: String) -> void:
	_status = UpdateStatus.APPLYING
	_current_package_id = package_id

	var target_version: String = manifest.get("version", "")
	var files: Array = manifest.get("files", [])

	if not _create_backup(package_id):
		_status = UpdateStatus.ERROR
		update_failed.emit(package_id, "创建备份失败，中止更新")
		return

	var applied_count: int = 0
	var failed_count: int = 0

	for file_info in files:
		var file_path: String = file_info.get("path", "")
		var file_type: String = file_info.get("type", "script")

		if file_path == "":
			continue

		var source_path: String = temp_dir + file_path.get_file()
		var dest_path: String = HOTFIX_DIR + "%s/%s" % [package_id, file_path]

		if not _apply_single_file(source_path, dest_path):
			failed_count += 1
			continue

		applied_count += 1

	if failed_count > 0:
		_perform_rollback(package_id)
		_status = UpdateStatus.ERROR
		update_failed.emit(package_id, "%d个文件应用失败，已回滚" % failed_count)
		return

	_update_records[package_id] = {
		"installed_version": target_version,
		"applied_at": Time.get_datetime_string_from_system(),
		"files_count": applied_count,
		"backup_exists": true,
	}
	_save_update_records()

	_cleanup_temp(package_id)

	_pending_updates.erase(package_id)

	_status = UpdateStatus.IDLE
	_current_package_id = ""
	update_completed.emit(package_id, target_version)

func _apply_update_from_zip(package_id: String, manifest: Dictionary, zip_path: String, temp_dir: String) -> void:
	_status = UpdateStatus.APPLYING
	_current_package_id = package_id

	var target_version: String = manifest.get("version", "")
	var files: Array = manifest.get("files", [])

	if not _create_backup(package_id):
		_status = UpdateStatus.ERROR
		update_failed.emit(package_id, "创建备份失败，中止更新")
		return

	var reader: ZIPReader = ZIPReader.new()
	var err: int = reader.open(zip_path)
	if err != OK:
		_status = UpdateStatus.ERROR
		update_failed.emit(package_id, "无法打开更新包 (错误码: %d)" % err)
		reader.close()
		return

	var file_list: PackedStringArray = reader.get_files()
	var applied_count: int = 0
	var failed_count: int = 0

	for file_entry in file_list:
		if file_entry.ends_with("/"):
			continue

		var dest_path: String = HOTFIX_DIR + "%s/%s" % [package_id, file_entry]
		var dest_dir: String = dest_path.get_base_dir()

		if not DirAccess.dir_exists_absolute(dest_dir):
			DirAccess.make_dir_recursive_absolute(dest_dir)

		var data: PackedByteArray = reader.read_file(file_entry)
		var file_access: FileAccess = FileAccess.open(dest_path, FileAccess.WRITE)
		if file_access == null:
			failed_count += 1
			continue

		file_access.store_buffer(data)
		file_access.close()
		applied_count += 1

	reader.close()

	if failed_count > 0:
		_perform_rollback(package_id)
		_status = UpdateStatus.ERROR
		update_failed.emit(package_id, "%d个文件解压失败，已回滚" % failed_count)
		return

	_update_records[package_id] = {
		"installed_version": target_version,
		"applied_at": Time.get_datetime_string_from_system(),
		"files_count": applied_count,
		"backup_exists": true,
	}
	_save_update_records()

	_cleanup_temp(package_id)

	_pending_updates.erase(package_id)

	_status = UpdateStatus.IDLE
	_current_package_id = ""
	update_completed.emit(package_id, target_version)

func _apply_single_file(source_path: String, dest_path: String) -> bool:
	if not FileAccess.file_exists(source_path):
		return false

	var dest_dir: String = dest_path.get_base_dir()
	if not DirAccess.dir_exists_absolute(dest_dir):
		DirAccess.make_dir_recursive_absolute(dest_dir)

	var source_file: FileAccess = FileAccess.open(source_path, FileAccess.READ)
	if source_file == null:
		return false

	var content: PackedByteArray = source_file.get_buffer(source_file.get_length())
	source_file.close()

	var dest_file: FileAccess = FileAccess.open(dest_path, FileAccess.WRITE)
	if dest_file == null:
		return false

	dest_file.store_buffer(content)
	dest_file.close()
	return true

func _create_backup(package_id: String) -> bool:
	var hotfix_path: String = HOTFIX_DIR + package_id + "/"
	if not DirAccess.dir_exists_absolute(hotfix_path):
		return true

	var backup_path: String = BACKUP_DIR + package_id + "/"
	if DirAccess.dir_exists_absolute(backup_path):
		_remove_directory(backup_path)

	DirAccess.make_dir_recursive_absolute(backup_path)

	return _copy_directory(hotfix_path, backup_path)

func _copy_directory(source: String, dest: String) -> bool:
	var dir: DirAccess = DirAccess.open(source)
	if dir == null:
		return false

	dir.list_dir_begin()
	var file_name: String = dir.get_next()
	while file_name != "":
		if dir.current_is_dir():
			var sub_source: String = source + file_name + "/"
			var sub_dest: String = dest + file_name + "/"
			DirAccess.make_dir_recursive_absolute(sub_dest)
			if not _copy_directory(sub_source, sub_dest):
				dir.list_dir_end()
				return false
		else:
			var src_file: String = source + file_name
			var dst_file: String = dest + file_name
			var err: int = DirAccess.copy_absolute(src_file, dst_file)
			if err != OK:
				dir.list_dir_end()
				return false
		file_name = dir.get_next()

	dir.list_dir_end()
	return true

func _remove_directory(path: String) -> void:
	var dir: DirAccess = DirAccess.open(path)
	if dir == null:
		return

	dir.list_dir_begin()
	var file_name: String = dir.get_next()
	while file_name != "":
		if dir.current_is_dir():
			_remove_directory(path + file_name + "/")
		else:
			DirAccess.remove_absolute(path + file_name)
		file_name = dir.get_next()
	dir.list_dir_end()

	DirAccess.remove_absolute(path)

func _cleanup_temp(package_id: String) -> void:
	var temp_dir: String = HOTFIX_DIR + "temp/%s/" % package_id
	if DirAccess.dir_exists_absolute(temp_dir):
		_remove_directory(temp_dir)

func _perform_rollback(package_id: String) -> void:
	var backup_path: String = BACKUP_DIR + package_id + "/"
	if not DirAccess.dir_exists_absolute(backup_path):
		return

	var hotfix_path: String = HOTFIX_DIR + package_id + "/"
	if DirAccess.dir_exists_absolute(hotfix_path):
		_remove_directory(hotfix_path)

	_copy_directory(backup_path, hotfix_path)

	_remove_directory(backup_path)
	if _update_records.has(package_id):
		_update_records[package_id]["backup_exists"] = false
		_save_update_records()

func rollback_update(package_id: String) -> bool:
	if _status != UpdateStatus.IDLE:
		update_failed.emit(package_id, "更新操作正在进行中，无法回滚")
		return false

	_status = UpdateStatus.ROLLBACK
	_current_package_id = package_id

	var backup_path: String = BACKUP_DIR + package_id + "/"
	if not DirAccess.dir_exists_absolute(backup_path):
		_status = UpdateStatus.IDLE
		_current_package_id = ""
		update_failed.emit(package_id, "没有找到备份数据")
		return false

	var hotfix_path: String = HOTFIX_DIR + package_id + "/"
	if DirAccess.dir_exists_absolute(hotfix_path):
		_remove_directory(hotfix_path)

	var success: bool = _copy_directory(backup_path, hotfix_path)

	if success:
		if _update_records.has(package_id):
			var record: Dictionary = _update_records[package_id]
			var prev_version: String = record.get("previous_version", "")
			_update_records[package_id]["installed_version"] = prev_version
			_update_records[package_id]["rolled_back_at"] = Time.get_datetime_string_from_system()
			_save_update_records()

	_remove_directory(backup_path)
	_update_records[package_id]["backup_exists"] = false
	_save_update_records()

	_status = UpdateStatus.IDLE
	_current_package_id = ""

	if success:
		update_completed.emit(package_id, get_installed_version(package_id))
	else:
		update_failed.emit(package_id, "回滚失败")

	return success

func get_hotfix_path(package_id: String, relative_path: String) -> String:
	var hotfix_file: String = HOTFIX_DIR + "%s/%s" % [package_id, relative_path]
	if FileAccess.file_exists(hotfix_file):
		return hotfix_file
	return ""

func get_hotfix_script(package_id: String, script_path: String) -> Script:
	var hotfix_path: String = get_hotfix_path(package_id, script_path)
	if hotfix_path == "":
		return null

	var script: Script = ResourceLoader.load(hotfix_path, "", ResourceLoader.CACHE_MODE_IGNORE) as Script
	return script

func reload_script_at_path(script_path: String) -> bool:
	if not FileAccess.file_exists(script_path):
		return false

	var script: Script = ResourceLoader.load(script_path, "", ResourceLoader.CACHE_MODE_IGNORE) as Script
	if script == null:
		return false

	if script.can_instantiate():
		return true

	return true

func get_all_hotfix_files(package_id: String) -> Array:
	var result: Array = []
	var hotfix_path: String = HOTFIX_DIR + package_id + "/"
	if not DirAccess.dir_exists_absolute(hotfix_path):
		return result

	_collect_files_recursive(hotfix_path, "", result)
	return result

func _collect_files_recursive(base_path: String, current_rel: String, result: Array) -> void:
	var full_path: String = base_path + current_rel
	var dir: DirAccess = DirAccess.open(full_path)
	if dir == null:
		return

	dir.list_dir_begin()
	var file_name: String = dir.get_next()
	while file_name != "":
		var rel_path: String = current_rel + file_name if current_rel == "" else current_rel + "/" + file_name
		if dir.current_is_dir():
			_collect_files_recursive(base_path, rel_path + "/", result)
		else:
			result.append(rel_path)
		file_name = dir.get_next()
	dir.list_dir_end()

func _is_version_newer(remote: String, local: String) -> bool:
	var remote_parts: PackedStringArray = remote.split(".")
	var local_parts: PackedStringArray = local.split(".")

	var max_len: int = maxi(remote_parts.size(), local_parts.size())

	for i in range(max_len):
		var r: int = remote_parts[i].to_int() if i < remote_parts.size() else 0
		var l: int = local_parts[i].to_int() if i < local_parts.size() else 0
		if r > l:
			return true
		if r < l:
			return false

	return false

func _compute_file_hash(file_path: String) -> String:
	if not FileAccess.file_exists(file_path):
		return ""
	return FileAccess.get_sha256(file_path)

func _get_cdn_base_url() -> String:
	var pkg_svc = get_node_or_null("/root/PackageService")
	if pkg_svc != null:
		var config: Dictionary = pkg_svc.get_package_config("_system")
		if not config.is_empty():
			return config.get("cdn_base_url", "")

	var config_path: String = "res://Config/Data/hotfix_config.json"
	if ResourceLoader.exists(config_path):
		var file: FileAccess = FileAccess.open(config_path, FileAccess.READ)
		if file != null:
			var json: JSON = JSON.new()
			if json.parse(file.get_as_text()) == OK:
				var data: Dictionary = json.data as Dictionary
				if data != null:
					return data.get("cdn_base_url", "http://localhost:8080")
	return "http://localhost:8080"

func get_update_history(package_id: String) -> Dictionary:
	return _update_records.get(package_id, {})

func clear_pending_update(package_id: String) -> void:
	_pending_updates.erase(package_id)

func get_all_pending_updates() -> Dictionary:
	return _pending_updates.duplicate()
