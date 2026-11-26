using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Creates an isolated four-way-stop traffic test junction. It does not alter
    /// the existing signalized Ocala intersection.
    /// </summary>
    public sealed class CountyFourWayStopTestSpawner : MonoBehaviour
    {
        [SerializeField] private float roadLength = 150f;
        [SerializeField] private float roadWidth = 14f;
        [SerializeField] private float vehicleSpeed = 5.5f;
        private Transform root;
        private WorldIncidentReporter reporter;
        private CountyFourWayStopController controller;

        public void Configure(Transform worldRoot, WorldIncidentReporter incidentReporter)
        {
            root = worldRoot;
            reporter = incidentReporter;
            if (root == null) return;
            BuildJunction();
        }

        private void BuildJunction()
        {
            Vector3 center = new Vector3(0f, 0f, 260f);
            Transform junctionRoot = new GameObject("Four_Way_Stop_Test_Junction").transform;
            junctionRoot.SetParent(root, false);
            junctionRoot.localPosition = center;

            Make(junctionRoot, "North South Road", PrimitiveType.Cube,
                new Vector3(0f, -.04f, 0f), new Vector3(roadWidth, .12f, roadLength),
                new Color(.14f, .14f, .14f));
            Make(junctionRoot, "East West Road", PrimitiveType.Cube,
                new Vector3(0f, -.02f, 0f), new Vector3(roadLength, .12f, roadWidth),
                new Color(.14f, .14f, .14f));

            controller = junctionRoot.gameObject.AddComponent<CountyFourWayStopController>();
            AddStopApproach(junctionRoot, center, Vector3.forward, Vector3.right,
                CountyIntersectionRules.Approach.North, "North");
            AddStopApproach(junctionRoot, center, Vector3.left, Vector3.back,
                CountyIntersectionRules.Approach.East, "East");
            AddStopApproach(junctionRoot, center, Vector3.back, Vector3.left,
                CountyIntersectionRules.Approach.South, "South");
            AddStopApproach(junctionRoot, center, Vector3.right, Vector3.forward,
                CountyIntersectionRules.Approach.West, "West");
        }

        private void AddStopApproach(Transform parent, Vector3 center, Vector3 forward,
            Vector3 side, CountyIntersectionRules.Approach approach, string label)
        {
            Vector3 stop = center - forward * 10f;
            Vector3 start = center - forward * 62f;
            Vector3 exit = center + forward * 25f;
            Vector3 loopA = exit + side * 48f;
            Vector3 loopB = start + side * 48f;

            Make(parent, label + " Stop Bar", PrimitiveType.Cube,
                stop - center, new Vector3(roadWidth * .72f, .025f, .45f),
                Color.white, (approach == CountyIntersectionRules.Approach.East || approach == CountyIntersectionRules.Approach.West)
                    ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity);
            CreateStopSign(parent, label + " STOP Sign", stop - center - forward * 5f,
                Quaternion.LookRotation(-forward, Vector3.up));

            Transform[] route =
            {
                Waypoint(parent, label + " Stop Line", stop - center),
                Waypoint(parent, label + " Intersection Center", Vector3.zero),
                Waypoint(parent, label + " Clear Conflict Area", exit - center),
                Waypoint(parent, label + " Return Outer A", loopA - center),
                Waypoint(parent, label + " Return Outer B", loopB - center),
                Waypoint(parent, label + " Return Start", start - center)
            };

            GameObject car = Make(parent, "Stop Test Sedan " + label, PrimitiveType.Cube,
                start - center + Vector3.up * .8f, new Vector3(4.2f, 1.25f, 1.9f),
                new Color(.28f, .38f, .52f), Quaternion.LookRotation(forward, Vector3.up));
            car.AddComponent<Rigidbody>().mass = 1150f;
            CountyTrafficVehicleAgent agent = car.AddComponent<CountyTrafficVehicleAgent>();
            agent.Configure(route, reporter, vehicleSpeed);
            agent.ConfigureFourWayStop(controller, approach,
                CountyIntersectionRules.Movement.Straight, stop, exit);
        }

        private void CreateStopSign(Transform parent, string signName, Vector3 localPosition,
            Quaternion rotation)
        {
            GameObject pole = Make(parent, signName + " Pole", PrimitiveType.Cylinder,
                localPosition + Vector3.up * 1.5f, new Vector3(.12f, 1.5f, .12f),
                new Color(.55f, .55f, .55f));
            GameObject plate = Make(parent, signName + " Plate", PrimitiveType.Cube,
                localPosition + Vector3.up * 2.7f, new Vector3(1.25f, 1.05f, .12f),
                new Color(.72f, .08f, .08f), rotation);
            TextMesh text = new GameObject(signName + " Text").AddComponent<TextMesh>();
            text.text = "STOP";
            text.fontSize = 64;
            text.characterSize = .16f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = Color.white;
            text.transform.SetParent(plate.transform, false);
            text.transform.localPosition = new Vector3(0f, 0f, .08f);
            text.transform.localRotation = Quaternion.identity;
        }

        private Transform Waypoint(Transform parent, string waypointName, Vector3 localPosition)
        {
            GameObject waypoint = new GameObject(waypointName);
            waypoint.transform.SetParent(parent, false);
            waypoint.transform.localPosition = localPosition;
            return waypoint.transform;
        }

        private GameObject Make(Transform parent, string objectName, PrimitiveType type,
            Vector3 localPosition, Vector3 scale, Color color, Quaternion rotation = default)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = objectName;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = scale;
            if (rotation != default) go.transform.localRotation = rotation;
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                Material material = new Material(shader);
                material.color = color;
                renderer.sharedMaterial = material;
            }
            return go;
        }
    }
}