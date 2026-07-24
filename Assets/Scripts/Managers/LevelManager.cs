using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// LevelManager — Mengatur alur kehidupan tiap level.
/// 
/// Alur:
/// 1. Nonaktifkan input player
/// 2. Dialog intro level (via DialogueManager)
/// 3. Aktifkan input player → gameplay dimulai
/// 4. Player menyentuh Flag → nonaktifkan input
/// 5. Dialog outro level
/// 6. Animasi reward + update Knowledge Bar + buka buku di Journey Book
/// 7. Muat level berikutnya (async)
/// 
/// Dikonfigurasi lewat LevelDataSO di Inspector — tanpa modifikasi kode untuk level baru.
/// Taruh satu instance per level scene.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Konfigurasi Level")]
    [Tooltip("Drag LevelDataSO yang sesuai untuk level ini.")]
    [SerializeField] private LevelDataSO dataLevel;

    [Header("Referensi Scene")]
    [Tooltip("Referensi player. Jika kosong akan dicari otomatis via FindFirstObjectByType.")]
    [SerializeField] private playerJ player;

    [Header("UI Level")]
    [SerializeField] private KnowledgeBarUI knowledgeBarUI;

    [Header("Events")]
    public UnityEvent OnLevelMulai;
    public UnityEvent OnLevelSelesai;
    public UnityEvent<int> OnHalamanDikumpulkan; // int = total halaman sejauh ini

    // ─── State Internal ──────────────────────────────────────────────
    private int  _halamanTerkumpul = 0;
    private bool _levelSelesai     = false;

    // ─── Lifecycle ───────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Cari player jika belum di-assign
        if (player == null)
            player = FindFirstObjectByType<playerJ>();

        if (dataLevel == null)
            Debug.LogWarning("[LevelManager] dataLevel belum di-assign! Assign LevelDataSO di Inspector.");

        MulaiLevel();
    }

    private void OnDestroy()
    {
        // Pastikan listener dibersihkan
        DialogueManager.OnDialogSelesaiStatic -= OnDialogIntroSelesai;
        DialogueManager.OnDialogSelesaiStatic -= OnDialogOutroSelesai;
    }

    // ─── Alur Level ──────────────────────────────────────────────────
    private void MulaiLevel()
    {
        _halamanTerkumpul = 0;
        _levelSelesai     = false;

        // Reset knowledge bar
        int total = dataLevel?.jumlahHalamanDibutuhkan ?? 10;
        knowledgeBarUI?.UpdateBar(0, total);

        // Nonaktifkan input player
        SetInputPlayer(false);

        // Putar BGM level
        if (dataLevel?.bgmLevel != null)
            AudioManager.Instance?.PutarBGM(dataLevel.bgmLevel);

        // Dialog intro
        if (dataLevel?.dialogIntro != null && DialogueManager.Instance != null)
        {
            DialogueManager.OnDialogSelesaiStatic += OnDialogIntroSelesai;
            DialogueManager.Instance.MulaiDialog(dataLevel.dialogIntro);
        }
        else
        {
            AktifkanGameplay();
        }

        OnLevelMulai?.Invoke();
    }

    private void OnDialogIntroSelesai()
    {
        DialogueManager.OnDialogSelesaiStatic -= OnDialogIntroSelesai;
        AktifkanGameplay();
    }

    private void AktifkanGameplay()
    {
        SetInputPlayer(true);
        UIManager.Instance?.TampilkanHUD(true);
        GameManager.Instance?.SetGameplayState();
    }

    // ─── Dipanggil dari HalamanBukuCollectible ───────────────────────

    /// <summary>Panggil ini setiap kali satu halaman buku dikumpulkan.</summary>
    public void LaporHalamanDikumpulkan(int nomorHalaman)
    {
        if (_levelSelesai) return;

        _halamanTerkumpul++;
        int total = dataLevel?.jumlahHalamanDibutuhkan ?? 10;
        knowledgeBarUI?.UpdateBar(_halamanTerkumpul, total);

        SaveManager.Instance?.SimpanProgressHalaman(dataLevel?.nomorLevel ?? 1, nomorHalaman);
        JourneyBookManager.Instance?.BukaHalaman(dataLevel?.nomorLevel ?? 1, nomorHalaman);

        OnHalamanDikumpulkan?.Invoke(_halamanTerkumpul);

        Debug.Log($"[LevelManager] Halaman {_halamanTerkumpul}/{total} terkumpul.");
    }

    // ─── Dipanggil dari Flag.cs ───────────────────────────────────────

    /// <summary>Dipanggil saat player menyentuh Flag (finish point).</summary>
    public void TriggerLevelSelesai()
    {
        if (_levelSelesai) return;
        _levelSelesai = true;

        // Nonaktifkan input
        SetInputPlayer(false);

        // Simpan scene ini sebagai progress terakhir
        if (dataLevel?.levelBerikutnya != null)
            SaveManager.Instance?.SimpanNamaSceneTerakhir(dataLevel.levelBerikutnya.namaScene);

        // Dialog outro
        if (dataLevel?.dialogOutro != null && DialogueManager.Instance != null)
        {
            DialogueManager.OnDialogSelesaiStatic += OnDialogOutroSelesai;
            DialogueManager.Instance.MulaiDialog(dataLevel.dialogOutro);
        }
        else
        {
            StartCoroutine(AnimasiRewardLaluMuat());
        }

        OnLevelSelesai?.Invoke();
    }

    private void OnDialogOutroSelesai()
    {
        DialogueManager.OnDialogSelesaiStatic -= OnDialogOutroSelesai;
        StartCoroutine(AnimasiRewardLaluMuat());
    }

    private IEnumerator AnimasiRewardLaluMuat()
    {
        // Cek apakah buku level ini terbuka (semua halaman terkumpul)
        int total = dataLevel?.jumlahHalamanDibutuhkan ?? 10;
        if (_halamanTerkumpul >= total)
        {
            JourneyBookManager.Instance?.BukaBuku(dataLevel?.nomorLevel ?? 1);
        }

        // Jeda untuk animasi reward (bisa diisi animasi di sini)
        yield return new WaitForSecondsRealtime(1.5f);

        MuatLevelBerikutnya();
    }

    private void MuatLevelBerikutnya()
    {
        GameManager.Instance?.SetLevelEndState();

        if (dataLevel?.levelBerikutnya != null)
        {
            GameManager.Instance?.MuatScene(dataLevel.levelBerikutnya.namaScene);
        }
        else
        {
            // Level terakhir → kembali ke Main Menu
            GameManager.Instance?.MuatMainMenu();
        }
    }

    // ─── Helper ──────────────────────────────────────────────────────
    private void SetInputPlayer(bool aktif)
    {
        if (player != null)
            player.inputEnabled = aktif;
    }

    // ─── Editor Helper ───────────────────────────────────────────────
#if UNITY_EDITOR
    [UnityEngine.ContextMenu("TEST: Trigger Level Selesai")]
    private void TestTriggerLevelSelesai() => TriggerLevelSelesai();
#endif
}
