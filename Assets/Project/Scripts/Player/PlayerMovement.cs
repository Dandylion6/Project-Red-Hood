using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] [Min(0.1f)] private float baseMoveSpeed = 4.6f;
    [SerializeField] [Tooltip("Determines the responsiveness of movement. 1.0 = force is directly protiononal to distance to target speed. > 1.0 = creates a more punchy relationship; huge speed changes are done faster.")] 
    [Range(0.5f, 2.0f)] private float movePower = 1.9f;
    [Space]
    [SerializeField] [Tooltip("How fast the player speeds up; relative to movePower.")] [Min(0.1f)] private float accelerationStrength = 2.0f;
    [SerializeField] [Tooltip("How fast the player slows down; relative to movePower.")] [Min(0.1f)] private float decelerationStrength = 3.0f;

    [Header("Jump Settings")]
    [SerializeField] private float maxJumpHeight = 1.2f;
    [SerializeField] private float mass = 1.0f;
    [SerializeField] [Tooltip("How much air control the player gets added in percentage. Example: -40% will mean the player will be able to move 60% of the base move speed.")] private float airStrafeMovePercentage = -40.0f;
    [SerializeField] private Vector3 groundCheckPosition = Vector3.zero;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask = new();


    public float MaxMoveSpeed => maxMoveSpeed;
    public float CurrentMoveSpeed => currentMoveVelocity;
    public bool IsGrounded => isGrounded;


    private CharacterController controller = null;

    private float maxMoveSpeed = 0.0f;
    private float moveSpeedPercentage = 100.0f;
    private float currentMoveVelocity = 0.0f;
    private float moveInput = 0.0f;

    private float verticalVelocity = Physics.gravity.y;
    private bool isGrounded = false;


    public void OnMoveInput(InputAction.CallbackContext context)
    {
        Vector2 rawMoveInput = context.ReadValue<Vector2>();
        moveInput = rawMoveInput.x; // Makes sure only the horizontal movement is used.
    }


    /// <summary>
    /// Adds percentage points to the speed multiplier.
    /// The multiplier starts at 100 (= base speed). Each point shifts it by 1%.
    /// e.g. +50 points → 150% of base speed (1.5× faster).
    /// </summary>
    public void AddSpeedPercentageChange(float percentage) => moveSpeedPercentage += percentage;

    /// <summary>
    /// Removes percentage points from the speed multiplier.
    /// e.g. −50 points from a 100-point multiplier → 50% of base speed (half speed).
    /// </summary>
    public void RemoveSpeedPercentageChange(float percentage) => moveSpeedPercentage -= percentage;


    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }


    private void Update()
    {
        UpdateGravity();

        maxMoveSpeed = baseMoveSpeed * moveSpeedPercentage * 0.01f;
        Vector3 moveVelocity = CalculateMoveVelocity();

        moveVelocity += Vector3.up * verticalVelocity;
        controller.Move(moveVelocity * Time.deltaTime);
    }


    /// <summary>
    /// This is code that I've modified based on Dawnosaur's video on Platformer movement https://www.youtube.com/watch?v=KKGdDBFcu0Q.
    /// I've used this system enough times to be able to make it by hand and see it as quite a robust method to move the player.
    /// </summary>
    /// <returns>
    /// A Vector3 that moves the player based on input.
    /// </returns>
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


    /// <summary>
    /// Applies a down force based on mass and gravity after checking whether the player is grounded.
    /// </summary>
    private void UpdateGravity()
    {
        Vector3 checkPoint = transform.position + groundCheckPosition;
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(checkPoint, groundCheckRadius, groundMask);

        bool stateChanged = isGrounded != wasGrounded;
        if (isGrounded)
        {
            verticalVelocity = Physics.gravity.y; // This is to make sure the player 'sticks' to the ground.
            if (stateChanged) RemoveSpeedPercentageChange(airStrafeMovePercentage);
            return;
        }

        verticalVelocity += mass * Physics.gravity.y * Time.deltaTime;
        if (stateChanged) AddSpeedPercentageChange(airStrafeMovePercentage);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + groundCheckPosition, groundCheckRadius);
    }
}
