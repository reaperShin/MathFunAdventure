using UnityEngine;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(-1000)]
public class EnsureSingleEventSystemRuntime : MonoBehaviour
{
    void Awake()
    {
        var systems = FindObjectsOfType<EventSystem>();
        if (systems.Length <= 1)
            return;

        // Keep the earliest created one (lowest instance id) or the first in the array
        EventSystem keeper = systems[0];
        for (int i = 1; i < systems.Length; i++)
        {
            if (systems[i] != null && systems[i] != keeper)
                Destroy(systems[i].gameObject);
        }

        Debug.Log($"Removed {systems.Length - 1} duplicate EventSystem(s) at runtime.");
    }
}
