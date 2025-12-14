using UnityEngine;
using UnityEngine.SceneManagement; // Add this line for the optional reload

public class GameDataResetter : MonoBehaviour
{
    // Call this public function from a button's OnClick event
    public void ResetGameProgress()
    {
        // This deletes ALL saved PlayerPrefs data (scores, settings, and unlocks).
        PlayerPrefs.DeleteAll();

        // Ensure the game is unpaused before resetting/reloading
        Time.timeScale = 1f;

        // Reload the Main menu scene to reflect the locked levels immediately
        SceneManager.LoadScene("Main menu"); 

        Debug.Log("Game progress has been reset.");
    }
}
