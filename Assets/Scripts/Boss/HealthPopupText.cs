using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Teks popup melayang "+1 HP" yang muncul saat player mengambil item darah.
/// Dipasang di prefab yang berisi TMP_Text, lalu di-destroy otomatis.
/// </summary>
public class HealthPopupText : MonoBehaviour
{
    [Header("Settings")]
    public float floatSpeed   = 1.5f;
    public float fadeDuration = 1f;
    public Color textColor    = Color.green;

    private TMP_Text label;

    private void Awake()
    {
        label = GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.color = textColor;
        }
    }

    private void Start()
    {
        StartCoroutine(FloatAndFade());
    }

    public void SetAmount(int amount)
    {
        if (label != null)
            label.text = $"+{amount} HP";
    }

    private IEnumerator FloatAndFade()
    {
        float elapsed = 0f;
        Color startColor = label != null ? label.color : textColor;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // Naik ke atas
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            // Fade out
            if (label != null)
            {
                Color c = startColor;
                c.a = 1f - t;
                label.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
