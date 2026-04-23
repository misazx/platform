extends Node2D

func _ready() -> void:
	print("=== TEST SCENE READY ===")
	print("Test print 1")
	print("Test print 2")
	
	var label = Label.new()
	label.text = "TEST SCENE WORKING"
	label.position = Vector2(100, 100)
	label.add_theme_font_size_override("font_size", 48)
	add_child(label)
	
	print("=== TEST SCENE SETUP DONE ===")
