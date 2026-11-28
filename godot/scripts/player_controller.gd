extends CharacterBody3D
class_name DeputyController

@export var walk_speed := 4.2
@export var sprint_speed := 6.5
@export var acceleration := 18.0
@export var look_sensitivity := 0.003
var camera_pitch := -0.18
var look_touch_id := -1
var camera: Camera3D

func _ready() -> void:
	var capsule := CapsuleMesh.new()
	capsule.radius = 0.34
	capsule.height = 1.8
	var visual := MeshInstance3D.new()
	visual.mesh = capsule
	visual.position.y = 0.9
	var material := StandardMaterial3D.new()
	material.albedo_color = Color(0.08, 0.13, 0.19)
	visual.material_override = material
	add_child(visual)
	var collider := CollisionShape3D.new()
	var shape := CapsuleShape3D.new()
	shape.radius = 0.34
	shape.height = 1.8
	collider.shape = shape
	collider.position.y = 0.9
	add_child(collider)
	var pivot := Node3D.new()
	pivot.position.y = 1.55
	add_child(pivot)
	camera = Camera3D.new()
	camera.position = Vector3(0, 0.2, 5.2)
	camera.rotation.x = camera_pitch
	camera.current = true
	pivot.add_child(camera)

func _physics_process(delta: float) -> void:
	var input2 := Input.get_vector("move_left", "move_right", "move_forward", "move_back")
	var direction := (transform.basis * Vector3(input2.x, 0, input2.y)).normalized()
	var speed := sprint_speed if Input.is_action_pressed("sprint") else walk_speed
	if direction:
		velocity.x = move_toward(velocity.x, direction.x * speed, acceleration * delta)
		velocity.z = move_toward(velocity.z, direction.z * speed, acceleration * delta)
	else:
		velocity.x = move_toward(velocity.x, 0.0, acceleration * delta)
		velocity.z = move_toward(velocity.z, 0.0, acceleration * delta)
	if not is_on_floor():
		velocity.y -= 9.8 * delta
	else:
		velocity.y = -0.1
	move_and_slide()

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventScreenTouch:
		if event.pressed and event.position.x > get_viewport().get_visible_rect().size.x * 0.42:
			look_touch_id = event.index
		elif not event.pressed and event.index == look_touch_id:
			look_touch_id = -1
	elif event is InputEventScreenDrag and event.index == look_touch_id:
		rotate_y(-event.relative.x * look_sensitivity)
		camera_pitch = clamp(camera_pitch - event.relative.y * look_sensitivity, -0.85, 0.25)
		camera.rotation.x = camera_pitch
