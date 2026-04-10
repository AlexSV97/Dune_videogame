extends Resource
class_name EnemyData

@export var id: String
@export var name: String
@export var description: String
@export var health: int
@export var energy: int
@export var attack: int
@export var defense: int
@export var speed: int
@export var experience_reward: int
@export var spice_reward: int
@export var sprite: Texture2D

enum EnemyType { SANDWORM, SARDUKAR, FREMEN, HARSH, CREEP }

var enemy_type: EnemyType = EnemyType.SANDWORM

func get_actions() -> Array:
	var actions = []
	match enemy_type:
		EnemyType.SANDWORM:
			actions.append({"name": "Bite", "damage": 20, "cost": 0})
			actions.append({"name": "Devour", "damage": 35, "cost": 10})
		EnemyType.SARDUKAR:
			actions.append({"name": "Strike", "damage": 15, "cost": 0})
			actions.append({"name": "Blade Storm", "damage": 25, "cost": 15})
			actions.append({"name": "Shield Block", "damage": 0, "cost": 5})
		EnemyType.FREMEN:
			actions.append({"name": "Crysknife Slash", "damage": 18, "cost": 0})
			actions.append({"name": "Desert Strike", "damage": 22, "cost": 8})
			actions.append({"name": "Call Sandworm", "damage": 30, "cost": 20})
		EnemyType.HARSH:
			actions.append({"name": "Acid Spit", "damage": 12, "cost": 0})
			actions.append({"name": "Paralyzing toxin", "damage": 5, "cost": 5})
		EnemyType.CREEP:
			actions.append({"name": "Claw", "damage": 10, "cost": 0})
			actions.append({"name": "Swarm", "damage": 15, "cost": 5})
	return actions