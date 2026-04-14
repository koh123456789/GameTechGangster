using UnityEngine;
using UnityEngine.SceneManagement; 

public class UIMenuController : MonoBehaviour
{
    public void ExitToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // Call this for the Boss dying
    public void GoToWinScene()
    {
        // Unlock the mouse so the player can click "Play Again" or "Menu"
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("WinScene");
    }

    public void GoToLoseScene()
    {
        // Same for the Lose Scene
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("LoseScene");
    }
    // Call this for a "Try Again" button
    public void RestartGame()
    {
        SceneManager.LoadScene("Main Game"); // Replace with your actual game scene name
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game button pressed! The application will close in the final build.");
        Application.Quit();
    }
}