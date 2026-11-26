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
| Same-approach queue | Two vehicles register from one approach | Only one vehicle can occupy the intersection at a time |
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
- The controller's arbitration delay and priority logic are source-level only.
- No compilation, Play Mode, physics, or iPhone runtime test has been run.
- A deterministic deadlock fallback is a prototype continuity safeguard, not a statement of Florida statutory right-of-way.
