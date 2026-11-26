# Isolated Four-Way-Stop Test Junction

## Scene integration

`CountyFourWayStopTestSpawner` builds a second, isolated junction in the runtime Ocala greybox. It is placed away from the existing signalized intersection and creates:

- A separate north-south and east-west road crossing
- Four stop bars and four visible STOP sign assemblies
- Two adjacent-lane test sedans on each approach (eight total), arriving together with representative straight-through, left-turn, and right-turn movement assignments
- Four full-road-width zebra crosswalks with matching trigger volumes and a shared `CountyFourWayStopController`
- One looping pedestrian test actor per crosswalk, walking between opposite sidewalks
- Pedestrian actors wait at the curb while a vehicle already occupies the junction; crosswalk triggers keep new intersection reservations closed while pedestrians are crossing
- Approaching vehicles brake at a dedicated yield line before the crosswalk while pedestrian occupancy is active
- Authored straight, left-turn, and right-turn waypoint paths with post-intersection clearance points and outer return loops

Each sedan receives its own approach enum, movement type, stop-line coordinate, curved turn waypoints where applicable, and clearance point. The current four-vehicle sample assigns north/south/east/west movements as straight/left/right/straight respectively. The traffic agent's optional stop-sign path is enabled only for these four test vehicles. The original eight signal-controlled vehicles remain on their existing signalized junction.

## Test purpose

This provides a scene-level integration target for observing:
- Full-stop approach and stop-line braking
- Registration of stopped vehicles
- Exclusive intersection reservations, including two same-approach vehicles attempting entry together
- A short arbitration window that allows arrivals from other approaches to register before right-of-way is decided
- A deterministic simulation-only gridlock breaker for circular priority conflicts, while still refusing to override earlier arrivals or simultaneous oncoming left-turn conflicts
- Vehicle release after clearing the conflict area
- Cleanup when an agent is disabled

A manual validation matrix is maintained in DESIGN/FOUR_WAY_STOP_TEST_MATRIX.md; its cases are not yet executed.

## Current limitations

This is still a greybox test, not a complete traffic simulation. Two sample vehicles per approach are spawned side by side to exercise same-approach simultaneous registration and exclusive reservation. This is still not a realistic queue model; the vehicles share a movement route and do not yet model lane-specific turn permissions or following distances. The four sample routes cover straight, left, and right movement types, but do not test every movement from every approach. Pedestrian crossing geometry and occupancy sensors are now present, and the controller tracks multiple pedestrian actors without releasing the gate until all tracked actors leave. The scene now spawns four simple looping capsule pedestrians. Their movement, curb waiting, trigger occupancy, and vehicle yielding are implemented at source level but remain unverified in Unity. There is no stop-sign dwell timer, no obstacle avoidance, and no automated test harness. The controller serializes all traffic to one vehicle at a time. A 0.25-second arbitration delay is used to collect near-simultaneous arrivals before applying arrival-time and right-side priority. If waiting vehicles form a circular priority deadlock, the controller uses a stable approach/vehicle-ID tie-break among vehicles that do not violate an earlier arrival or an oncoming left-turn conflict. This is a simulation safeguard, not a claim about Florida statutory right-of-way.

The route loops are simple waypoint paths intended for prototype observation. The stop-line waypoint now remains active until the vehicle obtains its reservation, preventing the previous waypoint-radius shortcut from skipping the stop request. Each approach also has a separate pedestrian yield line before its crosswalk; vehicles apply braking while a crossing is occupied. No Unity compilation, play-mode verification, collision/traffic-flow test, or iPhone profiling has been completed.
