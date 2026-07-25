using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// CharacterSelectController — Alur Pemilihan Karakter ala Metal Slug PS2.
/// 
/// ALUR GAMEPLAY (METAL SLUG ARCADE STYLE):
/// ────────────────────────────────────────────────────────────────────────
/// 1. LINEUP ROSTER (Pilih Karakter & Variasi Usia):
///    - Menampilkan jajaran 4 varian karakter (seperti di Arcade Metal Slug)
///    - Tombol Switch Gender: [ AWAN (Pria) ] | [ RENA (Wanita) ]
///    - Setiap kartu menampilkan Artwork Full, Nama, Usia & Identitas
/// 
/// 2. DETAIL LANJUTAN (Dossier / Summary Panel):
///    - Diklik kartu karakter → Muncul Panel Detail Lanjutan (Dossier Karakter)
///    - Menampilkan Artwork Besar, Identitas, Bio Latar Belakang, Moto & Statistik
///    - Tombol Action:
///      • [ MULAI PETUALANGAN ] → Simpan ke Database & Lanjut ke OpeningStory!
///      • [ KEMBALI ] → Tutup detail dan kembali ke lineup roster
/// </summary>
public class CharacterSelectController : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════════
    // DATA STRUKTUR VARIAN KARAKTER (INSPECTOR FRIENDLY)
    // ═══════════════════════════════════════════════════════════════════
    [System.Serializable]
    public class VarianKarakterData
    {
        [Header("Identitas Dasar")]
        public string namaKarakter = "Awan"; // "Awan" atau "Rena"
        public string gender = "Laki-laki";   // "Laki-laki" atau "Perempuan"
        public int umur = 21;                 // Umur spesifik (15, 21, 30, 50)
        public string labelKelompokUmur = "Dewasa Awal (18-24 Tahun)";
        public string identitasProfesi = "Junior IT Specialist";

        [Header("Visual & Artwork")]
        public Sprite spriteCardLineup;      // Visual full-body untuk lineup roster
        public Sprite spriteDetailLarge;     // Visual besar untuk panel Detail Lanjutan

        [Header("Detail Latar Belakang & Bio")]
        [TextArea(3, 6)]
        public string deskripsiCerita = "Sedang berjuang membangun karier di dunia teknologi...";

        [TextArea(2, 4)]
        public string kataMutiaraQuote = "\"Setiap langkah awal adalah keberanian besar.\"";

        [Header("Statistik Karakteristik (0 - 100)")]
        [Range(0, 100)] public int statFisik = 75;
        [Range(0, 100)] public int statFokus = 85;
        [Range(0, 100)] public int statPengalaman = 60;
    }

    // ═══════════════════════════════════════════════════════════════════
    // SECTION 1: DAFTAR VARIAN KARAKTER (4 AWAN + 4 RENA)
    // ═══════════════════════════════════════════════════════════════════
    [Header("── List Varian Karakter Awan (Pria) ─────────────────")]
    [SerializeField] private List<VarianKarakterData> varianAwan = new List<VarianKarakterData>();

    [Header("── List Varian Karakter Rena (Wanita) ───────────────")]
    [SerializeField] private List<VarianKarakterData> varianRena = new List<VarianKarakterData>();

    // ═══════════════════════════════════════════════════════════════════
    // SECTION 2: UI PANEL ROSTER LINEUP (SCREEN 1)
    // ═══════════════════════════════════════════════════════════════════
    [Header("── Panel Roster Lineup (Screen 1) ───────────────────")]
    [SerializeField] private GameObject panelRosterLineup;

    [Header("Tombol Switch Gender")]
    [SerializeField] private Button tombolSwitchAwan;
    [SerializeField] private Button tombolSwitchRena;
    [SerializeField] private GameObject highlightSwitchAwan;
    [SerializeField] private GameObject highlightSwitchRena;

    [Header("4 Slot Kartu Karakter di Lineup")]
    [SerializeField] private Button[] slotKartuButtons = new Button[4];
    [SerializeField] private Image[] slotKartuImages = new Image[4];
    [SerializeField] private TMP_Text[] slotKartuNamaTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text[] slotKartuIdentitasTexts = new TMP_Text[4];
    [SerializeField] private GameObject[] slotKartuGlowHighlights = new GameObject[4];

    // ═══════════════════════════════════════════════════════════════════
    // SECTION 3: UI PANEL DETAIL LANJUTAN DOSSIER (SCREEN 2)
    // ═══════════════════════════════════════════════════════════════════
    [Header("── Panel Detail Lanjutan / Dossier (Screen 2) ───────")]
    [SerializeField] private GameObject panelDetailLanjutan;

    [Header("Komponen Visual Detail")]
    [SerializeField] private Image imageDetailLargeArtwork;
    [SerializeField] private TMP_Text teksDetailNama;
    [SerializeField] private TMP_Text teksDetailKelompokUmur;
    [SerializeField] private TMP_Text teksDetailIdentitasProfesi;
    [SerializeField] private TMP_Text teksDetailDeskripsiCerita;


    [Header("Statistik Sliders / Progress Bars")]
    [SerializeField] private Slider sliderStatFisik;
    [SerializeField] private TMP_Text teksAngkaStatFisik;

    [SerializeField] private Slider sliderStatFokus;
    [SerializeField] private TMP_Text teksAngkaStatFokus;

    [SerializeField] private Slider sliderStatPengalaman;
    [SerializeField] private TMP_Text teksAngkaStatPengalaman;

    [Header("Tombol Action Detail")]
    [SerializeField] private Button tombolMulaiPetualangan;
    [SerializeField] private Button tombolBatalDetail;

    // ═══════════════════════════════════════════════════════════════════
    // SECTION 4: AUDIO SFX & NAVIGASI UPTOP
    // ═══════════════════════════════════════════════════════════════════
    [Header("── SFX & Audio Arcade ───────────────────────────────")]
    [SerializeField] private AudioClip sfxHoverCard;
    [SerializeField] private AudioClip sfxSelectCard;
    [SerializeField] private AudioClip sfxConfirmStart;
    [SerializeField] private AudioClip sfxBack;

    [Header("── Tombol Navigasi Top Bar ──────────────────────────")]
    [SerializeField] private Button tombolKembaliMainMenu;

    // ═══════════════════════════════════════════════════════════════════
    // STATE INTERNAL
    // ═══════════════════════════════════════════════════════════════════
    private string _genderTerpilih = "Laki-laki"; // "Laki-laki" atau "Perempuan"
    private VarianKarakterData _varianTerpilih = null;

    // ═══════════════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════════════════
    private void Start()
    {
        // ── 1. Setup Tombol Switch Gender ──
        tombolSwitchAwan?.onClick.AddListener(() => SwitchGender("Laki-laki"));
        tombolSwitchRena?.onClick.AddListener(() => SwitchGender("Perempuan"));

        // ── 2. Setup Listener Slot Kartu Lineup ──
        for (int i = 0; i < slotKartuButtons.Length; i++)
        {
            int index = i; // capture index
            if (slotKartuButtons[i] != null)
            {
                slotKartuButtons[i].onClick.AddListener(() => OnKlikKartuKarakter(index));
            }
        }

        // ── 3. Setup Listener Panel Detail Lanjutan ──
        tombolMulaiPetualangan?.onClick.AddListener(OnMulaiPetualangan);
        tombolBatalDetail?.onClick.AddListener(TutupDetailLanjutan);
        tombolKembaliMainMenu?.onClick.AddListener(OnKembaliToMainMenu);

        // ── 4. Inisialisasi Tampilan ──
        TutupDetailLanjutan();
        SwitchGender("Laki-laki");

        // Sembunyikan HUD In-Game
        UIManager.Instance?.TampilkanHUD(false);
    }

    // ═══════════════════════════════════════════════════════════════════
    // SWITCH GENDER (Awan <-> Rena)
    // ═══════════════════════════════════════════════════════════════════
    private void SwitchGender(string gender)
    {
        _genderTerpilih = gender;
        bool isAwan = (gender == "Laki-laki");

        PlaySFX(sfxHoverCard);

        // Highlight Tab Header
        if (highlightSwitchAwan != null) highlightSwitchAwan.SetActive(isAwan);
        if (highlightSwitchRena != null) highlightSwitchRena.SetActive(!isAwan);

        // Refresh 4 Kartu di Roster Lineup
        List<VarianKarakterData> listVarian = isAwan ? varianAwan : varianRena;

        for (int i = 0; i < slotKartuButtons.Length; i++)
        {
            if (i < listVarian.Count)
            {
                VarianKarakterData data = listVarian[i];
                if (slotKartuButtons[i] != null) slotKartuButtons[i].gameObject.SetActive(true);

                if (slotKartuImages[i] != null && data.spriteCardLineup != null)
                {
                    slotKartuImages[i].sprite = data.spriteCardLineup;
                }

                if (slotKartuNamaTexts[i] != null)
                {
                    slotKartuNamaTexts[i].text = data.namaKarakter;
                }

                if (slotKartuIdentitasTexts[i] != null)
                {
                    slotKartuIdentitasTexts[i].text = $"{data.labelKelompokUmur}\n<color=#FFD700>{data.identitasProfesi}</color>";
                }

                if (slotKartuGlowHighlights[i] != null)
                {
                    slotKartuGlowHighlights[i].SetActive(false);
                }
            }
            else
            {
                if (slotKartuButtons[i] != null) slotKartuButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // KLIK KARTU KARAKTER -> Buka Detail Lanjutan (Dossier)
    // ═══════════════════════════════════════════════════════════════════
    private void OnKlikKartuKarakter(int index)
    {
        List<VarianKarakterData> listVarian = (_genderTerpilih == "Laki-laki") ? varianAwan : varianRena;

        if (index < 0 || index >= listVarian.Count) return;

        _varianTerpilih = listVarian[index];
        PlaySFX(sfxSelectCard);

        // Visual Highlight Kartu yang Diklik
        for (int i = 0; i < slotKartuGlowHighlights.Length; i++)
        {
            if (slotKartuGlowHighlights[i] != null)
            {
                slotKartuGlowHighlights[i].SetActive(i == index);
            }
        }

        // Tampilkan Panel Detail Lanjutan
        BukaDetailLanjutan(_varianTerpilih);
    }

    // ═══════════════════════════════════════════════════════════════════
    // BUAT DISPLAY DETAIL LANJUTAN
    // ═══════════════════════════════════════════════════════════════════
    private void BukaDetailLanjutan(VarianKarakterData data)
    {
        if (data == null) return;

        if (panelDetailLanjutan != null) panelDetailLanjutan.SetActive(true);

        // Isi Artwork
        if (imageDetailLargeArtwork != null)
        {
            imageDetailLargeArtwork.sprite = data.spriteDetailLarge != null ? data.spriteDetailLarge : data.spriteCardLineup;
        }

        // Isi Informasi Teks
        if (teksDetailNama != null) teksDetailNama.text = data.namaKarakter;
        if (teksDetailKelompokUmur != null) teksDetailKelompokUmur.text = data.labelKelompokUmur;
        if (teksDetailIdentitasProfesi != null) teksDetailIdentitasProfesi.text = data.identitasProfesi;
        if (teksDetailDeskripsiCerita != null) teksDetailDeskripsiCerita.text = data.deskripsiCerita;
      

        // Isi Progress Bar Statistik
        if (sliderStatFisik != null) sliderStatFisik.value = data.statFisik / 100f;
        if (teksAngkaStatFisik != null) teksAngkaStatFisik.text = data.statFisik.ToString();

        if (sliderStatFokus != null) sliderStatFokus.value = data.statFokus / 100f;
        if (teksAngkaStatFokus != null) teksAngkaStatFokus.text = data.statFokus.ToString();

        if (sliderStatPengalaman != null) sliderStatPengalaman.value = data.statPengalaman / 100f;
        if (teksAngkaStatPengalaman != null) teksAngkaStatPengalaman.text = data.statPengalaman.ToString();
    }

    private void TutupDetailLanjutan()
    {
        PlaySFX(sfxBack);
        if (panelDetailLanjutan != null) panelDetailLanjutan.SetActive(false);

        // Matikansemua glow highlight di lineup
        for (int i = 0; i < slotKartuGlowHighlights.Length; i++)
        {
            if (slotKartuGlowHighlights[i] != null) slotKartuGlowHighlights[i].SetActive(false);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // TOMBOL MULAI PETUALANGAN (CONFIRM -> OPENING STORY)
    // ═══════════════════════════════════════════════════════════════════
    private void OnMulaiPetualangan()
    {
        if (_varianTerpilih == null) return;

        PlaySFX(sfxConfirmStart);

        // Simpan Data Karakter, Gender, & Umur Terpilih ke SaveManager Database
        SaveManager.Instance?.SimpanGenderDanUmur(
            _varianTerpilih.gender,
            _varianTerpilih.namaKarakter,
            _varianTerpilih.umur
        );

        Debug.Log($"[Metal Slug Select] CONFIRM: {_varianTerpilih.namaKarakter} ({_varianTerpilih.gender}), " +
                  $"Umur: {_varianTerpilih.umur} ({_varianTerpilih.labelKelompokUmur})");

        // Transisi Layar & Muat Opening Story Scene
        StartCoroutine(TransisiKeOpeningStory());
    }

    private IEnumerator TransisiKeOpeningStory()
    {
        // Fade Layar Gelap
        if (UIManager.Instance != null)
        {
            yield return StartCoroutine(UIManager.Instance.FadeLayar(0.8f, true));
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        // Panggil GameManager untuk muat Opening Story
        GameManager.Instance?.MuatOpeningStory();
    }

    private void OnKembaliToMainMenu()
    {
        PlaySFX(sfxBack);
        GameManager.Instance?.MuatMainMenu();
    }

    // ═══════════════════════════════════════════════════════════════════
    // AUDIO SFX HELPER
    // ═══════════════════════════════════════════════════════════════════
    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PutarSFX(clip);
        }
    }
}
