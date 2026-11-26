using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Spawns visible greybox actors for county incident scenarios.
    /// These are prototype scene representations, not final vehicle or character systems.
    /// </summary>
    public sealed class CountyIncidentActorSpawner : MonoBehaviour
    {
        [SerializeField] private Transform actorRoot;
        [SerializeField] private Material vehicleMaterial;
        [SerializeField] private Material suspectMaterial;
        [SerializeField] private Material emergencyMaterial;

        public void Configure(Transform parent)
        {
            actorRoot = parent;
        }

        public GameObject Spawn(WorldIncidentType type, Vector3 position)
        {
            switch (type)
            {
                case WorldIncidentType.VehicleCollision:
                    return SpawnCollision(position);
                case WorldIncidentType.Fire:
                    return SpawnFire(position);
                case WorldIncidentType.MedicalEmergency:
                    return SpawnMedicalScene(position);
                case WorldIncidentType.Theft:
                case WorldIncidentType.SuspiciousActivity:
                    return SpawnSuspectScene(position);
                case WorldIncidentType.Assault:
                case WorldIncidentType.Disturbance:
                    return SpawnDisturbanceScene(position);
                default:
                    return SpawnMarker("Incident Scene", position, new Color(.85f,.72f,.2f));
            }
        }

        private GameObject SpawnCollision(Vector3 p)
        {
            GameObject scene = NewRoot("Vehicle Collision Scene", p);
            GameObject carA = MakeBox("Damaged Sedan A", scene.transform, new Vector3(-2.2f,.65f,0),
                new Vector3(4.2f,1.2f,1.9f), vehicleMaterial != null ? vehicleMaterial.color : new Color(.18f,.28f,.38f));
            carA.transform.rotation = Quaternion.Euler(0,-12,0);
            GameObject carB = MakeBox("Damaged Sedan B", scene.transform, new Vector3(1.4f,.65f,1.2f),
                new Vector3(4.1f,1.2f,1.9f), new Color(.35f,.35f,.36f));
            carB.transform.rotation = Quaternion.Euler(0,68,0);
            MakeBox("Debris", scene.transform, new Vector3(0,.12f,-1.7f), new Vector3(.8f,.18f,.55f), new Color(.12f,.12f,.12f));
            MakeBox("Debris", scene.transform, new Vector3(2,.1f,-1.1f), new Vector3(.45f,.12f,.35f), new Color(.16f,.16f,.16f));
            AddSceneMarker(scene.transform, new Vector3(0,2.2f,0), new Color(1f,.65f,.08f));
            return scene;
        }

        private GameObject SpawnFire(Vector3 p)
        {
            GameObject scene = NewRoot("Fire Scene", p);
            MakeBox("Burning Structure", scene.transform, Vector3.zero, new Vector3(4,2.4f,3),
                new Color(.18f,.14f,.12f));
            GameObject flame = new GameObject("Flame and Smoke");
            flame.transform.SetParent(scene.transform, false);
            flame.transform.localPosition = new Vector3(0,1.2f,0);
            ParticleSystem particles = flame.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.1f,2.3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f,2.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(.45f,1.1f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f,.28f,.03f), new Color(1f,.78f,.08f));
            main.maxParticles = 180;
            var emission = particles.emission;
            emission.rateOverTime = 48;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 22;
            shape.radius = .8f;
            var smoke = particles.colorOverLifetime;
            smoke.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(new Color(.18f,.18f,.18f),0), new GradientColorKey(new Color(.08f,.08f,.08f),1) },
                new[] { new GradientAlphaKey(.75f,0), new GradientAlphaKey(0,1) });
            smoke.color = gradient;
            particles.Play();
            Light glow = flame.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(1f,.28f,.05f);
            glow.intensity = 3f;
            glow.range = 12f;
            AddSceneMarker(scene.transform, new Vector3(0,3.5f,0), new Color(1f,.2f,.03f));
            return scene;
        }

        private GameObject SpawnMedicalScene(Vector3 p)
        {
            GameObject scene = NewRoot("Medical Emergency Scene", p);
            GameObject patient = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            patient.name = "Injured Civilian - Greybox";
            patient.transform.SetParent(scene.transform, false);
            patient.transform.localPosition = new Vector3(0,.35f,0);
            patient.transform.localScale = new Vector3(.65f,.35f,.65f);
            patient.transform.localRotation = Quaternion.Euler(0,0,90);
            SetColor(patient, new Color(.25f,.4f,.75f));
            MakeBox("Medical Bag Placeholder", scene.transform, new Vector3(1,.2f,.8f),
                new Vector3(.45f,.3f,.3f), new Color(.85f,.85f,.82f));
            AddSceneMarker(scene.transform, new Vector3(0,1.8f,0), new Color(.9f,.1f,.1f));
            return scene;
        }

        private GameObject SpawnSuspectScene(Vector3 p)
        {
            GameObject scene = NewRoot("Reported Theft Scene", p);
            MakePerson("Reported Person", scene.transform, new Vector3(0,0,1.8f),
                suspectMaterial != null ? suspectMaterial.color : new Color(.12f,.13f,.16f));
            MakeBox("Property / Vehicle Placeholder", scene.transform, new Vector3(2,.55f,0),
                new Vector3(2.2f,1.1f,1.1f), new Color(.28f,.32f,.36f));
            AddSceneMarker(scene.transform, new Vector3(0,2.2f,1.8f), new Color(1f,.65f,.05f));
            return scene;
        }

        private GameObject SpawnDisturbanceScene(Vector3 p)
        {
            GameObject scene = NewRoot("Disturbance Scene", p);
            MakePerson("Person One", scene.transform, new Vector3(-1.1f,0,0), new Color(.3f,.45f,.72f));
            MakePerson("Person Two", scene.transform, new Vector3(1.1f,0,0), new Color(.68f,.38f,.25f));
            AddSceneMarker(scene.transform, new Vector3(0,2.2f,0), new Color(1f,.65f,.05f));
            return scene;
        }

        private GameObject SpawnMarker(string label, Vector3 p, Color color)
        {
            GameObject scene = NewRoot(label, p);
            AddSceneMarker(scene.transform, new Vector3(0,2,0), color);
            return scene;
        }

        private GameObject NewRoot(string label, Vector3 position)
        {
            GameObject root = new GameObject(label);
            if (actorRoot != null) root.transform.SetParent(actorRoot, true);
            root.transform.position = position;
            return root;
        }

        private static GameObject MakeBox(string label, Transform parent, Vector3 localPosition,
            Vector3 scale, Color color)
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = label;
            item.transform.SetParent(parent, false);
            item.transform.localPosition = localPosition;
            item.transform.localScale = scale;
            SetColor(item, color);
            return item;
        }

        private static void MakePerson(string label, Transform parent, Vector3 localPosition, Color color)
        {
            GameObject person = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            person.name = label;
            person.transform.SetParent(parent, false);
            person.transform.localPosition = localPosition + Vector3.up;
            person.transform.localScale = new Vector3(.7f,1f,.7f);
            SetColor(person, color);
        }

        private static void AddSceneMarker(Transform parent, Vector3 localPosition, Color color)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "Incident Marker";
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = localPosition;
            marker.transform.localScale = new Vector3(.18f,.12f,.18f);
            SetColor(marker, color);
        }

        private static void SetColor(GameObject target, Color color)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer == null) return;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader != null)
            {
                Material material = new Material(shader);
                material.color = color;
                renderer.sharedMaterial = material;
            }
        }
    }
}
