using ChefMachine.Core;
using ChefMachine.Ingredients;
using ChefMachine.Player;
using UnityEngine;

namespace ChefMachine.Stations
{
    /// <summary>
    /// One cooking position on the stove. Owns its own countdown so the two
    /// slots run independently of each other and of the player.
    /// </summary>
    [System.Serializable]
    public class CookingSlot
    {
        [Tooltip("Where the ingredient sits, relative to the stove.")]
        public Vector3 Offset = Vector3.zero;

        private Ingredient ingredient;
        private float duration;
        private float remaining;
        private bool cooking;

        public Ingredient Ingredient { get { return ingredient; } }
        public float Duration { get { return duration; } }
        public bool IsOccupied { get { return ingredient != null; } }
        public bool IsCooking { get { return cooking; } }
        public bool IsFinished { get { return ingredient != null && !cooking; } }
        public float RemainingTime { get { return cooking ? Mathf.Max(0f, remaining) : 0f; } }

        public float Progress01
        {
            get
            {
                if (!cooking || duration <= 0f) return 0f;
                return Mathf.Clamp01(1f - (remaining / duration));
            }
        }

        public void Begin(Ingredient item, float cookDuration)
        {
            ingredient = item;
            duration = cookDuration;
            remaining = cookDuration;
            cooking = true;
        }

        /// <summary>Holds a finished ingredient without running a timer.</summary>
        public void Hold(Ingredient item)
        {
            ingredient = item;
            cooking = false;
            remaining = 0f;
        }

        /// <summary>Advances the countdown. Returns true on the frame cooking completes.</summary>
        public bool Tick(float deltaTime)
        {
            if (!cooking) return false;

            remaining -= deltaTime;
            if (remaining > 0f) return false;

            remaining = 0f;
            cooking = false;
            return true;
        }

        public Ingredient Take()
        {
            Ingredient taken = ingredient;
            ingredient = null;
            cooking = false;
            remaining = 0f;
            return taken;
        }
    }

    /// <summary>
    /// Two-slot stove. Takes raw meat, cooks each piece for a fixed time, and
    /// holds the cooked result until the chef collects it.
    /// </summary>
    public class Stove : MonoBehaviour, IAutoInteractable
    {
        [Header("Recipe")]
        [SerializeField] private IngredientType acceptedType = IngredientType.Meat;
        [SerializeField] private IngredientState acceptedState = IngredientState.Raw;
        [SerializeField] private IngredientState resultState = IngredientState.Cooked;
        [SerializeField] private float cookDuration = 6f;

        [Header("Slots")]
        [SerializeField] private CookingSlot slotOne = new CookingSlot();
        [SerializeField] private CookingSlot slotTwo = new CookingSlot();

        [Header("Cooked Visual")]
        [SerializeField] private Material cookedMaterial;
        [SerializeField] private Vector3 cookedScale = new Vector3(0.45f, 0.22f, 0.45f);

        [Header("Stove Visual")]
        [SerializeField] private Material busyMaterial;
        [SerializeField] private Material readyMaterial;

        private MeshRenderer stoveRenderer;
        private Material idleMaterial;

        public const int SlotCount = 2;

        public CookingSlot SlotOne { get { return slotOne; } }
        public CookingSlot SlotTwo { get { return slotTwo; } }

        /// <summary>Read-only slot access for UI. Index 0 or 1.</summary>
        public CookingSlot GetSlot(int index)
        {
            return index == 0 ? slotOne : slotTwo;
        }

        public bool IsCooking { get { return slotOne.IsCooking || slotTwo.IsCooking; } }
        public bool HasFinishedFood { get { return slotOne.IsFinished || slotTwo.IsFinished; } }

        public string InteractionPrompt
        {
            get
            {
                if (HasFinishedFood) return "Stove (" + resultState + " " + acceptedType + " ready)";
                if (IsCooking) return "Stove (cooking...)";
                return "Stove";
            }
        }

        private void Awake()
        {
            stoveRenderer = GetComponent<MeshRenderer>();
            if (stoveRenderer != null) idleMaterial = stoveRenderer.sharedMaterial;
        }

        private void Update()
        {
            // Timers live here, not on the player: cooking runs wherever the chef is.
            if (slotOne.Tick(Time.deltaTime)) FinishSlot(slotOne);
            if (slotTwo.Tick(Time.deltaTime)) FinishSlot(slotTwo);
        }

        /// <summary>
        /// Empties both slots and stops any cooking. Used by the session when a
        /// game ends or a new one starts; normal cooking is untouched.
        /// </summary>
        public void ResetStation()
        {
            ClearSlot(slotOne);
            ClearSlot(slotTwo);
            RefreshStoveMaterial();
        }

        private void ClearSlot(CookingSlot slot)
        {
            Ingredient removed = slot.Take();
            if (removed != null) Destroy(removed.gameObject);
        }

        public void Interact(GameObject interactor)
        {
            PlayerHands hands = interactor.GetComponentInParent<PlayerHands>();
            if (hands == null)
            {
                Debug.LogWarning("Stove: interactor has no PlayerHands.", this);
                return;
            }

            if (!hands.IsHoldingIngredient)
            {
                TryHandOver(hands);
                return;
            }

            TryAccept(hands);
        }

        /// <summary>Gives the chef a finished piece from the first slot that has one.</summary>
        private void TryHandOver(PlayerHands hands)
        {
            CookingSlot slot = FindFinishedSlot();
            if (slot == null)
            {
                if (IsCooking) Debug.Log("Stove is still cooking.", this);
                return;
            }

            Ingredient finished = slot.Take();
            if (!hands.TryPickupIngredient(finished))
            {
                // Put it straight back rather than losing it.
                slot.Hold(finished);
                PlaceInSlot(finished, slot);
                return;
            }

            RefreshStoveMaterial();
            Debug.Log("Picked up " + finished + " from the Stove.", this);
        }

        /// <summary>Puts raw meat into the first free slot and starts its timer.</summary>
        private void TryAccept(PlayerHands hands)
        {
            Ingredient held = hands.GetCurrentIngredient();
            if (held.Type != acceptedType || held.State != acceptedState)
            {
                Debug.Log("Stove only takes " + acceptedState + " " + acceptedType +
                          " - keeping " + held + " in hand.", this);
                return;
            }

            CookingSlot slot = FindEmptySlot();
            if (slot == null)
            {
                Debug.Log("Both stove slots are full - keeping " + held + " in hand.", this);
                return;
            }

            Ingredient item = hands.ReleaseIngredient();
            slot.Begin(item, cookDuration);
            PlaceInSlot(item, slot);
            RefreshStoveMaterial();

            Debug.Log("Cooking " + item + " in slot " + SlotNumber(slot) +
                      " (" + cookDuration.ToString("F1") + "s).", this);
        }

        private void FinishSlot(CookingSlot slot)
        {
            Ingredient item = slot.Ingredient;
            if (item == null) return;

            item.SetState(resultState);
            item.ApplyAppearance(cookedMaterial, cookedScale);
            PlaceInSlot(item, slot);
            RefreshStoveMaterial();

            Debug.Log(item + " is ready in stove slot " + SlotNumber(slot) + ".", this);
        }

        private CookingSlot FindEmptySlot()
        {
            if (!slotOne.IsOccupied) return slotOne;
            if (!slotTwo.IsOccupied) return slotTwo;
            return null;
        }

        private CookingSlot FindFinishedSlot()
        {
            if (slotOne.IsFinished) return slotOne;
            if (slotTwo.IsFinished) return slotTwo;
            return null;
        }

        private int SlotNumber(CookingSlot slot)
        {
            return slot == slotOne ? 1 : 2;
        }

        private void PlaceInSlot(Ingredient ingredient, CookingSlot slot)
        {
            // Unparented: the stove cube is non-uniformly scaled and would squash the item.
            ingredient.transform.SetParent(null, true);
            ingredient.transform.position = transform.position + slot.Offset;
            ingredient.transform.rotation = Quaternion.identity;

            Collider col = ingredient.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        private void RefreshStoveMaterial()
        {
            if (stoveRenderer == null) return;

            Material target = idleMaterial;
            if (IsCooking) target = busyMaterial;
            else if (HasFinishedFood) target = readyMaterial;

            if (target != null) stoveRenderer.sharedMaterial = target;
        }
    }
}
