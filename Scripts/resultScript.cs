using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class resultScript : MonoBehaviour
{
    public GameObject resultUI;

    public Button _homebtn;
    public Button _levelSelectorbtn;
    public Button _nextbtn;

    private VideoManager videoManager;

    void Start()
    {
        videoManager = FindObjectOfType<VideoManager>();

        AutoFindButtons();
        SetupButtons();
    }

    void AutoFindButtons()
    {
        if (resultUI != null)
        {
            Button[] childButtons = resultUI.GetComponentsInChildren<Button>(true);
            foreach (var b in childButtons)
            {
                string lname = b.gameObject.name.ToLower();
                if (_homebtn == null && (lname.Contains("home") || lname.Contains("main"))) _homebtn = b;
                if (_levelSelectorbtn == null && (lname.Contains("restart") || lname.Contains("retry"))) _levelSelectorbtn = b;
                if (_nextbtn == null && (lname.Contains("next") || lname.Contains("forward"))) _nextbtn = b;
            }
        }
    }

    void SetupButtons()
    {
        // HOME BUTTON
        if (_homebtn != null)
        {
            _homebtn.onClick.RemoveAllListeners();
            _homebtn.onClick.AddListener(() =>
            {
                SceneManager.LoadSceneAsync("Main menu");
            });
        }

        // RESTART BUTTON
        if (_levelSelectorbtn != null)
        {
            _levelSelectorbtn.onClick.RemoveAllListeners();
            _levelSelectorbtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Level Selection");
            });
        }

        // NEXT BUTTON
        if (_nextbtn != null)
        {
            _nextbtn.onClick.RemoveAllListeners();
            _nextbtn.onClick.AddListener(() =>
            {
                int current = SceneManager.GetActiveScene().buildIndex;

                // 🚀 LEVEL 10 SPECIAL BEHAVIOR
                // Use the VideoManager flag when available instead of a hard-coded build index.
                bool isLevel10Scene = false;
                if (videoManager != null)
                {
                    isLevel10Scene = videoManager.isLevel10;
                }
                else
                {
                    // Fallback: detect by scene name if VideoManager isn't present
                    var name = SceneManager.GetActiveScene().name.ToLower();
                    isLevel10Scene = name.Contains("level10") || name.Contains("level_10");
                }

                if (isLevel10Scene)
                {
                    // Ensure timeScale is restored before playing video or changing scenes
                    Time.timeScale = 1f;
                    if (videoManager != null)
                    {
                        Debug.Log("resultScript: Triggering Level10 video via VideoManager.");
                        videoManager.TriggerLevel10Vid();
                    }
                    else
                    {
                        Debug.LogWarning("resultScript: VideoManager not found for Level10. Falling back to Main Menu.");
                        SceneManager.LoadSceneAsync("Main menu");
                    }
                    return;
                }

                // Default Next Level Behavior
                int nextIndex = current + 1;
                // Restore timeScale in case a previous flow paused the game (e.g. finish UI sets timeScale = 0)
                Time.timeScale = 1f;

                if (nextIndex < SceneManager.sceneCountInBuildSettings)
                {
                    SceneManager.LoadSceneAsync(nextIndex);
                }
                else
                {
                    SceneManager.LoadSceneAsync("Main menu");
                }
            });
        }
    }
}
