# Mobile Visual Quality and Performance Plan

## Current truth

The project is Unity 6.3.25f1 with URP 17.3.0 and targets iPhone and iPad. The current county and vehicle visuals are procedural blockout geometry. The vehicle catalog contains identity metadata only; no production vehicle meshes are assigned. The generated URP asset is created by editor setup rather than committed as a serialized asset. Editor setup now applies a balanced mobile baseline, but the generated asset and renderer still need inspection in Unity.

## Visual target

Aim for a grounded, high-detail Florida county presentation while preserving readable gameplay and stable frame pacing on supported iPhones and iPads. Visual quality must come from coherent art direction, PBR materials, strong environment composition, lighting, real vehicle meshes, authored foliage, and camera presentation—not simply enabling every expensive renderer feature.

### Art and asset requirements

- Replace primitive buildings with modular, original or properly licensed architecture, trim, signage, doors, windows, and interiors.
- Replace generic traffic silhouettes and the procedural patrol SUV with production meshes that include correct scale, pivots, UVs, material slots, wheel anchors, glass, lights, and interiors.
- Author PBR texture sets with mobile-appropriate resolutions and ASTC compression where supported.
- Use LOD Groups for vehicles, buildings, trees, and repeated props; simplify or hide small details at distance.
- Use shared materials and atlases where practical. Avoid runtime material instantiation per repeated object.
- Use baked lighting for static environment elements where it produces a better mobile cost/quality balance. Keep real-time lights limited and intentional.
- Use reflection probes selectively around important interiors and vehicle presentation areas rather than covering the whole map with expensive probes.
- Add authored sky, color grading, exposure, and restrained post-processing only after a real device profile confirms the cost.

## URP asset baseline added

Editor setup now configures the generated URP asset with HDR enabled, 2x MSAA, render scale 1.0, SRP Batcher enabled, dynamic batching disabled, 45 m shadow distance, two cascades, 2048 main-light shadow map, soft main-light shadows, additional-light shadows disabled, at most two additional lights per object, and depth/opaque textures disabled until a feature requires them. These are starting values and still require generated-asset inspection and device profiling.

## Runtime baseline added

`MobilePerformanceBootstrap` now applies:
- 60 FPS target with VSync disabled for mobile frame pacing.
- LOD bias of 1.0.
- 45 m global shadow distance and two cascades as a starting point.
- Background loading priority below normal.
- Mipmap streaming enabled with a 256 MB budget; this only helps textures authored with mipmap streaming enabled.

These are starting values, not measured final settings. URP asset settings can override or further constrain rendering behavior. The profile has not been compiled or tested on an iPhone/iPad.

## URP tuning gates

Inspect the generated URP asset and renderer in Unity 6.3.25f1 before changing settings. Establish separate high, balanced, and performance tiers only after profiling representative scenes.

| Feature | Initial direction |
|---|---|
| Renderer | URP Forward for broad mobile compatibility; evaluate Forward+ only if additional-light count justifies it |
| SRP Batcher | Enable and verify shader compatibility |
| Main light | One directional sun; real-time shadows with short mobile distance |
| Additional light shadows | Disabled |
| Additional lights | Disabled or per-vertex unless a measured scene needs more |
| Shadow cascades | Two as a starting point; test one on lower tier |
| Soft shadows | Test quality and cost; disable or reduce on lower tier |
| HDR | Enable only if the chosen color grading/bloom path needs it |
| Render scale | Start at 1.0 on high tier; test lower scales only if GPU-bound |
| MSAA | Compare 2x and 4x on device; keep the lowest cost that meets edge quality |
| Depth/opaque textures | Disable unless a feature actually consumes them |
| Reflection probes | Disable blending/box projection on lower tier unless visually necessary |
| Cameras | Keep one gameplay camera; avoid unnecessary secondary cameras |
| Post-processing | Keep restrained; measure bloom, SSAO, depth effects, and color grading individually |

## Optimization work already made

- Generated scene setup no longer creates a second directional sun; the world clock owns the county sun.
- Decorative visual-dressing primitives no longer retain primitive colliders. Gameplay collision remains on the underlying road/building geometry.
- Dressing-created materials are destroyed when their owning visual-dressing component is destroyed, preventing repeated world regeneration from leaving those materials allocated.
- Dynamic sun transform/color/intensity updates are throttled to a 0.15 second interval; shadow configuration is applied when the sun is assigned rather than rewritten on every lighting update.

## Required profiling and acceptance

Do not claim a performance target has been met until Unity Profiler and real-device measurements exist. Capture:
- CPU main/render thread and GPU frame time.
- Frame pacing and sustained FPS over a 30-minute patrol.
- Memory use, texture memory, draw calls, batches, SetPass calls, and shader variants.
- Thermal behavior and battery draw during driving, traffic, weather, and dense town views.
- Load time, background/resume, and low-storage behavior.

Profile the iPhone 16e and the lowest supported iPhone/iPad before choosing final quality tiers. A 60 FPS target is an aim, not a guarantee. A stable 30 FPS fallback is preferable to unstable frame pacing.

## Official Unity references

- [URP performance guidance for Unity 6.3](https://docs.unity.com/en-us/engine/6000.3/manual/analysis/graphics-performance-profiling/in-urp/understand-performance/understand-performance)
- [Configure URP for better performance](https://docs.unity.com/en-us/engine/6000.5/manual/render-pipelines/universal-render-pipeline/introduction/configure-for-better-performance)
- [URP asset settings reference](https://docs.unity.com/en-us/engine/6000.3/manual/render-pipelines/universal-render-pipeline/urp-reference/urp-reference)

## Verification status

All work in this document is source/configuration planning. Unity import, compilation, generated URP asset inspection, Play Mode, device profiling, and iOS build validation remain outstanding.
