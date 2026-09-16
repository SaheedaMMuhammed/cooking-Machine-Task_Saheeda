using System.Collections.Generic;
using ChefMachine.Ingredients;
using ChefMachine.Player;
using UnityEngine;

namespace ChefMachine.Stations
{
    /// <summary>
    /// Endless source of raw ingredients. Clicking an option in the world-space
    /// selector takes that ingredient straight away - no key press. The choice is
    /// per visit: it clears on pickup and when the chef walks away.
    /// </summary>
    public class Refrigerator : MonoBehaviour
    {
        [Header("Selection")]
        [Tooltip("Working-side volume the chef must stand in for the selector to appear.")]
        [SerializeField] private Collider usableZone;
        [Tooltip("Fallback radius used only when no usable zone is assigned.")]
        [SerializeField] private float selectionRadius = 3f;

        [Header("Visuals")]
        [Tooltip("How each ingredient type looks when taken out of the fridge.")]
        [SerializeField] private List<IngredientVisual> ingredientVisuals = new List<IngredientVisual>();

        private PlayerHands hands;
        private PlayerInteractor interactor;

        // Runtime only: nothing is pre-selected when a session or a visit starts.
        private bool hasSelection;
        private IngredientType selectedType;

        public float SelectionRadius { get { return selectionRadius; } }
        public bool HasSelection { get { return hasSelection; } }
        public IngredientType SelectedType { get { return selectedType; } }

        /// <summary>True while the chef stands on the usable side of the fridge.</summary>
        public bool PlayerInRange
        {
            get
            {
                if (hands == null) return false;

                Vector3 chef = hands.transform.position;
                if (usableZone != null) return usableZone.bounds.Contains(chef);

                return Vector3.Distance(chef, transform.position) <= selectionRadius;
            }
        }

        /// <summary>
        /// True while the chef may use the selector: close enough, and gameplay input
        /// is on (the session disables the interactor on pause / start / game over).
        /// </summary>
        public bool SelectionAvailable
        {
            get { return PlayerInRange && (interactor == null || interactor.enabled); }
        }

        private void Awake()
        {
            hands = FindFirstObjectByType<PlayerHands>();
            if (hands != null) interactor = hands.GetComponent<PlayerInteractor>();
        }

        private void Update()
        {
            // Walking away forgets the choice, so the next visit starts blank.
            if (!SelectionAvailable) ClearSelection();
        }

        /// <summary>
        /// Picks an ingredient and hands it over immediately. Hands full: nothing
        /// is spawned and the refusal is logged, exactly as before.
        /// </summary>
        public void SelectAndTake(IngredientType type)
        {
            if (!SelectionAvailable) return;

            hasSelection = true;
            selectedType = type;

            if (hands == null)
            {
                Debug.LogWarning("Refrigerator: no PlayerHands in the scene.", this);
                return;
            }

            if (hands.IsHoldingIngredient)
            {
                Debug.Log("Hands full - already holding " + hands.CurrentIngredient + ".", this);
                return;
            }

            IngredientVisual visual = IngredientVisual.Find(ingredientVisuals, type);

            Ingredient ingredient = Ingredient.Spawn(
                type,
                IngredientRules.SpawnState(type),
                visual != null ? visual.Material : null,
                transform.position,
                visual != null ? visual.Scale : new Vector3(0.35f, 0.35f, 0.35f));

            if (!hands.TryPickupIngredient(ingredient))
            {
                Destroy(ingredient.gameObject);
                return;
            }

            Debug.Log("Picked up " + ingredient + " from the Refrigerator.", this);
            ClearSelection();
        }

        public void ClearSelection()
        {
            hasSelection = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.6f);

            if (usableZone != null)
            {
                Gizmos.DrawWireCube(usableZone.bounds.center, usableZone.bounds.size);
                return;
            }

            Gizmos.DrawWireSphere(transform.position, selectionRadius);
        }
    }
}
