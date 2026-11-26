# Florida Intersection Priority Rules

## Policy foundation

`Assets/Scripts/World/CountyIntersectionRules.cs` adds a deterministic, engine-independent rule helper for future traffic-agent integration.

It covers:

- Four-way stop arrival order: the first vehicle to stop has priority.
- Near-simultaneous arrivals: the vehicle approaching from the other driver's right has priority.
- Left turns: yield to oncoming vehicles proceeding straight or turning right.
- Pedestrian conflict: a vehicle must yield when a pedestrian occupies its conflicting crosswalk path.

The approach enum describes the side from which a vehicle arrives (for example, `North` means it approaches from the north and travels south). Simultaneous arrival tolerance is configurable.

## Scope and limitations

This is a policy helper only. It is not yet connected to `CountyTrafficVehicleAgent`, does not track stop-line dwell time or arrival queues, and does not control a live four-way-stop scene. It does not replace intersection occupancy checks, collision avoidance, signal control, or pedestrian animation.

The current live prototype remains the signalized four-way intersection described in `SIGNALIZED_INTERSECTION.md`.

## Next integration steps

1. Add a stop-sign intersection controller that records full-stop arrival timestamps per approach.
2. Have traffic agents request entry permission and reserve the intersection until they clear it.
3. Apply the priority helper for simultaneous arrivals and left-turn conflicts.
4. Add crosswalk occupancy checks and pedestrian crossing signals.
5. Add tests for all approach combinations, ties, and turning conflicts before enabling the system in the live scene.

## Florida references

- Florida Statutes §316.123: stop/yield intersections and four-way-stop arrival priority.
- Florida Statutes §316.122: left-turning vehicles yield to oncoming traffic that is an immediate hazard.
- Florida Statutes §316.075: traffic signal indications and pedestrian provisions.
