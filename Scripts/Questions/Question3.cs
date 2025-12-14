using UnityEngine;

public class Lever3 : QuizQuestionBase
{
    protected override bool EvaluateAnswer(int index)
    {
        return index == correctChoiceIndex;
    }
}
