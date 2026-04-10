extends CanvasLayer

@export var minimap_size: Vector2 = Vector2(175, 175)
@export var tile_scale: float = 4.0
@export var show_fog: bool = true
@export var fog_color: Color = Color(0.1, 0.08, 0.05, 0.9)

enum TileType { SAND, GRAVEL, SPICE, WATER, STONE, BUILDING }

var tile_colors = {
	TileType.SAND: Color(0.76, 0.65, 0.42),
	TileType.GRAVEL: Color(0.5, 0.48, 0.45),
	TileType.SPICE: Color(0.85, 0.45, 0.1),
	TileType.WATER: Color(0.2, 0.4, 0.8),
	TileType.STONE: Color(0.6, 0.55, 0.5),
	TileType.BUILDING: Color(0.55, 0.45, 0.35)
}

@onready var minimap = $MinimapPanel/Minimap
@onready var player_dot = $MinimapPanel/PlayerDot

var world_tilemap: TileMap = null
var world: Node2D = null

func _ready():
	_setup_minimap_tiles()
	world = get_tree().get_first_node_in_group("world")
	if world:
		world_tilemap = world.get_node_or_null("TileMap")

func _process(_delta):
	if world and world_tilemap and player_dot:
		_update_player_position()

func _setup_minimap_tiles():
	var ts = TileSet.new()
	ts.tile_size = Vector2i(1, 1)
	
	for i in range(TileType.size()):
		var tile_img = Image.create(1, 1, false, Image.FORMAT_RGBA8)
		tile_img.set_pixel(0, 0, tile_colors[i])
		var tex = ImageTexture.create_from_image(tile_img)
		
		var source = TileSetAtlasSource.new()
		source.texture = tex
		source.create_tile(Vector2i(0, 0))
		ts.add_source(source)
	
	var fog_img = Image.create(1, 1, false, Image.FORMAT_RGBA8)
	fog_img.set_pixel(0, 0, fog_color)
	var fog_tex = ImageTexture.create_from_image(fog_img)
	
	var fog_source = TileSetAtlasSource.new()
	fog_source.texture = fog_tex
	fog_source.create_tile(Vector2i(0, 0))
	ts.add_source(fog_source)
	
	minimap.tile_set = ts

func _update_player_position():
	if not world or not world.player:
		return
	
	var player_pos = world.player.global_position
	var map_width = world.world_size_x * tile_scale
	var map_height = world.world_size_y * tile_scale
	
	var iso_pos = _cart_to_iso(player_pos)
	
	var screen_x = (iso_pos.x / tile_scale) + (minimap_size.x / 2.0)
	var screen_y = (iso_pos.y / tile_scale) + (minimap_size.y / 2.0)
	
	player_dot.position = Vector2(screen_x - 2, screen_y - 2)

func _cart_to_iso(cart: Vector2) -> Vector2:
	return Vector2(
		(cart.x - cart.y) * tile_scale,
		(cart.x + cart.y) * tile_scale * 0.5
	)

func is_minimap_visible() -> bool:
	return visible

func set_minimap_visible(value: bool):
	visible = value
	$MinimapPanel.visible = value