using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// ScriptableObject yang mendefinisikan urutan video sinematik beserta dialog-nya.
/// Buat instance baru via: klik kanan di Project → Heroes Game → Video Sequence
/// </summary>
[CreateAssetMenu(fileName = "VideoSequence_Baru", menuName = "Heroes Game/Video Sequence")]
public class VideoSequenceSO : ScriptableObject
{
    [System.Serializable]
    public class EntriVideo
    {
        [Header("Video")]
        [Tooltip("File video sinematik (.mp4 / .webm). Drag file video ke sini.")]
        public VideoClip videoClip;

        [Tooltip("True = video diulang terus sampai dialog selesai. False = putar sekali.")]
        public bool loop = true;

        [Header("Dialog")]
        [Tooltip("Data dialog yang muncul setelah video mulai diputar. Null = tidak ada dialog.")]
        public DialogueDataSO dialogSetelahMulai;

        [Header("Transisi")]
        [Tooltip("Durasi fade in / fade out layar dalam detik.")]
        public float durasiTalang = 1f;

        [Tooltip("Background music yang diputar bersamaan video ini. Null = tidak ada BGM.")]
        public AudioClip bgmVideo;
    }

    [Header("Daftar Video Sinematik")]
    [Tooltip("Video diputar secara berurutan dari atas ke bawah.")]
    public List<EntriVideo> daftarVideo = new List<EntriVideo>();
}
