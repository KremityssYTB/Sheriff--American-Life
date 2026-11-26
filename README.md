# SHERIFF: AMERICAN LIFE

A Florida-inspired open-world law-enforcement and civilian roleplay simulator. Target: iPhone 16e. Engine: Unity 6 with Universal Render Pipeline (URP).

## Current prototype foundation

- Unity 6 project pinned to 6000.3.25f1
- URP 17.0.3 package
- Editor setup command that creates a mobile URP asset, Universal Renderer, Main scene, and iOS player settings
- Cypress County runtime blockout
- Third-person movement and camera-mode toggle
- Basic patrol SUV movement prototype
- Dispatch call acceptance and arrival trigger
- GitHub Actions workflow that exports an Xcode project, compiles it on macOS, and packages an unsigned IPA

This is an early greybox foundation, not a finished open-world game. Models, animation, mobile touch HUD, traffic AI, interiors, audio, and full police/civilian systems remain to be developed.

## Project setup

1. Open the repository with Unity 6.3 / Editor 6000.3.25f1 and allow packages to resolve.
2. In Unity, run **SHERIFF → Setup Mobile URP Project**.
3. The setup command creates the mobile URP asset, its Universal Renderer, and Assets/Scenes/Main.unity.
4. Press Play to run the Cypress County blockout.
5. Keyboard prototype controls: WASD / arrow keys to move, V to toggle camera mode, hold E plus movement keys to drive the patrol SUV, and J to accept/clear the dispatch call.

## iOS build

The GitHub Actions workflow requires a valid Unity license in the repository secret UNITY_LICENSE. It exports the Unity iOS Xcode project on Linux, then uses a macOS runner with Xcode to compile and package an unsigned IPA. No Apple signing certificate or provisioning profile is added to this artifact.

An unsigned IPA is not directly installable on an iPhone. It must be signed with a compatible certificate and provisioning profile before installation.

## iOS configuration

- Bundle identifier: com.kremityss.sheriffamericanlife
- Architecture: ARM64
- Graphics API: Metal
- Scripting backend: IL2CPP
- Render pipeline: URP

## Planned counties

- Cypress County — Palmetto Crossing
- Bayshore County — Bayshore
- Pine Ridge County — Pine Ridge
- Suncoast County — Suncoast

## Roadmap

Greybox world → touch controls and mobile HUD → traffic/pedestrian AI → dispatch and police workflows → civilian jobs/economy/interiors → additional counties → mobile performance and visual polish.

All art, audio, and models must be original or properly licensed.
