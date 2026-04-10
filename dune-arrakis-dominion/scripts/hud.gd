extends CanvasLayer

@onready var name_label = $Panel/VBoxContainer/NameLabel
@onready var level_label = $Panel/VBoxContainer/LevelLabel
@onready var health_label = $Panel/VBoxContainer/HealthBar/HealthLabel
@onready var health_bar = $Panel/VBoxContainer/HealthBar
@onready var energy_label = $Panel/VBoxContainer/EnergyBar/EnergyLabel
@onready var energy_bar = $Panel/VBoxContainer/EnergyBar
@onready var water_label = $Panel/VBoxContainer/WaterBar/WaterLabel
@onready var water_bar = $Panel/VBoxContainer/WaterBar
@onready var spice_label = $Panel/VBoxContainer/SpiceLabel

@onready var combat_attr = $Panel/VBoxContainer/AttributesContainer/CombatAttr
@onready var diplomacy_attr = $Panel/VBoxContainer/AttributesContainer/DiplomacyAttr
@onready var stealth_attr = $Panel/VBoxContainer/AttributesContainer/StealthAttr
@onready var survival_attr = $Panel/VBoxContainer/AttributesContainer/SurvivalAttr

func _ready():
	var gm = get_node("/root/GameManager")
	gm.player_data_updated.connect(_update_hud)
	_update_hud()

func _update_hud():
	var gm = get_node("/root/GameManager")
	var p = gm.player_data
	
	name_label.text = p["name"]
	level_label.text = "Level " + str(p["level"])
	
	health_bar.max_value = p["max_health"]
	health_bar.value = p["health"]
	health_label.text = "Health: " + str(p["health"]) + "/" + str(p["max_health"])
	
	energy_bar.max_value = p["max_energy"]
	energy_bar.value = p["energy"]
	energy_label.text = "Energy: " + str(p["energy"]) + "/" + str(p["max_energy"])
	
	water_bar.max_value = p["max_water"]
	water_bar.value = p["water"]
	water_label.text = "Water: " + str(p["water"]) + "/" + str(p["max_water"])
	
	spice_label.text = "Spice: " + str(p["spice"])
	
	var attrs = p["attributes"]
	combat_attr.text = "Combat: " + str(attrs["combat"])
	diplomacy_attr.text = "  Diplomacy: " + str(attrs["diplomacy"])
	stealth_attr.text = "  Stealth: " + str(attrs["stealth"])
	survival_attr.text = "  Survival: " + str(attrs["survival"])