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

A traffic agent must detect a complete stop, maintain the stopped-speed threshold continuously for the configured 1.25-second minimum dwell, call RegisterStoppedVehicle with a stable nonzero vehicle ID, approach, movement, and stop time, call TryAcquireIntersection until granted, enter only after permission, release only after its rear clears the conflict area, and call RemoveVehicle when despawning.

Use one consistent clock for stoppedAt (for example Time.time). A stable vehicle ID can be GetInstanceID() for the lifetime of the object.

## Safety and known limits

This is a conservative prototype scaffold integrated with CountyTrafficVehicleAgent in a separate four-way-stop test junction. It is not the active signalized intersection in the main Ocala greybox. It serializes all entries, including movements that might safely proceed together. The traffic agent now uses a forward sphere-cast to detect vehicles and raised static obstacles, applying minimum clearance, time-headway, and braking-distance speed limits. It exposes an emergency designation that gives priority over ordinary waiting vehicles at the four-way stop, without pre-empting an occupying vehicle or bypassing pedestrian occupancy. The test spawner uses lane-specific movement paths with two vehicles queued in each inbound lane. Stop-line, clearance, and crosswalk positions are converted into world space through the junction transform. The agent now requires a configurable 1.25-second full-stop dwell before requesting a reservation and removes its waiting record if it rolls forward before receiving permission. It still does not model obstacle steering/lane changes, uncontrolled-road gap acceptance, robust deadlock recovery, pedestrian animation, emergency lights/sirens, dispatch activation, or a spawned emergency fleet. It is not a substitute for a complete legal traffic simulation.

The existing signalized intersection remains the only active traffic intersection in the current greybox.