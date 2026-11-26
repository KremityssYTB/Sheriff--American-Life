# Project Continuity Handoff

Last updated: 2026-11-26 (traffic obstacle/emergency priority pass)

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

## Current committed prototype

- Runtime greybox Ocala blockout; not an accurate finished map.
- World clock/weather, civilian routines, witness observations, incident bus, ambient incident director, greybox incident scenes, dispatch/CAD, civilian 911 call source, runtime mobile MDT.
- Eight-vehicle signalized traffic test junction.
- Separate four-way-stop test junction with Florida arrival-order/right-side priority policy, exclusive intersection reservation, pedestrian occupancy gate, full-stop dwell, and queue cleanup.
- Four-way-stop junction has left-turn, through, and right-turn inbound movement lanes on each approach, individual stop bars/signs, cubic Bézier movement paths, lane markings, and two queued vehicles per lane (24 test vehicles).
- Traffic agents use forward sphere-cast speed limiting for vehicle following and raised static obstacles. This is braking-only; no steering around obstacles or lane changes.
- One north-approach through-lane lead sedan is marked as the emergency test vehicle, with a dark-blue body and static red/blue roof lightbar. It receives priority over ordinary waiting traffic at the four-way stop, but cannot pre-empt an occupying vehicle or bypass pedestrian occupancy. Flashing lights, siren/audio, dispatch activation, full emergency fleet, and emergency pull-over behavior remain unimplemented.
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
- `d4b9725930be9494bfb131e8fca0aca5fbfc7de0` — README continuity link.
 
## Key source and documentation files

- `Assets/Scripts/World/CountyTrafficVehicleAgent.cs`
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

Repository files were fetched after the recent edits. Source-level checks confirmed lane movement assignments, Bézier waypoint generation, lane-marking orientation, static-obstacle filtering code, emergency-priority checks, preservation of the occupied-intersection/pedestrian gate, and balanced braces in the spawner. These checks are not a C# compiler.

Not completed:
- Unity compilation
- Unity Play Mode tests
- Physics/collider tuning
- iPhone 16e profiling or runtime test
- Successful iOS export/IPA or end-to-end build

The manual validation matrix is a checklist, not a test report. Do not claim any Unity/device test passed.

## Next development steps

1. Inspect the current traffic source before changing it. Validate obstacle detection against road colliders, stop signs, vehicle colliders, and objects on adjacent lanes; tune detection height/radius and avoid false braking.
2. Add flashing emergency lightbar behavior, siren/audio, and dispatch-triggered activation for the existing marked emergency test sedan.
3. Add safe emergency pull-over behavior and intersection handling. Never allow emergency vehicles to enter an occupied conflict area or drive through an active pedestrian crossing.
4. Implement obstacle steering/lane changes only after braking-only detection is stable.
5. Validate the 24-car setup, emergency priority, static-obstacle detection, and roadside stop-sign clearance in Unity.
6. Update this file, README, relevant design docs, and manual validation matrix after each significant implementation.

## Working protocol

When the user says “Continue”, resume from this file and the current GitHub `main` branch. Fetch current contents and SHAs before editing. Commit the next logical implementation and update the documentation. State exactly what changed and what was actually verified. Do not imply memory storage, compilation, or runtime testing that did not happen.
