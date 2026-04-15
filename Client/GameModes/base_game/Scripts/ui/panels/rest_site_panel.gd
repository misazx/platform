class_name RestSitePanel extends Control

signal rest_action_chosen(action: String)
signal close_pressed()

var _rest_btn: Button
var _smith_btn: Button
var _recall_btn: Button

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	_create_layout()

func _create_layout() -> void:
	var bg := ColorRect.new()
	bg.set_anchors_preset(Control.PRESET_FULL_RECT)
	bg.color = Color(0, 0, 0, 0.7)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	var container := VBoxContainer.new()
	container.set_anchors_preset(Control.PRESET_CENTER)
	container.offset_left = -150
	container.offset_top = -120
	container.offset_right = 150
	container.offset_bottom = 120
	container.add_theme_constant_override("separation", 15)
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(container)
	var title := Label.new()
	title.text = "🔥 篝火"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 22)
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(title)
	_rest_btn = Button.new()
	_rest_btn.text = "❤️ 休息 (恢复30%HP)"
	_rest_btn.custom_minimum_size = Vector2(240, 44)
	_rest_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	_rest_btn.pressed.connect(func(): rest_action_chosen.emit("rest"); visible = false)
	container.add_child(_rest_btn)
	_smith_btn = Button.new()
	_smith_btn.text = "🔨 锻造 (升级一张卡牌)"
	_smith_btn.custom_minimum_size = Vector2(240, 44)
	_smith_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	_smith_btn.pressed.connect(func(): rest_action_chosen.emit("smith"); visible = false)
	container.add_child(_smith_btn)
	_recall_btn = Button.new()
	_recall_btn.text = "📖 回忆 (查看牌组)"
	_recall_btn.custom_minimum_size = Vector2(240, 44)
	_recall_btn.mouse_filter = Control.MOUSE_FILTER_STOP
	_recall_btn.pressed.connect(func(): rest_action_chosen.emit("recall"); visible = false)
	container.add_child(_recall_btn)
