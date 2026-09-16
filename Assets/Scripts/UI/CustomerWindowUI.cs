using System.Collections.Generic;
using ChefMachine.Ingredients;
using ChefMachine.Orders;
using UnityEngine;
using UnityEngine.UI;

namespace ChefMachine.UI
{
    /// <summary>
    /// World-space order ticket for one customer window: what is wanted, what has
    /// been handed in, and how long the order has been open.
    /// Uses graphical icons instead of text labels for ingredients.
    /// </summary>
    public class CustomerWindowUI : MonoBehaviour
    {
        [System.Serializable]
        public class IngredientSlot
        {
            public GameObject Root;
            public Image Icon;
            public Text Label;
            public GameObject CheckMark;
        }

        [Header("Source")]
        [SerializeField] private CustomerWindow window;

        [Header("Panel")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Text headerLabel;
        [SerializeField] private GameObject waitingLabel;

        [Header("Order Timer")]
        [Tooltip("Radial 360 Image that drains as the order waits.")]
        [SerializeField] private Image timerFill;
        [SerializeField] private Text timerLabel;
        [Tooltip("Seconds the ring takes to drain. Presentation only - orders never expire.")]
        [SerializeField] private float visualTimerDuration = 60f;

        [Header("Requirements")]
        [SerializeField] private IngredientSlot[] slots = new IngredientSlot[0];

        [Header("Colors")]
        [Tooltip("Multiplier applied to a delivered requirement's icon.")]
        [SerializeField] private float completedDim = 0.35f;

        [Header("Urgency")]
        [SerializeField] private Color timerHealthy = new Color(0.45f, 0.85f, 0.45f);
        [SerializeField] private Color timerLow = new Color(0.98f, 0.75f, 0.20f);
        [SerializeField] private Color timerUrgent = new Color(0.95f, 0.35f, 0.30f);
        [Tooltip("Fill fraction below which the timer counts as running low.")]
        [SerializeField] private float lowThreshold = 0.5f;
        [SerializeField] private float urgentThreshold = 0.2f;

        private void Awake()
        {
            if (window == null) window = GetComponentInParent<CustomerWindow>();
            if (headerLabel != null && window != null)
            {
                headerLabel.text = window.name.Replace("CustomerWindow_", "CUSTOMER ");
            }
        }

        private void LateUpdate()
        {
            if (window == null) return;

            Order order = window.ActiveOrder;

            if (order == null)
            {
                ShowWaitingState();
                return;
            }

            Show(waitingLabel, false);
            ShowRequirements(order);
            ShowTimer(order);
        }

        /// <summary>Between orders: no icons, no timer, no stale numbers.</summary>
        private void ShowWaitingState()
        {
            for (int i = 0; i < slots.Length; i++) Show(slots[i].Root, false);

            Show(waitingLabel, true);
            if (timerFill != null) timerFill.fillAmount = 0f;
            if (timerLabel != null) timerLabel.text = "";
        }

        /// <summary>One slot per requirement, with graphical icon representing the ingredient.</summary>
        private void ShowRequirements(Order order)
        {
            IReadOnlyList<OrderRequirement> requirements = order.Requirements;

            for (int i = 0; i < slots.Length; i++)
            {
                IngredientSlot slot = slots[i];
                if (slot == null) continue;

                if (i >= requirements.Count)
                {
                    Show(slot.Root, false);
                    continue;
                }

                OrderRequirement requirement = requirements[i];
                Show(slot.Root, true);

                if (slot.Icon != null)
                {
                    // Assign procedural graphical icon matching the required ingredient state
                    slot.Icon.sprite = IngredientIconFactory.GetIcon(requirement.Type, requirement.RequiredState);
                    Color iconColor = requirement.Fulfilled ? new Color(completedDim, completedDim, completedDim, 1f) : Color.white;
                    slot.Icon.color = iconColor;
                }

                // Hide text label in favor of visual icon
                if (slot.Label != null)
                {
                    Show(slot.Label.gameObject, false);
                }

                Show(slot.CheckMark, requirement.Fulfilled);
            }
        }

        private void ShowTimer(Order order)
        {
            float duration = Mathf.Max(1f, visualTimerDuration);
            float fill = 1f - Mathf.Clamp01(order.ElapsedSeconds / duration);

            if (timerFill != null)
            {
                timerFill.fillAmount = fill;
                timerFill.color = fill <= urgentThreshold ? timerUrgent
                    : (fill <= lowThreshold ? timerLow : timerHealthy);
            }

            if (timerLabel != null) timerLabel.text = order.ElapsedSecondsFloored + "s";
        }

        private static void Show(GameObject target, bool visible)
        {
            if (target == null || target.activeSelf == visible) return;
            target.SetActive(visible);
        }
    }
}
