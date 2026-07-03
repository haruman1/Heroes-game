using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanelUI : MonoBehaviour
{
    public static SettingsPanelUI Instance;
    
    [Header("Audio Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Camera Zoom")]
    public Slider zoomSlider;

    [Header("Language Select (Left/Right Buttons)")]
    public TextMeshProUGUI languageTextLabel; // Label text in the middle (e.g. INDONESIA / ENGLISH)
    
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        var setting = SettingManager.Instance.CurrentSettings;

        masterSlider.SetValueWithoutNotify(setting.MasterVolume);
        musicSlider.SetValueWithoutNotify(setting.MusicVolume);
        sfxSlider.SetValueWithoutNotify(setting.SfxVolume);

        if (zoomSlider != null)
        {
            zoomSlider.SetValueWithoutNotify(setting.CameraZoom);
        }

        RefreshLanguageLabel();
    }

    public void MasterChanged(float value)
    {
        SettingManager.Instance.SetMaster(value);
    }
              
    public void MusicChanged(float value)
    {
        SettingManager.Instance.SetMusic(value);
    }

    public void SFXChanged(float value)
    {
        SettingManager.Instance.SetSFX(value);
    }

    // Call this from Zoom Slider's OnValueChanged event
    public void ZoomChanged(float value)
    {
        SettingManager.Instance.SetZoom(value);
    }

    // Call this from the Right Button click event
    public void NextLanguage()
    {
        ToggleLanguage();
    }

    // Call this from the Left Button click event
    public void PrevLanguage()
    {
        ToggleLanguage();
    }

    private void ToggleLanguage()
    {
        string currentLang = SettingManager.Instance.GetPendingLanguage();
        // Cycle between "id" (index 0) and "en" (index 1)
        int newLangIndex = currentLang == "id" ? 1 : 0;
        
        SettingManager.Instance.SetLanguage(newLangIndex);
        RefreshLanguageLabel();
    }

    private void RefreshLanguageLabel()
    {
        if (languageTextLabel != null)
        {
            string pendingLang = SettingManager.Instance.GetPendingLanguage();
            // Display capitalized text
            languageTextLabel.text = pendingLang == "id" ? "INDONESIA" : "ENGLISH";
        }
    }

    public void RefreshUI(GameSettingsData data)
    {
        masterSlider.SetValueWithoutNotify(data.MasterVolume);
        musicSlider.SetValueWithoutNotify(data.MusicVolume);
        sfxSlider.SetValueWithoutNotify(data.SfxVolume);

        if (zoomSlider != null)
        {
            zoomSlider.SetValueWithoutNotify(data.CameraZoom);
        }

        RefreshLanguageLabel();

        if (Fps.Instance != null)
        {
            Fps.Instance.RefreshLabel();
        }
    }
}