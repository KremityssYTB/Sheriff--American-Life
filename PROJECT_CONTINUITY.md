# Project Continuity Handoff

Last updated: 2026-11-26 (free Unity Build Automation setup path)

This file is the durable handoff for continuing SHERIFF: AMERICAN LIFE across ChatGPT conversations. The GitHub repository on `main` is the source of truth; inspect current file contents and SHAs before every edit.

## Project identity and constraints

- Project: SHERIFF: AMERICAN LIFE
- Repository: https://github.com/KremityssYTB/Sheriff--American-Life
- Branch: `main`
- Engine target: Unity 6.3 (`6000.3.25f1`) + URP 17.3.0
- Target device: iPhone 16e / A18, iOS 27.2.1 (current device OS)
- Bundle ID: `com.kremityss.sheriffamericanlife`
- User workflow: iPhone-only; no Mac, PC, or Chromebook available. Do not assume Unity Editor or Xcode can be run locally.
- Keep changes mobile-conscious and clearly distinguish source checks from Unity compilation, Play Mode, and device tests.

## World direction

Florida-inspired multi-county open-world law-enforcement and civilian-life simulator. Long-term 30 x 30 km streamed world, developed county-by-county. First playable area is Marion County / Ocala. Planned counties: Marion (Ocala), Hillsborough (Tampa including Ybor City), Orange (Orlando), Miami-Dade (Miami), Lee (Fort Myers), Volusia (Daytona Beach), Duval (Jacksonville), Leon (Tallahassee). First/third-person cameras; law enforcement, fire, EMS, and civilian gameplay.

## Core game vision and user requirements

The core game is an LSPDFR-style open-world roleplaying game with a highly detailed, lived-in Florida-inspired world. It is not just a traffic simulator or dispatch prototype. Preserve the full player-life loop as the highest-level design requirement.

- **On duty and off duty:** the player must be able to go on/off duty. Off duty allows free exploration, stores/businesses, homes, personal vehicles, and civilian activities. Duty state controls duty equipment, patrol assignment, radio/MDT, dispatch, and role-specific actions.
- **Police gameplay:** on-foot and vehicle patrol, walk/run, explore, enter/exit vehicles, traffic stops, driver interaction, ID checks, warnings/citations, investigations, evidence, witness/suspect interviews, reports, dispatch/911, radio, backup, pursuits, detain/cuff, search where justified, arrest, and prisoner transport.
- **Departments/roles:** sheriff deputy is the first playable role; longer-term roles include municipal police, highway patrol/trooper, SWAT, detective, firefighter, paramedic/EMS, and civilian roleplay. Include uniforms, equipment, ranks, career progression, and promotions.
- **Equipment:** interactive duty belt, flashlight, radar, bodycam, MDT, radio, taser/less-lethal tools, and appropriate weapons.
- **Civilian/world life:** believable NPC schedules, conversations and reactions; shopping, driving, traffic-law behavior, 911 calls, witness behavior, suspect/victim roles, fleeing and surrendering behavior.
- **Places that must become usable:** gas stations, convenience stores, grocery/retail stores, banks, houses, apartments, general businesses, sheriff/police facilities, fire stations, hospitals, jails, courthouses, and parking areas. Stores should be enterable and support staff/service interaction and purchases; gas stations support refuelling; homes and apartments support meaningful entry/home interactions.
- **World scale and setting:** huge GTA6-like ambition with realistic lived-in detail, but built as a mobile-scalable streamed world. Florida counties/cities remain: Marion/Ocala, Hillsborough/Tampa including Ybor City, Orange/Orlando, Miami-Dade/Miami, Lee/Fort Myers, Volusia/Daytona Beach, Duval/Jacksonville, Leon/Tallahassee. Long-term 30 × 30 km; start compact in Ocala/Marion. Include cities, towns, rural areas, highways, farms, forests, lakes, neighborhoods and service infrastructure.
- **Initial release scope:** single-player first. First- and third-person cameras. Build the sheriff-deputy vertical slice before expanding to every role/county.
- **Production limits:** iPhone 16e/A18, Unity 6.3 + URP 17.3, iOS; iPhone-only workflow. No local Mac/PC/Chromebook assumed. Use original/properly licensed assets and do not copy proprietary LSPDFR/GTA assets or source.

The detailed scope and staged build order are in `DESIGN/CORE_GAMEPLAY_SCOPE.md`. Treat these as user requirements and planned features, not as claims that they already exist.

## Current committed prototype

- Runtime greybox Ocala blockout; not an accurate finished map.
- World clock/weather, civilian routines, witness observations, incident bus, ambient incident director, greybox incident scenes, dispatch/CAD, civilian 911 call source, runtime mobile MDT.
- Eight-vehicle signalized traffic test junction.
- Separate four-way-stop test junction with Florida arrival-order/right-side priority policy, exclusive intersection reservation, pedestrian occupancy gate, full-stop dwell, and queue cleanup.
- Four-way-stop junction has left-turn, through, and right-turn inbound movement lanes on each approach, individual stop bars/signs, cubic Bézier movement paths, lane markings, and two queued vehicles per lane (24 test vehicles).
- Traffic agents use forward sphere-cast speed limiting for vehicle following and raised static obstacles. This is braking-only; no steering around obstacles or lane changes.
- One north-approach through-lane lead sedan is marked as the emergency test vehicle, with a dark-blue body, dispatch-controlled alternating red/blue lightbar, and generated two-tone siren. The unit is named `OCALA-1`: MDT ACCEPT assigns it, EN ROUTE activates lights and siren, ON SCENE stops the siren while keeping lights active, and RESOLVED/CANCELLED turns both off. It receives priority over ordinary waiting traffic at the four-way stop, but cannot pre-empt an occupying vehicle or bypass pedestrian occupancy. This is a single-unit prototype; full CAD assignment, emergency pull-over behavior, and fleet support remain unimplemented.
- Vehicle collisions can publish incident reports into dispatch.

## Player vertical-slice integration (2026-11-26)

New source committed on `main`:
- `Assets/Scripts/Player/CountyPlayerRuntimeController.cs` creates a player deputy, a patrol SUV, camera anchors, simple runtime touch-oriented IMGUI controls, on-foot movement/sprint, driving throttle/steering, vehicle enter/exit, camera switching, and a duty toggle.
- `Assets/Scripts/Systems/CountyDutyState.cs` provides a session-level on/off duty state and an `OnDutyChanged` event.
- `Assets/Scripts/Player/ThirdPersonWalker.cs` now exposes camera assignment and movement enable/disable methods.
- `Assets/Scripts/Vehicle/PatrolVehicleController.cs` now exposes control enable/disable and immediate stop methods.
- `Assets/Scripts/Systems/CameraModeController.cs` now exposes runtime camera/anchor configuration.
- `Assets/Scripts/World/CountyWorldBootstrap.cs` now spawns the player runtime controller after the Ocala world and civilian/incident setup.

Important limitations:
- The touch controls are temporary IMGUI buttons, not a polished joystick/HUD. Touch interaction behavior still needs Unity/iPhone verification.
- Vehicle motion remains simple transform-based movement, not Rigidbody vehicle physics.
- The duty state is session-only and currently changes the state/event; dispatch, MDT, equipment, uniform, and role gating are not yet connected.
- Enter/exit uses a distance check, hides the player while seated, and places the player beside the SUV on exit; safe exit collision/obstacle checks are not yet implemented.
- The player slice is source-integrated into the runtime county bootstrap, but has not been compiled or run in Unity.

The earlier duplicate root-level controller isolation remains in place. The legacy keyboard path is preserved for compatibility but the new runtime player controller is now additionally spawned by the world bootstrap. Do not treat source integration as a playable/tested build.

## Recent traffic commits (2026-11-26)

- `8e1b718c17dfd611f443cd9044321442271a78b9` — lane-specific turn paths.
- `0e260491ce53e9aabc790dc8453cf8d58cd0e94e` — route waypoint sequencing and stop-bar orientation.
- `12c4080a92bb5a894380a4a736e37f0226cec292` — README and validation updates for lane geometry.
- `5ebb8a69be12bedb64395672929ca58992d40e01e` — lane-marking alignment on both road axes.
- `6078a35fd9a28e1f20f9fa375f5561c64a7bce79` — forward obstacle-aware braking in traffic agent.
- `12a49520e51a38598c28b8b774f3ca5dfc347e39` — emergency vehicle priority in four-way-stop controller.
- `e63c64d5335521c7343b75bb8ccf5071080368c8` — emergency designation API on traffic agent.
- `4865f47945afc00c93e9476fb11456ea45b9ab77` — queue/following and obstacle/emergency design docs.
- `8bbdb284bece15ee02812f703f2a241d56ef934d` — four-way-stop controller docs.
- `3bff77cbcadb47c10fd00ad3f1356a339c87476f` — manual validation matrix.
- `fcb59ece86074c832898efca1e915f82222f257f` — README update.
- `2d7f889a72631ba2fc0e1a5199e98cc2e0d542fd` — move stop signs out of vehicle probe paths.
- `c65d6de9f66e48a4a2214a8e8b7a19290cb7a316` — protect emergency priority in deadlock fallback.
- `b84f5e631dbf74efc73b8980c2d467ce7965c2af` — spawn marked emergency test sedan.
- `7662b8a186862ac05213cbf83685843ab63a0f15` — add renderer-only alternating lightbar controller.
- `3790ac02b6e3af034085a13fa1b41e8c48bfeb8a` — wire animated lightbar into test sedan.
- `d4b9725930be9494bfb131e8fca0aca5fbfc7de0` — README continuity link.
- `e12560d910f0a91ef5ff72b1893420526006b909` — correct traffic count and obstacle validation in README.
- `05cbe89c357fe795267d79609bb92458ca3f09d6` — add stop-sign clearance test case.
- `1b13ed004bea33199c7f1212705da32721dda180` — document roadside sign and obstacle response.
- `c65d6de9f66e48a4a2214a8e8b7a19290cb7a316` — emergency fallback guard.
- `73b0d240799730e0b875efcebde149014353ac57` — document marked emergency sedan.
- `7662b8a186862ac05213cbf83685843ab63a0f15` — renderer-only lightbar controller.
- `3790ac02b6e3af034085a13fa1b41e8c48bfeb8a` — animated lightbar spawner integration.
- `954545430b970b68fe02d4f09c00b889062161ec` — generated two-tone emergency siren.
- `832223bc488ca61f2da4ba3fc44acdeaa0609bde` — dispatch lifecycle response-unit bridge.
- `325fd6c549b92bc951d693d2bc589d49e76d539b` — make emergency lightbar dispatch-activated.
- `d7cad6b2c7ddcb1307a5d1628fa9e909b4478a3e` — wire siren and response unit into test sedan.
- `d815ff3aef66d69f5c91244dea050314e7f8a348` — README dispatch-linked response update.
- `975e217dc1828c6ee2ed5a558ac463ce558e7108` — add dispatch/siren manual validation cases.
- `e956f30285e0152f46fefbd08525525b0a020198` — document emergency response state transitions.
- `180778b25675efd8f0b50619d285ee48ec7b6027` — ensure lightbar is off before dispatch.

## Key source and documentation files

- `Assets/Scripts/Player/ThirdPersonWalker.cs`
- `Assets/Scripts/Vehicle/PatrolVehicleController.cs`
- `Assets/Scripts/Systems/CameraModeController.cs`
- `Assets/Scripts/UI/MobileControls.cs`
- `Assets/Scripts/Systems/OfficerInteractionController.cs`
- `Assets/Scripts/World/CountyTrafficVehicleAgent.cs`
- `Assets/Scripts/World/CountyEmergencyLightbar.cs`
- `Assets/Scripts/World/CountyEmergencySiren.cs`
- `Assets/Scripts/World/CountyEmergencyResponseUnit.cs`
- `Assets/Scripts/World/CountyFourWayStopTestSpawner.cs`
- `DESIGN/PLAYER_VERTICAL_SLICE_AUDIT.md`
- `DESIGN/CORE_GAMEPLAY_SCOPE.md`

## Verification status

Repository files were fetched after the recent edits. Source-level checks confirmed the legacy namespace isolation and explicit bootstrap references, and inspected the mobile movement/vehicle/camera bridge APIs. This does not prove C# compilation or runtime behavior.

Not completed:
- Unity compilation
- Unity Play Mode tests
- Physics/collider tuning
- iPhone 16e profiling or runtime test
- Successful iOS export/IPA or end-to-end build

The manual validation matrix is a checklist, not a test report. Do not claim any Unity/device test passed.

## Next development steps

1. Run Unity compilation/Play Mode for the newly integrated player slice and fix errors.
2. Replace temporary IMGUI controls with a polished mobile joystick and hold-based buttons; verify portrait and landscape safe areas.
3. Improve patrol SUV movement, collision/physics, camera transitions, and safe exit placement.
4. Connect `CountyDutyState` to dispatch, MDT, uniforms/equipment, and duty-only actions while keeping off-duty free roam active.
5. Add player focus/raycast interaction with civilians and connect the existing officer interaction/report systems.
6. Build one traffic stop, suspect response, detain/cuff/search/arrest, and prisoner transport loop.
7. Add one usable store/gas station and one home/interior.
8. Expand Ocala only after the first loop is stable; traffic simulation expansion remains secondary.

## Working protocol

When the user says “Continue”, resume from this file and the current GitHub `main` branch. Fetch current contents and SHAs before editing. Commit the next logical implementation and update the documentation. State exactly what changed and what was actually verified. Do not imply memory storage, compilation, or runtime testing that did not happen.


## Ocala-first daily development decision (2026-11-26)

The user wants to complete Marion County/Ocala before beginning the other counties, with incremental GitHub updates each development day. This is the agreed production order: build a complete, connected Ocala/Marion first-release region, then reuse its systems and expand to Hillsborough/Tampa (including Ybor City), Orange/Orlando, Miami-Dade, Lee/Fort Myers, Volusia/Daytona, Duval/Jacksonville, and Leon/Tallahassee. “Complete Ocala” means a coherent playable county region with urban, residential, commercial, civic, rural, and surrounding-town content; do not attempt to model all 900 km² at full detail before a working slice exists.

Daily roadmap: `COUNTIES/OCALA_DAILY_ROADMAP.md`. It defines the county acceptance gate, daily push/documentation protocol, and a first-week milestone outline. The first vehicle roster should cover vehicle classes rather than every real-world make/model: patrol SUV, civilian sedan/SUV, pickup, delivery van, emergency vehicle, then additional motorcycles, buses, trucks, utility and specialty vehicles. Use original or licensed assets and a shared handling/data definition.

## Immediate iPhone playtest route

A standalone 2D browser preview has been added under `web-demo/index.html`. It is a temporary companion intended to be opened in mobile Safari; it is not the Unity runtime and does not replace the Unity project. A GitHub Pages deployment workflow is at `.github/workflows/deploy-web-demo.yml`. Its deployment must be checked in Actions before giving the user a live URL.

The Unity iOS workflow currently fails at its explicit preflight because the repository has no `UNITY_LICENSE` Actions secret. Latest inspected run #127 stopped at “Verify Unity project and license”; the Unity export and macOS packaging jobs were skipped. The repository also has no `ProjectSettings/EditorBuildSettings.asset` and no `Assets/Scenes` directory at the time of this audit, so the enabled-scene/build setup must be resolved before a Unity build can succeed. To avoid automatically failing the iOS workflow on every source push, its trigger is now manual-only until the user configures a valid Unity license and the scene/build settings are ready.

The mobile player runtime source had an unresolved `onDuty` identifier in its duty button label. This was corrected to read `CountyDutyState.IsOnDuty`; the patrol vehicle now starts with its controls disabled, and sprint uses a hold-style repeat button. This is a source correction only, not a compile/test result.


### Browser preview deployment status (2026-11-26)

- `web-demo/index.html` and `.github/workflows/deploy-web-demo.yml` are committed.
- Browser preview includes touch movement, walk/drive state, enter/exit, patrol SUV, duty toggle, emergency lights, dispatch call generation, location interactions, and basic Ocala landmark/building placeholders.
- First Pages workflow failed at Configure GitHub Pages because the repository Pages site did not exist.
- Second attempt used `enablement: true` but failed with “Resource not accessible by integration” when trying to create the Pages site. This is a repository configuration permission gate, not an HTML build error.
- User must open repository Settings → Pages → Build and deployment → Source = GitHub Actions, then rerun “Deploy Ocala browser preview”.
- Expected site URL after successful deployment: https://kremityssytb.github.io/Sheriff--American-Life/
- Do not claim it is live or tested on iPhone until the deployment and actual browser test succeed.
- The preview is a standalone 2D browser playtest companion; it is not the Unity game, WebGL build, or iOS IPA.

The Unity iOS workflow's latest run stopped at its explicit preflight: missing `UNITY_LICENSE` Actions secret. The export and macOS Xcode package jobs were skipped. The repository also currently has no `Assets/Scenes` directory or `ProjectSettings/EditorBuildSettings.asset`; scene creation and Build Settings must be added before a Unity build can pass. The iOS workflow is now manual-only to prevent every GitHub source push from triggering a known failing license preflight.


## Cloud licensing/build setup update (2026-11-26)

- User constraint remains iPhone-only, no Mac/PC/Chromebook, and $0 budget. Do not recommend paid cloud PCs or paid subscriptions.
- Private setup repository: https://github.com/KremityssYTB/Unity-License. Its README now documents Unity Build Automation as the recommended route, with GameCI/.ulf retained only as an alternative.
- Current preferred path: Unity Build Automation from the Unity Cloud Dashboard, connected to this public GitHub repository. Unity's 2026 DevOps pricing update lists 100 Mac build minutes/month for iOS/macOS builds, plus other free allocations. If a free limit is reached without a payment method, access is blocked; do not add billing details or enable pay-as-you-go.
- Unity Build Automation supports GitHub and Unity 6000.3 LTS. The game repo has `ProjectSettings/ProjectVersion.txt` identifying 6000.3.25f1.
- Unity Build Automation still requires one owner-authenticated setup in the Unity Cloud Dashboard: create/launch a free Unity Cloud Personal project, connect GitHub, select this repo and `main`, then create an iOS target on a macOS builder. Start with manual builds; auto-build requires webhook write permission.
- For manual builds of this public repo, use the minimum public-repository read access token in Unity's dashboard. Never send the token in chat or commit it.
- The repo now includes `Assets/Editor/EnsureProjectSetupBeforeBuild.cs` and `[InitializeOnLoad]` on `ProjectSetup` so the generated Main scene and iOS settings are prepared in a clean cloud checkout. These source changes have not been compiled or verified in Unity.
- Unity iOS output is an Xcode project. A distributable/installable IPA still needs Xcode packaging and Apple signing; do not call the Xcode export an IPA.
- Existing GameCI workflow still needs a valid `UNITY_LICENSE` secret. No .ulf has been generated, and no Unity license or Apple signing credentials are stored in the repo.


## Safe patrol vehicle exit update (2026-11-26)

- Commit: `202c33cb143c8a4e954579eb001d5d432f969352`.
- `Assets/Scripts/Player/CountyPlayerRuntimeController.cs` now searches left, right, behind, and ahead of the patrol SUV for an exit position.
- Candidate positions require a downward ground hit and an unoccupied player capsule volume.
- If no safe candidate is found, the player remains in the vehicle; the controller does not force an unsafe exit.
- This has only been checked at source level. Unity compilation, Play Mode physics behavior, and iPhone testing remain outstanding.
- Next source milestone: connect the player focus/interaction path to nearby civilians, then build the first traffic-stop encounter. Do not expand to another county before the Ocala gameplay loop is coherent.


## Civilian focus and mobile interaction update (2026-11-26)

- Added player-centered civilian focus in `CountyPlayerRuntimeController`: nearest civilian collider within 5.5 m becomes the active contact; focus clears while driving.
- Added touch HUD actions: greet, questions, request ID, warning, citation, detain, arrest, and release.
- `OfficerInteractionController` now receives the player's `CountyDutyState`; ID and enforcement actions require on-duty status. Greeting and questions remain available off duty.
- Added a downtown contact civilian near the initial spawn so the loop can be exercised immediately in the first county greybox.
- Source changes are committed to `main`; no Unity compile, Play Mode, iPhone input, or runtime tests have been run.
- Next: build a traffic-stop encounter from an actual traffic vehicle, including safe pull-over, driver contact, document check, warning/citation decision, and report linkage.


## Traffic stop pull-over update (2026-11-26)

- Added a traffic-stop request state to `CountyTrafficVehicleAgent`: it accepts a roadside target, steers toward it, brakes, exposes `IsPulledOver`, and can resume its route.
- The player patrol SUV now creates a simple red/blue lightbar and displays a PULL OVER control when an on-duty deputy is within 24 m of civilian traffic.
- During the stop the HUD reports PULLING OVER / VEHICLE STOPPED and allows CANCEL STOP / END STOP. Ending the stop resumes the civilian vehicle and disables the lightbar.
- This is not yet a complete traffic stop: driver contact, license/registration/insurance checks, warning/citation workflow, and report linkage remain next.
- Source was inspected and committed. No Unity compilation, physics simulation, or iPhone test has been performed.


## Traffic stop driver/document workflow update (2026-11-26)

- Added `Assets/Scripts/Systems/TrafficStopDriverProfile.cs` for fictional driver identity, license, plate, registration, and insurance records.
- `CountyWorldBootstrap` now assigns each spawned traffic vehicle a fictional driver profile. Most documents are valid; selected profiles have a suspended/expired license, expired registration, or missing insurance to support varied encounters.
- `CountyPlayerRuntimeController` now exposes license, registration, and insurance checks after a vehicle is fully pulled over.
- Each check and the final warning/citation decision is recorded in the session-only `IncidentReportBook` with driver and plate details.
- The player can end the stop to resume traffic and disable the emergency lightbar.
- This is a closed fictional gameplay loop; it does not query real Florida DMV, insurance, or police systems. No Unity compile, simulator, or device test has been run.
- Next: add driver response dialogue and escalation behavior, then connect citations to a ticket record and the MDT case view.


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
- Citation creation also adds a traffic-stop report entry. The ledger and payment state are saved locally as versioned JSON; repeat-driver history is also saved locally in a separate versioned file.
- Source checks confirmed the new data fields, ticket issuance path, MDT wiring, and balanced braces. Unity compilation, Play Mode, UI layout, and iPhone tests remain outstanding.


## Persistent traffic citation save update (2026-11-26)

- Commit: `040bfafe4e982561600e50cb045c59fdf354f593` — `Assets/Scripts/Systems/TrafficCitationBook.cs`.
- The citation ledger now reads/writes a versioned JSON save at `Application.persistentDataPath/traffic-citations.json`.
- Citation issuance and payment-status changes save immediately. The next ticket sequence is persisted and reconciled against loaded citation numbers.
- Empty, unsupported-version, malformed, or unreadable save data is handled without crashing the ledger; load failure starts an empty in-memory list. Save failure is logged while current-session records remain accessible.
- The save is local to the device; there is no cloud sync or cross-device sharing.
- Source inspection only. No Unity compile, JSON round-trip test, iOS storage test, Play Mode test, or iPhone runtime test has been run.
- Next: extend the arrest flow with transport and a usable jail facility, then verify the full civilian-to-booking loop in Unity when an editor/build environment is available. Keep records fictional and local.


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

- Commit `7ecc60ae319edde67200e7e42b54cf00fcd15fa2` added `Assets/Scripts/World/OcalaVisualDressing.cs`; commit `eefb677c30dd8596b97c60d746e931f8156e2128` corrected world-space placement of its generated details; commit `30476aa04b99e27be388a1257a8c38123897faa6` wires the dressing pass into `CountyWorldBootstrap`; commit `4289c669e6b32793fc4fe022e8e52b95aaae6586` documents the scope in README.
- The generated Ocala blockout now receives sidewalks, curbs, road markings, crosswalk stripes, basic facade windows/doors/roof caps, a gas canopy and pump islands, street lamps, utility cabinets, palms, broad-canopy trees, and distant rural tree belts.
- This is a first-pass procedural visual layer only. It does not yet produce photorealistic/GTA 6-level visuals: core buildings, cars, people, and much of the world remain primitive placeholders. Production-quality assets, detailed PBR texture sets, realistic vehicle models/interiors, improved lighting/reflections, LODs, and mobile profiling are still required.
- No Unity compile, Play Mode, visual inspection, or iPhone performance test was possible from the GitHub source connection. Treat this as committed source, not a verified in-game visual result.
- Next graphics milestone: replace the patrol SUV and civilian traffic cubes with original/licensed detailed vehicle prefabs, then establish a consistent PBR material/lighting target and profile on supported iPhone/iPad tiers before scaling the world.


## Patrol SUV visual pass (2026-11-27)

- Commit `c511a0803c2df8471b6e2918a6766a0609699702` adds `Assets/Scripts/Vehicle/PatrolSuvVisuals.cs`, a procedural first-pass SUV model. Commit `6c27ed0d1ebfa8b5cd416ef03949d058e7e3ceef` wires it into `CountyPlayerRuntimeController` and moves the red/blue lightbar modules above the roof. Commit `24a396bc2ec5debb8c90ce53e4a0dd280b3f8872` corrects front/rear badge text orientation.
- The original vehicle root and collider are preserved for compatibility with the current movement, enter/exit, and interaction code. The old cube renderer is hidden; visual detail primitives have their colliders removed.
- Model details include multi-piece body/cabin, tinted glass, trim, side mirrors, handles, running boards, four tire/rim assemblies, grille, headlamps, tail lamps, bumpers, plates, antenna, and Marion County Sheriff markings.
- This remains procedural blockout art, not GTA 6-level realism. It has no final UV/textures, real vehicle mesh, detailed interior, physically based glass implementation, suspension, animated wheels, or verified reflection setup. Unity compilation, Play Mode, visual inspection, and iPhone/iPad performance testing remain outstanding.
- Next: improve civilian traffic vehicle silhouettes and add a proper shared PBR material/lighting pipeline, then replace placeholder world assets with original or properly licensed high-quality art.


## Civilian traffic visual pass (2026-11-27)

- Commit `29ffeffe2235c5f506337ecf678a42cd6b3e8254` adds `Assets/Scripts/Vehicle/TrafficVehicleVisuals.cs` with procedural sedan body/cabin, glass, mirrors, wheels, lamps, bumpers, and trim. Commit `d63c33c68c5dd1a73b371b994d3a37a646f73797` wires the model into `CountyWorldBootstrap.SpawnTrafficVehicle`.
- Civilian traffic roots preserve their Rigidbody and waypoint AI, use unit transform scale, and receive a lengthwise 1.9 × 1.25 × 4.2 box collider. Visual child colliders are removed.
- This is still primitive placeholder geometry and has not been compiled or visually/performance tested in Unity. Confirm sedan heading, collider fit, wheel-ground contact, and mobile performance in Play Mode before treating it as verified.
- Next: replace procedural car parts with original/licensed production meshes and PBR textures; add interior geometry and wheel animation only after basic runtime validation.

- Follow-up commit `9096c0ba0be33664d7a94ed7a5e62223348050d8` adds first-pass patrol SUV cabin geometry: dashboard, instrument hood, center display/console, shifter, steering column/wheel, front seats/headrests, rear seating, and footwells. These details have not been inspected in Unity; verify visibility through the glazing and check for intersections before further refinement.


## Lighting and weather refinement (2026-11-27)

- Commit `a00aa680236aa8319cefc6372bc84b6b7c8deba8` updates `WorldClockSystem`: active sun is assigned to `RenderSettings.sun`, soft shadows and bias/strength are configured, and weather fog now uses Exponential Squared mode with color-tinted, lower-density haze for Fog/Rain.
- No Unity compile or visual/performance validation has been performed. Soft shadows may be expensive on older supported devices and will need quality-tier tuning after profiling.

- Commit `5754f76033a4966afbc6b18ddc567216625aefb5` changes the iOS player target from iPhone-only to iPhone + iPad, matching the stated supported-device goal. Minimum supported hardware/iOS still needs to be defined and tested.


## Civilian vehicle variety pass (2026-11-27)

- Commit `99ecde3cab338ac514770c48c9caca38b9da0092` adds `TrafficVehicleStyle` and separate procedural silhouettes for sedan, pickup, SUV, muscle, and exotic classes in `TrafficVehicleVisuals.cs`.
- Commit `c173d036a641cbc79d76d0eb8a5a4c28eec01ff5` assigns the eight current Ocala traffic actors a mix of those styles without replacing their existing traffic AI, Rigidbody, driver/document records, or root collider.
- Visual distinctions include pickup bed/rails, taller SUV cabin and cladding, muscle-car hood scoop/spoiler, and low/wide exotic body with front splitter, rear diffuser, and engine cover.
- These are generic unbranded placeholder designs, not exact Ford, Dodge, Toyota, or Lamborghini vehicles. Exact branded models require licensed/original production meshes and rights to use associated trademarks.
- No Unity compilation, Play Mode, collision/clearance inspection, or iPhone/iPad runtime test has been performed. Source-only milestone.


## Vehicle catalog and downloadable content direction (2026-11-27)

- Commit `013242228d25f32756319db7a2aff82f1feed96f` adds `Assets/Scripts/Vehicle/VehicleModelCatalog.cs`: an expandable historical vehicle catalog with model/year/manufacturer/style, traffic weight, speed multiplier, and optional visual prefab. Its representative built-in lineup spans early classics through modern models and includes Ford, Dodge, Toyota, Lamborghini, and other manufacturers. The built-in entries do not contain actual 3D models; until real original/properly licensed prefabs are assigned, the existing generic procedural silhouettes remain the fallback.
- Commit `e347117be2b983d4c6eb3fad32082059725aa859` updates `CountyWorldBootstrap` to select a catalog entry for each spawned traffic actor, apply its style/speed profile, and instantiate an assigned visual prefab as a child while preserving the existing root physics and traffic AI. The current test scene still spawns only eight traffic actors.
- Commit `481b13b20eb85264f027d57fe34776212fdfe4e4` adds `Assets/Scripts/Content/ContentPackCatalog.cs`, a ScriptableObject manifest for stable pack IDs, categories, versions, estimated sizes, HTTPS bundle URLs, SHA-256 metadata, dependencies, base-install inclusion, and removability.
- Commit `61824b55c3be24cee7ae2f663f6267e9f5bbc25a` adds `DESIGN/DOWNLOADABLE_CONTENT_ARCHITECTURE.md`. Target model: compact core game plus optional vehicle collections, county expansions, departments, interiors/businesses, gameplay systems, and audio/visual packs. It defines storage checks, integrity verification, dependencies, streaming, Wi-Fi preference, and safe uninstall requirements.
- The catalog/manifest are foundations only. No CDN, hosted bundles, remote manifest, in-game store/browser, actual downloader, resume logic, storage reservation, SHA verification pipeline, or pack install/uninstall workflow exists yet. Actual car meshes and other pack assets must still be imported and packaged.
- Source changes are committed but have not been compiled in Unity or tested in Play Mode/iOS on device.

## Beta preparation update (2026-11-27)

- Commit `641ea989535cb07a98725f7f71c012d2a9feace9` fixes a conflicting iOS build setting in `Assets/Editor/EnsureProjectSetupBeforeBuild.cs`: the pre-build hook was overriding the iPhone+iPad setting from `ProjectSetup` and forcing iPhone-only. It now sets `iOSTargetDevice.iPhoneAndiPad`.
- Commit `aabfd3c3b8974af4cd2b256bfaf8028c7e1780d3` adds `BETA_READINESS.md` with explicit acceptance gates for project import/compile, iOS export/build/sign/install, the patrol gameplay loop, world/traffic, performance/stability, and beta operations.
- Current source has core prototype systems, but the scene remains a generated blockout and the mobile HUD is still an immediate-mode GUI prototype. No Unity compilation, cloud build, Xcode build, signing, TestFlight upload, installation, or real-device tests have been completed in this work session. Treat all unchecked beta gates as unverified.


## Beta handling implementation (2026-11-27)

- Commit `d1ca0c1bad928bc76b0f17a82cc7e8d86ccd910c` replaces direct Transform movement in `PatrolVehicleController` with a required Rigidbody and FixedUpdate-based arcade movement. It configures interpolation, continuous collision detection, mass, and frozen roll/pitch; exposes acceleration, speed, steering, coast drag, and lateral grip tuning.
- Commit `498ad0c798baa5fbeb9f5dca6c148349ceba7e09` updates beta acceptance gates to require actual patrol vehicle collision/handling tuning. This does not yet provide WheelColliders, suspension, production vehicle meshes, or verified driving feel. Unity compile and device validation remain outstanding.


## Visual quality and mobile optimization pass (2026-11-27)

- No production vehicle models are integrated. The vehicle catalog contains real make/model/year metadata, but built-in entries have no visual prefabs; current traffic and patrol SUV art remain procedural placeholders.
- Commit `dc2f7b963dd0891ce0932cf0c7b11871a8692c85` removes the extra `Florida Sun` created by editor scene setup. `CountyWorldBootstrap` creates and assigns the single county sun to `WorldClockSystem`.
- Commit `3b17e8f5d120ae2e124cb4403b972fe562d80335` removes colliders from decorative Ocala dressing primitives and destroys the component-owned shared materials when the dressing component is destroyed. Gameplay collision remains on the base world geometry.
- Commit `0566b1bb0a3482afd09dc787f7e5452efc9cec51` throttles dynamic sun updates to a 0.15-second interval and applies shadow configuration when the sun is assigned rather than rewriting it each lighting tick.
- Commit `747c0a3e60036b9cced6a230d1806fa2b68a0b9b` adds `MobilePerformanceBootstrap`, a provisional 60 FPS/VSync-off baseline with LOD bias 1.0, 45 m global shadow distance, two cascades, background loading priority, and 256 MB mipmap-streaming budget. It is attached by `CountyWorldBootstrap` in commit `ceb2dedd9ae46bbf5fd4e3086b5931e112f4f5f7`.
- Commit `f547be738a9cfb97e02f565368cda0fe8b38407c` adds `DESIGN/MOBILE_VISUAL_QUALITY_BUDGET.md`, covering production art requirements, URP settings to inspect, profiling, and device quality tiers.
- Important: the generated URP asset still needs inspection/tuning in Unity 6.3.25f1. These source changes have not been compiled, visually inspected, profiled, or tested on iPhone/iPad. The 60 FPS value is a target cap, not a measured result.

- Follow-up URP configuration commits `eafcefd1b49ee0401aed8dca339f28a5d0fd3e87` and `a531a9b4be0f5222fb4373af2debf60975f04de0` configure the generated Unity 6.3 URP asset with HDR, 2x MSAA, 1.0 render scale, SRP Batcher, no dynamic batching, 45 m shadow distance, two cascades, 2048 main-light shadow map, soft main-light shadows, no additional-light shadows, a two-light per-object cap, and no depth/opaque texture unless later needed. These values use URP 17.3 public asset APIs and serialized shadow toggles. Verify the generated asset in the editor before build.

- Commit `47709ae53f7f7fba29783bf8444dc02f859af2bb` changes procedural civilian vehicles to share paint/glass/tire/trim/alloy/lamp materials and use per-renderer material property blocks for unique paint colors. Commit `57123039542119eacf1f3733d6e2e3a2e6f19c56` releases the shared traffic materials when the county world is regenerated; commit `626b19cc1d99156bf438646a34a210bcb7bb93a2` releases them when the county bootstrap is destroyed. Commit `3b4e1772806b9684dd57d0d14d7205449f51b76b` adds teardown cleanup for procedural patrol SUV materials. These changes reduce duplicate material allocations; no frame-time or memory measurement exists yet.

- Follow-up commit `1abd53e77c9d2ffc44cfabb38c1cd21d9f88a08b` makes the runtime bootstrap apply shadow distance/cascade values directly to the active `UniversalRenderPipelineAsset` (with QualitySettings fallback), so the runtime shadow budget is not dependent on built-in-pipeline settings.


## Production vehicle art pipeline (2026-11-27)

- Commit `fd284e4149c577c146bba7ebeb943bc18d7e6cc5` adds `Assets/Editor/VehiclePrefabValidator.cs`, an editor-only menu validator for all `VehicleModelCatalog` assets and a selected vehicle prefab. It reports missing prefabs as warnings (procedural fallback is intentional), and checks renderer presence, missing mesh/material references, LODGroup presence, renderer/material-slot budgets, root scale, and recommended wheel/light/interior anchors.
- Adds `DESIGN/VEHICLE_VISUAL_ASSET_STANDARD.md` defining legal asset provenance, prefab axes/hierarchy, wheel and optional light/interior anchors, LOD expectations, mobile budgets, and integration steps. Vehicle art must be original or properly licensed for commercial use and redistribution.
- The validator is source-only and has not been run in Unity. It does not validate license rights, visual quality, model dimensions, UVs, wheel placement, or actual mobile performance. No real vehicle model has been added; the catalog still falls back to procedural visuals until a valid prefab is assigned.


## Runtime mobile visual presets (2026-11-27)

- Commit `41c40494ad6fe8fd7629eca6af257d7f12c9db38` updates `MobilePerformanceBootstrap` with saved Performance, Balanced (default), and High presets. Presets apply frame-rate caps, LOD bias, shadow distance/cascades, MSAA, render scale, and HDR on the active URP asset; selection persists with PlayerPrefs.
- Adds `DESIGN/MOBILE_QUALITY_PRESETS.md` with the provisional preset matrix and required profiling/validation gates.
- No visible Settings UI is connected yet. Values are provisional; no compile, renderer validation, frame-time capture, thermal test, or iPhone/iPad test has been performed. Do not claim any preset achieves its target FPS.


## Frame-rate settings and ProMotion (2026-11-27)

- Commit `816c6e92577ca5642848ef7f6ed2090834f08220` introduces `MobileFrameRateMode` with 30, 60, 120 FPS, and Unlimited modes, saved separately from the visual-quality preset. Unlimited maps to Unity's `Application.targetFrameRate = -1`; it does not mean hardware can render infinitely fast.
- Commit `60d53841929e2bda9439f98855bb76082683219d` decouples the frame-rate choice from the visual-quality preset so changing graphics quality no longer overwrites the user's selected cap.
- Commit `166ea0f3703daac0d087327137d52710e986d6ee` adds a Settings button and in-game Frame Rate panel to the current prototype HUD. The panel exposes 30, 60, 120, and Unlimited and displays the selected mode.
- Commit `ae99b9657ec71b87e417b1068f7c72b9dc50d8eb` adds an iOS build postprocessor that sets `CADisableMinimumFrameDurationOnPhone=true` in the generated Info.plist. This is required for high refresh rates on supported ProMotion iPhones; iPad Pro does not require the key, but it is harmless.
- Unity's iOS behavior still follows device refresh support. The primary iPhone 16e target has a 60 Hz display, so it cannot physically show 120 FPS. ProMotion iPhones/iPad Pro models may support up to 120 Hz when enabled, but actual sustained game FPS is not guaranteed. Unlimited requests the platform's native maximum, not infinite rendering.
- The current HUD is still an immediate-mode prototype, not production UI. These changes have not been compiled or tested in Unity, exported to Xcode, or measured on hardware.

- Follow-up commit `f60687a58cc18cc43f060fa6cd43f47cd547025a` removes the final legacy frame-cap assignment from visual preset selection, so switching Performance/Balanced/High no longer overrides the separately selected frame-rate mode.
- Follow-up commit `e7831d15c883a658f162e7b147122a2a10898a54` moves the Settings panel draw call to the end of the prototype HUD pass so it renders above gameplay controls.
- Static source checks after these commits found balanced braces in all three changed C# files, all four frame-rate options wired, persistence present, Unlimited mapped to -1, and the ProMotion Info.plist key present. This is not a Unity compile or runtime test.


## Settings panel graphics controls (2026-11-27)

- Commit `ab82099a87ce8d7ec43eedc9de0ffdbbc239ca73` adds Performance, Balanced, and High graphics preset buttons to the in-game Settings panel. The buttons call `MobilePerformanceBootstrap.SetPreset`; quality and frame-rate preferences remain separate.
- Commit `0a6ae549ca8e558e5997bf0a4dbfc3d00ed79911` makes Settings modal while open (so gameplay controls are not processed behind it) and scales the panel against short screen heights.
- Current settings UI is still IMGUI prototype code. Static source checks only; no Unity compile, layout inspection, touch test, or device profiling has been completed.


## Input-release safety pass (2026-11-27)

- Commit `8db4deeef0bae5415071440f80f5a1854d4077e7` clears walking, sprint, throttle, and steering inputs while the Settings modal is open. This prevents a held sprint/gas input from remaining active behind the modal.
- The same change adds `OnDisable` cleanup to release movement and vehicle inputs when the player runtime controller is disabled.
- Static source checks only; Unity compilation and actual touch-input behavior remain unverified.


## HUD safe-area pass (2026-11-27)

- Commit `855fcd7f832714bd04cdcb0c2358ba64888dee92` moves the existing IMGUI HUD into the current `Screen.safeArea` rectangle. HUD layout width/height calculations now use the safe-area dimensions, and the GUI matrix is restored after both the Settings-modal and normal HUD draw paths.
- This is a first safe-area accommodation for notches, rounded corners, and home-indicator insets on iPhone/iPad. It is not a finished responsive UI; actual orientation changes, smallest supported screens, split-screen iPad layouts, and touch hit targets still require Unity/device validation.
- Static source review only; no Unity compilation or visual/device test was performed.


## Live frame performance readout (2026-11-27)

- Commit `7272dee6b3cc7b9ab4cb85a16d2428eac53551f1` adds smoothed unscaled frame-time sampling to `MobilePerformanceBootstrap`, exposing measured FPS and milliseconds per frame without allocating per sample.
- Commit `1e0939b7f109cb5da2828d1b4a3f0f6c812c9f2c` displays the live FPS/frame-time readout in the Settings panel and expands the panel slightly to fit it.
- These are runtime diagnostics, not a performance guarantee or a substitute for Unity Profiler CPU/GPU/memory/thermal captures. Metrics have not been verified in a Unity player or on devices.


## Three.js browser playtest companion (2026-11-27)

- Replaced the previous 2D web-demo/index.html preview with a standalone Three.js 0.180.0 ES-module scene rendered through WebGL.
- The preview is designed for mobile Safari on iPhone/iPad and responsive desktop browsers. It uses viewport-fit cover, safe-area insets, pointer/touch controls, adaptive renderer sizing, and a capped device-pixel ratio.
- The scene includes a schematic Ocala/Marion test district, road grid and lane markings, procedural buildings, trees/palms, civilian traffic, a patrol SUV with red/blue light emitters, a deputy avatar, a third-person follow camera, and camera drag look.
- Playtest actions include on/off duty, enter/exit patrol SUV, walk/sprint, basic driving/steering, emergency lights, location interaction prompts, dispatch call generation, and clearing a call by reaching its marker.
- A basic FPS readout is included for rough browser-side observation. It is not a benchmark or substitute for Safari/Web Inspector or Unity Profiler measurements.
- Three.js is loaded from the jsDelivr CDN; an internet connection is required. WebGL 2 support is required. The preview has not yet been verified in mobile Safari, on iPadOS, or on desktop browsers.
- The browser project is a separate prototype companion. It does not compile Unity C# into WebGL, share Unity runtime state, produce an IPA, or replace the native Unity iOS/iPadOS target.
- The browser map and art are schematic procedural placeholders, not an accurate GIS map or production-quality art. The browser driving model is intentionally simplified and is not the Unity Rigidbody vehicle implementation.
- The previous preview script contained a stale null element lookup for carInteract; replacing the page also removes that broken handler.
- GitHub Pages still requires repository Settings → Pages → Build and deployment → Source = GitHub Actions. The workflow must succeed before the expected URL can be called live: https://kremityssytb.github.io/Sheriff--American-Life/
- Source/documentation checks only. No successful Pages deployment, real browser launch, WebGL runtime test, touch test, or device performance test has yet been confirmed.


### GitHub Pages deployment confirmation (2026-11-27)
- The browser-preview workflow run #8 completed successfully for commit de0e4a142792d6d3fbee67a1ce5a045938e93f7a. GitHub Actions reported the Configure GitHub Pages, upload, and Deploy steps successful.
- Published preview URL: https://kremityssytb.github.io/Sheriff--American-Life/
- The deployment result confirms the workflow completed; it does not confirm that the page has been opened or that Three.js/WebGL works on a real iPhone or iPad. Those device checks remain outstanding.


## iOS frame-rate default adjustment (2026-11-28)

- Commit updates `MobilePerformanceBootstrap` to default to 60 FPS instead of 120 FPS. This avoids requesting a refresh rate above the 60 Hz display capability of the primary iPhone 16e target and provides a conservative baseline across supported iPhones/iPads.
- The optional 120 FPS and Unlimited modes remain user-selectable; the iOS ProMotion Info.plist postprocessor remains in place for compatible devices. Actual sustained performance and refresh behavior still require device profiling.
- Static source/documentation update only. No Unity compile, cloud build, Xcode build, signing, installation, or runtime test was performed.


## Mobile MDT safe-area pass (2026-11-28)

- Commit `8894fcdd52c74f2243f4de9b2f53196e3fbd037a` adds a dedicated safe-area RectTransform to the runtime-generated MDT Canvas. The MDT window and floating MDT toggle are now parented under that area, with anchors updated from `Screen.safeArea` when device insets or orientation change.
- Updated `DESIGN/MOBILE_MDT_INTERFACE.md` to record the implementation. This complements the existing IMGUI player HUD safe-area pass.
- Static source/documentation edit only. Unity compilation, notch/home-indicator layout inspection, portrait/landscape touch testing, and real iPhone/iPad validation remain outstanding.


## Responsive deputy touch HUD pass (2026-11-28)

- Commit `e6db81187f39539590cc16654073db23000c4594` updates `CountyPlayerRuntimeController.OnGUI` to derive HUD scale from both safe-area width and height, so short landscape layouts are considered rather than scaling from width alone. The status label now uses the remaining width beside the top-right action stack.
- Movement/vehicle button rectangles retain a minimum 54 GUI-point width/height after scaling. Font sizes follow the responsive scale. This is a source-level hit-area rule, not a guarantee of identical physical-pixel size on every display.
- Commit `cca15f9183505691b698800c936882ed7575ede1` documents the responsive sizing and outstanding device-layout validation.
- Static source change only. No Unity compilation, Play Mode inspection, touch test, or iPhone/iPad runtime test has been performed. Remaining checks include smallest supported iPhone, large iPhone, iPad portrait/landscape, split-screen/resizing, and contextual HUD overlap/clipping.


## Contextual HUD collision-avoidance pass (2026-11-28)

- Commit `69fa626e2f0c6c48d1c2de75aa75d59048552ae1` changes the prisoner-transport prompt to use a responsive width derived from safe-area width and places it in the upper-left contextual column while a traffic stop is active. This separates it from the traffic-stop document/action controls in the upper-right column.
- Commit `a1809d034e736dd3c38a79e95b2f16510134a554` records the layout rule in `DESIGN/MOBILE_MDT_INTERFACE.md`.
- Static source/documentation update only. No Unity compile, Game view inspection, or device touch/layout test has been performed. Verify all contextual HUD combinations, narrow phones, iPad split view, and long names/booking identifiers before treating the layout as complete.


## Prisoner-transport HUD placement refinement (2026-11-28)

- Commit `6656ef8bc089498edf4f6552876b9972e721ca8f` replaces the transport panel's side-column layout with a compact center-bottom strip above the vehicle control row. The strip combines intake distance and prisoner name; the booking action appears only within intake range. Its lower edge is kept above the movement/throttle buttons by a scaled gap.
- Commit `e7cdabde439ac9e0d67730cbcb83001e00586ab6` documents the new placement in `DESIGN/MOBILE_MDT_INTERFACE.md`.
- This reduces known side-column collisions in the source layout but is not proof of full-screen fit. Short landscape safe areas, unusually long prisoner names, and all combinations of active traffic-stop controls still require Unity Game view and device validation. No compile or runtime test was performed.


## Gameplay HUD Canvas migration audit (2026-11-28)

- Re-inspected current `CountyPlayerRuntimeController.cs` (blob `a6abbb9057279e909f5c0e9eb493069cd5a6a128`) and `MobileMdtScreen.cs` (blob `d416107ad1c924f75b793d6a83d70495c3e56e28`). The MDT is already a runtime-generated Screen Space Overlay Canvas with CanvasScaler, safe-area root, and InputSystemUIInputModule. The gameplay HUD remains IMGUI in `CountyPlayerRuntimeController.OnGUI`.
- Recorded a phased migration plan in `DESIGN/MOBILE_MDT_INTERFACE.md`, commit `cdc08108da687f519ca6457acf62e7f590989e48`. Order: (1) held touch movement/vehicle controls, (2) top-level duty/vehicle/camera/settings actions, (3) civilian/traffic-stop/transport contextual panels, (4) settings modal.
- Key safety requirement: held inputs must release on pointer-up, pointer-exit, focus loss, HUD disable, and vehicle-mode changes; movement/sprint and steering/throttle must support simultaneous touches. Canvas controls should forward to existing gameplay state rather than reimplement gameplay rules.
- This was a source audit and documentation-only step. No Canvas gameplay controls were implemented, no IMGUI controls were removed, and no Unity compile, Game view, or device test was performed. Next implementation step is the touch-control adapter, with duplicate IMGUI movement/vehicle controls removed in the same change.


## Gameplay HUD held-touch control implementation (2026-11-28)

- Commit `8050bd5259dbd6d54ac4b7140ff9f06dd20a9e8e` adds `Assets/Scripts/Systems/MobileTouchControls.cs`, a runtime-generated Screen Space Overlay Canvas for held walking/sprint and vehicle steering/throttle/brake controls.
- Commit `eec81c7d6e7c83780b0723f6d6759bd25f914dba` wires the adapter into `CountyPlayerRuntimeController`, maps independent held states to the existing `ThirdPersonWalker` and `PatrolVehicleController` inputs, and removes the old IMGUI movement/vehicle RepeatButtons to prevent duplicate controls.
- Follow-up commits `2d35f3d1122e8fde33de8a5d871f164e67917974`, `f3f1470189980b6db52542bcc0dea009a631412f`, `d8ffc22232cb349b79b09067078f7444684d11cb`, and `08abe057d3efdc67da7bd5f176833a5ac2d190b7` refine visibility lifecycle, disable behavior, use a top-level pointer handler component, and correct its runtime collection type.
- Held controls release on pointer-up, pointer-exit, button disable, app focus loss, application pause, vehicle-mode changes, settings-modal opening, and player-controller disable. Independent buttons allow simultaneous directional movement plus sprint and steering plus gas/brake.
- The touch Canvas uses a safe-area root and sorting order 40, below the MDT Canvas at sorting order 50. Existing duty/vehicle/camera/settings action buttons and civilian/traffic-stop/transport panels remain IMGUI; those are later migration phases.
- Static source checks performed: balanced braces in the new touch-control source and player controller; no remaining `GUI.RepeatButton` movement/vehicle controls; adapter callback and release paths present. These checks do not prove compilation or runtime behavior.
- No Unity compile, Play Mode test, device multi-touch test, visual layout inspection, cloud build, Xcode export, signing, or IPA installation has been performed. The implementation must be validated before being treated as working on iPhone/iPad.


## Gameplay HUD primary action migration (2026-11-28)

- Commit `b82f911aa0fc747f2ca6238a3c82f51445407a25` adds Canvas status text and duty, enter/exit vehicle, camera, and Settings action buttons to `MobileTouchControls`.
- Commit `513ac83c35f7fa2789e46a1f07b9cb9915fa622a` wires those actions to the existing `CountyPlayerRuntimeController` methods and removes their duplicate IMGUI status/action controls. Duty and vehicle labels are refreshed from current controller state.
- Commits `200a6a7aae7097943baf06b057431b26f549322e` and `2df6eb29c90e54e34337ea1a68a25c25f5230866` adjust the brake/settings touch hit areas and remove obsolete IMGUI layout variables.
- The first Canvas migration phases now cover held foot/vehicle controls and top-level status/actions. Civilian interactions, traffic-stop panels, prisoner transport, and the Settings modal remain IMGUI.
- Static source checks confirm balanced braces, correct touch-button collection types, held pointer lifecycle handlers, safe-area logic, controller event wiring, primary action dispatch, no old movement RepeatButtons, and no old top-level IMGUI action buttons. These are text-level checks only.
- Unity compile/runtime, actual multi-touch, visual layout, MDT overlap, portrait/landscape, iPad split view, cloud build, signing, and install remain unverified.


## Gameplay HUD civilian interaction Canvas migration (2026-11-28)

- Commit `bd7379dfd284ff4e15ad131a36608be0bba0b8b1` adds a contextual civilian panel to `MobileTouchControls`, including the nearby civilian name and eight interaction actions.
- Commit `3841c336b1ff6af37247c4c2a021b41a247a7691` wires the Canvas actions to the existing `OfficerInteractionController` methods and removes the duplicate civilian IMGUI label/buttons.
- Commit `afff9b8fa023319a63d1780afd89da9cb24dabfa` guards the per-frame contextual panel update when the touch-control component is unavailable.
- The panel visibility follows the existing focused-civilian state, is hidden in vehicle mode, and is hidden whenever the gameplay Canvas is disabled for the Settings modal. It uses safe-area-relative anchors and remains separate from the top-level action stack and foot movement buttons.
- `DESIGN/MOBILE_MDT_INTERFACE.md` records Phase 3a. Traffic-stop documents/decisions, prisoner transport, and Settings remain IMGUI; they are not migrated yet.
- Source-level checks only. Unity compilation, runtime UI/event routing, touch behavior, panel readability, and layout on portrait/landscape iPhone and iPad remain unverified.


## Gameplay HUD traffic-stop Canvas migration (2026-11-28)

- Commit `d27b1a967909d8038b25de03a7bd41c446d4f482` adds the traffic-stop panel and contextual Canvas action buttons to `MobileTouchControls`.
- Commit `19c93a753fbe3924f80a0372f15d501c309da0b8` refines the panel safe-area placement and dynamically lays out only the currently available actions.
- Commit `8565aa1b24d45949031568667eae1442005d7903` connects traffic-panel actions to existing controller methods and removes the duplicate traffic-stop IMGUI panel.
- Commit `4802bc347c46537b6c8a2a922891379bd0b98f1d` corrects line-break escaping in driver detail text.
- Commit `05dbf994d171b9eadf20fcc3038cbd3d41880604` moves End/Cancel Stop to the final position in the contextual action list.
- The Canvas shows the active stop status, driver name, plate, behavior disposition/response, available document checks, warning/citation choices, and stop termination. Pull Over appears for an eligible nearby traffic vehicle while on duty.
- The existing traffic-stop methods remain authoritative for driver responses, document checks, report logging, citations, driver history, and lightbar behavior. The old `DrawTrafficStopHud` IMGUI method and its call were removed.
- `DESIGN/MOBILE_MDT_INTERFACE.md` records Phase 3b. Civilian interactions and traffic stops are now Canvas panels; prisoner transport and the Settings modal remain IMGUI.
- Static checks confirm all nine UI action IDs exist and dispatch, the old traffic-stop IMGUI method/call are absent, and braces balance. No Unity compile, Play Mode, visual inspection, device touch test, cloud build, signing, or IPA installation has been performed.


## Gameplay HUD prisoner transport Canvas migration (2026-11-28)

- Commit `eeaaffff9011a5b43e24b3c4d6e447e7e30125cc` adds the Canvas prisoner transport panel, prisoner detail text, and BOOK INTAKE button.
- Commit `e7ded2940418f1fe99b4c71a646e4a53f5733216` connects the panel to the existing pending booking state, adds a fresh booking lookup for the button callback, and removes the old IMGUI transport strip.
- The panel appears only when the deputy is on duty and in the patrol vehicle with an escorted prisoner whose booking remains AwaitingTransport. It shows prisoner name, intake distance, and booking-range status; BOOK INTAKE appears only within 18 m.
- The action reuses `CompletePrisonerTransport`, which revalidates duty, vehicle, escort/booking identity, and intake distance before marking the booking Booked, restoring the civilian, recording the jail-intake report, and stopping the vehicle.
- Commit `87af666329205c73f15eae4a866c7b3e9b7e038f` documents Phase 3c in `DESIGN/MOBILE_MDT_INTERFACE.md`.
- Commit `69b173b1d458f7e64b09dd6b4e252c5b71d0370c` removes the obsolete `DrawPrisonerTransportHud` call and sizing helper after the method removal; the IMGUI transport path is fully disconnected.
- Civilian interaction, traffic-stop, and prisoner transport interfaces are now Canvas-based. Settings modal remains IMGUI. Static source inspection only; Unity compile, Play Mode, touch testing, visual layout, cloud build, signing, and IPA installation remain unverified.
