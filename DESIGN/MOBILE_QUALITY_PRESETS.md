# Mobile Visual Quality Presets

SHERIFF uses one scene and a selectable runtime quality preset rather than separate gameplay builds. The presets are starting points only; final values must be chosen from device profiling.

## Presets

| Setting | Performance | Balanced (default) | High |
|---|---:|---:|---:|
| Frame rate | Independently selected in Settings: 30 / 60 / 120 / Unlimited | Independently selected in Settings: 30 / 60 / 120 / Unlimited | Independently selected in Settings: 30 / 60 / 120 / Unlimited |
| LOD bias | 0.7 | 1.0 | 1.25 |
| Shadow distance | 18 m | 35 m | 50 m |
| Shadow cascades | 1 | 2 | 2 |
| MSAA | 2x | 2x | 4x |
| Render scale | 0.85x | 1.0x | 1.0x |
| HDR | Off | On | On |

These values are not promises of achieved frame rate, battery life, or visual quality. The Performance preset intentionally lowers resolution and shadow cost; the High preset can increase GPU and thermal load.

## Runtime integration

`MobilePerformanceBootstrap` applies the saved preset at startup. The default is Balanced. The current prototype HUD exposes a Frame Rate panel with 30 FPS, 60 FPS, 120 FPS, and Unlimited. The saved frame-rate mode is independent of the visual-quality preset. Code can call:

- `SetPreset(MobileVisualQualityPreset.Performance)`
- `SetPreset(MobileVisualQualityPreset.Balanced)`
- `SetPreset(MobileVisualQualityPreset.High)`

The quality choice and frame-rate choice are stored separately in PlayerPrefs. The default visual preset is Balanced; the default frame-rate mode is 120 FPS. The current Settings panel is an immediate-mode prototype and needs replacement with production mobile UI.

## Validation gate

Before beta:
1. Compile in the target Unity 6.3.25f1 editor and inspect URP property support.
2. Capture CPU, GPU, memory, frame pacing, and thermal behavior on supported iPhones and iPads.
3. Confirm MSAA/render-scale changes apply correctly on the active Metal renderer.
4. Test scene readability, shadows, vehicle materials, UI legibility, and day/night lighting at each preset.
5. Adjust or remove any preset that fails stability or sustained-performance targets.

Do not select a device preset based only on screen resolution or model-name heuristics. Profile real workloads first.
