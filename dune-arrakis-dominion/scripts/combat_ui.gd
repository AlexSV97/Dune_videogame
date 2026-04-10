extends Control

@onready var enemy_name_label = $CenterContainer/VBoxContainer/EnemyPanel/EnemyName
@onready var enemy_health_bar = $CenterContainer/VBoxContainer/EnemyPanel/EnemyHealthBar
@onready var enemy_health_label = $CenterContainer/VBoxContainer/EnemyPanel/EnemyHealthLabel
@onready var player_name_label = $CenterContainer/VBoxContainer/PlayerPanel/PlayerName
@onready var player_health_bar = $CenterContainer/VBoxContainer/PlayerPanel/PlayerHealthBar
@onready var player_health_label = $CenterContainer/VBoxContainer/PlayerPanel/PlayerHealthLabel
@onready var player_energy_bar = $CenterContainer/VBoxContainer/PlayerPanel/PlayerEnergyBar
@onready var player_energy_label = $CenterContainer/VBoxContainer/PlayerPanel/PlayerEnergyLabel
@onready var actions_grid = $CenterContainer/VBoxContainer/ActionsPanel/ActionsGrid
@onready var log_label = $CenterContainer/VBoxContainer/LogLabel

var action_buttons: Array[Button] = []

func _ready():
	visible = false
	var cm = get_node("/root/CombatManager")
	cm.combat_started.connect(_on_combat_started)
	cm.combat_ended.connect(_on_combat_ended)
	cm.turn_changed.connect(_on_turn_changed)
	cm.damage_dealt.connect(_on_damage_dealt)
	cm.action_performed.connect(_on_action_performed)
	_create_action_buttons()

func _create_action_buttons():
	var cm = get_node("/root/CombatManager")
	var actions = cm.get_player_actions()
	for i in range(actions.size()):
		var button = Button.new()
		button.text = actions[i]["name"]
		button.custom_minimum_size = Vector2(100, 30)
		button.pressed.connect(_on_action_button_pressed.bind(i))
		actions_grid.add_child(button)
		action_buttons.append(button)

func _on_combat_started(enemy):
	visible = true
	enemy_name_label.text = enemy.name
	log_label.text = "Combat started against " + enemy.name + "!"
	_update_stats()

func _on_combat_ended(_victory: bool):
	visible = false

func _on_turn_changed(_turn_owner: int):
	_update_buttons()
	_update_stats()

func _on_damage_dealt(target, amount):
	if target == 1:
		log_label.text = "You dealt " + str(amount) + " damage!"
	else:
		log_label.text = "You took " + str(amount) + " damage!"

func _on_action_performed(actor, action_name):
	if actor == 0:
		log_label.text = "You used " + action_name + "!"
	else:
		log_label.text = "Enemy used " + action_name + "!"

func _on_action_button_pressed(action_index):
	var cm = get_node("/root/CombatManager")
	cm.execute_player_action(action_index)
	_update_stats()

func _update_stats():
	var cm = get_node("/root/CombatManager")
	var gm = get_node("/root/GameManager")
	
	var enemy_stats = {
		"health": 0,
		"max_health": 1,
		"energy": 0,
		"max_energy": 1
	}
	var player_stats = {
		"health": 0,
		"max_health": 1,
		"energy": 0,
		"max_energy": 1
	}
	
	if cm.current_enemy:
		enemy_stats = {
			"health": cm.enemy_stats["health"],
			"max_health": cm.enemy_stats["max_health"],
			"energy": cm.enemy_stats["energy"],
			"max_energy": cm.enemy_stats["max_energy"]
		}
	
	player_stats = {
		"health": cm.player_stats["health"],
		"max_health": cm.player_stats["max_health"],
		"energy": cm.player_stats["energy"],
		"max_energy": cm.player_stats["max_energy"]
	}
	
	enemy_health_bar.max_value = enemy_stats["max_health"]
	enemy_health_bar.value = enemy_stats["health"]
	enemy_health_label.text = "HP: " + str(enemy_stats["health"]) + "/" + str(enemy_stats["max_health"])
	
	player_health_bar.max_value = player_stats["max_health"]
	player_health_bar.value = player_stats["health"]
	player_health_label.text = "HP: " + str(player_stats["health"]) + "/" + str(player_stats["max_health"])
	
	player_energy_bar.max_value = player_stats["max_energy"]
	player_energy_bar.value = player_stats["energy"]
	player_energy_label.text = "Energy: " + str(player_stats["energy"]) + "/" + str(player_stats["max_energy"])
	
	player_name_label.text = gm.player_data["name"] + " (Lv." + str(gm.player_data["level"]) + ")"

func _update_buttons():
	var cm = get_node("/root/CombatManager")
	var is_player_turn = cm.is_players_turn()
	for button in action_buttons:
		button.disabled = not is_player_turn