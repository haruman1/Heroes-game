using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuExplanationManager : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject explanationPanel;
    public TMP_Text characterNameText;
    public TMP_Text explanationText;
    public Button nextButton;

    [Header("Settings")]
    public float typeSpeed = 0.03f;
    
    // Kita gunakan PlayerPrefs untuk mengecek apakah penjelasan sudah pernah dilihat
    public string prefsKey = "HasSeenMainMenuExplanation";
    
    // Apakah kita ingin memaksa ini muncul setiap kali ke MainMenu atau hanya sekali?
    public bool showOnlyOnce = true;

    [Header("Explanations")]
    [TextArea(3, 5)]
    public string introText = "Selamat datang! Game ini adalah simulasi edukasi tentang dunia IT dan Administrasi Perkantoran.";
    
    [TextArea(3, 5)]
    public string level1Text = "Di Level 1, kamu telah mengumpulkan buku-buku dasar yang akan membantumu memahami fundamental dari pekerjaanmu.";
    
    [TextArea(3, 5)]
    public string level2Text = "Nantinya di Level 2, tantangannya akan berbeda. Kamu harus menyortir dokumen dan mengatasi masalah spesifik di lingkungan kerja!";
    
    [TextArea(3, 5)]
    public string outroText = "Persiapkan dirimu, pilih karaktermu, dan bersiaplah untuk petualangan selanjutnya. Semoga berhasil!";

    private string[] allDialogues;
    private int currentDialogueIndex = 0;

    private void Start()
    {
        allDialogues = new string[] { introText, level1Text, level2Text, outroText };
        
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        // Cek apakah perlu dimunculkan
        if (showOnlyOnce && PlayerPrefs.GetInt(prefsKey, 0) == 1)
        {
            // Sudah pernah melihat
            if (explanationPanel != null) explanationPanel.SetActive(false);
            return;
        }

        StartExplanation();
    }

    // Bisa dipanggil secara manual dari tombol jika ingin melihat lagi
    public void StartExplanation()
    {
        if (explanationPanel == null) return;
        
        explanationPanel.SetActive(true);
        currentDialogueIndex = 0;
        
        // Atur nama
        DatabaseManager dbManager = DatabaseManager.GetOrCreateInstance();
        string charName = "Karakter";
        if (dbManager != null && dbManager.GetPlayerData() != null)
        {
            charName = dbManager.GetPlayerData().SelectedCharacter;
            if (string.IsNullOrEmpty(charName)) charName = "Raka";
        }

        if (characterNameText != null)
        {
            characterNameText.text = charName;
        }

        StartCoroutine(TypeText(allDialogues[currentDialogueIndex]));
    }

    private void OnNextButtonClicked()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex < allDialogues.Length)
        {
            StartCoroutine(TypeText(allDialogues[currentDialogueIndex]));
        }
        else
        {
            CloseExplanation();
        }
    }

    private IEnumerator TypeText(string message)
    {
        explanationText.text = "";
        nextButton.interactable = false;

        foreach (char c in message)
        {
            explanationText.text += c;
            yield return new WaitForSeconds(typeSpeed); // di Main Menu timeScale biasanya 1
        }

        nextButton.interactable = true;
        
        if (currentDialogueIndex >= allDialogues.Length - 1)
        {
            TMP_Text btnText = nextButton.GetComponentInChildren<TMP_Text>();
            if (btnText != null) btnText.text = "Selesai";
        }
    }

    private void CloseExplanation()
    {
        explanationPanel.SetActive(false);
        PlayerPrefs.SetInt(prefsKey, 1);
        PlayerPrefs.Save();
    }
}
