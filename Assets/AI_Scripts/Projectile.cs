using UnityEngine;

public enum ProjectileSource { Boss, Player }

public class Projectile : MonoBehaviour
{
    [Header("Stats")]
    public float damage;
    public float speed;
    public float lifetime = 5f;

    [Header("Identity")]
    public ProjectileSource firedBy; // Set this when Instantiating

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move forward relative to its own rotation
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. IF FIRED BY BOSS -> LOOK FOR PLAYER
        if (firedBy == ProjectileSource.Boss && other.CompareTag("Player"))
        {
            NPCController bossScript = FindObjectOfType<NPCController>();
            if (bossScript != null)
            {
                bossScript.playerHealth -= damage;
                Debug.Log("<color=red>Boss hit Player!</color> New Player HP: " + bossScript.playerHealth);
            }
            Destroy(gameObject);
        }

        // 2. IF FIRED BY PLAYER -> LOOK FOR BOSS
        else if (firedBy == ProjectileSource.Player && (other.CompareTag("NPC")))
        {
            NPCController bossScript = other.GetComponent<NPCController>();
            if (bossScript != null)
            {
                bossScript.TakeDamage(damage);
                Debug.Log("<color=cyan>Player hit Boss!</color>");
            }
            Destroy(gameObject);
        }

        // 3. HIT ENVIRONMENT (Walls/Floors)
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstruction") || other.gameObject.layer == 0) // 0 is Default
        {
            Destroy(gameObject);
        }
    }
}