using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("Boss Stats")]
    public float bossNpcHealth = 100f;
    public float lowHealthThreshold = 30f;
    public float attackRange = 5f;

    [Header("Player Status")]
    public float playerHealth = 100f;
    public float playerDistance;
    public bool playerVisible;
    public bool damageReceived;

    [Header("Environmental Status")]
    public Transform safeAreaTransform;
    public bool beenToSafeArea = false;

    [Header("Debug Info")]
    public string currentState;
    public string currentAction;

    [Header("Line of Sight Settings")]
    public float viewDistance = 15f;
    public float viewAngle = 90f;
    public LayerMask obstructionMask; // Make sure your walls are on this layer

    [Header("Sensing Settings")]
    public float moneyDetectionRadius = 10f;
    public LayerMask moneyLayer; // Set this to a "Money" layer in Unity
    public bool moneyVisible;
    public Vector3 currentMoneyPos;

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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distToPlayer <= viewDistance)
        {
            // 1. Check Field of View
            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
            {
                Vector3 eyePos = transform.position + Vector3.up * 1.6f;
                Vector3 targetPos = player.transform.position + Vector3.up * 1.0f;
                Vector3 direction = (targetPos - eyePos).normalized;

                // 2. Raycast check
                if (Physics.Raycast(eyePos, direction, out RaycastHit hit, viewDistance))
                {
                    //Debug.Log("Ray hit: " + hit.collider.gameObject.name);

                    if (hit.collider.CompareTag("Player"))
                    {
                        playerVisible = true;

                        // Draw Green line and EXIT the function early
                        Debug.DrawLine(eyePos, targetPos, Color.green);
                        return;
                    }
                }
            }
        }

        // 3. If we reach here, it means the player was NOT seen
        playerVisible = false;
        Debug.DrawLine(transform.position + Vector3.up * 1.6f, player.transform.position + Vector3.up, Color.red);
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
        UpdateMoneySensing();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerDistance = Vector3.Distance(transform.position, player.transform.position);

            // CRITICAL: Call this so the Boss "sees" the player
            UpdateLineOfSight();
        }
    }


    public void DealDamageToPlayer(float amount)
    {
        // Subtract from the variable that lives ON THIS SCRIPT
        playerHealth -= amount;

        Debug.Log("<color=red>BOSS ATTACK: Player hit! Health is now: " + playerHealth + "</color>");

        // Check for Game Over
        if (playerHealth <= 0)
        {
            Debug.Log("<color=black><b>GAME OVER: The Boss has defeated you!</b></color>");
            // Optional: Time.timeScale = 0; // Freeze the game
        }
    }

    public bool IsPlayerDead()
    {
        return playerHealth <= 0;
    }

    public void SwitchToLongRange()
    {
        // Short range was ~2m, Long range is now ~8m
        attackRange = 8f;

        // We also need to tell the Steering to stop further away
        GetComponent<SteeringAgent>().arrivalDistance = 7f;
        GetComponent<SteeringAgent>().slowingDistance = 10f;

        Debug.Log("<color=red>PHASE 2: Boss is using Long Range Weapon!</color>");
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
    }

    private void ResetDamageFlag()
    {
        damageReceived = false;
        Debug.Log("Boss gave up searching.");
    }

}