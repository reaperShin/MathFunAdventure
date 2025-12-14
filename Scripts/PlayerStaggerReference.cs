using UnityEngine;

// This class name MUST match the file name (PlayerStaggerReference.cs)
public class PlayerStaggerReference : MonoBehaviour
{
    // This public slot will hold the reference to your movement script.
    // The name of your movement script class is NewMonoBehaviourScript.
    public NewMonoBehaviourScript movementScript;

    // A simple public function to confirm it has been assigned
    public NewMonoBehaviourScript GetMovementScript()
    {
        return movementScript;
    }
}
