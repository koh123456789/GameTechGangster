using UnityEngine;
using UnityEngine.SceneManagement; 

public class UIMenuController : MonoBehaviour
{
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game button pressed! The application will close in the final build.");
        Application.Quit();
    }
}