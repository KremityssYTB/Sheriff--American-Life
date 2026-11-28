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
