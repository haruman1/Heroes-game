using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Mengelola tampilan panel pembaca halaman in-game.
/// Menjeda game, menampilkan teks dengan efek mengetik, memutar narasi suara,
/// dan menonaktifkan input pemain selama panel terbuka.
/// </summary>
public class InGamePageReader : MonoBehaviour
{
    public static InGamePageReader Instance { get; private set; }

    [Header("UI Components")]
    public GameObject readerPanel;
    public TMP_Text pageTitleText;
    public TMP_Text pageContentText;
    public TMP_Text pageNumberText;
    public Button closeButton;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Level Pages Content (10 pages)")]
    public BookData[] levelPages = new BookData[10];

    private playerJ activePlayer;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        Instance = this;
        if (readerPanel != null) 
            readerPanel.SetActive(false);
            
        if (closeButton != null) 
            closeButton.onClick.AddListener(ClosePage);
    }

    /// <summary>
    /// Tampilkan halaman tertentu, jeda game, dan putar suara narasi.
    /// </summary>
    public void ShowPage(int pageNumber, playerJ player)
    {
        if (pageNumber < 1 || pageNumber > 10) return;
        activePlayer = player;

        // Nonaktifkan gerakan pemain dan stop kecepatan fisiknya
        if (activePlayer != null)
        {
            activePlayer.inputEnabled = false;
            Rigidbody2D rb = activePlayer.GetComponent<Rigidbody2D>();
            if (rb != null) 
                rb.linearVelocity = Vector2.zero;
        }
        
        // Jeda waktu game
        Time.timeScale = 0f;

        if (readerPanel != null) 
            readerPanel.SetActive(true);

        // Ambil data halaman (0-indexed)
        BookData data = levelPages[pageNumber - 1];

        if (pageNumberText != null) 
            pageNumberText.text = $"Halaman #{pageNumber:D2}";
            
        if (pageTitleText != null) 
            pageTitleText.text = data != null ? data.bookTitle : $"Halaman #{pageNumber:D2}";
        
        if (typingCoroutine != null) 
            StopCoroutine(typingCoroutine);
            
        string content = data != null ? data.bookDescription : "Isi deskripsi halaman ini tidak ditemukan.";
        typingCoroutine = StartCoroutine(TypeText(content));

        // Putar suara narasi
        if (audioSource != null && data != null && data.narrationAudio != null)
        {
            audioSource.clip = data.narrationAudio;
            
            // Sesuaikan volume dengan volume SFX dari database
            float sfxVolume = 1f;
            DatabaseManager dbManager = DatabaseManager.GetOrCreateInstance();
            if (dbManager != null)
            {
                GameSettingsData settings = dbManager.GetSettingsData();
                if (settings != null) sfxVolume = settings.SfxVolume;
            }
            audioSource.volume = sfxVolume;
            audioSource.Play();
        }
    }

    private IEnumerator TypeText(string message)
    {
        if (pageContentText == null) yield break;
        pageContentText.text = "";

        // Karena Time.timeScale = 0, kita harus memakai real time untuk efek mengetik
        foreach (char c in message)
        {
            pageContentText.text += c;
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < start + 0.02f)
            {
                yield return null;
            }
        }
    }

    /// <summary>
    /// Tutup panel pembaca halaman, hentikan suara narasi, dan lanjutkan game.
    /// </summary>
    public void ClosePage()
    {
        if (typingCoroutine != null) 
            StopCoroutine(typingCoroutine);
            
        if (audioSource != null) 
            audioSource.Stop();

        if (readerPanel != null) 
            readerPanel.SetActive(false);

        // Lanjutkan waktu game dan aktifkan kembali gerakan pemain
        Time.timeScale = 1f;
        if (activePlayer != null)
        {
            activePlayer.inputEnabled = true;
        }
    }
}
