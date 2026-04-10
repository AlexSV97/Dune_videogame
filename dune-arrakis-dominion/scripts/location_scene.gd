extends Node2D

@export var location_name: String = "Unknown Location"
@export var location_description: String = ""

@onready var tile_map = $TileMap
@onready var player = $Player
@onready var camera = $Camera2D

enum TileType { SAND, ROCK, SPICE, BUILDING, WATER }

var tile_types = {
	TileType.SAND: Vector2i(0, 0),
	TileType.ROCK: Vector2i(1, 0),
	TileType.SPICE: Vector2i(2, 0),
	TileType.BUILDING: Vector2i(3, 0),
	TileType.WATER: Vector2i(1, 1)
}

func _ready():
	_generate_world_map()

func _generate_world_map():
	for x in range(-20, 20):
		for y in range(-20, 20):
			var tile = _get_tile_type(x, y)
			tile_map.set_cell(0, Vector2i(x, y), 0, tile_types[tile])
	
	_place_buildings()

func _get_tile_type(x: int, y: int) -> TileType:
	var dist_from_center = Vector2(x, y).length()
	var random_val = randf()
	
	if dist_from_center < 3:
		return TileType.SAND
	elif dist_from_center < 8:
		if random_val < 0.1:
			return TileType.SPICE
		elif random_val < 0.15:
			return TileType.ROCK
		return TileType.SAND
	else:
		if random_val < 0.2:
			return TileType.SPICE
		elif random_val < 0.35:
			return TileType.ROCK
		return TileType.SAND

func _place_buildings():
	var buildings = [
		Vector2i(0, 0),
		Vector2i(2, -2),
		Vector2i(-2, 2),
		Vector2i(3, 3)
	]
	for pos in buildings:
		tile_map.set_cell(0, pos, 0, tile_types[TileType.BUILDING])

func _process(delta):
	_check_interactions()

func _check_interactions():
	if not player:
		return
	
	var player_pos = player.global_position
	var tile_pos = tile_map.local_to_map(player_pos)
	var atlas_coord = tile_map.get_cell_atlas_coords(0, tile_pos)
	
	if atlas_coord == Vector2i(2, 0):
		_collect_spice(tile_pos)

func _collect_spice(tile_pos: Vector2i):
	var gm = get_node("/root/GameManager")
	gm.add_spice(10)
	tile_map.set_cell(0, tile_pos, 0, tile_types[TileType.SAND])