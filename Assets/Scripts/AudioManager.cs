using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioMixer mixer;

    public AudioSource musicSource;
    public AudioSource sfxSource;

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

    public void SetMasterVolume(float value)
    {
        Debug.Log("Master = " + value);
        mixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
    }

    public void SetMusicVolume(float value)
    {
        Debug.Log("Music = " + value);
        mixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
    }

    public void SetSFXVolume(float value)
    {
        Debug.Log("SFX = " + value);
        mixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
    }
}