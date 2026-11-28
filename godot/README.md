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

## iOS build pipeline
GitHub Actions now runs on pushes and pull requests to `godot4`:
1. A Linux job imports the project with Godot 4 headless to catch project/script parse errors.
2. A macOS job exports the Godot iOS Xcode project and uploads it as a workflow artifact.

Open the repository's **Actions** tab, select **Godot iOS build**, open the latest successful run, and download `sheriff-american-life-ios-xcode-project`. This is an Xcode project, not an installable IPA.

### IPA signing requirement
A device-installable IPA requires Apple code signing with a valid Apple Developer team, certificate and provisioning profile. The current workflow deliberately does not contain signing credentials and does not claim to produce a signed IPA. Once signing credentials are safely configured as GitHub Actions secrets, a separate signing/export stage can be added. Never commit certificates, private keys, provisioning profiles or Apple account passwords to this repository.

## Open locally
Open the `godot/` folder in Godot 4 and run `scenes/main.tscn`.

## Controls
- On foot: WASD to move, Shift to sprint, drag on the right side to look.
- Enter/exit: E or the on-screen vehicle button when close to the SUV.
- Driving: WASD, Space to brake, L to toggle emergency lights.
- Touch: on-screen movement and driving controls.

## Important status
This is source implementation plus an automated build pipeline. The pipeline has not yet run successfully, so export success is not verified. Godot is not installed in the current build environment; no iPhone runtime, physics, camera, touch HUD or FPS test has been performed. Vehicle suspension, steering, collisions, camera transitions and mobile HUD layout remain unverified. The SUV and buildings are procedural placeholders, not production-grade real models. No actual Ocala GIS/map data or licensed vehicle assets are included yet.

## Next milestones
1. Get the first GitHub Actions validation and iOS export green.
2. Establish secure Apple signing and IPA export.
3. Install and test on a physical iPhone/iPad.
4. Fix gameplay/parser/runtime issues found by those tests.
5. Add a traffic-stop interaction and a basic dispatch call.
6. Replace the first district blockout with sourced, properly licensed Marion County/Ocala road and building data.
7. Add high-quality licensed vehicle/building/character assets and interiors.
8. Profile on target hardware; 30/60 FPS are targets and 120 FPS is aspirational, not guaranteed.
