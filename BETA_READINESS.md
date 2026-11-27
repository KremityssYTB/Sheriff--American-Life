# SHERIFF: AMERICAN LIFE — Beta Readiness

## Beta objective

Ship a small, stable, testable first release of the Ocala/Marion County patrol experience. Large vehicle collections, additional counties, and downloadable expansion packs are post-beta content unless they are required to make the core loop work.

## Beta acceptance gates

### 1. Build and installation
- [ ] Unity 6.3.25f1 project imports without compiler errors.
- [ ] URP 17.3.0 renderer and generated Main scene are valid.
- [ ] iOS build exports successfully from Unity Build Automation or another verified macOS build path.
- [ ] Xcode project builds successfully.
- [ ] App is signed with the intended Apple development/distribution identity and installs through the chosen beta channel.
- [ ] Launch, background/resume, and clean reinstall are tested on real hardware.
- [x] Pre-build hook targets both iPhone and iPad (source corrected in commit 641ea989535cb07a98725f7f71c012d2a9feace9).
- [ ] Confirm iPhone/iPad target settings survive the complete cloud build pipeline.

### 2. Core patrol loop
- [ ] Player can spawn, walk, sprint, enter/exit patrol SUV, drive, and change camera modes.
- [ ] Touch controls remain usable at supported screen sizes and safe areas.
- [ ] Player cannot become stuck in vehicle or on foot after repeated enter/exit.
- [ ] Duty state and basic officer interaction behave consistently.
- [ ] Traffic stop: approach, initiate stop, interact with driver, inspect documents/history, issue citation or warning, and close stop.
- [ ] Patrol SUV physics controller is tuned against road edges, traffic, props, slopes, and collisions on device (Rigidbody-based first pass is now in source).
- [ ] Arrest: complete arrest, escort prisoner, seat prisoner, transport to jail intake, and complete booking.
- [ ] Dispatch: receive a call, travel to its scene, resolve/report it, and clear it.

### 3. World and traffic
- [ ] Starter county scene loads without missing references or runtime exceptions.
- [ ] Traffic follows routes, obeys signals/stops, and does not repeatedly collide or block intersections.
- [ ] Civilian routines and incident spawns do not create unbounded actors.
- [ ] Patrol and civilian vehicles have correctly aligned visuals, wheels, colliders, and road contact.
- [ ] Player can reach the sheriff office and jail intake without falling through or leaving the playable area.

### 4. Stability and performance
- [ ] No crash during a 30-minute patrol session.
- [ ] No unbounded growth in active traffic, civilians, incident actors, materials, or UI objects.
- [ ] Profile CPU, GPU, memory, thermal behavior, and frame pacing on the lowest supported iPhone and iPad.
- [ ] Inspect the generated URP asset and renderer in Unity; validate SRP Batcher, HDR/MSAA, shadow settings, render scale, and texture streaming on target devices.
- [ ] Inspect the generated URP asset/renderer in Unity and validate SRP Batcher, main-light shadows, additional-light policy, render scale, MSAA, and texture streaming settings against device captures.
- [ ] Verify quality presets and 30/60/120/Unlimited frame-rate options on supported devices; confirm ProMotion plist output and sustained frame pacing.
- [ ] Test low-storage behavior and interrupted app suspension/resume.
- [ ] Resolve all reproducible critical/high-severity bugs before inviting external testers.

### 5. Beta operations
- [ ] Add a privacy policy and support/contact destination before external distribution.
- [ ] Add version/build number and a visible feedback/bug-report route.
- [ ] Keep a known-issues list and tester instructions.
- [ ] Choose distribution: TestFlight requires Apple Developer Program access and a signed build.
- [ ] Do not describe a build as installable or beta-ready until signing, installation, and real-device checks pass.

## Current status (source review only)

- Core prototype systems exist in source: player movement, patrol driving, camera modes, dispatch/incident generation, civilian interactions, traffic stops, citations, arrest/escort/transport/booking, and mobile MDT.
- The world is still a generated blockout with primitive geometry. Vehicle catalog entries are metadata; actual production car models are not yet integrated.
- Touch HUD is currently an immediate-mode Unity GUI prototype, not a finished production mobile interface.
- Optional downloadable-content metadata exists, but no live content browser/downloader/CDN is implemented.
- The iOS pre-build hook previously overrode the project setup's iPhone+iPad setting with iPhone-only. It has now been corrected to iPhoneAndiPad.
- Patrol SUV movement now uses a Rigidbody in FixedUpdate with configurable acceleration, speed, steering, lateral grip, interpolation, and continuous collision detection. This is a first-pass arcade controller, not wheel-collider vehicle simulation; handling still requires Unity and device tuning.
- Editor setup configures a provisional URP baseline with HDR, 2x MSAA, 1.0 render scale, SRP Batcher, 45 m shadows, two cascades, 2048 main-light shadow map, no additional-light shadows, and no depth/opaque textures by default. `MobilePerformanceBootstrap` now supports saved Performance, Balanced (default), and High presets with different frame caps, LOD bias, shadow distance/cascades, MSAA, render scale, and HDR. See `DESIGN/MOBILE_QUALITY_PRESETS.md`. These are provisional settings, not measured performance results; the presets are not yet connected to a visible Settings UI.
- Decorative world-dressing colliders are removed, their owned materials are cleaned up, the duplicate editor-created sun is removed, and world-clock lighting updates are throttled. Civilian traffic shares common materials and uses property blocks for paint; patrol SUV materials are released on teardown.
- An editor-only vehicle prefab validator is now in source at `Assets/Editor/VehiclePrefabValidator.cs`. It checks catalog prefabs for renderers, missing meshes/materials, LODGroup, root scale, renderer/material-slot budgets, and recommended hierarchy anchors. It has not yet been run in Unity.
- `DESIGN/VEHICLE_VISUAL_ASSET_STANDARD.md` defines asset licensing/provenance, prefab hierarchy, axes, wheel/light/interior anchors, LODs, material budgets, and the integration sequence. No production vehicle model is integrated yet.
- The new Settings panel, frame-rate persistence, and iOS ProMotion postprocessor are source-only changes. No Unity compilation, editor validation run, cloud build, Xcode build, signing, TestFlight upload, installation, or real-device performance test has been completed in this work session.

## Release discipline

Treat each unchecked gate as unverified, not as complete. Prioritize build/install and core-loop blockers over new features. Keep post-beta content work on a separate track so it does not destabilize the first playable release.
