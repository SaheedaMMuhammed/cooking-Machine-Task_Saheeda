using ChefMachine.Core;
using ChefMachine.Ingredients;
using ChefMachine.Player;
using UnityEngine;

namespace ChefMachine.Orders
{
    /// <summary>
    /// A customer hatch. Holds at most one active order, accepts ingredients the
    /// order still needs, and reports completion. It never creates its own
    /// orders - OrderManager does that. Display lives in CustomerWindowUI.
    /// </summary>
    public class CustomerWindow : MonoBehaviour, IAutoInteractable
    {
        [Header("Visual Feedback")]
        [SerializeField] private Color completedColor = new Color(0.25f, 0.85f, 0.35f);

        private Order activeOrder;
        private MeshRenderer meshRenderer;
        private Material idleMaterial;

        /// <summary>Raised when the order at this window is finished. OrderManager listens.</summary>
        public System.Action<CustomerWindow, Order> OrderCompleted;

        public Order ActiveOrder { get { return activeOrder; } }
        public bool HasActiveOrder { get { return activeOrder != null && !activeOrder.IsComplete; } }

        public string InteractionPrompt
        {
            get
            {
                return HasActiveOrder
                    ? gameObject.name + " wants: " + activeOrder.Describe()
                    : gameObject.name + " (no order)";
            }
        }

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null) idleMaterial = meshRenderer.sharedMaterial;
        }

        /// <summary>Assigns an order to this window and starts its waiting clock.</summary>
        public void SetOrder(Order order)
        {
            activeOrder = order;
            if (activeOrder != null) activeOrder.Activate();

            ResetWindowMaterial();
        }

        /// <summary>Leaves the window empty - it accepts nothing until it gets a new order.</summary>
        public void ClearOrder()
        {
            activeOrder = null;
        }

        public void Interact(GameObject interactor)
        {
            PlayerHands hands = interactor.GetComponentInParent<PlayerHands>();
            if (hands == null)
            {
                Debug.LogWarning("CustomerWindow: interactor has no PlayerHands.", this);
                return;
            }

            if (!HasActiveOrder)
            {
                Debug.Log(gameObject.name + " has no active order.", this);
                return;
            }

            if (!hands.IsHoldingIngredient)
            {
                Debug.Log(gameObject.name + " wants: " + activeOrder.Describe(), this);
                return;
            }

            Ingredient held = hands.GetCurrentIngredient();

            if (!activeOrder.TryFulfill(held.Type, held.State))
            {
                Debug.Log(gameObject.name + " does not need " + held + " - keeping it in hand.", this);
                return;
            }

            Debug.Log("Delivered " + held + " to " + gameObject.name + ".", this);
            hands.ClearIngredient();

            if (activeOrder.IsComplete) CompleteOrder();
        }

        private void CompleteOrder()
        {
            Order finished = activeOrder;
            Debug.Log("Order completed! " + gameObject.name + " (" + finished.Describe() + ")", this);

            ClearOrder();
            TintWindow(completedColor);

            if (OrderCompleted != null)
            {
                OrderCompleted(this, finished);
            }
            else
            {
                Debug.LogWarning(gameObject.name + " has no OrderManager listening; no score awarded.", this);
            }
        }

        /// <summary>Back to the plain window look, e.g. when a new order arrives.</summary>
        private void ResetWindowMaterial()
        {
            if (meshRenderer == null || idleMaterial == null) return;
            meshRenderer.sharedMaterial = idleMaterial;
        }

        /// <summary>Completion feedback: the hatch flashes a color until its next order.</summary>
        private void TintWindow(Color tint)
        {
            if (meshRenderer == null) return;
            meshRenderer.material.color = tint;
        }
    }
}
