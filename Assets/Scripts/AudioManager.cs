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
        // DontDestroyOnLoad(gameObject); // Diganti dengan arsitektur Additive CoreScene
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
        if (clip == null) return;

        if (voiceOverSource != null)
        {
            voiceOverSource.clip = clip;
            voiceOverSource.Play();
        }
        else if (sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            AudioSource tempSource = GetComponent<AudioSource>();
            if (tempSource == null) tempSource = gameObject.AddComponent<AudioSource>();
            tempSource.PlayOneShot(clip);
        }
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
