using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// BoosterCollectible — Item booster yang tersebar di level-level biasa.
/// 
/// Booster dikumpulkan sepanjang level dan disimpan di inventory pemain.
/// Digunakan saat melawan Boss di level boss.
/// 
/// Cara setup:
/// 1. Taruh komponen ini pada GameObject booster di scene level.
/// 2. Pilih tipeBooster dan nilaiBooster.
/// 3. Assign sfxAmbil dan efekAmbil untuk feedback visual/audio.
/// </summary>
public class BoosterCollectible : MonoBehaviour
{
    // ─── Tipe Booster ────────────────────────────────────────────────
    public enum TipeBooster
    {
        [Tooltip("Meningkatkan damage saat boss fight.")]
        Serangan,
        [Tooltip("Mengurangi damage yang diterima saat boss fight.")]
        Pertahanan,
        [Tooltip("Meningkatkan kecepatan gerakan sementara.")]
        Kecepatan
    }

    [Header("Identitas Booster")]
    public TipeBooster tipeBooster = TipeBooster.Serangan;

    [Header("Nilai")]
    [Tooltip("Berapa unit booster yang ditambahkan ke inventory.")]
    [Range(1, 10)]
    public int nilaiBooster = 1;

    [Header("Efek Audio")]
    [Tooltip("SFX saat booster diambil.")]
    public AudioClip sfxAmbil;

    [Header("Efek Visual")]
    [Tooltip("Prefab efek partikel saat diambil. Opsional.")]
    public GameObject efekAmbil;

    [Header("Events")]
    [Tooltip("Event dipanggil saat booster berhasil diambil. Berguna untuk update UI.")]
    public UnityEvent<TipeBooster, int> OnBoosterDiambil;

    // ─── Trigger ─────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerJ player = collision.GetComponent<playerJ>();
        if (player == null) return;

        // Simpan booster ke save
        SaveManager.Instance?.SimpanBooster(nilaiBooster);

        // SFX
        if (sfxAmbil != null)
            player.PlaySFX(sfxAmbil, 0.9f);

        // Efek visual
        if (efekAmbil != null)
            Instantiate(efekAmbil, transform.position, Quaternion.identity);

        // Fire event (untuk update BoosterInventoryUI)
        OnBoosterDiambil?.Invoke(tipeBooster, nilaiBooster);

        int totalBooster = SaveManager.Instance?.MuatJumlahBooster() ?? 0;
        Debug.Log($"[Booster] {tipeBooster} +{nilaiBooster} diambil. Total inventory: {totalBooster}");

        Destroy(gameObject);
    }
}
