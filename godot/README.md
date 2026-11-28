# SHERIFF: AMERICAN LIFE — Godot 4 branch

This is the active Godot 4 development track. The original Unity source remains preserved unchanged on branch `main`.

## Current prototype source
- Godot 4 project using the Mobile renderer.
- Procedural first-playable Ocala district blockout.
- Third-person deputy controller with keyboard and on-screen touch controls.
- Swipe-to-look camera control.
- Basic sky, directional sun, road grid, sidewalks, civic-office anchor, placeholder buildings and palms.
- First drivable Marion County-style patrol SUV prototype using Godot VehicleBody3D and VehicleWheel3D suspension.
- Enter/exit vehicle interaction (E on keyboard or on-screen button).
- On-screen accelerator, reverse, steering and brake controls.
- Follow camera and alternating red/blue emergency lightbar prototype.

## Open
Open the `godot/` folder in Godot 4 and run `scenes/main.tscn`.

## Controls
- On foot: WASD to move, Shift to sprint, drag on the right side to look.
- Enter/exit: E or the on-screen vehicle button when close to the SUV.
- Driving: WASD, Space to brake, L to toggle emergency lights.
- Touch: on-screen movement and driving controls.

## Important status
This is source implementation only. Godot is not installed in the current build environment, so the project has not been imported, parsed, run, or profiled in the Godot editor or on an iPhone. Vehicle suspension, steering, collisions, camera transitions and mobile HUD layout remain unverified. The SUV and buildings are procedural placeholders, not production-grade real vehicle/building models. No actual Ocala GIS/map data or licensed vehicle assets are included yet.

## Next milestones
1. Open in Godot 4 and fix any parser or runtime issues.
2. Validate walking, vehicle entry/exit, steering, braking, collisions and camera.
3. Add a traffic-stop interaction and a basic dispatch call.
4. Replace the first district blockout with sourced, properly licensed Marion County/Ocala road and building data.
5. Add high-quality licensed vehicle/building/character assets and interiors.
6. Profile on target iPhone/iPad hardware; 30/60 FPS are targets and 120 FPS is aspirational, not guaranteed.
