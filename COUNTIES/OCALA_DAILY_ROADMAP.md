# Marion County / Ocala — Daily Completion Roadmap

This is the first county production plan. All daily changes are pushed to GitHub `main`, recorded here, and kept playable where the build pipeline permits. Do not start the next county until the Ocala release checklist is met.

## What “Ocala complete” means for the first county release

A coherent, connected and playable Marion County region—not every square mile of the real county reproduced at full scale. Start with an explorable Ocala urban core, then add surrounding communities and rural routes in streamed/optimized chunks.

### Required Ocala districts and county areas

1. Downtown Ocala — recognizable street grid, courthouse/civic block, shops, restaurants, parking, pedestrians.
2. Sheriff / public safety district — sheriff HQ, parking, dispatch, holding/booking area, fire/EMS response facility.
3. Residential Ocala — houses, apartments, neighborhood streets, driveways, parks and civilian routines.
4. Commercial corridors — grocery, convenience store, gas stations, auto service, retail, bank and restaurants.
5. Highway and arterial connections — major road corridors, ramps, intersections, traffic controls, shoulders and speed limits.
6. Rural Marion — horse farms, ranches, pine woods, lakes, wetlands, two-lane roads and driveways.
7. Silver Springs — springs visitor area, waterways, wooded roads and tourism businesses.
8. Belleview — small-town center, homes and commercial strip.
9. Dunnellon — river town, historic main street and rural surroundings.
10. McIntosh and Ocklawaha — compact historic/lake-adjacent settlements and countryside.
11. Fort King — historic/public-history landmark area.

Exact map boundaries, road alignments and public points of interest must be based on documented, properly licensed geographic data. The first browser preview is schematic and is not an accurate GIS map.

## Vehicle catalogue strategy

Do not try to model every real-world make/model before the first playable release. Build a modular vehicle roster by gameplay role, using original/unlicensed-safe models and fictionalized names where necessary.

- Civilian: compact, sedan, SUV, pickup, minivan, delivery van, taxi, motorcycle.
- Public service: sheriff patrol SUV, sheriff sedan, unmarked unit, highway patrol sedan/SUV, fire engine, ambulance, tow truck.
- Utility: bus, box truck, semi tractor/trailer, work pickup, construction vehicle.
- Each vehicle needs a shared data definition for dimensions, mass, acceleration, top speed, braking, steering, seat count, audio, paint, lights and role.
- First playable roster: sheriff SUV, one civilian sedan, one civilian SUV, one pickup, one delivery van, one emergency vehicle. Expand the roster after the common driving system works.

## Daily update format

Every development day:
1. Inspect latest `main` and relevant file SHAs.
2. Choose one small playable milestone.
3. Implement source and map/content changes.
4. Update README, this roadmap and PROJECT_CONTINUITY.
5. Re-fetch changed files and run source-level checks.
6. If available, run the cloud build and report its actual status.
7. Never label an uncompiled/unrun source change as tested or playable.

## Day 1 — Immediate playable preview and build blockers

- [x] Audit current Unity player runtime prototype and cloud build status.
- [x] Fix the player HUD's unresolved duty-state variable.
- [x] Add a phone-browser preview source with walk/drive, enter/exit, duty toggle, dispatch call, and location interactions.
- [x] Add a Pages deployment workflow. Deployment still needs the owner to enable Pages in repository Settings.
- [x] Keep the Unity project as the main game; browser preview is a temporary playtest companion, not a Unity build.
- [x] Verify the Unity cloud workflow's missing license secret and missing scene/build configuration blockers.

## Day 2 — Ocala blockout and map sectors

- Build a connected, navigable Ocala street layout with named districts and road hierarchy.
- Add street names, road markings, sidewalks, driveways, parks and parking.
- Establish a map-coordinate system and sector boundaries for later streaming.

## Day 3 — Core vehicles

- Build the reusable vehicle data/handling system.
- Add the first six roster entries listed above.
- Add vehicle spawn/selection and safe enter/exit points.

## Day 4 — Public services and buildings

- Sheriff office, dispatch/parking, basic fire/EMS buildings.
- Enterable interiors for the sheriff office and one civilian building.

## Day 5 — Civilian life

- More pedestrians and traffic, schedules, shopping destinations, homes and ambient activities.
- Add a working convenience store/grocery interaction and basic purchase/economy state.

## Day 6 — Police gameplay

- Traffic stop interaction, driver dialogue, identification, warning/citation.
- Suspect compliance, detain/cuff and arrest state.

## Day 7 — First county playtest candidate

- Dispatch call loop, one pursuit/backup scenario, report/MDT link.
- Fix blockers, reduce mobile load, publish the latest playable candidate if cloud build works.

## County release gate

- Walk, run, drive, enter/exit and switch cameras.
- On-duty and off-duty states behave correctly.
- A complete traffic stop and arrest can be played.
- Dispatch calls can be accepted, reached and cleared.
- At least one store, gas station, home and public-service interior are usable.
- Core vehicle roster works.
- Ocala districts connect by drivable roads.
- Build succeeds and is tested on target hardware before declaring a release.


### Day 1 deployment gate (verified)

- Browser preview source exists at `web-demo/index.html`.
- Deployment workflow exists at `.github/workflows/deploy-web-demo.yml`.
- First deployment attempt failed because GitHub Pages was not enabled for the repository.
- A second attempt with automatic enablement failed because the connected GitHub integration is not authorized to create the Pages site.
- **Owner action required:** in repository Settings → Pages → Build and deployment, select **GitHub Actions** as the source. Then rerun the “Deploy Ocala browser preview” workflow from the Actions tab.
- Do not call the browser preview live until the deployment workflow succeeds and the page is opened on the iPhone.


## Day 1 follow-up — Three.js 3D browser playtest (2026-11-27)

- [x] Replace the 2D companion page with a Three.js 0.180.0 WebGL 3D scene.
- [x] Add procedural Ocala road grid, district buildings, greenery, civilian traffic, deputy avatar, and patrol SUV.
- [x] Add mobile touch movement, sprint, drag-to-look camera, vehicle entry/exit and simplified driving.
- [x] Add duty toggle, emergency lights, dispatch call marker, interaction prompts, and browser FPS readout.
- [x] Keep the native Unity iOS/iPadOS project separate and unchanged by the browser implementation.
- [ ] Verify GitHub Pages deployment.
- [ ] Open and test on iPhone Safari and iPadOS Safari.
- [ ] Validate orientation changes, touch controls, sustained FPS, memory, and background/resume.
- The browser scene is a schematic test district and not a Unity WebGL export or an accurate real-world map.


### Day 1 browser deployment result
- [x] GitHub Pages workflow run #8 completed successfully for the 3D browser preview.
- Published URL: https://kremityssytb.github.io/Sheriff--American-Life/
- [ ] Real iPhone Safari and iPadOS Safari launch, WebGL, touch, orientation, and performance tests remain pending.
