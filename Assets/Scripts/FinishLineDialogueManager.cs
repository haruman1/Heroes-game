using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class FinishLineDialogueManager : MonoBehaviour
{
    [Header("UI Dialogue Components")]
    public GameObject dialoguePanel;
    public TMP_Text characterNameText;
    public TMP_Text dialogueText;
    public Button nextDialogueButton;

    [Header("Portrait Positions (GameObjects)")]
    [Tooltip("GameObject tempat posisi gambar pemain muncul.")]
    public GameObject playerPortraitPosition;
    [Tooltip("GameObject tempat posisi gambar NPC muncul.")]
    public GameObject npcPortraitPosition;

    [Header("Player Sprites Default (Fallback)")]
    public Sprite malePlayerSprite;
    public Sprite femalePlayerSprite;

    [Header("Player Sprites Berdasarkan Usia (Remaja - Tua)")]
    public PlayerAgePortraits malePlayerPortraits;
    public PlayerAgePortraits femalePlayerPortraits;

    [Header("Dialogue Settings")]
    public float typeSpeed = 0.03f;

    [Header("Closing Dialogues Sequence")]
    public List<DialogueSequence> closingDialogues = new List<DialogueSequence>()
    {
        new DialogueSequence { speakerName = "", dialogueText = "Ternyata secara biologis tubuhku tetap sama kuatnya. Rasa lelah ini hanya karena pikiran-pikiranku yang sedang butuh istirahat. Aku akan melangkah pelan-pelan." }
    };

    [Header("Events (e.g. LoadScene)")]
    public UnityEvent onDialogueFinished;

    private int currentIndex = 0;
    private string playerName = "Karakter";
    private int playerAge = 18;
    private Coroutine typingCoroutine;

    private void Start()
    {
        if (nextDialogueButton != null)
        {
            nextDialogueButton.onClick.AddListener(OnNextButtonClicked);
        }
    }

    public void TriggerFinishLineDialogue()
    {
        if (dialoguePanel == null) return;
        
        if (closingDialogues == null || closingDialogues.Count == 0)
        {
            CloseDialogue();
            return;
        }

        dialoguePanel.SetActive(true);
        Time.timeScale = 0f; 

        DatabaseManager dbManager = DatabaseManager.GetOrCreateInstance();
        if (dbManager != null && dbManager.GetPlayerData() != null)
        {
            PlayerData playerData = dbManager.GetPlayerData();
            playerName = playerData.SelectedCharacter;
            if (string.IsNullOrEmpty(playerName)) playerName = "Awan";
            
            playerAge = playerData.SelectedAge <= 0 ? 18 : playerData.SelectedAge;
        }

        currentIndex = 0;
        ShowDialogueLine(currentIndex);
    }

    private void ShowDialogueLine(int index)
    {
        if (index < 0 || index >= closingDialogues.Count) return;

        DialogueSequence currentLine = closingDialogues[index];

        // 1. Set Nama
        if (string.IsNullOrEmpty(currentLine.speakerName))
        {
            if (characterNameText != null) characterNameText.text = playerName;
        }
        else
        {
            if (characterNameText != null) characterNameText.text = currentLine.speakerName;
        }

        // 2. Set Posisi dan Timpa Gambar
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
                bool isMale = (playerName == "Raka" || playerName == "Satria" || playerName == "Awan");
                Sprite chosenSprite = isMale 
                    ? malePlayerPortraits.GetSpriteForAge(playerAge, malePlayerSprite)
                    : femalePlayerPortraits.GetSpriteForAge(playerAge, femalePlayerSprite);
                
                SpriteRenderer sr = playerPortraitPosition.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = chosenSprite;

                Image img = playerPortraitPosition.GetComponent<Image>();
                if (img != null) img.sprite = chosenSprite;
            }
        }

        // 3. Ketik Teks
        string processedText = currentLine.dialogueText.Replace("{name}", playerName);
        processedText = processedText.Replace("{age}", playerAge.ToString());
        processedText = processedText.Replace("{usia}", playerAge.ToString());

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(processedText));
    }

    private void OnNextButtonClicked()
    {
        currentIndex++;

        if (currentIndex < closingDialogues.Count)
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
        if (nextDialogueButton != null) nextDialogueButton.interactable = false;

        foreach (char c in message)
        {
            dialogueText.text += c;
            yield return StartCoroutine(WaitForSecondsKlein(typeSpeed));
        }

        if (nextDialogueButton != null) nextDialogueButton.interactable = true;
        
        if (currentIndex >= closingDialogues.Count - 1)
        {
            TMP_Text btnText = nextDialogueButton != null ? nextDialogueButton.GetComponentInChildren<TMP_Text>() : null;
            if (btnText != null) btnText.text = "Selesai";
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
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        Time.timeScale = 1f; 
        onDialogueFinished?.Invoke(); 
    }
}
