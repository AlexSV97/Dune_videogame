extends CharacterBody2D

@export var move_speed: float = 180.0
var is_moving: bool = false
var facing_right: bool = true

@onready var body = $Sprite/Body

func _physics_process(_delta):
	if Input.is_action_just_pressed("build"):
		_try_build()
	
	var direction = Vector2.ZERO
	
	if Input.is_action_pressed("move_up"):
		direction.y = -1
	if Input.is_action_pressed("move_down"):
		direction.y = 1
	if Input.is_action_pressed("move_left"):
		direction.x = -1
	if Input.is_action_pressed("move_right"):
		direction.x = 1
	
	if direction != Vector2.ZERO:
		is_moving = true
		direction = direction.normalized()
		velocity = direction * move_speed
		
		if direction.x > 0:
			facing_right = true
		elif direction.x < 0:
			facing_right = false
		elif direction.y > 0:
			facing_right = true
		elif direction.y < 0:
			facing_right = false
	else:
		is_moving = false
		velocity = Vector2.ZERO
	
	_update_sprite()
	move_and_slide()

func _update_sprite():
	if facing_right:
		body.scale.x = 1.0
	else:
		body.scale.x = -1.0
	
	if is_moving:
		body.size.x = move_toward(body.size.x, 26, 1)
	else:
		body.size.x = move_toward(body.size.x, 24, 1)

func _try_build():
	var world = get_tree().get_first_node_in_group("world")
	if world:
		world.try_build_at_player()