using UnityEngine;

/// <summary>
/// ScriptableObject konfigurasi per-level.
/// Setiap level memiliki satu instance SO ini yang di-assign ke LevelManager.
/// Buat instance baru via: klik kanan di Project → Heroes Game → Level Data
/// </summary>
[CreateAssetMenu(fileName = "LevelData_Level1", menuName = "Heroes Game/Level Data")]
public class LevelDataSO : ScriptableObject
{
    [Header("Identitas Level")]
    [Tooltip("Nomor urut level (1, 2, 3, dst).")]
    public int nomorLevel = 1;

    [Tooltip("Nama scene Unity yang persis (harus sama dengan nama file .unity).")]
    public string namaScene = "LEVEL 1";

    [Header("Dialog Level")]
    [Tooltip("Dialog yang tampil di awal level sebelum gameplay dimulai. Null = langsung mulai.")]
    public DialogueDataSO dialogIntro;

    [Tooltip("Dialog yang tampil setelah player menyentuh Flag / finish point. Null = langsung muat level berikutnya.")]
    public DialogueDataSO dialogOutro;

    [Header("Kolektibel")]
    [Tooltip("Jumlah halaman buku yang harus dikumpulkan di level ini.")]
    [Range(1, 20)]
    public int jumlahHalamanDibutuhkan = 10;

    [Header("Navigasi Level")]
    [Tooltip("Data level berikutnya. Kosongkan (null) jika ini adalah level terakhir → kembali ke Main Menu.")]
    public LevelDataSO levelBerikutnya;

    [Header("Audio Level")]
    [Tooltip("Background music yang diputar selama gameplay level ini.")]
    public AudioClip bgmLevel;
}
