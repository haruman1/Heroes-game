using System;
using UnityEngine;

/// <summary>
/// ShopManager — Sistem transaksi Toko Item Boost untuk Main Menu.
/// 
/// MENDUKUNG 2 MODE TRANSAKSI (ITCH.IO COMPATIBLE):
/// ─────────────────────────────────────────────────────────────────
/// 1. MODE KOIN GAME:
///    - Dibeli menggunakan Koin hasil mengumpulkan koin dari gameplay level.
///    - Mengurangi koin player dan menambahkan item booster ke simpanan.
/// 
/// 2. MODE UANG ASLI / ITCH.IO SUPPORTER:
///    - Membuka tautan pembelian/donasi Itch.io (Application.OpenURL).
///    - Memberikan Paket Booster Supporter + bonus koin sebagai hadiah dukungan.
///    - Kompatibel dengan WebGL, Windows Desktop, dan Mac di Itch.io.
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Konfigurasi Itch.io Store")]
    [Tooltip("URL halaman toko / donasi game kamu di Itch.io (contoh: https://username.itch.io/heroes-game)")]
    [SerializeField] private string urlTokoItchIo = "https://itch.io";

    [Header("Audio Feedback")]
    [SerializeField] private AudioClip sfxSuksesBeli;
    [SerializeField] private AudioClip sfxGagalBeli;

    // ─── Events Transaksi ─────────────────────────────────────────────
    public static event Action<string, bool> OnTransaksiSelesai; // (pesan, isSuccess)

    // ─── Lifecycle ───────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // Diganti dengan arsitektur Additive CoreScene
    }

    // ─── 1. TRANSAKSI MENGGUNAKAN KOIN ────────────────────────────────

    /// <summary>
    /// Membeli Item Booster menggunakan Koin In-Game.
    /// </summary>
    /// <param name="hargaKoin">Jumlah koin yang dibutuhkan.</param>
    /// <param name="jumlahBooster">Jumlah booster yang didapat.</param>
    /// <param name="namaItem">Nama paket/item.</param>
    public bool BeliDenganKoin(int hargaKoin, int jumlahBooster, string namaItem)
    {
        int koinSaatIni = SaveManager.Instance?.MuatCoin() ?? 0;

        if (koinSaatIni < hargaKoin)
        {
            PlaySFX(sfxGagalBeli);
            string pesanGagal = $"Koin kamu kurang! Butuh {hargaKoin} koin, koin kamu saat ini: {koinSaatIni}. Kumpulkan lagi di level!";
            Debug.LogWarning($"[ShopManager] GAGAL: {pesanGagal}");
            OnTransaksiSelesai?.Invoke(pesanGagal, false);
            return false;
        }

        // Potong Koin
        bool sukses = SaveManager.Instance?.KurangiCoin(hargaKoin) ?? false;

        if (sukses)
        {
            // Tambah Booster
            SaveManager.Instance?.SimpanBooster(jumlahBooster);
            PlaySFX(sfxSuksesBeli);

            string pesanSukses = $"Berhasil membeli {namaItem} (+{jumlahBooster} Booster)! Sisa koin: {SaveManager.Instance.MuatCoin()}";
            Debug.Log($"[ShopManager] SUKSES: {pesanSukses}");
            OnTransaksiSelesai?.Invoke(pesanSukses, true);
            return true;
        }

        OnTransaksiSelesai?.Invoke("Transaksi gagal dikarenakan masalah database.", false);
        return false;
    }

    // ─── 2. TRANSAKSI UANG ASLI / ITCH.IO SUPPORTER ──────────────────

    /// <summary>
    /// Membeli Paket Booster Supporter via Uang Asli / Itch.io Store.
    /// Membuka URL Itch.io Store dan memberikan bonus item ke pemain.
    /// </summary>
    public void BeliDenganUangAsli(int jumlahBoosterBonus, int bonusKoin, string namaItem, string urlCustom = "")
    {
        string targetURL = string.IsNullOrEmpty(urlCustom) ? urlTokoItchIo : urlCustom;

        // 1. Tambahkan Item Supporter ke Inventory Player
        SaveManager.Instance?.SimpanBooster(jumlahBoosterBonus);
        if (bonusKoin > 0)
        {
            SaveManager.Instance?.TambahCoin(bonusKoin);
        }

        PlaySFX(sfxSuksesBeli);

        // 2. Buka Halaman Itch.io di Browser
        if (!string.IsNullOrEmpty(targetURL))
        {
            Application.OpenURL(targetURL);
        }

        string pesan = $"Terima kasih atas dukunganmu! Paket {namaItem} (+{jumlahBoosterBonus} Booster & +{bonusKoin} Koin) telah diklaim. Halaman Toko Itch.io sedang dibuka...";
        Debug.Log($"[ShopManager] ITCH.IO PURCHASE: {pesan}");
        OnTransaksiSelesai?.Invoke(pesan, true);
    }

    // ─── Helper Audio ────────────────────────────────────────────────
    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PutarSFX(clip);
        }
    }
}
