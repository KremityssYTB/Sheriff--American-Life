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
- **Ambient county incident director** with weighted collision, theft, disturbance, fire, medical, suspicious-activity, and assault reports
- **Physical greybox incident scenes** with collision wrecks/debris, fire and smoke particles, a prone medical patient, theft-scene actors, and disturbance participants
- **Civilian traffic prototype** with eight Rigidbody vehicles on a signalized junction plus a separate 24-vehicle four-way-stop test junction, visible stop signs and bars, stop-line braking, reservation-based entry, four full-road-width marked pedestrian crossings, four looping pedestrian test actors, multi-occupant trigger tracking, curb waiting, pedestrian-yield braking, three movement-specific inbound lanes per approach (left-turn, through, right-turn), cubic turn paths, and longitudinal two-car queues with speed/headway-based following-distance control, forward braking for raised static obstacles, a 1.25-second full-stop dwell before reservation requests, rolled-stop queue cleanup, emergency-vehicle reservation priority (without pre-emption or pedestrian bypass), and vehicle-to-vehicle collision reporting into dispatch
- **Civilian 911 call source** attached to test residents, with caller identity, location, and per-caller cooldown
- **Mobile MDT controller and runtime touch-screen prototype** for call selection, status actions, linked witness statements, and incident reports
- **Florida intersection priority rules and four-way-stop entry controller** with stopped-vehicle registration, arrival-order/right-side priority checks, simultaneous opposing left-turn conflict checks, exclusive intersection reservation with a short simultaneous-arrival arbitration window, and a pedestrian-conflict gate (integrated into the isolated four-way-stop test junction; the original signalized junction remains separate)
- Ocala first-playable system plan
- Mobile MDT interface design and controller foundation

The current world blockout is still placeholder geometry. It is not yet an accurate Ocala map, and the clock, civilian, and witness scripts have not yet been profiled on an iPhone. Civilian routines currently use direct waypoint movement. Witness detection is a simple distance/field-of-view prototype: sound obstruction, visual line-of-sight, memory decay, interviews, and persistent records remain future work. Dispatch automatically creates a call when a gameplay system publishes a world incident. The ambient scenario director now publishes timed sample reports, and civilian 911 call sources expose callable report methods. Ambient incidents now spawn visible greybox scene actors before dispatch publication. They are static placeholders: traffic currently uses eight test vehicles at one greybox signalized intersection and 24 at a separate four-way-stop test junction. The stop-sign junction now includes representative straight, left-turn, and right-turn routes, but three inbound movement lanes per approach (left-turn, through, right-turn), with two vehicles queued longitudinally in each lane; four looping pedestrian actors now exercise the crosswalk triggers; actors wait at the curb while a vehicle occupies the junction, and approaching test vehicles brake before an occupied crosswalk, and rear vehicles reduce speed to maintain a braking- and headway-based gap behind traffic ahead; stop and crosswalk stopping offsets account for the vehicle's front bumper. Stop-line, clearance, and crosswalk reference points are converted through the junction transform. This is source-level behavior and remains uncompiled/untested; the individual STOP signs are placed roadside outside the forward vehicle probe paths. Adaptive signals, steering/lane-change obstacle avoidance, emergency lights/sirens, dispatch activation of emergency agents, and vehicle occupant injuries are not implemented. A Florida intersection-priority policy helper, four-way-stop reservation controller, optional traffic-agent integration, and isolated four-approach test junction now exist. The original greybox junction remains signalized; the new stop-sign junction is active as a separate test area. Fire spread/extinguishing, patient treatment, suspect AI, EMS/firefighter AI, and civilian phone UI are also still not implemented. Dispatch calls, reports, and witness statements are session-only. The MDT now includes a runtime-generated Canvas screen with active calls, case details, witness/report tabs, status actions, and a floating open/close button. Dispatch resolution and cancellation now create reports linked to the originating incident, allowing the MDT to show a call's related records.

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

## Project continuity

- [Project continuity handoff](PROJECT_CONTINUITY.md) — current implemented systems, recent commits, verification limits, and next development steps for resuming work across conversations.

## Traffic prototype documentation

- [Four-way-stop controller](DESIGN/FOUR_WAY_STOP_CONTROLLER.md)
- [Four-way-stop manual validation matrix](DESIGN/FOUR_WAY_STOP_TEST_MATRIX.md)
- [Traffic queue and following-distance prototype](DESIGN/TRAFFIC_QUEUE_AND_FOLLOWING.md)

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
2. Run the manual cases in DESIGN/FOUR_WAY_STOP_TEST_MATRIX.md in Unity, validate crosswalk and simultaneous-arrival behavior, validate longitudinal queue spacing and following behavior, verify the full-stop dwell and rolled-stop reset, expand the stop-junction route matrix, validate the new three-lane turn geometry and 24-vehicle traffic behavior, then validate static-obstacle braking and emergency priority, add emergency vehicle models/lights/sirens and dispatch activation, then implement safe obstacle steering and lane changes; add suspect/pedestrian AI, patient treatment, fire response, and civilian 911 phone UI
3. Connect dispatch resolution to reports, evidence, and legal grounds
4. Build the rendered mobile radio/MDT screen from `DESIGN/MOBILE_MDT_INTERFACE.md`
5. Mobile controls, animation, audio, and performance profiling
6. Expand county-by-county

All art, audio, and models must be original or properly licensed.
