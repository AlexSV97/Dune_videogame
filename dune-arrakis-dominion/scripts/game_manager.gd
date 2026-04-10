extends Node

enum GameState { MENU, PLAYING, PAUSED, COMBAT, DIALOGUE }

var current_state: GameState = GameState.MENU
var player_data: Dictionary = {
	"name": "Paul Atreides",
	"level": 1,
	"experience": 0,
	"experience_to_next": 100,
	"health": 100,
	"max_health": 100,
	"energy": 50,
	"max_energy": 50,
	"mentat_points": 10,
	"max_mentat_points": 10,
	"water": 100,
	"max_water": 100,
	"spice": 0,
	"attributes": {
		"combat": 10,
		"diplomacy": 10,
		"stealth": 10,
		"survival": 10
	}
}

var current_location: String = "caladan_boarding"
var game_time: int = 0

signal state_changed(new_state: GameState)
signal player_data_updated()

func change_state(new_state: GameState) -> void:
	current_state = new_state
	state_changed.emit(new_state)

func update_player_attribute(attribute: String, value: int) -> void:
	if player_data["attributes"].has(attribute):
		player_data["attributes"][attribute] += value
		player_data_updated.emit()

func take_damage(amount: int) -> void:
	player_data["health"] = max(0, player_data["health"] - amount)
	player_data_updated.emit()
	if player_data["health"] <= 0:
		change_state(GameState.MENU)

func heal(amount: int) -> void:
	player_data["health"] = min(player_data["max_health"], player_data["health"] + amount)
	player_data_updated.emit()

func use_energy(amount: int) -> bool:
	if player_data["energy"] >= amount:
		player_data["energy"] -= amount
		player_data_updated.emit()
		return true
	return false

func use_water(amount: int) -> bool:
	if player_data["water"] >= amount:
		player_data["water"] -= amount
		player_data_updated.emit()
		return true
	return false

func add_experience(amount: int) -> void:
	player_data["experience"] += amount
	while player_data["experience"] >= player_data["experience_to_next"]:
		player_data["experience"] -= player_data["experience_to_next"]
		player_data["level"] += 1
		player_data["experience_to_next"] = int(player_data["experience_to_next"] * 1.5)
		player_data["max_health"] += 10
		player_data["health"] = player_data["max_health"]
		player_data["max_energy"] += 5
		player_data["energy"] = player_data["max_energy"]
	player_data_updated.emit()

func add_spice(amount: int) -> void:
	add_resource("spice", amount)

func add_resource(type: String, amount: int) -> void:
	if type == "spice":
		player_data["spice"] += amount
	elif type == "water":
		player_data["water"] = min(player_data["max_water"], player_data["water"] + amount)
	elif type == "stone":
		if not player_data.has("stone"):
			player_data["stone"] = 0
		player_data["stone"] += amount
	player_data_updated.emit()

func open_build_menu():
	print("Build menu opened")