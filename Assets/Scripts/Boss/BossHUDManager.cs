using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Mengelola seluruh UI dalam scene arena:
/// - Health bar bos (animasi smooth fill + shake)
/// - Counter musuh keroco (X / Y tersisa)
/// - Panel menang / kalah
/// - Label phase 2
/// </summary>
public class BossHUDManager : MonoBehaviour
{
    // =============================================
    //  PANEL UTAMA
    // =============================================

    [Header("Panels")]
    public GameObject bossHUDPanel;
    public GameObject kerocoHUDPanel;
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject phase2Panel;   // Muncul sebentar saat masuk phase 2

    // =============================================
    //  BOSS HUD
    // =============================================

    [Header("Boss Health Bar")]
    public Image     bossHealthFill;       // Image dengan Image Type = Filled
    public TMP_Text  bossNameText;
    public TMP_Text  bossHealthText;       // Opsional: "75 / 100"
    public Image     bossHealthBarRoot;    // Container bar (untuk efek shake)

    [Header("Boss Phase 2")]
    public TMP_Text  phase2Text;

    // =============================================
    //  KEROCO HUD
    // =============================================

    [Header("Keroco Counter")]
    public TMP_Text kerocoCountText;      // "Musuh tersisa: 3 / 5"
    public Image    kerocoProgressFill;   // Progress bar jumlah keroco mati

    // =============================================
    //  ARENA NAME
    // =============================================

    [Header("Arena Info")]
    public TMP_Text arenaNameText;

    // =============================================
    //  WIN / LOSE
    // =============================================

    [Header("Win / Lose")]
    public TMP_Text winTitleText;
    public TMP_Text loseTitleText;

    // =============================================
    //  ANIMASI
    // =============================================

    [Header("Animation Settings")]
    public float healthLerpSpeed = 5f;
    public float shakeMagnitude  = 8f;
    public float shakeDuration   = 0.4f;

    // =============================================
    //  PRIVATE
    // =============================================

    private float targetHealthRatio   = 1f;
    private float displayedHealthRatio = 1f;
    private int   kerocoTotal  = 0;
    private int   kerocoAlive  = 0;
    private Coroutine shakeRoutine;

    // =============================================
    //  UNITY
    // =============================================

    private void Awake()
    {
        HideAll();
    }

    private void Update()
    {
        // Smooth boss health bar
        if (bossHealthFill != null)
        {
            displayedHealthRatio = Mathf.Lerp(displayedHealthRatio, targetHealthRatio, Time.deltaTime * healthLerpSpeed);
            bossHealthFill.fillAmount = displayedHealthRatio;

            // Warna merah saat HP rendah
            if (displayedHealthRatio < 0.3f)
                bossHealthFill.color = Color.red;
            else if (displayedHealthRatio < 0.6f)
                bossHealthFill.color = new Color(1f, 0.5f, 0f); // oranye
            else
                bossHealthFill.color = Color.green;
        }
    }

    // =============================================
    //  SHOW ARENA HUD
    // =============================================

    public void ShowArenaHUD(string arenaName)
    {
        if (arenaNameText != null) arenaNameText.text = arenaName;

        // Scene boss hanya punya Boss HUD
        if (bossHUDPanel != null)   bossHUDPanel.SetActive(true);
        if (kerocoHUDPanel != null) kerocoHUDPanel.SetActive(false);
    }

    // =============================================
    //  BOSS HEALTH
    // =============================================

    public void SetBossInfo(string name, float healthRatio)
    {
        if (bossNameText != null) bossNameText.text = name;
        targetHealthRatio   = healthRatio;
        displayedHealthRatio = healthRatio;
        if (bossHealthFill != null) bossHealthFill.fillAmount = healthRatio;
    }

    public void UpdateBossHealth(float ratio)
    {
        targetHealthRatio = Mathf.Clamp01(ratio);

        // Teks HP (opsional)
        if (bossHealthText != null)
        {
            bossHealthText.text = $"{Mathf.RoundToInt(ratio * 100)}%";
        }

        // Shake bar saat kena damage
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeBar());
    }

    // =============================================
    //  KEROCO COUNTER
    // =============================================

    public void SetKerocoCount(int alive, int total)
    {
        kerocoAlive = alive;
        kerocoTotal = total;

        if (kerocoCountText != null)
            kerocoCountText.text = $"Musuh tersisa: {alive} / {total}";

        if (kerocoProgressFill != null && total > 0)
        {
            float ratio = 1f - (alive / (float)total);
            kerocoProgressFill.fillAmount = ratio;
        }
    }

    // =============================================
    //  PHASE 2
    // =============================================

    public void ShowPhase2Warning()
    {
        if (phase2Panel == null) return;
        StartCoroutine(Phase2Popup());
    }

    private IEnumerator Phase2Popup()
    {
        phase2Panel.SetActive(true);
        if (phase2Text != null)
        {
            phase2Text.text = "⚠ PHASE 2 ⚠";
            phase2Text.color = Color.red;
        }
        yield return new WaitForSeconds(2.5f);
        phase2Panel.SetActive(false);
    }

    // =============================================
    //  WIN / LOSE
    // =============================================

    public void ShowWinScreen()
    {
        if (winPanel != null) winPanel.SetActive(true);
        if (winTitleText != null) winTitleText.text = "🏆 Selamat! Arena Selesai!";
    }

    public void ShowLoseScreen()
    {
        if (losePanel != null) losePanel.SetActive(true);
        if (loseTitleText != null) loseTitleText.text = "💀 Kamu Kalah...";
    }

    // =============================================
    //  PRIVATE HELPER
    // =============================================

    private void HideAll()
    {
        if (bossHUDPanel != null)   bossHUDPanel.SetActive(false);
        if (kerocoHUDPanel != null) kerocoHUDPanel.SetActive(false);
        if (winPanel != null)       winPanel.SetActive(false);
        if (losePanel != null)      losePanel.SetActive(false);
        if (phase2Panel != null)    phase2Panel.SetActive(false);
    }

    private IEnumerator ShakeBar()
    {
        if (bossHealthBarRoot == null) yield break;

        Vector3 originalPos = bossHealthBarRoot.rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-shakeMagnitude, shakeMagnitude);
            float y = Random.Range(-shakeMagnitude * 0.3f, shakeMagnitude * 0.3f);
            bossHealthBarRoot.rectTransform.anchoredPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        bossHealthBarRoot.rectTransform.anchoredPosition = originalPos;
    }
}
