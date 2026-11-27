using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Lightweight sedan silhouette for procedural traffic actors. The parent keeps
    /// its existing Rigidbody and collision box; visual child colliders are removed.
    /// </summary>
    public sealed class TrafficVehicleVisuals : MonoBehaviour
    {
        public void Build(Color paintColor)
        {
            Renderer blockRenderer = GetComponent<Renderer>();
            if (blockRenderer != null) blockRenderer.enabled = false;

            Material paint = Create("Vehicle Paint", paintColor, .42f, .7f);
            Material glass = Create("Smoked Glass", new Color(.055f,.1f,.13f), .08f, .86f);
            Material tire = Create("Tire Rubber", new Color(.018f,.02f,.022f), 0f, .2f);
            Material trim = Create("Dark Trim", new Color(.045f,.05f,.055f), .2f, .45f);
            Material alloy = Create("Wheel Alloy", new Color(.4f,.43f,.45f), .8f, .68f);
            Material lamp = Create("Lamp Lens", new Color(.9f,.82f,.62f), .05f, .55f, true);
            Material tail = Create("Tail Lamp", new Color(.5f,.015f,.012f), .05f, .5f, true);

            Part("Sedan Lower Body", PrimitiveType.Cube, new Vector3(0,-.05f,0),
                new Vector3(1.82f,.58f,4f), paint);
            Part("Sedan Hood", PrimitiveType.Cube, new Vector3(0,.22f,1.35f),
                new Vector3(1.78f,.18f,1.25f), paint);
            Part("Sedan Trunk", PrimitiveType.Cube, new Vector3(0,.22f,-1.4f),
                new Vector3(1.78f,.18f,.95f), paint);
            Part("Sedan Cabin Roof", PrimitiveType.Cube, new Vector3(0,.67f,-.12f),
                new Vector3(1.48f,.13f,1.9f), paint);
            Part("Front Windshield", PrimitiveType.Cube, new Vector3(0,.48f,.88f),
                new Vector3(1.48f,.42f,.055f), glass).transform.localRotation = Quaternion.Euler(-18f,0,0);
            Part("Rear Windshield", PrimitiveType.Cube, new Vector3(0,.48f,-1.05f),
                new Vector3(1.48f,.4f,.055f), glass).transform.localRotation = Quaternion.Euler(18f,0,0);

            for (int side=-1;side<=1;side+=2)
            {
                float x=side*.76f;
                Part("Front Side Glass", PrimitiveType.Cube, new Vector3(x,.49f,.42f),
                    new Vector3(.04f,.37f,.72f), glass);
                Part("Rear Side Glass", PrimitiveType.Cube, new Vector3(x,.49f,-.48f),
                    new Vector3(.04f,.37f,.78f), glass);
                Part("B Pillar", PrimitiveType.Cube, new Vector3(x,.5f,-.02f),
                    new Vector3(.07f,.42f,.09f), paint);
                Part("Side Mirror", PrimitiveType.Cube, new Vector3(side*.94f,.42f,.83f),
                    new Vector3(.15f,.12f,.2f), trim);
                Part("Side Sill", PrimitiveType.Cube, new Vector3(side*.91f,-.31f,-.05f),
                    new Vector3(.1f,.1f,2.7f), trim);
            }

            for (int side=-1;side<=1;side+=2)
            for (int axle=-1;axle<=1;axle+=2)
            {
                Vector3 p=new Vector3(side*.91f,-.32f,axle*1.28f);
                GameObject wheel=Part("Sedan Tire",PrimitiveType.Cylinder,p,new Vector3(.62f,.2f,.62f),tire);
                wheel.transform.localRotation=Quaternion.Euler(0,0,90);
                GameObject face=Part("Sedan Alloy Face",PrimitiveType.Cylinder,
                    p+new Vector3(side*.12f,0,0),new Vector3(.39f,.22f,.39f),alloy);
                face.transform.localRotation=Quaternion.Euler(0,0,90);
            }

            for (int side=-1;side<=1;side+=2)
            {
                Part("Headlight",PrimitiveType.Cube,new Vector3(side*.65f,.12f,2.02f),
                    new Vector3(.36f,.16f,.05f),lamp);
                Part("Tail Light",PrimitiveType.Cube,new Vector3(side*.68f,.12f,-2.02f),
                    new Vector3(.28f,.22f,.05f),tail);
            }
            Part("Front Bumper",PrimitiveType.Cube,new Vector3(0,-.27f,2.02f),
                new Vector3(1.86f,.16f,.12f),trim);
            Part("Rear Bumper",PrimitiveType.Cube,new Vector3(0,-.27f,-2.02f),
                new Vector3(1.86f,.16f,.12f),trim);
        }

        private GameObject Part(string name,PrimitiveType type,Vector3 position,Vector3 scale,Material material)
        {
            GameObject part=GameObject.CreatePrimitive(type);
            part.name=name;
            part.transform.SetParent(transform,false);
            part.transform.localPosition=position;
            part.transform.localScale=scale;
            Renderer renderer=part.GetComponent<Renderer>();
            if(renderer!=null) renderer.sharedMaterial=material;
            Collider collider=part.GetComponent<Collider>();
            if(collider!=null) Destroy(collider);
            return part;
        }

        private Material Create(string name,Color color,float metallic,float smoothness,bool emission=false)
        {
            Shader shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null) shader=Shader.Find("Standard");
            Material material=new Material(shader){name=name,color=color};
            if(material.HasProperty("_Metallic")) material.SetFloat("_Metallic",metallic);
            if(material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness",smoothness);
            if(emission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor",color*.55f);
            }
            return material;
        }
    }
}