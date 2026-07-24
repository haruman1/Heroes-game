using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager — Mesin status utama game.
/// Mengatur state (MainMenu, CharacterSelect, OpeningStory, Gameplay, LevelEnd, Paused),
/// memuat scene secara async dengan loading screen, dan menjadi titik pusat orkestrasi.
/// 
/// Singleton DontDestroyOnLoad — taruh pada GameObject "GameManager" di scene pertama.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ─── State Machine ───────────────────────────────────────────────
    public enum GameState
    {
        MainMenu,
        CharacterSelect,
        OpeningStory,
        Gameplay,
        LevelEnd,
        Paused
    }

    private GameState _stateSaatIni;
    public GameState StateSaatIni => _stateSaatIni;

    /// <summary>Dipanggil setiap kali state game berubah.</summary>
    public static event Action<GameState> OnGameStateChanged;

    // ─── Nama Scene ──────────────────────────────────────────────────
    [Header("Nama Scene (harus sama persis dengan nama file .unity)")]
    [SerializeField] private string namaSceneMainMenu       = "MainMenu";
    [SerializeField] private string namaSceneCharacterSelect= "CharacterSelect";
    [SerializeField] private string namaSceneOpeningStory   = "OpeningStory";
    [SerializeField] private string namaSceneLevelPertama   = "LEVEL 1";

    // ─── Lifecycle ───────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─── State Management ────────────────────────────────────────────
    private void SetState(GameState state)
    {
        _stateSaatIni = state;
        OnGameStateChanged?.Invoke(state);
    }

    public void SetGameplayState() => SetState(GameState.Gameplay);
    public void SetLevelEndState()  => SetState(GameState.LevelEnd);

    // ─── Aksi Utama ──────────────────────────────────────────────────

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

    /// <summary>Muat scene Opening Story (4 video sinematik).</summary>
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

    // ─── Scene Loading Async ─────────────────────────────────────────

    /// <summary>Muat scene secara async dengan loading screen.</summary>
    public void MuatScene(string namaScene)
    {
        StartCoroutine(CoroutineMuatScene(namaScene));
    }

    private IEnumerator CoroutineMuatScene(string namaScene)
    {
        // Tampilkan loading screen
        UIManager.Instance?.TampilkanLoadingScreen(true);
        UIManager.Instance?.UpdateProgressLoadingScreen(0f);

        yield return null; // Satu frame agar loading screen render

        AsyncOperation op = SceneManager.LoadSceneAsync(namaScene);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            UIManager.Instance?.UpdateProgressLoadingScreen(progress);

            if (op.progress >= 0.9f)
            {
                UIManager.Instance?.UpdateProgressLoadingScreen(1f);
                yield return new WaitForSecondsRealtime(0.3f); // Jeda singkat agar progress 100% terlihat
                op.allowSceneActivation = true;
            }

            yield return null;
        }

        UIManager.Instance?.TampilkanLoadingScreen(false);
    }
}
