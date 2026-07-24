using System.Collections;
using UnityEngine;

/// <summary>
/// OpeningStoryController — Controller untuk Scene Opening Story.
/// 
/// Menjalankan rangkaian 4 video sinematik via VideoManager.
/// Setelah semua video selesai, muat LEVEL 1.
/// 
/// PENTING:
/// - Scene ini hanya diputar SEKALI saat New Game.
/// - Continue langsung ke level terakhir, tidak melalui scene ini.
/// - Tidak ada player yang di-spawn di scene ini.
/// - Dialog tampil di atas video via DialogueManager.
/// - Player tidak bisa bergerak (tidak ada player).
/// 
/// Cara setup di Unity:
/// 1. Buat scene "OpeningStory".
/// 2. Pastikan ada GameManager, DialogueManager, VideoManager, UIManager di DontDestroyOnLoad.
/// 3. Tambah GameObject dengan komponen ini.
/// 4. Assign VideoSequenceSO yang berisi 4 entri video ke field rangkaianOpeningStory.
/// 5. Di dalam VideoSequenceSO, tiap entri punya DialogueDataSO dengan kalimat dialog-nya.
/// </summary>
public class OpeningStoryController : MonoBehaviour
{
    [Header("Rangkaian Video Sinematik")]
    [Tooltip("SO berisi 4 video: Crossroad, Road to Library, Library Exterior, Library Interior.")]
    [SerializeField] private VideoSequenceSO rangkaianOpeningStory;

    [Header("Fallback")]
    [Tooltip("Nama scene yang dimuat jika VideoManager tidak tersedia atau video selesai.")]
    [SerializeField] private string namaSceneLevelPertama = "LEVEL 1";

    // ─── Lifecycle ───────────────────────────────────────────────────
    private void Start()
    {
        // Sembunyikan HUD saat cinematic
        UIManager.Instance?.TampilkanHUD(false);

        StartCoroutine(MulaiOpeningStory());
    }

    private void OnDestroy()
    {
        // Bersihkan listener saat scene di-unload
        VideoManager.OnRangkaianSelesai -= OnRangkaianVideoSelesai;
    }

    // ─── Coroutine ───────────────────────────────────────────────────
    private IEnumerator MulaiOpeningStory()
    {
        // Tunggu satu frame agar semua singleton siap
        yield return null;

        // Validasi
        if (VideoManager.Instance == null)
        {
            Debug.LogError("[OpeningStory] VideoManager tidak ditemukan! Periksa GameObject Manager di scene.");
            LanjutKeLevel1();
            yield break;
        }

        if (rangkaianOpeningStory == null)
        {
            Debug.LogError("[OpeningStory] rangkaianOpeningStory belum di-assign di Inspector!");
            LanjutKeLevel1();
            yield break;
        }

        // Daftarkan listener event selesai
        VideoManager.OnRangkaianSelesai += OnRangkaianVideoSelesai;

        // Mulai rangkaian video
        VideoManager.Instance.MulaiRangkaian(rangkaianOpeningStory);
    }

    // ─── Event Handler ───────────────────────────────────────────────
    private void OnRangkaianVideoSelesai()
    {
        VideoManager.OnRangkaianSelesai -= OnRangkaianVideoSelesai;
        LanjutKeLevel1();
    }

    // ─── Navigasi ────────────────────────────────────────────────────
    private void LanjutKeLevel1()
    {
        Debug.Log("[OpeningStory] Semua video selesai. Muat Level 1.");
        SaveManager.Instance?.SimpanNamaSceneTerakhir(namaSceneLevelPertama);
        GameManager.Instance?.SetGameplayState();
        GameManager.Instance?.MuatScene(namaSceneLevelPertama);
    }
}
