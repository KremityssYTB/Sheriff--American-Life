extends RefCounted
class_name LawEnforcementCareer

const COUNTY_IDS := [
"alachua","baker","bay","bradford","brevard","broward","calhoun","charlotte","citrus","clay","collier","columbia","desoto","dixie","duval","escambia","flagler","franklin","gadsden","gilchrist","glades","gulf","hamilton","hardee","hendry","hernando","highlands","hillsborough","holmes","indian_river","jackson","jefferson","lafayette","lake","lee","leon","levy","liberty","madison","manatee","marion","martin","miami_dade","monroe","nassau","okaloosa","okeechobee","orange","osceola","palm_beach","pasco","pinellas","polk","putnam","santa_rosa","sarasota","seminole","st_johns","st_lucie","sumter","suwannee","taylor","union","volusia","wakulla","walton","washington"
]
const COUNTY_NAMES := {
"alachua":"Alachua","baker":"Baker","bay":"Bay","bradford":"Bradford","brevard":"Brevard","broward":"Broward","calhoun":"Calhoun","charlotte":"Charlotte","citrus":"Citrus","clay":"Clay","collier":"Collier","columbia":"Columbia","desoto":"DeSoto","dixie":"Dixie","duval":"Duval","escambia":"Escambia","flagler":"Flagler","franklin":"Franklin","gadsden":"Gadsden","gilchrist":"Gilchrist","glades":"Glades","gulf":"Gulf","hamilton":"Hamilton","hardee":"Hardee","hendry":"Hendry","hernando":"Hernando","highlands":"Highlands","hillsborough":"Hillsborough","holmes":"Holmes","indian_river":"Indian River","jackson":"Jackson","jefferson":"Jefferson","lafayette":"Lafayette","lake":"Lake","lee":"Lee","leon":"Leon","levy":"Levy","liberty":"Liberty","madison":"Madison","manatee":"Manatee","marion":"Marion","martin":"Martin","miami_dade":"Miami-Dade","monroe":"Monroe","nassau":"Nassau","okaloosa":"Okaloosa","okeechobee":"Okeechobee","orange":"Orange","osceola":"Osceola","palm_beach":"Palm Beach","pasco":"Pasco","pinellas":"Pinellas","polk":"Polk","putnam":"Putnam","santa_rosa":"Santa Rosa","sarasota":"Sarasota","seminole":"Seminole","st_johns":"St. Johns","st_lucie":"St. Lucie","sumter":"Sumter","suwannee":"Suwannee","taylor":"Taylor","union":"Union","volusia":"Volusia","wakulla":"Wakulla","walton":"Walton","washington":"Washington"
}
const STATE_AGENCIES := ["Florida Highway Patrol","Florida Fish and Wildlife Conservation Commission","Florida Department of Law Enforcement"]
var county_id := "marion"
var agency_type := "sheriff"
var agency_name := "Marion County Sheriff's Office"
var rank_name := "Deputy Sheriff"
var callsign := "214"
var on_duty := true
var last_message := "On duty — Marion County Sheriff's Office"

func get_counties() -> Array:
	var result: Array = []
	for id in COUNTY_IDS:
		result.append({"id": id, "name": COUNTY_NAMES.get(id, id.capitalize())})
	return result

func get_agency_types() -> Array:
	return ["County Sheriff", "Florida Highway Patrol", "City Police", "Fish & Wildlife"]

func get_station_position(target_county: String, catalog: FloridaRegionCatalog) -> Vector3:
	if target_county == "marion":
		return Vector3(-29.0, 0.0, -18.0)
	for district in catalog.get_districts_for_county(target_county):
		var anchor: Array = district.get("anchor_m", [])
		if anchor.size() >= 2:
			return Vector3(float(anchor[0]), 0.0, float(anchor[1]))
	# Counties without a dedicated condensed-world district use their regional
	# headquarters hub until an individual station site is authored.
	var region: Dictionary = catalog.get_region_for_county(target_county)
	var hub_by_region := {
		"panhandle": "pensacola_escambia",
		"north_central": "gainesville_alachua",
		"northeast": "jacksonville_duval",
		"central_north": "ocala_marion",
		"central": "orlando_orange",
		"tampa_bay": "tampa_hillsborough",
		"southwest": "sarasota_sarasota",
		"southeast": "miami_dade",
		"keys": "keys_monroe"
	}
	var hub_id: String = str(hub_by_region.get(str(region.get("id", "")), ""))
	for district in catalog.get_districts():
		if str(district.get("id", "")) == hub_id:
			var hub_anchor: Array = district.get("anchor_m", [])
			if hub_anchor.size() >= 2:
				return Vector3(float(hub_anchor[0]), 0.0, float(hub_anchor[1]))
	return Vector3(INF, 0.0, INF)

func request_transfer(target_county: String, target_type: String, world_position: Vector3, catalog: RefCounted) -> bool:
	if not COUNTY_IDS.has(target_county):
		last_message = "Select a valid Florida county."
		return false
	var station := get_station_position(target_county, catalog)
	if not is_finite(station.x) or world_position.distance_to(station) > 22.0:
		last_message = "Travel to the selected agency station to transfer or clock in."
		return false
	if target_type == "Florida Highway Patrol":
		agency_type = "fhp"
		agency_name = "Florida Highway Patrol"
		rank_name = "State Trooper"
	elif target_type == "Fish & Wildlife":
		agency_type = "fwc"
		agency_name = "Florida Fish and Wildlife Conservation Commission"
		rank_name = "FWC Officer"
	elif target_type == "City Police":
		agency_type = "city"
		agency_name = COUNTY_NAMES.get(target_county, target_county) + " City Police Department"
		rank_name = "Police Officer"
	else:
		agency_type = "sheriff"
		agency_name = COUNTY_NAMES.get(target_county, target_county) + " County Sheriff's Office"
		rank_name = "Deputy Sheriff"
	county_id = target_county
	on_duty = true
	last_message = "Clocked in as %s — %s." % [rank_name, agency_name]
	return true

func clock_out() -> void:
	on_duty = false
	last_message = "Off duty — %s." % agency_name

func get_assignment_label() -> String:
	return "%s  /  %s  /  %s" % [agency_name.to_upper(), rank_name.to_upper(), "ON DUTY" if on_duty else "OFF DUTY"]
