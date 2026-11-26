using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Generates a lightweight two-tone siren clip at runtime; no external audio asset required.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class CountyEmergencySiren : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float volume = 0.22f;
        private AudioSource source;
        private AudioClip generatedClip;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 1f;
            source.volume = volume;
            source.dopplerLevel = 0f;
            generatedClip = CreateTwoToneClip();
            source.clip = generatedClip;
        }

        public void SetSirenEnabled(bool enabled)
        {
            if (source == null) return;
            if (enabled)
            {
                if (!source.isPlaying) source.Play();
            }
            else if (source.isPlaying)
            {
                source.Stop();
            }
        }

        private AudioClip CreateTwoToneClip()
        {
            const int sampleRate = 22050;
            const int durationSeconds = 2;
            int sampleCount = sampleRate * durationSeconds;
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)sampleRate;
                float phase = time % 1f;
                float frequency = phase < 0.5f ? 660f : 880f;
                float fade = Mathf.Min(1f, phase * 20f, (1f - phase) * 20f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * time) * 0.22f * fade;
            }

            AudioClip clip = AudioClip.Create("County Emergency Two Tone", sampleCount, 1,
                sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnDestroy()
        {
            if (source != null) source.Stop();
            if (generatedClip != null) Destroy(generatedClip);
        }
    }
}