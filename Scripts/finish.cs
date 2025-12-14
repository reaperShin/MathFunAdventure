using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class finish : MonoBehaviour
{
    [SerializeField] private GameObject _finishUI;
    [SerializeField] private GameObject _youLooseUI;
    [SerializeField] private TextMeshProUGUI _scoreText; // Ensure this is assigned in the Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Unlock Next Level
            string currentSceneName = SceneManager.GetActiveScene().name;

            PlayerPrefs.SetInt(currentSceneName + "_Completed", 1);
            PlayerPrefs.Save();

            // 2. Score Display and UI Activation
            PlayerData playerData = FindFirstObjectByType<PlayerData>();

            if (playerData != null && _scoreText != null)
            {
                // Set the final score text using the current score
                _scoreText.text = playerData.score.ToString();
            }
            else if (_scoreText == null)
            {
                Debug.LogError("Score Text (TextMeshProUGUI) is not assigned in the finish.cs Inspector!");
            }

            // Determine if the player lost (wrongAnswers >= 2)
            bool didLoose = (playerData != null && playerData.wrongAnswers >= 2);

            if (_finishUI != null && _youLooseUI != null)
            {
                if (didLoose)
                {
                    // Player Lost
                    _youLooseUI.SetActive(true);
                    // The audio (PlayLoseUI) is handled automatically by the loseScript's OnEnable() method.
                }
                else
                {
                    // Player Won
                    _finishUI.SetActive(true);

                    // Play the Victory/Finish sound ONLY on a win.
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayFinish();
                    }
                }
            }

            // 3. Freeze the game (Applies to both Win and Lose)
            Time.timeScale = 0f;
        }
    }
}
