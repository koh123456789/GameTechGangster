using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BTVisualizer : MonoBehaviour
{
    public NPCController npc;

    [Header("State Connection Lines (Root to State)")]
    public Image lineSurvival;
    public Image lineMoney;
    public Image lineCombat;
    public Image linePatrol;

    [Header("State Parent Boxes")]
    public Image survivalImg;
    public Image moneyImg;
    public Image combatImg;
    public Image patrolImg;

    [Header("Vertical Action Boxes")]
    // Survival
    public Image boxRetreat;
    public Image boxBackup;
    public Image boxBuff;
    public Image boxExitSafe;
    // Money
    public Image boxTrackCash;
    public Image boxCollectCash;
    // Combat
    public Image boxChase;
    public Image boxEnterRange;
    public Image boxAttack;
    public Image boxSearch;
    // Patrol
    public Image boxWandering;

    public Color activeColor = Color.green;
    public Color inactiveColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);

    // We keep a list of all action boxes to reset them easily
    private List<Image> allActionBoxes = new List<Image>();

    void Start()
    {
        // Automatically gather all action boxes assigned in inspector
        // This prevents us from forgetting to reset one!
        allActionBoxes.AddRange(new[] {
            boxRetreat, boxBackup, boxBuff, boxExitSafe,
            boxTrackCash, boxCollectCash,
            boxChase, boxEnterRange, boxAttack, boxSearch,
            boxWandering
        });
    }

    void Update()
    {
        ResetAll();

        // 1. Highlight Path and State Box
        // 2. Highlight Specific Action within that state
        switch (npc.currentState)
        {
            case "Survival":
                survivalImg.color = activeColor;
                lineSurvival.color = activeColor;
                HighlightAction(npc.currentAction);
                break;

            case "Interact": // Or "Money" depending on your Controller string
                moneyImg.color = activeColor;
                lineMoney.color = activeColor;
                HighlightAction(npc.currentAction);
                break;

            case "Combat":
                combatImg.color = activeColor;
                lineCombat.color = activeColor;
                HighlightAction(npc.currentAction);
                break;

            case "Idle/Patrol":
                patrolImg.color = activeColor;
                linePatrol.color = activeColor;
                HighlightAction(npc.currentAction);
                break;
        }
    }

    void ResetAll()
    {
        // Reset State Boxes
        survivalImg.color = inactiveColor;
        moneyImg.color = inactiveColor;
        combatImg.color = inactiveColor;
        patrolImg.color = inactiveColor;

        // Reset Lines
        lineSurvival.color = inactiveColor;
        lineMoney.color = inactiveColor;
        lineCombat.color = inactiveColor;
        linePatrol.color = inactiveColor;

        // Reset all individual action boxes
        foreach (Image img in allActionBoxes)
        {
            if (img != null) img.color = inactiveColor;
        }
    }

    void HighlightAction(string action)
    {
        // This looks at the string set in your ActionNodes
        switch (action)
        {
            // Survival Actions
            case "Retreat": boxRetreat.color = activeColor; break;
            case "Call Backup": boxBackup.color = activeColor; break;
            case "Upgrade & Buff": boxBuff.color = activeColor; break;
            case "Exit safe area": boxExitSafe.color = activeColor; break;

            // Money Actions
            case "Track Cash": boxTrackCash.color = activeColor; break;
            case "Collect Cash": boxCollectCash.color = activeColor; break;

            // Combat Actions
            case "Chase": boxChase.color = activeColor; break;
            case "Enter Attack Range": boxEnterRange.color = activeColor; break;
            case "Attack": boxAttack.color = activeColor; break;
            case "Search Player": boxSearch.color = activeColor; break;

            // Patrol Actions
            case "Wandering": boxWandering.color = activeColor; break;
        }
    }
}