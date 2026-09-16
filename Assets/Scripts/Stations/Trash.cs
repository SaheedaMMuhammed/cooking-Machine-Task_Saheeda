using ChefMachine.Core;
using ChefMachine.Ingredients;
using ChefMachine.Player;
using UnityEngine;

namespace ChefMachine.Stations
{
    /// <summary>
    /// Bin. Walking up with an ingredient throws it away; empty hands do nothing.
    /// </summary>
    public class Trash : MonoBehaviour, IAutoInteractable
    {
        public string InteractionPrompt
        {
            get { return "Trash"; }
        }

        public void Interact(GameObject interactor)
        {
            PlayerHands hands = interactor.GetComponentInParent<PlayerHands>();
            if (hands == null)
            {
                Debug.LogWarning("Trash: interactor has no PlayerHands.", this);
                return;
            }

            if (!hands.IsHoldingIngredient) return;

            Ingredient discarded = hands.GetCurrentIngredient();
            Debug.Log("Binned " + discarded + ".", this);
            hands.ClearIngredient();
        }
    }
}
