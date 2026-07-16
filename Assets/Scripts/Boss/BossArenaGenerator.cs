using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generator procedural untuk scene boss arena.
/// Dipanggil oleh BossArenaController sebelum arena dimulai.
/// Menggenerate:
///   - Platform-platform bertingkat secara acak
///   - Item darah (health pickup) di titik-titik strategis
///   - Posisi spawn bos dan player
/// </summary>
public class BossArenaGenerator : MonoBehaviour
{
    // =============================================
    //  ARENA BOUNDS
    // =============================================

    [Header("Arena Bounds")]
    [Tooltip("Lebar total arena (kiri-kanan).")]
    public float arenaWidth  = 24f;
    [Tooltip("Tinggi total arena (bawah-atas).")]
    public float arenaHeight = 14f;
    [Tooltip("Posisi tengah arena di dunia.")]
    public Vector2 arenaCenter = Vector2.zero;

    // =============================================
    //  PLATFORM GENERATION
    // =============================================

    [Header("Platform Generation")]
    public GameObject platformPrefab;

    [Tooltip("Jumlah platform yang di-generate (tidak termasuk lantai).")]
    public int platformCount = 6;

    [Tooltip("Panjang minimum platform.")]
    public float platformMinWidth = 2f;
    [Tooltip("Panjang maksimum platform.")]
    public float platformMaxWidth = 5f;

    [Tooltip("Ketebalan platform.")]
    public float platformThickness = 0.5f;

    [Tooltip("Jarak minimum antar platform secara vertikal.")]
    public float minVerticalGap   = 2.5f;
    [Tooltip("Jarak maksimum antar platform secara vertikal.")]
    public float maxVerticalGap   = 4.5f;

    [Tooltip("Margin dari tepi kiri/kanan arena.")]
    public float horizontalMargin = 1.5f;

    // =============================================
    //  HEALTH ITEM GENERATION
    // =============================================

    [Header("Health Item (Darah Tambahan)")]
    public GameObject healthPickupPrefab;

    [Tooltip("Jumlah item darah yang di-spawn.")]
    [Range(1, 10)]
    public int healthItemCount = 3;

    [Tooltip("Berapa HP yang dipulihkan per item.")]
    public int healAmount = 1;

    [Tooltip("Apakah item darah muncul di atas platform (jika false, di lantai).")]
    public bool spawnOnPlatforms = true;

    // =============================================
    //  SPAWN POINTS
    // =============================================

    [Header("Spawn Points")]
    [Tooltip("Posisi spawn player. Kosong = auto generate di kiri bawah.")]
    public Transform playerSpawnPoint;
    [Tooltip("Posisi spawn bos. Kosong = auto generate di kanan bawah.")]
    public Transform bossSpawnPoint;

    // =============================================
    //  FLOOR & WALLS
    // =============================================

    [Header("Floor & Walls")]
    [Tooltip("Prefab untuk lantai arena (tile tunggal panjang).")]
    public GameObject floorPrefab;
    [Tooltip("Prefab untuk dinding kiri/kanan (invisible collider opsional).")]
    public GameObject wallPrefab;

    // =============================================
    //  SEED
    // =============================================

    [Header("Generation Seed")]
    [Tooltip("Seed acak. 0 = acak setiap play.")]
    public int seed = 0;
    [Tooltip("Jika true, seed diambil dari level number via BossArenaBridge.")]
    public bool useLevelAsSeed = true;

    // =============================================
    //  READ-ONLY HASIL
    // =============================================

    // Daftar platform yang di-generate (untuk penempatan item)
    private List<GameObject> generatedPlatforms = new List<GameObject>();
    private List<GameObject> generatedItems      = new List<GameObject>();
    private GameObject       generatedFloor;

    // Posisi spawn hasil generate
    [HideInInspector] public Vector3 GeneratedPlayerSpawn;
    [HideInInspector] public Vector3 GeneratedBossSpawn;

    // =============================================
    //  UNITY
    // =============================================

    private void Awake()
    {
        // Seed diatur di Awake agar Generate bisa dipanggil di Start komponen lain
        int finalSeed = seed;
        if (useLevelAsSeed && BossArenaBridge.SourceLevelNumber > 0)
            finalSeed = BossArenaBridge.SourceLevelNumber * 137 + 42;

        if (finalSeed == 0)
            finalSeed = Random.Range(1, 99999);

        Random.InitState(finalSeed);
        Debug.Log($"[Generator] Seed arena: {finalSeed}");
    }

    private void Start()
    {
        GenerateArena();
    }

    // =============================================
    //  GENERATE ARENA
    // =============================================

    /// <summary>
    /// Entry point — generate semua elemen arena secara prosedural.
    /// </summary>
    public void GenerateArena()
    {
        ClearPrevious();
        GenerateFloor();
        GenerateWalls();
        GeneratePlatforms();
        GenerateHealthItems();
        SetupSpawnPoints();

        Debug.Log($"[Generator] Arena selesai di-generate: " +
                  $"{generatedPlatforms.Count} platform, {generatedItems.Count} item darah.");
    }

    // =============================================
    //  FLOOR
    // =============================================

    private void GenerateFloor()
    {
        if (floorPrefab == null) return;

        Vector3 floorPos = new Vector3(arenaCenter.x, arenaCenter.y - arenaHeight / 2f + platformThickness / 2f, 0);
        GameObject floor = Instantiate(floorPrefab, floorPos, Quaternion.identity, transform);
        floor.name = "Floor";

        // Scale lantai sesuai lebar arena
        floor.transform.localScale = new Vector3(arenaWidth, platformThickness, 1f);

        generatedFloor = floor;
    }

    // =============================================
    //  WALLS
    // =============================================

    private void GenerateWalls()
    {
        if (wallPrefab == null) return;

        float wallX_L = arenaCenter.x - arenaWidth / 2f;
        float wallX_R = arenaCenter.x + arenaWidth / 2f;
        float wallY   = arenaCenter.y;
        float wallH   = arenaHeight;

        GameObject wallL = Instantiate(wallPrefab, new Vector3(wallX_L, wallY, 0), Quaternion.identity, transform);
        wallL.transform.localScale = new Vector3(0.5f, wallH, 1f);
        wallL.name = "Wall_Left";

        GameObject wallR = Instantiate(wallPrefab, new Vector3(wallX_R, wallY, 0), Quaternion.identity, transform);
        wallR.transform.localScale = new Vector3(0.5f, wallH, 1f);
        wallR.name = "Wall_Right";
    }

    // =============================================
    //  PLATFORMS
    // =============================================

    private void GeneratePlatforms()
    {
        if (platformPrefab == null)
        {
            Debug.LogWarning("[Generator] platformPrefab belum di-assign!");
            return;
        }

        // Rentang Y untuk platform (mulai dari 1.5 di atas lantai, sampai 80% tinggi arena)
        float floorY    = arenaCenter.y - arenaHeight / 2f + platformThickness;
        float ceilingY  = arenaCenter.y + arenaHeight / 2f - 1.5f;
        float leftBound = arenaCenter.x - arenaWidth / 2f + horizontalMargin;
        float rightBound= arenaCenter.x + arenaWidth / 2f - horizontalMargin;

        float currentY  = floorY + Random.Range(minVerticalGap, maxVerticalGap);
        bool  placeLeft = Random.value > 0.5f; // Mulai dari kiri atau kanan secara bergantian

        for (int i = 0; i < platformCount; i++)
        {
            if (currentY > ceilingY) break;

            float width = Random.Range(platformMinWidth, platformMaxWidth);

            // Bergantian kiri-kanan untuk lebih mudah dijangkau player
            float xMin = placeLeft
                ? leftBound
                : rightBound - width - Random.Range(0f, 2f);
            float xMax = placeLeft
                ? leftBound + Random.Range(0f, 2f) + width
                : rightBound;

            xMin = Mathf.Clamp(xMin, leftBound, rightBound - width);
            xMax = Mathf.Clamp(xMax, leftBound + width, rightBound);

            float centerX = (xMin + xMax) / 2f;
            float actualWidth = xMax - xMin;
            actualWidth = Mathf.Clamp(actualWidth, platformMinWidth, platformMaxWidth);

            Vector3 pos = new Vector3(centerX, currentY, 0f);
            GameObject platform = Instantiate(platformPrefab, pos, Quaternion.identity, transform);
            platform.name = $"Platform_{i}";
            platform.transform.localScale = new Vector3(actualWidth, platformThickness, 1f);

            generatedPlatforms.Add(platform);

            // Naikkan Y untuk platform berikutnya
            currentY += Random.Range(minVerticalGap, maxVerticalGap);
            placeLeft = !placeLeft; // Alternasi sisi
        }
    }

    // =============================================
    //  HEALTH ITEMS
    // =============================================

    private void GenerateHealthItems()
    {
        if (healthPickupPrefab == null)
        {
            Debug.LogWarning("[Generator] healthPickupPrefab belum di-assign!");
            return;
        }

        List<Vector3> candidates = GetItemSpawnCandidates();

        // Acak urutan kandidat
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        int spawnCount = Mathf.Min(healthItemCount, candidates.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = candidates[i] + Vector3.up * 0.6f; // sedikit di atas permukaan
            GameObject item = Instantiate(healthPickupPrefab, pos, Quaternion.identity, transform);
            item.name = $"HealthPickup_{i}";

            // Set heal amount jika ada komponen HealthPickup
            HealthPickup hp = item.GetComponent<HealthPickup>();
            if (hp != null) hp.healAmount = healAmount;

            generatedItems.Add(item);
        }

        Debug.Log($"[Generator] {spawnCount} item darah di-spawn.");
    }

    /// <summary>
    /// Kumpulkan posisi kandidat untuk item: di atas platform dan di lantai.
    /// </summary>
    private List<Vector3> GetItemSpawnCandidates()
    {
        List<Vector3> candidates = new List<Vector3>();
        float topSurface = platformThickness / 2f;

        if (spawnOnPlatforms)
        {
            // Di atas setiap platform
            foreach (GameObject p in generatedPlatforms)
            {
                if (p == null) continue;
                float pW = p.transform.localScale.x;
                // Tambahkan 1–3 kandidat tersebar di sepanjang platform
                int perPlatform = Mathf.Max(1, Mathf.RoundToInt(pW / 2f));
                for (int k = 0; k < perPlatform; k++)
                {
                    float xOffset = Random.Range(-pW * 0.4f, pW * 0.4f);
                    candidates.Add(p.transform.position + new Vector3(xOffset, topSurface, 0));
                }
            }
        }

        // Tambahkan kandidat di lantai (pojok-pojok arena)
        float floorY   = arenaCenter.y - arenaHeight / 2f + topSurface;
        float leftX    = arenaCenter.x - arenaWidth / 2f + 2f;
        float rightX   = arenaCenter.x + arenaWidth / 2f - 2f;
        candidates.Add(new Vector3(leftX,  floorY, 0));
        candidates.Add(new Vector3(rightX, floorY, 0));
        candidates.Add(new Vector3(arenaCenter.x, floorY, 0));

        return candidates;
    }

    // =============================================
    //  SPAWN POINTS
    // =============================================

    private void SetupSpawnPoints()
    {
        float floorY = arenaCenter.y - arenaHeight / 2f + 1.5f;

        // Player spawn: kiri bawah
        if (playerSpawnPoint != null)
        {
            GeneratedPlayerSpawn = playerSpawnPoint.position;
        }
        else
        {
            GeneratedPlayerSpawn = new Vector3(arenaCenter.x - arenaWidth * 0.35f, floorY, 0);
        }

        // Boss spawn: kanan bawah
        if (bossSpawnPoint != null)
        {
            GeneratedBossSpawn = bossSpawnPoint.position;
        }
        else
        {
            GeneratedBossSpawn = new Vector3(arenaCenter.x + arenaWidth * 0.35f, floorY, 0);
        }
    }

    // =============================================
    //  CLEANUP
    // =============================================

    private void ClearPrevious()
    {
        foreach (GameObject g in generatedPlatforms)
            if (g != null) Destroy(g);
        foreach (GameObject g in generatedItems)
            if (g != null) Destroy(g);
        if (generatedFloor != null) Destroy(generatedFloor);

        generatedPlatforms.Clear();
        generatedItems.Clear();
    }

    // =============================================
    //  GIZMOS
    // =============================================

    private void OnDrawGizmos()
    {
        // Arena bounds
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
        Gizmos.DrawCube(new Vector3(arenaCenter.x, arenaCenter.y, 0),
                        new Vector3(arenaWidth, arenaHeight, 0.1f));

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(new Vector3(arenaCenter.x, arenaCenter.y, 0),
                            new Vector3(arenaWidth, arenaHeight, 0.1f));

        // Spawn points
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(GeneratedPlayerSpawn == Vector3.zero
            ? new Vector3(arenaCenter.x - arenaWidth * 0.35f, arenaCenter.y - arenaHeight / 2f + 1.5f, 0)
            : GeneratedPlayerSpawn, 0.4f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GeneratedBossSpawn == Vector3.zero
            ? new Vector3(arenaCenter.x + arenaWidth * 0.35f, arenaCenter.y - arenaHeight / 2f + 1.5f, 0)
            : GeneratedBossSpawn, 0.4f);
    }
}
