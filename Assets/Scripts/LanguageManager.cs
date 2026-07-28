using System.Collections.Generic;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    // Delegate and event for when the language changes
    public delegate void LanguageChangedHandler();
    public event LanguageChangedHandler OnLanguageChanged;

    private string currentLanguage = "id";
    private Dictionary<string, string> activeTranslations = new Dictionary<string, string>();

    // Helper classes to parse JSON
    [System.Serializable]
    private class TranslationItem
    {
        public string key;
        public string value;
    }

    [System.Serializable]
    private class TranslationList
    {
        public List<TranslationItem> translations;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad(gameObject); // Diganti dengan arsitektur Additive CoreScene
        
        // Load default language on start
        LoadTranslations(currentLanguage);
    }

    /// <summary>
    /// Set current language, load its JSON translation file, and update all localized UI texts.
    /// </summary>
    public void SetLanguage(string language)
    {
        // Treat "system" as default language (id or en depending on system settings)
        if (language == "system")
        {
            language = GetSystemLanguageCode();
        }

        currentLanguage = language;
        Debug.Log($"[LanguageManager] SetLanguage: {language}");

        LoadTranslations(language);
        ApplyLanguage();
    }

    public string GetCurrentLanguage()
    {
        return currentLanguage;
    }

    /// <summary>
    /// Retrieve translated string by key. Returns key if translation is missing.
    /// </summary>
    public string GetTranslation(string key)
    {
        if (activeTranslations.TryGetValue(key, out string translatedValue))
        {
            return translatedValue;
        }
        
        // Debug warning if key is missing
        Debug.LogWarning($"[LanguageManager] Missing translation key: {key}");
        return key;
    }

    private void LoadTranslations(string language)
    {
        activeTranslations.Clear();

        // Load JSON file from Resources/Languages/ folder (e.g. Languages/id or Languages/en)
        string resourcePath = $"Languages/{language}";
        TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);

        if (jsonAsset == null)
        {
            Debug.LogError($"[LanguageManager] Translation file not found at Resources/{resourcePath}.json. Falling back to English.");
            jsonAsset = Resources.Load<TextAsset>("Languages/en");
        }

        if (jsonAsset != null)
        {
            try
            {
                TranslationList list = JsonUtility.FromJson<TranslationList>(jsonAsset.text);
                if (list != null && list.translations != null)
                {
                    foreach (var item in list.translations)
                    {
                        if (!activeTranslations.ContainsKey(item.key))
                        {
                            activeTranslations.Add(item.key, item.value);
                        }
                    }
                    Debug.Log($"[LanguageManager] Successfully loaded {activeTranslations.Count} translations for '{language}'.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LanguageManager] Error parsing JSON translation file: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("[LanguageManager] No translation files could be loaded.");
        }
    }

    private void ApplyLanguage()
    {
        // Trigger event to notify all LocalizedText scripts to update their texts
        OnLanguageChanged?.Invoke();
    }

    private string GetSystemLanguageCode()
    {
        switch (Application.systemLanguage)
        {
            case SystemLanguage.Indonesian:
                return "id";
            default:
                return "en"; // Default fallback is English
        }
    }
}