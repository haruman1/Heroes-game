using UnityEngine;
using UnityEngine.InputSystem; // Wajib ditambahkan

public class playerJ : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask groundLayer;
    private Rigidbody2D body;
    private bool isGrounded;
    private SpriteRenderer spriteRenderer;

    private float moveInput;
    private bool jumpRequested;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // ========================================================
        // 1. MEMBACA INPUT HORIZONTAL (Jalan Kanan/Kiri)
        // ========================================================
        if (Keyboard.current != null)
        {
            // Mendukung Panah Kanan/Kiri dan A/D di PC
            moveInput =
                Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed
                    ? 1f
                    : (
                        Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed
                            ? -1f
                            : 0f
                    );
        }

        if (Gamepad.current != null)
        {
            // Mendukung Analog / Virtual Joystick di Android
            moveInput = Gamepad.current.leftStick.x.ReadValue();
        }

        // ========================================================
        // 2. MEMBACA INPUT LOMPAT (Menggunakan .wasPressedThisFrame)
        // ========================================================
        // Catatan: wasPressedThisFrame memastikan lompat hanya terpicu 1x saat tombol BARU ditekan
        if (
            (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                && isGrounded
            || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
                && isGrounded
        )
        {
            jumpRequested = true;
        }

        // ========================================================
        // 3. MENGATUR ARAH SPRITE (FLIP)
        // ==================   ======================================
        if (moveInput > 0)
            spriteRenderer.flipX = false;
        else if (moveInput < 0)
            spriteRenderer.flipX = true;
    }

    // Untuk urusan pergerakan Fisika (Rigidbody), wajib ditaruh di FixedUpdate agar stabil
    void FixedUpdate()
    {
        // ========================================================
        // 4. PERGERAKAN OBJEK
        // ========================================================
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        // Terapkan kecepatan jalan tanpa merusak kecepatan vertikal (Y) saat ini
        body.linearVelocity = new Vector2(moveInput * speed, body.linearVelocity.y);

        // Eksekusi lompat jika tombol sudah ditekan
        if (jumpRequested)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);
            jumpRequested = false; // Reset request setelah melompat
        }
    }
}
