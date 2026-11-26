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
| Existing civilian actions | `Assets/Scripts/Systems/OfficerInteractionController.cs` | Exists, but player focus/raycast and interaction HUD are not connected |
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
5. Add player-centered civilian focus/raycast and connect `OfficerInteractionController`.
6. Implement traffic stop interaction, suspect AI response, detain/cuff/search/arrest, and prisoner transport.
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
