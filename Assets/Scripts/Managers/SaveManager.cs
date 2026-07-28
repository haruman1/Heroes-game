using UnityEngine;

/// <summary>
/// SaveManager — Facade tipis di atas DatabaseManager.
/// Menyediakan API yang bersih untuk semua operasi simpan/muat data game.
/// 
/// Singleton DontDestroyOnLoad.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // Diganti dengan arsitektur Additive CoreScene
    }

    // ─── Helper Akses DB ─────────────────────────────────────────────
    private DatabaseManager DB => DatabaseManager.GetOrCreateInstance();

    // ─── Cek Save Data ───────────────────────────────────────────────

    /// <summary>
    /// True jika pemain sudah pernah memilih karakter dan memasukkan umur.
    /// Dipakai oleh MainMenuController untuk mengaktifkan tombol Continue.
    /// </summary>
    public bool AdaSaveData()
    {
        PlayerData data = DB?.GetPlayerData();
        return data != null
            && !string.IsNullOrEmpty(data.SelectedCharacter)
            && data.SelectedAge > 0;
    }

    // ─── Reset Save ──────────────────────────────────────────────────

    /// <summary>Reset semua data pemain dan progress level ke kondisi awal.</summary>
    public void ResetSave()
    {
        DB?.ResetPlayerData();
        DB?.ResetLevelProgress();
    }

    // ─── Player Data ─────────────────────────────────────────────────

    /// <summary>Muat data pemain dari database.</summary>
    public PlayerData MuatPlayerData() => DB?.GetPlayerData();

    /// <summary>
    /// Simpan pilihan gender dan karakter saat Character Select.
    /// gender: "Laki-laki" atau "Perempuan"
    /// namaKarakter: "Awan" atau "Rena"
    /// </summary>
    public void SimpanGenderDanUmur(string gender, string namaKarakter, int umur)
    {
        if (DB == null) return;

        // Simpan karakter dan umur via DatabaseManager yang sudah ada
        DB.SaveCharacterSelection(namaKarakter, umur);

        // Simpan gender ke field baru
        PlayerData data = DB.GetPlayerData();
        if (data == null) return;

        data.Gender = gender;
        DB.Connection?.Update(data);

        Debug.Log($"[SaveManager] SIMPAN gender={gender}, karakter={namaKarakter}, umur={umur}");
    }

    // ─── Scene Terakhir (untuk Continue) ─────────────────────────────

    /// <summary>Simpan nama scene terakhir yang dimainkan (untuk fitur Continue).</summary>
    public void SimpanNamaSceneTerakhir(string namaScene)
    {
        if (DB == null || string.IsNullOrEmpty(namaScene)) return;

        PlayerData data = DB.GetPlayerData();
        if (data == null) return;

        data.NamaSceneTerakhir = namaScene;
        DB.Connection?.Update(data);

        Debug.Log($"[SaveManager] SIMPAN scene terakhir = {namaScene}");
    }

    /// <summary>Muat nama scene terakhir. Kosong jika belum pernah ada.</summary>
    public string GetNamaSceneTerakhir()
    {
        return DB?.GetPlayerData()?.NamaSceneTerakhir ?? "";
    }

    // ─── Progress Halaman Buku ────────────────────────────────────────

    /// <summary>
    /// Simpan halaman buku yang baru dikumpulkan ke bitmask.
    /// nomorLevel: 1, 2, 3, dst
    /// nomorHalaman: 1–10
    /// </summary>
    public void SimpanProgressHalaman(int nomorLevel, int nomorHalaman)
    {
        if (DB == null) return;

        LevelProgressData progress = DB.GetLevelProgress(nomorLevel);
        if (progress == null) return;

        if (nomorHalaman >= 1 && nomorHalaman <= 32)
        {
            progress.CollectedBooksMask |= (1 << (nomorHalaman - 1));
            progress.BooksCollected     = LevelProgressData.CountBits(progress.CollectedBooksMask);
            DB.Connection?.Update(progress);
        }
    }

    // ─── Koin & Mata Uang ─────────────────────────────────────────────

    /// <summary>Tambah koin ke dompet pemain.</summary>
    public void TambahCoin(int jumlah)
    {
        if (DB == null) return;

        PlayerData data = DB.GetPlayerData();
        if (data == null) return;

        data.Coin = Mathf.Max(0, data.Coin + jumlah);
        DB.Connection?.Update(data);

        Debug.Log($"[SaveManager] Koin +{jumlah} → Total Koin: {data.Coin}");
    }

    /// <summary>Kurangi koin dari dompet pemain. Mengembalikan true jika koin cukup.</summary>
    public bool KurangiCoin(int jumlah)
    {
        if (DB == null) return false;

        PlayerData data = DB.GetPlayerData();
        if (data == null || data.Coin < jumlah) return false;

        data.Coin -= jumlah;
        DB.Connection?.Update(data);

        Debug.Log($"[SaveManager] Koin -{jumlah} → Sisa Koin: {data.Coin}");
        return true;
    }

    /// <summary>Muat jumlah koin yang dimiliki pemain saat ini.</summary>
    public int MuatCoin() => DB?.GetPlayerData()?.Coin ?? 0;
   // ─── Health ─────────────────────────────────────────────────────
   
    // ─── Booster ─────────────────────────────────────────────────────

    /// <summary>Tambah booster ke inventory pemain.</summary>
    public void SimpanBooster(int jumlah)
    {
        if (DB == null) return;

        PlayerData data = DB.GetPlayerData();
        if (data == null) return;

        data.JumlahBooster = Mathf.Max(0, data.JumlahBooster + jumlah);
        DB.Connection?.Update(data);

        Debug.Log($"[SaveManager] Booster +{jumlah} → total {data.JumlahBooster}");
    }

    /// <summary>Kurangi booster (saat digunakan di boss fight).</summary>
    public void GunakanBooster(int jumlah)
    {
        if (DB == null) return;

        PlayerData data = DB.GetPlayerData();
        if (data == null) return;

        data.JumlahBooster = Mathf.Max(0, data.JumlahBooster - jumlah);
        DB.Connection?.Update(data);

        Debug.Log($"[SaveManager] Booster -{jumlah} → sisa {data.JumlahBooster}");
    }

    /// <summary>Muat jumlah booster yang dimiliki pemain.</summary>
    public int MuatJumlahBooster() => DB?.GetPlayerData()?.JumlahBooster ?? 0;

    // ─── Settings ────────────────────────────────────────────────────

    /// <summary>Simpan settings game (volume, fullscreen, dll).</summary>
    public void SimpanSettings(GameSettingsData settings)
    {
        DB?.SaveSettings(settings);
    }

    /// <summary>Muat settings game.</summary>
    public GameSettingsData MuatSettings() => DB?.GetSettingsData();
}
