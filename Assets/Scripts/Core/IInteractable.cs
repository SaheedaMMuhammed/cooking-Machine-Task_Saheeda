namespace ChefMachine.Core
{
    /// <summary>
    /// Implemented by anything the player can interact with (stations, windows, pickups).
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Short description shown to the player, e.g. "Refrigerator".</summary>
        string InteractionPrompt { get; }

        /// <summary>Called when the player interacts with this object.</summary>
        void Interact(UnityEngine.GameObject interactor);
    }
}
