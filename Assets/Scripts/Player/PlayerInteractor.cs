using ChefMachine.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChefMachine.Player
{
    /// <summary>
    /// Finds the closest station whose working-side InteractionZone the chef is
    /// standing in. Stations marked IAutoInteractable fire on approach; the rest
    /// wait for the E key.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float interactionRadius = 1.75f;
        [Tooltip("How far ahead of the player the detection sphere sits.")]
        [SerializeField] private float forwardOffset = 0.75f;
        [SerializeField] private LayerMask interactableLayers = ~0;

        [Header("Automatic Interaction")]
        [Tooltip("Shortest gap between two automatic interactions.")]
        [SerializeField] private float autoInteractCooldown = 0.2f;

        [Header("Debug")]
        [SerializeField] private bool drawGizmo = true;

        private readonly Collider[] hits = new Collider[16];
        private PlayerHands hands;

        // The situation the last automatic interaction left behind: while nothing
        // about it changes, the station is not triggered again.
        private IInteractable autoTarget;
        private string autoSituation;
        private float nextAutoTime;

        private void Awake()
        {
            hands = GetComponent<PlayerHands>();
        }

        private void OnDisable()
        {
            // Coming back from a pause or a restart should re-evaluate the station.
            autoTarget = null;
            autoSituation = null;
        }

        private void Update()
        {
            TryAutoInteract();

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            // wasPressedThisFrame fires once per press, so one key press = one interaction.
            if (!keyboard.eKey.wasPressedThisFrame) return;

            IInteractable target = FindClosestInteractable(false);
            if (target == null) return;

            target.Interact(gameObject);
        }

        /// <summary>
        /// Fires the closest automatic station once per situation - a new station,
        /// a change in what the chef carries, or a change in the station's own state.
        /// </summary>
        private void TryAutoInteract()
        {
            IInteractable target = FindClosestInteractable(true);

            if (target == null)
            {
                autoTarget = null;
                autoSituation = null;
                return;
            }

            string situation = DescribeSituation(target);
            if (ReferenceEquals(target, autoTarget) && situation == autoSituation) return;
            if (Time.time < nextAutoTime) return;

            target.Interact(gameObject);

            autoTarget = target;
            // Recorded after the interaction, so its own result does not retrigger it.
            autoSituation = DescribeSituation(target);
            nextAutoTime = Time.time + autoInteractCooldown;
        }

        /// <summary>Station state plus what the chef is holding, as one comparable string.</summary>
        private string DescribeSituation(IInteractable target)
        {
            string held = "empty";
            if (hands != null && hands.IsHoldingIngredient)
            {
                Ingredients.Ingredient ingredient = hands.CurrentIngredient;
                held = ingredient.GetInstanceID() + ":" + ingredient.Type + "/" + ingredient.State;
            }

            return target.InteractionPrompt + "|" + held;
        }

        private Vector3 DetectionOrigin
        {
            get { return transform.position + transform.forward * forwardOffset; }
        }

        private IInteractable FindClosestInteractable(bool automaticOnly)
        {
            Vector3 origin = DetectionOrigin;
            int count = Physics.OverlapSphereNonAlloc(
                origin, interactionRadius, hits, interactableLayers, QueryTriggerInteraction.Collide);

            IInteractable closest = null;
            float closestSqr = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                // Stations are reached only through their working-side zone, and
                // the chef has to be standing inside it - not merely near it.
                InteractionZone zone = hits[i].GetComponent<InteractionZone>();
                if (zone == null) continue;
                if (!Contains(hits[i], transform.position)) continue;

                IInteractable candidate = zone.Interactable;
                if (candidate == null) continue;
                if (automaticOnly && !(candidate is IAutoInteractable)) continue;

                float sqr = (hits[i].ClosestPoint(origin) - origin).sqrMagnitude;
                if (sqr >= closestSqr) continue;

                closestSqr = sqr;
                closest = candidate;
            }

            return closest;
        }

        /// <summary>True when the point is inside the collider (ClosestPoint returns it unchanged).</summary>
        private static bool Contains(Collider zoneCollider, Vector3 point)
        {
            return (zoneCollider.ClosestPoint(point) - point).sqrMagnitude < 0.0001f;
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmo) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(DetectionOrigin, interactionRadius);
        }
    }
}
