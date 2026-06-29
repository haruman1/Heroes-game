using System.IO;
using SQLite;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance;
    private SQLiteConnection db;
    public SQLiteConnection Connection => db;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDatabase()
    {
        string dbPath = Path.Combine(Application.persistentDataPath, "game_database.db");
        Debug.Log($"Database path: {dbPath}");
        db = new SQLiteConnection(dbPath);
        Debug.Log($"Database connection established at: {dbPath}");
        db.CreateTable<PlayerData>();
        CreateDefaultPlayer();
    }

    private void CreateDefaultPlayer()
    {
        if (db.Table<PlayerData>().Count() == 0)
        {
            db.Insert(
                new PlayerData
                {
                    Id = 1,
                    Coin = 0,
                    Level = 1,
                    HP = 100,
                    Heart = 3,
                }
            );

            Debug.Log("Player default berhasil dibuat.");
        }
    }

    private void OnApplicationQuit()
    {
        if (db != null)
        {
            db.Close();
            Debug.Log("Database connection closed.");
        }
    }
}
