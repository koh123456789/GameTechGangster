using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // Drag your Pause Panel here
    private bool isPaused = false;

    void Update()
    {
        // Check if player presses Escape (or P)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Game speed back to normal
        isPaused = false;

        // Hide and lock the cursor again
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Freeze the game
        isPaused = true;

        // Release the cursor so the user can see it
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game is exiting...");
    }
}