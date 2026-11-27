using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Procedural first-pass sheriff SUV body. All detail meshes are children of the
    /// vehicle root and have their own primitive colliders removed; the root keeps
    /// the simple collision volume used by the existing driving/interaction loop.
    /// Replace this authored blockout with a licensed production vehicle mesh later.
    /// </summary>
    public sealed class PatrolSuvVisuals : MonoBehaviour
    {
        [Header("Paint")]
        [SerializeField] private Color bodyColor = new Color(.91f,.92f,.9f);
        [SerializeField] private Color darkPaint = new Color(.035f,.05f,.075f);
        [SerializeField] private Color glassColor = new Color(.055f,.13f,.18f);
        [SerializeField] private Color goldColor = new Color(.72f,.52f,.16f);

        private Transform visualRoot;
        private Material body, dark, glass, rubber, alloy, chrome, redLens, blueLens, gold, white, black;

        public void Build()
        {
            Renderer oldBody = GetComponent<Renderer>();
            if (oldBody != null) oldBody.enabled = false;

            body = Mat("Pearl White Police Paint", bodyColor, .48f, .72f);
            dark = Mat("Deep Navy Lower Trim", darkPaint, .22f, .48f);
            glass = Mat("Tinted Safety Glass", glassColor, .12f, .88f);
            rubber = Mat("Tire Rubber", new Color(.018f,.02f,.022f), 0f, .22f);
            alloy = Mat("Wheel Alloy", new Color(.42f,.45f,.47f), .82f, .72f);
            chrome = Mat("Brushed Chrome", new Color(.62f,.65f,.67f), .86f, .8f);
            redLens = Mat("Red Tail Lamp", new Color(.55f,.018f,.012f), .1f, .55f, true);
            blueLens = Mat("Blue Emergency Lens", new Color(.015f,.12f,.8f), .1f, .6f, true);
            gold = Mat("Sheriff Gold", goldColor, .28f, .62f);
            white = Mat("White Marking", new Color(.96f,.96f,.92f), .05f, .45f);
            black = Mat("Grille Black", new Color(.012f,.016f,.02f), .18f, .34f);

            visualRoot = new GameObject("Patrol SUV Visual Model").transform;
            visualRoot.SetParent(transform, false);

            // Main body and sculpted front/rear volumes.
            Part("Lower Body Shell", PrimitiveType.Cube, new Vector3(0,-.02f,0),
                new Vector3(2.02f,.72f,4.15f), body);
            Part("Front Hood", PrimitiveType.Cube, new Vector3(0,.34f,1.36f),
                new Vector3(1.94f,.22f,1.22f), body);
            Part("Rear Cargo Deck", PrimitiveType.Cube, new Vector3(0,.34f,-1.38f),
                new Vector3(1.94f,.22f,1.12f), body);
            Part("Front Bumper", PrimitiveType.Cube, new Vector3(0,-.32f,2.08f),
                new Vector3(2.04f,.24f,.18f), dark);
            Part("Rear Bumper", PrimitiveType.Cube, new Vector3(0,-.32f,-2.08f),
                new Vector3(2.04f,.24f,.18f), dark);

            // Raised cabin, roof, windshield and side glazing.
            Part("Cabin Roof", PrimitiveType.Cube, new Vector3(0,.91f,-.12f),
                new Vector3(1.76f,.16f,2.15f), body);
            Part("Windshield", PrimitiveType.Cube, new Vector3(0,.64f,1.02f),
                new Vector3(1.68f,.53f,.055f), glass).transform.localRotation = Quaternion.Euler(-17f,0,0);
            Part("Rear Window", PrimitiveType.Cube, new Vector3(0,.65f,-1.18f),
                new Vector3(1.65f,.48f,.055f), glass).transform.localRotation = Quaternion.Euler(17f,0,0);
            AddInterior();
            for (int side=-1; side<=1; side+=2)
            {
                float x = side*.89f;
                Part("Front Door Window", PrimitiveType.Cube, new Vector3(x,.66f,.48f),
                    new Vector3(.045f,.47f,.86f), glass);
                Part("Rear Door Window", PrimitiveType.Cube, new Vector3(x,.66f,-.48f),
                    new Vector3(.045f,.47f,.82f), glass);
                Part("Rear Quarter Window", PrimitiveType.Cube, new Vector3(x,.66f,-1.03f),
                    new Vector3(.045f,.43f,.24f), glass);
                Part("Window Pillar Front", PrimitiveType.Cube, new Vector3(x,.66f,.94f),
                    new Vector3(.09f,.57f,.1f), body);
                Part("Window Pillar Center", PrimitiveType.Cube, new Vector3(x,.66f,-.02f),
                    new Vector3(.09f,.57f,.1f), body);
                Part("Window Pillar Rear", PrimitiveType.Cube, new Vector3(x,.66f,-.91f),
                    new Vector3(.09f,.57f,.1f), body);

                // Door handles, mirrors, running boards and side livery.
                Part("Door Handle Front", PrimitiveType.Cube, new Vector3(side*1.025f,.22f,.55f),
                    new Vector3(.035f,.07f,.22f), chrome);
                Part("Door Handle Rear", PrimitiveType.Cube, new Vector3(side*1.025f,.22f,-.48f),
                    new Vector3(.035f,.07f,.22f), chrome);
                Part("Side Mirror", PrimitiveType.Cube, new Vector3(side*1.08f,.65f,.94f),
                    new Vector3(.2f,.16f,.24f), dark);
                Part("Running Board", PrimitiveType.Cube, new Vector3(side*1.02f,-.38f,-.05f),
                    new Vector3(.16f,.12f,1.7f), dark);
                Part("Gold Side Stripe", PrimitiveType.Cube, new Vector3(side*1.015f,-.08f,-.05f),
                    new Vector3(.025f,.16f,2.2f), gold);
                Part("Front Fender Flare", PrimitiveType.Cube, new Vector3(side*1.015f,-.04f,1.32f),
                    new Vector3(.09f,.18f,1.05f), dark);
                Part("Rear Fender Flare", PrimitiveType.Cube, new Vector3(side*1.015f,-.04f,-1.32f),
                    new Vector3(.09f,.18f,1.05f), dark);
            }

            // Four detailed wheel assemblies.
            for (int side=-1; side<=1; side+=2)
            for (int axle=-1; axle<=1; axle+=2)
            {
                Vector3 wheelPos = new Vector3(side*1.02f,-.39f,axle*1.34f);
                GameObject tire = Part("All Terrain Tire", PrimitiveType.Cylinder, wheelPos,
                    new Vector3(.68f,.24f,.68f), rubber);
                tire.transform.localRotation = Quaternion.Euler(0,0,90);
                GameObject rim = Part("Alloy Wheel Face", PrimitiveType.Cylinder,
                    wheelPos + new Vector3(side*.135f,0,0), new Vector3(.43f,.255f,.43f), alloy);
                rim.transform.localRotation = Quaternion.Euler(0,0,90);
                Part("Wheel Hub", PrimitiveType.Cylinder,
                    wheelPos + new Vector3(side*.275f,0,0), new Vector3(.16f,.27f,.16f), chrome)
                    .transform.localRotation = Quaternion.Euler(0,0,90);
                for (int spoke=0;spoke<6;spoke++)
                {
                    float a=spoke*60f*Mathf.Deg2Rad;
                    Part("Wheel Spoke", PrimitiveType.Cube,
                        wheelPos + new Vector3(side*.285f,Mathf.Sin(a)*.14f,Mathf.Cos(a)*.14f),
                        new Vector3(.035f,.055f,.27f), chrome).transform.localRotation =
                        Quaternion.Euler(spoke*60f,0,90);
                }
            }

            // Front fascia: grille, headlights, fog lamps and plate.
            Part("Front Grille", PrimitiveType.Cube, new Vector3(0,.02f,2.105f),
                new Vector3(.82f,.3f,.055f), black);
            for (int i=-2;i<=2;i++)
                Part("Grille Slat", PrimitiveType.Cube, new Vector3(i*.14f,.02f,2.145f),
                    new Vector3(.025f,.25f,.025f), chrome);
            for (int side=-1;side<=1;side+=2)
            {
                Part("LED Headlamp Housing", PrimitiveType.Cube, new Vector3(side*.73f,.12f,2.105f),
                    new Vector3(.43f,.25f,.08f), chrome);
                Part("LED Headlamp Lens", PrimitiveType.Cube, new Vector3(side*.73f,.12f,2.16f),
                    new Vector3(.35f,.15f,.035f), white);
                Part("Fog Lamp", PrimitiveType.Cube, new Vector3(side*.78f,-.25f,2.11f),
                    new Vector3(.18f,.12f,.045f), white);
                Part("Rear Tail Lamp Housing", PrimitiveType.Cube, new Vector3(side*.78f,.12f,-2.105f),
                    new Vector3(.32f,.42f,.07f), redLens);
                Part("Rear Reflector", PrimitiveType.Cube, new Vector3(side*.78f,-.2f,-2.11f),
                    new Vector3(.25f,.08f,.05f), redLens);
            }
            Part("Front License Plate", PrimitiveType.Cube, new Vector3(0,-.23f,2.12f),
                new Vector3(.42f,.18f,.025f), white);
            Part("Rear License Plate", PrimitiveType.Cube, new Vector3(0,-.23f,-2.12f),
                new Vector3(.42f,.18f,.025f), white);

            // Roof equipment and sheriff identity marks.
            Part("Roof Antenna", PrimitiveType.Cylinder, new Vector3(.48f,1.08f,-.76f),
                new Vector3(.025f,.2f,.025f), dark);
            Part("Push Bumper", PrimitiveType.Cube, new Vector3(0,-.14f,2.2f),
                new Vector3(1.42f,.12f,.12f), chrome);
            Part("Hood Center Accent", PrimitiveType.Cube, new Vector3(0,.47f,1.34f),
                new Vector3(.06f,.015f,.8f), dark);

            AddLabel("SHERIFF", new Vector3(0,.08f,2.19f), Quaternion.identity, .18f, gold);
            AddLabel("MARION COUNTY", new Vector3(0,.11f,-2.19f), Quaternion.Euler(0,180,0), .12f, gold);
            for (int side=-1;side<=1;side+=2)
            {
                AddLabel("SHERIFF", new Vector3(side*1.04f,.1f,-.08f),
                    Quaternion.Euler(0,side>0?90f:-90f,0), .19f, gold);
                AddLabel("MARION COUNTY", new Vector3(side*1.045f,-.23f,-.08f),
                    Quaternion.Euler(0,side>0?90f:-90f,0), .075f, white);
                AddStarBadge(side);
            }

            // Emergency lightbar base; animated red/blue modules remain controlled by existing system.
            Part("Lightbar Dark Housing", PrimitiveType.Cube, new Vector3(0,1.02f,-.12f),
                new Vector3(1.15f,.12f,.34f), dark);
        }


        private void AddInterior()
        {
            // Front dashboard and center stack.
            Part("Dashboard", PrimitiveType.Cube, new Vector3(0,.38f,.83f),
                new Vector3(1.62f,.28f,.42f), dark);
            Part("Instrument Cluster Hood", PrimitiveType.Cube, new Vector3(-.48f,.58f,.72f),
                new Vector3(.62f,.18f,.26f), black);
            Part("Center Infotainment Screen", PrimitiveType.Cube, new Vector3(.18f,.52f,.76f),
                new Vector3(.34f,.22f,.045f), glass);
            Part("Center Console", PrimitiveType.Cube, new Vector3(.12f,.08f,.05f),
                new Vector3(.42f,.22f,1.2f), black);
            Part("Gear Selector", PrimitiveType.Cube, new Vector3(.12f,.25f,.42f),
                new Vector3(.1f,.24f,.12f), chrome);
            Part("Steering Column", PrimitiveType.Cylinder, new Vector3(-.5f,.48f,.62f),
                new Vector3(.1f,.36f,.1f), dark).transform.localRotation = Quaternion.Euler(90f,0,0);
            Part("Steering Wheel", PrimitiveType.Cylinder, new Vector3(-.5f,.62f,.56f),
                new Vector3(.42f,.42f,.055f), black).transform.localRotation = Quaternion.Euler(90f,0,0);
            Part("Steering Wheel Center", PrimitiveType.Cylinder, new Vector3(-.5f,.62f,.515f),
                new Vector3(.12f,.12f,.065f), chrome).transform.localRotation = Quaternion.Euler(90f,0,0);

            // Two front bucket seats and two rear bench positions.
            for (int side=-1;side<=1;side+=2)
            {
                float x=side*.48f;
                Part("Front Seat Cushion", PrimitiveType.Cube, new Vector3(x,.12f,.12f),
                    new Vector3(.58f,.18f,.62f), black);
                Part("Front Seat Back", PrimitiveType.Cube, new Vector3(x,.48f,-.12f),
                    new Vector3(.58f,.66f,.18f), black);
                Part("Front Seat Headrest", PrimitiveType.Cube, new Vector3(x,.83f,-.12f),
                    new Vector3(.34f,.22f,.18f), dark);
                Part("Rear Seat Cushion", PrimitiveType.Cube, new Vector3(x,.12f,-.92f),
                    new Vector3(.58f,.18f,.52f), black);
                Part("Rear Seat Back", PrimitiveType.Cube, new Vector3(x,.43f,-1.12f),
                    new Vector3(.58f,.52f,.16f), black);
            }
            Part("Rear Bench Center", PrimitiveType.Cube, new Vector3(0,.25f,-1.08f),
                new Vector3(.42f,.3f,.2f), dark);
            Part("Driver Footwell", PrimitiveType.Cube, new Vector3(-.48f,-.2f,.72f),
                new Vector3(.55f,.08f,.5f), black);
            Part("Passenger Footwell", PrimitiveType.Cube, new Vector3(.48f,-.2f,.72f),
                new Vector3(.55f,.08f,.5f), black);
        }

        private void AddStarBadge(int side)
        {
            GameObject badge = Part("Sheriff Star Badge", PrimitiveType.Cylinder,
                new Vector3(side*1.055f,.08f,.68f), new Vector3(.34f,.025f,.34f), gold);
            badge.transform.localRotation = Quaternion.Euler(0,0,90);
        }

        private void AddLabel(string text, Vector3 localPosition, Quaternion localRotation, float size, Material material)
        {
            GameObject labelObject = new GameObject(text + " Marking");
            labelObject.transform.SetParent(visualRoot, false);
            labelObject.transform.localPosition = localPosition;
            labelObject.transform.localRotation = localRotation;
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = size;
            label.fontSize = 64;
            label.color = material.color;
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private GameObject Part(string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(visualRoot, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            return part;
        }

        private void OnDestroy()
        {
            Release(body);
            Release(dark);
            Release(glass);
            Release(rubber);
            Release(alloy);
            Release(chrome);
            Release(redLens);
            Release(blueLens);
            Release(gold);
            Release(white);
            Release(black);
        }

        private static void Release(Material material)
        {
            if (material == null) return;
            if (Application.isPlaying) Destroy(material);
            else DestroyImmediate(material);
        }

        private Material Mat(string name, Color color, float metallic, float smoothness, bool emission = false)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader) { name = name, color = color };
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (emission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .65f);
            }
            return material;
        }
    }
}