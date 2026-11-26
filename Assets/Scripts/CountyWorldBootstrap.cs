using UnityEngine;

namespace SheriffAmericanLife
{
    public class CountyWorldBootstrap : MonoBehaviour
    {
        [Header("World")]
        [SerializeField] private int blockCount = 8;
        [SerializeField] private float blockSpacing = 28f;
        [SerializeField] private Material roadMaterial;
        [SerializeField] private Material groundMaterial;

        private void Start()
        {
            BuildWorld();
        }

        [ContextMenu("Build Cypress County Blockout")]
        public void BuildWorld()
        {
            if (transform.childCount > 0) return;

            CreatePrimitive("County Ground", PrimitiveType.Plane, new Vector3(0, -0.15f, 0),
                new Vector3(20, 1, 20), groundMaterial);

            CreatePrimitive("Main Street", PrimitiveType.Cube, new Vector3(0, 0, 0),
                new Vector3(14, 0.12f, blockCount * blockSpacing), roadMaterial);
            CreatePrimitive("Cross Street", PrimitiveType.Cube, new Vector3(0, 0.01f, 0),
                new Vector3(blockCount * blockSpacing, 0.12f, 14), roadMaterial);

            for (int i = 0; i < blockCount; i++)
            {
                float p = (i - (blockCount - 1) * 0.5f) * blockSpacing;
                CreateBuilding(new Vector3(-24, 0, p), new Vector3(14, Random.Range(7, 18), 18));
                CreateBuilding(new Vector3(24, 0, p), new Vector3(14, Random.Range(7, 18), 18));
                CreateBuilding(new Vector3(p, 0, -24), new Vector3(18, Random.Range(7, 18), 14));
                CreateBuilding(new Vector3(p, 0, 24), new Vector3(18, Random.Range(7, 18), 14));
            }

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0, 1, -8);
            player.AddComponent<Legacy.ThirdPersonWalker>();
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0, 4, -14);
            cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            var follow = cameraObject.AddComponent<Legacy.CameraModeController>();
            follow.SetTarget(player.transform);
            var dispatch = gameObject.AddComponent<Legacy.DispatchCallSystem>();
            dispatch.SetPlayer(player.transform);

            CreatePatrolVehicle(new Vector3(5, 0.8f, -12));
        }

        private void CreateBuilding(Vector3 position, Vector3 scale)
        {
            var building = CreatePrimitive("Commercial / Residential Block", PrimitiveType.Cube,
                position + Vector3.up * (scale.y * 0.5f), scale, null);
            building.GetComponent<Renderer>().material.color = new Color(0.62f, 0.64f, 0.66f);
        }

        private void CreatePatrolVehicle(Vector3 position)
        {
            var car = CreatePrimitive("Sheriff Patrol SUV", PrimitiveType.Cube, position,
                new Vector3(2.2f, 1.3f, 4.2f), null);
            car.GetComponent<Renderer>().material.color = new Color(0.08f, 0.1f, 0.13f);
            car.AddComponent<Legacy.PatrolVehicleController>();
        }

        private GameObject CreatePrimitive(string objectName, PrimitiveType type, Vector3 position,
            Vector3 scale, Material material)
        {
            var item = GameObject.CreatePrimitive(type);
            item.name = objectName;
            item.transform.SetParent(transform);
            item.transform.position = position;
            item.transform.localScale = scale;
            if (material != null) item.GetComponent<Renderer>().sharedMaterial = material;
            return item;
        }
    }
}