using System;
using UnityEngine;
using UnityEngine.UI;

public class StorePanelManager : MonoBehaviour
{
    // ASSIGN THIS: Drag your MiniStorePanel GameObject here in the Inspector.
    public GameObject miniStorePanel;
    public Button health;
    public Button timer;
    public Button shield;
    [Tooltip("Optional: particle VFX prefab to spawn when a heart is purchased")]
    public GameObject heartVFX;
    PlayerData player;
    HealthScript healthScript;

    // Public function to be called by your main "Shop" button and the "Close" button.
    public void ToggleStorePanel()
    {
        if (miniStorePanel != null)
        {
            // Toggles the active state: true becomes false, false becomes true.
            bool isCurrentlyActive = miniStorePanel.activeSelf;
            miniStorePanel.SetActive(!isCurrentlyActive);

            // Optional: You could pause the game here if the panel opens
            // if (!isCurrentlyActive) Time.timeScale = 0f; else Time.timeScale = 1f;
        }
    }

    void OnEnable()
    {
        health.onClick.RemoveAllListeners();
        timer.onClick.RemoveAllListeners();
        shield.onClick.RemoveAllListeners();

        health.onClick.AddListener(() => PurchaseItem("Health", PlayerData.COST_EXTRA_HEART));
        shield.onClick.AddListener(() => PurchaseItem("Shield", 25)); // Assuming a new constant or keeping it at 25
        timer.onClick.AddListener(() => PurchaseItem("Timer", PlayerData.COST_EXTRA_TIME));

        player = FindFirstObjectByType<PlayerData>();
        healthScript = FindFirstObjectByType<HealthScript>();
    }

    void PurchaseItem(string itemName, int cost)
    {
        if (player == null)
        {
            Debug.Log("No PlayerData found!: Cannot process purchase.");
            return;
        }

        switch (itemName)
        {
            case "Health":
                // Prevent buying health if player is already at max (3)
                if (player.health >= 3)
                {
                    Debug.Log("Health is already full. Cannot purchase another heart.");
                    break;
                }

                if (IsAffordable(cost))
                {
                    player.DeductCoin(cost);
                    // Ensure we don't exceed the max health of 3
                    player.health = Mathf.Min(player.health + 1, 3);
                    Debug.Log("Health purchased!");
                    healthScript.updateHealth();
                    // Spawn heart VFX at the player's position (if assigned)
                    if (heartVFX != null)
                    {
                        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
                        if (playerGO != null)
                        {
                            Vector3 spawnPos = playerGO.transform.position + Vector3.up * 1.5f;
                            GameObject v = Instantiate(heartVFX, spawnPos, Quaternion.identity);
                            if (AudioManager.Instance != null) AudioManager.Instance.PlayHeartPurchase();
                            Destroy(v, 4f);
                        }
                        else if (Camera.main != null)
                        {
                            // Fallback: spawn in front of camera so the player still sees it
                            Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 2f + Vector3.up * 0.5f;
                            GameObject v = Instantiate(heartVFX, spawnPos, Quaternion.identity);
                            if (AudioManager.Instance != null) AudioManager.Instance.PlayHeartPurchase();
                            Destroy(v, 4f);
                        }
                    }
                }
                else
                {
                    Debug.Log("Not enough coins for Health.");
                }
                break;

            case "Timer":
                if (IsAffordable(cost))
                {
                    player.DeductCoin(cost);
                    var timer = FindFirstObjectByType<Timer>();
                    if (timer != null)
                    {
                        timer.AddTime(30.0);
                        timer.FlashTimerGreen(); // Visual feedback: flash timer green
                        Debug.Log("Timer purchased! 30 seconds added.");
                    }
                    else
                    {
                        Debug.LogError("Timer script not found!");
                        player.AddCoin(cost); // Refund
                    }
                }
                else
                {
                    Debug.Log("Not enough coins for Timer.");
                }
                break;

            case "Shield":
                if (IsAffordable(cost))
                {
                    player.DeductCoin(cost);
                    player.shield = true;
                    if (healthScript != null)
                    {
                        healthScript.TryUseStoredShield();
                    }
                    Debug.Log("Shield purchased and activated for 10 seconds!");
                }
                else
                {
                    Debug.Log("Not enough coins for Shield.");
                }
                break;

            // Hint removed from store; hint is now available from the question UI.

            default:
                Debug.Log("Unknown item: " + itemName);
                break;
        }
    }
    
    bool IsAffordable(int cost)
    {
        return player.coins >= cost;
    }
    
}
