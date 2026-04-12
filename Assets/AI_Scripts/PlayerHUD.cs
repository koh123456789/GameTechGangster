using UnityEngine;
using TMPro;
using UnityEngine.UI; // Required if you want to use a Slider (Health Bar)

public class PlayerHUD : MonoBehaviour
{
    public NPCController npc; // We get the player stats from the NPC's reference
    public TextMeshProUGUI playerStatsText;

    [Header("Optional Health Bar")]
    public Slider healthSlider;

    void Update()
    {
        if (npc == null) return;

        // 1. Update the Text
        playerStatsText.text =
            $"PLAYER HP: {npc.playerHealth:F0}\n" +
            $"<color=yellow>STATUS:</color> {(npc.playerHealth < 30 ? "WARNING" : "HEALTHY")}";
    }
}