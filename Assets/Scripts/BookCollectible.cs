using UnityEngine;

public class BookCollectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    public int bookValue = 1;

    [Header("Book Identity")]
    [Tooltip("Nomor urut buku di level ini (1–10). Harus unik per level!")]
    [Range(1, 10)]
    public int bookNumber = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerJ player = collision.GetComponent<playerJ>();
        if (player == null) return;

        // Tambahkan ke hitungan buku/halaman
        player.AddBook(bookValue);

        // Tandai buku/halaman nomor ini sudah dikumpulkan di sesi ini
        player.MarkBookCollected(bookNumber);

        // Efek suara pengambilan (legacy)
        if (player.coinSound != null)
            player.PlaySFX(player.coinSound, 0.8f);

        Debug.Log($"Pemain mengambil Halaman #{bookNumber:D2}. Total halaman sekarang: {player.bookCount}");

        // Tampilkan halaman di panel pembaca halaman in-game
        InGamePageReader reader = InGamePageReader.Instance;
        if (reader == null) 
            reader = FindFirstObjectByType<InGamePageReader>();
            
        if (reader != null)
        {
            reader.ShowPage(bookNumber, player);
        }

        Destroy(gameObject);
    }

}
