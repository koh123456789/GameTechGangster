using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public NPCController npc;

    [Header("UI Elements")]
    public Slider healthSlider;
    public TextMeshProUGUI hpNumberText; // The text to the right of the bar
    public TextMeshProUGUI statusText;   // Separate text for the Healthy/Warning status

    void Start()
    {
        if (npc != null && healthSlider != null)
        {
            // Sync slider max value with NPC data
            healthSlider.maxValue = 100f; // Or npc.maxPlayerHealth if you have it
        }
    }

    void Update()
    {
        if (npc == null) return;

        float currentHP = npc.playerHealth;

        // 1. Update Slider
        if (healthSlider != null)
        {
            healthSlider.value = currentHP;
        }

        // 2. Update HP Number (Right Side)
        if (hpNumberText != null)
        {
            hpNumberText.text = currentHP.ToString("F0"); // Shows as "100" instead of "100.00"
        }

        // 3. Update Status Text
        if (statusText != null)
        {
            statusText.text = currentHP < 30 ? "<color=red>WARNING</color>" : "<color=green>HEALTHY</color>";
        }
    }
}