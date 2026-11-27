# Downloadable Content and Expansion Architecture

## Product direction

SHERIFF: AMERICAN LIFE is planned as a large, expandable mobile game with a compact required base install and optional downloadable content. The game must not require every county, vehicle collection, interior, and audio pack to be installed at once.

## Base install

The base install contains the boot flow, save/profile systems, core gameplay code, shared shaders/materials, the starter Marion County/Ocala region, and the minimum vehicle/character assets needed to begin playing. Core systems must remain usable when optional packs are absent.

## Optional pack families

- Vehicle collections: classic and vintage vehicles, American muscle, pickups and work trucks, imports, sports cars, supercars, motorcycles, SUVs, police fleets, and future model-year collections.
- County expansions: additional Florida counties and their city, town, rural, road, building, and service-area content.
- Departments and roles: sheriff, municipal police, highway patrol, fire/rescue, EMS, and specialist units.
- World content: enterable homes, businesses, stations, hospitals, jails, courthouses, garages, and interiors.
- Gameplay: missions, call types, activities, equipment, clothing, character options, and role progression.
- Audio/visual: voice packs, radio/audio sets, weather/lighting enhancements, and optional high-detail assets.

## Runtime and storage principles

1. Show pack name, category, version, approximate download size, dependencies, and installed state in an in-game content browser.
2. Let players download, pause/resume, update, and remove optional packs. Never remove required base content or a dependency still used by an installed pack.
3. Check free storage before download and reserve temporary space for download plus extraction. Explain required space before the player confirms.
4. Download over HTTPS, verify SHA-256 before installation, and install atomically so interrupted or corrupt downloads do not replace a working pack.
5. Keep bundle versions and stable pack IDs separate from display names. Save games must tolerate a missing optional pack and explain which content is unavailable.
6. Load only nearby/active world content. Use Addressables or AssetBundles with additive scene loading, distance-based streaming, LODs, and memory budgets; unload unused bundles safely.
7. Provide Wi-Fi-only download preference and a setting to restrict large downloads on cellular. Respect iOS backgrounding and interrupted connections.
8. Keep platform-compatible bundles separate where required; iOS bundles must be built for the iOS player and Metal/URP configuration.
9. The content browser should distinguish free updates, included content, and optional add-ons. No paywall or monetization model is assumed by this technical design.

## Current source foundation

- `Assets/Scripts/Content/ContentPackCatalog.cs` defines categories and a ScriptableObject manifest entry with stable ID, version, estimated size, HTTPS bundle URL, SHA-256, dependencies, base-install status, and removability. It validates duplicate IDs and basic metadata.
- `Assets/Scripts/Vehicle/VehicleModelCatalog.cs` provides a representative historical vehicle lineup and supports assigning original or properly licensed model prefabs through catalog entries.
- `CountyWorldBootstrap` can select a vehicle catalog entry for traffic and instantiate an assigned visual prefab while preserving the traffic actor's root physics and AI.

## Not yet implemented

The manifest is a data foundation, not a working in-game downloader or store. No CDN, remote manifest endpoint, hosted AssetBundles, Addressables groups, download UI, progress/resume logic, SHA-256 download verification, install/uninstall manager, storage reservation, or save-game dependency handling is configured. Actual model meshes and world packs still need to be created/imported and packaged. Unity compilation, iOS build, network download, storage, and device performance tests remain outstanding.

## Production order

1. Finish a stable, playable Ocala base game.
2. Establish build automation for iOS-compatible content bundles.
3. Configure a versioned HTTPS content manifest and CDN/object storage.
4. Implement the in-game browser, download manager, integrity checks, and uninstall flow.
5. Release the first optional vehicle collection, then county/interior/gameplay expansions.
6. Add packs incrementally while profiling install size, memory, download size, and iPhone/iPad performance.
