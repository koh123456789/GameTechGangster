using UnityEngine;
using TMPro;
using System.Text; // Required for StringBuilder

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    private float deltaTime = 0.0f;
    private StringBuilder sb = new StringBuilder();

    void Update()
    {
        // 1. Calculate FPS
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        int fpsInt = Mathf.CeilToInt(fps);

        // 2. Use StringBuilder to avoid memory "Garbage Collection"
        sb.Clear(); // Empty the builder without destroying the memory
        sb.Append("FPS: ");
        sb.Append(fpsInt);

        // 3. Update the UI
        fpsText.SetText(sb); // SetText(StringBuilder) is faster than .text = string

        // 4. Color logic (Judged by integer for speed)
        if (fpsInt >= 60) fpsText.color = Color.green;
        else if (fpsInt >= 30) fpsText.color = Color.yellow;
        else fpsText.color = Color.red;
    }
}