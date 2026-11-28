using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SheriffAmericanLife
{
    /// <summary>Runtime-generated held touch controls for deputy movement and patrol driving.</summary>
    public sealed class MobileTouchControls : MonoBehaviour
    {
        private readonly Dictionary<string, MobileHoldButton> buttons = new Dictionary<string, MobileHoldButton>();
        private readonly Dictionary<string, TextMeshProUGUI> actionLabels = new Dictionary<string, TextMeshProUGUI>();
        private readonly Dictionary<string, Button> contextButtons = new Dictionary<string, Button>();
        private GameObject civilianPanel;
        private TextMeshProUGUI civilianHeader;
        private GameObject trafficPanel;
        private TextMeshProUGUI trafficHeader;
        private TextMeshProUGUI trafficDetails;
        private readonly Dictionary<string, Button> trafficButtons = new Dictionary<string, Button>();
        private GameObject transportPanel;
        private TextMeshProUGUI transportDetails;
        private Button transportButton;
        private Canvas canvas;
        private RectTransform safeRoot;
        private Rect lastSafeArea;
        private bool vehicleMode;

        public event Action<string, bool> ControlChanged;
        public event Action<string> ActionTriggered;

        public void SetTransportContext(bool visible, string details, bool canBook)
        {
            if (transportPanel == null) return;
            transportPanel.SetActive(visible);
            if (!visible) return;
            if (transportDetails != null) transportDetails.text = details ?? "PRISONER TRANSPORT";
            if (transportButton != null)
            {
                transportButton.gameObject.SetActive(canBook);
                TextMeshProUGUI label = transportButton.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = "BOOK INTAKE";
            }
        }

        public void SetTrafficContext(bool visible, string header, string details, string[] visibleActions)
        {
            if (trafficPanel == null) return;
            trafficPanel.SetActive(visible);
            if (!visible) return;
            if (trafficHeader != null) trafficHeader.text = header ?? "TRAFFIC STOP";
            if (trafficDetails != null) trafficDetails.text = details ?? string.Empty;
            foreach (KeyValuePair<string, Button> pair in trafficButtons)
                pair.Value.gameObject.SetActive(false);
            if (visibleActions == null) return;
            for (int i = 0; i < visibleActions.Length; i++)
            {
                if (!trafficButtons.TryGetValue(visibleActions[i], out Button button)) continue;
                RectTransform rect = button.GetComponent<RectTransform>();
                int column = i % 2;
                int row = i / 2;
                float xMin = .04f + column * .48f;
                float xMax = xMin + .44f;
                float yMax = .48f - row * .11f;
                rect.anchorMin = new Vector2(xMin, yMax - .10f);
                rect.anchorMax = new Vector2(xMax, yMax);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                button.gameObject.SetActive(true);
            }
        }

        public void SetCivilianContext(bool visible, string personName)
        {
            if (civilianPanel == null) return;
            civilianPanel.SetActive(visible);
            if (civilianHeader != null)
                civilianHeader.text = string.IsNullOrEmpty(personName) ? "CIVILIAN CONTACT" : "NEARBY: " + personName;
        }

        public void SetHudState(bool onDuty, bool inVehicle)
        {
            SetActionLabel("vehicle", inVehicle ? "EXIT VEHICLE" : "ENTER VEHICLE");
            SetActionLabel("duty", onDuty ? "GO OFF DUTY" : "GO ON DUTY");
            SetStatus("MARION COUNTY SHERIFF  |  " + (onDuty ? "ON DUTY" : "OFF DUTY"));
        }

        private void SetStatus(string value)
        {
            if (actionLabels.TryGetValue("status", out TextMeshProUGUI label) && label.text != value)
                label.text = value;
        }

        private void SetActionLabel(string id, string value)
        {
            if (actionLabels.TryGetValue(id, out TextMeshProUGUI label) && label.text != value)
                label.text = value;
        }

        private void Awake()
        {
            Build();
            SetVehicleMode(false);
        }

        private void OnDisable() { ReleaseAll(); }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) ReleaseAll();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) ReleaseAll();
        }

        private void Update()
        {
            ApplySafeArea();
        }

        public void SetVehicleMode(bool enabled)
        {
            vehicleMode = enabled;
            ReleaseAll();
            foreach (KeyValuePair<string, MobileHoldButton> pair in buttons)
                pair.Value.gameObject.SetActive(IsVehicleControl(pair.Key) == vehicleMode);
        }

        public void SetControlsVisible(bool visible)
        {
            if (canvas != null && canvas.enabled == visible) return;
            if (!visible) ReleaseAll();
            if (canvas != null) canvas.enabled = visible;
        }

        public void ReleaseAll()
        {
            foreach (MobileHoldButton button in buttons.Values)
                if (button != null) button.Release();
        }

        private bool IsVehicleControl(string id)
        {
            return id == "steer_left" || id == "steer_right" || id == "gas" || id == "brake";
        }

        private void Build()
        {
            if (EventSystem.current == null)
            {
                GameObject eventSystemObject = new GameObject("Gameplay Event System",
                    typeof(EventSystem), typeof(InputSystemUIInputModule));
                eventSystemObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            else if (EventSystem.current.GetComponent<BaseInputModule>() == null)
            {
                EventSystem.current.gameObject.AddComponent<InputSystemUIInputModule>()
                    .AssignDefaultActions();
            }

            GameObject canvasObject = new GameObject("Gameplay Touch Canvas", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(844f, 390f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject safeObject = new GameObject("Gameplay Touch Safe Area", typeof(RectTransform));
            safeObject.transform.SetParent(canvasObject.transform, false);
            safeRoot = safeObject.GetComponent<RectTransform>();
            ApplySafeArea();

            // Foot controls occupy the lower-left; sprint is separated on the lower-right.
            AddButton("move_left", "◀", new Vector2(.025f,.055f), new Vector2(.13f,.22f));
            AddButton("move_right", "▶", new Vector2(.285f,.055f), new Vector2(.39f,.22f));
            AddButton("move_up", "▲", new Vector2(.155f,.23f), new Vector2(.26f,.395f));
            AddButton("move_down", "▼", new Vector2(.155f,.055f), new Vector2(.26f,.22f));
            AddButton("sprint", "SPRINT", new Vector2(.30f,.25f), new Vector2(.46f,.40f));

            // Vehicle steering remains on the left; accelerator and brake are on the right.
            AddButton("steer_left", "LEFT", new Vector2(.025f,.055f), new Vector2(.15f,.22f));
            AddButton("steer_right", "RIGHT", new Vector2(.17f,.055f), new Vector2(.295f,.22f));
            AddButton("gas", "GAS", new Vector2(.84f,.12f), new Vector2(.975f,.30f));
            AddButton("brake", "BRAKE\nREV", new Vector2(.84f,.32f), new Vector2(.975f,.46f));

            AddActionButton("vehicle", "ENTER VEHICLE", new Vector2(.84f,.84f), new Vector2(.975f,.95f));
            AddActionButton("duty", "GO ON DUTY", new Vector2(.84f,.71f), new Vector2(.975f,.82f));
            AddActionButton("camera", "CAMERA", new Vector2(.84f,.58f), new Vector2(.975f,.69f));
            AddActionButton("settings", "SETTINGS", new Vector2(.84f,.48f), new Vector2(.975f,.59f));
            AddStatusLabel();\n            BuildCivilianPanel();\n            BuildTrafficPanel();\n            BuildTransportPanel();
        }


        private void BuildTransportPanel()
        {
            transportPanel = new GameObject("Prisoner Transport Panel", typeof(RectTransform), typeof(Image));
            transportPanel.transform.SetParent(safeRoot, false);
            RectTransform panelRect = transportPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(.20f, .12f);
            panelRect.anchorMax = new Vector2(.80f, .23f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            transportPanel.GetComponent<Image>().color = new Color(.025f, .04f, .055f, .92f);

            GameObject detailObject = new GameObject("Transport Details", typeof(RectTransform), typeof(TextMeshProUGUI));
            detailObject.transform.SetParent(transportPanel.transform, false);
            RectTransform detailRect = detailObject.GetComponent<RectTransform>();
            detailRect.anchorMin = new Vector2(.03f, .05f);
            detailRect.anchorMax = new Vector2(.62f, .95f);
            detailRect.offsetMin = new Vector2(3f, 2f);
            detailRect.offsetMax = new Vector2(-3f, -2f);
            transportDetails = detailObject.GetComponent<TextMeshProUGUI>();
            transportDetails.text = "PRISONER TRANSPORT";
            transportDetails.fontSize = 13f;
            transportDetails.enableAutoSizing = true;
            transportDetails.fontSizeMin = 8f;
            transportDetails.fontSizeMax = 13f;
            transportDetails.fontStyle = FontStyles.Bold;
            transportDetails.color = Color.white;
            transportDetails.alignment = TextAlignmentOptions.Center;
            transportDetails.raycastTarget = false;

            GameObject buttonObject = new GameObject("Book Intake", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(transportPanel.transform, false);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(.65f, .08f);
            buttonRect.anchorMax = new Vector2(.97f, .92f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(.08f, .12f, .15f, .98f);
            transportButton = buttonObject.GetComponent<Button>();
            transportButton.targetGraphic = buttonImage;
            transportButton.onClick.AddListener(() => ActionTriggered?.Invoke("transport_book"));

            GameObject buttonLabelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            buttonLabelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform buttonLabelRect = buttonLabelObject.GetComponent<RectTransform>();
            buttonLabelRect.anchorMin = Vector2.zero;
            buttonLabelRect.anchorMax = Vector2.one;
            buttonLabelRect.offsetMin = new Vector2(2f, 2f);
            buttonLabelRect.offsetMax = new Vector2(-2f, -2f);
            TextMeshProUGUI buttonLabel = buttonLabelObject.GetComponent<TextMeshProUGUI>();
            buttonLabel.text = "BOOK INTAKE";
            buttonLabel.alignment = TextAlignmentOptions.Center;
            buttonLabel.fontSize = 11f;
            buttonLabel.enableAutoSizing = true;
            buttonLabel.fontSizeMin = 8f;
            buttonLabel.fontSizeMax = 11f;
            buttonLabel.fontStyle = FontStyles.Bold;
            buttonLabel.color = Color.white;
            buttonLabel.raycastTarget = false;
            transportPanel.SetActive(false);
        }

        private void BuildTrafficPanel()
        {
            trafficPanel = new GameObject("Traffic Stop Panel", typeof(RectTransform), typeof(Image));
            trafficPanel.transform.SetParent(safeRoot, false);
            RectTransform panelRect = trafficPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(.025f, .22f);
            panelRect.anchorMax = new Vector2(.43f, .90f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            trafficPanel.GetComponent<Image>().color = new Color(.025f, .04f, .055f, .91f);

            trafficHeader = AddTrafficText("Traffic Header", new Vector2(.04f,.85f), new Vector2(.96f,.98f), 15f, true);
            trafficDetails = AddTrafficText("Traffic Details", new Vector2(.04f,.50f), new Vector2(.96f,.83f), 12f, false);
            trafficDetails.alignment = TextAlignmentOptions.TopLeft;

            string[] ids = { "traffic_pull_over", "traffic_calm", "traffic_firm", "traffic_license",
                "traffic_registration", "traffic_insurance", "traffic_warning", "traffic_citation", "traffic_end" };
            string[] captions = { "PULL OVER", "CALM APPROACH", "FIRM APPROACH", "CHECK LICENSE",
                "CHECK REG", "CHECK INSURANCE", "WARNING", "CITATION", "END / CANCEL STOP" };
            for (int i = 0; i < ids.Length; i++)
            {
                int column = i % 2;
                int row = i / 2;
                float xMin = .04f + column * .48f;
                float xMax = xMin + .44f;
                float yMax = .48f - row * .11f;
                float yMin = yMax - .10f;
                AddTrafficButton(ids[i], captions[i], new Vector2(xMin,yMin), new Vector2(xMax,yMax));
            }
            trafficPanel.SetActive(false);
        }

        private TextMeshProUGUI AddTrafficText(string objectName, Vector2 anchorMin, Vector2 anchorMax,
            float fontSize, bool centered)
        {
            GameObject labelObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(trafficPanel.transform, false);
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = new Vector2(3f,2f);
            rect.offsetMax = new Vector2(-3f,-2f);
            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.fontSize = fontSize;
            label.enableAutoSizing = true;
            label.fontSizeMin = 8f;
            label.fontSizeMax = fontSize;
            label.fontStyle = centered ? FontStyles.Bold : FontStyles.Normal;
            label.alignment = centered ? TextAlignmentOptions.Center : TextAlignmentOptions.TopLeft;
            label.color = Color.white;
            label.raycastTarget = false;
            return label;
        }

        private void AddTrafficButton(string id, string caption, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject buttonObject = new GameObject("Traffic Action " + id, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(trafficPanel.transform, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(.08f, .12f, .15f, .98f);
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => ActionTriggered?.Invoke(id));

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(2f,2f);
            labelRect.offsetMax = new Vector2(-2f,-2f);
            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = caption;
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 11f;
            label.enableAutoSizing = true;
            label.fontSizeMin = 7f;
            label.fontSizeMax = 11f;
            label.fontStyle = FontStyles.Bold;
            label.color = Color.white;
            label.raycastTarget = false;
            trafficButtons.Add(id, button);
        }

        private void BuildCivilianPanel()
        {
            civilianPanel = new GameObject("Civilian Interaction Panel", typeof(RectTransform), typeof(Image));
            civilianPanel.transform.SetParent(safeRoot, false);
            RectTransform panelRect = civilianPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(.47f, .40f);
            panelRect.anchorMax = new Vector2(.82f, .88f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            civilianPanel.GetComponent<Image>().color = new Color(.025f, .04f, .055f, .88f);

            GameObject headerObject = new GameObject("Civilian Contact Header", typeof(RectTransform), typeof(TextMeshProUGUI));
            headerObject.transform.SetParent(civilianPanel.transform, false);
            RectTransform headerRect = headerObject.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(.03f, .82f);
            headerRect.anchorMax = new Vector2(.97f, .98f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;
            civilianHeader = headerObject.GetComponent<TextMeshProUGUI>();
            civilianHeader.text = "CIVILIAN CONTACT";
            civilianHeader.fontSize = 15f;
            civilianHeader.enableAutoSizing = true;
            civilianHeader.fontSizeMin = 9f;
            civilianHeader.fontSizeMax = 15f;
            civilianHeader.fontStyle = FontStyles.Bold;
            civilianHeader.color = Color.white;
            civilianHeader.alignment = TextAlignmentOptions.Center;
            civilianHeader.raycastTarget = false;

            string[] ids = { "civilian_greet", "civilian_questions", "civilian_id", "civilian_warn",
                "civilian_cite", "civilian_detain", "civilian_arrest", "civilian_release" };
            string[] captions = { "GREET", "QUESTIONS", "ID", "WARN", "CITE", "DETAIN", "ARREST", "RELEASE" };
            for (int i = 0; i < ids.Length; i++)
            {
                int column = i % 2;
                int row = i / 2;
                float xMin = .035f + column * .49f;
                float xMax = xMin + .455f;
                float yMax = .79f - row * .19f;
                float yMin = yMax - .16f;
                AddContextButton(ids[i], captions[i], new Vector2(xMin, yMin), new Vector2(xMax, yMax));
            }
            civilianPanel.SetActive(false);
        }

        private void AddContextButton(string id, string caption, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject buttonObject = new GameObject("Context Action " + id, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(civilianPanel.transform, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(.08f, .12f, .15f, .98f);
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => ActionTriggered?.Invoke(id));

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(2f, 2f);
            labelRect.offsetMax = new Vector2(-2f, -2f);
            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = caption;
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 13f;
            label.enableAutoSizing = true;
            label.fontSizeMin = 8f;
            label.fontSizeMax = 13f;
            label.fontStyle = FontStyles.Bold;
            label.color = Color.white;
            label.raycastTarget = false;
            contextButtons.Add(id, button);
        }

        private void AddStatusLabel()
        {
            GameObject labelObject = new GameObject("Deputy Status", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(safeRoot, false);
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(.025f,.90f);
            rect.anchorMax = new Vector2(.70f,.98f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = "MARION COUNTY SHERIFF  |  OFF DUTY";
            label.fontSize = 17f;
            label.fontStyle = FontStyles.Bold;
            label.color = Color.white;
            label.raycastTarget = false;
            actionLabels.Add("status", label);
        }

        private void AddActionButton(string id, string caption, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject buttonObject = new GameObject("HUD Action " + id, typeof(RectTransform),
                typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(safeRoot, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(.055f,.075f,.095f,.88f);
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => ActionTriggered?.Invoke(id));

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(3f,3f);
            labelRect.offsetMax = new Vector2(-3f,-3f);
            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = caption;
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 15f;
            label.fontStyle = FontStyles.Bold;
            label.color = Color.white;
            label.raycastTarget = false;
            actionLabels.Add(id, label);
        }

        private void AddButton(string id, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject buttonObject = new GameObject("Touch " + id, typeof(RectTransform),
                typeof(Image), typeof(MobileHoldButton));
            buttonObject.transform.SetParent(safeRoot, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(.055f,.075f,.095f,.78f);
            image.raycastTarget = true;

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(3f,3f);
            labelRect.offsetMax = new Vector2(-3f,-3f);
            TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 17f;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.raycastTarget = false;

            MobileHoldButton hold = buttonObject.GetComponent<MobileHoldButton>();
            hold.controlId = id;
            hold.changed = (control, pressed) => ControlChanged?.Invoke(control, pressed);
            buttons.Add(id, hold);
        }

        private void ApplySafeArea()
        {
            if (safeRoot == null || Screen.width <= 0 || Screen.height <= 0) return;
            Rect safe = Screen.safeArea;
            if (safe == lastSafeArea) return;
            lastSafeArea = safe;
            safeRoot.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            safeRoot.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            safeRoot.offsetMin = Vector2.zero;
            safeRoot.offsetMax = Vector2.zero;
        }
    }

    public sealed class MobileHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public string controlId;
        public Action<string, bool> changed;
        private bool held;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (held) return;
            held = true;
            changed?.Invoke(controlId, true);
        }

        public void OnPointerUp(PointerEventData eventData) { Release(); }
        public void OnPointerExit(PointerEventData eventData) { Release(); }
        private void OnDisable() { Release(); }

        public void Release()
        {
            if (!held) return;
            held = false;
            changed?.Invoke(controlId, false);
        }
    }
}
