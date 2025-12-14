using System.Collections.Generic;
using UnityEngine;

public class mimicEneny : MonoBehaviour
{
    public Transform player;
    public float recordInterval = 0.2f;
    public float followDelay = 1.0f; // seconds behind the player
    public float moveSpeed = 3f;
    public float attackRange = 1.2f;
    public int damage = 1;
    public float attackCooldown = 1f;
    public float detectRadius = 15f;
    public LayerMask obstructionMask;

    [Header("Rotation")]
    public float rotationSpeed = 8f;

    private Queue<Vector3> pathPoints = new Queue<Vector3>();
    private int maxPoints;
    private float lastRecord = 0f;
    private float lastAttack = -999f;
    private HealthScript healthscript;
    
    [Header("Wobble Settings")]
    public Transform leftSide;
    public Transform rightSide;
    public float wobbleFrequency = 6f;
    public float wobbleAmplitude = 0.08f;
    private Vector3 leftOrigPos;
    private Vector3 rightOrigPos;
    private float wobbleTime = 0f;

    void Start()
    {
        if (player == null)
        {
            var pgo = GameObject.FindGameObjectWithTag("Player");
            if (pgo != null) player = pgo.transform;
        }

        maxPoints = Mathf.Max(2, Mathf.CeilToInt(followDelay / Mathf.Max(0.01f, recordInterval)));
        var uiObj = GameObject.Find("UILogic");
        healthscript = uiObj != null ? uiObj.GetComponent<HealthScript>() : null;

        if (leftSide != null) leftOrigPos = leftSide.localPosition;
        if (rightSide != null) rightOrigPos = rightSide.localPosition;
    }

    void Update()
    {
        if (player == null) return;

        float playerDist = Vector3.Distance(transform.position, player.position);

        // record player path at intervals only when player is in detection radius
        if (playerDist <= detectRadius)
        {
            if (Time.time - lastRecord >= recordInterval)
            {
                pathPoints.Enqueue(player.position);
                lastRecord = Time.time;
                while (pathPoints.Count > maxPoints) pathPoints.Dequeue();
            }
        }

        // follow the oldest point
        if (pathPoints.Count > 0)
        {
            Vector3 target = pathPoints.Peek();
            Vector3 dir = target - transform.position;
            dir.y = 0f;
            bool isMoving = false;
            if (dir.sqrMagnitude > 0.01f)
            {
                transform.position += dir.normalized * moveSpeed * Time.deltaTime;
                SmoothRotateTowards(dir);
                isMoving = true;
            }

            // wobble side parts when moving
            if (isMoving)
            {
                wobbleTime += Time.deltaTime;
                float wob = Mathf.Sin(wobbleTime * wobbleFrequency) * wobbleAmplitude;
                if (leftSide != null) leftSide.localPosition = Vector3.Lerp(leftSide.localPosition, leftOrigPos + new Vector3(wob, 0f, 0f), Time.deltaTime * 12f);
                if (rightSide != null) rightSide.localPosition = Vector3.Lerp(rightSide.localPosition, rightOrigPos - new Vector3(wob, 0f, 0f), Time.deltaTime * 12f);
            }
            else
            {
                wobbleTime = 0f;
                if (leftSide != null) leftSide.localPosition = Vector3.Lerp(leftSide.localPosition, leftOrigPos, Time.deltaTime * 8f);
                if (rightSide != null) rightSide.localPosition = Vector3.Lerp(rightSide.localPosition, rightOrigPos, Time.deltaTime * 8f);
            }

            if (Vector3.Distance(transform.position, target) <= 0.5f)
            {
                pathPoints.Dequeue();
            }
        }

        // attack if close to player
        if (playerDist <= attackRange && Time.time - lastAttack >= attackCooldown)
        {
            DoAttack();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Time.time - lastAttack >= attackCooldown)
        {
            DoAttack();
        }
    }

    void DoAttack()
    {
        lastAttack = Time.time;
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

    void SmoothRotateTowards(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude <= 0.0001f) return;
        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);
    }
}
