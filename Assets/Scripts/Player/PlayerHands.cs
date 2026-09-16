using ChefMachine.Ingredients;
using UnityEngine;

namespace ChefMachine.Player
{
    /// <summary>
    /// The chef carries exactly one ingredient at a time.
    /// The held ingredient is parented to a hold anchor in front of the player.
    /// </summary>
    public class PlayerHands : MonoBehaviour
    {
        [Header("Hold Point")]
        [Tooltip("Where the held ingredient is parented. Created automatically if empty.")]
        [SerializeField] private Transform holdAnchor;
        [SerializeField] private Vector3 holdOffset = new Vector3(0f, 0.35f, 0.6f);

        private Ingredient held;

        public bool IsHoldingIngredient { get { return held != null; } }
        public Ingredient CurrentIngredient { get { return held; } }

        private void Awake()
        {
            if (holdAnchor == null)
            {
                GameObject anchor = new GameObject("HoldAnchor");
                anchor.transform.SetParent(transform, false);
                anchor.transform.localPosition = holdOffset;
                holdAnchor = anchor.transform;
            }
        }

        /// <summary>Takes the ingredient if the hands are empty. Returns false when already full.</summary>
        public bool TryPickupIngredient(Ingredient ingredient)
        {
            if (ingredient == null) return false;
            if (IsHoldingIngredient) return false;

            held = ingredient;

            Transform t = ingredient.transform;
            t.SetParent(holdAnchor, false);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;

            // A held ingredient should not push the player or block interaction checks.
            Collider col = ingredient.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            return true;
        }

        /// <summary>
        /// Hands the ingredient over without destroying it - used when a station
        /// takes it for preparation. Returns null when the hands were empty.
        /// </summary>
        public Ingredient ReleaseIngredient()
        {
            if (held == null) return null;

            Ingredient released = held;
            held = null;

            released.transform.SetParent(null, true);

            Collider col = released.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            return released;
        }

        /// <summary>Removes and destroys the held ingredient (consumed or binned).</summary>
        public void ClearIngredient()
        {
            if (held == null) return;

            Destroy(held.gameObject);
            held = null;
        }

        /// <summary>True when holding an ingredient of this type, in any state.</summary>
        public bool HasIngredient(IngredientType type)
        {
            return held != null && held.Type == type;
        }

        /// <summary>True when holding an ingredient of this type in this exact state.</summary>
        public bool HasIngredient(IngredientType type, IngredientState state)
        {
            return held != null && held.Type == type && held.State == state;
        }

        public Ingredient GetCurrentIngredient()
        {
            return held;
        }
    }
}
