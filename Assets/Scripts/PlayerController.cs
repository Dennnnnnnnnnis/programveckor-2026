using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor.Animations;
using UnityEngine.SceneManagement;
using Unity.VectorGraphics;

[RequireComponent(typeof(Collision))]
public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        NORMAL,
        DASH,
        SWING,
        PEE,
        NOCLIP,
        BRAINDEAD
    };

    GameManager gm;
    Collision col;
    Animator anim;
    ParticleSystem pee;
    SpriteRenderer playerIndicator;

    public bool isDog = false;
    public PlayerState state = PlayerState.NORMAL;
    private bool facingRight = true;

    // Shitty temp stuff
    [SerializeField] private AnimatorController dogAnims;
    [SerializeField] private float dogColHeight, dogColEdge;
    [SerializeField] private float dogPeeOffset = 0.5f;
    private Animator dogMouth;
    private float idleTime = 0f;
    private float indicatorTime = 3f;

    // Tether
    TetherManager tether;
    private int tetherIndex = -1;
    private float tetherDrag = 0f;
    private float airborneTimer = 0f;

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

    private bool jumpInput = false, actionInput = false, action2Input = false, peeInput = false;
    private float jumpInputBuffer = 0f, actionInputBuffer = 0f, action2InputBuffer = 0f;

    void Awake()
    {
        // Some important components to grab
        gm = GameManager.Instance;
        col = GetComponent<Collision>();
        input = GetComponent<PlayerInput>();

        DontDestroyOnLoad(gameObject);

        // Try to get the animator
        if (transform.childCount > 0 && transform.GetChild(0).TryGetComponent<Animator>(out anim))
            Debug.Log("Got Animator for player.");
        else
            Debug.LogWarning("Couldn't find Animator for player.");

        // Try to get the dog mouth
        if (anim.transform.childCount > 0 && anim.transform.GetChild(0).TryGetComponent<Animator>(out dogMouth))
            Debug.Log("Got dog mouth for player.");
        else
            Debug.LogWarning("Couldn't find dog mouth for player.");

        // Try to get the particle system
        if (transform.childCount > 1 && transform.GetChild(1).TryGetComponent<ParticleSystem>(out pee))
            Debug.Log("Got Particle System for player.");
        else
            Debug.LogWarning("Couldn't find Particle System for player.");

        // Try to get the player indicator
        Animator playerIndicatorAnim = null;
        if (transform.childCount > 2 && transform.GetChild(2).TryGetComponent<Animator>(out playerIndicatorAnim))
        {
            playerIndicator = playerIndicatorAnim.GetComponent<SpriteRenderer>();
            Debug.Log("Got the player indicator.");
        }
        else
            Debug.LogWarning("Couldn't find the player indicator.");

        // Manage tether connection
        tether = Object.FindFirstObjectByType<TetherManager>();
        if(tether != null)
        {
            // Do player two stuff
            if(tether.connections.Count > 0)
            {
                for(int i = 0; i < tether.connections.Count; i++)
                {
                    PlayerController otherPlayer;
                    if (tether.connections[i].TryGetComponent<PlayerController>(out otherPlayer))
                    {
                        isDog = !otherPlayer.isDog;
                        if (isDog)
                        {
                            anim.runtimeAnimatorController = dogAnims;
                            col.ChangeHitboxY(dogColHeight, dogColHeight / 2f, dogColEdge);
                            if(playerIndicatorAnim != null)
                                playerIndicatorAnim.SetBool("isP2", true);
                            pee.transform.position += Vector3.up * dogPeeOffset;
                        }
                        break;
                    }
                }
            }

            // Connect to the tether
            tether.connections.Add(gameObject);
            tetherIndex = tether.connections.Count - 1;
        }

        // Add player to camera list
        Object.FindAnyObjectByType<CameraManager>().targets.Add(transform);
    }

    void Update()
    {
        UpdateInput();

        if (indicatorTime > 0f)
        {
            indicatorTime -= Time.deltaTime;
            if(playerIndicator.color.a < 1f)
                playerIndicator.color = new Color(1f, 1f, 1f, Mathf.Lerp(playerIndicator.color.a, 1f, Time.deltaTime * 8f));
        }
        else if (playerIndicator.color.a > 0f)
            playerIndicator.color = new Color(1f, 1f, 1f, Mathf.Lerp(playerIndicator.color.a, 0f, Time.deltaTime * 8f));

        if (idleTime > 12f)
            indicatorTime = 1f;
    }

    void FixedUpdate()
    {
        if (gm.state == GameManager.GameState.STANDARD)
        {
            // Tether drag
            col.weight = 1f;
            if (!tether.IsWithinBounds(tetherIndex))
                tetherDrag = 0.99f;
            else if (tetherDrag > 0f)
                tetherDrag = Mathf.Max(tetherDrag - Time.fixedDeltaTime, 0f);

            // States
            switch (state)
            {
                case PlayerState.NORMAL:
                    PlayerStateNormal();
                    break;
                case PlayerState.DASH:
                    PlayerStateDash();
                    break;
                case PlayerState.PEE:
                    PlayerStatePee();
                    break;
                case PlayerState.BRAINDEAD:
                    col.Velocity = Vector2.right * col.Velocity.x * tetherDrag + Vector2.up * Mathf.Max(col.Velocity.y - gravity * Time.fixedDeltaTime, -terminalVelocity);
                    col.Collide();
                    break;
                case PlayerState.NOCLIP:
                    col.Velocity = moveInput * walkSpeed;
                    transform.position += (Vector3)col.Velocity * Time.fixedDeltaTime;
                    col.weight = 50f;
                    break;
            }

            // Pee thing
            if (state == PlayerState.PEE)
                pee.Play();
            else
                pee.Stop();

            // Bite thing
            if (isDog && action2Input)
                dogMouth.Play("DogMouth", -1);
            else
                dogMouth.Play("DogNo", -1);

            // Idle timer
            if (state == PlayerState.NORMAL && Mathf.Abs(moveInput.x) < 0.1f)
                idleTime += Time.fixedDeltaTime;
            else
                idleTime = 0f;

            // Cooldowns
            if (abilityTimer > 0f)
                abilityTimer -= Time.fixedDeltaTime;

            // Buffers
            if (jumpInputBuffer > 0f)
                jumpInputBuffer -= Time.fixedDeltaTime;
            if (actionInputBuffer > 0f)
                actionInputBuffer -= Time.fixedDeltaTime;
            if (coyoteTime > 0f)
                coyoteTime -= Time.fixedDeltaTime;

            // Failsafe thing
            if (transform.position.y < -100f)
                gm.RestartLevel();
        }
        else
        {
            col.Velocity = Vector2.zero;
        }
    }

    void PlayerStateNormal()
    {
        // Horizontal movement
        col.Velocity = Vector2.right * (moveInput.x * walkSpeed * (1f - tetherDrag) + col.Velocity.x * tetherDrag) + Vector2.up * col.Velocity.y;
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

        // Weight
        if (Mathf.Abs(moveInput.x) > 0.05f)
            col.weight = 8f;
        if (!col.IsGrounded)
        {
            col.weight = Mathf.Max(col.weight + Mathf.Max(col.Velocity.y / 2f, 0f) - airborneTimer * 0.8f, 0.01f);
            airborneTimer += Time.fixedDeltaTime;
        }
        else
            airborneTimer = 0f;

        // Animations
        if (!col.IsGrounded)
        {
            if (airborneTimer > 3f)
                anim.Play("Scared", -1);
            else
                anim.Play("Jump", -1);
        }
        else
        {
            if (Mathf.Abs(moveInput.x) > 0.1f)
                anim.Play("Walk", -1);
            else
                anim.Play("Idle", -1);
        }
        anim.transform.localScale = new Vector3((facingRight ? 1f : -1f), 1f, 1f);

        // Pee
        if (peeInput && col.IsGrounded)
            state = PlayerState.PEE;
    }

    void PlayerStateDash()
    {
        // Very necessary commenting here

        // Gravity
        col.Velocity = Vector2.right * col.Velocity.x + Vector2.up * Mathf.Max(col.Velocity.y - jumpGravity * Time.fixedDeltaTime, -terminalVelocity);

        if (col.IsGrounded)
            coyoteTime = 0.1f;

        // Do collision
        col.Collide();

        // Weight
        col.weight = 30f;

        // Animation
        anim.Play("Dash", -1);

        // Switch back to normal state
        if (abilityTimer <= dogDashCooldown - dogDashLocktime)
            state = PlayerState.NORMAL;
    }

    void PlayerStatePee()
    {
        // Horizontal movement
        col.Velocity = Vector2.right * col.Velocity.x * tetherDrag + Vector2.up * col.Velocity.y;

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
        col.Velocity = Vector2.right * col.Velocity.x + Vector2.up * Mathf.Max(col.Velocity.y - gravity * Time.fixedDeltaTime, -terminalVelocity);
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

        // Animations
        anim.Play("Pee", -1);
        anim.transform.localScale = new Vector3((facingRight ? 1f : -1f), 1f, 1f);

        // Particle system
        if (moveInput != Vector2.zero)
        {
            pee.transform.localRotation = Quaternion.Euler(Mathf.Atan2(-moveInput.y, moveInput.x) * Mathf.Rad2Deg, 90f, -90f);
        }

        // Change state
        if (!peeInput || !col.IsGrounded)
            state = PlayerState.NORMAL;
    }

    void DogDash()
    {
        state = PlayerState.DASH;
        isJumping = false;
        abilityTimer = dogDashCooldown;
        col.Velocity = Vector2.up * col.Velocity.y + Vector2.right * dogDashForce * (Mathf.Abs(moveInput.x) > 0.1f ? Mathf.Sign(moveInput.x) : (facingRight ? 1f : -1f));
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

        action2Input = input.actions["Action2"].IsPressed();
        if (input.actions["Action2"].triggered)
            action2InputBuffer = 0.1f;

        peeInput = input.actions["Pee"].IsPressed();

        // This is for debug stuff
        if (input.actions["Debug"].triggered)
        {
            if (state == PlayerState.NOCLIP)
                state = PlayerState.NORMAL;
            else
                state = PlayerState.NOCLIP;
        }

        if (input.actions["Pause"].triggered)
        {
            gm.TogglePause();
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.tag == "Bush" && col.TryGetComponent<GoldenBush>(out GoldenBush bush))
        {
            if (state == PlayerState.PEE)
                gm.LoadLevel(bush.sceneName);
        }
        else if(col.tag == "Death")
        {
            gm.RestartLevel();
        }
    }
}
