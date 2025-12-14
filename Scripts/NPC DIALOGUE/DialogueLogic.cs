using System.Collections;
using System;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Dialogue
{
    public string npcName;
    public string[] sentences;

    public Dialogue(string name, string[] lines)
    {
        npcName = name;
        sentences = lines;
    }
}

public class DialogueLogic : MonoBehaviour
{
    [Header("Main Canvas Reference")]
    public GameObject uiPanel;
    public TextMeshProUGUI dialogueText;
    public GameObject mainui;
    public TextMeshProUGUI npcText;

    [Header("Lookup")]
    public string npcName;

    [Header("UI / Trigger")]
    public string triggerTag = "Player";

    private Coroutine typingRoutine;

    public Dialogue[] dialogues = new Dialogue[]
    {
        new Dialogue("Gerald", new string[]
        {
            "Hey there, traveler!",
            "Welcome to this strange world… definitely not normal.",
            "I'm your guide for now, so listen up.",
            "Use A and D to move left and right.",
            "Press Space to jump. Arrow keys work too.",
            "You’ll find platforms with questions.",
            "Answer all of them to move forward.",
            "Clear everything and the next level opens.",
            "Keep going… it's the only way back home.",
            "Good luck, traveler."
        }),

        new Dialogue("Gerald (Final)", new string[]
        {
            "Congratulations Travel!",
            "this is the final level",
            "When you finish this level you can now go home"
        })
    };

    // ✔ Correct Unity trigger names
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(triggerTag)) return;

        Dialogue found = FindDialogue(npcName);
        if (found == null) return;

        npcText.text = found.npcName;
        uiPanel.SetActive(true);
        mainui.SetActive(false);
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = StartCoroutine(ShowAllLines(found));
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(triggerTag)) return;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        dialogueText.text = "";
        npcText.text = "";
        uiPanel.SetActive(false);
        mainui.SetActive(true);
    }

    // Find dialogue by npcName
    Dialogue FindDialogue(string name)
    {
        foreach (Dialogue d in dialogues)
        {
            if (d == null) continue;

            if (string.Equals(d.npcName, name, StringComparison.OrdinalIgnoreCase))
                return d;
        }
        return null;
    }

    // Shows all lines in order
    IEnumerator ShowAllLines(Dialogue dlg)
    {
        foreach (var s in dlg.sentences)
        {
            yield return StartCoroutine(TypeLine(s));
            yield return new WaitForSeconds(2f);
        }
    }

    // Typewriter effect
    IEnumerator TypeLine(string sentence)
    {
        dialogueText.text = "";

        foreach (char c in sentence)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.03f);
        }
    }
}
