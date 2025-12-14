using UnityEngine;
using UnityEngine.UI;

public class Falldetect : MonoBehaviour
{
    [SerializeField] private Transform respawn;
    public Button restartBtn;
    public GameObject deathUI;

    private GameObject player;

    private HealthScript healthscript;

    private void Start()
    {
        restartBtn.onClick.AddListener(RespawnPlayer);
        healthscript = FindFirstObjectByType<HealthScript>();
        // Expose the respawn transform for other systems to reference when computing safe spawn positions
        RespawnPoint = respawn;

        // Record the initial spawn Z in PlayerData (per-scene) so checkpoint respawns reuse the level's start Z
        if (PlayerData.instance != null)
        {
            int idx = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            PlayerData.instance.SetStartSpawnZForScene(idx, respawn.position.z);
        }
    }

    // Public static reference to the scene's respawn transform (set in Start)
    public static Transform RespawnPoint { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;

            Time.timeScale = 0f;

            deathUI.SetActive(true);
        }
    }

    private void RespawnPlayer()
    {
        if (player != null)
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            Vector3 spawnPos = respawn.position;
            if (PlayerData.instance != null && PlayerData.instance.HasCheckpoint())
            {
                // Start with the exact checkpoint position (match the trigger area)
                spawnPos = PlayerData.instance.GetCheckpointPosition();

                Debug.Log($"[Falldetect] Using checkpoint spawn at {spawnPos}");

                // NOTE: preserving the checkpoint Z coordinate — forcing it to the scene start Z
                // can move the player away from the intended trigger area and cause overlap checks
                // to fail. Keep checkpoint Z as set by the trigger to improve correctness.

                // If we have a CharacterController, try to ensure the chosen position is not intersecting geometry.
                if (controller != null)
                {
                    float radius = controller.radius;
                    float height = Mathf.Max(controller.height, radius * 2f);
                    Vector3 centerOffset = controller.center;

                    int attempts = 0;
                    const int maxAttempts = 16;
                    const float stepUp = 0.5f;
                    bool foundSafe = false;

                    // Try a few small horizontal offsets in addition to stepping up to escape tight geometry
                    Vector3[] lateralOffsets = new Vector3[] {
                        Vector3.zero,
                        Vector3.right * 0.25f,
                        Vector3.left * 0.25f,
                        Vector3.forward * 0.25f,
                        Vector3.back * 0.25f
                    };

                    while (attempts < maxAttempts && !foundSafe)
                    {
                        // On each attempt, try each lateral offset at the current vertical offset
                        float verticalOffset = (attempts / (float)maxAttempts) * (stepUp * maxAttempts);

                        foreach (var lateral in lateralOffsets)
                        {
                            Vector3 candidate = spawnPos + lateral + Vector3.up * verticalOffset;
                            Vector3 center = candidate + centerOffset;
                            Vector3 top = center + Vector3.up * (height * 0.5f - radius);
                            Vector3 bottom = center + Vector3.down * (height * 0.5f - radius);

                            Collider[] hits = Physics.OverlapCapsule(top, bottom, radius, ~0, QueryTriggerInteraction.Ignore);
                            if (hits == null || hits.Length == 0)
                            {
                                spawnPos = candidate;
                                foundSafe = true;
                                break;
                            }
                        }

                        attempts++;
                    }

                    if (!foundSafe)
                    {
                        Debug.LogWarning("Unable to find non-overlapping spawn at checkpoint; falling back to scene respawn.");
                        spawnPos = respawn.position + Vector3.up * 0.5f;
                    }
                    else
                    {
                        Debug.Log($"[Falldetect] Found safe checkpoint spawn at {spawnPos} after {attempts} attempts");
                    }
                }
            }

            // Position the player at the computed spawn position (do not override player rotation so facing remains consistent)
            player.transform.position = spawnPos;

            // Reset any Rigidbody velocities if present
            var rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            var rb2d = player.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                rb2d.linearVelocity = Vector2.zero;
                rb2d.angularVelocity = 0f;
            }

            // Reset player movement script internal state to avoid immediate small drift after teleport
            var movement = player.GetComponent<NewMonoBehaviourScript>();
            if (movement != null)
            {
                movement.ResetMovementState();
            }

            if (controller != null)
            {
                controller.enabled = true;
            }
        }

        deathUI.SetActive(false);
        PlayerData.instance.DeductHealth(1);
        healthscript.updateHealth();

        Time.timeScale = 1f;

        player = null;
    }
}
