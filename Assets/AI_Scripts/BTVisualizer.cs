using UnityEngine;
using UnityEngine.UI;

public class BTVisualizer : MonoBehaviour
{
    public NPCController npc;

    [Header("Node Backgrounds")]
    public Image survivalImg;
    public Image moneyImg;
    public Image combatImg;
    public Image idleImg;

    public Color activeColor = Color.green;
    public Color inactiveColor = new Color(0.3f, 0.3f, 0.3f, 0.8f); // Dark Gray

    void Update()
    {
        // 1. Reset all nodes
        survivalImg.color = inactiveColor;
        moneyImg.color = inactiveColor;
        combatImg.color = inactiveColor;
        idleImg.color = inactiveColor;

        // 2. Highlight based on the NPC's current logic branch
        // We use the strings you already set in your ActionNodes
        switch (npc.currentState)
        {
            case "Survival":
                survivalImg.color = activeColor;
                break;
            case "Interact":
                moneyImg.color = activeColor;
                break;
            case "Combat":
            case "Chasing": // Match any strings you use in ActionNodes
                combatImg.color = activeColor;
                break;
            case "Idle/Patrol":
                idleImg.color = activeColor;
                break;
        }
    }
}