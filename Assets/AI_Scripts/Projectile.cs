using UnityEngine;

public enum ProjectileSource { Boss, Player }

public class Projectile : MonoBehaviour
{
    [Header("Stats")]
    public float damage;
    public float speed;
    public float lifetime = 5f;

    [Header("Identity")]
    public ProjectileSource firedBy;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. IF FIRED BY BOSS -> LOOK FOR PLAYERSTATS
        if (firedBy == ProjectileSource.Boss && other.CompareTag("Player"))
        {
            // CHANGE: Talk to PlayerStats, not the Boss script
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log("<color=red>Boss projectile hit Player!</color>");
            }
            Destroy(gameObject);
        }

        // 2. IF FIRED BY PLAYER -> LOOK FOR BOSS
        else if (firedBy == ProjectileSource.Player && other.CompareTag("NPC"))
        {
            // CHANGE: Use NPCController (base class) so it works for all NPCs/Bosses
            NPCController npc = other.GetComponent<NPCController>();
            if (npc != null)
            {
                npc.TakeDamage(damage);
                Debug.Log("<color=cyan>Player projectile hit NPC!</color>");
            }
            Destroy(gameObject);
        }

        // 3. HIT ENVIRONMENT
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstruction") || other.gameObject.layer == 0)
        {
            Destroy(gameObject);
        }
    }
}