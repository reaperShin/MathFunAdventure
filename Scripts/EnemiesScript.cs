using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavAgent : MonoBehaviour
{
    private bool isDead = false;
    public Transform player;
    public float detectRadius = 15f;
    public float attackRange = 2f;
    public float hopDistance = 3f;
    public float hopHeight = 1.5f;
    public float hopDuration = 0.5f;
    public float hopCooldown = 1f;
    public LayerMask obstructionMask;
    [Header("Rotation")]
    public float rotateDuration = 0.15f;
    public float rotationSpeedDuringHop = 8f;

    private HealthScript healthscript;
    private NavMeshAgent agent;

    private bool hopping = false;
    [Header("Stomp")]
    public float stompHeight = 0.5f;
    public float stompVelocityThreshold = -0.5f;
    public GameObject deathVFX;
    public float deathVFXDuration = 3f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // animator removed - animations handled elsewhere or not used
        var uiObj = GameObject.Find("UILogic");
        healthscript = uiObj != null ? uiObj.GetComponent<HealthScript>() : null;

        if (agent != null)
            agent.isStopped = true; // we’ll control movement manually
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= detectRadius && HasLineOfSight())
        {
            if (dist > attackRange && !hopping)
            {
                StartCoroutine(HopTowardsPlayer());
            }
            else if (dist <= attackRange)
            {
                FaceTarget();
                // Animation code removed - keep behavior simple
            }
        }
        else
        {
            // No animator - idle behavior can be handled here if needed
        }
    }

    IEnumerator HopTowardsPlayer()
    {
        hopping = true;

        Vector3 startPos = transform.position;
        Vector3 dir = (player.position - transform.position).normalized;

        // Face the player horizontally before starting the hop (smooth)
        Vector3 lookDir = (player.position - transform.position);
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            if (rotateDuration > 0f)
            {
                Quaternion startRot = transform.rotation;
                float rt = 0f;
                while (rt < rotateDuration)
                {
                    transform.rotation = Quaternion.Slerp(startRot, targetRot, rt / rotateDuration);
                    rt += Time.deltaTime;
                    yield return null;
                }
                transform.rotation = targetRot;
            }
            else
            {
                transform.rotation = targetRot;
            }
        }

        UpdateWalkingAnim(dir); // update left/right (no animator calls)

        Vector3 targetPos = startPos + dir * hopDistance;

        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, hopDistance, NavMesh.AllAreas))
            targetPos = hit.position;

        float elapsed = 0f;
        while (elapsed < hopDuration)
        {
            float t = elapsed / hopDuration;
            Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * hopHeight; // parabola
            transform.position = pos;

            // Smoothly correct rotation during hop towards targetRot
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotDuring = Quaternion.LookRotation(new Vector3(lookDir.x, 0f, lookDir.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotDuring, rotationSpeedDuringHop * Time.deltaTime);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        yield return new WaitForSeconds(hopCooldown);

        // Face player after landing (smooth)
        Vector3 postLookDir = (player.position - transform.position);
        postLookDir.y = 0f;
        if (postLookDir.sqrMagnitude > 0.0001f)
        {
            Quaternion postTarget = Quaternion.LookRotation(postLookDir);
            if (rotateDuration > 0f)
            {
                Quaternion start = transform.rotation;
                float rt = 0f;
                while (rt < rotateDuration)
                {
                    transform.rotation = Quaternion.Slerp(start, postTarget, rt / rotateDuration);
                    rt += Time.deltaTime;
                    yield return null;
                }
                transform.rotation = postTarget;
            }
            else
            {
                transform.rotation = postTarget;
            }
        }

        hopping = false;
    }

    void UpdateWalkingAnim(Vector3 dir)
    {
        // This method can be used to flip sprite or set direction flags on other systems.
        // Currently it does not set Animator parameters (animator removed).
        float dot = Vector3.Dot(transform.right, dir);
        // You can broadcast direction via events or set local scale here if needed.
    }

    bool HasLineOfSight()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, player.position);
        if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, dist, ~obstructionMask))
            return hit.transform == player;
        return true;
    }

    void FaceTarget()
    {
        Vector3 dir = (player.position - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Check for stomp
            CharacterController cc = other.GetComponent<CharacterController>();
            bool stomp = false;
            if (cc != null)
            {
                if (cc.velocity.y < stompVelocityThreshold && other.transform.position.y > transform.position.y + stompHeight)
                {
                    stomp = true;
                }
            }

            if (stomp)
            {
                Die();
            }
            else
            {
                if (healthscript != null)
                {
                    healthscript.TakeDamage(1);
                }
                else if (PlayerData.instance != null)
                {
                    PlayerData.instance.DeductHealth(1);
                }
            }
        }
    }

    void Die()
    {
        if (isDead) { Debug.Log("EnemyNavAgent.Die() called but already dead: ignoring."); return; }
        isDead = true;
        if (deathVFX != null)
        {
            var v = Instantiate(deathVFX, transform.position, Quaternion.identity);
            Destroy(v, deathVFXDuration);
        }
        else
        {
            DeathVFXFactory.CreateDeathVFX(transform.position, deathVFXDuration);
        }
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPickup();
        Destroy(gameObject);
    }
}
