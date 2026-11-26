# Ocala First Playable Systems

## Purpose

This document turns the living-world direction into an implementation order for the first playable Marion County / Ocala slice. The target is a connected simulation, not a collection of disconnected feature demos.

## First playable boundary

Build a compact district around an Ocala-style downtown and adjoining residential/commercial streets. Keep the initial playable area small enough for stable performance on iPhone 16e. The 30 × 30 km multi-county world remains the long-term target; it is not loaded as one scene.

## Core loop

1. Start a shift at the sheriff office.
2. Check the time, weather, radio, and available units.
3. Patrol streets where civilian and traffic activity is running.
4. Receive a dispatch call with a location, priority, and short description.
5. Respond, interact with involved people, and secure the scene.
6. Record the outcome in an incident report.
7. Return to patrol; the world continues to evolve.

## Systems and dependencies

### A. World clock and weather
- One authoritative in-game clock drives lighting and civilian schedules.
- Time advances at a configurable rate; pause it in menus and during editor debugging if required.
- Weather states: clear, cloudy, rain, fog.
- Weather changes ambient lighting, sun intensity, fog, and later road surface/audio/visibility.
- Rain particles, thunder, wet-road materials, puddles, and weather forecasting are later visual/audio layers, not implied by the current greybox controller.

### B. Civilian routines
- Each civilian has a small daily schedule: home, work, shopping, leisure, home.
- Schedules use game time, not real-world time.
- Citizens should not all use the same schedule; add deterministic variation by household/job archetype.
- Current prototype movement is direct waypoint movement for testing. Production pathfinding must use baked navigation, obstacle avoidance, sidewalks, and crossing rules.
- Off-screen citizens should be simulated cheaply using schedule state rather than full animation and physics.

### C. Traffic and road behaviour
- Vehicles follow lane paths, stop at junctions, obey traffic signals, yield to emergency vehicles, and avoid collisions.
- Traffic density changes by time and district.
- Distant traffic uses low-cost simulation; nearby traffic gets full movement and collision behaviour.
- No traffic should spawn inside the player, buildings, or intersections.

### D. Dispatch and incident lifecycle
- Calls have type, priority, location, involved entities, and status.
- Status sequence: Available → Accepted → En Route → On Scene → Resolved / Cancelled.
- Calls are generated from world events (collision, disturbance, shop alarm, welfare check) as well as scripted tutorial events.
- Completing a call creates an incident record with actions, people, evidence, and result.
- Avoid random call spam. Use cooldowns, district weights, and active-unit counts.

### E. Civilian interaction and consequences
- Interaction options depend on context: greet, ask questions, request identification, warn, cite, detain, arrest, assist.
- Nearby civilians can register an incident as seen and heard or heard nearby, with a confidence estimate and basic subject description.
- Witness observations are stored separately from officer incident reports and can later feed an MDT interview/investigation workflow.
- Witnesses should report incidents only when they are within hearing range; visual identification requires a forward field-of-view check. Production needs obstruction checks and more detailed perception.
- NPC reactions depend on threat, trust, stress, prior interactions, and nearby police presence.
- Consequences must be proportionate and recorded; no arbitrary instant wanted-level changes.

### F. Mobile immersion
- Touch-first movement, camera, interaction wheel, radio, lights, siren, and dispatch controls.
- Keep essential controls reachable with thumbs; allow HUD scaling and left/right-handed layouts.
- Use vibration sparingly for radio/collision/interaction feedback.
- First-person and third-person views must share the same interaction and movement state.

## Initial content for Ocala slice

- Sheriff office and parking lot.
- Downtown-style main street with storefronts and sidewalks.
- Residential street with varied Florida homes and driveways.
- Gas station / convenience store.
- Small park and roadside businesses.
- Highway/arterial connection and rural road edge.
- Pedestrians with home/work/shop/leisure schedules.
- A small set of civilian vehicles and patrol units.
- Introductory calls: suspicious vehicle, minor collision, noise complaint, welfare check, shoplifting report.

## Performance budgets for iPhone 16e

These are targets to profile, not claims of measured performance:
- 30 FPS minimum target in the first playable slice, with a 60 FPS quality option if stable.
- Keep nearby active civilians and vehicles capped; simulate distant entities abstractly.
- Pool pedestrians, vehicles, callout markers, and effects.
- Avoid per-frame allocations in routine/traffic loops.
- Use LODs, baked lighting, occlusion culling, texture compression, and streamed district chunks.
- Profile on physical iPhone hardware before increasing NPC count or world size.

## Acceptance criteria

The first playable milestone is complete when:
- The game clock visibly advances and can be set for testing.
- Weather can be changed and affects lighting/fog.
- At least three civilian archetypes follow distinct schedules between assigned destinations.
- The player can patrol and receive/accept a call.
- A call can be marked on-scene and resolved with a basic report record.
- The world continues to run while the player is away from the sheriff office.
- The scene holds the agreed mobile frame-rate target on iPhone 16e.

## Current implementation notes

- `Assets/Scripts/World/WorldClockSystem.cs` provides the first clock/weather foundation.
- `Assets/Scripts/World/CivilianRoutineAgent.cs` provides schedule selection and simple waypoint movement for greybox testing.
- `Assets/Scripts/World/CountyWorldBootstrap.cs` now creates two placeholder residents with different home/work/shop/park schedules, connects the world clock to the generated directional sun, and attaches witness agents.
- `Assets/Scripts/Systems/WorldIncidentBus.cs` defines incident types and a shared event channel; `WorldIncidentReporter` is the scene entry point for gameplay systems to publish events.
- `Assets/Scripts/World/CivilianWitnessAgent.cs` checks nearby incidents against a hearing radius and forward visual cone, then records a witness statement.
- `Assets/Scripts/Systems/WitnessStatementBook.cs` stores session-only statements and raises an event for a future MDT UI.
- This is not yet an autonomous crime/dispatch system: nothing currently publishes incidents on its own. The current visual check does not perform raycast occlusion, and witness statements are not saved between sessions.
- These scripts are foundations only. They do not yet provide NavMesh pathfinding, animation, traffic AI, rain VFX, or incident persistence.
