extends RefCounted
class_name FloridaRegionCatalog

const CATALOG_PATH := "res://data/florida_region.json"
var data: Dictionary = {}
var load_error := ""

func load_catalog() -> bool:
	if not FileAccess.file_exists(CATALOG_PATH):
		return _fail("Florida region data file is missing.")
	var file := FileAccess.open(CATALOG_PATH, FileAccess.READ)
	if file == null:
		return _fail("Could not open Florida region data.")
	var parsed: Variant = JSON.parse_string(file.get_as_text())
	if typeof(parsed) != TYPE_DICTIONARY:
		return _fail("Florida region data is not a JSON object.")
	data = parsed
	var counties: Array = data.get("county_roster", [])
	var districts: Array = data.get("districts", [])
	if counties.size() != 67:
		return _fail("Expected 67 Florida counties; found %d." % counties.size())
	if not is_equal_approx(float(data.get("map_area_km2", 0.0)), 225.0):
		return _fail("Map area must remain 225 km2.")
	if districts.is_empty():
		return _fail("Florida region catalog has no playable districts.")
	var district_ids := {}
	for district in districts:
		if typeof(district) != TYPE_DICTIONARY or not district.has("id") or not district.has("county_id"):
			return _fail("A district entry is missing its identity or county.")
		district_ids[str(district.id)] = true
	for access in data.get("watercraft_access_points", []):
		if not district_ids.has(str(access.get("district_id", ""))):
			return _fail("Water access point references an unknown district.")
	var graph: Dictionary = data.get("route_graph", {})
	var node_ids := {}
	for node in graph.get("nodes", []):
		if not district_ids.has(str(node.get("id", ""))) or node_ids.has(str(node.get("id", ""))):
			return _fail("Route graph contains an unknown or duplicate node.")
		node_ids[str(node.id)] = true
	for edge in graph.get("road_edges", []) + graph.get("water_edges", []):
		if not node_ids.has(str(edge.get("from", ""))) or not node_ids.has(str(edge.get("to", ""))):
			return _fail("A route edge references an unknown district node.")
	return true

func _fail(message: String) -> bool:
	load_error = message
	push_error(message)
	return false

func get_county_count() -> int:
	return data.get("county_roster", []).size()

func get_districts() -> Array:
	return data.get("districts", [])

func get_water_access_points() -> Array:
	return data.get("watercraft_access_points", [])

func get_map_area_km2() -> float:
	return float(data.get("map_area_km2", 0.0))

func get_district_by_id(district_id: String) -> Dictionary:
	for district in get_districts():
		if str(district.get("id", "")) == district_id:
			return district
	return {}

func get_route_graph() -> Dictionary:
	return data.get("route_graph", {})

func find_route(start_id: String, end_id: String, mode: String = "road") -> Dictionary:
	if mode != "road" and mode != "water":
		return {"found": false, "reason": "Unsupported travel mode."}
	var graph: Dictionary = get_route_graph()
	var positions := {}
	for node in graph.get("nodes", []):
		positions[str(node.id)] = Vector2(float(node.x_m), float(node.z_m))
	if not positions.has(start_id) or not positions.has(end_id):
		return {"found": false, "reason": "Unknown route endpoint."}
	if start_id == end_id:
		return {"found": true, "mode": mode, "nodes": [start_id], "distance_m": 0.0}
	var edges: Array = graph.get("road_edges", []) if mode == "road" else graph.get("water_edges", [])
	var unvisited: Array = positions.keys()
	var distances := {}
	var previous := {}
	for node_id in unvisited:
		distances[node_id] = INF
	distances[start_id] = 0.0
	while not unvisited.is_empty():
		var current := ""
		var best_distance := INF
		for candidate in unvisited:
			if float(distances[candidate]) < best_distance:
				best_distance = float(distances[candidate])
				current = str(candidate)
		if current.is_empty() or best_distance == INF:
			break
		unvisited.erase(current)
		if current == end_id:
			break
		for edge in edges:
			var neighbor := ""
			if str(edge.from) == current:
				neighbor = str(edge.to)
			elif str(edge.to) == current:
				neighbor = str(edge.from)
			if neighbor.is_empty() or not unvisited.has(neighbor):
				continue
			var route_factor := 1.0
			if mode == "road":
				match str(edge.get("road_class", "")):
					"interregional_highway":
						route_factor = 0.92
					"state_highway":
						route_factor = 0.96
					"urban_arterial":
						route_factor = 1.12
					"county_highway":
						route_factor = 1.08
					"causeway":
						route_factor = 1.02
			else:
				route_factor = 1.18
			var edge_distance: float = positions[current].distance_to(positions[neighbor]) * route_factor
			var alternative: float = best_distance + edge_distance
			if alternative < float(distances[neighbor]):
				distances[neighbor] = alternative
				previous[neighbor] = current
	if float(distances[end_id]) == INF:
		return {"found": false, "reason": "No connected route for this travel mode."}
	var path: Array[String] = [end_id]
	var cursor := end_id
	while cursor != start_id:
		if not previous.has(cursor):
			return {"found": false, "reason": "Route reconstruction failed."}
		cursor = str(previous[cursor])
		path.push_front(cursor)
	return {"found": true, "mode": mode, "nodes": path, "distance_m": float(distances[end_id])}

func is_inside_world(world_position: Vector2) -> bool:
	var bounds: Array = data.get("coordinate_system", {}).get("world_bounds_m", [])
	if bounds.size() != 4:
		return false
	return world_position.x >= float(bounds[0]) and world_position.y >= float(bounds[1]) and world_position.x <= float(bounds[2]) and world_position.y <= float(bounds[3])

func get_stream_cell(world_position: Vector2) -> Vector2i:
	if not is_inside_world(world_position):
		return Vector2i(-1, -1)
	var cell_size := int(data.get("streaming", {}).get("cell_size_m", 500))
	var bounds: Array = data.get("coordinate_system", {}).get("world_bounds_m", [])
	var counts := get_stream_cell_count()
	var cell_x := clampi(floori((world_position.x - float(bounds[0])) / float(cell_size)), 0, counts.x - 1)
	var cell_z := clampi(floori((world_position.y - float(bounds[1])) / float(cell_size)), 0, counts.y - 1)
	return Vector2i(cell_x, cell_z)

func get_stream_cell_count() -> Vector2i:
	var bounds: Array = data.get("coordinate_system", {}).get("world_bounds_m", [])
	if bounds.size() != 4:
		return Vector2i.ZERO
	var cell_size := int(data.get("streaming", {}).get("cell_size_m", 500))
	if cell_size <= 0:
		return Vector2i.ZERO
	return Vector2i(
		ceili((float(bounds[2]) - float(bounds[0])) / float(cell_size)),
		ceili((float(bounds[3]) - float(bounds[1])) / float(cell_size))
	)

func get_region_for_county(county_id: String) -> Dictionary:
	for region in data.get("regions", []):
		if county_id in region.get("county_ids", []):
			return region
	return {}

func get_districts_for_county(county_id: String) -> Array:
	var matches: Array = []
	for district in get_districts():
		if str(district.get("county_id", "")) == county_id:
			matches.append(district)
	return matches

func get_county_roster() -> Array:
	return data.get("county_roster", [])
