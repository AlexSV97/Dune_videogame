extends Resource
class_name DialogueData

@export var id: String
@export var speaker_name: String
@export var dialogue_text: String
@export var next_dialogue_id: String = ""
@export var choices: Array[Dictionary] = []
@export var condition: String = ""
@export var action: String = ""

func has_choices() -> bool:
	return choices.size() > 0