using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance;
public Slider masterSlider;
public Slider musicSlider;
public Slider sfxSlider;
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
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LoadSettings();
    }

   void LoadSettings()
{
    currentSettings = DatabaseManager.GetOrCreateInstance().GetSettingsData();

    if (currentSettings == null)
        return;

    // Isi slider
    masterSlider.SetValueWithoutNotify(currentSettings.MasterVolume);
    musicSlider.SetValueWithoutNotify(currentSettings.MusicVolume);
    sfxSlider.SetValueWithoutNotify(currentSettings.SfxVolume);

    // Terapkan ke AudioMixer
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.SetMasterVolume(currentSettings.MasterVolume);
        AudioManager.Instance.SetMusicVolume(currentSettings.MusicVolume);
        AudioManager.Instance.SetSFXVolume(currentSettings.SfxVolume);
    }
}

    private void ScheduleSave()
    {
         Debug.Log("ScheduleSave");
        if (pendingSave)
        {
            CancelInvoke(nameof(ExecuteSave));
        }
        pendingSave = true;
        Invoke(nameof(ExecuteSave), 0.5f);
    }

    private void ExecuteSave()
    {
         Debug.Log("ExecuteSave");
        pendingSave = false;
        if (currentSettings != null)
        {
            DatabaseManager.GetOrCreateInstance().SaveSettings(currentSettings);
        }
    }

    public void SetMaster(float value)
    {
        Debug.Log("SetMaster dipanggil : " + value);
        if (AudioManager.Instance != null) AudioManager.Instance.SetMasterVolume(value);

        if (currentSettings != null)
        {
            currentSettings.MasterVolume = value;
            ScheduleSave();
        }
    }

    public void SetMusic(float value)
    {
        Debug.Log("SetMusic dipanggil : " + value);
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(value);

        if (currentSettings != null)
        {
            currentSettings.MusicVolume = value;
            ScheduleSave();
        }
    }

    public void SetSFX(float value)
    {
        Debug.Log("SetSFX dipanggil : " + value);
        if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value);

        if (currentSettings != null)
        {
            currentSettings.SfxVolume = value;
            ScheduleSave();
        }
    }

    void OnApplicationQuit()
    {
        if (pendingSave)
        {
            CancelInvoke(nameof(ExecuteSave));
            ExecuteSave();
        }
    }
}