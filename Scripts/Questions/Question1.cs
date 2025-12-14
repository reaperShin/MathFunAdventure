using UnityEngine;

public class Lever1 : QuizQuestionBase
{
    // The Inspector fields (buttons, text, question data) are inherited from QuizQuestionBase.
    
    protected override bool EvaluateAnswer(int index)
    {
        // Checks the selected answer index against the index you set in the Inspector.
        return index == correctChoiceIndex;
    }
}
