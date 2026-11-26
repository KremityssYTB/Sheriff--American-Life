using UnityEngine;

namespace SheriffAmericanLife
{
    public class CountyWorldBootstrap : MonoBehaviour
    {
        [SerializeField] private int seed = 352;
        [SerializeField] private bool generateOnStart = true;
        private Transform root;

        private void Start() { if (generateOnStart) GenerateWorld(); }

        [ContextMenu("Generate Test County")]
        public void GenerateWorld()
        {
            if (root != null) Destroy(root.gameObject);
            root = new GameObject("Cypress_County_Blockout").transform;
            root.SetParent(transform, false);
            Random.InitState(seed);

            Make("Ground", PrimitiveType.Plane, new Vector3(0,-.2f,0), new Vector3(70,1,70), new Color(.23f,.38f,.2f));
            Make("Highway", PrimitiveType.Cube, new Vector3(0,0,80), new Vector3(18,.12f,700), new Color(.12f,.12f,.12f));
            Make("Main Street", PrimitiveType.Cube, new Vector3(0,.02f,-80), new Vector3(280,.14f,14), new Color(.16f,.16f,.16f));

            for (int x=-4; x<=4; x++)
            for (int z=-3; z<=2; z++)
            {
                if (Random.value < .18f) continue;
                float h=Random.Range(7f,20f);
                Make("Town Building", PrimitiveType.Cube, new Vector3(x*42,h/2,-80+z*38),
                    new Vector3(Random.Range(16f,27f),h,Random.Range(15f,25f)),
                    new Color(Random.Range(.35f,.72f),Random.Range(.35f,.65f),Random.Range(.3f,.58f)));
            }
            Make("Sheriff Office", PrimitiveType.Cube, new Vector3(-75,5,-15), new Vector3(42,10,28), new Color(.72f,.74f,.76f));
            Make("Gas Station", PrimitiveType.Cube, new Vector3(55,3,120), new Vector3(28,6,22), new Color(.78f,.72f,.48f));
            Make("Rural Barn", PrimitiveType.Cube, new Vector3(210,5,210), new Vector3(30,10,24), new Color(.42f,.16f,.12f));

            RenderSettings.ambientLight = new Color(.55f,.58f,.62f);
            var sun = new GameObject("County Sun").AddComponent<Light>();
            sun.type=LightType.Directional; sun.intensity=1.15f;
            sun.transform.rotation=Quaternion.Euler(48,-32,0); sun.transform.SetParent(root,true);
        }

        private GameObject Make(string n, PrimitiveType t, Vector3 p, Vector3 s, Color c)
        {
            var go=GameObject.CreatePrimitive(t); go.name=n; go.transform.SetParent(root,true);
            go.transform.position=p; go.transform.localScale=s;
            var r=go.GetComponent<Renderer>();
            if (r != null) { var shader=Shader.Find("Universal Render Pipeline/Lit"); if(shader==null) shader=Shader.Find("Standard"); var m=new Material(shader); m.color=c; r.sharedMaterial=m; }
            return go;
        }
    }
}