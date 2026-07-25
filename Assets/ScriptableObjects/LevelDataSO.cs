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

    [Header("Dialog / Light Novel Intro Level")]
    [Tooltip("Dialog/Monolog intro khusus untuk Awan (Pria). Jika diisi, akan mengutamakan dialog ini saat player memilih Awan.")]
    public DialogueDataSO dialogIntroAwan;

    [Tooltip("Dialog/Monolog intro khusus untuk Rena (Wanita). Jika diisi, akan mengutamakan dialog ini saat player memilih Rena.")]
    public DialogueDataSO dialogIntroRena;

    [Tooltip("Dialog/Monolog intro umum (fallback). Dipakai jika dialog khusus Awan/Rena kosong.")]
    public DialogueDataSO dialogIntro;

    [Header("Dialog / Light Novel Outro Level")]
    [Tooltip("Dialog outro khusus Awan.")]
    public DialogueDataSO dialogOutroAwan;

    [Tooltip("Dialog outro khusus Rena.")]
    public DialogueDataSO dialogOutroRena;

    [Tooltip("Dialog outro umum (fallback).")]
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
