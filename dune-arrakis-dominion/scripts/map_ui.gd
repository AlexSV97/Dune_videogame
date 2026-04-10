extends Control

@onready var locations_container = $Panel/LocationsContainer
@onready var current_location_label = $Panel/CurrentLocation
@onready var close_button = $Panel/CloseButton

var location_buttons: Array[Button] = []

func _ready():
	visible = false
	close_button.pressed.connect(_on_close_pressed)
	var lm = get_node("/root/LocationManager")
	lm.locations_updated.connect(_update_locations)
	lm.location_changed.connect(_on_location_changed)

func _update_locations():
	_clear_buttons()
	
	var lm = get_node("/root/LocationManager")
	var gm = get_node("/root/GameManager")
	var locations = lm.get_all_locations()
	var player_level = gm.player_data["level"]
	
	for loc in locations:
		var button = Button.new()
		button.custom_minimum_size = Vector2(450, 50)
		
		var is_accessible = loc.is_accessible(player_level)
		var status = "🔓" if is_accessible else "🔒"
		var danger = " ⚠️" if loc.is_dangerous else ""
		var level_req = " (Lv." + str(loc.required_level) + ")" if loc.required_level > 1 else ""
		
		button.text = status + " " + loc.name + level_req + danger
		button.tooltip_text = loc.description
		
		if not is_accessible:
			button.disabled = true
		else:
			button.pressed.connect(_on_location_button_pressed.bind(loc.id))
		
		locations_container.add_child(button)
		location_buttons.append(button)
	
	var current_loc = lm.get_current_location()
	if current_loc:
		current_location_label.text = "Current Location: " + current_loc.name

func _clear_buttons():
	for button in location_buttons:
		button.queue_free()
	location_buttons.clear()

func _on_location_button_pressed(location_id: String):
	var lm = get_node("/root/LocationManager")
	lm.travel_to(location_id)
	_update_locations()

func _on_close_pressed():
	visible = false

func _on_location_changed(_from: String, _to: String):
	_update_locations()

func toggle_map():
	visible = not visible
	if visible:
		_update_locations()