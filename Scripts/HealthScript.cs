using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HealthScript : MonoBehaviour
{
    // --- UI/Health Display Fields ---
    public GameObject heartLeft;
    public GameObject heartMiddle;
    public GameObject heartRight;
    public TextMeshProUGUI scoretext;
    public TextMeshProUGUI cointext;

    // --- Damage Effects Fields ---
    [Header("Damage Effects")]
    public float staggerDuration = 0.5f;
    public Image screenDamageIndicator;
    [Tooltip("How long it takes for the player's red tint to fade back to normal.")]
    public float playerFadeDuration = 0.3f;

    [Header("Knockback Settings")]
    public float knockbackPower = 5f;
    public float knockbackUpwardForce = 3f;

    // The component that will be found and disabled (Your NewMonoBehaviourScript)
    private MonoBehaviour playerMovementController;

    // Invulnerability flag
    private bool isInvulnerable = false;
    private bool isShieldActive = false;
    private bool staggerInvul = false;

    // --- Hurt Feedback Fields ---
    [Header("Hurt Feedback")]
    public GameObject playerObject; // Assign your main Player GameObject here
    public Color hurtColor = Color.red;
    public float hurtFlashDuration = 0.8f;
    public float heartShakeDuration = 0.8f;
    public float heartShakeMagnitude = 6f;

    [Header("Camera Shake")]
    public float cameraShakeDuration = 0.4f;
    public float cameraShakeMagnitude = 0.25f;

    [Header("Shield Visuals")]
    public Image shieldIcon;
    public Color shieldIconColor = new Color(0.2f, 0.6f, 1f, 0.85f);
    public Color storedShieldIconColor = new Color(0.2f, 0.6f, 1f, 0.4f);
    public GameObject shieldVFX;
    public GameObject shieldVFXPrefab;
    private GameObject activeShieldVFXInstance;

    public GameObject gameOverUI;

    int health;

    private Renderer[] playerRenderers;
    private Dictionary<Renderer, Material[]> originalMaterials;

    void Start()
    {
        // --- STAGGER FIX: ABSOLUTE COMPONENT LOOKUP (via index 0) ---
        if (playerObject != null)
        {
            // Get ALL MonoBehaviour scripts on the player object
            MonoBehaviour[] allScripts = playerObject.GetComponents<MonoBehaviour>();

            // The Input (Script) is the first MonoBehaviour script, so its index is 0.
            const int MOVEMENT_SCRIPT_INDEX = 0;

            if (allScripts.Length > MOVEMENT_SCRIPT_INDEX)
            {
                playerMovementController = allScripts[MOVEMENT_SCRIPT_INDEX];

                if (playerMovementController is NewMonoBehaviourScript)
                {
                    Debug.Log("HealthScript: Movement script successfully acquired at index 0.");
                }
            }

            if (playerMovementController == null)
            {
                Debug.LogError("HealthScript: FATAL ERROR. Could not find NewMonoBehaviourScript at index 0. Staggering will not work. DO NOT change the component order on the Player object.");
            }
        }

        if (PlayerData.instance != null && scoretext != null)
            scoretext.text = PlayerData.instance.score.ToString();

        if (screenDamageIndicator != null)
        {
            Color c = screenDamageIndicator.color;
            screenDamageIndicator.color = new Color(c.r, c.g, c.b, 0f);
        }

        updateHealth();
    }

    public void TakeDamage(int damage)
    {
        // INVULNERABILITY CHECK
        if (isInvulnerable)
        {
            Debug.Log("HealthScript: Player is currently invulnerable. Damage ignored.");
            return; // Ignore damage
        }

        if (PlayerData.instance != null)
        {
            PlayerData.instance.DeductHealth(damage);
        }
    }

    public void ActivateShield(float seconds)
    {
        if (seconds <= 0f) return;
        if (isShieldActive)
        {
            StopCoroutine("ShieldCoroutine");
        }
        StartCoroutine(ShieldCoroutine(seconds));
    }

    IEnumerator ShieldCoroutine(float seconds)
    {
        isShieldActive = true;
        isInvulnerable = true;
        if (shieldIcon != null)
        {
            shieldIcon.gameObject.SetActive(true);
            shieldIcon.color = shieldIconColor;
        }
        if (shieldVFXPrefab != null)
        {
            activeShieldVFXInstance = Instantiate(shieldVFXPrefab);
            if (playerObject != null)
            {
                activeShieldVFXInstance.transform.SetParent(playerObject.transform, false);
                activeShieldVFXInstance.transform.localPosition = Vector3.zero;
                activeShieldVFXInstance.transform.localRotation = Quaternion.identity;
            }
            activeShieldVFXInstance.SetActive(true);
            var ps = activeShieldVFXInstance.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            else
            {
                var children = activeShieldVFXInstance.GetComponentsInChildren<ParticleSystem>();
                foreach (var c in children) c.Play();
            }
        }
        else if (shieldVFX != null)
        {
            if (playerObject != null)
            {
                shieldVFX.transform.SetParent(playerObject.transform, false);
                shieldVFX.transform.localPosition = Vector3.zero;
                shieldVFX.transform.localRotation = Quaternion.identity;
            }
            shieldVFX.SetActive(true);
            var ps = shieldVFX.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            else
            {
                var children = shieldVFX.GetComponentsInChildren<ParticleSystem>();
                foreach (var c in children) c.Play();
            }
        }
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        isShieldActive = false;
        if (shieldIcon != null)
        {
            shieldIcon.gameObject.SetActive(false);
        }
        if (activeShieldVFXInstance != null)
        {
            var ps = activeShieldVFXInstance.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            else
            {
                var children = activeShieldVFXInstance.GetComponentsInChildren<ParticleSystem>();
                foreach (var c in children) c.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            Destroy(activeShieldVFXInstance, 0.2f);
            activeShieldVFXInstance = null;
        }
        else if (shieldVFX != null)
        {
            var ps = shieldVFX.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            else
            {
                var children = shieldVFX.GetComponentsInChildren<ParticleSystem>();
                foreach (var c in children) c.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            shieldVFX.SetActive(false);
            shieldVFX.transform.SetParent(null);
        }
        isInvulnerable = staggerInvul || isShieldActive;
    }

    public void TryUseStoredShield()
    {
        if (PlayerData.instance == null) return;
        if (PlayerData.instance.shield)
        {
            PlayerData.instance.shield = false;
            ActivateShield(10f);
        }
    }

    void Update()
    {
        if (PlayerData.instance != null && PlayerData.instance.shield)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryUseStoredShield();
            }
        }
    }

    void LateUpdate()
    {
        if (shieldIcon == null) return;
        if (isShieldActive)
        {
            shieldIcon.gameObject.SetActive(true);
            shieldIcon.color = shieldIconColor;
            return;
        }
        if (PlayerData.instance != null && PlayerData.instance.shield)
        {
            shieldIcon.gameObject.SetActive(true);
            shieldIcon.color = storedShieldIconColor;
            return;
        }
        shieldIcon.gameObject.SetActive(false);
    }

    public void updateHealth()
    {
        health = PlayerData.instance.health;

        if (health == 3)
        {
            heartLeft.SetActive(true);
            heartMiddle.SetActive(true);
            heartRight.SetActive(true);
        }
        else if (health == 2)
        {
            heartLeft.SetActive(true);
            heartMiddle.SetActive(true);
            heartRight.SetActive(false);
        }
        else if (health == 1)
        {
            heartLeft.SetActive(true);
            heartMiddle.SetActive(false);
            heartRight.SetActive(false);
        }
        else if (health <= 0)
        {
            heartLeft.SetActive(false);
            heartMiddle.SetActive(false);
            heartRight.SetActive(false);

            gameOverUI.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void updateScore()
    {
        scoretext.text = PlayerData.instance.score.ToString();
    }

    public void updateCoin()
    {
        cointext.text = PlayerData.instance.coins.ToString();
    }

    public void PlayHurtFeedback()
    {
        StartCoroutine(HurtFeedbackCoroutine());
        StartCoroutine(StaggerCoroutine());

        if (Camera.main != null)
        {
            CameraShake cs = Camera.main.GetComponent<CameraShake>();
            if (cs == null) cs = Camera.main.gameObject.AddComponent<CameraShake>();
            if (cs != null && cameraShakeDuration > 0f && cameraShakeMagnitude > 0f)
            {
                cs.Shake(cameraShakeDuration, cameraShakeMagnitude);
            }
        }

        if (AudioManager.Instance != null)
        {
            // AUDIO FIX
            AudioManager.Instance.PlayDamage();
        }
    }

    // Staggering Effect (Disable Movement Script AND apply knockback)
    IEnumerator StaggerCoroutine()
    {
        staggerInvul = true;
        isInvulnerable = true;
        if (playerMovementController != null)
        {
            playerMovementController.enabled = false;
        }

        // 2. Apply Knockback (requires CharacterController)
        CharacterController cc = playerObject.GetComponent<CharacterController>();
        if (cc != null)
        {
            // Determine the backward direction based on the player's rotation
            Vector3 knockbackDirection = -playerObject.transform.forward;

            // Add an upward component
            Vector3 knockbackVector = (knockbackDirection * knockbackPower) + (Vector3.up * knockbackUpwardForce);

            // Apply the force over a tiny duration to simulate an immediate push
            cc.Move(knockbackVector * Time.deltaTime * 5f);
        }

        // Wait for the duration of the stagger (which now also acts as the invulnerability timer)
        yield return new WaitForSeconds(staggerDuration);

        // 3. Re-enable the movement script and **END Invulnerability**
        if (playerMovementController != null)
        {
            playerMovementController.enabled = true;
        }
        staggerInvul = false;
        isInvulnerable = isShieldActive;
    }

    // Indicator and Fading Red Effect
    IEnumerator HurtFeedbackCoroutine()
    {
        // --- 1. SETUP PLAYER MESH FLASH (Initialization) ---
        if (playerRenderers == null)
        {
            GameObject targetObject = playerObject != null ? playerObject : this.gameObject;
            playerRenderers = targetObject.GetComponentsInChildren<Renderer>(true);
            originalMaterials = new Dictionary<Renderer, Material[]>();

            foreach (var rend in playerRenderers)
            {
                // Store a copy of the original shared materials
                Material[] origMats = new Material[rend.sharedMaterials.Length];
                for (int i = 0; i < rend.sharedMaterials.Length; i++)
                {
                    origMats[i] = rend.sharedMaterials[i];
                }
                originalMaterials[rend] = origMats;
            }
        }

        // Apply HURT COLOR (Initial Red Flash)
        // Create NEW materials instances for flashing to avoid changing assets
        foreach (var rend in playerRenderers)
        {
            if (rend == null) continue;
            Material[] mats = new Material[rend.sharedMaterials.Length];
            for (int i = 0; i < rend.sharedMaterials.Length; i++)
            {
                // Create new material instance from shared material
                mats[i] = new Material(rend.sharedMaterials[i]);
                if (mats[i].HasProperty("_Color"))
                {
                    mats[i].color = hurtColor;
                }
            }
            rend.materials = mats; // Apply the red instances
        }

        // --- 2. UI SHAKE & SCREEN FLASH ---
        Vector3 leftOrig = heartLeft ? heartLeft.transform.localPosition : Vector3.zero;
        Vector3 midOrig = heartMiddle ? heartMiddle.transform.localPosition : Vector3.zero;
        Vector3 rightOrig = heartRight ? heartRight.transform.localPosition : Vector3.zero;

        Image indicator = screenDamageIndicator;
        Color indicatorStartColor = new Color(1f, 0f, 0f, 0.6f);

        if (indicator != null)
        {
            indicator.color = indicatorStartColor;
        }

        float elapsed = 0f;
        float maxDuration = Mathf.Max(hurtFlashDuration, heartShakeDuration);

        while (elapsed < maxDuration)
        {
            // Heart Shake (Fades out)
            if (elapsed < heartShakeDuration)
            {
                float t = elapsed / heartShakeDuration;
                float currentMagnitude = heartShakeMagnitude * (1f - t);
                if (heartLeft) heartLeft.transform.localPosition = leftOrig + (Vector3)(Random.insideUnitCircle * currentMagnitude);
                if (heartMiddle) heartMiddle.transform.localPosition = midOrig + (Vector3)(Random.insideUnitCircle * currentMagnitude);
                if (heartRight) heartRight.transform.localPosition = rightOrig + (Vector3)(Random.insideUnitCircle * currentMagnitude);
            }

            // Screen Indicator Fade
            if (indicator != null && elapsed < hurtFlashDuration)
            {
                float t = elapsed / hurtFlashDuration;
                float alpha = Mathf.Lerp(indicatorStartColor.a, 0f, t);
                indicator.color = new Color(indicatorStartColor.r, indicatorStartColor.g, indicatorStartColor.b, alpha);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // --- 3. FADE PLAYER MESH BACK TO ORIGINAL COLOR ---
        float fadeElapsed = 0f;
        while (fadeElapsed < playerFadeDuration)
        {
            fadeElapsed += Time.deltaTime;
            float t = fadeElapsed / playerFadeDuration;

            foreach (var rend in playerRenderers)
            {
                if (rend == null || !originalMaterials.ContainsKey(rend)) continue;

                for (int i = 0; i < rend.materials.Length; i++)
                {
                    Material currentMat = rend.materials[i];

                    if (currentMat.HasProperty("_Color"))
                    {
                        Color origColor = originalMaterials[rend][i].HasProperty("_Color") ? originalMaterials[rend][i].color : Color.white;

                        currentMat.color = Color.Lerp(hurtColor, origColor, t);
                    }
                }
            }
            yield return null;
        }

        // --- 4. FINAL CLEANUP (Crucial Step for red tint fix) ---

        // RESTORE ORIGINAL MATERIALS
        foreach (var entry in originalMaterials)
        {
            if (entry.Key != null)
            {
                // Clean up the temporary material instances
                Material[] currentMaterials = entry.Key.materials;
                foreach (Material mat in currentMaterials)
                {
                    Destroy(mat);
                }

                // Assign the original, shared materials back to the renderer
                entry.Key.sharedMaterials = entry.Value;
            }
        }

        // Clean up UI elements
        if (indicator != null)
        {
            indicator.color = new Color(indicatorStartColor.r, indicatorStartColor.g, indicatorStartColor.b, 0f);
        }

        // Restore heart positions
        if (heartLeft) heartLeft.transform.localPosition = leftOrig;
        if (heartMiddle) heartMiddle.transform.localPosition = midOrig;
        if (heartRight) heartRight.transform.localPosition = rightOrig;
    }
}
