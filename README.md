# SHERIFF: AMERICAN LIFE

A Florida open-world law-enforcement and civilian life simulator. Target device: iPhone 16e. Engine: Unity 6.3 (6000.3.25f1) with URP 17.3.0.

## Core game identity and gameplay

SHERIFF: AMERICAN LIFE is an LSPDFR-inspired open-world roleplaying game, not merely a traffic or dispatch simulator. The player should be able to live in the Florida-inspired world: go on duty or off duty, walk and explore, enter and exit vehicles, patrol, conduct traffic stops, investigate incidents, interact with NPCs, issue citations, arrest and transport suspects, shop, visit businesses, and use homes and public services. First- and third-person play are planned.

Off-duty gameplay is explicitly part of the design. Players should be able to leave duty and freely explore, shop at stores, visit gas stations, banks, homes/apartments and other businesses, drive personal vehicles, and take part in civilian life. On-duty state governs department equipment, patrol assignment, radio/MDT, dispatch and duty-specific interactions.

The intended world includes gas stations, convenience stores, grocery/retail stores, banks, houses, apartments, businesses, sheriff/police facilities, fire stations, hospitals, jails, courthouses, parking areas, neighborhoods, highways, rural areas, farms, forests and lakes. NPCs should have routines and believable reactions, shop and drive, obey traffic laws, call 911, witness incidents, cooperate, flee or surrender according to scenario context. The initial game is single-player-first, with sheriff deputy gameplay as the first playable role; police, highway patrol, SWAT, detective, fire, EMS and civilian roles are longer-term targets.

See [Core Gameplay Scope](DESIGN/CORE_GAMEPLAY_SCOPE.md) for the complete requirements and vertical-slice order. These are design targets; they are not all implemented yet.

## Phone-playable browser preview

A separate lightweight browser preview is being added under [web-demo](web-demo/index.html), with touch movement, a patrol SUV, driving, on/off-duty toggle, simple dispatch calls, and location interactions. It is a temporary 2D playtest companion, not the Unity game build. The GitHub Pages deployment workflow is [deploy-web-demo.yml](.github/workflows/deploy-web-demo.yml). The first deployment is blocked because Pages is not enabled and the connected integration cannot enable it. In repository Settings → Pages, select **GitHub Actions** as the source, then rerun the workflow. The expected URL is https://kremityssytb.github.io/Sheriff--American-Life/ after a successful deployment.

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
- **Civilian traffic prototype** with eight Rigidbody vehicles on a signalized junction plus a separate 24-vehicle four-way-stop test junction, visible stop signs and bars, stop-line braking, reservation-based entry, four full-road-width marked pedestrian crossings, four looping pedestrian test actors, multi-occupant trigger tracking, curb waiting, pedestrian-yield braking, three movement-specific inbound lanes per approach (left-turn, through, right-turn), cubic turn paths, and longitudinal two-car queues with speed/headway-based following-distance control, forward braking for raised static obstacles, a 1.25-second full-stop dwell before reservation requests, rolled-stop queue cleanup, one marked emergency test sedan with dispatch-controlled alternating red/blue lightbar and generated two-tone siren (OCALA-1 activates on EN ROUTE, siren stops ON SCENE, equipment shuts off when resolved/cancelled), emergency-vehicle reservation priority (without pre-emption or pedestrian bypass), and vehicle-to-vehicle collision reporting into dispatch
- **Civilian 911 call source** attached to test residents, with caller identity, location, and per-caller cooldown
- **Mobile MDT controller and runtime touch-screen prototype** for call selection, status actions, linked witness statements, and incident reports
- **Florida intersection priority rules and four-way-stop entry controller** with stopped-vehicle registration, arrival-order/right-side priority checks, simultaneous opposing left-turn conflict checks, exclusive intersection reservation with a short simultaneous-arrival arbitration window, and a pedestrian-conflict gate (integrated into the isolated four-way-stop test junction; the original signalized junction remains separate)
- Ocala first-playable system plan
- Mobile MDT interface design and controller foundation
- **First runtime player slice (source prototype):** spawns a deputy and patrol SUV in the Ocala greybox, provides a simple touch-oriented IMGUI control surface for walking/sprinting and driving, first/third-person camera switching, vehicle enter/exit with input handoff, and a session duty-state toggle

The runtime Ocala bootstrap now adds `CountyPlayerRuntimeController`, which creates the player, patrol SUV, camera anchors, simple touch-oriented controls, enter/exit handoff, and a session-level `CountyDutyState`. Vehicle exit now checks four nearby ground positions and capsule clearance, leaving the player seated if all exit points are blocked; this safeguard remains untested in Unity. The older keyboard-only controllers remain isolated in `SheriffAmericanLife.Legacy`. This is a source integration prototype only: the IMGUI controls, camera, vehicle physics, and duty behavior have not been tested in Unity or on iPhone. Duty state currently changes and reports its state; dispatch/equipment gating is not yet connected. See [Player Vertical-Slice Architecture Audit](DESIGN/PLAYER_VERTICAL_SLICE_AUDIT.md) for current status and gaps.

The current world blockout is still placeholder geometry. It is not yet an accurate Ocala map, and the clock, civilian, and witness scripts have not yet been profiled on an iPhone. Civilian routines currently use direct waypoint movement. Witness detection is a simple distance/field-of-view prototype: sound obstruction, visual line-of-sight, memory decay, interviews, and persistent records remain future work. Dispatch automatically creates a call when a gameplay system publishes a world incident. The ambient scenario director now publishes timed sample reports, and civilian 911 call sources expose callable report methods. Ambient incidents now spawn visible greybox scene actors before dispatch publication. They are static placeholders: traffic currently uses eight test vehicles at one greybox signalized intersection and 24 at a separate four-way-stop test junction, including one marked emergency test sedan. The stop-sign junction now includes representative straight, left-turn, and right-turn routes, but three inbound movement lanes per approach (left-turn, through, right-turn), with two vehicles queued longitudinally in each lane; four looping pedestrian actors now exercise the crosswalk triggers; actors wait at the curb while a vehicle occupies the junction, and approaching test vehicles brake before an occupied crosswalk, and rear vehicles reduce speed to maintain a braking- and headway-based gap behind traffic ahead; stop and crosswalk stopping offsets account for the vehicle's front bumper. Stop-line, clearance, and crosswalk reference points are converted through the junction transform. This is source-level behavior and remains uncompiled/untested; the individual STOP signs are placed roadside outside the forward vehicle probe paths. Adaptive signals, steering/lane-change obstacle avoidance, dispatch-to-emergency activation beyond the single OCALA-1 test unit, and vehicle occupant injuries are not implemented. A Florida intersection-priority policy helper, four-way-stop reservation controller, optional traffic-agent integration, and isolated four-approach test junction now exist. The original greybox junction remains signalized; the new stop-sign junction is active as a separate test area. Fire spread/extinguishing, patient treatment, suspect AI, EMS/firefighter AI, and civilian phone UI are also still not implemented. Dispatch calls, reports, and witness statements are session-only. The MDT now includes a runtime-generated Canvas screen with active calls, case details, witness/report tabs, status actions, and a floating open/close button. Dispatch resolution and cancellation now create reports linked to the originating incident, allowing the MDT to show a call's related records.

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

## Design and validation documents

- [Core Gameplay Scope](DESIGN/CORE_GAMEPLAY_SCOPE.md)
- [Player Vertical-Slice Architecture Audit](DESIGN/PLAYER_VERTICAL_SLICE_AUDIT.md)
- [Four-way-stop controller](DESIGN/FOUR_WAY_STOP_CONTROLLER.md)
- [Four-way-stop manual validation matrix](DESIGN/FOUR_WAY_STOP_TEST_MATRIX.md)
- [Traffic queue and following-distance prototype](DESIGN/TRAFFIC_QUEUE_AND_FOLLOWING.md)

## Project setup

1. Open with Unity Editor 6000.3.25f1 and allow packages to resolve.
2. Run **SHERIFF → Setup Mobile URP Project**.
3. Open Assets/Scenes/Main.unity and press Play.
4. The current source prototype creates a basic touch-oriented control surface at runtime; verify it in Unity before treating it as usable.

## iOS cloud build

The recommended phone-only route is Unity Build Automation through the Unity Cloud Dashboard. It connects directly to this GitHub repository and supports iOS builds on hosted macOS builders. The free allocation published for 2026 includes 100 Mac build minutes per month. If the free allowance is exhausted and no payment method is on file, Unity blocks further usage; do not add billing details or enable pay-as-you-go.

See [BUILDING.md](BUILDING.md) and the private [Unity-License setup repository](https://github.com/KremityssYTB/Unity-License) for setup instructions. Unity Build Automation must still be connected and configured by the account owner in the Unity Cloud Dashboard. The first cloud build has not yet been run.

The iOS build process generates an Xcode project; Xcode then builds it into an app. A distributable/installable IPA requires a separate packaging and Apple-signing step. An unsigned IPA is not directly installable.

The older GitHub Actions/GameCI workflow remains an alternative, but it requires a valid Unity Personal license secret named `UNITY_LICENSE`. That secret is not configured. Unity Personal activation is through Unity Hub; do not attempt manual `.alf` activation.

## iOS configuration

- Bundle identifier: com.kremityss.sheriffamericanlife
- Architecture: ARM64
- Graphics API: Metal
- Scripting backend: IL2CPP
- Render pipeline: URP

## Development priorities

1. Compile and run the new runtime player slice; fix any Unity API, input, collider, or camera issues.
2. Replace temporary IMGUI controls with a polished touch HUD and add a real joystick/hold controls.
3. Improve vehicle physics, collision handling, and camera transitions; validate the new safe-exit checks in Unity.
4. Connect duty state to dispatch, MDT, and equipment; the first civilian ID/enforcement actions are now duty-gated while off-duty conversation remains available.
5. Expand the traffic-stop encounter with driver dialogue, ticket records, and MDT case linkage (pull-over, fictional driver records, license/registration/insurance checks, and warning/citation decisions are now in source).
6. Add one usable store/gas station and one home/interior to prove off-duty civilian gameplay.
7. Return to traffic simulation validation and expansion after the player vertical slice is stable.

All art, audio, and models must be original or properly licensed.


## Driver disposition and traffic-stop dialogue update (2026-11-26)

- Added `TrafficStopDriverBehavior` with four fictional dispositions: cooperative, nervous, argumentative, and fleeing.
- County traffic spawns receive weighted randomized dispositions: 50% cooperative, 25% nervous, 17% argumentative, and 8% fleeing.
- Fleeing drivers make a brief 1.8-second forward escape attempt before the existing pull-over steering/braking behavior resumes. This is a bounded prototype, not a pursuit system.
- Once stopped, the mobile HUD shows the driver's disposition and offers a calm or firm approach. The selected response is written to the session incident report.
- Ending the stop resets the behavior state. No real-world databases or external services are used.
- Source inspection only: Unity compilation, vehicle physics, UI layout, and iPhone runtime tests remain outstanding.


## Traffic citation ledger and MDT update (2026-11-26)

- Added `TrafficCitationBook` as a session-only ledger for fictional traffic citations. Records include unique citation number, driver name, license number, plate, issue timestamp, grid-based county location, officer unit, violation descriptions, fine total, and payment status.
- Citation issuance is triggered only after all three document checks are complete and the deputy chooses CITATION for a profile with a document violation. A warning does not create a ticket.
- Fictional violation categories currently include suspended/expired/missing license, expired/missing registration, and missing/expired insurance. Prototype fine amounts are gameplay placeholders, not statements of real Florida law or court schedules.
- The mobile MDT now has a TICKETS tab showing ticket details and outstanding/paid state. MARK PAID changes the in-game session record to Paid; no real payment is processed.
- Citation creation also adds a traffic-stop report entry. Citation records and payment state are saved locally as versioned JSON under Unity's persistent data path; they are not synced to a server or shared between devices.
- Source checks confirmed the new data fields, ticket issuance path, MDT wiring, and balanced braces. Unity compilation, Play Mode, UI layout, and iPhone tests remain outstanding.


## Persistent traffic citation save update (2026-11-26)

- The citation ledger now loads and saves a versioned JSON file in Unity's `Application.persistentDataPath`.
- Issuing a citation and changing its payment status both trigger a save. Citation numbers use a persisted sequence counter and are reconciled against loaded ticket numbers to avoid reusing IDs after relaunch.
- Loading ignores unsupported/empty save data and catches file/JSON errors without preventing the game from continuing with an empty in-memory ledger. Save failures are logged; current-session records remain available.
- Save contents are fictional gameplay records only. They remain local to the device and are not backed up to a server.
- Source inspection only. Unity compilation, JSON round-trip testing, storage behavior on iOS, and device testing have not been performed.


## Persistent repeat-driver history update (2026-11-26)

- Added `TrafficDriverHistoryBook`, a versioned local JSON ledger stored at `Application.persistentDataPath/traffic-driver-history.json`.
- The ledger identifies fictional drivers by normalized license number or plate, records warning and citation outcomes, and tracks total recorded stops, warning count, citation count, last contact, and recent event details.
- The traffic-stop warning path records a warning only when the deputy chooses WARNING. The citation path records a history event only when a citation is successfully created.
- The MDT now exposes a HISTORY tab showing driver identifiers, prior stop totals, warning/citation counts, last outcome, and up to five recent events per driver. Ticket text now correctly describes locally saved records.
- History is fictional and local to the device; it is not connected to DMV, court, insurance, or police databases. Records are capped at 500 drivers and 20 recent events per driver.
- Source-level verification only. Unity compilation, JSON round-trip, save migration, UI layout, and iPhone runtime tests remain outstanding.


## Arrest booking workflow update (2026-11-26)

- Added `Assets/Scripts/Systems/ArrestBookingBook.cs`, a versioned local JSON register saved at `Application.persistentDataPath/arrest-bookings.json`.
- A booking is created only when the existing civilian arrest action succeeds after detention. Failed arrest attempts and detentions alone do not create booking records.
- Each fictional booking stores a unique MCB number, civilian name and game ID, arrest reason, timestamp, placeholder property-custody inventory, and custody status (AwaitingTransport, Booked, Released).
- The MDT now has a BOOKINGS tab. A deputy can mark an awaiting booking as booked, then mark a booked person released. Invalid state transitions are rejected by the ledger.
- The current arrest reason is generated from the existing fictional warrant flag or the completed-detention path. Property entries are generic placeholders; this is not a real search, evidence chain, jail connection, or legal workflow.
- Booking records are local to the device, capped at 500, and are not synced. Load/save errors are caught and logged.
- Source inspection only. Unity compilation, JSON round-trip, UI layout, transport gameplay, and iPhone runtime tests have not been performed.


## Prisoner transport and jail intake update (2026-11-26)

- Added a greybox Marion County Jail Intake building and marked vehicle intake bay near the sheriff office in the Ocala blockout.
- The on-duty deputy must be in the patrol SUV, have an awaiting-transport booking, and drive within 18 m of the intake bay to complete transport.
- Successful transport advances the booking from AwaitingTransport to Booked through the persistent arrest-booking ledger and adds a jail-intake report entry. The patrol SUV is stopped after the transfer.
- The MDT BOOKINGS tab now displays awaiting-transport instructions instead of allowing the deputy to bypass the vehicle transport step. Release remains an MDT action after booking.
- This is a prototype vehicle-to-record custody transfer. The arrested civilian is not physically seated in the SUV, and there is no animated escort, prisoner AI, collision restraint, jail interior navigation, or real justice-system connection.
- Source inspection only; Unity compilation, scene layout, vehicle handling, persistence round-trip, and iPhone runtime remain untested.


## Physical prisoner escort update (2026-11-26)

- `OfficerInteractionController` now emits a successful-arrest event only after a booking record has been created.
- `CountyPlayerRuntimeController` binds the new arrestee to the matching awaiting-transport booking, disables the civilian's scheduled routine, and follows the deputy on foot.
- Entering the patrol SUV seats the civilian in a visible passenger position. The passenger collider is disabled while seated to avoid interfering with the prototype vehicle; it is restored when the civilian exits the vehicle.
- Exiting the SUV returns the civilian to a following position. Completing transport places the civilian at the jail intake staging area and advances only that civilian's matching booking to Booked.
- The transport HUD now requires the currently escorted civilian and matching booking; older or unrelated pending records cannot be completed through the active escort.
- Prototype limits: one escorted prisoner at a time; no handcuff animation, escort hand-hold, seatbelt/restraint animation, prisoner AI, rear-seat door interaction, jail interior navigation, or persistence of the live NPC's world position. Booking data remains local and persistent; NPC placement is session-only.
- Source inspection only. Unity compilation, animation, collider/vehicle interaction, scene layout, save/load, and iPhone runtime remain untested.


## Ocala visual foundation update (2026-11-27)

- Added `OcalaVisualDressing` and wired it into the runtime county bootstrap after the greybox town and civilian layout are generated.
- The first-pass dressing adds road center/edge markings, crosswalk stripes, sidewalks and curbs, building roof caps and facade windows/doors, a simple gas-station canopy and pump islands, street lamps, utility cabinets, Florida palms, broad-canopy trees, and distant rural tree belts.
- Materials use the URP Lit shader when available, with authored roughness/smoothness and limited metallic/emissive values. The pass reuses shared materials within the dressing component.
- This is a procedural blockout enhancement, not GTA 6-level art: buildings, vehicles, people, foliage, and many details are still primitive geometry; no photogrammetry, high-resolution texture sets, advanced reflections, or production lighting have been added.
- The new geometry has not been profiled. It may need batching/LOD/visibility work before mobile release. Unity compilation, Play Mode, visual inspection, and iPhone performance tests remain outstanding.


## Patrol SUV visual pass (2026-11-27)

- Added `PatrolSuvVisuals`, a procedural first-pass Marion County sheriff SUV model with a multi-piece body, raised cabin, tinted windows, trim, mirrors, door handles, running boards, wheel/tire assemblies, alloy faces, grille, lamps, bumpers, plates, roof antenna, and sheriff/county markings.
- The existing root vehicle object and simplified root collider remain in place so the current vehicle entry, movement, and interaction code continues to use the same transform. Detail-part primitive colliders are removed. Existing red/blue lightbar controller is retained and positioned above the roof.
- This is still placeholder procedural geometry and not a photoreal production vehicle. A licensed/original high-quality vehicle mesh, proper UVs and texture maps, physically accurate glass/paint, interior, suspension, wheel animation, and lighting/reflection work remain future tasks.
- No Unity compile, Play Mode, visual inspection, or mobile performance test has been run; the source changes are committed but not runtime-verified.


## Civilian traffic vehicle visual pass (2026-11-27)

- Added `TrafficVehicleVisuals` and applied it to generated civilian traffic cars. Sedan actors now have a lengthwise body/cabin silhouette, tinted glass, mirrors, side sills, four tire/alloy assemblies, headlamps, tail lamps, and bumpers instead of visible colored cubes.
- Traffic actor roots now use unit transform scale with a lengthwise box collider (1.9 wide × 1.25 high × 4.2 long); existing Rigidbody and waypoint AI components remain on the same root.
- This remains procedural placeholder geometry, not final vehicle art. No Unity compile, Play Mode, collision inspection, or iPhone performance test has been run.

- Follow-up: added a first-pass SUV cabin interior with dashboard, instrument hood, center screen/console, gear selector, steering column/wheel, front bucket seats, rear seating, and footwells. This is visible placeholder geometry, not a finished cockpit; no interior materials/textures, functional instruments, or runtime validation yet.


## Lighting and weather refinement (2026-11-27)

- Updated `WorldClockSystem` to configure the active directional sun as the scene sun, use soft shadows with tuned strength/bias, and apply subtle Exponential Squared fog colors/densities for Fog and Rain weather. This is a code-level lighting pass; visual appearance and shadow cost must be checked in Unity on target devices.
