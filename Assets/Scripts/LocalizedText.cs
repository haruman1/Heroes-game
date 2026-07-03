using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    [Tooltip("The translation key from JSON (e.g. menu_play)")]
    [SerializeField] private string translationKey;

    private Text unityText;
    private TMP_Text tmpText;

    private void Awake()
    {
        // Cache references to either legacy Text or modern TextMeshPro component
        unityText = GetComponent<Text>();
        tmpText = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        if (LanguageManager.Instance != null)
        {
            // Subscribe to language change events
            LanguageManager.Instance.OnLanguageChanged += UpdateText;
        }

        UpdateText();
    }

    private void OnDestroy()
    {
        if (LanguageManager.Instance != null)
        {
            // Unsubscribe to avoid memory leaks
            LanguageManager.Instance.OnLanguageChanged -= UpdateText;
        }
    }

    /// <summary>
    /// Updates the text element content using the translated value.
    /// </summary>
    public void UpdateText()
    {
        if (string.IsNullOrEmpty(translationKey))
            return;

        if (LanguageManager.Instance == null)
            return;

        string translatedString = LanguageManager.Instance.GetTranslation(translationKey);

        if (unityText != null)
        {
            unityText.text = translatedString;
        }
        
        if (tmpText != null)
        {
            tmpText.text = translatedString;
        }
    }

    /// <summary>
    /// Allows changing the translation key dynamically from code
    /// </summary>
    public void SetKey(string newKey)
    {
        translationKey = newKey;
        UpdateText();
    }
}
