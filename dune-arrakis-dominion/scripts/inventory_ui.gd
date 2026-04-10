extends Control

@onready var grid_container = $Panel/GridContainer
@onready var item_name_label = $Panel/DetailsPanel/ItemName
@onready var item_description_label = $Panel/DetailsPanel/ItemDescription
@onready var close_button = $Panel/CloseButton

var slot_buttons: Array[Button] = []
var selected_item: Dictionary = {}

func _ready():
	_create_slots()
	visible = false
	close_button.pressed.connect(_on_close_pressed)

func _create_slots():
	for i in range(30):
		var button = Button.new()
		button.custom_minimum_size = Vector2(70, 70)
		button.pressed.connect(_on_slot_pressed.bind(i))
		grid_container.add_child(button)
		slot_buttons.append(button)

func _update_display():
	var im = get_node("/root/InventoryManager")
	var items = im.get_all_items()
	for i in range(30):
		if i < items.size():
			var slot_data = items[i]
			var item = slot_data["item"]
			var quantity = slot_data["quantity"]
			
			slot_buttons[i].text = item.name + "\n[x" + str(quantity) + "]"
			slot_buttons[i].tooltip_text = item.description
		else:
			slot_buttons[i].text = ""
			slot_buttons[i].tooltip_text = ""

func _on_slot_pressed(index: int):
	var im = get_node("/root/InventoryManager")
	var items = im.get_all_items()
	if index < items.size():
		selected_item = items[index]
		item_name_label.text = selected_item["item"].name
		item_description_label.text = selected_item["item"].description

func _on_close_pressed():
	visible = false

func toggle_inventory():
	visible = not visible
	if visible:
		_update_display()

func _input(event):
	if event.is_action_pressed("toggle_inventory"):
		toggle_inventory()
		get_viewport().set_input_as_handled()