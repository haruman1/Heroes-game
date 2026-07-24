using System.Collections.Generic;
using System.IO;
using SQLite;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    private const int DefaultPlayerId = 1;
    private const int DefaultSettingId = 1;
    private const string ManagerObjectName = "DatabaseManager_AutoBootstrap";
    private const int DefaultCoin = 0;
    private const int DefaultLevel = 1;
    private const int DefaultHp = 100;
    private const int DefaultHeart = 3;
    private const int DefaultFpsLimit = 60;
    private const float DefaultMasterVolume = 1f;
    private const float DefaultMusicVolume = 1f;
    private const float DefaultSfxVolume = 1f;
    private const float DefaultCameraZoom = 5f;
    private const int DefaultFullscreen = 1;
    private const int DefaultResolutionWidth = 1920;
    private const int DefaultResolutionHeight = 1080;
    private const string DefaultLanguage = "system";

    [Header("Development")]
    [SerializeField]
    private bool enableShortLogs = true;

    public static DatabaseManager Instance;
    private SQLiteConnection db;
    public SQLiteConnection Connection => db;

    public static DatabaseManager GetOrCreateInstance()
    {
        if (Instance != null)
        {
            Instance.EnsureDatabaseReady();
            return Instance;
        }

        DatabaseManager existingManager = FindFirstObjectByType<DatabaseManager>();
        if (existingManager != null)
        {
            Instance = existingManager;
            Instance.EnsureDatabaseReady();
            return Instance;
        }

        GameObject managerObject = new GameObject(ManagerObjectName);
        Instance = managerObject.AddComponent<DatabaseManager>();
        return Instance;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoBootstrap()
    {
        GetOrCreateInstance();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            gameObject.name = ManagerObjectName;
            DontDestroyOnLoad(gameObject);
            EnsureDatabaseReady();
        }
        else if (Instance == this)
        {
            EnsureDatabaseReady();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void EnsureDatabaseReady()
    {
        if (db != null)
            return;

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        string dbPath = Path.Combine(Application.persistentDataPath, "game_database.db");
        db = new SQLiteConnection(dbPath);
        db.CreateTable<PlayerData>();
        db.CreateTable<GameSettingsData>();
        db.CreateTable<LevelProgressData>();
        MigrasiDatabase();
        CreateDefaultPlayer();
        CreateDefaultSettings();
        InitializeDefaultLevels();
        LogShort($"INIT path={dbPath}");
    }

    /// <summary>
    /// Menambah kolom baru ke tabel yang sudah ada secara aman.
    /// SQLite-net tidak otomatis menambah kolom baru pada tabel yang sudah ada,
    /// sehingga perlu ALTER TABLE manual. Exception diabaikan jika kolom sudah ada.
    /// </summary>
    private void MigrasiDatabase()
    {
        // PlayerData — kolom baru sistem baru
        try { db.Execute("ALTER TABLE PlayerData ADD COLUMN Gender TEXT DEFAULT ''"); } catch { }
        try { db.Execute("ALTER TABLE PlayerData ADD COLUMN JumlahBooster INTEGER DEFAULT 0"); } catch { }
        try { db.Execute("ALTER TABLE PlayerData ADD COLUMN NamaSceneTerakhir TEXT DEFAULT ''"); } catch { }
        LogShort("MIGRATE database columns applied");
    }

    private void CreateDefaultPlayer()
    {
        if (db.Table<PlayerData>().Count() == 0)
        {
            db.Insert(
                new PlayerData
                {
                    Id = DefaultPlayerId,
                    Coin = DefaultCoin,
                    Level = DefaultLevel,
                    HP = DefaultHp,
                    Heart = DefaultHeart,
                    SelectedCharacter = "Raka",
                    SelectedAge = 10,
                }
            );

            LogShort("CREATE default-player");
        }
    }

    public void SaveCharacterSelection(string character, int age)
    {
        EnsureDatabaseReady();

        if (db == null)
            return;

        PlayerData player = GetPlayerData();
        if (player == null)
            return;

        player.SelectedCharacter = character;
        player.SelectedAge = age;

        db.Update(player);
        LogShort($"SAVE SELECTION char={character}, age={age}");
    }

    private void CreateDefaultSettings()
    {
        if (db.Table<GameSettingsData>().Count() != 0)
            return;

        db.Insert(
            new GameSettingsData
            {
                Id = DefaultSettingId,
                FpsLimit = DefaultFpsLimit,
                MasterVolume = DefaultMasterVolume,
                MusicVolume = DefaultMusicVolume,
                SfxVolume = DefaultSfxVolume,
                CameraZoom = DefaultCameraZoom,
                Fullscreen = DefaultFullscreen,
                ResolutionWidth = DefaultResolutionWidth,
                ResolutionHeight = DefaultResolutionHeight,
                Language = DefaultLanguage,
            }
        );

        LogShort("CREATE default-settings");
    }

    public PlayerData GetPlayerData()
    {
        EnsureDatabaseReady();

        if (db == null)
            return null;

        PlayerData player = db.Find<PlayerData>(DefaultPlayerId);

        if (player == null)
        {
            CreateDefaultPlayer();
            player = db.Find<PlayerData>(DefaultPlayerId);
        }

        if (player != null)
        {
            LogShort(
                $"LOAD coin={player.Coin}, lvl={player.Level}, hp={player.HP}, heart={player.Heart}"
            );
        }

        return player;
    }

    public GameSettingsData GetSettingsData()
    {
        EnsureDatabaseReady();

        if (db == null)
            return null;

        GameSettingsData settings = db.Find<GameSettingsData>(DefaultSettingId);

        if (settings == null)
        {
            CreateDefaultSettings();
            settings = db.Find<GameSettingsData>(DefaultSettingId);
        }

        if (settings != null)
        {
            LogShort(
                $"LOAD settings fps={settings.FpsLimit}, master={settings.MasterVolume:0.00}, music={settings.MusicVolume:0.00}, sfx={settings.SfxVolume:0.00}, zoom={settings.CameraZoom:0.00}, fullscreen={settings.Fullscreen}, res={settings.ResolutionWidth}x{settings.ResolutionHeight}, lang={settings.Language}"
            );
        }

        return settings;
    }

    public void SaveSettings(GameSettingsData settings)
    {
        EnsureDatabaseReady();

        if (db == null || settings == null)
            return;

        settings.Id = DefaultSettingId;
        db.InsertOrReplace(settings);
        LogShort(
            $"SAVE settings fps={settings.FpsLimit}, master={settings.MasterVolume:0.00}, music={settings.MusicVolume:0.00}, sfx={settings.SfxVolume:0.00}, zoom={settings.CameraZoom:0.00}, fullscreen={settings.Fullscreen}, res={settings.ResolutionWidth}x{settings.ResolutionHeight}, lang={settings.Language}"
        );
    }

    public void SavePlayerState(int coin, int level, int hp, int heart)
    {
        EnsureDatabaseReady();

        if (db == null)
            return;

        PlayerData player = GetPlayerData();
        if (player == null)
            return;

        player.Coin = Mathf.Max(0, coin);
        player.Level = Mathf.Max(1, level);
        player.HP = Mathf.Max(0, hp);
        player.Heart = Mathf.Max(0, heart);

        db.Update(player);
        LogShort(
            $"SAVE coin={player.Coin}, lvl={player.Level}, hp={player.HP}, heart={player.Heart}"
        );
    }

    public void AddCoin(int amount)
    {
        EnsureDatabaseReady();

        if (db == null)
            return;

        PlayerData player = GetPlayerData();
        if (player == null)
            return;

        player.Coin = Mathf.Max(0, player.Coin + amount);
        db.Update(player);
        LogShort($"COIN +{amount} => {player.Coin}");
    }

    public void UnlockLevel(int level)
    {
        EnsureDatabaseReady();

        if (db == null)
            return;

        PlayerData player = GetPlayerData();
        if (player == null)
            return;

        player.Level = Mathf.Max(player.Level, level);
        db.Update(player);
        LogShort($"UNLOCK level={player.Level}");
    }

    // =========================================================
    //  LEVEL PROGRESS — Journey System
    // =========================================================

    private const int TotalLevels = 6;
    private const int DefaultBooksRequired = 10;

    /// <summary>
    /// Buat 6 baris default di tabel LevelProgressData.
    /// Level 1 langsung terbuka, Level 2-6 terkunci.
    /// Dipanggil sekali saat database pertama dibuat.
    /// </summary>
    private void InitializeDefaultLevels()
    {
        EnsureDatabaseReady();
        if (db == null) return;

        if (db.Table<LevelProgressData>().Count() > 0) return;

        for (int i = 1; i <= TotalLevels; i++)
        {
            db.Insert(new LevelProgressData
            {
                LevelNumber   = i,
                IsUnlocked    = i == 1 ? 1 : 0,
                IsCompleted   = 0,
                CollectedBooksMask = 0,
                BooksCollected = 0,
                BooksRequired  = DefaultBooksRequired,
                Stars          = 0,
                BestTime       = 0f,
                TotalDeaths    = 0,
            });
        }
        LogShort($"CREATE default-levels (1–{TotalLevels})");
    }

    /// <summary>Ambil data progress satu level. Null jika belum ada.</summary>
    public LevelProgressData GetLevelProgress(int levelNumber)
    {
        EnsureDatabaseReady();
        if (db == null) return null;
        return db.Find<LevelProgressData>(levelNumber);
    }

    /// <summary>Ambil semua data progress (6 level).</summary>
    public List<LevelProgressData> GetAllLevelProgress()
    {
        EnsureDatabaseReady();
        if (db == null) return new List<LevelProgressData>();
        return db.Table<LevelProgressData>().ToList();
    }

    /// <summary>
    /// Simpan progress level ke database.
    /// Hanya update jika data baru lebih baik:
    /// – Stars tidak turun
    /// – BooksCollected tidak turun
    /// – BestTime diambil nilai terkecil (waktu lebih cepat)
    /// </summary>
    public void SaveLevelProgress(int levelNumber, int booksMask, int stars, float time, int sessionDeaths)
    {
        EnsureDatabaseReady();
        if (db == null) return;

        LevelProgressData data = db.Find<LevelProgressData>(levelNumber);
        if (data == null)
        {
            data = new LevelProgressData
            {
                LevelNumber   = levelNumber,
                BooksRequired = DefaultBooksRequired,
            };
        }

        // Gabungkan bitmask (ambil terbaik dari semua sesi)
        int bestMask = data.CollectedBooksMask | booksMask;
        int bestBooks = LevelProgressData.CountBits(bestMask);

        data.IsCompleted       = 1;
        data.CollectedBooksMask = bestMask;
        data.BooksCollected    = bestBooks;
        data.Stars             = Mathf.Max(data.Stars, stars);
        data.TotalDeaths       += sessionDeaths;

        // Waktu terbaik: simpan yang lebih kecil (lebih cepat), abaikan 0
        if (time > 0f)
            data.BestTime = data.BestTime <= 0f ? time : Mathf.Min(data.BestTime, time);

        db.InsertOrReplace(data);
        LogShort($"SAVE level={levelNumber} books={bestBooks} stars={data.Stars} time={data.BestTime:F1}s deaths={data.TotalDeaths}");
    }

    /// <summary>Buka level berikutnya di tabel LevelProgressData.</summary>
    public void UnlockLevelProgress(int levelNumber)
    {
        EnsureDatabaseReady();
        if (db == null) return;

        LevelProgressData data = db.Find<LevelProgressData>(levelNumber);
        if (data == null)
        {
            data = new LevelProgressData
            {
                LevelNumber   = levelNumber,
                BooksRequired = DefaultBooksRequired,
            };
        }

        if (data.IsUnlocked == 1) return; // Sudah terbuka
        data.IsUnlocked = 1;
        db.InsertOrReplace(data);
        LogShort($"UNLOCK level-progress level={levelNumber}");
    }

    /// <summary>Tambah kematian permanen di level tertentu.</summary>
    public void IncrementDeaths(int levelNumber)
    {
        EnsureDatabaseReady();
        if (db == null) return;

        LevelProgressData data = db.Find<LevelProgressData>(levelNumber);
        if (data == null) return;

        data.TotalDeaths++;
        db.Update(data);
        LogShort($"DEATH level={levelNumber} total={data.TotalDeaths}");
    }

    // =========================================================
    //  PLAYER DATA (existing)
    // =========================================================

    [ContextMenu("Reset Player Data")]
    public void ResetPlayerData()
    {
        EnsureDatabaseReady();

        if (db == null)
            return;

        PlayerData player = GetPlayerData();
        if (player == null)
            return;

        player.Coin = DefaultCoin;
        player.Level = DefaultLevel;
        player.HP = DefaultHp;
        player.Heart = DefaultHeart;

        db.Update(player);
        LogShort("RESET player-data");
    }

    /// <summary>Reset semua progress level ke kondisi awal (level 1 terbuka, lainnya terkunci).</summary>
    [ContextMenu("Reset Level Progress")]
    public void ResetLevelProgress()
    {
        EnsureDatabaseReady();
        if (db == null) return;

        db.DeleteAll<LevelProgressData>();
        InitializeDefaultLevels();
        LogShort("RESET level-progress");
    }

    // Public wrapper for Unity UI Button OnClick.
    public void ResetPlayerDataFromButton()
    {
        ResetPlayerData();
    }

    private void OnApplicationQuit()
    {
        if (db != null)
        {
            db.Close();
            LogShort("CLOSE connection");
        }
    }

    private void LogShort(string message)
    {
        if (!enableShortLogs)
            return;

        Debug.Log($"[DB] {message}");
    }
}
