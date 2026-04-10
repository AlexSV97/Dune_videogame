extends Node2D
class_name TerrainGenerator

@export_category("Map Settings")
@export var map_width: int = 100
@export var map_height: int = 80
@export var map_seed: int = 42

@export_category("Terrain")
@export var noise_scale: float = 0.02
@export var gravel_threshold: float = 0.55
@export var gravel_density: float = 0.15

@export_category("Resources")
@export var spice_threshold: float = 0.6
@export var water_threshold: float = 0.65
@export var stone_threshold: float = 0.6
@export var resource_noise_scale: float = 0.025
@export var resource_density: float = 0.12

@export_category("Min Patch Size")
@export var min_gravel_patch: int = 400
@export var min_resource_patch: int = 400

@onready var tile_map = $TileMap
@onready var fog_tile_map = $FogTileMap
@onready var player = $Player
@onready var camera = $Camera2D

enum TileType { SAND, GRAVEL, SPICE, WATER, STONE, BUILDING }

var tile_types = {
	TileType.SAND: Vector2i(0, 0),
	TileType.GRAVEL: Vector2i(1, 0),
	TileType.SPICE: Vector2i(2, 0),
	TileType.WATER: Vector2i(3, 0),
	TileType.STONE: Vector2i(4, 0),
	TileType.BUILDING: Vector2i(5, 0)
}

var explored_tiles: Dictionary = {}
var exploration_radius: int = 5

var resource_tiles: Dictionary = {}
var collect_cooldowns: Dictionary = {}
var collect_cooldown_time: float = 1.0

var gravel_zones: Array = []
var buildings: Array = []

var enemies_container: Node2D
var enemy_spawn_timer: float = 0.0
var enemy_spawn_interval: float = 15.0

var world_size_x: int = 50
var world_size_y: int = 40

var gravel_noise: FastNoiseLite
var resource_noise: FastNoiseLite

func _ready():
	world_size_x = map_width
	world_size_y = map_height
	
	_setup_noise()
	_create_tileset()
	_create_fog_tileset()
	_generate_world_map()
	_update_fog()
	_setup_enemies()

func _setup_noise():
	gravel_noise = FastNoiseLite.new()
	gravel_noise.seed = map_seed
	gravel_noise.noise_type = FastNoiseLite.TYPE_PERLIN
	gravel_noise.frequency = noise_scale
	gravel_noise.fractal_type = FastNoiseLite.FRACTAL_FBM
	gravel_noise.fractal_octaves = 4
	
	resource_noise = FastNoiseLite.new()
	resource_noise.seed = map_seed + 1000
	resource_noise.noise_type = FastNoiseLite.TYPE_PERLIN
	resource_noise.frequency = resource_noise_scale
	resource_noise.fractal_type = FastNoiseLite.FRACTAL_FBM
	resource_noise.fractal_octaves = 3

func _create_tileset():
	var ts = TileSet.new()
	ts.tile_size = Vector2i(64, 32)
	
	var colors = {
		TileType.SAND: Color(0.76, 0.65, 0.42),
		TileType.GRAVEL: Color(0.5, 0.48, 0.45),
		TileType.SPICE: Color(0.85, 0.45, 0.1),
		TileType.WATER: Color(0.2, 0.4, 0.8),
		TileType.STONE: Color(0.6, 0.55, 0.5),
		TileType.BUILDING: Color(0.55, 0.45, 0.35)
	}
	
	for i in range(6):
		var img_size = Vector2(64, 32)
		if i == TileType.BUILDING:
			img_size = Vector2(64, 64)
		
		var img = Image.create(int(img_size.x), int(img_size.y), false, Image.FORMAT_RGBA8)
		img.fill(colors[i])
		
		if i == TileType.BUILDING:
			for px in range(8, 56):
				for py in range(8, 56):
					if px < 16 or px > 48 or py < 16 or py > 48:
						img.set_pixel(px, py, colors[i].darkened(0.2))
		
		var tex = ImageTexture.create_from_image(img)
		
		var source = TileSetAtlasSource.new()
		source.texture = tex
		source.create_tile(Vector2i(0, 0))
		ts.add_source(source)
	
	collect_cooldowns = {
		TileType.SPICE: 0.0,
		TileType.WATER: 0.0,
		TileType.STONE: 0.0
	}
	
	tile_map.tile_set = ts

func _create_fog_tileset():
	var ts = TileSet.new()
	ts.tile_size = Vector2i(64, 32)
	
	var img = Image.create(64, 32, false, Image.FORMAT_RGBA8)
	img.fill(Color(0.1, 0.08, 0.05, 0.95))
	var tex = ImageTexture.create_from_image(img)
	
	var source = TileSetAtlasSource.new()
	source.texture = tex
	source.create_tile(Vector2i(0, 0))
	ts.add_source(source)
	
	fog_tile_map.tile_set = ts

func _generate_world_map():
	var gravel_map = _generate_noise_map(gravel_noise, gravel_threshold, gravel_density)
	var resource_map = _generate_noise_map(resource_noise, resource_threshold, resource_density)
	
	_apply_terrain(gravel_map, resource_map)
	_place_initial_buildings()

func _generate_noise_map(noise: FastNoiseLite, threshold: float, density: float) -> Dictionary:
	var result = {}
	var adjusted_threshold = threshold - density
	
	for x in range(-world_size_x / 2.0, world_size_x / 2.0):
		for y in range(-world_size_y / 2.0, world_size_y / 2.0):
			var value = noise.get_noise_2d(x, y)
			if value > adjusted_threshold:
				result[Vector2i(x, y)] = value
	
	return result

func _apply_terrain(gravel_map: Dictionary, resource_map: Dictionary):
	for x in range(-world_size_x / 2.0, world_size_x / 2.0):
		for y in range(-world_size_y / 2.0, world_size_y / 2.0):
			var pos = Vector2i(x, y)
			var tile_type = TileType.SAND
			
			if pos in gravel_map:
				var connected_size = _get_connected_size(pos, gravel_map)
				if connected_size >= min_gravel_patch:
					tile_type = TileType.GRAVEL
					gravel_zones.append(pos)
			
			if tile_type == TileType.SAND and pos in resource_map:
				var connected_size = _get_connected_size(pos, resource_map)
				if connected_size >= min_resource_patch:
					var resource_value = resource_map[pos]
					if resource_value > spice_threshold:
						tile_type = TileType.SPICE
						resource_tiles[pos] = TileType.SPICE
					elif resource_value > water_threshold:
						tile_type = TileType.WATER
						resource_tiles[pos] = TileType.WATER
					elif resource_value > stone_threshold:
						tile_type = TileType.STONE
						resource_tiles[pos] = TileType.STONE
			
			tile_map.set_cell(0, pos, 0, tile_types[tile_type])
			fog_tile_map.set_cell(0, pos, 0, Vector2i(0, 0))

func _get_connected_size(start_pos: Vector2i, noise_map: Dictionary) -> int:
	var visited = {}
	var queue = [start_pos]
	var size = 0
	
	while queue.size() > 0:
		var current = queue.pop_front()
		if current in visited:
			continue
		if current not in noise_map:
			continue
		
		visited[current] = true
		size += 1
		
		var neighbors = [
			current + Vector2i(1, 0),
			current + Vector2i(-1, 0),
			current + Vector2i(0, 1),
			current + Vector2i(0, -1)
		]
		for n in neighbors:
			if n in noise_map and n not in visited:
				queue.append(n)
	
	return size

func _place_initial_buildings():
	_add_building(Vector2i(0, 0), "main_base")
	_add_building(Vector2i(2, 1), "harvester")
	_add_building(Vector2i(-2, -1), "barracks")

func _add_building(pos: Vector2i, building_type: String):
	tile_map.set_cell(0, pos, 0, tile_types[TileType.BUILDING])
	buildings.append({&"pos": pos, &"type": building_type})
	if pos in resource_tiles:
		resource_tiles.erase(pos)

func _setup_enemies():
	if not has_node("Enemies"):
		enemies_container = Node2D.new()
		enemies_container.name = "Enemies"
		add_child(enemies_container)

func _process(_delta):
	_update_cooldowns(_delta)
	_update_fog()
	_check_resource_collection()
	_spawn_enemies(_delta)
	_process_enemies(_delta)
	_check_enemy_collisions()

func _update_cooldowns(_delta):
	for key in collect_cooldowns:
		collect_cooldowns[key] = max(0, collect_cooldowns[key] - _delta)

func _update_fog():
	if not player:
		return
	
	var player_tile = tile_map.local_to_map(player.global_position)
	
	for x in range(player_tile.x - exploration_radius, player_tile.x + exploration_radius + 1):
		for y in range(player_tile.y - exploration_radius, player_tile.y + exploration_radius + 1):
			var dist = Vector2(x - player_tile.x, y - player_tile.y).length()
			if dist <= exploration_radius:
				explored_tiles[Vector2i(x, y)] = true
				fog_tile_map.set_cell(0, Vector2i(x, y), -1, Vector2i())

func _check_resource_collection():
	if not player:
		return
	
	var tile_pos = tile_map.local_to_map(player.global_position)
	
	if tile_pos in resource_tiles:
		var resource_type = resource_tiles[tile_pos]
		if collect_cooldowns[resource_type] <= 0:
			_collect_resource(resource_type)
			collect_cooldowns[resource_type] = collect_cooldown_time

func _collect_resource(type: int):
	var gm = get_node("/root/GameManager")
	
	match type:
		TileType.SPICE:
			gm.add_resource("spice", 10)
		TileType.WATER:
			gm.add_resource("water", 5)
		TileType.STONE:
			gm.add_resource("stone", 5)

func _spawn_enemies(_delta):
	enemy_spawn_timer += _delta
	
	if enemy_spawn_timer >= enemy_spawn_interval:
		enemy_spawn_timer = 0
		enemy_spawn_interval = randf_range(10.0, 20.0)
		
		if enemies_container.get_child_count() < 10:
			_spawn_single_enemy()

func _spawn_single_enemy():
	var player_tile = tile_map.local_to_map(player.global_position)
	var spawn_distance = randi_range(8, 15)
	var angle = randf() * TAU
	
	var ex = int(player_tile.x + spawn_distance * cos(angle))
	var ey = int(player_tile.y + spawn_distance * sin(angle))
	
	var enemy_pos = tile_map.map_to_local(Vector2i(ex, ey))
	
	var enemy = CharacterBody2D.new()
	enemy.name = "Enemy"
	enemy.position = enemy_pos
	
	var shape = CollisionShape2D.new()
	var shape_res = RectangleShape2D.new()
	shape_res.size = Vector2(20, 20)
	shape.shape = shape_res
	shape.position = Vector2(0, -10)
	enemy.add_child(shape)
	
	var body_rect = ColorRect.new()
	body_rect.size = Vector2(20, 24)
	body_rect.position = Vector2(-10, -24)
	body_rect.color = Color(0.8, 0.2, 0.2)
	enemy.add_child(body_rect)
	
	enemy.set_meta("enemy_type", "sand_worm")
	enemy.set_meta("health", 30)
	enemy.set_meta("damage", 10)
	enemy.set_meta("speed", 60.0)
	
	enemies_container.add_child(enemy)

func _process_enemies(_delta):
	if not enemies_container:
		return
	
	for enemy in enemies_container.get_children():
		if enemy is CharacterBody2D:
			_move_enemy(enemy, _delta)

func _move_enemy(enemy: CharacterBody2D, _delta):
	var enemy_speed = enemy.get_meta("speed", 60.0)
	var direction = (player.global_position - enemy.global_position).normalized()
	enemy.velocity = direction * enemy_speed
	enemy.move_and_slide()
	
	if direction.x > 0:
		enemy.scale.x = 1.0
	elif direction.x < 0:
		enemy.scale.x = -1.0

func _check_enemy_collisions():
	if not player:
		return
	
	var player_pos = player.global_position
	
	for enemy in enemies_container.get_children():
		if enemy is CharacterBody2D:
			var dist = player_pos.distance_to(enemy.global_position)
			if dist < 30:
				_take_damage(enemy)

func _take_damage(_enemy):
	var gm = get_node("/root/GameManager")
	gm.take_damage(5)

func try_build_at_player():
	var tile_pos = tile_map.local_to_map(player.global_position)
	
	if tile_pos in gravel_zones:
		if not tile_pos in buildings:
			_show_build_menu()
	else:
		_show_message("Solo puedes construir en zonas de grava")

func _show_build_menu():
	var gm = get_node("/root/GameManager")
	gm.open_build_menu()

func _show_message(msg: String):
	print(msg)