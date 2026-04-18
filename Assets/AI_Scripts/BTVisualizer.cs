using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BTVisualizer : MonoBehaviour
{
    public NPCController npc;

    [System.Serializable]
    public struct ActionUI
    {
        public BossAction actionEnum;
        public Image box;
        public GameObject lineParent;
        [HideInInspector] public Image[] cachedLineImages; // Added for caching
    }

    [System.Serializable]
    public struct StateGroup
    {
        public string stateName;
        public Image stateBox;
        public GameObject mainLineParent;
        [HideInInspector] public Image[] cachedMainLineImages; // Added for caching
        public List<ActionUI> actions;
    }

    public List<StateGroup> stateGroups;
    public Color activeColor = Color.green;
    public Color inactiveColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    private string lastState;
    private BossAction lastAction;

    void Start()
    {
        // Cache all images once at the start so we NEVER call GetComponentsInChildren in Update
        for (int i = 0; i < stateGroups.Count; i++)
        {
            var group = stateGroups[i];
            if (group.mainLineParent)
                group.cachedMainLineImages = group.mainLineParent.GetComponentsInChildren<Image>();

            for (int j = 0; j < group.actions.Count; j++)
            {
                var action = group.actions[j];
                if (action.lineParent)
                    action.cachedLineImages = action.lineParent.GetComponentsInChildren<Image>();

                group.actions[j] = action;
            }
            stateGroups[i] = group;
        }
    }

    void Update()
    {
        if (npc == null) return;

        // ONLY update if something actually changed
        if (npc.currentState == lastState && npc.currentAction == lastAction) return;

        lastState = npc.currentState;
        lastAction = npc.currentAction;

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        foreach (var group in stateGroups)
        {
            bool isStateActive = (npc.currentState == group.stateName);

            // Set State Box
            if (group.stateBox) group.stateBox.color = isStateActive ? activeColor : inactiveColor;

            // Set State Lines
            SetCachedColor(group.cachedMainLineImages, isStateActive ? activeColor : inactiveColor);

            foreach (var actionUI in group.actions)
            {
                bool isActionActive = isStateActive && (npc.currentAction == actionUI.actionEnum);

                if (actionUI.box) actionUI.box.color = isActionActive ? activeColor : inactiveColor;
                SetCachedColor(actionUI.cachedLineImages, isActionActive ? activeColor : inactiveColor);
            }
        }
    }

    void SetCachedColor(Image[] images, Color color)
    {
        if (images == null) return;
        foreach (Image img in images)
        {
            img.color = color;
        }
    }
}