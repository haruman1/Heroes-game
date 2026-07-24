using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// VideoManager — Mengontrol Unity VideoPlayer untuk scene sinematik.
/// 
/// Tanggung jawab:
/// - Putar / Hentikan video (VideoClip)
/// - Fade In / Fade Out layar via UIManager
/// - Merangkai video dari VideoSequenceSO secara berurutan
/// - Trigger DialogueManager setelah video mulai
/// - Trigger voice-over via AudioManager
/// 
/// Singleton DontDestroyOnLoad.
/// Taruh pada GameObject "VideoManager" beserta komponen VideoPlayer.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class VideoManager : MonoBehaviour
{
    public static VideoManager Instance { get; private set; }

    [Header("Komponen Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [Tooltip("RawImage di Canvas untuk menampilkan output video. Harus full screen.")]
    [SerializeField] private UnityEngine.UI.RawImage rawImageVideo;

    [Header("Pengaturan Default")]
    [SerializeField] private float durasiTalangDefault = 1f;

    // ─── Events ──────────────────────────────────────────────────────
    public static event Action OnVideoMulai;
    public static event Action OnVideoSelesai;
    public static event Action OnRangkaianSelesai;

    // ─── State Internal ──────────────────────────────────────────────
    private VideoSequenceSO _sequenceSaatIni;
    private int             _indeksVideo;
    private bool            _sedangRangkaian;

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

        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // Sembunyikan raw image saat awal
        if (rawImageVideo != null) rawImageVideo.gameObject.SetActive(false);
    }

    // ─── Public API ──────────────────────────────────────────────────

    /// <summary>Mulai memutar rangkaian video dari VideoSequenceSO.</summary>
    public void MulaiRangkaian(VideoSequenceSO sequence)
    {
        if (sequence == null || sequence.daftarVideo == null || sequence.daftarVideo.Count == 0)
        {
            Debug.LogWarning("[VideoManager] MulaiRangkaian: sequence null atau kosong.");
            OnRangkaianSelesai?.Invoke();
            return;
        }

        if (_sedangRangkaian) HentikanSemua();

        _sequenceSaatIni = sequence;
        _indeksVideo     = 0;
        _sedangRangkaian = true;

        StartCoroutine(PutarEntriVideo(_indeksVideo));
    }

    /// <summary>Hentikan semua coroutine dan video.</summary>
    public void HentikanSemua()
    {
        StopAllCoroutines();
        if (videoPlayer != null) videoPlayer.Stop();
        if (rawImageVideo != null) rawImageVideo.gameObject.SetActive(false);
        _sedangRangkaian = false;
    }

    // ─── Coroutine Inti ──────────────────────────────────────────────
    private IEnumerator PutarEntriVideo(int indeks)
    {
        // Selesai semua video?
        if (_sequenceSaatIni == null || indeks >= _sequenceSaatIni.daftarVideo.Count)
        {
            _sedangRangkaian = false;
            OnRangkaianSelesai?.Invoke();
            yield break;
        }

        VideoSequenceSO.EntriVideo entri = _sequenceSaatIni.daftarVideo[indeks];
        float talang = entri.durasiTalang > 0f ? entri.durasiTalang : durasiTalangDefault;

        // ── Fade IN ──
        if (UIManager.Instance != null)
            yield return StartCoroutine(UIManager.Instance.FadeLayar(talang, true));

        // ── Siapkan & putar video ──
        if (entri.videoClip != null)
        {
            videoPlayer.clip      = entri.videoClip;
            videoPlayer.isLooping = entri.loop;
            videoPlayer.Prepare();

            // Tunggu video siap
            yield return new WaitUntil(() => videoPlayer.isPrepared);
            videoPlayer.Play();
        }

        if (rawImageVideo != null) rawImageVideo.gameObject.SetActive(true);
        OnVideoMulai?.Invoke();

        // ── BGM Video ──
        if (entri.bgmVideo != null)
            AudioManager.Instance?.PutarBGM(entri.bgmVideo);

        // ── Dialog di atas video ──
        if (entri.dialogSetelahMulai != null)
        {
            yield return null; // Satu frame jeda

            bool dialogSelesai = false;
            Action tandaiSelesai = () => dialogSelesai = true;
            DialogueManager.OnDialogSelesaiStatic += tandaiSelesai;

            DialogueManager.Instance?.MulaiDialog(entri.dialogSetelahMulai);

            yield return new WaitUntil(() => dialogSelesai);
            DialogueManager.OnDialogSelesaiStatic -= tandaiSelesai;
        }
        else
        {
            // Tanpa dialog → tunggu video selesai (jika tidak loop)
            if (!entri.loop && videoPlayer.clip != null)
            {
                yield return new WaitUntil(() => !videoPlayer.isPlaying);
            }
        }

        // ── Fade OUT ──
        if (UIManager.Instance != null)
            yield return StartCoroutine(UIManager.Instance.FadeLayar(talang, false));

        // ── Bersihkan ──
        videoPlayer.Stop();
        if (rawImageVideo != null) rawImageVideo.gameObject.SetActive(false);
        AudioManager.Instance?.HentikanBGM();

        OnVideoSelesai?.Invoke();

        // ── Video berikutnya ──
        _indeksVideo++;
        StartCoroutine(PutarEntriVideo(_indeksVideo));
    }
}
