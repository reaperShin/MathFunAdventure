using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public abstract class QuizQuestionBase : MonoBehaviour
{
    // --- UI REFERENCES ---
    [Header("UI References")]
    public TextMeshProUGUI UIText;
    public TextMeshProUGUI choiceText1;
    public TextMeshProUGUI choiceText2;
    public TextMeshProUGUI choiceText3;
    public TextMeshProUGUI choiceText4;

    [Header("Buttons")]
    public Button choicebtn1;
    public Button choicebtn2;
    public Button choicebtn3;
    public Button choicebtn4;
    // If a shared `QuestionUIController` exists it will manage the hint button.
    private QuestionUIController sharedController;

    [Header("Game Objects")]
    public GameObject uiChoices; // The container holding the question UI

    // --- GAME DATA REFERENCES ---
    private PlayerData playerdata;
    private HealthScript dataupdate;

    // FIX: Changed to public so MinimapArrow can check if the question is done.
    [HideInInspector] public bool isAnswered = false;

    // --- QUESTION DATA (managed at runtime; NOT exposed in the Inspector) ---
    // Private backing fields (never serialized) and public read-only properties so
    // other scripts can read the values but they cannot be set via the Inspector.
    private string _question = "Override me!";
    private string[] _choices = new string[4] { "A", "B", "C", "D" };
    private int _correctChoiceIndex = 0;

    public string question { get { return _question; } }
    public string[] choices { get { return _choices; } }
    // Index of the correct choice (0..3)
    public int correctChoiceIndex { get { return _correctChoiceIndex; } }

    [Header("Feedback")]
    public float feedbackVisibleDuration = 1.5f;
    [Header("Timing")]
    [Tooltip("How many seconds before an unanswered question auto-fails and closes")]
    public float questionTimeLimit = 15f;
    private Coroutine questionTimer;
    private Color correctColor = new Color(0.0f, 1f, 0.2f);
    private Color wrongColor = new Color(1f, 0.2f, 0.2f);

    [Header("Visual Shake Settings")]
    public float shakeDuration = 0.2f;
    public float wrongStrength = 15f;
    public float correctStrength = 5f;

    [Header("Hint")]
    public int hintCost = 20;
    private bool hintUsed = false;

    [Header("Randomization")]
    [Tooltip("Category passed to questionRandomizer: Addition, Subtraction, Multiplication, Division")]
    public string questionCategory = "Addition";
    private questionRandomizer questionSource;

    void Start()
    {
        playerdata = FindFirstObjectByType<PlayerData>();
        dataupdate = FindFirstObjectByType<HealthScript>();
        sharedController = FindFirstObjectByType<QuestionUIController>();
        hintUsed = false;
        questionSource = FindFirstObjectByType<questionRandomizer>();
    }

    // --- CHOICE SHUFFLING LOGIC ---
    private void ShuffleChoices()
    {
        List<(string choice, int originalIndex)> pairs = new List<(string, int)>();
        for (int i = 0; i < choices.Length; i++)
        {
            pairs.Add((choices[i], i));
        }

        // Fisher-Yates shuffle
        for (int i = pairs.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = pairs[i];
            pairs[i] = pairs[j];
            pairs[j] = temp;
        }

        // Update the choices array and find the new correct index
        string originalCorrectChoice = choices[correctChoiceIndex];

        for (int i = 0; i < pairs.Count; i++)
        {
            // choices is a read-only property returning the backing array, so assigning
            // into its elements is fine (we're mutating the underlying array).
            choices[i] = pairs[i].choice;
            if (choices[i] == originalCorrectChoice)
            {
                // update the private backing field
                _correctChoiceIndex = i;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isAnswered)
        {
            if (playerdata != null)
            {
                playerdata.SetCheckpoint(transform.position);
            }

            // Reset hint state for this question and fetch a randomized question (always override inspector values)
            ResetHintState();

            // Ensure we have a question source. If none exists in the scene, create one at runtime.
            if (questionSource == null)
            {
                questionSource = FindFirstObjectByType<questionRandomizer>();
                if (questionSource == null)
                {
                    var go = new GameObject("questionRandomizer");
                    DontDestroyOnLoad(go);
                    questionSource = go.AddComponent<questionRandomizer>();
                }
            }

            // Always replace inspector values with a freshly generated random question when the player triggers the collider.
            if (questionSource != null)
            {
                Question q = questionSource.GetRandomQuestion();
                if (q != null)
                {
                    _question = q.questionText;
                    // Clone array to avoid accidental shared references
                    _choices = (string[])q.options.Clone();
                    // Defensive: ensure the provided correct index is valid
                    if (q.correctOptionIndex >= 0 && q.correctOptionIndex < _choices.Length)
                    {
                        _correctChoiceIndex = q.correctOptionIndex;
                    }
                    else
                    {
                        // Try to locate the correct answer string in options (best-effort)
                        if (q.correctOptionIndex >= 0)
                        {
                            Debug.LogWarning($"questionRandomizer returned invalid correctOptionIndex={q.correctOptionIndex}. Falling back to index 0.");
                        }
                        _correctChoiceIndex = 0;
                    }
                }
            }

            // Shuffle choices before displaying them
            ShuffleChoices();

            // If there's a shared UI controller, hand off display and return.
            if (sharedController != null)
            {
                sharedController.Open(this, question, choices, correctChoiceIndex);
                return;
            }

            // Ensure choice buttons are active (in case a previous hint hid them)
            if (choicebtn1 != null) choicebtn1.gameObject.SetActive(true);
            if (choicebtn2 != null) choicebtn2.gameObject.SetActive(true);
            if (choicebtn3 != null) choicebtn3.gameObject.SetActive(true);
            if (choicebtn4 != null) choicebtn4.gameObject.SetActive(true);

            // Clear and assign new listeners
            choicebtn1.onClick.RemoveAllListeners();
            choicebtn2.onClick.RemoveAllListeners();
            choicebtn3.onClick.RemoveAllListeners();
            choicebtn4.onClick.RemoveAllListeners();

            choicebtn1.onClick.AddListener(() => OnChoiceClicked(choicebtn1, 0));
            choicebtn2.onClick.AddListener(() => OnChoiceClicked(choicebtn2, 1));
            choicebtn3.onClick.AddListener(() => OnChoiceClicked(choicebtn3, 2));
            choicebtn4.onClick.AddListener(() => OnChoiceClicked(choicebtn4, 3));

            // Set UI texts (uses the newly shuffled 'choices' array)
            UIText.text = question;
            if (choices.Length > 0) choiceText1.text = choices[0];
            if (choices.Length > 1) choiceText2.text = choices[1];
            if (choices.Length > 2) choiceText3.text = choices[2];
            if (choices.Length > 3) choiceText4.text = choices[3];

            uiChoices.SetActive(true);
            StartQuestionTimer();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // If a shared controller is handling UI, tell it to close and return.
        if (sharedController != null)
        {
            sharedController.Close();
            return;
        }

        uiChoices.SetActive(false);
        StopQuestionTimer();
        // restore choice buttons (in case hint hid some)
        if (choicebtn1 != null) choicebtn1.gameObject.SetActive(true);
        if (choicebtn2 != null) choicebtn2.gameObject.SetActive(true);
        if (choicebtn3 != null) choicebtn3.gameObject.SetActive(true);
        if (choicebtn4 != null) choicebtn4.gameObject.SetActive(true);
    }

    protected abstract bool EvaluateAnswer(int index);

    void OnChoiceClicked(Button btn, int index)
    {
        if (isAnswered) return;
        StopQuestionTimer();
        StartCoroutine(ProvideFeedbackAndResolve(btn, index));
    }

    public void StartQuestionTimer()
    {
        StopQuestionTimer();
        if (questionTimeLimit > 0f)
            questionTimer = StartCoroutine(QuestionTimeoutCoroutine());
    }

    public void StopQuestionTimer()
    {
        if (questionTimer != null)
        {
            StopCoroutine(questionTimer);
            questionTimer = null;
        }
    }

    private IEnumerator QuestionTimeoutCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < questionTimeLimit)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (!isAnswered)
        {
            yield return StartCoroutine(HandleTimeoutFail());
        }
        questionTimer = null;
    }

    private IEnumerator HandleTimeoutFail()
    {
        choicebtn1.interactable = choicebtn2.interactable = choicebtn3.interactable = choicebtn4.interactable = false;

        var img1 = choicebtn1.GetComponent<Image>(); var img2 = choicebtn2.GetComponent<Image>();
        var img3 = choicebtn3.GetComponent<Image>(); var img4 = choicebtn4.GetComponent<Image>();
        Color c1 = img1 ? img1.color : Color.white; Color c2 = img2 ? img2.color : Color.white;
        Color c3 = img3 ? img3.color : Color.white; Color c4 = img4 ? img4.color : Color.white;

        var corrBtn = GetButtonByIndex(correctChoiceIndex);
        var corrImg = corrBtn ? corrBtn.GetComponent<Image>() : null;
        if (corrImg) corrImg.color = correctColor;

        Outline corrOutline = corrBtn ? corrBtn.GetComponent<Outline>() : null;
        if (corrBtn != null && corrOutline == null) corrOutline = corrBtn.gameObject.AddComponent<Outline>();
        if (corrOutline != null) { corrOutline.effectColor = Color.white; corrOutline.effectDistance = new Vector2(4, 4); }

        yield return new WaitForSeconds(feedbackVisibleDuration);

        if (playerdata != null) playerdata.wrongAnswers++;
        ShakeCanvas(wrongStrength);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayQuestionWrong();

        isAnswered = true;
        uiChoices.SetActive(false);

        if (img1) img1.color = c1; if (img2) img2.color = c2; if (img3) img3.color = c3; if (img4) img4.color = c4;
        choicebtn1.interactable = choicebtn2.interactable = choicebtn3.interactable = choicebtn4.interactable = true;
    }

    // Called by the shared UI controller when the player clicks a choice
    public void SubmitAnswerFromUI(Button clickedButton, int index)
    {
        if (isAnswered) return;
        StopQuestionTimer();
        StartCoroutine(ProvideFeedbackAndResolve(clickedButton, index));
    }


    protected Button GetButtonByIndex(int idx)
    {
        switch (idx)
        {
            default:
            case 0: return choicebtn1;
            case 1: return choicebtn2;
            case 2: return choicebtn3;
            case 3: return choicebtn4;
        }
    }

    // --- CANVAS SHAKE LOGIC ---
    private void ShakeCanvas(float strength)
    {
        // Safely find the Canvas component by searching up the hierarchy from uiChoices.
        Canvas rootCanvas = uiChoices.GetComponentInParent<Canvas>();

        if (rootCanvas == null) return;

        RectTransform canvasRT = rootCanvas.GetComponent<RectTransform>();
        if (canvasRT == null) return;

        StartCoroutine(ScreenShake(canvasRT, shakeDuration, strength));
    }

    System.Collections.IEnumerator ScreenShake(RectTransform target, float duration, float strength)
    {
        Vector3 originalPos = target.anchoredPosition3D;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;

            target.anchoredPosition3D = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.unscaledDeltaTime;
            strength = Mathf.Lerp(strength, 0f, elapsed / duration);

            yield return null;
        }

        target.anchoredPosition3D = originalPos;
    }

    // --- CORE RESOLUTION COROUTINE ---
    protected IEnumerator ProvideFeedbackAndResolve(Button clicked, int index)
    {
        // 1. Setup and Visual Feedback START
        choicebtn1.interactable = choicebtn2.interactable = choicebtn3.interactable = choicebtn4.interactable = false;

        // Store original colors
        var img1 = choicebtn1.GetComponent<Image>(); var img2 = choicebtn2.GetComponent<Image>();
        var img3 = choicebtn3.GetComponent<Image>(); var img4 = choicebtn4.GetComponent<Image>();
        Color c1 = img1 ? img1.color : Color.white; Color c2 = img2 ? img2.color : Color.white;
        Color c3 = img3 ? img3.color : Color.white; Color c4 = img4 ? img4.color : Color.white;

        bool isCorrect = EvaluateAnswer(index);

        var clickedImg = clicked ? clicked.GetComponent<Image>() : null;
        Transform clickedTf = clicked ? clicked.transform : null;
        Vector3 origScale = clickedTf ? clickedTf.localScale : Vector3.one;

        // Outline component setup
        Outline clickedOutline = clicked ? clicked.GetComponent<Outline>() : null;
        if (clicked != null && clickedOutline == null) clickedOutline = clicked.gameObject.AddComponent<Outline>();
        if (clickedOutline != null) { clickedOutline.effectColor = Color.white; clickedOutline.effectDistance = new Vector2(4, 4); }

        // Apply colors and hints
        if (isCorrect)
        {
            if (clickedImg) clickedImg.color = correctColor;
        }
        else
        {
            if (clickedImg) clickedImg.color = wrongColor;

            // HINT: Show the CORRECT answer
            var corrBtn = GetButtonByIndex(correctChoiceIndex);
            var corrImg = corrBtn ? corrBtn.GetComponent<Image>() : null;
            if (corrImg) corrImg.color = correctColor;

            // Outline the correct answer
            Outline corrOutline = corrBtn ? corrBtn.GetComponent<Outline>() : null;
            if (corrBtn != null && corrOutline == null) corrOutline = corrBtn.gameObject.AddComponent<Outline>();
            if (corrOutline != null) { corrOutline.effectColor = Color.white; corrOutline.effectDistance = new Vector2(4, 4); }
        }

        // Pulse Animation
        float pulseDur = 0.15f;
        if (clickedTf != null)
        {
            for (float t = 0; t < pulseDur; t += Time.deltaTime)
            {
                clickedTf.localScale = origScale * Mathf.Lerp(1f, 1.15f, t / pulseDur);
                yield return null;
            }
            for (float t = 0; t < pulseDur; t += Time.deltaTime)
            {
                clickedTf.localScale = origScale * Mathf.Lerp(1.15f, 1f, t / pulseDur);
                yield return null;
            }
            clickedTf.localScale = origScale;
        }

        // 2. Wait for visual feedback duration
        yield return new WaitForSeconds(feedbackVisibleDuration);

        // 3. Resolve Scoring and Audio 
        if (isCorrect)
        {
            ShakeCanvas(correctStrength);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayQuestionCorrect();
            if (playerdata != null) playerdata.AddScore(500);
            if (dataupdate != null) dataupdate.updateScore();
        }
        else
        {
            ShakeCanvas(wrongStrength);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayQuestionWrong();
            if (playerdata != null) playerdata.wrongAnswers++;
        }

        // 4. Cleanup
        isAnswered = true;
        uiChoices.SetActive(false);

        // Restore colors and re-enable buttons
        if (img1) img1.color = c1; if (img2) img2.color = c2; if (img3) img3.color = c3; if (img4) img4.color = c4;
        choicebtn1.interactable = choicebtn2.interactable = choicebtn3.interactable = choicebtn4.interactable = true;
    }

    // PUBLIC HINT METHOD - Call from hint button OnClick
    public void UseHint()
    {
        if (hintUsed)
        {
            Debug.Log("Hint already used for this question.");
            return;
        }

        if (PlayerData.instance == null)
        {
            Debug.Log("PlayerData not found.");
            return;
        }

        if (PlayerData.instance.coins < hintCost)
        {
            Debug.Log($"Not enough coins. Need {hintCost}, have {PlayerData.instance.coins}");
            return;
        }

        // Deduct coins
        PlayerData.instance.DeductCoin(hintCost);
        if (dataupdate != null) dataupdate.updateCoin();

        // Hide 2 wrong choices
        List<int> wrongIndices = new List<int>();
        Button[] buttons = { choicebtn1, choicebtn2, choicebtn3, choicebtn4 };

        for (int i = 0; i < buttons.Length; i++)
        {
            if (i != correctChoiceIndex)
                wrongIndices.Add(i);
        }

        // Shuffle and hide 2
        for (int i = wrongIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = wrongIndices[i];
            wrongIndices[i] = wrongIndices[j];
            wrongIndices[j] = temp;
        }

        int hideCount = Mathf.Min(2, wrongIndices.Count);
        for (int i = 0; i < hideCount; i++)
        {
            int idx = wrongIndices[i];
            if (idx >= 0 && idx < buttons.Length && buttons[idx] != null)
                buttons[idx].gameObject.SetActive(false);
        }

        hintUsed = true;
        Debug.Log("Hint used! 2 choices hidden.");
    }

    // Reset hint when question starts (call from OnTriggerEnter)
    private void ResetHintState()
    {
        hintUsed = false;
        if (choicebtn1 != null) choicebtn1.gameObject.SetActive(true);
        if (choicebtn2 != null) choicebtn2.gameObject.SetActive(true);
        if (choicebtn3 != null) choicebtn3.gameObject.SetActive(true);
        if (choicebtn4 != null) choicebtn4.gameObject.SetActive(true);
    }
}
