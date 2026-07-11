using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Komponen untuk satu slot buku di panel koleksi.
/// Attach ke prefab BookSlot yang berisi Image + TMP_Text + Button.
/// </summary>
public class BookSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image  bookImage;
    public TMP_Text numberText;
    public Button slotButton;

    [Tooltip("Highlight/border kuning saat slot ini dipilih (opsional)")]
    public GameObject selectedHighlight;

    /// <summary>
    /// Inisialisasi tampilan slot buku.
    /// </summary>
    /// <param name="bookNumber">Nomor buku (1–10)</param>
    /// <param name="isCollected">Sudah dikumpulkan atau belum</param>
    /// <param name="collectedSprite">Gambar buku (sama untuk semua)</param>
    /// <param name="lockedSprite">Gambar gembok</param>
    /// <param name="onClickCallback">Callback saat buku diklik (hanya aktif jika terkumpul)</param>
    public void Setup(int bookNumber, bool isCollected,
                      Sprite collectedSprite, Sprite lockedSprite,
                      Action onClickCallback)
    {
        // Nomor label (01, 02, …)
        if (numberText != null)
            numberText.text = $"{bookNumber:D2}";

        // Gambar
        if (bookImage != null)
            bookImage.sprite = isCollected ? collectedSprite : lockedSprite;

        // Transparansi: buku terkunci sedikit buram
        if (bookImage != null)
        {
            Color c = bookImage.color;
            c.a = isCollected ? 1f : 0.7f;
            bookImage.color = c;
        }

        // Tombol: hanya aktif jika sudah dikumpulkan
        if (slotButton != null)
        {
            slotButton.interactable = isCollected;
            slotButton.onClick.RemoveAllListeners();
            if (isCollected && onClickCallback != null)
                slotButton.onClick.AddListener(() => onClickCallback());
        }

        // Highlight dimatikan secara default
        if (selectedHighlight != null)
            selectedHighlight.SetActive(false);
    }

    /// <summary>Aktifkan/nonaktifkan highlight (border kuning).</summary>
    public void SetSelected(bool selected)
    {
        if (selectedHighlight != null)
            selectedHighlight.SetActive(selected);
    }
}
