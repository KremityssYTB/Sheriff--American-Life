# Vehicle Visual Asset Standard

This document defines the import contract for original or properly licensed vehicle assets in SHERIFF: AMERICAN LIFE. It is a production checklist, not a claim that any vehicle model is currently integrated.

## Asset source and rights

- Use original work, commissioned work, or assets whose license explicitly permits use in a commercial game and redistribution inside a shipped build.
- Record the source URL, creator, license name/version, attribution requirements, purchase receipt or permission record, and any restrictions in the asset's entry in the art inventory.
- Do not rip models, textures, sounds, logos, or liveries from commercial games, manufacturer configurators, or other proprietary sources.
- A vehicle's make/model/year metadata does not grant rights to its trademarked logos, design files, or 3D model.

## Prefab contract

- Import the model with a consistent forward axis (+Z) and up axis (+Y); apply import rotation and scale so the prefab root is at local position zero with local rotation identity and local scale (1,1,1).
- Keep the visual prefab separate from the gameplay vehicle root. Do not embed Rigidbody, WheelCollider, or gameplay scripts in the art prefab.
- Use a single clear root and a named body/visual hierarchy. Keep glass, lights, tires, trim, and paint in a small, intentional set of materials.
- Use shared URP/Lit-compatible materials where possible. Avoid per-vehicle duplicated materials when a MaterialPropertyBlock can carry paint color.
- Provide four wheel pivots named `Wheel_FL`, `Wheel_FR`, `Wheel_RL`, and `Wheel_RR`; wheel meshes should be children of their pivots. Keep wheel pivots aligned with the axle and rotation axis.
- Where present, provide `Headlight_L`, `Headlight_R`, `Taillight_L`, `Taillight_R`, `DriverSeat`, `PassengerSeat`, and `InteriorRoot` anchors. Missing optional anchors should be documented rather than faked.
- Include a simple collision proxy on the gameplay root, not a dense mesh collider on every art part.
- Author LOD0/LOD1/LOD2 in an LODGroup. Remove hidden interior and underbody detail from distant LODs. Set sensible screen-relative transition heights and test them on-device.
- Use baked/packed texture maps where practical. Keep texture dimensions, compression, mipmaps, and alpha usage appropriate for mobile. Enable mip streaming only for textures authored to support it.
- Keep a low material-slot count and avoid excessive transparent surfaces, real-time lights, and shadow-casting small parts.

## Mobile acceptance targets

These are review budgets, not hard engine guarantees. Profile actual assets in the full scene before approving them.

| Item | Initial review target |
|---|---:|
| Renderers per vehicle prefab | 80 or fewer |
| Material slots per vehicle | 16 or fewer |
| LODs | 3 preferred |
| Real-time light components on civilian vehicles | 0 |
| Gameplay colliders inside visual prefab | 0 |
| Root transform | position 0, rotation identity, scale 1 |
| Vehicle forward/up axes | +Z / +Y |

## Validation

In Unity Editor, use **Sheriff American Life → Art Validation → Validate All Vehicle Catalogs** or select a vehicle prefab and run **Validate Selected Vehicle Prefab**.

The validator checks renderer presence, missing meshes/materials, LODGroup presence, root scale, renderer/material-slot counts, and recommended hierarchy anchors. It reports missing prefabs as warnings because the current procedural fallback is intentional. Passing the validator does not prove licensing, correct dimensions, good topology, UV quality, visual fidelity, correct wheel placement, or acceptable GPU/memory cost.

## Integration sequence

1. Add a licensed source asset and record its provenance.
2. Normalize axes, scale, hierarchy, materials, and texture import settings.
3. Add LODs and the optional anchors supported by the asset.
4. Run the editor validator and resolve errors.
5. Assign the prefab to the matching `VehicleModelEntry.visualPrefab`.
6. Test the model with traffic spawning, player enter/exit, camera views, lighting, shadows, and device profiling.
7. Keep the procedural fallback available until the production prefab passes the complete test.
