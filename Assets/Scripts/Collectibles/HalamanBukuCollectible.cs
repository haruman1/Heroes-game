using UnityEngine;

/// <summary>
/// HalamanBukuCollectible — Collectible halaman buku edukatif (10 per level).
/// 
/// Menggantikan Coins_duit.cs dan BookCollectible.cs lama.
/// Setiap 10 halaman yang terkumpul di satu level = 1 Buku utuh di Journey Book.
/// 
/// Cara setup:
/// 1. Taruh komponen ini pada GameObject collectible di scene level.
/// 2. Set nomorHalaman (1–10, unik per level).
/// 3. Assign dataHalaman (HalamanBukuSO) jika ingin tampilkan konten edukatif.
/// </summary>
public class HalamanBukuCollectible : MonoBehaviour
{
    [Header("Identitas Halaman")]
    [Tooltip("Nomor urut halaman di level ini (1–10). HARUS unik per level!")]
    [Range(1, 10)]
    public int nomorHalaman = 1;

    [Header("Nilai")]
    [Tooltip("Berapa poin yang ditambahkan ke counter halaman player.")]
    public int nilaiHalaman = 1;

    [Header("Data Konten Edukatif")]
    [Tooltip("SO yang berisi konten edukatif halaman ini. Opsional — jika kosong halaman tidak ditampilkan.")]
    public HalamanBukuSO dataHalaman;

    [Header("Efek Audio")]
    [Tooltip("SFX saat halaman diambil. Jika kosong akan pakai suara koin player.")]
    public AudioClip sfxAmbil;

    [Header("Efek Visual")]
    [Tooltip("Prefab efek partikel saat diambil. Opsional.")]
    public GameObject efekAmbil;

    // ─── Trigger ─────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerJ player = collision.GetComponent<playerJ>();
        if (player == null) return;

        // Tambah hitungan halaman ke player
        player.AddBook(nilaiHalaman);
        player.MarkBookCollected(nomorHalaman);

        // SFX pengambilan
        if (sfxAmbil != null)
            player.PlaySFX(sfxAmbil, 0.8f);
        else if (player.coinSound != null)
            player.PlaySFX(player.coinSound, 0.8f);

        // Efek visual
        if (efekAmbil != null)
            Instantiate(efekAmbil, transform.position, Quaternion.identity);

        // Laporkan ke LevelManager → update Knowledge Bar
        LevelManager.Instance?.LaporHalamanDikumpulkan(nomorHalaman);

        // Tampilkan konten edukatif (InGamePageReader)
        InGamePageReader reader = InGamePageReader.Instance;
        if (reader == null) reader = FindFirstObjectByType<InGamePageReader>();
        if (reader != null) reader.ShowPage(nomorHalaman, player);

        Debug.Log($"[HalamanBuku] Halaman #{nomorHalaman:D2} diambil. Total: {player.bookCount}");

        Destroy(gameObject);
    }
}
