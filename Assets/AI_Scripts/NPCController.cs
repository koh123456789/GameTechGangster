using UnityEngine;

// Keep the Enum outside the class
public enum BossAction { Wandering, Chase, EnterAttackRange, Attack, Search, Retreat, CallBackup, Upgrade, TrackMoney, CollectMoney, Dead }

public class NPCController : MonoBehaviour
{
    [Header("Core Stats")]
    public float maxHealth = 100f;
    public float bossNpcHealth = 100f;
    public float attackRange = 2.5f;
    public bool isDead = false;
    public bool damageReceived;
    public float playerDistance;
    public bool playerVisible;

    [Header("Sensing Settings")]
    public float viewDistance = 15f;
    public float viewAngle = 90f;
    public LayerMask obstructionMask;
    protected float losePlayerTimer;
    public float losePlayerDelay = 1.0f;

    [Header("Debug & State")]
    public string currentState;
    public BossAction currentAction;

    [Header("Components")]
    protected GameObject playerRef;
    protected SteeringAgent steering;
    protected Animator anim;
    protected BossAudio bossAudio;

    protected virtual void Awake() => bossNpcHealth = maxHealth;

    protected virtual void Start()
    {
        steering = GetComponent<SteeringAgent>();
        anim = GetComponent<Animator>();
        bossAudio = GetComponent<BossAudio>();
        playerRef = GameObject.FindGameObjectWithTag("Player");
    }

    public virtual void TakeDamage(float amount)
    {
        if (isDead) return;

        bossNpcHealth -= amount;

        // ADD THIS LOGIC:
        damageReceived = true;
        CancelInvoke("ResetDamageFlag");
        Invoke("ResetDamageFlag", 5f); // Boss searches for 5 seconds after being hit

        if (bossNpcHealth <= 0) Die();
    }

    private void ResetDamageFlag()
    {
        damageReceived = false;
    }

    protected virtual void Die()
    {
        isDead = true;
        if (bossAudio != null) bossAudio.PlayOneShot(bossAudio.deathScream);
        if (anim != null) anim.SetTrigger("isDead");
        if (steering != null) { steering.Stop(); steering.enabled = false; }
        GetComponent<Collider>().enabled = false;
    }

    public void UpdateLineOfSight()
    {
        if (playerRef == null) return;

        Vector3 dirToPlayer = (playerRef.transform.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, playerRef.transform.position);

        // 1. Check Distance
        if (distanceToPlayer <= viewDistance)
        {
            // 2. Check Angle (FOV)
            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
            {
                // 3. Check Obstructions (Raycast)
                // Raycast starting slightly above ground (waist height)
                if (Physics.Raycast(transform.position + Vector3.up * 1f, dirToPlayer, out RaycastHit hit, viewDistance, obstructionMask))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        playerVisible = true;
                        losePlayerTimer = losePlayerDelay; // Reset the timer while visible
                        return;
                    }
                }
            }
        }

        // 4. If any check fails, start the "Losing Player" timer
        losePlayerTimer -= Time.deltaTime;
        if (losePlayerTimer <= 0)
        {
            playerVisible = false;
        }
    }

}