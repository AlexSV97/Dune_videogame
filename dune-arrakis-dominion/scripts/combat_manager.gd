extends Node

signal combat_started(enemy: EnemyData)
signal combat_ended(victory: bool)
signal turn_changed(turn_owner: int)
signal damage_dealt(target: int, amount: int)
signal action_performed(actor: int, action_name: String)

enum CombatantType { PLAYER, ENEMY }

var current_turn: CombatantType = CombatantType.PLAYER
var player_stats: Dictionary = {}
var enemy_stats: Dictionary = {}
var current_enemy: EnemyData = null
var is_combat_active: bool = false

var player_actions = [
	{"name": "Attack", "damage": 10, "cost": 5},
	{"name": "Power Strike", "damage": 20, "cost": 15},
	{"name": "Defend", "damage": 0, "cost": 0, "effect": "defend"},
	{"name": "Use Item", "damage": 0, "cost": 0},
	{"name": "Flee", "damage": 0, "cost": 0}
]

func _get_gm():
	return get_node("/root/GameManager")

func start_combat(enemy: EnemyData) -> void:
	current_enemy = enemy
	var gm = _get_gm()
	
	player_stats = {
		"health": gm.player_data["health"],
		"max_health": gm.player_data["max_health"],
		"energy": gm.player_data["energy"],
		"max_energy": gm.player_data["max_energy"],
		"defense_mod": 0
	}
	
	enemy_stats = {
		"health": enemy.health,
		"max_health": enemy.health,
		"energy": enemy.energy,
		"max_energy": enemy.energy,
		"defense_mod": 0
	}
	
	is_combat_active = true
	current_turn = CombatantType.PLAYER
	emit_signal("combat_started", enemy)
	emit_signal("turn_changed", current_turn)

func execute_player_action(action_index: int) -> void:
	if not is_combat_active or current_turn != CombatantType.PLAYER:
		return
	
	var action = player_actions[action_index]
	var cost = action.get("cost", 0)
	
	if player_stats["energy"] < cost:
		return
	
	player_stats["energy"] -= cost
	
	match action["name"]:
		"Attack":
			var damage = _calculate_damage(action["damage"], player_stats["defense_mod"], enemy_stats["defense_mod"])
			enemy_stats["health"] = max(0, enemy_stats["health"] - damage)
			emit_signal("damage_dealt", CombatantType.ENEMY, damage)
			emit_signal("action_performed", CombatantType.PLAYER, action["name"])
		"Power Strike":
			var damage = _calculate_damage(action["damage"], player_stats["defense_mod"], enemy_stats["defense_mod"])
			enemy_stats["health"] = max(0, enemy_stats["health"] - damage)
			emit_signal("damage_dealt", CombatantType.ENEMY, damage)
			emit_signal("action_performed", CombatantType.PLAYER, action["name"])
		"Defend":
			player_stats["defense_mod"] = 5
			emit_signal("action_performed", CombatantType.PLAYER, action["name"])
		"Use Item":
			emit_signal("action_performed", CombatantType.PLAYER, action["name"])
		"Flee":
			_flee_combat()
			return
	
	if enemy_stats["health"] <= 0:
		_end_combat(true)
	else:
		_end_player_turn()

func execute_enemy_action() -> void:
	if not is_combat_active or current_turn != CombatantType.ENEMY:
		return
	
	var actions = current_enemy.get_actions()
	var action = actions.pick_random()
	
	var cost = action.get("cost", 0)
	if enemy_stats["energy"] < cost:
		action = actions[0]
		cost = 0
	
	enemy_stats["energy"] -= cost
	
	if action["damage"] > 0:
		var damage = _calculate_damage(action["damage"], enemy_stats["defense_mod"], player_stats["defense_mod"])
		player_stats["health"] = max(0, player_stats["health"] - damage)
		emit_signal("damage_dealt", CombatantType.PLAYER, damage)
		emit_signal("action_performed", CombatantType.ENEMY, action["name"])
	else:
		if action["name"] == "Shield Block":
			enemy_stats["defense_mod"] = 5
		emit_signal("action_performed", CombatantType.ENEMY, action["name"])
	
	if player_stats["health"] <= 0:
		_end_combat(false)
	else:
		_end_enemy_turn()

func _calculate_damage(base_damage: int, _attacker_defense: int, target_defense: int) -> int:
	var variance = randf_range(0.8, 1.2)
	var defense_reduction = target_defense * 0.5
	var final_damage = int((base_damage - defense_reduction) * variance)
	return max(1, final_damage)

func _end_player_turn():
	player_stats["defense_mod"] = 0
	current_turn = CombatantType.ENEMY
	emit_signal("turn_changed", current_turn)
	
	await get_tree().create_timer(1.0).timeout
	execute_enemy_action()

func _end_enemy_turn():
	enemy_stats["defense_mod"] = 0
	current_turn = CombatantType.PLAYER
	emit_signal("turn_changed", current_turn)

func _end_combat(victory: bool):
	is_combat_active = false
	var gm = _get_gm()
	
	if victory:
		gm.add_experience(current_enemy.experience_reward)
		gm.add_spice(current_enemy.spice_reward)
		gm.heal(10)
	
	gm.player_data["health"] = player_stats["health"]
	gm.player_data["energy"] = player_stats["energy"]
	gm.player_data_updated.emit()
	
	emit_signal("combat_ended", victory)
	current_enemy = null

func _flee_combat():
	var flee_success = randf() > 0.4
	if flee_success:
		is_combat_active = false
		emit_signal("combat_ended", false)
	else:
		emit_signal("action_performed", CombatantType.PLAYER, "Flee Failed")
		_end_player_turn()

func get_player_actions() -> Array:
	return player_actions

func is_players_turn() -> bool:
	return current_turn == CombatantType.PLAYER and is_combat_active