using UnityEngine;
using UnityEngine.UI;

public class TimesUpUI : MonoBehaviour
{
    [Tooltip("Assign the 'Time's Up' UI Panel GameObject here.")]
    public GameObject timesUpPanel;

    [Tooltip("Assign the Button that will restart the level from the Time's Up UI.")]
    public Button restartBtn;
    private HealthScript healthScript;
    private float previousTimeScale = 1f;
    private float previousFixedDelta = 0.02f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (timesUpPanel != null)
        {
            timesUpPanel.SetActive(false);
        }

        if (restartBtn != null)
        {
            restartBtn.onClick.RemoveAllListeners();
            restartBtn.onClick.AddListener(RestartLevel);
        }
        healthScript = FindFirstObjectByType<HealthScript>();
        // Auto-subscribe to the Timer's onTimerEnd event so Show() is called when time runs out.
        var timer = FindFirstObjectByType<Timer>();
        if (timer != null)
        {
            timer.onTimerEnd.AddListener(Show);
        }
        else
        {
            Debug.LogWarning("TimesUpUI: No Timer found to subscribe to onTimerEnd. Make sure the Timer exists or wire the event in the Inspector.");
        }
    }

    /// <summary>
    /// Shows the 'Time's Up' UI, plays a sound, and pauses the game.
    /// This should be called by the Timer script when it finishes.
    /// </summary>
    public void Show()
    {
        Debug.Log("TimesUpUI.Show() called — displaying Times Up UI and pausing game.");
        if (timesUpPanel != null) timesUpPanel.SetActive(true);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayTimesUp();
        // Freeze game time and physics
        previousTimeScale = Time.timeScale;
        previousFixedDelta = Time.fixedDeltaTime;
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;
        // Optionally pause audio globally
        AudioListener.pause = true;
    }

    void RestartLevel()
    {
        // Restore time settings before reloading
        Time.timeScale = previousTimeScale;
        Time.fixedDeltaTime = previousFixedDelta;
        AudioListener.pause = false;
        PlayerData.instance.health = 3;
        PlayerData.instance.coins = 0;
        healthScript.updateHealth();
        healthScript.updateCoin();
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
