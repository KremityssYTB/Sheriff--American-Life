# Mobile MDT Interface

## Purpose

The Mobile Data Terminal (MDT) is the deputy's in-vehicle and handheld computer. It presents dispatch calls and linked case information without owning the underlying simulation data.

## Current implementation

`Assets/Scripts/Systems/MobileMdtScreen.cs` builds a runtime Canvas interface and touch controls. It creates an EventSystem with the Input System UI module when the scene does not already have one. The screen is landscape-first and rearranges the call list and case panel for portrait orientation. A dedicated safe-area root keeps the window and floating MDT toggle inside iPhone/iPad display insets and updates when the OS safe area changes. A floating MDT button opens the panel; the panel can be closed from its header.

The current screen uses a simple active-call list and three case tabs: details, witnesses, and reports. Five action buttons invoke the controller's validated dispatch transitions. The call list and details refresh when the MDT controller receives data events.

`Assets/Scripts/Systems/MobileMdtController.cs` is the presentation-facing controller.

- Reads calls from `DispatchCallManager`.
- Selects and clears the current call.
- Retrieves witness statements using the call's source incident ID.
- Retrieves incident reports linked to that same source incident ID.
- Exposes accept, en-route, on-scene, resolve, and cancel actions.
- Raises `MdtChanged` when calls, reports, or witness statements change.

Dispatch resolution and cancellation create an incident report through `IncidentReportBook`. Reports store `relatedIncidentId`, allowing the MDT to show records associated with a specific call.

## Proposed touch interface

### 1. Active calls

Show a scrollable list sorted by priority, then received time.

Each row should display:
- CAD call number
- Priority and incident title
- Short location label
- Received game time
- Current status
- Assigned unit

Use distinct but restrained priority indicators. Critical calls must be visually obvious without relying on color alone.

### 2. Call details

When a call is selected, show:
- Full incident description
- Subject description, when available
- Location and map marker
- Received time
- Assigned unit
- Current call status
- Available status actions

Only show actions valid for the current lifecycle state:
- Available: Accept
- Accepted: Mark en route
- En route: Mark on scene
- On scene: Resolve
- Any non-terminal state: Cancel

The controller currently validates transitions through DispatchCallManager. UI buttons must still handle a false return value and refresh from the latest call state.

### 3. Case information

Use sub-tabs or collapsible sections:
- Witnesses: witness name, seen/heard type, statement, confidence
- Reports: report number, category, action, outcome, narrative, recorded time
- Incident: source incident description and subject details

Do not imply that a witness statement is proof of guilt. Confidence is an estimate of observation quality, not a credibility verdict.

### 4. Map and navigation

The call location is currently a Unity world-space Vector3. The future map should convert this into a county map coordinate and support:
- Show call on map
- Set waypoint
- Route guidance
- Unit location and status

Street addresses and real road routing are not implemented yet.

## Mobile layout guidance

- Landscape-first in-vehicle layout with a compact portrait fallback.
- Large touch targets for accept, en-route, on-scene, resolve, and cancel.
- Keep call priority, call number, and status visible while scrolling.
- Avoid dense desktop-style tables.
- Provide readable text at iPhone 16e resolution.
- Support safe areas and scalable UI.
- Do not require keyboard input for routine dispatch actions; use preset dispositions with an optional notes field.

## Data and persistence limitations

All calls, reports, and witness statements are currently session-only. No save/load, server CAD, multiplayer unit synchronization, GPS routing, or rendered MDT screen exists yet. The screen remains a prototype: it has no county map rendering, address lookup, route guidance, radio audio, custom icons, saved case data, or multiplayer synchronization. Visual sizing and touch behavior still require testing on an actual iPhone.

### Responsive layout validation notes

- The runtime-generated MDT Canvas uses a safe-area root and a scale-with-screen-size CanvasScaler. Its call-list/case-panel split switches between landscape and portrait arrangements based on screen orientation.
- The deputy's separate IMGUI HUD now derives its scale from both safe-area width and height, rather than width alone. This reduces oversized controls on short landscape screens; the status label is constrained to the remaining width beside the top-right action stack.
- Movement and vehicle buttons retain a minimum 54-point GUI hit rectangle after scaling. This is a source-level sizing rule, not a measured physical-pixel guarantee across display densities.
- Remaining validation: inspect smallest supported iPhone, large iPhone, iPad portrait/landscape, split-screen/resizing, text clipping, and overlapping contextual HUD panels in Unity and on devices. The current IMGUI control scheme remains a prototype, not a final virtual joystick.

- The prisoner-transport prompt now uses a responsive panel width and shifts to the upper-left contextual column while a traffic stop is active, avoiding the shared upper-right action column used by document checks. This is a deterministic source-level collision avoidance rule; other combinations and narrow-screen clipping still need visual testing.

- Follow-up layout refinement: the prisoner-transport HUD is now a compact center-bottom strip above the vehicle control row. It combines intake distance and prisoner name into one status line and only shows the booking action when within intake range. This keeps it out of both traffic-stop side columns and leaves the steering/throttle hit areas clear by a scaled vertical gap.


## Gameplay HUD migration plan

The gameplay HUD is separate from the MDT. The MDT already uses a runtime-generated Screen Space Overlay Canvas, a CanvasScaler, a safe-area root, and the Input System UI module. The deputy gameplay HUD in `CountyPlayerRuntimeController` still uses IMGUI and should be migrated without rewriting gameplay systems.

### Migration boundaries

1. **Touch movement and vehicle controls:** move directional movement, sprint, steering, throttle, and brake/reverse to Canvas controls. Use pointer-down/pointer-up semantics for continuous inputs; a normal Button onClick is not sufficient for held controls. Preserve the controller's existing `moveInput`, `throttle`, and `steering` gameplay values as the adapter boundary.
2. **Top-level status and actions:** migrate duty, enter/exit vehicle, camera, and settings actions to Canvas buttons. Keep their existing controller methods as the action targets.
3. **Contextual interactions:** migrate civilian interaction, traffic-stop documents/decisions, and prisoner transport into separate contextual panels. Panels must be state-driven and must not overlap one another or the driving controls.
4. **Settings:** move graphics and frame-rate selectors into a Canvas modal, preserving `MobilePerformanceBootstrap` as the settings authority.

### Implementation constraints

- Do not duplicate controls: once a Canvas control owns an input, remove its IMGUI counterpart in the same change.
- Continuous controls must release safely on pointer-up, pointer-exit, focus loss, HUD disable, and vehicle-mode changes. A lost pointer-up must never leave throttle or movement stuck.
- Support simultaneous touch where appropriate (movement plus sprint; steering plus throttle) and prevent UI touches from leaking into world interaction.
- Keep controls within `Screen.safeArea`; scale from available safe-area dimensions, not raw screen dimensions.
- Keep large touch targets and readable labels, but allow compact layouts on short landscape screens and iPad split view.
- Preserve the existing MDT Canvas rather than creating a second EventSystem. If HUD initialization can occur before the MDT, safely reuse or create exactly one EventSystem and Input System UI module.
- Keep gameplay rules in `CountyPlayerRuntimeController` and domain controllers; Canvas components should only present state and forward input/actions.

### Current migration status

**Phases 1–2 source implementation are committed; Unity validation is still pending.**

- `Assets/Scripts/Systems/MobileTouchControls.cs` creates a Screen Space Overlay Canvas with a CanvasScaler, safe-area root, and held pointer controls. It reuses the current EventSystem or creates one with the Input System UI module when none exists.
- Foot mode provides four directional controls and sprint. Vehicle mode provides left/right steering, gas, and brake/reverse. Each control uses pointer-down, pointer-up, pointer-exit, and disable release semantics.
- Independent controls can be held together, supporting movement plus sprint and steering plus throttle/brake.
- `CountyPlayerRuntimeController` receives the control states and maps them to its existing walker and vehicle inputs. The previous IMGUI movement and vehicle RepeatButtons have been removed to avoid duplicate inputs.
- The adapter releases held inputs on focus loss, application pause, control-mode changes, settings-modal opening, and controller disable. Controls are hidden while the controller is disabled or the settings modal is open.
- The touch Canvas currently has sorting order 40, below the MDT Canvas at 50. Duty status and the primary vehicle/duty/camera/settings actions are now Canvas elements; callbacks invoke the existing controller methods and labels refresh from duty/vehicle state.
- The old IMGUI movement/vehicle RepeatButtons and top-level status/action buttons have been removed. Civilian interaction is now a state-driven Canvas panel; traffic-stop, prisoner-transport, and Settings modal content remain IMGUI for later phases.
- Vehicle gas/brake controls and the Settings action use separate screen regions to avoid a known hit-target overlap. The floating MDT toggle remains on its higher-sorting Canvas.

**Not yet verified:** Unity compilation, runtime EventSystem behavior, multi-touch on device, safe-area layout in portrait/landscape and iPad split view, button hit areas, and interaction with the MDT Canvas. Do not treat this as a tested or production-ready control scheme.


### Phase 3a — civilian interaction panel (2026-11-28)

- `MobileTouchControls` now builds a contextual civilian panel inside the gameplay Canvas, with the nearby person's name and the existing Greet, Questions, ID, Warn, Cite, Detain, Arrest, and Release actions.
- `CountyPlayerRuntimeController` updates panel visibility from the existing civilian-focus result and forwards each Canvas action to the existing `OfficerInteractionController` methods. No civilian gameplay rules were moved into the UI.
- Removed the duplicate civilian interaction IMGUI label and buttons. The panel is hidden when no civilian is focused or when the player is in a vehicle; it is also hidden with the gameplay Canvas while Settings is open.
- The panel uses safe-area-relative anchors and sits left of the primary action stack and clear of the foot movement controls. Actual overlap and readability still require Unity Game view inspection on narrow phones and iPad layouts.
- Traffic-stop documents/decisions, prisoner transport, and Settings remain IMGUI and are not represented as migrated.


### Phase 3b — traffic-stop Canvas panel (2026-11-28)

- The gameplay Canvas now presents nearby traffic contact and active stop state in a dedicated safe-area panel, separate from the civilian panel and vehicle steering/throttle controls.
- The panel displays driver name, plate, driver disposition, and latest response when those data are available. It exposes Pull Over, Calm/Firm Approach, License/Registration/Insurance checks, Warning/Citation decisions, and End/Cancel Stop according to the existing traffic-stop state.
- Each Canvas action dispatches to the existing controller methods. Document checks remain one-time actions based on the existing profile flags; warning/citation remain gated by all document checks, a detected violation, and no prior decision.
- Removed the duplicate traffic-stop IMGUI labels and buttons. The traffic-stop simulation, citation creation, driver history, report logging, and lightbar behavior remain in their existing gameplay methods.
- The panel is hidden outside vehicle mode or when there is neither an active stop nor an eligible nearby traffic target. The Settings modal still hides the entire gameplay Canvas.
- The panel uses a responsive two-column action layout within the safe area. Its actual touch-target sizing, long-text wrapping, and layout on compact portrait/landscape phones and iPad remain unverified.
- Prisoner transport is now a state-driven Canvas intake panel. The Settings modal remains IMGUI and is the next migration target.


### Phase 3c — prisoner transport Canvas panel (2026-11-28)

- The gameplay Canvas now shows the escorted prisoner's name, distance to Marion County jail intake, and whether the vehicle is within the existing 18 m booking range.
- The BOOK INTAKE action is enabled only while the player is on duty, inside the patrol vehicle, escorting the matching AwaitingTransport booking, and within 18 m of intake. The controller revalidates all of these conditions in `CompletePrisonerTransport` before changing the booking.
- Successful booking continues to use `ArrestBookingBook.MarkBooked`, restores/unparents the escorted civilian, records the jail-intake report, and stops the patrol vehicle. These gameplay operations remain in the controller.
- Removed the duplicate center-bottom IMGUI transport strip. The Canvas panel uses safe-area-relative anchors and remains hidden unless a valid escorted booking is pending.
- The actual layout and touch target still require Unity visual inspection on compact phones, landscape, and iPad. Unity compilation and runtime booking tests have not been performed.
- Settings remains the final IMGUI HUD section to migrate.
