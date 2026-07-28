using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    // Fungsi ini dipanggil dari Tombol "New Game"
    public void OnKlikNewGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.MulaiGameBaru();
        }
        else
        {
            Debug.LogError("GameManager belum dimuat! Pastikan play dari CoreScene atau Bootstrapper aktif.");
        }
    }

    // Fungsi ini dipanggil dari Tombol "Continue"
    public void OnKlikContinue()
    {
       
            GameManager.Instance?.Lanjutkan();
       
    }

    // Fungsi ini dipanggil dari Tombol "Settings"
    public void OnKlikSettings()
    {
        
            UIManager.Instance?.TampilkanSettings(true);
       
    }

    // Fungsi ini dipanggil dari Tombol "Keluar"
    public void OnKlikKeluar()
    {
        GameManager.Instance?.KeluarGame();
        
    }

    // Fungsi untuk memunculkan Shop UI (bisa instantiate dari Prefab)
    public void OnKlikShop()
    {
        Debug.Log("Shop belum diimplementasikan sepenuhnya!");
        // Instantiate prefab Shop di bawah Canvas ini
    }
}
