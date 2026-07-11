using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Controller utama layar "Total Perjalanan Mu".
/// Attach ke root GameObject scene Journey.
///
/// Tanggungjawab:
///   1. Membaca semua LevelProgressData dari database
///   2. Mengisi 6 LevelCard dengan data yang benar
///   3. Menghitung dan menampilkan statistik total di header
///   4. Mengelola buka/tutup BookCollectionPanel
/// </summary>
public class JourneyManager : MonoBehaviour
{
    // ----------------------------------------------------------------
    //  Level Cards
    // ----------------------------------------------------------------

    [Header("Level Cards — Assign 6 LevelCard, urut Level 1 s.d. 6")]
    public LevelCard[] levelCards = new LevelCard[6];

    // ----------------------------------------------------------------
    //  Header Stats UI
    // ----------------------------------------------------------------

    [Header("Header Stats")]
    [Tooltip("Teks progress keseluruhan, misal: 0 / 60 Buku")]
    public TMP_Text overallProgressText;

    [Tooltip("Label 'Level Selesai', misal: 0 / 6")]
    public TMP_Text levelsCompletedText;

    [Tooltip("Label 'Total Waktu', misal: 00:00")]
    public TMP_Text totalTimeText;

    [Tooltip("Label 'Kematian', misal: 0")]
    public TMP_Text totalDeathsText;

    [Tooltip("Label 'Koleksi Buku', misal: 0 / 60")]
    public TMP_Text totalCollectionText;

    // ----------------------------------------------------------------
    //  Panel Koleksi
    // ----------------------------------------------------------------

    [Header("Panel Koleksi")]
    public BookCollectionPanel bookCollectionPanel;

    // ================================================================
    //  Unity Lifecycle
    // ================================================================

    private void OnEnable()
    {
        // Refresh setiap kali layar ini diaktifkan
        RefreshUI();
    }

    // ================================================================
    //  Public API
    // ================================================================

    /// <summary>
    /// Refresh semua kartu dan statistik header dari database.
    /// Bisa dipanggil ulang setelah kembali dari sebuah level.
    /// </summary>
    public void RefreshUI()
    {
        DatabaseManager db = DatabaseManager.GetOrCreateInstance();

        // Bangun dictionary untuk lookup cepat
        List<LevelProgressData> allProgress = db.GetAllLevelProgress();
        var progressMap = new Dictionary<int, LevelProgressData>();
        foreach (var p in allProgress)
            progressMap[p.LevelNumber] = p;

        int totalBooks      = 0;
        int maxBooks        = 0;
        int levelsCompleted = 0;
        float totalTime     = 0f;
        int totalDeaths     = 0;

        foreach (LevelCard card in levelCards)
        {
            if (card == null) continue;

            // Ambil data dari DB, atau buat default jika belum ada
            LevelProgressData data = progressMap.ContainsKey(card.LevelNumber)
                ? progressMap[card.LevelNumber]
                : new LevelProgressData
                  {
                      LevelNumber   = card.LevelNumber,
                      IsUnlocked    = card.LevelNumber == 1 ? 1 : 0,
                      BooksRequired = 10
                  };

            card.Setup(data, this);

            totalBooks      += data.BooksCollected;
            maxBooks        += (data.BooksRequired > 0 ? data.BooksRequired : 10);
            if (data.CompletedBool) levelsCompleted++;
            if (data.CompletedBool) totalTime += data.BestTime;
            totalDeaths     += data.TotalDeaths;
        }

        // Update header
        if (overallProgressText  != null)
            overallProgressText.text  = $"{totalBooks} / {maxBooks} Buku";
        if (levelsCompletedText  != null)
            levelsCompletedText.text  = $"{levelsCompleted} / {levelCards.Length}";
        if (totalTimeText        != null)
            totalTimeText.text        = FormatTime(totalTime);
        if (totalDeathsText      != null)
            totalDeathsText.text      = totalDeaths.ToString();
        if (totalCollectionText  != null)
            totalCollectionText.text  = $"{totalBooks} / {maxBooks}";
    }

    /// <summary>
    /// Buka BookCollectionPanel untuk level tertentu.
    /// Dipanggil oleh LevelCard.OnCollectButtonClicked.
    /// </summary>
    public void OpenBookCollection(LevelCard card)
    {
        if (bookCollectionPanel == null || card == null) return;

        DatabaseManager db   = DatabaseManager.GetOrCreateInstance();
        LevelProgressData data = db.GetLevelProgress(card.LevelNumber)
            ?? new LevelProgressData { LevelNumber = card.LevelNumber, BooksRequired = 10 };

        bookCollectionPanel.Show(
            data,
            card.GetBookDataArray(),
            card.LevelNumber,
            card.GetLevelName()
        );
    }

    // ================================================================
    //  Helper
    // ================================================================

    private string FormatTime(float seconds)
    {
        if (seconds <= 0f) return "00:00";
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }
}
