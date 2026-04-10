extends Node

signal inventory_updated()
signal item_added(item: ItemData, quantity: int)
signal item_removed(item: ItemData, quantity: int)

const MAX_SLOTS = 30

var slots: Array[Dictionary] = []

func _ready():
	for i in range(MAX_SLOTS):
		slots.append({"item": null, "quantity": 0})

func add_item(item: ItemData, quantity: int = 1) -> bool:
	if not item:
		return false
	
	var existing_slot = _find_slot(item)
	if existing_slot != -1:
		var current_qty = slots[existing_slot]["quantity"]
		var max_stack = item.stack_size if item.stack_size > 0 else 99
		var space = max_stack - current_qty
		
		if space > 0:
			var to_add = min(space, quantity)
			slots[existing_slot]["quantity"] += to_add
			inventory_updated.emit()
			item_added.emit(item, to_add)
			return true
	
	var empty_slot = _find_empty_slot()
	if empty_slot != -1:
		slots[empty_slot]["item"] = item
		slots[empty_slot]["quantity"] = quantity
		inventory_updated.emit()
		item_added.emit(item, quantity)
		return true
	
	return false

func remove_item(item: ItemData, quantity: int = 1) -> bool:
	var slot_idx = _find_slot(item)
	if slot_idx == -1:
		return false
	
	var current_qty = slots[slot_idx]["quantity"]
	if current_qty >= quantity:
		slots[slot_idx]["quantity"] -= quantity
		if slots[slot_idx]["quantity"] == 0:
			slots[slot_idx]["item"] = null
		inventory_updated.emit()
		item_removed.emit(item, quantity)
		return true
	
	return false

func get_item_count(item: ItemData) -> int:
	var slot_idx = _find_slot(item)
	if slot_idx != -1:
		return slots[slot_idx]["quantity"]
	return 0

func get_all_items() -> Array[Dictionary]:
	var result: Array[Dictionary] = []
	for slot in slots:
		if slot["item"] != null and slot["quantity"] > 0:
			result.append(slot.duplicate())
	return result

func has_item(item: ItemData) -> bool:
	return _find_slot(item) != -1

func clear_inventory():
	for i in range(MAX_SLOTS):
		slots[i] = {"item": null, "quantity": 0}
	inventory_updated.emit()

func _find_slot(item: ItemData) -> int:
	for i in range(MAX_SLOTS):
		if slots[i]["item"] != null and slots[i]["item"].id == item.id:
			return i
	return -1

func _find_empty_slot() -> int:
	for i in range(MAX_SLOTS):
		if slots[i]["item"] == null:
			return i
	return -1