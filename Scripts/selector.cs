using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Selector : MonoBehaviour
{
    // ... (Button variables remain the same) ...
    [Header("Buttons")]
    public Button _1btn;
    public Button _2btn;
    public Button _3btn;
    public Button _4btn;
    public Button _5btn;
    
    // --- NEW BUTTONS FOR LEVELS 6-10 ---
    public Button _6btn;
    public Button _7btn;
    public Button _8btn;
    public Button _9btn;
    public Button _10btn;
    // ------------------------------------

    [Header("Lock Icons")]
    public Image _1lock;
    public Image _2lock;
    public Image _3lock;
    public Image _4lock;
    public Image _5lock;
    public Image _6lock;
    public Image _7lock;
    public Image _8lock;
    public Image _9lock;
    public Image _10lock;

    public Button _backbtn;

    void Start()
    {
        // Ensures the level selector scene is unpaused when it loads.
        Time.timeScale = 1f;

        // --- LEVEL UNLOCK LOGIC: Check and Apply Locks (10 Levels) ---
        
        // Level 1: Always unlocked (prerequisite is null)
        SetupLevelButton(_1btn, _1lock, "Level1", null); 
        
        // Levels 2-10: Locked until the previous level is completed.
        // NOTE: All scene names are now capitalized (LevelN) for consistency!
        SetupLevelButton(_2btn, _2lock, "Level2", "Level1"); 
        SetupLevelButton(_3btn, _3lock, "Level3", "Level2"); 
        SetupLevelButton(_4btn, _4lock, "Level4", "Level3"); 
        SetupLevelButton(_5btn, _5lock, "Level5", "Level4"); 
        
        SetupLevelButton(_6btn, _6lock, "Level6", "Level5"); 
        SetupLevelButton(_7btn, _7lock, "Level7", "Level6"); 
        SetupLevelButton(_8btn, _8lock, "Level8", "Level7"); 
        SetupLevelButton(_9btn, _9lock, "Level9", "Level8"); 
        SetupLevelButton(_10btn, _10lock, "Level10", "Level9"); 
        
        // -----------------------------------------------------------
        
        // Setup the back button functionality
        _backbtn.onClick.RemoveAllListeners();
        _backbtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Main menu");
        });
    }
    
    // ... (The SetupLevelButton function remains the same as it correctly uses the passed strings) ...
    private void SetupLevelButton(Button levelButton, Image lockImage, string targetSceneName, string prerequisiteSceneName)
    {
        if (levelButton == null) return; 

        levelButton.onClick.RemoveAllListeners();
        
        // default: hide lock image
        if (lockImage != null) lockImage.gameObject.SetActive(false);

        if (!string.IsNullOrEmpty(prerequisiteSceneName))
        {
            // The key used in PlayerPrefs is the prerequisite scene name + "_Completed"
            // This now uses the capitalized "LevelN" name!
            bool isUnlocked = PlayerPrefs.GetInt(prerequisiteSceneName + "_Completed", 0) == 1;

            if (isUnlocked)
            {
                levelButton.interactable = true;
                levelButton.onClick.AddListener(() =>
                {
                    SceneManager.LoadScene(targetSceneName); // Uses capitalized "LevelN"
                });
                if (lockImage != null) lockImage.gameObject.SetActive(false);
            }
            else
            {
                levelButton.interactable = false;
                if (lockImage != null) lockImage.gameObject.SetActive(true);
            }
        }
        else
        {
            // This is for Level 1 (no prerequisite)
            levelButton.interactable = true;
            levelButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(targetSceneName); // Uses capitalized "Level1"
            });
            if (lockImage != null) lockImage.gameObject.SetActive(false);
        }
    }
}
