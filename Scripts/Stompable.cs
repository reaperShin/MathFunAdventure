using UnityEngine;

[AddComponentMenu("Gameplay/Stompable")]
public class Stompable : MonoBehaviour
{
    [Tooltip("Vertical offset: player must be above (enemyY + this) to count as stomp")]
    public float stompHeight = 0.4f;
    [Tooltip("Player downward velocity threshold (negative). If player's downward velocity is less than this value, it's considered a stomp.")]
    public float stompVelocityThreshold = -0.3f;

    [Tooltip("Optional VFX prefab spawned on stomp")]
    public GameObject deathVFX;
    [Tooltip("Seconds to keep VFX alive before destroying")]
    public float deathVFXDuration = 3f;

    [Tooltip("Optional: bounce player upwards slightly on stomp")]
    public bool bouncePlayer = true;
    public float bounceForce = 5f;

    [Tooltip("Optional: if set, this GameObject will be destroyed on stomp. If empty, the component's GameObject is destroyed.")]
    public GameObject targetToDestroy;

    [Tooltip("Optional sound to play on stomp using AudioManager.PlaySfx" )]
    public AudioClip stompSound;

    private bool isDead = false;

    void Reset()
    {
        // sensible defaults
        stompHeight = 0.4f;
        stompVelocityThreshold = -0.3f;
        deathVFXDuration = 3f;
    }

    void Awake()
    {
        EnsureHitboxExists();
    }

    // Ensure a child trigger ('StompHitbox') exists and forwards trigger events to this component
    void EnsureHitboxExists()
    {
        Transform existing = transform.Find("StompHitbox");
        if (existing != null) return;

        // Try to derive reasonable size/offset from existing collider or renderer
        Vector3 size = new Vector3(1f, stompHeight + 0.2f, 1f);
        Vector3 localCenter = Vector3.up * (stompHeight * 0.5f + 0.1f);

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Bounds b = col.bounds;
            size = new Vector3(b.size.x * 0.9f, stompHeight + 0.2f, b.size.z * 0.9f);
            float top = b.center.y + b.extents.y;
            localCenter = transform.InverseTransformPoint(new Vector3(b.center.x, top + (size.y * 0.5f) - transform.position.y, b.center.z));
            // localCenter computed in world->local may be off; simplify to upward offset
            localCenter = Vector3.up * (b.extents.y + size.y * 0.5f);
        }

        GameObject hb = new GameObject("StompHitbox");
        hb.transform.SetParent(transform, false);
        hb.transform.localPosition = localCenter;
        var box = hb.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = size;

        // Add forwarder
        var f = hb.AddComponent<StompHitboxForwarder>();
        f.owner = this;
    }

    // Public entry used by the hitbox forwarder
    public void ProcessTrigger(Collider other)
    {
        // keep existing behavior
        if (!other.CompareTag("Player")) return;

        // Try to read CharacterController velocity first
        var cc = other.GetComponent<CharacterController>();
        float playerVelY = 0f;
        if (cc != null)
        {
            playerVelY = cc.velocity.y;
        }
        else
        {
            var rb = other.attachedRigidbody;
            if (rb != null) playerVelY = rb.linearVelocity.y;
        }

        bool isAbove = other.transform.position.y > transform.position.y + stompHeight;
        bool isFalling = playerVelY < stompVelocityThreshold;

        if (isAbove && isFalling)
        {
            HandleStomp(other.gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Try to read CharacterController velocity first
        var cc = other.GetComponent<CharacterController>();
        float playerVelY = 0f;
        if (cc != null)
        {
            playerVelY = cc.velocity.y;
        }
        else
        {
            var rb = other.attachedRigidbody;
            if (rb != null) playerVelY = rb.linearVelocity.y;
        }

        bool isAbove = other.transform.position.y > transform.position.y + stompHeight;
        bool isFalling = playerVelY < stompVelocityThreshold;

        if (isAbove && isFalling)
        {
            HandleStomp(other.gameObject);
        }
    }

    void HandleStomp(GameObject player)
    {
        if (isDead) return;
        isDead = true;

        // Play VFX
        if (deathVFX != null)
        {
            var v = Instantiate(deathVFX, transform.position, Quaternion.identity);
            Destroy(v, deathVFXDuration);
        }
        else
        {
            DeathVFXFactory.CreateDeathVFX(transform.position, deathVFXDuration);
        }

        // Play sound via AudioManager if provided
        if (stompSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(stompSound, 1f);
        }
        else if (AudioManager.Instance != null)
        {
            // fallback: use pickup sound to indicate kill
            AudioManager.Instance.PlayPickup();
        }

        // Optional bounce
        if (bouncePlayer)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                // simple upward move. This doesn't change CC velocity reliably but gives a bounce feel
                cc.Move(Vector3.up * (bounceForce * Time.deltaTime));
            }
            else
            {
                var rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, bounceForce, rb.linearVelocity.z);
                }
            }
        }

        // Destroy target
        var target = targetToDestroy != null ? targetToDestroy : gameObject;
        Destroy(target);
    }
}
