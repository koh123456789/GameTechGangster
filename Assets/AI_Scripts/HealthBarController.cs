using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class HealthBarController : MonoBehaviour
{
    [Header("References")]
    public Slider healthSlider;
    public TextMeshProUGUI hpText;
    public NPCController bossData; // The script where HP is stored

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
        if (cam != null) transform.LookAt(transform.position + cam.forward);

        if (bossData != null && healthSlider != null)
            healthSlider.value = bossData.bossNpcHealth;

        UpdateStatusIcons();
    }

    void UpdateStatusIcons()
    {
        if (bossData == null) return;

        // 1. Search Icon (Question Mark)
        if (questionMarkIcon != null)
        {
            bool isSearching = (bossData.currentAction == BossAction.Search);
            questionMarkIcon.SetActive(isSearching);
        }

        // 2. Retreat Icon (e.g., a Running Man or Shield)
        if (retreatIcon != null)
        {
            bool isRetreating = (bossData.currentAction == BossAction.Retreat);
            retreatIcon.SetActive(isRetreating);
        }

        // 3. Found Icon (e.g., an Exclamation Mark "!")
        // We use the bool from the NPCController directly for this
        if (foundIcon != null)
        {
            bool playerFound = bossData.playerVisible;
            foundIcon.SetActive(playerFound);
        }
    }
}