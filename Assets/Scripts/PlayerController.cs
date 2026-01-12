using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collision))]
public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        NORMAL,
        DASH,
        SWING,
        NOCLIP
    };

    GameManager gm;
    Collision col;
    public bool isDog = false;
    [HideInInspector] public PlayerState state = PlayerState.NORMAL;
    private bool facingRight = true;

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

    [Header("Abilities")]
    [SerializeField] private float dogDashForce = 10f;
    [SerializeField] private float dogDashLocktime = 0.2f;
    [SerializeField] private float dogDashCooldown = 0.5f;

    private float abilityTimer = 0f;

    [Header("Input")]
    private PlayerInput input;
    private Vector2 moveInput;

    private bool jumpInput = false, actionInput = false;
    private float jumpInputBuffer = 0f, actionInputBuffer = 0f;

    void Awake()
    {
        gm = GameManager.Instance;
        col = GetComponent<Collision>();
        input = GetComponent<PlayerInput>();

        // Make sure the player is either the dog or the guy
        PlayerController[] allPlayers = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        if (allPlayers.Length >= 2)
        {
            foreach (PlayerController p in allPlayers)
            {
                if (p != this)
                {
                    isDog = !p.isDog;
                    print("yeah this should work");
                }
                print("got into loop");
            }
            print("got into player check");
        }
    }

    void Update()
    {
        UpdateInput();
    }

    void FixedUpdate()
    {
        if (gm.state == GameManager.GameState.STANDARD)
        {
            switch (state)
            {
                case PlayerState.NORMAL:
                    PlayerStateNormal();
                    break;
                case PlayerState.DASH:
                    PlayerStateDash();
                    break;
                case PlayerState.NOCLIP:
                    col.Velocity = moveInput * walkSpeed;
                    transform.position += (Vector3)col.Velocity;
                    break;
            }

            // Cooldowns
            if(abilityTimer > 0f)
                abilityTimer -= Time.fixedDeltaTime;

            // Buffers
            if (jumpInputBuffer > 0f)
                jumpInputBuffer -= Time.fixedDeltaTime;
            if (actionInputBuffer > 0f)
                actionInputBuffer -= Time.fixedDeltaTime;
            if (coyoteTime > 0f)
                coyoteTime -= Time.fixedDeltaTime;
        }
        else
        {
            col.Velocity = Vector2.zero;
        }
    }

    void PlayerStateNormal()
    {
        // Horizontal movement
        col.Velocity = Vector2.right * moveInput.x * walkSpeed + Vector2.up * col.Velocity.y;
        if (col.Velocity.x != 0f)
            facingRight = (col.Velocity.x > 0f);

        #region Vertical Movement

        // Cancel jumping
        if (isJumping)
        {
            if (col.Velocity.y < 0f)
                isJumping = false;
            else if (!jumpInput)
            {
                isJumping = false;
                col.Velocity = col.Velocity.x * Vector2.right + col.Velocity.y / 2f * Vector2.up;
            }
        }

        // Gravity
        float grv = gravity;
        if (isJumping)
        {
            if (col.Velocity.y < 2f)
                grv = peakGravity;
            else
                grv = jumpGravity;
        }
        col.Velocity = Vector2.right * col.Velocity.x + Vector2.up * Mathf.Max(col.Velocity.y - grv * Time.fixedDeltaTime, -terminalVelocity);

        if (col.IsGrounded)
            coyoteTime = 0.1f;

        // Do the jump
        if (jumpInputBuffer > 0f && coyoteTime > 0f)
        {
            col.Velocity = Vector2.up * Mathf.Sqrt(2f * gravity * jumpHeight) + Vector2.right * col.Velocity;

            jumpInputBuffer = 0f;
            coyoteTime = 0f;
            col.IsGrounded = false;
            isJumping = true;
        }

        #endregion

        // Ablilities
        if (isDog && actionInputBuffer > 0f && abilityTimer <= 0f)
            DogDash();

        // Do collision
        col.Collide();
    }

    void PlayerStateDash()
    {
        // Gravity
        col.Velocity = Vector2.right * col.Velocity.x + Vector2.up * Mathf.Max(col.Velocity.y - jumpGravity * Time.fixedDeltaTime, -terminalVelocity);

        if (col.IsGrounded)
            coyoteTime = 0.1f;

        // Do collision
        col.Collide();

        // Switch back to normal state
        if (abilityTimer <= dogDashCooldown - dogDashLocktime)
            state = PlayerState.NORMAL;
    }

    void DogDash()
    {
        state = PlayerState.DASH;
        isJumping = false;
        abilityTimer = dogDashCooldown;
        col.Velocity = Vector2.up * col.Velocity.y + Vector2.right * dogDashForce * (facingRight ? 1f : -1f);
    }

    void UpdateInput()
    {
        moveInput = input.actions["Move"].ReadValue<Vector2>();

        jumpInput = input.actions["Jump"].IsPressed();
        if (input.actions["Jump"].triggered)
            jumpInputBuffer = 0.1f;

        actionInput = input.actions["Action"].IsPressed();
        if (input.actions["Action"].triggered)
            actionInputBuffer = 0.1f;

        // This is for debug stuff
        if (input.actions["Debug"].triggered)
        {
            if (state == PlayerState.NOCLIP)
                state = PlayerState.NORMAL;
            else
                state = PlayerState.NOCLIP;
        }
    }
}
