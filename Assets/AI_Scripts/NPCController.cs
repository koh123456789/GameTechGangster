using UnityEngine;

public enum BossAction
{
    Idle,
    Wandering,
    Chase,
    EnterAttackRange,
    Attack,
    Search,
    Retreat,
    CallBackup,
    Upgrade,
    TrackMoney,
    CollectMoney
}

public class NPCController : MonoBehaviour
{
    [Header("Boss Stats")]
    public float maxHealth = 100f; // Set default to 100
    public float bossNpcHealth = 100f;
    public float lowHealthThreshold = 30f;
    public float attackRange = 2.5f;
    public float phaseTwoAttackRange = 12f;
    public bool isDead = false;

    [Header("Player Status")]
    public float playerHealth = 100f;
    public float playerDistance;
    public bool playerVisible;
    public bool damageReceived;
    public float memoryTime = 5f;

    [Header("Environmental Status")]
    public Transform safeAreaTransform;
    public bool beenToSafeArea = false;

    [Header("Debug Info")]
    public string currentState;
    public BossAction currentAction;

    [Header("Line of Sight Settings")]
    public float viewDistance = 15f;
    public float viewAngle = 90f;
    public LayerMask obstructionMask; // Make sure your walls are on this layer
    public float combatProximityBuffer = 4.0f; // NEW: Stay in combat if this close
    private float losePlayerTimer;
    public float losePlayerDelay = 1.0f;

    [Header("Sensing Settings")]
    public float moneyDetectionRadius = 10f;
    public LayerMask moneyLayer; // Set this to a "Money" layer in Unity
    public bool moneyVisible;
    public Vector3 currentMoneyPos;

    [Header("Manual Override")]
    public bool isManualMode = false;
    public int forcedStateIndex = 0; // 0 = Auto, 1 = Patrol, 2 = Combat, 3 = Survival

    [Header("Combat Settings")]
    public float closeRangeDamage = 10f;
    public float longRangeDamage = 20f;
    public float currentDamage = 10f; // This is what DealDamageToPlayer will use

    [Header("Projectile Settings (Phase 2)")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 20f;

    [Header("Weapon Visuals")]
    public GameObject meleeWeapon;      // Drag your Sword here
    public GameObject longRangeWeapon;  // Drag your Gun/Bow here

    private GameObject playerRef;
    private SteeringAgent steering;
    private Animator anim;

    [Header("Phase 2 Settings")]
    public bool isUpgraded = false;

    void Awake()
    {
        // Initialize current health to max at the very start
        bossNpcHealth = maxHealth;
    }

    // Call this ONCE when arriving at the safe area
    public void PerformUpgrade()
    {
        if (isUpgraded) return;

        bossNpcHealth = maxHealth; // Refill
        closeRangeDamage *= 1.5f; // Buff damage
        longRangeDamage *= 1.5f;
        isUpgraded = true;
        beenToSafeArea = true;

        if (meleeWeapon != null) meleeWeapon.SetActive(false);
        if (longRangeWeapon != null) longRangeWeapon.SetActive(true);

        Debug.Log("<color=cyan>BOSS: Health Refilled & Upgraded to Hybrid Mode!</color>");
    }

    private void Start()
    {
        steering = GetComponent<SteeringAgent>();
        anim = GetComponent<Animator>();

        // PERFORMANCE FIX: Find player once at start, not every frame
        playerRef = GameObject.FindGameObjectWithTag("Player");

        if (meleeWeapon != null) meleeWeapon.SetActive(true);
        if (longRangeWeapon != null) longRangeWeapon.SetActive(false);
    }

    public void UpdateMoneySensing()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, moneyDetectionRadius, moneyLayer);
        int layerIndex = (int)Mathf.Log(moneyLayer.value, 2);

        //Debug.Log("Sensing Money... Found: " + hitColliders.Length + " objects on layer: " + LayerMask.LayerToName(layerIndex));
        if (hitColliders.Length > 0)
        {
            moneyVisible = true;

            // Find the closest one instead of just the first one
            float closestDist = Mathf.Infinity;
            foreach (Collider coin in hitColliders)
            {
                float dist = Vector3.Distance(transform.position, coin.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    currentMoneyPos = coin.transform.position;
                }
            }
        }
        else
        {
            moneyVisible = false;
        }
    }

    public void UpdateLineOfSight()
    {
        if (playerRef == null) return;

        // 1. PROXIMITY CHECK (The "Melee Fix")
        // If the player is right in his face, he "sees" them regardless of angles
        if (playerDistance <= combatProximityBuffer)
        {
            playerVisible = true;
            losePlayerTimer = losePlayerDelay;
            return;
        }

        Vector3 dirToPlayer = (playerRef.transform.position - transform.position).normalized;

        if (playerDistance <= viewDistance)
        {
            // 2. FIELD OF VIEW CHECK
            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
            {
                Vector3 eyePos = transform.position + Vector3.up * 1.6f;
                Vector3 targetPos = playerRef.transform.position + Vector3.up * 1.0f;
                Vector3 direction = (targetPos - eyePos).normalized;

                // 3. RAYCAST WITH MASK
                if (Physics.Raycast(eyePos, direction, out RaycastHit hit, viewDistance, obstructionMask))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        playerVisible = true;
                        losePlayerTimer = losePlayerDelay; // Keep timer full
                        Debug.DrawLine(eyePos, targetPos, Color.green);
                        return;
                    }
                }
            }
        }

        // 4. COMBAT MEMORY (The "Wandering Fix")
        // Don't turn playerVisible to false immediately. Count down first.
        losePlayerTimer -= Time.deltaTime;
        if (losePlayerTimer <= 0)
        {
            playerVisible = false;
            Debug.DrawLine(transform.position + Vector3.up * 1.6f, playerRef.transform.position + Vector3.up, Color.red);
        }
    }

    public Vector3 safeAreaPosition
    {
        get
        {
            if (safeAreaTransform != null) return safeAreaTransform.position;
            return transform.position; // Default to current spot if no safe area exists
        }
    }

    private void Update()
    {
        // PERFORMANCE FIX: Only run sensing if money exists (or use a timer)
        UpdateMoneySensing();

        if (playerRef != null)
        {
            playerDistance = Vector3.Distance(transform.position, playerRef.transform.position);
            UpdateLineOfSight();
        }

        if (anim != null && steering != null)
        {
            float currentMovementSpeed = steering.GetVelocity().magnitude;
            anim.SetFloat("Speed", currentMovementSpeed);
        }
    }

    private void ManageRangerDistance(Vector3 playerPos)
    {
        float keepAwayDistance = 6f; // If player is closer than 6m, back up

        if (playerDistance < keepAwayDistance)
        {
            //currentAction = "Kiting Player";
            steering.Leave(playerPos); // Move away
        }
    }

    public void DealDamageToPlayer()
    {
        if (!isUpgraded)
        {
            // Phase 1: Melee
            playerHealth -= closeRangeDamage;
            if (anim != null) anim.SetTrigger("SwordTrigger");
            Debug.Log("Sword Hit!");
        }
        else
        {
            // Phase 2: Shoot
            ShootProjectile(longRangeDamage);
            if (anim != null) anim.SetTrigger("ShootTrigger");
            Debug.Log("Gun Shot!");
        }
    }



    public bool IsPlayerDead()
    {
        return playerHealth <= 0;
    }

    public void SwitchToLongRange()
    {
        // This triggers the health refill and the model swap
        PerformUpgrade();

        // Update the steering to keep distance as a ranger
        if (steering == null) steering = GetComponent<SteeringAgent>();
        steering.arrivalDistance = 8f;
        steering.slowingDistance = 10f;

        Debug.Log("<color=red>PHASE 2: Hybrid Combat Initialized!</color>");
    }

    public void CollectMoney()
    {
        // Find the actual object at the location we are standing on
        Collider[] coins = Physics.OverlapSphere(transform.position, 1.5f, moneyLayer);

        foreach (Collider coin in coins)
        {
            // Option A: Just turn it off (Fastest)
            coin.gameObject.SetActive(false);

            // Reset the sensing variables immediately
            moneyVisible = false;

            Debug.Log("<color=gold>NPC: Money Collected and Deactivated!</color>");
            break; // Only pick up one at a time to keep it realistic
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the money sensing range in yellow
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, moneyDetectionRadius);

        // Draw the player view range in blue
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
    }

    public void TakeDamage(float amount)
    {
        bossNpcHealth -= amount;

        if (isDead) return;

        if (bossNpcHealth <= lowHealthThreshold)
        {
            // CLEAR EVERYTHING: Stop searching, stop visible player tracking
            damageReceived = false;
            playerVisible = false;
            CancelInvoke("ResetDamageFlag");
        }
        else
        {
            damageReceived = true;
            CancelInvoke("ResetDamageFlag");
            Invoke("ResetDamageFlag", 5f);
        }

        if (bossNpcHealth <= 0)
        {
            Die();
        }
    }

    private void ResetDamageFlag()
    {
        damageReceived = false;
        Debug.Log("Boss gave up searching.");
    }

    public void ShootProjectile(float dmg)
    {
        if (projectilePrefab != null && firePoint != null)
        {
            // Aim logic
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 targetPos = player.transform.position + Vector3.up * 1.2f;
                firePoint.LookAt(targetPos);
            }

            // Spawn
            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

            // Get Script
            Projectile projScript = bullet.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.damage = dmg;
                projScript.speed = projectileSpeed;

                // ADD THIS LINE:
                projScript.firedBy = ProjectileSource.Boss;
            }
        }
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("isDead"); // Make sure your Animator has a "Die" trigger

        // Stop the boss from moving
        GetComponent<SteeringAgent>().Stop();

        // Disable the collider so the player can walk through the body
        GetComponent<Collider>().enabled = false;

        Debug.Log("Boss has been defeated!");

        // Optional: Destroy the object after a few seconds
        // Destroy(gameObject, 5f); 
    }
}