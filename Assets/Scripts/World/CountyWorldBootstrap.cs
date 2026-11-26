using UnityEngine;

namespace SheriffAmericanLife
{
    public class CountyWorldBootstrap : MonoBehaviour
    {
        [SerializeField] private int seed = 352;
        [SerializeField] private bool generateOnStart = true;
        private Transform root;
        private IncidentReportBook reportBook;
        private OfficerInteractionController interactionController;
        private WitnessStatementBook witnessStatementBook;
        private DispatchCallManager dispatchCallManager;

        private void Start() { if (generateOnStart) GenerateWorld(); }

        [ContextMenu("Generate Test County")]
        public void GenerateWorld()
        {
            if (root != null) Destroy(root.gameObject);
            root = new GameObject("Marion_County_Ocala_Blockout").transform;
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
            var clock = root.gameObject.AddComponent<WorldClockSystem>();
            clock.SetSun(sun);
            reportBook = root.gameObject.AddComponent<IncidentReportBook>();
            witnessStatementBook = root.gameObject.AddComponent<WitnessStatementBook>();
            dispatchCallManager = root.gameObject.AddComponent<DispatchCallManager>();
            dispatchCallManager.Configure(reportBook);
            root.gameObject.AddComponent<MobileMdtController>().Configure(dispatchCallManager, reportBook, witnessStatementBook);
            root.gameObject.AddComponent<MobileMdtScreen>();
            var incidentReporter = root.gameObject.AddComponent<WorldIncidentReporter>();
            incidentReporter.Configure(clock);
            var incidentDirector = root.gameObject.AddComponent<CountyIncidentDirector>();
            incidentDirector.Configure(incidentReporter, new CountyIncidentDirector.Scenario[] {
                new CountyIncidentDirector.Scenario {
                    type = WorldIncidentType.VehicleCollision,
                    description = "Two-vehicle collision reported near the main road. Injuries unknown.",
                    subjectDescription = "Two passenger vehicles; driver details not confirmed.",
                    location = new Vector3(12, 0, -80), weight = 2f
                },
                new CountyIncidentDirector.Scenario {
                    type = WorldIncidentType.Theft,
                    description = "Resident reports a vehicle break-in at a local parking area.",
                    subjectDescription = "Unknown suspect; property details pending.",
                    location = new Vector3(58, 0, -108), weight = 1.5f
                },
                new CountyIncidentDirector.Scenario {
                    type = WorldIncidentType.Disturbance,
                    description = "911 caller reports a loud argument outside a residence.",
                    subjectDescription = "Two people reportedly involved; identities unknown.",
                    location = new Vector3(72, 0, -30), weight = 2f
                },
                new CountyIncidentDirector.Scenario {
                    type = WorldIncidentType.Fire,
                    description = "Smoke reported coming from a small outbuilding. Fire size unknown.",
                    subjectDescription = "No confirmed occupants.",
                    location = new Vector3(210, 0, 210), weight = .7f
                },
                new CountyIncidentDirector.Scenario {
                    type = WorldIncidentType.MedicalEmergency,
                    description = "Caller requests medical assistance for an unresponsive person.",
                    subjectDescription = "Patient identity and condition not confirmed.",
                    location = new Vector3(20, 0, -170), weight = 1f
                },
                new CountyIncidentDirector.Scenario {
                    type = WorldIncidentType.SuspiciousActivity,
                    description = "Caller reports a person checking vehicle door handles.",
                    subjectDescription = "Unknown person wearing dark clothing.",
                    location = new Vector3(55, 0, 120), weight = 1.2f
                },
                new CountyIncidentDirector.Scenario {
                    type = WorldIncidentType.Assault,
                    description = "911 caller reports a physical altercation near a business.",
                    subjectDescription = "Suspect description unavailable; scene safety unknown.",
                    location = new Vector3(-75, 0, -15), weight = .6f
                }
            });
            interactionController = root.gameObject.AddComponent<OfficerInteractionController>();
            interactionController.Configure(reportBook);
            SpawnTestCivilians(clock, witnessStatementBook, incidentReporter);
        }

        private void SpawnTestCivilians(WorldClockSystem clock, WitnessStatementBook statements, WorldIncidentReporter reporter)
        {
            Transform homeA = CreateWaypoint("Civilian A Home", new Vector3(-28, 0, -112));
            Transform workA = CreateWaypoint("Civilian A Work", new Vector3(18, 0, -72));
            Transform shopA = CreateWaypoint("Civilian A Shop", new Vector3(58, 0, -108));
            Transform homeB = CreateWaypoint("Civilian B Home", new Vector3(72, 0, -30));
            Transform workB = CreateWaypoint("Civilian B Work", new Vector3(-22, 0, -72));
            Transform parkB = CreateWaypoint("Civilian B Park", new Vector3(20, 0, -170));

            CivilianCharacter firstResident = CreateCivilian("Resident - Worker", new Vector3(-28, 1, -112), new Color(.22f,.42f,.72f), clock, statements, reporter,
                new CivilianRoutineAgent.RoutineStop[] {
                    Stop(7, 0, workA, "Commuting to work"),
                    Stop(17, 0, shopA, "Shopping after work"),
                    Stop(18, 0, homeA, "At home")
                });
            CreateCivilian("Resident - Local", new Vector3(72, 1, -30), new Color(.72f,.48f,.25f), clock, statements,
                new CivilianRoutineAgent.RoutineStop[] {
                    Stop(8, 0, workB, "Heading to work"),
                    Stop(16, 30, parkB, "Visiting the park"),
                    Stop(18, 0, homeB, "At home")
                });
            interactionController.SetFocusedCivilian(firstResident);
        }

        private Transform CreateWaypoint(string waypointName, Vector3 position)
        {
            var waypoint = new GameObject(waypointName);
            waypoint.transform.SetParent(root, false);
            waypoint.transform.position = position;
            return waypoint.transform;
        }

        private CivilianCharacter CreateCivilian(string civilianName, Vector3 position, Color color, WorldClockSystem clock,
            WitnessStatementBook statements, WorldIncidentReporter reporter, CivilianRoutineAgent.RoutineStop[] schedule)
        {
            GameObject civilian = Make(civilianName, PrimitiveType.Capsule, position, new Vector3(.8f, 1f, .8f), color);
            civilian.AddComponent<CivilianRoutineAgent>().Configure(clock, schedule);
            CivilianCharacter character = civilian.AddComponent<CivilianCharacter>();
            civilian.AddComponent<CivilianWitnessAgent>().Configure(statements);
            civilian.AddComponent<Civilian911CallSource>().Configure(reporter, character);
            return character;
        }

        private CivilianRoutineAgent.RoutineStop Stop(int hour, int minute, Transform destination, string activity)
        {
            return new CivilianRoutineAgent.RoutineStop {
                startHour = hour,
                startMinute = minute,
                destination = destination,
                arrivalPauseSeconds = 5f,
                activity = activity
            };
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