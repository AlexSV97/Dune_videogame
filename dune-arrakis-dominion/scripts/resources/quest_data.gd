extends Resource
class_name QuestData

@export var id: String
@export var name: String
@export var description: String
@export var quest_giver: String
@export var location_id: String

enum QuestType { MAIN, SIDE, TUTORIAL }
enum QuestStatus { AVAILABLE, IN_PROGRESS, COMPLETED, FAILED }

var quest_type: QuestType = QuestType.MAIN
var status: QuestStatus = QuestStatus.AVAILABLE

@export var objectives: Array[Dictionary] = []
@export var rewards: Dictionary = {
	"experience": 0,
	"spice": 0,
	"items": []
}

var current_objective_index: int = 0

func get_current_objective() -> Dictionary:
	if current_objective_index < objectives.size():
		return objectives[current_objective_index]
	return {}

func advance_objective():
	current_objective_index += 1
	if current_objective_index >= objectives.size():
		status = QuestStatus.COMPLETED
	return current_objective_index >= objectives.size()

func get_progress_text() -> String:
	if current_objective_index >= objectives.size():
		return "Completed"
	var obj = objectives[current_objective_index]
	return obj.get("description", "Unknown objective")

func is_completed() -> bool:
	return status == QuestStatus.COMPLETED