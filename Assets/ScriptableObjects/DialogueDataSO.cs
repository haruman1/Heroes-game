using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject yang menyimpan urutan baris dialog.
/// Buat instance baru via: klik kanan di Project → Heroes Game → Dialogue Data
/// </summary>
[CreateAssetMenu(fileName = "DialogueData_Baru", menuName = "Heroes Game/Dialogue Data")]
public class DialogueDataSO : ScriptableObject
{
    [System.Serializable]
    public class BarisDialog
    {
        [Tooltip("Nama yang tampil di kotak nama. Kosongkan untuk otomatis memakai nama pemain.")]
        public string namaSpeaker = "";

        [Tooltip("Teks dialog. Mendukung Rich Text TMPro: <b>bold</b>, <color=red>merah</color>, dll.")]
        [TextArea(2, 5)]
        public string teks = "";

        [Tooltip("Portrait karakter. Null = otomatis pakai portrait pemain (Awan/Rena).")]
        public Sprite portrait;

        [Tooltip("Audio voice over untuk baris ini. Opsional — jika kosong tidak ada suara.")]
        public AudioClip audioVoice;

        [Tooltip("True = portrait tampil di sisi kanan layar (biasanya NPC/Narrator). False = kiri (biasanya pemain).")]
        public bool sisiKanan = false;
    }

    [Header("Baris-Baris Dialog")]
    public List<BarisDialog> barisDialog = new List<BarisDialog>();
}
