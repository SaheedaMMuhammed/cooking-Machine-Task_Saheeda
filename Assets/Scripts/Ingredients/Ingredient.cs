using ChefMachine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ChefMachine.Ingredients
{
    /// <summary>
    /// One ingredient instance in the world: its type, state, and visual representation.
    /// Attaches a world-space icon badge floating above the object so players see
    /// the exact emoji/icon in hands, on tables, and on stoves.
    /// </summary>
    public class Ingredient : MonoBehaviour
    {
        [SerializeField] private IngredientType type = IngredientType.Cheese;
        [SerializeField] private IngredientState state = IngredientState.Raw;

        private GameObject worldIconObj;
        private Image worldIconImage;

        public IngredientType Type { get { return type; } }
        public IngredientState State { get { return state; } }
        public int Points { get { return IngredientPoints.For(type); } }

        private void Start()
        {
            UpdateVisualAppearance(null, Vector3.zero);
        }

        public void SetState(IngredientState newState)
        {
            state = newState;
            UpdateVisualAppearance(null, Vector3.zero);
        }

        /// <summary>Updates the visual appearance based on material/scale or default state colors.</summary>
        public void ApplyAppearance(Material material, Vector3 scale)
        {
            UpdateVisualAppearance(material, scale);
        }

        private void UpdateVisualAppearance(Material overrideMaterial, Vector3 overrideScale)
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

            if (overrideMaterial != null)
            {
                if (meshRenderer != null) meshRenderer.sharedMaterial = overrideMaterial;
            }
            else
            {
                // Fallback / standard dynamic material matching type & state
                if (meshRenderer != null)
                {
                    Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                    mat.color = GetDefaultColor(type, state);
                    meshRenderer.material = mat;
                }
            }

            Vector3 targetScale = overrideScale != Vector3.zero ? overrideScale : GetDefaultScale(type, state);
            transform.localScale = targetScale;

            gameObject.name = ToString();

            UpdateWorldIcon();
        }

        private void EnsureWorldIcon()
        {
            if (worldIconObj != null) return;

            worldIconObj = new GameObject("WorldIconCanvas");
            worldIconObj.transform.SetParent(transform, false);
            // Positioned slightly above the item
            worldIconObj.transform.localPosition = new Vector3(0f, 0.45f, 0f);

            Canvas canvas = worldIconObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 50;

            RectTransform rect = canvas.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80f, 80f);
            rect.localScale = new Vector3(0.005f, 0.005f, 0.005f);

            // Dark background badge for high contrast in 3D world space
            GameObject bgGo = new GameObject("IconBG");
            bgGo.transform.SetParent(worldIconObj.transform, false);
            Image bgImage = bgGo.AddComponent<Image>();
            bgImage.color = new Color(0.12f, 0.14f, 0.18f, 0.88f);
            RectTransform bgRect = bgImage.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(90f, 90f);
            bgRect.anchoredPosition = Vector2.zero;

            // Icon Image
            GameObject imgGo = new GameObject("IconImage");
            imgGo.transform.SetParent(worldIconObj.transform, false);
            worldIconImage = imgGo.AddComponent<Image>();
            RectTransform imgRect = worldIconImage.GetComponent<RectTransform>();
            imgRect.sizeDelta = new Vector2(76f, 76f);
            imgRect.anchoredPosition = Vector2.zero;
        }

        private void UpdateWorldIcon()
        {
            EnsureWorldIcon();
            if (worldIconImage != null)
            {
                worldIconImage.sprite = IngredientIconFactory.GetIcon(type, state);
                worldIconImage.color = Color.white;
            }
        }

        private void LateUpdate()
        {
            // Billboard effect: always face the camera
            if (worldIconObj != null)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    worldIconObj.transform.rotation = Quaternion.LookRotation(
                        worldIconObj.transform.position - cam.transform.position, Vector3.up);
                }
            }
        }

        /// <summary>
        /// Distinct color coding:
        /// Raw Meat -> Bright Vivid Red
        /// Cooked Meat -> Rich Dark Brown
        /// Raw Veg -> Fresh Dark Green (Whole block)
        /// Chopped Veg -> Light Diced Green (Flat slice)
        /// Cheese -> Bright Yellow
        /// </summary>
        public static Color GetDefaultColor(IngredientType type, IngredientState state)
        {
            switch (type)
            {
                case IngredientType.Meat:
                    return state == IngredientState.Cooked
                        ? new Color(0.38f, 0.18f, 0.08f) // Cooked Brown
                        : new Color(0.85f, 0.15f, 0.15f); // Raw Red

                case IngredientType.Vegetable:
                    return state == IngredientState.Chopped
                        ? new Color(0.50f, 0.90f, 0.30f) // Chopped Diced Green
                        : new Color(0.18f, 0.78f, 0.25f); // Raw Fresh Green

                case IngredientType.Cheese:
                default:
                    return new Color(1.00f, 0.85f, 0.15f); // Cheese Yellow
            }
        }

        public static Vector3 GetDefaultScale(IngredientType type, IngredientState state)
        {
            switch (type)
            {
                case IngredientType.Meat:
                    return state == IngredientState.Cooked
                        ? new Vector3(0.45f, 0.18f, 0.45f) // Cooked steak shape
                        : new Vector3(0.40f, 0.25f, 0.40f); // Raw meat cutlet

                case IngredientType.Vegetable:
                    return state == IngredientState.Chopped
                        ? new Vector3(0.55f, 0.12f, 0.55f) // Chopped flat slice
                        : new Vector3(0.35f, 0.45f, 0.35f); // Raw vertical block

                case IngredientType.Cheese:
                default:
                    return new Vector3(0.35f, 0.35f, 0.35f);
            }
        }

        /// <summary>Spawns a primitive cube ingredient at the given position with state visuals and floating icon.</summary>
        public static Ingredient Spawn(IngredientType type, IngredientState state, Material material, Vector3 position, Vector3 scale)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = position;

            Ingredient ingredient = go.AddComponent<Ingredient>();
            ingredient.type = type;
            ingredient.state = state;
            ingredient.ApplyAppearance(material, scale);
            return ingredient;
        }

        public override string ToString()
        {
            return state + " " + type;
        }
    }
}
