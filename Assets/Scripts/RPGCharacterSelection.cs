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
        public string job;
        public string quote;
        public GameObject characterObject;
        public Animator animator;
        public SpriteRenderer spriteRenderer;
        public Transform walkTarget;
        public Sprite portraitSprite;
        public GameObject characterCard;
        public GameObject glowHighlight;
        [TextArea(2, 5)]
        public string[] introDialogues;

        [Header("Sprite Transitions (No Animation Mode)")]
        public Sprite idleSprite;
        public Sprite walkSprite;
        public Sprite neatenSprite;
        public Sprite smileSprite;
    }

    [Header("Characters")]
    public CharacterData characterMale;   // Raka
    public CharacterData characterFemale; // Nadia

    [Header("Lobby Chat bubbles")]
    public GameObject maleChatBubble;
    public TMP_Text maleChatText;
    public GameObject femaleChatBubble;
    public TMP_Text femaleChatText;
    public float chatInterval = 4f;

    [Header("Selection Info & Hint")]
    public GameObject selectionHintUI; // UI at the bottom: "Klik salah satu karakter untuk memilih"

    [Header("Dialogue Box UI")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueNameText;
    public TMP_Text dialogueBodyText;
    public Button nextDialogueButton;

    [Header("Animator Parameters")]
    public string walkParamName = "Walk";
    public string neatenParamName = "Neaten";
    public string smileParamName = "Smile";

    [Header("Animation Durations")]
    public float neatenDuration = 1.5f;
    public float smileDuration = 1.0f;

    [Header("Age Range Selection UI")]
    public GameObject ageRangePanel;
    public Button ageButton18_24;
    public Button ageButton25_34;
    public Button ageButton35_44;
    public Button ageButton45_plus;

    [Header("Summary UI")]
    public GameObject summaryPanel;
    public Image summaryPortrait;
    public TMP_Text summaryNameText;
    public TMP_Text summaryAgeText;
    public TMP_Text summaryJobText;
    public Button summaryContinueButton;

    [Header("Fade UI")]
    public CanvasGroup fadeOverlay;
    public float fadeDuration = 1f;

    [Header("Gameplay Scene settings")]
    public string gameSceneName = "Level 1";

    // Keep old fields for backward compatibility to avoid scene reference breaks
    [Header("Deprecated (Backward Compatibility)")]
    public GameObject agePanel;
    public TMP_InputField ageInputField;
    public Button startGameButton;
    public TMP_Text ageWarningText;

    private CharacterData selectedCharacter;
    private CharacterData unselectedCharacter;
    private bool isCharacterSelected = false;
    private bool isWalking = false;
    private int currentDialogueIndex = 0;
    
    private enum SelectionState
    {
        LobbyChatting,
        CharacterMoving,
        CharacterNeaten,
        CharacterSmile,
        CharacterIntro,
        AgeSelection,
        SelectionSummary,
        Fading
    }
    private SelectionState currentState = SelectionState.LobbyChatting;

    private Coroutine lobbyChatCoroutine;
    private Coroutine typingCoroutine;
    private int chosenAge = 21;
    private string chosenAgeRangeLabel = "";
    private Vector3 targetScale;

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
        if (ageRangePanel != null) ageRangePanel.SetActive(false);
        if (summaryPanel != null) summaryPanel.SetActive(false);
        
        // Deprecated panels
        if (agePanel != null) agePanel.SetActive(false);
        if (maleChatBubble != null) maleChatBubble.SetActive(false);
        if (femaleChatBubble != null) femaleChatBubble.SetActive(false);
        if (ageWarningText != null) ageWarningText.text = "";

        // Matikan efek gravitasi/fisika pada karakter di lobby agar tidak jatuh
        DisablePhysics(characterMale.characterObject);
        DisablePhysics(characterFemale.characterObject);

        // Inisialisasi sprite awal jika ditugaskan
        if (characterMale.spriteRenderer != null && characterMale.idleSprite != null)
        {
            characterMale.spriteRenderer.sprite = characterMale.idleSprite;
        }
        if (characterFemale.spriteRenderer != null && characterFemale.idleSprite != null)
        {
            characterFemale.spriteRenderer.sprite = characterFemale.idleSprite;
        }

        // Pastikan glow highlight dinonaktifkan di awal
        if (characterMale.glowHighlight != null) characterMale.glowHighlight.SetActive(false);
        if (characterFemale.glowHighlight != null) characterFemale.glowHighlight.SetActive(false);

        // Pastikan kartu karakter lobby aktif di awal
        if (characterMale.characterCard != null) characterMale.characterCard.SetActive(true);
        if (characterFemale.characterCard != null) characterFemale.characterCard.SetActive(true);

        // Pastikan petunjuk pemilihan aktif di awal
        if (selectionHintUI != null) selectionHintUI.SetActive(true);

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

        // Setup age range buttons
        if (ageButton18_24 != null)
        {
            ageButton18_24.onClick.RemoveAllListeners();
            ageButton18_24.onClick.AddListener(() => OnAgeRangeSelected(21, "18-24 Tahun"));
        }
        if (ageButton25_34 != null)
        {
            ageButton25_34.onClick.RemoveAllListeners();
            ageButton25_34.onClick.AddListener(() => OnAgeRangeSelected(30, "25-34 Tahun"));
        }
        if (ageButton35_44 != null)
        {
            ageButton35_44.onClick.RemoveAllListeners();
            ageButton35_44.onClick.AddListener(() => OnAgeRangeSelected(40, "35-44 Tahun"));
        }
        if (ageButton45_plus != null)
        {
            ageButton45_plus.onClick.RemoveAllListeners();
            ageButton45_plus.onClick.AddListener(() => OnAgeRangeSelected(50, "45+ Tahun"));
        }

        // Setup summary continue button
        if (summaryContinueButton != null)
        {
            summaryContinueButton.onClick.RemoveAllListeners();
            summaryContinueButton.onClick.AddListener(OnContinueToGame);
        }

        // Terapkan trigger klik pada objek karakter secara dinamis
        SetupClickTrigger(characterMale.characterObject, true);
        SetupClickTrigger(characterFemale.characterObject, false);
    }

    // Coroutine untuk transisi sprite secara mulus (crossfade sederhana)
    private IEnumerator TransitionSprite(SpriteRenderer renderer, Sprite newSprite, float duration = 0.2f)
    {
        if (renderer == null) yield break;
        if (newSprite == null) yield break;

        // Jika sprite saat ini sudah sama dengan sprite baru, lewati transisi
        if (renderer.sprite == newSprite) yield break;

        float elapsed = 0f;
        Color originalColor = renderer.color;
        
        // Fade out (meredup)
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsed / (duration / 2f));
            renderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        renderer.sprite = newSprite;
        elapsed = 0f;

        // Fade in (kembali terang)
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, originalColor.a, elapsed / (duration / 2f));
            renderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        renderer.color = originalColor;
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

                // Membesarkan karakter secara bertahap saat berjalan
                charObj.transform.localScale = Vector3.Lerp(
                    charObj.transform.localScale,
                    targetScale,
                    5f * Time.deltaTime
                );

                // Memeriksa apakah sudah sangat dekat dengan target
                if (Vector3.Distance(charObj.transform.position, target.position) < 0.05f)
                {
                    charObj.transform.position = target.position;
                    charObj.transform.localScale = targetScale; // Pastikan ukurannya mencapai target akhir
                    isWalking = false;
                    StartCoroutine(PlaySelectionSequence());
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

            // Obrolan Nadia (Wanita)
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

    // Dipanggil ketika player mengklik Karakter Wanita (Nadia)
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

        // Sembunyikan kartu karakter lobby
        if (characterMale.characterCard != null) characterMale.characterCard.SetActive(false);
        if (characterFemale.characterCard != null) characterFemale.characterCard.SetActive(false);

        // Sembunyikan petunjuk pemilihan
        if (selectionHintUI != null) selectionHintUI.SetActive(false);

        // Aktifkan glow highlight pada karakter terpilih
        if (selected.glowHighlight != null) selected.glowHighlight.SetActive(true);

        // Beri efek dim/redup untuk karakter yang tidak dipilih
        if (unselected.spriteRenderer != null)
        {
            Color unselectedColor = unselected.spriteRenderer.color;
            unselectedColor.a = 0.4f; // buat transparan di belakang
            unselected.spriteRenderer.color = unselectedColor;
        }

        // Periksa apakah menggunakan sistem transisi gambar/sprite (non-animasi)
        bool useSpriteTransition = (selected.idleSprite != null || selected.walkSprite != null || selected.neatenSprite != null || selected.smileSprite != null);

        if (useSpriteTransition)
        {
            // Nonaktifkan Animator agar tidak menimpa sprite renderer kita
            if (selected.animator != null) selected.animator.enabled = false;
            if (unselected.animator != null) unselected.animator.enabled = false;

            // Transisi sprite terpilih ke walkSprite (atau fallback ke idleSprite)
            Sprite nextSprite = selected.walkSprite != null ? selected.walkSprite : selected.idleSprite;
            StartCoroutine(TransitionSprite(selected.spriteRenderer, nextSprite, 0.2f));
        }
        else
        {
            // Jalankan animasi jalan pada karakter yang dipilih (jika menggunakan Animator)
            if (selected.animator != null)
            {
                if (HasParameter(selected.animator, walkParamName))
                {
                    selected.animator.SetBool(walkParamName, true); // Memasang animasi jalan
                }
                else
                {
                    selected.animator.Play(walkParamName); // Fallback jika menggunakan direct state
                }
            }
        }

        // Cek jika walkTarget atau characterObject tidak lengkap
        if (selected.walkTarget == null || selected.characterObject == null)
        {
            Debug.LogWarning("[RPGCharacterSelection] WalkTarget atau CharacterObject null! Langsung memainkan sekuens berikutnya.");
            isWalking = false;
            StartCoroutine(PlaySelectionSequence());
        }
        else
        {
            isWalking = true;
            targetScale = selected.characterObject.transform.localScale * 1.7f; // Skala membesar menjadi lebih gede (1.7x)
            Debug.Log("Karakter Terpilih: " + selected.name + ". Berjalan maju...");
        }
    }

    // Coroutine memainkan urutan sekuens animasi/gambar terpilih
    private IEnumerator PlaySelectionSequence()
    {
        bool useSpriteTransition = (selectedCharacter.idleSprite != null || selectedCharacter.walkSprite != null || selectedCharacter.neatenSprite != null || selectedCharacter.smileSprite != null);

        // 1. Berhenti berjalan
        if (!useSpriteTransition)
        {
            if (selectedCharacter.animator != null)
            {
                if (HasParameter(selectedCharacter.animator, walkParamName))
                {
                    selectedCharacter.animator.SetBool(walkParamName, false);
                }
                else
                {
                    selectedCharacter.animator.Play("Idle");
                }
            }
        }

        // 2. Berhenti & Merapikan Diri
        currentState = SelectionState.CharacterNeaten;
        if (useSpriteTransition)
        {
            Sprite nextSprite = selectedCharacter.neatenSprite != null ? selectedCharacter.neatenSprite : selectedCharacter.idleSprite;
            yield return StartCoroutine(TransitionSprite(selectedCharacter.spriteRenderer, nextSprite, 0.2f));
        }
        else
        {
            if (selectedCharacter.animator != null)
            {
                if (HasParameter(selectedCharacter.animator, neatenParamName))
                {
                    selectedCharacter.animator.SetTrigger(neatenParamName);
                }
                else
                {
                    // Fallback play state langsung
                    selectedCharacter.animator.Play(neatenParamName);
                }
            }
        }
        yield return new WaitForSeconds(neatenDuration);

        // 3. Lihat Ke Player & Senyum
        currentState = SelectionState.CharacterSmile;
        if (useSpriteTransition)
        {
            Sprite nextSprite = selectedCharacter.smileSprite != null ? selectedCharacter.smileSprite : (selectedCharacter.neatenSprite != null ? selectedCharacter.neatenSprite : selectedCharacter.idleSprite);
            yield return StartCoroutine(TransitionSprite(selectedCharacter.spriteRenderer, nextSprite, 0.2f));
        }
        else
        {
            if (selectedCharacter.animator != null)
            {
                if (HasParameter(selectedCharacter.animator, smileParamName))
                {
                    selectedCharacter.animator.SetTrigger(smileParamName);
                }
                else
                {
                    selectedCharacter.animator.Play(smileParamName);
                }
            }
        }
        yield return new WaitForSeconds(smileDuration);

        // 4. Dialog Muncul
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
            // Selesai dialog, langsung masuk pilihan umur
            TransitionToAgeSelection();
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
            // Selesai dialog perkenalan, masuk ke pemilihan rentang usia
            TransitionToAgeSelection();
        }
    }

    // Masuk ke fase pemilihan rentang usia
    private void TransitionToAgeSelection()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        currentState = SelectionState.AgeSelection;
        if (ageRangePanel != null) ageRangePanel.SetActive(true);
    }

    // Dipanggil ketika salah satu rentang usia dipilih
    private void OnAgeRangeSelected(int representativeAge, string label)
    {
        chosenAge = representativeAge;
        chosenAgeRangeLabel = label;

        if (ageRangePanel != null) ageRangePanel.SetActive(false);

        // Masuk ke Fase Ringkasan (Summary)
        currentState = SelectionState.SelectionSummary;
        ShowSummaryScreen();
    }

    // Menampilkan layar ringkasan profil
    private void ShowSummaryScreen()
    {
        if (summaryPanel != null)
        {
            summaryPanel.SetActive(true);
            
            if (summaryPortrait != null) summaryPortrait.sprite = selectedCharacter.portraitSprite;
            if (summaryNameText != null) summaryNameText.text = selectedCharacter.name;
            if (summaryAgeText != null) summaryAgeText.text = chosenAgeRangeLabel;
            if (summaryJobText != null) summaryJobText.text = selectedCharacter.job;
        }
        else
        {
            // Fallback jika panel ringkasan tidak dibuat
            OnContinueToGame();
        }
    }

    // Fungsi klik tombol Lanjut pada panel ringkasan untuk berpindah ke game scene
    private void OnContinueToGame()
    {
        // Simpan pilihan karakter dan umur ke database
        DatabaseManager dbManager = DatabaseManager.GetOrCreateInstance();
        if (dbManager != null)
        {
            dbManager.SaveCharacterSelection(selectedCharacter.name, chosenAge);
        }

        Debug.Log($"Pilihan Disimpan. Karakter: {selectedCharacter.name}, Usia Representatif: {chosenAge} ({chosenAgeRangeLabel})");
        
        // Memulai proses transisi fade out dan load scene
        StartCoroutine(FadeAndLoadScene());
    }

    private IEnumerator FadeAndLoadScene()
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

        // Pindah ke level permainan pertama
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
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

        // Pastikan ada Collider2D agar klik terdeteksi
        Collider2D collider = charObj.GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = charObj.GetComponentInChildren<Collider2D>();
        }

        if (collider == null)
        {
            collider = charObj.AddComponent<BoxCollider2D>();
            Debug.Log($"[RPGCharacterSelection] Menambahkan BoxCollider2D otomatis pada {charObj.name} agar bisa menerima klik.");
        }

        GameObject targetObj = collider.gameObject;
        LobbyCharacterClickTrigger trigger = targetObj.GetComponent<LobbyCharacterClickTrigger>();
        if (trigger == null)
        {
            trigger = targetObj.AddComponent<LobbyCharacterClickTrigger>();
        }
        trigger.isMale = isMale;
    }

    // Menonaktifkan efek fisika/gravitasi pada karakter agar tidak jatuh
    private void DisablePhysics(GameObject charObj)
    {
        if (charObj == null) return;

        Rigidbody2D[] rbs = charObj.GetComponentsInChildren<Rigidbody2D>(true);
        foreach (var rb in rbs)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }
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

