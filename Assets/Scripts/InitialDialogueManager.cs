using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

[System.Serializable]
public struct DialogueSequence
{
    [Tooltip("Kosongkan jika yang bicara adalah karakter pemain utama.")]
    public string speakerName;
    
    [Tooltip("Gambar NPC (misal wajah dokter). Kosongkan jika ini dialog pemain.")]
    public Sprite npcSprite;

    [TextArea(3, 5)]
    public string dialogueText;
}

public class InitialDialogueManager : MonoBehaviour
{
    [Header("UI Dialogue Components")]
    public GameObject dialoguePanel;
    public TMP_Text characterNameText;
    public TMP_Text dialogueText;
    public Button nextDialogueButton;

    [Header("Portrait Positions (GameObjects)")]
    [Tooltip("GameObject tempat posisi gambar pemain muncul.")]
    public GameObject playerPortraitPosition;
    [Tooltip("GameObject tempat posisi gambar NPC (Dokter) muncul.")]
    public GameObject npcPortraitPosition;

    [Header("Player Sprites (Gambar)")]
    public Sprite malePlayerSprite;
    public Sprite femalePlayerSprite;

    [Header("Dialogue Speed")]
    public float typeSpeed = 0.03f;

    [Header("Events")]
    public UnityEvent onDialogueFinished;

    [Header("Dialogue Sequence")]
    public List<DialogueSequence> dialogues = new List<DialogueSequence>()
    {
        new DialogueSequence { speakerName = "", dialogueText = "Dok, jujur badan saya rasanya lemas sekali hari ini. Seperti kehilangan tenaga untuk sekedar berdiri lama." },
        new DialogueSequence { speakerName = "Dokter Soni", dialogueText = "{dynamic_text}" },
        new DialogueSequence { speakerName = "", dialogueText = "Jadi, fisik saya sebenarnya tidak apa-apa, Dok?" },
        new DialogueSequence { speakerName = "Dokter Soni", dialogueText = "Betul. Secara biologis, tubuhmu masih sekuat kemarin. Di perpustakaan depan ada catatan medis soal ini. Coba kamu baca agar hatimu lebih tenang." }
    };

    [Header("Dynamic Dialogues (Age/Gender Overrides)")]
    [Header("Raka (Male) - Age Groups")]
    [TextArea(3, 5)] public string maleDialogue18_24 = "Halo {name}! Di usiamu yang ke-{age} tahun ini (Dewasa Awal), kamu baru saja memulai langkahmu di dunia IT. Kumpulkan 10 halaman buku dasar IT di level ini untuk mengasah kemampuan teknismu!";
    [TextArea(3, 5)] public string maleDialogue25_34 = "Salam {name}! Di usiamu yang ke-{age} tahun ini (Fokus Karier), kamu sedang fokus membangun kariermu. Kumpulkan 10 halaman buku referensi sistem di level ini agar bisa menyelesaikan proyek server tepat waktu!";
    [TextArea(3, 5)] public string maleDialogue35_44 = "Halo {name}! Di usiamu yang ke-{age} tahun (Karier & Keluarga), kariermu mulai stabil dan kini fokus mengurus keluarga. Kumpulkan 10 halaman buku manajemen waktu di level ini demi keseimbangan kerja dan keluarga!";
    [TextArea(3, 5)] public string maleDialogue45_59 = "Selamat datang, {name}! Di usiamu yang ke-{age} tahun (Dewasa Madya), kamu adalah senior IT dengan segudang pengalaman. Kumpulkan 10 halaman buku arsip legendaris di level ini untuk mewariskan ilmumu!";

    [Header("Rini (Female) - Age Groups")]
    [TextArea(3, 5)] public string femaleDialogue18_24 = "Halo {name}! Di usiamu yang ke-{age} tahun ini (Dewasa Awal), langkah pertamamu di administrasi kantor baru saja dimulai. Kumpulkan 10 halaman buku panduan admin di level ini untuk membiasakan dirimu!";
    [TextArea(3, 5)] public string femaleDialogue25_34 = "Salam {name}! Di usiamu yang ke-{age} tahun ini (Fokus Karier), kamu berfokus penuh membangun kariermu. Kumpulkan 10 halaman buku berkas arsip di level ini agar efisiensi kerja kantormu meningkat pesat!";
    [TextArea(3, 5)] public string femaleDialogue35_44 = "Halo {name}! Di usiamu yang ke-{age} tahun (Karier & Keluarga), kariermu mulai stabil dan fokusmu beralih mengurus keluarga. Kumpulkan 10 halaman buku panduan keluarga di level ini demi kebahagiaan di rumah!";
    [TextArea(3, 5)] public string femaleDialogue45_59 = "Selamat datang, {name}! Di usiamu yang ke-{age} tahun (Dewasa Madya), pengalaman adminmu sangat berharga bagi perusahaan. Kumpulkan 10 halaman buku dokumen bersejarah di level ini untuk diselesaikan!";

    private int currentIndex = 0;
    private string playerName = "Karakter";
    private int playerAge = 18;
    private string dynamicAgeText = "";
    private Coroutine typingCoroutine;

    private void Start()
    {
        if (dialoguePanel == null || characterNameText == null || dialogueText == null || nextDialogueButton == null)
        {
            Debug.LogError("[InitialDialogueManager] Komponen UI belum terhubung lengkap di inspector!");
            return;
        }

        nextDialogueButton.onClick.AddListener(OnNextClicked);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    public void TriggerDialogue()
    {
        if (dialogues == null || dialogues.Count == 0) return;

        DatabaseManager dbManager = DatabaseManager.GetOrCreateInstance();
        playerAge = 18;
        if (dbManager != null && dbManager.GetPlayerData() != null)
        {
            PlayerData playerData = dbManager.GetPlayerData();
            playerName = string.IsNullOrEmpty(playerData.SelectedCharacter) ? "Raka" : playerData.SelectedCharacter;
            playerAge = playerData.SelectedAge <= 0 ? 18 : playerData.SelectedAge;
        }

        bool isMale = (playerName == "Raka" || playerName == "Satria");
        if (isMale)
        {
            if (playerAge >= 18 && playerAge <= 24) dynamicAgeText = maleDialogue18_24;
            else if (playerAge >= 25 && playerAge <= 34) dynamicAgeText = maleDialogue25_34;
            else if (playerAge >= 35 && playerAge <= 44) dynamicAgeText = maleDialogue35_44;
            else dynamicAgeText = maleDialogue45_59;
        }
        else
        {
            if (playerAge >= 18 && playerAge <= 24) dynamicAgeText = femaleDialogue18_24;
            else if (playerAge >= 25 && playerAge <= 34) dynamicAgeText = femaleDialogue25_34;
            else if (playerAge >= 35 && playerAge <= 44) dynamicAgeText = femaleDialogue35_44;
            else dynamicAgeText = femaleDialogue45_59;
        }

        dynamicAgeText = dynamicAgeText.Replace("{name}", playerName).Replace("{age}", playerAge.ToString()).Replace("{usia}", playerAge.ToString());

        dialoguePanel.SetActive(true);
        Time.timeScale = 0f; 
        currentIndex = 0;
        
        ShowDialogueLine(currentIndex);
    }

    private void ShowDialogueLine(int index)
    {
        if (index < 0 || index >= dialogues.Count) return;

        DialogueSequence currentLine = dialogues[index];

        // 1. Set Nama
        if (string.IsNullOrEmpty(currentLine.speakerName))
        {
            characterNameText.text = playerName;
        }
        else
        {
            characterNameText.text = currentLine.speakerName;
        }

        // 2. Set Posisi dan Timpa Gambar (Support SpriteRenderer & Image)
        if (playerPortraitPosition != null) playerPortraitPosition.SetActive(false);
        if (npcPortraitPosition != null) npcPortraitPosition.SetActive(false);

        if (currentLine.npcSprite != null)
        {
            // NPC yang bicara
            if (npcPortraitPosition != null)
            {
                npcPortraitPosition.SetActive(true);
                
                SpriteRenderer sr = npcPortraitPosition.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = currentLine.npcSprite;
                
                Image img = npcPortraitPosition.GetComponent<Image>();
                if (img != null) img.sprite = currentLine.npcSprite;
            }
        }
        else
        {
            // Pemain yang bicara
            if (playerPortraitPosition != null)
            {
                playerPortraitPosition.SetActive(true);
                bool isMale = (playerName == "Raka" || playerName == "Satria");
                Sprite chosenSprite = isMale ? malePlayerSprite : femalePlayerSprite;
                
                SpriteRenderer sr = playerPortraitPosition.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = chosenSprite;

                Image img = playerPortraitPosition.GetComponent<Image>();
                if (img != null) img.sprite = chosenSprite;
            }
        }

        // 3. Ketik Teks
        string processedText = currentLine.dialogueText.Replace("{name}", playerName);
        string nameCharacterText = currentLine.speakerName;
        nameCharacterText = nameCharacterText.Replace("{name}", playerName);
        characterNameText.text = nameCharacterText;
        processedText = processedText.Replace("{age}", playerAge.ToString());
        processedText = processedText.Replace("{usia}", playerAge.ToString());
        processedText = processedText.Replace("{dynamic_text}", dynamicAgeText);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(processedText));
        
    }

    private void OnNextClicked()
    {
        currentIndex++;
        
        if (currentIndex < dialogues.Count)
        {
            ShowDialogueLine(currentIndex);
        }
        else
        {
            CloseDialogue();
        }
    }

    private IEnumerator TypeText(string message)
    {
        dialogueText.text = "";
        nextDialogueButton.interactable = false;

        foreach (char c in message)
        {
            dialogueText.text += c;
            yield return StartCoroutine(WaitForSecondsKlein(typeSpeed));
        }

        nextDialogueButton.interactable = true;
        
        if (currentIndex >= dialogues.Count - 1)
        {
            TMP_Text btnText = nextDialogueButton.GetComponentInChildren<TMP_Text>();
            if (btnText != null) btnText.text = "Buka Pintu";
        }
    }

    private IEnumerator WaitForSecondsKlein(float seconds)
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < start + seconds)
        {
            yield return null;
        }
    }

    private void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        Time.timeScale = 1f; 
        onDialogueFinished?.Invoke();
    }
}
