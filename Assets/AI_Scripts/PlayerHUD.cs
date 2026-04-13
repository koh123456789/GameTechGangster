using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    // CHANGE: Point this to the PlayerStats script on your actual Player object
    public PlayerController playerStats;

    [Header("UI Elements")]
    public Slider healthSlider;
    public TextMeshProUGUI hpNumberText;
    public TextMeshProUGUI statusText;

    void Start()
    {
        // If you don't want to drag it in the inspector, find it automatically
        if (playerStats == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerStats = player.GetComponent<PlayerController>();
        }

        if (playerStats != null && healthSlider != null)
        {
            healthSlider.maxValue = playerStats.maxHealth;
            healthSlider.value = playerStats.currentHealth;
        }
    }

    void Update()
    {
        if (playerStats == null) return;

        float currentHP = playerStats.currentHealth;

        // 1. Update Slider
        if (healthSlider != null)
        {
            healthSlider.value = currentHP;
        }

        // 2. Update HP Number
        if (hpNumberText != null)
        {
            hpNumberText.text = currentHP.ToString("F0");
        }

        // 3. Update Status Text
        if (statusText != null)
        {
            // Using a threshold of 30% of max health is safer than a hardcoded 30
            float warningThreshold = playerStats.maxHealth * 0.3f;
            statusText.text = currentHP < warningThreshold ? "<color=red>WARNING</color>" : "<color=green>HEALTHY</color>";
        }
    }
}