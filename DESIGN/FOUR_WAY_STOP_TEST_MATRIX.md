# Four-Way-Stop Validation Matrix

This is the manual validation checklist for the isolated Ocala greybox junction. It is a specification for Unity Play Mode testing, not evidence that the tests have passed.

## Priority cases

| Case | Setup | Expected result |
|---|---|---|
| Earlier arrival | North stops, then West stops | North receives the first reservation |
| Simultaneous, vehicle on right | North and West stop within 0.15 seconds | North yields to West (West is on North's right) |
| Simultaneous, vehicle on left | North and East stop within 0.15 seconds | North does not yield to East under the right-side rule |
| Earlier left-turn arrival | East left-turn stops before South straight-through | East keeps first-arrival priority; later South arrival must not override it solely because of the left-turn conflict |
| Simultaneous left-turn conflict | East left-turn and South straight/right movement stop together | East yields to the oncoming conflicting movement |
| Pedestrian occupies crosswalk | One or more pedestrian IDs enter sensor | New vehicle reservations remain blocked until all tracked pedestrians exit |
| Two pedestrians overlap | Two IDs enter; first exits while second remains | Gate remains closed until second exits |
| Lane-specific movement assignment | Each approach has a left-turn lane, through lane, and right-turn lane | Vehicles follow the matching movement path and remain in their assigned inbound lane |
| Turn-path separation | All three movements operate at one approach | Left, straight, and right paths remain distinct and do not collapse into one waypoint line |
| Outbound lane alignment | Vehicle completes a left or right turn | Vehicle exits aligned with the destination road and does not reverse direction or cut across a curb |
| 24-vehicle test load | Two vehicles spawn in each of three lanes on all four approaches | All 24 agents initialize, maintain queue spacing, and do not overlap at spawn |
| Same-approach queue | Two vehicles approach in one inbound lane | Rear vehicle maintains a safe gap, does not overlap the lead vehicle, and reaches the stop line only after the lead vehicle clears |
| Stop-sign clearance | Vehicle approaches a stop bar with the roadside STOP sign present | The sign does not block the lane or cause premature braking before the stop line |
| Static obstacle response | Place a raised static obstacle in a vehicle's lane | Vehicle reduces speed and stops with the configured clearance; it must not treat the road surface as an obstacle |
| Non-blocking ground geometry | Place a low ground-level collider near the lane | Vehicle ignores low ground geometry and continues |
| Emergency priority | North through-lane lead sedan (marked with static red/blue lightbar) and ordinary vehicles arrive at the stop | Emergency vehicle receives priority after its stop dwell; ordinary traffic waits |
| Emergency safety gate | Keep another vehicle in the junction or a pedestrian in the crosswalk | Emergency vehicle cannot pre-empt an occupant or bypass pedestrian occupancy |
| Following-distance response | Lead vehicle slows or stops before the junction | Rear vehicle reduces speed using headway and braking limits without contacting the lead vehicle |
| Full-stop dwell | Vehicle reaches the stop line and remains below the stopped-speed threshold | Reservation is not requested until 1.25 seconds of continuous stopped time has elapsed |
| Rolled stop reset | Waiting vehicle rolls forward beyond the stop line or accelerates before reservation | Its waiting record is removed and a new full-stop dwell is required |
| Stop-line bumper clearance | Vehicle completes a stop at the stop bar | Front bumper remains behind the stop bar |
| Crosswalk bumper clearance | Pedestrian occupies the crossing and vehicle stops | Front bumper remains behind the crosswalk yield line |
| Junction transform | Move or rotate the test junction root | Stop-line, clearance, and crosswalk reference points remain aligned with the junction |
| Occupant cleanup | Waiting or occupying vehicle is disabled | Its queue entry or reservation is removed |
| Gridlock fallback | All approaches form a circular simultaneous priority conflict | Stable fallback selects a non-earlier-arrival vehicle without overriding an oncoming left-turn conflict |

## Test procedure

1. Open the project in the configured Unity Editor version and enter Play Mode.
2. Observe the isolated stop junction at the far end of the Ocala greybox.
3. Confirm every test vehicle reaches a complete stop before requesting entry.
4. Use the Inspector or temporary debug logging to inspect WaitingVehicleCount, OccupyingVehicleId, and pedestrian occupancy transitions.
5. Repeat each priority case with controlled arrival times; the current scene's autonomous vehicles alone do not guarantee every case.
6. Verify that a reservation is released only after the vehicle clears the conflict area.
7. Repeat with multiple pedestrians in the same crosswalk sensor.

## Current test limitations

- No automated Unity Test Framework suite is configured in the project manifest.
- The current runtime spawner creates representative traffic, not deterministic arrival-time scenarios.
- The controller's arbitration delay, 1.25-second stop dwell, and priority logic are source-level only.
- The vehicle/obstacle probe is a simple sphere-cast prototype; it has not been tuned against Unity collider layouts or dense traffic.
- Emergency priority is source-level only; one marked emergency test sedan is spawned with a static lightbar; flashing, siren/audio, and dispatch-to-agent activation are not implemented.
- The lane-specific test junction now uses a 22 m road width, three 3.4 m inbound movement lanes per approach, cubic Bezier turn paths, and two queued vehicles per lane (24 total). Route geometry and outbound lane alignment remain unverified in Unity.
- No compilation, Play Mode, physics, or iPhone runtime test has been run.
- A deterministic deadlock fallback is a prototype continuity safeguard, not a statement of Florida statutory right-of-way.
