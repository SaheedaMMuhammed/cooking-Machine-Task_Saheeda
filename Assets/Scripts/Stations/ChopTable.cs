using ChefMachine.Core;
using ChefMachine.Ingredients;
using ChefMachine.Player;
using UnityEngine;

namespace ChefMachine.Stations
{
    /// <summary>
    /// Takes one raw vegetable, chops it over a fixed duration, and holds the
    /// result until the chef has a free hand. Chopping runs whether or not the
    /// player stays nearby.
    /// </summary>
    public class ChopTable : MonoBehaviour, IAutoInteractable
    {
        [Header("Recipe")]
        [SerializeField] private IngredientType acceptedType = IngredientType.Vegetable;
        [SerializeField] private IngredientState acceptedState = IngredientState.Raw;
        [SerializeField] private IngredientState resultState = IngredientState.Chopped;
        [SerializeField] private float prepDuration = 2f;

        [Header("Ingredient Visual")]
        [SerializeField] private Material resultMaterial;
        [SerializeField] private Vector3 resultScale = new Vector3(0.5f, 0.12f, 0.5f);
        [Tooltip("Where the ingredient sits while it is on the table.")]
        [SerializeField] private Vector3 itemOffset = new Vector3(0f, 0.65f, 0f);

        [Header("Table Visual")]
        [SerializeField] private Material busyMaterial;
        [SerializeField] private Material readyMaterial;

        private Ingredient item;
        private float remaining;
        private bool processing;
        private MeshRenderer tableRenderer;
        private Material idleMaterial;

        public bool IsOccupied { get { return item != null; } }
        public bool IsProcessing { get { return processing; } }
        public Ingredient Item { get { return item; } }

        /// <summary>How long a full chop takes, in seconds.</summary>
        public float PrepDuration { get { return prepDuration; } }

        /// <summary>Seconds left on the current chop; 0 when idle. Read-only for the UI.</summary>
        public float RemainingTime { get { return processing ? Mathf.Max(0f, remaining) : 0f; } }

        /// <summary>Chop progress from 0 (just started) to 1 (finished).</summary>
        public float Progress01
        {
            get
            {
                if (!processing || prepDuration <= 0f) return 0f;
                return Mathf.Clamp01(1f - (remaining / prepDuration));
            }
        }

        public string InteractionPrompt
        {
            get
            {
                if (processing) return "Chop Table (chopping...)";
                if (item != null) return "Chop Table (" + item + " ready)";
                return "Chop Table";
            }
        }

        private void Awake()
        {
            tableRenderer = GetComponent<MeshRenderer>();
            if (tableRenderer != null) idleMaterial = tableRenderer.sharedMaterial;
        }

        private void Update()
        {
            if (!processing) return;

            remaining -= Time.deltaTime;
            if (remaining > 0f) return;

            FinishPrep();
        }

        /// <summary>
        /// Empties the table and stops any chop in progress. Used by the session
        /// when a game ends or a new one starts; normal chopping is untouched.
        /// </summary>
        public void ResetStation()
        {
            if (item != null) Destroy(item.gameObject);

            item = null;
            processing = false;
            remaining = 0f;
            SetTableMaterial(idleMaterial);
        }

        public void Interact(GameObject interactor)
        {
            PlayerHands hands = interactor.GetComponentInParent<PlayerHands>();
            if (hands == null)
            {
                Debug.LogWarning("ChopTable: interactor has no PlayerHands.", this);
                return;
            }

            if (processing)
            {
                Debug.Log("Chop Table is busy - " + remaining.ToString("F1") + "s left.", this);
                return;
            }

            if (item != null)
            {
                TryHandOver(hands);
                return;
            }

            TryAccept(hands);
        }

        /// <summary>Gives the finished ingredient back to the chef, if a hand is free.</summary>
        private void TryHandOver(PlayerHands hands)
        {
            if (hands.IsHoldingIngredient)
            {
                Debug.Log("Hands full - " + item + " stays on the Chop Table.", this);
                return;
            }

            Ingredient finished = item;
            item = null;
            SetTableMaterial(idleMaterial);

            if (!hands.TryPickupIngredient(finished))
            {
                // Put it back rather than losing it.
                item = finished;
                SetTableMaterial(readyMaterial);
                return;
            }

            Debug.Log("Picked up " + finished + " from the Chop Table.", this);
        }

        /// <summary>Accepts a valid raw ingredient and starts the prep timer.</summary>
        private void TryAccept(PlayerHands hands)
        {
            if (!hands.IsHoldingIngredient) return;

            Ingredient held = hands.GetCurrentIngredient();
            if (held.Type != acceptedType || held.State != acceptedState)
            {
                Debug.Log("Chop Table only takes " + acceptedState + " " + acceptedType +
                          " - keeping " + held + " in hand.", this);
                return;
            }

            item = hands.ReleaseIngredient();
            PlaceOnTable(item);

            processing = true;
            remaining = prepDuration;
            SetTableMaterial(busyMaterial);

            Debug.Log("Chopping " + item + " (" + prepDuration.ToString("F1") + "s).", this);
        }

        private void FinishPrep()
        {
            processing = false;
            remaining = 0f;

            item.SetState(resultState);
            item.ApplyAppearance(resultMaterial, resultScale);
            SetTableMaterial(readyMaterial);

            Debug.Log(item + " is ready on the Chop Table.", this);
        }

        private void PlaceOnTable(Ingredient ingredient)
        {
            // Not parented: the table cube is non-uniformly scaled and would squash the item.
            ingredient.transform.SetParent(null, true);
            ingredient.transform.position = transform.position + itemOffset;
            ingredient.transform.rotation = Quaternion.identity;

            Collider col = ingredient.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        private void SetTableMaterial(Material material)
        {
            if (tableRenderer == null || material == null) return;
            tableRenderer.sharedMaterial = material;
        }
    }
}
