# Marion County — First Playable Region

Project: SHERIFF: AMERICAN LIFE  
Engine: Unity 6.3  
Render pipeline: Universal Render Pipeline (URP)  
Target device: iPhone 16e  
Bundle ID: `com.kremityss.sheriffamericanlife`

## Scope

Marion County is the first county to be developed and tested. Build a compact, playable vertical slice before expanding it. The long-term world target remains 30 km × 30 km across multiple county regions; this is not a requirement to build all 900 km² in the first milestone.

The first prototype region should prioritize recognizable local road structure and a convincing mix of town, suburban, and rural environments. Exact initial playable bounds should be selected after reviewing usable public GIS data and mobile performance.

## Geographic references

Use public geographic datasets and references for:
- Marion County boundary and road centerlines
- Major roads and highway connections
- Waterways, lakes, wetlands, and public conservation areas
- Publicly documented historic landmarks
- Public aerial/geographic data only where its license permits game-development use

Do not assume every public map tile or aerial image is licensed for redistribution in a game. Keep source attribution and license notes with imported datasets. Rebuild buildings, signs, vehicles, and props as original assets or use assets with compatible licenses.

## Priority locations

1. Ocala — primary urban hub; downtown street grid, civic buildings, local shops, residential streets, and a sheriff facility.
2. Silver Springs — springs-oriented visitor area, waterways, wooded roads, tourism businesses.
3. Belleview — small-town center, residential streets, commercial strip.
4. Dunnellon — small-town streets, river setting, nearby rural roads.
5. McIntosh — historic small-town character and surrounding countryside.
6. Ocklawaha — lake-adjacent rural communities and wooded roads.
7. Fort King — historic landmark reference for a public-history location.

These are reference destinations. Their in-game distances and relative positions may be compressed for playability; label any fictionalized or relocated content clearly in design data.

## Environment requirements

### Roads and navigation
- Recreate selected public road alignments and junctions from licensed GIS centerline data.
- Include county roads, local streets, rural two-lane roads, and highway segments.
- Add road markings, speed limits, stop signs, traffic signals, shoulders, ditches, driveways, and road names where appropriate.
- Build a connected route network with useful alternate paths rather than a decorative road grid.

### Buildings and properties
- Florida-style single-family homes, mobile homes, apartments, duplexes, shops, restaurants, gas stations, warehouses, barns, horse farms, and ranch properties.
- Distinct town-center architecture and locally appropriate landscaping.
- Original building meshes/materials or properly licensed assets; do not copy proprietary game assets.

### Public services and gameplay
- Sheriff station with parking and an interior-ready layout.
- Fire/EMS facilities as the region expands.
- Dispatch call locations, traffic stops, witness locations, and basic civilian activity.
- Initial gameplay loop: patrol, receive call, navigate to call, interact, and clear call.

## Mobile implementation

- Unity 6 URP; target iPhone 16e.
- Divide the world into streamed sections with clear load/unload boundaries.
- Use LOD groups, occlusion culling, baked lighting where appropriate, texture atlases, and GPU instancing for repeated props.
- Use configurable traffic and pedestrian density; avoid simulating distant NPCs at full fidelity.
- Profile on device before increasing view distance, density, or asset detail.
- Keep the first milestone focused on stable movement, driving, navigation, and dispatch rather than adding every planned system at once.

## Milestone sequence

1. Import and document permitted GIS road/boundary data.
2. Create terrain and a road-network greybox.
3. Add Ocala town-center and representative residential/rural blocks.
4. Add a drivable patrol vehicle and walking controls.
5. Add a basic sheriff facility and dispatch call loop.
6. Add Silver Springs, Belleview, Dunnellon, McIntosh, and Ocklawaha as subsequent subregions.
7. Test streaming and performance on iPhone 16e.
8. Only then begin the next county.

## Acceptance criteria for the first playable slice

- The player can walk and drive through a connected area.
- Selected roads and intersections follow documented geographic references.
- At least one recognizable town-center block, residential block, and rural route are explorable.
- A basic dispatch call can be accepted, reached, and cleared.
- Region loading does not cause unacceptable stalls or memory spikes on the target phone.
- All imported data and assets have recorded source/license information.

## Scope boundary

This document defines the first county's design target. It does not claim that the county has already been modeled, that GIS data has already been imported, or that a playable iOS build has passed testing.


## Daily delivery plan

The agreed strategy is to complete Marion County/Ocala before starting the other counties. Daily work should add a playable map/system increment, push to GitHub `main`, update `README.md`, `PROJECT_CONTINUITY.md`, and `COUNTIES/OCALA_DAILY_ROADMAP.md`, and report build/test status accurately. See [Ocala Daily Roadmap](OCALA_DAILY_ROADMAP.md).

The initial vehicle catalogue is class-based, not a claim to recreate every real-world model: patrol SUV, civilian sedan, civilian SUV, pickup, delivery van and emergency vehicle first; add motorcycles, buses, heavy trucks, utility vehicles and more specialist fleet types after the shared vehicle system is working.
