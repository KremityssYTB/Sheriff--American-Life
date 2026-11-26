# Four-Way Stop Traffic-Agent Integration

## Agent support added

`CountyTrafficVehicleAgent` now has an optional four-way-stop configuration path. Existing signal-controlled agents are unchanged unless a `CountyFourWayStopController` is explicitly supplied.

Configure an agent with:
- Its shared intersection controller
- Its approach side and intended movement
- The stop-line position
- A clearance point beyond the conflict area

The agent uses a braking-distance speed target while approaching, registers only after reaching the stop-line tolerance at near-zero speed, waits for `TryAcquireIntersection`, and releases its reservation after reaching the configured clearance point. On disable it removes its vehicle ID from the controller.

## Important integration constraints

- Do not configure both `TrafficSignalController` and `CountyFourWayStopController` for the same junction. The existing Ocala intersection remains signalized.
- All four approaches at a stop-controlled junction must share the same controller instance.
- The clearance point must be beyond the intersection conflict area, not at the stop line.
- The stop-line coordinate and approach orientation must agree with the vehicle's forward direction.
- Routes must physically pass through the stop line and clearance point; the current two-point loop routes are not suitable as final turning routes.
- The controller is still conservative and grants one vehicle at a time.

## Validation status

This is source-level integration only. No Unity compilation, play-mode test, traffic-flow test, or iPhone performance test has been run. The feature is not enabled on the current greybox traffic vehicles.
