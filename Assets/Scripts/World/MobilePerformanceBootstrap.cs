using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SheriffAmericanLife
{
    public enum MobileFrameRateMode
    {
        Fps30 = 30,
        Fps60 = 60,
        Fps120 = 120,
        Unlimited = 0
    }

    public enum MobileVisualQualityPreset
    {
        Performance = 0,
        Balanced = 1,
        High = 2
    }

    /// <summary>
    /// Applies one of the game's mobile visual presets. Defaults are provisional
    /// and must be tuned using Unity Profiler captures on supported devices.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class MobilePerformanceBootstrap : MonoBehaviour
    {
        private const string QualityPreferenceKey = "Sheriff.MobileVisualQuality";
        private const string FrameRatePreferenceKey = "Sheriff.MobileFrameRate";

        [SerializeField] private MobileVisualQualityPreset defaultPreset = MobileVisualQualityPreset.Balanced;
        [SerializeField] private MobileFrameRateMode defaultFrameRateMode = MobileFrameRateMode.Fps60;
        [SerializeField, Min(64f)] private float streamingMemoryBudgetMb = 256f;

        public MobileVisualQualityPreset CurrentPreset { get; private set; }
        public MobileFrameRateMode CurrentFrameRateMode { get; private set; }
        public float MeasuredFramesPerSecond { get; private set; }
        public float MeasuredFrameTimeMilliseconds { get; private set; }

        private float smoothedFrameDelta;
        private const float FrameTimeSmoothing = 0.12f;

        private void Update()
        {
            float frameDelta = Time.unscaledDeltaTime;
            if (frameDelta <= 0f) return;

            smoothedFrameDelta = smoothedFrameDelta <= 0f
                ? frameDelta
                : Mathf.Lerp(smoothedFrameDelta, frameDelta, FrameTimeSmoothing);

            MeasuredFramesPerSecond = 1f / smoothedFrameDelta;
            MeasuredFrameTimeMilliseconds = smoothedFrameDelta * 1000f;
        }

        private void Awake()
        {
            Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;
            QualitySettings.vSyncCount = 0;
            QualitySettings.streamingMipmapsActive = true;
            QualitySettings.streamingMipmapsMemoryBudget = streamingMemoryBudgetMb;

            int saved = PlayerPrefs.GetInt(QualityPreferenceKey, (int)defaultPreset);
            if (!System.Enum.IsDefined(typeof(MobileVisualQualityPreset), saved))
                saved = (int)defaultPreset;

            ApplyPreset((MobileVisualQualityPreset)saved, false);

            int savedFrameRate = PlayerPrefs.GetInt(FrameRatePreferenceKey, (int)defaultFrameRateMode);
            if (!System.Enum.IsDefined(typeof(MobileFrameRateMode), savedFrameRate))
                savedFrameRate = (int)defaultFrameRateMode;
            SetFrameRateMode((MobileFrameRateMode)savedFrameRate, false);
        }

        /// <summary>
        /// Called by the runtime Settings panel. Saves the user's choice when requested.
        /// </summary>
        public void SetPreset(MobileVisualQualityPreset preset)
        {
            ApplyPreset(preset, true);
        }

        public void SetFrameRateMode(MobileFrameRateMode mode)
        {
            SetFrameRateMode(mode, true);
        }

        private void SetFrameRateMode(MobileFrameRateMode mode, bool save)
        {
            CurrentFrameRateMode = mode;
            Application.targetFrameRate = mode == MobileFrameRateMode.Unlimited ? -1 : (int)mode;
            if (save)
            {
                PlayerPrefs.SetInt(FrameRatePreferenceKey, (int)mode);
                PlayerPrefs.Save();
            }
        }

        private void ApplyPreset(MobileVisualQualityPreset preset, bool save)
        {
            CurrentPreset = preset;

            float lodBias;
            float shadowDistance;
            int shadowCascades;
            int msaaSamples;
            float renderScale;
            bool hdr;

            switch (preset)
            {
                case MobileVisualQualityPreset.Performance:
                    lodBias = 0.7f;
                    shadowDistance = 18f;
                    shadowCascades = 1;
                    msaaSamples = 2;
                    renderScale = 0.85f;
                    hdr = false;
                    break;

                case MobileVisualQualityPreset.High:
                    lodBias = 1.25f;
                    shadowDistance = 50f;
                    shadowCascades = 2;
                    msaaSamples = 4;
                    renderScale = 1f;
                    hdr = true;
                    break;

                default:
                    CurrentPreset = MobileVisualQualityPreset.Balanced;
                    lodBias = 1f;
                    shadowDistance = 35f;
                    shadowCascades = 2;
                    msaaSamples = 2;
                    renderScale = 1f;
                    hdr = true;
                    break;
            }

            QualitySettings.lodBias = lodBias;
            QualitySettings.shadowDistance = shadowDistance;
            QualitySettings.shadowCascades = shadowCascades;

            UniversalRenderPipelineAsset urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp != null)
            {
                urp.shadowDistance = shadowDistance;
                urp.shadowCascadeCount = shadowCascades;
                urp.msaaSampleCount = msaaSamples;
                urp.renderScale = renderScale;
                urp.supportsHDR = hdr;
            }

            if (save)
            {
                PlayerPrefs.SetInt(QualityPreferenceKey, (int)CurrentPreset);
                PlayerPrefs.Save();
            }
        }
    }
}