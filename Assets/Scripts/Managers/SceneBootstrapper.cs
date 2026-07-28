using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// SceneBootstrapper — Pembantu saat testing di Editor.
///
/// ═══════════════════════════════════════════════════════════════
/// MASALAH YANG DIPECAHKAN:
/// ═══════════════════════════════════════════════════════════════
/// Saat Anda menekan tombol Play langsung dari scene "LEVEL 1" (atau scene
/// mana pun selain CoreScene), GameManager.Instance akan null karena
/// CoreScene belum dimuat. Ini menyebabkan NullReferenceException di mana-mana.
///
/// SOLUSI:
/// Pasang script ini di sembarang GameObject di setiap scene gameplay
/// (Main Menu, Level 1, Level 2, Boss Arena, dll).
/// Saat game dijalankan dari scene tersebut, Bootstrapper otomatis memuat
/// CoreScene secara Additive agar seluruh Manager tersedia.
///
/// ═══════════════════════════════════════════════════════════════
/// CARA PAKAI DI EDITOR:
/// ═══════════════════════════════════════════════════════════════
/// 1. Buat GameObject kosong di scene (misal beri nama "_Bootstrapper")
/// 2. Pasang script SceneBootstrapper.cs
/// 3. Di Inspector, pastikan coreSceneName = "CoreScene"
/// 4. Selesai! Tekan Play dari mana saja tanpa error.
/// </summary>
public class SceneBootstrapper : MonoBehaviour
{
    [Tooltip("Nama scene yang berisi semua Manager Global (GameManager, AudioManager, dll). Default: CoreScene")]
    [SerializeField] private string coreSceneName = "CoreScene";

    private void Awake()
    {
        // Cek apakah GameManager sudah ada (artinya CoreScene sudah dimuat)
        if (GameManager.Instance != null) return;

        Debug.Log($"[Bootstrapper] GameManager tidak ditemukan. Memuat {coreSceneName} secara Additive...");

        // Cek antisipasi double-load
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == coreSceneName)
            {
                Debug.Log($"[Bootstrapper] {coreSceneName} sudah ada di scene stack, skip.");
                return;
            }
        }

        // Muat CoreScene secara sinkron (blocking) di Awake agar Manager
        // sudah siap digunakan oleh script lain di frame yang sama.
        SceneManager.LoadScene(coreSceneName, LoadSceneMode.Additive);
        Debug.Log($"[Bootstrapper] {coreSceneName} berhasil dimuat.");
    }
}
