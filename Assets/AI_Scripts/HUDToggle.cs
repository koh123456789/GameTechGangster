using UnityEngine;

public class HUDToggle : MonoBehaviour
{
    public GameObject behaviorTreePanel; // Drag your whole BT UI here
    private bool isHudOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isHudOpen = !isHudOpen;
            ToggleHUD(isHudOpen);
        }
    }

    void ToggleHUD(bool show)
    {
        behaviorTreePanel.SetActive(show);

        //if (show)
        //{
        //    // Unlock mouse to edit stats
        //    Cursor.lockState = CursorLockMode.None;
        //    Cursor.visible = true;
        //    // Optional: slow down time so you can "think" while editing
        //    // Time.timeScale = 0.5f; 
        //}
        //else
        //{
        //    // Relock mouse to play
        //    Cursor.lockState = CursorLockMode.Locked;
        //    Cursor.visible = false;
        //    Time.timeScale = 1f;
        //}
    }
}