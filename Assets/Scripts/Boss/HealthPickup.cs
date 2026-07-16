using System.Collections;
using UnityEngine;

/// <summary>
/// Item darah yang bisa dipungut player untuk memulihkan HP.
/// Dipasang sebagai komponen di prefab HealthPickup.
/// Fitur:
///   - Animasi bobbing (naik turun)
///   - Animasi glow/pulse via SpriteRenderer
///   - Partikel saat diambil (opsional)
///   - Sound saat diambil
///   - Efek teks popup "+ HP"
/// </summary>
public class HealthPickup : MonoBehaviour
{
    // =============================================
    //  INSPECTOR
    // =============================================

    [Header("Heal")]
    [Tooltip("Jumlah HP yang dipulihkan saat diambil.")]
    public int healAmount = 1;

    [Header("Bobbing Animation")]
    public bool  enableBobbing    = true;
    public float bobbingAmplitude = 0.25f;  // Seberapa jauh naik/turun
    public float bobbingSpeed     = 2.5f;   // Kecepatan naik turun

    [Header("Pulse Animation")]
    public bool  enablePulse   = true;
    public float pulseMin      = 0.85f;
    public float pulseMax      = 1.15f;
    public float pulseSpeed    = 3f;

    [Header("Glow Color Cycle")]
    [Tooltip("Aktifkan pergantian warna merah-putih-merah.")]
    public bool enableColorCycle = true;
    public Color glowColor1      = Color.red;
    public Color glowColor2      = Color.white;
    public float colorCycleSpeed = 2f;

    [Header("FX")]
    public ParticleSystem pickupParticles;  // Prefab partikel saat diambil (opsional)
    public AudioClip      pickupSound;

    [Header("Popup Text")]
    [Tooltip("Prefab teks '+HP' yang melayang ke atas saat diambil (opsional).")]
    public GameObject popupTextPrefab;

    // =============================================
    //  PRIVATE
    // =============================================

    private Vector3        basePosition;
    private SpriteRenderer sr;
    private bool           collected = false;

    // =============================================
    //  UNITY
    // =============================================

    private void Start()
    {
        basePosition = transform.position;
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (collected) return;

        if (enableBobbing)
            Bobbing();

        if (enablePulse)
            Pulse();

        if (enableColorCycle)
            ColorCycle();
    }

    // =============================================
    //  ANIMASI
    // =============================================

    private void Bobbing()
    {
        float y = basePosition.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmplitude;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }

    private void Pulse()
    {
        float scale = pulseMin + (pulseMax - pulseMin) * (0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed));
        transform.localScale = Vector3.one * scale;
    }

    private void ColorCycle()
    {
        if (sr == null) return;
        float t = 0.5f + 0.5f * Mathf.Sin(Time.time * colorCycleSpeed);
        sr.color = Color.Lerp(glowColor1, glowColor2, t);
    }

    // =============================================
    //  TRIGGER — Player mengambil item
    // =============================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        playerJ player = other.GetComponent<playerJ>();
        if (player == null) return;

        Collect(player);
    }

    // =============================================
    //  COLLECT
    // =============================================

    private void Collect(playerJ player)
    {
        collected = true;

        // Pulihkan HP player
        int restored = Mathf.Min(healAmount, player.maxHealth - player.currentHealth);
        player.currentHealth = Mathf.Min(player.maxHealth, player.currentHealth + healAmount);

        Debug.Log($"[HealthPickup] Player memungut item darah! +{healAmount} HP (HP sekarang: {player.currentHealth}/{player.maxHealth})");

        // Sound
        if (pickupSound != null)
        {
            player.PlaySFX(pickupSound);
        }

        // Partikel
        if (pickupParticles != null)
        {
            Instantiate(pickupParticles, transform.position, Quaternion.identity);
        }

        // Popup text
        if (popupTextPrefab != null)
        {
            GameObject popup = Instantiate(popupTextPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            HealthPopupText txt = popup.GetComponent<HealthPopupText>();
            if (txt != null) txt.SetAmount(healAmount);
        }

        // Sembunyikan item segera, destroy setelah delay
        if (sr != null) sr.enabled = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 0.5f);
    }
}
