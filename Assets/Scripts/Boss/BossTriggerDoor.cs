using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Dipasang di pintu/terowongan di scene level utama.
/// Saat player mendekati, muncul prompt "Masuk?".
/// Saat dikonfirmasi, pindah ke scene arena boss atau keroco.
/// </summary>
public class BossTriggerDoor : MonoBehaviour
{
    // =============================================
    //  INSPECTOR
    // =============================================

    [Header("Arena Scene")]
    [Tooltip("Nama scene arena yang akan di-load saat player masuk.")]
    public string arenaSceneName = "Boss Arena Level 1";

    [Header("Arena Type Info (untuk display saja)")]
    public bool isBossMode = true; // false = Keroco Mode

    [Header("Visuals")]
    [Tooltip("Sprite pintu terbuka (opsional).")]
    public SpriteRenderer doorSprite;
    public Sprite openSprite;
    public Sprite closedSprite;

    [Tooltip("Efek partikel saat pintu aktif (opsional).")]
    public ParticleSystem doorParticles;

    [Header("Prompt UI")]
    [Tooltip("GameObject panel yang berisi tombol 'Masuk' dan 'Batal'.")]
    public GameObject promptPanel;

    [Header("Status")]
    [Tooltip("Jika true, arena sudah diselesaikan dan pintu dikunci.")]
    public bool isCompleted = false;
    [Tooltip("Jika true, pintu tidak bisa dimasuki (terkunci dari awal).")]
    public bool isLocked    = false;

    [Header("Events")]
    public UnityEvent onPlayerEnterPrompt;
    public UnityEvent onArenaEntered;
    public UnityEvent onArenaWonReturn; // Dipanggil saat kembali dari arena dengan kemenangan

    // =============================================
    //  PRIVATE
    // =============================================

    private bool playerNearby   = false;
    private bool promptShowing  = false;

    // =============================================
    //  UNITY
    // =============================================

    private void Start()
    {
        if (promptPanel != null)
            promptPanel.SetActive(false);

        // Cek apakah player baru kembali dari arena ini dengan kemenangan
        CheckArenaReturn();

        UpdateDoorVisual();
    }

    private void Update()
    {
        if (!playerNearby || promptShowing || isCompleted || isLocked) return;

        // Konfirmasi masuk dengan Enter, E, atau tombol South gamepad
        bool confirm = UnityEngine.InputSystem.Keyboard.current != null &&
                       (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame ||
                        UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame);

        if (confirm)
        {
            ShowPrompt();
        }
    }

    // =============================================
    //  TRIGGER
    // =============================================

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (isCompleted || isLocked) return;

        playerNearby = true;
        Debug.Log($"[BossTriggerDoor] Player mendekati pintu: {arenaSceneName}");

        // Auto-show prompt
        ShowPrompt();
        onPlayerEnterPrompt?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        playerNearby = false;
        HidePrompt();
    }

    // =============================================
    //  PROMPT
    // =============================================

    private void ShowPrompt()
    {
        if (promptShowing) return;
        promptShowing = true;

        if (promptPanel != null) promptPanel.SetActive(true);

        // Pause game agar player bisa memilih
        Time.timeScale = 0f;
    }

    private void HidePrompt()
    {
        promptShowing = false;
        if (promptPanel != null) promptPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // =============================================
    //  BUTTON CALLBACKS (dipanggil dari UI Button)
    // =============================================

    /// <summary>Dipanggil oleh tombol "Masuk" di prompt UI.</summary>
    public void OnConfirmEnter()
    {
        HidePrompt();

        // Set data di bridge sebelum pindah scene
        BossArenaBridge.ReturnSceneName      = SceneManager.GetActiveScene().name;
        BossArenaBridge.ArenaSceneName       = arenaSceneName;
        BossArenaBridge.SourceLevelNumber    = GetCurrentLevelNumber();
        BossArenaBridge.ArenaResult          = BossArenaBridge.Result.None;

        // Tandai state ke BossFight
        GameManager.Instance?.SetBossFightState();

        onArenaEntered?.Invoke();

        Debug.Log($"[BossTriggerDoor] Masuk arena: {arenaSceneName}");

        // ✅ Gunakan GameManager.MuatScene agar PersistentScene (CoreScene) tidak ikut di-unload!
        // AudioManager, SaveManager, dll tetap hidup saat boss fight.
        if (GameManager.Instance != null)
            GameManager.Instance.MuatScene(arenaSceneName);
        else
            SceneManager.LoadScene(arenaSceneName); // Fallback jika Bootstrapper belum jalan
    }

    /// <summary>Dipanggil oleh tombol "Batal" di prompt UI.</summary>
    public void OnCancelEnter()
    {
        HidePrompt();
        Debug.Log("[BossTriggerDoor] Dibatalkan.");
    }

    // =============================================
    //  ARENA RETURN CHECK
    // =============================================

    /// <summary>
    /// Cek apakah player baru kembali dari arena ini dengan kemenangan.
    /// Dipanggil di Start() scene level.
    /// </summary>
    private void CheckArenaReturn()
    {
        if (BossArenaBridge.ArenaResult == BossArenaBridge.Result.None) return;
        if (BossArenaBridge.ArenaSceneName != arenaSceneName) return;

        if (BossArenaBridge.ArenaResult == BossArenaBridge.Result.Won)
        {
            Debug.Log($"[BossTriggerDoor] Player menang di {arenaSceneName}! Kunci pintu.");
            isCompleted = true;
            UpdateDoorVisual();
            onArenaWonReturn?.Invoke();

            // Simpan progres ke database
            SaveArenaCompletion();
        }

        BossArenaBridge.ClearResult();
    }

    private void SaveArenaCompletion()
    {
        // Tandai di database bahwa arena di level ini sudah selesai
        // (bisa dikembangkan untuk menyimpan tipe arena yang diselesaikan)
        Debug.Log($"[BossTriggerDoor] Simpan arena selesai untuk level {GetCurrentLevelNumber()}");
    }

    // =============================================
    //  VISUAL
    // =============================================

    private void UpdateDoorVisual()
    {
        if (doorSprite == null) return;

        if (isCompleted)
        {
            // Pintu sudah selesai — tampilkan berbeda (abu2 / tertutup)
            if (closedSprite != null) doorSprite.sprite = closedSprite;
            doorSprite.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        }
        else if (isLocked)
        {
            doorSprite.color = new Color(0.4f, 0.4f, 0.4f, 1f);
        }
        else
        {
            // Pintu aktif — bercahaya
            if (openSprite != null) doorSprite.sprite = openSprite;
            doorSprite.color = Color.white;
            if (doorParticles != null && !doorParticles.isPlaying)
                doorParticles.Play();
        }
    }

    // =============================================
    //  HELPER
    // =============================================

    private int GetCurrentLevelNumber()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string numStr = "";
        foreach (char c in sceneName)
            if (char.IsDigit(c)) numStr += c;

        return int.TryParse(numStr, out int n) ? n : 1;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isCompleted ? Color.gray : (isBossMode ? Color.red : Color.yellow);
        Gizmos.DrawWireCube(transform.position, new Vector3(2f, 3f, 0));
    }
}
