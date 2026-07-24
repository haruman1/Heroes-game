using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// LoadingScreenController — Tampilan loading screen saat transisi scene.
/// 
/// Diaktifkan / dinonaktifkan oleh UIManager saat GameManager melakukan LoadSceneAsync.
/// Menampilkan tips acak dari daftar yang dikonfigurasi di Inspector.
/// </summary>
public class LoadingScreenController : MonoBehaviour
{
    [Header("Komponen UI")]
    [SerializeField] private Slider   sliderProgress;
    [SerializeField] private TMP_Text teksProgress;
    [SerializeField] private TMP_Text teksTips;

    [Header("Animasi")]
    [Tooltip("Ikon loading yang berputar (RectTransform). Opsional.")]
    [SerializeField] private RectTransform ikonPutar;
    [SerializeField] private float kecepatanPutar = 200f;

    [Header("Tips Saat Loading")]
    [TextArea(2, 4)]
    public string[] daftarTips = new string[]
    {
        "Kumpulkan semua 10 halaman di setiap level untuk membuka Buku di Journey Book!",
        "Booster yang dikumpulkan akan berguna saat menghadapi Boss.",
        "Kamu bisa membuka Journey Book dari Pause Menu kapan saja.",
        "Hati-hati dengan musuh! Kesehatan berkurang jika terkena.",
        "Gunakan tombol Shift untuk berlari lebih cepat.",
    };

    // ─── Lifecycle ───────────────────────────────────────────────────
    private void OnEnable()
    {
        // Tampilkan tips acak setiap kali loading screen muncul
        if (daftarTips != null && daftarTips.Length > 0 && teksTips != null)
        {
            teksTips.text = daftarTips[Random.Range(0, daftarTips.Length)];
        }

        SetProgress(0f);
    }

    private void Update()
    {
        // Putar ikon loading
        if (ikonPutar != null)
        {
            ikonPutar.Rotate(0f, 0f, -kecepatanPutar * Time.unscaledDeltaTime);
        }
    }

    // ─── Public API ──────────────────────────────────────────────────

    /// <summary>Update progress bar loading (0.0 – 1.0).</summary>
    public void SetProgress(float progress)
    {
        if (sliderProgress != null) sliderProgress.value   = progress;
        if (teksProgress   != null) teksProgress.text = $"{Mathf.RoundToInt(progress * 100)}%";
    }
}
