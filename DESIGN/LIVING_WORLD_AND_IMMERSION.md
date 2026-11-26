# SHERIFF: AMERICAN LIFE — Living World & Immersion Specification

## Product goal

Make the player feel as if they live in a persistent, believable Florida world. SHERIFF is not only a patrol simulator: it is a first-person/third-person life and public-safety sandbox where ordinary routines continue whether or not the player is watching.

The world must remain responsive and readable on iPhone 16e. Simulation detail is prioritized near the player; distant activity is simplified.

## Core design principles

1. Independent world activity: pedestrians, traffic, shops, service workers, and emergency services continue their routines without requiring player interaction.
2. Believable cause and effect: player actions can create witnesses, reports, calls, investigations, fines, arrests, reputation changes, and follow-up events.
3. Everyday life matters: non-police activities are first-class gameplay, not decorative menu options.
4. Florida identity: use recognizable public geography, regional architecture, vegetation, road patterns, weather, and place names. Create original models, textures, signs, logos, dialogue, and audio; do not copy proprietary game assets.
5. Simulation with budgets: use distance-based update rates, pooling, streaming, LOD, baked lighting, and event-driven AI to protect mobile performance.
6. Player agency: avoid forcing every session into a police callout. The player can patrol, work, explore, drive, shop, or simply live in the world.

## The living-world systems

### 1. Time, weather, and atmosphere

- Continuous local clock with sunrise, sunset, and night lighting.
- Weather states: clear, cloudy, rain, thunderstorms, fog, and post-storm conditions.
- Wet roads and reduced visibility during rain; weather changes pedestrian and traffic behavior.
- Ambient sound layers by location and time: insects, birds, traffic, neighborhood activity, business interiors, radio chatter, and storms.
- Calendar and event hooks for weekends, holidays, school days, and local events.
- Use a scalable time system so long sessions do not require expensive per-frame updates.

### 2. Civilian population and routines

NPCs should have lightweight identities and schedules:
- Home, work, school, shopping, recreation, and return-home routines.
- Different behavior profiles for residents, visitors, workers, students, drivers, and service staff.
- Contextual reactions to sirens, crashes, dangerous driving, violence, police presence, and severe weather.
- Witnesses may call 911, leave, record an incident, or provide a statement.
- NPC memory should store only relevant recent events and decay over time.
- Use deterministic schedule templates and seeded variation rather than expensive full-life simulation for every NPC.

### 3. Traffic and road behavior

- Cars, pickups, SUVs, motorcycles, buses, delivery vehicles, and emergency vehicles.
- Lane following, traffic lights, stop signs, yielding, turning, parking, and route selection.
- Drivers react to collisions, blocked roads, sirens, pursuits, and road hazards.
- Traffic density varies by district, time, weather, and events.
- Traffic violations can be detected by patrol systems and reported by witnesses.
- Distant vehicles use simplified movement; detailed driving AI runs only near the player.

### 4. Businesses and civilian life

- Businesses have opening hours, staff, customers, deliveries, and closing routines.
- Enterable priority locations: sheriff station, convenience store, gas station, diner, grocery store, hospital, bank, and selected homes.
- Interactions: enter/exit, browse, buy, pay, refuel, use ATM, speak to staff, and access services.
- Jobs and activities can provide money and unlock vehicles, housing, and services.
- Store inventory and prices should be data-driven.
- Begin with a small number of fully interactive businesses; expand through reusable business templates.

### 5. Homes, vehicles, and personal progression

- Player home or starting residence with sleep/time-skip, wardrobe, storage, and vehicle access.
- Civilian vehicles with interiors, doors, lights, engine state, fuel, damage, and basic maintenance.
- Vehicle ownership, garages, purchase/sale, and persistent customization.
- Player money, job status, licenses, fines, and selected personal records.
- Police career progression is separate from civilian finances and should not require pay-to-win systems.

### 6. Sheriff and emergency response

Adapt the broad gameplay loop associated with police simulation:
**Patrol → Dispatch → Respond → Assess → Act → Document → Return to service.**

Systems:
- Dispatch queue with priority, location, call details, and updates.
- Callout generator with authored scenarios and controlled procedural variation.
- Traffic stops, citations, warnings, DUI investigations, pursuits, arrests, transport, and booking.
- Suspect compliance, flight, resistance, surrender, and negotiation states.
- Witness interviews, evidence collection, photographs, scene control, and case reports.
- Backup requests, radio channels, supervisor support, fire/EMS response, and tow services.
- MDT with call history, vehicle/plate/person lookup, warrants, and report forms.
- Bodycam and dashcam as optional immersive review features, not actual device recording.
- Consequences for excessive force, unsafe driving, missed calls, and poor procedure.

### 7. World persistence and consequences

Persist important state:
- Player position, time, owned vehicles, money, selected job, and unlocked locations.
- Business state and selected world events.
- Active callouts and mission outcomes where safe to restore.
- Evidence and case status.
- Recent NPC witness memories and wanted/suspect state.

Do not persist every pedestrian or traffic car. Rebuild ambient populations from schedules and world seeds after loading. Save atomically and version save data.

### 8. Audio, animation, and interaction

- Contextual footstep surfaces, doors, vehicle sounds, sirens, radio, rain, and interiors.
- NPC idle, walking, working, talking, driving, reacting, and emergency animations.
- First-person hands/interaction animations where affordable; third-person character animation remains readable.
- Use a contextual interaction prompt and radial action wheel.
- Touch controls must be large, adjustable, and usable one-handed; provide controller support later without requiring it.
- Avoid long mandatory animation sequences. Allow skip/quick interactions for repetitive actions.

## World implementation: Florida county rollout

Build counties in the agreed order, beginning with Marion County and Ocala. Each county should have a playable downtown, residential areas, rural roads, recognizable landmarks, emergency services, and a set of enterable locations before expanding outward.

1. Marion County — Ocala, Silver Springs, Belleview, Dunnellon, McIntosh, Ocklawaha, farms, horse properties, pine forests, wetlands, lakes, and Fort King.
2. Hillsborough County — Tampa, including Ybor City as a distinct historic district, Downtown Tampa, Channelside, West Tampa, and outskirts.
3. Orange County — Orlando.
4. Miami-Dade County — Miami.
5. Lee County — Fort Myers.
6. Volusia County — Daytona Beach.
7. Duval County — Jacksonville.
8. Leon County — Tallahassee.

Long-term world target: 30 km × 30 km, developed incrementally. The first playable vertical slice should be much smaller and polished; do not attempt to load the entire target world at once.

## Mobile performance architecture

- Stream world chunks around the player; unload or sleep distant chunks.
- Use LOD groups, occlusion culling, baked lighting, reflection probes, and texture atlases.
- Pool vehicles, pedestrians, effects, and UI objects.
- Use event-driven AI and staggered updates; avoid running every NPC decision every frame.
- Use NavMesh or waypoint graphs for nearby pedestrians and vehicles, with simpler route-following for distant traffic.
- Use a fixed simulation tick for traffic/NPC logic and interpolate visuals.
- Expose quality settings for traffic count, pedestrian count, shadows, reflections, and effects.
- Profile on iPhone 16e early; target stable frame pacing before increasing population density.

## Recommended implementation architecture

Keep systems modular and data-driven:
- WorldClock — local time, calendar, time scaling.
- WeatherController — weather state and environmental effects.
- WorldStreamer — county chunks, loading, unloading, and spawn zones.
- CivilianScheduleSystem — daily routine templates and spawn scheduling.
- PedestrianBrain — nearby civilian behavior and reactions.
- TrafficManager — traffic spawning, routing, density, and incident response.
- BusinessManager — hours, staff, customer flow, inventory, and interactions.
- PlayerLifeManager — money, jobs, home, licenses, and personal progression.
- VehicleManager — ownership, driving state, fuel, damage, and garages.
- DispatchManager — emergency calls and radio queue.
- CalloutManager — authored scenarios and procedural variation.
- IncidentManager — crimes, collisions, witnesses, evidence, and consequences.
- SaveSystem — versioned persistent state and safe saves.
- AmbientAudioManager — location/time/weather audio layers.

Prefer ScriptableObjects or serializable data assets for schedules, businesses, callouts, vehicle definitions, and dialogue. Systems should communicate through events/interfaces rather than hard-coded scene references.

## Development phases and acceptance criteria

### Phase 1 — Immersive Marion County vertical slice
- One compact, recognizable Ocala-area district with connected roads.
- Player can walk, enter/exit a vehicle, drive, and switch camera modes.
- Time-of-day cycle, basic weather, ambient audio, and traffic/pedestrians.
- One working gas station or convenience store and one sheriff station.
- One dispatch callout with witness response and a completed report.
- Save/load of player and vehicle state.
- Must run on iPhone 16e with adjustable population and graphics settings.

### Phase 2 — Daily-life simulation
- Home, job, money, shopping, fuel, vehicle ownership, and business schedules.
- NPC daily routines and witness memory.
- More enterable locations and civilian activities.

### Phase 3 — Full public-safety loop
- Traffic stops, pursuits, backup, arrests, evidence, MDT, reports, and fire/EMS.
- More varied callouts and meaningful procedural consequences.

### Phase 4 — County expansion
- Expand Marion County first, then follow the county rollout order.
- Add districts and landmarks only after streaming and performance budgets are proven.

## Quality bar

A feature is not complete merely because a button or menu exists. It must have a working interaction, appropriate feedback, persistence where relevant, and believable reactions from the world. Prioritize a smaller world that feels inhabited over a huge empty map.
