using UnityEngine;

/// <summary>
/// ScriptableObject yang menyimpan konten edukatif satu halaman buku.
/// 10 halaman dari level yang sama = 1 Buku utuh di Journey Book.
/// Buat instance baru via: klik kanan di Project → Heroes Game → Halaman Buku
/// </summary>
[CreateAssetMenu(fileName = "HalamanBuku_L1_H1", menuName = "Heroes Game/Halaman Buku")]
public class HalamanBukuSO : ScriptableObject
{
    [Header("Identitas Halaman")]
    [Tooltip("Nomor halaman dalam level ini (1–10). Harus unik per level.")]
    [Range(1, 10)]
    public int idHalaman = 1;

    [Tooltip("Nomor level tempat halaman ini berada.")]
    public int nomorLevel = 1;

    [Header("Konten Halaman")]
    [Tooltip("Judul halaman yang tampil saat dibuka.")]
    public string judulHalaman = "Judul Halaman";

    [Tooltip("Konten edukatif halaman ini (bisa panjang).")]
    [TextArea(5, 15)]
    public string kontenEdukasi = "Masukkan konten edukatif di sini.";

    [Tooltip("Gambar ilustrasi halaman. Opsional.")]
    public Sprite gambarHalaman;

    [Header("Informasi Buku Induk")]
    [Tooltip("Judul buku ketika semua 10 halaman di level ini terkumpul.")]
    public string judulBuku = "Judul Buku";

    [Tooltip("Gambar sampul buku yang tampil di Journey Book.")]
    public Sprite sampulBuku;

    [Tooltip("Deskripsi singkat buku yang tampil di Journey Book.")]
    [TextArea(2, 4)]
    public string deskrpsiBuku = "Deskripsi singkat buku ini.";
}
