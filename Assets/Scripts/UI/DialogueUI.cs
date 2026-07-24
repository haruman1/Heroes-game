using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// DialogueUI — Komponen View pada Prefab Canvas Dialog.
/// Menyediakan tampilan untuk DialogueManager (Controller).
/// 
/// Cara setup di Unity:
/// 1. Buat Canvas (Screen Space - Overlay, Sort Order tinggi agar di depan semua UI)
/// 2. Di dalam Canvas buat structure:
///    DialoguePanel
///    ├── PanelBackground (Image)
///    ├── NamaBox (Image background nama)
///    │   └── TeksNama (TMP_Text)
///    ├── TeksDialog (TMP_Text, area besar)
///    ├── PortraitKiri (Image — untuk player)
///    ├── PortraitKanan (Image — untuk NPC/Narrator)
///    ├── TombolLanjut (Button + TMP_Text "Lanjut ▶")
///    └── TombolLewati (Button + TMP_Text "Lewati")
/// 3. Assign semua referensi ke Inspector DialogueUI.
/// 4. Assign komponen DialogueUI ke field dialogueUI di DialogueManager.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Panel Utama")]
    [SerializeField] private GameObject panelDialog;

    [Header("Teks")]
    [SerializeField] private TMP_Text teksNamaSpeaker;
    [SerializeField] private TMP_Text teksDialog;

    [Header("Portrait Karakter")]
    [Tooltip("Image untuk portrait di sisi KIRI layar (biasanya player).")]
    [SerializeField] private Image portraitKiri;
    [Tooltip("Image untuk portrait di sisi KANAN layar (biasanya NPC/Narrator).")]
    [SerializeField] private Image portraitKanan;

    [Header("Tombol")]
    [SerializeField] private Button tombolLanjut;
    [SerializeField] private Button tombolLewati;
    [SerializeField] private TMP_Text teksTombolLanjut;

    [Header("Teks Label Tombol")]
    [SerializeField] private string labelLanjut  = "Lanjut ▶";
    [SerializeField] private string labelSelesai = "Selesai ✓";

    // ─── Lifecycle ───────────────────────────────────────────────────
    private void Start()
    {
        tombolLanjut?.onClick.AddListener(OnKlikLanjut);
        tombolLewati?.onClick.AddListener(OnKlikLewati);

        // Sembunyikan portrait di awal
        SembunyikanSemuaPortrait();
    }

    // ─── Public API (dipanggil oleh DialogueManager) ─────────────────

    public bool PanelAktif => panelDialog != null && panelDialog.activeSelf;

    public void TampilkanPanel(bool aktif)
    {
        if (panelDialog != null) panelDialog.SetActive(aktif);

        if (!aktif)
        {
            SembunyikanSemuaPortrait();
            // Reset label tombol
            if (teksTombolLanjut != null) teksTombolLanjut.text = labelLanjut;
        }
    }

    public void SetNamaSpeaker(string nama)
    {
        if (teksNamaSpeaker != null) teksNamaSpeaker.text = nama;
    }

    public void SetTeks(string teks)
    {
        if (teksDialog != null) teksDialog.text = teks;
    }

    public void TambahKarakter(char c)
    {
        if (teksDialog != null) teksDialog.text += c;
    }

    public void SetPortrait(Sprite sprite, bool sisiKanan)
    {
        SembunyikanSemuaPortrait();

        if (sprite == null) return;

        if (sisiKanan && portraitKanan != null)
        {
            portraitKanan.gameObject.SetActive(true);
            portraitKanan.sprite = sprite;
        }
        else if (!sisiKanan && portraitKiri != null)
        {
            portraitKiri.gameObject.SetActive(true);
            portraitKiri.sprite = sprite;
        }
    }

    public void SetTombolLanjutInteraktif(bool aktif)
    {
        if (tombolLanjut != null) tombolLanjut.interactable = aktif;
    }

    public void SetLabelTombolSelesai()
    {
        if (teksTombolLanjut != null) teksTombolLanjut.text = labelSelesai;
    }

    // ─── Private ─────────────────────────────────────────────────────
    private void SembunyikanSemuaPortrait()
    {
        if (portraitKiri  != null) portraitKiri.gameObject.SetActive(false);
        if (portraitKanan != null) portraitKanan.gameObject.SetActive(false);
    }

    private void OnKlikLanjut() => DialogueManager.Instance?.Lanjut();
    private void OnKlikLewati() => DialogueManager.Instance?.Lewati();
}
