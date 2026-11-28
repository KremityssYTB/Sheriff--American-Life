extends Node3D

const DeputyController = preload("res://scripts/player_controller.gd")
const PatrolSUVController = preload("res://scripts/patrol_suv.gd")
const DispatchManager = preload("res://scripts/dispatch_manager.gd")
var player: CharacterBody3D
var patrol_suv: VehicleBody3D
var in_vehicle := false
var status_label: Label
var foot_controls: Array[Button] = []
var vehicle_controls: Array[Button] = []
var enter_button: Button
var lights_button: Button
var vehicle_look_touch_id := -1
var dispatch_manager: Node
var dispatch_summary: Label
var dispatch_state_label: Label
var dispatch_distance_label: Label
var dispatch_request_button: Button
var dispatch_accept_button: Button
var dispatch_clear_button: Button
var dispatch_marker: Node3D

func _ready() -> void:
	# Supersample the desktop Forward+ path; keep iOS at native resolution.
	get_viewport().scaling_3d_scale = 1.0 if OS.has_feature("mobile") else 1.25
	_setup_input()
	_build_environment()
	_build_district()
	_spawn_player()
	_spawn_patrol_suv()
	dispatch_manager = DispatchManager.new()
	dispatch_manager.name = "DispatchManager"
	dispatch_manager.state_changed.connect(_refresh_dispatch_ui)
	add_child(dispatch_manager)
	_build_touch_hud()

func _setup_input() -> void:
	_add_key("move_forward", KEY_W)
	_add_key("move_back", KEY_S)
	_add_key("move_left", KEY_A)
	_add_key("move_right", KEY_D)
	_add_key("sprint", KEY_SHIFT)
	_add_key("drive_forward", KEY_W)
	_add_key("drive_back", KEY_S)
	_add_key("drive_left", KEY_A)
	_add_key("drive_right", KEY_D)
	_add_key("vehicle_brake", KEY_SPACE)
	_add_key("interact", KEY_E)
	_add_key("toggle_lights", KEY_L)

func _add_key(action: String, key: Key) -> void:
	if not InputMap.has_action(action):
		InputMap.add_action(action)
	var event := InputEventKey.new()
	event.physical_keycode = key
	InputMap.action_add_event(action, event)

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("interact"):
		_toggle_vehicle()
	elif event.is_action_pressed("toggle_lights") and in_vehicle:
		patrol_suv.toggle_lights()
	if in_vehicle and event is InputEventScreenTouch:
		if event.pressed and event.position.x > get_viewport().get_visible_rect().size.x * 0.42:
			vehicle_look_touch_id = event.index
		elif not event.pressed and event.index == vehicle_look_touch_id:
			vehicle_look_touch_id = -1
	elif in_vehicle and event is InputEventScreenDrag and event.index == vehicle_look_touch_id:
		patrol_suv.vehicle_camera.rotation.y = clamp(patrol_suv.vehicle_camera.rotation.y - event.relative.x * 0.0025, -0.9, 0.9)
		patrol_suv.vehicle_camera.rotation.x = clamp(patrol_suv.vehicle_camera.rotation.x - event.relative.y * 0.002, -0.55, 0.1)

func _process(_delta: float) -> void:
	if not in_vehicle and is_instance_valid(player) and is_instance_valid(patrol_suv):
		var near_vehicle := player.global_position.distance_to(patrol_suv.global_position) < 4.2
		enter_button.visible = near_vehicle
		if near_vehicle:
			status_label.text = "ON FOOT  •  Patrol SUV nearby — press E / ENTER VEHICLE"
		else:
			status_label.text = "ON FOOT  •  WASD move  •  drag right side to look"
	elif in_vehicle:
		var speed := patrol_suv.linear_velocity.length() * 3.6
		status_label.text = "IN PATROL SUV  •  %.0f km/h  •  WASD drive  •  SPACE brake  •  L lights" % speed
	if is_instance_valid(dispatch_manager) and dispatch_manager.get_state_label() == "UNIT EN ROUTE":
		var unit_position: Vector3 = patrol_suv.global_position if in_vehicle else player.global_position
		var distance := unit_position.distance_to(dispatch_manager.get_destination())
		dispatch_distance_label.text = "Destination: %.0f m away" % distance

func _material(color: Color, roughness := 0.9) -> StandardMaterial3D:
	var mat := StandardMaterial3D.new()
	mat.albedo_color = color
	mat.roughness = roughness
	return mat

func _add_box(parent: Node3D, name: String, pos: Vector3, size: Vector3, mat: Material, collision := false) -> void:
	var body: Node3D = StaticBody3D.new() if collision else Node3D.new()
	body.name = name
	body.position = pos
	parent.add_child(body)
	var mesh := MeshInstance3D.new()
	var box := BoxMesh.new()
	box.size = size
	mesh.mesh = box
	mesh.material_override = mat
	body.add_child(mesh)
	if collision:
		var shape_node := CollisionShape3D.new()
		var shape := BoxShape3D.new()
		shape.size = size
		shape_node.shape = shape
		body.add_child(shape_node)

func _build_environment() -> void:
	var world := WorldEnvironment.new()
	var env := Environment.new()
	env.background_mode = Environment.BG_SKY
	var sky := Sky.new()
	var sky_mat := ProceduralSkyMaterial.new()
	sky_mat.sky_top_color = Color(0.12, 0.28, 0.48)
	sky_mat.sky_horizon_color = Color(0.72, 0.79, 0.83)
	sky.sky_material = sky_mat
	env.sky = sky
	env.ambient_light_source = Environment.AMBIENT_SOURCE_SKY
	env.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	# High-end desktop effects; keep the iOS/mobile path lightweight.
	if not OS.has_feature("mobile"):
		# High-end Forward+ lighting for desktop builds.
		env.sdfgi_enabled = true
		env.sdfgi_energy = 1.0
		env.volumetric_fog_enabled = true
		env.volumetric_fog_density = 0.012
		env.volumetric_fog_albedo = Color(0.72, 0.78, 0.84)
	world.environment = env
	add_child(world)
	var sun := DirectionalLight3D.new()
	sun.rotation_degrees = Vector3(-48, -32, 0)
	sun.light_energy = 1.25
	sun.shadow_enabled = true
	add_child(sun)

func _build_district() -> void:
	var grass := _material(Color(0.20, 0.31, 0.17))
	var asphalt := _material(Color(0.12, 0.13, 0.14))
	var concrete := _material(Color(0.48, 0.49, 0.46))
	var white := _material(Color(0.86, 0.86, 0.79))
	var glass := _material(Color(0.12, 0.23, 0.29), 0.22)
	var facade_colors := [Color(0.67,0.61,0.51), Color(0.73,0.72,0.66), Color(0.55,0.59,0.57), Color(0.75,0.68,0.56)]
	_add_box(self, "Ground", Vector3(0,-0.35,0), Vector3(240,0.6,240), grass, true)
	_add_box(self, "MainRoad", Vector3(0,0,0), Vector3(16,0.12,220), asphalt)
	_add_box(self, "CrossRoad", Vector3(0,0,0), Vector3(220,0.13,14), asphalt)
	for z in range(-100, 101, 12):
		_add_box(self, "CenterLine", Vector3(0,0.075,z), Vector3(0.22,0.025,5), white)
	for x in range(-96, 97, 12):
		_add_box(self, "CrossCenterLine", Vector3(x,0.08,0), Vector3(5,0.025,0.2), white)
	for side in [-1, 1]:
		_add_box(self, "Sidewalk", Vector3(side*10,0.12,0), Vector3(3,0.3,220), concrete)
		_add_box(self, "Sidewalk", Vector3(0,0.12,side*9), Vector3(220,0.3,3), concrete)
	var index := 0
	for z in range(-84, 85, 28):
		for side in [-1, 1]:
			var h := 8.0 + float((index * 7) % 15)
			var w := 13.0 + float((index * 3) % 8)
			var depth := 15.0
			var color: Color = facade_colors[index % facade_colors.size()]
			_add_box(self, "OcalaBuilding_%02d" % index, Vector3(side * 28, h/2.0, z), Vector3(w,h,depth), _material(color), true)
			_add_box(self, "FacadeGlass_%02d" % index, Vector3(side * 28, h*0.56, z-depth*0.501), Vector3(w*0.68,h*0.32,0.12), glass)
			_add_box(self, "RoofCap_%02d" % index, Vector3(side * 28,h+0.18,z), Vector3(w+0.4,0.35,depth+0.4), concrete)
			index += 1
	_add_box(self, "MarionSheriffOffice", Vector3(-29,3.2,-18), Vector3(23,6.4,19), _material(Color(0.65,0.66,0.63)), true)
	_add_box(self, "SheriffOfficeWindows", Vector3(-29,3.3,-27.58), Vector3(17,2.2,0.12), glass)
	_add_box(self, "SheriffSign", Vector3(-29,5.4,-27.7), Vector3(8,0.65,0.18), _material(Color(0.08,0.12,0.16)))
	for i in range(18):
		var x := -55.0 + float((i*17)%110)
		var z := -90.0 + float((i*23)%180)
		_add_box(self, "PalmTrunk", Vector3(x,3,z), Vector3(0.45,6,0.45), _material(Color(0.34,0.23,0.13)))
		_add_box(self, "PalmCanopy", Vector3(x,6.3,z), Vector3(4.2,0.35,2.1), _material(Color(0.13,0.29,0.12)))

func _spawn_player() -> void:
	player = DeputyController.new()
	player.name = "Deputy"
	player.position = Vector3(-1.5,0.2,12)
	add_child(player)

func _spawn_patrol_suv() -> void:
	patrol_suv = PatrolSUVController.new()
	patrol_suv.name = "MarionCountyPatrolSUV"
	patrol_suv.position = Vector3(2.2,0.0,12)
	patrol_suv.rotation.y = PI
	add_child(patrol_suv)

func _toggle_vehicle() -> void:
	if not in_vehicle:
		if player.global_position.distance_to(patrol_suv.global_position) > 4.2:
			return
		in_vehicle = true
		player.hide()
		player.set_physics_process(false)
		player.camera.current = false
		patrol_suv.vehicle_camera.current = true
		for button in foot_controls:
			button.hide()
		for button in vehicle_controls:
			button.show()
		enter_button.text = "EXIT VEHICLE"
		enter_button.show()
		lights_button.show()
	else:
		if patrol_suv.linear_velocity.length() > 1.5:
			status_label.text = "Slow down before exiting."
			return
		in_vehicle = false
		player.global_position = patrol_suv.global_position + patrol_suv.global_transform.basis.x * 2.2 + Vector3.UP * 0.2
		player.rotation.y = patrol_suv.rotation.y
		patrol_suv.vehicle_camera.current = false
		player.camera.current = true
		player.show()
		player.set_physics_process(true)
		for button in foot_controls:
			button.show()
		for button in vehicle_controls:
			button.hide()
		enter_button.text = "ENTER VEHICLE"
		enter_button.hide()
		lights_button.hide()

func _build_touch_hud() -> void:
	var layer := CanvasLayer.new()
	add_child(layer)
	var title := Label.new()
	title.text = "SHERIFF: AMERICAN LIFE  |  OCALA DISTRICT TEST"
	title.position = Vector2(24,18)
	title.add_theme_font_size_override("font_size", 22)
	layer.add_child(title)
	status_label = Label.new()
	status_label.text = "ON FOOT  •  WASD move  •  drag right side to look"
	status_label.position = Vector2(24,52)
	layer.add_child(status_label)
	var foot := [
		["UP","move_forward",Vector2(112,560)],
		["DOWN","move_back",Vector2(112,660)],
		["LEFT","move_left",Vector2(12,610)],
		["RIGHT","move_right",Vector2(212,610)],
		["RUN","sprint",Vector2(250,550)]
	]
	for item in foot:
		var button := TouchButton.new()
		button.text = item[0]
		button.position = item[2]
		button.size = Vector2(88,76)
		button.action_name = item[1]
		layer.add_child(button)
		foot_controls.append(button)
	var drive := [
		["GAS","drive_forward",Vector2(1080,560)],
		["REVERSE","drive_back",Vector2(1080,660)],
		["LEFT","drive_left",Vector2(880,610)],
		["RIGHT","drive_right",Vector2(1280-180,610)],
		["BRAKE","vehicle_brake",Vector2(1080,460)]
	]
	for item in drive:
		var button := TouchButton.new()
		button.text = item[0]
		button.position = item[2]
		button.size = Vector2(88,76)
		button.action_name = item[1]
		layer.add_child(button)
		vehicle_controls.append(button)
		button.hide()
	enter_button = Button.new()
	enter_button.text = "ENTER VEHICLE"
	enter_button.position = Vector2(24,100)
	enter_button.size = Vector2(210,64)
	enter_button.visible = false
	enter_button.pressed.connect(_toggle_vehicle)
	layer.add_child(enter_button)
	lights_button = Button.new()
	lights_button.text = "LIGHTS / SIREN"
	lights_button.position = Vector2(24,174)
	lights_button.size = Vector2(210,58)
	lights_button.visible = false
	lights_button.pressed.connect(func(): patrol_suv.toggle_lights())
	layer.add_child(lights_button)
	_build_dispatch_panel(layer)

class TouchButton:
	extends Button
	var action_name := ""
	func _ready() -> void:
		modulate = Color(1,1,1,0.78)
		add_theme_font_size_override("font_size", 16)
		button_down.connect(func(): Input.action_press(action_name))
		button_up.connect(func(): Input.action_release(action_name))

func _build_dispatch_panel(layer: CanvasLayer) -> void:
	var panel := PanelContainer.new()
	panel.name = "DispatchPanel"
	panel.set_anchors_preset(Control.PRESET_TOP_RIGHT)
	panel.offset_left = -352
	panel.offset_right = -20
	panel.offset_top = 18
	panel.offset_bottom = 254
	var panel_style := StyleBoxFlat.new()
	panel_style.bg_color = Color(0.035, 0.065, 0.09, 0.94)
	panel_style.border_color = Color(0.34, 0.48, 0.58, 0.9)
	panel_style.set_border_width_all(1)
	panel_style.set_corner_radius_all(12)
	panel_style.content_margin_left = 14
	panel_style.content_margin_right = 14
	panel_style.content_margin_top = 12
	panel_style.content_margin_bottom = 12
	panel.add_theme_stylebox_override("panel", panel_style)
	layer.add_child(panel)
	var content := VBoxContainer.new()
	content.add_theme_constant_override("separation", 7)
	panel.add_child(content)
	var heading := Label.new()
	heading.text = "MARION COUNTY  |  DISPATCH"
	heading.add_theme_font_size_override("font_size", 15)
	heading.add_theme_color_override("font_color", Color(0.78, 0.87, 0.94))
	content.add_child(heading)
	dispatch_state_label = Label.new()
	dispatch_state_label.text = "AVAILABLE FOR CALLS"
	dispatch_state_label.add_theme_font_size_override("font_size", 13)
	dispatch_state_label.add_theme_color_override("font_color", Color(0.42, 0.83, 0.68))
	content.add_child(dispatch_state_label)
	dispatch_summary = Label.new()
	dispatch_summary.text = "No active call. Request a dispatch assignment when ready."
	dispatch_summary.custom_minimum_size = Vector2(290, 82)
	dispatch_summary.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	dispatch_summary.add_theme_font_size_override("font_size", 12)
	content.add_child(dispatch_summary)
	dispatch_distance_label = Label.new()
	dispatch_distance_label.text = "Unit status: Available"
	dispatch_distance_label.add_theme_font_size_override("font_size", 12)
	content.add_child(dispatch_distance_label)
	var actions := HBoxContainer.new()
	actions.add_theme_constant_override("separation", 6)
	content.add_child(actions)
	dispatch_request_button = _dispatch_button("REQUEST", _offer_dispatch_call)
	dispatch_accept_button = _dispatch_button("ACCEPT", _accept_dispatch_call)
	dispatch_clear_button = _dispatch_button("CLEAR", _complete_dispatch_call)
	actions.add_child(dispatch_request_button)
	actions.add_child(dispatch_accept_button)
	actions.add_child(dispatch_clear_button)
	_refresh_dispatch_ui()

func _dispatch_button(caption: String, callback: Callable) -> Button:
	var button := Button.new()
	button.text = caption
	button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	button.custom_minimum_size = Vector2(0, 38)
	button.add_theme_font_size_override("font_size", 12)
	button.add_theme_color_override("font_color", Color(0.94, 0.97, 1.0))
	button.pressed.connect(callback)
	return button

func _offer_dispatch_call() -> void:
	if dispatch_manager.offer_call():
		dispatch_distance_label.text = "Review the call and accept when ready."

func _accept_dispatch_call() -> void:
	if dispatch_manager.accept_call():
		_create_dispatch_marker(dispatch_manager.get_destination())
		_refresh_dispatch_ui()

func _complete_dispatch_call() -> void:
	var unit_position: Vector3 = patrol_suv.global_position if in_vehicle else player.global_position
	if unit_position.distance_to(dispatch_manager.get_destination()) > 10.0:
		dispatch_distance_label.text = "Move within 10 m of the call location to clear it."
		return
	if dispatch_manager.clear_call():
		if is_instance_valid(dispatch_marker):
			dispatch_marker.queue_free()
		dispatch_marker = null
		dispatch_distance_label.text = "Report complete. Unit available."
		_refresh_dispatch_ui()

func _create_dispatch_marker(destination: Vector3) -> void:
	if is_instance_valid(dispatch_marker):
		dispatch_marker.queue_free()
	dispatch_marker = Node3D.new()
	dispatch_marker.name = "DispatchDestinationMarker"
	dispatch_marker.position = destination
	add_child(dispatch_marker)
	var marker_material := StandardMaterial3D.new()
	marker_material.albedo_color = Color(1.0, 0.48, 0.08)
	marker_material.emission_enabled = true
	marker_material.emission = Color(1.0, 0.24, 0.025)
	var pole := MeshInstance3D.new()
	var pole_mesh := CylinderMesh.new()
	pole_mesh.top_radius = 0.07
	pole_mesh.bottom_radius = 0.07
	pole_mesh.height = 3.2
	pole.mesh = pole_mesh
	pole.position.y = 1.6
	pole.material_override = marker_material
	dispatch_marker.add_child(pole)
	var beacon := MeshInstance3D.new()
	var beacon_mesh := SphereMesh.new()
	beacon_mesh.radius = 0.42
	beacon_mesh.height = 0.84
	beacon.mesh = beacon_mesh
	beacon.position.y = 3.35
	beacon.material_override = marker_material
	dispatch_marker.add_child(beacon)

func _refresh_dispatch_ui() -> void:
	if not is_instance_valid(dispatch_manager) or not is_instance_valid(dispatch_summary):
		return
	var state_text: String = dispatch_manager.get_state_label()
	dispatch_state_label.text = state_text
	dispatch_summary.text = dispatch_manager.get_call_summary()
	dispatch_request_button.visible = state_text == "AVAILABLE FOR CALLS" or state_text == "CALL CLEARED"
	dispatch_accept_button.visible = state_text == "CALL OFFERED"
	dispatch_clear_button.visible = state_text == "UNIT EN ROUTE"
	if state_text == "AVAILABLE FOR CALLS" or state_text == "CALL CLEARED":
		dispatch_distance_label.text = "Unit status: Available"
	elif state_text == "CALL OFFERED":
		dispatch_distance_label.text = "Awaiting unit acknowledgement."
	elif state_text == "UNIT EN ROUTE":
		dispatch_distance_label.text = "Proceed to the marked location."
