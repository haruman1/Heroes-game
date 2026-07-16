/// <summary>
/// Static bridge untuk meneruskan data antara scene level dan scene arena.
/// Data di sini bertahan selama aplikasi berjalan (tidak di-reset antar scene).
/// </summary>
public static class BossArenaBridge
{
    public enum Result { None, Won, Lost }

    // ---- Data dikirim dari level ke arena ----

    /// <summary>Nama scene level yang harus dikembalikan setelah arena selesai.</summary>
    public static string ReturnSceneName = "";

    /// <summary>Tipe arena yang dibuka: "BossMode" atau "KerocoMode".</summary>
    public static string ArenaSceneName  = "";

    /// <summary>Nomor level yang membuka arena ini (untuk menyimpan progress).</summary>
    public static int    SourceLevelNumber = 1;

    // ---- Data dikembalikan dari arena ke level ----

    /// <summary>Hasil arena setelah kembali ke scene level.</summary>
    public static Result ArenaResult = Result.None;

    /// <summary>Tipe arena yang baru saja diselesaikan.</summary>
    public static string CompletedArenaType = "";

    /// <summary>Reset data hasil (panggil setelah dibaca di scene level).</summary>
    public static void ClearResult()
    {
        ArenaResult = Result.None;
        CompletedArenaType = "";
    }
}
