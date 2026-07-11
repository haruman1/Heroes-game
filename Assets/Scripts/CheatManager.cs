using UnityEngine;

public class CheatManager : MonoBehaviour
{
    [Header("Informasi Player Saat Ini")]
    [Tooltip("Level tertinggi yang terbuka saat ini. Tekan tombol Refresh untuk memperbarui.")]
    public int currentUnlockedLevel = 1;
    
    [Tooltip("Jumlah koin saat ini.")]
    public int currentCoins = 0;

    [Header("Pengaturan Cheat")]
    [Tooltip("Masukkan level yang ingin dibuka (1-6) lalu klik kanan pada komponen ini -> 'Unlock Level'")]
    [Range(1, 6)]
    public int levelToUnlock = 1;
    
    [Tooltip("Masukkan jumlah koin yang ingin ditambahkan lalu klik kanan pada komponen ini -> 'Add Coins'")]
    public int coinsToAdd = 100;

    private DatabaseManager db;

    void Start()
    {
        db = DatabaseManager.GetOrCreateInstance();
        RefreshData();
    }

    [ContextMenu("Refresh Data (Lihat Status Saat Ini)")]
    public void RefreshData()
    {
        if (db == null) db = DatabaseManager.GetOrCreateInstance();
        
        PlayerData player = db.GetPlayerData();
        if (player != null)
        {
            currentUnlockedLevel = player.Level;
            currentCoins = player.Coin;
            Debug.Log($"[CheatManager] Status saat ini - Level Terbuka: {currentUnlockedLevel}, Koin: {currentCoins}");
        }
    }

    [ContextMenu("Unlock Level (Buka Level Tujuan)")]
    public void CheatUnlockLevel()
    {
        if (db == null) db = DatabaseManager.GetOrCreateInstance();
        
        // Membuka level di PlayerData (progress global)
        db.UnlockLevel(levelToUnlock);
        
        // Membuka progress spesifik untuk level tersebut
        db.UnlockLevelProgress(levelToUnlock);
        
        Debug.Log($"[CheatManager] Berhasil membuka level {levelToUnlock}!");
        RefreshData();
    }

    [ContextMenu("Add Coins (Tambah Koin)")]
    public void CheatAddCoins()
    {
        if (db == null) db = DatabaseManager.GetOrCreateInstance();
        
        db.AddCoin(coinsToAdd);
        Debug.Log($"[CheatManager] Berhasil menambahkan {coinsToAdd} koin!");
        RefreshData();
    }

    [ContextMenu("Reset Semua Progress (Kembali ke Awal)")]
    public void CheatResetProgress()
    {
        if (db == null) db = DatabaseManager.GetOrCreateInstance();
        
        db.ResetPlayerData();
        db.ResetLevelProgress();
        Debug.Log("[CheatManager] SEMUA PROGRESS TELAH DIRESET!");
        RefreshData();
    }
}
