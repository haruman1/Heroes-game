using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panel "Lihat Koleksi" — menampilkan grid 10 buku dan detail per buku.
/// Attach ke root GameObject panel koleksi di scene Journey.
///
/// Struktur hierarki UI yang direkomendasikan:
/// BookCollectionPanel (this script)
///   ├── CollectionGridView
///   │     ├── TitleText
///   │     ├── ProgressText
///   │     ├── BookGrid (GridLayoutGroup, berisi 10 BookSlot prefab)
///   │     ├── HintText
///   │     └── CloseButton (panah kiri)
///   └── BookDetailView
///         ├── HeaderText         ("BUKU #03")
///         ├── BackButton         (panah kiri)
///         ├── CloseButton        (X)
///         ├── BookImage          (Image – sprite sama untuk semua)
///         ├── BookTitleText
///         ├── BookDescriptionText
///         └── TutupButton
/// </summary>
public class BookCollectionPanel : MonoBehaviour
{
    [Header("Root Panel")]
    public GameObject panelRoot;

    // ----------------------------------------------------------------
    //  Grid View
    // ----------------------------------------------------------------
    [Header("Collection Grid View")]
    public GameObject collectionGridView;
    [Tooltip("Teks judul, misal: LEVEL 2 - HUTAN")]
    public TMP_Text   collectionTitleText;
    [Tooltip("Teks progress, misal: Progress : 7 / 10 Buku")]
    public TMP_Text   progressText;
    [Tooltip("Parent GridLayoutGroup tempat BookSlot diinstansiasi")]
    public Transform  bookGridParent;
    [Tooltip("Prefab BookSlot (berisi BookSlotUI component)")]
    public GameObject bookSlotPrefab;
    [Tooltip("Teks petunjuk di bagian bawah")]
    public TMP_Text   hintText;
    [Tooltip("Tombol kembali / tutup dari grid")]
    public Button     closeGridButton;

    // ----------------------------------------------------------------
    //  Detail View
    // ----------------------------------------------------------------
    [Header("Book Detail View")]
    public GameObject bookDetailView;
    [Tooltip("Header, misal: BUKU #03")]
    public TMP_Text   bookDetailHeaderText;
    [Tooltip("Gambar buku — sprite-nya tetap sama untuk semua buku")]
    public Image      bookDetailImage;
    public TMP_Text   bookDetailTitleText;
    public TMP_Text   bookDetailDescText;
    [Tooltip("Tombol 'Tutup' di detail view")]
    public Button     tutupButton;
    [Tooltip("Tombol panah kembali ke grid")]
    public Button     backToGridButton;

    // ----------------------------------------------------------------
    //  Sprite Buku
    // ----------------------------------------------------------------
    [Header("Book Sprites")]
    [Tooltip("Sprite buku yang sudah dikumpulkan (sama untuk semua slot)")]
    public Sprite bookCollectedSprite;
    [Tooltip("Sprite gembok untuk buku yang belum dikumpulkan")]
    public Sprite bookLockedSprite;

    // ----------------------------------------------------------------
    //  State internal
    // ----------------------------------------------------------------
    private LevelProgressData currentData;
    private BookData[]        currentBooks;
    private string            currentLevelName;
    private int               currentLevelNumber;
    private BookSlotUI[]      spawnedSlots;

    // ================================================================
    //  Unity Lifecycle
    // ================================================================

    private void Awake()
    {
        // Pastikan panel tersembunyi di awal
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void Start()
    {
        if (closeGridButton   != null) closeGridButton.onClick.AddListener(Hide);
        if (tutupButton       != null) tutupButton.onClick.AddListener(Hide);
        if (backToGridButton  != null) backToGridButton.onClick.AddListener(ShowGrid);
    }

    // ================================================================
    //  Public API
    // ================================================================

    /// <summary>
    /// Tampilkan panel koleksi untuk level tertentu.
    /// Dipanggil dari JourneyManager saat tombol "Lihat Koleksi" ditekan.
    /// </summary>
    public void Show(LevelProgressData data, BookData[] books,
                     int levelNumber, string levelName)
    {
        currentData        = data;
        currentBooks       = books;
        currentLevelNumber = levelNumber;
        currentLevelName   = levelName;

        if (panelRoot != null) panelRoot.SetActive(true);
        ShowGrid();
    }

    /// <summary>Sembunyikan panel sepenuhnya.</summary>
    public void Hide()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    // ================================================================
    //  Grid View
    // ================================================================

    private void ShowGrid()
    {
        if (collectionGridView != null) collectionGridView.SetActive(true);
        if (bookDetailView     != null) bookDetailView.SetActive(false);

        // Judul
        if (collectionTitleText != null)
            collectionTitleText.text = $"LEVEL {currentLevelNumber} - {currentLevelName.ToUpper()}";

        // Progress
        int required  = currentData?.BooksRequired > 0 ? currentData.BooksRequired : 10;
        int collected = currentData?.BooksCollected ?? 0;
        if (progressText != null)
            progressText.text = $"Progress : {collected} / {required} Buku";

        // Petunjuk
        if (hintText != null)
        {
            hintText.text = collected >= required
                ? "Selamat! Kamu sudah mengumpulkan semua buku! ⭐⭐⭐"
                : "Kumpulkan semua buku untuk mendapatkan Bintang 3 di level ini!";
        }

        BuildBookSlots(required);
    }

    private void BuildBookSlots(int total)
    {
        if (bookGridParent == null || bookSlotPrefab == null) return;

        // Hapus slot lama
        foreach (Transform child in bookGridParent)
            Destroy(child.gameObject);

        spawnedSlots = new BookSlotUI[total];

        for (int i = 0; i < total; i++)
        {
            int bookNumber  = i + 1; // 1-indexed
            bool isCollected = currentData != null && currentData.IsBookCollected(bookNumber);

            GameObject slotGO = Instantiate(bookSlotPrefab, bookGridParent);
            BookSlotUI slot   = slotGO.GetComponent<BookSlotUI>();

            if (slot != null)
            {
                int capturedNumber = bookNumber; // closure capture
                slot.Setup(
                    bookNumber,
                    isCollected,
                    bookCollectedSprite,
                    bookLockedSprite,
                    () => ShowBookDetail(capturedNumber)
                );
                spawnedSlots[i] = slot;
            }
        }
    }

    // ================================================================
    //  Detail View
    // ================================================================

    private void ShowBookDetail(int bookNumber)
    {
        if (collectionGridView != null) collectionGridView.SetActive(false);
        if (bookDetailView     != null) bookDetailView.SetActive(true);

        // Header
        if (bookDetailHeaderText != null)
            bookDetailHeaderText.text = $"BUKU #{bookNumber:D2}";

        // Konten dari BookData (dikonfigurasi di Inspector LevelCard)
        int idx = bookNumber - 1;
        string title = "Judul Buku";
        string desc  = "";

        if (currentBooks != null && idx >= 0 && idx < currentBooks.Length)
        {
            title = currentBooks[idx].bookTitle;
            desc  = currentBooks[idx].bookDescription;
        }

        if (bookDetailTitleText != null) bookDetailTitleText.text = title;
        if (bookDetailDescText  != null) bookDetailDescText.text  = desc;

        // Gambar buku: sprite sudah diset di Inspector, tidak berubah antar buku
        // (bookDetailImage.sprite tidak diubah di sini agar tetap sama)
    }
}
