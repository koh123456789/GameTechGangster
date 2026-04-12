using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [Header("References")]
    public Slider healthSlider;
    public NPCController bossData; // The script where HP is stored

    private Transform cam;

    void Start()
    {
        // Find the main camera automatically
        if (Camera.main != null)
            cam = Camera.main.transform;

        // Initialize Slider
        if (bossData != null && healthSlider != null)
        {
            healthSlider.maxValue = bossData.maxHealth;
            healthSlider.value = bossData.bossNpcHealth;
        }

        // Assign the Event Camera automatically for World Space
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = Camera.main;
        }
    }

    // LateUpdate is best for UI and Cameras to prevent "jittery" movement
    void LateUpdate()
    {
        // 1. FACE THE CAMERA (Billboard Logic)
        if (cam != null)
        {
            // This makes the bar face the camera perfectly
            transform.LookAt(transform.position + cam.forward);
        }

        // 2. UPDATE THE VALUE
        if (bossData != null && healthSlider != null)
        {
            healthSlider.value = bossData.bossNpcHealth;
        }
    }
}