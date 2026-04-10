extends Control

@onready var name_input = $Panel/NameInput
@onready var description_label = $Panel/DescriptionLabel
@onready var start_button = $Panel/StartButton
@onready var back_button = $Panel/BackButton
@onready var error_label = $Panel/ErrorLabel

var selected_house: String = ""
var is_loading: bool = false

var house_data = {
	"Atreides": {
		"description": "House Atreides - Noble warriors from Caladan. Bonus: +20% diplomacy",
		"stats": {
			"health": 120, "max_health": 120, "energy": 60, "max_energy": 60,
			"combat": 12, "diplomacy": 12, "stealth": 8, "survival": 8
		}
	},
	"Harkonen": {
		"description": "House Harkonnen - Ruthless rulers from Giedi Prime. Bonus: +20% combat damage",
		"stats": {
			"health": 100, "max_health": 100, "energy": 50, "max_energy": 50,
			"combat": 15, "diplomacy": 5, "stealth": 10, "survival": 10
		}
	},
	"Fremen": {
		"description": "The Fremen - Native desert warriors of Arrakis. Bonus: +30% desert survival",
		"stats": {
			"health": 100, "max_health": 100, "energy": 70, "max_energy": 70,
			"combat": 10, "diplomacy": 8, "stealth": 15, "survival": 15
		}
	}
}

func _ready():
	$Panel/AtreidesButton.pressed.connect(_on_atreides_pressed)
	$Panel/HarkonenButton.pressed.connect(_on_harkonen_pressed)
	$Panel/FremenButton.pressed.connect(_on_fremen_pressed)
	start_button.pressed.connect(_on_start_pressed)
	back_button.pressed.connect(_on_back_pressed)
	name_input.text_changed.connect(_on_name_changed)
	start_button.disabled = true
	error_label.visible = false

func _on_name_changed(_new_text):
	_validate_inputs()

func _validate_inputs():
	var has_name = name_input.text.strip_edges().length() > 0
	var has_house = not selected_house.is_empty()
	start_button.disabled = not (has_name and has_house)
	error_label.visible = false

func _on_atreides_pressed():
	selected_house = "Atreides"
	_show_house_description("Atreides")
	_validate_inputs()

func _on_harkonen_pressed():
	selected_house = "Harkonen"
	_show_house_description("Harkonen")
	_validate_inputs()

func _on_fremen_pressed():
	selected_house = "Fremen"
	_show_house_description("Fremen")
	_validate_inputs()

func _show_house_description(house: String):
	var stats = house_data[house]["stats"]
	var desc = "Stats for " + house + ":\n\n"
	desc += "Health: " + str(stats["health"]) + "\n"
	desc += "Energy: " + str(stats["energy"]) + "\n"
	desc += "Combat: " + str(stats["combat"]) + "\n"
	desc += "Diplomacy: " + str(stats["diplomacy"]) + "\n"
	desc += "Stealth: " + str(stats["stealth"]) + "\n"
	desc += "Survival: " + str(stats["survival"]) + "\n\n"
	desc += house_data[house]["description"]
	description_label.text = desc

func _on_start_pressed():
	var player_name = name_input.text.strip_edges()
	
	if player_name.is_empty():
		error_label.text = "Please enter a name"
		error_label.visible = true
		return
	
	if selected_house.is_empty():
		error_label.text = "Please select a house"
		error_label.visible = true
		return
	
	_show_loading_screen()

func _show_loading_screen():
	is_loading = true
	$LoadingPanel.visible = true
	$Panel.visible = false
	
	var loading_bar = $LoadingPanel/LoadingBar
	var loading_text = $LoadingPanel/LoadingLabel
	var progress = 0.0
	
	while progress < 1.0:
		progress += 0.02
		loading_bar.value = progress * 100
		loading_text.text = "Loading... " + str(int(progress * 100)) + "%"
		await get_tree().process_frame
	
	_start_game()

func _start_game():
	var player_name = name_input.text.strip_edges()
	var house_stats = house_data[selected_house]["stats"]
	
	var gm = get_node("/root/GameManager")
	gm.player_data = {
		"name": player_name,
		"house": selected_house,
		"level": 1,
		"experience": 0,
		"experience_to_next": 100,
		"health": house_stats["health"],
		"max_health": house_stats["max_health"],
		"energy": house_stats["energy"],
		"max_energy": house_stats["max_energy"],
		"mentat_points": 10,
		"max_mentat_points": 10,
		"water": 100,
		"max_water": 100,
		"spice": 0,
		"attributes": {
			"combat": house_stats["combat"],
			"diplomacy": house_stats["diplomacy"],
			"stealth": house_stats["stealth"],
			"survival": house_stats["survival"]
		}
	}
	gm.current_location = "caladan"
	gm.current_state = 1
	
	var im = get_node("/root/InventoryManager")
	im.clear_inventory()
	
	get_tree().change_scene_to_file("res://scenes/world.tscn")

func _on_back_pressed():
	get_tree().change_scene_to_file("res://scenes/main_menu.tscn")