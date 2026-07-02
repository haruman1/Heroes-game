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
