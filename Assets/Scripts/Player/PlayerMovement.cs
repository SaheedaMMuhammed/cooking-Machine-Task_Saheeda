using UnityEngine;
using UnityEngine.InputSystem;

namespace ChefMachine.Player
{
    /// <summary>
    /// Top-down WASD / arrow-key movement on the XZ plane using a CharacterController.
    /// Keeps the chef upright and blocked by kitchen walls.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [Tooltip("How quickly the player reaches target speed. Higher = snappier.")]
        [SerializeField] private float acceleration = 40f;
        [Tooltip("Degrees per second the chef turns toward the movement direction.")]
        [SerializeField] private float turnSpeed = 900f;

        [Header("Grounding")]
        [Tooltip("Constant downward force so the player stays on the floor.")]
        [SerializeField] private float gravity = 20f;

        private CharacterController controller;
        private Vector3 horizontalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Vector3 input = ReadMoveInput();
            Vector3 target = input * moveSpeed;

            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity, target, acceleration * Time.deltaTime);

            Vector3 motion = horizontalVelocity;
            motion.y = -gravity;

            controller.Move(motion * Time.deltaTime);

            if (input.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(input, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, look, turnSpeed * Time.deltaTime);
            }
        }

        /// <summary>Returns a normalized XZ direction from WASD / arrow keys.</summary>
        private static Vector3 ReadMoveInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return Vector3.zero;

            float x = 0f;
            float z = 0f;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) z -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) z += 1f;

            Vector3 dir = new Vector3(x, 0f, z);
            return dir.sqrMagnitude > 1f ? dir.normalized : dir;
        }
    }
}
