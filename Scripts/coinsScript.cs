using UnityEngine;

public class coinsScript : MonoBehaviour
{

    HealthScript healthscript;

    void Start()
    {
        healthscript = FindFirstObjectByType<HealthScript>();
    }

    void OnTriggerEnter(Collider other)
    {
        // guard against unexpected nulls (can happen if objects are destroyed or not assigned)
        if (other == null) return;

        if (!other.CompareTag("Player"))
            return;

        if (PlayerData.instance != null)
        {
            PlayerData.instance.AddCoin(1);
        }
        else
        {
            Debug.LogWarning("coinsScript: PlayerData.instance is null when collecting a coin.");
        }

        // ensure we have a HealthScript reference; try to find it lazily if missing
        if (healthscript == null)
        {
            healthscript = FindFirstObjectByType<HealthScript>();
            if (healthscript == null)
                Debug.LogWarning("coinsScript: HealthScript not found in scene.");
        }

        if (healthscript != null)
            healthscript.updateCoin();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCoin();

        Destroy(gameObject);
    }
}
