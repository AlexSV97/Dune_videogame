extends Area2D

@export var enemy_data: Resource
@export var trigger_distance: float = 100.0

func _ready():
	body_entered.connect(_on_body_entered)

func _on_body_entered(body):
	if body.name == "Player" and enemy_data:
		var cm = get_node("/root/CombatManager")
		cm.start_combat(enemy_data)
		queue_free()