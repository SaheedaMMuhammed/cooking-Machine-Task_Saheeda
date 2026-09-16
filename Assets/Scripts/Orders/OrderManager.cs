using System.Collections;
using System.Collections.Generic;
using ChefMachine.Core;
using ChefMachine.Ingredients;
using UnityEngine;

namespace ChefMachine.Orders
{
    /// <summary>
    /// Creates random orders, hands them to the four customer windows, scores
    /// completed orders, and refills a window after a short pause.
    /// </summary>
    public class OrderManager : MonoBehaviour
    {
        [Header("Windows")]
        [SerializeField] private List<CustomerWindow> windows = new List<CustomerWindow>();

        [Header("Generation")]
        [Tooltip("Half the orders use the small size, half the large one.")]
        [SerializeField] private int smallOrderSize = 2;
        [SerializeField] private int largeOrderSize = 3;
        [SerializeField] private float respawnDelay = 5f;

        [Header("Score")]
        [SerializeField] private ScoreManager scoreManager;

        private static readonly IngredientType[] OrderableTypes =
        {
            IngredientType.Vegetable,
            IngredientType.Cheese,
            IngredientType.Meat
        };

        public float RespawnDelay { get { return respawnDelay; } }

        private void Awake()
        {
            if (scoreManager == null) scoreManager = ScoreManager.Instance;

            // Subscribed once for the lifetime of the object, so restarting a
            // session never stacks duplicate callbacks.
            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i] != null) windows[i].OrderCompleted += HandleOrderCompleted;
            }
        }

        /// <summary>Called by GameSession when a session starts: every window gets a fresh order.</summary>
        public void BeginSession()
        {
            if (scoreManager == null) scoreManager = ScoreManager.Instance;

            StopAllCoroutines();

            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i] != null) AssignNewOrder(windows[i]);
            }
        }

        /// <summary>Called by GameSession when a session ends or has not begun: no orders, no respawns.</summary>
        public void EndSession()
        {
            StopAllCoroutines();

            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i] != null) windows[i].ClearOrder();
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i] != null) windows[i].OrderCompleted -= HandleOrderCompleted;
            }
        }

        /// <summary>Builds a fresh random order and puts it in the window.</summary>
        public void AssignNewOrder(CustomerWindow window)
        {
            Order order = GenerateOrder();
            window.SetOrder(order);
            Debug.Log("Generated Order for " + window.name + ": " + order.Describe(), window);
        }

        /// <summary>2 or 3 requirements, each an independent random ingredient.</summary>
        public Order GenerateOrder()
        {
            int count = Random.value < 0.5f ? smallOrderSize : largeOrderSize;

            List<OrderRequirement> required = new List<OrderRequirement>(count);
            for (int i = 0; i < count; i++)
            {
                // Independent draws, so duplicates happen naturally.
                IngredientType type = OrderableTypes[Random.Range(0, OrderableTypes.Length)];
                required.Add(new OrderRequirement(type, IngredientRules.ServeState(type)));
            }

            return new Order(required);
        }

        private void HandleOrderCompleted(CustomerWindow window, Order order)
        {
            int basePoints = order.BasePoints;
            int waited = order.ElapsedSecondsFloored;
            int awarded = order.CalculateScore();

            Debug.Log("Score for " + window.name + ": base " + basePoints + " - " + waited + "s waited = " + awarded, window);

            if (scoreManager != null) scoreManager.AddScore(awarded);
            else Debug.LogWarning("OrderManager has no ScoreManager; score not counted.", this);

            // Show floating score popup near the window
            ShowScorePopup(window, awarded);

            StartCoroutine(RespawnAfterDelay(window));
        }

        /// <summary>
        /// Spawns a world-space floating text showing the awarded score near the
        /// customer window. It fades out and self-destructs after a few seconds.
        /// </summary>
        private void ShowScorePopup(CustomerWindow window, int score)
        {
            // Create a world-space Canvas + Text for the popup
            GameObject popupGo = new GameObject("ScorePopup");
            popupGo.transform.position = window.transform.position + new Vector3(0f, 1.8f, -0.5f);

            Canvas canvas = popupGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(200f, 60f);
            canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            // Make it face the camera
            Camera cam = Camera.main;
            if (cam != null)
            {
                popupGo.transform.rotation = Quaternion.LookRotation(
                    popupGo.transform.position - cam.transform.position, Vector3.up);
            }

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(popupGo.transform, false);

            UnityEngine.UI.Text text = textGo.AddComponent<UnityEngine.UI.Text>();
            text.text = score >= 0 ? "+" + score : score.ToString();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 36;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = score >= 0 ? new Color(0.2f, 0.85f, 0.3f) : new Color(0.95f, 0.3f, 0.25f);

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(200f, 60f);
            textRect.anchoredPosition = Vector2.zero;

            // Outline for readability
            UnityEngine.UI.Outline outline = textGo.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            StartCoroutine(FadeAndDestroy(popupGo, text, 2.5f));
        }

        private IEnumerator FadeAndDestroy(GameObject popupGo, UnityEngine.UI.Text text, float duration)
        {
            float riseSpeed = 0.3f;
            float elapsed = 0f;

            Color startColor = text.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // Float upward
                if (popupGo != null)
                    popupGo.transform.position += Vector3.up * riseSpeed * Time.deltaTime;

                // Fade out in the second half
                float alpha = elapsed < duration * 0.5f
                    ? 1f
                    : Mathf.Lerp(1f, 0f, (elapsed - duration * 0.5f) / (duration * 0.5f));

                if (text != null)
                {
                    Color c = startColor;
                    c.a = alpha;
                    text.color = c;
                }

                yield return null;
            }

            if (popupGo != null) Destroy(popupGo);
        }

        private IEnumerator RespawnAfterDelay(CustomerWindow window)
        {
            // The window stays empty and refuses ingredients while this runs.
            yield return new WaitForSeconds(respawnDelay);

            if (window == null) yield break;
            AssignNewOrder(window);
        }
    }
}
