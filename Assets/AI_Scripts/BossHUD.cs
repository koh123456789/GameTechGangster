using UnityEngine;
using TMPro;

public class BossHUD : MonoBehaviour
{
    public BossController npc;
    public TextMeshProUGUI liveStatsText;

    void Update()
    {
        if (npc == null) return;

        // We can now show if he's in Phase 2 or searching for a specific spot!
        string phaseStatus = npc.isUpgraded ? "RANGED (P2)" : "MELEE (P1)";

        liveStatsText.text =
            $"Mode: {phaseStatus}\n" +
            $"Health: {npc.bossNpcHealth:F0}\n" +
            $"Target Dist: {npc.playerDistance:F1}m\n" +
            $"Visible: {(npc.playerVisible ? "<color=green>YES</color>" : "<color=red>NO</color>")}\n" +
            $"Action: <color=yellow>{npc.currentAction}</color>\n" +
            $"Target Last Seen: {npc.lastKnownPlayerPos}"; // Helpful for debugging Search logic
    }
}