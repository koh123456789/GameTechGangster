using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BTVisualizer : MonoBehaviour
{
    public BossController npc;

    [System.Serializable]
    public struct ActionUI
    {
        public BossAction actionEnum;
        public Image box;
        public GameObject lineParent; // Drag the Empty Parent that holds | and _ here
    }

    [System.Serializable]
    public struct StateGroup
    {
        public string stateName;
        public Image stateBox;
        public GameObject mainLineParent; // Line from Root to State
        public List<ActionUI> actions;
    }

    public List<StateGroup> stateGroups;
    public Color activeColor = Color.green;
    public Color inactiveColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    void Update()
    {
        ResetAll();

        foreach (var group in stateGroups)
        {
            if (npc.currentState == group.stateName)
            {
                // Highlight State
                if (group.stateBox) group.stateBox.color = activeColor;
                SetObjectColor(group.mainLineParent, activeColor);

                // Highlight Action
                foreach (var actionUI in group.actions)
                {
                    if (npc.currentAction == actionUI.actionEnum)
                    {
                        if (actionUI.box) actionUI.box.color = activeColor;
                        SetObjectColor(actionUI.lineParent, activeColor);
                        break;
                    }
                }
                break;
            }
        }
    }

    void ResetAll()
    {
        foreach (var group in stateGroups)
        {
            if (group.stateBox) group.stateBox.color = inactiveColor;
            SetObjectColor(group.mainLineParent, inactiveColor);

            foreach (var actionUI in group.actions)
            {
                if (actionUI.box) actionUI.box.color = inactiveColor;
                SetObjectColor(actionUI.lineParent, inactiveColor);
            }
        }
    }

    // This is the "Magic" function that colors both parts of the L-shape
    void SetObjectColor(GameObject parent, Color color)
    {
        if (parent == null) return;

        // If the parent itself has an Image, color it
        Image mainImg = parent.GetComponent<Image>();
        if (mainImg != null) mainImg.color = color;

        // Color all children (the | and the _)
        Image[] childImages = parent.GetComponentsInChildren<Image>();
        foreach (Image img in childImages)
        {
            img.color = color;
        }
    }
}