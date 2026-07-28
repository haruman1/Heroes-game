using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// JourneyBookManager — Mengurus sistem buku perjalanan edukatif.
/// 
/// Setiap level memiliki 10 halaman yang bisa dikumpulkan.
/// Ketika semua 10 halaman terkumpul → 1 Buku utuh terbuka di Journey Book.
/// 
/// Singleton DontDestroyOnLoad.
/// </summary>
public class JourneyBookManager : MonoBehaviour
{
    public static JourneyBookManager Instance { get; private set; }

    [Header("Data Semua Halaman Buku")]
    [Tooltip("Drag semua HalamanBukuSO (dari semua level) ke sini.")]
    [SerializeField] private List<HalamanBukuSO> semuaHalamanBuku = new List<HalamanBukuSO>();

    [Header("Events")]
    /// <summary>Dipanggil ketika sebuah halaman dibuka. Parameter: (nomorLevel, nomorHalaman)</summary>
    public UnityEvent<int, int> OnHalamanTerbuka;

    /// <summary>Dipanggil ketika satu buku utuh terbuka. Parameter: nomorLevel</summary>
    public UnityEvent<int> OnBukuTerbuka;

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

    // ─── Public API ──────────────────────────────────────────────────

    /// <summary>
    /// Buka (unlock) halaman tertentu di level tertentu.
    /// Dipanggil dari HalamanBukuCollectible dan LevelManager.
    /// </summary>
    public void BukaHalaman(int nomorLevel, int nomorHalaman)
    {
        OnHalamanTerbuka?.Invoke(nomorLevel, nomorHalaman);
        Debug.Log($"[JourneyBook] Halaman {nomorHalaman} Level {nomorLevel} terbuka.");

        // Cek apakah semua halaman level ini sudah terkumpul
        LevelProgressData progress = DatabaseManager.GetOrCreateInstance()?.GetLevelProgress(nomorLevel);
        if (progress == null) return;

        int terkumpul = LevelProgressData.CountBits(progress.CollectedBooksMask);
        if (terkumpul >= progress.BooksRequired)
        {
            BukaBuku(nomorLevel);
        }
    }

    /// <summary>
    /// Buka (unlock) satu buku utuh untuk level tertentu.
    /// Dipanggil secara otomatis saat semua halaman terkumpul, atau dari LevelManager.
    /// </summary>
    public void BukaBuku(int nomorLevel)
    {
        OnBukuTerbuka?.Invoke(nomorLevel);
        Debug.Log($"[JourneyBook] ✅ Buku Level {nomorLevel} TERBUKA!");
    }

    /// <summary>Tampilkan panel Journey Book.</summary>
    public void TampilkanJourneyBook()
    {
        UIManager.Instance?.TampilkanJourneyBook(true);
        RefreshUIJourneyBook();
    }

    /// <summary>Sembunyikan panel Journey Book.</summary>
    public void TutupJourneyBook()
    {
        UIManager.Instance?.TampilkanJourneyBook(false);
    }

    // ─── Data Queries ────────────────────────────────────────────────

    /// <summary>Ambil semua HalamanBukuSO untuk level tertentu.</summary>
    public List<HalamanBukuSO> GetHalamanUntukLevel(int nomorLevel)
    {
        return semuaHalamanBuku.FindAll(h => h.nomorLevel == nomorLevel);
    }

    /// <summary>Ambil HalamanBukuSO spesifik berdasarkan level dan nomor halaman.</summary>
    public HalamanBukuSO GetHalaman(int nomorLevel, int nomorHalaman)
    {
        return semuaHalamanBuku.Find(h => h.nomorLevel == nomorLevel && h.idHalaman == nomorHalaman);
    }

    /// <summary>Cek apakah buku untuk level tertentu sudah terbuka.</summary>
    public bool ApakahBukuTerbuka(int nomorLevel)
    {
        LevelProgressData progress = DatabaseManager.GetOrCreateInstance()?.GetLevelProgress(nomorLevel);
        if (progress == null) return false;
        int terkumpul = LevelProgressData.CountBits(progress.CollectedBooksMask);
        return terkumpul >= progress.BooksRequired;
    }

    /// <summary>Cek apakah halaman tertentu sudah terbuka.</summary>
    public bool ApakahHalamanTerbuka(int nomorLevel, int nomorHalaman)
    {
        LevelProgressData progress = DatabaseManager.GetOrCreateInstance()?.GetLevelProgress(nomorLevel);
        if (progress == null) return false;
        if (nomorHalaman < 1 || nomorHalaman > 32) return false;
        return (progress.CollectedBooksMask & (1 << (nomorHalaman - 1))) != 0;
    }

    // ─── UI Refresh ──────────────────────────────────────────────────
    private void RefreshUIJourneyBook()
    {
        // Komponen JourneyBookUI di scene yang aktif akan mendengarkan event OnBukuTerbuka
        // untuk refresh tampilan secara mandiri
    }
}
