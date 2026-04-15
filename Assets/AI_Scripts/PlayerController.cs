using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;


    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"Player hit! Remaining Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }
    private void Die()
    {
        if (isDead) return; // Prevent multiple calls
        isDead = true;

        Debug.Log("<color=red>PLAYER DIED</color>");

        StartCoroutine(WaitAndLose(2.0f));
    }

    private System.Collections.IEnumerator WaitAndLose(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Unlock cursor right before switching
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        FindObjectOfType<UIMenuController>().GoToLoseScene();
    }
}