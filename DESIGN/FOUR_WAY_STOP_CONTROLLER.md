# Four-Way Stop Entry Controller

## Added prototype component

Assets/Scripts/World/CountyFourWayStopController.cs adds an intersection-entry arbiter on top of CountyIntersectionRules.cs.

It provides:
- Registration of vehicles after they have fully stopped.
- Preserved stop-arrival timestamps while a vehicle waits.
- Priority checks for earlier arrivals and near-simultaneous arrivals.
- Left-turn conflict checks against oncoming straight/right movements.
- Exclusive intersection occupancy: only one vehicle is granted entry at a time.
- Explicit release when the vehicle clears the conflict area.
- A pedestrian-conflict gate that blocks new entries while a crossing is occupied.
- Vehicle removal cleanup for despawned traffic.

## Integration contract

A traffic agent must detect and complete a full stop, call RegisterStoppedVehicle with a stable nonzero vehicle ID, approach, movement, and stop time, call TryAcquireIntersection until granted, enter only after permission, release only after its rear clears the conflict area, and call RemoveVehicle when despawning.

Use one consistent clock for stoppedAt (for example Time.time). A stable vehicle ID can be GetInstanceID() for the lifetime of the object.

## Safety and known limits

This is a conservative prototype scaffold and is not yet connected to CountyTrafficVehicleAgent or spawned in the live Ocala scene. It serializes all entries, including movements that might safely proceed together. It does not model stop-sign placement, stop duration, lane geometry, uncontrolled-road gap acceptance, deadlock recovery, pedestrian animation, or emergency vehicle exceptions. It is not a substitute for a complete legal traffic simulation.

The existing signalized intersection remains the only active traffic intersection in the current greybox.