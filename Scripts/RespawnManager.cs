using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [Tooltip("Assign the player's GameObject here, or make sure the player has the 'Player' tag")]
    public GameObject player;

    [Tooltip("Transform to move the player to on respawn")]
    public Transform respawnPoint;

    // Optional: if your camera follow script exposes a ResetPosition method, it will be called
    // after moving the player so the camera doesn't clip through geometry.
    
    public GameObject cinemachineVirtualCamera;

    public void RespawnPlayer()
    {
        if (player == null)
        {
            // Try find by tag
            var found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
                player = found;
        }

        if (player == null)
        {
            Debug.LogWarning("[RespawnManager] No player assigned or found with tag 'Player'. Falling back to scene reload.");
            // As a safe fallback, reload the scene to reset everything
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            return;
        }

        if (respawnPoint == null)
        {
            Debug.LogWarning("[RespawnManager] respawnPoint not assigned. Falling back to scene reload.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            return;
        }

        // Move player to respawn position
        player.transform.position = respawnPoint.position;
        player.transform.rotation = respawnPoint.rotation;

        // Reset Rigidbody if exists
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Reset 2D Rigidbody if exists
        var rb2d = player.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }

        // If a Cinemachine virtual camera was assigned, try to force camera position so there's no clipping/blend
        if (cinemachineVirtualCamera != null && Camera.main != null)
        {
            // compute current camera offset relative to player so we keep same framing
            Vector3 camOffset = Camera.main.transform.position - player.transform.position;
            Vector3 desiredCamPos = player.transform.position + camOffset;
            Quaternion desiredCamRot = Camera.main.transform.rotation;

            // Try to find the Cinemachine virtual camera type at runtime (works even if Cinemachine isn't installed)
            var vcamType = System.Type.GetType("Cinemachine.CinemachineVirtualCamera, Cinemachine");
            if (vcamType != null)
            {
                var vcamComp = cinemachineVirtualCamera.GetComponent(vcamType);
                if (vcamComp != null)
                {
                    // ForceCameraPosition(Vector3 pos, Quaternion rot) exists on many Cinemachine vcams
                    var forceMethod = vcamType.GetMethod("ForceCameraPosition", new System.Type[]{ typeof(Vector3), typeof(Quaternion) });
                    if (forceMethod != null)
                    {
                        forceMethod.Invoke(vcamComp, new object[]{ desiredCamPos, desiredCamRot });
                    }
                    else
                    {
                        Debug.LogWarning("[RespawnManager] Cinemachine vcam found but ForceCameraPosition not available on this type.");
                    }
                }
            }
        }

        // Also ensure UI/game state is unpaused and Game Over UI is hidden if present
        Time.timeScale = 1f;

        // Try to hide a common GameOver UI if present on the player or in scene (optional)
        var healthScript = player.GetComponentInChildren<HealthScript>(true);
        if (healthScript != null)
        {
            // If HealthScript manages GameOver UI, call updateHealth to refresh visuals
            healthScript.updateHealth();
            healthScript.updateScore();
            healthScript.updateCoin();
        }
    }
}
