using UnityEngine;

/// <summary>
/// Data konten satu buku: judul dan deskripsi.
/// Dikonfigurasi di Inspector pada komponen LevelCard.
/// Gambar buku ditentukan secara global (satu Sprite untuk semua buku).
/// </summary>
[System.Serializable]
public class BookData
{
    [Tooltip("Judul buku yang muncul di panel detail")]
    public string bookTitle = "Judul Buku";

    [TextArea(3, 6)]
    [Tooltip("Isi / deskripsi buku")]
    public string bookDescription = "Deskripsi isi buku ini...";

    [Tooltip("Suara narasi tambahan untuk halaman ini")]
    public AudioClip narrationAudio;
}

