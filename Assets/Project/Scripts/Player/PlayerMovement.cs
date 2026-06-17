using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    private const float SPEED_PERCENTAGE_POWER_MULT = 0.5f; // How much speed scales with power.


    [Header("Movement Settings")]
    [SerializeField] [Min(0.1f)] private float baseMoveSpeed = 4.6f;
    [SerializeField] [Tooltip("Determines the responsiveness of movement. 1.0 = force is directly protiononal to distance to target speed. > 1.0 = creates a more punchy relationship; huge speed changes are done faster.")] 
    [Range(0.5f, 2.0f)] private float movePower = 1.9f;
    [Space]
    [SerializeField] [Tooltip("How fast the player speeds up; relative to movePower.")] [Min(0.1f)] private float accelerationStrength = 2.0f;
    [SerializeField] [Tooltip("How fast the player slows down; relative to movePower.")] [Min(0.1f)] private float decelerationStrength = 3.0f;

    [Header("Jump Settings")]
    [SerializeField] private float maxJumpHeight = 1.2f;
    [SerializeField] [Tooltip("The time window in which a jump will occur after pressing whenever possible in seconds.")] [Min(0.0f)] private float jumpPressTimeWindow = 0.2f;
    [SerializeField] [Tooltip("The time window in which you can still jump after not being grounded in seconds")] [Min(0.0f)] private float coyoteTime = 0.1f;
    [SerializeField] [Tooltip("Applies a percentage debuff to max speed and power while in the air.")] [Range(0.0f, 100.0f)] private float airstrafeMoveDebuff = 40.0f;
    [SerializeField] private float fallingGravityMultiplier = 1.5f;
    [SerializeField] private float jumpCutOffMultiplier = 0.2f;
    [SerializeField] private Vector3 groundCheckPosition = Vector3.zero;
    [SerializeField] [Min(0.01f)] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask = new();


    public Vector2 Velocity => velocity;
    public float MaxMoveSpeed => maxMoveSpeed;
    public bool IsJumping => isJumping;
    public bool IsGrounded => isGrounded;

    private Rigidbody body = null;

    private Vector2 velocity = Vector2.zero;
    private float maxMoveSpeed = 0.0f;
    private float moveSpeedPercentage = 100.0f;
    private float moveInput = 0.0f;
    private bool isFacingRight = true;

    private float timeSinceGrounded = 0.0f;
    private float jumpPressedTimeLeft = 0.0f; // Will give jumping some time left to activate after pressing.
    private bool isJumping = false;
    private bool isGrounded = false;
    private bool isAirstrafeDebuffActive = false;


    public void OnMoveInput(InputAction.CallbackContext context)
    {
        Vector2 rawMoveInput = context.ReadValue<Vector2>();
        moveInput = rawMoveInput.x; // Makes sure only the horizontal movement is used.
    }


    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.performed) jumpPressedTimeLeft = jumpPressTimeWindow;
        else if (context.canceled && isJumping)
        {
            // Reduces the velocity to end the jump early.
            body.linearVelocity = new(body.linearVelocity.x, body.linearVelocity.y * jumpCutOffMultiplier, body.linearVelocity.z);
        }
    }


    public void AddSpeedBuff(float percentage) => moveSpeedPercentage += percentage;
    public void AddSpeedDebuff(float percentage) => moveSpeedPercentage -= percentage;

    public void RemoveSpeedBuff(float percentage) => moveSpeedPercentage -= percentage;
    public void RemoveSpeedDebuff(float percentage) => moveSpeedPercentage += percentage;



    private void Start()
    {
        body = GetComponent<Rigidbody>();
    }


    private void Update()
    {
        CheckGround();
        UpdateRotation();
    }


    private void CheckGround()
    {
        Vector3 checkPoint = transform.position + groundCheckPosition;
        isGrounded = Physics.CheckSphere(checkPoint, groundCheckRadius, groundMask);
        timeSinceGrounded = isGrounded ? 0.0f : timeSinceGrounded + Time.deltaTime;
    }


    private void UpdateRotation()
    {
        bool isMoving = Mathf.Abs(moveInput) > float.Epsilon;
        if (isMoving)
            isFacingRight = moveInput > 0.0f;

        transform.rotation = isFacingRight ? Quaternion.Euler(0.0f, 90.0f, 0.0f) : Quaternion.Euler(0.0f, -90.0f, 0.0f);
    }


    private void FixedUpdate()
    {
        UpdateJump();
        UpdateAirStrafe();

        maxMoveSpeed = baseMoveSpeed * moveSpeedPercentage * 0.01f;
        Vector3 moveForce = CalculateMoveForce();

        body.AddForce(moveForce);
        velocity = new(body.linearVelocity.x, body.linearVelocity.y);
    }

    
    private bool CanJump()
    {
        if (isJumping) return false;
        if (jumpPressedTimeLeft <= float.Epsilon) return false;
        if (timeSinceGrounded > coyoteTime) return false;
        return true;
    }


    private void UpdateJump()
    {
        jumpPressedTimeLeft = Mathf.Max(jumpPressedTimeLeft - Time.fixedDeltaTime, 0.0f);

        bool isFalling = body.linearVelocity.y <= float.Epsilon;
        if (isFalling)
        {
            isJumping = false;
            Vector3 fallingGravity = (fallingGravityMultiplier - 1.0f) * Time.deltaTime * Physics.gravity;
            body.linearVelocity += fallingGravity;
        }

        if (!CanJump()) return;

        jumpPressedTimeLeft = 0.0f;
        Jump();
    }


    private void Jump()
    {
        isJumping = true;
        float jumpVelocity = Mathf.Sqrt(2.0f * Mathf.Abs(Physics.gravity.y) * maxJumpHeight);
        body.linearVelocity = new(body.linearVelocity.x, jumpVelocity, body.linearVelocity.z);
    }


    private void UpdateAirStrafe()
    {
        if (isGrounded && isAirstrafeDebuffActive)
        {
            RemoveSpeedDebuff(airstrafeMoveDebuff);
            isAirstrafeDebuffActive = false;
            return;
        }
        
        if (!isGrounded && !isJumping && !isAirstrafeDebuffActive)
        {
            AddSpeedDebuff(airstrafeMoveDebuff);
            isAirstrafeDebuffActive = true;
        }
    }


    /// <summary>
    /// This is code that I've modified based on Dawnosaur's video on Platformer movement https://www.youtube.com/watch?v=KKGdDBFcu0Q.
    /// I've used this system enough times to be able to make it by hand and see it as quite a robust method to move the player.
    /// </summary>
    /// <returns>
    /// A Vector3 that moves the player based on input.
    /// </returns>
    private Vector3 CalculateMoveForce()
    {
        bool isAccelerating = Mathf.Abs(moveInput) > float.Epsilon;
        float acceleration = isAccelerating ? accelerationStrength : decelerationStrength;

        float targetVelocity = moveInput * maxMoveSpeed;
        float velocityDifference = targetVelocity - body.linearVelocity.x;

        float powerMultiplier = moveSpeedPercentage * 0.01f * SPEED_PERCENTAGE_POWER_MULT;
        float velocity = Mathf.Pow(Mathf.Abs(velocityDifference), movePower * powerMultiplier);
        float force = acceleration * Mathf.Sign(velocityDifference) * velocity;

        return force * Vector3.right;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + groundCheckPosition, groundCheckRadius);
    }
}
