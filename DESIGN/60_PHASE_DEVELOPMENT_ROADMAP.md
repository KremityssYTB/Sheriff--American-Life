# SHERIFF: AMERICAN LIFE — 60-Phase Development Roadmap

This is a planning roadmap, not a claim that planned systems are implemented. Phases 1–10 were completed as source-level work and audits. Phases 11–60 are planned sequential milestones. Each phase should be implemented in small, reviewable commits, with Unity compilation and runtime/device verification reported separately.

## Data integrity and gameplay foundations
11. Persist incident reports locally with versioned JSON and stable report numbering.
12. Persist witness statements and reconcile statement IDs after reload.
13. Persist dispatch calls and validate call lifecycle restoration.
14. Add shared local-save infrastructure, schema migration policy, and corruption recovery.
15. Add cross-ledger case references, a read-only case index, and explicit citation/booking linking.
16. Add case-level grouping and case search in the MDT.
17. Add save-slot/profile metadata and a clean new-game flow.
18. Add save diagnostics and user-facing recovery messaging.
19. Add bounded retention, export-safe fictional record snapshots, and storage limits.
20. Audit all gameplay records for restart consistency and duplicate identifiers.

## Deputy career and duty loop
21. Connect duty state to dispatch availability and duty-only interactions.
22. Add department selection foundations, beginning with Marion County Sheriff.
23. Add rank, XP, and career progression data models.
24. Add shift start/end, patrol assignment, and shift summary.
25. Add equipment inventory and duty-belt loadout data.
26. Add uniform selection and role-appropriate appearance slots.
27. Add radio channel and basic radio event model.
28. Add backup request and responding-unit state.
29. Add patrol zones and beat assignment.
30. Complete one repeatable deputy shift loop with persisted career results.

## Civilian life and world interactions
31. Add interaction points for one usable convenience store.
32. Add purchase/refuel transaction model using fictional in-game currency.
33. Add one enterable residential interior and home interaction loop.
34. Add vehicle ownership and personal-vehicle selection foundations.
35. Add civilian schedule profiles and time-of-day routine transitions.
36. Add pedestrian crossing and sidewalk navigation improvements.
37. Add ambient service/business NPC roles.
38. Add public location interaction prompts and accessibility options.
39. Add world event scheduling and incident cooldown controls.
40. Audit off-duty free roam so law-enforcement UI and actions are correctly gated.

## Vehicles, traffic, and response
41. Add safe vehicle enter/exit and obstruction regression cases.
42. Improve vehicle collision response and recovery behavior.
43. Add lane-change and obstacle-avoidance planning for traffic agents.
44. Add traffic signal phase controller and intersection conflict checks.
45. Add emergency vehicle pull-over behavior for civilian traffic.
46. Add pursuit state machine and safe termination conditions.
47. Add vehicle condition, fuel, and service interactions.
48. Add vehicle roster data validation and prefab fallback rules.
49. Add mobile vehicle camera and control calibration options.
50. Audit traffic density, physics cost, and mobile performance budgets.

## World, presentation, and release readiness
51. Improve Ocala district layout with connected road and block data.
52. Add county map coordinates and in-game waypoint foundations.
53. Add route guidance prototype using authored road graph data.
54. Add weather transitions and wet-road response hooks.
55. Add audio mix groups, radio levels, and accessibility controls.
56. Add scalable visual quality tiers and runtime diagnostics.
57. Add localization-ready UI text catalog and input accessibility pass.
58. Add automated source validation and build preflight checks.
59. Complete the first end-to-end Ocala deputy vertical-slice acceptance review.
60. Run a release-readiness review and define the next county expansion gate.

## Phase completion rules

- A phase is complete only when its code/documentation changes are committed and its stated checks are recorded.
- Source inspection is not Unity compilation. Unity compilation is not Play Mode validation. Neither is an iPhone/iPad device test.
- Do not claim multiplayer, production assets, full county coverage, or a signed installable app unless those capabilities are actually implemented and verified.
- Preserve the iPhone-only and zero-budget workflow; avoid paid cloud desktop dependencies.


## Phase 15 — cross-system case references (2026-11-28)

- Added a shared `caseId` field to incident reports, witness statements, dispatch calls, traffic citations, and arrest bookings. Legacy report `relatedIncidentId`, witness `incidentId`, and dispatch `sourceIncidentId` values are used as fallbacks when loading older records.
- Added `CaseFileIndex`, a read-only lookup that collects all five record types for a case ID. It is attached to the runtime world root by `CountyPlayerRuntimeController` and discovers the existing ledgers on that object.
- Dispatch-generated reports inherit the call's case ID. Arrest reports and jail-intake reports inherit the booking case ID. Citation-generated follow-up reports inherit the citation case ID. Citations and bookings receive stable default case IDs and expose explicit `LinkToCase` methods for joining an existing case.
- This is a local prototype index, not a real records-management system. It does not merge unrelated cases, infer identity, provide legal authority, or synchronize across devices. Witness/dispatch records with an existing world incident ID naturally group under that ID; separately created arrest/citation cases remain separate until explicitly linked.
- Static source checks only. Unity compilation, runtime component wiring, lookup behavior, save compatibility, and device tests remain outstanding.
