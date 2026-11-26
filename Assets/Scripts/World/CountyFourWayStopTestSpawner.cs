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
        [SerializeField] private float roadWidth = 22f;
        [SerializeField] private float laneWidth = 3.4f;
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

            AddLaneMarkings(junctionRoot, center, Vector3.forward);
            AddLaneMarkings(junctionRoot, center, Vector3.right);

            AddCrosswalkSensor(junctionRoot, center, Vector3.back, "North");
            AddCrosswalkSensor(junctionRoot, center, Vector3.left, "East");
            AddCrosswalkSensor(junctionRoot, center, Vector3.forward, "South");
            AddCrosswalkSensor(junctionRoot, center, Vector3.right, "West");
        }

        private void AddStopApproach(Transform parent, Vector3 center, Vector3 forward,
            CountyIntersectionRules.Approach approach, string label,
            CountyIntersectionRules.Movement ignoredDefaultMovement)
        {
            Vector3 side = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 stop = center - forward * 10f;
            Vector3 start = center - forward * 62f;

            // Right-hand traffic: left-turn lane, through lane, and right-turn lane.
            // Lane offsets are measured along the vehicle's right vector.
            AddStopLane(parent, center, start, stop, side, forward, approach,
                CountyIntersectionRules.Movement.Left, label, -laneWidth, "Left Turn");
            AddStopLane(parent, center, start, stop, side, forward, approach,
                CountyIntersectionRules.Movement.Straight, label, 0f, "Through");
            AddStopLane(parent, center, start, stop, side, forward, approach,
                CountyIntersectionRules.Movement.Right, label, laneWidth, "Right Turn");

        }

        private void AddLaneMarkings(Transform parent, Vector3 center, Vector3 roadForward)
        {
            Vector3 side = Vector3.Cross(Vector3.up, roadForward).normalized;
            for (int laneBoundary = -2; laneBoundary <= 2; laneBoundary++)
            {
                float offset = laneBoundary * laneWidth;
                for (float distance = -roadLength * 0.5f; distance <= roadLength * 0.5f; distance += 8f)
                {
                    if (Mathf.Abs(distance) < 18f) continue;
                    Vector3 position = center + roadForward * distance + side * offset;
                    Make(parent, "Lane Marking " + roadForward + " " + laneBoundary + " " + distance,
                        PrimitiveType.Cube, position - center + Vector3.up * 0.035f,
                        new Vector3(0.12f, 0.025f, 4f), new Color(0.88f, 0.78f, 0.25f),
                        Quaternion.LookRotation(roadForward, Vector3.up));
                }
            }
        }

        private void AddStopLane(Transform parent, Vector3 center, Vector3 start,
            Vector3 stop, Vector3 side, Vector3 forward,
            CountyIntersectionRules.Approach approach, CountyIntersectionRules.Movement movement,
            string label, float laneOffset, string laneName)
        {
            Vector3 inboundOffset = side * laneOffset;
            Vector3 exitDirection = movement == CountyIntersectionRules.Movement.Straight
                ? forward
                : (movement == CountyIntersectionRules.Movement.Left ? -side : side);
            Vector3 exitSide = Vector3.Cross(Vector3.up, exitDirection).normalized;
            Vector3 exitOffset = movement == CountyIntersectionRules.Movement.Straight
                ? exitSide * laneOffset
                : exitSide * laneWidth;

            Vector3 p0 = center + forward * -7f + inboundOffset;
            Vector3 p3 = center + forward * 9f + exitDirection * 24f + exitOffset;
            Vector3 p1 = p0 + forward * 13f;
            Vector3 p2 = p3 - exitDirection * 13f;
            Vector3 exit = p3;
            Vector3 loopA = exit + exitDirection * 42f + exitSide * 35f;
            Vector3 loopB = start + inboundOffset + side * 35f;

            Vector3 stopLane = stop + inboundOffset;
            Make(parent, label + " " + laneName + " Stop Bar", PrimitiveType.Cube,
                stopLane - center, new Vector3(laneWidth * 0.88f, .025f, .45f),
                Color.white, Quaternion.LookRotation(forward, Vector3.up));
            CreateStopSign(parent, label + " " + laneName + " STOP Sign",
                stopLane - center - forward * 5f + side * (laneWidth * 0.68f),
                Quaternion.LookRotation(-forward, Vector3.up));

            Vector3[] routePoints = new Vector3[6];
            for (int i = 0; i < 5; i++)
            {
                float t = (i + 1) / 5f;
                float u = 1f - t;
                routePoints[i] = u * u * u * p0 +
                    3f * u * u * t * p1 +
                    3f * u * t * t * p2 +
                    t * t * t * p3;
            }
            routePoints[5] = exit + exitDirection * 28f + exitOffset;

            AddStopVehicle(parent, center, start, stopLane, routePoints,
                loopA, loopB, side, forward, exitDirection, approach, movement,
                label, laneName, 0f, "Queue Lead", Vector3.zero);
            AddStopVehicle(parent, center, start, stopLane, routePoints,
                loopA, loopB, side, forward, exitDirection, approach, movement,
                label, laneName, 0f, "Queue Rear", -forward * 16f);
        }

        private void AddStopVehicle(Transform parent, Vector3 center, Vector3 start,
            Vector3 stop, Vector3[] turnRoute, Vector3 loopA, Vector3 loopB,
            Vector3 side, Vector3 forward, Vector3 exitDirection,
            CountyIntersectionRules.Approach approach, CountyIntersectionRules.Movement movement,
            string label, string laneName, float unusedLaneOffset, string queueName, Vector3 startOffset)
        {
            string routeName = label + " " + laneName + " " + queueName + " " + movement;
            Transform[] route = new Transform[turnRoute.Length + 3];
            route[0] = Waypoint(parent, routeName + " Stop Line", stop - center);
            for (int i = 0; i < turnRoute.Length; i++)
                route[i + 1] = Waypoint(parent, routeName + " Turn Path " + i, turnRoute[i] - center);
            route[route.Length - 2] = Waypoint(parent, routeName + " Outer Loop A", loopA - center);
            route[route.Length - 1] = Waypoint(parent, routeName + " Outer Loop B", loopB - center);

            Vector3 initialForward = forward;
            GameObject car = Make(parent, "Stop Test Sedan " + routeName,
                PrimitiveType.Cube, start - center + startOffset + Vector3.up * .8f,
                new Vector3(1.9f, 1.25f, 4.2f), new Color(.28f, .38f, .52f),
                Quaternion.LookRotation(initialForward, Vector3.up));
            car.AddComponent<Rigidbody>().mass = 1150f;
            CountyTrafficVehicleAgent agent = car.AddComponent<CountyTrafficVehicleAgent>();
            agent.Configure(route, reporter, vehicleSpeed);

            // One clearly marked test emergency vehicle exercises reservation priority.
            bool isEmergencyTestVehicle = label == "North" &&
                laneName == "Through" && queueName == "Queue Lead";
            if (isEmergencyTestVehicle)
            {
                agent.ConfigureEmergencyVehicle(true);
                Renderer bodyRenderer = car.GetComponent<Renderer>();
                if (bodyRenderer != null) bodyRenderer.material.color = new Color(.035f, .09f, .22f);
                GameObject redLight = Make(car.transform, "Emergency Lightbar Red", PrimitiveType.Cube,
                    new Vector3(-.28f, .68f, 0f), new Vector3(.28f, .14f, .34f),
                    new Color(.9f, .04f, .035f));
                GameObject blueLight = Make(car.transform, "Emergency Lightbar Blue", PrimitiveType.Cube,
                    new Vector3(.28f, .68f, 0f), new Vector3(.28f, .14f, .34f),
                    new Color(.04f, .2f, .95f));
                CountyEmergencyLightbar lightbar = car.AddComponent<CountyEmergencyLightbar>();
                lightbar.Configure(redLight.GetComponent<Renderer>(),
                    blueLight.GetComponent<Renderer>(), 2.5f);
            }

            Vector3 crosswalkStop = center - forward * 24f + (stop - (center - forward * 10f));
            agent.ConfigureFourWayStop(controller, approach, movement,
                parent.TransformPoint(stop - center),
                parent.TransformPoint(turnRoute[turnRoute.Length - 1] - center),
                parent.TransformPoint(crosswalkStop - center));
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
            sensor.Configure(controller, new Vector3(3.8f, 1.8f, roadWidth * 0.82f));

            SpawnPedestrian(parent, position - center, side, label);
            for (int i = -3; i <= 3; i++)
            {
                Vector3 stripePosition = position - center + approachForward * (i * 0.45f) + Vector3.up * 0.035f;
                Make(parent, label + " Crosswalk Stripe " + i, PrimitiveType.Cube,
                    stripePosition, new Vector3(0.45f, 0.035f, roadWidth * 0.82f), Color.white,
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