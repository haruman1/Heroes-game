using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Mengontrol keseluruhan sesi arena boss.
/// Bekerja bersama BossArenaGenerator untuk:
///   1. Generate arena secara prosedural (platform + item darah)
///   2. Spawn player dan bos di posisi yang di-generate
///   3. Mengelola state Fighting → Won/Lost
///   4. Kembali ke scene level setelah arena selesai
/// </summary>
[RequireComponent(typeof(BossArenaGenerator))]
public class BossArenaController : MonoBehaviour
{
    // =============================================
    //  INSPECTOR
    // =============================================

    [Header("Arena Info")]
    public string arenaName   = "Goa Kegelapan";
    public int    levelNumber = 1;

    [Header("Boss")]
    [Tooltip("Prefab bos yang akan di-spawn setelah arena di-generate.")]
    public GameObject bossPrefab;

    [Header("Player")]
    [Tooltip("Prefab player yang akan di-spawn di scene arena.")]
    public GameObject playerPrefab;

    [Header("Scene Navigation")]
    [Tooltip("Nama scene level yang akan dikembalikan setelah arena selesai.")]
    public string returnSceneName = "LEVEL 1";
    [Tooltip("Jika diisi, load scene ini saat menang (misal cutscene). Kosong = langsung balik ke level.")]
    public string winSceneName    = "";

    [Header("UI")]
    public BossHUDManager hud;

    [Header("Events")]
    public UnityEvent onArenaStart;
    public UnityEvent onArenaWon;
    public UnityEvent onArenaLost;

    // =============================================
    //  STATE
    // =============================================

    private enum ArenaState { Generating, Fighting, Won, Lost }
    private ArenaState arenaState = ArenaState.Generating;

    private BossArenaGenerator generator;
    private BossController     spawnedBoss;
    private GameObject         spawnedPlayer;

    // =============================================
    //  UNITY
    // =============================================

    private void Awake()
    {
        generator = GetComponent<BossArenaGenerator>();

        // Override nama scene kembali dari bridge jika ada
        if (!string.IsNullOrEmpty(BossArenaBridge.ReturnSceneName))
            returnSceneName = BossArenaBridge.ReturnSceneName;

        if (BossArenaBridge.SourceLevelNumber > 0)
            levelNumber = BossArenaBridge.SourceLevelNumber;
    }

    private void Start()
    {
        // Generator sudah jalan di Start-nya sendiri (Awake = seed, Start = generate)
        // Kita tunggu 1 frame agar generator selesai, lalu mulai arena
        StartCoroutine(StartAfterGeneration());
    }

    private IEnumerator StartAfterGeneration()
    {
        // Tunggu 1 frame agar BossArenaGenerator.Start() selesai menggenerate
        yield return null;

        SpawnPlayerAndBoss();
        StartArena();
    }

    // =============================================
    //  SPAWN
    // =============================================

    private void SpawnPlayerAndBoss()
    {
        // --- Spawn Player ---
        if (playerPrefab != null)
        {
            spawnedPlayer = Instantiate(playerPrefab, generator.GeneratedPlayerSpawn, Quaternion.identity);
            Debug.Log($"[Arena] Player di-spawn di {generator.GeneratedPlayerSpawn}");
        }
        else
        {
            // Cari player yang mungkin sudah ada di scene (saat testing di editor)
            GameObject existing = GameObject.FindGameObjectWithTag("Player");
            if (existing != null)
            {
                existing.transform.position = generator.GeneratedPlayerSpawn;
                spawnedPlayer = existing;
            }
        }

        // --- Spawn Boss ---
        if (bossPrefab != null)
        {
            GameObject bossObj = Instantiate(bossPrefab, generator.GeneratedBossSpawn, Quaternion.identity);
            spawnedBoss = bossObj.GetComponent<BossController>();
            Debug.Log($"[Arena] Boss di-spawn di {generator.GeneratedBossSpawn}");
        }
        else
        {
            // Fallback: cari BossController yang sudah ada di scene
            spawnedBoss = FindFirstObjectByType<BossController>();
            if (spawnedBoss != null)
                spawnedBoss.transform.position = generator.GeneratedBossSpawn;
        }
    }

    // =============================================
    //  ARENA FLOW
    // =============================================

    public void StartArena()
    {
        arenaState = ArenaState.Fighting;

        if (hud != null)
        {
            hud.ShowArenaHUD(arenaName);

            if (spawnedBoss != null)
                hud.SetBossInfo(spawnedBoss.bossName, 1f);
        }

        if (spawnedBoss != null)
        {
            spawnedBoss.onBossDead.AddListener(OnBossDead);
            spawnedBoss.onHealthChanged.AddListener(OnBossHealthChanged);
            spawnedBoss.onPhase2Entered.AddListener(OnBossPhase2);
        }
        else
        {
            Debug.LogWarning("[Arena] Tidak ada BossController di scene!");
        }

        onArenaStart?.Invoke();
        Debug.Log($"[Arena] Arena dimulai: {arenaName} (Level {levelNumber})");
    }

    // =============================================
    //  BOSS EVENTS
    // =============================================

    private void OnBossDead()
    {
        if (arenaState != ArenaState.Fighting) return;
        Debug.Log("[Arena] Boss mati! Arena dimenangkan.");
        WinArena();
    }

    private void OnBossHealthChanged(float ratio)
    {
        if (hud != null) hud.UpdateBossHealth(ratio);
    }

    private void OnBossPhase2()
    {
        if (hud != null) hud.ShowPhase2Warning();
    }

    // =============================================
    //  WIN / LOSE
    // =============================================

    public void WinArena()
    {
        if (arenaState == ArenaState.Won || arenaState == ArenaState.Lost) return;
        arenaState = ArenaState.Won;

        BossArenaBridge.ArenaResult        = BossArenaBridge.Result.Won;
        BossArenaBridge.CompletedArenaType = "BossMode";

        if (hud != null) hud.ShowWinScreen();
        onArenaWon?.Invoke();

        StartCoroutine(LoadReturnScene(3f));
    }

    public void LoseArena()
    {
        if (arenaState == ArenaState.Won || arenaState == ArenaState.Lost) return;
        arenaState = ArenaState.Lost;

        BossArenaBridge.ArenaResult = BossArenaBridge.Result.Lost;

        if (hud != null) hud.ShowLoseScreen();
        onArenaLost?.Invoke();

        StartCoroutine(LoadReturnScene(3f));
    }

    private IEnumerator LoadReturnScene(float delay)
    {
        yield return new WaitForSeconds(delay);

        string targetScene;
        if (!string.IsNullOrEmpty(winSceneName) && arenaState == ArenaState.Won)
            targetScene = winSceneName;
        else if (!string.IsNullOrEmpty(returnSceneName))
            targetScene = returnSceneName;
        else
            targetScene = "Main Menu";

        // ✅ Gunakan GameManager.MuatScene agar PersistentScene (CoreScene) tidak ikut di-unload!
        if (GameManager.Instance != null)
            GameManager.Instance.MuatScene(targetScene);
        else
            SceneManager.LoadScene(targetScene); // Fallback
    }
}
