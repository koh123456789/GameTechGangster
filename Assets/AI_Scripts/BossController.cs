using UnityEngine;

public class BossController : NPCController
{
    [Header("Boss Specific Logic")]
    public Transform safeArea;
    public Vector3 lastKnownPlayerPos;
    public bool isUpgraded = false;
    public bool beenToSafeArea = false;
    public float lowHealthThreshold = 30f;
    protected bool wasRetreating = false;


    [Header("Money Sensing")]
    public float moneyDetectionRadius = 10f;
    public LayerMask moneyLayer;
    public bool moneyVisible;
    public Vector3 currentMoneyPos;

    [Header("Weapons")]
    public GameObject meleeWeapon;
    public GameObject longRangeWeapon;
    public float closeRangeDamage = 5f;
    public float longRangeDamage = 10f;
    public float phase2AttackRange = 15f;

    [Header("Projectiles")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    public float attackRangeBuffer = 2f; // Softcode the "dead zone"

    [Header("Minion Settings")]
    public GameObject minionPrefab;
    public Transform[] spawnPoints;
    public bool hasCalledBackup = false;
    public Transform bossTransform;
    public bool isBoss = true; // Boss = true, Minion Prefab = false

    public void CallBackup()
    {
        if (hasCalledBackup || minionPrefab == null) return;

        foreach (Transform point in spawnPoints)
        {
            GameObject minion = Instantiate(minionPrefab, point.position, point.rotation);
            BossController m = minion.GetComponent<BossController>();

            // Add this 'if' check to be safe!
            if (m != null)
            {
                m.isBoss = false;
                m.isUpgraded = false;
                m.playerRef = this.playerRef;
                m.bossTransform = this.transform;
            }
        }
        hasCalledBackup = true;
    }
    protected override void Start() // Use override since NPCController has a Start
    {
        base.Start(); // This runs the player search and component fetching from NPCController

        // Ensure Phase 1 state is correct
        if (meleeWeapon) meleeWeapon.SetActive(true);
        if (longRangeWeapon) longRangeWeapon.SetActive(false);
    }

    private void Update()
    {
        if (isDead) return;

        // 1. Shared Sensing
        UpdateLineOfSight();
        if (playerRef != null)
        {
            playerDistance = Vector3.Distance(transform.position, playerRef.transform.position);
            if (playerVisible) lastKnownPlayerPos = playerRef.transform.position;
        }

        // 2. Boss Sensing
        UpdateMoneySensing();

        if (currentAction == BossAction.Retreat && !beenToSafeArea)
        {
            if (Vector3.Distance(transform.position, safeAreaPosition) < 2f)
            {
                beenToSafeArea = true;
                // Now that beenToSafeArea is true, the Behavior Tree 
                // will see it's time to trigger PerformUpgrade()
            }
        }

        // 3. Audio/Visuals
        HandleBossAudio();
        if (anim != null && steering != null)
        {
            float speed = steering.GetVelocity().magnitude;
            // If speed is basically zero, force it to 0
            anim.SetFloat("Speed", (speed < 0.1f) ? 0f : speed);
        }
    }

    public void UpdateMoneySensing()
    {
        Collider[] coins = Physics.OverlapSphere(transform.position, moneyDetectionRadius, moneyLayer);
        moneyVisible = coins.Length > 0;
        if (moneyVisible) currentMoneyPos = coins[0].transform.position;
    }

    public void PerformUpgrade()
    {
        if (isUpgraded) return;

        isUpgraded = true;
        beenToSafeArea = true;
        bossNpcHealth = maxHealth;
        attackRange = phase2AttackRange; 

        if (steering != null)
        {
            // We set it to 12 (15 - 3). 
            // This ensures he stops BEFORE he hits the flicker-zone at 15m.
            steering.stoppingDistance = attackRange - 3f;

            steering.Stop();
        }
        // ------------------------------------------------

        if (meleeWeapon) meleeWeapon.SetActive(false);
        if (longRangeWeapon) longRangeWeapon.SetActive(true);
        if (anim) anim.SetBool("isUpgraded", true);
        if (bossAudio) bossAudio.PlayOneShot(bossAudio.upgradeSound);
    }

    public void DealDamageToPlayer()
    {
        if (playerRef == null) return;
        PlayerController player = playerRef.GetComponent<PlayerController>();

        FaceTarget(playerRef.transform.position);

        if (!isUpgraded)
        {
            // PHASE 1: Melee
            if (player != null) player.TakeDamage(closeRangeDamage);
            if (anim) anim.SetTrigger("SwordTrigger"); // Swing the sword
            if (bossAudio) bossAudio.PlayOneShot(bossAudio.swordSwoosh);
        }
        else
        {
            // PHASE 2: Long Range
            ShootProjectile(longRangeDamage);

            if (anim) anim.SetTrigger("ShootTrigger"); // Fire the gun
            if (bossAudio) bossAudio.PlayOneShot(bossAudio.shootingSound);
        }
    }

    public void ShootProjectile(float dmg)
    {
        if (projectilePrefab == null || firePoint == null || playerRef == null) return;

        // 1. Calculate the actual direction to the player's chest (up 1 unit)
        Vector3 targetCenter = playerRef.transform.position + Vector3.up * 1f;
        Vector3 shotDirection = (targetCenter - firePoint.position).normalized;

        // 2. Instantiate the bullet
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(shotDirection));

        // 3. Set damage
        Projectile p = bullet.GetComponent<Projectile>();
        if (p)
        {
            p.damage = dmg;
            p.firedBy = ProjectileSource.Boss;
        }
    }

    public void CollectMoney()
    {
        if (!moneyVisible) return;
        Collider[] coins = Physics.OverlapSphere(currentMoneyPos, 2f, moneyLayer);
        foreach (var c in coins) { c.gameObject.SetActive(false); break; }
        if (bossAudio) bossAudio.PlayOneShot(bossAudio.moneyCollect);
        moneyVisible = false;
    }

    public void TrackMoney()
    {
        if (moneyVisible && steering != null)
        {
            steering.Seek(currentMoneyPos);
        }
    }

    private void HandleBossAudio()
    {
        bool isRetreating = (currentAction == BossAction.Retreat);
        if (isRetreating && !wasRetreating && bossAudio) bossAudio.PlayOneShot(bossAudio.retreatGoofyRun);
        wasRetreating = isRetreating;
    }

    public void FaceTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0; // Keep him upright so he doesn't tilt into the ground
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    public void HandleMovementLogic()
    {
        if (playerRef == null || steering == null) return;

        if (!isUpgraded)
        {
            // --- MINION / PHASE 1 (SWORD) ---
            // They need to get close!
            if (playerDistance > 1.5f)
            {
                steering.maxSpeed = 5f; // Sprint to the player
                steering.Seek(playerRef.transform.position);
            }
            else
            {
                steering.Stop(); // Close enough to swing
            }
        }
        else
        {
            // --- BOSS PHASE 2 (GUN) ---
            float stopBuffer = 7f;

            if (playerDistance > stopBuffer)
            {
                // Walk and shoot zone
                steering.maxSpeed = 3f;
                steering.Arrive(playerRef.transform.position);
            }
            else
            {
                steering.Stop(); // Personal space reached
            }
        }

        FaceTarget(playerRef.transform.position);
    }

    public Vector3 safeAreaPosition
    {
        get { return safeArea != null ? safeArea.position : transform.position; }
    }

}