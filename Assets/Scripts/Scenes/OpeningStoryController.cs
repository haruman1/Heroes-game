using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Struct untuk menyimpan data satu baris dialog (Teks, Speaker, Voice Audio, & Sisi Portrait).
/// </summary>
[System.Serializable]
public struct StoryLine
{
    [Tooltip("Nama pembicara. Kosongkan untuk otomatis memakai nama pemain (Awan/Rena).")]
    public string speakerName;

    [Tooltip("Teks isi dialog.")]
    [TextArea(2, 5)]
    public string dialogueText;

    [Tooltip("Audio Voice Over / Dubbing untuk baris dialog ini.")]
    public AudioClip voiceAudio;

    [Tooltip("True = Portrait di kanan (KP/NPC). False = Portrait di kiri (Pemain).")]
    public bool isRightSide;
}

/// <summary>
/// OpeningStoryController — Controller untuk Scene Opening Story & Prolog Cerita Utama.
/// 
/// Alur Lengkap:
/// 1. Memutar Babak 1: Rangkaian Dialog Awal (Narasi, Monolog KU, & Percakapan KU dengan KP Penjaga Perpustakaan).
/// 2. Menampilkan Pop-Up Pilihan Interaktif: "APAKAH KAMU INGIN MEMULAI PERJALANAN?" [Ya / Tidak].
/// 3. Jika Pemain Memilih "TIDAK":
///    - Pop-Up disembunyikan.
///    - Memutar Babak 3A: Dialog KU ("Aku belum yakin.") & KP ("Tidak apa-apa...").
///    - Setelah dialog selesai, Pop-Up Pilihan [Ya / Tidak] TAMPIL KEMBALI.
/// 4. Jika Pemain Memilih "YA":
///    - Pop-Up disembunyikan.
///    - Menyimpan 3 Item Booster Vitamin ke Database SQLite.
///    - Memutar Babak 3B: Dialog KP ("Setiap halaman yang kamu temukan...").
///    - Setelah dialog selesai, game berpindah ke Scene Peta Perjalanan ("Pilih Maps").
/// </summary>
public class OpeningStoryController : MonoBehaviour
{
    [Header("Babak 1: Rangkaian Dialog Awal (Sebelum Pilihan)")]
    [Tooltip("Daftar dialog percakapan KU & KP sebelum muncul pilihan.")]
    [SerializeField]
    private List<StoryLine> dialogAwalOpening = new List<StoryLine>()
    {
        new StoryLine { speakerName = "", dialogueText = "Setiap orang memiliki perjalanan hidupnya masing-masing. Tidak semua perjalanan berjalan dengan mudah. Namun selalu ada kesempatan untuk terus melangkah.", isRightSide = false },
        new StoryLine { speakerName = "", dialogueText = "Sejak mengetahui kondisiku… Aku masih berusaha menjalani hidup seperti biasa... Tapi kadang, aku masih bingung mulai dari mana. Aku ingin belajar menerima ini mengenai bagaimana menjaga diriku.", isRightSide = false },
        new StoryLine { speakerName = "", dialogueText = "Halo permisi, apa ada orang di dalam?", isRightSide = false },
        new StoryLine { speakerName = "Penjaga Perpustakaan", dialogueText = "Halo, selamat datang.", isRightSide = true },
        new StoryLine { speakerName = "", dialogueText = "Aku dengar tempat ini menyimpan banyak pengetahuan.", isRightSide = false },
        new StoryLine { speakerName = "Penjaga Perpustakaan", dialogueText = "Yaa itu dulu.. (tersenyum sedih)", isRightSide = true },
        new StoryLine { speakerName = "", dialogueText = "Dulu? Lalu bagaimana dengan sekarang?", isRightSide = false },
        new StoryLine { speakerName = "Penjaga Perpustakaan", dialogueText = "Sudah kosong.", isRightSide = true },
        new StoryLine { speakerName = "", dialogueText = "Kemana semua isi buku itu?", isRightSide = false },
        new StoryLine { speakerName = "Penjaga Perpustakaan", dialogueText = "Hilang.. sejak makhluk itu mencuri semua halaman-halaman buku ini dan sekarang tinggal tersisa sampulnya. Padahal buku ini membantu banyak orang untuk memahami cara menjaga kualitas hidupnya.", isRightSide = true },
        new StoryLine { speakerName = "", dialogueText = "Karna dicuri makhluk itu isi pengetahuannya juga hilang?", isRightSide = false },
        new StoryLine { speakerName = "Penjaga Perpustakaan", dialogueText = "Iyaa..", isRightSide = true },
        new StoryLine { speakerName = "", dialogueText = "Kalau aku menemukan halaman-halaman yang hilang itu, apakah bukunya kembali utuh dan aku bisa ikut membaca dan mempelajarinya?", isRightSide = false },
        new StoryLine { speakerName = "Penjaga Perpustakaan", dialogueText = "Yaa tentu, namun tidak semua halaman akan datang sekaligus, halaman itu ditemukan sedikit demi sedikit.", isRightSide = true }
    };

    [Header("UI Pop-up Pilihan Interaktif")]
    [Tooltip("Panel UI Pop-up Pilihan 'Apakah Kamu Ingin Memulai Perjalanan?'")]
    [SerializeField] private GameObject choicePanelUI;
    [SerializeField] private TMP_Text choicePromptText;
    [SerializeField] private Button buttonYa;
    [SerializeField] private Button buttonTidak;

    [Header("Babak 3A: Dialog Jika Pemain Memilih TIDAK")]
    [Tooltip("Daftar dialog jika pemain mengklik TIDAK.")]
    [SerializeField]
    private List<StoryLine> dialogPilihTidak = new List<StoryLine>()
    {
        new StoryLine { speakerName = "", dialogueText = "Aku belum yakin.", isRightSide = false },
        new StoryLine { speakerName = "Penjaga Perpustakaan", dialogueText = "Tidak apa-apa, semua perjalanan dimulai saat seseorang siap melangkah.", isRightSide = true }
    };

    [Header("Babak 3B: Dialog Jika Pemain Memilih YA")]
    [Tooltip("Daftar dialog jika pemain mengklik YA.")]
    [SerializeField]
    private List<StoryLine> dialogPilihYa = new List<StoryLine>()
    {
        new StoryLine { speakerName = "Penjaga Perpustakaan", dialogueText = "Setiap halaman yang kamu temukan, akan tersimpan di sini, gunakan ini (item) jika di perjalanan terasa berat.", isRightSide = true }
    };

    [Header("Navigasi Scene")]
    [Tooltip("Nama scene Peta Perjalanan yang dimuat setelah memilih YA.")]
    [SerializeField] private string namaScenePetaPerjalanan = "Pilih Maps";
    [Tooltip("Nama scene level pertama jika Peta Perjalanan dilewati.")]
    [SerializeField] private string namaSceneLevelPertama = "LEVEL 1";

    [Header("Reward Booster Awal (Vitamin Kesehatan)")]
    [Tooltip("Jumlah item booster vitamin kesehatan yang diberikan di awal perjalanan.")]
    [SerializeField] private int jumlahBoosterAwal = 3;

    // ─── Lifecycle ───────────────────────────────────────────────────
    private void Start()
    {
        UIManager.Instance?.TampilkanHUD(false);

        if (choicePanelUI != null) choicePanelUI.SetActive(false);

        if (buttonYa != null) buttonYa.onClick.AddListener(OnPilihYa);
        if (buttonTidak != null) buttonTidak.onClick.AddListener(OnPilihTidak);

        MulaiBabak1();
    }

    // ─── BABAK 1: Dialog Awal ─────────────────────────────────────────
    private void MulaiBabak1()
    {
        if (dialogAwalOpening != null && dialogAwalOpening.Count > 0)
        {
            PutarRangkaianStoryLine(dialogAwalOpening, TampilkanPilihanPerjalanan);
        }
        else
        {
            TampilkanPilihanPerjalanan();
        }
    }

    // ─── BABAK 2: Pilihan Interaktif ─────────────────────────────────
    public void TampilkanPilihanPerjalanan()
    {
        if (choicePanelUI != null)
        {
            choicePanelUI.SetActive(true);
            if (choicePromptText != null) 
                choicePromptText.text = "APAKAH KAMU INGIN MEMULAI PERJALANAN?";
        }
        else
        {
            // Fallback jika UI Pilihan belum dipasang di Inspector
            OnPilihYa();
        }
    }

    // ─── BABAK 3B: Pilih YA ──────────────────────────────────────────
    private void OnPilihYa()
    {
        if (choicePanelUI != null) choicePanelUI.SetActive(false);

        // 1. Berikan Item Booster Vitamin (Kesehatan & Kualitas Hidup)
        SaveManager.Instance?.SimpanBooster(jumlahBoosterAwal);
        Debug.Log($"[OpeningStory] Pemain memilih YA! Menerima Tas & {jumlahBoosterAwal} Booster Vitamin.");

        // 2. Putar Dialog Babak 3B dan pindah ke Peta Perjalanan saat selesai
        if (dialogPilihYa != null && dialogPilihYa.Count > 0)
        {
            PutarRangkaianStoryLine(dialogPilihYa, LanjutKePetaPerjalanan);
        }
        else
        {
            LanjutKePetaPerjalanan();
        }
    }

    // ─── BABAK 3A: Pilih TIDAK ───────────────────────────────────────
    private void OnPilihTidak()
    {
        if (choicePanelUI != null) choicePanelUI.SetActive(false);

        Debug.Log("[OpeningStory] Pemain memilih TIDAK ('Aku belum yakin...').");

        // Putar Dialog Babak 3A dan TAMPILKAN POP-UP PILIHAN KEMBALI saat selesai
        if (dialogPilihTidak != null && dialogPilihTidak.Count > 0)
        {
            PutarRangkaianStoryLine(dialogPilihTidak, TampilkanPilihanPerjalanan);
        }
        else
        {
            TampilkanPilihanPerjalanan();
        }
    }

    // ─── Helper: Konversi StoryLine ke DialogueDataSO & Putar ────────
    private void PutarRangkaianStoryLine(List<StoryLine> storyLines, Action onSelesaiCallback)
    {
        if (DialogueManager.Instance == null || storyLines == null || storyLines.Count == 0)
        {
            onSelesaiCallback?.Invoke();
            return;
        }

        // Buat DialogueDataSO temporer di memory
        DialogueDataSO dataSO = ScriptableObject.CreateInstance<DialogueDataSO>();
        dataSO.barisDialog = new List<DialogueDataSO.BarisDialog>();

        foreach (StoryLine line in storyLines)
        {
            dataSO.barisDialog.Add(new DialogueDataSO.BarisDialog
            {
                namaSpeaker = line.speakerName,
                teks = line.dialogueText,
                audioVoice = line.voiceAudio,
                sisiKanan = line.isRightSide
            });
        }

        // Daftarkan event callback saat dialog selesai
        Action onDone = null;
        onDone = () =>
        {
            DialogueManager.OnDialogSelesaiStatic -= onDone;
            onSelesaiCallback?.Invoke();
        };

        DialogueManager.OnDialogSelesaiStatic += onDone;
        DialogueManager.Instance.MulaiDialog(dataSO);
    }

    // ─── Navigasi Peta Perjalanan ────────────────────────────────────
    private void LanjutKePetaPerjalanan()
    {
        Debug.Log("[OpeningStory] Dialog selesai. Berpindah ke Peta Perjalanan.");
        
        string targetScene = !string.IsNullOrEmpty(namaScenePetaPerjalanan) 
            ? namaScenePetaPerjalanan 
            : namaSceneLevelPertama;

        SaveManager.Instance?.SimpanNamaSceneTerakhir(targetScene);
        GameManager.Instance?.SetGameplayState();
        GameManager.Instance?.MuatScene(targetScene);
    }
}
