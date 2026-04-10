extends Node

signal location_changed(from_location: String, to_location: String)
signal locations_updated()

var available_locations: Array[Resource] = []
var current_location: String = "caladan"
var visited_locations: Array[String] = []

const LOCATION_PATH = "res://resources/locations/"

func _ready():
	_load_locations()

func _load_locations():
	var locations = [
		preload("res://resources/locations/caladan.tres"),
		preload("res://resources/locations/arrakeen.tres"),
		preload("res://resources/locations/sietch_tabr.tres"),
		preload("res://resources/locations/deep_desert.tres")
	]
	
	for loc in locations:
		if loc:
			available_locations.append(loc)
	
	visited_locations.append(current_location)
	locations_updated.emit()

func get_available_locations() -> Array:
	var result: Array[Resource] = []
	var gm = get_node("/root/GameManager")
	var player_level = gm.player_data["level"]
	for loc in available_locations:
		if loc.is_accessible(player_level):
			result.append(loc)
	return result

func get_all_locations() -> Array:
	return available_locations

func travel_to(location_id: String) -> bool:
	var gm = get_node("/root/GameManager")
	var player_level = gm.player_data["level"]
	for loc in available_locations:
		if loc.id == location_id:
			if loc.is_accessible(player_level):
				var from = current_location
				current_location = location_id
				
				if not location_id in visited_locations:
					visited_locations.append(location_id)
				
				gm.current_location = location_id
				location_changed.emit(from, location_id)
				locations_updated.emit()
				return true
			else:
				return false
	return false

func unlock_location(location_id: String) -> void:
	for loc in available_locations:
		if loc.id == location_id:
			loc.is_unlocked = true
			locations_updated.emit()

func is_location_unlocked(location_id: String) -> bool:
	for loc in available_locations:
		if loc.id == location_id:
			return loc.is_unlocked
	return false

func get_current_location() -> Resource:
	for loc in available_locations:
		if loc.id == current_location:
			return loc
	return null