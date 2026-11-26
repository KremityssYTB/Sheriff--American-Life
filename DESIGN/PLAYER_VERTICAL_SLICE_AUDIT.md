# Player Vertical-Slice Architecture Audit

Last reviewed: 2026-11-26

## Current integration status

The runtime county bootstrap now spawns a first mobile-oriented player/vehicle source prototype through `CountyPlayerRuntimeController`.

| Area | Source | Status and limits |
|---|---|---|
| Player spawn | `Assets/Scripts/Player/CountyPlayerRuntimeController.cs` | Spawns one deputy capsule in the Ocala greybox |
| On-foot movement | `Assets/Scripts/Player/ThirdPersonWalker.cs` | Camera-relative movement and sprint; runtime input is routed by the new controller |
| Touch controls | `CountyPlayerRuntimeController.OnGUI` | Temporary IMGUI directional and sprint buttons; not a polished joystick and not device-tested |
| Patrol vehicle | `Assets/Scripts/Vehicle/PatrolVehicleController.cs` | Transform-based throttle/steering; not Rigidbody vehicle physics |
| Enter/exit | `CountyPlayerRuntimeController.ToggleVehicle` | Distance-gated enter; exit now searches four nearby positions, checks ground and capsule clearance, and keeps the player seated if all positions are blocked |
| Camera | `Assets/Scripts/Systems/CameraModeController.cs` | Runtime-configured first/third-person anchors; no collision avoidance |
| Duty state | `Assets/Scripts/Systems/CountyDutyState.cs` | Session state/event and HUD toggle; not yet connected to dispatch/equipment gating |
| Civilian focus/actions | `CountyPlayerRuntimeController` + `OfficerInteractionController` | Player-centered 5.5 m overlap focus, mobile interaction buttons, session report logging; enforcement actions require on-duty state |
| Bootstrap | `Assets/Scripts/World/CountyWorldBootstrap.cs` | Adds the player runtime controller after county, incident, traffic, and civilian setup |

## Recent source integration

- Added `CountyPlayerRuntimeController` and called it from the runtime Ocala bootstrap.
- Added `CountyDutyState` with `IsOnDuty`, `SetDuty`, `ToggleDuty`, and `OnDutyChanged`.
- Added public camera configuration to the camera-mode controller.
- Added camera assignment and movement enable/disable methods to the walker.
- Added vehicle control enable/disable and immediate-stop methods to the patrol vehicle controller.
- The older root-level keyboard controllers remain isolated under `SheriffAmericanLife.Legacy`.

## Integration gaps and next tasks

1. Compile and run in Unity; resolve all C# and scene/bootstrap errors.
2. Verify IMGUI touch controls on an iPhone; replace with a proper joystick and hold-based touch buttons if unreliable.
3. Improve driving physics, collision handling, and vehicle entry/exit animation; safe exit-position checks are now present in source but need runtime validation.
4. Subscribe dispatch, MDT, uniforms/equipment, and duty-only interactions to `CountyDutyState.OnDutyChanged`.
5. Replace the current proximity focus with a proper view/line-of-sight selection system and add dialogue/response depth.
6. Extend the new arrest booking ledger with prisoner transport and a usable jail intake location; the current booking status buttons are an initial MDT prototype.
7. Add one usable store/gas station and one home/interior for off-duty gameplay.

## Acceptance checks

- One player and one player camera spawn in the Ocala bootstrap.
- On-foot movement, sprint, and camera toggle respond to touch.
- Player can approach, enter, drive, stop, and exit the patrol SUV.
- On-foot controls cannot move the player while seated; vehicle controls stop on exit.
- Duty toggle changes state and raises the duty-state event.
- Off-duty state leaves ordinary movement and driving available.
- No camera clipping or unsafe vehicle exit placement.
- No errors or warnings that prevent Play Mode.

## Verification limits

Only GitHub source was inspected and committed. No Unity compilation, Play Mode test, iPhone touch test, physics validation, performance profiling, iOS export, or IPA installation has been performed. The temporary IMGUI controls and all runtime behavior remain unverified. This document is an implementation audit, not a passed test report.


## Safe vehicle exit source update (2026-11-26)

- `CountyPlayerRuntimeController` now checks four candidate exit locations around the patrol SUV.
- Each candidate must have ground beneath it and enough capsule clearance for the player.
- If every candidate is blocked or lacks ground, the player stays seated rather than being placed inside an obstacle.
- This is a source-level safeguard only. Physics behavior, trigger/layer interactions, and iPhone use have not been tested in Unity.


## Civilian interaction source update (2026-11-26)

- The player runtime now searches a 5.5 m sphere around the deputy and focuses the nearest civilian collider.
- A mobile HUD exposes greet, questions, ID, warning, citation, detain, arrest, and release actions for the focused civilian.
- Identification and enforcement actions are blocked while off duty; ordinary greeting and questions remain available.
- A downtown contact resident is spawned near the initial player location so the first interaction can be reached without travelling across the greybox.
- Interaction actions continue to write to the session-only incident report book.
- This is source-level integration only. Unity compilation, touch layout, interaction range, and runtime behavior have not been tested.


## Traffic stop pull-over source update (2026-11-26)

- Civilian traffic agents now expose a pull-over request, a shoulder target, a stopped state, and a resume method.
- The patrol SUV now creates a simple red/blue renderer lightbar and exposes a mobile PULL OVER control when an on-duty deputy is within 24 m of a traffic vehicle.
- The targeted traffic vehicle steers toward a roadside point and brakes to a stop; the HUD exposes END STOP or CANCEL STOP and turns the lightbar off when the stop ends.
- This is only the initial vehicle pull-over phase. Driver identity/contact, vehicle registration, license/insurance checks, citations, and stop reports are not yet connected.
- Source-level checks only; Unity compilation, vehicle physics, collision avoidance, and mobile HUD behavior remain untested.


## Traffic stop document-check source update (2026-11-26)

- Added `TrafficStopDriverProfile` to each spawned civilian traffic vehicle with fictional driver name, date of birth, Florida-style license number and plate, registration state, and insurance state.
- The bootstrap generates mostly valid records with a small chance of a suspended/expired license, expired registration, or missing insurance so stops can have different outcomes.
- Once a vehicle is pulled over, the patrol HUD exposes separate license, registration, and insurance checks.
- Each check is added to the session incident report book. After all three checks, a document issue permits a warning or citation decision; clear records show no document violation.
- The stop can then be ended, which resumes the vehicle and turns off the lightbar.
- These are fictional gameplay records and not connected to real Florida DMV, insurance, or law-enforcement databases. Source-only verification; Unity compilation and gameplay testing remain outstanding.


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


## Persistent citation ledger source update (2026-11-26)

- `TrafficCitationBook` now serializes citation records and the next ticket sequence into versioned JSON at `Application.persistentDataPath/traffic-citations.json`.
- The ledger loads during component initialization and saves after citation issuance or payment-state changes.
- Loaded ticket numbers are checked when restoring the next sequence, reducing the chance of duplicate ticket IDs if the sequence counter is stale.
- File and JSON exceptions are caught and logged. Load errors leave an empty in-memory ledger; save errors do not remove records from the current session.
- Persistence is local-only. There is no account sync, server storage, or real-world records integration.
- Verification is source inspection only. Unity compilation, JSON serialization round-trip, iOS sandbox storage, Play Mode, and iPhone testing remain outstanding.


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


## Prisoner transport and jail intake source update (2026-11-26)

- Added a greybox Marion County Jail Intake building and marked vehicle intake bay near the sheriff office in the Ocala blockout.
- The on-duty deputy must be in the patrol SUV, have an awaiting-transport booking, and drive within 18 m of the intake bay to complete transport.
- Successful transport advances the booking from AwaitingTransport to Booked through the persistent arrest-booking ledger and adds a jail-intake report entry. The patrol SUV is stopped after the transfer.
- The MDT BOOKINGS tab now displays awaiting-transport instructions instead of allowing the deputy to bypass the vehicle transport step. Release remains an MDT action after booking.
- This is a prototype vehicle-to-record custody transfer. The arrested civilian is not physically seated in the SUV, and there is no animated escort, prisoner AI, collision restraint, jail interior navigation, or real justice-system connection.
- Source inspection only; Unity compilation, scene layout, vehicle handling, persistence round-trip, and iPhone runtime remain untested.
