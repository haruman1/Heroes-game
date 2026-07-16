using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Mengontrol perilaku Boss: patrol, chase player, serang, dan sistem health.
/// Bos memiliki 2 phase — normal dan agresif (< 50% HP).
/// </summary>
public class BossController : MonoBehaviour
{
    // =============================================
    //  INSPECTOR FIELDS
    // =============================================

    [Header("Boss Identity")]
    public string bossName = "Boss";
    public int bossLevel = 1;

    [Header("Health")]
    public int maxHealth = 100;
    [HideInInspector] public int currentHealth;

    [Header("Movement")]
    public float patrolSpeed    = 2f;
    public float chaseSpeed     = 4f;
    public float phase2SpeedMul = 1.5f;  // Kecepatan di phase 2
    public Transform[] patrolPoints;

    [Header("Detection")]
    public float detectionRange  = 8f;
    public float attackRange     = 1.5f;
    public LayerMask playerLayer;

    [Header("Attack")]
    public int   attackDamage    = 1;
    public float attackCooldown  = 1.5f;
    public float phase2CooldownMul = 0.6f; // Cooldown lebih pendek di phase 2

    [Header("Damage Flash")]
    public float damageFlashDuration = 0.15f;

    [Header("Events")]
    public UnityEvent onBossDead;
    public UnityEvent onPhase2Entered;
    public UnityEvent<float> onHealthChanged; // float = 0..1 (persen)

    // =============================================
    //  PRIVATE STATE
    // =============================================

    private enum BossState { Idle, Patrol, Chase, Attack, Dead }
    private BossState state = BossState.Idle;

    private bool     phase2Active  = false;
    private float    attackTimer   = 0f;
    private int      currentPoint  = 0;

    private Transform    playerTransform;
    private playerJ      playerScript;
    private Rigidbody2D  rb;
    private SpriteRenderer sr;
    private Animator     anim;
    private BossArenaController arenaController;

    // =============================================
    //  UNITY EVENTS
    // =============================================

    private void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        sr   = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        arenaController = FindFirstObjectByType<BossArenaController>();

        // Cari player di scene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerScript    = playerObj.GetComponent<playerJ>();
        }

        state = (patrolPoints != null && patrolPoints.Length > 0)
            ? BossState.Patrol
            : BossState.Idle;
    }

    private void Update()
    {
        if (state == BossState.Dead) return;

        attackTimer -= Time.deltaTime;
        CheckPhase2();
        DecideState();
        ExecuteState();
    }

    // =============================================
    //  STATE MACHINE
    // =============================================

    private void DecideState()
    {
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (dist <= attackRange)
        {
            state = BossState.Attack;
        }
        else if (dist <= detectionRange)
        {
            state = BossState.Chase;
        }
        else if (patrolPoints != null && patrolPoints.Length > 0)
        {
            state = BossState.Patrol;
        }
        else
        {
            state = BossState.Idle;
        }
    }

    private void ExecuteState()
    {
        switch (state)
        {
            case BossState.Idle:
                break;

            case BossState.Patrol:
                DoPatrol();
                break;

            case BossState.Chase:
                DoChase();
                break;

            case BossState.Attack:
                DoAttack();
                break;
        }
    }

    private void DoPatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform target = patrolPoints[currentPoint];
        float speed = patrolSpeed * (phase2Active ? phase2SpeedMul : 1f);
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        FlipTowards(target.position.x);

        if (Vector2.Distance(transform.position, target.position) < 0.3f)
        {
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
        }

        TrySetAnimBool("isWalking", true);
        TrySetAnimBool("isChasing", false);
    }

    private void DoChase()
    {
        if (playerTransform == null) return;

        float speed = chaseSpeed * (phase2Active ? phase2SpeedMul : 1f);
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, speed * Time.deltaTime);
        FlipTowards(playerTransform.position.x);

        TrySetAnimBool("isWalking", false);
        TrySetAnimBool("isChasing", true);
    }

    private void DoAttack()
    {
        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        float cooldown = attackCooldown * (phase2Active ? phase2CooldownMul : 1f);

        if (attackTimer <= 0f)
        {
            attackTimer = cooldown;
            PerformAttack();
        }

        TrySetAnimBool("isWalking", false);
        TrySetAnimBool("isChasing", false);
    }

    private void PerformAttack()
    {
        TrySetAnimTrigger("attack");

        // Damage langsung jika player dalam jangkauan
        if (playerScript != null && playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist <= attackRange * 1.1f)
            {
                // Gunakan reflection untuk memanggil TakeDamageFromBoss agar bisa override
                playerScript.TakeDamageFromBoss(attackDamage);
            }
        }

        Debug.Log($"[Boss] {bossName} menyerang!");
    }

    // =============================================
    //  DAMAGE & HEALTH
    // =============================================

    /// <summary>
    /// Dipanggil saat bos kena damage (dari stomp, projectile, dll.)
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (state == BossState.Dead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        float ratio = currentHealth / (float)maxHealth;
        onHealthChanged?.Invoke(ratio);

        Debug.Log($"[Boss] {bossName} kena {amount} damage. HP: {currentHealth}/{maxHealth}");

        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void CheckPhase2()
    {
        if (!phase2Active && currentHealth <= maxHealth / 2)
        {
            phase2Active = true;
            Debug.Log($"[Boss] {bossName} masuk Phase 2!");
            onPhase2Entered?.Invoke();
            TrySetAnimTrigger("phase2");
        }
    }

    private void Die()
    {
        state = BossState.Dead;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        TrySetAnimTrigger("die");

        Debug.Log($"[Boss] {bossName} mati!");
        onBossDead?.Invoke();

        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }

    private IEnumerator DamageFlash()
    {
        if (sr != null) sr.color = Color.red;
        yield return new WaitForSeconds(damageFlashDuration);
        if (sr != null) sr.color = Color.white;
    }

    // =============================================
    //  COLLISION — Player terkena bos
    // =============================================

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (state == BossState.Dead) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        playerJ player = collision.gameObject.GetComponent<playerJ>();
        if (player == null) return;

        // Cek apakah player STOMP dari atas
        bool isStompFromAbove = collision.relativeVelocity.y > 2f;

        if (isStompFromAbove)
        {
            // Player menginjak bos dari atas → bos kena damage
            TakeDamage(25);
            // Pantulkan player ke atas
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, player.jumpForce * 0.8f);
        }
        else
        {
            // Player kena body bos dari samping → player kena damage
            player.TakeDamageFromBoss(attackDamage);
        }
    }

    // =============================================
    //  HELPER
    // =============================================

    private void FlipTowards(float targetX)
    {
        if (sr == null) return;
        sr.flipX = targetX < transform.position.x;
    }

    private void TrySetAnimBool(string param, bool value)
    {
        if (anim == null) return;
        try { anim.SetBool(param, value); } catch { /* Animator mungkin tidak punya param ini */ }
    }

    private void TrySetAnimTrigger(string param)
    {
        if (anim == null) return;
        try { anim.SetTrigger(param); } catch { }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
