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
