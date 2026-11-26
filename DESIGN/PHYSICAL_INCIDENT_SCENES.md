# Physical Incident Scene Actors

## Purpose

Ambient incident scenarios now create visible greybox scene actors before their dispatch report is published. This connects the CAD event to a physical location in the county scene.

## Implemented

`Assets/Scripts/World/CountyIncidentActorSpawner.cs` is attached by `CountyWorldBootstrap` and called by `CountyIncidentDirector`.

- Vehicle collision: two angled damaged-vehicle blockouts and debris.
- Fire: a structure blockout with looping flame/smoke particles and a point light.
- Medical emergency: a prone civilian placeholder and medical-bag placeholder.
- Theft / suspicious activity: a person placeholder and property/vehicle placeholder.
- Assault / disturbance: two person placeholders.
- Other incident types: a simple scene marker.

All calls through `WorldIncidentReporter` now use the same actor-spawning path, including ambient scenarios, manual scenario triggers, and civilian 911 reports. The actor is spawned before the incident is published to the dispatch event bus, so CAD and the MDT receive the same incident location.

## Important limitations

These actors are static visual representations. The collision scene does not yet simulate moving cars, impact forces, damage, traffic obstruction, or injuries. Fire has particles and a light but no spread, heat, extinguishing, or firefighter response. The medical placeholder has no health state or treatment interaction. Suspect and disturbance actors do not yet have behavior, animation, AI, or arrest/evidence links. The current positions are greybox coordinates and may overlap placeholder buildings.

This milestone closes the gap between a dispatch text entry and a visible scene, but it does not complete the underlying simulation systems.
