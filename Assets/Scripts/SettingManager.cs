using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance;
    public GameSettingsData CurrentSettings => currentSettings;
    private GameSettingsData pendingSettings;   // data yang sedang diedit
    [Header("Development")]
    [SerializeField]
    private bool enableShortLogs = true;
    private GameSettingsData currentSettings;
    private bool pendingSave = false;
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad(gameObject); // Diganti dengan arsitektur Additive CoreScene
    }

    void Start()
    {
        LoadSettings();
    }
public GameSettingsData Clone(GameSettingsData source)
{
    return new GameSettingsData
    {
        Id = source.Id,
        FpsLimit = source.FpsLimit,
        MasterVolume = source.MasterVolume,
        MusicVolume = source.MusicVolume,
        SfxVolume = source.SfxVolume,
        CameraZoom = source.CameraZoom,
        Fullscreen = source.Fullscreen,
        ResolutionWidth = source.ResolutionWidth,
        ResolutionHeight = source.ResolutionHeight,
        Language = source.Language
    };
}
public int GetPendingFPS()
{
    return pendingSettings.FpsLimit;
}

public string GetPendingLanguage()
{
    return pendingSettings != null ? pendingSettings.Language : "id";
}

public void SetFPS(int fps)
{
    pendingSettings.FpsLimit = fps;

    Application.targetFrameRate = fps;
}

public void SetZoom(float value)
{
    pendingSettings.CameraZoom = value;

    if (CameraZoom.Instance != null)
    {
        CameraZoom.Instance.SetZoom(value);
    }
}
private void ApplySettings(GameSettingsData data)
{
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.SetMasterVolume(data.MasterVolume);
        AudioManager.Instance.SetMusicVolume(data.MusicVolume);
        AudioManager.Instance.SetSFXVolume(data.SfxVolume);
    }

    Application.targetFrameRate = data.FpsLimit;

    if (LanguageManager.Instance != null)
    {
        LanguageManager.Instance.SetLanguage(data.Language);
    }

    if (CameraZoom.Instance != null)
    {
        CameraZoom.Instance.SetZoom(data.CameraZoom);
    }
}
   void LoadSettings()
{
    currentSettings = DatabaseManager.GetOrCreateInstance().GetSettingsData();
    pendingSettings = Clone(currentSettings);

    ApplySettings(currentSettings);
    if (currentSettings == null)
        return;
    
    // Terapkan ke AudioMixer
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.SetMasterVolume(currentSettings.MasterVolume);
        AudioManager.Instance.SetMusicVolume(currentSettings.MusicVolume);
        AudioManager.Instance.SetSFXVolume(currentSettings.SfxVolume);
    }
    // FPS
    Application.targetFrameRate = currentSettings.FpsLimit;

    // Bahasa
   LanguageManager.Instance.SetLanguage(currentSettings.Language);
}
public void SetLanguage(int index)
{
    string language = index == 0 ? "id" : "en";

    pendingSettings.Language = language;

    if (LanguageManager.Instance != null)
    {
        LanguageManager.Instance.SetLanguage(language);
    }
}
   public void SaveSettings()
{
    if (pendingSettings == null)
        return;

    currentSettings = Clone(pendingSettings);

    DatabaseManager.GetOrCreateInstance().SaveSettings(currentSettings);
    SettingsPanelUI.Instance.RefreshUI(currentSettings);
    LogShort("Settings berhasil disimpan.");
}
public void CancelSettings()
{
    pendingSettings = Clone(currentSettings);

    ApplySettings(currentSettings);
    SettingsPanelUI.Instance.RefreshUI(currentSettings);
   LogShort("Settings dibatalkan, kembali ke pengaturan sebelumnya.");
}
    private void ExecuteSave()
    {
        LogShort("ExecuteSave");
        pendingSave = false;
        if (currentSettings != null)
        {
            DatabaseManager.GetOrCreateInstance().SaveSettings(currentSettings);
        }
    }

public void SetMaster(float value)
{
    LogShort("SetMaster : " + value);
    AudioManager.Instance.SetMasterVolume(value);

    pendingSettings.MasterVolume = value; // ✅ BENAR
}

public void SetMusic(float value)
{
    LogShort("SetMusic : " + value);

  AudioManager.Instance.SetMusicVolume(value);

    pendingSettings.MusicVolume = value; // ✅ BENAR
}

   public void SetSFX(float value)
{
    LogShort("SetSFX : " + value);
    AudioManager.Instance.SetSFXVolume(value);

    pendingSettings.SfxVolume = value; // ✅ BENAR
}
    void OnApplicationQuit()
    {
        if (pendingSave)
        {
            CancelInvoke(nameof(ExecuteSave));
            ExecuteSave();
        }
    }
     private void LogShort(string message)
    {
        if (!enableShortLogs)
            return;

        Debug.Log($"[DB] {message}");
    }
}