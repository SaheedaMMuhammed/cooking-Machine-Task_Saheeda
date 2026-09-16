using ChefMachine.Ingredients;
using ChefMachine.Stations;
using UnityEngine;
using UnityEngine.UI;

namespace ChefMachine.UI
{
    /// <summary>
    /// World-space progress readout for a ChopTable with graphical ingredient icon badge.
    /// </summary>
    public class ChopTableUI : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private ChopTable table;

        [Header("Widgets")]
        [Tooltip("Toggled on while the table is chopping or ready.")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text timeLabel;
        [SerializeField] private Image progressFill;
        [SerializeField] private Image iconImage;

        [Header("Text")]
        [SerializeField] private string title = "Chopping";

        private void Awake()
        {
            if (table == null) table = GetComponentInParent<ChopTable>();
            if (titleLabel != null) titleLabel.text = title;
            EnsureIconImage();
            Show(false);
        }

        private void EnsureIconImage()
        {
            if (iconImage != null || panel == null) return;

            // Dynamically create icon image widget if missing from prefab
            GameObject iconGo = new GameObject("TableIconImage");
            iconGo.transform.SetParent(panel.transform, false);
            iconGo.transform.SetAsFirstSibling();

            iconImage = iconGo.AddComponent<Image>();
            RectTransform rect = iconImage.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(36f, 36f);
            rect.anchoredPosition = new Vector2(-45f, 0f);
        }

        private void LateUpdate()
        {
            if (table == null) return;

            bool occupied = table.IsOccupied || table.IsProcessing;
            Show(occupied);
            if (!occupied) return;

            IngredientState currentState = table.IsProcessing ? IngredientState.Raw : IngredientState.Chopped;
            if (iconImage != null)
            {
                iconImage.sprite = IngredientIconFactory.GetIcon(IngredientType.Vegetable, currentState);
                iconImage.color = Color.white;
            }

            if (titleLabel != null)
            {
                titleLabel.text = table.IsProcessing ? "Chopping..." : "Vegetable Ready!";
            }

            if (timeLabel != null)
            {
                timeLabel.text = table.IsProcessing ? table.RemainingTime.ToString("F1") + "s" : "READY";
            }

            if (progressFill != null)
            {
                progressFill.fillAmount = table.IsProcessing ? table.Progress01 : 1f;
            }
        }

        private void Show(bool visible)
        {
            if (panel == null || panel.activeSelf == visible) return;
            panel.SetActive(visible);
        }
    }
}
