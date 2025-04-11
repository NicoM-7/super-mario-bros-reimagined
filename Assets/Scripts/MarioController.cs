using UnityEngine;

public class MarioPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;          
    public float runSpeed = 8f;           
    public float acceleration = 20f;      
    public float deceleration = 20f;      
    public float skidDeceleration = 30f;  

    [Header("Jump Settings - Phase 1 (Normal Jump)")]
    public float jumpForce1 = 10f;            
    public float maxJumpTime1 = 0.2f;           
    public float jumpHoldForce1Stationary = 5f; 
    public float jumpHoldForce1Low = 6f;        
    public float jumpHoldForce1Medium = 7f;     
    public float jumpHoldForce1High = 8f;       

    [Header("Jump Settings - Phase 2 (Double Jump)")]
    public float jumpForce2 = 12f;
    public float maxJumpTime2 = 0.25f;
    public float jumpHoldForce2Stationary = 6f;
    public float jumpHoldForce2Low = 7f;
    public float jumpHoldForce2Medium = 8f;
    public float jumpHoldForce2High = 9f;

    [Header("Jump Settings - Phase 3 (Triple Jump / Flip)")]
    public float jumpForce3 = 14f;
    public float maxJumpTime3 = 0.3f;
    public float jumpHoldForce3Stationary = 7f;
    public float jumpHoldForce3Low = 8f;
    public float jumpHoldForce3Medium = 9f;
    public float jumpHoldForce3High = 10f;  

    [Header("Spin Jump Settings")]
    public float spinJumpForce = 11f;         
    public float maxSpinJumpTime = 0.25f;       
    public float spinJumpHoldForceStationary = 5f;
    public float spinJumpHoldForceLow = 6f;
    public float spinJumpHoldForceMedium = 7f;
    public float spinJumpHoldForceHigh = 8f;

    [Header("Jump Timing")]
    public float coyoteTime = 0.1f;       
    public float jumpBufferTime = 0.1f;   
    public float jumpChainWindow = 0.3f;

    [Header("Ground Check")]
    public Transform groundCheck;         
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;         

    [Header("Audio")]
    public AudioClip jumpClip1;           
    public AudioClip jumpClip2;           
    public AudioClip jumpClip3;           
    public AudioClip spinJumpClip;        

    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;

    private float horizontalInput;
    private bool runInput;
    private bool jumpInputHeld;
    private bool spinJumpInputHeld;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float jumpChainTimer;

    // Normal jump chain state: 0 = ready, 1 = Phase 1, 2 = Phase 2, 3 = Phase 3 (flip)
    private bool isJumping;
    private float jumpTimeCounter;
    private float jumpSpeedAtStart;
    private int jumpPhase = 0;

    // Spin jump state
    private bool isSpinJumping = false;
    private float spinJumpTimeCounter = 0f;
    private float spinJumpBufferCounter = 0f;
    private bool spinJumpUsed = false; // Prevents multiple spin jumps midair

    // Cached ground state.
    private bool isGrounded;
    private bool wasGrounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // --- Input ---
        horizontalInput = Input.GetAxisRaw("Horizontal");
        runInput = Input.GetKey(KeyCode.A);
        jumpInputHeld = Input.GetKey(KeyCode.Space);
        spinJumpInputHeld = Input.GetKey(KeyCode.X);

        // --- Sprite Facing Direction ---
        if (horizontalInput > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (horizontalInput < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // --- Ground Check & Coyote Time ---
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            // On landing (transition from air to ground), reset spin jump state and usage.
            if (!wasGrounded)
            {
                isSpinJumping = false;
                spinJumpUsed = false;
                animator.SetBool("IsSpinJumping", false);
            }

            // Normal jump chain resets if not in a jump hold.
            if (!isJumping)
            {
                if (jumpPhase == 3)
                {
                    jumpPhase = 0;
                    jumpChainTimer = 0;
                }
                else if (jumpPhase > 0)
                {
                    jumpChainTimer += Time.deltaTime;
                    if (jumpChainTimer > jumpChainWindow)
                    {
                        jumpPhase = 0;
                        jumpChainTimer = 0;
                    }
                }
                else
                {
                    jumpChainTimer = 0;
                }
            }
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            jumpChainTimer = 0;
        }
        wasGrounded = isGrounded;

        // --- Jump Buffer for normal jump (Space) ---
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // --- Spin Jump Buffer (X) ---
        if (Input.GetKeyDown(KeyCode.X))
            spinJumpBufferCounter = jumpBufferTime;
        else
            spinJumpBufferCounter = Mathf.Max(0, spinJumpBufferCounter - Time.deltaTime);

        // --- Initiate Spin Jump ---
        // Use the spin jump buffer (like normal jump buffer) and trigger spin jump only when grounded.
        if (isGrounded && spinJumpBufferCounter > 0 && !spinJumpUsed)
        {
            spinJumpUsed = true;
            // Reset normal jump chain.
            jumpPhase = 0;
            jumpChainTimer = 0;
            jumpBufferCounter = 0;
            isJumping = false;
            // Initiate spin jump.
            isSpinJumping = true;
            spinJumpTimeCounter = maxSpinJumpTime;
            rb.velocity = new Vector2(rb.velocity.x, spinJumpForce);
            audioSource.PlayOneShot(spinJumpClip);
            animator.SetBool("IsSpinJumping", true);
            spinJumpBufferCounter = 0; // Clear buffer after triggering.
        }

        if (isSpinJumping && !Input.GetKey(KeyCode.X))
        {
            spinJumpTimeCounter = 0;
        }

        // --- Update Animator Parameters (non-spin parts) ---
        if (animator != null)
        {
            animator.SetFloat("Horizontal", Mathf.Abs(rb.velocity.x));
            animator.SetFloat("Vertical", rb.velocity.y);
            animator.SetBool("Ground", isGrounded);
        }
    }

    void FixedUpdate()
    {
        // --- Horizontal Movement (common for both jump types) ---
        float currentVelX = rb.velocity.x;
        float targetSpeed = horizontalInput * (runInput ? runSpeed : walkSpeed);
        float newVelX = currentVelX;
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            if (Mathf.Sign(horizontalInput) != Mathf.Sign(currentVelX) && Mathf.Abs(currentVelX) > 0.1f)
                newVelX = Mathf.MoveTowards(currentVelX, targetSpeed, skidDeceleration * Time.fixedDeltaTime);
            else
                newVelX = Mathf.MoveTowards(currentVelX, targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            newVelX = Mathf.MoveTowards(currentVelX, 0, deceleration * Time.fixedDeltaTime);
        }
        rb.velocity = new Vector2(newVelX, rb.velocity.y);

        // --- Normal Jumping (Triple Jump chain using Space) ---
        if (!isSpinJumping)
        {
            if (jumpBufferCounter > 0 && ((jumpPhase == 0 && coyoteTimeCounter > 0 && isGrounded) || (jumpPhase > 0 && isGrounded)) && !isJumping)
            {
                if (jumpPhase == 0)
                {
                    jumpPhase = 1;
                    rb.velocity = new Vector2(rb.velocity.x, jumpForce1);
                    jumpTimeCounter = maxJumpTime1;
                    audioSource.PlayOneShot(jumpClip1);
                    animator.SetTrigger("Jump1");
                }
                else if (jumpPhase == 1)
                {
                    if (jumpChainTimer <= jumpChainWindow)
                    {
                        jumpPhase = 2;
                        rb.velocity = new Vector2(rb.velocity.x, jumpForce2);
                        jumpTimeCounter = maxJumpTime2;
                        audioSource.PlayOneShot(jumpClip2);
                        animator.SetTrigger("Jump2");
                    }
                }
                else if (jumpPhase == 2)
                {
                    if (jumpChainTimer <= jumpChainWindow)
                    {
                        if (Mathf.Abs(horizontalInput) >= 0.1f)
                        {
                            jumpPhase = 3;
                            rb.velocity = new Vector2(rb.velocity.x, jumpForce3);
                            jumpTimeCounter = maxJumpTime3;
                            audioSource.PlayOneShot(jumpClip3);
                            animator.SetBool("IsTripleJumping", true);
                            animator.SetTrigger("Jump3");
                        }
                        else
                        {
                            jumpPhase = 2;
                            rb.velocity = new Vector2(rb.velocity.x, jumpForce2);
                            jumpTimeCounter = maxJumpTime2;
                            audioSource.PlayOneShot(jumpClip2);
                            animator.SetTrigger("Jump2");
                        }
                    }
                }
                jumpChainTimer = 0f;
                jumpSpeedAtStart = Mathf.Abs(rb.velocity.x);
                jumpBufferCounter = 0;
                isJumping = true;
            }

            if (isJumping && jumpInputHeld && jumpTimeCounter > 0)
            {
                float effectiveHoldForce = GetEffectiveJumpHoldForce(jumpSpeedAtStart, jumpPhase);
                rb.AddForce(new Vector2(0, effectiveHoldForce), ForceMode2D.Force);
                jumpTimeCounter -= Time.fixedDeltaTime;
            }
            else if (!jumpInputHeld)
            {
                isJumping = false;
            }
        }

        // --- Spin Jump Hold ---
        if (isSpinJumping)
        {
            // Only apply spin jump hold if X is still held.
            if (spinJumpInputHeld && spinJumpTimeCounter > 0)
            {
                float effectiveSpinHold = GetEffectiveSpinJumpHoldForce(Mathf.Abs(rb.velocity.x));
                rb.AddForce(new Vector2(0, effectiveSpinHold), ForceMode2D.Force);
                spinJumpTimeCounter -= Time.fixedDeltaTime;
            }
        }
    }

    private float GetEffectiveJumpHoldForce(float horizontalSpeed, int phase)
    {
        switch (phase)
        {
            case 1:
                if (horizontalSpeed == 0f)
                    return jumpHoldForce1Stationary;
                else if (horizontalSpeed <= walkSpeed)
                    return jumpHoldForce1Low;
                else if (horizontalSpeed > walkSpeed && horizontalSpeed < runSpeed)
                    return jumpHoldForce1Medium;
                else
                    return jumpHoldForce1High;
            case 2:
                if (horizontalSpeed == 0f)
                    return jumpHoldForce2Stationary;
                else if (horizontalSpeed <= walkSpeed)
                    return jumpHoldForce2Low;
                else if (horizontalSpeed > walkSpeed && horizontalSpeed < runSpeed)
                    return jumpHoldForce2Medium;
                else
                    return jumpHoldForce2High;
            case 3:
                if (horizontalSpeed == 0f)
                    return jumpHoldForce3Stationary;
                else if (horizontalSpeed <= walkSpeed)
                    return jumpHoldForce3Low;
                else if (horizontalSpeed > walkSpeed && horizontalSpeed < runSpeed)
                    return jumpHoldForce3Medium;
                else
                    return jumpHoldForce3High;
            default:
                return 0f;
        }
    }

    private float GetEffectiveSpinJumpHoldForce(float horizontalSpeed)
    {
        if (horizontalSpeed == 0f)
            return spinJumpHoldForceStationary;
        else if (horizontalSpeed <= walkSpeed)
            return spinJumpHoldForceLow;
        else if (horizontalSpeed > walkSpeed && horizontalSpeed < runSpeed)
            return spinJumpHoldForceMedium;
        else
            return spinJumpHoldForceHigh;
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}