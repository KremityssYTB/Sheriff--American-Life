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
        private sealed class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
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

        private readonly Dictionary<string, HoldButton> buttons = new Dictionary<string, HoldButton>();
        private Canvas canvas;
        private RectTransform safeRoot;
        private Rect lastSafeArea;
        private bool vehicleMode;

        public event Action<string, bool> ControlChanged;

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
            foreach (KeyValuePair<string, HoldButton> pair in buttons)
                pair.Value.gameObject.SetActive(IsVehicleControl(pair.Key) == vehicleMode);
        }

        public void SetControlsVisible(bool visible)
        {
            if (!visible) ReleaseAll();
            if (canvas != null) canvas.enabled = visible;
        }

        public void ReleaseAll()
        {
            foreach (HoldButton button in buttons.Values)
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
            AddButton("gas", "GAS", new Vector2(.84f,.055f), new Vector2(.975f,.235f));
            AddButton("brake", "BRAKE\nREV", new Vector2(.84f,.25f), new Vector2(.975f,.43f));
        }

        private void AddButton(string id, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject buttonObject = new GameObject("Touch " + id, typeof(RectTransform),
                typeof(Image), typeof(HoldButton));
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

            HoldButton hold = buttonObject.GetComponent<HoldButton>();
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
}
