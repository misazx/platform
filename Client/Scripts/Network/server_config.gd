extends Node

var server_url: String = "http://127.0.0.1:5002"

static var instance: Node = null

func _ready() -> void:
	if instance != null and instance != self:
		queue_free()
		return
	instance = self
	_load_config()

func _load_config() -> void:
	var config_path := "user://server_config.json"
	if FileAccess.file_exists(config_path):
		var file := FileAccess.open(config_path, FileAccess.READ)
		if file != null:
			var json := JSON.new()
			if json.parse(file.get_as_text()) == OK and json.data is Dictionary:
				var data: Dictionary = json.data
				var url: String = data.get("server_url", "")
				if url != "":
					server_url = url

func set_server_url(url: String) -> void:
	server_url = url
	var data := {"server_url": url}
	var file := FileAccess.open("user://server_config.json", FileAccess.WRITE)
	if file != null:
		file.store_string(JSON.stringify(data, "\t"))

func get_server_url() -> String:
	return server_url
