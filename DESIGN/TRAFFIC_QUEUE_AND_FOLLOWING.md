# Traffic Queue and Following-Distance Prototype

## Purpose

The isolated four-way-stop test junction now uses three inbound movement lanes per approach: a left-turn lane, a through lane, and a right-turn lane. Each lane has two vehicles arranged longitudinally, for 24 test vehicles total. The road is widened to 22 m to accommodate the six-lane cross-section in this isolated test scene.

## Spawn arrangement

- Lead vehicle begins at the approach's existing start point.
- Rear vehicle begins 16 m behind the lead vehicle.
- Both vehicles follow the same approach and stop-line route.
- Both request the same intersection controller reservation; only one can occupy the junction.
- The queue is a test arrangement, not final road lane geometry.

## Lane assignment and turn geometry

Each approach assigns the leftmost inbound lane to left turns, the middle lane to straight-through traffic, and the rightmost inbound lane to right turns. Lane offsets are measured from the approaching vehicle's right vector. Each movement receives a separate cubic Bezier path with control points aligned to the inbound and outbound directions. Straight movements preserve their lane offset; turning movements target the near-side outbound lane on the destination road. Each lane has its own stop bar and roadside STOP sign, offset to the right of the lane so it does not sit in the vehicle's forward probe path. Dashed lane markings are generated along both road axes and stop before the central junction area. The 24-vehicle setup preserves two-car longitudinal queues in all movement lanes.

This is a test-junction layout, not a final Florida road cross-section. The paths require visual and physics validation for curb clearance, turning radius, lane alignment, and conflicts at the center of the junction.

## Following controller

`CountyTrafficVehicleAgent.CalculateFollowingSpeedLimit` performs a forward sphere-cast from just ahead of the vehicle body. It considers other `CountyTrafficVehicleAgent` vehicles and raised static colliders in front, ignoring triggers, the vehicle's own Rigidbody colliders, and low ground geometry. It caps requested speed using three limits:

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
- Static-obstacle clearance: 2.5 m

These are initial prototype values and require tuning against the final vehicle collider, speed, and road scale.

## Full-stop dwell and queue cleanup

A vehicle must remain at or below the configured stopped-speed threshold for at least 1.25 seconds before it can request an intersection reservation. The dwell timer starts when the agent first registers its stopped arrival, so the controller's simultaneous-arrival arbitration window is already elapsed by the time the dwell completes. If the vehicle rolls forward beyond the stop line or accelerates above twice the stopped-speed threshold before receiving a reservation, its waiting record is removed and the dwell timer resets. This prevents a brief speed dip from counting as a legal stop and prevents stale queue entries after a failed stop.

The dwell value is a prototype tuning parameter; it is not intended as a final legal timing rule.

## Junction coordinate handling

The spawner creates route waypoints as children of the junction root. Stop-line, conflict-area clearance, and crosswalk-stop references are now converted with the junction root's `TransformPoint` before they are passed to the traffic agent. This keeps those references aligned if the test junction is moved or rotated.

## Manual validation

Use `DESIGN/FOUR_WAY_STOP_TEST_MATRIX.md` for the queue, following-distance, and transform cases. In particular:

- Rear vehicle must not contact the lead vehicle when the lead vehicle brakes or stops.
- Vehicle front bumpers must remain behind stop bars and crosswalk yield lines when stopped.
- A vehicle must remain fully stopped for at least 1.25 seconds before it can request a reservation.
- If a waiting vehicle rolls forward or accelerates before receiving a reservation, its waiting record and dwell timer must reset.
- Rear vehicle must not cross the stop line while waiting for the lead vehicle's reservation.
- The lead vehicle must release its reservation only after clearing the conflict area.
- Moving or rotating the junction root must preserve stop-line and crosswalk alignment.

## Known limitations

- Sphere-cast following is a lightweight prototype, not a production car-following model.
- Static-obstacle response is a forward-probe prototype; it does not steer around obstructions, change lanes, overtake, handle cut-ins/reversing vehicles, or route around blocked roads.
- A fixed minimum gap and time headway are not yet adapted to weather, tire grip, road grade, or vehicle mass.
- The test vehicles now use lane-specific left/straight/right paths, but the cubic curves and outbound lane alignment remain unverified. Lane changes, queue branching, and multi-lane merge behavior are future work.
- One North-approach through-lane lead vehicle is designated as the emergency test sedan and receives a dark-blue body with alternating red/blue roof lightbar blocks driven by a renderer-only controller. At a four-way stop, it receives priority over ordinary waiting traffic, but cannot pre-empt an occupant or enter while a pedestrian conflict is active. Siren/audio, dispatch activation, and a full emergency vehicle model are not implemented yet.
- No Unity compilation, Play Mode test, physics test, or iPhone profiling has been completed.
