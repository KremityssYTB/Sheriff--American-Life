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
        private OfficerInteractionController interactions;
        private readonly Collider[] focusBuffer = new Collider[32];
        private CivilianCharacter focusedCivilian;
        private Vector2 moveInput;
        private float throttle;
        private float steering;
        private GUIStyle buttonStyle;
        private GUIStyle statusStyle;

        public void Initialize(Transform worldRoot)
        {
            dutyState = gameObject.AddComponent<CountyDutyState>();
            interactions = worldRoot.GetComponent<OfficerInteractionController>();
            if (interactions != null) interactions.ConfigureDutyState(dutyState);
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player Deputy";
            player.transform.SetParent(worldRoot, false);
            player.transform.position = new Vector3(-6.5f, 1f, -26.5f);
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
            vehicle.SetControlsEnabled(false);

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
            anchor.localRotation = anchorName.Contains("Third Person") ? Quaternion.Euler(12f,0f,0f) : Quaternion.identity;
            return anchor;
        }

        private void Update()
        {
            RefreshCivilianFocus();
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
            if (GUI.Button(new Rect(Screen.width-112*s,pad+50*s,100*s,42*s), dutyState != null && dutyState.IsOnDuty ? "GO OFF DUTY" : "GO ON DUTY", buttonStyle))
                dutyState.ToggleDuty();
            if (GUI.Button(new Rect(Screen.width-112*s,pad+100*s,100*s,42*s), "CAMERA", buttonStyle))
                cameraModes.ToggleCameraMode();

            DrawCivilianInteractionHud(s, pad);

            if (!inVehicle)
            {
                moveInput = Vector2.zero;
                if (GUI.RepeatButton(new Rect(pad+w+4*s,Screen.height-h*2-pad,w,h), "▲", buttonStyle)) moveInput.y = 1;
                if (GUI.RepeatButton(new Rect(pad+w+4*s,Screen.height-h-pad,w,h), "▼", buttonStyle)) moveInput.y = -1;
                if (GUI.RepeatButton(new Rect(pad,Screen.height-h-pad,w,h), "◀", buttonStyle)) moveInput.x = -1;
                if (GUI.RepeatButton(new Rect(pad+w*2+8*s,Screen.height-h-pad,w,h), "▶", buttonStyle)) moveInput.x = 1;
                if (GUI.RepeatButton(new Rect(pad+w*2+8*s,Screen.height-h*2-pad,w,h), "SPRINT", buttonStyle))
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

        private void RefreshCivilianFocus()
        {
            if (inVehicle || player == null || interactions == null)
            {
                SetFocusedCivilian(null);
                return;
            }

            Vector3 focusOrigin = player.transform.position + Vector3.up * 0.9f;
            int count = Physics.OverlapSphereNonAlloc(
                focusOrigin,
                5.5f,
                focusBuffer,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            CivilianCharacter nearest = null;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                Collider candidateCollider = focusBuffer[i];
                if (candidateCollider == null) continue;

                CivilianCharacter candidate = candidateCollider.GetComponentInParent<CivilianCharacter>();
                if (candidate == null) continue;

                float distance = Vector3.Distance(focusOrigin, candidate.transform.position);
                if (distance < nearestDistance)
                {
                    nearest = candidate;
                    nearestDistance = distance;
                }
            }

            SetFocusedCivilian(nearest);
        }

        private void SetFocusedCivilian(CivilianCharacter civilian)
        {
            if (focusedCivilian == civilian) return;
            focusedCivilian = civilian;
            if (interactions != null) interactions.SetFocusedCivilian(focusedCivilian);
        }

        private void DrawCivilianInteractionHud(float scale, float padding)
        {
            if (inVehicle || focusedCivilian == null || interactions == null) return;

            GUI.Label(
                new Rect(padding, padding + 34f * scale, 260f * scale, 28f * scale),
                "NEARBY: " + focusedCivilian.FullName,
                statusStyle);

            float buttonWidth = 76f * scale;
            float buttonHeight = 36f * scale;
            float gap = 5f * scale;
            float x = Screen.width - (buttonWidth * 2f + gap + padding);
            float y = padding + 150f * scale;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "GREET", buttonStyle))
                interactions.Greet();
            if (GUI.Button(new Rect(x + buttonWidth + gap, y, buttonWidth, buttonHeight), "QUESTIONS", buttonStyle))
                interactions.AskQuestions();

            y += buttonHeight + gap;
            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "ID", buttonStyle))
                interactions.RequestIdentification();
            if (GUI.Button(new Rect(x + buttonWidth + gap, y, buttonWidth, buttonHeight), "WARN", buttonStyle))
                interactions.IssueWarning();

            y += buttonHeight + gap;
            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "CITE", buttonStyle))
                interactions.IssueCitation();
            if (GUI.Button(new Rect(x + buttonWidth + gap, y, buttonWidth, buttonHeight), "DETAIN", buttonStyle))
                interactions.Detain();

            y += buttonHeight + gap;
            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "ARREST", buttonStyle))
                interactions.Arrest();
            if (GUI.Button(new Rect(x + buttonWidth + gap, y, buttonWidth, buttonHeight), "RELEASE", buttonStyle))
                interactions.Release();
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
                // Do not eject the player into a wall, another vehicle, or an occupied space.
                // If every candidate is blocked, remain seated and let the player reposition.
                if (!TryFindSafeExitPosition(out Vector3 exitPosition)) return;

                vehicle.StopImmediately();
                vehicle.SetControlsEnabled(false);
                inVehicle = false;
                player.transform.SetPositionAndRotation(exitPosition, patrolVehicle.transform.rotation);
                player.GetComponent<Renderer>().enabled = true;
                characterController.enabled = true;
                walker.SetMovementEnabled(true);
                cameraModes.Configure(Camera.main, footThird, footFirst);
                moveInput = Vector2.zero;
                throttle = 0f; steering = 0f;
            }
        }

        private bool TryFindSafeExitPosition(out Vector3 safePosition)
        {
            safePosition = Vector3.zero;
            if (patrolVehicle == null) return false;

            Transform vehicleTransform = patrolVehicle.transform;
            Vector3[] directions =
            {
                -vehicleTransform.right,
                vehicleTransform.right,
                -vehicleTransform.forward,
                vehicleTransform.forward
            };

            for (int i = 0; i < directions.Length; i++)
            {
                float distance = i < 2 ? 2.4f : 3.2f;
                Vector3 probe = vehicleTransform.position + directions[i] * distance;
                Vector3 rayOrigin = probe + Vector3.up * 3f;

                if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit groundHit,
                    8f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    continue;

                Vector3 candidate = groundHit.point + Vector3.up * 0.08f;
                Vector3 capsuleBottom = candidate + Vector3.up * 0.34f;
                Vector3 capsuleTop = candidate + Vector3.up * 1.46f;

                bool occupied = Physics.CheckCapsule(
                    capsuleBottom,
                    capsuleTop,
                    0.32f,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore);

                if (occupied) continue;

                safePosition = candidate;
                return true;
            }

            return false;
        }
    }
}