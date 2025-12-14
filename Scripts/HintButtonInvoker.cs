using UnityEngine;

public class HintButtonInvoker : MonoBehaviour
{
    // Call this from the Hint button's OnClick event in the Inspector.
    public void InvokeHint()
    {
        Debug.Log("[HintInvoker] InvokeHint called");
        var controller = UnityEngine.Object.FindAnyObjectByType<QuestionUIController>();
        if (controller != null)
        {
            Debug.Log("[HintInvoker] Found controller, calling UseHintPublic");
            controller.UseHintPublic();
        }
        else
        {
            Debug.Log("[HintInvoker] No QuestionUIController found");
        }
    }
}
