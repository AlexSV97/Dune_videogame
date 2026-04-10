extends Camera2D

@export var target: Node2D
@export var follow_speed: float = 5.0
@export var camera_offset: Vector2 = Vector2(0, -50)

func _physics_process(delta):
	if target:
		global_position = global_position.lerp(target.global_position + camera_offset, follow_speed * delta)