using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float baseMoveSpeed = 4.6f;
    [SerializeField] [Tooltip("Determines the responsiveness of movement. 1.0 = force is directly protiononal to distance to target speed. > 1.0 = creates a more punchy relationship; huge speed changes are done faster.")] 
    private float movePower = 1.9f;
    [SerializeField] [Tooltip("How fast the player speeds up; relative to movePower.")] private float accelerationStrength = 2.0f;
    [SerializeField] [Tooltip("How fast the player slows down; relative to movePower.")] private float decelerationStrength = 3.0f;


    public float MaxMoveSpeed => maxMoveSpeed;
    public float CurrentMoveSpeed => currentMoveVelocity;


    private CharacterController controller = null;

    private float maxMoveSpeed = 0.0f;
    private float moveSpeedMultiplier = 1.0f;
    private float currentMoveVelocity = 0.0f;
    private float moveInput = 0.0f;


    public void OnMoveInput(InputAction.CallbackContext context)
    {
        Vector2 rawMoveInput = context.ReadValue<Vector2>();
        moveInput = rawMoveInput.x; // Makes sure only the horizontal movement is used.
    }


    /// <summary>
    /// Adds a scalar that is used on the base move speed.
    /// Example: Adding 0.4 with the existing 1.1 multiplier will result in being 50% faster than base speed.
    /// </summary>
    public void AddSpeedMultiplier(float multiplier) => moveSpeedMultiplier += multiplier;

    /// <summary>
    /// Removes a scalar that is used on the base move speed.
    /// Example: Removing 0.5 from the existing 1.0 multiplier will result in being 50% slower than base speed.
    /// </summary>
    public void RemoveSpeedMultiplier(float multiplier) => moveSpeedMultiplier -= multiplier;


    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }


    private void Update()
    {
        maxMoveSpeed = baseMoveSpeed * moveSpeedMultiplier;
        Vector3 moveVelocity = CalculateMoveVelocity();

        controller.Move(moveVelocity * Time.deltaTime);
    }


    private Vector3 CalculateMoveVelocity()
    {
        bool isAccelerating = Mathf.Abs(moveInput) > float.Epsilon;
        float acceleration = isAccelerating ? accelerationStrength : decelerationStrength;

        float targetVelocity = moveInput * maxMoveSpeed;
        float velocityDifference = targetVelocity - currentMoveVelocity;
        float velocity = Mathf.Pow(Mathf.Abs(velocityDifference), movePower);

        currentMoveVelocity += Mathf.Sign(velocityDifference) * velocity * acceleration * Time.deltaTime;
        return Vector3.right * currentMoveVelocity;
    }
}
