using UnityEngine;
using UnityEngine.InputSystem;

public class playerJ : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 10f;
    public int extraJumpsValues = 1;
    public int extraJumps;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private bool isGrounded;
    private bool jumpRequested;

    private float moveInput;
    private bool isRunning;

    private string currentAnimation = "";

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        extraJumps = extraJumpsValues;
    }

    void Update()
    {
        HandleInput();
        HandleFlip();
        SetAnimation();
    }

    void FixedUpdate()
    {
        GroundCheck();
        MovePlayer();
        JumpPlayer();
    }

    private void HandleInput()
    {
        moveInput = 0f;
        //extra jumps

        // Keyboard
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                moveInput = 1f;
            }
            else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                moveInput = -1f;
            }

            isRunning = Keyboard.current.leftShiftKey.isPressed;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                jumpRequested = true;
            }
        }

        // Gamepad
        if (Gamepad.current != null)
        {
            float stick = Gamepad.current.leftStick.x.ReadValue();

            if (Mathf.Abs(stick) > 0.1f)
            {
                moveInput = stick;
            }

            // RT = Run
            isRunning = Gamepad.current.rightTrigger.ReadValue() > 0.5f;

            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                jumpRequested = true;
            }
        }
    }

    private void GroundCheck()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            extraJumps = extraJumpsValues;
        }
    }

    private void MovePlayer()
    {
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        body.linearVelocity = new Vector2(moveInput * currentSpeed, body.linearVelocity.y);
    }

    private void JumpPlayer()
    {
        if (!jumpRequested)
            return;

        if (isGrounded)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);
        }
        else if (extraJumps > 0)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);
            extraJumps--;
        }

        jumpRequested = false;
    }

    private void HandleFlip()
    {
        if (moveInput > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    private void SetAnimation()
    {
        if (!isGrounded)
        {
            if (body.linearVelocity.y > 0.1f)
            {
                ChangeAnimation("Jump");
            }
            else
            {
                ChangeAnimation("Fall");
            }

            return;
        }

        if (Mathf.Abs(moveInput) < 0.1f)
        {
            ChangeAnimation("PlayerIdle");
        }
        else
        {
            if (isRunning)
            {
                ChangeAnimation("Run");
            }
            else
            {
                ChangeAnimation("Walk");
            }
        }
    }

    private void ChangeAnimation(string animationName)
    {
        if (currentAnimation == animationName)
            return;

        currentAnimation = animationName;
        animator.Play(animationName);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
