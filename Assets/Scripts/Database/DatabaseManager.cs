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
        CreateDefaultPlayer();
        CreateDefaultSettings();
        LogShort($"INIT path={dbPath}");
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
                }
            );

            LogShort("CREATE default-player");
        }
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
