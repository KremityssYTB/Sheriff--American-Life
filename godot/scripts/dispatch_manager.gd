extends Node
class_name DispatchManager

signal state_changed

enum CallState { IDLE, OFFERED, ACTIVE, CLEARED }

const DISPATCH_CALLS: Array[Dictionary] = [
	{
		"type": "WELFARE CHECK",
		"priority": "ROUTINE",
		"location": "Pine Avenue / Ocala District",
		"details": "Caller requests a welfare check at the reported location.",
		"destination": Vector3(0.0, 0.0, -38.0)
	},
	{
		"type": "TRAFFIC COMPLAINT",
		"priority": "ROUTINE",
		"location": "Central Avenue / Ocala District",
		"details": "Vehicle complaint reported. Locate the vehicle and assess the situation.",
		"destination": Vector3(0.0, 0.0, 34.0)
	},
	{
		"type": "SUSPICIOUS VEHICLE",
		"priority": "STANDARD",
		"location": "Marion County civic block",
		"details": "A parked vehicle has been reported. Respond and investigate.",
		"destination": Vector3(-28.0, 0.0, 18.0)
	}
]

var state: CallState = CallState.IDLE
var current_call: Dictionary = {}

func offer_call() -> bool:
	if state == CallState.OFFERED or state == CallState.ACTIVE:
		return false
	current_call = DISPATCH_CALLS[randi_range(0, DISPATCH_CALLS.size() - 1)].duplicate(true)
	state = CallState.OFFERED
	state_changed.emit()
	return true

func accept_call() -> bool:
	if state != CallState.OFFERED:
		return false
	state = CallState.ACTIVE
	state_changed.emit()
	return true

func clear_call() -> bool:
	if state != CallState.ACTIVE:
		return false
	state = CallState.CLEARED
	state_changed.emit()
	return true

func get_state_label() -> String:
	match state:
		CallState.OFFERED:
			return "CALL OFFERED"
		CallState.ACTIVE:
			return "UNIT EN ROUTE"
		CallState.CLEARED:
			return "CALL CLEARED"
		_:
			return "AVAILABLE FOR CALLS"

func get_call_summary() -> String:
	if current_call.is_empty():
		return "No active call. Request a dispatch assignment when ready."
	return "%s  |  %s\n%s\n%s" % [
		str(current_call.get("priority", "ROUTINE")),
		str(current_call.get("type", "CALL")),
		str(current_call.get("location", "Unknown location")),
		str(current_call.get("details", ""))
	]

func get_destination() -> Vector3:
	return current_call.get("destination", Vector3.ZERO)
