using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class death : MonoBehaviour
{
    // CRITICAL: Ensure the button is assigned here in the Inspector
    public Button level;
    public Button revive;

    void Start()
    {
        level.onClick.AddListener(() =>
        {
            // This 'if' check ensures the reset ONLY happens when health is 0.
            if (PlayerData.instance != null && PlayerData.instance.health == 0)
            {
                // **These lines only run if the player has died.**
                PlayerData.instance.health = 3;
                PlayerData.instance.score = 0;
                PlayerData.instance.coins = 0;
                PlayerData.instance.ClearCheckpoint();
            }

            // This always runs to return to the selection screen.
            Time.timeScale = 1f;
            SceneManager.LoadScene("Level Selection");
        });

        revive.onClick.AddListener(() =>
        {
            // This logic is correct for the revive button.
            if (PlayerData.instance != null && PlayerData.instance.health == 0)
            {
                // Reset key player data on revive so UI and game state reflect a fresh life
                PlayerData.instance.health = 3;
                PlayerData.instance.score = 0;
                PlayerData.instance.coins = 0;
                PlayerData.instance.ClearCheckpoint();
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        });
    }
}
