class_name AudioManager extends Node

var _bgm_player: AudioStreamPlayer
var _sfx_players: Array = []
var _sfx_index := 0
const SFX_PLAYER_COUNT := 8
const BGM_VOLUME := -12.0
const SFX_VOLUME := 0.0

var _current_bgm := ""
var _bgm_positions: Dictionary = {}

var music_volume := 0.8
var sfx_volume := 1.0

const SFX_PATH := "res://GameModes/base_game/Resources/Audio/SFX/"
const BGM_PATH := "res://GameModes/base_game/Resources/Audio/BGM/"

func _ready() -> void:
	_bgm_player = AudioStreamPlayer.new()
	_bgm_player.bus = "BGM"
	_bgm_player.volume_db = BGM_VOLUME
	add_child(_bgm_player)

	for i in range(SFX_PLAYER_COUNT):
		var sfx := AudioStreamPlayer.new()
		sfx.bus = "SFX"
		sfx.volume_db = SFX_VOLUME
		add_child(sfx)
		_sfx_players.append(sfx)

	GD.print("[AudioManager] Initialized")

func play_bgm(bgm_name: String, loop := true, fade_time := 0.5) -> void:
	if _current_bgm == bgm_name and _bgm_player.playing:
		return

	if _current_bgm != "":
		_bgm_positions[_current_bgm] = _bgm_player.get_playback_position()
		tween_out(_bgm_player, fade_time)

	var stream := load_audio(BGM_PATH + bgm_name)
	if stream != null:
		_bgm_player.stream = stream
		_bgm_player.play(_bgm_positions.get(bgm_name, 0.0))
		_current_bgm = bgm_name
		tween_in(_bgm_player, fade_time)
	else:
		GD.print("[AudioManager] BGM not found: %s" % bgm_name)

func pause_bgm(fade_time := 0.3) -> void:
	if _current_bgm != "":
		_bgm_positions[_current_bgm] = _bgm_player.get_playback_position()
		tween_out(_bgm_player, fade_time)
		_bgm_player.stop()

func resume_bgm(fade_time := 0.3) -> void:
	if _current_bgm != "" and _bgm_player.stream != null:
		_bgm_player.play(_bgm_positions.get(_current_bgm, 0.0))
		tween_in(_bgm_player, fade_time)

func stop_bgm(fade_time := 0.5) -> void:
	tween_out(_bgm_player, fade_time)
	_bgm_player.stop()
	_current_bgm = ""

func play_sfx(sfx_name: String) -> void:
	var path := SFX_PATH + sfx_name
	var stream := load_audio(path)
	if stream == null:
		path = "res://Assets/Audio/SFX/" + sfx_name
		stream = load_audio(path)
	if stream == null:
		return

	var player: AudioStreamPlayer = _sfx_players[_sfx_index]
	player.stream = stream
	player.play()
	_sfx_index = (_sfx_index + 1) % SFX_PLAYER_COUNT

func play_click() -> void:
	play_sfx("click.ogg")

func play_button_click() -> void:
	play_sfx("button_click.wav")

func play_card_play() -> void:
	play_sfx("card_play.wav")

func play_combat_hit() -> void:
	play_sfx("combat_hit.wav")

func play_victory_fanfare() -> void:
	play_sfx("victory_fanfare.ogg")

func load_audio(path: String) -> AudioStream:
	if ResourceLoader.exists(path):
		return load(path)
	return null

func tween_out(player: AudioStreamPlayer, duration: float) -> void:
	var tween := create_tween()
	tween.tween_property(player, "volume_db", -80.0, duration)

func tween_in(player: AudioStreamPlayer, duration: float) -> void:
	var tween := create_tween()
	tween.set_parallel(true)
	tween.tween_property(player, "volume_db", BGM_VOLUME, duration)
