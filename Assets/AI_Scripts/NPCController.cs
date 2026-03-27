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
    public bool moneyVisible;
    public bool beenToSafeArea = false;

    [Header("Debug Info")]
    public string currentState;
    public string currentAction;

    [Header("Line of Sight Settings")]
    public float viewDistance = 15f;
    public float viewAngle = 90f;
    public LayerMask obstructionMask; // Make sure your walls are on this layer

    public void UpdateLineOfSight()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distToPlayer <= viewDistance)
        {
            // Check if player is inside the FOV cone
            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
            {
                // Raycast to check for walls (Obstructions)
                // We start the ray slightly up (Vector3.up) so it's at "eye level"
                if (!Physics.Raycast(transform.position + Vector3.up, dirToPlayer, distToPlayer, obstructionMask))
                {
                    playerVisible = true;
                    return;
                }
            }
        }
        playerVisible = false;
    }
    private void Update()
    {
        // Example: Update player distance every frame for the BT to evaluate
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerDistance = Vector3.Distance(transform.position, player.transform.position);
        }
    }
}