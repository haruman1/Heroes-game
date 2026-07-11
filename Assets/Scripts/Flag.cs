using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class Flag : MonoBehaviour
{
    public GameObject WinFlag;
    public FinishLineDialogueManager finishLineDialogue;
    public UnityEvent onFinishLevel;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerJ player = collision.GetComponent<playerJ>();
        if (player == null) return;

        // ---- Kumpulkan data sesi ----
        int levelNumber = player.GetCurrentLevelNumber();
        int booksCollected = player.bookCount;
        int booksRequired  = player.booksRequiredPerLevel;
        float sessionTime  = player.GetSessionTime();
        int sessionDeaths  = player.GetSessionDeaths();

        // Bangun bitmask dari buku yang dikumpulkan sesi ini
        int booksMask = 0;
        foreach (int num in player.CollectedBookNumbers)
        {
            if (num >= 1 && num <= 32)
                booksMask |= (1 << (num - 1));
        }

        // ---- Hitung bintang ----
        // ⭐   = selesai level (buku boleh tidak lengkap)
        // ⭐⭐  = kumpul buku tapi kurang ≤ 2 (8 atau 9 dari 10)
        // ⭐⭐⭐ = kumpul semua buku (10/10)
        int stars = CalculateStars(booksCollected, booksRequired);

        // ---- Simpan ke database ----
        DatabaseManager dbManager = DatabaseManager.GetOrCreateInstance();
        if (dbManager != null)
        {
            dbManager.SaveLevelProgress(levelNumber, booksMask, stars, sessionTime, sessionDeaths);

            // Buka level berikutnya
            int nextLevelNumber = levelNumber + 1;
            dbManager.UnlockLevelProgress(nextLevelNumber);

            // Tetap update PlayerData.Level (kompatibilitas sistem lama)
            int nextBuildIdx = SceneManager.GetActiveScene().buildIndex + 2;
            dbManager.UnlockLevel(nextBuildIdx);
        }

        Debug.Log($"[Flag] Level {levelNumber} selesai! Buku:{booksCollected}/{booksRequired} " +
                  $"Bintang:{stars} Waktu:{sessionTime:F1}s Kematian:{sessionDeaths}");

        // ---- Tampilkan dialog atau layar menang ----
        if (finishLineDialogue != null)
        {
            finishLineDialogue.TriggerFinishLineDialogue();
        }
        else
        {
            Time.timeScale = 0f;
            if (WinFlag != null) WinFlag.SetActive(true);
        }

        onFinishLevel?.Invoke();
    }

    /// <summary>
    /// Hitung bintang berdasarkan jumlah buku yang dikumpulkan.
    /// ⭐   = selesai (kurang lebih dari 2 buku)
    /// ⭐⭐  = kurang maksimal 2 buku (8 atau 9 dari 10)
    /// ⭐⭐⭐ = semua buku terkumpul (10/10)
    /// </summary>
    private int CalculateStars(int booksCollected, int booksRequired)
    {
        if (booksCollected >= booksRequired)
            return 3;
        if (booksCollected >= booksRequired - 2)
            return 2;
        return 1;
    }
}
