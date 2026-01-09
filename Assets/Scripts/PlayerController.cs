using UnityEngine;

[RequireComponent(typeof(Collision))]
public class PlayerController : MonoBehaviour
{
    GameManager gm;
    Collision col;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float jumpHeight = 4f;
    [Space]
    [SerializeField] private float gravity = 60f;
    [SerializeField] private float jumpGravity = 50f;
    [SerializeField] private float peakGravity = 30f;
    [SerializeField] private float terminalVelocity = 80f;

    private bool isJumping = false;
    private float coyoteTime = 0f;

    [Header("Input")]
    private Vector2 moveInput;

    private bool jumpInput = false, jumpInputDown = false;
    private bool actionInput = false, actionInputDown = false;

    private float jumpInputBuffer = 0f;

    void Awake()
    {
        gm = GameManager.Instance;
        col = GetComponent<Collision>();
    }

    void Update()
    {
        UpdateInput();
    }

    void FixedUpdate()
    {
        if (gm.state == GameManager.GameState.STANDARD)
        {
            // Horizontal movement
            col.Velocity = Vector2.right * moveInput.x * walkSpeed + Vector2.up * col.Velocity.y;

            // Vertical movement
            if (isJumping)
            {
                if (col.Velocity.y < 0f)
                    isJumping = false;
                else if(!jumpInput)
                {
                    isJumping = false;
                    col.Velocity = col.Velocity.x * Vector2.right + col.Velocity.y / 2f * Vector2.up;
                }
            }

            float grv = gravity;
            if(isJumping)
            {
                if (col.Velocity.y < 2f)
                    grv = peakGravity;
                else
                    grv = jumpGravity;
            }
            col.Velocity = Vector2.right * col.Velocity.x + Vector2.up * Mathf.Max(col.Velocity.y - grv * Time.fixedDeltaTime, -terminalVelocity);

            if (col.IsGrounded)
                coyoteTime = 0.1f;

            if(jumpInputBuffer > 0f && coyoteTime > 0f)
            {
                col.Velocity = Vector2.up * Mathf.Sqrt(2f * gravity * jumpHeight) + Vector2.right * col.Velocity;

                jumpInputBuffer = 0f;
                coyoteTime = 0f;
                col.IsGrounded = false;
                isJumping = true;
            }

            // Buffers
            if (jumpInputBuffer > 0f)
                jumpInputBuffer -= Time.fixedDeltaTime;
            if (coyoteTime > 0f)
                coyoteTime -= Time.fixedDeltaTime;
        }
        else
        {
            col.Velocity = Vector2.zero;
        }

        col.Collide();

        // Reset down inputs
        actionInputDown = false;
    }

    void UpdateInput()
    {
        moveInput = gm.input.Player.Move.ReadValue<Vector2>();

        jumpInput = gm.input.Player.Jump.IsPressed();
        if (gm.input.Player.Jump.triggered)
            jumpInputBuffer = 0.1f;
    }
}
