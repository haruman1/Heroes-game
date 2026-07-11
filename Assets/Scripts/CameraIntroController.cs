using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public enum AutoTargetType
{
    None,
    Book,
    Flag
}

[System.Serializable]
public struct IntroShot
{
    [Tooltip("Nama shot untuk mempermudah identifikasi di Inspector.")]
    public string name;

    [Tooltip("Kamera Cinemachine untuk shot ini.")]
    public CinemachineCamera camera;

    [Tooltip("Pilih tipe target otomatis jika ingin mencari otomatis di scene.")]
    public AutoTargetType autoTargetType;

    [Tooltip("Tarik Transform target secara manual (mengabaikan pencarian otomatis jika diisi).")]
    public Transform manualTarget;

    [Tooltip("Durasi shot ini aktif (dalam detik).")]
    public float duration;
}

public class CameraIntroController : MonoBehaviour
{
    [Header("Sequence Settings")]
    [Tooltip("Daftar kamera intro yang akan dimainkan secara berurutan.")]
    public List<IntroShot> introShots = new List<IntroShot>();

    [Header("Gameplay Camera")]
    [Tooltip("Kamera utama gameplay yang menyorot player.")]
    public CinemachineCamera playerCamera;

    [Header("Player Control")]
    [Tooltip("Apakah kontrol player akan dimatikan selama kamera intro bermain.")]
    public bool disablePlayerInputDuringIntro = true;

    void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        playerJ player = FindFirstObjectByType<playerJ>();
        if (player != null && disablePlayerInputDuringIntro)
        {
            player.inputEnabled = false;
        }

        // Tunggu satu frame agar Cinemachine selesai menginisialisasi kamera
        yield return null;

        // Resolve semua target otomatis terlebih dahulu
        for (int i = 0; i < introShots.Count; i++)
        {
            IntroShot shot = introShots[i];
            if (shot.camera == null) continue;

            Transform targetTransform = shot.manualTarget;

            if (targetTransform == null)
            {
                if (shot.autoTargetType == AutoTargetType.Book)
                {
                    BookCollectible book = FindFirstObjectByType<BookCollectible>();
                    if (book != null)
                    {
                        targetTransform = book.transform;
                    }
                }
                else if (shot.autoTargetType == AutoTargetType.Flag)
                {
                    Flag flag = FindFirstObjectByType<Flag>();
                    if (flag != null)
                    {
                        targetTransform = flag.transform;
                    }
                }
            }

            if (targetTransform != null)
            {
                shot.camera.Follow = targetTransform;
                shot.camera.LookAt = targetTransform;
            }
        }

        // Mainkan sequence
        foreach (var shot in introShots)
        {
            if (shot.camera == null) continue;

            Debug.Log($"[CameraIntroController] Memainkan shot: {shot.name} selama {shot.duration} detik.");
            CameraManager.SwitchCamera(shot.camera);

            // Gunakan WaitForSecondsRealtime agar tetap berfungsi meskipun game di-pause (Time.timeScale = 0)
            yield return new WaitForSecondsRealtime(shot.duration);
        }

        // Kembalikan ke kamera player
        if (playerCamera != null)
        {
            Debug.Log("[CameraIntroController] Intro selesai, mengembalikan kamera ke player.");
            CameraManager.SwitchCamera(playerCamera);
        }

        // Aktifkan kembali kontrol player
        if (player != null)
        {
            player.inputEnabled = true;
        }
    }
}
