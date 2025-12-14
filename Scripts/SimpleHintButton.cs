using UnityEngine;
using UnityEngine.UI;

public class SimpleHintButton : MonoBehaviour
{
    [Header("Choice Buttons")]
    public Button choiceBtn1;
    public Button choiceBtn2;
    public Button choiceBtn3;
    public Button choiceBtn4;

    [Header("Hint Cost")]
    public int hintCost = 20;

    private int correctChoiceIndex = -1;
    private bool hintUsed = false;

    // Call this from the Hint button's OnClick event
    public void UseHint()
    {
        // Check if hint already used
        if (hintUsed)
        {
            Debug.Log("Hint already used for this question.");
            return;
        }

        // Check if player has enough coins
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
        var healthScript = FindFirstObjectByType<HealthScript>();
        if (healthScript != null) healthScript.updateCoin();

        // Hide 2 wrong choices (keep correct answer and 1 other)
        Button[] buttons = { choiceBtn1, choiceBtn2, choiceBtn3, choiceBtn4 };
        System.Collections.Generic.List<int> wrongIndices = new System.Collections.Generic.List<int>();

        // Find which buttons are wrong (this is a simple approach: hide any 2 except potentially the first)
        // For a proper solution, you'd need to know the correct answer index
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null && i != correctChoiceIndex)
            {
                wrongIndices.Add(i);
            }
        }

        // Shuffle and hide 2 wrong ones
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
            {
                buttons[idx].gameObject.SetActive(false);
            }
        }

        hintUsed = true;
        Debug.Log("Hint used! 2 choices hidden.");
    }

    // Call this when a new question appears (to reset hint state)
    public void ResetHint(int correctIndex = -1)
    {
        hintUsed = false;
        correctChoiceIndex = correctIndex;

        // Re-enable all choice buttons
        if (choiceBtn1 != null) choiceBtn1.gameObject.SetActive(true);
        if (choiceBtn2 != null) choiceBtn2.gameObject.SetActive(true);
        if (choiceBtn3 != null) choiceBtn3.gameObject.SetActive(true);
        if (choiceBtn4 != null) choiceBtn4.gameObject.SetActive(true);

        Debug.Log("Hint reset for new question.");
    }
}
