# Core Gameplay Scope — SHERIFF: AMERICAN LIFE

## Non-negotiable identity

This is a single-player-first, Florida-inspired, open-world life and law-enforcement roleplaying game. The core police experience should take inspiration from the freedom and scenario-based policing of LSPDFR, while the world aims for a highly detailed, lived-in open-world scale. It must not become only a traffic simulator or a dispatch screen with roads around it.

The player should feel like they live in the county. They can choose to work, go off duty, explore, shop, drive, interact with people, and take part in ordinary civilian life as well as law-enforcement duties. First-person and third-person play are both part of the target.

## On-duty and off-duty play

- The player can go on duty and off duty. Off duty is a real gameplay state, not simply a menu toggle.
- While off duty, the player can freely explore the world, visit stores and businesses, go home, drive personal vehicles, and take part in ordinary civilian activities.
- On duty, the player can operate as a deputy/officer, receive dispatch calls, patrol, respond to incidents, conduct stops and investigations, and use department equipment.
- Duty state should control access to duty uniform/equipment, patrol assignment, radio/MDT, dispatch availability, and duty-specific interactions. Do not arbitrarily prevent ordinary free-roam activities when off duty.
- Future role choices include sheriff's deputy, municipal police officer, highway patrol/trooper, SWAT, detective, firefighter, paramedic/EMS, and civilian roleplay. Build the first playable slice around the sheriff deputy role.

## Police roleplay systems

- Patrol on foot and in vehicles; walk, run, explore, enter and exit vehicles.
- Conduct traffic stops, approach drivers, communicate with occupants, request identification, issue warnings/citations, investigate violations, and decide how to handle encounters.
- Respond to 911 calls and dispatch callouts; use radio communication, request backup, and coordinate with responding units.
- Investigate crimes and scenes; speak with witnesses and suspects, collect and record evidence, write reports, and use the MDT.
- Search suspects where legally justified, detain/cuff, arrest, transport prisoners, and process incidents.
- Support pursuits, fleeing/surrendering suspects, backup, emergency response, and escalation/de-escalation.
- Equipment and tools include duty belt, flashlight, radar, body camera, MDT, radio, less-lethal tools such as a taser, and appropriate weapons. These systems should be implemented as interactive gameplay, not decorative props.
- Include department identity, uniforms, ranks, career progression, and promotions as longer-term systems.

## Civilian life and world interaction

The world must contain useful, enterable, interactable places rather than decorative storefronts only. The intended categories explicitly include:
- Gas stations and convenience stores.
- Grocery stores and other retail stores.
- Banks.
- Houses and apartments.
- General businesses and public places.
- Police/sheriff facilities, fire stations, hospitals, jails, courthouses, and parking areas.

Store/business gameplay should support entering the location, interacting with staff or services, and shopping/purchasing where appropriate. Gas stations should support refuelling. Banks should be represented as real world locations and support appropriate bank interactions. Homes and apartments should support meaningful entry and home-life interactions as the world systems mature.

Civilian/NPC life should include believable schedules and routines, conversations and reactions, traffic-law behavior, shopping, driving, and responses to emergencies. NPCs may become callers, witnesses, victims, suspects, fleeing persons, or cooperating/surrendering subjects depending on the scenario. Avoid treating every NPC as a combat target.

## World, geography, and services

- Florida-inspired, multi-county world developed county by county.
- Planned regions: Marion County (Ocala, Silver Springs, Belleview, Dunnellon and surrounding rural areas); Hillsborough County (Tampa, including Ybor City); Orange County (Orlando); Miami-Dade (Miami); Lee County (Fort Myers); Volusia County (Daytona Beach); Duval County (Jacksonville); Leon County (Tallahassee).
- Long-term 30 × 30 km streamed-world ambition; first playable area is a compact Ocala/Marion County district.
- Include cities, towns, rural areas, highways, farms, forests, lakes, neighborhoods, and service infrastructure.
- Day/night cycle, weather, traffic, ambient activity, emergency services, and NPC schedules should make the world feel active.

## Technical and production constraints

- Target: iPhone 16e / A18, Unity 6.3 (6000.3.25f1), URP 17.3.0, iOS; bundle ID `com.kremityss.sheriffamericanlife`.
- User workflow is iPhone-only; do not assume access to a Mac, PC, Chromebook, Unity Editor, or Xcode locally.
- Build scalable mobile systems and a compact vertical slice before attempting the full map or high-end visual ambition.
- Single-player is the initial scope. Multiplayer is not a current requirement.
- Use original or properly licensed assets; take inspiration from gameplay patterns, not proprietary LSPDFR/GTA assets or source.

## Vertical-slice order

1. Player character movement, first/third-person camera, interaction, and mobile controls.
2. Enter/exit and drive one patrol vehicle; support walking between the vehicle and a scene.
3. On-duty/off-duty state and a basic sheriff role.
4. One traffic stop: approach vehicle, interact with driver, check details, issue warning/citation, or escalate to a lawful arrest flow.
5. Basic suspect behavior, detain/cuff, search, and prisoner transport.
6. Dispatch callout, witness interaction, evidence/report, and MDT loop.
7. One usable store/gas station and one home/interior to prove off-duty civilian life.
8. Expand Ocala/Marion County, then add other counties and departments.

Traffic signal and four-way-stop simulations support the living world, but they must not displace the player/police/off-duty vertical slice as the main development priority.

## Scope discipline

The categories above are the user's stated target features. They are design requirements, not claims that those systems are already implemented. Mark every system as prototype, implemented, or planned accurately in the README and continuity handoff.