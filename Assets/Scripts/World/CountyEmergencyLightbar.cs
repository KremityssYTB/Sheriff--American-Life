using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Lightweight renderer-only emergency lightbar. Alternates red and blue
    /// emissive blocks without creating realtime Light components.
    /// </summary>
    public sealed class CountyEmergencyLightbar : MonoBehaviour
    {
        [SerializeField] private Renderer redRenderer;
        [SerializeField] private Renderer blueRenderer;
        [SerializeField, Min(0.5f)] private float flashCyclesPerSecond = 2.5f;

        public void Configure(Renderer red, Renderer blue, float frequency = 2.5f)
        {
            redRenderer = red;
            blueRenderer = blue;
            flashCyclesPerSecond = Mathf.Max(0.5f, frequency);
            ApplyState(false);
        }

        private void Update()
        {
            bool redActive = Mathf.Sin(Time.time * flashCyclesPerSecond * Mathf.PI * 2f) >= 0f;
            ApplyState(redActive);
        }

        private void ApplyState(bool redActive)
        {
            if (redRenderer != null) redRenderer.enabled = redActive;
            if (blueRenderer != null) blueRenderer.enabled = !redActive;
        }

        private void OnDisable()
        {
            if (redRenderer != null) redRenderer.enabled = true;
            if (blueRenderer != null) blueRenderer.enabled = true;
        }
    }
}