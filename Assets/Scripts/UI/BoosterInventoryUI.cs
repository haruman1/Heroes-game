using UnityEngine;
using TMPro;

/// <summary>
/// BoosterInventoryUI — Menampilkan jumlah booster di HUD.
/// 
/// Refresh otomatis saat diaktifkan (OnEnable) dan dapat dipanggil
/// secara manual setelah booster diambil.
/// </summary>
public class BoosterInventoryUI : MonoBehaviour
{
    [Header("Komponen UI")]
    [SerializeField] private TMP_Text teksJumlahBooster;
    [Tooltip("Ikon booster — tersembunyi jika jumlah = 0.")]
    [SerializeField] private GameObject ikonBooster;

    // ─── Lifecycle ───────────────────────────────────────────────────
    private void OnEnable()
    {
        RefreshUI();
    }

    // ─── Public API ──────────────────────────────────────────────────

    /// <summary>Refresh tampilan dari data save terbaru.</summary>
    public void RefreshUI()
    {
        int jumlah = SaveManager.Instance?.MuatJumlahBooster() ?? 0;

        if (teksJumlahBooster != null)
            teksJumlahBooster.text = $"x{jumlah}";

        if (ikonBooster != null)
            ikonBooster.SetActive(jumlah > 0);
    }

    /// <summary>
    /// Dipanggil dari BoosterCollectible.OnBoosterDiambil UnityEvent.
    /// Parameter tipe dan nilai diabaikan — hanya trigger refresh.
    /// </summary>
    public void OnBoosterDiambil(BoosterCollectible.TipeBooster tipe, int nilai)
    {
        RefreshUI();
    }
}
