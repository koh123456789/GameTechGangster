using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 10f;
    public float damageDealt = 20f;
    public LayerMask bossLayer; // Set this to the "Boss" layer in the Inspector

    void Update()
    {
        // 0 = Left Mouse Button
        if (Input.GetMouseButtonDown(0))
        {
            PerformAttack();
        }
    }

    void PerformAttack()
    {
        // Fire a ray from the center of the screen (where the mouse is)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, attackRange, bossLayer))
        {
            // Check if what we hit has the NPCController
            NPCController boss = hit.collider.GetComponent<NPCController>();

            if (boss != null)
            {
                Debug.Log("<color=cyan>Player: Direct Hit on Boss!</color>");
                boss.TakeDamage(damageDealt);
            }
        }
        else
        {
            Debug.Log("Player: Missed!");
        }
    }
}