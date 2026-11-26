using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Builds an isolated four-way-stop junction with representative straight,
    /// left-turn and right-turn routes. The signalized Ocala junction is untouched.
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
            if (root != null) BuildJunction();
        }

        private void BuildJunction()
        {
            Vector3 center = new Vector3(0f, 0f, 260f);
            Transform junctionRoot = new GameObject("Four_Way_Stop_Test_Junction").transform;
            junctionRoot.SetParent(root, false);
            junctionRoot.localPosition = center;

            Make(junctionRoot, "North South Road", PrimitiveType.Cube,
                Vector3.zero + Vector3.down * .04f, new Vector3(roadWidth, .12f, roadLength),
                new Color(.14f, .14f, .14f));
            Make(junctionRoot, "East West Road", PrimitiveType.Cube,
                Vector3.down * .02f, new Vector3(roadLength, .12f, roadWidth),
                new Color(.14f, .14f, .14f));

            controller = junctionRoot.gameObject.AddComponent<CountyFourWayStopController>();
            AddStopApproach(junctionRoot, center, Vector3.back,
                CountyIntersectionRules.Approach.North, "North", CountyIntersectionRules.Movement.Straight);
            AddStopApproach(junctionRoot, center, Vector3.left,
                CountyIntersectionRules.Approach.East, "East", CountyIntersectionRules.Movement.Left);
            AddStopApproach(junctionRoot, center, Vector3.forward,
                CountyIntersectionRules.Approach.South, "South", CountyIntersectionRules.Movement.Right);
            AddStopApproach(junctionRoot, center, Vector3.right,
                CountyIntersectionRules.Approach.West, "West", CountyIntersectionRules.Movement.Straight);

            AddCrosswalkSensor(junctionRoot, center, Vector3.back, "North");
            AddCrosswalkSensor(junctionRoot, center, Vector3.left, "East");
            AddCrosswalkSensor(junctionRoot, center, Vector3.forward, "South");
            AddCrosswalkSensor(junctionRoot, center, Vector3.right, "West");
        }

        private void AddStopApproach(Transform parent, Vector3 center, Vector3 forward,
            CountyIntersectionRules.Approach approach, string label,
            CountyIntersectionRules.Movement movement)
        {
            // Derive the vehicle's right-hand side from its travel direction.
            Vector3 side = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 stop = center - forward * 10f;
            Vector3 start = center - forward * 62f;
            Vector3 turnDirection = movement == CountyIntersectionRules.Movement.Left ? -side : side;
            Vector3 exitDirection = movement == CountyIntersectionRules.Movement.Straight
                ? forward : turnDirection;
            Vector3 exit = movement == CountyIntersectionRules.Movement.Straight
                ? center + forward * 25f
                : center + forward * 10f + turnDirection * 25f;
            Vector3 curveA = movement == CountyIntersectionRules.Movement.Straight
                ? center + forward * 7f
                : center + forward * 8f + turnDirection * 3f;
            Vector3 curveB = movement == CountyIntersectionRules.Movement.Straight
                ? center + forward * 17f
                : center + forward * 10f + turnDirection * 13f;
            Vector3 loopA = exit + exitDirection * 42f + side * 35f;
            Vector3 loopB = start + side * 35f;

            Make(parent, label + " Stop Bar", PrimitiveType.Cube,
                stop - center, new Vector3(roadWidth * .72f, .025f, .45f),
                Color.white, (approach == CountyIntersectionRules.Approach.East ||
                    approach == CountyIntersectionRules.Approach.West)
                    ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity);
            CreateStopSign(parent, label + " STOP Sign", stop - center - forward * 5f,
                Quaternion.LookRotation(-forward, Vector3.up));

            string movementName = movement.ToString();
            Transform[] route =
            {
                Waypoint(parent, label + " " + movementName + " Stop Line", stop - center),
                Waypoint(parent, label + " " + movementName + " Entry", curveA - center),
                Waypoint(parent, label + " " + movementName + " Turn", curveB - center),
                Waypoint(parent, label + " " + movementName + " Clear Conflict Area", exit - center),
                Waypoint(parent, label + " " + movementName + " Outer Loop A", loopA - center),
                Waypoint(parent, label + " " + movementName + " Outer Loop B", loopB - center),
                Waypoint(parent, label + " " + movementName + " Return Start", start - center)
            };

            GameObject car = Make(parent, "Stop Test Sedan " + label + " " + movementName,
                PrimitiveType.Cube, start - center + Vector3.up * .8f,
                new Vector3(4.2f, 1.25f, 1.9f), new Color(.28f, .38f, .52f),
                Quaternion.LookRotation(forward, Vector3.up));
            car.AddComponent<Rigidbody>().mass = 1150f;
            CountyTrafficVehicleAgent agent = car.AddComponent<CountyTrafficVehicleAgent>();
            agent.Configure(route, reporter, vehicleSpeed);
            Vector3 crosswalkStop = center - forward * 24f;
            agent.ConfigureFourWayStop(controller, approach, movement, stop, exit, crosswalkStop);
        }

        private void AddCrosswalkSensor(Transform parent, Vector3 center, Vector3 approachForward, string label)
        {
            Vector3 position = center - approachForward * 17f;
            GameObject zone = new GameObject(label + " Pedestrian Crosswalk Trigger");
            zone.transform.SetParent(parent, false);
            zone.transform.localPosition = position - center + Vector3.up * 0.9f;
            Vector3 side = Vector3.Cross(Vector3.up, approachForward).normalized;
            zone.transform.localRotation = Quaternion.LookRotation(side, Vector3.up);
            CountyPedestrianCrossingSensor sensor = zone.AddComponent<CountyPedestrianCrossingSensor>();
            sensor.Configure(controller, new Vector3(roadWidth * 0.82f, 1.8f, 3.2f));

            SpawnPedestrian(parent, position - center, side, label);
            for (int i = -3; i <= 3; i++)
            {
                Vector3 stripePosition = position - center + approachForward * (i * 0.45f) + Vector3.up * 0.035f;
                Make(parent, label + " Crosswalk Stripe " + i, PrimitiveType.Cube,
                    stripePosition, new Vector3(0.72f, 0.035f, 2.6f), Color.white,
                    Quaternion.LookRotation(side, Vector3.up));
            }
        }

        private void SpawnPedestrian(Transform parent, Vector3 localCrossingCenter, Vector3 crossingDirection, string label)
        {
            Vector3 start = localCrossingCenter - crossingDirection * 10f + Vector3.up * 0.9f;
            Vector3 finish = localCrossingCenter + crossingDirection * 10f + Vector3.up * 0.9f;
            GameObject actor = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            actor.name = label + " Crosswalk Pedestrian Test Actor";
            actor.transform.SetParent(parent, false);
            actor.transform.localPosition = start;
            actor.transform.localScale = new Vector3(0.48f, 0.9f, 0.48f);
            Renderer renderer = actor.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                Material material = new Material(shader);
                material.color = new Color(0.78f, 0.62f, 0.42f);
                renderer.sharedMaterial = material;
            }
            actor.AddComponent<Rigidbody>().isKinematic = true;
            CountyPedestrianCrossingAgent agent = actor.AddComponent<CountyPedestrianCrossingAgent>();
            agent.Configure(parent.TransformPoint(start), parent.TransformPoint(finish), controller, 1.2f);
        }

        private void CreateStopSign(Transform parent, string signName, Vector3 localPosition,
            Quaternion rotation)
        {
            Make(parent, signName + " Pole", PrimitiveType.Cylinder,
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