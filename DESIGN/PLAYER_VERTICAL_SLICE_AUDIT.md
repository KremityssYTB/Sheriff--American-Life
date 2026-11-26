# Player Vertical-Slice Architecture Audit

Last reviewed: 2026-11-26

## Findings from the current main branch

The repository already contains early player and mobile-control components, but they are split between an older greybox implementation and newer folder-organized components.

| Area | Existing source | Current limitation |
|---|---|---|
| On-foot movement | `Assets/Scripts/Player/ThirdPersonWalker.cs` | Has camera-relative movement, sprint, and a public movement-input API; not yet wired by `CountyWorldBootstrap` |
| Vehicle movement | `Assets/Scripts/Vehicle/PatrolVehicleController.cs` | Has public throttle/steering methods; no enter/exit ownership or player handoff |
| Camera modes | `Assets/Scripts/Systems/CameraModeController.cs` | Supports first/third-person anchor transforms and mode selection; not wired into the current bootstrap |
| Touch bridge | `Assets/Scripts/UI/MobileControls.cs` | Exposes movement, sprint, driving, camera, and test-dispatch methods; no generated touch HUD or bootstrap wiring yet |
| Civilian interaction | `Assets/Scripts/Systems/OfficerInteractionController.cs` | Supports focused-civilian actions and reports; not yet connected to a player focus/interaction loop |
| Current bootstrap | `Assets/Scripts/CountyWorldBootstrap.cs` | Still creates the older keyboard-only greybox player, camera, dispatch placeholder, and patrol vehicle |

## Compile hygiene correction

Four root-level scripts and their newer folder-organized counterparts declared identical class names in the same `SheriffAmericanLife` namespace:

- `CameraModeController`
- `DispatchCallSystem`
- `PatrolVehicleController`
- `ThirdPersonWalker`

That creates duplicate type declarations in a normal Unity assembly. The root-level versions have now been moved into `SheriffAmericanLife.Legacy`. The existing bootstrap explicitly references those legacy types so its old greybox behavior remains isolated. The newer implementations retain the primary `SheriffAmericanLife` type names in their organized folders.

This is a source-level namespace correction, not proof that the project compiles. Other duplicate types or unrelated compile errors may still exist.

## Integration order

1. Replace the bootstrap's legacy player with `Player.ThirdPersonWalker` and provide a CharacterController without leaving the primitive capsule collider active.
2. Bind camera-relative movement to a generated, touch-friendly virtual joystick and sprint control; connect camera anchors and a first/third-person toggle.
3. Add a player vehicle interaction component that finds a compatible nearby patrol vehicle, enters only when within a safe interaction distance, disables on-foot movement while driving, transfers camera focus, and exits at a safe side/rear offset.
4. Route mobile throttle and steering only to the currently occupied vehicle. Ensure input is cleared on enter, exit, disable, and loss of focus.
5. Add an explicit duty-state service. Going off duty must not delete the player's character or disable ordinary movement, driving, stores, or home interactions; it should gate only duty-specific dispatch/equipment/actions.
6. Connect civilian focus and interaction to the same player interaction system.
7. Build the first traffic-stop interaction loop, then one usable store and one home interior.

## Acceptance checks for the first player slice

- Player spawns once and has one movement controller.
- No duplicate class declarations remain in the shared gameplay namespace.
- Touch movement and sprint work without a physical keyboard.
- First/third-person toggle moves between valid camera anchors without camera clipping through the player.
- Player can approach, enter, drive, stop, and exit the patrol SUV.
- On-foot input cannot move the player while seated/driving; vehicle input cannot continue after exit.
- Duty state visibly changes between ON DUTY and OFF DUTY and gates only duty-only actions.
- Mobile input remains responsive at iPhone portrait/landscape safe areas.
- No claim of success until Unity compilation and Play Mode checks have actually been run.

## Verification limits

The audit inspected GitHub source and directory contents. It did not run Unity, compile C#, inspect a live scene in the Editor, or test on an iPhone. The current bootstrap still uses the legacy keyboard-only player and vehicle path. The newer mobile-oriented components are not yet integrated into that bootstrap.
