using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Added for cleaner SceneManager usage

public class loseScript : MonoBehaviour
{
    public Button restartBtn;
    public GameObject loseUI;

    void OnEnable()
    {
        // ⭐ NEW: Play the dedicated Lose UI sound effect
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLoseUI();
        }
        
        // Setup the Restart Button listener
        restartBtn.onClick.RemoveAllListeners();
        restartBtn.onClick.AddListener(RestartLevel);
    }

    void RestartLevel()
    {
        // Stop the loss music/sound if it's still playing
        // While not strictly necessary due to the one-shot nature, 
        // it's good practice to handle scene transitions cleanly.
        
        Time.timeScale = 1f;
        loseUI.SetActive(false);
        
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
