using UnityEngine;
using TMPro;

public class BossHUD : MonoBehaviour
{
    public NPCController npc;
    public TextMeshProUGUI liveStatsText;

    void Update()
    {
        if (npc == null) return;

        liveStatsText.text =
            $"Health: {npc.bossNpcHealth:F0}\n" +
            $"Target Dist: {npc.playerDistance:F1}m\n" +
            $"Player Visible: {(npc.playerVisible ? "<color=green>YES</color>" : "<color=red>NO</color>")}\n" +
            $"Action: <color=yellow>{npc.currentAction}</color>";
    }
}