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

        // Tambahkan ke hitungan buku
        player.AddBook(bookValue);

        // Tandai buku nomor ini sudah dikumpulkan di sesi ini
        player.MarkBookCollected(bookNumber);

        // Efek suara
        if (player.coinSound != null)
            player.PlaySFX(player.coinSound, 0.8f);

        Debug.Log($"Pemain mengambil Buku #{bookNumber:D2}. Total buku sekarang: {player.bookCount}");

        Destroy(gameObject);
    }
}
