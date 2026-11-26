# SHERIFF: AMERICAN LIFE

A Florida open-world law-enforcement and civilian life simulator. Target device: iPhone 16e. Engine: Unity 6.3 (6000.3.25f1) with URP 17.3.0.

## Current prototype foundation

- Unity 6 project and mobile URP setup
- Runtime greybox county blockout
- Third-person movement and camera-mode toggle
- Basic patrol vehicle controller
- Dispatch call prototype
- **World clock and weather foundation** (clear, cloudy, rain, fog lighting states)
- **Schedule-driven civilian prototype** with configurable daily destinations
- **Civilian witness prototype** that records nearby incident observations (seen/heard, description, confidence, location)
- **World incident event channel** for gameplay systems to publish disturbances, collisions, thefts, fires, and emergencies
- **Session-only witness statement collection** ready for a future MDT/investigation screen
- **Incident-driven dispatch prototype** that converts published world events into CAD calls with priority and lifecycle tracking
- Ocala first-playable system plan

The current world blockout is still placeholder geometry. It is not yet an accurate Ocala map, and the clock, civilian, and witness scripts have not yet been profiled on an iPhone. Civilian routines currently use direct waypoint movement. Witness detection is a simple distance/field-of-view prototype: sound obstruction, visual line-of-sight, memory decay, interviews, and persistent records remain future work. Dispatch automatically creates a call when a gameplay system publishes a world incident. No crime simulation currently publishes incidents on its own; gameplay systems must call the incident reporter. Dispatch calls, reports, and witness statements are session-only.

## World direction

The long-term world is based on real Florida geography and developed county-by-county:

1. Marion County — Ocala, Silver Springs, Belleview, Dunnellon and surrounding rural areas
2. Hillsborough County — Tampa, including Ybor City
3. Orange County — Orlando
4. Miami-Dade County — Miami
5. Lee County — Fort Myers
6. Volusia County — Daytona Beach
7. Duval County — Jacksonville
8. Leon County — Tallahassee

The 30 × 30 km multi-county world is a long-term target. Development starts with a compact, streamed Ocala district rather than loading the full world at once.

## Project setup

1. Open with Unity Editor 6000.3.25f1 and allow packages to resolve.
2. Run **SHERIFF → Setup Mobile URP Project**.
3. Open Assets/Scenes/Main.unity and press Play.
4. Current keyboard prototype controls are documented in the relevant controller scripts; touch-first HUD work remains in progress.

## iOS build

The repository includes a GitHub Actions workflow for an unsigned iOS export. A valid Unity license is required for Unity batchmode. The iOS Xcode build stage requires a macOS runner with Xcode. The workflow has not yet been verified by a successful end-to-end build.

An unsigned IPA is not directly installable. It must be signed with a compatible certificate and provisioning profile before installation.

## iOS configuration

- Bundle identifier: com.kremityss.sheriffamericanlife
- Architecture: ARM64
- Graphics API: Metal
- Scripting backend: IL2CPP
- Render pipeline: URP

## Development priorities

1. Accurate Ocala district blockout and roads
2. Add gameplay event producers for collisions, alarms, disturbances, fires, and medical emergencies
3. Connect dispatch resolution to reports, evidence, and legal grounds
4. Build the mobile radio/MDT and witness interview workflow
5. Mobile controls, animation, audio, and performance profiling
6. Expand county-by-county

All art, audio, and models must be original or properly licensed.
