using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapArrow : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform playerTransform;

    [Header("Tags & Settings")]
    public string playerTag = "Player";
    public string finishLineTag = "Finish";
    [Tooltip("Add an extra rotation offset (degrees) if your arrow sprite's default orientation differs.")]
    public float rotationOffset = 0f;
    [Header("Fade Settings")]
    [Tooltip("Seconds it takes to fade the arrow when a question UI is visible.")]
    public float fadeDuration = 0.25f;
    [Tooltip("Alpha when arrow should be visible.")]
    public float visibleAlpha = 1f;
    [Tooltip("Alpha when arrow should be hidden (question UI visible).")]
    public float hiddenAlpha = 0f;

    [Header("Rotation/Smoothing")]
    [Tooltip("Speed of rotation smoothing (degrees per second). Set high for snappy arrow.)")]
    public float rotationSmoothSpeed = 720f;

    private RectTransform arrowRectTransform;
    private Transform _lastTargetLogged = null;
    private CanvasGroup _canvasGroup;
    private float _currentAlpha = 1f;
    private float _currentZ = 0f;

    void Start()
    {
        arrowRectTransform = GetComponent<RectTransform>();

        if (playerTransform == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag(playerTag);
            if (playerGO != null)
            {
                playerTransform = playerGO.transform;
            }
            else
            {
                Debug.LogError("MinimapArrow: Player not found! Ensure your player object is tagged '" + playerTag + "'.");
                enabled = false;
                return;
            }
        }

        // Debug: enumerate question objects and markers present at Start
        QuizQuestionBase[] qs = Object.FindObjectsByType<QuizQuestionBase>(FindObjectsSortMode.None);
        int sceneQCount = 0;
        foreach (var q in qs)
        {
            if (q == null || q.gameObject == null) continue;
            if (!q.gameObject.scene.IsValid()) continue;
            sceneQCount++;
            Debug.Log($"MinimapArrow Start: Found QuizQuestionBase in scene: {q.gameObject.name} at {q.transform.position} (answered={q.isAnswered})");
        }
        Debug.Log($"MinimapArrow Start: total scene QuizQuestionBase count = {sceneQCount}");

        QuestionMarker[] markers = Object.FindObjectsByType<QuestionMarker>(FindObjectsSortMode.None);
        int sceneMCount = 0;
        foreach (var m in markers)
        {
            if (m == null || m.gameObject == null) continue;
            if (!m.gameObject.scene.IsValid()) continue;
            sceneMCount++;
            Debug.Log($"MinimapArrow Start: Found QuestionMarker in scene: {m.gameObject.name} at {m.transform.position} (answered={m.isAnswered})");
        }
        Debug.Log($"MinimapArrow Start: total scene QuestionMarker count = {sceneMCount}");

        // Ensure a CanvasGroup exists for fade in/out
        _canvasGroup = arrowRectTransform.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = arrowRectTransform.gameObject.AddComponent<CanvasGroup>();
        }
        _canvasGroup.alpha = visibleAlpha;
        _currentAlpha = _canvasGroup.alpha;
    }

    void Update()
    {
        if (playerTransform == null) return;

        Transform currentTarget = FindNextObjective();

        if (currentTarget == null)
        {
            arrowRectTransform.gameObject.SetActive(false);
            return;
        }

        arrowRectTransform.gameObject.SetActive(true);

        // --- FINAL, CORRECTED X-AXIS ALIGNMENT LOGIC ---

        // 1. Calculate the vector from the player to the target
        Vector3 directionToTarget = currentTarget.position - playerTransform.position;

        // 2a. Calculate world-space angle for reference (XZ plane)
        float angleRadiansWorld = Mathf.Atan2(directionToTarget.z, directionToTarget.x);
        float angleDegreesWorld = angleRadiansWorld * Mathf.Rad2Deg;

        // 2b. Calculate screen-space direction from the ARROW'S screen position to the target
        float angleDegreesScreen = 0f;
        // Compute arrow and target in screen-space. Use `null` for RectTransformUtility so this works
        // with Screen Space - Overlay canvases too (no camera needed).
        Vector2 arrowScreenPos = RectTransformUtility.WorldToScreenPoint(null, arrowRectTransform.position);
        Vector3 targetScreen = Camera.main != null ? Camera.main.WorldToScreenPoint(currentTarget.position) : new Vector3(currentTarget.position.x, currentTarget.position.y, 1f);

        Vector2 dirScreen = new Vector2(targetScreen.x - arrowScreenPos.x, targetScreen.y - arrowScreenPos.y);

        // If the target is behind the camera, invert direction so arrow points toward screen-edge
        if (targetScreen.z < 0f)
        {
            dirScreen = -dirScreen;
        }

        if (dirScreen.sqrMagnitude > 0.000001f)
        {
            angleDegreesScreen = Mathf.Atan2(dirScreen.y, dirScreen.x) * Mathf.Rad2Deg;
        }

        // Debug detailed screen-space info when target changes
        if (currentTarget == _lastTargetLogged)
        {
            // no-op
        }
        else
        {
            Debug.Log($"MinimapArrow Debug: arrowScreenPos={arrowScreenPos}, targetScreen={targetScreen}, dirScreen={dirScreen}");
        }

        // Apply smoothing to rotation and an offset to match sprite orientation
        float appliedZScreen = angleDegreesScreen + rotationOffset;
        _currentZ = Mathf.MoveTowardsAngle(_currentZ, appliedZScreen, rotationSmoothSpeed * Time.deltaTime);
        arrowRectTransform.localEulerAngles = new Vector3(0f, 0f, _currentZ);

        // Debug: log both world-space and screen-space angles when target changes
        if (currentTarget != _lastTargetLogged)
        {
            _lastTargetLogged = currentTarget;
            Debug.Log($"MinimapArrow: WorldAngle={angleDegreesWorld:F2} deg, ScreenAngle={angleDegreesScreen:F2} deg, AppliedZ={_currentZ:F2} deg for target {currentTarget.name}");
        }

        // (additional target-change logging handled above)

        // Fade logic: hide the arrow while any question UI is visible
        bool anyQuestionUIVisible = false;
        QuizQuestionBase[] allQuestions = Object.FindObjectsByType<QuizQuestionBase>(FindObjectsSortMode.None);
        foreach (var q in allQuestions)
        {
            if (q == null || q.gameObject == null) continue;
            if (!q.gameObject.scene.IsValid()) continue;
            if (q.uiChoices != null && q.uiChoices.activeSelf)
            {
                anyQuestionUIVisible = true;
                break;
            }
        }

        float targetAlpha = anyQuestionUIVisible ? hiddenAlpha : visibleAlpha;
        if (_canvasGroup != null)
        {
            // Smoothly animate alpha
            _currentAlpha = Mathf.MoveTowards(_currentAlpha, targetAlpha, (1f / Mathf.Max(0.0001f, fadeDuration)) * Time.deltaTime);
            _canvasGroup.alpha = _currentAlpha;
        }
    }

    // Helper function to dynamically select the nearest objective (Logic remains correct)
    private Transform FindNextObjective()
    {
        // 1) Try to find QuizQuestionBase objects (scene instances) first
        QuizQuestionBase[] allQuestions = Object.FindObjectsByType<QuizQuestionBase>(FindObjectsSortMode.None);

        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach (var q in allQuestions)
        {
            if (q == null || q.gameObject == null) continue;
            var scene = q.gameObject.scene;
            if (!scene.IsValid()) continue; // ignore prefab/assets
            if (q.isAnswered) continue;

            float dist = Vector3.Distance(playerTransform.position, q.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = q.transform;
            }
        }

        // 2) If no QuizQuestionBase found (or none unanswered), fall back to QuestionMarker
        if (nearest == null)
        {
            QuestionMarker[] markers = Object.FindObjectsByType<QuestionMarker>(FindObjectsSortMode.None);
            foreach (var m in markers)
            {
                if (m == null || m.gameObject == null) continue;
                var scene = m.gameObject.scene;
                if (!scene.IsValid()) continue;
                if (m.isAnswered) continue;

                float dist = Vector3.Distance(playerTransform.position, m.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = m.transform;
                }
            }
        }

        // 3) If we found either a QuizQuestionBase or a marker, return it
        if (nearest != null) return nearest;

        // 4) Fallback: point to finish line
        GameObject finishGO = GameObject.FindGameObjectWithTag(finishLineTag);
        if (finishGO != null) return finishGO.transform;

        return null;
    }
}
