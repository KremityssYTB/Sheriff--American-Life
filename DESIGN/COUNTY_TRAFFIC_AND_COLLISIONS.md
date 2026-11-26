# County Traffic and Collision Prototype

## Implemented

`CountyTrafficVehicleAgent` provides a simple Rigidbody-based civilian traffic vehicle:

- Follows a configurable loop of waypoint transforms.
- Uses acceleration and braking forces to approach a target speed.
- Steers toward the next waypoint and damps lateral velocity.
- Uses continuous dynamic collision detection and interpolated Rigidbody movement.
- Detects vehicle-to-vehicle impacts above a configurable relative-speed threshold.
- Reports a collision through `WorldIncidentReporter`, which creates a physical collision scene and publishes a CAD call.

The Ocala bootstrap currently spawns four sedan-shaped greybox vehicles on two opposing Main Street lanes.

## Limitations

- This is a short, fixed test road, not a county road network.
- Vehicles are primitive boxes, not drivable or licensed vehicle models.
- There are no traffic lights, stop signs, lane changes, intersections, pedestrian crossings, yielding, or traffic rules.
- Waypoint loops make simple U-turns at their endpoints.
- Collision reporting is limited to traffic-vehicle impacts; there is no occupant injury model, vehicle deformation, tow service, or insurance system.
- AI avoidance and path replanning are not implemented.
- Traffic density and physics cost have not been profiled on iPhone.

## Next engineering steps

1. Add intersections, traffic signals, stop/yield rules, and obstacle avoidance.
2. Add civilian vehicle occupants and injury states.
3. Connect collision scene actors to evidence, witnesses, and report workflows.
4. Add player-controlled driving and a shared vehicle handling model.
5. Profile physics and traffic density on the iPhone target.
