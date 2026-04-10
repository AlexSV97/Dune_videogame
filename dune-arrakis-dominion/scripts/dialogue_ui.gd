extends Control

@onready var speaker_label = $Panel/SpeakerLabel
@onready var dialogue_label = $Panel/DialogueLabel
@onready var choices_container = $Panel/ChoicesContainer

var current_dialogue: DialogueData = null
var is_showing_choices: bool = false

signal dialogue_ended()
signal choice_selected(choice_index: int)

func _ready():
	visible = false

func start_dialogue(dialogue: DialogueData):
	current_dialogue = dialogue
	visible = true
	_update_dialogue()
	is_showing_choices = false
	choices_container.visible = false

func _update_dialogue():
	if current_dialogue:
		speaker_label.text = current_dialogue.speaker_name
		dialogue_label.text = current_dialogue.dialogue_text
		
		if current_dialogue.has_choices():
			_show_choices()
			is_showing_choices = true

func _show_choices():
	choices_container.visible = true
	
	for i in range(choices_container.get_child_count()):
		choices_container.get_child(i).queue_free()
	
	for i in range(current_dialogue.choices.size()):
		var button = Button.new()
		button.text = current_dialogue.choices[i].get("text", "Choice " + str(i))
		button.custom_minimum_size = Vector2(700, 30)
		button.pressed.connect(_on_choice_selected.bind(i))
		choices_container.add_child(button)

func _on_choice_selected(choice_index: int):
	choice_selected.emit(choice_index)
	
	var choice_data = current_dialogue.choices[choice_index]
	var next_id = choice_data.get("next", "")
	
	if next_id != "":
		_load_dialogue(next_id)
	else:
		_end_dialogue()

func _input(event):
	if not current_dialogue or is_showing_choices:
		return
	
	if event.is_action_pressed("ui_accept"):
		_advance_dialogue()

func _advance_dialogue():
	if current_dialogue.next_dialogue_id != "":
		_load_dialogue(current_dialogue.next_dialogue_id)
	else:
		_end_dialogue()

func _load_dialogue(dialogue_id: String):
	pass

func _end_dialogue():
	visible = false
	current_dialogue = null
	dialogue_ended.emit()