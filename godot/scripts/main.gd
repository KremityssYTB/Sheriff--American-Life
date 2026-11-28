extends Node3D

const DeputyController = preload("res://scripts/player_controller.gd")
const PatrolSUVController = preload("res://scripts/patrol_suv.gd")
const DispatchManager = preload("res://scripts/dispatch_manager.gd")
const FloridaRegionCatalog = preload("res://scripts/florida_region_catalog.gd")
const LawEnforcementCareer = preload("res://scripts/law_enforcement_career.gd")
var player: CharacterBody3D
var patrol_suv: VehicleBody3D
var in_vehicle := false
var status_label: Label
var foot_controls: Array[Button] = []
var vehicle_controls: Array[Button] = []
var enter_button: Button
var lights_button: Button
var vehicle_look_touch_id := -1
var region_catalog: FloridaRegionCatalog
var dispatch_manager: Node
var dispatch_summary: Label
var dispatch_state_label: Label
var dispatch_distance_label: Label
var dispatch_request_button: Button
var dispatch_accept_button: Button
var dispatch_clear_button: Button
var dispatch_marker: Node3D
var career: LawEnforcementCareer
var county_picker: OptionButton
var agency_picker: OptionButton
var transfer_button: Button
var agency_assignment_label: Label

func _ready() -> void:
	# Supersample the desktop Forward+ path; keep iOS at native resolution.
	get_viewport().scaling_3d_scale = 1.0 if OS.has_feature("mobile") else 1.25
	_setup_input()
	region_catalog = FloridaRegionCatalog.new()
	career = LawEnforcementCareer.new()
	if region_catalog.load_catalog():
		print("Florida region catalog loaded: %d counties, %d playable districts, %.0f km2." % [region_catalog.get_county_count(), region_catalog.get_districts().size(), region_catalog.get_map_area_km2()])
		var stream_grid: Vector2i = region_catalog.get_stream_cell_count()
		print("Florida world streaming grid: %d x %d cells at %d m per cell." % [stream_grid.x, stream_grid.y, int(region_catalog.data.get("streaming", {}).get("cell_size_m", 500))])
		var road_test: Dictionary = region_catalog.find_route("ocala_marion", "miami_dade", "road")
		var water_test: Dictionary = region_catalog.find_route("ocala_marion", "rural_heartland", "water")
		print("Florida route checks — Ocala to Miami road: %s; Ocala to Heartland water: %s." % [road_test.get("found", false), water_test.get("found", false)])
	_build_environment()
	_build_district()
	_spawn_player()
	_spawn_patrol_suv()
	dispatch_manager = DispatchManager.new()
	dispatch_manager.name = "DispatchManager"
	dispatch_manager.state_changed.connect(_refresh_dispatch_ui)
	add_child(dispatch_manager)
	_build_touch_hud()

func _request_agency_transfer() -> void:
	if not is_instance_valid(player) or county_picker.selected < 0:
		return
	var county_id: String = str(county_picker.get_item_metadata(county_picker.selected))
	var agency_type: String = agency_picker.get_item_text(agency_picker.selected)
	if career.request_transfer(county_id, agency_type, player.global_position, region_catalog):
		agency_assignment_label.text = career.get_assignment_label()
		status_label.text = "UNIT %s  •  %s  •  RADIO 1" % [career.callsign, career.rank_name.to_upper()]
	else:
		agency_assignment_label.text = career.last_message.to_upper()

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
	if is_instance_valid(transfer_button) and is_instance_valid(player):
		var selected_county: String = str(county_picker.get_item_metadata(county_picker.selected))
		var station_position: Vector3 = career.get_station_position(selected_county, region_catalog)
		transfer_button.disabled = not is_finite(station_position.x) or player.global_position.distance_to(station_position) > 22.0
	if not in_vehicle and is_instance_valid(player) and is_instance_valid(patrol_suv):
		var near_vehicle := player.global_position.distance_to(patrol_suv.global_position) < 4.2
		enter_button.visible = near_vehicle
		if near_vehicle:
			status_label.text = "UNIT 214  •  ON FOOT  •  VEHICLE IN RANGE"
		else:
			status_label.text = "UNIT 214  •  ON FOOT  •  RADIO 1 / PRIMARY"
	elif in_vehicle:
		var speed := patrol_suv.linear_velocity.length() * 3.6
		status_label.text = "UNIT 214  •  PATROL SUV  •  %.0f KM/H  •  RADIO 1" % speed
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
	var asphalt := _material(Color(0.105, 0.115, 0.12))
	var concrete := _material(Color(0.48, 0.49, 0.46))
	var lane_paint := _material(Color(0.88, 0.84, 0.66))
	var white_paint := _material(Color(0.88, 0.89, 0.86))
	var glass := _material(Color(0.12, 0.23, 0.29), 0.22)
	var curb := _material(Color(0.56, 0.57, 0.54))
	var facade_colors := [
		Color(0.67, 0.61, 0.51), Color(0.73, 0.72, 0.66),
		Color(0.55, 0.59, 0.57), Color(0.75, 0.68, 0.56)
	]
	_add_box(self, "CountyTerrain", Vector3(0, -0.4, 0), Vector3(240, 0.7, 240), grass, true)

	# A connected, legible street grid replaces the single-crossroad test block.
	# Major north/south corridor and four east/west neighborhood streets.
	var road_centers := [-80.0, -40.0, 0.0, 40.0, 80.0]
	for x in road_centers:
		var width := 14.0 if is_zero_approx(x) else 10.0
		_add_box(self, "NorthSouthRoad_%s" % str(x), Vector3(x, 0.0, 0), Vector3(width, 0.14, 210), asphalt)
		_add_box(self, "RoadEdgeWest_%s" % str(x), Vector3(x - width * 0.5 - 0.65, 0.10, 0), Vector3(0.35, 0.20, 210), curb)
		_add_box(self, "RoadEdgeEast_%s" % str(x), Vector3(x + width * 0.5 + 0.65, 0.10, 0), Vector3(0.35, 0.20, 210), curb)
		for z in range(-96, 97, 16):
			_add_box(self, "CenterDashNS_%s" % str(x), Vector3(x, 0.085, z), Vector3(0.16, 0.025, 5.5), lane_paint)
	for z in road_centers:
		_add_box(self, "EastWestRoad_%s" % str(z), Vector3(0, 0.015, z), Vector3(210, 0.14, 10), asphalt)
		_add_box(self, "RoadEdgeNorth_%s" % str(z), Vector3(0, 0.10, z - 5.65), Vector3(210, 0.20, 0.35), curb)
		_add_box(self, "RoadEdgeSouth_%s" % str(z), Vector3(0, 0.10, z + 5.65), Vector3(210, 0.20, 0.35), curb)
		for x in range(-96, 97, 16):
			_add_box(self, "CenterDashEW_%s" % str(z), Vector3(x, 0.09, z), Vector3(5.5, 0.025, 0.16), lane_paint)

	# Sidewalks, pedestrian crossings and curb ramps at the central civic junction.
	for side in [-1.0, 1.0]:
		_add_box(self, "CivicSidewalkNS", Vector3(side * 8.8, 0.18, 0), Vector3(2.2, 0.36, 210), concrete)
		_add_box(self, "CivicSidewalkEW", Vector3(0, 0.18, side * 8.8), Vector3(210, 0.36, 2.2), concrete)
	for stripe in range(7):
		_add_box(self, "CrosswalkNorth_%02d" % stripe, Vector3(-4.0 + stripe * 1.3, 0.10, -8.1), Vector3(0.72, 0.035, 2.0), white_paint)
		_add_box(self, "CrosswalkSouth_%02d" % stripe, Vector3(-4.0 + stripe * 1.3, 0.10, 8.1), Vector3(0.72, 0.035, 2.0), white_paint)

	# Commercial and residential blocks with varied building footprints and facade heights.
	var index := 0
	for block_x in [-60.0, -20.0, 20.0, 60.0]:
		for block_z in [-60.0, -20.0, 20.0, 60.0]:
			for slot in range(2):
				var x := block_x + (-7.0 if slot == 0 else 7.0)
				var z := block_z + (5.0 if slot == 0 else -5.0)
				if absf(x + 29.0) < 14.0 and absf(z + 18.0) < 13.0:
					continue
				var height := 5.5 + float((index * 7) % 13)
				var width := 10.0 + float((index * 3) % 5)
				var depth := 11.0 + float((index * 5) % 5)
				var facade: Color = facade_colors[index % facade_colors.size()]
				_add_box(self, "CountyBuilding_%02d" % index, Vector3(x, height * 0.5, z), Vector3(width, height, depth), _material(facade), true)
				_add_box(self, "StorefrontGlass_%02d" % index, Vector3(x, 1.9, z - depth * 0.501), Vector3(width * 0.62, 2.1, 0.12), glass)
				_add_box(self, "RoofParapet_%02d" % index, Vector3(x, height + 0.16, z), Vector3(width + 0.35, 0.32, depth + 0.35), concrete)
				index += 1

	# Marion County Sheriff Office civic campus and marked patrol parking.
	_add_box(self, "MarionSheriffOffice", Vector3(-29, 3.2, -18), Vector3(23, 6.4, 19), _material(Color(0.65, 0.66, 0.63)), true)
	_add_box(self, "SheriffOfficeWindows", Vector3(-29, 3.3, -27.58), Vector3(17, 2.2, 0.12), glass)
	_add_box(self, "SheriffSign", Vector3(-29, 5.4, -27.7), Vector3(8, 0.65, 0.18), _material(Color(0.08, 0.12, 0.16)))
	_add_box(self, "SheriffParkingLot", Vector3(-29, 0.035, -40), Vector3(28, 0.12, 15), asphalt)
	for bay in range(7):
		_add_box(self, "SheriffParkingBay_%02d" % bay, Vector3(-40.5 + bay * 3.8, 0.11, -40), Vector3(0.12, 0.025, 11), white_paint)

	# Palm rows and low-detail street furniture establish Florida streetscape scale.
	for i in range(28):
		var x := -96.0 + float((i * 37) % 192)
		var z := -96.0 + float((i * 61) % 192)
		if absf(fposmod(x, 40.0)) < 7.0 or absf(fposmod(z, 40.0)) < 7.0:
			continue
		_add_box(self, "PalmTrunk_%02d" % i, Vector3(x, 3.0, z), Vector3(0.38, 6.0, 0.38), _material(Color(0.34, 0.23, 0.13)))
		_add_box(self, "PalmCanopy_%02d" % i, Vector3(x, 6.3, z), Vector3(4.2, 0.35, 2.1), _material(Color(0.13, 0.29, 0.12)))

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
	layer.name = "PoliceInterface"
	add_child(layer)

	# Compact CAD-style unit header. Anchors keep it usable across phone and tablet sizes.
	var header := PanelContainer.new()
	header.name = "UnitStatusHeader"
	header.set_anchors_preset(Control.PRESET_TOP_LEFT)
	header.position = Vector2(18, 18)
	header.custom_minimum_size = Vector2(310, 82)
	var header_style := StyleBoxFlat.new()
	header_style.bg_color = Color(0.025, 0.045, 0.065, 0.94)
	header_style.border_color = Color(0.28, 0.39, 0.48, 0.95)
	header_style.set_border_width_all(1)
	header_style.set_corner_radius_all(8)
	header_style.content_margin_left = 12
	header_style.content_margin_right = 12
	header_style.content_margin_top = 8
	header_style.content_margin_bottom = 8
	header.add_theme_stylebox_override("panel", header_style)
	layer.add_child(header)
	var header_content := VBoxContainer.new()
	header_content.add_theme_constant_override("separation", 4)
	header.add_child(header_content)
	var title := Label.new()
	title.text = "FLORIDA LAW ENFORCEMENT"
	title.add_theme_font_size_override("font_size", 13)
	title.add_theme_color_override("font_color", Color(0.76, 0.84, 0.91))
	header_content.add_child(title)
	agency_assignment_label = Label.new()
	agency_assignment_label.text = career.get_assignment_label()
	agency_assignment_label.add_theme_font_size_override("font_size", 10)
	agency_assignment_label.add_theme_color_override("font_color", Color(0.48, 0.78, 0.72))
	header_content.add_child(agency_assignment_label)
	county_picker = OptionButton.new()
	county_picker.custom_minimum_size = Vector2(138, 32)
	for county in career.get_counties():
		county_picker.add_item(county.name)
		county_picker.set_item_metadata(county_picker.item_count - 1, county.id)
		if county.id == career.county_id:
			county_picker.select(county_picker.item_count - 1)
	agency_picker = OptionButton.new()
	agency_picker.custom_minimum_size = Vector2(138, 32)
	for agency_type in career.get_agency_types():
		agency_picker.add_item(agency_type)
	var career_row := HBoxContainer.new()
	career_row.add_child(county_picker)
	career_row.add_child(agency_picker)
	header_content.add_child(career_row)
	transfer_button = Button.new()
	transfer_button.text = "CLOCK IN / TRANSFER AT STATION"
	transfer_button.custom_minimum_size = Vector2(276, 30)
	transfer_button.pressed.connect(_request_agency_transfer)
	header_content.add_child(transfer_button)
	status_label = Label.new()
	status_label.text = "UNIT 214  •  AVAILABLE  •  ON FOOT"
	status_label.add_theme_font_size_override("font_size", 12)
	status_label.add_theme_color_override("font_color", Color(0.91, 0.95, 0.98))
	header_content.add_child(status_label)
	var radio_label := Label.new()
	radio_label.text = "RADIO 1  /  PRIMARY     GPS  /  OCALA DISTRICT"
	radio_label.add_theme_font_size_override("font_size", 10)
	radio_label.add_theme_color_override("font_color", Color(0.48, 0.72, 0.78))
	header_content.add_child(radio_label)

	var foot := [
		["UP", "move_forward"],
		["DOWN", "move_back"],
		["LEFT", "move_left"],
		["RIGHT", "move_right"],
		["RUN", "sprint"]
	]
	for item in foot:
		var button := TouchButton.new()
		button.text = item[0]
		button.action_name = item[1]
		layer.add_child(button)
		foot_controls.append(button)
	var drive := [
		["GAS", "drive_forward"],
		["REVERSE", "drive_back"],
		["LEFT", "drive_left"],
		["RIGHT", "drive_right"],
		["BRAKE", "vehicle_brake"]
	]
	for item in drive:
		var button := TouchButton.new()
		button.text = item[0]
		button.action_name = item[1]
		layer.add_child(button)
		vehicle_controls.append(button)
		button.hide()

	enter_button = Button.new()
	enter_button.text = "ENTER PATROL SUV"
	enter_button.custom_minimum_size = Vector2(184, 48)
	enter_button.position = Vector2(18, 112)
	enter_button.visible = false
	enter_button.pressed.connect(_toggle_vehicle)
	layer.add_child(enter_button)
	lights_button = Button.new()
	lights_button.text = "EMERGENCY LIGHTS"
	lights_button.custom_minimum_size = Vector2(184, 44)
	lights_button.position = Vector2(18, 166)
	lights_button.visible = false
	lights_button.pressed.connect(func(): patrol_suv.toggle_lights())
	layer.add_child(lights_button)
	_build_dispatch_panel(layer)
	_layout_touch_controls()
	get_viewport().size_changed.connect(_layout_touch_controls)

func _layout_touch_controls() -> void:
	var viewport_size := get_viewport().get_visible_rect().size
	var button_size := Vector2(64, 58)
	var bottom := viewport_size.y - button_size.y - 20.0
	var upper := bottom - button_size.y - 8.0
	var left_x := 14.0
	var center_x := left_x + button_size.x + 8.0
	var right_x := left_x + (button_size.x + 8.0) * 2.0
	var drive_right := viewport_size.x - button_size.x - 14.0
	var drive_center := drive_right - button_size.x - 8.0
	var drive_left := drive_center - button_size.x - 8.0
	var foot_positions := [
		Vector2(center_x, upper),
		Vector2(center_x, bottom),
		Vector2(left_x, bottom),
		Vector2(right_x, bottom),
		Vector2(left_x, upper)
	]
	var vehicle_positions := [
		Vector2(drive_right, upper),
		Vector2(drive_right, bottom),
		Vector2(drive_left, bottom),
		Vector2(drive_center, bottom),
		Vector2(drive_left, upper)
	]
	for i in range(foot_controls.size()):
		foot_controls[i].position = foot_positions[i]
		foot_controls[i].size = button_size
	for i in range(vehicle_controls.size()):
		vehicle_controls[i].position = vehicle_positions[i]
		vehicle_controls[i].size = button_size

class TouchButton:
	extends Button
	var action_name := ""
	func _ready() -> void:
		add_theme_font_size_override("font_size", 12)
		add_theme_color_override("font_color", Color(0.91, 0.95, 0.98))
		add_theme_color_override("font_hover_color", Color(1.0, 1.0, 1.0))
		add_theme_color_override("font_pressed_color", Color(0.72, 0.86, 0.94))
		var normal := StyleBoxFlat.new()
		normal.bg_color = Color(0.025, 0.055, 0.078, 0.88)
		normal.border_color = Color(0.32, 0.48, 0.58, 0.95)
		normal.set_border_width_all(1)
		normal.set_corner_radius_all(10)
		normal.content_margin_left = 5
		normal.content_margin_right = 5
		var hover := normal.duplicate() as StyleBoxFlat
		hover.bg_color = Color(0.07, 0.13, 0.17, 0.96)
		var pressed := normal.duplicate() as StyleBoxFlat
		pressed.bg_color = Color(0.12, 0.23, 0.29, 0.98)
		add_theme_stylebox_override("normal", normal)
		add_theme_stylebox_override("hover", hover)
		add_theme_stylebox_override("pressed", pressed)
		add_theme_stylebox_override("focus", normal)
		button_down.connect(func(): Input.action_press(action_name))
		button_up.connect(func(): Input.action_release(action_name))

func _build_dispatch_panel(layer: CanvasLayer) -> void:
	var panel := PanelContainer.new()
	panel.name = "DispatchPanel"
	var viewport_width := get_viewport().get_visible_rect().size.x
	if viewport_width < 700.0:
		panel.set_anchors_preset(Control.PRESET_TOP_LEFT)
		panel.position = Vector2(18, 112)
		panel.custom_minimum_size = Vector2(minf(viewport_width - 36.0, 354.0), 0)
	else:
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
