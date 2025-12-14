using UnityEngine;
using UnityEngine.EventSystems;

// Ensures there is exactly one EventSystem at runtime.
[DefaultExecutionOrder(-1000)]
public class EventSystemCleaner : MonoBehaviour
{
    void Awake()
    {
        // Find all EventSystem instances in the scene. Use the modern, build-safe API.
        EventSystem[] systems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

        if (systems == null || systems.Length <= 1) return;

        // Keep the first one and destroy any others to avoid duplicate EventSystem warnings
        bool keptOne = false;
        foreach (var s in systems)
        {
            if (s == null) continue;
            // Skip EventSystems that are not part of a loaded scene (e.g., prefabs in the project).
            if (!s.gameObject.scene.isLoaded) continue;

            if (!keptOne)
            {
                keptOne = true;
                continue;
            }

            // Destroy duplicates
            Debug.Log("EventSystemCleaner: Destroying duplicate EventSystem: " + s.gameObject.name);
            Destroy(s.gameObject);
        }
    }
}
