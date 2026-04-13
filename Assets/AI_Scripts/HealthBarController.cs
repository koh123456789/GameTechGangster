using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class HealthBarController : MonoBehaviour
{
    [Header("References")]
    public Slider healthSlider;
    public TextMeshProUGUI hpText;
    public BossController bossData; // The script where HP is stored

    [Header("Status Icons")]
    public GameObject questionMarkIcon; // For "Search"
    public GameObject retreatIcon;      // For "Retreat"
    public GameObject foundIcon;        // For "Player Visible"

    private Transform cam;

    void Start()
    {
        // Find the main camera automatically
        if (Camera.main != null)
            cam = Camera.main.transform;

        if (bossData != null)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = bossData.maxHealth;
                healthSlider.value = bossData.bossNpcHealth;
            }
            UpdateHPText();
        }

        // Assign the Event Camera automatically for World Space
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = Camera.main;
        }

        if (questionMarkIcon != null) questionMarkIcon.SetActive(false);
        if (retreatIcon != null) retreatIcon.SetActive(false);
        if (foundIcon != null) foundIcon.SetActive(false);
    }

    void UpdateHPText()
    {
        if (hpText != null && bossData != null)
        {
            // Format: "85 / 100"
            hpText.text = $"{bossData.bossNpcHealth:F0} / {bossData.maxHealth:F0}";
        }
    }

    void LateUpdate()
    {
        // 1. If the boss is dead, just hide the whole UI and stop processing
        if (bossData != null && bossData.isDead)
        {
            // This makes the health bar and all child icons vanish
            gameObject.SetActive(false);
            return;
        }

        if (cam != null) transform.LookAt(transform.position + cam.forward);

        if (bossData != null && healthSlider != null)
        {
            healthSlider.value = bossData.bossNpcHealth;
            UpdateHPText(); // Keep text updated until the very last hit
        }

        UpdateStatusIcons();
    }

    void UpdateStatusIcons()
    {
        if (bossData == null || bossData.isDead) return;

        // 1. Search Icon
        if (questionMarkIcon != null)
        {
            questionMarkIcon.SetActive(bossData.currentAction == BossAction.Search);
        }

        // 2. Retreat Icon
        if (retreatIcon != null)
        {
            retreatIcon.SetActive(bossData.currentAction == BossAction.Retreat);
        }

        // 3. Found Icon
        if (foundIcon != null)
        {
            // Add "Upgrade" here too so it doesn't overlap
            bool isBusy = bossData.currentAction == BossAction.Retreat ||
                          bossData.currentAction == BossAction.Upgrade;

            bool playerFound = bossData.playerVisible && !isBusy;
            foundIcon.SetActive(playerFound);
        }

        // 4. NEW: Upgrade/Phase 2 Visual (Optional but helpful)
        // If you have a special icon for Phase 2, you can toggle it here
        // using bossData.isUpgraded
    }
}