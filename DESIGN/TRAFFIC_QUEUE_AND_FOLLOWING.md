# Traffic Queue and Following-Distance Prototype

## Purpose

The isolated four-way-stop test junction now uses one inbound lane per approach with two vehicles arranged longitudinally. This replaces the earlier side-by-side arrangement, which did not represent a normal single-lane queue.

## Spawn arrangement

- Lead vehicle begins at the approach's existing start point.
- Rear vehicle begins 16 m behind the lead vehicle.
- Both vehicles follow the same approach and stop-line route.
- Both request the same intersection controller reservation; only one can occupy the junction.
- The queue is a test arrangement, not final road lane geometry.

## Following controller

`CountyTrafficVehicleAgent.CalculateFollowingSpeedLimit` performs a forward sphere-cast from just ahead of the vehicle body. It selects the nearest detected `CountyTrafficVehicleAgent` in front and caps the requested speed using three limits:

1. Configured target speed.
2. Available gap divided by configured time headway.
3. Braking-distance speed limit, derived from the available gap and configured braking value.

The available gap subtracts a minimum following distance before calculating the headway and braking limits. The result is applied before stop-line, pedestrian, and signal constraints; those constraints can reduce speed further.

Current serialized defaults:
- Front bumper offset: 2.1 m
- Stop-line center stopping tolerance: 2.8 m (keeps the bumper approximately 0.7 m before the line)
- Crosswalk stopping offset: front bumper plus 0.5 m
- Minimum following distance: 2.5 m
- Time headway: 1.35 s
- Detection distance: 18 m
- Sphere-cast radius: 0.75 m

These are initial prototype values and require tuning against the final vehicle collider, speed, and road scale.

## Junction coordinate handling

The spawner creates route waypoints as children of the junction root. Stop-line, conflict-area clearance, and crosswalk-stop references are now converted with the junction root's `TransformPoint` before they are passed to the traffic agent. This keeps those references aligned if the test junction is moved or rotated.

## Manual validation

Use `DESIGN/FOUR_WAY_STOP_TEST_MATRIX.md` for the queue, following-distance, and transform cases. In particular:

- Rear vehicle must not contact the lead vehicle when the lead vehicle brakes or stops.
- Vehicle front bumpers must remain behind stop bars and crosswalk yield lines when stopped.
- Rear vehicle must not cross the stop line while waiting for the lead vehicle's reservation.
- The lead vehicle must release its reservation only after clearing the conflict area.
- Moving or rotating the junction root must preserve stop-line and crosswalk alignment.

## Known limitations

- Sphere-cast following is a lightweight prototype, not a production car-following model.
- It does not yet handle lane changes, overtaking, multiple lanes, vehicle classes, cut-ins, reverse movement, or obstacle avoidance.
- A fixed minimum gap and time headway are not yet adapted to weather, tire grip, road grade, or vehicle mass.
- The test vehicles still share identical movement paths; turn-specific lane selection and queue branching are future work.
- No Unity compilation, Play Mode test, physics test, or iPhone profiling has been completed.
