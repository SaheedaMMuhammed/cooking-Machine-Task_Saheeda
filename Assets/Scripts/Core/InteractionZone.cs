using UnityEngine;

namespace ChefMachine.Core
{
    /// <summary>
    /// The working side of a station: a small trigger volume the chef must stand
    /// in for the station to react. Detection only - it never blocks movement.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractionZone : MonoBehaviour
    {
        [Tooltip("The station this zone belongs to. Must implement IInteractable.")]
        [SerializeField] private MonoBehaviour station;

        private IInteractable interactable;

        /// <summary>The station reached through this zone, or null when misconfigured.</summary>
        public IInteractable Interactable
        {
            get
            {
                if (interactable == null) interactable = station as IInteractable;
                return interactable;
            }
        }

        private void Awake()
        {
            Collider zoneCollider = GetComponent<Collider>();
            if (zoneCollider != null) zoneCollider.isTrigger = true;

            if (Interactable == null)
            {
                Debug.LogWarning(name + ": no station assigned that implements IInteractable.", this);
            }
        }

        private void OnDrawGizmosSelected()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null) return;

            Gizmos.color = new Color(0.3f, 0.9f, 0.5f, 0.9f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}
