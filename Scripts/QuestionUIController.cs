using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionUIController : MonoBehaviour
{
    [Header("UI References (shared)")]
    public GameObject uiChoices;
    public TextMeshProUGUI UIText;
    public TextMeshProUGUI[] choiceTexts = new TextMeshProUGUI[4];
    public Button[] choiceButtons = new Button[4];

    [Header("Hint")]
    public Button hintButton;
    public int hintCost = 20;

    private QuizQuestionBase activeQuestion;
    private bool hintUsed = false;
    private HealthScript healthScript;

    void Start()
    {
        if (uiChoices != null) uiChoices.SetActive(false);
        if (hintButton != null)
        {
            hintButton.onClick.RemoveAllListeners();
            hintButton.onClick.AddListener(OnHintPressed);
            hintButton.gameObject.SetActive(false);
        }

        healthScript = FindFirstObjectByType<HealthScript>();
    }

    public void Open(QuizQuestionBase questionOwner, string question, string[] choices, int correctIndex)
    {
        activeQuestion = questionOwner;
        hintUsed = false;

        // Setup texts
        if (UIText != null) UIText.text = question;
        for (int i = 0; i < choiceTexts.Length; i++)
        {
            if (i < choices.Length && choiceTexts[i] != null)
                choiceTexts[i].text = choices[i];
            if (i < choiceButtons.Length && choiceButtons[i] != null)
            {
                choiceButtons[i].gameObject.SetActive(true);
                int idx = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnChoicePressed(idx));
            }
        }

        // Show hint button if present
        if (hintButton != null)
        {
            hintButton.gameObject.SetActive(true);
            hintButton.interactable = (PlayerData.instance != null && PlayerData.instance.coins >= hintCost && !hintUsed);
        }

        if (uiChoices != null) uiChoices.SetActive(true);

        // Start the question timer in the QuizQuestionBase
        activeQuestion?.StartQuestionTimer();
    }

    public void Close()
    {
        if (uiChoices != null) uiChoices.SetActive(false);
        if (activeQuestion != null)
        {
            activeQuestion.StopQuestionTimer();
            activeQuestion = null;
        }

        if (hintButton != null)
        {
            hintButton.onClick.RemoveAllListeners();
            hintButton.gameObject.SetActive(false);
            hintButton.onClick.AddListener(OnHintPressed);
        }

        // remove listeners from choice buttons
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null) choiceButtons[i].onClick.RemoveAllListeners();
        }
    }

    void OnChoicePressed(int index)
    {
        if (activeQuestion == null) return;
        // Pass the actual Button reference to the question so it can run feedback logic
        Button btn = (index >= 0 && index < choiceButtons.Length) ? choiceButtons[index] : null;
        activeQuestion.SubmitAnswerFromUI(btn, index);
    }

    void OnHintPressed()
    {
        Debug.Log("[Hint] OnHintPressed called");
        if (hintUsed) { Debug.Log("[Hint] Already used"); return; }
        if (PlayerData.instance == null) { Debug.Log("[Hint] No PlayerData"); return; }
        if (PlayerData.instance.coins < hintCost) { Debug.Log($"[Hint] Not enough coins: {PlayerData.instance.coins} < {hintCost}"); return; }

        // Deduct coins and update HUD
        PlayerData.instance.DeductCoin(hintCost);
        if (healthScript != null) healthScript.updateCoin();

        // Hide up to two wrong choices
        if (activeQuestion == null) return;

        List<int> wrong = new List<int>();
        for (int i = 0; i < 4; i++) if (i != activeQuestion.correctChoiceIndex) wrong.Add(i);
        for (int i = wrong.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); var t = wrong[i]; wrong[i] = wrong[j]; wrong[j] = t; }
        int removeCount = Mathf.Min(2, wrong.Count);
        for (int r = 0; r < removeCount; r++)
        {
            int idx = wrong[r];
            if (idx >= 0 && idx < choiceButtons.Length && choiceButtons[idx] != null)
                choiceButtons[idx].gameObject.SetActive(false);
        }

        hintUsed = true;
        if (hintButton != null) hintButton.interactable = false;
    }

    // Public bridge so the UI Button OnClick can call the hint behaviour directly
    public void UseHintPublic()
    {
        OnHintPressed();
    }
}
