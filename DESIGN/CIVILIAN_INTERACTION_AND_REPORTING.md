# Civilian Interaction and Incident Reporting

## Implemented prototype layer

The first interaction layer provides touch-HUD-ready public methods and a session incident log.

### Civilian profile
- Basic identity: name, occupation, identification number.
- Disposition: calm, nervous, agitated, cooperative, resistant.
- Stress value from 0 to 100.
- Legal state: free, detained, arrested.
- Interaction reactions for greeting, questions, identification, warnings, citations, detention, arrest, and release.

### Officer interaction controller
- A focused civilian is assigned by `SetFocusedCivilian`.
- Public methods are intended to be wired to mobile buttons or a contextual interaction wheel.
- Arrest is only allowed after detention in this prototype.
- Actions create report entries when an IncidentReportBook is assigned.

### Incident report book
- Adds a sequential session incident number, category, subject, action, outcome, narrative, and timestamp.
- Raises an event so a future MDT screen can update.
- Incident reports and witness statements currently exist only for the current play session. Save/load persistence is not implemented.

## Witness observations and world incidents

- `WorldIncidentBus` publishes typed world events with a unique session ID, location, game-clock time, description, and subject description.
- `WorldIncidentReporter` is the scene-facing component for other gameplay systems to publish incidents.
- `CivilianWitnessAgent` listens for incidents and records a statement when the civilian is within hearing range. A civilian in the forward visual cone and visual radius is marked as having seen and heard it; otherwise the statement is marked heard nearby.
- `WitnessStatementBook` stores witness name, incident ID, descriptions, observation type, confidence estimate, position, and record time. It exposes `StatementAdded` and lookup by incident ID for a future MDT.
- The bootstrap attaches witness agents to the two test residents and creates the reporter and statement book.

## Incident-to-dispatch flow

- `WorldIncidentReporter.Report` publishes a typed incident with location and game-clock time.
- `DispatchCallManager` subscribes to the incident bus and creates a CAD-style call once per incident ID.
- Prototype priority mapping: assault/fire/medical emergency = critical; collision/theft = high; disturbance = medium; suspicious activity = low.
- Call lifecycle: Available → Accepted → En Route → On Scene → Resolved. Calls can also be Cancelled with a reason.
- The manager raises `CallCreated` and `CallUpdated` events for a future mobile radio/MDT UI. Calls are currently session-only and are not yet linked to report completion.

## Next realism requirements

- Add evidence and legal grounds to each enforcement action; do not make detention/arrest an arbitrary button outcome.
- Add consent and refusal responses, witness interviews, suspect risk assessment, and backup requests.
- Add line-of-sight raycasts, sound obstruction/attenuation, witness attention, memory decay, and the ability to misremember details.
- Add automatic world-event producers (collision, shop alarm, disturbance, fire/medical event) and connect validated events to dispatch.
- Connect call resolution to the incident report book and require a documented disposition before closing calls.
- Add a real ID / warrant database with fictional data.
- Add animation states, dialogue lines, voice/audio, and localization.
- Connect callout lifecycle and dispatch to incident reports.
- Build a touch-first interaction panel with clear context and confirmation for high-impact actions.
- Replace test state changes with scenario-specific rules and a complete procedural workflow.

## Prototype limitations

This is gameplay scaffolding, not a legal simulator or a complete police procedure model. It does not connect to real personal data, law-enforcement databases, or real-world records.
