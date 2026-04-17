using UnityEngine;
using TMPro;
using System.Text; // Required for StringBuilder

public class BossHUD : MonoBehaviour
{
    public NPCController npc;
    public TextMeshProUGUI priorityPanel;
    public TextMeshProUGUI statsPanel;

    // We create these once so we don't reallocate them every frame
    private StringBuilder sbPriority = new StringBuilder(256);
    private StringBuilder sbStats = new StringBuilder(256);

    void Update()
    {
        if (npc == null || npc.isDead) return;

        UpdatePriorityPanel();
        UpdateStatsPanel();
    }

    void UpdatePriorityPanel()
    {
        // 1. Survival: If Boss, Low HP, and hasn't reached safety yet
        bool sGate = npc.isBoss && npc.NpcHealth <= npc.lowHealthThreshold && !npc.beenToSafeArea;

        // 2. Money: ONLY true if the NPC sees money AND doesn't have enough cash yet
        // This ensures the gate closes once they hit their target.
        bool mGate = npc.moneyVisible && npc.NpcCash < npc.cashTarget;

        // 3. Combat: Player is visible or we just took damage
        bool cGate = npc.playerVisible || npc.damageReceived;

        // 4. Patrol: The fallback if no other gates are open
        bool pGate = !sGate && !mGate && !cGate;

        sbPriority.Clear();

        // Updated text to show (See Money + Need Cash) for clarity
        sbPriority.Append("<color=#FF5555>SURVIVAL</color> (HP < ").Append(npc.lowHealthThreshold).Append("): ").Append(F(sGate)).Append("\n");
        sbPriority.Append("<color=#55FF55>MONEY</color> (Vision + Need): ").Append(F(mGate)).Append("\n"); // <--- Changed text here
        sbPriority.Append("<color=#5555FF>COMBAT</color> (SeePlayer/Hit): ").Append(F(cGate)).Append("\n");
        sbPriority.Append("<color=#AAAAAA>PATROL</color> (Fallback): ").Append(F(pGate)).Append("\n");
        sbPriority.Append("--------------------------------------\n");
        sbPriority.Append("<b>ACTIVE STATE: <color=yellow>").Append(npc.currentState.ToUpper()).Append("</color></b>");

        priorityPanel.SetText(sbPriority);
    }

    void UpdateStatsPanel()
    {
        sbStats.Clear();

        sbStats.Append("NPC HP: <color=yellow>").Append(npc.NpcHealth.ToString("F0")).Append("</color>\n");
        sbStats.Append("PlayerVisible: ").Append(F(npc.playerVisible)).Append("\n");
        sbStats.Append("MoneyVisible:  ").Append(F(npc.moneyVisible)).Append("\n");
        sbStats.Append("DamageReceived: ").Append(F(npc.damageReceived)).Append("\n");
        sbStats.Append("NPC Cash: ").Append(npc.NpcCash).Append("/").Append(npc.cashTarget).Append("\n");
        sbStats.Append("Upgrade: ").Append(npc.isUpgraded ? "<color=green>YES</color>" : "<color=red>NO</color>").Append("\n");
        sbStats.Append("Action: <color=orange>").Append(npc.currentAction).Append("</color>");

        statsPanel.SetText(sbStats);
    }

    // Static strings to avoid re-creating "YES" and "NO" strings
    private const string textYes = "<color=green>YES</color>";
    private const string textNo = "<color=red>NO</color>";
    string F(bool val) => val ? textYes : textNo;
}