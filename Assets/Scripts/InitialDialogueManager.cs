using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InitialDialogueManager : MonoBehaviour
{
    [Header("UI Dialogue Components")]
    public GameObject dialoguePanel;
    public TMP_Text characterNameText;
    public TMP_Text dialogueText;
    public Button closeDialogueButton;

    [Header("Dialogue Speed")]
    public float typeSpeed = 0.03f;

    [Header("Custom Dialogues (Optional Overrides)")]
    [Header("Raka (Male) - Age Groups")]
    [TextArea(3, 5)] public string maleDialogue18_24; // Dewasa awal
    [TextArea(3, 5)] public string maleDialogue25_34; // Fokus membangun karier
    [TextArea(3, 5)] public string maleDialogue35_44; // Karier stabil, fokus keluarga
    [TextArea(3, 5)] public string maleDialogue45_59; // Dewasa madya

    [Header("Rini (Female) - Age Groups")]
    [TextArea(3, 5)] public string femaleDialogue18_24; // Dewasa awal
    [TextArea(3, 5)] public string femaleDialogue25_34; // Fokus membangun karier
    [TextArea(3, 5)] public string femaleDialogue35_44; // Karier stabil, fokus keluarga
    [TextArea(3, 5)] public string femaleDialogue45_59; // Dewasa madya

    private void Start()
    {
        if (dialoguePanel == null || characterNameText == null || dialogueText == null || closeDialogueButton == null)
        {
            Debug.LogError("[InitialDialogueManager] Komponen UI belum terhubung lengkap di inspector!");
            return;
        }

        closeDialogueButton.onClick.AddListener(CloseDialogue);
        SetupAndShowDialogue();
    }

    private void SetupAndShowDialogue()
    {
        // Ambil data karakter dan umur dari SQLite database
        DatabaseManager dbManager = DatabaseManager.GetOrCreateInstance();
        if (dbManager == null)
        {
            dialoguePanel.SetActive(false);
            return;
        }

        PlayerData playerData = dbManager.GetPlayerData();
        if (playerData == null)
        {
            dialoguePanel.SetActive(false);
            return;
        }

        string charName = playerData.SelectedCharacter;
        if (string.IsNullOrEmpty(charName))
        {
            charName = "Raka"; // Default jika kosong
        }

        int age = playerData.SelectedAge;
        if (age <= 0)
        {
            age = 18; // Default jika kosong ke Dewasa Awal
        }

        string finalDialogue = "";

        // Tentukan dialog berdasarkan Karakter dan Umur (4 Kategori)
        bool isMale = (charName == "Raka" || charName == "Satria");

        if (isMale)
        {
            characterNameText.text = "Raka";
            if (age >= 18 && age <= 24)
            {
                finalDialogue = !string.IsNullOrEmpty(maleDialogue18_24)
                    ? maleDialogue18_24.Replace("{age}", age.ToString())
                    : $"Halo Raka! Di usiamu yang ke-{age} tahun ini (Dewasa Awal), kamu baru saja memulai langkahmu di dunia IT. Kumpulkan 10 buku dasar IT di level ini untuk mengasah kemampuan teknismu!";
            }
            else if (age >= 25 && age <= 34)
            {
                finalDialogue = !string.IsNullOrEmpty(maleDialogue25_34)
                    ? maleDialogue25_34.Replace("{age}", age.ToString())
                    : $"Salam Raka! Di usiamu yang ke-{age} tahun ini (Fokus Karier), kamu sedang fokus membangun kariermu. Kumpulkan 10 buku referensi sistem di level ini agar bisa menyelesaikan proyek server tepat waktu!";
            }
            else if (age >= 35 && age <= 44)
            {
                finalDialogue = !string.IsNullOrEmpty(maleDialogue35_44)
                    ? maleDialogue35_44.Replace("{age}", age.ToString())
                    : $"Halo Raka! Di usiamu yang ke-{age} tahun (Karier & Keluarga), kariermu mulai stabil dan kini fokus mengurus keluarga. Kumpulkan 10 buku manajemen waktu di level ini demi keseimbangan kerja dan keluarga!";
            }
            else
            {
                // 45-59 tahun atau lainnya (Dewasa Madya)
                finalDialogue = !string.IsNullOrEmpty(maleDialogue45_59)
                    ? maleDialogue45_59.Replace("{age}", age.ToString())
                    : $"Selamat datang, Raka! Di usiamu yang ke-{age} tahun (Dewasa Madya), kamu adalah senior IT dengan segudang pengalaman. Kumpulkan 10 buku arsip legendaris di level ini untuk mewariskan ilmumu!";
            }
        }
        else
        {
            characterNameText.text = "Rini";
            if (age >= 18 && age <= 24)
            {
                finalDialogue = !string.IsNullOrEmpty(femaleDialogue18_24)
                    ? femaleDialogue18_24.Replace("{age}", age.ToString())
                    : $"Halo Rini! Di usiamu yang ke-{age} tahun ini (Dewasa Awal), langkah pertamamu di administrasi kantor baru saja dimulai. Kumpulkan 10 buku panduan admin di level ini untuk membiasakan dirimu!";
            }
            else if (age >= 25 && age <= 34)
            {
                finalDialogue = !string.IsNullOrEmpty(femaleDialogue25_34)
                    ? femaleDialogue25_34.Replace("{age}", age.ToString())
                    : $"Salam Rini! Di usiamu yang ke-{age} tahun ini (Fokus Karier), kamu berfokus penuh membangun kariermu. Kumpulkan 10 buku berkas arsip di level ini agar efisiensi kerja kantormu meningkat pesat!";
            }
            else if (age >= 35 && age <= 44)
            {
                finalDialogue = !string.IsNullOrEmpty(femaleDialogue35_44)
                    ? femaleDialogue35_44.Replace("{age}", age.ToString())
                    : $"Halo Rini! Di usiamu yang ke-{age} tahun (Karier & Keluarga), kariermu mulai stabil dan fokusmu beralih mengurus keluarga. Kumpulkan 10 buku panduan keluarga di level ini demi kebahagiaan di rumah!";
            }
            else
            {
                // 45-59 tahun atau lainnya (Dewasa Madya)
                finalDialogue = !string.IsNullOrEmpty(femaleDialogue45_59)
                    ? femaleDialogue45_59.Replace("{age}", age.ToString())
                    : $"Selamat datang, Rini! Di usiamu yang ke-{age} tahun (Dewasa Madya), pengalaman adminmu sangat berharga bagi perusahaan. Kumpulkan 10 buku dokumen bersejarah di level ini untuk diselesaikan!";
            }
        }

        // Tampilkan panel dialog dan hentikan waktu game
        dialoguePanel.SetActive(true);
        Time.timeScale = 0f;

        StartCoroutine(TypeText(finalDialogue));
    }

    private IEnumerator TypeText(string message)
    {
        dialogueText.text = "";
        closeDialogueButton.interactable = false;

        foreach (char c in message)
        {
            dialogueText.text += c;
            // Gunakan RealtimeSinceStartup karena Time.timeScale = 0 mematikan Time.deltaTime biasa
            yield return StartCoroutine(WaitForSecondsKlein(typeSpeed));
        }

        closeDialogueButton.interactable = true;
    }

    // Helper untuk menunggu waktu nyata saat timeScale = 0
    private IEnumerator WaitForSecondsKlein(float seconds)
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < start + seconds)
        {
            yield return null;
        }
    }

    public void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        Time.timeScale = 1f; // Jalankan kembali game
        Debug.Log("[InitialDialogueManager] Dialog ditutup, game dimulai.");
    }
}
