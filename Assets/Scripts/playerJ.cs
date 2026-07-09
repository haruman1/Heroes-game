using System.Collections; // Required for IEnumerator
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class playerJ : MonoBehaviour
{
    [Header("Books")]
    public int bookCount = 0;
    public int booksRequiredPerLevel = 10;
    public TMPro.TMP_Text bookText;
    [HideInInspector]
    public int coinCount = 0; // Legacy variable

    [Header("Character Animators")]
    public RuntimeAnimatorController character1Animator; // Raden
    public RuntimeAnimatorController character2Animator; // Wanita (Rena)
    
    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip doubleJumpSound;
    public AudioClip coinSound;
    public AudioClip damageSound;
    public AudioClip deathSound;
    public AudioSource audioSource;
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 10f;
    public float jumpContinuousForce = 5f;
    public int extraJumpsValues = 1;
    public int extraJumps;

    [Header("Damage")]
    public int maxHealth = 100;
    public int currentHealth;
    public int maxLives = 3;
    public int currentLives;
    public Image healthImage;
    public Image[] lifeImages;
    public Sprite fullLifeSprite;
    public Sprite emptyLifeSprite;

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
    [Header("coyote Time")]
    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;
        currentLives = maxLives;
        extraJumps = extraJumpsValues;

        LoadPlayerFromDatabase();
    }

    void Update()
    {
        HandleInput();
        HandleFlip();
        SetAnimation();
        UpdateUI();
        OfftoGround();
    }

    void FixedUpdate()
    {
        GroundCheck();
        MovePlayer();
        JumpPlayer();
    }
    private void OfftoGround()
    {
        if(transform.position.y < -3f)
        {
            Die();
        }
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
            coyoteTimeCounter = coyoteTime;
            extraJumps = extraJumpsValues;
        }else{
            coyoteTimeCounter -= Time.deltaTime;
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

        if (coyoteTimeCounter > 0f)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);
            coyoteTimeCounter = 0f;
        }
        else if (extraJumps > 0)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);
            extraJumps--;
            if(Input.GetKey(KeyCode.Space) && body.linearVelocity.y > 0f || (Gamepad.current != null && Gamepad.current.buttonSouth.isPressed)   && body.linearVelocity.y > 0f)
            {
                body.AddForceY(jumpContinuousForce );
            }
            if (extraJumps > 0)
            {
                PlaySFX(doubleJumpSound);
            }
            else
            {
                PlaySFX(jumpSound);
            }
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Damage")
        {
            TakeDamage(1); // Set to 1 damage instead of 25
            PlaySFX(damageSound);
            StartCoroutine(BlinkRed());
        }
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Player Health: " + currentHealth);
        body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);

        if (currentHealth > 0)
            return;

        currentLives--;

        if (currentLives <= 0)
        {
            currentHealth = 0;
            Debug.Log("Player is dead!");
            Die();
            return;
        }

        currentHealth = maxHealth;
        SaveProgress();
    }

    private void UpdateUI()
    {
        if (healthImage != null)
        {
            healthImage.fillAmount = Mathf.Clamp01(currentHealth / (float)maxHealth);
        }

        if (bookText != null)
        {
            bookText.text = "Buku: " + bookCount + " / " + booksRequiredPerLevel;
        }

        if (lifeImages == null)
            return;

        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (lifeImages[i] == null)
                continue;

            lifeImages[i].sprite = i < currentLives ? fullLifeSprite : emptyLifeSprite;
        }
    }

    private IEnumerator BlinkRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
    public void PlaySFX(AudioClip clip, float volume = 1f){
        audioSource.clip = clip;
        float sfxVolume = 1f;
        GameSettingsData settings = DatabaseManager.GetOrCreateInstance().GetSettingsData();
        if (settings != null) sfxVolume = settings.SfxVolume;
        
        audioSource.volume = Mathf.Clamp01(volume) * sfxVolume;
        audioSource.Play();
    }
    private void Die()
    {
        PlaySFX(deathSound);
        Debug.Log("Player has died. Reset HP/nyawa lalu reload scene.");

        currentHealth = maxHealth;
        currentLives = maxLives;
        SaveProgress();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AddBook(int amount)
    {
        bookCount = Mathf.Max(0, bookCount + amount);
        UpdateUI();
    }

    public void AddCoin(int amount)
    {
        AddBook(amount);
    }

    public int GetCurrentLevelNumber()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string numberString = "";
        foreach (char c in sceneName)
        {
            if (char.IsDigit(c))
            {
                numberString += c;
            }
        }
        if (int.TryParse(numberString, out int level))
        {
            return level;
        }
        int index = SceneManager.GetActiveScene().buildIndex;
        return index > 0 ? index : 1;
    }

    private void LoadPlayerFromDatabase()
    {
        DatabaseManager dbManager = DatabaseManager.GetOrCreateInstance();
        if (dbManager == null)
            return;

        PlayerData data = dbManager.GetPlayerData();
        if (data == null)
            return;

        // Reset bookCount untuk level ini
        bookCount = 0;

        // Atur HP maks berdasarkan nomor level
        int levelNum = GetCurrentLevelNumber();
        maxHealth = levelNum;
        currentHealth = maxHealth;

        currentLives = data.Heart > 0 ? data.Heart : maxLives;

        // Terapkan visual karakter terpilih
        string selectedChar = data.SelectedCharacter;
        if (string.IsNullOrEmpty(selectedChar))
        {
            selectedChar = "Raden";
        }

        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            if (selectedChar == "Raden" || selectedChar == "Satria")
            {
                if (character1Animator != null)
                {
                    anim.runtimeAnimatorController = character1Animator;
                }
            }
            else
            {
                if (character2Animator != null)
                {
                    anim.runtimeAnimatorController = character2Animator;
                }
            }
        }
    }

    private void SaveProgress()
    {
        DatabaseManager dbManager = DatabaseManager.GetOrCreateInstance();
        if (dbManager == null)
            return;

        int currentLevel = SceneManager.GetActiveScene().buildIndex + 1;
        dbManager.SavePlayerState(coinCount, currentLevel, currentHealth, currentLives);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveProgress();
        }
    }

    private void OnApplicationQuit()
    {
        SaveProgress();
    }
}
