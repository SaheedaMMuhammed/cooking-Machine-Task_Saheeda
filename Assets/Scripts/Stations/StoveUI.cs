using ChefMachine.Ingredients;
using ChefMachine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ChefMachine.Stations
{
    /// <summary>
    /// World-space cooking readout for the two stove slots with graphical meat icons.
    /// </summary>
    public class StoveUI : MonoBehaviour
    {
        [System.Serializable]
        public class SlotWidgets
        {
            [Tooltip("Row toggled on while this slot is cooking or occupied.")]
            public GameObject Row;
            public Text TitleLabel;
            public Text TimeLabel;
            public Image ProgressFill;
            public Image IconImage;
        }

        [Header("Source")]
        [SerializeField] private Stove stove;

        [Header("Widgets")]
        [SerializeField] private GameObject panel;
        [SerializeField] private SlotWidgets slotOne = new SlotWidgets();
        [SerializeField] private SlotWidgets slotTwo = new SlotWidgets();

        private void Awake()
        {
            if (stove == null) stove = GetComponentInParent<Stove>();
            EnsureIconImages(slotOne, "Slot1Icon");
            EnsureIconImages(slotTwo, "Slot2Icon");
            Show(panel, false);
        }

        private void EnsureIconImages(SlotWidgets widgets, string name)
        {
            if (widgets == null || widgets.IconImage != null || widgets.Row == null) return;

            GameObject iconGo = new GameObject(name);
            iconGo.transform.SetParent(widgets.Row.transform, false);
            iconGo.transform.SetAsFirstSibling();

            widgets.IconImage = iconGo.AddComponent<Image>();
            RectTransform rect = widgets.IconImage.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(30f, 30f);
            rect.anchoredPosition = new Vector2(-40f, 0f);
        }

        private void LateUpdate()
        {
            if (stove == null) return;

            bool anyOccupied = stove.IsCooking || stove.HasFinishedFood;
            Show(panel, anyOccupied);

            UpdateRow(slotOne, stove.GetSlot(0), "Slot 1");
            UpdateRow(slotTwo, stove.GetSlot(1), "Slot 2");
        }

        private void UpdateRow(SlotWidgets widgets, CookingSlot slot, string slotName)
        {
            if (widgets == null || slot == null) return;

            bool occupied = slot.IsOccupied;
            Show(widgets.Row, occupied);
            if (!occupied) return;

            IngredientState currentState = slot.IsFinished ? IngredientState.Cooked : IngredientState.Raw;

            if (widgets.IconImage != null)
            {
                widgets.IconImage.sprite = IngredientIconFactory.GetIcon(IngredientType.Meat, currentState);
                widgets.IconImage.color = Color.white;
            }

            if (widgets.TitleLabel != null)
            {
                widgets.TitleLabel.text = slot.IsFinished ? slotName + ": Cooked Meat!" : slotName + ": Cooking...";
            }

            if (widgets.TimeLabel != null)
            {
                widgets.TimeLabel.text = slot.IsFinished ? "READY" : slot.RemainingTime.ToString("F1") + "s";
            }

            if (widgets.ProgressFill != null)
            {
                widgets.ProgressFill.fillAmount = slot.IsFinished ? 1f : slot.Progress01;
            }
        }

        private static void Show(GameObject target, bool visible)
        {
            if (target == null || target.activeSelf == visible) return;
            target.SetActive(visible);
        }
    }
}
