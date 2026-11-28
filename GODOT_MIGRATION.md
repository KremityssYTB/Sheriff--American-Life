# Engine migration decision — 2026-11-28

## Source preservation
- Unity source is preserved unchanged on branch `main`.
- Godot 4 development starts on branch `godot4`.
- No Unity files were deleted or rewritten.
- To return to the Unity project, switch GitHub back to `main`.

## New target
- Engine: Godot 4.x
- Renderer: Mobile
- Initial device focus: iPhone 16e / A18, with broader compatible iPhone/iPad support as a release requirement.
- Initial performance target: stable 60 FPS in representative gameplay; 30 FPS fallback for demanding scenes; 120 FPS only where device, display mode, thermal conditions and scene budget permit.

## Migration approach
This is a clean parallel implementation, not an automatic Unity-to-Godot conversion. Existing Unity C# gameplay systems and assets remain available as reference material on `main`; gameplay systems must be reimplemented in GDScript or suitable Godot-native code. Reuse only assets whose licenses and formats permit it.

## First playable scope
The initial vertical slice is a compact Ocala-inspired district with a controllable deputy, touch controls, camera look, roads, sidewalks, placeholder civic buildings and outdoor environment. The complete Ocala/Marion County map, real vehicle models, real building assets, interiors, traffic simulation and full police gameplay are later milestones, not yet complete.
