using UnityEngine;
using UnityEngine.SceneManagement;

public class Flag : MonoBehaviour
{
    public GameObject WinFlag;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerJ player = collision.GetComponent<playerJ>();
            if (player != null)
            {
                if (player.bookCount < player.booksRequiredPerLevel)
                {
                    Debug.LogWarning($"Belum cukup buku! Kumpulkan {player.booksRequiredPerLevel} buku. Baru terkumpul {player.bookCount}.");
                    return; // Jangan selesaikan level jika buku belum cukup
                }
            }

            DatabaseManager dbManager = DatabaseManager.GetOrCreateInstance();
            if (dbManager != null)
            {
                int nextLevel = SceneManager.GetActiveScene().buildIndex + 2;
                dbManager.UnlockLevel(nextLevel);
            }

            Time.timeScale = 0f; // Pause the game
            WinFlag.SetActive(true);
        }
    }
}
