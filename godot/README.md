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
- Touch: on-screen movement and driving controls.

## Important status
The new unsigned-IPA workflow has been committed but has not yet completed a successful run. Export success and IPA validity are therefore unverified. No iPhone runtime, physics, camera, touch HUD or FPS test has been performed. Vehicle suspension, steering, collisions, camera transitions and mobile HUD layout remain unverified. The SUV and buildings are procedural placeholders, not production-grade real models. No actual Ocala GIS/map data or licensed vehicle assets are included yet.

## Next milestones
1. Get the first GitHub Actions validation and unsigned IPA export green.
2. Sign the IPA for installation on a physical iPhone/iPad.
3. Test on target devices and fix runtime issues.
4. Add traffic-stop interaction and a basic dispatch call.
5. Replace the first district blockout with sourced, properly licensed Marion County/Ocala road and building data.
6. Add high-quality licensed vehicle/building/character assets and interiors.
7. Profile on target hardware; 30/60 FPS are targets and 120 FPS is aspirational, not guaranteed.
