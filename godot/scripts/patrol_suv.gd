extends VehicleBody3D
class_name PatrolSUVController

@export var engine_power := 1850.0
@export var reverse_power := 1050.0
@export var brake_power := 38.0
@export var max_steering := 0.48
@export var steering_response := 2.8

var vehicle_camera: Camera3D
var light_left: MeshInstance3D
var light_right: MeshInstance3D
var siren_active := false

func _ready() -> void:
	mass = 1550.0
	center_of_mass = Vector3(0, -0.35, 0.05)
	_build_body()
	_build_wheels()
	_build_camera()
	_build_lightbar()

func _physics_process(delta: float) -> void:
	var throttle := Input.get_action_strength("drive_forward") - Input.get_action_strength("drive_back")
	var steer_input := Input.get_action_strength("drive_right") - Input.get_action_strength("drive_left")
	var speed_kph := linear_velocity.length() * 3.6
	var target_steering := steer_input * max_steering
	steering = move_toward(steering, target_steering, steering_response * delta)
	if throttle > 0.0:
		engine_force = throttle * engine_power
	elif throttle < 0.0 and speed_kph > 4.0:
		engine_force = 0.0
		brake = brake_power * absf(throttle)
	else:
		engine_force = throttle * reverse_power
		brake = 0.0
	if Input.is_action_pressed("vehicle_brake"):
		brake = brake_power
	elif absf(throttle) > 0.05 and not (throttle < 0.0 and speed_kph > 4.0):
		brake = 0.0
	if speed_kph > 105.0 and throttle > 0.0:
		engine_force *= 0.35
	_update_lightbar()

func _build_body() -> void:
	var paint := _mat(Color(0.83, 0.84, 0.82), 0.28, 0.35)
	var dark := _mat(Color(0.035, 0.055, 0.075), 0.22)
	var glass := _mat(Color(0.045, 0.11, 0.15), 0.16, 0.45)
	var trim := _mat(Color(0.12, 0.13, 0.14), 0.42)
	_add_box("Body", Vector3(0, 0.78, 0), Vector3(2.0, 0.72, 4.45), paint)
	_add_box("Cabin", Vector3(0, 1.42, -0.18), Vector3(1.78, 0.85, 2.35), paint)
	_add_box("Windshield", Vector3(0, 1.47, -1.37), Vector3(1.58, 0.65, 0.06), glass)
	_add_box("RearGlass", Vector3(0, 1.47, 1.02), Vector3(1.58, 0.62, 0.06), glass)
	for side in [-1.0, 1.0]:
		_add_box("SideGlass", Vector3(side * 0.91, 1.48, -0.23), Vector3(0.045, 0.58, 1.78), glass)
		_add_box("RunningBoard", Vector3(side * 1.02, 0.48, 0), Vector3(0.12, 0.13, 2.5), trim)
		_add_box("Mirror", Vector3(side * 1.02, 1.35, -1.13), Vector3(0.2, 0.16, 0.28), trim)
	_add_box("FrontBumper", Vector3(0, 0.52, -2.28), Vector3(2.05, 0.25, 0.18), trim)
	_add_box("RearBumper", Vector3(0, 0.52, 2.28), Vector3(2.05, 0.25, 0.18), trim)
	_add_box("Grille", Vector3(0, 0.9, -2.255), Vector3(0.9, 0.34, 0.035), dark)
	for side in [-1.0, 1.0]:
		_add_box("Headlamp", Vector3(side * 0.68, 1.0, -2.27), Vector3(0.42, 0.2, 0.045), _mat(Color(0.82,0.9,1.0),0.18,0.1))
		_add_box("TailLamp", Vector3(side * 0.72, 0.98, 2.27), Vector3(0.32, 0.3, 0.05), _mat(Color(0.55,0.025,0.02),0.2,0.15))
	_add_box("SheriffDoorBadge", Vector3(1.012, 0.92, -0.12), Vector3(0.025, 0.42, 0.72), _mat(Color(0.08,0.13,0.17),0.55))
	var collision := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = Vector3(1.85, 1.15, 4.15)
	collision.shape = shape
	collision.position = Vector3(0,0.88,0)
	add_child(collision)

func _build_wheels() -> void:
	for axle in [-1.32, 1.32]:
		for side in [-0.91, 0.91]:
			var wheel := VehicleWheel3D.new()
			wheel.name = "Wheel_%s_%s" % [str(side), str(axle)]
			wheel.position = Vector3(side, 0.43, axle)
			wheel.wheel_radius = 0.38
			wheel.wheel_rest_length = 0.18
			wheel.suspension_travel = 0.16
			wheel.suspension_stiffness = 24.0
			wheel.damping_compression = 3.2
			wheel.damping_relaxation = 4.2
			wheel.wheel_friction_slip = 3.0
			wheel.use_as_traction = true
			wheel.use_as_steering = axle < 0.0
			add_child(wheel)
			var wheel_mesh := MeshInstance3D.new()
			var cylinder := CylinderMesh.new()
			cylinder.top_radius = 0.36
			cylinder.bottom_radius = 0.36
			cylinder.height = 0.22
			wheel_mesh.mesh = cylinder
			wheel_mesh.rotation.z = PI / 2.0
			wheel_mesh.material_override = _mat(Color(0.025,0.025,0.028),0.8)
			wheel.add_child(wheel_mesh)

func _build_camera() -> void:
	vehicle_camera = Camera3D.new()
	vehicle_camera.position = Vector3(0, 3.15, 7.3)
	vehicle_camera.fov = 72.0
	vehicle_camera.look_at(Vector3(0, 1.0, 0))
	vehicle_camera.current = false
	add_child(vehicle_camera)

func _build_lightbar() -> void:
	_add_box("LightbarBase", Vector3(0, 1.92, -0.15), Vector3(1.35, 0.12, 0.32), _mat(Color(0.035,0.04,0.05),0.3))
	light_left = _add_box("RedEmergencyLight", Vector3(-0.42, 2.04, -0.15), Vector3(0.52,0.15,0.28), _mat(Color(0.9,0.015,0.01),0.2,0.6))
	light_right = _add_box("BlueEmergencyLight", Vector3(0.42, 2.04, -0.15), Vector3(0.52,0.15,0.28), _mat(Color(0.01,0.08,0.9),0.2,0.6))

func _update_lightbar() -> void:
	if not siren_active:
		light_left.visible = false
		light_right.visible = false
		return
	var phase := int(Time.get_ticks_msec() / 140) % 2
	light_left.visible = phase == 0
	light_right.visible = phase == 1

func toggle_lights() -> void:
	siren_active = not siren_active

func _mat(color: Color, roughness := 0.8, metallic := 0.0) -> StandardMaterial3D:
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	material.roughness = roughness
	material.metallic = metallic
	return material

func _add_box(node_name: String, pos: Vector3, size: Vector3, material: Material, add_collision := false) -> MeshInstance3D:
	var mesh_instance := MeshInstance3D.new()
	mesh_instance.name = node_name
	var box := BoxMesh.new()
	box.size = size
	mesh_instance.mesh = box
	mesh_instance.material_override = material
	mesh_instance.position = pos
	add_child(mesh_instance)
	return mesh_instance
