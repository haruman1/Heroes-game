using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager — Mesin status utama game dan pengatur alur scene.
/// 
/// ═══════════════════════════════════════════════════════════════
/// ARSITEKTUR SCENE: Boot + Persistent Pattern
/// ═══════════════════════════════════════════════════════════════
/// 
/// CoreScene (Index 0) — Persistent, TIDAK PERNAH di-unload
///   ├── GameManager       ← Script ini
///   ├── AudioManager      ← BGM/SFX/VoiceOver tetap nyambung antar level
///   ├── SaveManager       ← Data player aman selama game berjalan
///   ├── DatabaseManager   ← Koneksi SQLite tidak perlu dibuat ulang
///   ├── UIManager         ← Loading Screen, Fade, HUD, Settings
///   ├── DialogueManager   ← State dialog tidak terputus
///   ├── ShopManager       ← Logika transaksi
///   └── JourneyBookManager← Progress buku tersimpan
///
/// Gameplay Scenes (Additive, berganti-ganti di atas CoreScene):
///   Main Menu → Character Select → Opening Story →
///   Level 1 (+ Boss 1 di dalamnya) → Level 2 (+ Boss 2) → ... → Level 6
///
/// ═══════════════════════════════════════════════════════════════
/// ALUR SAAT GAME START (di Editor maupun Build):
/// ═══════════════════════════════════════════════════════════════
///   1. Unity memuat CoreScene (index 0) secara otomatis
///   2. GameManager.Start() berjalan → deteksi jika tidak ada scene lain
///   3. Jika tidak ada scene lain → load MainMenu secara Additive
///   4. Selanjutnya semua perpindahan scene via GameManager.MuatScene()
///
/// TESTING DI EDITOR:
///   Pasang SceneBootstrapper.cs di scene mana pun (misal LEVEL 1).
///   Tekan Play → CoreScene dimuat otomatis → Manager tersedia → level berjalan.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ─── State Machine ───────────────────────────────────────────────
    public enum GameState
    {
        Boot,            // Saat CoreScene pertama kali dimuat, sebelum scene apapun
        MainMenu,
        CharacterSelect,
        OpeningStory,
        Gameplay,        // Level sedang dimainkan
        BossFight,       // Boss fight dalam level (tidak pindah scene, hanya state)
        LevelEnd,        // Level selesai, sedang transisi ke level berikutnya
        Shop,            // Panel shop terbuka (bisa dari MainMenu atau Pause)
        Paused
    }

    private GameState _stateSaatIni = GameState.Boot;
    public  GameState  StateSaatIni => _stateSaatIni;

    /// <summary>Dipanggil setiap kali state game berubah.</summary>
    public static event Action<GameState> OnGameStateChanged;

    // ─── Nama Scene ──────────────────────────────────────────────────
    [Header("Nama Scene Persistent")]
    [Tooltip("Nama scene yang berisi semua Manager Global. TIDAK PERNAH di-unload.")]
    [SerializeField] private string persistentSceneName = "CoreScene";

    [Header("Nama Scene Gameplay (harus sama persis dengan nama file .unity)")]
    [SerializeField] private string namaSceneMainMenu        = "Main Menu";
    [SerializeField] private string namaSceneCharacterSelect = "CharacterSelect";
    [SerializeField] private string namaSceneOpeningStory    = "OpeningStory";
    [SerializeField] private string namaSceneLevelPertama    = "LEVEL 1";

    // ─── State Internal ──────────────────────────────────────────────
    [Header("State Internal (Read Only di Runtime)")]
    [Tooltip("Nama scene gameplay yang sedang aktif menumpuk di atas CoreScene.")]
    [SerializeField] private string _activeSceneName = "";

    // ─── Lifecycle ───────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // CoreScene TIDAK menggunakan DontDestroyOnLoad — ia bertahan karena
        // tidak pernah di-unload, bukan karena DontDestroyOnLoad.
    }

    private void Start()
    {
        // Cek apakah ada scene lain yang sudah di-load bersama CoreScene
        // (terjadi saat developer menekan Play langsung dari scene Level di Editor)
        bool adaSceneGameplay = false;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            // Abaikan CoreScene sendiri
            if (s.name == persistentSceneName || s.name == gameObject.scene.name) continue;
            if (s.isLoaded)
            {
                _activeSceneName = s.name;
                SceneManager.SetActiveScene(s);
                adaSceneGameplay = true;
                Debug.Log($"[GameManager] Detected scene dari Editor: {s.name}");
                break;
            }
        }

        // Jika tidak ada scene gameplay lain → kita baru start dari CoreScene murni
        // Muat Main Menu secara Additive (inilah fungsi "Boot")
        if (!adaSceneGameplay)
        {
            Debug.Log("[GameManager] Tidak ada scene gameplay. Memuat Main Menu...");
            SetState(GameState.MainMenu);
            StartCoroutine(CoroutineMuatScene(namaSceneMainMenu));
        }
    }

    // ─── State Management ────────────────────────────────────────────
    private void SetState(GameState state)
    {
        _stateSaatIni = state;
        OnGameStateChanged?.Invoke(state);
        Debug.Log($"[GameManager] State → {state}");
    }

    // ─── State Setters (dipanggil oleh LevelManager, BossArenaController, dll) ─
    public void SetGameplayState()  => SetState(GameState.Gameplay);
    public void SetLevelEndState()  => SetState(GameState.LevelEnd);
    public void SetBossFightState() => SetState(GameState.BossFight);
    public void SetPausedState()    => SetState(GameState.Paused);

    // ─── Navigasi Utama ──────────────────────────────────────────────

    /// <summary>New Game: reset semua save lalu muat CharacterSelect.</summary>
    public void MulaiGameBaru()
    {
        SaveManager.Instance?.ResetSave();
        SetState(GameState.CharacterSelect);
        MuatScene(namaSceneCharacterSelect);
    }

    /// <summary>Continue: muat scene terakhir yang disimpan.</summary>
    public void Lanjutkan()
    {
        string namaScene = SaveManager.Instance?.GetNamaSceneTerakhir();
        if (string.IsNullOrEmpty(namaScene))
            namaScene = namaSceneLevelPertama;

        SetState(GameState.Gameplay);
        MuatScene(namaScene);
    }

    /// <summary>Muat scene Opening Story.</summary>
    public void MuatOpeningStory()
    {
        SetState(GameState.OpeningStory);
        MuatScene(namaSceneOpeningStory);
    }

    /// <summary>Kembali ke Main Menu.</summary>
    public void MuatMainMenu()
    {
        Time.timeScale = 1f;
        SetState(GameState.MainMenu);
        MuatScene(namaSceneMainMenu);
    }

    // ─── Pause / Resume ──────────────────────────────────────────────
    public void PauseGame()
    {
        SetState(GameState.Paused);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        SetState(GameState.Gameplay);
        Time.timeScale = 1f;
    }

    // ─── Keluar Game ─────────────────────────────────────────────────
    public void KeluarGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ─── Scene Loading Async (Additive) ──────────────────────────────

    /// <summary>
    /// Muat scene secara async dengan sistem Additive.
    /// Scene lama di-unload dulu, scene baru ditumpuk di atas CoreScene.
    /// CoreScene (PersistentScene) TIDAK PERNAH disentuh.
    /// </summary>
    public void MuatScene(string namaScene)
    {
        if (_activeSceneName == namaScene)
        {
            Debug.LogWarning($"[GameManager] Scene '{namaScene}' sudah aktif, diabaikan.");
            return;
        }
        if (string.IsNullOrEmpty(namaScene))
        {
            Debug.LogError("[GameManager] MuatScene dipanggil dengan nama scene kosong!");
            return;
        }
        StartCoroutine(CoroutineMuatScene(namaScene));
    }

    private IEnumerator CoroutineMuatScene(string namaScene)
    {
        // 1. Fade keluar (layar gelap)
        if (UIManager.Instance != null)
            yield return StartCoroutine(UIManager.Instance.FadeLayar(0.4f, true));

        // 2. Tampilkan loading screen
        UIManager.Instance?.TampilkanLoadingScreen(true);
        UIManager.Instance?.UpdateProgressLoadingScreen(0f);
        UIManager.Instance?.TampilkanHUD(false);

        yield return null; // Satu frame agar loading screen render

        // 3. Unload scene gameplay sebelumnya (bukan CoreScene!)
        if (!string.IsNullOrEmpty(_activeSceneName))
        {
            Scene sceneLama = SceneManager.GetSceneByName(_activeSceneName);
            if (sceneLama.IsValid() && sceneLama.isLoaded)
            {
                Debug.Log($"[GameManager] Unload scene lama: {_activeSceneName}");
                yield return SceneManager.UnloadSceneAsync(_activeSceneName);
                // Paksa GC agar memori dari scene lama dibebaskan segera
                System.GC.Collect();
                yield return null;
            }
        }

        // 4. Load scene baru secara Additive (menumpuk di atas CoreScene)
        Debug.Log($"[GameManager] Load scene baru: {namaScene} (Additive)");
        AsyncOperation op = SceneManager.LoadSceneAsync(namaScene, LoadSceneMode.Additive);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            UIManager.Instance?.UpdateProgressLoadingScreen(progress);

            if (op.progress >= 0.9f)
            {
                UIManager.Instance?.UpdateProgressLoadingScreen(1f);
                yield return new WaitForSecondsRealtime(0.3f);
                op.allowSceneActivation = true;
            }

            yield return null;
        }

        // 5. Set scene baru sebagai Active Scene
        // (agar Instantiate() dari script level masuk ke scene yang benar, bukan CoreScene)
        Scene sceneBaru = SceneManager.GetSceneByName(namaScene);
        if (sceneBaru.IsValid())
            SceneManager.SetActiveScene(sceneBaru);

        _activeSceneName = namaScene;

        // 6. Sembunyikan loading screen + fade masuk (layar terang kembali)
        UIManager.Instance?.TampilkanLoadingScreen(false);
        if (UIManager.Instance != null)
            yield return StartCoroutine(UIManager.Instance.FadeLayar(0.4f, false));

        Debug.Log($"[GameManager] Scene '{namaScene}' aktif. Total scenes loaded: {SceneManager.sceneCount}");
    }
}
