using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller per kartu level di layar "Total Perjalanan Mu".
/// Satu komponen ini dipasang ke setiap kartu (6 kartu total).
///
/// Konfigurasi yang bisa diubah di Inspector:
///   - LevelNumber   : nomor level (1–6)
///   - levelName     : nama yang muncul di kartu
///   - sceneName     : nama scene Unity untuk level ini
///   - thumbnail     : gambar kartu per level (unik tiap level)
///   - books[0-9]    : judul + deskripsi 10 buku di level ini
/// </summary>
public class LevelCard : MonoBehaviour
{
    // ----------------------------------------------------------------
    //  Konfigurasi Inspector
    // ----------------------------------------------------------------

    [Header("Level Config — Atur di Inspector")]
    public int LevelNumber = 1;

    [Tooltip("Nama level yang ditampilkan di kartu (misal: Desa Awal, Hutan Larangan)")]
    [SerializeField] private string levelName = "Desa Awal";

    [Tooltip("Nama scene Unity untuk level ini (harus sama persis dengan nama scene di Build Settings)")]
    [SerializeField] private string sceneName = "Level1";

    [Tooltip("Gambar thumbnail unik per level")]
    [SerializeField] private Sprite thumbnail;

    [Header("Book Data — 10 buku per level (isi judul & deskripsi)")]
    [SerializeField] private BookData[] books = new BookData[10]
    {
        new BookData { bookTitle = "Buku #01", bookDescription = "Deskripsi buku pertama." },
        new BookData { bookTitle = "Buku #02", bookDescription = "Deskripsi buku kedua." },
        new BookData { bookTitle = "Buku #03", bookDescription = "Deskripsi buku ketiga." },
        new BookData { bookTitle = "Buku #04", bookDescription = "Deskripsi buku keempat." },
        new BookData { bookTitle = "Buku #05", bookDescription = "Deskripsi buku kelima." },
        new BookData { bookTitle = "Buku #06", bookDescription = "Deskripsi buku keenam." },
        new BookData { bookTitle = "Buku #07", bookDescription = "Deskripsi buku ketujuh." },
        new BookData { bookTitle = "Buku #08", bookDescription = "Deskripsi buku kedelapan." },
        new BookData { bookTitle = "Buku #09", bookDescription = "Deskripsi buku kesembilan." },
        new BookData { bookTitle = "Buku #10", bookDescription = "Deskripsi buku kesepuluh." },
    };

    // ----------------------------------------------------------------
    //  Referensi UI
    // ----------------------------------------------------------------

    [Header("UI References")]
    [Tooltip("Image untuk thumbnail level")]
    public Image     thumbnailImage;

    [Tooltip("Teks angka level (\"1\", \"2\", …)")]
    public TMP_Text  levelNumberText;

    [Tooltip("Teks nama level")]
    public TMP_Text  levelNameText;

    [Tooltip("Teks progress buku, misal: 7 / 10")]
    public TMP_Text  bookCountText;

    [Tooltip("Slider progress bar (0–1)")]
    public Slider    progressBar;

    [Header("Stars — 3 Image, urut dari kiri ke kanan")]
    public Image[]   starImages;
    public Sprite    starFilledSprite;
    public Sprite    starEmptySprite;

    [Header("Tombol & Overlay")]
    [Tooltip("Tombol 'Lihat Koleksi'")]
    public Button    collectButton;

    [Tooltip("GameObject overlay gembok saat level terkunci")]
    public GameObject lockOverlay;

    // ----------------------------------------------------------------
    //  State internal
    // ----------------------------------------------------------------

    private LevelProgressData currentData;
    private JourneyManager    manager;
    private const int         BooksRequired = 10;

    // ================================================================
    //  Public API
    // ================================================================

    /// <summary>
    /// Dipanggil oleh JourneyManager untuk mengisi data kartu.
    /// </summary>
    public void Setup(LevelProgressData data, JourneyManager journeyManager)
    {
        currentData = data;
        manager     = journeyManager;

        // Nomor & nama
        if (levelNumberText != null) levelNumberText.text = LevelNumber.ToString();
        if (levelNameText   != null) levelNameText.text   = levelName;

        // Thumbnail
        if (thumbnailImage != null && thumbnail != null)
            thumbnailImage.sprite = thumbnail;

        // Progress buku
        int collected = data.BooksCollected;
        if (bookCountText != null)
            bookCountText.text = $"{collected} / {BooksRequired}";
        if (progressBar != null)
            progressBar.value = (float)collected / BooksRequired;

        // Bintang
        RefreshStars(data.Stars);

        // Kunci / buka
        bool unlocked  = data.UnlockedBool;
        bool completed = data.CompletedBool;

        if (lockOverlay != null)
            lockOverlay.SetActive(!unlocked);

        if (collectButton != null)
        {
            // Tombol aktif hanya jika level sudah selesai minimal sekali
            collectButton.interactable = completed;
            collectButton.onClick.RemoveAllListeners();
            collectButton.onClick.AddListener(OnCollectButtonClicked);
        }
    }

    // ================================================================
    //  Accessor untuk JourneyManager & BookCollectionPanel
    // ================================================================

    public BookData[] GetBookDataArray() => books;
    public string     GetLevelName()     => levelName;
    public string     GetSceneName()     => sceneName;

    // ================================================================
    //  Private
    // ================================================================

    private void RefreshStars(int starCount)
    {
        if (starImages == null) return;
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;
            starImages[i].sprite = i < starCount ? starFilledSprite : starEmptySprite;
        }
    }

    private void OnCollectButtonClicked()
    {
        manager?.OpenBookCollection(this);
    }
}
