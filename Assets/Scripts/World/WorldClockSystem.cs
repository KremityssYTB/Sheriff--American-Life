using UnityEngine;

namespace SheriffAmericanLife
{
    public enum FloridaWeather { Clear, Cloudy, Rain, Fog }

    /// <summary>
    /// Lightweight world clock and weather controller for the mobile prototype.
    /// Attach to one scene object and assign the directional sun.
    /// </summary>
    public sealed class WorldClockSystem : MonoBehaviour
    {
        [Header("Time")]
        [Range(0f, 24f)] [SerializeField] private float startHour = 8f;
        [Min(1f)] [SerializeField] private float minutesPerGameDay = 24f;
        [SerializeField] private bool advanceTime = true;
        [SerializeField] private Light sun;
        [SerializeField] private Gradient sunlight = new Gradient();
        [SerializeField] private AnimationCurve sunIntensity = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Weather")]
        [SerializeField] private FloridaWeather weather = FloridaWeather.Clear;
        [SerializeField] private Color clearAmbient = new Color(.62f, .65f, .7f);
        [SerializeField] private Color cloudyAmbient = new Color(.42f, .45f, .5f);
        [SerializeField] private Color fogAmbient = new Color(.5f, .53f, .56f);
        [SerializeField] private Color rainAmbient = new Color(.32f, .36f, .42f);

        public float CurrentHour { get; private set; }
        public FloridaWeather CurrentWeather => weather;
        public string TimeLabel => FormatTime(CurrentHour);

        private void Awake()
        {
            CurrentHour = startHour;
            ApplyWeather();
            UpdateLighting();
        }

        private void Update()
        {
            if (advanceTime)
            {
                CurrentHour += (24f / (minutesPerGameDay * 60f)) * Time.deltaTime;
                if (CurrentHour >= 24f) CurrentHour -= 24f;
            }
            UpdateLighting();
        }

        public void SetSun(Light directionalSun)
        {
            sun = directionalSun;
            UpdateLighting();
        }

        public void SetTime(float hour)
        {
            CurrentHour = Mathf.Repeat(hour, 24f);
            UpdateLighting();
        }

        public void SetWeather(FloridaWeather nextWeather)
        {
            weather = nextWeather;
            ApplyWeather();
        }

        private void UpdateLighting()
        {
            float normalized = CurrentHour / 24f;
            float sunAngle = normalized * 360f - 90f;
            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Euler(sunAngle,  -32f, 0f);
                float daylight = Mathf.Clamp01(Mathf.Sin(normalized * Mathf.PI * 2f - Mathf.PI * .5f) * .5f + .5f);
                sun.intensity = sunIntensity.Evaluate(daylight) * (weather == FloridaWeather.Cloudy ? .55f : weather == FloridaWeather.Rain ? .38f : 1f);
                sun.color = sunlight.Evaluate(daylight);
                sun.shadows = LightShadows.Soft;
                sun.shadowStrength = .82f;
                sun.shadowBias = .045f;
                sun.shadowNormalBias = .35f;
                RenderSettings.sun = sun;
            }
        }

        private void ApplyWeather()
        {
            RenderSettings.ambientLight = weather switch
            {
                FloridaWeather.Clear => clearAmbient,
                FloridaWeather.Cloudy => cloudyAmbient,
                FloridaWeather.Rain => rainAmbient,
                FloridaWeather.Fog => fogAmbient,
                _ => clearAmbient
            };
            RenderSettings.fog = weather == FloridaWeather.Fog || weather == FloridaWeather.Rain;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = weather == FloridaWeather.Fog
                ? new Color(.62f,.66f,.69f)
                : weather == FloridaWeather.Rain
                    ? new Color(.38f,.43f,.49f)
                    : new Color(.68f,.73f,.78f);
            // Keep weather haze subtle enough to preserve visibility on a small screen.
            RenderSettings.fogDensity = weather == FloridaWeather.Fog ? .0035f : weather == FloridaWeather.Rain ? .0012f : 0f;
        }

        private static string FormatTime(float hour)
        {
            int totalMinutes = Mathf.FloorToInt(hour * 60f);
            int h = (totalMinutes / 60) % 24;
            int m = totalMinutes % 60;
            string suffix = h < 12 ? "AM" : "PM";
            int displayHour = h % 12;
            if (displayHour == 0) displayHour = 12;
            return displayHour.ToString("00") + ":" + m.ToString("00") + " " + suffix;
        }
    }
}