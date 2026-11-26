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

This is a conservative prototype scaffold integrated with CountyTrafficVehicleAgent in a separate four-way-stop test junction. It is not the active signalized intersection in the main Ocala greybox. It serializes all entries, including movements that might safely proceed together. The traffic agent now includes a lightweight forward sphere-cast that finds the nearest vehicle ahead and caps speed using minimum gap, time headway, and braking distance. The test spawner uses two vehicles in one inbound lane, with the rear vehicle starting 16 m behind the lead vehicle. Stop-line, clearance, and crosswalk positions are converted into world space through the junction transform. It still does not model stop dwell time, lane-specific turn geometry, lane changes, uncontrolled-road gap acceptance, robust deadlock recovery, pedestrian animation, or emergency vehicle exceptions. It is not a substitute for a complete legal traffic simulation.

The existing signalized intersection remains the only active traffic intersection in the current greybox.