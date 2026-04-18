//using UnityEngine;

//public class BossController : NPCController
//{
//    [Header("Boss Specific Logic")]
//    public Transform safeArea;
//    public bool isUpgraded = false;
//    public bool beenToSafeArea = false;
//    public float lowHealthThreshold = 30f;
//    protected bool wasRetreating = false;


//    [Header("Money Sensing")]
//    public float moneyDetectionRadius = 10f;
//    public LayerMask moneyLayer;
//    public bool moneyVisible;
//    public Vector3 currentMoneyPos;

//    [Header("Weapons")]
//    public GameObject meleeWeapon;
//    public GameObject longRangeWeapon;
//    public float closeRangeDamage = 5f;
//    public float longRangeDamage = 10f;
//    public float phase2AttackRange = 15f;

//    [Header("Projectiles")]
//    public GameObject projectilePrefab;
//    public Transform firePoint;

//    public float attackRangeBuffer = 2f; // Softcode the "dead zone"

//    [Header("Minion Settings")]
//    public GameObject minionPrefab;
//    public Transform[] spawnPoints;
//    public float followDistance;
//    public bool hasCalledBackup = false;
//    public Transform bossTransform;

//    [Header("API Variables")]
//    public float attackCooldown = 2.0f; // Ensure this is here too

//    public void CallBackup()
//    {
//        // 1. Null/Empty Check
//        if (hasCalledBackup || minionPrefab == null || spawnPoints.Length == 0) return;

//        foreach (Transform point in spawnPoints)
//        {
//            if (point == null) continue; // Skip if a spawn point was deleted by mistake

//            GameObject minion = Instantiate(minionPrefab, point.position, point.rotation);
//            BossController m = minion.GetComponent<BossController>();

//            if (m != null)
//            {
//                m.isBoss = false;
//                m.playerRef = this.playerRef;
//                m.bossTransform = this.transform;
//            }
//        }
//        hasCalledBackup = true;
//    }

//    protected override void Start() // Use override since NPCController has a Start
//    {
//        base.Start(); // This runs the player search and component fetching from NPCController

//        // Ensure Phase 1 state is correct
//        if (meleeWeapon) meleeWeapon.SetActive(true);
//        if (longRangeWeapon) longRangeWeapon.SetActive(false);
//    }

//    private void Update()
//    {
//        if (isDead) return;

//        if (isUpgraded)
//        {
//            attackRange = phase2AttackRange;
//        }

//        // 1. Shared Sensing
//        UpdateLineOfSight();
//        if (playerRef != null)
//        {
//            playerDistance = Vector3.Distance(transform.position, playerRef.transform.position);
//            if (playerVisible) lastKnownPlayerPos = playerRef.transform.position;
//        }

//        // 2. Boss Sensing
//        UpdateMoneySensing();

//        if (currentAction == BossAction.Retreat && !beenToSafeArea)
//        {
//            if (Vector3.Distance(transform.position, safeAreaPosition) < 2f)
//            {
//                beenToSafeArea = true;
//                // Now that beenToSafeArea is true, the Behavior Tree 
//                // will see it's time to trigger PerformUpgrade()
//            }
//        }

//        // 3. Audio/Visuals
//        HandleBossAudio();
//        if (anim != null && steering != null)
//        {
//            float speed = steering.GetVelocity().magnitude;
//            // If speed is basically zero, force it to 0
//            anim.SetFloat("Speed", (speed < 0.1f) ? 0f : speed);
//        }
//    }

//    public void UpdateMoneySensing()
//    {
//        // Ensure radius is positive to avoid OverlapSphere errors
//        float radius = Mathf.Max(0.1f, moneyDetectionRadius);
//        Collider[] coins = Physics.OverlapSphere(transform.position, radius, moneyLayer);

//        moneyVisible = coins.Length > 0;
//        if (moneyVisible && coins[0] != null)
//            currentMoneyPos = coins[0].transform.position;
//    }

//    public void PerformUpgrade()
//    {
//        if (isUpgraded) return;

//        // 1. Logic Guard: Ensure phase2AttackRange is not negative or zero
//        phase2AttackRange = Mathf.Max(5f, phase2AttackRange);

//        isUpgraded = true;
//        beenToSafeArea = true;
//        bossNpcHealth = maxHealth;
//        attackRange = phase2AttackRange;

//        if (steering != null)
//        {
//            // 2. Defensive Math: Ensure stopping distance is always positive
//            steering.stoppingDistance = Mathf.Max(1f, attackRange - 3f);
//            steering.Stop();
//        }

//        if (meleeWeapon) meleeWeapon.SetActive(false);
//        if (longRangeWeapon) longRangeWeapon.SetActive(true);
//        if (anim) anim.SetBool("isUpgraded", true);
//        if (bossAudio) bossAudio.PlayOneShot(bossAudio.upgradeSound);
//    }

//    public void DealDamageToPlayer()
//    {
//        if (playerRef == null) return;
//        PlayerController player = playerRef.GetComponent<PlayerController>();
//        FaceTarget(playerRef.transform.position);

//        if (!isUpgraded)
//        {
//            // Guard against negative melee damage
//            float damage = Mathf.Max(0, closeRangeDamage);
//            if (player != null) player.TakeDamage(damage);

//            if (anim) anim.SetTrigger("SwordTrigger");
//            if (bossAudio) bossAudio.PlayOneShot(bossAudio.swordSwoosh);
//        }
//        else
//        {
//            // Guard against negative projectile damage
//            float damage = Mathf.Max(0, longRangeDamage);
//            ShootProjectile(damage);

//            if (anim) anim.SetTrigger("ShootTrigger");
//            if (bossAudio) bossAudio.PlayOneShot(bossAudio.shootingSound);
//        }
//    }

//    public void ShootProjectile(float dmg)
//    {
//        if (projectilePrefab == null || firePoint == null || playerRef == null) return;

//        // 1. Calculate the actual direction to the player's chest (up 1 unit)
//        Vector3 targetCenter = playerRef.transform.position + Vector3.up * 1f;
//        Vector3 shotDirection = (targetCenter - firePoint.position).normalized;

//        // 2. Instantiate the bullet
//        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(shotDirection));

//        // 3. Set damage
//        Projectile p = bullet.GetComponent<Projectile>();
//        if (p)
//        {
//            p.damage = dmg;
//            p.firedBy = ProjectileSource.Boss;
//        }
//    }

//    public void CollectMoney()
//    {
//        if (!moneyVisible) return;
//        Collider[] coins = Physics.OverlapSphere(currentMoneyPos, 2f, moneyLayer);
//        foreach (var c in coins) { c.gameObject.SetActive(false); break; }
//        if (bossAudio) bossAudio.PlayOneShot(bossAudio.moneyCollect);
//        moneyVisible = false;
//    }

//    public void TrackMoney()
//    {
//        if (moneyVisible && steering != null)
//        {
//            steering.Seek(currentMoneyPos);
//        }
//    }

//    private void HandleBossAudio()
//    {
//        bool isRetreating = (currentAction == BossAction.Retreat);
//        if (isRetreating && !wasRetreating && bossAudio) bossAudio.PlayOneShot(bossAudio.retreatGoofyRun);
//        wasRetreating = isRetreating;
//    }

//    public void FaceTarget(Vector3 targetPos)
//    {
//        Vector3 direction = (targetPos - transform.position).normalized;
//        direction.y = 0; // Keep him upright so he doesn't tilt into the ground
//        Quaternion lookRotation = Quaternion.LookRotation(direction);
//        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
//    }

//    public void HandleMovementLogic()
//    {
//        if (playerRef == null || steering == null) return;

//        // Use a percentage of the actual attackRange so he stops BEFORE the limit
//        // If range is 15, he stops at 12. If range is 5, he stops at 4.
//        float stopDistance = attackRange * 0.8f;

//        if (playerDistance > attackRange)
//        {
//            // Player is too far: Move closer
//            steering.maxSpeed = isUpgraded ? 3.5f : 5f;
//            steering.Seek(playerRef.transform.position);
//        }
//        else if (playerDistance < stopDistance)
//        {
//            // Player is in range: Stop moving and prepare to attack
//            steering.Stop();
//        }

//        // Always face the player during combat
//        FaceTarget(playerRef.transform.position);
//    }

//    public Vector3 safeAreaPosition
//    {
//        get { return safeArea != null ? safeArea.position : transform.position; }
//    }

//    //private void OnValidate()
//    //{
//    //    // 1. Validate Parent Variables (from NPCController)
//    //    // Ensure Max Health is at least 1 so the boss doesn't spawn dead
//    //    maxHealth = Mathf.Max(1f, maxHealth);

//    //    // Ensure view distance and angle are functional
//    //    viewDistance = Mathf.Max(1f, viewDistance);
//    //    viewAngle = Mathf.Clamp(viewAngle, 10f, 360f);

//    //    // 2. Validate Boss Specific Variables
//    //    // SOFT CLAMP: Retreat Threshold cannot be higher than Max Health
//    //    // This uses 'maxHealth' which is inherited from your NPCController!
//    //    lowHealthThreshold = Mathf.Clamp(lowHealthThreshold, 0f, maxHealth);

//    //    // 3. Combat & Sensing Safety
//    //    closeRangeDamage = Mathf.Max(0f, closeRangeDamage);
//    //    longRangeDamage = Mathf.Max(0f, longRangeDamage);
//    //    attackCooldown = Mathf.Max(0.1f, attackCooldown);
//    //    phase2AttackRange = Mathf.Max(2f, phase2AttackRange);
//    //    moneyDetectionRadius = Mathf.Max(1f, moneyDetectionRadius);
//    //}
//}