using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Musuh Keroco — ada di scene level utama (bukan scene boss).
/// Berbeda dari Enemy.cs biasa: keroco punya health sendiri
/// dan bisa mati saat diinjak/diserang player.
/// 
/// Cara melukai keroco:
///   - Lompat dari atas ke kepala keroco (stomp) → langsung mati
///   - Bisa dikembangkan: terkena proyektil / serangan
/// </summary>
public class KerocoBoss : MonoBehaviour
{
    // =============================================
    //  INSPECTOR
    // =============================================

    [Header("Identity")]
    public string kerocoName = "Keroco";

    [Header("Health")]
    public int maxHealth = 30;
    [HideInInspector] public int currentHealth;

    [Header("Movement — Patrol")]
    public float patrolSpeed = 2.5f;
    public Transform[] patrolPoints;

    [Header("Chase")]
    [Tooltip("Jika true, keroco mengejar player saat terdeteksi.")]
    public bool canChase = true;
    public float chaseSpeed     = 3.5f;
    public float detectionRange = 6f;

    [Header("Attack / Damage")]
    [Tooltip("Damage yang diberikan ke player saat menyentuh.")]
    public int   contactDamage  = 1;
    public float damageCooldown = 1f; // Jeda antar damage ke player

    [Header("Damage Flash")]
    public float damageFlashDuration = 0.12f;

    [Header("Drop Items")]
    [Tooltip("Prefab item yang di-drop saat keroco mati (opsional).")]
    public GameObject[] dropPrefabs;
    [Tooltip("Persentase chance drop (0–100).")]
    [Range(0, 100)]
    public int dropChance = 50;

    [Header("Events")]
    public UnityEvent onKerocolDead;
    public UnityEvent<float> onHealthChanged;

    // =============================================
    //  PRIVATE
    // =============================================

    private enum KerocoState { Patrol, Chase, Dead }
    private KerocoState state = KerocoState.Patrol;

    private int          currentPoint;
    private float        damageTimer;
    private Transform    playerTransform;
    private playerJ      playerScript;
    private SpriteRenderer sr;
    private Animator       anim;
    private Rigidbody2D    rb;

    // =============================================
    //  UNITY
    // =============================================

    private void Awake()
    {
        sr   = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb   = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerScript    = playerObj.GetComponent<playerJ>();
        }

        state = (patrolPoints != null && patrolPoints.Length > 0)
            ? KerocoState.Patrol
            : KerocoState.Chase;
    }

    private void Update()
    {
        if (state == KerocoState.Dead) return;

        damageTimer -= Time.deltaTime;

        if (canChase) DecideChase();
        ExecuteState();
    }

    // =============================================
    //  STATE MACHINE
    // =============================================

    private void DecideChase()
    {
        if (playerTransform == null) return;
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        state = dist <= detectionRange ? KerocoState.Chase : KerocoState.Patrol;
    }

    private void ExecuteState()
    {
        switch (state)
        {
            case KerocoState.Patrol: DoPatrol(); break;
            case KerocoState.Chase:  DoChase();  break;
        }
    }

    private void DoPatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform target = patrolPoints[currentPoint];
        transform.position = Vector2.MoveTowards(
            transform.position, target.position, patrolSpeed * Time.deltaTime);
        Flip(target.position.x);

        if (Vector2.Distance(transform.position, target.position) < 0.25f)
            currentPoint = (currentPoint + 1) % patrolPoints.Length;

        TrySetAnimBool("isWalking", true);
        TrySetAnimBool("isChasing", false);
    }

    private void DoChase()
    {
        if (playerTransform == null) return;
        transform.position = Vector2.MoveTowards(
            transform.position, playerTransform.position, chaseSpeed * Time.deltaTime);
        Flip(playerTransform.position.x);

        TrySetAnimBool("isWalking", false);
        TrySetAnimBool("isChasing", true);
    }

    // =============================================
    //  DAMAGE & DEATH
    // =============================================

    /// <summary>
    /// Terima damage. Dipanggil dari stomp collision atau serangan lain.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (state == KerocoState.Dead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        onHealthChanged?.Invoke(currentHealth / (float)maxHealth);
        StartCoroutine(DamageFlash());

        Debug.Log($"[Keroco] {kerocoName} kena {amount} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        state = KerocoState.Dead;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType       = RigidbodyType2D.Kinematic;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        TrySetAnimTrigger("die");

        // Drop item
        TryDrop();

        onKerocolDead?.Invoke();
        Debug.Log($"[Keroco] {kerocoName} mati!");

        StartCoroutine(DieRoutine());
    }

    private void TryDrop()
    {
        if (dropPrefabs == null || dropPrefabs.Length == 0) return;
        if (Random.Range(0, 100) >= dropChance) return;

        int idx = Random.Range(0, dropPrefabs.Length);
        if (dropPrefabs[idx] != null)
            Instantiate(dropPrefabs[idx], transform.position, Quaternion.identity);
    }

    private IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
    }

    private IEnumerator DamageFlash()
    {
        if (sr != null) sr.color = Color.red;
        yield return new WaitForSeconds(damageFlashDuration);
        if (sr != null) sr.color = Color.white;
    }

    // =============================================
    //  COLLISION — Player menyentuh / stomp keroco
    // =============================================

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (state == KerocoState.Dead) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        playerJ player = collision.gameObject.GetComponent<playerJ>();
        if (player == null) return;

        // Cek stomp dari atas: velocity bawah player + kontak dari atas
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        bool stompFromAbove  = playerRb != null && playerRb.linearVelocity.y < -1f;

        // Cek posisi relatif: player harus berada lebih tinggi dari keroco
        bool playerAbove = collision.transform.position.y > transform.position.y + 0.3f;

        if (stompFromAbove && playerAbove)
        {
            // Keroco mati seketika saat diinjak
            TakeDamage(maxHealth);

            // Pantulkan player
            if (playerRb != null)
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, player.jumpForce * 0.7f);
        }
        else
        {
            // Player kena damage dari samping
            if (damageTimer <= 0f)
            {
                damageTimer = damageCooldown;
                player.TakeDamageFromBoss(contactDamage);
            }
        }
    }

    // =============================================
    //  HELPER
    // =============================================

    private void Flip(float targetX)
    {
        if (sr != null)
            sr.flipX = targetX < transform.position.x;
    }

    private void TrySetAnimBool(string param, bool value)
    {
        if (anim == null) return;
        try { anim.SetBool(param, value); } catch { }
    }

    private void TrySetAnimTrigger(string param)
    {
        if (anim == null) return;
        try { anim.SetTrigger(param); } catch { }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
