using UnityEngine;

namespace SheriffAmericanLife
{
    public enum TrafficVehicleStyle
    {
        Sedan,
        Pickup,
        Suv,
        Muscle,
        Exotic
    }

    /// <summary>
    /// Procedural traffic silhouettes used until licensed production vehicle meshes are integrated.
    /// These are generic vehicle classes, not exact reproductions or branded models.
    /// </summary>
    public sealed class TrafficVehicleVisuals : MonoBehaviour
    {
        public void Build(Color paintColor) { Build(paintColor, TrafficVehicleStyle.Sedan); }

        public void Build(Color paintColor, TrafficVehicleStyle style)
        {
            Renderer blockRenderer = GetComponent<Renderer>();
            if (blockRenderer != null) blockRenderer.enabled = false;

            float width = style == TrafficVehicleStyle.Exotic ? 2.02f :
                style == TrafficVehicleStyle.Suv || style == TrafficVehicleStyle.Pickup ? 1.98f : 1.84f;
            float bodyHeight = style == TrafficVehicleStyle.Suv || style == TrafficVehicleStyle.Pickup ? .72f :
                style == TrafficVehicleStyle.Exotic ? .42f : .56f;
            float cabinHeight = style == TrafficVehicleStyle.Suv ? .88f :
                style == TrafficVehicleStyle.Pickup ? .72f :
                style == TrafficVehicleStyle.Exotic ? .43f :
                style == TrafficVehicleStyle.Muscle ? .48f : .58f;
            float cabinLength = style == TrafficVehicleStyle.Pickup ? 1.55f :
                style == TrafficVehicleStyle.Exotic ? 1.48f :
                style == TrafficVehicleStyle.Muscle ? 1.72f : 1.95f;
            float cabinZ = style == TrafficVehicleStyle.Pickup ? .05f :
                style == TrafficVehicleStyle.Exotic ? -.08f : -.12f;
            float wheelZ = style == TrafficVehicleStyle.Exotic ? 1.25f : 1.3f;
            float wheelRadius = style == TrafficVehicleStyle.Suv || style == TrafficVehicleStyle.Pickup ? .36f :
                style == TrafficVehicleStyle.Exotic ? .32f : .31f;
            float bodyY = style == TrafficVehicleStyle.Exotic ? -.16f : -.05f;

            Material paint = Create("Vehicle Paint", paintColor, .48f, .78f);
            Material glass = Create("Smoked Glass", new Color(.035f,.065f,.085f), .08f, .9f);
            Material tire = Create("Tire Rubber", new Color(.018f,.02f,.022f), 0f, .2f);
            Material trim = Create("Dark Trim", new Color(.035f,.04f,.045f), .25f, .55f);
            Material alloy = Create("Wheel Alloy", new Color(.48f,.51f,.54f), .82f, .78f);
            Material lamp = Create("Lamp Lens", new Color(.9f,.84f,.68f), .05f, .6f, true);
            Material tail = Create("Tail Lamp", new Color(.55f,.012f,.008f), .05f, .55f, true);

            Part("Vehicle Main Body", PrimitiveType.Cube, new Vector3(0,bodyY,0),
                new Vector3(width,bodyHeight,4.15f), paint);
            Part("Front Hood", PrimitiveType.Cube, new Vector3(0,bodyY + bodyHeight*.48f,1.42f),
                new Vector3(width*.96f,.16f,1.18f), paint);

            if (style == TrafficVehicleStyle.Pickup)
            {
                Part("Pickup Cab Roof", PrimitiveType.Cube, new Vector3(0,.77f,.12f),
                    new Vector3(1.58f,.14f,1.52f), paint);
                Part("Open Pickup Bed Floor", PrimitiveType.Cube, new Vector3(0,.18f,-1.38f),
                    new Vector3(1.68f,.12f,1.18f), trim);
                for (int side=-1;side<=1;side+=2)
                    Part("Pickup Bed Rail", PrimitiveType.Cube, new Vector3(side*.84f,.43f,-1.38f),
                        new Vector3(.12f,.48f,1.28f), paint);
                Part("Pickup Tailgate", PrimitiveType.Cube, new Vector3(0,.38f,-1.98f),
                    new Vector3(1.72f,.48f,.12f), paint);
            }
            else
            {
                float roofY = style == TrafficVehicleStyle.Suv ? 1.08f :
                    style == TrafficVehicleStyle.Exotic ? .48f :
                    style == TrafficVehicleStyle.Muscle ? .62f : .72f;
                Part("Cabin Roof", PrimitiveType.Cube, new Vector3(0,roofY,cabinZ),
                    new Vector3(width*.82f,.14f,cabinLength), paint);
            }

            float glassY = style == TrafficVehicleStyle.Suv ? .76f :
                style == TrafficVehicleStyle.Pickup ? .58f :
                style == TrafficVehicleStyle.Exotic ? .25f : .48f;
            Part("Front Windshield", PrimitiveType.Cube, new Vector3(0,glassY,.86f),
                new Vector3(width*.8f,.42f,.055f), glass).transform.localRotation = Quaternion.Euler(-18f,0,0);
            Part("Rear Windshield", PrimitiveType.Cube, new Vector3(0,glassY,-1.05f),
                new Vector3(width*.8f,.38f,.055f), glass).transform.localRotation = Quaternion.Euler(18f,0,0);

            for (int side=-1;side<=1;side+=2)
            {
                float x=side*(width*.42f);
                Part("Front Side Glass", PrimitiveType.Cube, new Vector3(x,glassY,.38f),
                    new Vector3(.045f,.34f,style == TrafficVehicleStyle.Exotic ? .58f : .68f), glass);
                if (style != TrafficVehicleStyle.Exotic)
                    Part("Rear Side Glass", PrimitiveType.Cube, new Vector3(x,glassY,-.48f),
                        new Vector3(.045f,.34f,.72f), glass);
                Part("Side Mirror", PrimitiveType.Cube, new Vector3(side*(width*.52f),glassY-.06f,.83f),
                    new Vector3(.15f,.12f,.2f), trim);
                Part("Side Sill", PrimitiveType.Cube, new Vector3(side*(width*.5f),-.31f,-.05f),
                    new Vector3(.1f,.1f,2.7f), trim);
                if (style == TrafficVehicleStyle.Suv)
                    Part("SUV Lower Cladding", PrimitiveType.Cube, new Vector3(side*(width*.5f),-.2f,0),
                        new Vector3(.08f,.22f,3.5f), trim);
            }

            for (int side=-1;side<=1;side+=2)
            for (int axle=-1;axle<=1;axle+=2)
            {
                Vector3 p=new Vector3(side*(width*.5f),-.32f,axle*wheelZ);
                GameObject wheel=Part("Vehicle Tire",PrimitiveType.Cylinder,p,
                    new Vector3(wheelRadius*2,.22f,wheelRadius*2),tire);
                wheel.transform.localRotation=Quaternion.Euler(0,0,90);
                GameObject face=Part("Alloy Wheel Face",PrimitiveType.Cylinder,
                    p+new Vector3(side*.12f,0,0),new Vector3(wheelRadius*1.25f,.24f,wheelRadius*1.25f),alloy);
                face.transform.localRotation=Quaternion.Euler(0,0,90);
            }

            for (int side=-1;side<=1;side+=2)
            {
                Part("Headlight",PrimitiveType.Cube,new Vector3(side*width*.34f,.12f,2.1f),
                    new Vector3(.38f,.15f,.055f),lamp);
                Part("Tail Light",PrimitiveType.Cube,new Vector3(side*width*.36f,.12f,-2.1f),
                    new Vector3(.3f,.2f,.055f),tail);
            }
            Part("Front Bumper",PrimitiveType.Cube,new Vector3(0,-.27f,2.1f),
                new Vector3(width+.04f,.18f,.14f),trim);
            Part("Rear Bumper",PrimitiveType.Cube,new Vector3(0,-.27f,-2.1f),
                new Vector3(width+.04f,.18f,.14f),trim);

            if (style == TrafficVehicleStyle.Muscle)
            {
                Part("Muscle Car Hood Scoop",PrimitiveType.Cube,new Vector3(0,.31f,1.42f),
                    new Vector3(.48f,.13f,.62f),trim);
                Part("Rear Spoiler",PrimitiveType.Cube,new Vector3(0,.52f,-1.78f),
                    new Vector3(1.45f,.08f,.2f),trim);
            }
            if (style == TrafficVehicleStyle.Exotic)
            {
                Part("Low Front Splitter",PrimitiveType.Cube,new Vector3(0,-.38f,2.12f),
                    new Vector3(1.9f,.07f,.35f),trim);
                Part("Rear Diffuser",PrimitiveType.Cube,new Vector3(0,-.34f,-2.05f),
                    new Vector3(1.5f,.12f,.32f),trim);
                Part("Rear Engine Cover",PrimitiveType.Cube,new Vector3(0,.14f,-.95f),
                    new Vector3(1.3f,.1f,.8f),trim);
            }
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