extends Resource
class_name ItemData

@export var id: String
@export var name: String
@export var description: String
@export var icon: Texture2D
@export var category: ItemCategory
@export var stack_size: int = 99
@export var value: int = 0

enum ItemCategory { RESOURCE, WEAPON, ARMOR, CONSUMABLE, QUEST_ITEM, TOOL }

func can_stack_with(other: ItemData) -> bool:
	return id == other.id and category == ItemCategory.RESOURCE