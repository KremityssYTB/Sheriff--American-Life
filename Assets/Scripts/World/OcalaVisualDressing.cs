using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Adds a first-pass visual dressing layer to the procedural Ocala blockout.
    /// These are lightweight original meshes/primitives, not final production art.
    /// </summary>
    public sealed class OcalaVisualDressing : MonoBehaviour
    {
        private Transform root;
        private Material asphalt, lanePaint, concrete, curb, glass, trim, foliage, trunk, warmLight, metal, roof;

        public void Build(Transform worldRoot)
        {
            root = worldRoot;
            asphalt = MakeMaterial("Road Asphalt", new Color(.075f,.083f,.09f), 0f, .22f);
            lanePaint = MakeMaterial("Reflective Road Marking", new Color(.88f,.84f,.68f), 0f, .38f);
            concrete = MakeMaterial("Sidewalk Concrete", new Color(.42f,.43f,.41f), 0f, .25f);
            curb = MakeMaterial("Curb Stone", new Color(.31f,.33f,.32f), 0f, .2f);
            glass = MakeMaterial("Blue Tinted Architectural Glass", new Color(.12f,.24f,.3f), .35f, .78f);
            trim = MakeMaterial("Building Trim", new Color(.82f,.79f,.69f), 0f, .3f);
            foliage = MakeMaterial("Florida Foliage", new Color(.12f,.25f,.12f), 0f, .28f);
            trunk = MakeMaterial("Palm Trunk", new Color(.28f,.19f,.12f), 0f, .22f);
            warmLight = MakeMaterial("Warm Street Lighting", new Color(1f,.72f,.38f), 0f, .5f, true);
            metal = MakeMaterial("Street Hardware", new Color(.16f,.18f,.19f), .72f, .62f);
            roof = MakeMaterial("Roof Shingle", new Color(.16f,.17f,.18f), 0f, .32f);

            DressRoads();
            DressBuildings();
            AddStreetFurniture();
            AddFloridaVegetation();
        }

        private Material MakeMaterial(string name, Color color, float metallic, float smoothness, bool emission = false)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var material = new Material(shader) { name = name, color = color };
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (emission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .45f);
            }
            return material;
        }

        private GameObject Part(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material, Transform parent = null)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent != null ? parent : root, true);
            part.transform.position = position;
            part.transform.localScale = scale;
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;

            // These pieces are visual dressing only. The underlying road/building
            // collision remains on the gameplay geometry, so decorative colliders
            // only add needless physics broadphase and contact work.
            Collider decorativeCollider = part.GetComponent<Collider>();
            if (decorativeCollider != null) Destroy(decorativeCollider);
            return part;
        }

        private void OnDestroy()
        {
            DestroyOwnedMaterial(asphalt);
            DestroyOwnedMaterial(lanePaint);
            DestroyOwnedMaterial(concrete);
            DestroyOwnedMaterial(curb);
            DestroyOwnedMaterial(glass);
            DestroyOwnedMaterial(trim);
            DestroyOwnedMaterial(foliage);
            DestroyOwnedMaterial(trunk);
            DestroyOwnedMaterial(warmLight);
            DestroyOwnedMaterial(metal);
            DestroyOwnedMaterial(roof);
        }

        private static void DestroyOwnedMaterial(Material material)
        {
            if (material != null) Destroy(material);
        }

        private void DressRoads()
        {
            // Main Street sidewalks, curb edges, lane separators and stop bars.
            Part("Main Street South Sidewalk", PrimitiveType.Cube, new Vector3(0,.08f,-91), new Vector3(280,.16f,7), concrete);
            Part("Main Street North Sidewalk", PrimitiveType.Cube, new Vector3(0,.08f,-69), new Vector3(280,.16f,7), concrete);
            Part("Main Street South Curb", PrimitiveType.Cube, new Vector3(0,.2f,-87.2f), new Vector3(280,.35f,.45f), curb);
            Part("Main Street North Curb", PrimitiveType.Cube, new Vector3(0,.2f,-72.8f), new Vector3(280,.35f,.45f), curb);
            for (int x=-130; x<=130; x+=12)
            {
                Part("Main Street Dashed Center Line", PrimitiveType.Cube, new Vector3(x,.105f,-80), new Vector3(5,.025f,.16f), lanePaint);
            }

            Part("Cross Street West Sidewalk", PrimitiveType.Cube, new Vector3(-21,.08f,-80), new Vector3(7,.16f,280), concrete);
            Part("Cross Street East Sidewalk", PrimitiveType.Cube, new Vector3(21,.08f,-80), new Vector3(7,.16f,280), concrete);
            Part("Cross Street West Curb", PrimitiveType.Cube, new Vector3(-17.2f,.2f,-80), new Vector3(.45f,.35f,280), curb);
            Part("Cross Street East Curb", PrimitiveType.Cube, new Vector3(17.2f,.2f,-80), new Vector3(.45f,.35f,280), curb);
            for (int z=-210; z<=50; z+=12)
                Part("Cross Street Dashed Center Line", PrimitiveType.Cube, new Vector3(0,.11f,z), new Vector3(.16f,.025f,5), lanePaint);

            // Highway edge lines and intermittent center dashes.
            Part("Highway Edge Line West", PrimitiveType.Cube, new Vector3(-8.2f,.09f,80), new Vector3(.18f,.025f,700), lanePaint);
            Part("Highway Edge Line East", PrimitiveType.Cube, new Vector3(8.2f,.09f,80), new Vector3(.18f,.025f,700), lanePaint);
            for (int z=-260; z<=420; z+=18)
                Part("Highway Center Dash", PrimitiveType.Cube, new Vector3(0,.095f,z), new Vector3(.18f,.025f,8), lanePaint);

            // Painted crosswalks at the Main/Cross Street junction.
            for (int i=-5; i<=5; i++)
            {
                Part("West Crosswalk Stripe", PrimitiveType.Cube, new Vector3(-10f,.115f,-80f+i*.82f), new Vector3(3.2f,.035f,.42f), lanePaint);
                Part("East Crosswalk Stripe", PrimitiveType.Cube, new Vector3(10f,.115f,-80f+i*.82f), new Vector3(3.2f,.035f,.42f), lanePaint);
                Part("North Crosswalk Stripe", PrimitiveType.Cube, new Vector3(i*.82f,.115f,-70f), new Vector3(.42f,.035f,3.2f), lanePaint);
                Part("South Crosswalk Stripe", PrimitiveType.Cube, new Vector3(i*.82f,.115f,-90f), new Vector3(.42f,.035f,3.2f), lanePaint);
            }
        }

        private void DressBuildings()
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Transform building = renderer.transform;
                if (building.name != "Town Building" && building.name != "Sheriff Office" &&
                    building.name != "Marion County Jail Intake" && building.name != "Gas Station" &&
                    building.name != "Rural Barn") continue;

                Vector3 size = building.localScale;
                Vector3 center = building.position;
                if (building.name == "Rural Barn") continue;

                // Roof cap and parapet make the placeholder blocks read as buildings.
                Part(building.name + " Roof Cap", PrimitiveType.Cube,
                    center + Vector3.up * (size.y * .52f), new Vector3(size.x + .5f,.35f,size.z + .5f),
                    building.name == "Gas Station" ? trim : roof);

                if (building.name == "Gas Station")
                {
                    Part("Gas Station Canopy", PrimitiveType.Cube, center + new Vector3(0,4.2f,13),
                        new Vector3(24,.45f,10), trim);
                    for (int x=-8; x<=8; x+=8)
                        Part("Fuel Pump Island", PrimitiveType.Cube, center + new Vector3(x,1.2f,13),
                            new Vector3(1.2f,2.2f,1.2f), metal);
                    continue;
                }

                float windowY = center.y + Mathf.Min(size.y * .2f, 2.2f);
                for (float x=-size.x*.34f; x<=size.x*.34f; x+=Mathf.Max(4f,size.x*.28f))
                {
                    Part("Facade Window", PrimitiveType.Cube, center + new Vector3(x,windowY,-size.z*.505f),
                        new Vector3(Mathf.Min(3.2f,size.x*.18f),2.1f,.12f), glass);
                    Part("Facade Window", PrimitiveType.Cube, center + new Vector3(x,windowY,size.z*.505f),
                        new Vector3(Mathf.Min(3.2f,size.x*.18f),2.1f,.12f), glass);
                }
                Part("Main Entrance Door", PrimitiveType.Cube, center + new Vector3(0,-size.y*.22f,-size.z*.51f),
                    new Vector3(1.5f,3f,.16f), metal);
                if (building.name == "Sheriff Office" || building.name == "Marion County Jail Intake")
                {
                    Part("Facility Sign Panel", PrimitiveType.Cube, center + new Vector3(0,size.y*.28f,-size.z*.52f),
                        new Vector3(Mathf.Min(size.x*.65f,18f),1.15f,.2f), trim);
                }
            }
        }

        private void AddStreetFurniture()
        {
            for (int x=-120; x<=120; x+=40)
            {
                AddStreetLamp(new Vector3(x,.0f,-94));
                AddStreetLamp(new Vector3(x,.0f,-66));
            }
            for (int z=-190; z<=30; z+=40)
            {
                AddStreetLamp(new Vector3(-24,0,z));
                AddStreetLamp(new Vector3(24,0,z));
            }
            // Sidewalk planters and low utility cabinets add scale cues.
            for (int x=-100; x<=100; x+=50)
                Part("Sidewalk Utility Cabinet", PrimitiveType.Cube, new Vector3(x,.65f,-95), new Vector3(1.1f,1.3f,.8f), metal);
        }

        private void AddStreetLamp(Vector3 position)
        {
            Part("Street Lamp Pole", PrimitiveType.Cylinder, position + Vector3.up*4f,
                new Vector3(.12f,4f,.12f), metal);
            Part("Street Lamp Arm", PrimitiveType.Cube, position + new Vector3(.55f,7.8f,0),
                new Vector3(1.2f,.12f,.12f), metal);
            Part("Street Lamp Fixture", PrimitiveType.Cube, position + new Vector3(1.05f,7.7f,0),
                new Vector3(.65f,.18f,.4f), warmLight);
        }

        private void AddFloridaVegetation()
        {
            // Palms and broad-canopy trees frame the first downtown block.
            for (int x=-140; x<=140; x+=28)
            {
                AddPalm(new Vector3(x,0,-101));
                AddPalm(new Vector3(x,0,-59));
            }
            for (int z=-180; z<=20; z+=34)
            {
                AddCanopyTree(new Vector3(-31,0,z), 1f);
                AddCanopyTree(new Vector3(31,0,z), .9f);
            }
            // Rural tree belts create a horizon beyond the first greybox streets.
            for (int i=0;i<24;i++)
            {
                float x = -330f + i*28f;
                AddCanopyTree(new Vector3(x,0,270), Random.Range(.8f,1.35f));
                AddCanopyTree(new Vector3(x,0,-330), Random.Range(.8f,1.35f));
            }
        }

        private void AddPalm(Vector3 p)
        {
            Part("Florida Palm Trunk", PrimitiveType.Cylinder, p+Vector3.up*3.2f,
                new Vector3(.42f,3.2f,.42f), trunk);
            for (int i=0;i<7;i++)
            {
                float angle = i*(360f/7f);
                GameObject frond = Part("Palm Frond", PrimitiveType.Capsule,
                    p+Vector3.up*6.35f+Quaternion.Euler(0,angle,0)*new Vector3(0,0,1.35f),
                    new Vector3(.18f,.12f,1.8f), foliage);
                frond.transform.rotation = Quaternion.Euler(22f,angle,18f);
            }
        }

        private void AddCanopyTree(Vector3 p, float scale)
        {
            Part("Tree Trunk", PrimitiveType.Cylinder, p+Vector3.up*(2.1f*scale),
                new Vector3(.38f*scale,2.1f*scale,.38f*scale), trunk);
            Part("Tree Canopy", PrimitiveType.Sphere, p+Vector3.up*(4.2f*scale),
                new Vector3(4.2f*scale,3.2f*scale,4.2f*scale), foliage);
            Part("Tree Canopy Secondary", PrimitiveType.Sphere,
                p+new Vector3(1.25f*scale,3.6f*scale,.4f*scale),
                new Vector3(2.6f*scale,2.4f*scale,2.6f*scale), foliage);
        }
    }
}
