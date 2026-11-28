# SHERIFF: AMERICAN LIFE — Godot 4 branch

This is the active Godot 4 development track. The original Unity source remains preserved unchanged on branch `main`.

## Current prototype source
- Godot 4 project using Forward+ on desktop and the Mobile renderer on mobile/iOS.
- Desktop high-quality environment enables SDFGI and Volumetric Fog; these are disabled on mobile to preserve compatibility and performance.
- 3D render scale is set to 1.0 (native resolution; no internal 3D downscaling).
- Procedural first-playable Ocala district blockout.
- Third-person deputy controller with keyboard and on-screen touch controls.
- Swipe-to-look camera control.
- Basic sky, directional sun, road grid, sidewalks, civic-office anchor, placeholder buildings and palms.
- First drivable patrol SUV prototype using Godot VehicleBody3D and VehicleWheel3D suspension.
- Enter/exit vehicle interaction and on-screen driving controls.
- Dispatch call manager with routine welfare checks, traffic complaints and suspicious-vehicle calls.
- In-game dispatch panel with offer/accept states, incident details, a 3D destination beacon, live unit-distance readout and a 10 m arrival check before call clearance.
- Follow camera and alternating red/blue emergency lightbar prototype.

## iOS unsigned IPA pipeline
GitHub Actions runs on pushes to `godot4` and manual dispatch:
1. Linux imports the project with Godot 4 headless to catch project/script parse errors.
2. macOS exports the Godot iOS Xcode project.
3. Xcode builds the app with code signing disabled.
4. The workflow packages the resulting `.app` inside `Payload/` as `SheriffAmericanLife-unsigned.ipa` and uploads it as the artifact `sheriff-american-life-unsigned-ipa`.

To retrieve it, open the repository's **Actions** tab, select **Godot iOS build**, open a successful run, and download the unsigned IPA artifact. No Apple Team ID or signing secrets are required for this unsigned build path.

### Important: unsigned does not mean installable
The IPA is an unsigned package for later signing. iOS normally will not install or launch it as-is. It must be signed with a valid certificate and provisioning profile for the intended installation method/device. This workflow does not produce a signed IPA and does not bypass Apple's signing requirements. Never commit certificates, private keys, provisioning profiles or Apple account passwords.

## Open locally
Open the `godot/` folder in Godot 4 and run `scenes/main.tscn`.

## Controls
- On foot: WASD to move, Shift to sprint, drag on the right side to look.
- Enter/exit: E or the on-screen vehicle button when close to the SUV.
- Driving: WASD, Space to brake, L to toggle emergency lights.
- Dispatch: request a call, review and accept it, travel to the 3D beacon, then clear the call within 10 m.
- Touch: on-screen movement and driving controls.

## Important status
The unsigned-IPA workflow is still under active repair. Godot validation has passed on recent commits, and the iOS export has generated an Xcode project, but packaging has not yet completed successfully. IPA availability and validity remain unverified. No iPhone runtime, physics, camera, touch HUD or FPS test has been performed. Vehicle suspension, steering, collisions, camera transitions and mobile HUD layout remain unverified. The SUV and buildings are procedural placeholders, not production-grade real models. No actual Ocala GIS/map data or licensed vehicle assets are included yet.

## Next milestones
1. Get the first GitHub Actions validation and unsigned IPA export green.
2. Sign the IPA for installation on a physical iPhone/iPad.
3. Test on target devices and fix runtime issues.
4. Add traffic-stop interaction, vehicle approach/positioning and a structured incident report after call clearance.
5. Replace the first district blockout with sourced, properly licensed Marion County/Ocala road and building data.
6. Add high-quality licensed vehicle/building/character assets and interiors.
7. Profile on target hardware; 30/60 FPS are targets and 120 FPS is aspirational, not guaranteed.

## Product direction: open-world law-enforcement roleplay

The design target is a grounded, persistent-feeling police and civilian roleplay experience inspired by the systemic depth of LSPDFR and the player-led roleplay structure of FiveM. It is not intended to be a small arcade driving game.

### Experience pillars
- **Believable county world:** build a connected Marion County/Ocala region with recognizable road hierarchy, neighborhoods, commercial strips, rural roads, public buildings, parking lots, interiors and navigable landmarks. Use appropriately licensed GIS/map references and original game-ready assets; do not present the current blockout as an accurate real-world map.
- **Authentic police workflow:** duty selection, unit/callsign identity, radio channels, dispatch calls, traffic stops, citations, arrests, prisoner transport, reports, evidence and an in-game MDT/CAD workflow.
- **Civilian roleplay:** civilian identities, jobs, vehicles, homes, stores, interactions, lawful and unlawful activities, and persistent character progression. Multiplayer/server authority is a later architecture milestone, not implemented in this prototype.
- **Professional interface:** restrained county-agency visual language, readable CAD/MDT panels, clear unit state, radio and dispatch information, consistent button states, and touch layouts that adapt to phone/tablet dimensions.
- **Simulation-first presentation:** credible vehicle proportions and handling, human-scale architecture, road markings, traffic behavior, lighting, weather, ambient sound and day/night transitions. Realistic appearance requires production assets and on-device iteration; procedural geometry remains temporary.

### Latest interface milestone
- Replaced the plain prototype title/status text with a county/unit/radio header.
- Added compact dark CAD-style touch controls with normal, hover and pressed visual states.
- Repositioned the dispatch panel below the unit header on narrow phone screens to prevent the two panels from occupying the same area.
- Reflowed on-screen movement and vehicle controls using the current viewport size, including a resize callback.
- This is a source implementation milestone only. Godot headless validation and physical-device layout checks are still required.

### Next production sequence
1. Validate the revised mobile HUD in Godot and on an iPhone/iPad aspect ratio; fix any input capture or overlap defects.
2. Replace the single cross-shaped blockout with a road-network-driven county layout and named districts/landmarks.
3. Add a minimap/GPS route layer tied to the same road graph used by traffic and dispatch.
4. Build a proper MDT/CAD screen with unit login, callsign, radio, call queue, call history and incident report workflow.
5. Add traffic-stop interaction, civilian AI responses and evidence/report linkage.
6. Establish an asset pipeline for original/licensed characters, vehicles, buildings, interiors, road props, audio and animations.
7. Design the civilian roleplay and multiplayer authority model before attempting online persistence.

The current Godot scene is still an early procedural prototype. It does not yet contain a real Marion County map, photoreal assets, multiplayer, a complete civilian roleplay loop, production animation or verified iOS runtime performance.

## County environment milestone

The procedural test cross has been replaced with a connected five-by-five street grid spanning the current playable blockout. The center corridor is wider than neighborhood streets; road edges, dashed center markings, sidewalks, central crosswalks, varied building footprints, a sheriff office parking area and Florida palm landscaping establish a more coherent county-town layout.

This is still a greybox environment, not a geographically accurate Marion County recreation. The next map pass should introduce named districts, a road graph for GPS/dispatch routing, intersections with turning geometry, traffic signals, parking access, rural connectors and authored landmark silhouettes. Replace primitive building and vehicle meshes with original or properly licensed production assets before describing the map as photorealistic.


## Final world scale target (design specification)

The final playable region is **225 km² total**, counting both land and water. This replaces the earlier 33 mi × 33 mi proposal. A square of 225 km² would be 15 km × 15 km, but the actual region should use an irregular, believable regional shape rather than a square boundary. The current 240 m × 240 m scene remains a temporary first-district greybox; this target is a design specification, not implemented map content.

### Land, water and regional layout
- Design a connected Florida-inspired region with a dense Ocala-style urban center, suburbs, commercial and industrial corridors, smaller towns, rural communities, farms, forests, wetlands, lakes, rivers and navigable waterways.
- The 225 km² budget includes all land and water. Do not count water as extra area outside the stated total. Set the final land/water balance through the authored geography; do not assume a percentage before the regional layout is designed.
- Water must have gameplay purpose: navigable routes, shoreline access, bridges, boat ramps, docks, marinas and suitable waterfront locations. Waterways should connect naturally where geography allows, not appear as disconnected decorative pools.
- Support road vehicles including patrol SUVs, civilian cars, trucks and motorcycles, plus boats for suitable rivers, lakes and coastal/inlet areas. Vehicle and boat handling, access points, fuel/maintenance and emergency response behavior should be developed as separate gameplay systems.
- Build a coherent road network rather than stretching the current five-by-five grid. Include highways, county roads, local streets, bridges, rural connectors and distinct neighborhoods.
- Major destinations and district-to-district journeys should commonly take about **10–20 minutes by road or water**, depending on distance, route, vehicle, traffic and speed. Do not force every short trip to take that long.
- Keep the region connected and avoid artificial loading screens between ordinary neighboring areas.
- Use streamed world cells and level-of-detail tiers. Keep nearby streets, buildings, vehicles, boats and pedestrians detailed; simplify distant scenery and reduce simulation for far-away actors.
- Keep roads, waterways, intersections, bridges, addresses, speed limits, jurisdiction boundaries, docks, boat ramps and destination nodes in shared world data so GPS, dispatch, traffic, civilian navigation and marine navigation use consistent routes.
- Treat county lines and neighboring jurisdictions as explicit world geography. Validate route lengths and travel times in-game before locking district boundaries.

### Production sequence
1. Define the region boundary and land/water geography within the 225 km² total.
2. Build the shared road-and-water route graph, including bridges, docks and launch points.
3. Divide the region into streamable cells with mobile LOD and simulation budgets.
4. Expand outward from the current Ocala prototype with named districts, towns and rural areas.
5. Add road vehicles and marine craft with separate, testable handling and interaction systems.
6. Validate performance and realistic travel times on supported iPhones and iPads before increasing world density.

The map is intended to deliver the scale, variety and systemic open-world feel requested, without copying another game's map or assets. It remains a staged production target; the current build does not yet contain this 225 km² region.

## Florida geographic data plan

Use real Florida geospatial datasets as the geographic foundation for the 225 km² world, centered on the existing Ocala/Marion County concept. This means adapting real road geometry, waterways, land cover, terrain and public geographic features into a game-scale scene; it does not mean importing the entire state or copying aerial imagery as finished game art.

Candidate authoritative sources to evaluate before importing each layer:
- Florida Department of Transportation GIS: statewide road centerlines, county roads, state roads, interstates, bridges and traffic-related layers. https://www.fdot.gov/statistics/gis/default.shtm
- Florida Geographic Data Library (University of Florida GeoPlan): statewide and regional GIS layers including transportation, hydrography, land cover, soils and boundaries. https://fgdl.org/
- Florida Department of Environmental Protection Geospatial Open Data: water resources, wetlands, land cover, oceans/estuaries, recreation and environmental layers. https://geodata.dep.state.fl.us/
- FDOT Transportation Data Portal: searchable and downloadable transportation datasets. https://www.fdot.gov/data/index

For every imported dataset, record the publisher, dataset title, version/date, coordinate reference system, attribution requirements, license/terms and any restrictions in a project data manifest. Confirm that the specific layer permits redistribution and use in a commercial game before committing it. Convert source coordinates into a local metric coordinate system, clip data to the chosen 225 km² boundary, simplify geometry for mobile rendering, and preserve road/water connectivity in shared navigation data. GIS features provide geographic structure only; create original game-ready buildings, props, vegetation, signage, vehicles and interiors.

The initial geographic focus should connect Ocala's urban street pattern with a plausible surrounding Marion County landscape. Include usable waterways only where the selected footprint and source data support them; add boat ramps, docks and navigable water routes as explicit gameplay nodes. Do not claim exact real-world accuracy until the imported layers have been checked against their metadata and on-the-ground geography.


## Florida regional catalog milestone

A data-driven Florida region catalog now lives at `data/florida_region.json`. It records the 225 km² total playable-area target, a metric coordinate envelope, all 67 Florida county identifiers grouped into nine broad geographic regions, twelve condensed destination districts, road/water transport modes, representative landmarks and watercraft access points. The catalog is explicitly a **condensed fictional game geography**: statewide county/jurisdiction coverage does not mean all 67 counties are represented at real-world scale or with accurate county boundary polygons.

The `scripts/florida_region_catalog.gd` loader validates the county count, area target, district structure and boat-access references. The main scene loads this catalog during startup and reports the loaded counts. This is a data/architecture milestone, not a completed terrain import: the current 240 m blockout remains the only built environment. Actual Florida GIS road, hydrography, land-cover and elevation layers still need to be sourced, license-checked, clipped to the 225 km² footprint, converted to local metric coordinates and rendered through streamed cells. The current boat access entries are design metadata; no drivable boat is implemented yet.


### Florida navigation graph milestone

The regional catalog now includes twelve authored district anchor points within the 15 km × 15 km metric envelope, plus a first-pass connected road graph and separate marine-route graph. Road edges describe interregional highways, county roads, urban arterials and causeways; marine edges describe bay channels, intracoastal routes, rivers, lakes and canals. These are compressed-world design connections, not verified real-world route geometry, and are not yet rendered or used for live GPS/dispatch pathfinding. The catalog loader validates graph node and edge references at startup. The next engineering pass should turn this shared data into route queries and streamed terrain cells, then replace authored placeholder geometry with appropriately licensed Florida GIS layers.


### Shared route query milestone

The Florida catalog now exposes a shortest-path query for the authored road graph and the separate marine graph. It returns an ordered district-node path and an estimated route length based on compressed anchor coordinates and basic road-class multipliers. Startup performs a small road-route and water-route connectivity smoke check. This is an early navigation foundation: route lengths are design estimates, not real drive/boat times, and the graph does not yet contain street-level intersections, turn restrictions, one-way roads, bridges with clearance, tides, hazards or live traffic. Those are required before production GPS, dispatch routing or realistic marine navigation.


### GIS source registry and streaming grid

The catalog now records official source families for FDOT roadway GIS, the University of Florida GeoPlan Florida Geographic Data Library, Florida DEP geospatial open data and the South Florida Water Management District portal. FDOT's published GIS page describes statewide road/bridge/traffic shapefile layers and notes UTM 17 / NAD83 for listed statewide downloads; source data must be transformed into the game's local metric coordinate system. FGDL layers may originate with different agencies, so the publisher and terms must be checked per layer. The project deliberately marks all source licensing checks as pending: no dataset is cleared for redistribution merely because it is publicly viewable or downloadable.

The 15 km × 15 km design envelope is divided into a 30 × 30 grid of 500 m cells. The catalog exposes world-bound checks and world-position-to-cell lookup to support a future streamer. This is only the coordinate/cell foundation; no runtime cell loading, terrain generation or GIS geometry import is implemented yet.
