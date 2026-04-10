extends Area2D

@export var item_data: Resource
@export var quantity: int = 1

var collected: bool = false

func _ready():
	body_entered.connect(_on_body_entered)

func _on_body_entered(body):
	if body.name == "Player" and not collected:
		_try_pickup(body)

func _try_pickup(player):
	if item_data:
		var im = get_node("/root/InventoryManager")
		if im.add_item(item_data, quantity):
			collected = true
			queue_free()