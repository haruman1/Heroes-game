using SQLite;

public class PlayerData
{
    [PrimaryKey]
    public int Id { get; set; }

    // === Data Lama (dipertahankan) ===
    public int Coin { get; set; }
    public int Level { get; set; }
    public int HP { get; set; }
    public int Heart { get; set; }
    public string SelectedCharacter { get; set; }
    public int SelectedAge { get; set; }

    // === Data Baru ===

    /// <summary>
    /// Gender pemain: "Laki-laki" atau "Perempuan"
    /// </summary>
    public string Gender { get; set; }

    /// <summary>
    /// Jumlah Booster yang dikumpulkan dari seluruh level (dipakai saat boss fight).
    /// </summary>
    public int JumlahBooster { get; set; }

    /// <summary>
    /// Nama scene terakhir yang dimainkan — dipakai fitur Continue.
    /// </summary>
    public string NamaSceneTerakhir { get; set; }
}
