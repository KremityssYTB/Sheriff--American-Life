# Project Continuity Handoff

Last updated: 2026-11-26 (core open-world/off-duty gameplay scope captured)

This file is the durable handoff for continuing SHERIFF: AMERICAN LIFE across ChatGPT conversations. The GitHub repository on `main` is the source of truth; inspect current file contents and SHAs before every edit.

## Project identity and constraints

- Project: SHERIFF: AMERICAN LIFE
- Repository: https://github.com/KremityssYTB/Sheriff--American-Life
- Branch: `main`
- Engine target: Unity 6.3 (`6000.3.25f1`) + URP 17.3.0
- Target device: iPhone 16e / A18, iOS 26.5
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

The detailed scope and staged build order are in `DESIGN/CORE_GAMEPLAY_SCOPE.md`. Treat these as user requirements and planned features, not as claims that they already exist in the prototype.

## Current committed prototype

- Runtime greybox Ocala blockout; not an accurate finished map.
- World clock/weather, civilian routines, witness observations, incident bus, ambient incident director, greybox incident scenes, dispatch/CAD, civilian 911 call source, runtime mobile MDT.
- Eight-vehicle signalized traffic test junction.
- Separate four-way-stop test junction with Florida arrival-order/right-side priority policy, exclusive intersection reservation, pedestrian occupancy gate, full-stop dwell, and queue cleanup.
- Four-way-stop junction has left-turn, through, and right-turn inbound movement lanes on each approach, individual stop bars/signs, cubic Bézier movement paths, lane markings, and two queued vehicles per lane (24 test vehicles).
- Traffic agents use forward sphere-cast speed limiting for vehicle following and raised static obstacles. This is braking-only; no steering around obstacles or lane changes.
- One north-approach through-lane lead sedan is marked as the emergency test vehicle, with a dark-blue body, dispatch-controlled alternating red/blue lightbar, and generated two-tone siren. The unit is named `OCALA-1`: MDT ACCEPT assigns it, EN ROUTE activates lights and siren, ON SCENE stops the siren while keeping lights active, and RESOLVED/CANCELLED turns both off. It receives priority over ordinary waiting traffic at the four-way stop, but cannot pre-empt an occupying vehicle or bypass pedestrian occupancy. This is a single-unit prototype; full CAD assignment, emergency pull-over behavior, and fleet support remain unimplemented.
- Vehicle collisions can publish incident reports into dispatch.

## Recent traffic commits (2026-11-26)

- `8e1b718c17dfd611f443cd9044321442271a78b9` — lane-specific turn paths.
- `0e260491ce53e9aabc790dc8453cf8d58cd0e94e` — route waypoint sequencing and stop-bar orientation.
- `12c4080a92bb5a894380a4a736e37f0226cec292` — README and validation updates for lane geometry.
- `5ebb8a69be12bedb64395672929ca58992d40e01` — lane-marking alignment on both road axes.
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

- `Assets/Scripts/World/CountyTrafficVehicleAgent.cs`
- `Assets/Scripts/World/CountyEmergencyLightbar.cs`
- `Assets/Scripts/World/CountyEmergencySiren.cs`
- `Assets/Scripts/World/CountyEmergencyResponseUnit.cs`
- `Assets/Scripts/World/CountyFourWayStopTestSpawner.cs`
- `Assets/Scripts/World/CountyFourWayStopController.cs`
- `Assets/Scripts/World/CountyIntersectionRules.cs`
- `Assets/Scripts/World/CountyPedestrianCrossingAgent.cs`
- `Assets/Scripts/World/CountyPedestrianCrossingSensor.cs`
- `DESIGN/TRAFFIC_QUEUE_AND_FOLLOWING.md`
- `DESIGN/FOUR_WAY_STOP_CONTROLLER.md`
- `DESIGN/FOUR_WAY_STOP_TEST_MATRIX.md`
- `README.md`

## Verification status

Repository files were fetched after the recent edits. Source-level checks confirmed lane movement assignments, Bézier waypoint generation, lane-marking orientation, static-obstacle filtering code, roadside stop-sign placement, emergency-priority and fallback guards, preservation of the occupied-intersection/pedestrian gate, renderer-only dispatch-controlled lightbar wiring, OCALA-1 dispatch lifecycle matching, generated siren clip setup, and balanced braces in the main emergency/traffic scripts. These checks are not a C# compiler.

Not completed:
- Unity compilation
- Unity Play Mode tests
- Physics/collider tuning
- iPhone 16e profiling or runtime test
- Successful iOS export/IPA or end-to-end build

The manual validation matrix is a checklist, not a test report. Do not claim any Unity/device test passed.

## Next development steps

1. Validate the OCALA-1 MDT sequence: ACCEPT, EN ROUTE, ON SCENE, RESOLVED/CANCELLED; verify lightbar and siren transitions and ensure unrelated unit assignments do not activate this vehicle.
2. Add safe emergency pull-over behavior and intersection handling. Never allow emergency vehicles to enter an occupied conflict area or drive through an active pedestrian crossing.
3. Inspect and tune obstacle detection against road colliders, stop signs, vehicle colliders, and objects on adjacent lanes; avoid false braking.
4. Implement obstacle steering/lane changes only after braking-only detection is stable.
5. Validate the 24-car setup, emergency priority, static-obstacle detection, roadside stop-sign clearance, and dispatch audio in Unity.
6. Update this file, README, relevant design docs, and manual validation matrix after each significant implementation.

## Working protocol

When the user says “Continue”, resume from this file and the current GitHub `main` branch. Fetch current contents and SHAs before editing. Commit the next logical implementation and update the documentation. State exactly what changed and what was actually verified. Do not imply memory storage, compilation, or runtime testing that did not happen.
