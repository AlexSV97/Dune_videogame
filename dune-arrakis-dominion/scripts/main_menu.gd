extends Control

@onready var new_game_button = $VBoxContainer/NewGameButton
@onready var continue_button = $VBoxContainer/ContinueButton
@onready var options_button = $VBoxContainer/OptionsButton
@onready var quit_button = $VBoxContainer/QuitButton
@onready var version_label = $Version

var save_file_path = "user://save_game.dat"

func _ready():
	_update_continue_button()
	new_game_button.pressed.connect(_on_new_game_pressed)
	continue_button.pressed.connect(_on_continue_pressed)
	options_button.pressed.connect(_on_options_pressed)
	quit_button.pressed.connect(_on_quit_pressed)

func _update_continue_button():
	var has_save = FileAccess.file_exists(save_file_path)
	continue_button.disabled = not has_save
	if has_save:
		continue_button.text = "Continue"
	else:
		continue_button.text = "No Save"

func _on_new_game_pressed():
	get_tree().change_scene_to_file("res://scenes/character_creation.tscn")

func _on_continue_pressed():
	if FileAccess.file_exists(save_file_path):
		_load_game()
		get_tree().change_scene_to_file("res://scenes/world.tscn")

func _on_options_pressed():
	_show_options_popup()

func _on_quit_pressed():
	get_tree().quit()

func _show_options_popup():
	var popup = Control.new()
	popup.set_anchors_preset(Control.PRESET_FULL_RECT)
	add_child(popup)
	
	var panel = Panel.new()
	panel.set_anchors_preset(Control.PRESET_CENTER)
	panel.custom_minimum_size = Vector2(400, 300)
	popup.add_child(panel)
	
	var vbox = VBoxContainer.new()
	panel.add_child(vbox)
	vbox.set_anchors_preset(Control.PRESET_FULL_RECT)
	vbox.add_theme_constant_override("separation", 20)
	
	var title = Label.new()
	title.text = "Options"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	vbox.add_child(title)
	
	var resolution_label = Label.new()
	resolution_label.text = "Resolution"
	vbox.add_child(resolution_label)
	
	var resolutions = ["1280x720", "1920x1080", "1366x768"]
	for res in resolutions:
		var btn = Button.new()
		btn.text = res
		btn.pressed.connect(_set_resolution.bind(res))
		vbox.add_child(btn)
	
	var close_btn = Button.new()
	close_btn.text = "Close"
	close_btn.pressed.connect(popup.queue_free)
	vbox.add_child(close_btn)

func _set_resolution(res: String):
	var parts = res.split("x")
	var width = int(parts[0])
	var height = int(parts[1])
	get_viewport().size = Vector2i(width, height)

func _load_game():
	if not FileAccess.file_exists(save_file_path):
		return
	
	var file = FileAccess.open(save_file_path, FileAccess.READ)
	if file:
		var gm = get_node("/root/GameManager")
		var save_data = JSON.parse_string(file.get_line())
		gm.player_data = save_data["player_data"]
		gm.current_location = save_data["current_location"]
		gm.current_state = save_data["current_state"]
		file.close()

func _notification(what):
	if what == NOTIFICATION_WM_CLOSE_REQUEST:
		_save_game_on_exit()

func _save_game_on_exit():
	var gm = get_node("/root/GameManager")
	var save_data = {
		"player_data": gm.player_data,
		"current_location": gm.current_location,
		"current_state": gm.current_state
	}
	
	var file = FileAccess.open(save_file_path, FileAccess.WRITE)
	if file:
		file.store_line(JSON.stringify(save_data))
		file.close()