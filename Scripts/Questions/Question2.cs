using UnityEngine;

public class Lever2 : QuizQuestionBase
{
    protected override bool EvaluateAnswer(int index)
    {
        return index == correctChoiceIndex;
    }
}
