namespace ChefMachine.Core
{
    /// <summary>
    /// Marks a station whose single sensible action should fire as soon as the
    /// chef walks up to it, with no key press. Stations that offer a choice
    /// (the refrigerator) stay on the manual E action.
    /// </summary>
    public interface IAutoInteractable : IInteractable
    {
    }
}
