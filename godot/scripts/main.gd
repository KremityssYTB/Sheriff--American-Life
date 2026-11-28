extends Node3D

const DeputyController = preload("res://scripts/player_controller.gd")
var player: CharacterBody3D
var status_label: Label

func _ready() -> void:
	_setup_input()
	_build_environment()
	_build_district()
	_spawn_player()
	_build_touch_hud()

func _setup_input() -> void:
	_add_key("move_forward", KEY_W)
	_add_key("move_back", KEY_S)
	_add_key("move_left", KEY_A)
	_add_key("move_right", KEY_D)
	_add_key("sprint", KEY_SHIFT)

func _add_key(action: String, key: Key) -> void:
	if not InputMap.has_action(action):
		InputMap.add_action(action)
	var event := InputEventKey.new()
	event.physical_keycode = key
	InputMap.action_add_event(action, event)

func _material(color: Color, roughness := 0.9) -> StandardMaterial3D:
	var mat := StandardMaterial3D.new()
	mat.albedo_color = color
	mat.roughness = roughness
	return mat

func _add_box(parent: Node3D, name: String, pos: Vector3, size: Vector3, mat: Material, collision := false) -> void:
	var body: Node3D
	if collision:
		body = StaticBody3D.new()
	else:
		body = Node3D.new()
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
	# Recognizable first-playable civic anchor: sheriff office and marked sign.
	_add_box(self, "MarionSheriffOffice", Vector3(-29,3.2,-18), Vector3(23,6.4,19), _material(Color(0.65,0.66,0.63)), true)
	_add_box(self, "SheriffOfficeWindows", Vector3(-29,3.3,-27.58), Vector3(17,2.2,0.12), glass)
	_add_box(self, "SheriffSign", Vector3(-29,5.4,-27.7), Vector3(8,0.65,0.18), _material(Color(0.08,0.12,0.16)))
	# Palms are deliberately low-poly placeholders pending licensed art assets.
	for i in range(18):
		var x := -55.0 + float((i*17)%110)
		var z := -90.0 + float((i*23)%180)
		_add_box(self, "PalmTrunk", Vector3(x,3,z), Vector3(0.45,6,0.45), _material(Color(0.34,0.23,0.13)))
		_add_box(self, "PalmCanopy", Vector3(x,6.3,z), Vector3(4.2,0.35,2.1), _material(Color(0.13,0.29,0.12)))

func _spawn_player() -> void:
	player = DeputyController.new()
	player.name = "Deputy"
	player.position = Vector3(-4,0.2,12)
	add_child(player)

func _build_touch_hud() -> void:
	var layer := CanvasLayer.new()
	add_child(layer)
	var title := Label.new()
	title.text = "SHERIFF: AMERICAN LIFE  |  OCALA DISTRICT TEST"
	title.position = Vector2(24,18)
	title.add_theme_font_size_override("font_size", 22)
	layer.add_child(title)
	status_label = Label.new()
	status_label.text = "ON FOOT  •  W/A/S/D to move  •  drag right side to look"
	status_label.position = Vector2(24,52)
	layer.add_child(status_label)
	var controls := [
		["UP","move_forward",Vector2(112,560)],
		["DOWN","move_back",Vector2(112,660)],
		["LEFT","move_left",Vector2(12,610)],
		["RIGHT","move_right",Vector2(212,610)],
		["RUN","sprint",Vector2(250,550)]
	]
	for item in controls:
		var button := TouchButton.new()
		button.text = item[0]
		button.position = item[2]
		button.size = Vector2(88,76)
		button.action_name = item[1]
		layer.add_child(button)

class TouchButton:
	extends Button
	var action_name := ""
	func _ready() -> void:
		modulate = Color(1,1,1,0.78)
		add_theme_font_size_override("font_size", 16)
		button_down.connect(func(): Input.action_press(action_name))
		button_up.connect(func(): Input.action_release(action_name))
