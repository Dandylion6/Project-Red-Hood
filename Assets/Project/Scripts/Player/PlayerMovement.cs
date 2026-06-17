using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using static UnityEngine.CullingGroup;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private const float SPEED_PERCENTAGE_POWER_MULT = 0.5f; // How much speed scales with power.


    public enum State
    {
        Grounded,
        Jumping,
        Falling
    }


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
    [SerializeField] [Tooltip("The window in which a jump will occur after pressing whenever possible in seconds.")] private float jumpPressTimeWindow = 0.2f;
    [SerializeField] [Tooltip("The seconds that ground checks will be ignored after jumping.")] private float groundCheckCooldown = 0.3f;
    [SerializeField] [Tooltip("Applies a percentage debuff to max speed and power while in the air.")] private float airstrafeMoveDebuff = 40.0f;
    [SerializeField] private Vector3 groundCheckPosition = Vector3.zero;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask = new();
    [SerializeField] [Tooltip("How much gravity to have after falling.")] private float fallingGravityMultiplier = 1.2f;


    public float MaxMoveSpeed => maxMoveSpeed;
    public float CurrentMoveSpeed => Mathf.Abs(currentMoveVelocity);
    public Vector2 Velocity => new(controller.velocity.x, controller.velocity.y);
    public bool IsGrounded => state == State.Grounded;
    public bool IsJumping => state == State.Jumping;
    public bool IsFalling => state == State.Falling;


    private CharacterController controller = null;

    private float maxMoveSpeed = 0.0f;
    private float moveSpeedPercentage = 100.0f;
    private float currentMoveVelocity = 0.0f;
    private float moveInput = 0.0f;
    private bool facingRight = true;

    private float verticalVelocity = Physics.gravity.y;
    private float jumpPressedTimeLeft = 0.0f; // Will give jumping some time left to activate after pressing.
    private float groundCheckIgnoreLeft = 0.0f;
    private State state = State.Falling;
    private bool isAirstrafeDebuffActive = false;


    public void OnMoveInput(InputAction.CallbackContext context)
    {
        Vector2 rawMoveInput = context.ReadValue<Vector2>();
        moveInput = rawMoveInput.x; // Makes sure only the horizontal movement is used.
    }


    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.performed) jumpPressedTimeLeft = jumpPressTimeWindow;

        // Will shorten the jump.
        if (state == State.Jumping && context.canceled)
        {
            state = State.Falling;
            groundCheckIgnoreLeft = 0.0f;
        }
    }


    /// <summary>
    /// Adds a percentage buff to the movement speed.
    /// </summary>
    public void AddSpeedBuff(float percentage) => moveSpeedPercentage += percentage;

    /// <summary>
    /// Adds a percentage de-buff to the movement speed.
    /// </summary>
    public void AddSpeedDebuff(float percentage) => moveSpeedPercentage -= percentage;

    /// <summary>
    /// Removes a percentage buff to the movement speed.
    /// </summary>
    public void RemoveSpeedBuff(float percentage) => moveSpeedPercentage -= percentage;

    /// <summary>
    /// Removes a percentage de-buff to the movement speed.
    /// </summary>
    public void RemoveSpeedDebuff(float percentage) => moveSpeedPercentage += percentage;



    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }


    private void Update()
    {
        UpdateGravity();
        UpdateJump();

        UpdateAirStrafe();

        maxMoveSpeed = baseMoveSpeed * moveSpeedPercentage * 0.01f;
        Vector3 moveVelocity = CalculateMoveVelocity();

        //rotation check
        if (moveInput != 0)
        {
            if (moveInput > 0)
            {
                facingRight = true;
            }
            else
            {
                facingRight = false;
            }
        }

        transform.rotation = facingRight ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, -90, 0);
        moveVelocity += Vector3.up * verticalVelocity;
        controller.Move(moveVelocity * Time.deltaTime);
    }


    /// <summary>
    /// Applies a down force based on mass and gravity after checking whether the player is grounded.
    /// </summary>
    private void UpdateGravity()
    {
        if (groundCheckIgnoreLeft > float.Epsilon)
        {
            ApplyGravity();
            groundCheckIgnoreLeft -= Time.deltaTime;
            return;
        }

        Vector3 checkPoint = transform.position + groundCheckPosition;
        bool isGrounded = Physics.CheckSphere(checkPoint, groundCheckRadius, groundMask);
        if (isGrounded)
        {
            state = State.Grounded;
            verticalVelocity = Physics.gravity.y; // This is to make sure the player 'sticks' to the ground.
            return;
        }

        state = State.Falling;
        ApplyGravity();
    }


    private void ApplyGravity()
    {
        float multiplier = state == State.Falling ? fallingGravityMultiplier : 1.0f;
        verticalVelocity += mass * Physics.gravity.y * multiplier * Time.deltaTime;
    }


    private void UpdateJump()
    {
        bool canJump = jumpPressedTimeLeft > float.Epsilon && state == State.Grounded;
        if (canJump)
        {
            jumpPressedTimeLeft = 0.0f;
            Jump();
        }

        jumpPressedTimeLeft = Mathf.Max(jumpPressedTimeLeft - Time.deltaTime, 0.0f);
        
        if (state == State.Grounded) return;
        if (verticalVelocity <= float.Epsilon)
            state = State.Falling;
    }


    private void Jump()
    {
        state = State.Jumping;
        groundCheckIgnoreLeft = groundCheckCooldown;

        float jumpVelocity = Mathf.Sqrt(2.0f * mass * Mathf.Abs(Physics.gravity.y) * maxJumpHeight);
        verticalVelocity = jumpVelocity;
    }


    private void UpdateAirStrafe()
    {
        if (state == State.Grounded && isAirstrafeDebuffActive)
        {
            RemoveSpeedDebuff(airstrafeMoveDebuff);
            isAirstrafeDebuffActive = false;
            return;
        }
        
        if (state == State.Falling && !isAirstrafeDebuffActive)
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
    private Vector3 CalculateMoveVelocity()
    {
        bool isAccelerating = Mathf.Abs(moveInput) > float.Epsilon;
        float acceleration = isAccelerating ? accelerationStrength : decelerationStrength;

        float targetVelocity = moveInput * maxMoveSpeed;
        float velocityDifference = targetVelocity - currentMoveVelocity;

        float powerMultiplier = moveSpeedPercentage * 0.01f * SPEED_PERCENTAGE_POWER_MULT;
        float velocity = Mathf.Pow(Mathf.Abs(velocityDifference), movePower * powerMultiplier);

        currentMoveVelocity += Mathf.Sign(velocityDifference) * velocity * acceleration * Time.deltaTime;
        return Vector3.right * currentMoveVelocity;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = state == State.Grounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + groundCheckPosition, groundCheckRadius);
    }
}
