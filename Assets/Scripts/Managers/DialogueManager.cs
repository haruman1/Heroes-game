using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// DialogueManager — Sistem dialog universal yang dapat dipakai di mana saja.
/// 
/// Fitur:
/// - Efek typewriter (berjalan di unscaled time agar bekerja saat pause)
/// - Kotak nama karakter
/// - Portrait karakter (kiri = player, kanan = NPC/Narrator)
/// - Voice over per baris via AudioManager
/// - Tombol Lanjut / Lewati
/// - Support Rich Text TMPro
/// - Support banyak speaker berbeda
/// - Event OnDialogSelesai
/// 
/// Cara pakai: DialogueManager.Instance.MulaiDialog(dialogueDataSO)
/// Singleton DontDestroyOnLoad.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Referensi UI Dialog")]
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Pengaturan Typewriter")]
    [Tooltip("Jeda (detik) antar karakter typewriter. Semakin kecil semakin cepat.")]
    [SerializeField] private float kecepatanKetik = 0.03f;

    [Header("Auto Play")]
    [Tooltip("Jika true, dialog lanjut otomatis setelah typewriter selesai.")]
    [SerializeField] private bool autoPlay = false;
    [SerializeField] private float jedaAutoPlay = 2f;

    [Header("Portrait Default Pemain")]
    [Tooltip("Portrait Awan (Laki-laki) yang dipakai jika tidak ada portrait di BarisDialog.")]
    [SerializeField] private Sprite portraitAwan;
    [Tooltip("Portrait Rena (Perempuan) yang dipakai jika tidak ada portrait di BarisDialog.")]
    [SerializeField] private Sprite portraitRena;

    [Header("Events")]
    public UnityEvent OnDialogSelesai;

    /// <summary>Event statis untuk didengarkan tanpa referensi instance (dipakai VideoManager, LevelManager).</summary>
    public static event Action OnDialogSelesaiStatic;

    // ─── State Internal ──────────────────────────────────────────────
    private DialogueDataSO  _dataSaatIni;
    private int             _indeksBaris;
    private Coroutine       _coroutineKetik;
    private bool            _sedangKetik;

    // Info pemain (dari save)
    private string _namaPemain = "Awan";
    private bool   _isMale     = true;

    // ─── Lifecycle ───────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─── Public API ──────────────────────────────────────────────────

    /// <summary>Mulai dialog dari DialogueDataSO.</summary>
    public void MulaiDialog(DialogueDataSO data)
    {
        if (data == null || data.barisDialog == null || data.barisDialog.Count == 0)
        {
            Debug.LogWarning("[DialogueManager] MulaiDialog dipanggil dengan data null/kosong.");
            SelesaikanDialog();
            return;
        }

        MuatInfoPemain();

        _dataSaatIni = data;
        _indeksBaris = 0;

        dialogueUI?.TampilkanPanel(true);
        TampilkanBaris(_indeksBaris);
    }

    /// <summary>Lanjut ke baris berikutnya. Jika sedang mengetik, langsung tampilkan seluruh teks.</summary>
    public void Lanjut()
    {
        if (_sedangKetik)
        {
            // Skip typewriter — langsung tampilkan teks penuh
            if (_coroutineKetik != null) StopCoroutine(_coroutineKetik);
            _sedangKetik = false;

            if (_dataSaatIni != null && _indeksBaris < _dataSaatIni.barisDialog.Count)
            {
                string teks = ProsesTeks(_dataSaatIni.barisDialog[_indeksBaris].teks);
                dialogueUI?.SetTeks(teks);
            }
            dialogueUI?.SetTombolLanjutInteraktif(true);
            return;
        }

        _indeksBaris++;
        if (_dataSaatIni != null && _indeksBaris < _dataSaatIni.barisDialog.Count)
        {
            TampilkanBaris(_indeksBaris);
        }
        else
        {
            SelesaikanDialog();
        }
    }

    /// <summary>Lewati seluruh dialog sekaligus.</summary>
    public void Lewati()
    {
        if (_coroutineKetik != null) StopCoroutine(_coroutineKetik);
        _sedangKetik = false;
        SelesaikanDialog();
    }

    /// <summary>Cek apakah dialog sedang berjalan.</summary>
    public bool SedangDialog => dialogueUI != null && dialogueUI.PanelAktif;

    // ─── Internal ────────────────────────────────────────────────────
    private void MuatInfoPemain()
    {
        PlayerData data = SaveManager.Instance?.MuatPlayerData();
        if (data != null && !string.IsNullOrEmpty(data.SelectedCharacter))
        {
            _namaPemain = data.SelectedCharacter;
            _isMale     = data.Gender != "Perempuan";
        }
        else
        {
            _namaPemain = "Awan";
            _isMale     = true;
        }
    }

    private void TampilkanBaris(int indeks)
    {
        if (_dataSaatIni == null || indeks < 0 || indeks >= _dataSaatIni.barisDialog.Count)
        {
            SelesaikanDialog();
            return;
        }

        DialogueDataSO.BarisDialog baris = _dataSaatIni.barisDialog[indeks];

        // Nama speaker
        string nama = string.IsNullOrEmpty(baris.namaSpeaker) ? _namaPemain : baris.namaSpeaker;
        dialogueUI?.SetNamaSpeaker(nama);

        // Portrait
        Sprite portrait = baris.portrait;
        if (portrait == null && string.IsNullOrEmpty(baris.namaSpeaker))
        {
            // Speaker adalah pemain → pakai portrait default
            portrait = _isMale ? portraitAwan : portraitRena;
        }
        dialogueUI?.SetPortrait(portrait, baris.sisiKanan);

        // Voice over
        if (baris.audioVoice != null)
        {
            AudioManager.Instance?.PutarVoiceOver(baris.audioVoice);
        }
        else
        {
            AudioManager.Instance?.HentikanVoiceOver();
        }

        // Typewriter
        string teks = ProsesTeks(baris.teks);
        if (_coroutineKetik != null) StopCoroutine(_coroutineKetik);
        _coroutineKetik = StartCoroutine(EfekKetik(teks));
    }

    private string ProsesTeks(string teks)
    {
        if (string.IsNullOrEmpty(teks)) return "";

        PlayerData data = SaveManager.Instance?.MuatPlayerData();
        string nama = data?.SelectedCharacter ?? _namaPemain;
        string umur = data != null && data.SelectedAge > 0 ? data.SelectedAge.ToString() : "18";

        return teks
            .Replace("{nama}",  nama)
            .Replace("{name}",  nama)
            .Replace("{umur}",  umur)
            .Replace("{age}",   umur)
            .Replace("{usia}",  umur);
    }

    private IEnumerator EfekKetik(string teks)
    {
        _sedangKetik = true;
        dialogueUI?.SetTombolLanjutInteraktif(false);
        dialogueUI?.SetTeks("");

        foreach (char c in teks)
        {
            dialogueUI?.TambahKarakter(c);
            yield return new WaitForSecondsRealtime(kecepatanKetik);
        }

        _sedangKetik = false;
        dialogueUI?.SetTombolLanjutInteraktif(true);

        if (autoPlay)
        {
            yield return new WaitForSecondsRealtime(jedaAutoPlay);
            Lanjut();
        }
    }

    private void SelesaikanDialog()
    {
        AudioManager.Instance?.HentikanVoiceOver();
        dialogueUI?.TampilkanPanel(false);
        _dataSaatIni = null;

        OnDialogSelesai?.Invoke();
        OnDialogSelesaiStatic?.Invoke();
    }
}
