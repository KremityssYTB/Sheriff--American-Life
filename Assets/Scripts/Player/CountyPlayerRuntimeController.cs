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
        private CountyEmergencyLightbar lightbar;
        private CountyTrafficVehicleAgent nearbyTrafficVehicle;
        private CountyTrafficVehicleAgent activeTrafficStop;
        private TrafficStopDriverProfile activeDriverProfile;
        private TrafficStopDriverBehavior activeDriverBehavior;
        private IncidentReportBook reportBook;
        private bool trafficStopDecisionRecorded;
        private readonly Collider[] trafficBuffer = new Collider[48];
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
            reportBook = worldRoot.GetComponent<IncidentReportBook>();
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

            Renderer redLight = CreateLightbarSegment("Red Emergency Light", new Vector3(-.42f,.79f,0f),
                new Vector3(.62f,.13f,.34f), new Color(.95f,.04f,.04f));
            Renderer blueLight = CreateLightbarSegment("Blue Emergency Light", new Vector3(.42f,.79f,0f),
                new Vector3(.62f,.13f,.34f), new Color(.04f,.18f,1f));
            lightbar = patrolVehicle.AddComponent<CountyEmergencyLightbar>();
            lightbar.Configure(redLight, blueLight, 3f);

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
            RefreshTrafficTarget();
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
            DrawTrafficStopHud(s, pad);

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

        private Renderer CreateLightbarSegment(string segmentName, Vector3 localPosition,
            Vector3 localScale, Color color)
        {
            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.name = segmentName;
            segment.transform.SetParent(patrolVehicle.transform, false);
            segment.transform.localPosition = localPosition;
            segment.transform.localScale = localScale;
            Collider segmentCollider = segment.GetComponent<Collider>();
            if (segmentCollider != null) Destroy(segmentCollider);

            Renderer segmentRenderer = segment.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader);
            material.color = color;
            segmentRenderer.sharedMaterial = material;
            return segmentRenderer;
        }

        private void RefreshTrafficTarget()
        {
            if (!inVehicle || patrolVehicle == null || activeTrafficStop != null ||
                dutyState == null || !dutyState.IsOnDuty)
            {
                nearbyTrafficVehicle = null;
                return;
            }

            int count = Physics.OverlapSphereNonAlloc(
                patrolVehicle.transform.position,
                24f,
                trafficBuffer,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            CountyTrafficVehicleAgent nearest = null;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                Collider candidateCollider = trafficBuffer[i];
                if (candidateCollider == null) continue;
                CountyTrafficVehicleAgent candidate =
                    candidateCollider.GetComponentInParent<CountyTrafficVehicleAgent>();
                if (candidate == null || candidate.HasActiveTrafficStop) continue;

                float distance = Vector3.Distance(
                    patrolVehicle.transform.position, candidate.transform.position);
                if (distance < nearestDistance)
                {
                    nearest = candidate;
                    nearestDistance = distance;
                }
            }

            nearbyTrafficVehicle = nearest;
        }

        private void DrawTrafficStopHud(float scale, float padding)
        {
            if (!inVehicle) return;

            float x = Screen.width - 112f * scale;
            float y = padding + 150f * scale;
            if (activeTrafficStop != null)
            {
                GUI.Label(new Rect(padding, padding + 64f * scale, 280f * scale, 28f * scale),
                    activeTrafficStop.IsPulledOver ? "VEHICLE STOPPED" : "VEHICLE PULLING OVER",
                    statusStyle);

                if (activeTrafficStop.IsPulledOver && activeDriverProfile != null)
                {
                    GUI.Label(new Rect(padding, padding + 92f * scale, 300f * scale, 24f * scale),
                        "DRIVER: " + activeDriverProfile.DriverName, statusStyle);
                    GUI.Label(new Rect(padding, padding + 116f * scale, 300f * scale, 24f * scale),
                        "PLATE: " + activeDriverProfile.PlateNumber, statusStyle);

                    if (activeDriverBehavior != null)
                    {
                        GUI.Label(new Rect(padding, padding + 144f * scale, 300f * scale, 24f * scale),
                            "DRIVER: " + activeDriverBehavior.Disposition.ToString().ToUpperInvariant(), statusStyle);
                        GUI.Label(new Rect(padding, padding + 168f * scale, 300f * scale, 38f * scale),
                            activeDriverBehavior.LatestResponse, statusStyle);
                        if (!activeDriverBehavior.OfficerSpoke)
                        {
                            if (GUI.Button(new Rect(padding, padding + 208f * scale, 104f * scale, 36f * scale),
                                "CALM APPROACH", buttonStyle)) RespondToDriver(true);
                            if (GUI.Button(new Rect(padding + 108f * scale, padding + 208f * scale, 104f * scale, 36f * scale),
                                "FIRM APPROACH", buttonStyle)) RespondToDriver(false);
                        }
                    }

                    float checkY = padding + 250f * scale;
                    if (!activeDriverProfile.LicenseChecked &&
                        GUI.Button(new Rect(x, checkY, 112f * scale, 38f * scale), "CHECK LICENSE", buttonStyle))
                        CheckTrafficLicense();
                    checkY += 43f * scale;
                    if (!activeDriverProfile.RegistrationChecked &&
                        GUI.Button(new Rect(x, checkY, 112f * scale, 38f * scale), "CHECK REG", buttonStyle))
                        CheckTrafficRegistration();
                    checkY += 43f * scale;
                    if (!activeDriverProfile.InsuranceChecked &&
                        GUI.Button(new Rect(x, checkY, 112f * scale, 38f * scale), "CHECK INSURANCE", buttonStyle))
                        CheckTrafficInsurance();

                    if (activeDriverProfile.AllDocumentsChecked)
                    {
                        string result = activeDriverProfile.HasViolation ? "DOCUMENT ISSUE FOUND" : "ALL DOCUMENTS VALID";
                        GUI.Label(new Rect(padding, padding + 250f * scale, 300f * scale, 26f * scale),
                            result, statusStyle);
                        if (!trafficStopDecisionRecorded && activeDriverProfile.HasViolation)
                        {
                            if (GUI.Button(new Rect(x, padding + 332f * scale, 112f * scale, 38f * scale),
                                "WARNING", buttonStyle)) ResolveTrafficStop(false);
                            if (GUI.Button(new Rect(x, padding + 375f * scale, 112f * scale, 38f * scale),
                                "CITATION", buttonStyle)) ResolveTrafficStop(true);
                        }
                        else if (!trafficStopDecisionRecorded)
                        {
                            GUI.Label(new Rect(padding, padding + 276f * scale, 300f * scale, 24f * scale),
                                "No document violation found.", statusStyle);
                        }
                        else
                        {
                            GUI.Label(new Rect(padding, padding + 276f * scale, 300f * scale, 24f * scale),
                                "STOP DECISION RECORDED", statusStyle);
                        }
                    }
                }

                if (GUI.Button(new Rect(x, Screen.height - 112f * scale, 112f * scale, 42f * scale),
                    activeTrafficStop.IsPulledOver ? "END STOP" : "CANCEL STOP", buttonStyle))
                    EndTrafficStop();
            }
            else if (nearbyTrafficVehicle != null && dutyState != null && dutyState.IsOnDuty)
            {
                if (GUI.Button(new Rect(x, y, 100f * scale, 42f * scale), "PULL OVER", buttonStyle))
                    BeginTrafficStop();
            }
        }

        private void BeginTrafficStop()
        {
            if (!inVehicle || nearbyTrafficVehicle == null || lightbar == null) return;
            Vector3 shoulder = nearbyTrafficVehicle.transform.position +
                nearbyTrafficVehicle.transform.right * 4.5f;
            if (!nearbyTrafficVehicle.RequestTrafficStop(shoulder)) return;

            activeTrafficStop = nearbyTrafficVehicle;
            activeDriverProfile = activeTrafficStop.GetComponent<TrafficStopDriverProfile>();
            activeDriverBehavior = activeTrafficStop.GetComponent<TrafficStopDriverBehavior>();
            if (activeDriverProfile != null) activeDriverProfile.ResetChecks();
            trafficStopDecisionRecorded = false;
            nearbyTrafficVehicle = null;
            lightbar.SetLightbarEnabled(true);
        }

        private void RespondToDriver(bool calmApproach)
        {
            if (activeTrafficStop == null || !activeTrafficStop.IsPulledOver ||
                activeDriverBehavior == null || dutyState == null || !dutyState.IsOnDuty ||
                activeDriverBehavior.OfficerSpoke) return;

            string response = activeDriverBehavior.RespondToOfficer(calmApproach);
            RecordTrafficStopAction(calmApproach ? "Calm driver contact" : "Firm driver contact", response);
            Debug.Log(response);
        }

        private void CheckTrafficLicense()
        {
            if (!CanProcessTrafficStop()) return;
            string result = activeDriverProfile.CheckLicense();
            RecordTrafficStopAction("License check", result);
            Debug.Log(result);
        }

        private void CheckTrafficRegistration()
        {
            if (!CanProcessTrafficStop()) return;
            string result = activeDriverProfile.CheckRegistration();
            RecordTrafficStopAction("Registration check", result);
            Debug.Log(result);
        }

        private void CheckTrafficInsurance()
        {
            if (!CanProcessTrafficStop()) return;
            string result = activeDriverProfile.CheckInsurance();
            RecordTrafficStopAction("Insurance check", result);
            Debug.Log(result);
        }

        private bool CanProcessTrafficStop()
        {
            return activeTrafficStop != null && activeTrafficStop.IsPulledOver &&
                activeDriverProfile != null && dutyState != null && dutyState.IsOnDuty;
        }

        private void ResolveTrafficStop(bool issueCitation)
        {
            if (!CanProcessTrafficStop() || !activeDriverProfile.AllDocumentsChecked ||
                !activeDriverProfile.HasViolation || trafficStopDecisionRecorded) return;

            string action = issueCitation ? "Citation issued" : "Verbal warning";
            string outcome = issueCitation
                ? "Citation issued after document discrepancy review."
                : "Verbal warning issued; driver advised to correct document issue.";
            RecordTrafficStopAction(action, outcome);
            trafficStopDecisionRecorded = true;
        }

        private void RecordTrafficStopAction(string action, string outcome)
        {
            if (reportBook == null || activeDriverProfile == null) return;
            reportBook.AddReport("Traffic stop", activeDriverProfile.DriverName, action, outcome,
                "Vehicle plate " + activeDriverProfile.PlateNumber +
                ". Driver license: " + activeDriverProfile.LicenseStatus +
                ". Registration: " + activeDriverProfile.RegistrationStatus +
                ". Insurance: " + activeDriverProfile.InsuranceStatus + ".");
        }

        private void EndTrafficStop()
        {
            if (activeTrafficStop != null) activeTrafficStop.ResumeTraffic();
            activeTrafficStop = null;
            activeDriverProfile = null;
            activeDriverBehavior = null;
            trafficStopDecisionRecorded = false;
            if (lightbar != null) lightbar.SetLightbarEnabled(false);
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