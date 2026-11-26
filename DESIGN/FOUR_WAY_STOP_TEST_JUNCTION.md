# Isolated Four-Way-Stop Test Junction

## Scene integration

`CountyFourWayStopTestSpawner` builds a second, isolated junction in the runtime Ocala greybox. It is placed away from the existing signalized intersection and creates:

- A separate north-south and east-west road crossing
- Four stop bars and four visible STOP sign assemblies
- One test sedan on each approach, with representative straight-through, left-turn, and right-turn movement assignments
- Four marked zebra crosswalks with trigger volumes and a shared `CountyFourWayStopController`
- One looping pedestrian test actor per crosswalk, walking between opposite sidewalks
- Pedestrian actors wait at the curb while a vehicle already occupies the junction; crosswalk triggers keep new intersection reservations closed while pedestrians are crossing
- Approaching vehicles brake at a dedicated yield line before the crosswalk while pedestrian occupancy is active
- Authored straight, left-turn, and right-turn waypoint paths with post-intersection clearance points and outer return loops

Each sedan receives its own approach enum, movement type, stop-line coordinate, curved turn waypoints where applicable, and clearance point. The current four-vehicle sample assigns north/south/east/west movements as straight/left/right/straight respectively. The traffic agent's optional stop-sign path is enabled only for these four test vehicles. The original eight signal-controlled vehicles remain on their existing signalized junction.

## Test purpose

This provides a scene-level integration target for observing:
- Full-stop approach and stop-line braking
- Registration of stopped vehicles
- Exclusive intersection reservations
- Vehicle release after clearing the conflict area
- Cleanup when an agent is disabled

## Current limitations

This is still a greybox test, not a complete traffic simulation. Only one sample vehicle per approach is spawned, so this is not yet a balanced traffic-flow test. The four sample routes cover straight, left, and right movement types, but do not test every movement from every approach. Pedestrian crossing geometry and occupancy sensors are now present, and the controller tracks multiple pedestrian actors without releasing the gate until all tracked actors leave. The scene now spawns four simple looping capsule pedestrians. Their movement, curb waiting, trigger occupancy, and vehicle yielding are implemented at source level but remain unverified in Unity. There is no stop-sign dwell timer, no obstacle avoidance, and no automated test harness. The controller serializes all traffic to one vehicle at a time.

The route loops are simple waypoint paths intended for prototype observation. The stop-line waypoint now remains active until the vehicle obtains its reservation, preventing the previous waypoint-radius shortcut from skipping the stop request. Each approach also has a separate pedestrian yield line before its crosswalk; vehicles apply braking while a crossing is occupied. No Unity compilation, play-mode verification, collision/traffic-flow test, or iPhone profiling has been completed.
