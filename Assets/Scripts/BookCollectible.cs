using UnityEngine;

public class BookCollectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    public int bookValue = 1; // Jumlah buku yang didapatkan

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Pastikan objek yang menyentuh memiliki tag "Player"
        if (collision.CompareTag("Player"))
        {
            // Ambil script playerJ dari player
            playerJ player = collision.GetComponent<playerJ>();
            
            if (player != null)
            {
                // Tambahkan buku
                player.AddBook(bookValue);
                
                // Mainkan efek suara (menggunakan suara koin yang sudah ada di playerJ)
                if (player.coinSound != null)
                {
                    player.PlaySFX(player.coinSound, 0.8f);
                }
                
                Debug.Log("Pemain mengambil buku. Total buku sekarang: " + player.bookCount);
                
                // Hancurkan objek buku dari scene setelah diambil
                Destroy(gameObject);
            }
        }
    }
}
