# SHERIFF: AMERICAN LIFE

Florida-inspired open-world law-enforcement and civilian roleplay simulator. Target engine: Unity 6 + URP. Initial platform: iPhone 16e.

## First prototype
Cypress County blockout, third-person walking, patrol vehicle prototype, camera toggle, dispatch call waypoint. This repository is source code, not a finished game or IPA.

## Requirements
Unity 6 (6000.x), Universal Render Pipeline, iOS Build Support. Final iOS build/signing requires Xcode on a supported Mac.

## Setup
1. Clone/download this repository.
2. Add its folder in Unity Hub and open with Unity 6.
3. Allow packages to resolve.
4. Create `Assets/Scenes/Main.unity`.
5. Add an empty GameObject named `WorldBootstrap` and attach `CountyWorldBootstrap`.
6. Press Play to generate the test county blockout.

## Planned counties
- Cypress County — Palmetto Crossing
- Bayshore County — Bayshore
- Pine Ridge County — Pine Ridge
- Suncoast County — Suncoast

## Roadmap
Greybox world → mobile HUD and controls → NPC traffic/pedestrians → dispatch and police workflows → civilian jobs/economy/interiors → additional counties and optimization.

All art/audio/models must be original or properly licensed. The first milestone is intentionally small; the full multi-county world is a staged project.