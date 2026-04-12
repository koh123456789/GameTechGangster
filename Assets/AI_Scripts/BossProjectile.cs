using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float damage;
    public float speed;
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime); // Clean up bullet after 5 seconds
    }

    void Update()
    {
        // Move the bullet forward
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Check if the object we hit is tagged "Player"
        if (other.CompareTag("Player"))
        {
            Debug.Log("<color=red>HIT CONFIRMED:</color> Projectile touched Player!");

            // 2. Find the NPCController in the scene
            NPCController bossScript = FindObjectOfType<NPCController>();

            if (bossScript != null)
            {
                // 3. Deduct the damage from the boss's record of player health
                bossScript.playerHealth -= damage;
                Debug.Log("New Player HP: " + bossScript.playerHealth);
            }
            else
            {
                Debug.LogError("Could not find NPCController to deduct HP!");
            }

            // 4. Destroy the bullet so it doesn't hit twice
            Destroy(gameObject);
        }
        // Optional: Destroy if it hits a wall so they don't pile up
        else if (other.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            Destroy(gameObject);
        }
    }
}