using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;

    public int health = 3;
    public int score = 0;

    public int coins = 0;

    public int wrongAnswers = 0;

    public Boolean timer;
    public Boolean shield;
    public Boolean hint;

    public struct Position
    {
        public float x;
        public float y;
        public float z;
    }

    public Position startPosition;

    // Per-level mapping from scene build index -> start spawn Z coordinate
    // Stored so checkpoint respawns can use the original level start Z for that scene.
    private System.Collections.Generic.Dictionary<int, float> startSpawnZByScene = new System.Collections.Generic.Dictionary<int, float>();

    // Setter for the current scene
    public void SetStartSpawnZForScene(int sceneIndex, float z)
    {
        startSpawnZByScene[sceneIndex] = z;
    }

    // Getter with fallback
    public float GetStartSpawnZForScene(int sceneIndex, float fallback)
    {
        if (startSpawnZByScene.TryGetValue(sceneIndex, out float z))
            return z;
        return fallback;
    }

    public Position lastCheckpoint;

    private bool hasCheckpoint = false;


    // --- NEW CONSTANTS FOR STORE COSTS ---
    public const int COST_EXTRA_HEART = 25;
    public const int COST_EXTRA_TIME = 25;
    // ------------------------------------

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        lastSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    int lastSceneIndex = -1;

    // Helper to find HealthScript reliably (Fastest, non-obsolete method)
    private HealthScript GetHealthScript()
    {
        // Assuming your health UI script is named HealthScript
        return UnityEngine.Object.FindAnyObjectByType<HealthScript>();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        lastSceneIndex = scene.buildIndex;
        // Reset per-run values when a scene loads so levels start fresh
        score = 0;
        wrongAnswers = 0;
        // Reset health and coins to defaults for a fresh start
        health = 3;
        coins = 0;

        var hs = GetHealthScript();
        if (hs != null)
        {
            hs.updateScore();
            hs.updateCoin();
            // Ensure health UI is refreshed when a new scene loads
            hs.updateHealth();
        }
    }

    public void DeductHealth(int amount)
    {
        health -= amount;
        if (health < 0) health = 0;

        var hs = GetHealthScript();
        if (hs != null)
        {
            hs.PlayHurtFeedback(); // Audio call is here
            hs.updateHealth();
            hs.updateScore();
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        var hs = GetHealthScript();
        if (hs != null) hs.updateScore();
    }

    public void AddCoin(int amount)
    {
        coins += amount;
        var hs = GetHealthScript();
        if (hs != null) hs.updateCoin();
    }

    public void DeductCoin(int amount)
    {
        coins -= amount;
        if (coins < 0) coins = 0;

        var hs = GetHealthScript();
        if (hs != null) hs.updateCoin();
    }

    // --- NEW METHOD FOR IN-GAME STORE ---
    public bool TryPurchase(int cost, string itemType)
    {
        // 1. Check affordability
        if (coins >= cost)
        {
            coins -= cost;

            // 2. Apply effect
            switch (itemType)
            {
                case "Heart":
                    health += 1;
                    break;
                case "Time":
                    // Finds the Timer script (which you provided) and calls its AddTime method
                    var timer = UnityEngine.Object.FindAnyObjectByType<Timer>();

                    if (timer != null)
                    {
                        timer.AddTime(30.0); // Purchase grants 30 seconds
                    }
                    else
                    {
                        Debug.LogError("Timer script ('Timer.cs') not found! Cannot add time. Cost refunded.");
                        coins += cost; // Refund cost
                        return false;
                    }
                    break;
            }

            // 3. Update the HUD
            var hs = GetHealthScript();
            if (hs != null)
            {
                hs.updateCoin();    // Refreshes coin count
                hs.updateHealth();  // Refreshes health display
            }

            Debug.Log($"Purchase successful: {itemType} bought for {cost} coins.");
            return true;
        }
        else
        {
            Debug.Log($"Purchase failed: Not enough coins to buy {itemType}.");
            return false;
        }
    }

    public void SetCheckpoint(Vector3 position)
    {
        // Store the exact trigger position as the checkpoint so respawns match the trigger area.
        lastCheckpoint.x = position.x;
        lastCheckpoint.y = position.y;
        lastCheckpoint.z = position.z;
        hasCheckpoint = true;
        Debug.Log($"Checkpoint set at {position}");
    }

    public Vector3 GetCheckpointPosition()
    {
        return new Vector3(lastCheckpoint.x, lastCheckpoint.y, lastCheckpoint.z);
    }

    public bool HasCheckpoint()
    {
        return hasCheckpoint;
    }

    public void ClearCheckpoint()
    {
        hasCheckpoint = false;
    }
}
