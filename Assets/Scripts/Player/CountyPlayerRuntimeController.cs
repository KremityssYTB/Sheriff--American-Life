using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Creates the first mobile-controllable player/patrol-vehicle loop in the runtime county.
    /// Touch UI is deliberately simple and temporary; replace with a polished HUD later.
    /// </summary>
    public sealed class CountyPlayerRuntimeController : MonoBehaviour
    {
        private ThirdPersonWalker walker;
        private PatrolVehicleController vehicle;
        private CameraModeController cameraModes;
        private CharacterController characterController;
        private GameObject player;
        private GameObject patrolVehicle;
        private Transform footThird;
        private Transform footFirst;
        private Transform vehicleThird;
        private Transform vehicleFirst;
        private bool inVehicle;
        private CountyDutyState dutyState;
        private Vector2 moveInput;
        private float throttle;
        private float steering;
        private GUIStyle buttonStyle;
        private GUIStyle statusStyle;

        public void Initialize(Transform worldRoot)
        {
            dutyState = gameObject.AddComponent<CountyDutyState>();
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player Deputy";
            player.transform.SetParent(worldRoot, false);
            player.transform.position = new Vector3(-8f, 1f, -24f);
            var capsule = player.GetComponent<CapsuleCollider>();
            if (capsule) Destroy(capsule);
            characterController = player.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = .32f;
            characterController.center = new Vector3(0f, .9f, 0f);
            walker = player.AddComponent<ThirdPersonWalker>();

            patrolVehicle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            patrolVehicle.name = "Marion County Sheriff Patrol SUV";
            patrolVehicle.transform.SetParent(worldRoot, false);
            patrolVehicle.transform.position = new Vector3(-3f, .8f, -28f);
            patrolVehicle.transform.localScale = new Vector3(2.1f, 1.45f, 4.4f);
            patrolVehicle.GetComponent<Renderer>().material.color = new Color(.08f,.1f,.14f);
            vehicle = patrolVehicle.AddComponent<PatrolVehicleController>();

            footThird = MakeAnchor(player.transform, "Third Person Camera", new Vector3(0,2.7f,-5.2f));
            footFirst = MakeAnchor(player.transform, "First Person Camera", new Vector3(0,1.55f,.12f));
            vehicleThird = MakeAnchor(patrolVehicle.transform, "Vehicle Third Person Camera", new Vector3(0,3.2f,-7f));
            vehicleFirst = MakeAnchor(patrolVehicle.transform, "Vehicle First Person Camera", new Vector3(0,1.15f,.35f));

            var cameraObject = new GameObject("Player Main Camera");
            cameraObject.tag = "MainCamera";
            var cam = cameraObject.AddComponent<Camera>();
            cam.nearClipPlane = .05f;
            cameraModes = cameraObject.AddComponent<CameraModeController>();
            cameraModes.Configure(cam, footThird, footFirst);
            walker.SetCameraTransform(cameraObject.transform);
            cameraModes.SetFirstPerson(false);
        }

        private Transform MakeAnchor(Transform parent, string anchorName, Vector3 localPosition)
        {
            var anchor = new GameObject(anchorName).transform;
            anchor.SetParent(parent, false);
            anchor.localPosition = localPosition;
            anchor.localRotation = Quaternion.identity;
            return anchor;
        }

        private void Update()
        {
            if (walker && !inVehicle) walker.SetMoveInput(moveInput);
            if (vehicle)
            {
                vehicle.SetThrottle(inVehicle ? throttle : 0f);
                vehicle.SetSteering(inVehicle ? steering : 0f);
            }
        }

        private void OnGUI()
        {
            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 18, wordWrap = true };
                statusStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, normal = { textColor = Color.white } };
            }
            float s = Mathf.Clamp(Screen.width / 390f, .8f, 1.25f);
            float w = 66f*s, h = 58f*s, pad = 12f*s;
            GUI.Label(new Rect(pad, pad, 260*s, 30*s), dutyState != null && dutyState.IsOnDuty ? "MARION COUNTY SHERIFF  •  ON DUTY" : "OFF DUTY", statusStyle);
            if (GUI.Button(new Rect(Screen.width-112*s,pad,100*s,42*s), inVehicle ? "EXIT" : "ENTER", buttonStyle))
                ToggleVehicle();
            if (GUI.Button(new Rect(Screen.width-112*s,pad+50*s,100*s,42*s), onDuty ? "GO OFF DUTY" : "GO ON DUTY", buttonStyle))
                dutyState.ToggleDuty();
            if (GUI.Button(new Rect(Screen.width-112*s,pad+100*s,100*s,42*s), "CAMERA", buttonStyle))
                cameraModes.ToggleCameraMode();

            if (!inVehicle)
            {
                moveInput = Vector2.zero;
                if (GUI.RepeatButton(new Rect(pad+w+4*s,Screen.height-h*2-pad,w,h), "▲", buttonStyle)) moveInput.y = 1;
                if (GUI.RepeatButton(new Rect(pad+w+4*s,Screen.height-h-pad,w,h), "▼", buttonStyle)) moveInput.y = -1;
                if (GUI.RepeatButton(new Rect(pad,Screen.height-h-pad,w,h), "◀", buttonStyle)) moveInput.x = -1;
                if (GUI.RepeatButton(new Rect(pad+w*2+8*s,Screen.height-h-pad,w,h), "▶", buttonStyle)) moveInput.x = 1;
                if (GUI.Button(new Rect(pad+w*2+8*s,Screen.height-h*2-pad,w,h), "SPRINT", buttonStyle))
                    walker.SetSprint(true);
                else walker.SetSprint(false);
                throttle = 0f; steering = 0f;
            }
            else
            {
                moveInput = Vector2.zero;
                if (GUI.RepeatButton(new Rect(pad,Screen.height-h-pad,w,h), "LEFT", buttonStyle)) steering = -1;
                else if (GUI.RepeatButton(new Rect(pad+w+4*s,Screen.height-h-pad,w,h), "RIGHT", buttonStyle)) steering = 1;
                else steering = 0;
                if (GUI.RepeatButton(new Rect(Screen.width-w-pad,Screen.height-h-pad,w,h), "GAS", buttonStyle)) throttle = 1;
                else if (GUI.RepeatButton(new Rect(Screen.width-w-pad,Screen.height-h*2-pad,w,h), "BRAKE / REV", buttonStyle)) throttle = -1;
                else throttle = 0;
            }
        }

        private void ToggleVehicle()
        {
            if (!inVehicle)
            {
                if (Vector3.Distance(player.transform.position, patrolVehicle.transform.position) > 5f) return;
                inVehicle = true;
                characterController.enabled = false;
                walker.SetMovementEnabled(false);
                player.GetComponent<Renderer>().enabled = false;
                vehicle.SetControlsEnabled(true);
                cameraModes.Configure(Camera.main, vehicleThird, vehicleFirst);
            }
            else
            {
                vehicle.StopImmediately();
                vehicle.SetControlsEnabled(false);
                inVehicle = false;
                Vector3 exit = patrolVehicle.transform.position - patrolVehicle.transform.right * 2.4f;
                player.transform.SetPositionAndRotation(exit + Vector3.up*.15f, patrolVehicle.transform.rotation);
                player.GetComponent<Renderer>().enabled = true;
                characterController.enabled = true;
                walker.SetMovementEnabled(true);
                cameraModes.Configure(Camera.main, footThird, footFirst);
                moveInput = Vector2.zero;
                throttle = 0f; steering = 0f;
            }
        }
    }
}