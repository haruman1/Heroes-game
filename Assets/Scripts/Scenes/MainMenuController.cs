using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// MainMenuController — Controller untuk Scene Main Menu.
/// 
/// Tanggung jawab:
/// - Aktifkan/nonaktifkan tombol Continue berdasarkan save data
/// - Hubungkan tombol ke GameManager dan UIManager
/// - Akses Journey Book dari Main Menu
/// 
/// Cara setup di Unity:
/// 1. Buat Canvas di scene MainMenu.
/// 2. Buat tombol: NewGame, Continue, Settings, JourneyBook, Exit.
/// 3. Assign semua referensi ke Inspector.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Tombol Utama")]
    [SerializeField] private Button tombolGameBaru;
    [SerializeField] private Button tombolLanjutkan;
    [SerializeField] private Button tombolSettings;
    [SerializeField] private Button tombolJourneyBook;
    [SerializeField] private Button tombolKeluar;

    [Header("Feedback Visual")]
    [Tooltip("Teks/ikon yang tampil di atas tombol Continue saat tidak ada save.")]
    [SerializeField] private GameObject infoTidakAdaSave;
    [Tooltip("Versi game (contoh: v1.0.0). Opsional.")]
    [SerializeField] private TMP_Text teksVersiGame;
    [SerializeField] private string versiGame = "v1.0";

    // ─── Lifecycle ───────────────────────────────────────────────────
    private void Start()
    {
        // Set versi game
        if (teksVersiGame != null) teksVersiGame.text = versiGame;

        // Cek apakah ada save data
        bool adaSave = SaveManager.Instance?.AdaSaveData() ?? false;

        // Aktifkan/nonaktifkan tombol Continue
        if (tombolLanjutkan != null)
            tombolLanjutkan.interactable = adaSave;

        if (infoTidakAdaSave != null)
            infoTidakAdaSave.SetActive(!adaSave);

        // Pasang listener tombol
        tombolGameBaru  ?.onClick.AddListener(OnKlikGameBaru);
        tombolLanjutkan ?.onClick.AddListener(OnKlikLanjutkan);
        tombolSettings  ?.onClick.AddListener(OnKlikSettings);
        tombolJourneyBook?.onClick.AddListener(OnKlikJourneyBook);
        tombolKeluar    ?.onClick.AddListener(OnKlikKeluar);

        // Pastikan HUD tersembunyi di Main Menu
        UIManager.Instance?.TampilkanHUD(false);
    }

    // ─── Handler Tombol ──────────────────────────────────────────────
    private void OnKlikGameBaru()
    {
        GameManager.Instance?.MulaiGameBaru();
    }

    private void OnKlikLanjutkan()
    {
        GameManager.Instance?.Lanjutkan();
    }

    private void OnKlikSettings()
    {
        UIManager.Instance?.TampilkanSettings(true);
    }

    private void OnKlikJourneyBook()
    {
        JourneyBookManager.Instance?.TampilkanJourneyBook();
    }

    private void OnKlikKeluar()
    {
        GameManager.Instance?.KeluarGame();
    }
}
