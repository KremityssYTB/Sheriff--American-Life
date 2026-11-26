# County Incident and 911 Event System

## Overview

World incidents are published through `WorldIncidentReporter`, received by `DispatchCallManager`, and displayed by the mobile MDT. This milestone adds two incident producers.

## Ambient county scenario director

`Assets/Scripts/World/CountyIncidentDirector.cs` creates weighted, timed scenario reports for the greybox:

- Vehicle collisions
- Theft reports
- Disturbances
- Fires
- Medical emergencies
- Suspicious activity
- Assault reports

Default bootstrap settings wait 25 seconds before the first event, then generate another report every 65–115 seconds, up to 12 ambient reports per session. Scenario weights influence selection. These are dispatch scenario reports only; they do not yet spawn physical crashes, fires, suspects, injuries, or emergency-service AI.

The director exposes `TriggerScenario(index)` for future mission triggers and `ScenarioReported` for UI or analytics listeners.

## Civilian 911 call source

`Assets/Scripts/Systems/Civilian911CallSource.cs` is attached to each test resident. It exposes callable methods for other game systems or future phone UI:

- `ReportDisturbance(details)`
- `ReportTheft(details)`
- `ReportAssault(details)`
- `ReportFire(details)`
- `RequestMedicalHelp(details)`
- `ReportSuspiciousActivity(details)`
- `ReportVehicleCollision(details)`

Each call includes the reporting civilian's name and world position when available. A 30-second per-caller cooldown prevents repeated submissions. The message is explicitly a caller report, not confirmation that the alleged event occurred.

## Dispatch flow

1. An ambient scenario or civilian call invokes `WorldIncidentReporter.Report`.
2. `WorldIncidentBus` broadcasts the new incident.
3. `DispatchCallManager` creates a CAD call and assigns priority.
4. The MDT refreshes its active-call list.
5. Nearby witness agents can record what they heard or observed.

## Current limitations

- Ambient scenarios are abstract reports, not physical world events.
- No civilian phone UI or player-facing 911 interaction is implemented yet; the callable source is the integration point.
- No real traffic simulation, collision physics, fire propagation, injury model, EMS/firefighter AI, suspect AI, or crime investigation behavior is implemented.
- Caller statements are unverified and should remain distinguishable from witnessed facts.
- All data remains session-only.
