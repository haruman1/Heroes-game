using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIManager — Pusat manajemen semua layer Canvas.
/// 
/// Tanggung jawab:
/// - Tampilkan / sembunyikan panel (HUD, PauseMenu, JourneyBook, Settings, LoadingScreen)
/// - Fade layar masuk/keluar dengan Image overlay hitam
/// - Update progress bar loading screen
/// 
/// Singleton DontDestroyOnLoad.
/// Taruh pada GameObject "UIManager" yang bertahan antar scene.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panel-Panel Canvas")]
    [SerializeField] private GameObject panelHUD;
    [SerializeField] private GameObject panelPauseMenu;
    [SerializeField] private GameObject panelJourneyBook;
    [SerializeField] private GameObject panelSettings;
    [SerializeField] private GameObject panelLoadingScreen;
    [SerializeField] private GameObject panelToko;

    [Header("Loading Screen")]
    [SerializeField] private Slider     sliderProgressLoading;
    [SerializeField] private TMPro.TMP_Text teksProgressLoading;

    [Header("Fade Layar")]
    [Tooltip("Image hitam full-screen untuk fade in/out. Atur warna ke hitam, alpha 0.")]
    [SerializeField] private Image imageOverlayFade;

    // ─── Lifecycle ───────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // Diganti dengan arsitektur Additive CoreScene

        // Pastikan overlay fade tersembunyi di awal
        if (imageOverlayFade != null)
        {
            Color c = imageOverlayFade.color;
            c.a = 0f;
            imageOverlayFade.color = c;
            imageOverlayFade.gameObject.SetActive(false);
        }
    }

    // ─── Panel Control ───────────────────────────────────────────────
    public void TampilkanHUD(bool aktif)
    {
        if (panelHUD != null) panelHUD.SetActive(aktif);
    }

    public void TampilkanPauseMenu(bool aktif)
    {
        if (panelPauseMenu != null) panelPauseMenu.SetActive(aktif);
    }

    public void TampilkanJourneyBook(bool aktif)
    {
        if (panelJourneyBook != null) panelJourneyBook.SetActive(aktif);
    }

    public void TampilkanSettings(bool aktif)
    {
        if (panelSettings != null) panelSettings.SetActive(aktif);
    }

    public void TampilkanToko(bool aktif)
    {
        if (panelToko != null) panelToko.SetActive(aktif);
    }

    public void TampilkanLoadingScreen(bool aktif)
    {
        if (panelLoadingScreen != null) panelLoadingScreen.SetActive(aktif);
        if (!aktif) UpdateProgressLoadingScreen(0f);
    }

    public void UpdateProgressLoadingScreen(float progress)
    {
        if (sliderProgressLoading != null)
            sliderProgressLoading.value = progress;
        if (teksProgressLoading != null)
            teksProgressLoading.text = $"{Mathf.RoundToInt(progress * 100)}%";
    }

    // ─── Fade Layar ──────────────────────────────────────────────────

    /// <summary>
    /// Fade layar secara perlahan.
    /// fadeIn=true → layar menjadi hitam (gelap).
    /// fadeIn=false → layar muncul dari hitam (terang).
    /// Gunakan: yield return StartCoroutine(UIManager.Instance.FadeLayar(durasi, true));
    /// </summary>
    public IEnumerator FadeLayar(float durasi, bool fadeIn)
    {
        if (imageOverlayFade == null) yield break;

        imageOverlayFade.gameObject.SetActive(true);
        imageOverlayFade.raycastTarget = fadeIn; // Block klik saat layar gelap

        float alphaAwal  = fadeIn ? 0f : 1f;
        float alphaAkhir = fadeIn ? 1f : 0f;
        float waktu      = 0f;

        Color warna = imageOverlayFade.color;
        warna.a             = alphaAwal;
        imageOverlayFade.color = warna;

        while (waktu < durasi)
        {
            waktu  += Time.unscaledDeltaTime;
            warna.a = Mathf.Lerp(alphaAwal, alphaAkhir, waktu / durasi);
            imageOverlayFade.color = warna;
            yield return null;
        }

        warna.a             = alphaAkhir;
        imageOverlayFade.color = warna;

        // Matikan overlay jika sudah transparan
        if (!fadeIn)
        {
            imageOverlayFade.gameObject.SetActive(false);
            imageOverlayFade.raycastTarget = false;
        }
    }

    /// <summary>Set alpha overlay seketika tanpa animasi.</summary>
    public void FadeLayarInstan(bool fadeIn)
    {
        if (imageOverlayFade == null) return;

        imageOverlayFade.gameObject.SetActive(true);
        Color warna = imageOverlayFade.color;
        warna.a             = fadeIn ? 1f : 0f;
        imageOverlayFade.color = warna;

        if (!fadeIn) imageOverlayFade.gameObject.SetActive(false);
    }
}
