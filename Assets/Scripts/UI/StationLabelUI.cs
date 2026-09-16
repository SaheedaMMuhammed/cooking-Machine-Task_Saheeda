using ChefMachine.Orders;
using ChefMachine.Stations;
using UnityEngine;
using UnityEngine.UI;

namespace ChefMachine.UI
{
    /// <summary>
    /// Displays a sleek floating world-space text label above a kitchen station
    /// (Refrigerator, Chopping Table, Stove, Trash, Customer Windows).
    /// Positions labels cleanly above physical objects without blocking station visual elements.
    /// Aligns flat towards camera (no tilt). Exposed in Hierarchy for easy re-arrangement.
    /// </summary>
    public class StationLabelUI : MonoBehaviour
    {
        [Header("Label Content")]
        [SerializeField] private string labelText = "STATION";
        [SerializeField] private Vector3 offset = new Vector3(0f, 2.2f, 0f);

        [Header("Appearance")]
        [SerializeField] private Color badgeColor = new Color(0.1f, 0.13f, 0.18f, 0.88f);
        [SerializeField] private Color textColor = new Color(0.95f, 0.96f, 0.98f);
        [SerializeField] private Vector2 sizeDelta = new Vector2(240f, 48f);

        [Header("Hierarchy Reference")]
        [Tooltip("The canvas GameObject created in the hierarchy for easy positioning in Inspector.")]
        public GameObject labelCanvasObj;

        public void SetLabel(string text, Vector3 positionOffset)
        {
            labelText = text;
            offset = positionOffset;
            CreateOrUpdateCanvas();
        }

        private void Start()
        {
            CreateOrUpdateCanvas();
        }

        public void CreateOrUpdateCanvas()
        {
            if (labelCanvasObj == null)
            {
                // Visible in Hierarchy under the station object as 'StationLabelCanvas'
                labelCanvasObj = new GameObject("StationLabelCanvas");
                labelCanvasObj.transform.SetParent(transform, false);
            }

            labelCanvasObj.transform.localPosition = offset;

            Canvas canvas = labelCanvasObj.GetComponent<Canvas>();
            if (canvas == null) canvas = labelCanvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 40;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = sizeDelta;
            canvasRect.localScale = new Vector3(0.0055f, 0.0055f, 0.0055f);

            // Badge Background
            Transform bgTransform = labelCanvasObj.transform.Find("BadgeBG");
            GameObject bgGo;
            if (bgTransform == null)
            {
                bgGo = new GameObject("BadgeBG");
                bgGo.transform.SetParent(labelCanvasObj.transform, false);
            }
            else
            {
                bgGo = bgTransform.gameObject;
            }

            Image bgImage = bgGo.GetComponent<Image>();
            if (bgImage == null) bgImage = bgGo.AddComponent<Image>();
            bgImage.color = badgeColor;

            RectTransform bgRect = bgImage.GetComponent<RectTransform>();
            bgRect.sizeDelta = sizeDelta;
            bgRect.anchoredPosition = Vector2.zero;

            Outline outline = bgGo.GetComponent<Outline>();
            if (outline == null) outline = bgGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.4f, 0.55f, 0.75f, 0.7f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            // Text Label
            Transform textTransform = labelCanvasObj.transform.Find("LabelText");
            GameObject textGo;
            if (textTransform == null)
            {
                textGo = new GameObject("LabelText");
                textGo.transform.SetParent(labelCanvasObj.transform, false);
            }
            else
            {
                textGo = textTransform.gameObject;
            }

            Text text = textGo.GetComponent<Text>();
            if (text == null) text = textGo.AddComponent<Text>();
            text.text = labelText;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = textColor;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.sizeDelta = sizeDelta;
            textRect.anchoredPosition = Vector2.zero;
        }

        private void LateUpdate()
        {
            if (labelCanvasObj == null) return;

            // Keep position synced with offset in case edited in inspector
            labelCanvasObj.transform.localPosition = offset;

            // Align flat towards camera without vertical pitch/tilt (no tilts)
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 camEuler = cam.transform.eulerAngles;
                labelCanvasObj.transform.rotation = Quaternion.Euler(0f, camEuler.y, 0f);
            }
        }

        /// <summary>
        /// Automatically attaches floating text labels to all kitchen stations in the scene at runtime load.
        /// Non-blocking offsets keep labels clear of physical station models.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoLabelAllStations()
        {
            // Refrigerator (Floating high above fridge top edge)
            var fridges = FindObjectsByType<Refrigerator>(FindObjectsSortMode.None);
            foreach (var f in fridges)
            {
                if (f.GetComponent<StationLabelUI>() == null)
                {
                    var label = f.gameObject.AddComponent<StationLabelUI>();
                    label.SetLabel("REFRIGERATOR", new Vector3(0f, 2.5f, 0f));
                }
            }

            // Chopping Table (Floating high above chop table surface)
            var chopTables = FindObjectsByType<ChopTable>(FindObjectsSortMode.None);
            foreach (var ct in chopTables)
            {
                if (ct.GetComponent<StationLabelUI>() == null)
                {
                    var label = ct.gameObject.AddComponent<StationLabelUI>();
                    label.SetLabel("CHOPPING TABLE", new Vector3(0f, 2.2f, 0f));
                }
            }

            // Stove (Floating high above stove top)
            var stoves = FindObjectsByType<Stove>(FindObjectsSortMode.None);
            foreach (var s in stoves)
            {
                if (s.GetComponent<StationLabelUI>() == null)
                {
                    var label = s.gameObject.AddComponent<StationLabelUI>();
                    label.SetLabel("STOVE", new Vector3(0f, 2.2f, 0f));
                }
            }

            // Trash (Floating high above trash bin)
            var trashes = FindObjectsByType<Trash>(FindObjectsSortMode.None);
            foreach (var t in trashes)
            {
                if (t.GetComponent<StationLabelUI>() == null)
                {
                    var label = t.gameObject.AddComponent<StationLabelUI>();
                    label.SetLabel("TRASH", new Vector3(0f, 1.8f, 0f));
                }
            }

            // Customer Windows (Floating nicely aligned above hatch)
            var windows = FindObjectsByType<CustomerWindow>(FindObjectsSortMode.None);
            foreach (var w in windows)
            {
                if (w.GetComponent<StationLabelUI>() == null)
                {
                    var label = w.gameObject.AddComponent<StationLabelUI>();
                    string windowName = w.name.Replace("CustomerWindow_", "CUSTOMER ").Replace("CustomerWindow", "CUSTOMER ");
                    if (string.IsNullOrEmpty(windowName)) windowName = "CUSTOMER WINDOW";
                    label.SetLabel(windowName.ToUpper(), new Vector3(0f, 2.3f, 0f));
                }
            }
        }
    }
}
