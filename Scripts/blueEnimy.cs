using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class blueEnimy : MonoBehaviour
{
    public Transform player;
    public float detectRadius = 15f;
    public float attackRange = 1.2f;
    public float moveSpeed = 3.5f;
    public int damage = 1;
    public float attackCooldown = 1f;
    public LayerMask obstructionMask;

    [Header("Rotation")]
    public float rotateDuration = 0.15f;
    public float rotationSpeed = 8f;

    private NavMeshAgent agent;
    private float lastAttackTime = -999f;
    private HealthScript healthscript;
    private bool isDead = false;
    [Header("Stomp")]
    public float stompHeight = 0.5f;
    public float stompVelocityThreshold = -0.5f;
    public GameObject deathVFX;
    public float deathVFXDuration = 3f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.updateRotation = false;
            agent.updateUpAxis = true;
        }

        if (player == null)
        {
            var pgo = GameObject.FindGameObjectWithTag("Player");
            if (pgo != null) player = pgo.transform;
        }

        var uiObj = GameObject.Find("UILogic");
        healthscript = uiObj != null ? uiObj.GetComponent<HealthScript>() : null;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= detectRadius && HasLineOfSight())
        {
            if (dist > attackRange)
            {
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                }
                else
                {
                    Vector3 dir = (player.position - transform.position);
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.01f)
                    {
                        transform.position += dir.normalized * moveSpeed * Time.deltaTime;
                        SmoothRotateTowards(dir);
                    }
                }
            }
            else
            {
                if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
                SmoothRotateTowards(player.position - transform.position);
            }
        }
        else
        {
            if (agent != null && agent.isOnNavMesh) agent.ResetPath();
        }

        if (dist <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            DealDamage();
        }
    }

    bool HasLineOfSight()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, player.position);
        if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, dist, ~obstructionMask))
            return hit.transform == player;
        return true;
    }

    void SmoothRotateTowards(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude <= 0.0001f) return;
        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);
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
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    DealDamage();
                }
            }
        }
    }

    void Die()
    {
        if (isDead) { Debug.Log("blueEnimy.Die() called but already dead: ignoring."); return; }
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

    void DealDamage()
    {
        lastAttackTime = Time.time;
        if (healthscript != null)
        {
            healthscript.TakeDamage(damage);
        }
        else if (PlayerData.instance != null)
        {
            PlayerData.instance.DeductHealth(damage);
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayDamage();
    }
}
