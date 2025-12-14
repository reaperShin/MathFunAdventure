using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class pinkEnemy : MonoBehaviour
{
    private bool isDead = false;
    public Transform player;
    public float detectRadius = 12f;
    public float chaseSpeed = 3.5f;
    public float chargeDistance = 3f;
    public float chargeSpeed = 10f;
    public float chargeDuration = 0.6f;
    public float chargeCooldown = 3f;
    public int damage = 1;
    public LayerMask obstructionMask;

    [Header("Rotation")]
    public float rotateDuration = 0.12f;
    public float rotationSpeed = 10f;

    private NavMeshAgent agent;
    private bool isCharging = false;
    private float lastCharge = -999f;
    private HealthScript healthscript;
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
            agent.speed = chaseSpeed;
            agent.stoppingDistance = 0.5f;
            agent.updateRotation = false;
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

        if (!isCharging)
        {
            if (dist <= detectRadius && HasLineOfSight())
            {
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                    SmoothRotateToVelocity();
                }
                else
                {
                    Vector3 dir = (player.position - transform.position);
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.01f)
                    {
                        transform.position += dir.normalized * chaseSpeed * Time.deltaTime;
                        SmoothRotateTowards(dir);
                    }
                }

                if (dist <= chargeDistance && Time.time - lastCharge >= chargeCooldown)
                {
                    StartCoroutine(Charge());
                }
            }
            else
            {
                if (agent != null && agent.isOnNavMesh) agent.ResetPath();
            }
        }
    }

    System.Collections.IEnumerator Charge()
    {
        isCharging = true;
        lastCharge = Time.time;

        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;

        Vector3 dir = (player != null) ? (player.position - transform.position).normalized : transform.forward;
        float elapsed = 0f;

        while (elapsed < chargeDuration)
        {
            float move = chargeSpeed * Time.deltaTime;
            transform.position += dir * move;
            transform.forward = dir;

            if (player != null && Vector3.Distance(transform.position, player.position) <= 1.2f)
            {
                DealDamage();
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
        isCharging = false;
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

    void SmoothRotateToVelocity()
    {
        if (agent == null) return;
        Vector3 vel = agent.velocity;
        if (vel.sqrMagnitude > 0.01f)
        {
            SmoothRotateTowards(vel);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Check for stomp: player above enemy and falling down
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
                DealDamage();
            }
        }
    }

    void Die()
    {
        if (isDead) { Debug.Log("pinkEnemy.Die() called but already dead: ignoring."); return; }
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
        lastCharge = Time.time;
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
