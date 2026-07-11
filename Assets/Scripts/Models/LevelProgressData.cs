using SQLite;

/// <summary>
/// Model tabel SQLite untuk menyimpan progress setiap level (1–6).
/// Satu baris per level, disimpan permanen di database.
/// </summary>
public class LevelProgressData
{
    [PrimaryKey]
    public int LevelNumber { get; set; }

    /// <summary>1 = level bisa dimainkan, 0 = terkunci</summary>
    public int IsUnlocked { get; set; }

    /// <summary>1 = sudah pernah diselesaikan, 0 = belum</summary>
    public int IsCompleted { get; set; }

    /// <summary>
    /// Bitmask buku yang pernah dikumpulkan (lintas semua sesi, selalu ambil terbaik).
    /// Bit 0 = Buku #01, Bit 1 = Buku #02, ..., Bit 9 = Buku #10.
    /// </summary>
    public int CollectedBooksMask { get; set; }

    /// <summary>Jumlah buku terbaik yang pernah dikumpulkan (cached dari mask)</summary>
    public int BooksCollected { get; set; }

    /// <summary>Target buku di level ini (default 10)</summary>
    public int BooksRequired { get; set; }

    /// <summary>Bintang terbaik yang pernah diraih (0–3)</summary>
    public int Stars { get; set; }

    /// <summary>Waktu terbaik untuk menyelesaikan level (dalam detik; 0 = belum selesai)</summary>
    public float BestTime { get; set; }

    /// <summary>Total kematian di level ini (akumulasi semua sesi)</summary>
    public int TotalDeaths { get; set; }

    // ---- Helper properties (tidak disimpan ke DB) ----

    [Ignore]
    public bool UnlockedBool
    {
        get => IsUnlocked == 1;
        set => IsUnlocked = value ? 1 : 0;
    }

    [Ignore]
    public bool CompletedBool
    {
        get => IsCompleted == 1;
        set => IsCompleted = value ? 1 : 0;
    }

    /// <summary>Cek apakah buku nomor tertentu sudah pernah dikumpulkan (1-indexed).</summary>
    public bool IsBookCollected(int bookNumber)
    {
        if (bookNumber < 1 || bookNumber > 32) return false;
        return (CollectedBooksMask & (1 << (bookNumber - 1))) != 0;
    }

    /// <summary>Hitung jumlah bit yang aktif (jumlah buku dikumpulkan).</summary>
    public static int CountBits(int mask)
    {
        int count = 0;
        int n = mask;
        while (n != 0)
        {
            count += n & 1;
            n >>= 1;
        }
        return count;
    }
}
