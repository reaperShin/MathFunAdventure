using UnityEngine;
using System.Collections;

/// <summary>
/// Simple, reusable camera shake component. Call Shake(duration, magnitude) to trigger a one-shot shake.
/// It will stop any in-progress shake and restore the camera's transform when finished.
/// </summary>
public class CameraShake : MonoBehaviour
{
    // Public helper - starts a one-shot shake
    public void Shake(float duration, float magnitude)
    {
        // stop any running shake and start a fresh one
        StopAllCoroutines();
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        if (duration <= 0f || magnitude <= 0f)
            yield break;

        Transform camT = this.transform;
        Vector3 originalPos = camT.localPosition;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            // ease out the intensity so shake decays over time
            float damper = 1f - Mathf.Clamp01(progress);

            Vector3 offset = Random.insideUnitSphere * magnitude * damper;
            camT.localPosition = originalPos + offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // restore exactly to the original position
        camT.localPosition = originalPos;
    }
}
