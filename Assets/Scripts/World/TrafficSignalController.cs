using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>Two-phase signal controller for the prototype four-way intersection.</summary>
    public sealed class TrafficSignalController : MonoBehaviour
    {
        public enum Phase { EastWestGreen, EastWestAmber, AllRed, NorthSouthGreen, NorthSouthAmber }

        [SerializeField] private float greenSeconds = 16f;
        [SerializeField] private float amberSeconds = 3f;
        [SerializeField] private float allRedSeconds = 1.5f;

        private float phaseTimer;
        private Phase phase;
        private Renderer[] eastWestLamps;
        private Renderer[] northSouthLamps;
        private bool nextGreenEastWest;

        public Phase CurrentPhase => phase;
        public bool CanProceed(bool eastWest) =>
            eastWest ? phase == Phase.EastWestGreen : phase == Phase.NorthSouthGreen;

        public void Configure()
        {
            phase = Phase.EastWestGreen;
            phaseTimer = greenSeconds;
            CreateSignalHeads();
            RefreshLights();
        }

        private void Update()
        {
            phaseTimer -= Time.deltaTime;
            if (phaseTimer > 0f) return;

            switch (phase)
            {
                case Phase.EastWestGreen:
                    phase = Phase.EastWestAmber; phaseTimer = amberSeconds; break;
                case Phase.EastWestAmber:
                    phase = Phase.AllRed; nextGreenEastWest = false; phaseTimer = allRedSeconds; break;
                case Phase.NorthSouthGreen:
                    phase = Phase.NorthSouthAmber; phaseTimer = amberSeconds; break;
                case Phase.NorthSouthAmber:
                    phase = Phase.AllRed; nextGreenEastWest = true; phaseTimer = allRedSeconds; break;
                default:
                    phase = nextGreenEastWest ? Phase.EastWestGreen : Phase.NorthSouthGreen;
                    phaseTimer = greenSeconds;
                    break;
            }
            RefreshLights();
        }

        private void CreateSignalHeads()
        {
            eastWestLamps = new Renderer[3];
            northSouthLamps = new Renderer[3];
            CreateHead("Signal East-West", new Vector3(-9,0,-89), eastWestLamps);
            CreateHead("Signal North-South", new Vector3(9,0,-71), northSouthLamps);
        }

        private void CreateHead(string label, Vector3 position, Renderer[] lamps)
        {
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = label + " Pole";
            pole.transform.SetParent(transform, false);
            pole.transform.localPosition = position + Vector3.up * 3f;
            pole.transform.localScale = new Vector3(.18f,3f,.18f);

            for (int i=0;i<3;i++)
            {
                GameObject bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bulb.name = label + (i==0 ? " Red" : i==1 ? " Amber" : " Green");
                bulb.transform.SetParent(transform, false);
                bulb.transform.localPosition = position + Vector3.up * (5.5f - i*.65f);
                bulb.transform.localScale = Vector3.one * .42f;
                Renderer renderer = bulb.GetComponent<Renderer>();
                Material material = new Material(Shader.Find("Universal Render Pipeline/Lit") ??
                    Shader.Find("Standard"));
                material.color = new Color(.12f,.12f,.12f);
                renderer.sharedMaterial = material;
                lamps[i] = renderer;
            }
        }

        private void RefreshLights()
        {
            SetLampState(eastWestLamps, phase == Phase.EastWestGreen, phase == Phase.EastWestAmber);
            SetLampState(northSouthLamps, phase == Phase.NorthSouthGreen, phase == Phase.NorthSouthAmber);
        }

        private static void SetLampState(Renderer[] lamps, bool green, bool amber)
        {
            if (lamps == null) return;
            Color[] colors = { green || amber ? new Color(.12f,.12f,.12f) : Color.red,
                amber ? new Color(1f,.55f,.05f) : new Color(.12f,.12f,.12f),
                green ? new Color(.1f,1f,.2f) : new Color(.12f,.12f,.12f) };
            for (int i=0;i<lamps.Length;i++)
            {
                if (lamps[i] == null) continue;
                lamps[i].material.color = colors[i];
            }
        }
    }
}