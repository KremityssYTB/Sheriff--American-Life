using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace SheriffAmericanLife
{
    /// <summary>Runtime-generated touch MDT screen. Attach to the same object as MobileMdtController.</summary>
    [RequireComponent(typeof(MobileMdtController))]
    public sealed class MobileMdtScreen : MonoBehaviour
    {
        [SerializeField] private string unitName = "OCALA-1";
        private MobileMdtController mdt;
        private Canvas canvas;
        private RectTransform root;
        private RectTransform callList;
        private RectTransform detailsContent;
        private TextMeshProUGUI statusText;
        private GameObject window;
        private bool visible;
        private string activeTab = "DETAILS";
        private readonly List<GameObject> callRows = new List<GameObject>();

        private void Awake()
        {
            mdt = GetComponent<MobileMdtController>();
            BuildScreen();
            SetVisible(false);
        }

        private void OnEnable()
        {
            if (mdt != null) mdt.MdtChanged += Refresh;
        }

        private void OnDisable()
        {
            if (mdt != null) mdt.MdtChanged -= Refresh;
        }

        private void Update()
        {
            if (canvas != null) canvas.enabled = true;
            if (root != null) ApplyResponsiveLayout();
        }

        private void BuildScreen()
        {
            if (EventSystem.current == null)
            {
                GameObject eventSystemObject = new GameObject("MDT Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
                eventSystemObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }

            GameObject canvasObject = new GameObject("Mobile MDT Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            window = Panel("MDT Window", canvasObject.transform, new Color(.035f,.055f,.075f,.97f));
            root = window.GetComponent<RectTransform>();
            root.anchorMin = new Vector2(.025f,.04f);
            root.anchorMax = new Vector2(.975f,.96f);
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            Text("SHERIFF MOBILE DATA TERMINAL", window.transform, 34, TextAlignmentOptions.Left,
                new Vector2(.025f,.91f), new Vector2(.72f,.99f));
            statusText = Text("UNIT " + unitName + "  |  READY", window.transform, 22,
                TextAlignmentOptions.Right, new Vector2(.68f,.92f), new Vector2(.86f,.99f));
            MakeButton("CLOSE", window.transform, new Vector2(.88f,.92f), new Vector2(.98f,.99f),
                () => SetVisible(false));

            Panel("Call List Panel", window.transform, new Color(.07f,.095f,.12f,1f),
                new Vector2(.015f,.035f), new Vector2(.32f,.90f));
            Panel("Case Panel", window.transform, new Color(.07f,.095f,.12f,1f),
                new Vector2(.335f,.035f), new Vector2(.985f,.90f));

            Transform left = window.transform.Find("Call List Panel");
            Text("ACTIVE CALLS", left, 25, TextAlignmentOptions.Left,
                new Vector2(.05f,.91f), new Vector2(.95f,.99f));
            ScrollArea list = CreateScrollArea(left, new Vector2(.035f,.035f), new Vector2(.965f,.89f));
            callList = list.content;

            Transform right = window.transform.Find("Case Panel");
            MakeButton("DETAILS", right, new Vector2(.025f,.91f), new Vector2(.25f,.99f), () => SetTab("DETAILS"));
            MakeButton("WITNESSES", right, new Vector2(.27f,.91f), new Vector2(.51f,.99f), () => SetTab("WITNESSES"));
            MakeButton("REPORTS", right, new Vector2(.53f,.91f), new Vector2(.75f,.99f), () => SetTab("REPORTS"));
            ScrollArea detail = CreateScrollArea(right, new Vector2(.025f,.16f), new Vector2(.975f,.89f));
            detailsContent = detail.content;
            CreateActionButtons(right);
            CreateFloatingToggle(canvasObject.transform);
            Refresh();
        }

        private void CreateActionButtons(Transform parent)
        {
            string[] labels = { "ACCEPT", "EN ROUTE", "ON SCENE", "RESOLVE", "CANCEL" };
            System.Action[] actions = {
                () => RunAction(mdt.AcceptSelectedCall(unitName)),
                () => RunAction(mdt.MarkSelectedEnRoute()),
                () => RunAction(mdt.MarkSelectedOnScene()),
                () => RunAction(mdt.ResolveSelectedCall("Call completed")),
                () => RunAction(mdt.CancelSelectedCall("Cancelled by unit"))
            };
            for (int i = 0; i < labels.Length; i++)
            {
                float min = .02f + i * .197f;
                MakeButton(labels[i], parent, new Vector2(min,.035f),
                    new Vector2(min + .185f,.14f), actions[i]);
            }
        }

        private void CreateFloatingToggle(Transform parent)
        {
            MakeButton("MDT", parent, new Vector2(.86f,.02f), new Vector2(.98f,.09f),
                () => SetVisible(!visible));
        }

        private void SetTab(string tab)
        {
            activeTab = tab;
            RefreshDetails();
        }

        private void RunAction(bool success)
        {
            statusText.text = success ? "UNIT " + unitName + "  |  UPDATED" :
                "UNIT " + unitName + "  |  ACTION NOT AVAILABLE";
            Refresh();
        }

        private void Refresh()
        {
            if (mdt == null || callList == null) return;
            foreach (GameObject row in callRows) if (row != null) Destroy(row);
            callRows.Clear();

            IReadOnlyList<DispatchCall> calls = mdt.Calls;
            for (int i = 0; i < calls.Count; i++)
            {
                DispatchCall call = calls[i];
                string callId = call.callId;
                GameObject row = MakeButtonObject(call.callId + "  |  " + call.priority + "\n" +
                    call.title + "\n" + call.status + "  •  " + call.receivedAtGameTime,
                    callList, () => { mdt.SelectCall(callId); activeTab = "DETAILS"; Refresh(); });
                row.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 112);
                callRows.Add(row);
            }
            statusText.text = "UNIT " + unitName + "  |  " + calls.Count + " CALL(S)";
            RefreshDetails();
        }

        private void RefreshDetails()
        {
            if (detailsContent == null) return;
            ClearChildren(detailsContent);
            DispatchCall call = mdt.SelectedCall;
            if (call == null)
            {
                AddContentText("SELECT A CALL", 30);
                AddContentText("No dispatch call selected. New calls published by the world incident system will appear in the list.", 22);
                return;
            }

            if (activeTab == "DETAILS")
            {
                AddContentText(call.callId + "   /   " + call.priority + "   /   " + call.status, 28);
                AddContentText(call.title, 32);
                AddContentText("RECEIVED: " + call.receivedAtGameTime + "\nUNIT: " + call.assignedUnit +
                    "\nSOURCE: " + call.sourceIncidentId, 21);
                AddContentText("INCIDENT\n" + call.description, 23);
                AddContentText("SUBJECT\n" + (string.IsNullOrEmpty(call.subjectDescription) ? "Not provided" : call.subjectDescription), 22);
                AddContentText("WORLD POSITION\nX " + call.location.x.ToString("0.0") +
                    "   Y " + call.location.y.ToString("0.0") + "   Z " + call.location.z.ToString("0.0"), 20);
                if (!string.IsNullOrEmpty(call.disposition)) AddContentText("DISPOSITION\n" + call.disposition, 22);
                AddContentText("Use the action bar to update this call. Actions are validated by dispatch status.", 19);
            }
            else if (activeTab == "WITNESSES")
            {
                AddContentText("WITNESS STATEMENTS", 28);
                List<WitnessStatement> items = mdt.GetSelectedWitnessStatements();
                if (items.Count == 0) AddContentText("No witness statements linked to this incident.", 22);
                foreach (WitnessStatement item in items)
                    AddContentText(item.witnessName + "  |  " + item.observationType +
                        "\nConfidence estimate: " + Mathf.RoundToInt(item.confidence * 100f) + "%" +
                        "\n" + item.incidentDescription + "\nSubject: " + item.subjectDescription, 22);
                AddContentText("Observation confidence is an estimate, not a determination of guilt or credibility.", 18);
            }
            else
            {
                AddContentText("RELATED INCIDENT REPORTS", 28);
                List<IncidentReport> items = mdt.GetSelectedReports();
                if (items.Count == 0) AddContentText("No reports linked to this incident yet.", 22);
                foreach (IncidentReport item in items)
                    AddContentText(item.incidentNumber + "  |  " + item.category + "\n" +
                        item.recordedAt + "\nAction: " + item.actionTaken + "\nOutcome: " + item.outcome +
                        "\n" + item.narrative, 21);
            }
        }

        private void AddContentText(string value, int size)
        {
            TextMeshProUGUI label = Text(value, detailsContent, size, TextAlignmentOptions.TopLeft,
                Vector2.zero, Vector2.one);
            label.rectTransform.sizeDelta = new Vector2(0, Mathf.Max(65, size * 3.2f));
            label.rectTransform.anchorMin = new Vector2(0,1);
            label.rectTransform.anchorMax = new Vector2(1,1);
            label.rectTransform.pivot = new Vector2(.5f,1);
            label.gameObject.AddComponent<LayoutElement>().preferredHeight = Mathf.Max(65, size * 3.2f);
        }

        private void ApplyResponsiveLayout()
        {
            bool portrait = Screen.height > Screen.width;
            Transform left = window.transform.Find("Call List Panel");
            Transform right = window.transform.Find("Case Panel");
            if (left == null || right == null) return;
            RectTransform l = left.GetComponent<RectTransform>();
            RectTransform r = right.GetComponent<RectTransform>();
            if (portrait)
            {
                l.anchorMin = new Vector2(.015f,.53f); l.anchorMax = new Vector2(.985f,.90f);
                r.anchorMin = new Vector2(.015f,.035f); r.anchorMax = new Vector2(.985f,.51f);
            }
            else
            {
                l.anchorMin = new Vector2(.015f,.035f); l.anchorMax = new Vector2(.32f,.90f);
                r.anchorMin = new Vector2(.335f,.035f); r.anchorMax = new Vector2(.985f,.90f);
            }
            l.offsetMin = l.offsetMax = Vector2.zero;
            r.offsetMin = r.offsetMax = Vector2.zero;
        }

        private void SetVisible(bool value)
        {
            visible = value;
            if (window != null) window.SetActive(value);
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--) Destroy(parent.GetChild(i).gameObject);
        }

        private static GameObject Panel(string name, Transform parent, Color color,
            Vector2? min = null, Vector2? max = null)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = min ?? Vector2.zero;
            rect.anchorMax = max ?? Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static TextMeshProUGUI Text(string value, Transform parent, int size,
            TextAlignmentOptions alignment, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = new Vector2(8,4);
            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.text = value; label.fontSize = size; label.color = Color.white;
            label.alignment = alignment; label.enableWordWrapping = true;
            return label;
        }

        private static void MakeButton(string label, Transform parent, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
        {
            GameObject go = MakeButtonObject(label, parent, action);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = new Vector2(3,3);
        }

        private static GameObject MakeButtonObject(string label, Transform parent, UnityEngine.Events.UnityAction action)
        {
            GameObject go = new GameObject("Button - " + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>(); image.color = new Color(.12f,.22f,.30f,1);
            Button button = go.GetComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(action);
            Text(label, go.transform, 19, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            return go;
        }

        private sealed class Scroll
        {
            public RectTransform content;
        }

        private static Scroll CreateScrollArea(Transform parent, Vector2 min, Vector2 max)
        {
            GameObject viewportObject = new GameObject("Scroll View", typeof(RectTransform), typeof(Image),
                typeof(Mask), typeof(ScrollRect));
            viewportObject.transform.SetParent(parent, false);
            RectTransform viewport = viewportObject.GetComponent<RectTransform>();
            viewport.anchorMin = min; viewport.anchorMax = max; viewport.offsetMin = viewport.offsetMax = new Vector2(8,6);
            viewportObject.GetComponent<Image>().color = new Color(0,0,0,.12f);
            viewportObject.GetComponent<Mask>().showMaskGraphic = false;

            GameObject contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObject.transform.SetParent(viewportObject.transform, false);
            RectTransform content = contentObject.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0,1); content.anchorMax = Vector2.one;
            content.pivot = new Vector2(.5f,1); content.offsetMin = content.offsetMax = Vector2.zero;
            VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8; layout.padding = new RectOffset(8,8,8,8);
            layout.childControlWidth = true; layout.childControlHeight = true;
            layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
            ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            ScrollRect scroll = viewportObject.GetComponent<ScrollRect>();
            scroll.viewport = viewport; scroll.content = content;
            scroll.horizontal = false; scroll.vertical = true;
            return new Scroll { content = content };
        }
    }
}
