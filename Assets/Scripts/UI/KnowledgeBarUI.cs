using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// KnowledgeBarUI — Menampilkan progress pengumpulan halaman buku di HUD level.
/// 
/// Tampil sebagai progress bar (0–10 halaman).
/// Update otomatis via LevelManager.LaporHalamanDikumpulkan().
/// </summary>
public class KnowledgeBarUI : MonoBehaviour
{
    [Header("Komponen UI")]
    [Tooltip("Slider untuk progress bar. Atur Min=0, Max=1.")]
    [SerializeField] private Slider     slider;
    [Tooltip("Image dengan Image Type = Filled (alternatif dari Slider).")]
    [SerializeField] private Image      imageFill;
    [Tooltip("Teks yang menampilkan '3 / 10 Halaman'.")]
    [SerializeField] private TMP_Text   teksProgress;
    [Tooltip("Icon buku di sebelah bar. Opsional.")]
    [SerializeField] private Image      ikonBuku;

    [Header("Animasi")]
    [Tooltip("Kecepatan animasi fill (lerp). 0 = instan, 5 = lambat.")]
    [SerializeField] private float kecepatanAnimasi = 8f;

    // ─── State Internal ──────────────────────────────────────────────
    private float _targetFill = 0f;
    private float _fillSaatIni = 0f;
    private int   _totalHalaman = 10;

    // ─── Lifecycle ───────────────────────────────────────────────────
    private void Update()
    {
        // Animasi smooth lerp ke target
        if (Mathf.Abs(_fillSaatIni - _targetFill) > 0.001f)
        {
            _fillSaatIni = Mathf.Lerp(_fillSaatIni, _targetFill, Time.deltaTime * kecepatanAnimasi);
            ApplyFill(_fillSaatIni);
        }
    }

    // ─── Public API ──────────────────────────────────────────────────

    /// <summary>Update knowledge bar dengan nilai halaman saat ini.</summary>
    public void UpdateBar(int halamanTerkumpul, int totalHalaman)
    {
        _totalHalaman = totalHalaman > 0 ? totalHalaman : 10;
        _targetFill   = Mathf.Clamp01((float)halamanTerkumpul / _totalHalaman);

        if (teksProgress != null)
            teksProgress.text = $"{halamanTerkumpul} / {_totalHalaman} Halaman";
    }

    /// <summary>Reset bar ke 0.</summary>
    public void Reset()
    {
        _targetFill   = 0f;
        _fillSaatIni  = 0f;
        ApplyFill(0f);

        if (teksProgress != null)
            teksProgress.text = $"0 / {_totalHalaman} Halaman";
    }

    // ─── Private ─────────────────────────────────────────────────────
    private void ApplyFill(float nilai)
    {
        if (slider    != null) slider.value        = nilai;
        if (imageFill != null) imageFill.fillAmount = nilai;
    }
}
