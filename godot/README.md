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
