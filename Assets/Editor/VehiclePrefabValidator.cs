using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SheriffAmericanLife.Editor
{
    /// <summary>
    /// Editor-only quality gate for imported vehicle visual prefabs.
    /// This validates asset structure; it does not certify mesh quality, licensing, or runtime performance.
    /// </summary>
    public static class VehiclePrefabValidator
    {
        private const string MenuRoot = "Sheriff American Life/Art Validation/";

        [MenuItem(MenuRoot + "Validate All Vehicle Catalogs")]
        public static void ValidateAllCatalogs()
        {
            string[] guids = AssetDatabase.FindAssets("t:VehicleModelCatalog");
            int checkedEntries = 0;
            int errors = 0;
            int warnings = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                VehicleModelCatalog catalog = AssetDatabase.LoadAssetAtPath<VehicleModelCatalog>(path);
                if (catalog == null || catalog.vehicles == null) continue;

                for (int i = 0; i < catalog.vehicles.Length; i++)
                {
                    VehicleModelEntry entry = catalog.vehicles[i];
                    if (entry == null) continue;
                    checkedEntries++;
                    string label = entry.DisplayName;
                    if (entry.visualPrefab == null)
                    {
                        warnings++;
                        Debug.LogWarning("[Vehicle Art] " + label + " has no visual prefab; procedural fallback remains active. Catalog: " + path, catalog);
                        continue;
                    }

                    ValidatePrefab(entry.visualPrefab, label, catalog, ref errors, ref warnings);
                }
            }

            Debug.Log("[Vehicle Art] Validation finished. Catalog entries checked: " + checkedEntries +
                      "; errors: " + errors + "; warnings: " + warnings +
                      ". Missing prefabs are warnings because the current catalog intentionally supports procedural fallbacks.");
        }

        [MenuItem(MenuRoot + "Validate Selected Vehicle Prefab", true)]
        private static bool CanValidateSelectedPrefab()
        {
            return Selection.activeObject is GameObject;
        }

        [MenuItem(MenuRoot + "Validate Selected Vehicle Prefab")]
        public static void ValidateSelectedPrefab()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected == null) return;
            int errors = 0;
            int warnings = 0;
            ValidatePrefab(selected, selected.name, null, ref errors, ref warnings);
            Debug.Log("[Vehicle Art] Selected prefab validation finished. Errors: " + errors + "; warnings: " + warnings + ".", selected);
        }

        private static void ValidatePrefab(GameObject prefab, string label, Object context, ref int errors, ref int warnings)
        {
            if (prefab == null)
            {
                errors++;
                Debug.LogError("[Vehicle Art] " + label + " references a missing prefab.", context);
                return;
            }

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                errors++;
                Debug.LogError("[Vehicle Art] " + label + " has no Renderer components.", prefab);
                return;
            }

            int materialSlots = 0;
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;
                Material[] materials = renderer.sharedMaterials;
                materialSlots += materials.Length;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == null)
                    {
                        errors++;
                        Debug.LogError("[Vehicle Art] " + label + " has a missing material on " + renderer.name + ".", renderer);
                    }
                }

                MeshFilter filter = renderer.GetComponent<MeshFilter>();
                SkinnedMeshRenderer skinned = renderer as SkinnedMeshRenderer;
                if (filter != null && filter.sharedMesh == null)
                {
                    errors++;
                    Debug.LogError("[Vehicle Art] " + label + " has a MeshRenderer without a mesh on " + renderer.name + ".", renderer);
                }
                if (skinned != null && skinned.sharedMesh == null)
                {
                    errors++;
                    Debug.LogError("[Vehicle Art] " + label + " has a SkinnedMeshRenderer without a mesh.", renderer);
                }
            }

            LODGroup lodGroup = prefab.GetComponentInChildren<LODGroup>(true);
            if (lodGroup == null)
            {
                warnings++;
                Debug.LogWarning("[Vehicle Art] " + label + " has no LODGroup. Add authored LODs before using it in dense traffic.", prefab);
            }

            if (renderers.Length > 80)
            {
                warnings++;
                Debug.LogWarning("[Vehicle Art] " + label + " has " + renderers.Length + " renderers; merge static parts or reduce submeshes where practical.", prefab);
            }
            if (materialSlots > 16)
            {
                warnings++;
                Debug.LogWarning("[Vehicle Art] " + label + " uses " + materialSlots + " material slots; review draw-call and memory cost.", prefab);
            }

            Vector3 scale = prefab.transform.localScale;
            if (Mathf.Abs(scale.x - 1f) > 0.01f || Mathf.Abs(scale.y - 1f) > 0.01f || Mathf.Abs(scale.z - 1f) > 0.01f)
            {
                warnings++;
                Debug.LogWarning("[Vehicle Art] " + label + " root scale is not (1,1,1). Normalize import scale or document the intended scale.", prefab);
            }

            HashSet<string> names = new HashSet<string>();
            Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
            foreach (Transform item in transforms) names.Add(item.name);

            string[] recommendedAnchors = {
                "Wheel_FL", "Wheel_FR", "Wheel_RL", "Wheel_RR",
                "Headlight_L", "Headlight_R", "Taillight_L", "Taillight_R",
                "DriverSeat", "PassengerSeat", "InteriorRoot"
            };
            List<string> missing = new List<string>();
            foreach (string anchor in recommendedAnchors)
                if (!names.Contains(anchor)) missing.Add(anchor);

            if (missing.Count > 0)
            {
                warnings++;
                Debug.LogWarning("[Vehicle Art] " + label + " is missing recommended named anchors: " +
                                 string.Join(", ", missing.ToArray()) +
                                 ". Add only the anchors supported by the model; this check does not block import.", prefab);
            }

            Debug.Log("[Vehicle Art] " + label + " inspected: " + renderers.Length +
                      " renderers, " + materialSlots + " material slots, LODGroup=" +
                      (lodGroup != null ? "yes" : "no") + ".", prefab);
        }
    }
}