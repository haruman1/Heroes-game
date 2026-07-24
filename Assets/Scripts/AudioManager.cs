using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer mixer;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Tooltip("AudioSource khusus untuk Voice Over / Dubbing. Terpisah dari SFX.")]
    public AudioSource voiceOverSource;

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

    // ─── Volume Mixer ─────────────────────────────────────────────────
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

    // ─── BGM ─────────────────────────────────────────────────────────
    /// <summary>Putar Background Music. Otomatis mengganti clip jika berbeda.</summary>
    public void PutarBGM(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    /// <summary>Hentikan Background Music.</summary>
    public void HentikanBGM()
    {
        if (musicSource != null) musicSource.Stop();
    }

    // ─── Voice Over ───────────────────────────────────────────────────
    /// <summary>Putar Voice Over / Dubbing untuk baris dialog.</summary>
    public void PutarVoiceOver(AudioClip clip)
    {
        if (voiceOverSource == null || clip == null) return;
        voiceOverSource.clip = clip;
        voiceOverSource.Play();
    }

    /// <summary>Hentikan Voice Over yang sedang diputar.</summary>
    public void HentikanVoiceOver()
    {
        if (voiceOverSource != null && voiceOverSource.isPlaying)
            voiceOverSource.Stop();
    }

    // ─── SFX ─────────────────────────────────────────────────────────
    /// <summary>Putar SFX sekali pakai.</summary>
    public void PutarSFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }
}