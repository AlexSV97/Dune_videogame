extends Node

signal quest_updated(quest: QuestData)
signal quest_completed(quest: QuestData)
signal quest_started(quest: QuestData)

var available_quests: Array[QuestData] = []
var active_quests: Array[QuestData] = []
var completed_quests: Array[String] = []

func _ready():
	_load_quests()

func _load_quests():
	var quest_resources = [
		preload("res://resources/quests/tutorial_1.tres"),
		preload("res://resources/quests/fremen_1.tres"),
		preload("res://resources/quests/arrakeen_1.tres")
	]
	
	for quest in quest_resources:
		if quest:
			available_quests.append(quest)

func get_quests_for_location(location_id: String) -> Array[QuestData]:
	var result: Array[QuestData] = []
	
	for quest in available_quests:
		if quest.location_id == location_id:
			if not _is_quest_active(quest.id) and not _is_quest_completed(quest.id):
				result.append(quest)
	
	for quest in active_quests:
		if quest.location_id == location_id:
			result.append(quest)
	
	return result

func start_quest(quest_id: String) -> bool:
	for quest in available_quests:
		if quest.id == quest_id:
			if quest.status == QuestData.QuestStatus.AVAILABLE:
				quest.status = QuestData.QuestStatus.IN_PROGRESS
				active_quests.append(quest)
				quest_started.emit(quest)
				return true
	return false

func complete_objective(quest_id: String, objective_type: String, target: String) -> void:
	for quest in active_quests:
		if quest.id == quest_id:
			var current_obj = quest.get_current_objective()
			if current_obj.get("type") == objective_type and current_obj.get("target") == target:
				var is_complete = quest.advance_objective()
				quest_updated.emit(quest)
				
				if is_complete:
					_complete_quest(quest)
				return

func _complete_quest(quest: QuestData):
	quest.status = QuestData.QuestStatus.COMPLETED
	active_quests.erase(quest)
	completed_quests.append(quest.id)
	
	var gm = get_node("/root/GameManager")
	gm.add_experience(quest.rewards.get("experience", 0))
	gm.add_spice(quest.rewards.get("spice", 0))
	
	quest_completed.emit(quest)

func _is_quest_active(quest_id: String) -> bool:
	for quest in active_quests:
		if quest.id == quest_id:
			return true
	return false

func _is_quest_completed(quest_id: String) -> bool:
	return quest_id in completed_quests

func get_active_quests() -> Array[QuestData]:
	return active_quests

func get_completed_quests() -> Array[String]:
	return completed_quests