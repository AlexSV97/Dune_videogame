extends Control

@onready var active_quests_list = $Panel/Tabs/ActiveQuests/ActiveQuestsList
@onready var completed_quests_list = $Panel/Tabs/CompletedQuests/CompletedQuestsList
@onready var close_button = $Panel/CloseButton

var quest_labels: Array[Label] = []

func _ready():
	visible = false
	close_button.pressed.connect(_on_close_pressed)
	var qm = get_node("/root/QuestManager")
	qm.quest_updated.connect(_update_quests)
	qm.quest_started.connect(_update_quests)
	qm.quest_completed.connect(_update_quests)

func _update_quests():
	_clear_quests()
	
	var qm = get_node("/root/QuestManager")
	var active = qm.get_active_quests()
	var completed = qm.get_completed_quests()
	
	for quest in active:
		var label = Label.new()
		label.text = quest.name + "\n  " + quest.get_progress_text()
		label.custom_minimum_size = Vector2(0, 50)
		active_quests_list.add_child(label)
		quest_labels.append(label)
	
	for quest_id in completed:
		var label = Label.new()
		label.text = quest_id + " (Completed)"
		label.custom_minimum_size = Vector2(0, 30)
		completed_quests_list.add_child(label)
		quest_labels.append(label)

func _clear_quests():
	for label in quest_labels:
		label.queue_free()
	quest_labels.clear()

func _on_close_pressed():
	visible = false

func toggle_quests():
	visible = not visible
	if visible:
		_update_quests()