using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class RPGCharacterSelection : MonoBehaviour
{
    public static RPGCharacterSelection Instance { get; private set; }

    [System.Serializable]
    public struct CharacterData
    {
        public string name;
        public GameObject characterObject;
        public Animator animator;
        public SpriteRenderer spriteRenderer;
        public Transform walkTarget;
        [TextArea(2, 5)]
        public string[] introDialogues;
    }

    [Header("Characters")]
    public CharacterData characterMale;   // Raka (Laki-laki)
    public CharacterData characterFemale; // Rini (Wanita)

    [Header("Lobby Chat bubbles")]
    public GameObject maleChatBubble;
    public TMP_Text maleChatText;
    public GameObject femaleChatBubble;
    public TMP_Text femaleChatText;
    public float chatInterval = 4f;

    [Header("Dialogue Box UI")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueNameText;
    public TMP_Text dialogueBodyText;
    public Button nextDialogueButton;

    [Header("Fade UI")]
    public CanvasGroup fadeOverlay;
    public float fadeDuration = 1f;

    [Header("Age Input UI")]
    public GameObject agePanel;
    public TMP_InputField ageInputField;
    public Button startGameButton;
    public TMP_Text ageWarningText;

    [Header("Gameplay Scene settings")]
    public string gameSceneName = "Level 1"; // Nama scene game level 1

    private CharacterData selectedCharacter;
    private CharacterData unselectedCharacter;
    private bool isCharacterSelected = false;
    private bool isWalking = false;
    private int currentDialogueIndex = 0;
    
    private enum SelectionState
    {
        LobbyChatting,
        CharacterMoving,
        CharacterIntro,
        Fading,
        AgeInput
    }
    private SelectionState currentState = SelectionState.LobbyChatting;

    private Coroutine lobbyChatCoroutine;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Menyembunyikan panel-panel yang tidak diperlukan di awal secara aman
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (agePanel != null) agePanel.SetActive(false);
        if (maleChatBubble != null) maleChatBubble.SetActive(false);
        if (femaleChatBubble != null) femaleChatBubble.SetActive(false);
        if (ageWarningText != null) ageWarningText.text = "";

        // Pastikan overlay fade transparan di awal
        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
        }

        // Mulai obrolan lobby berkala
        lobbyChatCoroutine = StartCoroutine(LobbyChatRoutine());

        // Daftarkan listener tombol secara bersih
        if (nextDialogueButton != null)
        {
            nextDialogueButton.onClick.RemoveAllListeners();
            nextDialogueButton.onClick.AddListener(OnNextDialogueClick);
        }

        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(OnStartGameClick);
        }

        // Konfigurasi input field agar menampilkan keyboard angka di Android
        if (ageInputField != null)
        {
            ageInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            ageInputField.characterLimit = 3; // Batasi angka umur (maks 3 digit)
        }

        // Terapkan trigger klik pada objek karakter secara dinamis
        SetupClickTrigger(characterMale.characterObject, true);
        SetupClickTrigger(characterFemale.characterObject, false);
    }

    private void Update()
    {
        // Handle pergerakan karakter terpilih
        if (currentState == SelectionState.CharacterMoving && isWalking)
        {
            Transform target = selectedCharacter.walkTarget;
            GameObject charObj = selectedCharacter.characterObject;

            if (target != null && charObj != null)
            {
                // Menggerakkan karakter ke target walkTarget
                charObj.transform.position = Vector3.MoveTowards(
                    charObj.transform.position,
                    target.position,
                    3f * Time.deltaTime
                );

                // Memeriksa apakah sudah sangat dekat dengan target
                if (Vector3.Distance(charObj.transform.position, target.position) < 0.05f)
                {
                    charObj.transform.position = target.position;
                    isWalking = false;
                    StartCoroutine(ArriveAndSetupDialogue());
                }
            }
        }

        // Tangani input sentuhan Android dan klik mouse PC
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        if (currentState != SelectionState.LobbyChatting) return;

        // Pendeteksian klik mouse kiri (fallback untuk PC Editor)
        if (Input.GetMouseButtonDown(0))
        {
            DetectSelection(Input.mousePosition);
        }

        // Pendeteksian sentuhan layar (untuk Android)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                DetectSelection(touch.position);
            }
        }
    }

    private void DetectSelection(Vector3 screenPosition)
    {
        if (isCharacterSelected) return;

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Vector3 worldPos = mainCam.ScreenToWorldPoint(screenPosition);
            Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);
            RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero);

            if (hit.collider != null)
            {
                GameObject hitObj = hit.collider.gameObject;
                
                if (IsCharacterObject(hitObj, characterMale.characterObject))
                {
                    ClickMaleCharacter();
                }
                else if (IsCharacterObject(hitObj, characterFemale.characterObject))
                {
                    ClickFemaleCharacter();
                }
            }
        }
    }

    private bool IsCharacterObject(GameObject hitObj, GameObject charObj)
    {
        if (charObj == null) return false;
        return hitObj == charObj || hitObj.transform.IsChildOf(charObj.transform);
    }

    // Coroutine obrolan kecil berkala saat lobby
    private IEnumerator LobbyChatRoutine()
    {
        string[] maleChats = {
            "Semoga hari ini lancar.",
            "Semangat kerja untuk hari ini!",
            "Wah, sepertinya hari ini akan sibuk."
        };

        string[] femaleChats = {
            "Iya, semangat!",
            "Semoga saja semuanya berjalan baik.",
            "Jangan lupa minum kopi/teh ya."
        };

        int chatIndex = 0;

        while (currentState == SelectionState.LobbyChatting)
        {
            yield return new WaitForSeconds(chatInterval);
            if (currentState != SelectionState.LobbyChatting) break;

            // Obrolan Raka (Laki-laki)
            if (maleChatBubble != null && maleChatText != null)
            {
                maleChatText.text = maleChats[chatIndex % maleChats.Length];
                maleChatBubble.SetActive(true);
                yield return new WaitForSeconds(2f);
                maleChatBubble.SetActive(false);
            }

            yield return new WaitForSeconds(1f);
            if (currentState != SelectionState.LobbyChatting) break;

            // Obrolan Rini (Wanita)
            if (femaleChatBubble != null && femaleChatText != null)
            {
                femaleChatText.text = femaleChats[chatIndex % femaleChats.Length];
                femaleChatBubble.SetActive(true);
                yield return new WaitForSeconds(2f);
                femaleChatBubble.SetActive(false);
            }

            chatIndex++;
        }
    }

    // Dipanggil ketika player mengklik Karakter Laki-laki (Raka)
    public void ClickMaleCharacter()
    {
        if (isCharacterSelected) return;
        SelectCharacter(characterMale, characterFemale);
    }

    // Dipanggil ketika player mengklik Karakter Wanita (Rini)
    public void ClickFemaleCharacter()
    {
        if (isCharacterSelected) return;
        SelectCharacter(characterFemale, characterMale);
    }

    // Menangani aksi seleksi karakter
    private void SelectCharacter(CharacterData selected, CharacterData unselected)
    {
        isCharacterSelected = true;
        currentState = SelectionState.CharacterMoving;
        selectedCharacter = selected;
        unselectedCharacter = unselected;

        // Hentikan coroutine obrolan lobby secara instan
        if (lobbyChatCoroutine != null)
        {
            StopCoroutine(lobbyChatCoroutine);
            lobbyChatCoroutine = null;
        }

        // Matikan balon obrolan lobby
        if (maleChatBubble != null) maleChatBubble.SetActive(false);
        if (femaleChatBubble != null) femaleChatBubble.SetActive(false);

        // Beri efek dim/redup untuk karakter yang tidak dipilih
        if (unselected.spriteRenderer != null)
        {
            Color unselectedColor = unselected.spriteRenderer.color;
            unselectedColor.a = 0.4f; // buat transparan di belakang
            unselected.spriteRenderer.color = unselectedColor;
        }

        // Jalankan animasi jalan pada karakter yang dipilih
        if (selected.animator != null)
        {
            if (HasParameter(selected.animator, "Walk"))
            {
                selected.animator.SetBool("Walk", true); // Memasang animasi jalan
            }
            else
            {
                selected.animator.Play("Walk"); // Fallback jika menggunakan direct state
            }
        }

        // Cek jika walkTarget atau characterObject tidak lengkap
        if (selected.walkTarget == null || selected.characterObject == null)
        {
            Debug.LogWarning("[RPGCharacterSelection] WalkTarget atau CharacterObject null! Langsung menampilkan dialog perkenalan.");
            isWalking = false;
            StartCoroutine(ArriveAndSetupDialogue());
        }
        else
        {
            isWalking = true;
            Debug.Log("Karakter Terpilih: " + selected.name + ". Berjalan maju...");
        }
    }

    // Coroutine setelah karakter sampai di target posisi
    private IEnumerator ArriveAndSetupDialogue()
    {
        // Matikan animasi jalan
        if (selectedCharacter.animator != null)
        {
            if (HasParameter(selectedCharacter.animator, "Walk"))
            {
                selectedCharacter.animator.SetBool("Walk", false);
            }
            else
            {
                selectedCharacter.animator.Play("Idle"); // Fallback ke Idle direct state
            }

            if (HasParameter(selectedCharacter.animator, "Smile"))
            {
                selectedCharacter.animator.SetTrigger("Smile"); // Pemicu animasi senyum
            }
        }

        yield return new WaitForSeconds(0.5f);

        // Mulai dialog perkenalan
        currentState = SelectionState.CharacterIntro;
        currentDialogueIndex = 0;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueNameText != null) dialogueNameText.text = selectedCharacter.name;

        if (selectedCharacter.introDialogues != null && selectedCharacter.introDialogues.Length > 0)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(selectedCharacter.introDialogues[currentDialogueIndex]));
        }
        else
        {
            // Selesai dialog, masuk ke Fase Fade
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            StartCoroutine(FadeToAgeInput());
        }
    }

    // Menampilkan teks dengan efek mengetik (typewriter)
    private IEnumerator TypeText(string targetText)
    {
        if (dialogueBodyText != null) dialogueBodyText.text = "";
        if (nextDialogueButton != null) nextDialogueButton.interactable = false;

        foreach (char c in targetText)
        {
            if (dialogueBodyText != null) dialogueBodyText.text += c;
            yield return new WaitForSeconds(0.03f); // Kecepatan ketikan
        }

        if (nextDialogueButton != null) nextDialogueButton.interactable = true;
    }

    // Fungsi klik tombol dialog Lanjut
    private void OnNextDialogueClick()
    {
        currentDialogueIndex++;
        if (selectedCharacter.introDialogues != null && currentDialogueIndex < selectedCharacter.introDialogues.Length)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(selectedCharacter.introDialogues[currentDialogueIndex]));
        }
        else
        {
            // Selesai dialog, masuk ke Fase Fade
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            StartCoroutine(FadeToAgeInput());
        }
    }

    // Coroutine untuk memudarkan layar lalu menampilkan panel umur
    private IEnumerator FadeToAgeInput()
    {
        currentState = SelectionState.Fading;
        
        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
        }

        // Tampilkan panel umur setelah layar hitam
        currentState = SelectionState.AgeInput;
        if (agePanel != null) agePanel.SetActive(true);
    }

    // Fungsi klik tombol konfirmasi umur dan mulai game
    private void OnStartGameClick()
    {
        if (ageInputField == null) return;

        string inputVal = ageInputField.text;
        if (string.IsNullOrEmpty(inputVal))
        {
            if (ageWarningText != null) ageWarningText.text = "Mohon masukkan umur Anda!";
            return;
        }

        if (int.TryParse(inputVal, out int age))
        {
            if (age <= 0)
            {
                if (ageWarningText != null) ageWarningText.text = "Umur harus lebih besar dari 0!";
                return;
            }

            // Simpan pilihan karakter dan umur ke database
            DatabaseManager dbManager = DatabaseManager.GetOrCreateInstance();
            if (dbManager != null)
            {
                dbManager.SaveCharacterSelection(selectedCharacter.name, age);
            }

            Debug.Log("Pilihan Disimpan. Karakter: " + selectedCharacter.name + ", Umur: " + age);
            
            // Pindah ke level permainan pertama
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            if (ageWarningText != null) ageWarningText.text = "Masukkan angka umur yang valid!";
        }
    }

    // Helper untuk mendeteksi parameter Animator
    private bool HasParameter(Animator animator, string paramName)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    // Setup klik trigger pada objek karakter
    private void SetupClickTrigger(GameObject charObj, bool isMale)
    {
        if (charObj == null) return;

        // Pastikan ada Collider2D pada objek atau anaknya agar klik terdeteksi
        Collider2D collider = charObj.GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = charObj.GetComponentInChildren<Collider2D>();
        }

        if (collider == null)
        {
            // Tambahkan BoxCollider2D otomatis jika benar-benar tidak ada collider
            collider = charObj.AddComponent<BoxCollider2D>();
            Debug.Log($"[RPGCharacterSelection] Menambahkan BoxCollider2D otomatis pada {charObj.name} agar bisa menerima klik.");
        }

        // Tambahkan script trigger ke GameObject yang memiliki collider
        GameObject targetObj = collider.gameObject;
        LobbyCharacterClickTrigger trigger = targetObj.GetComponent<LobbyCharacterClickTrigger>();
        if (trigger == null)
        {
            trigger = targetObj.AddComponent<LobbyCharacterClickTrigger>();
        }
        trigger.isMale = isMale;
    }
}

// Helper trigger klik yang dipasang secara dinamis di GameObject yang memiliki collider
public class LobbyCharacterClickTrigger : MonoBehaviour
{
    public bool isMale;

    private void OnMouseDown()
    {
        if (RPGCharacterSelection.Instance != null)
        {
            if (isMale)
            {
                RPGCharacterSelection.Instance.ClickMaleCharacter();
            }
            else
            {
                RPGCharacterSelection.Instance.ClickFemaleCharacter();
            }
        }
    }
}
