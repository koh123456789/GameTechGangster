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
        // FIX: Fire from the center of the viewport (camera lens) 
        // instead of the mouse cursor position
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Visualize the ray in the Scene view so you can see if it's hitting
        Debug.DrawRay(ray.origin, ray.direction * attackRange, Color.red, 1f);

        if (Physics.Raycast(ray, out hit, attackRange, bossLayer))
        {
            NPCController boss = hit.collider.GetComponent<NPCController>();
            if (boss != null)
            {
                Debug.Log("<color=green>Direct Hit on Boss!</color>");
                boss.TakeDamage(damageDealt);
            }
        }
        else
        {
            Debug.Log("Missed!");
        }
    }
}