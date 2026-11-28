# SHERIFF: AMERICAN LIFE — Godot 4 branch

This is the new Godot 4 development track. The original Unity project remains preserved on the `main` branch and is not overwritten.

## Current prototype
- Godot 4 project using the Mobile renderer.
- Procedural first-playable Ocala district blockout.
- Third-person deputy controller with keyboard and on-screen touch controls.
- Swipe-to-look camera control.
- Basic sky, directional sun, road grid, sidewalks, civic-office anchor, placeholder buildings and palms.

## Open
Open this folder in Godot 4 and run `scenes/main.tscn` (or the project main scene).

## Important status
This is an initial source prototype. It has not yet been imported, parsed, run, or profiled in the Godot editor or on an iPhone. The buildings, vegetation and deputy are placeholder geometry, not photogrammetry or production assets. No actual Ocala GIS/map data or licensed real vehicle models are included yet.

## Next milestones
1. Validate the project in Godot 4 and fix parser/runtime issues.
2. Add a drivable patrol SUV with enter/exit and mobile vehicle controls.
3. Replace the first district blockout with sourced, properly licensed Marion County/Ocala road and building data.
4. Add high-quality licensed vehicle/building/character assets.
5. Add police duty loop, traffic stops, dispatch, MDT, arrest and jail workflows.
6. Profile on target iPhone/iPad hardware; 30/60 FPS are initial targets, 120 FPS is aspirational and device/scene dependent.
