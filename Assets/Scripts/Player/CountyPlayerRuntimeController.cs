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
        private TrafficCitationBook citationBook;
        private TrafficDriverHistoryBook driverHistoryBook;
        private ArrestBookingBook arrestBookingBook;
        private Transform worldRoot;
        private CivilianCharacter escortedPrisoner;
        private string escortedBookingId;
        private Vector3 escortedPrisonerOriginalScale;
        private Collider escortedPrisonerCollider;
        private bool escortedColliderWasEnabled;
        private bool prisonerSeated;
        private readonly Vector3 jailIntakePosition = new Vector3(-102f, 0f, -15f);
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
        private MobilePerformanceBootstrap performanceSettings;
        private bool settingsOpen;
        private MobileTouchControls touchControls;
        private bool touchMoveLeft, touchMoveRight, touchMoveUp, touchMoveDown, touchSprint;
        private bool touchSteerLeft, touchSteerRight, touchGas, touchBrake;

        private float UiWidth => Screen.safeArea.width;
        private float UiHeight => Screen.safeArea.height;
        private float hForTransport(float scale) => Mathf.Max(54f, 58f * scale);

        public void Initialize(Transform worldRoot)
        {
            this.worldRoot = worldRoot;
            performanceSettings = worldRoot.GetComponent<MobilePerformanceBootstrap>();
            dutyState = gameObject.AddComponent<CountyDutyState>();
            reportBook = worldRoot.GetComponent<IncidentReportBook>();
            citationBook = worldRoot.GetComponent<TrafficCitationBook>();
            driverHistoryBook = worldRoot.GetComponent<TrafficDriverHistoryBook>();
            arrestBookingBook = worldRoot.GetComponent<ArrestBookingBook>();
            interactions = worldRoot.GetComponent<OfficerInteractionController>();
            if (interactions != null)
            {
                interactions.ConfigureDutyState(dutyState);
                interactions.ArrestCompleted += HandleArrestCompleted;
            }
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
            patrolVehicle.AddComponent<PatrolSuvVisuals>().Build();
            vehicle = patrolVehicle.AddComponent<PatrolVehicleController>();
            vehicle.SetControlsEnabled(false);

            Renderer redLight = CreateLightbarSegment("Red Emergency Light", new Vector3(-.42f,1.13f,-.12f),
                new Vector3(.62f,.13f,.34f), new Color(.95f,.04f,.04f));
            Renderer blueLight = CreateLightbarSegment("Blue Emergency Light", new Vector3(.42f,1.13f,-.12f),
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

            GameObject touchObject = new GameObject("Gameplay Touch Controls");
            touchObject.transform.SetParent(worldRoot, false);
            touchControls = touchObject.AddComponent<MobileTouchControls>();
            touchControls.ControlChanged += HandleTouchControlChanged;
            touchControls.ActionTriggered += HandleHudAction;
            touchControls.SetVehicleMode(false);
            touchControls.SetHudState(dutyState != null && dutyState.IsOnDuty, inVehicle);
        }

        private Transform MakeAnchor(Transform parent, string anchorName, Vector3 localPosition)
        {
            var anchor = new GameObject(anchorName).transform;
            anchor.SetParent(parent, false);
            anchor.localPosition = localPosition;
            anchor.localRotation = anchorName.Contains("Third Person") ? Quaternion.Euler(12f,0f,0f) : Quaternion.identity;
            return anchor;
        }

        private void OnDestroy()
        {
            if (interactions != null) interactions.ArrestCompleted -= HandleArrestCompleted;
            if (touchControls != null)
            {
                touchControls.ControlChanged -= HandleTouchControlChanged;
                touchControls.ActionTriggered -= HandleHudAction;
                touchControls.ReleaseAll();
            }
        }

        private void OnEnable()
        {
            if (touchControls != null) touchControls.SetControlsVisible(!settingsOpen);
        }

        private void OnDisable()
        {
            if (touchControls != null) touchControls.SetControlsVisible(false);
            if (touchControls != null) touchControls.ReleaseAll();
            ClearTouchState();
            moveInput = Vector2.zero;
            throttle = 0f;
            steering = 0f;
            if (walker != null)
            {
                walker.SetMoveInput(Vector2.zero);
                walker.SetSprint(false);
            }
            if (vehicle != null)
            {
                vehicle.SetThrottle(0f);
                vehicle.SetSteering(0f);
            }
        }

        private void Update()
        {
            RefreshCivilianFocus();
            RefreshTrafficTarget();
            UpdatePrisonerEscort();
            if (touchControls != null)
            {
                touchControls.SetHudState(dutyState != null && dutyState.IsOnDuty, inVehicle);
                touchControls.SetCivilianContext(!inVehicle && focusedCivilian != null && interactions != null,
                    focusedCivilian != null ? focusedCivilian.FullName : null);
                bool trafficVisible = inVehicle && (activeTrafficStop != null ||
                    (nearbyTrafficVehicle != null && dutyState != null && dutyState.IsOnDuty));
                string trafficHeader = activeTrafficStop != null
                    ? (activeTrafficStop.IsPulledOver ? "TRAFFIC STOP" : "VEHICLE PULLING OVER")
                    : "TRAFFIC CONTACT";
                string trafficDetails = string.Empty;
                System.Collections.Generic.List<string> trafficActions = new System.Collections.Generic.List<string>();
                if (activeTrafficStop == null)
                {
                    if (trafficVisible) trafficActions.Add("traffic_pull_over");
                }
                else
                {
                    if (activeTrafficStop.IsPulledOver && activeDriverProfile != null)
                    {
                        trafficDetails = "DRIVER: " + activeDriverProfile.DriverName +
                            "\nPLATE: " + activeDriverProfile.PlateNumber;
                        if (activeDriverBehavior != null)
                        {
                            trafficDetails += "\nBEHAVIOR: " + activeDriverBehavior.Disposition.ToString().ToUpperInvariant() +
                                "\n" + activeDriverBehavior.LatestResponse;
                            if (!activeDriverBehavior.OfficerSpoke)
                            {
                                trafficActions.Add("traffic_calm");
                                trafficActions.Add("traffic_firm");
                            }
                        }
                        if (!activeDriverProfile.LicenseChecked) trafficActions.Add("traffic_license");
                        if (!activeDriverProfile.RegistrationChecked) trafficActions.Add("traffic_registration");
                        if (!activeDriverProfile.InsuranceChecked) trafficActions.Add("traffic_insurance");
                        if (activeDriverProfile.AllDocumentsChecked)
                        {
                            trafficDetails += activeDriverProfile.HasViolation
                                ? "\nDOCUMENT ISSUE FOUND"
                                : "\nALL DOCUMENTS VALID";
                            if (activeDriverProfile.HasViolation && !trafficStopDecisionRecorded)
                            {
                                trafficActions.Add("traffic_warning");
                                trafficActions.Add("traffic_citation");
                            }
                            else if (trafficStopDecisionRecorded)
                                trafficDetails += "\nSTOP DECISION RECORDED";
                        }
                    }
                    trafficActions.Add("traffic_end");
                }
                touchControls.SetTrafficContext(trafficVisible, trafficHeader, trafficDetails, trafficActions.ToArray());
            }
            if (walker && !inVehicle)
            {
                walker.SetMoveInput(moveInput);
                walker.SetSprint(touchSprint && !settingsOpen);
            }
            if (vehicle)
            {
                vehicle.SetThrottle(inVehicle ? throttle : 0f);
                vehicle.SetSteering(inVehicle ? steering : 0f);
            }
        }

        private void OnGUI()
        {
            Rect safeArea = Screen.safeArea;
            Matrix4x4 previousGuiMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Translate(new Vector3(
                safeArea.xMin, Screen.height - safeArea.yMax, 0f)) * previousGuiMatrix;

            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 18, wordWrap = true };
                statusStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, normal = { textColor = Color.white } };
            }
            // Scale against both safe-area dimensions so short landscape screens do not
            // inherit oversized portrait controls. Keep touch targets at least 54 GUI points.
            float widthScale = UiWidth / 390f;
            float heightScale = UiHeight / 640f;
            float s = Mathf.Clamp(Mathf.Min(widthScale, heightScale), .8f, 1.25f);
            buttonStyle.fontSize = Mathf.RoundToInt(18f * s);
            statusStyle.fontSize = Mathf.RoundToInt(18f * s);
            float pad = 12f*s;
            if (settingsOpen)
            {
                if (touchControls != null) touchControls.SetControlsVisible(false);
                moveInput = Vector2.zero;
                throttle = 0f;
                steering = 0f;
                if (walker != null) walker.SetSprint(false);
                DrawSettingsPanel(s);
                GUI.matrix = previousGuiMatrix;
                return;
            }
            if (touchControls != null) touchControls.SetControlsVisible(true);
            DrawPrisonerTransportHud(s, pad);

            GUI.matrix = previousGuiMatrix;
        }

        private void HandleHudAction(string action)
        {
            switch (action)
            {
                case "vehicle":
                    ToggleVehicle();
                    break;
                case "duty":
                    if (dutyState != null) dutyState.ToggleDuty();
                    break;
                case "camera":
                    if (cameraModes != null) cameraModes.ToggleCameraMode();
                    break;
                case "settings":
                    settingsOpen = !settingsOpen;
                    if (touchControls != null) touchControls.SetControlsVisible(!settingsOpen);
                    break;
                case "civilian_greet": if (interactions != null) interactions.Greet(); break;
                case "civilian_questions": if (interactions != null) interactions.AskQuestions(); break;
                case "civilian_id": if (interactions != null) interactions.RequestIdentification(); break;
                case "civilian_warn": if (interactions != null) interactions.IssueWarning(); break;
                case "civilian_cite": if (interactions != null) interactions.IssueCitation(); break;
                case "civilian_detain": if (interactions != null) interactions.Detain(); break;
                case "civilian_arrest": if (interactions != null) interactions.Arrest(); break;
                case "civilian_release": if (interactions != null) interactions.Release(); break;
                case "traffic_pull_over": BeginTrafficStop(); break;
                case "traffic_calm": RespondToDriver(true); break;
                case "traffic_firm": RespondToDriver(false); break;
                case "traffic_license": CheckTrafficLicense(); break;
                case "traffic_registration": CheckTrafficRegistration(); break;
                case "traffic_insurance": CheckTrafficInsurance(); break;
                case "traffic_warning": ResolveTrafficStop(false); break;
                case "traffic_citation": ResolveTrafficStop(true); break;
                case "traffic_end": EndTrafficStop(); break;
            }
        }

        private void HandleTouchControlChanged(string control, bool pressed)
        {
            switch (control)
            {
                case "move_left": touchMoveLeft = pressed; break;
                case "move_right": touchMoveRight = pressed; break;
                case "move_up": touchMoveUp = pressed; break;
                case "move_down": touchMoveDown = pressed; break;
                case "sprint": touchSprint = pressed; break;
                case "steer_left": touchSteerLeft = pressed; break;
                case "steer_right": touchSteerRight = pressed; break;
                case "gas": touchGas = pressed; break;
                case "brake": touchBrake = pressed; break;
            }
            RefreshTouchInputs();
        }

        private void RefreshTouchInputs()
        {
            moveInput = inVehicle ? Vector2.zero : new Vector2(
                (touchMoveRight ? 1f : 0f) - (touchMoveLeft ? 1f : 0f),
                (touchMoveUp ? 1f : 0f) - (touchMoveDown ? 1f : 0f));
            if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();
            steering = inVehicle
                ? (touchSteerRight ? 1f : 0f) - (touchSteerLeft ? 1f : 0f)
                : 0f;
            throttle = inVehicle
                ? (touchGas ? 1f : 0f) - (touchBrake ? 1f : 0f)
                : 0f;
            if (walker != null) walker.SetSprint(touchSprint && !inVehicle && !settingsOpen);
        }

        private void ClearTouchState()
        {
            touchMoveLeft = touchMoveRight = touchMoveUp = touchMoveDown = touchSprint = false;
            touchSteerLeft = touchSteerRight = touchGas = touchBrake = false;
            RefreshTouchInputs();
        }

        private void DrawSettingsPanel(float scale)
        {
            scale = Mathf.Min(scale, Mathf.Max(.6f, (UiHeight - 20f) / 360f));
            float width = 350f * scale;
            float height = 360f * scale;
            float x = (UiWidth - width) * .5f;
            float y = (UiHeight - height) * .5f;
            GUI.Box(new Rect(x, y, width, height), "GAME SETTINGS");

            GUI.Label(new Rect(x + 14f * scale, y + 34f * scale, width - 28f * scale, 24f * scale),
                "GRAPHICS QUALITY", statusStyle);
            float qualityY = y + 62f * scale;
            float qualityGap = 5f * scale;
            float qualityWidth = (width - 38f * scale - qualityGap * 2f) / 3f;
            DrawQualityButton(x + 14f * scale, qualityY, qualityWidth, "PERFORMANCE",
                MobileVisualQualityPreset.Performance, scale);
            DrawQualityButton(x + 19f * scale + qualityWidth, qualityY, qualityWidth, "BALANCED",
                MobileVisualQualityPreset.Balanced, scale);
            DrawQualityButton(x + 24f * scale + qualityWidth * 2f, qualityY, qualityWidth, "HIGH",
                MobileVisualQualityPreset.High, scale);

            GUI.Label(new Rect(x + 14f * scale, y + 112f * scale, width - 28f * scale, 24f * scale),
                "FRAME RATE", statusStyle);
            float buttonY = y + 142f * scale;
            float buttonWidth = (width - 40f * scale) * .5f;
            DrawFrameRateButton(x + 14f * scale, buttonY, buttonWidth, "30 FPS", MobileFrameRateMode.Fps30, scale);
            DrawFrameRateButton(x + 26f * scale + buttonWidth, buttonY, buttonWidth, "60 FPS", MobileFrameRateMode.Fps60, scale);
            DrawFrameRateButton(x + 14f * scale, buttonY + 46f * scale, buttonWidth, "120 FPS", MobileFrameRateMode.Fps120, scale);
            DrawFrameRateButton(x + 26f * scale + buttonWidth, buttonY + 46f * scale, buttonWidth, "UNLIMITED", MobileFrameRateMode.Unlimited, scale);

            string quality = performanceSettings != null
                ? "GRAPHICS: " + performanceSettings.CurrentPreset.ToString().ToUpperInvariant()
                : "GRAPHICS SETTINGS UNAVAILABLE";
            string frameRate = performanceSettings != null
                ? performanceSettings.CurrentFrameRateMode == MobileFrameRateMode.Unlimited
                    ? "FRAME RATE: UNLIMITED"
                    : "FRAME RATE: " + (int)performanceSettings.CurrentFrameRateMode + " FPS"
                : "FRAME RATE SETTINGS UNAVAILABLE";
            GUI.Label(new Rect(x + 14f * scale, y + 240f * scale, width - 28f * scale, 22f * scale),
                quality, statusStyle);
            GUI.Label(new Rect(x + 14f * scale, y + 264f * scale, width - 28f * scale, 22f * scale),
                frameRate, statusStyle);

            string performanceReadout = performanceSettings != null &&
                performanceSettings.MeasuredFramesPerSecond > 0f
                ? "LIVE: " + performanceSettings.MeasuredFramesPerSecond.ToString("0") +
                  " FPS  /  " + performanceSettings.MeasuredFrameTimeMilliseconds.ToString("0.0") + " ms"
                : "LIVE: Measuring frame performance...";
            GUI.Label(new Rect(x + 14f * scale, y + 290f * scale, width - 130f * scale, 22f * scale),
                performanceReadout, statusStyle);

            if (GUI.Button(new Rect(x + width - 104f * scale, y + height - 46f * scale,
                90f * scale, 32f * scale), "CLOSE", buttonStyle))
                settingsOpen = false;
        }

        private void DrawQualityButton(float x, float y, float width, string label,
            MobileVisualQualityPreset preset, float scale)
        {
            bool selected = performanceSettings != null && performanceSettings.CurrentPreset == preset;
            string caption = selected ? "SELECTED\n" + label : label;
            if (GUI.Button(new Rect(x, y, width, 42f * scale), caption, buttonStyle) &&
                performanceSettings != null)
                performanceSettings.SetPreset(preset);
        }

        private void DrawFrameRateButton(float x, float y, float width, string label,
            MobileFrameRateMode mode, float scale)
        {
            bool selected = performanceSettings != null && performanceSettings.CurrentFrameRateMode == mode;
            string caption = selected ? "SELECTED: " + label : label;
            if (GUI.Button(new Rect(x, y, width, 40f * scale), caption, buttonStyle) &&
                performanceSettings != null)
                performanceSettings.SetFrameRateMode(mode);
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

        private void HandleArrestCompleted(CivilianCharacter civilian)
        {
            if (civilian == null || arrestBookingBook == null) return;
            if (escortedPrisoner != null && escortedPrisoner != civilian)
            {
                Debug.LogWarning("Only one prisoner can be escorted at a time in this prototype.");
                return;
            }

            ArrestBookingRecord booking = null;
            foreach (ArrestBookingRecord record in arrestBookingBook.Records)
            {
                if (record != null && record.status == BookingStatus.AwaitingTransport &&
                    record.identificationNumber == civilian.IdentificationNumber)
                {
                    booking = record;
                    break;
                }
            }
            if (booking == null) return;

            escortedPrisoner = civilian;
            escortedBookingId = booking.bookingId;
            escortedPrisonerOriginalScale = civilian.transform.localScale;
            escortedPrisonerCollider = civilian.GetComponent<Collider>();
            escortedColliderWasEnabled = escortedPrisonerCollider != null && escortedPrisonerCollider.enabled;
            CivilianRoutineAgent routine = civilian.GetComponent<CivilianRoutineAgent>();
            if (routine != null) routine.enabled = false;
            prisonerSeated = false;
            Debug.Log("Prisoner escort started: " + escortedBookingId);
        }

        private void UpdatePrisonerEscort()
        {
            if (escortedPrisoner == null) return;

            if (inVehicle)
            {
                if (!prisonerSeated) SeatEscortedPrisoner();
                return;
            }

            if (prisonerSeated) UnseatEscortedPrisoner();
            if (player == null) return;

            Vector3 followPosition = player.transform.position -
                player.transform.forward * 1.35f + player.transform.right * .45f;
            followPosition.y = escortedPrisoner.transform.position.y;
            escortedPrisoner.transform.position = Vector3.MoveTowards(
                escortedPrisoner.transform.position, followPosition, 1.8f * Time.deltaTime);

            Vector3 direction = player.transform.position - escortedPrisoner.transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > .05f)
                escortedPrisoner.transform.rotation = Quaternion.RotateTowards(
                    escortedPrisoner.transform.rotation,
                    Quaternion.LookRotation(direction), 240f * Time.deltaTime);
        }

        private void SeatEscortedPrisoner()
        {
            if (escortedPrisoner == null || patrolVehicle == null) return;
            escortedPrisoner.transform.SetParent(patrolVehicle.transform, false);
            escortedPrisoner.transform.localPosition = new Vector3(.58f, .88f, .12f);
            escortedPrisoner.transform.localRotation = Quaternion.identity;
            escortedPrisoner.transform.localScale = escortedPrisonerOriginalScale * .68f;
            if (escortedPrisonerCollider != null) escortedPrisonerCollider.enabled = false;
            prisonerSeated = true;
        }

        private void UnseatEscortedPrisoner()
        {
            if (escortedPrisoner == null) return;
            escortedPrisoner.transform.SetParent(worldRoot, true);
            escortedPrisoner.transform.position = player.transform.position +
                player.transform.right * 1.1f;
            escortedPrisoner.transform.localScale = escortedPrisonerOriginalScale;
            if (escortedPrisonerCollider != null) escortedPrisonerCollider.enabled = escortedColliderWasEnabled;
            prisonerSeated = false;
        }

        private void DrawPrisonerTransportHud(float scale, float padding)
        {
            if (!inVehicle || arrestBookingBook == null || dutyState == null ||
                !dutyState.IsOnDuty || escortedPrisoner == null ||
                string.IsNullOrEmpty(escortedBookingId))
                return;

            ArrestBookingRecord pending = null;
            foreach (ArrestBookingRecord record in arrestBookingBook.Records)
            {
                if (record != null && record.bookingId == escortedBookingId &&
                    record.status == BookingStatus.AwaitingTransport)
                {
                    pending = record;
                    break;
                }
            }
            if (pending == null) return;

            float distance = Vector3.Distance(patrolVehicle.transform.position, jailIntakePosition);
            // Use a compact center-bottom strip above the vehicle controls. This avoids
            // the traffic-stop document column and the driver's contact/action column.
            float panelWidth = Mathf.Min(Mathf.Max(180f, UiWidth * .72f), UiWidth - padding * 2f);
            float panelHeight = 42f * scale;
            float x = (UiWidth - panelWidth) * .5f;
            float y = UiHeight - hForTransport(scale) - padding - panelHeight - 8f * scale;
            float actionWidth = Mathf.Min(112f * scale, panelWidth * .42f);
            bool atIntake = distance <= 18f;
            string transportStatus = "INTAKE " + distance.ToString("0") + "m  •  " +
                pending.personName;
            GUI.Label(new Rect(x, y, panelWidth - actionWidth - 6f * scale, panelHeight),
                transportStatus, statusStyle);
            if (atIntake && GUI.Button(new Rect(x + panelWidth - actionWidth, y,
                actionWidth, panelHeight), "BOOK INTAKE", buttonStyle))
                CompletePrisonerTransport(pending);
        }

        private void CompletePrisonerTransport(ArrestBookingRecord booking)
        {
            if (!inVehicle || booking == null || escortedPrisoner == null ||
                booking.bookingId != escortedBookingId || arrestBookingBook == null ||
                dutyState == null || !dutyState.IsOnDuty ||
                Vector3.Distance(patrolVehicle.transform.position, jailIntakePosition) > 18f)
                return;

            if (!arrestBookingBook.MarkBooked(booking.bookingId)) return;
            if (escortedPrisoner != null && escortedBookingId == booking.bookingId)
            {
                escortedPrisoner.transform.SetParent(worldRoot, true);
                escortedPrisoner.transform.position = jailIntakePosition + new Vector3(0f, 1f, 5f);
                escortedPrisoner.transform.localScale = escortedPrisonerOriginalScale;
                if (escortedPrisonerCollider != null) escortedPrisonerCollider.enabled = escortedColliderWasEnabled;
                escortedPrisoner = null;
                escortedPrisonerCollider = null;
                escortedBookingId = null;
                prisonerSeated = false;
            }
            if (reportBook != null)
                reportBook.AddReport("Jail intake", booking.personName, "Prisoner transport",
                    "Transport completed at Marion County jail intake. Booking " + booking.bookingId + " received.",
                    "Fictional gameplay custody transfer recorded at the Ocala greybox jail intake.");
            vehicle.StopImmediately();
            Debug.Log("Prisoner transport completed: " + booking.bookingId);
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
            if (!issueCitation && driverHistoryBook != null)
                driverHistoryBook.RecordWarning(activeDriverProfile, outcome);
            if (issueCitation && citationBook != null)
            {
                Vector3 stopPosition = activeTrafficStop.transform.position;
                string location = "Marion County road, grid X " + stopPosition.x.ToString("0") +
                    " / Z " + stopPosition.z.ToString("0");
                TrafficCitation citation = citationBook.Issue(activeDriverProfile, location, "OCALA-1");
                if (citation != null)
                {
                    if (driverHistoryBook != null) driverHistoryBook.RecordCitation(activeDriverProfile, citation);
                    RecordTrafficStopAction("Citation " + citation.citationNumber,
                        "Citation created for $" + citation.fineAmount + ". " + citation.violations);
                }
            }
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

        private void ToggleVehicle()
        {
            if (!inVehicle)
            {
                if (Vector3.Distance(player.transform.position, patrolVehicle.transform.position) > 5f) return;
                inVehicle = true;
                if (touchControls != null) touchControls.SetVehicleMode(true);
                ClearTouchState();
                characterController.enabled = false;
                walker.SetMovementEnabled(false);
                player.GetComponent<Renderer>().enabled = false;
                vehicle.SetControlsEnabled(true);
                cameraModes.Configure(Camera.main, vehicleThird, vehicleFirst);
                SeatEscortedPrisoner();
            }
            else
            {
                // Do not eject the player into a wall, another vehicle, or an occupied space.
                // If every candidate is blocked, remain seated and let the player reposition.
                if (!TryFindSafeExitPosition(out Vector3 exitPosition)) return;

                vehicle.StopImmediately();
                vehicle.SetControlsEnabled(false);
                inVehicle = false;
                if (touchControls != null) touchControls.SetVehicleMode(false);
                ClearTouchState();
                player.transform.SetPositionAndRotation(exitPosition, patrolVehicle.transform.rotation);
                player.GetComponent<Renderer>().enabled = true;
                characterController.enabled = true;
                walker.SetMovementEnabled(true);
                cameraModes.Configure(Camera.main, footThird, footFirst);
                UnseatEscortedPrisoner();
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