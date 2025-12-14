using UnityEngine;

// Attach this to scene objects representing question locations (empty GameObjects or the question trigger).
// The Minimap will use these markers when full QuizQuestionBase instances are not yet present.
public class QuestionMarker : MonoBehaviour
{
    [Tooltip("If true, the minimap will treat this marker as already answered and ignore it.")]
    public bool isAnswered = false;

    // Optional id to link with dynamic question instances (not required).
    public string markerId;

    // Mark the marker answered from code (e.g., from QuizQuestionBase when resolved).
    public void MarkAnswered()
    {
        isAnswered = true;
    }

    public void ResetMarker()
    {
        isAnswered = false;
    }
}
