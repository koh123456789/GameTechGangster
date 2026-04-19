using UnityEngine;
using System.Collections;

public enum BossAction { Wandering, Chase, Attack, Search, Retreat, CallBackup, Upgrade, TrackMoney, CollectMoney, Dead }

public class NPCController : MonoBehaviour
{
    [Header("Is this NPC the boss")]
    public bool isBoss = true;

    [Header("--- 1. SURVIVAL BRANCH ---")]
    public float NpcHealth = 100f; // This is the ONLY health variable now
    public float lowHealthThreshold = 30f;
    public Transform safeArea;
    public bool beenToSafeArea = false;
    public bool isUpgraded = false;
    public bool wasRetreating = false;

    [Header("--- 2. MONEY BRANCH ---")]
    public float NpcCash = 5f;
    public float cashTarget = 5f;    
    public bool moneyVisible;
    public LayerMask moneyLayer;
    private Vector3 currentMoneyPos; // Hidden from Inspector for cleanliness                                     
    private Collider[] coinResults = new Collider[10]; // Pre-allocated "bucket" for coins

    [Header("--- 3. COMBAT BRANCH ---")]
    public bool playerVisible;
    public bool damageReceived;
    public float meleeWeaponRange = 5f;
    public float rangedWeaponRange = 15f;
    public float attackCooldown = 1.2f;
    public ParticleSystem muzzleFlash;

    [Header("Weapons & Projectiles")]
    public GameObject meleeWeapon;
    public GameObject rangedWeapon;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float meleeWeaponDamage = 5f;
    public float rangedWeaponDamage = 10f;

    // NPCController.cs to allow the Editor to see them
    [SerializeField] private float originalMeleeDamage;
    [SerializeField] private float originalRangedDamage;
    [SerializeField] private float originalMeleeRange;
    [SerializeField] private float originalHealth;
    public float CurrentAttackRange => isUpgraded ? rangedWeaponRange : meleeWeaponRange;


    [Header("Sensing Settings")]
    public float viewDistance = 15f;
    public float viewAngle = 90f;
    public LayerMask obstructionMask;
    public Vector3 lastKnownPlayerPos;
    public float searchDuration = 5.0f;

    [Header("Minions Setting")]
    public float followDistance = 4f;
    public Transform bossTransform;
    public GameObject minionPrefab;
    public Transform[] spawnPoints;
    public bool hasCalledBackup = false;

    [Header("--- DEBUG TALLY ---")]
    public string currentState;
    public BossAction currentAction;
    public bool isDead = false;
    public float playerDistance;

    // Components
    protected SteeringAgent steering;
    protected GameObject playerRef;
    protected Animator anim;
    protected BossAudio bossAudio;

    // --- CACHED PROPERTY IDS ---
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int UpgradedHash = Animator.StringToHash("isUpgraded");
    private static readonly int DeadTrigger = Animator.StringToHash("isDead");


    protected virtual void Awake()
    {
        // 1. Take a "Snapshot" of whatever you typed in the Inspector
        originalMeleeDamage = meleeWeaponDamage;
        originalRangedDamage = rangedWeaponDamage;
        originalMeleeRange = meleeWeaponRange;
        originalHealth = NpcHealth; 
    }

    private void ValidateStats()
    {
        // The "Clamp" ensures that if someone enters -50, it becomes 0.
        NpcHealth = Mathf.Max(0, NpcHealth);
        meleeWeaponDamage = Mathf.Max(0, meleeWeaponDamage);
        rangedWeaponDamage = Mathf.Max(0, rangedWeaponDamage);
        meleeWeaponRange = Mathf.Max(0.1f, meleeWeaponRange);
        rangedWeaponRange = Mathf.Max(0.1f, rangedWeaponRange);
        NpcCash = Mathf.Max(0, NpcCash);
    }

    protected virtual void Start()
    {
        steering = GetComponent<SteeringAgent>();
        anim = GetComponent<Animator>();
        bossAudio = GetComponent<BossAudio>();
        playerRef = GameObject.FindGameObjectWithTag("Player");

        // Initial Weapon Setup
        if (meleeWeapon) meleeWeapon.SetActive(true);
        if (rangedWeapon) rangedWeapon.SetActive(false);

        StartCoroutine(SensingHeartbeat());
    }

    protected virtual void Update()
    {
        if (isDead) return;

        if (NpcHealth <= 0)
        {
            Die();
            return;
        }

        // Smoothly update animator
        if (anim != null && steering != null)
        {
            float speed = steering.GetVelocity().magnitude;
            anim.SetFloat(SpeedHash, speed < 0.1f ? 0f : speed);
        }

        // Handle Boss-only Audio logic (Checking once per frame is fine for flags)
        HandleBossAudio();
    }

    private IEnumerator SensingHeartbeat()
    {
        // 1. ADD A RANDOM START DELAY (Crucial for performance)
        // This prevents all NPCs from running their vision math on the exact same frame.
        yield return new WaitForSeconds(Random.Range(0f, 0.1f));

        // 2. SET INTERVAL BASED ON ROLE
        // Boss gets 10 checks per second (0.1s), Minions get 5 (0.2s).
        float interval = isBoss ? 0.1f : 0.2f;
        WaitForSeconds wait = new WaitForSeconds(interval);

        while (!isDead)
        {
            // Run the heavy vision math (Raycasts and OverlapSpheres)
            UpdateVision();

            if (playerRef != null)
            {
                // Cache distance once here instead of every frame in Update
                playerDistance = Vector3.Distance(transform.position, playerRef.transform.position);
            }

            // Boss-only retreat logic
            if (isBoss && currentAction == BossAction.Retreat && !beenToSafeArea)
            {
                // sqrMagnitude is faster than Vector3.Distance
                Vector3 distVec = transform.position - safeAreaPosition;
                if (distVec.sqrMagnitude < 4f) // 2.0m distance squared
                {
                    beenToSafeArea = true;
                }
            }

            // Wait for the next beat
            yield return wait;
        }
    }

    // --- LOGIC METHODS ---

    // 2. Update your UpdateVision method
    public void UpdateVision()
    {
        Vector3 eyePos = transform.position + Vector3.up * 1.0f;
        moneyVisible = false;

        // Use NonAlloc to stop the "Purple" spikes in your profiler
        int numCoinsFound = Physics.OverlapSphereNonAlloc(transform.position, viewDistance, coinResults, moneyLayer);

        if (numCoinsFound > 0 && NpcCash < cashTarget)
        {
            float closestSqrDist = Mathf.Infinity;
            Vector3 bestCoinPos = Vector3.zero;
            bool foundValidCoin = false;

            // Only loop through the actual number of coins found
            for (int i = 0; i < numCoinsFound; i++)
            {
                Collider coin = coinResults[i];
                if (coin == null || !coin.gameObject.activeInHierarchy) continue;

                Vector3 mPos = coin.transform.position;
                Vector3 mDir = (mPos - eyePos).normalized;
                float sqrDist = (mPos - eyePos).sqrMagnitude;

                if (sqrDist < closestSqrDist)
                {
                    if (Vector3.Angle(transform.forward, mDir) < viewAngle / 2)
                    {
                        // Only Raycast for the closest coin to save CPU power
                        if (!Physics.Raycast(eyePos, mDir, Mathf.Sqrt(sqrDist), obstructionMask))
                        {
                            closestSqrDist = sqrDist;
                            bestCoinPos = mPos;
                            foundValidCoin = true;
                        }
                    }
                }
            }

            if (foundValidCoin)
            {
                moneyVisible = true;
                currentMoneyPos = bestCoinPos;              
            }
            else
            {
                moneyVisible = false;
            }

        }

        UpdatePlayerLineOfSight(eyePos);
    }

    private void UpdatePlayerLineOfSight(Vector3 eyePos)
    {
        if (playerRef == null) return;

        Vector3 target = playerRef.transform.position + Vector3.up * 1.0f;
        Vector3 dir = (target - eyePos).normalized;
        float dist = Vector3.Distance(eyePos, target);

        bool inFov = dist <= viewDistance && Vector3.Angle(transform.forward, dir) < viewAngle / 2;
        bool tooClose = dist <= 2.5f;

        if (inFov || tooClose)
        {
            if (Physics.Raycast(eyePos, dir, out RaycastHit hit, viewDistance, obstructionMask))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    playerVisible = true;
                    lastKnownPlayerPos = playerRef.transform.position;
                    return; // Exit early, we found them
                }
            }
        }
        playerVisible = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (((1 << other.gameObject.layer) & moneyLayer) != 0)
        {
            NpcCash += 1f;
            if (bossAudio) bossAudio.PlayOneShot(bossAudio.moneyCollect);

            // Remove the 'moneyVisible = false' line. 
            // Instead, force a vision refresh immediately!
            UpdateVision();

            StartCoroutine(RespawnCoin(other.gameObject, 5.0f));
        }
    }

    // Called by Behavior Tree Action Node
    public void TrackMoney()
    {
        if (moneyVisible && steering != null)
        {
            steering.Seek(currentMoneyPos);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        // 1. Subtract Health and Clamp it so it never goes below 0
        float damageToApply = Mathf.Max(0, amount);
        NpcHealth -= damageToApply;
        NpcHealth = Mathf.Max(NpcHealth, 0f);

        damageReceived = true;

        // 2. Wrap Boss-only money loss with a check for negative values
        if (isBoss && NpcCash > 0)
        {
            NpcCash -= 1f;
            // Clamp money so it never hits -1
            NpcCash = Mathf.Max(NpcCash, 0f);
            Debug.Log($"Boss hit! Health: {NpcHealth}, Cash: {NpcCash}");
        }

        // 3. Null Check for Player Reference
        if (playerRef != null)
        {
            lastKnownPlayerPos = playerRef.transform.position;
        }
        else
        {
            // If player is missing (destroyed), find them again or stop
            playerRef = GameObject.FindGameObjectWithTag("Player");
        }

        CancelInvoke("ResetDamageFlag");
        Invoke("ResetDamageFlag", searchDuration);

        if (NpcHealth <= 0) Die();
    }

    private void ResetDamageFlag() => damageReceived = false;

    // --- ACTIONS (CALLED BY BEHAVIOR TREE) ---

    public void PerformUpgrade()
    {
        if (!isBoss || isUpgraded) return;

        isUpgraded = true;
        NpcHealth = 100f;

        // Set stats once here instead of checking every frame in Update        

        if (meleeWeapon) meleeWeapon.SetActive(false);
        if (rangedWeapon) rangedWeapon.SetActive(true);

        if (anim) anim.SetBool(UpgradedHash, true);
        if (bossAudio) bossAudio.PlayOneShot(bossAudio.upgradeSound);
    }

    public void DealDamageToPlayer()
    {
        if (playerRef == null) return;
        FaceTarget(playerRef.transform.position);

        if (!isUpgraded)
        {
            playerRef.GetComponent<PlayerController>()?.TakeDamage(Mathf.Max(0, meleeWeaponDamage));
            if (anim) anim.SetTrigger("SwordTrigger");
            if (bossAudio) bossAudio.PlayOneShot(bossAudio.swordSwoosh);
        }
        else
        {
            ShootProjectile(Mathf.Max(0, rangedWeaponDamage));
            if (anim) anim.SetTrigger("ShootTrigger");
            if (bossAudio) bossAudio.PlayOneShot(bossAudio.shootingSound);
        }
    }

    public void ShootProjectile(float dmg)
    {
        // Combined Null Check
        if (projectilePrefab == null || firePoint == null || playerRef == null) return;

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        Vector3 dir = (playerRef.transform.position + Vector3.up - firePoint.position).normalized;
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir));

        // Check if the bullet actually has the Projectile script
        Projectile pScript = bullet.GetComponent<Projectile>();
        if (pScript != null) pScript.damage = dmg;
    }

    public void CallBackup()
    {
        if (hasCalledBackup || minionPrefab == null) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        foreach (Transform pt in spawnPoints)
        {
            GameObject m = Instantiate(minionPrefab, pt.position, pt.rotation);
            NPCController c = m.GetComponent<NPCController>();
            if (c) { c.isBoss = false; c.bossTransform = transform; }
        }
        hasCalledBackup = true;
    }

    public void Search()
    {
        steering?.Arrive(lastKnownPlayerPos);
    }

    protected void Die()
    {
        isDead = true;
        if (anim) anim.SetTrigger("isDead");
        if (bossAudio) bossAudio.PlayOneShot(bossAudio.deathScream);
        steering?.Stop();
        if (isBoss) StartCoroutine(WaitAndWin(3.0f));
        else Destroy(gameObject, 3f);
    }

    private IEnumerator WaitAndWin(float delay)
    {
        yield return new WaitForSeconds(delay);
        FindObjectOfType<UIMenuController>()?.GoToWinScene();
    }

    public void FaceTarget(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
    }

    private void HandleBossAudio()
    {
        bool isRetreating = (currentAction == BossAction.Retreat);
        if (isRetreating && !wasRetreating && bossAudio) bossAudio.PlayOneShot(bossAudio.retreatGoofyRun);
        wasRetreating = isRetreating;
    }

    public Vector3 safeAreaPosition => safeArea != null ? safeArea.position : transform.position;

    public void HandleMovementLogic()
    {
        if (playerRef == null || steering == null) return;

        // We want the boss to stop slightly before the absolute edge of his range
        // Example: If range is 5m, he stops at 4m so the player is definitely hittable.
        float stopDistance = CurrentAttackRange * 0.8f;

        if (playerDistance > CurrentAttackRange)
        {
            // Player is escaping! Move closer.
            steering.maxSpeed = isUpgraded ? 4f : 5f; // Faster in Phase 1
            steering.Seek(playerRef.transform.position);
        }
        else if (playerDistance < stopDistance)
        {
            // Player is close enough! Stop moving so we can play attack animations.
            steering.Stop();
        }

        // Always keep looking at the target while in combat
        FaceTarget(playerRef.transform.position);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw from eye level so the editor matches the code logic
        Vector3 eyePos = transform.position + Vector3.up * 1.0f;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(eyePos, viewDistance);

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(eyePos, left * viewDistance);
        Gizmos.DrawRay(eyePos, right * viewDistance);

        // Debug line to player if seen
        if (playerVisible && playerRef != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(eyePos, playerRef.transform.position + Vector3.up * 1.0f);
        }
    }

    private IEnumerator RespawnCoin(GameObject coin, float delay)
    {
        // 1. Hide the coin
        coin.SetActive(false);

        // 2. Wait for the specified time
        yield return new WaitForSeconds(delay);

        // 3. Bring the coin back
        coin.SetActive(true);
    }

    public void ResetToPhase1()
    {
        // 2. Reset the Logic
        isUpgraded = false;
        beenToSafeArea = false;
        hasCalledBackup = false;

        // 3. Restore from the Snapshot (Soft-coding!)
        meleeWeaponDamage = Mathf.Max(0, originalMeleeDamage);
        rangedWeaponDamage = Mathf.Max(0, originalRangedDamage);
        meleeWeaponRange = Mathf.Max(0.1f, originalMeleeRange);
        NpcHealth = Mathf.Max(0, originalHealth);

        // 4. Handle the physical weapons
        if (meleeWeapon != null) meleeWeapon.SetActive(true);
        if (rangedWeapon != null) rangedWeapon.SetActive(false);

        Debug.Log("Boss Reset: Restored to your Inspector-defined defaults.");
    }
}