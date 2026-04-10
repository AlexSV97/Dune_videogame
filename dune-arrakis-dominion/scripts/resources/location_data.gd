extends Resource
class_name LocationData

@export var id: String
@export var name: String
@export var description: String
@export var scene_path: String
@export var is_unlocked: bool = false
@export var required_level: int = 1
@export var available_quests: Array[String] = []
@export var enemies: Array[Resource] = []
@export var items: Array[Resource] = []
@export var is_dangerous: bool = false

enum LocationType { HOME, CITY, SIETCH, DESERT, SPECIAL }

var location_type: LocationType = LocationType.DESERT

func is_accessible(player_level: int) -> bool:
	return is_unlocked and player_level >= required_level