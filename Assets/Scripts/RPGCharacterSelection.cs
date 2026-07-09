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
        public TMP_Text characterCardName;
        public TMP_Text characterDescriptionText;
        [TextArea(2, 5)]
        public string characterDescription;
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

    [Header("Back & Cancellation Buttons")]
    public Button characterSelectionBackButton; // Used as "Ubah Usia" button in lobby
    public Button summaryBackButton;

    [Header("Prolog UI & GameObjects")]
    public GameObject prologPanel;
    public TMP_Text prologTitleText; 
    public GameObject prologMaleObject; // Karakter pria yang duduk
    public GameObject prologFemaleObject; // Karakter wanita yang duduk
    public TMP_Text prologSpeakerText;
    public TMP_Text prologDialogueText;
    public Button prologNextButton;

    [Header("Button Highlighting Colors")]
    public Color selectedButtonColor = new Color(0f, 0.7f, 1f, 1f); // Sky blue
    public Color normalButtonColor = Color.white;

    [Header("Fade UI")]
    public CanvasGroup fadeOverlay;
    public float fadeDuration = 1f;

    [Header("Gameplay Scene settings")]
    public string gameSceneName = "Level 1";


    private CharacterData selectedCharacter;
    private CharacterData unselectedCharacter;
    private bool isCharacterSelected = false;
    private bool isWalking = false;
    private int currentDialogueIndex = 0;
    
    private enum SelectionState
    {
        AgeSelection,
        LobbyChatting,
        CharacterMoving,
        CharacterNeaten,
        CharacterSmile,
        CharacterIntro,
        SelectionSummary,
        Prolog,
        Fading
    }
    private SelectionState currentState = SelectionState.AgeSelection;

    private Coroutine lobbyChatCoroutine;
    private Coroutine typingCoroutine;
    private int chosenAge = 21;
    private string chosenAgeRangeLabel = "";
    private Vector3 targetScale;
    private string chosenGender = "";

    // Store initial transform configurations to allow canceling selection
    private Vector3 maleInitialPosition;
    private Vector3 maleInitialScale;
    private Vector3 femaleInitialPosition;
    private Vector3 femaleInitialScale;

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
        // Store initial positions and scales of characters
        if (characterMale.characterObject != null)
        {
            maleInitialPosition = characterMale.characterObject.transform.position;
            maleInitialScale = characterMale.characterObject.transform.localScale;
        }
        if (characterFemale.characterObject != null)
        {
            femaleInitialPosition = characterFemale.characterObject.transform.position;
            femaleInitialScale = characterFemale.characterObject.transform.localScale;
        }

        // Menyembunyikan panel-panel yang tidak diperlukan di awal secara aman
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (summaryPanel != null) summaryPanel.SetActive(false);
        if (prologPanel != null) prologPanel.SetActive(false);
       
        // Menampilkan panel pemilihan umur di awal
        currentState = SelectionState.AgeSelection;
        if (ageRangePanel != null)
        {
            ageRangePanel.SetActive(true);
        }

        // Sembunyikan karakter di awal saat sedang milih usia
        if (characterMale.characterObject != null) characterMale.characterObject.SetActive(false);
        if (characterFemale.characterObject != null) characterFemale.characterObject.SetActive(false);

        // Sembunyikan chat bubble di awal
        if (maleChatBubble != null) maleChatBubble.SetActive(false);
        if (femaleChatBubble != null) femaleChatBubble.SetActive(false);

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

        // Sembunyikan kartu karakter di awal saat sedang milih usia
        if (characterMale.characterCard != null) characterMale.characterCard.SetActive(false);
        if (characterFemale.characterCard != null) characterFemale.characterCard.SetActive(false);
        
        // Petunjuk pemilihan tidak aktif saat mengisi panel umur
        if (selectionHintUI != null) selectionHintUI.SetActive(false);

        // Pastikan overlay fade transparan di awal
        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
        }

        // Jangan mulai obrolan lobby dulu, baru dimulai setelah umur diisi
        lobbyChatCoroutine = null;

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

        // Setup back/cancel buttons
        if (characterSelectionBackButton != null)
        {
            characterSelectionBackButton.onClick.RemoveAllListeners();
            characterSelectionBackButton.onClick.AddListener(OnBackToAgeSelection);
            characterSelectionBackButton.gameObject.SetActive(false);
        }

        if (summaryBackButton != null)
        {
            summaryBackButton.onClick.RemoveAllListeners();
            summaryBackButton.onClick.AddListener(ResetCharactersToInitialState);
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

    public string GetChosenGender()
    {
        return chosenGender;
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

        // Sembunyikan tombol kembali
        if (characterSelectionBackButton != null) characterSelectionBackButton.gameObject.SetActive(false);

        // // Aktifkan glow highlight pada karakter terpilih
        // if (selected.glowHighlight != null) selected.glowHighlight.SetActive(true);

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
            // Selesai dialog, langsung masuk ringkasan
            TransitionToSummary();
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
            // Selesai dialog perkenalan, masuk ke ringkasan
            TransitionToSummary();
        }
    }

    // Selesai dialog perkenalan, langsung masuk ke ringkasan (summary)
    private void TransitionToSummary()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        
        // Sembunyikan tombol kembali lobby
        if (characterSelectionBackButton != null) characterSelectionBackButton.gameObject.SetActive(false);

        currentState = SelectionState.SelectionSummary;
        ShowSummaryScreen();
    }



    // Dipanggil ketika salah satu rentang usia dipilih di panel input
    private void OnAgeRangeSelected(int representativeAge, string label)
    {
        chosenAge = representativeAge;
        chosenAgeRangeLabel = label;

        if (ageRangePanel != null) ageRangePanel.SetActive(false);

        // Memunculkan karakter kembali saat masuk lobby
        if (characterMale.characterObject != null) characterMale.characterObject.SetActive(true);
        if (characterFemale.characterObject != null) characterFemale.characterObject.SetActive(true);

        // Menampilkan dan mengatur info kartu karakter
        if (characterMale.characterCard != null) characterMale.characterCard.SetActive(true);
        if (characterMale.characterCardName != null) characterMale.characterCardName.text = characterMale.name;
        if (characterMale.characterDescriptionText != null) characterMale.characterDescriptionText.text = characterMale.characterDescription;

        if (characterFemale.characterCard != null) characterFemale.characterCard.SetActive(true);
        if (characterFemale.characterCardName != null) characterFemale.characterCardName.text = characterFemale.name;
        if (characterFemale.characterDescriptionText != null) characterFemale.characterDescriptionText.text = characterFemale.characterDescription;

        // Masuk ke fase obrolan/pemilihan di lobby
        currentState = SelectionState.LobbyChatting;

        // Tampilkan petunjuk pemilihan
        if (selectionHintUI != null) selectionHintUI.SetActive(true);

        // Tampilkan tombol kembali ("Ubah Usia")
        if (characterSelectionBackButton != null)
        {
            characterSelectionBackButton.gameObject.SetActive(true);
            TMP_Text buttonText = characterSelectionBackButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null) buttonText.text = "Ubah Usia";
        }

        // Mulai obrolan lobby berkala
        if (lobbyChatCoroutine == null)
        {
            lobbyChatCoroutine = StartCoroutine(LobbyChatRoutine());
        }
    }

    // Dipanggil ketika klik tombol kembali ke panel pemilihan umur ("Ubah Usia")
    private void OnBackToAgeSelection()
    {
        currentState = SelectionState.AgeSelection;
        
        if (ageRangePanel != null) ageRangePanel.SetActive(true);
        if (characterSelectionBackButton != null) characterSelectionBackButton.gameObject.SetActive(false);
        if (selectionHintUI != null) selectionHintUI.SetActive(false);
        
        // Sembunyikan karakter lagi karena kembali ke menu usia
        if (characterMale.characterObject != null) characterMale.characterObject.SetActive(false);
        if (characterFemale.characterObject != null) characterFemale.characterObject.SetActive(false);

        // Sembunyikan kartu karakter
        if (characterMale.characterCard != null) characterMale.characterCard.SetActive(false);
        if (characterFemale.characterCard != null) characterFemale.characterCard.SetActive(false);

        if (lobbyChatCoroutine != null)
        {
            StopCoroutine(lobbyChatCoroutine);
            lobbyChatCoroutine = null;
        }
    }

    // Dipanggil ketika player mengklik Karakter Laki-laki (Raka)
    public void ClickMaleCharacter()
    {
        if (isCharacterSelected) return;
        chosenGender = "Male";
        SelectCharacter(characterMale, characterFemale);
    }

    // Dipanggil ketika player mengklik Karakter Wanita (Nadia)
    public void ClickFemaleCharacter()
    {
        if (isCharacterSelected) return;
        chosenGender = "Female";
        SelectCharacter(characterFemale, characterMale);
    }

    // Dipanggil untuk membatalkan pilihan karakter dan merestore ke posisi semula
    private void ResetCharactersToInitialState()
    {
        isCharacterSelected = false;
        isWalking = false;
        chosenGender = "";
        
        if (lobbyChatCoroutine != null)
        {
            StopCoroutine(lobbyChatCoroutine);
            lobbyChatCoroutine = null;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // Kembalikan posisi, skala, dan status animator Raka (Male)
        if (characterMale.characterObject != null)
        {
            characterMale.characterObject.transform.position = maleInitialPosition;
            characterMale.characterObject.transform.localScale = maleInitialScale;
            if (characterMale.spriteRenderer != null)
            {
                Color c = characterMale.spriteRenderer.color;
                c.a = 1f;
                characterMale.spriteRenderer.color = c;
                if (characterMale.idleSprite != null) characterMale.spriteRenderer.sprite = characterMale.idleSprite;
            }
            if (characterMale.animator != null)
            {
                characterMale.animator.enabled = true;
                if (HasParameter(characterMale.animator, walkParamName)) characterMale.animator.SetBool(walkParamName, false);
            }
        }

        // Kembalikan posisi, skala, dan status animator Nadia (Female)
        if (characterFemale.characterObject != null)
        {
            characterFemale.characterObject.transform.position = femaleInitialPosition;
            characterFemale.characterObject.transform.localScale = femaleInitialScale;
            if (characterFemale.spriteRenderer != null)
            {
                Color c = characterFemale.spriteRenderer.color;
                c.a = 1f;
                characterFemale.spriteRenderer.color = c;
                if (characterFemale.idleSprite != null) characterFemale.spriteRenderer.sprite = characterFemale.idleSprite;
            }
            if (characterFemale.animator != null)
            {
                characterFemale.animator.enabled = true;
                if (HasParameter(characterFemale.animator, walkParamName)) characterFemale.animator.SetBool(walkParamName, false);
            }
        }

        if (maleChatBubble != null) maleChatBubble.SetActive(false);
        if (femaleChatBubble != null) femaleChatBubble.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (summaryPanel != null) summaryPanel.SetActive(false);
        if (prologPanel != null) prologPanel.SetActive(false);
        
        currentState = SelectionState.LobbyChatting;

        if (selectionHintUI != null) selectionHintUI.SetActive(true);
        if (characterSelectionBackButton != null) characterSelectionBackButton.gameObject.SetActive(true);

        if (lobbyChatCoroutine == null)
        {
            lobbyChatCoroutine = StartCoroutine(LobbyChatRoutine());
        }
    }

    // Helper untuk mengubah warna tombol pilihan (keep for references/compatibility)
    private void HighlightButton(Button button, bool highlight)
    {
        if (button == null) return;
        ColorBlock colors = button.colors;
        colors.normalColor = highlight ? selectedButtonColor : normalButtonColor;
        colors.selectedColor = colors.normalColor;
        colors.highlightedColor = colors.normalColor;
        button.colors = colors;
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
        
        StartProlog();
    }

    [System.Serializable]
    public struct PrologDialogueLine
    {
        public string speaker;
        [TextArea(2, 5)]
        public string dialogueText;
    }

    private PrologDialogueLine[] prologDialogueLines;
    private int currentPrologIndex = 0;
    private Coroutine prologTypingCoroutine;

    private void StartProlog()
    {
        if (summaryPanel != null) summaryPanel.SetActive(false);
        
        currentState = SelectionState.Prolog;
        
        if (prologPanel != null)
        {
            prologPanel.SetActive(true);
        }
        else
        {
            // Fallback langsung load scene jika prologPanel tidak diassign
            StartCoroutine(FadeAndLoadSceneToMap());
            return;
        }

        // Sembunyikan SEMUA karakter yang berdiri di lobby
        if (characterMale.characterObject != null) characterMale.characterObject.SetActive(false);
        if (characterFemale.characterObject != null) characterFemale.characterObject.SetActive(false);

        // Aktifkan GameObject karakter yang duduk sesuai pilihan, sembunyikan yang tidak dipilih
        if (chosenGender == "Male")
        {
            if (prologMaleObject != null) prologMaleObject.SetActive(true);
            if (prologFemaleObject != null) prologFemaleObject.SetActive(false);
        }
        else
        {
            if (prologFemaleObject != null) prologFemaleObject.SetActive(true);
            if (prologMaleObject != null) prologMaleObject.SetActive(false);
        }

        // Siapkan dialog prolog
        string characterName = selectedCharacter.name;
        prologDialogueLines = new PrologDialogueLine[]
        {
            new PrologDialogueLine { speaker = "Dokter", dialogueText = "Selamat! Hasil pemeriksaan kesehatan Anda menunjukkan hasil positif (sangat sehat dan fit)." },
            new PrologDialogueLine { speaker = characterName, dialogueText = "Terima kasih, Dokter. Apakah itu berarti saya sudah bisa langsung bertugas?" },
            new PrologDialogueLine { speaker = "Dokter", dialogueText = "Tentu saja, semua indikator kondisi fisik Anda prima. Sekarang silakan tentukan peta wilayah tugas Anda." },
            new PrologDialogueLine { speaker = characterName, dialogueText = "Baik, Dokter. Saya akan melihat dan memilih wilayah tugas sekarang." }
        };

        currentPrologIndex = 0;

        if (prologNextButton != null)
        {
            prologNextButton.onClick.RemoveAllListeners();
            prologNextButton.onClick.AddListener(OnPrologNextClick);
        }

        ShowPrologDialogue();
    }

    private void ShowPrologDialogue()
    {
        if (prologDialogueLines == null || currentPrologIndex >= prologDialogueLines.Length)
        {
            StartCoroutine(FadeAndLoadSceneToMap());
            return;
        }

        PrologDialogueLine line = prologDialogueLines[currentPrologIndex];
        
        // Update teks pembicara ke kedua field yang mungkin dipakai (Speaker atau Title)
        if (prologSpeakerText != null)
        {
            prologSpeakerText.text = line.speaker;
        }
        if (prologTitleText != null)
        {
            prologTitleText.text = line.speaker;
        }

        if (prologTypingCoroutine != null)
        {
            StopCoroutine(prologTypingCoroutine);
        }
        prologTypingCoroutine = StartCoroutine(TypePrologText(line.dialogueText));
    }

    private IEnumerator TypePrologText(string targetText)
    {
        if (prologDialogueText != null) prologDialogueText.text = "";
        if (prologNextButton != null) prologNextButton.interactable = false;

        foreach (char c in targetText)
        {
            if (prologDialogueText != null) prologDialogueText.text += c;
            yield return new WaitForSeconds(0.03f);
        }

        if (prologNextButton != null) prologNextButton.interactable = true;
    }

    private void OnPrologNextClick()
    {
        currentPrologIndex++;
        if (currentPrologIndex < prologDialogueLines.Length)
        {
            ShowPrologDialogue();
        }
        else
        {
            StartCoroutine(FadeAndLoadSceneToMap());
        }
    }

    private IEnumerator FadeAndLoadSceneToMap()
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

        // Pindah ke Pilih Maps scene
        Time.timeScale = 1f;
        SceneManager.LoadScene("Pilih Maps");
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

