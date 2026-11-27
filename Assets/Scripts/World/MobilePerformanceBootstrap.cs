using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Applies a conservative mobile baseline before the generated county scene
    /// becomes active. Final device tiers must be tuned from Unity Profiler data.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class MobilePerformanceBootstrap : MonoBehaviour
    {
        [Header("Frame pacing")]
        [SerializeField, Range(30, 120)] private int targetFrameRate = 60;

        [Header("Mobile quality baseline")]
        [SerializeField, Range(.5f, 2f)] private float lodBias = 1f;
        [SerializeField, Min(0f)] private float shadowDistance = 45f;
        [SerializeField, Range(0, 4)] private int shadowCascades = 2;

        [Header("Texture streaming")]
        [SerializeField, Min(64f)] private float streamingMemoryBudgetMb = 256f;

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
            Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;
            QualitySettings.vSyncCount = 0;

            QualitySettings.lodBias = lodBias;
            QualitySettings.shadowDistance = shadowDistance;
            QualitySettings.shadowCascades = shadowCascades;

            // This only benefits textures authored with mipmap streaming enabled.
            QualitySettings.streamingMipmapsActive = true;
            QualitySettings.streamingMipmapsMemoryBudget = streamingMemoryBudgetMb;
        }
    }
}