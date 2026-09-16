using ChefMachine.Ingredients;
using ChefMachine.Stations;
using UnityEngine;
using UnityEngine.UI;

namespace ChefMachine.UI
{
    /// <summary>
    /// Small world-space selector next to the refrigerator: three clickable
    /// ingredient options with graphical icons. Clicking one takes that ingredient straight away.
    /// </summary>
    public class RefrigeratorUI : MonoBehaviour
    {
        [System.Serializable]
        public class Option
        {
            public IngredientType Type = IngredientType.Cheese;
            public Button Button;
            public Image Icon;
            public Text Label;
        }

        [Header("Source")]
        [SerializeField] private Refrigerator refrigerator;

        [Header("Widgets")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Option[] options = new Option[0];

        [Header("Highlight")]
        [SerializeField] private Color selectedBackground = new Color(0.98f, 0.78f, 0.20f, 1f);
        [SerializeField] private Color unselectedBackground = new Color(0.22f, 0.24f, 0.28f, 0.95f);
        [SerializeField] private Color selectedLabel = new Color(0.05f, 0.06f, 0.08f);
        [SerializeField] private Color unselectedLabel = new Color(0.85f, 0.87f, 0.90f);

        private void Awake()
        {
            if (refrigerator == null) refrigerator = GetComponentInParent<Refrigerator>();

            for (int i = 0; i < options.Length; i++)
            {
                Option option = options[i];
                if (option == null) continue;

                IngredientType type = option.Type;
                IngredientState spawnState = IngredientRules.SpawnState(type);

                // Assign graphical icon
                if (option.Icon != null)
                {
                    option.Icon.sprite = IngredientIconFactory.GetIcon(type, spawnState);
                    option.Icon.color = Color.white;
                }

                if (option.Button != null)
                {
                    option.Button.onClick.AddListener(() => OnOptionClicked(type));
                }
            }

            Show(false);
        }

        private void LateUpdate()
        {
            if (refrigerator == null) return;

            bool available = refrigerator.SelectionAvailable;
            Show(available);
            if (!available) return;

            Refresh();
        }

        /// <summary>One click both chooses and takes; the refrigerator does the work.</summary>
        private void OnOptionClicked(IngredientType type)
        {
            if (refrigerator == null || !refrigerator.SelectionAvailable) return;

            refrigerator.SelectAndTake(type);
            Refresh();
        }

        private void Refresh()
        {
            bool hasSelection = refrigerator.HasSelection;
            IngredientType selected = refrigerator.SelectedType;

            for (int i = 0; i < options.Length; i++)
            {
                Option option = options[i];
                if (option == null) continue;

                bool isSelected = hasSelection && option.Type == selected;

                if (option.Icon != null)
                {
                    option.Icon.sprite = IngredientIconFactory.GetIcon(option.Type, IngredientRules.SpawnState(option.Type));
                    option.Icon.color = Color.white;
                }

                if (option.Button != null)
                {
                    Image background = option.Button.image;
                    if (background != null) background.color = isSelected ? selectedBackground : unselectedBackground;
                }

                if (option.Label != null)
                {
                    option.Label.color = isSelected ? selectedLabel : unselectedLabel;
                    option.Label.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
                }
            }
        }

        private void Show(bool visible)
        {
            if (panel == null || panel.activeSelf == visible) return;
            panel.SetActive(visible);
        }
    }
}
